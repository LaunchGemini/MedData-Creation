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
            using (var zip = new ZipArchive(new MemoryStream(zipArchive), ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    yield return DicomFileAndPath.SafeCreate(new MemoryStream(ToByteArray(entry)), string.Empty);
                }
            }
        }

        /// <summary>
        /// Creates a Zip archive that contains all the given Dicom files, and writes the Zip archive
        /// to the stream.
        /// </summary>
        /// <param name="stream">The stream to which the Zip archive should be written.</param>
        /// <param name="dicomFiles">The Dicom files to write to the Zip archive.</param>
        /// <returns></returns>
        public static void DicomFilesToZipArchive(IEnumerable<DicomFileAndPath> dicomFiles, Stream stream)
        {
            using (var zip = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                var dicomCount = 0;
                foreach (var file in dicomFiles)
                {
                    dicomCount++;
                    var dicomFileStream = new MemoryStream();
                    file.File.Save(dicomFileStream);
                    dicomFileStream.Seek(0, SeekOrigin.Begin);
                    var zipName =
                        string.IsNullOrWhiteSpace(file.Path)
                        ? $"DicomFile{dicomCount}.dcm"
      