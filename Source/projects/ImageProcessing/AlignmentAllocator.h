/*  ------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
 *  ------------------------------------------------------------------------------------------
 */

#pragma once

namespace createdataset
{
  template <typename T, std::size_t N = 16>
  class AlignmentAllocator {
  public:
    typedef T value_type;
    typedef std::size_t size_type;
    typedef std::ptrdiff_t difference_type;

    typedef T * pointer;
    typedef const T * const_pointer;

    typedef T & reference;
    typedef const T & const_reference;

  public:
    inline AlignmentAllocator() throw () {