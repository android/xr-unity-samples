// <copyright file="CatapultController.cs" company="Google LLC">
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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace AndroidXRUnitySamples.GazeAndPinch
{
    /// <summary>
    /// Class used for controlling a catapult.
    /// </summary>
    public class CatapultController : MonoBehaviour
    {
        /// <summary>
        /// Game object used as parent for the proectile while still attached to it.
        /// </summary>
        public Transform ProjectileSpawn;

        /// <summary>
        /// Controller used for manging animations.
        /// </summary>
        public Animator Animator;

        /// <summary>
        /// Prefab to use to when spawning a projectile.
        /// </summary>
        public GameObject ProjectilePrefab;

        /// <summary>
        /// Base values used when launching a projectile.
        /// </summary>
        public Vector3 LaunchForce;

        /// <summary>
        /// Force scale used when launching a projectile.
        /// </summary>
        public float LaunchForceScale;

        /// <summary>
        /// Interactable which manages the enablement of the catapult.
        /// </summary>
        public XRBaseInteractable Interactable;

        /// <summary>
        /// Material to use for all renderers when the catapult is deactivated.
        /// </summary>
        public Material DeactivatedMaterial;

        /// <summary>
        /// List of all MeshRenderers used to render the catapult.
        /// </summary>
        public MeshRenderer[] MeshRenderers;

        /// <summary>
        /// Hand action representing the pinch status.
        /// </summary>
        public InputActionProperty PinchAction;

        /// <summary>
        /// Sound effect to play from the animation when a projectile is launched.
        /// </summary>
        public AudioClipData LaunchClip;

        /// <summary>
        /// Sound effect to play from the animation when reloading begins.
        /// </summary>
        public AudioClipData ReloadClip;

        static CatapultController _activeCatapult;

        bool _active = false;
        bool _ready = false;
        ProjectileController _projectile;
        Dictionary<MeshRenderer, Material> _rendererToMaterialMap =
            new Dictionary<MeshRenderer, Material>();

        /// <summary>
        /// Called by the animator when the catapult is ready to have a projectile attached
        /// to it.
        /// </summary>
        public void ProjectileReady()
        {
            _ready = true;

            // Clean up previous projectiles from previous runs.
            foreach (Transform child in ProjectileSpawn.transform)
            {
                Destroy(child.gameObject);
            }

            GameObject projectileObject = Instantiate(ProjectilePrefab, ProjectileSpawn);
            _projectile = projectileObject.GetComponent<ProjectileController>();
            _projectile.Attach();
            SetProjectileActive(_active);
        }

        /// <summary>
        /// Called by the animator when the catapult has begun the fire animation.
        /// </summary>
        public void PlayFireSfx()
        {
            Singleton.Instance.Audio.PlayOneShot(LaunchClip, transform.position);
        }

        /// <summary>
        /// Called by the animator when the catapult has reached the state where the projectile
        /// must be detached.
        /// </summary>
        public void ProjectileDetach()
        {
            Vector3 force = LaunchForce.z * _projectile.transform.forward +
                            LaunchForce.y * _projectile.transform.up +
                            LaunchForce.x * _projectile.transform.right;
            force *= LaunchForceScale;
            _projectile.Detach(force);
        }

        /// <summary>
        /// Called by the animator when the catapult has reached the point in the fire
        /// animation when it begins its reload.
        /// </summary>
        public void PlayReloadSfx()
        {
            Singleton.Instance.Audio.PlayOneShot(ReloadClip, transform.position);
        }

        /// <summary>
        /// Called by the animator when the catapult is done launching and is ready to reset.
        /// </summary>
        public void LaunchDone()
        {
            Animator.SetInteger("State", 0);
        }

        void Awake()
        {
            PopulateMaterialsMap();
        }

        void OnEnable()
        {
            Interactable.firstHoverEntered.AddListener(HandleHover);

            Deactivate();
        }

        void OnDisable()
        {
            Interactable.firstHoverEntered.RemoveListener(HandleHover);
        }

        void Update()
        {
            if (Keyboard.current[Key.A].wasPressedThisFrame)
            {
                if (!_active)
                {
                    Activate();
                }
            }

            if (Keyboard.current[Key.D].wasPressedThisFrame)
            {
                if (_active)
                {
                    Deactivate();
                }
            }

            if (PinchAction.action.WasPressedThisFrame() ||
                Keyboard.current[Key.L].wasPressedThisFrame)
            {
                if (_ready && _active)
                {
                    Launch();
                }
            }
        }

        void HandleHover(HoverEnterEventArgs eventData)
        {
            if (_activeCatapult != null && _activeCatapult != this)
            {
                _activeCatapult.Deactivate();
            }

            _activeCatapult = this;
            Activate();
        }

        void Activate()
        {
            ApplyMaterialsMap();

            if (_projectile != null)
            {
                SetProjectileActive(true);
            }

            _active = true;
        }

        void Deactivate()
        {
            _active = false;

            ApplyDeactivatedMaterial();

            // Only disable projectile if not launching.
            if (_projectile != null && Animator.GetInteger("State") == 0)
            {
                SetProjectileActive(false);
            }
        }

        void ApplyMaterialsMap()
        {
            foreach (MeshRenderer renderer in MeshRenderers)
            {
                renderer.material = _rendererToMaterialMap[renderer];
            }
        }

        void ApplyDeactivatedMaterial()
        {
            foreach (MeshRenderer renderer in MeshRenderers)
            {
                renderer.material = DeactivatedMaterial;
            }
        }

        void PopulateMaterialsMap()
        {
            _rendererToMaterialMap.Clear();

            foreach (MeshRenderer renderer in MeshRenderers)
            {
                _rendererToMaterialMap[renderer] = renderer.material;
            }
        }

        void Launch()
        {
            if (!_ready)
            {
                Debug.LogError("Trying to launch projectile from cataput that is not ready", this);
                return;
            }

            Animator.SetInteger("State", 1);
        }

        void SetProjectileActive(bool enable)
        {
            if (enable)
            {
                _projectile.Activate();
            }
            else
            {
                _projectile.Deactivate();
            }
        }
    }
}
