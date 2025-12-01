// <copyright file="AvatarHandTracking.cs" company="Google LLC">
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

using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// Manages hand tracking data from <see cref="XRHandSkeletonDriver"/> components
    /// and provides it as <see cref="AvatarHandsData"/>.
    /// </summary>
    public class AvatarHandTracking : MonoBehaviour
    {
        [Tooltip("The XRHandSkeletonDriver component for the left hand.")]
        [SerializeField] private XRHandSkeletonDriver _lfSkeletonDriver;

        [Tooltip("The XRHandSkeletonDriver component for the right hand.")]
        [SerializeField] private XRHandSkeletonDriver _rtSkeletonDriver;

        /// <summary>The current hand tracking data.</summary>
        private AvatarHandsData _avatarHandsData = new AvatarHandsData();

        /// <summary>The number of joints in the hand skeleton.</summary>
        private int _jointCount;

        /// <summary>Gets the current hand tracking data.</summary>
        public AvatarHandsData HandData => _avatarHandsData;

        /// <summary>Gets or sets the <see cref="XRHandSkeletonDriver"/> for the left
        /// hand.</summary>
        public XRHandSkeletonDriver LfSkeletonDriver
        {
            get => _lfSkeletonDriver;
            set => _lfSkeletonDriver = value;
        }

        /// <summary>Gets or sets the <see cref="XRHandSkeletonDriver"/> for the right
        /// hand.</summary>
        public XRHandSkeletonDriver RtSkeletonDriver
        {
            get => _rtSkeletonDriver;
            set => _rtSkeletonDriver = value;
        }

        /// <summary>
        /// Populates an <see cref="AvatarJointPoseWithID"/> array from a list of <see
        /// cref="JointToTransformReference"/>s.
        /// </summary>
        /// <param name="jointTransformReferences">The list of <see
        /// cref="JointToTransformReference"/> to extract data from.</param> <param
        /// name="avatarJointPosesWithIDs">The <see cref="AvatarJointPoseWithID"/> array to
        /// populate.</param> <param name="jointCount">The number of joints to process.</param>
        public static void HandDataFromJointToTransformReferences(
            List<JointToTransformReference> jointTransformReferences,
            AvatarJointPoseWithID[] avatarJointPosesWithIDs, int jointCount)
        {
            for (int i = 0; i < jointCount; i++)
            {
                avatarJointPosesWithIDs[i].XRHandJointID =
                    jointTransformReferences[i].xrHandJointID;
                avatarJointPosesWithIDs[i].JointLocalPose =
                    jointTransformReferences[i].jointTransform.GetLocalPose();
            }
        }

        /// <summary>
        /// Called before the first frame update.
        /// Initializes joint counts and validates skeleton configurations.
        /// </summary>
        private void Start()
        {
#if UNITY_EDITOR
            Debug.Log("Class AvatarHandTracking cannot access hand tracking data from editor. " +
                      "This script will reproduce the hand bones motion.");
#endif

            if (_lfSkeletonDriver == null || _rtSkeletonDriver == null)
            {
                Debug.LogError("XRHandSkeletonDriver for left and/or right hand is not assigned. " +
                                   "Disabling script.",
                               this);
                enabled = false;
                return;
            }

            int lfJointCount = _lfSkeletonDriver.jointTransformReferences.Count;
            int rtJointCount = _rtSkeletonDriver.jointTransformReferences.Count;

            if (lfJointCount != AvatarHandData.OpenXRHandBoneCount ||
                rtJointCount != AvatarHandData.OpenXRHandBoneCount)
            {
                Debug.LogError(
                    $"Left and/or Right hand skeletons have different joint counts from the " +
                    $"OpenXR standard ({AvatarHandData.OpenXRHandBoneCount})" +
                    $"This script will be deactivated.");
                enabled = false;
                return;
            }

            _jointCount = lfJointCount;
            _avatarHandsData = new AvatarHandsData(
                _jointCount);  // Pass _jointCount, though constructor currently doesn't use it.
        }

        /// <summary>
        /// Called once per frame.
        /// Updates the hand data from the skeleton drivers.
        /// </summary>
        private void Update()
        {
            // Update left hand data
            HandDataFromJointToTransformReferences(_lfSkeletonDriver.jointTransformReferences,
                                                   _avatarHandsData.LfHandData.JointPosesWithID,
                                                   _jointCount);
            _avatarHandsData.LfHandData.RootPose = _lfSkeletonDriver.rootTransform.GetLocalPose();

            // Update right hand data
            HandDataFromJointToTransformReferences(_rtSkeletonDriver.jointTransformReferences,
                                                   _avatarHandsData.RtHandData.JointPosesWithID,
                                                   _jointCount);
            _avatarHandsData.RtHandData.RootPose = _rtSkeletonDriver.rootTransform.GetLocalPose();
        }
    }
}
