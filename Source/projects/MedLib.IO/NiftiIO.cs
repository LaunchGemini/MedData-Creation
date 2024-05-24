
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
                public float slice_duration;           // Time for 1 slice.
                public float toffset;                  // Time axis shift.
                public int glmax;                      // ++UNUSED++
                public int glmin;                      // ++UNUSED++

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
                public byte[] descrip;                 // any text you like.

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
                public byte[] aux_file;                // auxiliary filename.

                public Nifti1XFormCode qform_code;     // Transform code.
                public Nifti1XFormCode sform_code;     // Transform code.

                public float quatern_b;                // Quaternion b param.
                public float quatern_c;                // Quaternion c param.
                public float quatern_d;                // Quaternion d param.
                public float qoffset_x;                // Quaternion x shift.
                public float qoffset_y;                // Quaternion y shift.
                public float qoffset_z;                // Quaternion z shift.

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public float[] srow_x;                 // 1st row affine transform.
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public float[] srow_y;                 // 2nd row affine transform.
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public float[] srow_z;                 // 3rd row affine transform.

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
                public byte[] intent_name;             //'name' or meaning of data.

                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
                public byte[] magic;                   // MUST be "ni1\0" or "n+1\0".
            };
            // 348 bytes total

            public static Tuple<Matrix3, Point3D> QuaternianToMatrix4(
                float qb, float qc, float qd,
                float qx, float qy, float qz,
                float dx, float dy, float dz, float qfac)
            {
                var R = new Matrix3();
                var t = new Point3D();
                double a, b = qb, c = qc, d = qd, xd, yd, zd;

                // compute a from b,c,d
                a = 1.0 - (b * b + c * c + d * d);
                if (a < 1.0e-7)
                {
                    // special case tiny a
                    a = 1.0 / Math.Sqrt(b * b + c * c + d * d);
                    b *= a;
                    c *= a;
                    d *= a; // normalize (b,c,d) vector
                    a = 0.0; // a = 0 ==> 180 degree rotation
                }
                else
                {
                    a = Math.Sqrt(a); // angle = 2*arccos(a)
                }

                // load rotation matrix, including scaling factors for voxel sizes
                xd = (dx > 0.0) ? dx : 1.0; // make sure are positive
                yd = (dy > 0.0) ? dy : 1.0;
                zd = (dz > 0.0) ? dz : 1.0;

                if (qfac < 0.0)
                    zd = -zd; // left handedness?

                R[0, 0] = (float)((a * a + b * b - c * c - d * d) * xd);
                R[0, 1] = 2.0 * (b * c - a * d) * yd;
                R[0, 2] = 2.0 * (b * d + a * c) * zd;
                R[1, 0] = 2.0 * (b * c + a * d) * xd;
                R[1, 1] = (float)((a * a + c * c - b * b - d * d) * yd);
                R[1, 2] = 2.0 * (c * d - a * b) * zd;
                R[2, 0] = 2.0 * (b * d - a * c) * xd;
                R[2, 1] = 2.0 * (c * d + a * b) * yd;
                R[2, 2] = (float)((a * a + d * d - c * c - b * b) * zd);

                // load offsets
                t.X = qx;
                t.Y = qy;
                t.Z = qz;

                return new Tuple<Matrix3, Point3D>(R, t);
            }

            public static void Matrix4ToQuaternion(
                Matrix3 R, Point3D t,
                out float qb, out float qc, out float qd,
                out float qx, out float qy, out float qz,
                out float dx, out float dy, out float dz,
                out float qfac)
            {
                double r11, r12, r13, r21, r22, r23, r31, r32, r33;
                double xd, yd, zd, a, b, c, d;
                var P = new Matrix3();
                var Q = new Matrix3();

                // offset outputs are read write out of input matrix
                qx = (float)t.X;
                qy = (float)t.Y;
                qz = (float)t.Z;

                // shorthand
                r11 = R[0, 0];
                r12 = R[0, 1];
                r13 = R[0, 2];
                r21 = R[1, 0];
                r22 = R[1, 1];
                r23 = R[1, 2];
                r31 = R[2, 0];
                r32 = R[2, 1];
                r33 = R[2, 2];

                // compute lengths of each column; these determine grid spacings
                xd = Math.Sqrt(r11 * r11 + r21 * r21 + r31 * r31);
                yd = Math.Sqrt(r12 * r12 + r22 * r22 + r32 * r32);
                zd = Math.Sqrt(r13 * r13 + r23 * r23 + r33 * r33);

                // if a column length is zero, patch the trouble
                if (xd == 0.0)
                {
                    r11 = 1.0;
                    r21 = r31 = 0.0;
                    xd = 1.0;
                }
                if (yd == 0.0)
                {
                    r22 = 1.0;
                    r12 = r32 = 0.0;
                    yd = 1.0;
                }
                if (zd == 0.0)
                {
                    r33 = 1.0;
                    r13 = r23 = 0.0;
                    zd = 1.0;
                }

                // assign the output lengths
                dx = (float)xd;
                dy = (float)yd;
                dz = (float)zd;

                // normalize the columns
                r11 /= xd;
                r21 /= xd;
                r31 /= xd;
                r12 /= yd;
                r22 /= yd;
                r32 /= yd;
                r13 /= zd;
                r23 /= zd;
                r33 /= zd;

                // TODO: Check orthonormality (det=1?)

                zd = r11 * r22 * r33 - r11 * r32 * r23 - r21 * r12 * r33
                     + r21 * r32 * r13 + r31 * r12 * r23 - r31 * r22 * r13; /* should be -1 or 1 */

                if (zd > 0)
                {
                    // proper
                    qfac = 1.0f;
                }
                else
                {
                    // improper ==> flip 3rd column
                    qfac = -1.0f;
                    r13 = -r13;
                    r23 = -r23;
                    r33 = -r33;
                }

                // now, compute quaternion parameters
                a = r11 + r22 + r33 + 1.0;

                if (a > 0.5)
                {
                    // simplest case
                    a = 0.5 * Math.Sqrt(a);
                    b = 0.25 * (r32 - r23) / a;
                    c = 0.25 * (r13 - r31) / a;
                    d = 0.25 * (r21 - r12) / a;
                }
                else
                {
                    // trickier case
                    xd = 1.0 + r11 - (r22 + r33); // 4*b*b
                    yd = 1.0 + r22 - (r11 + r33); // 4*c*c
                    zd = 1.0 + r33 - (r11 + r22); // 4*d*d
                    if (xd > 1.0)
                    {
                        b = 0.5 * Math.Sqrt(xd);
                        c = 0.25 * (r12 + r21) / b;
                        d = 0.25 * (r13 + r31) / b;
                        a = 0.25 * (r32 - r23) / b;
                    }
                    else if (yd > 1.0)
                    {
                        c = 0.5 * Math.Sqrt(yd);
                        b = 0.25 * (r12 + r21) / c;
                        d = 0.25 * (r23 + r32) / c;
                        a = 0.25 * (r13 - r31) / c;
                    }
                    else
                    {
                        d = 0.5 * Math.Sqrt(zd);
                        b = 0.25 * (r13 + r31) / d;
                        c = 0.25 * (r23 + r32) / d;
                        a = 0.25 * (r21 - r12) / d;
                    }
                    if (a < 0.0)
                    {
                        b = -b;
                        c = -c;
                        d = -d;
                        a = -a;
                    }
                }

                qb = (float)b;
                qc = (float)c;
                qd = (float)d;
            }

            public static Nifti1Header CreateNifti1Header<T>(Volume3D<T> volume, Nifti1Datatype datatype)
            {
                Nifti1Header header = new Nifti1Header();
                header.Initialize();

                // NB. Fields not explicity mentioned below can safely be left as 0

                header.sizeof_hdr = 348;

                header.dim[0] = 3;
                header.dim[1] = (short)(volume.DimX);
                header.dim[2] = (short)(volume.DimY);
                header.dim[3] = (short)(volume.DimZ);
                header.dim[4] = 1;
                header.dim[5] = 1;
                header.dim[6] = 1;
                header.dim[7] = 1;
                header.intent_code = (short)(NiftiIntentCodes.NIFTI_INTENT_NONE);
                header.datatype = datatype;

                switch (datatype)
                {
                    case Nifti1Datatype.DT_SIGNED_SHORT:
                        header.bitpix = 16;
                        break;
                    case Nifti1Datatype.DT_UINT16:
                        header.bitpix = 16;
                        break;
                    case Nifti1Datatype.DT_FLOAT:
                        header.bitpix = 32;
                        break;
                    case Nifti1Datatype.DT_UNSIGNED_CHAR:
                        header.bitpix = 8;
                        break;
                    default:
                        throw new Exception("Unsupported NIFTI data type.");
                }

                header.pixdim[0] = 1.0f; // Repurposed to store qfac when qform_code>0
                header.pixdim[1] = (float)(volume.SpacingX);
                header.pixdim[2] = (float)(volume.SpacingY);
                header.pixdim[3] = (float)(volume.SpacingZ);
                header.vox_offset = 352.0f;

                // The volume representation has had rescale slope/intercept already applied.
                header.scl_slope = 1.0f;
                header.scl_inter = 0.0f;

                header.xyzt_units = Nifti1Units.NIFTI_UNITS_MM; // Units of pixdim[1..4].

                // TODO: Non-zero cal_max and cal_min?
                // TODO: Write descrip and intent_name?
                // TODO: Support two-file NIFTI?

                header.qform_code = Nifti1XFormCode.NIFTI_XFORM_SCANNER_ANAT;
                header.sform_code = Nifti1XFormCode.NIFTI_XFORM_UNKNOWN;

                // Dicom's coordinate system has +x is Left, +y is Posterior, +z is Superior;
                // Nifti's has +x is Right, +y is Anterior, +z is Superior.
                Matrix3 dicomToNiftiRotation = new Matrix3();
                dicomToNiftiRotation.Identity();
                dicomToNiftiRotation[0, 0] = -1.0;
                dicomToNiftiRotation[1, 1] = -1.0;

                float dx, dy, dz;
                Matrix4ToQuaternion(
                    dicomToNiftiRotation * volume.Direction, dicomToNiftiRotation * volume.Origin,
                    out header.quatern_b, out header.quatern_c, out header.quatern_d, // quaternion b,c,d param.
                    out header.qoffset_x, out header.qoffset_y, out header.qoffset_z, // quaternion x,y,z shift.
                    out dx, out dy, out dz,
                    // these are scales that will be 1.0 since volume.Direction is just a rotation matrix
                    out header.pixdim[0]);

                // One-file NIFTI
                header.magic[0] = (byte)('n');
                header.magic[1] = (byte)('+');
                header.magic[2] = (byte)('1');
                header.magic[3] = (byte)('\0');

                return header;
            }

            private static byte[] getBytes(Nifti1Header h)
            {
                int length = Marshal.SizeOf(h);
                IntPtr ptr = Marshal.AllocHGlobal(length);
                byte[] buffer = new byte[length];

                Marshal.StructureToPtr(h, ptr, true);
                Marshal.Copy(ptr, buffer, 0, length);
                Marshal.FreeHGlobal(ptr);

                return buffer;
            }

            private static Nifti1Header getHeader(byte[] buffer)
            {
                var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                var header = Marshal.PtrToStructure<Nifti1Header>(handle.AddrOfPinnedObject());
                handle.Free();

                return header;
            }

            private const int BufferSizeForCompression = 8192;

            /// <summary>
            /// Writes a Volume3D object to the given stream. The stream is left open after writing, and needs
            /// to disposed of by the caller.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="volume"></param>
            /// <param name="niftiCompression">Defines the compression method that should be applied to the stream.</param>
            /// <param name="datatype"></param>
            /// <param name="writeVoxel"></param>
            /// <param name="stream">The stream to write to. It must be disposed of by the caller.</param>
            /// <returns></returns>
            public static void WriteToStreamWithVoxelWriter<T>(Stream stream, Volume3D<T> volume,
                NiftiCompression niftiCompression,
                Nifti1Datatype datatype,
                Action<BinaryWriter, T> writeVoxel)
            {
                var header = CreateNifti1Header(volume, datatype);
                try
                {
                    switch (niftiCompression)
                    {
                        case NiftiCompression.GZip:
                            using (var gzipStream = new GZipStream(stream, CompressionLevel.Fastest, leaveOpen: true))
                            {
                                using (var buffer = new BufferedStream(gzipStream, BufferSizeForCompression))
                                {
                                    WriteHeaderAndVoxelsToStream(buffer, header, volume, writeVoxel);
                                }
                            }
                            break;
                        case NiftiCompression.LZ4:
                            var lz4Flags = LZ4StreamFlags.IsolateInnerStream | LZ4StreamFlags.HighCompression;
                            using (var lz4Stream = new LZ4Stream(stream, LZ4StreamMode.Compress, lz4Flags))
                            {
                                WriteHeaderAndVoxelsToStream(lz4Stream, header, volume, writeVoxel);
                            }
                            break;
                        case NiftiCompression.Uncompressed:
                            WriteHeaderAndVoxelsToStream(stream, header, volume, writeVoxel);
                            break;
                        default:
                            throw new ArgumentException($"Compression level {niftiCompression} is not handled.", nameof(niftiCompression));
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error writing to stream: {ex.Message}", ex);
                }
            }

            private static void WriteHeaderAndVoxelsToStream<T>(Stream stream, Nifti1Header header, Volume3D<T> volume, Action<BinaryWriter, T> writeVoxel)
            {
                using (var writer = new BinaryWriter(stream, System.Text.Encoding.Default, leaveOpen: true))
                {
                    var newBuffer = getBytes(header);
                    writer.Write(newBuffer);
                    writer.Write(0); // write four empty bytes
                    for (var i = 0; i < volume.Length; i++)
                    {
                        var value = volume[i];
                        writeVoxel(writer, value);
                    }
                }
            }

            /// <summary>
            /// Get a Volume3D object from a nifti stream. The stream can be compressed, if so, it will
            /// be decompressed before further processing.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="stream">The raw input stream to read from, can be compressed.</param>
            /// <param name="niftiCompression">The compression that is applied on the input stream.</param>
            /// <param name="expectedDataTypes">The set of Nifti data types that is considered valid input.
            /// Throws an exception if the stream contains a data type not given here.</param>
            /// <param name="batchConverterCreator">A function to create a converter that maps from the scaled voxel value
            /// (after applying intercept and slope) to the output voxel type.</param>
            /// <returns></returns>
            public static Volume3D<T> ReadStream<T>(Stream stream,
                NiftiCompression niftiCompression,
                IEnumerable<Nifti1Datatype> expectedDataTypes,
                Func<Nifti1Datatype, byte[], T[], float, float, Action<int, int>> batchConverterCreator)
            {
                if (stream == null)
                {
                    throw new ArgumentNullException(nameof(stream));
                }

                if (expectedDataTypes == null)
                {
                    throw new ArgumentNullException(nameof(expectedDataTypes));
                }

                if (batchConverterCreator == null)
                {
                    throw new ArgumentNullException(nameof(batchConverterCreator));
                }

                switch (niftiCompression)
                {
                    case NiftiCompression.Uncompressed:
                        return ReadFromStream<T>(stream, expectedDataTypes, batchConverterCreator);
                    case NiftiCompression.GZip:
                        using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true))
                        {
                            return ReadFromStream<T>(gzipStream, expectedDataTypes, batchConverterCreator);
                        }
                    case NiftiCompression.LZ4:
                        var lz4Flags = LZ4StreamFlags.IsolateInnerStream | LZ4StreamFlags.HighCompression;
                        using (var lz4Stream = new LZ4Stream(stream, LZ4StreamMode.Decompress, lz4Flags))
                        {
                            return ReadFromStream<T>(lz4Stream, expectedDataTypes, batchConverterCreator);
                        }
                    default:
                        throw new ArgumentException($"Compression level {niftiCompression} is not handled.", nameof(niftiCompression));
                }
            }

            /// <summary>
            /// Reads the Nifti header from the given stream reader, and creates an empty volume based on the header
            /// information. The reader is then advanced to the start of the actual voxel information.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="reader"></param>
            /// <param name="expectedDataTypes">The set of Nifti data types that is considered valid input.
            /// Throws an exception if the stream contains a data type not given here.</param>
            /// <returns></returns>
            private static (Volume3D<T> Volume, Nifti1Header Header) ReadHeaderAndCreateVolume<T>(BinaryReader reader, IEnumerable<Nifti1Datatype> expectedDataTypes)
            {
                var buffer = reader.ReadBytes(348);
                var header = getHeader(buffer);
                var expected = new HashSet<Nifti1Datatype>(expectedDataTypes);
                if (!expected.Contains(header.datatype))
                {
                    var validDataTypes = String.Join(", ", expectedDataTypes);
                    throw new InvalidDataException($"Expected to read a Nifti file with one of the following datatypes {validDataTypes}, but got {header.datatype}");
                }
                var problems = FindHeaderValidationProblems(header, strict: false, expected);
                if (problems.Count > 1)
                {
                    // If there is only 1 exception thrown, throw it directly, because aggregate exceptions are more difficult
                    // to parse.
                    if (problems.Count == 1)
                    {
                        throw problems[0];
                    }
                    throw new AggregateException("Invalid NIFTI header in stream", problems);
                }

                var transform = QuaternianToMatrix4(header.quatern_b, header.quatern_c, header.quatern_d,
                    header.qoffset_x, header.qoffset_y, header.qoffset_z,
                    1.0f, 1.0f, 1.0f, header.pixdim[0]);

                // Dicom's coordinate system has +x is Left, +y is Posterior, +z is Superior;
                // Nifti's has +x is Right, +y is Anterior, +z is Superior.
                var niftiToDicomRotation = new Matrix3();
                niftiToDicomRotation.Identity();
                niftiToDicomRotation[0, 0] = -1.0;
                niftiToDicomRotation[1, 1] = -1.0;

                var volume = new Volume3D<T>(
                    header.dim[1], header.dim[2], header.dim[3],
                    header.pixdim[1], header.pixdim[2], header.pixdim[3], // NB NOT pixdim[0,1,2]
                    niftiToDicomRotation * transform.Item2, niftiToDicomRotation * transform.Item1);

                reader.ReadBytes((int)header.vox_offset - 348);
                if (expected.Count > 1)
                {
                    Trace.TraceInformation($"Reading a NIFTI file in format {header.datatype} as .NET datatype {typeof(T).Name}");
                }

                return (volume, header);
            }

            /// <summary>
            /// Reads a Nifti file from a stream, and converts to <see cref="Volume3D{T}"/>. 
            /// The stream is expected to already be decompressed.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="stream">The stream to read from.</param>
            /// <param name="expectedDataTypes">The set of Nifti data types that is considered valid input.
            /// Throws an exception if the stream contains a data type not given here.</param>
            /// <param name="batchConverterCreator">A method to create a converter that maps from the scaled voxel value
            /// (after applying intercept and slope) to the output voxel type.</param>
            /// <returns></returns>
            private static Volume3D<T> ReadFromStream<T>(Stream stream,
                IEnumerable<Nifti1Datatype> expectedDataTypes,
                Func<Nifti1Datatype, byte[], T[], float, float, Action<int, int>> batchConverterCreator)
            {
                using (var reader = new BinaryReader(stream, System.Text.Encoding.Default))
                {
                    var (volume, header) = ReadHeaderAndCreateVolume<T>(reader, expectedDataTypes);
                    var voxels = volume.Array;
                    switch (header.datatype)
                    {
                        case Nifti1Datatype.DT_SIGNED_SHORT:
                            var voxelsAsBytes = reader.ReadBytes(sizeof(short) * voxels.Length);
                            var batchConverterShort = batchConverterCreator(Nifti1Datatype.DT_SIGNED_SHORT, voxelsAsBytes, voxels, header.scl_slope, header.scl_inter);
                            FastParallel.BatchLoop(voxels.Length, Environment.ProcessorCount, batchConverterShort);
                            break;
                        case Nifti1Datatype.DT_UINT16:
                            var uint16AsBytes = reader.ReadBytes(sizeof(ushort) * voxels.Length);
                            var batchConverterUInt16 = batchConverterCreator(Nifti1Datatype.DT_UINT16, uint16AsBytes, voxels, header.scl_slope, header.scl_inter);
                            FastParallel.BatchLoop(voxels.Length, Environment.ProcessorCount, batchConverterUInt16);
                            break;
                        case Nifti1Datatype.DT_FLOAT:
                            var floatsAsBytes = reader.ReadBytes(sizeof(float) * voxels.Length);
                            var batchConverterFloat = batchConverterCreator(Nifti1Datatype.DT_FLOAT, floatsAsBytes, voxels, header.scl_slope, header.scl_inter);
                            FastParallel.BatchLoop(voxels.Length, Environment.ProcessorCount, batchConverterFloat);
                            break;
                        case Nifti1Datatype.DT_UNSIGNED_CHAR:
                            var bytes = reader.ReadBytes(sizeof(byte) * voxels.Length);
                            var batchConverterByte = batchConverterCreator(Nifti1Datatype.DT_UNSIGNED_CHAR, bytes, voxels, header.scl_slope, header.scl_inter);
                            FastParallel.BatchLoop(voxels.Length, Environment.ProcessorCount, batchConverterByte);
                            break;
                        default:
                            throw new InvalidDataException($"The stream contains a NIFTI file in format {header.datatype}, which is presently not supported.");
                    }

                    return volume;
                }
            }

            private static List<InvalidDataException> FindHeaderValidationProblems(Nifti1Header header, bool strict,
                HashSet<Nifti1Datatype> expectedDataTypes)
            {
                var problems = new List<string>();
                // TODO: Validate header and size of volume, otherwise following assumptinons will break
                if (header.sizeof_hdr != 348)
                    problems.Add("The sizeof_hdr field was not equal to 348.");

                // public char dim_info;                  // MRI slice ordering.
                if (header.dim[1] < 1 || header.dim[2] < 1 || header.dim[3] < 1)
                    problems.Add("Spatial dimensions must be greater than 0.");

                // NB This test is right, but possibly too pernickity. Broken writers may put 0's here.
                if (strict && (header.dim[4] != 1 || header.dim[5] != 1 || header.dim[6] != 1 || header.dim[7] != 1))
                    problems.Add("Non-spatial image dimensions other than 1 are not supported.");

                if (!expectedDataTypes.Contains(header.datatype))
                {
                    var knownDataTypes = String.Join(", ", expectedDataTypes);
                    problems.Add($"Unsupported NIFTI datatype {header.datatype}. Expected the file to contain one of these datatypes: {knownDataTypes}");
                }

                switch (header.datatype)
                {
                    case Nifti1Datatype.DT_SIGNED_SHORT:
                        if (header.bitpix != 16)
                            problems.Add("Inconsistent bitpix and datatype field values.");
                        break;
                    case Nifti1Datatype.DT_UINT16:
                        if (header.bitpix != 16)
                            problems.Add("Inconsistent bitpix and datatype field values.");
                        break;
                    case Nifti1Datatype.DT_FLOAT:
                        if (header.bitpix != 32)
                            problems.Add("Inconsistent bitpix and datatype field values.");
                        break;
                    case Nifti1Datatype.DT_UNSIGNED_CHAR:
                        if (header.bitpix != 8)
                            problems.Add("Inconsistent bitpix and datatype field values.");
                        break;
                    default:
                        problems.Add("Unsupported datatype.");
                        break;
                }

                if (header.pixdim[1] < 0.0f || header.pixdim[2] < 0.0f || header.pixdim[3] < 0.0f)
                    problems.Add("Voxel dimensions must be positive.");

                if (header.xyzt_units != Nifti1Units.NIFTI_UNITS_MM)
                    problems.Add($"Units other than mm (like {header.xyzt_units}) are not supported.");

                // TODO: Use cal_max and cal_min?
                // TODO: expose descrip and intent_name fielda?
                // TODO: support two-file NIFTI (via aux_file)

                if (header.qform_code == (short)(Nifti1XFormCode.NIFTI_XFORM_UNKNOWN))
                    problems.Add("Unsupported qform_code.");

                if (header.pixdim[0] != -1.0f && header.pixdim[0] != 1.0f)
                    problems.Add("Invalid qfac field (pixdim[0]).");

                if (header.magic[0] == (byte)('n') && header.magic[1] == (byte)('i') && header.magic[2] != (byte)('1') &&
                    header.magic[3] != (byte)('\0'))
                    problems.Add("Two-file NIFTI is not supported.");

                if (header.magic[0] != (byte)('n') || header.magic[1] != (byte)('+') || header.magic[2] != (byte)('1') ||
                    header.magic[3] != (byte)('\0'))
                    problems.Add("Invalid magic bytes.");

                return problems.Select(s => new InvalidDataException(s)).ToList();
            }

            #region Legacy code to read Nifti - Slow, but retained for testing

            /// <summary>
            /// Get a Volume3D object from a nifti stream, where the stream is expected to have data in signed short format.
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="niftiCompression">Defines the decompression method that should be applied to the stream.</param>
            /// <returns></returns>
            [Obsolete("This is the slow legacy version of Nifti loading, that should only be used for profiling.")]
            public static Volume3D<short> LegacyReadNiftiInShortFormat(Stream stream, NiftiCompression niftiCompression)
            {
                var voxelReaders = new Dictionary<Nifti1Datatype, Func<BinaryReader, Nifti1Header, short>>
            {
                {Nifti1Datatype.DT_SIGNED_SHORT, (r, h) => SingleToShort(h.scl_slope * r.ReadInt16() + h.scl_inter)},
            };
                return LegacyReadStream<short>(stream, voxelReaders, niftiCompression);
            }

            /// <summary>
            /// Get a Volume3D object from a nifti stream, where the stream is expected to have data in byte format.
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="niftiCompression">Defines the decompression method that should be applied to the stream.</param>
            /// <returns></returns>
            [Obsolete("This is the slow legacy version of Nifti loading, that should only be used for profiling.")]
            public static Volume3D<byte> LegacyReadNiftiInByteFormat(Stream stream, NiftiCompression niftiCompression)
            {
                var voxelReaders = new Dictionary<Nifti1Datatype, Func<BinaryReader, Nifti1Header, byte>>
            {
                {Nifti1Datatype.DT_UNSIGNED_CHAR, (r, h) => SingleToByte(h.scl_slope * r.ReadByte() + h.scl_inter)}
            };
                return LegacyReadStream<byte>(stream, voxelReaders, niftiCompression);
            }

            /// <summary>
            /// Get a Volume3D object from a nifti stream, where the stream is expected to have data in single precision
            /// floating point format.
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="niftiCompression">Defines the decompression method that should be applied to the stream.</param>
            /// <returns></returns>
            [Obsolete("This is the slow legacy version of Nifti loading, that should only be used for profiling.")]
            public static Volume3D<float> LegacyReadNiftiInFloatFormat(Stream stream, NiftiCompression niftiCompression)
            {
                var voxelReaders = new Dictionary<Nifti1Datatype, Func<BinaryReader, Nifti1Header, float>>
            {
                {Nifti1Datatype.DT_FLOAT, (r, h) => h.scl_slope * r.ReadSingle() + h.scl_inter},
            };
                return LegacyReadStream<float>(stream, voxelReaders, niftiCompression);
            }

            /// <summary>
            /// Get a Volume3D object from a nifti stream.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="stream"></param>
            /// <param name="voxelReaders"></param>
            /// <param name="niftiCompression"></param>
            /// <param name="strict"></param>
            /// <returns></returns>
            private static Volume3D<T> LegacyReadStream<T>(Stream stream,
                Dictionary<Nifti1Datatype, Func<BinaryReader, Nifti1Header, T>> voxelReaders,
                NiftiCompression niftiCompression)
            {
                var strict = false;
                switch (niftiCompression)
                {
                    case NiftiCompression.Uncompressed:
                        return LegacyReadFromStream<T>(stream, strict, voxelReaders);
                    case NiftiCompression.GZip:
                        using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true))
                        {
                            return LegacyReadFromStream<T>(gzipStream, strict, voxelReaders);
                        }
                    case NiftiCompression.LZ4:
                        var lz4Flags = LZ4StreamFlags.IsolateInnerStream | LZ4StreamFlags.HighCompression;
                        using (var lz4Stream = new LZ4Stream(stream, LZ4StreamMode.Decompress, lz4Flags))
                        {
                            return LegacyReadFromStream<T>(lz4Stream, strict, voxelReaders);
                        }
                    default:
                        throw new ArgumentException($"Compression level {niftiCompression} is not handled.", nameof(niftiCompression));
                }
            }

            private static Volume3D<T> LegacyReadFromStream<T>(Stream stream, bool strict,
                Dictionary<Nifti1Datatype, Func<BinaryReader, Nifti1Header, T>> voxelReaders)
            {
                using (var reader = new BinaryReader(stream, System.Text.Encoding.Default))
                {
                    var (volume, header) = ReadHeaderAndCreateVolume<T>(reader, voxelReaders.Keys);
                    var readVoxel = voxelReaders[header.datatype];
                    var voxels = volume.Array;
                    for (var i = 0; i < voxels.Length; i++)
                    {
                        voxels[i] = readVoxel(reader, header);
                    }

                    return volume;
                }
            }

            #endregion
        }

        /*---------------------------------------------------------------------------*/
        /* HEADER EXTENSIONS:
           -----------------
           After the end of the 348 byte header (e.g., after the magic field),
           the next 4 bytes are a char array field named "extension". By default,
           all 4 bytes of this array should be set to zero. In a .nii file, these
           4 bytes will always be present, since the earliest start point for
           the image data is byte #352. In a separate .hdr file, these bytes may
           or may not be present. If not present (i.e., if the length of the .hdr
           file is 348 bytes), then a NIfTI-1 compliant program should use the
           default value of extension={0,0,0,0}. The first byte (extension[0])
           is the only value of this array that is specified at present. The other
           3 bytes are reserved for future use.

           If extension[0] is nonzero, it indicates that extended header information
           is present in the bytes following the extension array. In a .nii file,
           this extended header data is before the image data (and vox_offset
           must be set correctly to allow for this). In a .hdr file, this extended
           data follows extension and proceeds (potentially) to the end of the file.

           The format of extended header data is weakly specified. Each extension
           must be an integer multiple of 16 bytes long. The first 8 bytes of each
           extension comprise 2 integers:
              int esize , ecode ;
           These values may need to be byte-swapped, as indicated by dim[0] for
           the rest of the header.
             * esize is the number of bytes that form the extended header data
               + esize must be a positive integral multiple of 16
               + this length includes the 8 bytes of esize and ecode themselves
             * ecode is a non-negative integer that indicates the format of the
               extended header data that follows
               + different ecode values are assigned to different developer groups
               + at present, the "registered" values for code are
                 = 0 = unknown private format (not recommended!)
                 = 2 = DICOM format (i.e., attribute tags and values)
                 = 4 = AFNI group (i.e., ASCII XML-ish elements)
           In the interests of interoperability (a primary rationale for NIfTI),
           groups developing software that uses this extension mechanism are
           encouraged to document and publicize the format of their extensions.
           To this end, the NIfTI DFWG will assign even numbered codes upon request
           to groups submitting at least rudimentary documentation for the format
           of their extension; at present, the contact is mailto:rwcox@nih.gov.
           The assigned codes and documentation will be posted on the NIfTI
           website. All odd values of ecode (and 0) will remain unassigned;
           at least, until the even ones are used up, when we get to 2,147,483,646.

           Note that the other contents of the extended header data section are
           totally unspecified by the NIfTI-1 standard. In particular, if binary
           data is stored in such a section, its byte order is not necessarily
           the same as that given by examining dim[0]; it is incumbent on the
           programs dealing with such data to determine the byte order of binary
           extended header data.

           Multiple extended header sections are allowed, each starting with an
           esize,ecode value pair. The first esize value, as described above,
           is at bytes #352-355 in the .hdr or .nii file (files start at byte #0).
           If this value is positive, then the second (esize2) will be found
           starting at byte #352+esize1 , the third (esize3) at byte #352+esize1+esize2,
           et cetera.  Of course, in a .nii file, the value of vox_offset must
           be compatible with these extensions. If a malformed file indicates
           that an extended header data section would run past vox_offset, then
           the entire extended header section should be ignored. In a .hdr file,
           if an extended header data section would run past the end-of-file,
           that extended header data should also be ignored.

           With the above scheme, a program can successively examine the esize
           and ecode values, and skip over each extended header section if the
           program doesn't know how to interpret the data within. Of course, any
           program can simply ignore all extended header sections simply by jumping
           straight to the image data using vox_offset.
        -----------------------------------------------------------------------------*/

        /*---------------------------------------------------------------------------*/
        /* DATA DIMENSIONALITY (as in ANALYZE 7.5):
           ---------------------------------------
             dim[0] = number of dimensions;
                      - if dim[0] is outside range 1..7, then the header information
                        needs to be byte swapped appropriately
                      - ANALYZE supports dim[0] up to 7, but NIFTI-1 reserves
                        dimensions 1,2,3 for space (x,y,z), 4 for time (t), and
                        5,6,7 for anything else needed.

             dim[i] = length of dimension #i, for i=1..dim[0]  (must be positive)
                      - also see the discussion of intent_code, far below

             pixdim[i] = voxel width along dimension #i, i=1..dim[0] (positive)
                         - cf. ORIENTATION section below for use of pixdim[0]
                         - the units of pixdim can be specified with the xyzt_units
                           field (also described far below).

           Number of bits per voxel value is in bitpix, which MUST correspond with
           the datatype field.  The total number of bytes in the image data is
             dim[1] * ... * dim[dim[0]] * bitpix / 8

           In NIFTI-1 files, dimensions 1,2,3 are for space, dimension 4 is for time,
           and dimension 5 is for storing multiple values at each spatiotemporal
           voxel.  Some examples:
             - A typical whole-brain FMRI experiment's time series:
                - dim[0] = 4
                - dim[1] = 64   pixdim[1] = 3.75 xyzt_units =  NIFTI_UNITS_MM
                - dim[2] = 64   pixdim[2] = 3.75             | NIFTI_UNITS_SEC
                - dim[3] = 20   pixdim[3] = 5.0
                - dim[4] = 120  pixdim[4] = 2.0
             - A typical T1-weighted anatomical volume:
                - dim[0] = 3
                - dim[1] = 256  pixdim[1] = 1.0  xyzt_units = NIFTI_UNITS_MM
                - dim[2] = 256  pixdim[2] = 1.0
                - dim[3] = 128  pixdim[3] = 1.1
             - A single slice EPI time series:
                - dim[0] = 4
                - dim[1] = 64   pixdim[1] = 3.75 xyzt_units =  NIFTI_UNITS_MM
                - dim[2] = 64   pixdim[2] = 3.75             | NIFTI_UNITS_SEC
                - dim[3] = 1    pixdim[3] = 5.0
                - dim[4] = 1200 pixdim[4] = 0.2
             - A 3-vector stored at each point in a 3D volume:
                - dim[0] = 5
                - dim[1] = 256  pixdim[1] = 1.0  xyzt_units = NIFTI_UNITS_MM
                - dim[2] = 256  pixdim[2] = 1.0
                - dim[3] = 128  pixdim[3] = 1.1
                - dim[4] = 1    pixdim[4] = 0.0
                - dim[5] = 3                     intent_code = NIFTI_INTENT_VECTOR
             - A single time series with a 3x3 matrix at each point:
                - dim[0] = 5
                - dim[1] = 1                     xyzt_units = NIFTI_UNITS_SEC
                - dim[2] = 1
                - dim[3] = 1
                - dim[4] = 1200 pixdim[4] = 0.2
                - dim[5] = 9                     intent_code = NIFTI_INTENT_GENMATRIX
                - intent_p1 = intent_p2 = 3.0    (indicates matrix dimensions)
        -----------------------------------------------------------------------------*/

        /*---------------------------------------------------------------------------*/
        /* DATA STORAGE:
           ------------
           If the magic field is "n+1", then the voxel data is stored in the
           same file as the header.  In this case, the voxel data starts at offset
           (int)vox_offset into the header file.  Thus, vox_offset=352.0 means that
           the data starts immediately after the NIFTI-1 header.  If vox_offset is
           greater than 352, the NIFTI-1 format does not say much about the
           contents of the dataset file between the end of the header and the
           start of the data.

           FILES:
           -----
           If the magic field is "ni1", then the voxel data is stored in the
           associated ".img" file, starting at offset 0 (i.e., vox_offset is not
           used in this case, and should be set to 0.0).

           When storing NIFTI-1 datasets in pairs of files, it is customary to name
           the files in the pattern "name.hdr" and "name.img", as in ANALYZE 7.5.
           When storing in a single file ("n+1"), the file name should be in
           the form "name.nii" (the ".nft" and ".nif" suffixes are already taken;
           cf. http://www.icdatamaster.com/n.html ).

           BYTE ORDERING:
           -------------
           The byte order of the data arrays is presumed to be the same as the byte
           order of the header (which is determined by examining dim[0]).

           Floating point types are presumed to be stored in IEEE-754 format.
        -----------------------------------------------------------------------------*/

        /*---------------------------------------------------------------------------*/
        /* DETAILS ABOUT vox_offset:
           ------------------------
           In a .nii file, the vox_offset field value is interpreted as the start
           location of the image data bytes in that file. In a .hdr/.img file pair,
           the vox_offset field value is the start location of the image data
           bytes in the .img file.
            * If vox_offset is less than 352 in a .nii file, it is equivalent
              to 352 (i.e., image data never starts before byte #352 in a .nii file).
            * The default value for vox_offset in a .nii file is 352.
            * In a .hdr file, the default value for vox_offset is 0.
            * vox_offset should be an integer multiple of 16; otherwise, some
              programs may not work properly (e.g., SPM). This is to allow
              memory-mapped input to be properly byte-aligned.
           Note that since vox_offset is an IEEE-754 32 bit float (for compatibility
           with the ANALYZE-7.5 format), it effectively has a 24 bit mantissa. All
           integers from 0 to 2^24 can be represented exactly in this format, but not
           all larger integers are exactly storable as IEEE-754 32 bit floats. However,
           unless you plan to have vox_offset be potentially larger than 16 MB, this
           should not be an issue. (Actually, any integral multiple of 16 up to 2^27
           can be represented exactly in this format, which allows for up to 128 MB
           of random information before the image data.  If that isn't enough, then
           perhaps this format isn't right for you.)

           In a .img file (i.e., image data stored separately from the NIfTI-1
           header), data bytes between #0 and #vox_offset-1 (inclusive) are completely
           undefined and unregulated by the NIfTI-1 standard. One potential use of
           having vox_offset > 0 in the .hdr/.img file pair storage method is to make
           the .img file be a copy of (or link to) a pre-existing image file in some
           other format, such as DICOM; then vox_offset would be set to the offset of
           the image data in this file. (It may not be possible to follow the
           "multiple-of-16 rule" with an arbitrary external file; using the NIfTI-1
           format in such a case may lead to a file that is incompatible with software
           that relies on vox_offset being a multiple of 16.)

           In a .nii file, data bytes between #348 and #vox_offset-1 (inclusive) may
           be used to store user-defined extra information; similarly, in a .hdr file,
           any data bytes after byte #347 are available for user-defined extra
           information. The (very weak) regulation of this extra header data is
           described elsewhere.
        -----------------------------------------------------------------------------*/

        /*---------------------------------------------------------------------------*/
        /* DATA SCALING:
           ------------
           If the scl_slope field is nonzero, then each voxel value in the dataset
           should be scaled as
              y = scl_slope * x + scl_inter
           where x = voxel value stored
                 y = "true" voxel value
           Normally, we would expect this scaling to be used to store "true" floating
           values in a smaller integer datatype, but that is not required.  That is,
           it is legal to use scaling even if the datatype is a float type.
            - However, the scaling is to be ignored if datatype is DT_RGB24.
            - If datatype is a complex type, then the scaling is to be
              applied to both the real and imaginary parts.

           The cal_min and cal_max fields (if nonzero) are used for mapping (possibly
           scaled) dataset values to display colors:
            - Minimum display intensity (black) corresponds to dataset value cal_min.
            - Maximum display intensity (white) corresponds to dataset value cal_max.
            - Dataset values below cal_min should display as black also, and values
              above cal_max as white.
            - Colors "black" and "white", of course, may refer to any scalar display
              scheme (e.g., a color lookup table specified via aux_file).
            - cal_min and cal_max only make sense when applied to scalar-valued
              datasets (i.e., dim[0] < 5 or dim[5] = 1).
        -----------------------------------------------------------------------------*/

        /*---------------------------------------------------------------------------*/
        /* TYPE OF DATA (acceptable values for datatype field):
           ---------------------------------------------------
           Values of datatype smaller than 256 are ANALYZE 7.5 compatible.
           Larger values are NIFTI-1 additions.  These are all multiples of 256, so
           that no bits below position 8 are set in datatype.  But there is no need
           to use only powers-of-2, as the original ANALYZE 7.5 datatype codes do.

           The additional codes are intended to include a complete list of basic
           scalar types, including signed and unsigned integers from 8 to 64 bits,
           floats from 32 to 128 bits, and complex (float pairs) from 64 to 256 bits.

           Note that most programs will support only a few of these datatypes!
           A NIFTI-1 program should fail gracefully (e.g., print a warning message)
           when it encounters a dataset with a type it doesn't like.
        -----------------------------------------------------------------------------*/

        /*---------------------------------------------------------------------------*/
        /* INTERPRETATION OF VOXEL DATA:
           ----------------------------
           The intent_code field can be used to indicate that the voxel data has
           some particular meaning.  In particular, a large number of codes is
           given to indicate that the the voxel data should be interpreted as
           being drawn from a given probability distribution.

           VECTOR-VALUED DATASETS:
           ----------------------
           The 5th dimension of the dataset, if present (i.e., dim[0]=5 and
           dim[5] > 1), contains multiple values (e.g., a vector) to be stored
           at each spatiotemporal location.  For example, the header values
            - dim[0] = 5
            - dim[1] = 64
            - dim[2] = 64
            - dim[3] = 20
            - dim[4] = 1     (indicates no time axis)
            - dim[5] = 3
            - datatype = DT_FLOAT
            - intent_code = NIFTI_INTENT_VECTOR
           mean that this dataset should be interpreted as a 3D volume (64x64x20),
           with a 3-vector of floats defined at each point in the 3D grid.

           A program reading a dataset with a 5th dimension may want to reformat
           the image data to store each voxels' set of values together in a struct
           or array.  This programming detail, however, is beyond the scope of the
           NIFTI-1 file specification!  Uses of dimensions 6 and 7 are also not
           specified here.

           STATISTICAL PARAMETRIC DATASETS (i.e., SPMs):
           --------------------------------------------
           Values of intent_code from NIFTI_FIRST_STATCODE to NIFTI_LAST_STATCODE
           (inclusive) indicate that the numbers in the dataset should be interpreted
           as being drawn from a given distribution.  Most such distributions have
           auxiliary parameters (e.g., NIFTI_INTENT_TTEST has 1 DOF parameter).

           If the dataset DOES NOT have a 5th dimension, then the auxiliary parameters
           are the same for each voxel, and are given in header fields intent_p1,
           intent_p2, and intent_p3.

           If the dataset DOES have a 5th dimension, then the auxiliary parameters
           are different for each voxel.  For example, the header values
            - dim[0] = 5
            - dim[1] = 128
            - dim[2] = 128
            - dim[3] = 1      (indicates a single slice)
            - dim[4] = 1      (indicates no time axis)
            - dim[5] = 2
            - datatype = DT_FLOAT
            - intent_code = NIFTI_INTENT_TTEST
           mean that this is a 2D dataset (128x128) of t-statistics, with the
           t-statistic being in the first "plane" of data and the degrees-of-freedom
           parameter being in the second "plane" of data.

           If the dataset 5th dimension is used to store the voxel-wise statistical
           parameters, then dim[5] must be 1 plus the number of parameters required
           by that distribution (e.g., intent_code=NIFTI_INTENT_TTEST implies dim[5]
           must be 2, as in the example just above).

           Note: intent_code values 2..10 are compatible with AFNI 1.5x (which is
           why there is no code with value=1, which is obsolescent in AFNI).

           OTHER INTENTIONS:
           ----------------
           The purpose of the intent_* fields is to help interpret the values
           stored in the dataset.  Some non-statistical values for intent_code
           and conventions are provided for storing other complex data types.

           The intent_name field provides space for a 15 character (plus 0 byte)
           'name' string for the type of data stored. Examples:
            - intent_code = NIFTI_INTENT_ESTIMATE; intent_name = "T1";
               could be used to signify that the voxel values are estimates of the
               NMR parameter T1.
            - intent_code = NIFTI_INTENT_TTEST; intent_name = "House";
               could be used to signify that the voxel values are t-statistics
               for the significance of 'activation' response to a House stimulus.
            - intent_code = NIFTI_INTENT_DISPVECT; intent_name = "ToMNI152";
               could be used to signify that the voxel values are a displacement
               vector that transforms each voxel (x,y,z) location to the
               corresponding location in the MNI152 standard brain.
            - intent_code = NIFTI_INTENT_SYMMATRIX; intent_name = "DTI";
               could be used to signify that the voxel values comprise a diffusion
               tensor image.

           If no data name is implied or needed, intent_name[0] should be set to 0.
        -----------------------------------------------------------------------------*/

        enum NiftiIntentCodes
        {
            // default: no intention is indicated in the header.
            NIFTI_INTENT_NONE = 0,

            /*-------- These codes are for probability distributions ---------------*/
            /* Most distributions have a number of parameters,
               below denoted by p1, p2, and p3, and stored in
                - intent_p1, intent_p2, intent_p3 if dataset doesn't have 5th dimension
                - image data array                if dataset does have 5th dimension

               Functions to compute with many of the distributions below can be found
               in the CDF library from U Texas.

               Formulas for and discussions of these distributions can be found in the
               following books:

                [U] Univariate Discrete Distributions,
                    NL Johnson, S Kotz, AW Kemp.

                [C1] Continuous Univariate Distributions, vol. 1,
                     NL Johnson, S Kotz, N Balakrishnan.

                [C2] Continuous Univariate Distributions, vol. 2,
                     NL Johnson, S Kotz, N Balakrishnan.                            */
            /*----------------------------------------------------------------------*/

            /*! [C2, chap 32] Correlation coefficient R (1 param):
                 p1 = degrees of freedom
                 R/sqrt(1-R*R) is t-distributed with p1 DOF. */
            NIFTI_INTENT_CORREL = 2,

            /*! [C2, chap 28] Student t statistic (1 param): p1 = DOF. */
            NIFTI_INTENT_TTEST = 3,

            /*! [C2, chap 27] Fisher F statistic (2 params):
                 p1 = numerator DOF, p2 = denominator DOF. */
            NIFTI_INTENT_FTEST = 4,

            /*! [C1, chap 13] Standard normal (0 params): Density = N(0,1). */
            NIFTI_INTENT_ZSCORE = 5,

            /*! [C1, chap 18] Chi-squared (1 param): p1 = DOF.
                Density(x) proportional to exp(-x/2) * x^(p1/2-1). */

            NIFTI_INTENT_CHISQ = 6,

            /*! [C2, chap 25] Beta distribution (2 params): p1=a, p2=b.
                Density(x) proportional to x^(a-1) * (1-x)^(b-1). */

            NIFTI_INTENT_BETA = 7,

            /*! [U, chap 3] Binomial distribution (2 params):
                 p1 = number of trials, p2 = probability per trial.
                Prob(x) = (p1 choose x) * p2^x * (1-p2)^(p1-x), for x=0,1,...,p1. */

            NIFTI_INTENT_BINOM = 8,

            /*! [C1, chap 17] Gamma distribution (2 params):
                 p1 = shape, p2 = scale.
                Density(x) proportional to x^(p1-1) * exp(-p2*x). */

            NIFTI_INTENT_GAMMA = 9,

            /*! [U, chap 4] Poisson distribution (1 param): p1 = mean.
                Prob(x) = exp(-p1) * p1^x / x! , for x=0,1,2,.... */

            NIFTI_INTENT_POISSON = 10,

            /*! [C1, chap 13] Normal distribution (2 params):
                 p1 = mean, p2 = standard deviation. */

            NIFTI_INTENT_NORMAL = 11,

            /*! [C2, chap 30] Noncentral F statistic (3 params):
                 p1 = numerator DOF, p2 = denominator DOF,
                 p3 = numerator noncentrality parameter.  */

            NIFTI_INTENT_FTEST_NONC = 12,

            /*! [C2, chap 29] Noncentral chi-squared statistic (2 params):
                 p1 = DOF, p2 = noncentrality parameter.     */

            NIFTI_INTENT_CHISQ_NONC = 13,

            /*! [C2, chap 23] Logistic distribution (2 params):
                 p1 = location, p2 = scale.
                Density(x) proportional to sech^2((x-p1)/(2*p2)). */

            NIFTI_INTENT_LOGISTIC = 14,

            /*! [C2, chap 24] Laplace distribution (2 params):
                 p1 = location, p2 = scale.
                Density(x) proportional to exp(-abs(x-p1)/p2). */

            NIFTI_INTENT_LAPLACE = 15,

            /*! [C2, chap 26] Uniform distribution: p1 = lower end, p2 = upper end. */

            NIFTI_INTENT_UNIFORM = 16,

            /*! [C2, chap 31] Noncentral t statistic (2 params):
                 p1 = DOF, p2 = noncentrality parameter. */

            NIFTI_INTENT_TTEST_NONC = 17,

            /*! [C1, chap 21] Weibull distribution (3 params):
                 p1 = location, p2 = scale, p3 = power.
                Density(x) proportional to
                 ((x-p1)/p2)^(p3-1) * exp(-((x-p1)/p2)^p3) for x > p1. */

            NIFTI_INTENT_WEIBULL = 18,

            /*! [C1, chap 18] Chi distribution (1 param): p1 = DOF.
                Density(x) proportional to x^(p1-1) * exp(-x^2/2) for x > 0.
                 p1 = 1 = 'half normal' distribution
                 p1 = 2 = Rayleigh distribution
                 p1 = 3 = Maxwell-Boltzmann distribution.                  */

            NIFTI_INTENT_CHI = 19,

            /*! [C1, chap 15] Inverse Gaussian (2 params):
                 p1 = mu, p2 = lambda
                Density(x) proportional to
                 exp(-p2*(x-p1)^2/(2*p1^2*x)) / x^3  for x > 0. */

            NIFTI_INTENT_INVGAUSS = 20,

            /*! [C2, chap 22] Extreme value type I (2 params):
                 p1 = location, p2 = scale
                cdf(x) = exp(-exp(-(x-p1)/p2)). */

            NIFTI_INTENT_EXTVAL = 21,

            /*! Data is a 'p-value' (no params). */

            NIFTI_INTENT_PVAL = 22,

            /*! Data is ln(p-value) (no params).
                To be safe, a program should compute p = exp(-abs(this_value)).
                The nifti_stats.c library returns this_value
                as positive, so that this_value = -log(p). */


            NIFTI_INTENT_LOGPVAL = 23,

            /*! Data is log10(p-value) (no params).
                To be safe, a program should compute p = pow(10.,-abs(this_value)).
                The nifti_stats.c library returns this_value
                as positive, so that this_value = -log10(p). */

            NIFTI_INTENT_LOG10PVAL = 24,

            /*! Smallest intent_code that indicates a statistic. */

            NIFTI_FIRST_STATCODE = 2,

            /*! Largest intent_code that indicates a statistic. */

            NIFTI_LAST_STATCODE = 24,

            /*---------- these values for intent_code aren't for statistics ----------*/

            /*! To signify that the value at each voxel is an estimate
                of some parameter, set intent_code = NIFTI_INTENT_ESTIMATE.
                The name of the parameter may be stored in intent_name.     */

            NIFTI_INTENT_ESTIMATE = 1001,

            /*! To signify that the value at each voxel is an index into
                some set of labels, set intent_code = NIFTI_INTENT_LABEL.
                The filename with the labels may stored in aux_file.        */

            NIFTI_INTENT_LABEL = 1002,

            /*! To signify that the value at each voxel is an index into the
                NeuroNames labels set, set intent_code = NIFTI_INTENT_NEURONAME. */

            NIFTI_INTENT_NEURONAME = 1003,

            /*! To store an M x N matrix at each voxel:
                  - dataset must have a 5th dimension (dim[0]=5 and dim[5]>1)
                  - intent_code must be NIFTI_INTENT_GENMATRIX
                  - dim[5] must be M*N
                  - intent_p1 must be M (in float format)
                  - intent_p2 must be N (ditto)
                  - the matrix values A[i][[j] are stored in row-order:
                    - A[0][0] A[0][1] ... A[0][N-1]
                    - A[1][0] A[1][1] ... A[1][N-1]
                    - etc., until
                    - A[M-1][0] A[M-1][1] ... A[M-1][N-1]        */

            NIFTI_INTENT_GENMATRIX = 1004,

            /*! To store an NxN symmetric matrix at each voxel:
                  - dataset must have a 5th dimension
                  - intent_code must be NIFTI_INTENT_SYMMATRIX
                  - dim[5] must be N*(N+1)/2
                  - intent_p1 must be N (in float format)
                  - the matrix values A[i][[j] are stored in row-order:
                    - A[0][0]
                    - A[1][0] A[1][1]
                    - A[2][0] A[2][1] A[2][2]
                    - etc.: row-by-row                           */

            NIFTI_INTENT_SYMMATRIX = 1005,

            /*! To signify that the vector value at each voxel is to be taken
                as a displacement field or vector:
                  - dataset must have a 5th dimension
                  - intent_code must be NIFTI_INTENT_DISPVECT
                  - dim[5] must be the dimensionality of the displacment
                    vector (e.g., 3 for spatial displacement, 2 for in-plane) */

            NIFTI_INTENT_DISPVECT = 1006,  /* specifically for displacements */
            NIFTI_INTENT_VECTOR = 1007,   /* for any other type of vector */

            /*! To signify that the vector value at each voxel is really a
                spatial coordinate (e.g., the vertices or nodes of a surface mesh):
                  - dataset must have a 5th dimension
                  - intent_code must be NIFTI_INTENT_POINTSET
                  - dim[0] = 5
                  - dim[1] = number of points
                  - dim[2] = dim[3] = dim[4] = 1
                  - dim[5] must be the dimensionality of space (e.g., 3 => 3D space).
                  - intent_name may describe the object these points come from
                    (e.g., "pial", "gray/white" , "EEG", "MEG").                   */

            NIFTI_INTENT_POINTSET = 1008,

            /*! To signify that the vector value at each voxel is really a triple
                of indexes (e.g., forming a triangle) from a pointset dataset:
                  - dataset must have a 5th dimension
                  - intent_code must be NIFTI_INTENT_TRIANGLE
                  - dim[0] = 5
                  - dim[1] = number of triangles
                  - dim[2] = dim[3] = dim[4] = 1
                  - dim[5] = 3
                  - datatype should be an integer type (preferably DT_INT32)
                  - the data values are indexes (0,1,...) into a pointset dataset. */

            NIFTI_INTENT_TRIANGLE = 1009,

            /*! To signify that the vector value at each voxel is a quaternion:
                  - dataset must have a 5th dimension
                  - intent_code must be NIFTI_INTENT_QUATERNION
                  - dim[0] = 5
                  - dim[5] = 4
                  - datatype should be a floating point type     */

            NIFTI_INTENT_QUATERNION = 1010,

            /*! Dimensionless value - no params - although, as in _ESTIMATE
                the name of the parameter may be stored in intent_name.     */

            NIFTI_INTENT_DIMLESS = 1011,

            /*---------- these values apply to GIFTI datasets ----------*/

            /*! To signify that the value at each location is from a time series. */

            NIFTI_INTENT_TIME_SERIES = 2001,

            /*! To signify that the value at each location is a node index, from
                a complete surface dataset.                                       */

            NIFTI_INTENT_NODE_INDEX = 2002,

            /*! To signify that the vector value at each location is an RGB triplet,
                of whatever type.
                  - dataset must have a 5th dimension
                  - dim[0] = 5
                  - dim[1] = number of nodes
                  - dim[2] = dim[3] = dim[4] = 1
                  - dim[5] = 3
               */

            NIFTI_INTENT_RGB_VECTOR = 2003,

            /*! To signify that the vector value at each location is a 4 valued RGBA
                vector, of whatever type.
                  - dataset must have a 5th dimension
                  - dim[0] = 5
                  - dim[1] = number of nodes
                  - dim[2] = dim[3] = dim[4] = 1
                  - dim[5] = 4
               */

            NIFTI_INTENT_RGBA_VECTOR = 2004,

            /*! To signify that the value at each location is a shape value, such
                as the curvature.  */

            NIFTI_INTENT_SHAPE = 2005
        }

        /*---------------------------------------------------------------------------*/
        /* 3D IMAGE (VOLUME) ORIENTATION AND LOCATION IN SPACE:
           ---------------------------------------------------
           There are 3 different methods by which continuous coordinates can
           attached to voxels.  The discussion below emphasizes 3D volumes, and
           the continuous coordinates are referred to as (x,y,z).  The voxel
           index coordinates (i.e., the array indexes) are referred to as (i,j,k),
           with valid ranges:
             i = 0 .. dim[1]-1
             j = 0 .. dim[2]-1  (if dim[0] >= 2)
             k = 0 .. dim[3]-1  (if dim[0] >= 3)
           The (x,y,z) coordinates refer to the CENTER of a voxel.  In methods
           2 and 3, the (x,y,z) axes refer to a subject-based coordinate system,
           with
             +x = Right  +y = Anterior  +z = Superior.
           This is a right-handed coordinate system.  However, the exact direction
           these axes point with respect to the subject depends on qform_code
           (Method 2) and sform_code (Method 3).

           N.B.: The i index varies most rapidly, j index next, k index slowest.
            Thus, voxel (i,j,k) is stored starting at location
              (i + j*dim[1] + k*dim[1]*dim[2]) * (bitpix/8)