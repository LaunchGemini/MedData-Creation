
///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Runner
{
    using System;
    using System.Net;
    using System.Threading;
    using CommandLine;
    using InnerEye.CreateDataset.Core;

    public static class Program
    {
        private static void Main(string[] args)
        {
            ThreadPool.SetMinThreads(100, 100);
            ServicePointManager.DefaultConnectionLimit = 100;

            Parser.Default.ParseArguments<CommandlineShared,CommandlineCreateDataset,CommandlineAnalyzeDataset>(args)
                .WithParsed<CommandlineAnalyzeDataset>(opts => RunTask(opts, DatasetAnalysisFromConvertedDataset.AnalyzeDataset))
                .WithParsed<CommandlineCreateDataset>(opts =>
                {
                    opts.RawCommandlineArguments = args;
                    RunTask(opts, ConvertDicomToNifti.CreateDataset); 
                })
                .WithNotParsed(errs =>
                {
                    void action(CommandlineShared _)
                    {
                        foreach (var err in errs)
                        {