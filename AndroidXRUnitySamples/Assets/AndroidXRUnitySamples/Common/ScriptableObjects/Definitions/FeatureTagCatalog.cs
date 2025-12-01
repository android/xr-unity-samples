// <copyright file="FeatureTagCatalog.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// General feature tag container.
    /// </summary>
    [CreateAssetMenu(menuName = "AndroidXRUnitySamples/FeatureTagCatalog")]
    public class FeatureTagCatalog : ScriptableObject
    {
        /// <summary>
        /// Feature tag definitions.
        /// </summary>
        public FeatureTag[] Tags;

        /// <summary>
        /// Defines the master list of feature types.
        /// The catalog array will be forced to match this list.
        /// Only add to the end of this list or it'll break ExperienceSettings resources.
        /// </summary>
        public enum Feature
        {
            /// <summary> Body tracking feature. </summary>
            BodyTracking,

            /// <summary> Face tracking feature. </summary>
            FaceTracking,

            /// <summary> Eye tracking feature. </summary>
            EyeTracking,

            /// <summary> Keyboard tracking feature. </summary>
            KeyboardTracking,

            /// <summary> Scene mesh feature. </summary>
            SceneMesh,

            /// <summary> Controller support feature. </summary>
            ControllerSupport,

            /// <summary> Gemini feature. </summary>
            Gemini,

            /// <summary> Hand gestures feature. </summary>
            HandGestures,

            /// <summary> Hand tracking feature. </summary>
            HandTracking,

            /// <summary> Marker tracking feature. </summary>
            MarkerTracking,

            /// <summary> Passthrough feature. </summary>
            Passthrough,

            /// <summary> Plane tracking feature. </summary>
            PlaneTracking,

            /// <summary> QR code reading feature. </summary>
            QrCode,

            /// <summary> Depth texture feature. </summary>
            DepthTexture
        }

        /// <summary>
        /// Ensures there's exactly one FeatureTag for each element in the enum.
        /// </summary>
        private void OnValidate()
        {
            // Get all the names from the FeatureType enum
            string[] enumNames = Enum.GetNames(typeof(Feature));
            int enumCount = enumNames.Length;

            // If the array is null or the length doesn't match, we need to fix it.
            if (Tags == null || Tags.Length != enumCount)
            {
                // Create a new array of the correct size
                FeatureTag[] newTags = new FeatureTag[enumCount];

                int itemsToCopy = 0;
                if (Tags != null)
                {
                    itemsToCopy = Mathf.Min(Tags.Length, enumCount);
                }

                for (int i = 0; i < itemsToCopy; i++)
                {
                    newTags[i] = Tags[i];
                }

                for (int i = itemsToCopy; i < enumCount; i++)
                {
                    newTags[i] = new FeatureTag
                    {
                        FeatureName = enumNames[i]
                    };
                }

                // Assign the new, correct-sized array
                Tags = newTags;
            }
        }
    }
}
