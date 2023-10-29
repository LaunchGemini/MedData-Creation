///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿using MedLib.IO;
using MedLib.IO.Readers;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using MedLib.IO.Models;

namespace InnerEye.CreateDataset.Core
{
    class DatasetLoader
    {
        private string _datasetPath;

        public DatasetLoader(string datasetPath)
        {
            _datasetPath = datasetPath;
        }

        /// <summary>
        /// Iterates through all subfolders (non recursive) and reads a Dicom series from each one.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IReadOnlyList<VolumeAndMetadata>> LoadAllDicomSeries()
        {
           
            // TODO group by SubjectID
            var acceptanceTest = new NonStrictGeometricAcceptanceTest("Non square pixels", "Orientation unsupported");
            var subjectIdsToIndices = new List<string>();

            foreach (var folder in Directory.EnumerateDirectories(_datasetPath))
            {
                var subjectVolumes = new List<VolumeAndMetadata>();
                var stopWatch = Stopwatch.StartNew();
                var volume = MedIO.LoadSingleDicomSeriesAsync(folder, acceptanceTest).Result;
                var seriesId = getSeriesId(volume);
                var subjectId = getSubjectId(volume);
             