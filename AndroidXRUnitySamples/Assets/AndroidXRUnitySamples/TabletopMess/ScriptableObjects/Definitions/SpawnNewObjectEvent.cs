// <copyright file="SpawnNewObjectEvent.cs" company="Google LLC">
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

using AndroidXRUnitySamples.Events.Generic;
using UnityEngine;

namespace AndroidXRUnitySamples.TabletopMess
{
    /// <summary>
    /// <inheritdoc cref="Event{T}" />
    /// </summary>
    [CreateAssetMenu(menuName = "AndroidXRUnitySamples/TabletopMess/SpawnNewObjectEvent")]
    public class SpawnNewObjectEvent : Event<SpawnNewObjectEvent.Info>
    {
        /// <summary>
        /// Type of object.
        /// </summary>
        public enum ObjectType
        {
            /// <summary>
            /// Sphere shape.
            /// </summary>
            Sphere,

            /// <summary>
            /// Capsule shape.
            /// </summary>
            Capsule,

            /// <summary>
            /// Cube shape.
            /// </summary>
            Cube
        }

        /// <summary>
        /// Spawn info for the object.
        /// </summary>
        public struct Info
        {
            /// <summary>
            /// Type of the object.
            /// </summary>
            public ObjectType Type;

            /// <summary>
            /// Origin position.
            /// </summary>
            public Vector3 Origin;
        }
    }
}
