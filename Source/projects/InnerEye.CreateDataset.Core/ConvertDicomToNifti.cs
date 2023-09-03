///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using MedLib.IO;
    using InnerEye.CreateDataset.Common;
    using InnerEye.CreateDataset.Volumes;
    using itk.simple;
    using InnerEye.CreateDataset.Data;
    using InnerEye.CreateDataset.Math;
    using MoreLinq;

    public static class ConvertDicomToNifti
    {
        /// <summary>
        /// The name of a file that is written during dataset creation, and contains info about how dataset
        /// creation was done.
        /// </summary>
        public const string DatasetCreationStatusFile = "info.txt";

        /// <summary>
        /// Converts a dataset from DICOM to Nifti.
        /// Each channel in the dataset will be turned into Int16 Nifti files, each segmented
        /// structure will be turned into a binary mask stored as a Byte Nifti file.
        /// After this method finishes, the converted dataset will be in folder <paramref name="dataRoot"/>.
        /// </summary>
        /// <param name="dataRoot">The folder whichh contains the DICOM dataset.</param>
        /// <param name="options">The commandline options that guide the dataset creation.</param>
        /// <returns></returns>
        public static void CreateDataset(LocalFileSystem dataRoot, CommandlineCreateDataset options)
        {

            var datasetPath = StreamsFromFileSystem.JoinPath(dataRoot.RootDirectory, options.NiftiDirectory);
            Directory.CreateDirectory(datasetPath);
            var datasetRoot = new LocalFileSystem(datasetPath, false);

            // TODO: Get metadata on DICOM dataset
            //PrintDatasetMetadata(metadata);
            //if (metadata.DatasetSize == 0)
            //{
            //    throw new InvalidDataException("The dataset is empty.");
            //}

            var dataLoader = new DatasetLoader(StreamsFromFileSystem.JoinPath(dataRoot.RootDirectory, options.DicomDirectory));
            var datasetAsVolumeAndMetadata = dataLoader.LoadAllDicomSeries();
            var datasetAsVolumes = datasetAsVolumeAndMetadata.SelectMany(itemsPerSubject =>
                {
                    if (AreDatasetItemsValid(itemsPerSubject, options.DiscardInvalidSubjects))
                    {
                        return
                            new List<List<VolumeAndStructures>>() { itemsPerSubject
                            .Select(item => VolumeAndStructures.FromMedicalVolume(item, isLowerCaseConversionEnabled: true, dropRepeats: true))
                            .ToList() };
                    }
                    return new List<List<VolumeAndStructures>>();
                });

            var writer = new DatasetWriter(datasetRoot, NiftiCompression.GZip);
            var message =
                writer.WriteDatasetToFolder(datasetAsVolumes,
                    itemsPerSubject => ConvertSingleSubject(itemsPerSubject, options));
            var datasetCsvString = VolumeWriteInfo.BuildDatasetCsvFile(writer.WrittenVolumes());
            writer.WriteText(DatasetReader.DatasetCsvFile, datasetCsvString);
            var status = new StringBuilder(options.SettingsOverview());
            status.AppendLine("Per-subject status information:");
            status.AppendLine(message);
            writer.WriteText(DatasetCreationStatusFile, status.ToString());
        }

        /// <summary>
        /// Performs all dataset creation options on the data for a single subject:
        /// * Registration on a reference volume
        /// * Renaming structures
        /// * Making structures mutually exclusive
        /// * Geometric normalization
        /// * Compute derived structures
        /// </summary>
        /// <param name="itemsPerSubject">All volumes and their associated structures for the subject.</param>
        /// <param name="options">The commandline options that guide dataset creation.</param>
        /// <returns></returns>
        public static IEnumerable<VolumeAndStructures> ConvertSingleSubject(IReadOnlyList<VolumeAndStructures> itemsPerSubject,
            CommandlineCreateDataset options)
        {
            var volumes = RegisterSubjectVolumes(itemsPerSubject, options.RegisterVolumesOnReferenceChannel).ToList();
            if (volumes.Count == 0)
            {
                // We silently drop a subject with no volumes at all, even if options.RequireAllGroundTruthStructures is set.
                return volumes;
            }
            // By this point we should have at most one volume with structures attached. If we don't find one, we take the
            // first volume as the one that will eventually receive structures.
            var mainVolume = volumes.FirstOrDefault(volume => volume.Structures.Count > 0) ?? volumes[0];
            var subjectId = mainVolume.Metadata.SubjectId;
            var renamingOK = mainVolume.Rename(options.NameMappings, allowNameClashes: options.AllowNameClashes,
                throwIfInvalid: !options.DiscardInvalidSubjects);
            if (!renamingOK)
            {
                // then discard all the volumes
                return new List<VolumeAndStructures>();
            }
            mainVolume.AddEmptyStructures(options.CreateIfMissing);
            bool structuresAreGood = true;
            if (options.GroundTruthDescendingPriority != null && options.GroundTruthDescendingPriority.Any())
            {
                var namesInPriorityOrder = options.GroundTruthDescendingPriority.ToArray();
                var namesToRemove = MakeStructuresMutuallyExclusiveInPlace(mainVolume.Structures, namesInPriorityOrder, subjectId);
                foreach (var name in namesToRemove)
                {
                    Trace.TraceInformation($"Subject {subjectId}: removing structure named {name}");
                    mainVolume.Remove(name);
                }
                if (options.RequireAllGroundTruthStructures)
                {
                    var namesToFind = new HashSet<string>(namesInPriorityOrder.Select(name => name.TrimStart(new[] { '+' })));
                    var namesPresent = mainVolume.Structures.Select(structure => structure.Key).ToHashSet();
                    var namesMissing = namesToFind.Except(namesPresent).ToList();
                    if (!namesMissing.IsNullOrEmpty())
                    {
                        var message = $"Subject {subjectId}: Error: no structure(s) named " + string.Join(", ", namesMissing);
                        if (options.DiscardInvalidSubjects)
                        {
                            Trace.TraceInformation(message);
                            structuresAreGood = false;
                        }
                        else
                        {
                            throw new InvalidOperationException(message);
                        }
                    }
                }
            }
            if (!structuresAreGood)
            {
                // then discard all the volumes
                return new List<VolumeAndStructures>();
            }
            var spacing = options.GeometricNormalizationSpacingMillimeters?.ToArray();
            volumes = volumes.Select(volume => volume.GeometricNormalization(spacing)).ToList();
            if (options.DerivedStructures != null)
            {
                options.DerivedStructures.ForEach(derived => AddDerivedStructures(volumes, derived));
            }
            Trace.TraceInformation($"Subject {subjectId}: has all required structures");
            return volumes;
        }
