///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

namespace InnerEye.CreateDataset.Volumes
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public struct Point3D
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3D(IReadOnlyList<double> data)
        {
            if (data.Count == 3)
            {
                X = data[0];
                Y = data[1];
                Z = data[2];
            }
            e