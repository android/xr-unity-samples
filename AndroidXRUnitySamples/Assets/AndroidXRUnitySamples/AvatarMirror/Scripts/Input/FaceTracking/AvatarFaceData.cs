// <copyright file="AvatarFaceData.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// Represents face tracking blend shape data for an avatar.
    /// </summary>
    [Serializable]
    public class AvatarFaceData : IEquatable<AvatarFaceData>
    {
        /// <summary>The number of OpenXR face blend shapes.</summary>
        public static int OpenXRFaceBlendshapeCount =
            Enum.GetNames(typeof(XRFaceParameterIndices)).Count();

        /// <summary>The array of blend shape parameter values.</summary>
        public float[] Parameters = new float[OpenXRFaceBlendshapeCount];

        /// <summary>
        /// Initializes a new instance of the <see cref="AvatarFaceData"/> class.
        /// </summary>
        public AvatarFaceData()
        {
            Parameters = new float[OpenXRFaceBlendshapeCount];
        }

        /// <summary>
        /// Indicates whether the current object is equal to another.
        /// </summary>
        /// <param name="data">An object to compare with this object.</param>
        /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
        public bool Equals(AvatarFaceData data)
        {
            if (data == null)
            {
                return false;
            }

            return Parameters.SequenceEqual(data.Parameters);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see
        /// cref="AvatarFaceData"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as AvatarFaceData);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            // Combine hash codes of all relevant fields.
            // For an array, we can combine hashes of a few key elements or a computed hash.
            // Using a simple hash for the first few elements to avoid iterating the whole array
            // every time, which might be slow if the array is large.
            // If more robust hash is needed for `Parameters` content, a custom hash calculation
            // (e.g., CRC or hashing a subset) would be required.
            return HashCode.Combine(Parameters.Length,
                                    Parameters.Length > 0 ? Parameters[0].GetHashCode() : 0);
        }

        /// <summary>
        /// Creates a shallow copy of the <see cref="AvatarFaceData"/> object.
        /// </summary>
        /// <returns>A shallow copy of the object.</returns>
        public AvatarFaceData ShallowCopy()
        {
            return (AvatarFaceData)MemberwiseClone();
        }

        /// <summary>
        /// Creates a deep copy of the <see cref="AvatarFaceData"/> object.
        /// </summary>
        /// <returns>A deep copy of the object.</returns>
        public AvatarFaceData DeepCopy()
        {
            AvatarFaceData faceData = new AvatarFaceData();
            Parameters.CopyTo(faceData.Parameters, 0);
            return faceData;
        }
    }
}
