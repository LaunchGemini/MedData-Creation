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

        /// <summary>
        /// Performs consistency checks on the structures that are available for a subject. 
        /// In particular, structure names must be unique after conversion to lower case, across
        /// all channels for the subject. Returns true if no problems are found. If any
        /// problems are found, either returns false (if discardInvalidSubjects is true) or throws an
        /// <see cref="InvalidOperationException"/>. Details of the errors are output to Trace.
        /// </summary>
        /// <param name="itemsPerSubject">A list of channels for a single subject.</param>
        /// <param name="discardInvalidSubjects">whether to drop problematic subjects (rather than throwing)</param>
        public static bool AreDatasetItemsValid(IReadOnlyList<VolumeAndMetadata> itemsPerSubject, bool discardInvalidSubjects)
        {
            var errorMessages = new List<string>();
            var warningMessages = new List<string>();
            var text = new StringBuilder();
            // Maps from a structure name in lower case to the series ID that contained that structure
            var structureNamesLowerCase = new Dictionary<string, VolumeMetadata>();
            string channelWithStructures = null;
            foreach (var item in itemsPerSubject)
            {
                var contourNames = string.Join(", ", item.Volume.Struct.Contours.Select(c => c.StructureSetRoi.RoiName));
                text.AppendLine($"Subject {item.Metadata.SubjectId}: series {item.Metadata.SeriesId}, channel '{item.Metadata.Channel}': {contourNames}");
                // We don't currently handle subjects that have structures attached to multiple different channels.
                if (item.Volume.Struct.Contours.Count == 0)
                {
                    continue;
                } else if (channelWithStructures != null)
                {
                    errorMessages.Append($"Series {item.Metadata.SeriesId} has structures on multiple channels: {channelWithStructures} and {item.Metadata.Channel}.");
                } else
                {
                    channelWithStructures = item.Metadata.Channel;
                }
                foreach (var contour in item.Volume.Struct.Contours)
                {
                    var contourNameLowerCase = contour.StructureSetRoi.RoiName.ToLowerInvariant();
                    if (structureNamesLowerCase.TryGetValue(contourNameLowerCase, out var otherVolume))
                    {
                        var thisSeries = item.Metadata.SeriesId;
                        var otherSeries = otherVolume.SeriesId;
                        var message = new StringBuilder($"Subject {item.Metadata.SubjectId}: after conversion to lower case, there is more than one structure with name '{contourNameLowerCase}'");
                        if (otherSeries == thisSeries)
                        {
                            message.Append($" in series {thisSeries}");
                        }
                        else
                        {
                            message.Append($". Affected series are {thisSeries} and {otherSeries} ");
                        }
                        warningMessages.Add(message.ToString());
                    }
                    else
                    {
                        structureNamesLowerCase.Add(contourNameLowerCase, item.Metadata);
                    }
                }
            }

            Trace.TraceInformation(text.ToString());
            warningMessages.ForEach(message => Trace.TraceWarning(message));
            if (errorMessages.Count > 0)
            {
                errorMessages.ForEach(message => Trace.TraceError(message));
                if (!discardInvalidSubjects)
                {
                    throw new InvalidOperationException("The dataset contains invalid structures. Inspect the console for details. First error: " + errorMessages.First());
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Computes a derived structure by applying an operator as specified in <paramref name="derived"/>, and saves 
        /// that to the first element of the argument <paramref name="itemsPerSubject"/>.
        /// </summary>
        /// <param name="itemsPerSubject"></param>
        /// <param name="derived"></param>
        public static void AddDerivedStructures(IReadOnlyList<VolumeAndStructures> itemsPerSubject, DerivedStructure derived)
        {
            Volume3D<byte> find(string name)
            {
                var result =
                    itemsPerSubject
                    .SelectMany(v => v.Structures)
                    .Where(s => s.Key == name)
                    .Select(v => v.Value)
                    .SingleOrDefault();
                if (result == null)
                {
                    throw new KeyNotFoundException($"There is no structure with name '{name}', which is required to compute the derived structure '{derived.Result}'");
                }

                return result;
            }
            var left = find(derived.LeftSide);
            var right = find(derived.RightSide);
            itemsPerSubject[0].Add(derived.Result, ComputeDerivedStructure(left, right, derived));
        }

        /// <summary>
        /// Creates a derived structure, with the given left and right sides of the operator. The operator 
        /// itself is consumed from <paramref name="derived"/>.Operator
        /// </summary>
        /// <param name="left">The binary mask that is the left side of the operator.</param>
        /// <param name="right">The binary mask that is the right side of the operator.</param>
        /// <param name="derived"></param>
        /// <returns></returns>
        public static Volume3D<byte> ComputeDerivedStructure(Volume3D<byte> left, Volume3D<byte> right, DerivedStructure derived)
        {
            var leftSize = Volume3DDimensions.Create(left);
            var rightSize = Volume3DDimensions.Create(right);
            if (!leftSize.Equals(rightSize))
            {
                throw new InvalidOperationException($"Structure for '{derived.LeftSide}' has size {leftSize}, but '{derived.RightSide}' has size {rightSize}");
            }

            Volume3D<byte> result;
            switch (derived.Operator)
            {
                case DerivedStructureOperator.Except:
                    result = left.MapIndexed(null, (leftValue, index) =>
                        leftValue == ModelConstants.MaskBackgroundIntensity
                        ? ModelConstants.MaskBackgroundIntensity
                        : right[index] == ModelConstants.MaskBackgroundIntensity
                        ? ModelCons