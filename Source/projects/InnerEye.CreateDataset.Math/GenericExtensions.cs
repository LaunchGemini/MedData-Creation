///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Math
{
    using System;

    public static class GenericExtensions
    {
        /// <summary>
        /// Sets all entries in the array to the given value.
        /// </summary>
        public static void Fill<T>(this T[] array, T value)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }

        /// <summary>
        /// Creates an array of the 