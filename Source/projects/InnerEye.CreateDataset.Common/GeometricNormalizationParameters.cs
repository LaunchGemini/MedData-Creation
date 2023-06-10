///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InnerEye.CreateDataset.Common.Models
{
    /// <summary>
    /// Parameters class to encapsulate the configurations required to perform Geometric Normalization operations
    /// </summary>
    public class GeometricNormalizationParameters
    {
        /// <summary>
        /// Enumeration to encapsulate the differnt kinds of channels that can be provided as input for Geometric Normalization
        /// </summary>
        public enum GeometricNormalizationChannelType
        {
            /// <summary>
            /// Specifies that the channel contains an image, as short volume.
            /// </summary>
            Image,
            /// <summary>
            /// Specifies that the channel contains a ground truth segmentation, as a byte volume.
            /// </summary>
            GroundTruth
        }

        /// <summary>
        /// Wrapper class to encapsulate a single input-output-chanel type config
        /// </summary>
        public class InputOutputChannel
        {
            [JsonRequired]
            public string InputChannelId { get; set; }

            [JsonRequired]
            public string OutputChannelId { get; set; }

            [JsonRequired]
            public GeometricNormalizationChannelType ChannelType { get; set; }

            public InputOutputChannel(string inputChannelId, string outputChannelId, GeometricNormalizationChannelType channelType)
            {
                InputChannelId = inputChannelId;
                OutputChannelId = output