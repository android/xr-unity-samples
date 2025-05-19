// <copyright file="CoinController.cs" company="Google LLC">
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
    /// Class used to manage coin collisions with the marble.
    /// </summary>
    public class CoinController : MonoBehaviour
    {
        /// <summary>
        /// Animator component of the coin.
        /// </summary>
        public Animator Animator;

        /// <summary>
        /// ParticleSystem component of the coin.
        /// </summary>
        public ParticleSystem ParticleSystem;

        /// <summary>
        /// Sound effect to play when collected.
        /// </summary>
        public AudioClipData CollectClip;

        /// <summary>
        /// Hook for the collect animation to call back into.
        /// </summary>
        public void PlayParticles()
        {
            ParticleSystem.Play();
        }

        void OnTriggerEnter(Collider other)
        {
            // Filter non-marble collisions.
            if (other.tag != "Player")
            {
                return;
            }

            Singleton.Instance.Audio.PlayOneShot(CollectClip, transform.position);
            Animator.SetTrigger("Collect");
        }
    }
}
