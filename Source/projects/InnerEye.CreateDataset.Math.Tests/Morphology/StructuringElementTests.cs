///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿using InnerEye.CreateDataset.Math.Morphology;
using InnerEye.CreateDataset.Volumes;

using NUnit.Framework;

namespace InnerEye.CreateDataset.Math.Tests.Morphology
{
    [TestFixture]
    public class StructuringElementTests
    {
        ///<summary>
        /// Test to capture encoding of the structuring element
        ///</summary>
        [Test]
        public void StructuringE