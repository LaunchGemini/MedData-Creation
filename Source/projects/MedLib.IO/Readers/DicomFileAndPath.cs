///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿
namespace MedLib.IO.Readers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using Dicom;

    /// <summary>
    /// Read only tuple of DicomFile and its original path
    /// </summary>
    public sealed class DicomFileAndPath
    {
        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="dicomFile">The value to use for the File property of the object.</param>
        /// <param name="path">The value to use for the Path property of the object.</param>
        public DicomFileAndPath(DicomFile dicomFile, string path)
        {
            File = dicomFile;
            Path = path;
        }

        /// <summary>
        /// Create a DicomFileAndPath from a path to a folder
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DicomFileAndPath SafeCreate(string path)
        {
            try
            {
                return new D