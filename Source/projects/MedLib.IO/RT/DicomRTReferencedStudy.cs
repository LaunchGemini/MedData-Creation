///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

namespace MedLib.IO.RT
{
    using System;
    using System.Collections.Generic;

    using Dicom;

    using MedLib.IO.Extensions;

    public class DicomRTReferencedStudy
    {

        public static readonly string StudyComponentManagementSopClass = DicomUID.StudyComponentManagementSOPClassRETIRED.UID;

        public string ReferencedSOPClassUID { get; }

        public string ReferencedSOPInstanceUID { get; }

        public IReadOnlyList<DicomRTReferencedSeries> ReferencedSeries { get; }

        public DicomRTReferencedStudy(
            string referencedSopClassUid,
            string referencedSopInstanceUid,
            IReadOnlyList<DicomRTReferencedSeries> referencedSeries)
        {
            ReferencedSOPClassUID = referencedSopClassUid;
            ReferencedSOPInstanceUID = referencedSopInstanceUid;
            ReferencedSeries