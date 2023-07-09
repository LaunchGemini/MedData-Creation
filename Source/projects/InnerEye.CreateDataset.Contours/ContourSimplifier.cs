///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Contours
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Drawing;
    using PointInt = System.Drawing.Point;

    /// <summary>
    /// Class to simplify contours induced by segmentation masks defined on a raster grid.
    /// Such contours can be described by sequences of one-unit-long moves with