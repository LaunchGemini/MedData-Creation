///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Math
{
    using System.Threading.Tasks;
    using InnerEye.CreateDataset.Volumes;
    using InnerEye.CreateDataset.Contours;

    public static class ContourGeometryOperations
    {
        public static Volume3D<byte> GeometryUnion(this ContoursPerSlice contour1, ContoursPerSlice contour2, Volume3D<s