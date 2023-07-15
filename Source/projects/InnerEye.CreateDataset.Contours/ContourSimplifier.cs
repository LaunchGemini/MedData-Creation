///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Contours
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Drawing;
    using PointInt = System.Drawing.Point;

    /// <summary>
    /// Class to simplify contours induced by segmentation masks defined on a raster grid.
    /// Such contours can be described by sequences of one-unit-long moves with direction
    /// expressed relative to the preceding move. For illustration, consider the following open
    /// contour:
    /// x0
    /// |
    /// x1
    /// |
    /// x2--x3--x4
    ///          |
    ///          x5
    /// This contour may be defined by 6 vertices x0, x1, etc and 5 edges. Equivalently
    /// the contour may be defined using a starting position (x0) and starting direction
    /// (down the page) and a sequence of moves (in {Forwards, Left, Right}:
    /// FFLFR
    /// The appeal of this representation is invariance to local orientation. This motivates
    /// the contour simplification approach used here, which works by replacing patterns of moves
    /// described by sequences of characters in a string (e.g. LRL) with more complex paths defined
    /// (in a direction-relative way) in the continuous 2D domain.
    /// </summary>
    public static class ContourSimplifier
    {
        private static readonly Tuple<string, PointF[]>[] PatternToFragmentMap = new Tuple<string, PointF[]>[]
        {
            // NB These fragments are defined in order of decreasing priorty.
            Tuple.Create("FRF", new PointF[] { new PointF(0, -0.5f), new PointF(0, 0.1f), new PointF(-0.9f, 1), new PointF(-1.5f, 1) }),
            Tuple.Create("FLF", new PointF[] { new PointF(0, -0.5f), new PointF(0, 0.1f), new PointF(0.9f, 1), new PointF(1.5f, 1) }),
            Tuple.Create("RFL", new PointF[] { new PointF(0, -0.5f), new PointF(-2, 0.5f) }),
            Tuple.Create("LFR", new PointF[] { new PointF(0, -0.5f), new PointF(2, 0.5f) }),
            Tuple.Create("RL", new PointF[] { new PointF(0, -0.5f), new PointF(-1, 0.5f) }),
            Tuple.Create("LR", new PointF[] { new PointF(0, -0.5f), new PointF(1, 0.5f) }),
            Tuple.Create("R", new PointF[] { new PointF(0, -0.5f), new PointF(-0.5f, 0f) }),
            Tuple.Create("L", new PointF[] { new PointF(0, -0.5f), new PointF(0.5f, 0f) })
        };

        /// <summary>
        /// Simplify a contour defined by a string of moves.
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="d0"></param>
        /// <param name="turns">String of characters in {F,R,L} defining a sequence of one unit long moves.</param>
        /// <returns>Simplified, smoother polygon with fewer vertices.</returns>
        public static PointF[] Simplify(PointInt x0, PointInt d0, string turns)
        {
            var isClosed = TraceContour(x0, d0, turns, out PointInt[] points, out PointInt[] directions);
            var sequences = new PointF[turns.Length][];

            foreach (var pattern in PatternToFragmentMap)
            {
                MatchFragments(turns, isClosed, pattern.Item1, pattern.Item2, sequences);
            }

            return ExpandFragments(points, directions, sequences);
        }

        /// <summary>
        /// Walk around a contour defined by a sequence of one pixel long moves.
        /// </summary>
        /// <param name="x0">First vertex position.</param>
        /// <param name="d0">Unit vector describing initial direction of motion.</param>
        /// <param name="recipe">String with characters in {F,L,R} defining a seqeunce of moves required to build an open or closed polygon.</param>
        /// <param name="points">Output polygon vertices</param>
        /// <param name="directions">Output edge directions</param>
        /// <returns></returns>
        public static bool TraceContour(PointInt x0, PointInt d0, string recipe, out PointInt[] points, out PointInt[] directions)
        {
            points = new PointInt[recipe.Length + 1];
            points[0] = x0;

            directions = new PointInt[recipe.Length + 1];

            directions[0] = d0;

            var simplified = new List<PointInt>();
            simplified.Add(x0);

            var p = new PointInt(0, 0);

            // we evaluate one more PointF than is necessary, to determine whether contour is properly closed
            for (int i = 0; i < recipe.Length + 1; i++)
            {
                var d = recipe[i >= recipe.Length ? i - recipe.Length : i];

                // identity
                int r00 = 1;
                int r01 = 0;
                int r10 = 0;
                int r11 = 1;

                // left turn
                if (d == 'L')
                {
                    r00 = 0;
                    r01 = -1;
                    r10 = 1;
                    r11 = 0;
                }

                // right turn
                else if (d == 'R')
                {
                    r00 = 0;
                    r01 = 1;
                    r10 = -1;
                    r11 = 0;
                }

                // Update delta
                d0 = new PointInt(r00 * d0.X + r01 * d0.Y, r10 * d0.X + r11 * d0.Y);
                p = new PointInt(x0.X + d0.X, x0.Y + d0.Y);

                if (i + 1 <= recipe.Length)
                {
                    points[i + 1] = p;
                    directions[i + 1] = d0;
                }

                x0 = p;
            }

            // Test for a closed contour... Must start with the turn reference frame
            // defined relative to the last edge and start and end at the same PointF.
            return points[0].Equals(points[points.Length - 1]) && points[1].Equals(p);
        }

        /// <summary>
        /// Removes repeated or intermediate points that are not necessary on a polygon
        /// Without changing the shape
        /// </summary>
        /// <par