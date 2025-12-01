// <copyright file="AvatarHandData.cs" company="Google LLC">
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
    /// Represents hand tracking data for a single hand.
    /// </summary>
    [Serializable]
    public class AvatarHandData : IEquatable<AvatarHandData>
    {
        /// <summary>The number of OpenXR hand bones.</summary>
        public static int OpenXRHandBoneCount = 26;

        /// <summary>The root pose of the hand in local space.</summary>
        public Pose RootPose;

        /// <summary>The array of joint poses with their respective IDs.</summary>
        public AvatarJointPoseWithID[] JointPosesWithID;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvatarHandData"/> class.
        /// </summary>
        public AvatarHandData()
        {
            RootPose = new Pose();
            JointPosesWithID = new AvatarJointPoseWithID[OpenXRHandBoneCount];
        }

        /// <summary>
        /// Indicates whether the current object is equal to another.
        /// </summary>
        /// <param name="data">An object to compare with this object.</param>
        /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(AvatarHandData data)
        {
            if (data == null)
            {
                return false;
            }

            if (!RootPose.Equals(data.RootPose))
            {
                return false;
            }

            if (!JointPosesWithID.SequenceEqual(data.JointPosesWithID))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see
        /// cref="AvatarHandData"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as AvatarHandData);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            // Combine hash codes, considering the array.
            // For arrays, a common approach is to combine the array's length and a hash of a few
            // elements.
            int hash = JointPosesWithID.Length > 0 ? JointPosesWithID[0].GetHashCode() : 0;

            return HashCode.Combine(RootPose, JointPosesWithID.Length, hash);
        }

        /// <summary>
        /// Creates a shallow copy of the <see cref="AvatarHandData"/> object.
        /// </summary>
        /// <returns>A shallow copy of the object.</returns>
        public AvatarHandData ShallowCopy()
        {
            return (AvatarHandData)MemberwiseClone();
        }

        /// <summary>
        /// Creates a deep copy of the <see cref="AvatarHandData"/> object.
        /// </summary>
        /// <returns>A deep copy of the object.</returns>
        public AvatarHandData DeepCopy()
        {
            AvatarHandData handData = new AvatarHandData();

            // Deep copy the array itself, not just the reference
            handData.JointPosesWithID = new AvatarJointPoseWithID[JointPosesWithID.Length];
            Array.Copy(JointPosesWithID, handData.JointPosesWithID, JointPosesWithID.Length);
            handData.RootPose = RootPose;
            return handData;
        }
    }

    /// <summary>
    /// Represents hand tracking data for both hands of an avatar.
    /// </summary>
    [Serializable]
    public class AvatarHandsData : IEquatable<AvatarHandsData>
    {
        /// <summary>The left hand data.</summary>
        public AvatarHandData LfHandData;

        /// <summary>The right hand data.</summary>
        public AvatarHandData RtHandData;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvatarHandsData"/> class.
        /// </summary>
        public AvatarHandsData()
        {
            LfHandData = new AvatarHandData();
            RtHandData = new AvatarHandData();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AvatarHandsData"/> class.
        /// </summary>
        /// <param name="n">This parameter is currently unused.</param>
        public AvatarHandsData(int n) : this()  // Call default constructor
        {
            // The parameter 'n' is not used in the existing logic.
        }

        /// <summary>
        /// Indicates whether the current object is equal to another.
        /// </summary>
        /// <param name="data">An object to compare with this object.</param>
        /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(AvatarHandsData data)
        {
            if (data == null)
            {
                return false;
            }

            if (!LfHandData.Equals(data.LfHandData))
            {
                return false;
            }

            if (!RtHandData.Equals(data.RtHandData))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see
        /// cref="AvatarHandsData"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as AvatarHandsData);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(LfHandData, RtHandData);
        }

        /// <summary>
        /// Creates a shallow copy of the <see cref="AvatarHandsData"/> object.
        /// </summary>
        /// <returns>A shallow copy of the object.</returns>
        public AvatarHandsData ShallowCopy()
        {
            return (AvatarHandsData)MemberwiseClone();
        }

        /// <summary>
        /// Creates a deep copy of the <see cref="AvatarHandsData"/> object.
        /// </summary>
        /// <returns>A deep copy of the object.</returns>
        public AvatarHandsData DeepCopy()
        {
            AvatarHandsData handsData = new AvatarHandsData();
            handsData.LfHandData = LfHandData.DeepCopy();
            handsData.RtHandData = RtHandData.DeepCopy();
            return handsData;
        }
    }
}
