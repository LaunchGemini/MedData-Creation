///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

namespace InnerEye.CreateDataset.Volumes
{
    using System;

    public class Matrix4
    {
        public Matrix4()
        {
            Data = new double[16];
        }

        public Matrix4(Matrix4 matrix)
        {
            Data = new double[16];
            Array.Copy(matrix.Data, this.Data, this.Data.Length);
        }
        
        public double this[int row, int column]
        {
            get { return Data[row + column * 4]; }
       