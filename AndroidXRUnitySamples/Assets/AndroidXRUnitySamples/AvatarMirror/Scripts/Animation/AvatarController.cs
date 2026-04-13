// <copyright file="AvatarController.cs" company="Google LLC">
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
using System.Collections.Generic;
using Google.XR.Extensions;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Hands;

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// Manages the animation and input for an avatar, controlling its head, eyes, face, hands, and
    /// body based on selected input sources and animation settings.
    /// </summary>
    public class AvatarController : MonoBehaviour
    {
        [Tooltip("Reference to the AvatarAnimationSettings script that holds animation and input " +
                 "configuration.")]
        [SerializeField] internal AvatarAnimationSettings _settings;

        /// <summary>The offset controller used to manipulate the avatar's position and
        /// rotation.</summary>
        [SerializeField] internal OffsetController _offsetController;

        [Tooltip("The GameObject whose position and rotation determine the overall placement of " +
                 "the avatar.")]
        [SerializeField] internal GameObject _avatarPlacement;

        [Tooltip("The root transform of the avatar hierarchy. All avatar animations are relative " +
                 "to this transform.")]
        [SerializeField] internal GameObject _avatarRoot;

        [Tooltip("The GameObject representing the avatar's left eye.")]
        [SerializeField] internal GameObject _lfEye;

        [Tooltip("The GameObject representing the avatar's right eye.")]
        [SerializeField] internal GameObject _rtEye;

        [Tooltip("The SkinnedMeshRenderer component for the avatar's mesh, used for blendshape " +
                 "animation.")]
        [SerializeField]
        internal SkinnedMeshRenderer _mainMesh;

        [Tooltip("The root transform of the left avatar hand's skeletal hierarchy.")]
        [SerializeField] internal Transform _lfAvatarHandRoot;

        [Tooltip("The root transform of the right avatar hand's skeletal hierarchy.")]
        [SerializeField] internal Transform _rtAvatarHandRoot;

        [Tooltip("A list of mappings for the left hand's joint IDs to their corresponding " +
                 "Transforms on the avatar.")]
        [SerializeField] internal List<AvatarJointIDToTransform> _lfAvatarJointIDToTransforms;

        [Tooltip("A list of mappings for the right hand's joint IDs to their corresponding " +
                 "Transforms on the avatar.")]
        [SerializeField] internal List<AvatarJointIDToTransform> _rtAvatarJointIDToTransforms;

        [Tooltip("A list of mappings for the avatar's body joint IDs to their corresponding " +
                 "Transforms.")]
        [SerializeField] internal List<AvatarBodyJointIDToTransform> _avatarBodyJointIDToTransforms;

        [Tooltip("The transform representing the root of the avatar's skeletal rig.")]
        [SerializeField] internal Transform _avatarRig;

        private static readonly Quaternion _bodyTrackingOffsetRot =
                Quaternion.Euler(0f, 180f, 0f);

        /// <summary>A Dictionary mapping <see cref="XRAvatarSkeletonJointID"/> to their <see
        /// cref="Transform"/> for the avatar's body.</summary>
        private Dictionary<XRUpperBodyJointID, Transform> _avatarBodyJointIDToTransformsDict =
            new Dictionary<XRUpperBodyJointID, Transform>();

        /// <summary>A Dictionary mapping <see cref="XRHandJointID"/> to their <see
        /// cref="Transform"/> for the left hand.</summary>
        private Dictionary<XRHandJointID, Transform> _lfAvatarJointIDToTransformsDict =
            new Dictionary<XRHandJointID, Transform>();

        /// <summary>A Dictionary mapping <see cref="XRHandJointID"/> to their <see
        /// cref="Transform"/> for the right hand.</summary>
        private Dictionary<XRHandJointID, Transform> _rtAvatarJointIDToTransformsDict =
            new Dictionary<XRHandJointID, Transform>();

        /// <summary>Data containing the avatar's head pose (position and rotation).</summary>
        private AvatarHeadData _headData;

        /// <summary>Data containing the avatar's eye gaze direction.</summary>
        private AvatarEyeData _eyeData;

        /// <summary>Data containing the avatar's face blendshape parameters.</summary>
        private AvatarFaceData _faceData;

        /// <summary>Data containing the poses for both left and right hands.</summary>
        private AvatarHandsData _handsData;

        /// <summary>Data containing the poses for the avatar's body joints.</summary>
        private AvatarBodyData _bodyData;

        /// <summary>The starting local rotation of the right eye.</summary>
        private Quaternion _rtEyeStart;

        /// <summary>The starting local rotation of the left eye.</summary>
        private Quaternion _lfEyeStart;

        /// <summary>The starting local rotation of the avatar's root.</summary>
        private Quaternion _rootStart;

        // Note: _headtwist and _headswing are declared but not used in the provided script.
        private Quaternion _headtwist;
        private Quaternion _headswing;

        /// <summary>Reference to the scene's main camera.</summary>
        private Camera _mainCamera;

        /// <summary>Exponential filter for smoothing the left eye's rotation.</summary>
        private ExponentialFilter _lfEyeRotExponentialFilter;

        /// <summary>Exponential filter for smoothing the right eye's rotation.</summary>
        private ExponentialFilter _rtEyeRotExponentialFilter;

        /// <summary>Array of exponential filters for smoothing face blendshape
        /// parameters.</summary>
        private ExponentialFilter[] _faceExponentialFilters;

        /// <summary>Array of exponential filters for smoothing left hand finger
        /// positions.</summary>
        private ExponentialFilter[] _lfFingerPosExponentialFilter;

        /// <summary>Array of exponential filters for smoothing left hand finger
        /// rotations.</summary>
        private ExponentialFilter[] _lfFingerRotExponentialFilter;

        /// <summary>Array of exponential filters for smoothing right hand finger
        /// positions.</summary>
        private ExponentialFilter[] _rtFingerPosExponentialFilter;

        /// <summary>Array of exponential filters for smoothing right hand finger
        /// rotations.</summary>
        private ExponentialFilter[] _rtFingerRotExponentialFilter;

        /// <summary>
        /// Calls a utility method to set up a new avatar.
        /// </summary>
        public void SetupNewAvatar()
        {
            AvatarControllerUtility.SetupNewAvatar(this, _avatarRig);
        }

        /// <summary>
        /// Switches a Unity <see cref="Quaternion"/> between Left-Handed System (LHS) and
        /// Right-Handed System (RHS).
        /// </summary>
        /// <param name="quaternion">The Unity Quaternion to mirror.</param>
        /// <param name="axis">The coordinate axis to invert (0 for X, 1 for Y, 2 for Z). Default is
        /// X (0).</param> <returns>The mirrored Unity Quaternion.</returns>
        private static Quaternion Mirror(Quaternion quaternion, int axis = 0)
        {
            Quaternion newQuaternion = quaternion;
            newQuaternion.w = -newQuaternion.w;
            newQuaternion[axis] = -newQuaternion[axis];
            return newQuaternion;
        }

        /// <summary>
        /// Called when the script is loaded or a value is changed in the Inspector.
        /// This is used to set up or update the exponential filters for animation smoothing.
        /// </summary>
        private void OnValidate()
        {
            SetupFilters();
        }

        /// <summary>
        /// Initializes the avatar controller.
        /// </summary>
        private void Start()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                Debug.LogError(
                    "AvatarController requires a main camera (tagged 'MainCamera') in the scene.",
                    this);
                enabled = false;
                return;
            }

            _rtEyeStart = _rtEye.transform.localRotation;
            _lfEyeStart = _lfEye.transform.localRotation;
            _rootStart = _avatarRoot.transform.localRotation;

            InitializeJointDictionaries();
            SetupFilters();
        }

        /// <summary>
        /// Called once per frame to update the avatar's animation.
        /// </summary>
        private void Update()
        {
            _eyeData = AvatarInputPerception.EyeData;
            _faceData = AvatarInputPerception.FaceData;
            _handsData = AvatarInputPerception.HandData;
            _headData = AvatarInputPerception.HeadData;
            _bodyData = AvatarInputPerception.BodyData;

            if (_eyeData == null)
            {
                Debug.LogWarning("Avatar EyeData is null. Check your input system.");
                return;
            }

            if (_faceData == null)
            {
                Debug.LogWarning("Avatar FaceData is null. Check your input system.");
                return;
            }

            if (_handsData == null)
            {
                Debug.LogWarning("Avatar HandData is null. Check your input system.");
                return;
            }

            if (_headData == null)
            {
                Debug.LogWarning("Avatar HeadData is null. Check your input system.");
                return;
            }

            if (_bodyData == null)
            {
                Debug.LogWarning("Avatar BodyData is null. Check your input system.");
                return;
            }

            _lfEye.transform.localRotation = _lfEyeRotExponentialFilter.Filter(
                Quaternion.LookRotation(_eyeData.GazeDirection) * _lfEyeStart);
            _rtEye.transform.localRotation = _rtEyeRotExponentialFilter.Filter(
                Quaternion.LookRotation(_eyeData.GazeDirection) * _rtEyeStart);

            for (int i = 0; i < _faceData.Parameters.Length; i++)
            {
                if (i < _mainMesh.sharedMesh.blendShapeCount)
                {
                    SetAnimationBlendshapes(
                        i, _faceExponentialFilters[i].Filter(_faceData.Parameters[i] * 100));
                }
            }

            UpdateHand(_handsData.LfHandData, _lfAvatarJointIDToTransformsDict,
                       _lfFingerPosExponentialFilter, _lfFingerRotExponentialFilter);

            UpdateHand(_handsData.RtHandData, _rtAvatarJointIDToTransformsDict,
                       _rtFingerPosExponentialFilter, _rtFingerRotExponentialFilter);

            if (!_settings.UseBodyTracking)
            {
                return;
            }

            for (int i = 0; i < AvatarBodyData.OpenXRBodyBoneCount; i++)
            {
                AvatarBodyJointPoseWithID bodyJointPoseWithID = _bodyData.Joints[i];

                if (_avatarBodyJointIDToTransformsDict.ContainsKey(bodyJointPoseWithID.ID))
                {
                    Transform jointTransform =
                        _avatarBodyJointIDToTransformsDict[bodyJointPoseWithID.ID];

                    if (bodyJointPoseWithID.ID == XRUpperBodyJointID.Hips)
                    {
                        Vector3 posTarget = bodyJointPoseWithID.AnchorPose
                                                .GetTransformedBy(_settings.AvatarOrigin.transform)
                                                .position;

                        if (_settings.AllowRelativeMotion)
                        {
                            _avatarRoot.transform.position = posTarget;
                        }
                        else
                        {
                            _avatarRoot.transform.position = new Vector3(
                                _settings.AvatarOrigin.transform.position.x, posTarget.y,
                                _settings.AvatarOrigin.transform.position.z);
                        }
                    }

                    Quaternion offset = Quaternion.identity;
                    if (_offsetController != null)
                    {
                        offset = _offsetController._rotationOffset;
                    }

                    bodyJointPoseWithID.AnchorPose.rotation =
                        bodyJointPoseWithID.AnchorPose.rotation.normalized;
                    var anchorRotCorrected = _settings.AvatarOrigin.transform.rotation * offset *
                                             _bodyTrackingOffsetRot *
                                             bodyJointPoseWithID.AnchorPose.rotation;

                    if (!_settings.BodyOnlyRotation)
                    {
                        jointTransform.position = bodyJointPoseWithID.AnchorPose.position;
                    }

                    if (_settings.FlipBody)
                    {
                        jointTransform.rotation =
                            Mirror(anchorRotCorrected, _settings.FlipBodyAxis);
                    }
                    else
                    {
                        jointTransform.rotation = anchorRotCorrected;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the blendshape weight for the avatar's face mesh.
        /// </summary>
        /// <param name="index">The index of the face parameter from the input data.</param>
        /// <param name="weight">The weight (0-100) to apply to the blendshape.</param>
        private void SetAnimationBlendshapes(int index, float weight)
        {
            _mainMesh.SetBlendShapeWeight(index, weight);
        }

        /// <summary>
        /// Updates the animation of a single hand.
        /// </summary>
        /// <param name="handData">The <see cref="AvatarHandData"/> for the hand to update.</param>
        /// <param name="jointTransforms">A dictionary mapping <see cref="XRHandJointID"/> to their
        /// target <see cref="Transform"/>.</param> <param name="fingerPosFilters">Array of <see
        /// cref="ExponentialFilter"/> for smoothing finger positions.</param> <param
        /// name="fingerRotFilters">Array of <see cref="ExponentialFilter"/> for smoothing finger
        /// rotations.</param>
        private void UpdateHand(AvatarHandData handData,
                                Dictionary<XRHandJointID, Transform> jointTransforms,
                                ExponentialFilter[] fingerPosFilters,
                                ExponentialFilter[] fingerRotFilters)
        {
            for (int i = 0; i < AvatarHandData.OpenXRHandBoneCount; i++)
            {
                AvatarJointPoseWithID jointPoseWithID = handData.JointPosesWithID[i];

                if (_settings.SkipMetacarpals &&
                    (jointPoseWithID.XRHandJointID == XRHandJointID.IndexMetacarpal ||
                     jointPoseWithID.XRHandJointID == XRHandJointID.MiddleMetacarpal ||
                     jointPoseWithID.XRHandJointID == XRHandJointID.RingMetacarpal ||
                     jointPoseWithID.XRHandJointID == XRHandJointID.LittleMetacarpal))
                {
                    continue;
                }

                if (jointTransforms.TryGetValue(jointPoseWithID.XRHandJointID,
                                                out Transform jointTransform))
                {
                    if (_settings.HandOnlyRotation)
                    {
                        jointTransform.localRotation =
                            fingerRotFilters[i].Filter(jointPoseWithID.JointLocalPose.rotation);
                    }
                    else
                    {
                        jointTransform.localPosition =
                            fingerPosFilters[i].Filter(jointPoseWithID.JointLocalPose.position);
                        jointTransform.localRotation =
                            fingerRotFilters[i].Filter(jointPoseWithID.JointLocalPose.rotation);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the dictionaries that map XR Hand Joint IDs and XR Body Joint IDs
        /// to their corresponding <see cref="Transform"/> components on the avatar.
        /// </summary>
        private void InitializeJointDictionaries()
        {
            _lfAvatarJointIDToTransformsDict.Clear();
            foreach (AvatarJointIDToTransform jointIDToTransform in _lfAvatarJointIDToTransforms)
            {
                _lfAvatarJointIDToTransformsDict[jointIDToTransform.XRHandJointID] =
                    jointIDToTransform.JointTransform;
            }

            _rtAvatarJointIDToTransformsDict.Clear();
            foreach (AvatarJointIDToTransform jointIDToTransform in _rtAvatarJointIDToTransforms)
            {
                _rtAvatarJointIDToTransformsDict[jointIDToTransform.XRHandJointID] =
                    jointIDToTransform.JointTransform;
            }

            _avatarBodyJointIDToTransformsDict.Clear();
            foreach (AvatarBodyJointIDToTransform bodyJointIDToTransform in
                         _avatarBodyJointIDToTransforms)
            {
                _avatarBodyJointIDToTransformsDict[bodyJointIDToTransform.XRSkelJointID] =
                    bodyJointIDToTransform.JointTransform;
            }
        }

        /// <summary>
        /// Initializes or re-initializes the exponential filters used for smoothing animation
        /// parameters.
        /// </summary>
        private void SetupFilters()
        {
            if (_settings == null || _settings.AnimationSettings == null ||
                _settings.AnimationSettings.BlendingSettings == null)
            {
                Debug.LogWarning("AvatarController settings or blending settings are null. " +
                                     "Filters cannot be set up.",
                                 this);
                return;
            }

            float eBlendFactor = _settings.AnimationSettings.BlendingSettings.EyeBlendFactor;

            ExponentialFilterSettingsQuat lfEyeRotFilterSettings =
                new ExponentialFilterSettingsQuat(eBlendFactor);

            _lfEyeRotExponentialFilter = new ExponentialFilter(lfEyeRotFilterSettings);

            ExponentialFilterSettingsQuat rtEyeRotFilterSettings =
                new ExponentialFilterSettingsQuat(eBlendFactor);

            _rtEyeRotExponentialFilter = new ExponentialFilter(rtEyeRotFilterSettings);

            _faceExponentialFilters =
                new ExponentialFilter[AvatarFaceData.OpenXRFaceBlendshapeCount];

            float faBlendFactor =
                _settings.AnimationSettings.BlendingSettings.FaceBlendshapeBlendFactor;

            ExponentialFilterSettingsFloat faceFilterSettings =
                new ExponentialFilterSettingsFloat(faBlendFactor);

            for (int i = 0; i < _faceExponentialFilters.Length; i++)
            {
                _faceExponentialFilters[i] = new ExponentialFilter(faceFilterSettings);
            }

            float fBlendFactor = _settings.AnimationSettings.BlendingSettings.FingerBlendFactor;

            ExponentialFilterSettingsV3 fingerPosFilterSettings =
                new ExponentialFilterSettingsV3(fBlendFactor);

            _lfFingerPosExponentialFilter =
                new ExponentialFilter[AvatarHandData.OpenXRHandBoneCount];
            _lfFingerRotExponentialFilter =
                new ExponentialFilter[AvatarHandData.OpenXRHandBoneCount];
            _rtFingerPosExponentialFilter =
                new ExponentialFilter[AvatarHandData.OpenXRHandBoneCount];
            _rtFingerRotExponentialFilter =
                new ExponentialFilter[AvatarHandData.OpenXRHandBoneCount];

            for (int i = 0; i < AvatarHandData.OpenXRHandBoneCount; i++)
            {
                _lfFingerPosExponentialFilter[i] = new ExponentialFilter(fingerPosFilterSettings);
                _lfFingerRotExponentialFilter[i] = new ExponentialFilter(fingerPosFilterSettings);
                _rtFingerPosExponentialFilter[i] = new ExponentialFilter(fingerPosFilterSettings);
                _rtFingerRotExponentialFilter[i] = new ExponentialFilter(fingerPosFilterSettings);
            }
        }
    }
}
