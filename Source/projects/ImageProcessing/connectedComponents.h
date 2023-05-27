
/*  ------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
 *  ------------------------------------------------------------------------------------------
 */

#pragma once

#include <vector>
#include <map>
#include <limits>
#include <iostream>

namespace createdataset
{
// A set that supports the unite-find algorithm
template<typename U>
class Set {
public:
  static const U UNITIALIZED = U(0);

private:
  typedef unsigned short RANK_TYPE;

  RANK_TYPE rank_;

public:
  // If there are many connected components, allowing the labelling code to track the label
  // belonging to the Set here rather than in e.g. a std::map<Set,U> provides a big speedup,
  // especially if the type of U won't increase the size of this struct beyond its (padded)
  // 16 byte size.
  bool hasLabel_;
  U label_;

private:
  Set* parent_;

public:
  Set(): rank_(0), parent_(0), label_(UNITIALIZED), hasLabel_(false)
  {
  }

  inline static void unite(Set* x, Set* y)
  {
    Set* xRoot = x->find();
    Set* yRoot = y->find();

    if(xRoot->rank_ > yRoot->rank_)
      yRoot->parent_ = xRoot;
    else if(xRoot->rank_ < yRoot->rank_)
      xRoot->parent_ = yRoot;
    else if(xRoot != yRoot) // Unless x and y are already in same set, merge them
    {
      yRoot->parent_ = xRoot;
      if (xRoot->rank_ == std::numeric_limits<RANK_TYPE>::max()) // annoyingly slow, but better than silent error
        throw std::exception("Connected components graph overflow.");
      xRoot->rank_++;
    }
  }

  inline void unite(Set* x)
  {
    Set::unite(this, x);
  }

  Set* find()
  {
    if(parent_ == 0)
      return this;
    else
    {
      parent_ = parent_->find(); // a trick to keep the tree flat
      return parent_;
    }
  }

  void print()
  {
    std::cout << this << std::endl;
  }
};

void test_Set();

// Template print function for pixel types that support 'put to' std::ostream.
// Intended only for displaying simple test images to the console during
// development and debugging.
template<typename T>
void printImage(int width, int height, void* buffer, int stride)
{
  for(int v=0; v<height; v++)
  {
    T* p = (T*)((unsigned char*)(buffer) + v*stride);
    for(int u=0; u<width; u++)
    {
      cout << p[0];
      if(u!=width-1)
        cout << "\t";
      p++;
    }
    cout << endl;
  }
}

template<>
void printImage<unsigned char>(int width, int height, void* buffer, int stride);

template<typename T, typename U>
struct ComponentStatistics
{
  unsigned long pixelCount_;
  T inputLabel_;
};

// Find connected components in 3D volume using one pass unite-find approach and
// label associated voxels in output volume. Voxels with the specified background colour
// are all assigned the background label.
// Returns a vector of statistics per connected component.
template<typename T, typename U>
std::vector<ComponentStatistics<T, U> > findConnectedComponents3d(
  int width,
  int height,
  int depth,
  void* inputBuffer,  // of type T
  int inputLeap,
  int inputStride,
  T backgroundColor, // TODO: Allow for no background color with a seperate function?
  void* outputBuffer, // of type U
  int outputLeap,
  int outputStride,
  U backgroundLabel)
{
  // This version faster, apparently because compiler doesn't realize that Set constructor
  // is just zeroing memory.
  std::vector<unsigned char> components_(width*height*depth*sizeof(Set<U>), 0);
  Set<U>* components = (Set<U>*)(&components_[0]);

  //std::vector<Set<U>> components_(width*height*depth);
  //Set<U>* components = &components_[0];

  // Vertex/edge/face connectivity version
  //const int N = 13;
  //const int du[] = { -1,  0,  1, -1,  0,  1, -1,  0,  1, -1, -1,  0,  1 };
  //const int dv[] = { -1, -1, -1,  0,  0,  0,  1,  1,  1,  0, -1, -1, -1 };
  //const int dw[] = { -1, -1, -1, -1, -1, -1, -1, -1, -1,  0,  0,  0,  0 };

  // Face connectivity
  const int N = 3; // back, left, up
  const int du[] = { 0, -1,  0 };
  const int dv[] = { 0,  0, -1 };
  const int dw[] = { -1,  0,  0 };

  for (int w = 0; w < depth; w++)
  {
    for (int v = 0; v < height; v++)
    {
      T* p = (T*)((unsigned char*)inputBuffer + w*inputLeap + v*inputStride);
      auto s = &components[w*width*height + v*width];