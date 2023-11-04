///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿
namespace InnerEye.CreateDataset.Core
{
    using System.Collections.Concurrent;
    using InnerEye.CreateDataset.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MoreLinq;
    using InnerEye.CreateDataset.Math;
    using InnerEye.CreateDataset.Volumes;
    using MedLib.IO;
    using System.IO;


    /// <summary>
    /// Handles the writing a dataset to a folder.
    /// </summary>
    public class DatasetWriter
    {
        private LocalFileSystem _datasetRoot;
        private NiftiCompression _niftiCompression;
        private ConcurrentBag<VolumeWriteInfo> _writtenVolumes = new ConcurrentBag<VolumeWriteInfo>();

        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="datasetRoot">The folder to which the dataset should be written.</param>
        /// <param name="niftiCompression">The Nifti compression level that should be used to write 
        /// all volumes.</param>
        public DatasetWriter(LocalFileSystem datasetRoot, NiftiCompression niftiCompression)
        {
            _datasetRoot = datasetRoot;
            _niftiCompression = niftiCompression;
        }

        /// <summary>
        /// Writes text to a file.
        /// </summary>
        /// <param name="fileName">The file name to to write to within the dataset folder</param>
        /// <param name="text">The text to write.</param>
        public void WriteText(string fileName, string text)
        {
            _datasetRoot.WriteAllText(fileName, text);
        }

        /// <summary>
        /// Writes a full dataset to the dataset folder.
        /// datasets. Returns a human readable string with diagnostic information.
        /// </summary>
        /// <param name="dataset">The individual items of the dataset, one item per subject.</param>
        public string WriteDatasetToFolder(IEnumerable<IReadOnlyList<VolumeAndStructures>> dataset,
            Func<IReadOnlyList<VolumeAndStructures>, IEnumerable<VolumeAndStructures>> converter)
        {
            var foundStructures = new ConcurrentDictionary<string, int>();
            var _counterLock = new object();
            var subjectCount = 0;
            Parallel.ForEach(
                dataset,
                new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount },
                itemsPerSubject =>
                {
                    lock (_counterLock)
                    {
                        subjectCount++;
                    }
                    