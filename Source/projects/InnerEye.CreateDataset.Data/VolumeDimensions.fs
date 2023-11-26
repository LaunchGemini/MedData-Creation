namespace InnerEye.CreateDataset.Data

///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

open System
open InnerEye.CreateDataset.Volumes
open System.Diagnostics

/// Describes the size of a 3D volume.
[<CLIMutableAttribute>]
type Volume3DDimensions =
    {
        /// The size of the volume in the X dimension
        X: int
        /// The size of the volume in the Y dimension
        Y: int
        /// The size of the volume in the Z dimension
        Z: int
    }

    override this.ToString() = sprintf "%i x %i x %i" this.X this.Y this.Z

    /// Creates a Volume3DDimensions instance from the arguments.
    static member Create (dimX, dimY, dimZ) = { X = dimX; Y = dimY; Z = dimZ }

    /// Creates a VolumeDimensions instance that stores the size of the given Volume3D instance.
    static member Create (volume: Volume3D<_>) = 
        { X = volume.DimX; Y = volume.DimY; Z = volume.DimZ }

    /// Returns true if the volume dimensions in the present object are strictly smaller in each dimension
    /// than the v