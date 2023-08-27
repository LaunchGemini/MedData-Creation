
///  ------------------------------------------------------------------------------------------
///  Copyright (c) Microsoft Corporation. All rights reserved.
///  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
///  ------------------------------------------------------------------------------------------

﻿namespace InnerEye.CreateDataset.Core
{
    class CommandlineCreateDatasetRecipes
    {

        /// <summary>
        /// Array of arrays of (parsed) prespecified name mappings. The value of the switch "--renameIndex"
        /// should be a valid index of this array, and the relevant (sub)array is then used instead of
        /// the result of parsing the value of "--rename".
        /// </summary>
        public static readonly NameMapping[][] PrespecifiedNameMappings = new[]
        {
            // --renameIndex=0
            new[]
            {
                // This one has to come before its uses for mpc_muscle and spc_muscle
                new NameMapping("almost_full_pc", new[] {
                    "full_pc.lt.toppc_split",
                    "full_pcag.lt.toppc_split",
                }),
                new NameMapping("brain", new[] {
                    "brainag",
                    "brain",
                    "brain2",
                    "brian",
                    "whole brain",
                    "whole brain1",
                    "wholebrain"
                }),
                new NameMapping("brainstem", new[] {
                    "brainstemag",
                    "brainstem ag",
                    "brainstem_ag",
                    "brainstem",
                    "brain stem",
                    "brainstem_new",
                    "avoid b stem",
                    "bs opt",
                    "bsopt",
                }),
                new NameMapping("cochlea_l", new[] {
                    "cochlea_lag",
                    "left cochlea",
                    "cochlea l",
                    "cochlea_l",
                    "l cochlea",
                    "cochlea lt",
                    "lcochlea",
                    "lt cochlea",
                    "lt cochlea_new"
                }),
                new NameMapping("cochlea_r", new[] {
                    "cochlea_rag",
                    "right cochlea",
                    "cochlea r",
                    "r cochlea",
                    "cochlea rt",
                    "cochlea_r",
                    "right coclea",
                    "rt cochlea",
                    "rt cochlea_new",
                }),
                new NameMapping("external", new[] {
                    "external",
                    "skin",
                    "skinshell",
                    "surfaceskin"
                }),
                new NameMapping("globe_l", new[] {