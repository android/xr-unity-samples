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

using Google.XR.Extensions;
using Unity.Collections;
using UnityEngine;
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

        [SerializeField] private XRFaceTrackingFeature _faceTrackingFeature;
        [SerializeField] private XRFaceTrackingManager _faceManager;
        [SerializeField] private ARPlaneManager _planeManager;
        [SerializeField] private ARTrackedObjectManager _objectManager;
        [SerializeField] private XRReferenceObjectLibrary _objectReferenceLibrary;

        private InputAction _headPositionAction = new InputAction();
        private InputAction _headRotationAction = new InputAction();
        private InputAction _pinchStateLeft = new InputAction();
        private InputAction _pinchStateRight = new InputAction();
        private InputAction _etState = new InputAction();
        private bool _gesturesEverBeenReady = false;
        private bool _anyPlaneDetected = false;
        private bool _anyObjectDetected = false;

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

#if !UNITY_EDITOR
            _faceManager.enabled = true;
#endif  // UNITY_EDITOR

            Singleton.Instance.OriginManager.PlanePrefab = null;
            Singleton.Instance.OriginManager.EnablePlaneDetection = true;

            Singleton.Instance.OriginManager.ObjectTrackingReferenceLibrary =
                _objectReferenceLibrary;
            Singleton.Instance.OriginManager.ObjectTrackingObjectPrefab = null;
            Singleton.Instance.OriginManager.EnableObjectTracking = true;

            Singleton.Instance.OriginManager.EnableFaceManager = true;

            Singleton.Instance.OriginManager.EnableDepthTexture = true;

            XRInputModalityManager.currentInputMode.SubscribeAndUpdate(UpdateInputModeState);
        }

        private void OnEnable()
        {
            _planeManager.trackablesChanged.AddListener(OnPlaneDetected);
            _objectManager.trackablesChanged.AddListener(OnObjectDetected);
        }

        private void OnDisable()
        {
            _planeManager.trackablesChanged.RemoveListener(OnPlaneDetected);
            _objectManager.trackablesChanged.RemoveListener(OnObjectDetected);
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
        }

        private void UpdatePassthroughState()
        {
            if (XRPassthroughFeature.IsExensionEnabled == null)
            {
                _passthroughStatus.UpdateStatus("Feature not found", false);
                return;
            }
            else if (!XRPassthroughFeature.IsExensionEnabled.Value)
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
            _gesturesEverBeenReady = _gesturesEverBeenReady ||
                (_pinchStateLeft.IsPressed() || _pinchStateRight.IsPressed());
            _gesturesStatus.UpdateStatus(_gesturesEverBeenReady.ToString(), _gesturesEverBeenReady);
        }

        private void UpdateEyeTrackingState()
        {
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
            else
            {
                _inputModeStatus.UpdateStatus("Controller", true);
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
            if (_faceTrackingFeature == null)
            {
                _faceTrackingStatus.UpdateStatus("Feature not found", false);
                return;
            }
            else if (!_faceTrackingFeature.enabled)
            {
                _faceTrackingStatus.UpdateStatus("Feature not enabled", false);
                return;
            }

            if (Application.isEditor)
            {
                _faceTrackingStatus.UpdateStatus("Not supported", false);
            }
            else
            {
                XRFaceTrackingStates state = _faceManager.Face.TrackingState;
                _faceTrackingStatus.UpdateStatus(state.ToString(),
                    state == XRFaceTrackingStates.Tracking);
            }
        }

        private void UpdatePlaneDetectionState()
        {
            _planeDetectionStatus.UpdateStatus(_anyPlaneDetected.ToString(), _anyPlaneDetected);
        }

        private void UpdateObjectTrackingState()
        {
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
    }
}
