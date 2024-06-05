///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿
namespace MedLib.IO.Readers
{
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Dicom;
    using RT;
    using MedLib.IO.Extensions;

    public sealed class DicomFolderContents
    {
        /// <summary>
        /// The list of {SeriesUID, SOPInstance list} for recognized image types CT & MR in this folder 
        /// </summary>
        public IReadOnlyList<DicomSeriesContent> Series { get; private set; }

        /// <summary>
        /// The list of {Referenced SeriesUID, SOPInstance list} for recognized RTStructs in this folder. 
        /// </summary>
        public IReadOnlyList<DicomSeriesContent> RTStructs { get; private set; }

        /// <summary>
        /// The folder path this instance was generated from.
        /// </summary>
        public string FolderPath { get; private set; }

        public static DicomFolderContents Build(IReadOnlyList<DicomFileAndPath> fileAndPaths)
        {
            // Extract the RT structs and group by referenced SeriesUID
            var rtStructs = fileAndPaths.Where((fp) => fp.File.Dataset.GetSingleValueOrDefault(DicomTag.SOPClassUID, DicomExtensions.EmptyUid) == DicomUID.RTStructureSetStorage);

            // We assume there is 1 and only 1 referenced series
            var parsedReferencedFoR = rtStructs.GroupBy(
                (rt) => DicomRTStructureSet.Read(rt.File.Dataset).
                    ReferencedFramesOfRef.FirstOrDefault()?.
                    ReferencedStudies?.FirstOrDefault()?.
                    ReferencedSeries.FirstOrDefault()?.
                    SeriesInstanceUID ?? string.Empty
            );

            // Filter out the CT and MR SOPClasses
            var ctMR = fileAndPaths.Where((fp