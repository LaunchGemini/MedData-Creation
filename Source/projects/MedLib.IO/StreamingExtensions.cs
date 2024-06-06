///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace MedLib.IO
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Compression;
    using MedLib.IO.Readers;

    public class ZippedDicom
    {
        /// <summary>
        /// Opens the given byte array as a Zip archive, and returns Dicom files for all entries 
        /// in the archive.
        /// </summary>
        /// <param name="zipArchive">The full Zip archive as a byte array.</param>
        /// <returns></returns>
        public static IEnumerable<DicomFileAndPath> DicomFilesFromZipArchive(byte[] zipArchive)
        {
           