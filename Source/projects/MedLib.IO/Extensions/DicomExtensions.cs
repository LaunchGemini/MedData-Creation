///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace MedLib.IO.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Dicom;
    using InnerEye.CreateDataset.Volumes;    
    using MedLib.IO.RT;
    using MedLib.IO.Readers;

    public static class DicomExtensions
    {
        /// <summary>
        /// DICOM Code String (CS) String literal for the types of contours we produce
        /// </summary>
        public const string ClosedPlanarString = "CLOSED_PLANAR";

        public static string GetStringOrEmpty(this DicomDataset ds, DicomTag tag)
        {
            return ds.GetSingleValueOrDefaul