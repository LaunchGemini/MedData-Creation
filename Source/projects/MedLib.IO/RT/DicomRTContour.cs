///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

namespace MedLib.IO.RT
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Dicom;

    public class DicomRTContour
    {
        public Tuple<byte, byte, byte> RGBColor { get; }

        public IReadOnlyList<DicomRTContourItem> DicomRtContourItems { get; }

        public string RgbColorAsString()
        {
            return string.Join(@"\", new int[] { RGBColor.Item1, RGBColor.Item2, RGBColor.Item3 });
        }

        public string ReferencedRoiNumber { get; }

        public DicomRTContour(string referencedRoiNumber, Tuple<byte, byte, byte> colorRgb, IReadOnlyList<DicomRTContourItem> contoursPerSlice)
        {
            Referenced