
///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

namespace MedLib.IO.RT
{
    using Dicom;
    using Extensions;

    public class DicomStudy
    {
        /// <summary>
        /// The unique identifier for this study, Type 1, VR UI
        /// </summary>
        public string StudyInstanceUid { get; }

        /// <summary>
        /// The date the study started, Type 2
        /// </summary>
        public string StudyDate { get; }

        /// <summary>
        /// The time the study started, Type 2
        /// </summary>
        public string StudyTime { get; }

        /// <summary>
        /// Name of the physician who referred the patient, Type 2, PN 
        /// </summary>
        public string ReferringPhysicianName { get; }

        /// <summary>
        /// User or equipment generated study identifier, Type 3 VR short string
        /// </summary>
        public string StudyId { get; }

        /// <summary>
        /// RIS generated number for study, Type 2, Short string
        /// </summary>
        public string AccessionNumber { get; }

        /// <summary>
        /// Description, Type 3, Long string
        /// </summary>
        public string StudyDescription { get; }

        public DicomStudy(string studyInstanceUid, string studyDate, string studyTime, string referringPhysicianName, string studyId, string accessionNumber, string studyDescription)
        {
            StudyInstanceUid = studyInstanceUid;
            StudyDate = studyDate;
            StudyTime = studyTime;