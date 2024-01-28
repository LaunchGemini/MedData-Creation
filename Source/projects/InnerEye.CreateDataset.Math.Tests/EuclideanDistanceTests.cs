///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace MedLib.IO.Tests
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using InnerEye.CreateDataset.Math;
    using InnerEye.CreateDataset.Volumes;

    using NUnit.Framework;

    [TestFixture]
    public class EuclideanDistanceTests
    {
        [TestCase(@"LoadTest1\\triangle.png")]
        public void EuclideanDistanceTest(string filename)
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", filename);
            string resultPath = Path.GetDirectoryName(filePath) + @"\\result.png";
            
            var image = new Bitmap(filePath);
            byte[] mask = ImageToByte(image);

            var mask2d = new InnerEye.CreateDataset.Volumes.Volume2D<byte>(mask, image.Width, image.Height, 1, 1, new Point2D(), Matrix2.CreateIdentity());

            var contours = mask2d.ContoursWithHoles();
            mask2d.Fill(contours, (byte)1);

            var contourMask = new InnerEye.CreateDataset.Vol