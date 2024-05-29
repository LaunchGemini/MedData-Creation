///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

namespace MedLib.IO.RT
{
    using System.Collections.Generic;

    using Dicom;
    
    /// <summary>
    /// The Referenced Frame of Reference Sequence (3006,0010) describes a set of frames of reference in
    /// which some or all of the ROIs are expressed.Since the Referenced Frame of Reference UID 
    /// (3006,0024) is required for each ROI, each frame of reference used to express the coordinates of an ROI 
    /// shall be listed in the Referenced Frame of Reference Sequence(3006,0010) once and only once.
    /// </summary>
    public class DicomRTFrameOFReference
    {
        public string FrameOfRefUID { get; }

        public IReadOnlyList<DicomRTReferencedStudy> ReferencedStudies { get; }

        public DicomRTFrameOFReference(
            string frameOfRefUid,
            IReadOnlyList<DicomRTReferencedStudy> referencedStudies)
        {
            FrameOfRefUID = frameOfRefUid;
            ReferencedStudies = referencedStudies;
        }

        p