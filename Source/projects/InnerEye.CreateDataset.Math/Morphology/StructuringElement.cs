///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿using InnerEye.CreateDataset.Volumes;
using System.Collections.Generic;

namespace InnerEye.CreateDataset.Math.Morphology
{
    /// <summary>
    ///  An ellipsoid structuring element (SE) for use in morphological operations
    ///  
    ///  1) We create a mask (a cuboid with radius equal to the dilation/erosion radius in each dimension) to hold the ellipsoid
    ///  and use the equation of the ellipsoid to mark points that lie inside it as foreground
    ///  2) We extract all of the surface points (points that lie on the edge or have a BG neighbor in their 1-connectivity radius) and store these points 
    ///  as relative offsets from the center of the structuring element (to be used to paint arbitrary points on arbitrary volumes)
    ///  
    /// </summary>
    public class StructuringElement
    {
        /// <summary>
        /// This is a binary mask representing an ellipsoid
        /// </summary>
        public Volume3D<byte> Mask { get; }

        /// <summary>
        /// The absolute coordinates of the mask center in absolute coordinate space
        /// </summary>
        public (int x, int y, int z) AbsoluteMaskCenter { get; }

        // set of surface points (ie: foreground points that are on the edge of the volume or have a BG neighbor in their 1-connectivity radius)
        protected HashSet<(int x, int y, int z)> SurfacePointsRelativeToAbsoluteCenter { get; }

        public StructuringElement(int xNumberOfPixels, int yNumberOfPixels, int zNumberOfPixels)
        {
            // Prepare the SE mask
            Mask = CreateMask(xNumberOfPixels, yNumberOfPixels, zNumberOfPixels);
            AbsoluteMaskCenter = (Mask.DimX / 2, Mask.DimY / 2, Mask.DimZ / 2);
            SurfacePointsRelativeToAbsoluteCenter = ExtractSurfacePointsRelativeToAbsoluteCenter();
        }

        /// <summary>
        /// For a given point in absolute coordinate space of an input volume, this function paints all of the points centered at the provided x,y,z coordinates
        /// that fall inside structuring element mask with the label value, taking into account a restriction volume
        ///
        /// <param name="input">Volume to paint</param>
        /// <param name="restriction">This is to stop the volume growing outside a region. E.g. dilating a structure and stoping it from growing outside the skin</param>
        /// <param name="label">Value to paint</param>
        /// <param name="x">x-coordinate of the point to center the structuring element on</param>
        /// <param name="y">y-coordinate of the point to center the structuring element on</param>
        /// <param name="z">z-coordinate of the point to center the structuring element on</param>
        /// </summary>
        public void PaintAllForegroundPointsOntoVolume(Volume3D<byte> input, Volume3D<byte> restriction, byte label, int x, int y, int z)
        {
            Mask.IterateSlices(p =>
            {
                if (Mask[p.x, p.y, p.z] == ModelConstants.MaskForegroundIntensity)
                {
                    // convert absolute mask points to relative points used to offset the input absolute points on the input volume
                    int maskPointOffsetFromMaskCenterX = p.x - AbsoluteMaskCenter.x;
                    int maskPointOffsetFromMaskCenterY = p.y - AbsoluteMaskCenter.y;
                    int maskPointOffsetFromMaskCenterZ = p.z - AbsoluteMaskCenter.z;
                    // map the relative mask points to absolute coordinates on the results volume for painting
                    int resultOffsetX = x + maskPointOffsetFromMaskCenterX;
                    int resultOffsetY = y + maskPointOffsetFromMaskCenterY;
                    int resultOffsetZ = z + maskPointOffsetFromMaskCenterZ;

                    PaintPointOntoVolume(input, restriction, label, resultOffsetX, resultOffsetY, resultOffsetZ);
                }
            });
        }

        /// <summary>
        /// For a given point in absolute coordinate space of an input volume, this function paints all of the surface points centered at the provided x, y, z coordinates
        /// of structuring element mask with the label value, taking into account a restriction volume
        ///
        /// <param name="input">Volume to paint</param>
        /// <param name="restriction">This is to stop the volume growing outside a region. E.g. dilating a structure and stoping it from growing outside the skin</param>
        /// <param name="label">Value to paint</param>
        /// <param name="x">x-coordinate of the point to center the structuring element on</param>
        /// <param name="y">y-coordinate of the point to center the structuring element on</param>
        /// <param name="z">z-coordinate of the point to center the structuring element on</param>
        /// </summary>
        public void PaintSurfacePointsOntoVolume(Volume3D<byte> input, Volume3D<byte> restriction, byte label, int x, int y, int z)
        {
            foreach (var sp in SurfacePointsRelativeToAbsoluteCenter)
            {
                // We need to add the offset of the SE surface points from the center of the SE to the current surface point in order to 
                // re-center the points around the current surface point
                int resultOffsetX = x + sp.x;
                int resultOffsetY = y + sp.y;
                int resultOffsetZ = z + sp.z;
                PaintPointOntoVolume(input, restriction, label, resultOffsetX, resultOffsetY, resultOffsetZ);
            }
        }

        private void PaintPointOntoVolume(Volume3D<byte> input, Volume3D<byte> restriction, byte label, int x, int 