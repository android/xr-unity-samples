// <copyright file="Plane.cs" company="Google LLC">
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

using AndroidXRUnitySamples.Variables;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace AndroidXRUnitySamples.GazeAndPinch
{
    /// <summary>
    /// A XR tracked plane.
    /// </summary>
    public class Plane : MonoBehaviour
    {
        /// <summary>
        /// SceneState indicating the state of the scene.
        /// </summary>
        public SceneState SceneState;

        /// <summary>
        /// Mesh renderer for the plane.
        /// </summary>
        public MeshRenderer MeshRenderer;

        /// <summary>
        /// The LineRenderer component of the plan.
        /// </summary>
        public LineRenderer LineRenderer;

        /// <summary>
        /// Target interactable.
        /// </summary>
        public XRBaseInteractable Interactable;

        /// <summary>
        /// Material used to render the plane when it's selectable.
        /// </summary>
        public Material SelectableMaterial;

        /// <summary>
        /// Material used to render the plane when it's not selectable.
        /// </summary>
        public Material NonSelectableMaterial;

        /// <summary>
        /// Material when hovered.
        /// </summary>
        public Material HoverMaterial;

        /// <summary>
        /// Material used to render the plane lines when it's selectable.
        /// </summary>
        public Material SelectableLineMaterial;

        /// <summary>
        /// Material used to render the plane lines when it's not selectable.
        /// </summary>
        public Material NonSelectableLineMaterial;

        /// <summary>
        /// Reference to the debug mode setting.
        /// </summary>
        public BoolVariable DebugModeSetting;

        /// <summary>
        /// Hand action representing the pinch status.
        /// </summary>
        public InputActionProperty PinchAction;

        /// <summary>
        /// The ARPlane component of the plane.
        /// </summary>
        public ARPlane ARPlane;

        /// <summary>
        /// The Collider component of the plane.
        /// </summary>
        public Collider Collider;

        /// <summary>
        /// Sound effect to play when the plane is selected.
        /// </summary>
        public AudioClipData SelectionClip;

        bool _interactionEnabled = true;

        /// <summary>
        /// Gets or sets a value indicating whether the plane can be interacted
        /// via gaze or not.
        /// </summary>
        public bool EnableInteraction
        {
            get
            {
                return _interactionEnabled;
            }

            set
            {
                _interactionEnabled = value;
                Collider.enabled = _interactionEnabled;
                Interactable.enabled = _interactionEnabled;
                UpdateMaterials();
            }
        }

        bool _isScenePlane
        {
            get
            {
                return SceneState.ScenePlane.Value == this;
            }
        }

        void OnEnable()
        {
            DebugModeSetting.AddListener(HandleDebugModeChange);
            HandleDebugModeChange(DebugModeSetting.Value);

            Interactable.firstHoverEntered.AddListener(HandleHover);
            Interactable.lastHoverExited.AddListener(HandleUnhover);

            PinchAction.action.performed += HandlePinchSelect;

            ARPlane.boundaryChanged += HandleBoundaryChanged;

            SceneState.Phase.AddListener(HandleSceneStateChange);
            HandleSceneStateChange(SceneState.Phase.Value);

            UpdateMaterials();
        }

        void OnDisable()
        {
            DebugModeSetting.RemoveListener(HandleDebugModeChange);

            Interactable.firstHoverEntered.RemoveListener(HandleHover);
            Interactable.lastHoverExited.RemoveListener(HandleUnhover);

            PinchAction.action.performed -= HandlePinchSelect;

            ARPlane.boundaryChanged -= HandleBoundaryChanged;

            SceneState.Phase.RemoveListener(HandleSceneStateChange);

            if (_isScenePlane)
            {
                if (SceneState.Phase.Value == Phase.Shooting)
                {
                    // The plane that was used for the scene was disabled.
                    // Go back to plane selection.
                    SceneState.Phase.Value = Phase.SearchingForPlane;
                }

                SceneState.ScenePlane.Value = null;
            }
        }

        void HandleSceneStateChange(Phase phase)
        {
            switch (phase)
            {
                case Phase.SearchingForPlane:
                    EnableInteraction = true;
                    break;
                case Phase.Shooting:
                    if (_isScenePlane)
                    {
                        EnableInteraction = true;
                    }
                    else
                    {
                        EnableInteraction = false;
                    }

                    break;
            }

            UpdateMaterials();
        }

        void HandleDebugModeChange(bool debugMode)
        {
            UpdateMaterials();
        }

        void HandleHover(HoverEnterEventArgs eventData)
        {
            switch (SceneState.Phase.Value)
            {
                case Phase.SearchingForPlane:
                    SceneState.ScenePlane.Value = this;
                    break;
                case Phase.Shooting:
                    break;
            }

            UpdateMaterials();
        }

        void HandleUnhover(HoverExitEventArgs eventData)
        {
            if (_isScenePlane)
            {
                SceneState.ScenePlane.Value = null;
            }

            UpdateMaterials();
        }

        void HandlePinchSelect(InputAction.CallbackContext context)
        {
            switch (SceneState.Phase.Value)
            {
                case Phase.SearchingForPlane:
                    if (_isScenePlane)
                    {
                        SceneState.Phase.Value = Phase.Shooting;
                        Singleton.Instance.Audio.PlayOneShot(SelectionClip, transform.position);
                    }

                    break;
                case Phase.Shooting:
                    break;
            }
        }

        Material GetMaterial()
        {
            switch (SceneState.Phase.Value)
            {
                case Phase.SearchingForPlane:
                    if (_isScenePlane)
                    {
                        return HoverMaterial;
                    }
                    else
                    {
                        if (DebugModeSetting.Value)
                        {
                            return SelectableMaterial;
                        }
                    }

                    break;
                case Phase.Shooting:
                    if (DebugModeSetting.Value)
                    {
                        return SelectableMaterial;
                    }

                    break;
            }

            return NonSelectableMaterial;
        }

        Material GetLineMaterial()
        {
            if (EnableInteraction || DebugModeSetting.Value)
            {
                return SelectableLineMaterial;
            }
            else
            {
                return NonSelectableLineMaterial;
            }
        }

        void HandleBoundaryChanged(ARPlaneBoundaryChangedEventArgs args)
        {
            if (_isScenePlane && SceneState.Phase.Value == Phase.SearchingForPlane)
            {
                SceneState.ScenePlane.Value = this;
            }
        }

        void UpdateMaterials()
        {
            MeshRenderer.material = GetMaterial();
            LineRenderer.material = GetLineMaterial();
        }
    }
}
