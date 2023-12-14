///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Math
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;

    using InnerEye.CreateDataset.Contours;
    using InnerEye.CreateDataset.Volumes;

    /// <summary>
    /// Contains extension methods for working with contours.
    /// </summary>
    public static class ContourExtensions
    {
        /// <summary>
        /// Extracts contours from all slices of the given volume, searching for the given foreground value.
        /// Contour extraction will take holes into account.
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="foregroundId">The voxel value that should be used in the contour search as foreground.</param>
        /// <param name="axialSmoothingType">The smoothing that should be applied when going from a point polygon to
        /// contours. This will only affect axial slice, for other slice types no smoothing will be applied.
        /// <param name="sliceType">The type of slice that should be used for contour extraction.</param>
        /// <param name="filterEmptyContours">If true, contours with no points are not extracted.</param>
        /// <param name="regionOfInterest"></param>
        /// <returns></returns>
        public static ContoursPerSlice ContoursWithHolesPerSlice(
            this Volume3D<byte> volume,
            byte foregroundId = ModelConstants.MaskForegroundIntensity,
            SliceType sliceType = SliceType.Axial,
            bool filterEmptyContours = true,
            Region3D<int> regionOfInterest = null,
            ContourSmoothingType axialSmoothingType = ContourSmoothingType.Small)
            => ExtractContours.ContoursWithHolesPerSlice(volume, foregroundId, sliceType, filterEmptyContours, regionOfInterest, axialSmoothingType);

        /// <summary>
        /// Extracts the contours around all voxel values in the volume that have the given foreground value.
        /// All other voxel values (zero and anything that is not the foreground value) is treated as background.
        /// Contour extraction will take account of holes and inserts, up to the default nesting level.
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="foregroundId">The voxel value that should be used as foreground in the contour search.</param>
        /// <param name="smoothingType">The smoothing that should be applied when going from a point polygon to
        /// a contour.</param>
        /// <returns></returns>
        public static IReadOnlyList<ContourPolygon> ContoursWithHoles(
            this Volume2D<byte> volume,
            byte foregroundId = 1,
            ContourSmoothingType smoothingType = ContourSmoothingType.Small)
            => ExtractContours.ContoursWithHoles(volume, foregroundId, smoothingType);

        /// <summary>
        /// Extracts the contours around all voxel values in the volume that have the given foreground value.
        /// All other voxel values (zero and anything