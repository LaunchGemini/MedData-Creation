///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Volumes
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    [Obsolete("All contour-related code should move to using the new classes in the InnerEye.CreateDataset.Contours namespace.")]
    public static class PolygonHelpers
    {
        public static double CalculateDistance(double point1X, double point1Y, double point2X, double point2Y)
        {
            return Math.Pow(point2X - point1X, 2) + Math.Pow(point2Y - point1Y, 2);
        }

        public static bool TryGetClosestPointOnPolygon(this System.Drawing.Point[] polygon, Point currentPosition, out Tuple<double, System.Drawing.Point> closestPoint)
        {
            var distance = double.MaxValue;
            var bestPoint = new System.Drawing.Point();

            if (polygon == null || polygon.Length == 0)
            {
                closestPoint = Tuple.Create(distance, bestPoint);

                return false;
            }

            foreach (var point in polygon)
            {
                var tempDistance = CalculateDistance(currentPosition.X, currentPosition.Y, point.X, point.Y);

                if (tempDistance < distance)
                {
                    bestPoint = point;
                    distance = tempDistance;
                }
            }

            closestPoint = Tuple.Create(distance, bestPoint);

            return true;
        }

        public static bool TryGetClosestPointOnPolygon(Point[] polygon, Point currentPosition, out Tuple<double, Point> closestPoint)
        {
            if (polygon == null || polygon.Length == 0)
    