// <copyright file="DestructibleSettings.cs" company="Google LLC">
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

using UnityEngine;

namespace AndroidXRUnitySamples.GazeAndPinch
{
    /// <summary>
    /// Class used to represent a destructible object settings.
    /// </summary>
    [CreateAssetMenu(menuName = "AndroidXRUnitySamples/GazeAndPinch/DestructibleSettings")]
    public class DestructibleSettings : ScriptableObject
    {
        /// <summary>
        /// Starting health of the object. When it hits 0, the object is dead.
        /// </summary>
        public float Health;

        /// <summary>
        /// Threshold under which the object is considered damaged.
        /// </summary>
        public float DamagedThreshold;

        /// <summary>
        /// Threshold under which the object is considered severely damaged.
        /// </summary>
        public float SeverelyDamagedThreshold;

        /// <summary>
        /// Material to use for the object when it has full health.
        /// </summary>
        public Material FullHealthMaterial;

        /// <summary>
        /// Material to use for the object when it is damaged.
        /// </summary>
        public Material DamagedHealthMaterial;

        /// <summary>
        /// Material to use for the object when it is severely damaged.
        /// </summary>
        public Material SeverelyDamagedHealthMaterial;

        /// <summary>
        /// Specifies if the given health is considered severely damaged.
        /// </summary>
        /// <param name="health">The current health.</param>
        /// <returns>Returns true if the object is considered severely damaged.</returns>
        public bool IsSeverelyDamaged(float health)
        {
            return (health / Health) <= SeverelyDamagedThreshold;
        }

        /// <summary>
        /// Specifies if the given health is considered damaged.
        /// </summary>
        /// <param name="health">The current health.</param>
        /// <returns>Returns true if the object is considered damaged.</returns>
        public bool IsDamaged(float health)
        {
            return (health / Health) <= DamagedThreshold;
        }
    }
}
