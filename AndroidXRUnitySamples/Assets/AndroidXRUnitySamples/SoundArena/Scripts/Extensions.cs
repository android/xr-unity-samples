// <copyright file="Extensions.cs" company="Google LLC">
//
// Copyright 2025 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
// ----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AndroidXRUnitySamples.SoundArena
{
    /// <summary>
    /// Class extensions local to the SoundDirectivity namespace.
    /// </summary>
    public static class Extensions
    {
    }

    /// <summary>
    /// Extension methods for <see cref="List{T}" />.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Returns a random index from the list.
        /// </summary>
        /// <param name="list">The list to pick an index from.</param>
        /// <typeparam name="T">Type of the list elements.</typeparam>
        /// <returns>A random index within the supplied list's bounds.</returns>
        public static int RandomIndex<T>(this IList<T> list)
        {
            if (list.Count <= 0)
            {
                throw new IndexOutOfRangeException();
            }

            return Random.Range(0, list.Count);
        }

        /// <summary>
        /// Retrieves a random element from the list.
        /// </summary>
        /// <param name="list">The list to pick an index from.</param>
        /// <typeparam name="T">Type of the list elements.</typeparam>
        /// <returns>A random element from the supplied list.</returns>
        public static T RandomElement<T>(this IList<T> list)
        {
            return list.Count > 0 ? list[list.RandomIndex()] : default;
        }
    }

    /// <summary>
    /// Extension methods for <see cref="Color" />.
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Determines if the color is approximately equal to another color within a specified
        /// tolerance.
        /// </summary>
        /// <param name="me">Color #1.</param>
        /// <param name="other">Color #2.</param>
        /// <param name="tolerance">
        /// Tolerance within which the two colors
        /// are considered equal.
        /// </param>
        /// <returns>
        /// Whether or not the two colors can be considered
        /// equal within the given tolerance.
        /// </returns>
        public static bool IsEqualTo(this Color me, Color other, float tolerance)
        {
            return Mathf.Abs(me.r - other.r) <= tolerance &&
                   Mathf.Abs(me.g - other.g) <= tolerance &&
                   Mathf.Abs(me.b - other.b) <= tolerance && Mathf.Abs(me.a - other.a) <= tolerance;
        }
    }
}
