// <copyright file="StatusDashboard.cs" company="Google LLC">
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
using AndroidXRUnitySamples.Variables;
using Google.XR.Extensions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Management;

namespace AndroidXRUnitySamples.Home
{
    /// <summary>
    /// Controller for the dashboard in Home.
    /// </summary>
    public class StatusDashboard : MonoBehaviour
    {
        [SerializeField] private StatusItem _passthroughStatus;
        [SerializeField] private StatusItem _headTrackingStatus;
        [SerializeField] private StatusItem _handTrackingStatus;
        [SerializeField] private StatusItem _gesturesStatus;
        [SerializeField] private StatusItem _inputModeStatus;
        [SerializeField] private StatusItem _eyeTrackingStatus;
        [SerializeField] private StatusItem _faceSystemStatus;
        [SerializeField] private StatusItem _faceTrackingStatus;
        [SerializeField] private StatusItem _planeDetectionStatus;
        [SerializeField] private StatusItem _objectTrackingStatus;
        [SerializeField] private StatusItem _depthTextureStatus;
        [SerializeField] private StatusItem _sceneMeshStatus;

        [SerializeField] private ARFaceManager _faceManager;
        [SerializeField] private ARPlaneManager _planeManager;
        [SerializeField] private ARTrackedObjectManager _objectManager;
        [SerializeField] private XRReferenceObjectLibrary _objectReferenceLibrary;
        [SerializeField] private ARMeshManager _meshManager;
        [SerializeField] private MeshFilter _meshManagerMeshPrefab;
        [SerializeField] private InputModeVariable _inputMode;

        private InputAction _headPositionAction = new InputAction();
        private InputAction _headRotationAction = new InputAction();
        private InputAction _pinchStateLeft = new InputAction();
        private InputAction _pinchStateRight = new InputAction();
        private InputAction _etState = new InputAction();
        private bool _gesturesEverBeenReady = false;
        private bool _anyPlaneDetected = false;
        private bool _anyObjectDetected = false;
        private bool _anySceneMeshChangesDetected = false;
        private HashSet<AndroidXRPermission> _grantedPermissions =
            new HashSet<AndroidXRPermission>();

        private bool _faceBlendShapesAvailable = false;

        /// <summary>
        /// Requests the specified permissions and then calls CheckForPermissionsUpdate.
        /// </summary>
        /// <param name="permissions">The permissions to request.</param>
        public void RequestUserPermissions(AndroidXRPermission[] permissions)
        {
            var missingPermissions = new List<string>();
            foreach (var permission in permissions)
            {
                string permissionString = permission.ToPermissionString();
                if (!Permission.HasUserAuthorizedPermission(permissionString))
                {
                    missingPermissions.Add(permissionString);
                }
            }

            if (missingPermissions.Count != 0)
            {
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionDenied += permission =>
                {
                    Debug.Log("Permission denied: " + permission);
                    CheckForPermissionsUpdate();
                };

                callbacks.PermissionGranted += permission =>
                {
                    Debug.Log("Permission granted: " + permission);
                    CheckForPermissionsUpdate();
                };

                Permission.RequestUserPermissions(missingPermissions.ToArray(), callbacks);
            }
            else
            {
                CheckForPermissionsUpdate();
            }
        }

        /// <summary>
        /// Checks for all necessary permissions and enables or disables systems based on which
        /// permissions are granted.
        /// </summary>
        public void CheckForPermissionsUpdate()
        {
            _grantedPermissions.Clear();
            AndroidXRPermission[] allPermissions =
              (AndroidXRPermission[])Enum.GetValues(typeof(AndroidXRPermission));
            foreach (var permission in allPermissions)
            {
                if (Permission.HasUserAuthorizedPermission(permission.ToPermissionString()))
                {
                    _grantedPermissions.Add(permission);
                }
            }

            Singleton.Instance.OriginManager.EnableFaceManager = _grantedPermissions.Contains(
                AndroidXRPermission.EyeTrackingCoarse) && _grantedPermissions.Contains(
                AndroidXRPermission.FaceTracking);
            Singleton.Instance.OriginManager.EnablePlaneDetection = _grantedPermissions.Contains(
                AndroidXRPermission.SceneUnderstandingCoarse);
            Singleton.Instance.OriginManager.EnableObjectTracking = _grantedPermissions.Contains(
                AndroidXRPermission.SceneUnderstandingCoarse);
            Singleton.Instance.OriginManager.EnableDepthTexture = _grantedPermissions.Contains(
                AndroidXRPermission.SceneUnderstandingFine);
            Singleton.Instance.OriginManager.EnableMeshManager = _grantedPermissions.Contains(
                AndroidXRPermission.SceneUnderstandingFine);
        }

        private void OnFaceChanged(ARTrackablesChangedEventArgs<ARFace> eventArgs)
        {
            foreach (ARFace face in eventArgs.added)
            {
                face.updated += OnFaceUpdate;
            }
        }

        private void OnFaceUpdate(ARFaceUpdatedEventArgs eventArgs)
        {
            ARFace face = eventArgs.face;
            if (face.trackingState != TrackingState.Tracking)
            {
                return;
            }

            Result<NativeArray<XRFaceBlendShape>> result =
                _faceManager.TryGetBlendShapes(face, Allocator.Temp);

            // TODO: Update this once Unity has an API to check the calibration state.
            _faceBlendShapesAvailable = result.status.IsSuccess();
        }

        private void Start()
        {
            _headPositionAction.AddBinding("<XRHMD>/centerEyePosition");
            _headPositionAction.Enable();
            _headRotationAction.AddBinding("<XRHMD>/centerEyeRotation");
            _headRotationAction.Enable();

            _pinchStateLeft.AddBinding("<HandInteraction>{LeftHand}/pinchReady");
            _pinchStateLeft.Enable();
            _pinchStateRight.AddBinding("<HandInteraction>{RightHand}/pinchReady");
            _pinchStateRight.Enable();

            _etState.AddBinding("<EyeGaze>/pose/trackingState");
            _etState.Enable();

            Singleton.Instance.OriginManager.PlanePrefab = null;

            Singleton.Instance.OriginManager.ObjectTrackingReferenceLibrary =
                _objectReferenceLibrary;
            Singleton.Instance.OriginManager.ObjectTrackingObjectPrefab = null;

            Singleton.Instance.OriginManager.MeshManagerMeshPrefab = _meshManagerMeshPrefab;

            CheckForPermissionsUpdate();
            UpdateInputModeState(_inputMode.Value);
        }

        private void OnEnable()
        {
            _faceManager.trackablesChanged.AddListener(OnFaceChanged);
            _planeManager.trackablesChanged.AddListener(OnPlaneDetected);
            _objectManager.trackablesChanged.AddListener(OnObjectDetected);
            _meshManager.meshInfosChanged.AddListener(OnMeshesChanged);
            _inputMode.AddListener(UpdateInputModeState);
        }

        private void OnDisable()
        {
            _faceManager.trackablesChanged.RemoveListener(OnFaceChanged);
            _planeManager.trackablesChanged.RemoveListener(OnPlaneDetected);
            _objectManager.trackablesChanged.RemoveListener(OnObjectDetected);
            _meshManager.meshInfosChanged.RemoveListener(OnMeshesChanged);
            _inputMode.RemoveListener(UpdateInputModeState);
        }

        private void Update()
        {
            UpdatePassthroughState();
            UpdateHeadTrackingState();
            UpdateHandTrackingState();
            UpdateGesturesState();
            UpdateEyeTrackingState();
            UpdateARFaceState();
            UpdateFaceTrackingState();
            UpdatePlaneDetectionState();
            UpdateObjectTrackingState();
            UpdateDepthTextureState();
            UpdateSceneMeshState();
        }

        private void UpdatePassthroughState()
        {
            if (XRPassthroughFeature.IsExtensionEnabled == null)
            {
                _passthroughStatus.UpdateStatus("Feature not found", false);
                return;
            }
            else if (!XRPassthroughFeature.IsExtensionEnabled.Value)
            {
                _passthroughStatus.UpdateStatus("Feature not enabled", false);
                return;
            }

            XRPassthroughCameraStates state = XRPassthroughFeature.GetState();
            _passthroughStatus.UpdateStatus(state.ToString(),
                state == XRPassthroughCameraStates.Ready);
        }

        private void UpdateHeadTrackingState()
        {
            var headPosition = _headPositionAction.ReadValue<Vector3>();
            var headRotation = _headRotationAction.ReadValue<Quaternion>();

            bool nonIdentityPose =
                headPosition != Vector3.zero || headRotation != Quaternion.identity;

            _headTrackingStatus.UpdateStatus(nonIdentityPose.ToString(), nonIdentityPose);
        }

        private void UpdateHandTrackingState()
        {
            _handTrackingStatus.StartButtonActive = false;
            XRHandSubsystem system = XRGeneralSettings.Instance?.Manager?.activeLoader
                                             ?.GetLoadedSubsystem<XRHandSubsystem>();

            if (system == null)
            {
                _handTrackingStatus.UpdateStatus("Not found", false);
                return;
            }
            else if (!system.running)
            {
                _handTrackingStatus.UpdateStatus("Not running", false);
                return;
            }

            Vector3 posL = system.leftHand.rootPose.position;
            Quaternion rotL = system.leftHand.rootPose.rotation;
            Vector3 posR = system.rightHand.rootPose.position;
            Quaternion rotR = system.rightHand.rootPose.rotation;

            bool nonIdentityPose = posL != Vector3.zero || rotL != Quaternion.identity ||
                posR != Vector3.zero || rotR != Quaternion.identity;

            _handTrackingStatus.UpdateStatus(nonIdentityPose.ToString(), nonIdentityPose);
        }

        private void UpdateGesturesState()
        {
            _gesturesStatus.StartButtonActive = false;
            _gesturesEverBeenReady = _gesturesEverBeenReady ||
                (_pinchStateLeft.IsPressed() || _pinchStateRight.IsPressed());
            _gesturesStatus.UpdateStatus(_gesturesEverBeenReady.ToString(), _gesturesEverBeenReady);
        }

        private void UpdateEyeTrackingState()
        {
            _eyeTrackingStatus.StartButtonActive =
                !_grantedPermissions.Contains(AndroidXRPermission.EyeTrackingFine);
            InputTrackingState state = (InputTrackingState)_etState.ReadValue<int>();
            _eyeTrackingStatus.UpdateStatus(state.ToString(), state != InputTrackingState.None);
        }

        private void UpdateInputModeState(XRInputModalityManager.InputMode inputMode)
        {
            if (inputMode == XRInputModalityManager.InputMode.None)
            {
                _inputModeStatus.UpdateStatus("None", false);
            }
            else if (inputMode == XRInputModalityManager.InputMode.TrackedHand)
            {
                _inputModeStatus.UpdateStatus("Hands", true);
            }
            else if (inputMode == XRInputModalityManager.InputMode.MotionController)
            {
                _inputModeStatus.UpdateStatus("Controller", true);
            }
            else
            {
                _inputModeStatus.UpdateStatus("Unknown", true);
            }
        }

        private void UpdateARFaceState()
        {
            if (Application.isEditor)
            {
                _faceSystemStatus.UpdateStatus("Not supported", false);
                return;
            }

            ARFaceManager faceManager = Singleton.Instance.OriginManager.ARFaceManager;
            _faceSystemStatus.StartButtonActive = !faceManager.enabled;

            if (faceManager.trackables.count == 0)
            {
                _faceSystemStatus.UpdateStatus("No faces detected", false);
            }
            else
            {
                _faceSystemStatus.UpdateStatus("Face detected", true);
            }
        }

        private void UpdateFaceTrackingState()
        {
            if (Application.isEditor)
            {
                _faceTrackingStatus.UpdateStatus("Not supported", false);
                return;
            }

            _faceTrackingStatus.StartButtonActive = !_faceManager.enabled;

            if (_faceBlendShapesAvailable)
            {
                _faceTrackingStatus.UpdateStatus("Got Blend Shapes", true);
            }
            else
            {
                _faceTrackingStatus.UpdateStatus("No Blend Shapes", false);
            }
        }

        private void UpdatePlaneDetectionState()
        {
            _planeDetectionStatus.StartButtonActive =
                !Singleton.Instance.OriginManager.EnablePlaneDetection;
            _planeDetectionStatus.UpdateStatus(_anyPlaneDetected.ToString(), _anyPlaneDetected);
        }

        private void UpdateObjectTrackingState()
        {
            _objectTrackingStatus.StartButtonActive =
                !Singleton.Instance.OriginManager.EnableObjectTracking;
            _objectTrackingStatus.UpdateStatus(_anyObjectDetected.ToString(), _anyObjectDetected);
        }

        private void OnPlaneDetected(ARTrackablesChangedEventArgs<ARPlane> changes)
        {
            _anyPlaneDetected = _anyPlaneDetected || (changes.added.Count > 0);
        }

        private void OnObjectDetected(ARTrackablesChangedEventArgs<ARTrackedObject> changes)
        {
            _anyObjectDetected = _anyObjectDetected || (changes.added.Count > 0);
        }

        private void UpdateDepthTextureState()
        {
            _depthTextureStatus.StartButtonActive =
                !Singleton.Instance.OriginManager.EnableDepthTexture;
            XROcclusionSubsystem subsystem =
                Singleton.Instance.OriginManager.AROcclusionManager.subsystem;

            if (subsystem == null)
            {
                _depthTextureStatus.UpdateStatus("Not supported", false);
                return;
            }

            if (!subsystem.running)
            {
                _depthTextureStatus.UpdateStatus("Not running", false);
                return;
            }

            NativeArray<XRTextureDescriptor> descriptors =
                subsystem.GetTextureDescriptors(Allocator.Temp);

            if (descriptors.Length == 0)
            {
                _depthTextureStatus.UpdateStatus("No descriptors", false);
                return;
            }

            bool valid = true;
            foreach (XRTextureDescriptor descriptor in descriptors)
            {
                if (!descriptor.valid)
                {
                    valid = false;
                    break;
                }
            }

            if (!valid)
            {
                _depthTextureStatus.UpdateStatus("Not valid", false);
            }

            int width = descriptors[0].width;
            int height = descriptors[0].height;
            _depthTextureStatus.UpdateStatus(
                $"Available ({descriptors.Length}x{width}x{height})", true);
        }

        private void UpdateSceneMeshState()
        {
            _sceneMeshStatus.StartButtonActive =
                !Singleton.Instance.OriginManager.EnableMeshManager;
            XRMeshSubsystem subsystem =
                Singleton.Instance.OriginManager.ARMeshManager.subsystem;

            if (subsystem == null)
            {
                _sceneMeshStatus.UpdateStatus("Not supported", false);
                return;
            }

            if (!subsystem.running)
            {
                _sceneMeshStatus.UpdateStatus("Not running", false);
                return;
            }

            _sceneMeshStatus.UpdateStatus(
                _anySceneMeshChangesDetected.ToString(), _anySceneMeshChangesDetected);
        }

        private void OnMeshesChanged(ARMeshInfosChangedEventArgs eventArgs)
        {
            XRMeshSubsystem subsystem =
                Singleton.Instance.OriginManager.ARMeshManager.subsystem;

            // Check to see if any meshes were updated which have the Scene Mesh id.
            foreach (MeshUpdateInfo meshUpdateInfo in eventArgs.updated)
            {
                TrackableId trackableId = HandMeshController.ConvertARMeshNameToTrackableId(
                    meshUpdateInfo.meshFilter);

                if (subsystem.IsSceneMeshId(trackableId))
                {
                    _anySceneMeshChangesDetected = true;
                }
            }
        }
    }
}
