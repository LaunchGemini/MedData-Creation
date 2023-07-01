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
        /// Throws an exception if the file already exists, and the <see cref="AllowFileOverwrite"/>
        /// property is false.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public virtual Stream GetStreamForWriting(string fileName)
        {
            ThrowIfReadOnly();
            // Multiple threads can request read streams at the same time, that sometimes
            // leads to IndexOutOfRangeExceptions.
            lock (listLock)
            {
                _filesWritten.Add(fileName);
            }
            // Implement this is a virtual function such that implementations can re-use the read-only check
            // and the bookkeeping. Returning null is not optimal, but I could not come up with a better solution.
            return null;
        }

        /// <summary>
        /// Writes a file from stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="fileName">Name of the file to be written</param>
        public abstract void CopyFromStreamToFile(Stream stream, string fileName);

        /// <summary>
        /// Gets all file names that are available, where the file name
        /// starts with the given prefix. Each of the returned files will start with that prefix.
        /// The prefix does not need to be aligned in any way with a folder structure that the
        /// underlying file system may implement. When the files live in a directory structure,
        /// the returned file name will use DirectorySeparator as the sepator, independent of the
        /// separator character that the underlying file system uses.
        /// The file names returned must be such that they can directly be used in a GetStreamForReading
        /// operation.
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public abstract IReadOnlyList<string> EnumerateFiles(string prefix);

        /// <summary>
        /// Gets the size in bytes of a specific file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public abstract long GetFileLength(string fileName);

        /// <summary>
        /// Gets whether an input file of the given name exists.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public abstract bool FileExists(string fileName);

        /// <summary>
        /// Gets the URI for the file of the given name, including all relevant root folders if needed.
        /// The URI should include any access tokens to allow read access, if needed. The file may or may not
        /// exist already. Throws an ArgumentException if the filename is missing or an empty string.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public abstract string GetUri(string fileName);

        /// <summary>
        /// Gets the URI of the root folder or container that stores the files.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public abstract string GetUriForRoot();

        /// <summary>
        /// Deletes all files that are in the directory/container,
        /// as well as the actual directory/container. Throws an exception if
        /// the object is in read-only mode.
        /// </summary>
        public virtual void DeleteAll()
        {
            ThrowIfReadOnly();
        }

        /// <summary>
        /// Gets the encoding that the IO abstraction uses for reading and writing text.
        /// </summary>
        public static readonly Encoding TextEncoding = Encoding.UTF8;

        /// <summary>
        /// Writes text data to the file system.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="text"></param>
        public void WriteAllText(string fileName, string text)
        {
            using (var writeStream = GetStreamForWriting(fileName))
            {
                var bytes = TextEncoding.GetBytes(text);
                writeStream.Write(bytes, 0, bytes.Length);
            }
      