///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using MedLib.IO;

    using InnerEye.CreateDataset.Volumes;

    using MoreLinq;

    /// <summary>
    /// Provides an abstraction for reading and writing streams to a file system.
    /// </summary>
    public abstract class StreamsFromFileSystem
    {
        private readonly List<string> _filesWritten = new List<string>();

        // An object for locking the _filesWritten list. 
        private object listLock = new object();

        /// <summary>
        /// Gets the filenames of all files that have yet been written by this object.
        /// </summary>
        public IReadOnlyList<string> FilesWritten()
        {
            List<string> files;
            lock (listLock)
            {
                files = _filesWritten.ToList();
            }
            return files;
        }

        /// <summary>
        /// Throws an exception if the object is in read-only mode.
        /// </summary>
        public void ThrowIfReadOnly()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("The 