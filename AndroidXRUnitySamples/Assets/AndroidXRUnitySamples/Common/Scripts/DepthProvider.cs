// <copyright file="DepthProvider.cs" company="Google LLC">
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
using AndroidXRUnitySamples.Variables;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Class used to read the Depth Texture from the device.
    /// </summary>
    public class DepthProvider : MonoBehaviour
    {
        /// <summary>
        /// Event which is fired each time a new depth texture is read.
        /// </summary>
        public DepthFrameVariable DepthFrame;

        private Texture2D _depthTexture;

        private void Start()
        {
            AROcclusionManager occlusionManager =
                Singleton.Instance.OriginManager.AROcclusionManager;
            if (occlusionManager == null || occlusionManager.subsystem == null)
            {
                Debug.Log($"{nameof(occlusionManager)} not found");
                return;
            }

            if (!occlusionManager.subsystem.running)
            {
                Debug.Log($"{nameof(occlusionManager)} not running");
                return;
            }

            occlusionManager.frameReceived += OnOcclusionFrameReceived;
        }

        private void OnDestroy()
        {
            AROcclusionManager occlusionManager =
                Singleton.Instance.OriginManager.AROcclusionManager;
            if (occlusionManager != null)
            {
                occlusionManager.frameReceived -= OnOcclusionFrameReceived;
            }
        }

        private void Update()
        {
            if (Application.isEditor)
            {
                EmitMockDepthFrame();
                return;
            }
        }

        private void OnOcclusionFrameReceived(AROcclusionFrameEventArgs eventArgs)
        {
            int textureCount = eventArgs.externalTextures.Count;
            if (textureCount < 1)
            {
                Debug.Log($"Invalid depth texture count: {textureCount} (expected 1 or more)");
                return;
            }

            Texture t = eventArgs.externalTextures[0].texture;

            if (!SystemInfo.SupportsTextureFormat((TextureFormat)t.graphicsFormat))
            {
                return;
            }

            if (!SystemInfo.IsFormatSupported(t.graphicsFormat, GraphicsFormatUsage.Sample))
            {
                return;
            }

            if (_depthTexture == null || _depthTexture.width != t.width ||
                _depthTexture.height != t.height)
            {
                _depthTexture = new Texture2D(
                    t.width, t.height, t.graphicsFormat, TextureCreationFlags.None);

                if (_depthTexture == null)
                {
                    Debug.LogError("Unable to create depth texture");
                    return;
                }

                Debug.Log($"Created depth texture: {t.width}x{t.height} {t.graphicsFormat}");

                _depthTexture.filterMode = FilterMode.Point;
            }

            // To use the texture on the CPU, we need to read it back from the GPU.
            if (t is RenderTexture rt)
            {
                var currentRT = RenderTexture.active;
                RenderTexture.active = rt;
                _depthTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                _depthTexture.Apply();
                RenderTexture.active = currentRT;
            }
            else
            {
                Debug.LogError("Failed to read depth texture");
                return;
            }

            if (!eventArgs.TryGetFovs(out var fovs))
            {
                Debug.LogError("Failed to get depth texture fovs");
                return;
            }

            if (!eventArgs.TryGetPoses(out var poses))
            {
                Debug.LogError("Failed to get depth texture poses");
                return;
            }

            DepthFrame.Value = new DepthFrame
            {
                Texture = _depthTexture,
                View = fovs[0],
                Pose = poses[0],
            };
        }

        private void EmitMockDepthFrame()
        {
            if (_depthTexture == null)
            {
                const int size = 30;
                var depthData = new NativeArray<float>(size * size, Allocator.Persistent);
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        depthData[i * size + j] = Mathf.Sin(i / (float)size * 10f) * .2f + 1.0f;
                    }
                }

                _depthTexture = new Texture2D(size, size, TextureFormat.RFloat, mipChain: false);
                _depthTexture.SetPixelData(depthData, 0);
                _depthTexture.Apply();
            }

            if (Camera.main != null)
            {
                var view0 = new XRFov(-MathF.PI / 4f, MathF.PI / 4f, MathF.PI / 4f, -MathF.PI / 4f);
                var pose0 =
                    new Pose(Camera.main.transform.position, Camera.main.transform.rotation);

                DepthFrame.Value = new DepthFrame
                {
                    Texture = _depthTexture,
                    View = view0,
                    Pose = pose0,
                };
            }
        }
    }
}
