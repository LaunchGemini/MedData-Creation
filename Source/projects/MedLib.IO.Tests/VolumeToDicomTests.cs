///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace MedLib.IO.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Dicom;
    using InnerEye.CreateDataset.Common;
    using InnerEye.CreateDataset.TestHelpers;
    using InnerEye.CreateDataset.Contours;
    using InnerEye.CreateDataset.Math;
    using InnerEye.CreateDataset.Volumes;
    using NUnit.Framework;

    /// <summary>
    /// Contains tests to ensure that we can round trip from (VolumeShort, Set of binary masks) to Dicom
    /// to MedicalVolume and back.
    /// </summary>
    [TestFixture]
    public class VolumeToDicomTests
    {
        /// <summary>
        /// Tests whether a scan and a set of binary masks can be written to Dicom, and read back in again.
        /// </summary>
        [Test]
        public void VolumeToDicomAndBack()
        {
            var outputFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, "DicomOutput");
            if (Directory.Exists(outputFolder))
            {
                Directory.Delete(outputFolder, recursive: true);
                Thread.Sleep(1000);
            }
            Directory.CreateDirectory(outputFolder);
            var scan = new Volume3D<