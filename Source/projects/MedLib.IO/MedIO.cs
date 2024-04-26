///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace MedLib.IO
{
    using Dicom;
    using MedLib.IO.Extensions;
    using InnerEye.CreateDataset.Volumes;
    using Models;
    using Models.DicomRt;
    using Readers;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Writers;
    using static MedLib.IO.NiftiIO.NiftiInternal;

    public struct VolumeLoaderResult
    {
        public VolumeLoaderResult(string seriesId, MedicalVolume volume, Exception error, IReadOnlyList<string> warnings)
        {
            SeriesUid = seriesId;
            Volume = volume;
            Error = error;
            Warnings = warnings;
        }

        /// <summary>
        /// The DICOM series UID that the volume belongs to
        /// </summary>
        public string SeriesUid { get; }

        /// <summary>
        /// The medical volume or null if Error != null
        /// </summary>
        public MedicalVolume Volume { get; }

        /// <summary>
        /// Contains the first exception that occurred attempting to load a volume => volume = null
        /// </summary>
        public Exception Error { get; }

        /// <summary>
        /// A list of warnings that occured loading the volume => volume != null
        /// </summary>
        public IReadOnlyList<string> Warnings { get; }
    }

    /// <summary>
    /// Contains methods to load and save different representations of medical volumes, working
    /// with Dicom, Nifti and HDF5 files.
    /// </summary>
    public class MedIO
    {
        /// <summary>
        /// The suffix for files that contain uncompressed Nifti data.
        /// </summary>
 