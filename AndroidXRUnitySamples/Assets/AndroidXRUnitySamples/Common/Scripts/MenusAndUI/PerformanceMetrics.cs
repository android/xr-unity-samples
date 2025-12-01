// <copyright file="PerformanceMetrics.cs" company="Google LLC">
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
using UnityEngine.XR;
using UnityEngine.XR.OpenXR.Features.Android;
using XRStats = UnityEngine.XR.Provider.XRStats;

namespace AndroidXRUnitySamples.MenusAndUI
{
    /// <summary>
    /// Class used to fill in contents of the Developer tab on the main menu.
    /// </summary>
    public class PerformanceMetrics : MonoBehaviour
    {
        [SerializeField] private AndroidXRPerformanceMetrics _androidXRPerformanceMetrics;
        [SerializeField] private TMP_Text _text;

        StringBuilder _stringBuilder = new StringBuilder();
        XRDisplaySubsystem _display;

        void Start()
        {
            _display = GetDisplaySubsystem();
        }

        XRDisplaySubsystem GetDisplaySubsystem()
        {
            var displays = new List<XRDisplaySubsystem>();
            SubsystemManager.GetSubsystems(displays);
            if (displays.Count != 1)
            {
                Debug.Log($"Invalid display count: {displays.Count}");
                return null;
            }

            return displays[0];
        }

        void Update()
        {
            if (_androidXRPerformanceMetrics == null ||
                !_androidXRPerformanceMetrics.enabled)
            {
                _text.text = "AndroidXRPerformanceMetrics not enabled";
                return;
            }

            if (_display == null)
            {
                _text.text = "Invalid display subsystem";
                return;
            }

            if (_androidXRPerformanceMetrics.supportedMetricPaths == null)
            {
                _text.text = "No supported metric paths";
                return;
            }

            _stringBuilder.Clear();
            foreach (string metric in _androidXRPerformanceMetrics.supportedMetricPaths)
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

            _text.text = _stringBuilder.ToString();
        }
    }
}
