///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Contours
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;

    /// <summary>
    /// Contains methods for distance computation to polygons.
    /// </summary>
    public static class PolygonHelpers
    {
        /// <summary>
        /// Computes the square of the Euclidean distance between the two points.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static double CalculateDistance(PointF point1, PointF point2)
        {
            var dx = point1.X - point2.X;
            var dy = point1.Y - point2.Y;
            return dx * dx + dy * dy;
        }

        /// <summary>
        /// Computes the point in the polygon that is closest to the point given in <paramref name="currentPosition"/>.
        /// Returns false if the polygon is null or does not contain any points. If the function returns true,
        /// <paramref name="closestPoint"/> contains the squared Euclidean distance to the closest point in the polygon,
        /// and the closest polygon point itself.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="currentPosition"></param>
        /// <param name="closestPoint"></param>
        /// <returns></returns>
        public static bool TryGetClosestPointOnPolygon(IEnumerable<Point> polygon, PointF currentPosition, out Tuple<double, Point> closestPoint)
        {
            var distance = double.MaxValue;
            var bestPoint = new Point(0, 0);
            var any = false;
            if (polygon != null)
            {
                foreach (var point in polygon)
                {
                    any = true;
                    var tempDistance = CalculateDistance(currentPosition, point);
                    if (tempDistance < distance)
                    {
                        bestPoint = point;
                        distance = tempDistance;
                    }
                }
            }
            closestPoint = Tuple.Create(distance, bestPoint);
            return any;
        }

        /// <summary>
        /// Computes the point in the polygon that is closest to the point given in <paramref name="currentPosition"/>.
        /// This is legacy code for which no tests existed. After finding the closest point, some additional logic is applied,
        /// based on how close polygon segments get to the point given in <paramref name="currentPosition"/>.
        /// Returns false if the polygon is null or does not contain any points. 
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="currentPosition"></param>
        /// <param name="closestPoint"></param>
        /// <returns></returns>
        public static bool TryGetClosestPointOnPolygon(IReadOnlyList<PointF> polygon, PointF currentPosition, out Tuple<double, PointF> closestPoint)
        {
            if (polygon == null || polygon.Count == 0)
            {
                closestPoint = Tuple.Create(0d, currentPosition);
                return false;
            }

            var currentPoint = polygon[0];
            var distance = CalculateDistance(currentPoint, curr