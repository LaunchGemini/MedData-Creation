///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Core
{
    using CommandLine;
    
    [Verb("analyze", HelpText = "Analyzes a converted dataset in NIFTI format by deriving statistics.")]
    public class CommandlineAnalyzeDataset : CommandlineShared
    {
        /// <summary>
        /// Gets or sets the un-processed commandline arguments that have been passed into the CreateDataset runner.
        /// </summary>
        public string[] RawCommandlineArguments { get; set; }

        /// <summary>
        /// The full path to the dataset folder to be analyzed.
        /// </summary>
        [Option('d', "datasetFolder", HelpText = "Location of the nifti dataset to be analyzed")]
        public string DatasetFolder { get; set; }

        [Option('s', "statisticsFolder", Default="statistics", HelpText = "Name of subfolder to receive statistics files (must not already exist)")]
        public string StatisticsFolde