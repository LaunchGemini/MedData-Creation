///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace MedLib.IO
{
    using System;

    using Dicom;

    using MedLib.IO.Models.DicomRt;
    using MedLib.IO.Extensions;

    using InnerEye.CreateDataset.Volumes;

    /// <summary>
    /// Helpers for Dicom Rt files
    /// </summary>
    public static class DicomRtHelpers
    {
        /// <summary>
        /// Converts a Dicom file to a radiotherapy structure set.
        /// </summary>
        