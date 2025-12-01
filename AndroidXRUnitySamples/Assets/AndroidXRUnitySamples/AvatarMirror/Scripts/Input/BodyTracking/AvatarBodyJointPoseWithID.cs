// <copyright file="AvatarBodyJointPoseWithID.cs" company="Google LLC">
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
using Google.XR.Extensions;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// A struct that contains information about a single avatar body joint's pose and tracking
    /// status, along with its <see cref="XRAvatarSkeletonJointID"/>.
    /// </summary>
    [Serializable]
    public struct AvatarBodyJointPoseWithID : IEquatable<AvatarBodyJointPoseWithID>
    {
        /// <summary>The XR Avatar Skeleton Joint Identifier for this bone.</summary>
        [Tooltip("The XR Avatar Skeleton Joint Identifier for this bone.")]
        [SerializeField] public XRAvatarSkeletonJointID ID;

        /// <summary>The index of this joint's parent in the skeleton hierarchy.</summary>
        [Tooltip("The index of this joint's parent in the skeleton hierarchy.")]
        [SerializeField] public int ParentIndex;

        /// <summary>The local scale of the joint relative to its parent bone.</summary>
        public Vector3 LocalScale;

        /// <summary>The local pose (position and rotation) of the joint relative to its parent
        /// bone.</summary>
        public Pose LocalPose;

        /// <summary>The scale of the joint within the anchor's coordinate system.</summary>
        public Vector3 AnchorScale;

        /// <summary>The pose (position and rotation) of the joint within the anchor's coordinate
        /// system.</summary>
        public Pose AnchorPose;

        /// <summary>Indicates whether this specific joint is currently being tracked.</summary>
        public bool Tracked;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/>
        /// parameter; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(AvatarBodyJointPoseWithID other)
        {
            return ID == other.ID && ParentIndex == other.ParentIndex &&
                   LocalScale.Equals(other.LocalScale) && LocalPose.Equals(other.LocalPose) &&
                   AnchorScale.Equals(other.AnchorScale) && AnchorPose.Equals(other.AnchorPose) &&
                   Tracked == other.Tracked;
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="obj"/> and this instance are the same type
        /// and represent the same value; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is AvatarBodyJointPoseWithID other && Equals(other);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine((int)ID, ParentIndex, LocalScale, LocalPose, AnchorScale,
                                    AnchorPose, Tracked);
        }
    }
}
