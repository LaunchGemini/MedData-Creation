///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Contours
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    /// <summary>
    /// Stores a contour for slices of a volume, indexed by slice number.
    /// </summary>
    // Threadsafe from outside
    [Serializable]
    public class ContoursPerSlice : IEnumerable<KeyValuePair<int, IReadOnlyList<ContourPolygon>>>, INotifyCollectionChanged
    {
        private readonly IDictionary<int, IReadOnlyList<ContourPolygon>> _contoursBySliceDictionary;

        private readonly object _lock = new object();

        public ContoursPerSlice()
        {
            _contoursBySliceDictionary = new Dictionary<int, IReadOnlyList<ContourPolygon>>();
        }

        public ContoursPerSlice(IDictionary<int, IReadOnlyList<ContourPolygon>> contours)
        {
            _contoursBySliceDictionary = contours;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Gets the number of slices that have contours with non-empty sets of points.
        /// </summary>
        public int SliceCount
        {
            get
            {
                lock (_lock)
                {
                    return
                        _contoursBySliceDictionary.Count(
                            x => x.Value.Any(
  