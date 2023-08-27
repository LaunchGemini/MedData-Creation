///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using MedLib.IO;
    using InnerEye.CreateDataset.Common;
    using InnerEye.CreateDataset.Volumes;
    using itk.simple;
    using InnerEye.CreateDataset.Data;
    using InnerEye.CreateDataset.Math;
    using MoreLinq;

    public static class ConvertDicomToNifti
    {
        /// <summary>
        /// The name of a file that is written during dataset creation, and contains info about how dataset
        /// creation was done.
        /// </summary>
     