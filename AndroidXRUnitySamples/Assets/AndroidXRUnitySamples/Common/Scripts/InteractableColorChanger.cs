// <copyright file="InteractableColorChanger.cs" company="Google LLC">
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
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Handles color change affordance for an interactable.
    /// </summary>
    public class InteractableColorChanger : MonoBehaviour
    {
        /// <summary>
        /// Target interactable.
        /// </summary>
        public XRBaseInteractable Interactable;

        /// <summary>
        /// Color when hovered.
        /// </summary>
        public Color HoverColor;

        /// <summary>
        /// Color when unhovered.
        /// </summary>
        public Color UnhoverColor;

        /// <summary>
        /// Target material.
        /// </summary>
        public Material Material;

        private void HandleHover(HoverEnterEventArgs eventData)
        {
            Material.SetColor("_Color", HoverColor);
        }

        private void HandleUnhover(HoverExitEventArgs eventData)
        {
            Material.SetColor("_Color", UnhoverColor);
        }

        private void Start()
        {
            Interactable.firstHoverEntered.AddListener(HandleHover);
            Interactable.lastHoverExited.AddListener(HandleUnhover);
        }

        private void OnDestroy()
        {
            Interactable.firstHoverEntered.RemoveListener(HandleHover);
            Interactable.lastHoverExited.RemoveListener(HandleUnhover);
        }
    }
}
