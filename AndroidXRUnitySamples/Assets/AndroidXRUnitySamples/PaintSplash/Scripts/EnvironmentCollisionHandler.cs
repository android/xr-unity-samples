// <copyright file="EnvironmentCollisionHandler.cs" company="Google LLC">
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace AndroidXRUnitySamples.PaintSplash
{
    /// <summary>
    /// Triggers sound and instantiates a prefab when a collision occurs.
    /// </summary>
    [RequireComponent(typeof(MeshCollider))]
    [RequireComponent(typeof(MeshFilter))]
    public class EnvironmentCollisionHandler : MonoBehaviour
    {
        /// <summary>
        /// The sound to play when a collision occurs.
        /// </summary>
        [Space]
        public AudioClip CollisionSound;

        /// <summary>
        /// Minimum pitch variation for the sound.
        /// </summary>
        public float MinPitch = 0.9f;

        /// <summary>
        /// Maximum pitch variation for the sound.
        /// </summary>
        public float MaxPitch = 1.1f;

        /// <summary>
        /// A prefab to instantiate at the point of impact when a collision occurs.
        /// </summary>
        [Space]
        public GameObject ImpactEffectPrefab;

        private const string _kColorParam = "_BaseColor";
        private ObjectPool<AudioSource> _audioSourcePool;
        private ObjectPool<GameObject> _effectPool;

        [Space]

        [SerializeField] private GameObject _decalPrefab;
        [SerializeField] private float _decalScale = 0.3f;

        /// <summary>
        /// The facing direction of the previously created decal.
        /// Use the previous facing direction in case the calculated decal
        /// facing direction at the current collision is a zero vector.
        /// </summary>
        private Vector3 _previousDecalFacingDirection = Vector3.up;

        private void Start()
        {
            _audioSourcePool = new ObjectPool<AudioSource>(
                    createFunc: () =>
                    {
                        var audioSourceGO = new GameObject("PooledAudioSource");
                        var audioSource = audioSourceGO.AddComponent<AudioSource>();
                        audioSource.loop = false;
                        return audioSource;
                    },
                    actionOnGet: audioSource => audioSource.gameObject.SetActive(true),
                    actionOnRelease: audioSource => audioSource.gameObject.SetActive(false),
                    actionOnDestroy: audioSource =>
                    {
                        if (audioSource != null)
                        {
                            Destroy(audioSource.gameObject);
                        }
                    });

            _effectPool = new ObjectPool<GameObject>(
                    createFunc: () => Instantiate(ImpactEffectPrefab),
                    actionOnGet: effect => effect.SetActive(true),
                    actionOnRelease: effect => effect.SetActive(false),
                    actionOnDestroy: Destroy);
        }

        private void OnDestroy()
        {
            _audioSourcePool.Clear();
            _effectPool.Clear();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (CollisionSound != null)
            {
                AudioSource audioSource = _audioSourcePool.Get();
                audioSource.transform.position = collision.contacts[0].point;
                audioSource.pitch = Random.Range(MinPitch, MaxPitch);
                audioSource.PlayOneShot(CollisionSound);
                StartCoroutine(ReleaseAudioSourceAfterPlaying(audioSource));
            }

            Color particleColor = Color.black;
            ContactPoint contact = collision.contacts[0];
            MeshRenderer particleMesh =
                contact.otherCollider.GetComponentInChildren<MeshRenderer>();
            if (particleMesh != null)
            {
                particleColor = particleMesh.sharedMaterial.GetColor(_kColorParam);
            }

            if (ImpactEffectPrefab != null)
            {
                GameObject effectInstance = _effectPool.Get();
                effectInstance.transform.position = contact.point;
                effectInstance.transform.rotation = Quaternion.LookRotation(contact.normal);

                ParticleSystem[] particles =
                    effectInstance.GetComponentsInChildren<ParticleSystem>();
                Assert.IsNotNull(particles, "Impact effect prefab must have a Particle System.");

                // Copy the color from the projectile to the splash effect.
                for (int i = 0; i < particles.Length; ++i)
                {
                    if (particles[i].TryGetComponent<ParticleSystemRenderer>(out var r))
                    {
                        r.material.SetColor(_kColorParam, particleColor);
                    }
                }

                StartCoroutine(ReleaseEffectAfterLifetime(particles[0]));
            }

            // Create a decal game object at the collision point.
            GenerateDecal(collision.contacts[0].point, CalculateDecalOrientation(collision),
                          particleColor);
        }

        private IEnumerator ReleaseAudioSourceAfterPlaying(AudioSource audioSource)
        {
            yield return new WaitWhile(() => audioSource.isPlaying);
            _audioSourcePool.Release(audioSource);
        }

        private IEnumerator ReleaseEffectAfterLifetime(ParticleSystem effect)
        {
            yield return new WaitWhile(() => effect.isPlaying);
            _effectPool.Release(effect.transform.parent.gameObject);
        }

        /// <summary>
        /// Calculate the orientation of a decal to be created at collision.
        /// </summary>
        /// <param name="collision">The collision info.</param>
        /// <returns>The orientation of a decal to be created at the collision point.</returns>
        private Quaternion CalculateDecalOrientation(Collision collision)
        {
            // Randomly decide the upward direction for LookRotation().
            Vector3 randomUp = Random.insideUnitSphere.normalized;

            // Use the collision impulse direction as the forward direction
            // for LookRotation(). It will be the direction where the decal
            // will be facing.
            Vector3 decalFacingDirection = collision.impulse.normalized;
            if (decalFacingDirection == Vector3.zero)
            {
                // The impulse vector can be zero if colliding at a boundary of the mesh.
                // Use the previously stored non-zero value instead as a workaround.
                decalFacingDirection = _previousDecalFacingDirection;
            }
            else
            {
                // Store the current non-zero value of the facing direction.
                _previousDecalFacingDirection = decalFacingDirection;
            }

            // Return the decal orientation.
            return Quaternion.LookRotation(decalFacingDirection, randomUp);
        }

        /// <summary>
        /// Generates a decal the specified position and orientation onto the depth mesh.
        /// </summary>
        /// <param name="position">Position to place the decal.</param>
        /// <param name="orientation">Orientation to place the decal.</param>
        /// <param name="color">Color of the decal.</param>
        private void GenerateDecal(Vector3 position, Quaternion orientation, Color color)
        {
            // A projectile can hit near to mesh borders. Need to collect and
            // combine nearby meshes first, and then generate a decal mesh from
            // the combined mesh.

            // Collect nearby meshes.
            // About the radius of the overlap sphere:
            // DecalMesh.CreateFromMesh() uses a unit bound scaled by the input
            // scale to filter triangles. Thus here use a unit shpere scaled by
            // the same scale to cover the whole filtering bound.
            Collider[] hitColliders = Physics.OverlapSphere(position, _decalScale);
            List<MeshFilter> hitMeshFilters = new List<MeshFilter>();
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent<MeshFilter>(out var hitMeshFilter))
                {
                    hitMeshFilters.Add(hitMeshFilter);
                }
            }

            // Prepare to combine the collected meshes.
            CombineInstance[] combineInstances = new CombineInstance[hitMeshFilters.Count];
            for (int i = 0; i < combineInstances.Length; ++i)
            {
                combineInstances[i].mesh = hitMeshFilters[i].mesh;
                combineInstances[i].transform = hitMeshFilters[i].transform.localToWorldMatrix;
            }

            // Combine the meshes.
            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combineInstances, true, true);

            // Instantiate a decal prefab, and create the decal mesh from the
            // combined mesh.
            GameObject decal = Instantiate(_decalPrefab, transform.root);
            var decalMesh = decal.GetComponent<DecalMesh>();
            decalMesh.CreateFromMesh(combinedMesh, Matrix4x4.identity, position, orientation,
                                     _decalScale * Vector3.one);
            decalMesh.GetComponent<MeshRenderer>().material.SetColor(_kColorParam, color);

            // Destroy the procedurally generated combined mesh.
            Destroy(combinedMesh);
        }
    }
}
