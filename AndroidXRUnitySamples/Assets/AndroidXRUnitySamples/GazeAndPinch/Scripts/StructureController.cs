// <copyright file="StructureController.cs" company="Google LLC">
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

using AndroidXRUnitySamples.MenusAndUI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace AndroidXRUnitySamples.GazeAndPinch
{
    /// <summary>
    /// Used to control a structure and allow for actions such as resetting back to original
    /// shape.
    /// </summary>
    public class StructureController : MonoBehaviour
    {
        /// <summary>
        /// SceneState indicating the state of the scene.
        /// </summary>
        public SceneState SceneState;

        /// <summary>
        /// The container of the structure game objects.
        /// </summary>
        public GameObject Container;

        /// <summary>
        /// The set of prefabs used to create the structure.
        /// </summary>
        public GameObject[] StructurePrefabs;

        /// <summary>
        /// Interactable which triggers the enablement of the UI.
        /// </summary>
        public XRBaseInteractable Interactable;

        /// <summary>
        /// Animator which manages the animtations of the UI.
        /// </summary>
        public Animator Animator;

        /// <summary>
        /// Button used to reset the structure to its original shape.
        /// </summary>
        public ShadowButton ResetButton;

        /// <summary>
        /// Button used to use the next structure.
        /// </summary>
        public ShadowButton NextButton;

        /// <summary>
        /// Button used to restart the whole experience.
        /// </summary>
        public ShadowButton RestartButton;

        int _structureIndex = 0;

        enum UIState
        {
            StructureNotLookedAt = 0,
            StructureLookedAt = 1,
        }

        void OnEnable()
        {
            Interactable.firstHoverEntered.AddListener(HandleHover);
            Interactable.lastHoverExited.AddListener(HandleUnhover);

            ResetButton.OnPress.AddListener(Reset);
            NextButton.OnPress.AddListener(NextStructure);
            RestartButton.OnPress.AddListener(RestartExperience);

            SetUIState(UIState.StructureNotLookedAt);

            Reset();
        }

        void OnDisable()
        {
            Interactable.firstHoverEntered.RemoveListener(HandleHover);
            Interactable.lastHoverExited.RemoveListener(HandleUnhover);

            ResetButton.OnPress.RemoveListener(Reset);
            NextButton.OnPress.RemoveListener(NextStructure);
            RestartButton.OnPress.RemoveListener(RestartExperience);
        }

        void SetUIState(UIState state)
        {
            Animator.SetInteger("State", (int)state);
        }

        void HandleHover(HoverEnterEventArgs eventData)
        {
            SetUIState(UIState.StructureLookedAt);
        }

        void HandleUnhover(HoverExitEventArgs eventData)
        {
            SetUIState(UIState.StructureNotLookedAt);
        }

        void Reset()
        {
            // Destroy the remainder of the existing structure.
            foreach (Transform child in Container.transform)
            {
                Destroy(child.gameObject);
            }

            Instantiate(StructurePrefabs[_structureIndex], Container.transform);
        }

        void NextStructure()
        {
            _structureIndex = (_structureIndex + 1) % StructurePrefabs.Length;
            Reset();
        }

        void RestartExperience()
        {
            SceneState.Phase.Value = Phase.SearchingForPlane;
        }

#if UNITY_EDITOR
        void Update()
        {
            if (Keyboard.current[Key.R].wasPressedThisFrame)
            {
                Reset();
            }

            if (Keyboard.current[Key.O].wasPressedThisFrame)
            {
                NextStructure();
            }

            if (Keyboard.current[Key.I].wasPressedThisFrame)
            {
                RestartExperience();
            }
        }
#endif
    }
}
