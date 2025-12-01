// <copyright file="AvatarBodyJointIDToTransform.cs" company="Google LLC">
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

using Google.XR.Extensions;
using UnityEngine;

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// A struct that contains an <see cref="XRAvatarSkeletonJointID"/> and a reference to the <see
    /// cref="Transform"/> to drive with that joint. Used for mapping body joint data to avatar
    /// bones.
    /// </summary>
    [System.Serializable]
    public struct AvatarBodyJointIDToTransform
    {
        /// <summary>The XR Skeleton Joint Identifier that will drive the Transform.</summary>
        [Tooltip("The XR Skeleton Joint Identifier that will drive the Transform.")]
        public XRAvatarSkeletonJointID XRSkelJointID;

        /// <summary>The Transform that will be driven by the specified XR Joint.</summary>
        [Tooltip("The Transform that will be driven by the specified XR Joint.")]
        public Transform JointTransform;
    }
}
