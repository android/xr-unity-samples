// <copyright file="DepthFrameVariable.cs" company="Google LLC">
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

using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace AndroidXRUnitySamples.Variables
{
    /// <summary>
    /// <inheritdoc cref="Variable{T}" />
    /// <para />
    /// This is the <see cref="Variable{T}" /> extension for the <see cref="DepthFrame" /> type.
    /// </summary>
    [CreateAssetMenu(menuName = "AndroidXRUnitySamples/Common/Variables/DepthFrameVariable")]
    public class DepthFrameVariable : Variable<DepthFrame>
    {
    }

    /// <summary>
    /// Environment depth texture data.
    /// </summary>
    [System.Serializable]
    public class DepthFrame
    {
        /// <summary>
        /// Depth texture corresponding to the left eye view. This is an external texture for use
        /// on the GPU.
        /// </summary>
        public Texture2D Texture;

        /// <summary>
        /// Depth field of view information for the left eye.
        /// </summary>
        public XRFov View;

        /// <summary>
        /// 6 DoF pose of the left depth texture relative to XR Origin.
        /// </summary>
        public Pose Pose;

        /// <summary>
        /// Returns intrinsics based on given view and depth texture.
        /// </summary>
        /// <returns>The intrinsic camera parameters.</returns>
        public (float fx, float fy, float cx, float cy) GetIntrinsics()
        {
            float hFov = View.angleLeft - View.angleRight;
            float vFov = View.angleDown - View.angleUp;
            float hSize = Texture.width / 2.0f;
            float fx = hSize / Mathf.Tan(hFov / 2.0f);
            float fy = hSize / Mathf.Tan(vFov / 2.0f);
            float cx = hSize;
            float cy = hSize;
            return (fx, fy, cx, cy);
        }
    }
}
