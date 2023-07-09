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
            Tuple.Create("FLF", new PointF[] { new PointF(0, -0.5f), new PointF(0, 0.1f), new PointF(0.9f, 1), new P