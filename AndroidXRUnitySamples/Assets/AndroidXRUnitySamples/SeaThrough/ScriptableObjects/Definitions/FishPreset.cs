// <copyright file="FishPreset.cs" company="Google LLC">
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
namespace AndroidXRUnitySamples.SeaThrough
{
    using UnityEngine;

    /// <summary>
    /// A preset for procedurally generating fish.
    /// </summary>
    [CreateAssetMenu(
        fileName = "New Fish Preset",
        menuName = "AndroidXRUnitySamples/SeaThrough/Fish Preset",
        order = 1)]
    public class FishPreset : ScriptableObject
    {
        /// <summary>
        /// Color of the fish body.
        /// </summary>
        [Header("Appearance")]
        public Color BodyColor = Color.white;

        /// <summary>
        /// Pattern on the fish.
        /// </summary>
        public PatternType Pattern = PatternType.None;

        /// <summary>
        /// Color of the fish pattern.
        /// </summary>
        [Tooltip("This color is only used if the pattern is not 'None'.")]
        public Color PatternColor = Color.black;

        [Header("Body Shape")]

        /// <summary>
        /// Length of the fish body.
        /// </summary>
        [Range(5f, 15f)] public float BodyLength = 10f;

        /// <summary>
        /// Height of the fish body.
        /// </summary>
        [Range(1f, 5f)] public float BodyHeight = 3f;

        [Header("Fins")]

        /// <summary>
        /// Size of the fish tail.
        /// </summary>
        [Range(1f, 5f)] public float TailSize = 2.2f;

        /// <summary>
        /// Size of the fish dorsal fin.
        /// </summary>
        [Range(1f, 5f)] public float DorsalFinSize = 3f;

        /// <summary>
        /// Pattern on the fish.
        /// </summary>
        public enum PatternType
        {
            /// <summary>
            /// No pattern.
            /// </summary>
            None,

            /// <summary>
            /// Stripes on the fish.
            /// </summary>
            Stripes,

            /// <summary>
            /// Spots on the fish.
            /// </summary>
            Spots
        }
    }
}
