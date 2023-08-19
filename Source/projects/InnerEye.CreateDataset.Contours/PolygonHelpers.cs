///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Contours
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    /// <summary>
    /// Contains methods for distance computation to polygons.
    /// </summary>
    public static class PolygonHelpers
    {
        /// <summary>
        /// Computes the square of the Euclidean distance between the two points.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns><