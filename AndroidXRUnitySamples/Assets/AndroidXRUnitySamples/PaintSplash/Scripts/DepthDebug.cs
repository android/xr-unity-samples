// <copyright file="DepthDebug.cs" company="Google LLC">
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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AndroidXRUnitySamples.PaintSplash
{
    /// <summary>
    /// Class used to show debug info for depth when in debug mode.
    /// </summary>
    public class DepthDebug : MonoBehaviour
    {
        [SerializeField]
        private DepthFrameVariable _depthFrame;
        [SerializeField]
        private BoolVariable _debugMode;
        [SerializeField]
        private GameObject _debugContainer;
        [SerializeField]
        private RawImage _debugImage;
        [SerializeField]
        private TMP_Text _debugText;
        [SerializeField]
        private MeshRenderer _depthMeshRenderer;
        [SerializeField]
        private Material _depthMeshDebugMaterial;
        [SerializeField]
        [Tooltip("Whether the depth mesh should be visible when debug is off.")]
        private bool _keepDepthMeshVisible = false;
        private Material _originalDepthMeshMaterial;

        private void OnEnable()
        {
            _depthFrame.AddListener(HandleDepthFrame);

            _debugMode.AddListener(HandleDebugModeChange);
            HandleDebugModeChange(_debugMode.Value);

            Application.logMessageReceived += OnApplicationOnLogMessageReceived;
            _debugText.text = string.Empty;
        }

        private void OnDisable()
        {
            _depthFrame.RemoveListener(HandleDepthFrame);
            _debugMode.RemoveListener(HandleDebugModeChange);
            Application.logMessageReceived -= OnApplicationOnLogMessageReceived;
        }

        private void OnApplicationOnLogMessageReceived(string logString, string _, LogType type)
        {
            if (!_debugMode.Value)
            {
                return;
            }

            string color;
            switch (type)
            {
                case LogType.Exception:
                case LogType.Error:
                case LogType.Assert:
                    color = "red";
                    break;
                case LogType.Warning:
                    color = "yellow";
                    break;
                case LogType.Log:
                    color = "green";
                    break;
                default:
                    color = "white";
                    break;
            }

            string richText = $"<color={color}>{logString}</color>";

            if (_debugText.text.StartsWith(richText))
            {
                return;
            }

            _debugText.text = $"{richText}\n{_debugText.text}";
        }

        private void HandleDepthFrame(DepthFrame depthFrame)
        {
            if (!_debugMode.Value)
            {
                return;
            }

            _debugImage.texture = depthFrame.Texture;
        }

        private void HandleDebugModeChange(bool debug)
        {
            _debugContainer.SetActive(debug);
            _depthMeshRenderer.enabled = debug || _keepDepthMeshVisible;
            if (debug && _depthMeshDebugMaterial != null)
            {
                _originalDepthMeshMaterial = _depthMeshRenderer.material;
                _depthMeshRenderer.material = _depthMeshDebugMaterial;
            }
            else if (!debug && _originalDepthMeshMaterial != null)
            {
                _depthMeshRenderer.material = _originalDepthMeshMaterial;
            }
        }
    }
}
