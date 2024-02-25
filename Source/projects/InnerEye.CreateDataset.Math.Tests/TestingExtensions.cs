///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿using NUnit.Framework;
using System;

namespace InnerEye.Tests.Common
{
    public static class TestingExtension
    {
        public static void Throws<T>(Action task, string expectedMessage = "") where T : Exception
        {
            try
            {
                task();
            }
            catch (Exception ex)
            {
                if (expectedMessage != "")