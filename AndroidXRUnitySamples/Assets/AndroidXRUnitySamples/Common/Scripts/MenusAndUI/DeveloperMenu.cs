// <copyright file="DeveloperMenu.cs" company="Google LLC">
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
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features.Android;
using XRStats = UnityEngine.XR.Provider.XRStats;

namespace AndroidXRUnitySamples.MenusAndUI
{
    /// <summary>
    /// Class used to fill in contents of the Developer tab on the main menu.
    /// </summary>
    public class DeveloperMenu : MonoBehaviour
    {
        [SerializeField] private TMP_Text _leftHandStatus;
        [SerializeField] private TMP_Text _rightHandStatus;
        [SerializeField] private TMP_Text _eyeStatus;
        [SerializeField] private TMP_Text _performanceMetrics;
        [SerializeField] private AndroidXRPerformanceMetrics _axrPerformanceMetrics;
        [SerializeField] private InputActionProperty _leftHandTrackedAction;
        [SerializeField] private InputActionProperty _rightHandTrackedAction;
        [SerializeField] private InputActionProperty _eyeTrackedAction;

        StringBuilder _stringBuilder = new StringBuilder();
        XRDisplaySubsystem _display;

        void Start()
        {
            _display = GetFirstDisplaySubsystem();
        }

        void Update()
        {
            _leftHandStatus.text = GetActionText(_leftHandTrackedAction);
            _rightHandStatus.text = GetActionText(_rightHandTrackedAction);
            _eyeStatus.text = GetEyeStateText(_eyeTrackedAction);
            _performanceMetrics.text = GetPerformanceMetrics();
        }

        string GetActionText(InputActionProperty property)
        {
            if (IsDisabledReferenceAction(property))
            {
                return "disabled";
            }

            return property.action.IsPressed().ToString();
        }

        string GetEyeStateText(InputActionProperty property)
        {
            if (IsDisabledReferenceAction(property))
            {
                return "disabled";
            }

            InputTrackingState state = (InputTrackingState)property.action.ReadValue<int>();
            return (state != InputTrackingState.None).ToString();
        }

        bool IsDisabledReferenceAction(InputActionProperty property)
        {
            return property.reference != null && property.reference.action != null &&
                   !property.reference.action.enabled;
        }

        string GetPerformanceMetrics()
        {
            if (_axrPerformanceMetrics == null || !_axrPerformanceMetrics.enabled)
            {
                return "AndroidXRPerformanceMetrics not enabled";
            }

            if (_axrPerformanceMetrics.supportedMetricPaths == null)
            {
                return "No supported metric paths";
            }

            if (_display == null)
            {
                return "Invalid display subsystem";
            }

            _stringBuilder.Clear();
            foreach (string metric in _axrPerformanceMetrics.supportedMetricPaths)
            {
                if (XRStats.TryGetStat(_display, metric, out float value))
                {
                    _stringBuilder.AppendFormat("{0}: {1}\n", metric, value.ToString("F2"));
                }
                else
                {
                    _stringBuilder.AppendFormat("{0}: n/a\n", metric);
                }
            }

            return _stringBuilder.ToString();
        }

        XRDisplaySubsystem GetFirstDisplaySubsystem()
        {
            var displays = new List<XRDisplaySubsystem>();
            SubsystemManager.GetSubsystems(displays);
            if (displays.Count == 0)
            {
                Debug.Log("No display subsystem found.");
                return null;
            }

            return displays[0];
        }
    }
}
