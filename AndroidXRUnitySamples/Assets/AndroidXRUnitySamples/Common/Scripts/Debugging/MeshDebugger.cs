// <copyright file="MeshDebugger.cs" company="Google LLC">
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
using AndroidXRUnitySamples.Variables;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Show mesh debug info when it is in the debug mode.
    /// </summary>
    public abstract class MeshDebugger : MonoBehaviour
    {
        /// <summary>
        /// A collection of mesh renderers of all currently existing meshes
        /// created by ARMeshManager.
        /// </summary>
        private readonly HashSet<MeshRenderer> _meshRenderers = new HashSet<MeshRenderer>();

        /// <summary>
        /// The settings of whether it is in the debug mode.
        /// </summary>
        [Tooltip("The scriptable object of whether it is in the debug mode.")]
        [SerializeField] private BoolVariable _isDebugMode;

        /// <summary>
        /// Whether the class instance is initialized or not.
        /// </summary>
        private bool _initialized = false;

        /// <summary>
        /// Used to collect new mesh renderers in OnMeshesChanged.
        /// Collect new mesh renderers in order to set them up by the debug
        /// settings as soon as they are collected.
        /// Reuse the list in every call to OnMeshesChanged.
        /// </summary>
        private List<MeshRenderer> _newMeshRenderers = new List<MeshRenderer>();

        /// <summary>
        /// Set up the mesh renderer according to the debug settings.
        /// </summary>
        /// <param name="meshRenderer">A mesh renderer guaranteed to be not null.</param>
        /// <param name="isDebugMode">Whether it is in the debug mode.</param>
        protected abstract void SetUpMeshRenderer(MeshRenderer meshRenderer, bool isDebugMode);

        private void Update()
        {
            // Initialize the class in Update() in order to be called after Start()
            // of all other classes.
            if (!_initialized)
            {
                _initialized = Initialize();
            }
        }

        /// <summary>
        /// Initialize the class instance. The method is called in Update().
        /// Do not put the initialization in Start() because this class requires
        /// the ARMeshManager being enabled before initialization, and the order
        /// of Start() across classes cannot be controlled.
        /// </summary>
        /// <returns>Whether initialization succeeds or not.</returns>
        private bool Initialize()
        {
            // Get the ARMeshManager.
            ARMeshManager meshManager = Singleton.Instance.OriginManager.ARMeshManager;
            if (meshManager == null)
            {
                Debug.LogError($"{nameof(meshManager)} not found");
                return false;
            }

            if (meshManager.subsystem == null)
            {
                Debug.LogError($"{nameof(meshManager)} subsystem not found");
                return false;
            }

            if (!meshManager.subsystem.running)
            {
                Debug.LogError($"{nameof(meshManager)} not running");
                return false;
            }

            // Register a meshes-changed callback.
            meshManager.meshInfosChanged.AddListener(OnMeshesChanged);

            // Register a debug-mode-changed callback.
            _isDebugMode.AddListener(OnDebugModeChanged);

            return true;
        }

        private void OnDestroy()
        {
            if (!_initialized)
            {
                return;
            }

            // Unregister the debug-mode-changed callback.
            _isDebugMode.RemoveListener(OnDebugModeChanged);

            if (Singleton.Instance.OriginManager == null)
            {
                Debug.LogWarning($"{nameof(Singleton.Instance.OriginManager)} not found");
                return;
            }

            // Get the ARMeshManager.
            ARMeshManager meshManager = Singleton.Instance.OriginManager.ARMeshManager;
            if (meshManager == null)
            {
                Debug.LogWarning($"{nameof(meshManager)} not found");
                return;
            }

            // Unregister the meshes-changed callback.
            meshManager.meshInfosChanged.RemoveListener(OnMeshesChanged);
        }

        private void OnMeshesChanged(ARMeshInfosChangedEventArgs eventArgs)
        {
            // Maintain a collection that stores all existing mesh renderers
            // of the meshes created by the ARMeshManager.

            // Prepare to collect any new mesh renderers.
            _newMeshRenderers.Clear();

            // Add mesh renderers of added meshes to the list.
            foreach (MeshUpdateInfo meshUpdateInfo in eventArgs.added)
            {
                var meshRenderer = meshUpdateInfo.meshFilter.GetComponent<MeshRenderer>();
                if (_meshRenderers.Add(meshRenderer))
                {
                    _newMeshRenderers.Add(meshRenderer);
                }
            }

            // Add mesh renderers of updated meshes to the list.
            foreach (MeshUpdateInfo meshUpdateInfo in eventArgs.updated)
            {
                var meshRenderer = meshUpdateInfo.meshFilter.GetComponent<MeshRenderer>();
                if (_meshRenderers.Add(meshUpdateInfo.meshFilter.GetComponent<MeshRenderer>()))
                {
                    _newMeshRenderers.Add(meshRenderer);
                }
            }

            // Remove mesh renderers from the list.
            foreach (MeshUpdateInfo meshUpdateInfo in eventArgs.removed)
            {
                _meshRenderers.Remove(meshUpdateInfo.meshFilter.GetComponent<MeshRenderer>());
            }

            // Set up the newly collected mesh renderers according to the debug
            // settings.
            if (_newMeshRenderers.Count > 0)
            {
                SetUpMeshRenderers(_newMeshRenderers, _isDebugMode.Value);
            }
        }

        private void OnDebugModeChanged(bool isDebugMode)
        {
            // Set up all mesh renderers according to the debug settings.
            SetUpMeshRenderers(_meshRenderers, isDebugMode);
        }

        private void SetUpMeshRenderers(IEnumerable<MeshRenderer> meshRenderers, bool isDebugMode)
        {
            foreach (var meshRenderer in meshRenderers)
            {
                if (meshRenderer == null)
                {
                    Debug.LogError("meshRenderer is null");
                    continue;
                }

                SetUpMeshRenderer(meshRenderer, isDebugMode);
            }
        }
    }
}
