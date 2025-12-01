// <copyright file="IAvatarInput.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// Interface for providing avatar input data (head, eye, face, hand, and body).
    /// </summary>
    public interface IAvatarInput
    {
        /// <summary>Gets or sets the head tracking data.</summary>
        public static AvatarHeadData HeadData { get; set; }

        /// <summary>Gets or sets the eye tracking data.</summary>
        public static AvatarEyeData EyeData { get; set; }

        /// <summary>Gets or sets the face tracking data.</summary>
        public static AvatarFaceData FaceData { get; set; }

        /// <summary>Gets or sets the hand tracking data.</summary>
        public static AvatarHandsData HandData { get; set; }

        /// <summary>Gets or sets the body tracking data.</summary>
        public static AvatarBodyData BodyData { get; set; }
    }
}
