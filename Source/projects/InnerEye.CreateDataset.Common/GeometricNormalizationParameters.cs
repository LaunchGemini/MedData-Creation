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
                OutputChannelId = outputChannelId;
                ChannelType = channelType;
            }

            public InputOutputChannel() { }
        }

        private int _medianFilterRadius;
        private double[] _standardiseSpacings;

        /// <summary>
        /// Flag to determine if Geometric Normalization needs to be performed
        /// </summary>
        [JsonIgnore]
        public bool DoGeometricNormalization { get => StandardiseSpacings != null; }

        /// <summary>
        /// [input channel, output channel, channel type] mappings for each of the channel to process
        /// </summary>
        public InputOutputChannel[] InputOutputChannels { get; set; }

        /// <summary>
        /// Ground truth channels to be loaded (assumed to have a byte datatype)
        /// </summary>
        [JsonIgnore]
        public IEnumerable<InputOutputChannel> GroundTruthChannelsToLoad
        {
            get => GetChannelsToLoad(GeometricNormalizationChannelType.GroundTruth);
        }

        /// <summary>
        /// Image channels to be loaded (assumed to have a short datatype)
        /// </summary>
        [JsonIgnore]
        public IEnumerable<InputOutputChannel> ImageChannelsToLoad
        {
            get => GetChannelsToLoad(GeometricNormalizationChannelType.Image);
        }

        /// <summary