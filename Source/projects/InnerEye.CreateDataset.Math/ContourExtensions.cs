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
        /// All other voxel values (zero and anything that is not the foreground value) is treated as background.
        /// Contour extraction will not take account of holes, and hence only return the outermost
        /// contour around a region of interest.
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="foregroundId">The voxel value that should be used as foreground in the contour search.</param>
        /// <param name="smoothingType">The smoothing that should be applied when going from a point polygon to
        /// a contour.</param>
        /// <returns></returns>
        public static IReadOnlyList<ContourPolygon> ContoursFilled(
            this Volume2D<byte> volume,
            byte foregroundId = 1,
            ContourSmoothingType smoothingType = ContourSmoothingType.Small)
            => ExtractContours.ContoursFilled(volume, foregroundId, smoothingType);

        /// <summary>
        /// Modifies the present volume by filling all points that fall inside of the given contours,
        /// using the provided fill value. Contours are filled on axial slices.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="volume">The volume that should be modified.</param>
        /// <param name="contours">The contours per axial slice.</param>
        /// <param name="value">The value that should be used to fill all points that fall inside of
        /// the given contours.</param>
        public static void Fill<T>(this Volume3D<T> volume, ContoursPerSlice contours, T value)
            => FillPolygon.FillContours(volume, contours, value);

        /// <summary>
        /// Modifies the present volume by filling all points that fall inside of the given contours,
        /// using the provided fill value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="volume">The volume that should be modified.</param>
        /// <param name="contours">The contours that should be used for filling.</param>
        /// <param name="value">The value that should be used to fill all points that fall inside of
        /// any of the given contours.</param>
        public static void Fill<T>(this Volume2D<T> volume, IEnumerable<ContourPolygon> contours, T value)
            => FillPolygon.FillContours(volume, contours, value);

        /// <summary>
        /// Applies flood filling to all holes in all Z slices of the given volume.
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="foregroundId"></param>
        /// <param name="backgroundId"></param>
        public static void FillHoles(
            this Volume3D<byte> volume,
            byte foregroundId = ModelConstants.MaskForegroundIntensity,
            byte backgroundId = ModelConstants.MaskBackgroundIntensity)
            => FillPolygon.FloodFillH