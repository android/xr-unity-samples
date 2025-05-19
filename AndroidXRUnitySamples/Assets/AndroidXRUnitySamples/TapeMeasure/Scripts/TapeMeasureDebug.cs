// <copyright file="TapeMeasureDebug.cs" company="Google LLC">
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
using UnityEngine.UI;

namespace AndroidXRUnitySamples.TapeMeasure
{
    /// <summary>
    /// Class used to show debug info for Tape Measure  when in debug mode.
    /// </summary>
    public class TapeMeasureDebug : MonoBehaviour
    {
        [SerializeField] private DepthProvider _depthProvider;
        [SerializeField] private BoolVariable _debugMode;
        [SerializeField] private GameObject _debugContainer;
        [SerializeField] private RawImage _debugImage;

        private void OnEnable()
        {
            _depthProvider.OnDepthFrame.AddListener(HandleDepthFrame);

            _debugMode.AddListener(HandleDebugModeChange);
            HandleDebugModeChange(_debugMode.Value);
        }

        private void OnDisable()
        {
            _depthProvider.OnDepthFrame.RemoveListener(HandleDepthFrame);
            _debugMode.RemoveListener(HandleDebugModeChange);
        }

        private void HandleDepthFrame(Texture2D texture)
        {
            _debugImage.texture = texture;
        }

        private void HandleDebugModeChange(bool debug)
        {
            _debugContainer.SetActive(debug);
        }
    }
}
