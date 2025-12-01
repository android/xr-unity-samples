// <copyright file="AnimationRetargeting.cs" company="Google LLC">
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

using UnityEngine;
using UnityEngine.Serialization;

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// Retargets human animation from a reference <see cref="Animator"/> to a puppet <see
    /// cref="Animator"/>. This script uses <see cref="HumanPoseHandler"/> to transfer pose data
    /// between two humanoids.
    /// </summary>
    public class AnimationRetargeting : MonoBehaviour
    {
        [Tooltip("The Animator component of the avatar that will be moved and driven.")]
        [SerializeField] private Animator _puppetAvatarAnimator = null;

        [Tooltip("The Animator component of the reference rig from which animation data will be " +
                 "sourced.")]
        [SerializeField] private Animator _referenceAvatarAnimator = null;

        private HumanPoseHandler _puppetPoseHandler = null;
        private HumanPoseHandler _referencePoseHandler = null;

        [SerializeField] private int[] _fixedMuscleIndices = new int[0];
        [SerializeField] private float[] _fixedMuscleWeights = new float[0];

        private void Start()
        {
            if (_puppetAvatarAnimator == null || _puppetAvatarAnimator.avatar == null)
            {
                Debug.LogError("Missing required property: Puppet Avatar Animator or its Avatar " +
                                   "is null. This script will be disabled.",
                               this);
                enabled = false;
                return;  // Exit early if requirements are not met
            }

            if (_referenceAvatarAnimator == null || _referenceAvatarAnimator.avatar == null)
            {
                Debug.LogError("Missing required property: Reference Avatar Animator or its " +
                                   "Avatar is null. This script will be disabled.",
                               this);
                enabled = false;
                return;  // Exit early if requirements are not met
            }

            // Initialize HumanPoseHandlers with the animators' avatars and their root transforms.
            _puppetPoseHandler = new HumanPoseHandler(_puppetAvatarAnimator.avatar,
                _puppetAvatarAnimator.avatarRoot);
            _referencePoseHandler = new HumanPoseHandler(_referenceAvatarAnimator.avatar,
                _referenceAvatarAnimator.avatarRoot);

            // Disable original animators as their poses will be handled manually.
            _referenceAvatarAnimator.enabled = false;
            _puppetAvatarAnimator.enabled = false;
        }

        private void Update()
        {
            // Create a HumanPose struct to hold the pose data.
            HumanPose humanPose = new HumanPose();

            // Get the human pose from the reference avatar.
            _referencePoseHandler.GetHumanPose(ref humanPose);

            for (int i = 0; i < _fixedMuscleIndices.Length; i++)
            {
                if (_fixedMuscleIndices[i] != humanPose.muscles.Length)
                {
                    humanPose.muscles[_fixedMuscleIndices[i]] = _fixedMuscleWeights[i];
                }
                else
                {
                    Debug.LogWarning(
                        $"_fixedMuscleIndice[i]: {_fixedMuscleIndices[i]} is out of range. " +
                        humanPose.muscles.Length);
                }
            }

            // Apply the human pose to the puppet avatar.
            _puppetPoseHandler.SetHumanPose(ref humanPose);

            // Explicitly match the hips position to avoid potential discrepancies
            // that might arise from HumanPoseHandler's root motion handling.
            _puppetAvatarAnimator.GetBoneTransform(HumanBodyBones.Hips).position =
                _referenceAvatarAnimator.GetBoneTransform(HumanBodyBones.Hips).position;
        }
    }
}
