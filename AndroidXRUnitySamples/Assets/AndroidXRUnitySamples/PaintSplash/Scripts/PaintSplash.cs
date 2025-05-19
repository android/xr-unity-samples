// <copyright file="PaintSplash.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.PaintSplash
{
    /// <summary>
    /// Class used to control the Paint Splash experience.
    /// </summary>
    public class PaintSplash : MonoBehaviour
    {
        [SerializeField]
        private DepthFrameVariable _depthFrame;
        [SerializeField]
        private Launcher[] _launchers;

        private void OnEnable()
        {
            if (_depthFrame == null)
            {
                Debug.LogError("Occlusion Manager is not set.");
                enabled = false;
                return;
            }

            _depthFrame.AddListener(OnDepthFrameReceived);
        }

        private void OnDisable()
        {
            _depthFrame.RemoveListener(OnDepthFrameReceived);
        }

        private void Start()
        {
            Singleton.Instance.OriginManager.EnablePassthrough = true;
            Singleton.Instance.OriginManager.EnableDepthTexture = true;
            Singleton.Instance.OriginManager.EnableShaderOcclusion = true;

            // Disable launchers until the depth texture is ready.
            foreach (var launcher in _launchers)
            {
                launcher.enabled = false;
            }
        }

        private void OnDepthFrameReceived(DepthFrame depthFrame)
        {
            // Enable launchers once the depth texture is ready.
            foreach (var launcher in _launchers)
            {
                launcher.enabled = true;
            }

            _depthFrame.RemoveListener(OnDepthFrameReceived);
        }
    }
}
