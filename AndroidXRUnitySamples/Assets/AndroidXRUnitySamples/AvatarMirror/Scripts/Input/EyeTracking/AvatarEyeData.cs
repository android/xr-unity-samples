// <copyright file="AvatarEyeData.cs" company="Google LLC">
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
    /// Represents eye tracking data for an avatar.
    /// </summary>
    [Serializable]
    public class AvatarEyeData : IEquatable<AvatarEyeData>
    {
        /// <summary>The position of the gaze origin in local space.</summary>
        public Vector3 GazePosition;

        /// <summary>The rotation of the combined gaze.</summary>
        public Quaternion GazeRotation;

        /// <summary>The rotation of the left eye.</summary>
        public Quaternion LeftEyeRotation;

        /// <summary>The rotation of the right eye.</summary>
        public Quaternion RightEyeRotation;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvatarEyeData"/> class.
        /// </summary>
        public AvatarEyeData()
        {
            GazePosition = new Vector3();
            GazeRotation = Quaternion.identity;
            LeftEyeRotation = Quaternion.identity;
            RightEyeRotation = Quaternion.identity;
        }

        /// <summary>The normalized direction of the combined gaze.</summary>
        public Vector3 GazeDirection => (GazeRotation * Vector3.forward).normalized;

        /// <summary>The normalized direction of the left eye.</summary>
        public Vector3 LeftEyeDirection => (LeftEyeRotation * Vector3.forward).normalized;

        /// <summary>The normalized direction of the right eye.</summary>
        public Vector3 RightEyeDirection => (RightEyeRotation * Vector3.forward).normalized;

        /// <summary>
        /// Indicates whether the current object is equal to another.
        /// </summary>
        /// <param name="data">An object to compare with this object.</param>
        /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(AvatarEyeData data)
        {
            if (data == null)
            {
                return false;
            }

            if (!GazePosition.Equals(data.GazePosition))
            {
                return false;
            }

            if (!GazeRotation.Equals(data.GazeRotation))
            {
                return false;
            }

            if (!LeftEyeRotation.Equals(data.LeftEyeRotation))
            {
                return false;
            }

            if (!RightEyeRotation.Equals(data.RightEyeRotation))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see
        /// cref="AvatarEyeData"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as AvatarEyeData);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(GazePosition, GazeRotation, LeftEyeRotation, RightEyeRotation);
        }

        /// <summary>
        /// Creates a shallow copy of the <see cref="AvatarEyeData"/> object.
        /// </summary>
        /// <returns>A shallow copy of the object.</returns>
        public AvatarEyeData ShallowCopy()
        {
            return (AvatarEyeData)MemberwiseClone();
        }

        /// <summary>
        /// Creates a deep copy of the <see cref="AvatarEyeData"/> object.
        /// </summary>
        /// <returns>A deep copy of the object.</returns>
        public AvatarEyeData DeepCopy()
        {
            return (AvatarEyeData)MemberwiseClone();
        }
    }
}
