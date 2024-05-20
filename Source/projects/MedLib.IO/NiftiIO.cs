
///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿using System.Linq;

namespace MedLib.IO
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using LZ4;
    using InnerEye.CreateDataset.Volumes;
    using System.CodeDom.Compiler;
    using System.Diagnostics;
    using Extensions;

    /// <summary>
    /// Contains the different forms in which a Nifti file can be compressed.
    /// </summary>
    public enum NiftiCompression
    {
        /// <summary>
        /// A Nifti file that is not compressed at all.
        /// </summary>
        Uncompressed,
        /// <summary>
        /// A Nifti file that is compressed using GZIP.
        /// </summary>
        GZip,
        /// <summary>
        /// A Nifti file that is compressed using LZ4.
        /// </summary>
        LZ4
    }

    /// <summary>
    /// Contains methods to load and save instances of <see cref="Volume{T}"/> from/to streams
    /// in Nifti format.
    /// </summary>
    public static class NiftiIO
    {
        /// <summary>
        /// Write Volume3D object to stream.
        /// </summary>
        /// <param name="stream">The stream to write to. It must be disposed of by the caller.</param>
        /// <param name="volume"></param>
        /// <param name="compress"></param>
        /// <returns></returns>
        public static void WriteToStream(Stream stream, Volume3D<byte> volume, NiftiCompression niftiCompression)
        {
            NiftiInternal.WriteToStreamWithVoxelWriter(stream, volume, niftiCompression, NiftiInternal.Nifti1Datatype.DT_UNSIGNED_CHAR, (w, r) => w.Write(r));
        }

        /// <summary>
        /// Write Volume3D object to stream.
        /// </summary>
        /// <param name="stream">The stream to write to. It must be disposed of by the caller.</param>
        /// <param name="volume"></param>
        /// <param name="compress"></param>
        /// <returns></returns>
        public static void WriteToStream(Stream stream, Volume3D<short> volume, NiftiCompression niftiCompression)
        {
            NiftiInternal.WriteToStreamWithVoxelWriter(stream, volume, niftiCompression, NiftiInternal.Nifti1Datatype.DT_SIGNED_SHORT, (w, r) => w.Write(r));
        }

        /// <summary>
        /// Write Volume3D object to stream.
        /// </summary>
        /// <param name="stream">The stream to write to. It must be disposed of by the caller.</param>
        /// <param name="volume"></param>
        /// <param name="compress"></param>
        /// <returns></returns>
        public static void WriteToStream(Stream stream, Volume3D<float> volume, NiftiCompression niftiCompression)
        {
            NiftiInternal.WriteToStreamWithVoxelWriter(stream, volume, niftiCompression, NiftiInternal.Nifti1Datatype.DT_FLOAT, (w, r) => w.Write(r));
        }

        /// <summary>
        /// Converts a float value to a byte, by rounding and truncating at the minimum and maximum values 
        /// that byte takes on.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static byte SingleToByte(float x)
        {
            if (x >= byte.MaxValue)
            {
                return byte.MaxValue;
            }

            if (x <= byte.MinValue)
            {
                return byte.MinValue;
            }

            return (byte)Math.Round(x);
        }

        /// <summary>
        /// Converts a float value to a short (signed 16 bit integer), by rounding and truncating 
        /// at the minimum and maximum values that 'short' takes on.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static short SingleToShort(float x)
        {
            if (x >= short.MaxValue)
            {
                return short.MaxValue;
            }

            if (x <= short.MinValue)
            {
                return short.MinValue;
            }

            return (short)Math.Round(x);
        }

        /// <summary>
        /// Contains the three Nifti datatypes that the generic loading routines accept as input.
        /// </summary>
        private static NiftiInternal.Nifti1Datatype[] SupportedDataTypes = new []
        {
           NiftiInternal.Nifti1Datatype.DT_SIGNED_SHORT,
           NiftiInternal.Nifti1Datatype.DT_FLOAT,
           NiftiInternal.Nifti1Datatype.DT_UNSIGNED_CHAR,
           NiftiInternal.Nifti1Datatype.DT_UINT16
        };

        /// <summary>
        /// Get a Volume3D object from a Nifti stream. The input Nifti file can be in any of the 
        /// supported Nifti data types, and will be returned as a <see cref="Volume3D{T}"/> of type <see cref="short"/>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="niftiCompression">Defines the decompression method that should be applied to the stream.</param>
        /// <returns></returns>
        public static Volume3D<short> ReadNiftiAsShort(Stream stream, NiftiCompression niftiCompression)
        {
            return NiftiInternal.ReadStream<short>(stream, niftiCompression, SupportedDataTypes, VolumeRescaleConvert.BatchConvert);
        }

        /// <summary>
        /// Get a Volume3D object from a Nifti stream. The input Nifti file can be in any of the 
        /// supported Nifti data types, and will be returned as a <see cref="Volume3D{T}"/> of type <see cref="ushort"/>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="niftiCompression">Defines the decompression method that should be applied to the stream.</param>
        /// <returns></returns>
        public static Volume3D<ushort> ReadNiftiAsUShort(Stream stream, NiftiCompression niftiCompression)
        {
            return NiftiInternal.ReadStream<ushort>(stream, niftiCompression, SupportedDataTypes, VolumeRescaleConvert.BatchConvert);
        }

        /// <summary>
        /// Get a Volume3D object from a Nifti stream. The input Nifti file can be in any of the 
        /// supported Nifti data types, and will be returned as a <see cref="Volume3D{T}"/> of type <see cref="byte"/>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="niftiCompression">Defines the decompression method that should be applied to the stream.</param>
        /// <returns></returns>
        public static Volume3D<byte> ReadNiftiAsByte(Stream stream, NiftiCompression niftiCompression)
        {
            return NiftiInternal.ReadStream<byte>(stream, niftiCompression, SupportedDataTypes, VolumeRescaleConvert.BatchConvert);
        }

        /// <summary>
        /// Get a Volume3D object from a Nifti stream. The input Nifti file can be in any of the 
        /// supported Nifti data types, and will be returned as a <see cref="Volume3D{T}"/> of type <see cref="float"/>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="niftiCompression">Defines the decompression method that should be applied to the stream.</param>
        /// <returns></returns>
        public static Volume3D<float> ReadNiftiAsFloat(Stream stream, NiftiCompression niftiCompression)
        {
            return NiftiInternal.ReadStream<float>(stream, niftiCompression, SupportedDataTypes, VolumeRescaleConvert.BatchConvert);
        }

        /// <summary>
        /// Get a Volume3D object from a nifti stream, where the stream is expected to have data in signed short format.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="niftiCompression">Defines the decompression method that should be applied to the stream.</param>
        /// <returns></returns>
        public static Volume3D<short> ReadNiftiInShortFormat(Stream stream, NiftiCompression niftiCompression)
        {
            return NiftiInternal.ReadStream<short>(stream, niftiCompression, new[] { NiftiInternal.Nifti1Datatype.DT_SIGNED_SHORT }, VolumeRescaleConvert.BatchConvert);
        }

        /// <summary>
        /// Get a Volume3D object from a nifti stream, where the stream is expected to have data in unsigned short format.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="niftiCompression">Defines the decompression method that should be applied to the stream.</param>
        /// <returns></returns>
        public static Volume3D<ushort> ReadNiftiInUShortFormat(Stream stream, NiftiCompression niftiCompression)
        {
            return NiftiInternal.ReadStream<ushort>(stream, niftiCompression, new[] { NiftiInternal.Nifti1Datatype.DT_UINT16 }, VolumeRescaleConvert.BatchConvert);
        }

        /// <summary>
        /// Get a Volume3D object from a nifti stream, where the stream is expected to have data in byte format.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="niftiCompression">Defines the decompression method that should be applied to the stream.</param>
        /// <returns></returns>
        public static Volume3D<byte> ReadNiftiInByteFormat(Stream stream, NiftiCompression niftiCompression)
        {
            return NiftiInternal.ReadStream<byte>(stream, niftiCompression, new[] { NiftiInternal.Nifti1Datatype.DT_UNSIGNED_CHAR }, VolumeRescaleConvert.BatchConvert);
        }

        /// <summary>
        /// Get a Volume3D object from a nifti stream, where the stream is expected to have data in single precision
        /// floating point format.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="niftiCompression">Defines the decompression method that should be applied to the stream.</param>
        /// <returns></returns>
        public static Volume3D<float> ReadNiftiInFloatFormat(Stream stream, NiftiCompression niftiCompression)
        {
            return NiftiInternal.ReadStream<float>(stream, niftiCompression, new[] { NiftiInternal.Nifti1Datatype.DT_FLOAT }, VolumeRescaleConvert.BatchConvert);
        }

        [GeneratedCode("nifti", "1.0")]
        // Methods for reading and writing NIFTI files
        public static class NiftiInternal
        {
            // *** NB Names used below are chosen to agree with NIFTI 1 documentation. ***
            [GeneratedCode("nifti", "1.0")]
            // nifti1 datatype codes
            public enum Nifti1Datatype : short
            {
                DT_NONE = 0,
                DT_UNKNOWN = 0,    // what it says, dude
                DT_BINARY = 1,     // binary (1 bit/voxel)
                DT_UNSIGNED_CHAR = 2,     // unsigned char (8 bits/voxel)
                DT_SIGNED_SHORT = 4,     // signed short (16 bits/voxel)
                DT_SIGNED_INT = 8,    // signed int (32 bits/voxel)
                DT_FLOAT = 16,     // float (32 bits/voxel)
                DT_COMPLEX = 32,     // complex (64 bits/voxel)
                DT_DOUBLE = 64,     // double (64 bits/voxel)
                DT_RGB = 128,     // RGB triple (24 bits/voxel)
                DT_ALL = 255,     // not very useful (?)

                DT_INT8 = 256,     // signed char (8 bits)
                DT_UINT16 = 512,     // unsigned short (16 bits)
                DT_UINT32 = 768,     // unsigned int (32 bits)
                DT_INT64 = 1024,     // long long (64 bits)
                DT_UINT64 = 1280,     // unsigned long long (64 bits)
                DT_FLOAT128 = 1536,     // long double (128 bits)
                DT_COMPLEX128 = 1792,     // double pair (128 bits)
                DT_COMPLEX256 = 2048,     // long double pair (256 bits)
                DT_RGBA32 = 2304     // 4 byte RGBA (32 bits/voxel)
            }

            [GeneratedCode("nifti", "1.0")]
            // nifti1 xform codes to describe the "standard" coordinate system
            public enum Nifti1XFormCode : short
            {
                NIFTI_XFORM_UNKNOWN = 0,            // Arbitrary coordinates (Method 1).
                NIFTI_XFORM_SCANNER_ANAT = 1,       // Scanner-based anatomical coordinates
                NIFTI_XFORM_ALIGNED_ANAT = 2,       // Coordinates aligned to another file's,  or to anatomical "truth".
                NIFTI_XFORM_TALAIRACH = 3,          // Coordinates aligned to Talairach-Tournoux Atlas; (0,0,0)=AC, etc.
                NIFTI_XFORM_MNI_152 = 4             // MNI 152 normalized coordinates.
            }

            [GeneratedCode("nifti", "1.0")]
            // nifti1 units codes to describe the unit of measurement for each dimension of the dataset
            public enum Nifti1Units : byte
            {
                // NIFTI code for unspecified units.
                NIFTI_UNITS_UNKNOWN = 0,

                // Space codes are multiples of 1.
                NIFTI_UNITS_METER = 1,
                NIFTI_UNITS_MM = 2,
                NIFTI_UNITS_MICRON = 3,

                // Time codes are multiples of 8.
                NIFTI_UNITS_SEC = 8,
                NIFTI_UNITS_MSEC = 16,
                NIFTI_UNITS_USEC = 24,

                // These units are for spectral data:
                NIFTI_UNITS_HZ = 32,
                NIFTI_UNITS_PPM = 40,
                NIFTI_UNITS_RADS = 48
            }

            [GeneratedCode("nifti", "1.0")]
            // nifti1 slice order codes, describing the acquisition order of the slices
            public enum Nifti1SliceOrder : byte
            {
                NIFTI_SLICE_UNKNOWN = 0,
                NIFTI_SLICE_SEQ_INC = 1,
                NIFTI_SLICE_SEQ_DEC = 2,
                NIFTI_SLICE_ALT_INC = 3,
                NIFTI_SLICE_ALT_DEC = 4,
                NIFTI_SLICE_ALT_INC2 = 5,
                NIFTI_SLICE_ALT_DEC2 = 6
            }

            [GeneratedCode("nifti", "1.0")]
            // NIFTI file header
            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct Nifti1Header
            {
                // Call to complete construction (normal struct constructor has to
                // initialize everything, which is too much typing).
                public void Initialize()
                {
                    db_name = new byte[18];
                    data_type = new byte[10];
                    dim = new short[8];
                    pixdim = new float[8];
                    descrip = new byte[80];
                    aux_file = new byte[24];

                    srow_x = new float[4];
                    srow_y = new float[4];
                    srow_z = new float[4];

                    intent_name = new byte[16];
                    magic = new byte[4];
                }

                public int sizeof_hdr;                 // MUST be 348

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
                public byte[] data_type;               // ++UNUSED++

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
                public byte[] db_name;                 // ++UNUSED++
                public int extents;                    // ++UNUSED++
                public short session_error;            // ++UNUSED++
                public byte regular;                   // ++UNUSED++ // simpleItk puts 'r' here
                public byte dim_info;                  // MRI slice ordering.

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
                public short[] dim;                    // Data array dimensions.
                public float intent_p1;                // 1st intent parameter.
                public float intent_p2;                // 2nd intent parameter.
                public float intent_p3;                // 3rd intent parameter.

                public short intent_code;              // Intent code.
                public Nifti1Datatype datatype;        // Defines data type.
                public short bitpix;                   // Number bits/voxel.
                public short slice_start;              // First slice index.

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
                public float[] pixdim;                 // Grid spacings.
                public float vox_offset;               // Offset into .nii file
                public float scl_slope;                // Data scaling: slope.
                public float scl_inter;                // Data scaling: offset.
                public short slice_end;                // Last slice index.
                public byte slice_code;                // Slice timing order.
                public Nifti1Units xyzt_units;         // Units of pixdim[1..4].
                public float cal_max;                  // Max display intensity.
                public float cal_min;                  // Min display intensity.