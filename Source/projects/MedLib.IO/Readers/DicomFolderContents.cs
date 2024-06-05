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