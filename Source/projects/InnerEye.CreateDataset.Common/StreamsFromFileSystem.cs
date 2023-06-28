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
                throw new InvalidOperationException("The operation cannot be completed because the object accesses the file system in read-only mode.");
            }
        }

        /// <summary>
        /// The directory separator that should be used when enumerating files.
        /// </summary>
        public const char DirectorySeparator = '/';

        /// <summary>
        /// Gets whether the object is in read only mode. Writing to the file system
        /// will cause an exception in read-only mode.
        /// </summary>
        public bool IsReadOnly;

        /// <summary>
        /// Gets whether the object allows to overwrite existing files.
        /// </summary>
        public virtual bool AllowFileOverwrite { get; set; }

        /// <summary>
        /// Gets a stream that can be used to read from an existing file.
        /// Throws a <see cref="FileNotFoundException"/> if no file of that given name exists.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public abstract Stream GetStreamForReading(string fileName);

        /// <summary>
        /// Gets a stream that can be used to write to the file system, where the result
        /// will get the given file name.
        /// Throws an exception if the file system access is read-only.
        /// Throws an exception if the file alread