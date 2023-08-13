///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Contours
{
    using System;
    using System.Drawing;

    /// <summary>
    /// Contains arithmetic operations for the <see cref="PointF"/> class.
    /// </summary>
    public static class PointExtensions
    {
        /// <summary>
        /// Computes a point that is the coordinate-wise subtraction of the arguments.
        /// In the result, X will be equal to this.X - p2.X.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static PointF Subtract (this PointF p1, PointF p2)
            => new PointF(p1.X - p2.X, p1.Y - p2.Y);

        /// <summary>
        /// Computes a point that is the coordinate-wise addition of the arguments.
        /// In the result, X will be equal to this.X + p2.X.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static PointF Add(this PointF p1, PointF p2)
            => new PointF(p1.X + p2.X, p1.Y + p2.Y);

        /// <summary>
        /// Computes the dot product of two points, interpreting them as vectors:
        /// For vectors (x1, y1) and (x2, y2), the dot product is
        /// x1 * x2 + y1 + y2.
        /// </summary>
        /// <param name="p1"></param>
        /// <pa