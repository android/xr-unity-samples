// <copyright file="ProjectileController.cs" company="Google LLC">
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
    /// Class used for controlling a projectile launched from a catapult.
    /// </summary>
    public class ProjectileController : MonoBehaviour
    {
        /// <summary>
        /// The Rigidbody component on the projectile.
        /// </summary>
        public Rigidbody Rigidbody;

        /// <summary>
        /// Particle systems used for effects.
        /// </summary>
        public ParticleSystem[] ParticleSystems;

        /// <summary>
        /// Trail renderer used for effects.
        /// </summary>
        public GameObject Trail;

        /// <summary>
        /// MeshRenderer of the mesh.
        /// </summary>
        public MeshRenderer MeshRenderer;

        /// <summary>
        /// IdleCleanup component.
        /// </summary>
        public IdleCleanup IdleCleanup;

        /// <summary>
        /// Material to use for all renderers when the projectile is deactivated.
        /// </summary>
        public Material DeactivatedMaterial;

        Material _activeMaterial;

        /// <summary>
        /// Should be called when the projectile is attached to the catapult.
        /// </summary>
        public void Attach()
        {
            Rigidbody.isKinematic = true;
            Trail.SetActive(false);
        }

        /// <summary>
        /// Should be called when the projectile is detached to the catapult and should start
        /// running physics.
        /// </summary>
        /// <param name="force">The force to apply as the projectile is being detched.</param>
        public void Detach(Vector3 force)
        {
            Rigidbody.isKinematic = false;
            transform.SetParent(null);
            Rigidbody.AddForce(force);
            IdleCleanup.Activate();
            Trail.SetActive(true);
        }

        /// <summary>
        /// Called when the projectile is active.
        /// </summary>
        public void Activate()
        {
            if (_activeMaterial != null)
            {
                MeshRenderer.material = _activeMaterial;
            }

            for (int i = 0; i < ParticleSystems.Length; ++i)
            {
                ParticleSystems[i].Play();
            }
        }

        /// <summary>
        /// Called when the projectile is inactive.
        /// </summary>
        public void Deactivate()
        {
            _activeMaterial = MeshRenderer.material;
            MeshRenderer.material = DeactivatedMaterial;
            Trail.SetActive(false);
            for (int i = 0; i < ParticleSystems.Length; ++i)
            {
                ParticleSystems[i].Stop();
            }
        }
    }
}
