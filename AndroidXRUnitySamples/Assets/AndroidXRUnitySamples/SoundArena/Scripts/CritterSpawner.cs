// <copyright file="CritterSpawner.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.SoundArena
{
    /// <summary>
    /// Spawns the critter in a random position taken from a list.
    /// </summary>
    public class CritterSpawner : MonoBehaviour
    {
        [SerializeField] private float _spacing;

        private Vector3[] _positions;

        /// <summary>
        /// Returns a random position.
        /// </summary>
        /// <returns>The random position.</returns>
        public Vector3 GetRandomSpawnPosition()
        {
            return _positions[Random.Range(0, _positions.Length)];
        }

        /// <summary>
        /// Positions the critter, then activates it.
        /// </summary>
        public void Spawn()
        {
        }

        /// <summary>
        /// Deactivates the active critter.
        /// </summary>
        public void Despawn()
        {
        }

        private void Awake()
        {
            _positions = new Vector3[(3 * 3 * 3) - 3];
            int index = 0;
            for (int x = 0; x < 3; ++x)
            {
                for (int y = 0; y < 3; ++y)
                {
                    for (int z = 0; z < 3; ++z)
                    {
                        if (x == 1 && z == 1)
                        {
                            continue;
                        }

                        Vector3 pos = ComputePos(x, y, z);
                        _positions[index++] = pos;
                    }
                }
            }
        }

        private Vector3 ComputePos(int x, int y, int z)
        {
            Vector3 offset = new Vector3(x * _spacing, y * _spacing, z * _spacing);
            Vector3 basePos = new Vector3(-_spacing, 0.0f, -_spacing);
            return basePos + offset;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            for (int x = 0; x < 3; ++x)
            {
                for (int y = 0; y < 3; ++y)
                {
                    for (int z = 0; z < 3; ++z)
                    {
                        if (x == 1 && z == 1)
                        {
                            continue;
                        }

                        Vector3 pos = ComputePos(x, y, z);
                        Gizmos.DrawSphere(pos, 0.1f);
                    }
                }
            }
        }
    }
}
