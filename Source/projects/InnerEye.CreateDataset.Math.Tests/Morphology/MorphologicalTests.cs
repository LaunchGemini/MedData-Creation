///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Math.Tests.Morphology
{
    using System.Collections.Generic;
    using System.Linq;
    using MedLib.IO;
    using InnerEye.CreateDataset.Math;
    using InnerEye.CreateDataset.Volumes;
    using NUnit.Framework;
    using System.IO;
    using System;

    [TestFixture]
    public class MorphologicalTests
    {
        private static string BaseFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            @"Morphology\TestData\Structures");

        [Test]
        public void IntersectTest()
        {
            var structure1 = MedIO.LoadNiftiAsByte(BaseFolder + @"\Structure1.nii.gz");
            var structure2 = MedIO.LoadNiftiAsByte(BaseFolder + @"\Structure2.nii.gz");

            var volume = MedIO.LoadNiftiAsShort(BaseFolder + @"\ParentVolume.nii.gz");

            var structure1Contour = structure1.ContoursWithHolesPerSlice();
            var structure2Contour = structure2.ContoursWithHolesPerSlice();

            var volumeResult = structure1Contour.GeometryIntersect(structure2Contour, volume);

            var actualVolumeResult = MedIO.LoadNiftiAsByte(BaseFolder + @"\Structure1IntersectStructure2.nii.gz");

            Assert.AreEqual(volumeResult.Length, actualVolumeResult.Length);

            for (var i = 0; i < actualVolumeResult.Length; i++)
            {
                Assert.AreEqual(volumeResult[i], actualVolumeResult[i]);
            }
        }

        [Test]
        public void MinusTest()
        {
            var structure1 = MedIO.LoadNiftiAsByte(BaseFolder + @"\Structure1.nii.gz");
            var structure2 = MedIO.LoadNiftiAsByte(BaseFolder + @"\Structure2.nii.gz");

            var volume = MedIO.LoadNiftiAsShort(BaseFolder + @"\ParentVolume.nii.gz");

            var structure1Contour = structure1.ContoursWithHolesPerSlice();
            var structure2Contour = structure2.ContoursWithHolesPerSlice();

            var volumeResult = structure1Contour.GeometryExclude(structure2Contour, volume);

            var actualVolumeResult = MedIO.LoadNiftiAsByte(BaseFolder + @"\Structure1MinusStructure2.nii.gz");

            Assert.AreEqual(volumeResult.Length, actualVolumeResult.Length);

            for (var i = 0; i < actualVolumeResult.Length; i++)
            {
                Assert.AreEqual(volumeResult[i], actualVolumeResult[i]);
            }
        }

        [Test]
        public void UnionTest()
        {
            var structure1 = MedIO.LoadNiftiAsByte(BaseFolder + @"\Structure1.nii.gz");
            var structure2 = MedIO.LoadNiftiAsByte(BaseFolder + @"\Structure2.nii.gz");

            var volume = MedIO.LoadNiftiAsShort(BaseFolder + @"\ParentVolume.nii.gz");

            var structu