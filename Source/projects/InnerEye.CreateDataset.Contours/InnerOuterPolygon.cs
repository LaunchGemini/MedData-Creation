///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Contours
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Describes a region of a binary mask via the points that make up the outer rim of a contiguous region,
    /// and possibly the inner rim if the region has holes (doughnut shape).
    /// </summary>
    public class InnerOuterPolygon
    {
        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="outer">The points that make up the outer rim of the region, traversed clockwise.</param>
        public InnerOuterPolygon(PolygonPoints outer)
        {
            Outer = outer ?? throw new ArgumentNullException(nameof(outer));
            Inner = new List<PolygonPoints>();
            TotalPixels = outer.VoxelCounts.Total;
        }

        /// <summary>
        /// Gets the points that describe the outer rim of region, traversed in clockwise order, with no gaps
        /// (each point must be in the 8-neighborhood of its successor), wrapping around at the end.
        /// </summary>
        public PolygonPoints Outer { get; }

        /// <summary>
        /// Gets the points that describe the inner rim of region, traversed in counter-clockwise order, with no gaps
        /// (each point must be in the 8-neighborhood of its successor), wrapping around at the end. There can
        /// be multiple such inner rims if a contour has multiple "holes".
        /// </summary>
        public List<PolygonPoints> Inner { get; }

        /// <summary>
        /// Gets the number of pixels tha