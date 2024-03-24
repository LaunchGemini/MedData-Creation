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
            else
            {
                throw new Exception("Point3D struct: data size does not match point size");
            }
        }

        /// <summary>
        /// Returns a zero Point3D.
        /// </summary>
        /// <returns>The zero Point3D.</returns>
        public static Point3D Zero()
        {
            return new Point3D(0,0,0);
        }

        public double this[int row]
        {
            get { return Data[row]; }
            set
            {
                switch (row)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                }
            }
        }

        public double[] Data
        {
            get
            {
                return new [] { X, Y, Z };
            }

            set
            {
                if (value.Length == 3)
                {
                    X = value[0];
                    Y = value[1];
                    Z = value[2];
                }
                else
                {
                    throw new Exception("Point3D struct: data size does not match point size");
                }
            }
        }

        public static Point3D operator -(Point3D a)
        {
            return new Point3D(-a.X, -a.Y, -a.Z); 
        }

        public static Point3D operato