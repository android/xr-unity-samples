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

        /// <summary>
        /// A decal manager to generate a decal when a collision occurs.
        /// </summary>
        public DecalManager DecalManager;

        private const string _kColorParam = "_BaseColor";
        private ObjectPool<AudioSource> _audioSourcePool;
        private ObjectPool<GameObject> _effectPool;

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

            if (DecalManager != null)
            {
                Vector3 randomUp = Random.insideUnitSphere;
                randomUp.z = 0.0f;

                Quaternion orientation =
                    Quaternion.LookRotation(collision.impulse.normalized, randomUp.normalized);

                DecalManager.GenerateDecal(collision.contacts[0].point, orientation, particleColor);
            }
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
    }
}
