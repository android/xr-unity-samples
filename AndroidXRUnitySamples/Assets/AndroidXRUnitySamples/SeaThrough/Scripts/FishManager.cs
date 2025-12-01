// <copyright file="FishManager.cs" company="Google LLC">
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
    using System.Collections.Generic;
    using Unity.XR.CoreUtils;
    using UnityEngine;

    /// <summary>
    /// The fish manager will be responsible for spawning in fish near the camera and despawning
    /// fish when they reach too far away from the camera.
    /// </summary>
    public class FishManager : MonoBehaviour
    {
        /// <summary>
        /// Enable spawning fish.
        /// </summary>
        public bool SpawnFish = false;

        /// <summary>
        /// List of fish presets to generate from.
        /// </summary>
        [SerializeField] private List<FishPreset> _fishPresets;

        /// <summary>
        /// Fish generator.
        /// </summary>
        [SerializeField] private FishGenerator _fishGenerator;

        /// <summary>
        /// Minimum distance to spawn fish.
        /// </summary>
        [SerializeField] private float _spawnMinDistance = 4.0f;

        /// <summary>
        /// Maximum distance to spawn fish.
        /// </summary>
        [SerializeField] private float _spawnMaxDistance = 10.0f;

        [SerializeField] private float _spawnPositionYMultiplier = 0.1f;

        /// <summary>
        /// Distance to despawn fish.
        /// </summary>
        [SerializeField] private float _despawnDistance = 15.0f;

        /// <summary>
        /// Ideal number of fish to spawn.
        /// </summary>
        [SerializeField] private int _idealNumberOfFish = 55;

        /// <summary>
        /// Maximum fish pitch in degrees.
        /// </summary>
        [SerializeField] private float _maxFishPitchDegrees = 10.0f;

        /// <summary>
        /// Minimum fish speed in meters per second.
        /// </summary>
        [SerializeField] private float _minFishSpeed = 0.1f;

        /// <summary>
        /// Maximum fish speed in meters per second.
        /// </summary>
        [SerializeField] private float _maxFishSpeed = 0.3f;

        /// <summary>
        /// Left hand game object.
        /// </summary>
        [SerializeField] private GameObject _leftHand;

        /// <summary>
        /// Right hand game object.
        /// </summary>
        [SerializeField] private GameObject _rightHand;

        private GameObject _fishCache;
        private MaterialPropertyBlock _materialPropertyBlock;

        private void Awake()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }

        private void Start()
        {
            // Cache an instance of each fish mesh.
            _fishCache = new GameObject("FishCache");
            foreach (FishPreset fishPreset in _fishPresets)
            {
                _fishGenerator.Preset = fishPreset;
                _fishGenerator.LoadFromPreset();
                GameObject fishObject = _fishGenerator.GenerateFish();
                fishObject.transform.SetParent(_fishCache.transform, false);
                fishObject.SetActive(false);
            }
        }

        private void Update()
        {
            foreach (Transform child in transform)
            {
                float distance = Vector3.Distance(
                    child.position, Camera.main.transform.position);
                if (distance > _despawnDistance)
                {
                    child.SetLocalPose(GenerateRandomFishPose());
                }
            }

            // Spawn fish if there are not enough fish.
            while (SpawnFish && transform.childCount < _idealNumberOfFish)
            {
                // Copy a random fish prefab.
                int randomIndex = Random.Range(0, _fishCache.transform.childCount);
                GameObject fishObject = CloneFishFromCache(randomIndex);
                fishObject.SetActive(true);
                fishObject.transform.SetLocalPose(GenerateRandomFishPose());

                SphereCollider collider = fishObject.AddComponent<SphereCollider>();
                collider.radius = 0.05f;
                collider.isTrigger = true;

                Rigidbody rigidbody = fishObject.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = false;

                SwimmingFish fish = fishObject.AddComponent<SwimmingFish>();
                fish.SwimmingSpeed = Random.Range(_minFishSpeed, _maxFishSpeed);
                fish.LeftHand = _leftHand;
                fish.RightHand = _rightHand;
            }
        }

        private GameObject CloneFishFromCache(int index)
        {
            Transform originalFish = _fishCache.transform.GetChild(index);
            GameObject clone = Instantiate(originalFish, transform, false).gameObject;
            clone.SetActive(true);
            RecursivelyCopyMaterialPropertyBlocks(originalFish, clone.transform);
            return clone;
        }

        private void RecursivelyCopyMaterialPropertyBlocks(Transform transform, Transform clone)
        {
            if (transform.TryGetComponent<MeshRenderer>(out var renderer) &&
                clone.TryGetComponent<MeshRenderer>(out var cloneRenderer))
            {
                _materialPropertyBlock.Clear();
                renderer.GetPropertyBlock(_materialPropertyBlock);
                cloneRenderer.SetPropertyBlock(_materialPropertyBlock);
            }

            for (int i = 0; i < transform.childCount && i < clone.childCount; i++)
            {
                RecursivelyCopyMaterialPropertyBlocks(
                    transform.GetChild(i), clone.GetChild(i));
            }
        }

        private Pose GenerateRandomFishPose()
        {
            Vector3 spawnPosition = Random.onUnitSphere *
                Random.Range(_spawnMinDistance, _spawnMaxDistance);
            spawnPosition.y *= _spawnPositionYMultiplier;
            spawnPosition = Camera.main.transform.position + spawnPosition;

            Quaternion randomYaw = Quaternion.Euler(0, Random.Range(0, 360), 0);
            Quaternion randomPitch = Quaternion.Euler(
                0, 0, Random.Range(-_maxFishPitchDegrees, _maxFishPitchDegrees));
            return new Pose(spawnPosition, randomYaw * randomPitch);
        }
    }
}
