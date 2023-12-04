///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Math
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using InnerEye.CreateDataset.Volumes;

    /// <summary>
    /// Stores the information about one bin entry of a histogram over <see cref="short"/> values.
    /// </summary>
    [DebuggerDisplay("Bin {Index}: {Count} values >= {MinimumInclusive}")]
    public class HistogramBin
    {
        /// <summary>
        /// Gets the index of bin inside the histogram. Index can have values from 0 to 
        /// (number of bins) - 1.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets the minimum value that is counted as belonging to this bin.
        /// </summary>
        public short MinimumInclusive { get; }

        /// <summary>
        /// Gets or sets the number of values in this histogram bin.
        /// </summary>
        public int Count { get; set; } = 0;

        /// <summary>
        /// Creates a new histogram bin, with the count set to zero.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="minimumInclusive"></param>
        public HistogramBin(int index, short minimumInclusive)
        {
            Index = index;
            MinimumInclusive = minimumInclusive;
        }
    }

    /// <summary>
    /// Helper class for auto-windowing a volume.
    /// </summary>
    public static class AutoWindowLevelHelper
    {
        /// <summary>
        /// The number of keys in the histogram.
        /// </summary>
        private const int DefaultHistogramSize = 150;

        /// <summary>
        /// The number of keys in the histogram, converted to double.
        /// </summary>
        private const double DefaultHistogramSizeDouble = DefaultHistogramSize;

        /// <summary>
        /// The first N number of keys that will be ignored in the histogram.
        /// Currently set to ignore the first 7% of histogram values.
        /// This should be a value between 0-1.
        /// </summary>
        private const double CtMininmumPercentile = 0.07;

        /// <summary>
        /// This is a minimum threshold value when the provided volume is CT. Any value less than this
        /// will be threshold to this value. This is to avoid picking the wrong peak when artifical values
        /// have been used for background.
        /// </summary>
        private const short CtMinimumThreshold = -1000;

        /// <summary>
        /// This is a maximum threshold value when the provided volume is CT. Any value greater than this
        /// will be threshold to this value. This is to avoid picking the wrong peak when artifical values
        /// have been used for background.
        /// </summary>
        private const short CtMaximumThreshold = 2000;

        /// <summary>
        /// The window for a CT image will be at least this percentage further away from the chosen level value.
        /// </summary>
        private const double CtWindowMinimumRestrictionPercentage = 0.025;

        /// <summary>
        /// The window for a CT image will be at least this percentage close to the chosen level value.
        /// </summary>
        private const double CtWindowMaximumRestrictionPercentage = 0.15;

        /// <summary>
        /// The start percentage of histogram values that will be inspected when chosing a level value.
        /// </summary>
        private const double MrMinimumPercentile = 0.25;

        /// <summary>
        /// The finish percentage of histogram values that will be inspected when chosing a level value.
        /// </summary>
        private const double MrMaximumPercentile = 0.38;

        /// <summary>
        /// For MR we create a histogram and find the lowest histogram values in the first X% to Y% of values.
        /// The window is then set to be as far left as possible of the selected window value.
        /// </summary>
        /// <param name="volume">The volume to calculate the window/ level from.</param>
        /// <param name="volumeSkip">
        /// The number of items to skip over when computing the histogram (this is an optimisation to make the compute faster). 
        /// If set to 0, the histogram will look at every voxel in the volume when computing the auto/ window level.
        /// If set to 1, this histogram will look at every other voxel etc.
        /// </param>
        /// <returns>A tuple of the Window (item 1) and Level (item 2)</returns>
        /// <exception cref="ArgumentNullException">The provided volume was null.</exception>
        public static (int Window, int Level) ComputeMrAutoWindowLevel(short[] volume, uint volumeSkip = 5)
        {
            volume = volume ?? throw new ArgumentNullException(nameof(volume));
            var stopwatch = Stopwatch.StartNew();

            var minMax = FindMinMax(volume, volumeSkip);

            // Create a histogram, and find the lowest histogram value in the first X% to Y% of values.
            var levelKeyValue = 
                CreateHistogram(volume, minMax, volumeSkip)
                .Where(hist => hist.Index > MrMinimumPercentile * DefaultHistogramSizeDouble && hist.Index < MrMaximumPercentile * DefaultHistogramSizeDouble)
                .OrderBy(hist => hist.Count)
                .First();

            // Calculate the chosen level value from the histogram key.
            var minMaxDifference = minMax.Range();
            var level = levelKeyValue.MinimumInclusive;

            // For MR we create a wide window from the beginning of the voxel distribution to the level.
            var window = (level - minMax.Minimum) * 2;

            stopwatch.Stop();
            Console.WriteLine($"[{nameof(ComputeMrAutoWindowLevel)}] {stopwatch.ElapsedMilliseconds} milliseconds - Level: {level} Window: {window}");

            return (window, level);
        }

        /// <summary>
        /// For CT we aim to find the highest peak right on the histogram for the level.
        /// For the window we aim to the find lowest point left of the highest right peak within a restricted range.
        /// </summary>
        /// <param name="volume">The CT volume.</param>
        /// <param name="volumeSkip">
        /// The number of items to skip over when computing the histogram (this is an optimisation to make the compute faster). 
        /// If set to 0, the histogram will look at every voxel in the volume when computing the auto/ window level.
        /// If set to 1, this histogram will look at every other voxel etc.