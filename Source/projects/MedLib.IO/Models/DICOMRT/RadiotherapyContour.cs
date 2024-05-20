///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace MedLib.IO.Models.DicomRt
{
    using MedLib.IO.RT;
    using InnerEye.CreateDataset.Contours;

    /// <summary>
    /// A RadiotherapyContour is a set of contours forming a single structure within
    /// an RT Structure set. This class brings together:
    ///  * Contour information defining the geometry of the structure
    ///  * An obser