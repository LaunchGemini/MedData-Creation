
///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

namespace InnerEye.CreateDataset.Volumes
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows;

    using Bitmap = System.Drawing.Bitmap;
    using Rectangle = System.Drawing.Rectangle;

    [Obsolete("All contour-related code should move to using the new classes in the InnerEye.CreateDataset.Contours namespace.")]
    public static class FillPolygonHelpers
    {
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
        public static void FloodFillHoles(byte[] mask, int dimX, int dimY, int dimZ, int sliceZ, byte foregroundId, byte backgroundId)
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
                {
                    for (var x = left; x < right + 1; x++)
                    {
                        if (mask[x + y * dimX + sliceZ * dimXy] == backgroundId)
                        {
                            FloodFill(mask, dimX, dimY, sliceZ, new System.Drawing.Point(x, y), tempBackgoundId, backgroundId, bounds);
                        }
                    }
                }
                // Middle rows
                else
                {
                    if (mask[left + y * dimX + sliceZ * dimXy] == backgroundId)
                    {
                        FloodFill(mask, dimX, dimY, sliceZ, new System.Drawing.Point(left, y), tempBackgoundId, backgroundId, bounds);