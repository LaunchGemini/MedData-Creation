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
            var scan = new Volume3D<short>(5, 5, 5);
            foreach (var index in scan.Array.Indices())
            {
                scan.Array[index] = (short)index;
            }
            // Create 3 structures, each with a different color
            var masks = new List<ContourRenderingInformation>();
            var colors = new[]
            {
                new RGBValue(255, 0, 0),
                new RGBValue(0, 255, 0),
                new RGBValue(0, 0, 255),
            };
            foreach (var index in Enumerable.Range(0, 3))
            {
                var mask = scan.CreateSameSize<byte>();
                mask[index+1, index+1, index+1] = 1;
                masks.Add(new ContourRenderingInformation($"structure_{index}", colors[index], mask));
            }
            var seriesDescription = "description";
            var patientId = DicomUID.Generate().UID;
            var studyId = DicomUID.Generate().UID;
            var dicomFiles = NiiToDicomHelpers.ScanAndContoursToDicom(scan, ImageModality.CT, masks, 
                seriesDescription, patientId, studyId);
            // Write to disk, so that we can load it into the App as well
            var dicomFilesOnDisk = new List<string>();
            foreach (var dicomFile in dicomFiles)
            {
                dicomFilesOnDisk.Add(dicomFile.SaveToFolder(outputFolder));
            }
            // Test if the first returned Dicom file is really the RTStruct
            var rtStructFromFile = RtStructReader.LoadContours(dicomFilesOnDisk[0], scan.Transform.DicomToData);
            Assert.IsNotNull(rtStructFromFile);
            Assert.AreEqual(masks.Count, rtStructFromFile.Item1.Contours.Count);
            var fromDisk = NiiToDicomHelpers.MedicalVolumeFromDicomFolder(outputFolder);
            VolumeAssert.AssertVolumesMatch(scan, fromDisk.Volume, "Loaded scan does not match");
            Assert.AreEqual(seriesDescription, fromDisk.Identifiers.First().Series.SeriesDescription);
            Assert.AreEqual(patientId, fromDisk.Identifiers.First().Patient.Id);
            Assert.AreEqual(studyId, fromDisk.Identifiers.First().Study.StudyInstanceUid);
            foreach (var index in Enumerable.Range(0, fromDisk.Struct.Contours.Count))
            {
                var loadedMask = fromDisk.Struct.Contours[index].Contours.ToVolume3D(scan);
                VolumeAssert.AssertVolumesMatch(masks[index].Contour.ToVolume3D(scan), loadedMask, $"Loaded mask {index}");
                Assert.AreEqual(masks[index].Name, fromDisk.Struct.Contours[index].StructureSetRoi.RoiName, $"Loaded mask name {index}");
            }

            // Now test if we can ZIP up all the Dicom files, and read them back in. 
            var zippedDicom = ZippedDicom.Dic