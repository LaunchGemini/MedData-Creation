///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace MedLib.IO.RT
{
    using Dicom;
    using MedLib.IO.Extensions;

    /// <summary>
    /// Encodes important Type 1 tags from the SOP Common module
    /// <see cref="http://dicom.nema.org/medical/dicom/current/output/chtml/part03/sect_C.12.html"/>
    /// </summary>
    public class DicomSOPCommon
    {
        /// <summary>
        /// The SOP Class UID of the parent instance. This uniquely and authoratively defines
        /// the modules expected within a Dicom dataset. Type 1, VR: UI
        /// </summary>
        public string SopClassUid { get; }

        /// <summary>
        /// A unique identifier fo