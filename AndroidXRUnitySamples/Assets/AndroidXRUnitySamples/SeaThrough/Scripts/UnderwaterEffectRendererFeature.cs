// <copyright file="UnderwaterEffectRendererFeature.cs" company="Google LLC">
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
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.RenderGraphModule;
    using UnityEngine.Rendering.Universal;

    /// <summary>
    /// Underwater effect for the sea through scene.
    /// This effect applies some fog based on the virtual and real depth.
    /// </summary>
    public class UnderwaterEffectRendererFeature : ScriptableRendererFeature
    {
        /// <summary>
        /// Where to inject the render feature.
        /// </summary>
        public RenderPassEvent InjectionPoint = RenderPassEvent.AfterRenderingPostProcessing;

        /// <summary>
        /// The underwater material.
        /// </summary>
        public Material UnderwaterMaterial;

        /// <summary>
        /// The point cloud material for reprojecting the depth.
        /// </summary>
        public Material LeftDepthPointCloudMaterial;

        /// <summary>
        /// The number of points in the point cloud.
        /// </summary>
        [System.NonSerialized]
        public int LeftDepthPointCloudNumPoints;

        /// <summary>
        /// The point cloud material for reprojecting the depth.
        /// </summary>
        public Material RightDepthPointCloudMaterial;

        /// <summary>
        /// The number of points in the point cloud.
        /// </summary>
        [System.NonSerialized]
        public int RightDepthPointCloudNumPoints;

        /// <summary>
        /// Per-eye resolution to use for the depth point cloud texture. The generated render
        /// texture will have twice the width since both the point cloud for each eye will be
        /// rendered side-by-side.
        /// </summary>
        public int DepthPointCloudTextureResolution = 256;

        private UnderwaterEffectPass _scriptablePass;

        /// <inheritdoc/>
        public override void Create()
        {
            _scriptablePass = new UnderwaterEffectPass();
            _scriptablePass.ConfigureInput(ScriptableRenderPassInput.Depth);
            _scriptablePass.renderPassEvent = InjectionPoint;
        }

        /// <inheritdoc/>
        public override void AddRenderPasses(
            ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (UnderwaterMaterial == null)
            {
                Debug.LogWarning("UnderwaterMaterial is null and will be skipped");
                return;
            }

            _scriptablePass.Setup(UnderwaterMaterial,
                LeftDepthPointCloudMaterial, LeftDepthPointCloudNumPoints,
                RightDepthPointCloudMaterial, RightDepthPointCloudNumPoints,
                DepthPointCloudTextureResolution);
            renderer.EnqueuePass(_scriptablePass);
        }

        internal class UnderwaterEffectPass : ScriptableRenderPass
        {
            private const string _passName = "UnderwaterPass";
            private static readonly MaterialPropertyBlock _sharedPropertyBlock =
                new MaterialPropertyBlock();

            private static readonly int _blitTexture = Shader.PropertyToID("_BlitTexture");
            private static readonly int _blitScaleBias = Shader.PropertyToID("_BlitScaleBias");
            private static readonly int _projectedDepthTextureProperty =
                Shader.PropertyToID("_ProjectedDepthTexture");

            private static readonly int _viewProjectionMatrixProperty =
                Shader.PropertyToID("_ViewProjectionMatrix");

            private static readonly int _viewMatrixProperty = Shader.PropertyToID("_ViewMatrix");

            private static readonly Matrix4x4 _flipYMatrix =
                Matrix4x4.Scale(new Vector3(1.0f, -1.0f, 1.0f));

            private Material _underwaterMaterial;
            private Material _leftDepthPointCloudMaterial;
            private int _leftDepthPointCloudNumPoints;
            private Material _rightDepthPointCloudMaterial;
            private int _rightDepthPointCloudNumPoints;
            private RTHandle _depthPointCloudRenderTextureHandle;
            private int _depthPointCloudTextureResolution;

            public void Setup(
                Material underwaterMaterial, Material leftDepthPointCloudMaterial,
                int leftDepthPointCloudNumPoints, Material rightDepthPointCloudMaterial,
                int rightDepthPointCloudNumPoints, int depthPointCloudTextureResolution)
            {
                _underwaterMaterial = underwaterMaterial;
                _leftDepthPointCloudMaterial = leftDepthPointCloudMaterial;
                _leftDepthPointCloudNumPoints = leftDepthPointCloudNumPoints;
                _rightDepthPointCloudMaterial = rightDepthPointCloudMaterial;
                _rightDepthPointCloudNumPoints = rightDepthPointCloudNumPoints;
                _depthPointCloudTextureResolution = depthPointCloudTextureResolution;

                requiresIntermediateTexture = true;
            }

            public override void RecordRenderGraph(
                RenderGraph renderGraph, ContextContainer frameData)
            {
                var resourceData = frameData.Get<UniversalResourceData>();
                if (resourceData.isActiveTargetBackBuffer)
                {
                    Debug.LogError(
                        "Skipping UnderwaterEffectPass pass because " +
                        "isActiveTargetBackBuffer is true");
                    return;
                }

                // --- 1. Render Point Cloud to a render texture ---
                var pointCloudTextureProperties =
                    new RenderTextureDescriptor(
                        2 * _depthPointCloudTextureResolution, _depthPointCloudTextureResolution,
                        RenderTextureFormat.RFloat, 0);
                RenderingUtils.ReAllocateHandleIfNeeded(
                    ref _depthPointCloudRenderTextureHandle, pointCloudTextureProperties,
                    FilterMode.Bilinear, TextureWrapMode.Clamp, name: "Left Depth Point Cloud");

                if (_leftDepthPointCloudNumPoints > 0 && _leftDepthPointCloudMaterial != null ||
                    _rightDepthPointCloudNumPoints > 0 && _rightDepthPointCloudMaterial != null)
                {
                    SetupDepthPointCloudPass(
                        renderGraph, _leftDepthPointCloudMaterial, _leftDepthPointCloudNumPoints,
                        _rightDepthPointCloudMaterial, _rightDepthPointCloudNumPoints,
                        _depthPointCloudRenderTextureHandle, _depthPointCloudTextureResolution);
                }

                // --- 2. Render the Underwater Effect ---
                var source = resourceData.activeColorTexture;
                var destinationDesc = renderGraph.GetTextureDesc(source);
                destinationDesc.name = $"CameraColor-{_passName}";
                destinationDesc.clearBuffer = true;
                TextureHandle destination = renderGraph.CreateTexture(destinationDesc);
                AddFullscreenRenderPassInputPass(renderGraph, resourceData, source, destination);
                resourceData.cameraColor = destination;
            }

            private static void ExecutePointCloudPassForEye(RasterCommandBuffer cmd,
                Camera mainCamera, int renderTextureResolution, Material material,
                int numPoints, Camera.StereoscopicEye eye)
            {
                cmd.SetViewport(
                    new Rect(eye == Camera.StereoscopicEye.Left ? 0 : renderTextureResolution, 0,
                    renderTextureResolution, renderTextureResolution));

                var projectionMatrix = _flipYMatrix * mainCamera.GetStereoProjectionMatrix(eye);
                var viewMatrix = mainCamera.GetStereoViewMatrix(eye);
                _sharedPropertyBlock.Clear();
                _sharedPropertyBlock.SetMatrix(_viewProjectionMatrixProperty,
                    projectionMatrix * viewMatrix);
                _sharedPropertyBlock.SetMatrix(_viewMatrixProperty, viewMatrix);

                cmd.DrawProcedural(
                    Matrix4x4.identity, material, /*shaderPass=*/0,
                    MeshTopology.Points, numPoints, /*instanceCount=*/1,
                    _sharedPropertyBlock);
            }

            private static void SetupDepthPointCloudPass(
                RenderGraph renderGraph, Material leftMaterial, int leftNumPoints,
                Material rightMaterial, int rightNumPoints, RTHandle destination,
                int destinationResolution)
            {
                using (var builder = renderGraph.AddRasterRenderPass<PointCloudPassData>(
                        "Render Depth Point Cloud", out var passData))
                {
                    passData._leftMaterial = leftMaterial;
                    passData._leftNumPoints = leftNumPoints;
                    passData._rightMaterial = rightMaterial;
                    passData._rightNumPoints = rightNumPoints;
                    passData._renderTextureResolution = destinationResolution;

                    TextureHandle pointCloudTexture =
                        renderGraph.ImportTexture(destination);
                    builder.SetRenderAttachment(pointCloudTexture, 0, AccessFlags.Write);

                    builder.SetRenderFunc((PointCloudPassData data, RasterGraphContext context) =>
                    {
                        context.cmd.ClearRenderTarget(
                            /*clearDepth=*/true, /*clearColor=*/true, Color.white);
                        var mainCamera = Camera.main;
                        ExecutePointCloudPassForEye(context.cmd, mainCamera,
                            data._renderTextureResolution, data._leftMaterial, data._leftNumPoints,
                            Camera.StereoscopicEye.Left);
                        ExecutePointCloudPassForEye(context.cmd, mainCamera,
                            data._renderTextureResolution, data._rightMaterial,
                            data._rightNumPoints, Camera.StereoscopicEye.Right);
                    });
                }
            }

            private static void ExecuteMainPass(
                RasterCommandBuffer cmd, RTHandle sourceTexture, Material material, int passIndex,
                TextureHandle projectedDepthTexture)
            {
                _sharedPropertyBlock.Clear();
                if (sourceTexture != null)
                {
                    _sharedPropertyBlock.SetTexture(_blitTexture, sourceTexture);
                }

                _sharedPropertyBlock.SetVector(_blitScaleBias, new Vector4(1, 1, 0, 0));
                _sharedPropertyBlock.SetTexture(
                    _projectedDepthTextureProperty, projectedDepthTexture);

                cmd.DrawProcedural(
                    Matrix4x4.identity, material, passIndex, MeshTopology.Triangles, 3, 1,
                    _sharedPropertyBlock);
            }

            private void AddFullscreenRenderPassInputPass(
                RenderGraph renderGraph, UniversalResourceData resourcesData,
                TextureHandle source, TextureHandle destination)
            {
                using (var builder = renderGraph.AddRasterRenderPass<MainPassData>(
                    passName, out var passData, profilingSampler))
                {
                    passData._material = _underwaterMaterial;
                    passData._passIndex = 0;
                    passData._projectedDepthTexture =
                        renderGraph.ImportTexture(_depthPointCloudRenderTextureHandle);

                    passData._inputTexture = source;

                    if (passData._inputTexture.IsValid())
                    {
                        builder.UseTexture(passData._inputTexture, AccessFlags.Read);
                    }

                    Debug.Assert(resourcesData.cameraDepthTexture.IsValid());
                    builder.UseTexture(resourcesData.cameraDepthTexture);

                    builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

                    builder.SetRenderFunc((MainPassData data, RasterGraphContext rgContext) =>
                    {
                        ExecuteMainPass(
                            rgContext.cmd, data._inputTexture, data._material, data._passIndex,
                            data._projectedDepthTexture);
                    });
                }
            }

            private class PointCloudPassData
            {
                internal Material _leftMaterial;
                internal int _leftNumPoints;
                internal Material _rightMaterial;
                internal int _rightNumPoints;
                internal int _renderTextureResolution;
            }

            private class MainPassData
            {
                internal Material _material;
                internal int _passIndex;
                internal TextureHandle _inputTexture;
                internal TextureHandle _projectedDepthTexture;
            }
        }
    }
}
