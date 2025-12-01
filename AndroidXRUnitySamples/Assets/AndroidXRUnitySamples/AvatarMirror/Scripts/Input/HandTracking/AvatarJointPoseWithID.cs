// <copyright file="AvatarJointPoseWithID.cs" company="Google LLC">
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
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// Contains an <see cref="XRHandJointID"/> and its local pose.
    /// </summary>
    [Serializable]
    public struct AvatarJointPoseWithID : IEquatable<AvatarJointPoseWithID>
    {
        /// <summary>The XR Hand Joint Identifier.</summary>
        [Tooltip("The XR Hand Joint Identifier.")]
        [SerializeField] public XRHandJointID XRHandJointID;

        /// <summary>The local pose of the joint relative to its parent.</summary>
        [Tooltip("The local pose of the joint relative to its parent.")]
        [SerializeField] public Pose JointLocalPose;

        /// <summary>
        /// Indicates whether this object is equal to another.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(AvatarJointPoseWithID other)
        {
            return XRHandJointID == other.XRHandJointID &&
                   JointLocalPose.Equals(other.JointLocalPose);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see
        /// cref="AvatarJointPoseWithID"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return obj is AvatarJointPoseWithID other && Equals(other);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine((int)XRHandJointID, JointLocalPose);
        }
    }
}
