// <copyright file="GroundSegmentController.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.GestureRun
{
    /// <summary>
    /// Class used to control scrolling ground segments which hold obstacles.
    /// </summary>
    public class GroundSegmentController : MonoBehaviour
    {
        /// <summary>
        /// Controls the spawn rate each time the segment is reset.
        /// </summary>
        [Range(0.001f, 1f)]
        public float SpawnProbability = 0.5f;

        /// <summary>
        /// The relative offset for a segment to start at.
        /// </summary>
        public float StartOffset;

        /// <summary>
        /// The relative offset for a segment to reset at.
        /// </summary>
        public float EndOffset;

        /// <summary>
        /// The relative offset for a segment to enable the colliders of the obstacles.
        /// </summary>
        public float ActivationOffset;

        /// <summary>
        /// The scrolling speed of the segment. Should match that of the scrolling bar.
        /// </summary>
        public float Speed;

        /// <summary>
        /// Starting offset of this specific segment.
        /// </summary>
        public float StartTimer;

        /// <summary>
        /// The GameObject used as the parent of the obstacles.
        /// </summary>
        public GameObject ObstacleContainer;

        /// <summary>
        /// List of obstacles to spawn. One is randomly selected each time.
        /// </summary>
        public GameObject[] ObstaclePrefabs;

        float _timer = 0;

        void Start()
        {
            _timer = StartTimer;
        }

        void Update()
        {
            _timer += Time.deltaTime;
            UpdatePosition();

            if (HasPassedActivationPoint())
            {
                MarkObstaclesKinematic(false);
            }

            if (HasPassedEndPoint())
            {
                Reset();
            }
        }

        void Reset()
        {
            _timer = 0;

            // Clean up obstacles.
            foreach (Transform child in ObstacleContainer.transform)
            {
                Destroy(child.gameObject);
            }

            // Check if the segment should have an obstacle.
            if (Random.Range(0f, 1f) > SpawnProbability)
            {
                return;
            }

            GameObject prefab = ObstaclePrefabs[Random.Range(0, ObstaclePrefabs.Length)];
            Instantiate(prefab, ObstacleContainer.transform);

            // The segment is scrolling at a constant speed. This causes stacked physics objects
            // to slowly drift over time and fall off each other before getting to the marble.
            // To avoid this, we mark the colliders as kinematic until they get close enough to
            // the marble.
            MarkObstaclesKinematic(true);
        }

        void UpdatePosition()
        {
            float offset = StartOffset + _timer * Speed;
            transform.localPosition = new Vector3(0, 0, offset);
        }

        bool HasPassedEndPoint()
        {
            return StartOffset + _timer * Speed >= EndOffset;
        }

        bool HasPassedActivationPoint()
        {
            return ActivationOffset + _timer * Speed >= EndOffset;
        }

        void MarkObstaclesKinematic(bool isKinematic)
        {
            Rigidbody[] rigidbodies = ObstacleContainer.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rigidbody in rigidbodies)
            {
                rigidbody.isKinematic = isKinematic;
            }
        }
    }
}
