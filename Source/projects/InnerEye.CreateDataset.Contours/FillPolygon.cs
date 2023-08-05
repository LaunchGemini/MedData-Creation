
///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Contours
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Threading.Tasks;
    using InnerEye.CreateDataset.Volumes;
    using PointInt = System.Drawing.Point;

    /// <summary>
    /// Contains helper methods for doing polygon filling.
    /// </summary>
    public static class FillPolygon
    {
        private enum State
        {
            Background, Bottom, Top, Inside
        }

        /// <summary>
        /// Modifies the present volume by filling all points that fall inside of the given contours,
        /// using the provided fill value. Contours are filled on axial slices.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="volume">The volume that should be modified.</param>
        /// <param name="contours">The contours per axial slice.</param>
        /// <param name="value">The value that should be used to fill all points that fall inside of
        /// the given contours.</param>
        public static void FillContours<T>(Volume3D<T> volume, ContoursPerSlice contours, T value)
        {
            foreach (var contourPerSlice in contours)
            {
                foreach (var contour in contourPerSlice.Value)
                {
                    Fill(contour.ContourPoints, volume.Array, volume.DimX, volume.DimY, volume.DimZ, contourPerSlice.Key, value);
                }
            }
        }

        /// <summary>
        /// Modifies the present volume by filling all points that fall inside of the given contours,
        /// using the provided fill value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="volume">The volume that should be modified.</param>
        /// <param name="contours">The contours that should be used for filling.</param>
        /// <param name="value">The value that should be used to fill all points that fall inside of
        /// any of the given contours.</param>
        public static void FillContours<T>(Volume2D<T> volume, IEnumerable<ContourPolygon> contours, T value)
        {
            Parallel.ForEach(
                contours,
                contour =>
                {
                    FillContour(volume, contour.ContourPoints, value);
                });
        }

        /// <summary>
        /// Fills the contour using high accuracy (point in polygon testing).
        /// </summary>
        /// <typeparam name="T">The volume type.</typeparam>
        /// <param name="volume">The volume.</param>
        /// <param name="contourPoints">The points that defines the contour we are filling.</param>
        /// <param name="region">The value we will mark in the volume when a point is within the contour.</param>
        /// <returns>The number of points filled.</returns>
        public static int FillContour<T>(Volume2D<T> volume, PointF[] contourPoints, T value)
        {
            return Fill(contourPoints, volume.Array, volume.DimX, volume.DimY, 0, 0, value);
        }

        /// <summary>
        /// Applies flood filling to all holes in all Z slices of the given volume.
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="foregroundId"></param>
        /// <param name="backgroundId"></param>
        public static void FloodFillHoles(
            Volume3D<byte> volume,
            byte foregroundId = ModelConstants.MaskForegroundIntensity,
            byte backgroundId = ModelConstants.MaskBackgroundIntensity)
        {
            Parallel.For(0, volume.DimZ, sliceZ =>
            {
                FloodFillHoles(volume.Array, volume.DimX, volume.DimY, volume.DimZ, sliceZ, foregroundId, backgroundId);
            });
        }

        /// <summary>
        /// Applies flood filling to all holes in the given volume.
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="foregroundId"></param>
        /// <param name="backgroundId"></param>
        public static void FloodFillHoles(
            Volume2D<byte> volume,
            byte foregroundId = ModelConstants.MaskForegroundIntensity,
            byte backgroundId = ModelConstants.MaskBackgroundIntensity)
        {
            FloodFillHoles(volume.Array, volume.DimX, volume.DimY, 0, 0, foregroundId, backgroundId);
        }

        /// <summary>
        /// Uses a scan line implementation of flood filling to fill holes within a mask.
        /// </summary>
        /// <param name="mask">The mask we are flood filling.</param>
        /// <param name="dimX">The X-Dimension length of the mask.</param>
        /// <param name="dimY">The Y-Dimension length of the mask.</param>
        /// <param name="dimZ">The Z-Dimension length of the mask.</param>
        /// <param name="sliceZ">The Z dimension slice we are flood filling holes on.</param>
        /// <param name="foregroundId">The foreground ID.</param>
        /// <param name="backgroundId">The background ID.</param>
        public static void FloodFillHoles(
            byte[] mask,
            int dimX,
            int dimY,
            int dimZ,
            int sliceZ,
            byte foregroundId,
            byte backgroundId)
        {
            if (mask == null)
            {
                throw new ArgumentNullException(nameof(mask));
            }

            if (foregroundId == backgroundId)
            {
                throw new ArgumentException("The foreground ID cannot be the same as the background ID");
            }

            if (mask.Length != dimX * dimY * (dimZ <= 0 ? 1 : dimZ))
            {
                throw new ArgumentException("The X or Y dimension length is incorrect.");
            }

            var bounds = GetBoundingBox(mask, foregroundId, dimX, dimY, dimZ, sliceZ);

            var left = bounds.X;
            var right = bounds.X + bounds.Width;
            var top = bounds.Y;
            var bottom = bounds.Y + bounds.Height;

            var tempBackgoundId = (byte)(foregroundId > backgroundId ? foregroundId + 1 : backgroundId + 1);
            var dimXy = dimX * dimY;

            // Start by flood filling from the outer edge of the mask
            for (var y = top; y < bottom + 1; y++)
            {
                // Top or bottom rows
                if (y == top || y == bottom)