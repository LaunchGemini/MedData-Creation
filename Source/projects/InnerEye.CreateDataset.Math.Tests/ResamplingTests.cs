///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Math.Tests
{
    using System.Linq;
    using InnerEye.CreateDataset.Contours;
    using InnerEye.CreateDataset.Math;
    using InnerEye.CreateDataset.Volumes;

    using NUnit.Framework;

    [TestFixture]
    public class ResamplingTests
    {
        [TestCase(0.5)]
        [TestCase(0.8)]
        [TestCase(0.6)]
        [TestCase(1.0)]
        [TestCase(2.0)]

        public void CheckResamplingNearestAndLinear(double factorXY)
        {
            int dimX = 100;
            int dimY = 100;
            int dimZ = 3;
            int spacingX = 1;
            int spacingY = 1;
            int spacingZ = 3;
            var volume = new Volume3D<int>(dimX, dimY, dimZ, spacingX, spacingY, spacingZ);
            FillHalf(volume);
            PrintToPng(volume, $"input{factorXY}");
            var output = volume.ResampleNearest((int)(dimX * factorXY), (int)(dimY * factorXY), dimZ, 0);
            PrintToPng(output, $"outputNearest{factorXY}");

            var expectedReduction = volume.Array.Sum() * factorXY * factorXY;

            Assert.AreEqual(expectedReduction, output.Array.Sum(), $"{output.Array.Sum()}");

            output = volume.ResampleLinear((int)(dimX * factorXY), (int)(dimY * factorXY), dimZ);
            PrintToPng(output, $"outputLinea{factorXY}");
            Assert.AreEqual(expectedReduction, output.Array.Sum(), $"{output.Array.Sum()}");
        }

        private static void PrintToPng(Volume3D<int> output, st