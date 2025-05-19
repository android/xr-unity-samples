// <copyright file="EventToggleGroup.cs" company="Google LLC">
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

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AndroidXRUnitySamples.SurfaceCanvas
{
    /// <summary>
    /// Manages a group of UI toggles belonging to a ToggleGroup,
    /// and fires an event when a toggle changes its value.
    /// </summary>
    [RequireComponent(typeof(ToggleGroup))]
    public class EventToggleGroup : MonoBehaviour
    {
        /// <summary>
        /// This event is fired when an active toggle changes its value.
        /// </summary>
        [SerializeField]
        public ToggleEvent OnActiveTogglesChanged;

        [SerializeField]
        private Toggle[] _toggles;

        private ToggleGroup _toggleGroup;

        private void Awake()
        {
            _toggleGroup = GetComponent<ToggleGroup>();
        }

        private void OnEnable()
        {
            foreach (Toggle toggle in _toggles)
            {
                if (toggle.group != null && toggle.group != _toggleGroup)
                {
                    Debug.LogError(
                            "EventToggleGroup is trying to register a "
                          + "Toggle that is a member of another group.");
                }

                toggle.group = _toggleGroup;
                toggle.onValueChanged.AddListener(HandleToggleValueChanged);
            }
        }

        private void HandleToggleValueChanged(bool isOn)
        {
            if (isOn)
            {
                OnActiveTogglesChanged?.Invoke(_toggleGroup.ActiveToggles().FirstOrDefault());
            }
        }

        private void OnDisable()
        {
            foreach (Toggle toggle in _toggleGroup.ActiveToggles())
            {
                toggle.onValueChanged.RemoveListener(HandleToggleValueChanged);
                toggle.group = null;
            }
        }

        /// <summary>
        /// A simple extension to the UnityEvent class,
        /// typed to transmit a parameter of type Toggle.
        /// </summary>
        [Serializable]
        public class ToggleEvent : UnityEvent<Toggle>
        {
        }
    }
}
