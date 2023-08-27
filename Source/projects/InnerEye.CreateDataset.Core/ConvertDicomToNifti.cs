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
   