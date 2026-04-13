// <copyright file="AvatarBodyData.cs" company="Google LLC">
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
    /// Represents body tracking data for an avatar.
    /// </summary>
    [Serializable]
    public class AvatarBodyData : IEquatable<AvatarBodyData>
    {
        /// <summary>The total number of OpenXR body bones defined in the skeleton
        /// standard.</summary>
        public static int OpenXRBodyBoneCount =
            XRBodyJointSetUtility.JointCount(typeof(XRUpperBodyJointID));

        /// <summary>
        /// An estimated scale factor representing the height of the tracked body.
        /// </summary>
        public float EstimatedHeightScaleFactor = 1.5f;

        /// <summary>
        /// An array containing the pose data for each individual body joint in the skeleton.
        /// </summary>
        public AvatarBodyJointPoseWithID[] Joints =
            new AvatarBodyJointPoseWithID[OpenXRBodyBoneCount];

        /// <summary>
        /// Creates a shallow copy of the current <see cref="AvatarBodyData"/> instance.
        /// </summary>
        /// <returns>A shallow copy of the <see cref="AvatarBodyData"/> object.</returns>
        public AvatarBodyData ShallowCopy()
        {
            return (AvatarBodyData)MemberwiseClone();
        }

        /// <summary>
        /// Creates a deep copy of the current <see cref="AvatarBodyData"/> instance.
        /// </summary>
        /// <returns>A deep copy of the <see cref="AvatarBodyData"/> object.</returns>
        public AvatarBodyData DeepCopy()
        {
            AvatarBodyData bodyData = new AvatarBodyData();
            Array.Copy(Joints, bodyData.Joints, Joints.Length);
            bodyData.EstimatedHeightScaleFactor = EstimatedHeightScaleFactor;
            return bodyData;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="data">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="data"/>
        /// parameter; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(AvatarBodyData data)
        {
            if (data == null)
            {
                return false;
            }

            if (Mathf.Abs(EstimatedHeightScaleFactor - data.EstimatedHeightScaleFactor) >
                float.Epsilon)
            {
                return false;
            }

            if (!Joints.SequenceEqual(data.Joints))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see
        /// cref="AvatarBodyData"/>.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with the current <see
        /// cref="AvatarBodyData"/>.</param> <returns><see langword="true"/> if the specified <see
        /// cref="object"/> is equal to the current <see cref="AvatarBodyData"/>; otherwise, <see
        /// langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as AvatarBodyData);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(EstimatedHeightScaleFactor,
                                    Joints.Length > 0 ? Joints[0].GetHashCode() : 0);
        }
    }
}
