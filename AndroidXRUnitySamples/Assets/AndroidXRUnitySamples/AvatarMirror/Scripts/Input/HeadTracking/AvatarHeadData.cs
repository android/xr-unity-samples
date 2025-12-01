// <copyright file="AvatarHeadData.cs" company="Google LLC">
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
using UnityEngine;

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// Represents head tracking data for an avatar.
    /// </summary>
    [Serializable]
    public class AvatarHeadData : IEquatable<AvatarHeadData>
    {
        /// <summary>The local pose of the head.</summary>
        public Pose LocalPose;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvatarHeadData"/> class.
        /// </summary>
        public AvatarHeadData()
        {
            LocalPose = Pose.identity;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another.
        /// </summary>
        /// <param name="data">An object to compare with this object.</param>
        /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(AvatarHeadData data)
        {
            if (data == null)
            {
                return false;
            }

            return LocalPose.Equals(data.LocalPose);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see
        /// cref="AvatarHeadData"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as AvatarHeadData);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return LocalPose.GetHashCode();
        }

        /// <summary>
        /// Creates a shallow copy of the <see cref="AvatarHeadData"/> object.
        /// </summary>
        /// <returns>A shallow copy of the object.</returns>
        public AvatarHeadData ShallowCopy()
        {
            return (AvatarHeadData)MemberwiseClone();
        }

        /// <summary>
        /// Creates a deep copy of the <see cref="AvatarHeadData"/> object.
        /// </summary>
        /// <returns>A deep copy of the object.</returns>
        public AvatarHeadData DeepCopy()
        {
            // For a class with only value type members (like Pose), MemberwiseClone performs a deep
            // copy.
            return ShallowCopy();
        }
    }
}
