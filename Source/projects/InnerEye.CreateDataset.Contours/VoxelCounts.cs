///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Contours
{
    /// <summary>
    /// Contains statistics about the set of voxel values found in a region.
    /// </summary>
    public class VoxelCounts
    {
        /// <summary>
        /// Creates a new instance of the class, with all counters set to 0.
        /// </summary>
        public VoxelCounts()
        {
            Foreground = 0;
            Other = 0;
        }

        /// <summary>
        /// Creates a new instance of the class, with all counters set to the given values.
        /// </summar