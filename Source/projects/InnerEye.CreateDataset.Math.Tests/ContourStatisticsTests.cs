///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Math.Tests
{
    using InnerEye.CreateDataset.Contours;
    using InnerEye.CreateDataset.Volumes;

    using NUnit.Framework;

    using System.Linq;

    [TestFixture]
    public class ContourStatisticsTests
    {
        private Volume3D<T> CreateVolume<T>(T