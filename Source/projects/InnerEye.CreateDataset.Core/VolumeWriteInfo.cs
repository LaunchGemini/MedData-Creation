///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Core
{
    using MedLib.IO;
    using InnerEye.CreateDataset.Common;
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Holds the information about how and where a volume was written, when creating
    /// a dataset.
    /// </summary>
    public class VolumeWriteInfo
    {
        public VolumeWriteInfo(VolumeMetadata metadata, string pathRelativeToDatasetFolder)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            if (string.IsNullOrWhiteSpace(pathRelativeToDatasetFolder))
            {
                throw new ArgumentException(nameof(pathRelativeToDatasetFolder));
            }
            UploadPathRelativeToDatasetFolder = pathRelativeToDatasetFolder;
        }

        /// <summary>
        /// Gets or sets the information about the subject, channel, and other metadata.
        /// </summary