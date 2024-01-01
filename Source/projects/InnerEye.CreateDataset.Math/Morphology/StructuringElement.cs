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
 