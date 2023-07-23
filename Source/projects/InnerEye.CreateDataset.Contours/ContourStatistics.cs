///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Contours
{
    using System;
    using InnerEye.CreateDataset.Volumes;

    /// <summary>
    /// Contains statistics about the voxels inside of a contour.
    /// </summary>
    public class ContourStatistics
    {
        /// <summary>
        /// Creates a new instance of the class.
        /// </summary>
        /// <param name="sizeInCubicCentimeters"></param>
        /// <param name="voxelValueMean"></param>
        /// <param name="voxelValueStandardDeviation"></param>
        public ContourStatistics(double sizeInCubicCentimeters, double voxelValueMean, double voxelValueStandardDeviation)
        {
            SizeInCubicCentimeters = sizeInCubicCentimeters;
            VoxelValueMean = voxelValueMean;
            VoxelValueStandardDeviation = voxelValueStandardDeviation;
        }

        /// <summary>
        /// Gets the volume of the region enclosed by the contour, in cubic centimeters.
   