///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace MedLib.IO.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using MedLib.IO;
    using NUnit.Framework;

    [TestFixture]
    public class FastParallelTests
    {
        [Test]
        // maxThreads == null should run a plain vanilla for loop.
        [TestCase(null)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(10)]
        public void FastParallelLoop(int? maxThreads)
        {
            FastParallel.Loop(0, maxThreads, _ => Assert.Fail("When count is empty, no action should be called"));
            foreach (var count in Enumerable.Range(1, 20))
            {
                var executed = new ConcurrentBag<int>();
                FastParallel.Loop(count, maxThreads, index =>
                {
                    if (executed.Contains(index))
                    {
                        Assert.Fail($"Action for index {index} has been called already.");
                    }
                    executed.Add(index);
                });
                var expected = Enumerable.Range(0, count).ToArray();
                var actual = executed.