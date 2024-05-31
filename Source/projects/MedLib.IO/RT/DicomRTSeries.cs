///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace MedLib.IO.RT
{
    using Dicom;

    using Extensions;
    using System;
    using System.Globalization;

    /// <summary>
    /// Encodes  parts of the RTSeries DICOM module we use.
    /// see http://dicom.nema.org/medical/Dicom/current/output/chtml/part03/sect_C.8.8.html
    /// </summary>
    public class DicomRTSeries
    {
        public const string RtModality = "RTSTRUCT";

        public string Modality { get; }

        public string SeriesInstanceUID
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set the Series Description of this RT Series
        /// </summary>
        public string SeriesDescription { get; set;  }

        public DicomRTSeries(string modality, s