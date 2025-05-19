// <copyright file="SceneState.cs" company="Google LLC">
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
using AndroidXRUnitySamples.Variables;
using UnityEngine;

namespace AndroidXRUnitySamples.TabletopMess
{
    /// <summary>
    /// Container for scene state data for the planes scene.
    /// </summary>
    [CreateAssetMenu(menuName = "AndroidXRUnitySamples/TabletopMess/SceneState")]
    public class SceneState : ScriptableObject
    {
        /// <summary>
        /// Triggered when new object spawned.
        /// </summary>
        public SpawnNewObjectEvent OnSpawnNewObject;

        /// <summary>
        /// References to the prefabs.
        /// </summary>
        public ObjectPrefabs[] Prefabs;

        /// <summary>
        /// Variable specifying the if a plane is tracked.
        /// </summary>
        public BoolVariable PlaneTracked;

        /// <summary>
        /// Get the prefab for a given type.
        /// </summary>
        /// <param name="type">The type of object to get the prefab for.</param>
        /// <returns>A prefab game object.</returns>
        public GameObject GetObjectPrefab(SpawnNewObjectEvent.ObjectType type)
        {
            foreach (ObjectPrefabs prefab in Prefabs)
            {
                if (prefab.Type == type)
                {
                    return prefab.Prefab;
                }
            }

            Debug.LogError($"Unable to find prefab for object type: {type}");
            return null;
        }

        /// <summary>
        /// Prefabs that can be instantiated.
        /// </summary>
        [Serializable]
        public class ObjectPrefabs
        {
            /// <summary>
            /// Type of the spawned object.
            /// </summary>
            public SpawnNewObjectEvent.ObjectType Type;

            /// <summary>
            /// Prefab to spawn.
            /// </summary>
            public GameObject Prefab;
        }
    }
}
