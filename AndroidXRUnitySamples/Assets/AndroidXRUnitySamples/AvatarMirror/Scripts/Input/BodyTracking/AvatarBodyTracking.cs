// <copyright file="AvatarBodyTracking.cs" company="Google LLC">
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
using UnityEngine.XR.ARFoundation;

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// Manages human body tracking data from <see cref="ARHumanBodyManager"/> and provides it as
    /// <see cref="AvatarBodyData"/>.
    /// </summary>
    public class AvatarBodyTracking : MonoBehaviour
    {
        [Tooltip("The Transform that defines the avatar's overall reference frame.")]
        [SerializeField] private Transform _reference;

        [Tooltip("The ARHumanBodyManager component for body tracking.")]
        [SerializeField] private ARHumanBodyManager _bodyManager;

        /// <summary>The current body data.</summary>
        private AvatarBodyData _avatarBodyData = new AvatarBodyData();

        /// <summary>Gets the current body data.</summary>
        public AvatarBodyData BodyData => _avatarBodyData;

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Deactivates the component in the editor, as body tracking requires a device.
        /// </summary>
        private void Start()
        {
#if UNITY_EDITOR
            Debug.Log("Class AvatarBodyTracking is designed for device runtime and will be " +
                      "deactivated in the editor.");
            enabled = false;
#endif
        }

        /// <summary>
        /// Called once per frame.
        /// Extracts joint data from tracked human bodies and populates the <see
        /// cref="_avatarBodyData"/> object.
        /// </summary>
        private void Update()
        {
            ARHumanBodyManager bodyManager = Singleton.Instance.OriginManager.ARHumanBodyManager;

            foreach (ARHumanBody body in bodyManager.trackables)
            {
                if (body != null)
                {
                    for (int i = 0; i < AvatarBodyData.OpenXRBodyBoneCount; i++)
                    {
                        _avatarBodyData.Joints[i].ParentIndex = body.joints[i].parentIndex;
                        _avatarBodyData.Joints[i].LocalScale = body.joints[i].localScale;
                        _avatarBodyData.Joints[i].LocalPose = body.joints[i].localPose;
                        _avatarBodyData.Joints[i].AnchorScale = body.joints[i].anchorScale;
                        _avatarBodyData.Joints[i].AnchorPose =
                            body.joints[i].anchorPose.InverseTransformedBy(_reference);
                        _avatarBodyData.Joints[i].Tracked = body.joints[i].tracked;
                        _avatarBodyData.Joints[i].ID =
                            XRAvatarSkeletonJointIDUtility.FromIndex(body.joints[i].index);
                    }

                    _avatarBodyData.EstimatedHeightScaleFactor = body.estimatedHeightScaleFactor;
                }

                break;
            }
        }
    }
}
