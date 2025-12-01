// <copyright file="SeaThrough.cs" company="Google LLC">
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
namespace AndroidXRUnitySamples.SeaThrough
{
    using AndroidXRUnitySamples;
    using Google.XR.Extensions;
    using Unity.XR.CoreUtils.Collections;
    using UnityEngine;
    using UnityEngine.Assertions;
    using UnityEngine.Experimental.Rendering;
    using UnityEngine.Rendering;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.XR.ARSubsystems;
    using UnityEngine.XR.OpenXR;

    /// <summary>
    /// Controls the SeaThrough scene, which combines real-world depth with virtual content.
    /// </summary>
    public class SeaThrough : MonoBehaviour
    {
        private static readonly int _leftDepthTextureProperty =
            Shader.PropertyToID("_LeftDepthTexture");

        private static readonly int _rightDepthTextureProperty =
            Shader.PropertyToID("_RightDepthTexture");

        private static readonly int _leftDepthTextureFromWorldProperty = Shader.PropertyToID(
            "_LeftDepthTextureFromWorld");

        private static readonly int _rightDepthTextureFromWorldProperty = Shader.PropertyToID(
            "_RightDepthTextureFromWorld");

        private static readonly int _mainTexProperty = Shader.PropertyToID("_MainTex");
        private static readonly int _tanFovProperty = Shader.PropertyToID("_TanFov");
        private static readonly int _depthCameraProperty = Shader.PropertyToID("_DepthCamera");
        private static readonly int _depthResProperty = Shader.PropertyToID("_DepthRes");

        [SerializeField] private Material _underwaterFullscreenMaterial;
        [SerializeField] private Material _leftDepthPointCloudMaterial;
        [SerializeField] private Material _rightDepthPointCloudMaterial;
        [SerializeField] private UnderwaterEffectRendererFeature _underwaterEffectRendererFeature;
        [SerializeField] private Material _fishMaterial;
        [SerializeField] private FishManager _fishManager;
        [SerializeField] private GameObject _leftHandBubbles;
        [SerializeField] private GameObject _rightHandBubbles;
        [SerializeField] private GameObject _waterSurface;
        [SerializeField] private Material _waterSurfaceMaterial;
        [SerializeField] private float _renderScale = 1.0f;
        [SerializeField] private bool _enableCaustics;

        private ReadOnlyList<Pose> _poses;
        private ReadOnlyList<XRFov> _fovs;
        private Texture2D[] _depthTextures = new Texture2D[2];
        private GlobalKeyword _haveDepthTextureKeyword;
        private Matrix4x4 _leftEyeDepthFromWorld;
        private Matrix4x4 _rightEyeDepthFromWorld;
        private float _originalRenderScale = 1.0f;
        private XRSceneMeshingFeature _sceneMeshingFeature = null;

        private static Matrix4x4 ProjectionMatrixFromFov(
            XRFov fov, float nearClipPlane = 0.001f, float farClipPlane = 100.0f)
        {
            float left = Mathf.Tan(fov.angleLeft) * nearClipPlane;
            float right = Mathf.Tan(fov.angleRight) * nearClipPlane;
            float top = Mathf.Tan(fov.angleUp) * nearClipPlane;
            float bottom = Mathf.Tan(fov.angleDown) * nearClipPlane;
            return Matrix4x4.Frustum(left, right, bottom, top, nearClipPlane, farClipPlane);
        }

        private void Start()
        {
            _originalRenderScale = Singleton.Instance.OriginManager.RenderScale;
            Singleton.Instance.OriginManager.RenderScale = _renderScale;
            Debug.Log("Changing render scale from " + _originalRenderScale + " to " + _renderScale);

            // Disable scene mesh so we can use hand mesh without the performance penalty of scene
            // meshing.
            _sceneMeshingFeature = OpenXRSettings.Instance.GetFeature<XRSceneMeshingFeature>();
            if (_sceneMeshingFeature != null)
            {
                _sceneMeshingFeature.enabled = false;
            }

            // Temporary hack to force disable scene meshing.
            XRSceneMeshingApi.SetEnabled(false);

            _haveDepthTextureKeyword = GlobalKeyword.Create("_HAVEDEPTHTEXTURE");
            var occlusionManager = Singleton.Instance.OriginManager.AROcclusionManager;
            if (occlusionManager == null)
            {
                Debug.LogError("AROcclusionManager not found");
                return;
            }

            var causticsKeyword = new LocalKeyword(
                _underwaterFullscreenMaterial.shader, "_ENABLECAUSTICS");
            _underwaterFullscreenMaterial.SetKeyword(
                causticsKeyword, _enableCaustics);

#if !UNITY_EDITOR
            occlusionManager.enabled = true;
            occlusionManager.frameReceived += OnDepthFrameReceived;
#endif
        }

        private void Update()
        {
            bool waterIsAboveCamera =
                _waterSurface.transform.position.y > Camera.main.transform.position.y;
            _leftHandBubbles.gameObject.SetActive(waterIsAboveCamera);
            _rightHandBubbles.gameObject.SetActive(waterIsAboveCamera);
            _fishManager.SpawnFish = waterIsAboveCamera;
        }

        private int SetDepthPointCloudMaterialParams(int view, Material material)
        {
            Assert.IsTrue(view == 0 || view == 1);
            XRFov fov = _fovs[view];
            Pose pose = _poses[view];
            Matrix4x4 depthCamera =
                Matrix4x4.TRS(pose.position, pose.rotation, Vector3.one);
            var tanFov = new Vector4(
                Mathf.Tan(fov.angleLeft), Mathf.Tan(fov.angleRight),
                Mathf.Tan(fov.angleDown), Mathf.Tan(fov.angleUp));
            material.SetTexture(_mainTexProperty, _depthTextures[view]);
            material.SetVector(_tanFovProperty, tanFov);
            material.SetMatrix(_depthCameraProperty, depthCamera);
            material.SetFloat(_depthResProperty, _depthTextures[view].width);
            return _depthTextures[view].width * _depthTextures[view].height;
        }

        private void OnDepthFrameReceived(AROcclusionFrameEventArgs eventArgs)
        {
            if (!eventArgs.TryGetFovs(out _fovs))
            {
                Debug.LogError("Failed to get depth texture fovs");
                return;
            }

            if (!eventArgs.TryGetPoses(out _poses))
            {
                Debug.LogError("Failed to get depth texture poses");
                return;
            }

            Texture depthTextures = eventArgs.externalTextures[0].texture;
            Assert.IsTrue(depthTextures is RenderTexture);
            for (int i = 0; i < 2; i++)
            {
                RenderTexture depthRenderTextures = depthTextures as RenderTexture;
                if (_depthTextures[i] == null)
                {
                    _depthTextures[i] = new Texture2D(
                            depthTextures.width, depthTextures.height,
                            depthRenderTextures.graphicsFormat, TextureCreationFlags.None);
                }

                Graphics.CopyTexture(depthRenderTextures, i, _depthTextures[i], 0);
            }

            _underwaterEffectRendererFeature.LeftDepthPointCloudNumPoints =
                SetDepthPointCloudMaterialParams(0, _leftDepthPointCloudMaterial);
            _underwaterEffectRendererFeature.RightDepthPointCloudNumPoints =
                SetDepthPointCloudMaterialParams(1, _rightDepthPointCloudMaterial);

            Pose leftEyePose = _poses[0];
            _leftEyeDepthFromWorld = ProjectionMatrixFromFov(_fovs[0]) *
                Matrix4x4.TRS(leftEyePose.position, leftEyePose.rotation, Vector3.one).inverse;

            Pose rightEyePose = _poses[1];
            _rightEyeDepthFromWorld = ProjectionMatrixFromFov(_fovs[1]) *
                Matrix4x4.TRS(rightEyePose.position, rightEyePose.rotation, Vector3.one).inverse;

            Shader.SetGlobalTexture(_leftDepthTextureProperty, _depthTextures[0]);
            Shader.SetGlobalTexture(_rightDepthTextureProperty, _depthTextures[1]);
            Shader.SetGlobalMatrix(_leftDepthTextureFromWorldProperty, _leftEyeDepthFromWorld);
            Shader.SetGlobalMatrix(_rightDepthTextureFromWorldProperty, _rightEyeDepthFromWorld);
            Shader.EnableKeyword(_haveDepthTextureKeyword);
        }

        private void OnDestroy()
        {
#if !UNITY_EDITOR
            var occlusionManager = Singleton.Instance.OriginManager.AROcclusionManager;
            occlusionManager.frameReceived -= OnDepthFrameReceived;
#endif
            Singleton.Instance.OriginManager.RenderScale = _originalRenderScale;
            Debug.Log("Reverting render scale back to " + _originalRenderScale);

            // Restore scene mesh feature.
            XRSceneMeshingApi.SetEnabled(true);
            if (_sceneMeshingFeature != null)
            {
                _sceneMeshingFeature.enabled = true;
            }
        }
    }
}
