// <copyright file="XRSamplesHandDevice.cs" company="Google LLC">
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
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR;
using UnityEngine.XR.Hands;
using CommonUsages = UnityEngine.InputSystem.CommonUsages;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AndroidXRUnitySamples.InputDevices
{
    /// <summary>
    /// Class used to get Hand Tracking information via Unity's Input System.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [Preserve]
    [InputControlLayout(displayName = "Android XR Unity Samples Hand Device",
            commonUsages = new[]
            {
                    "LeftHand", "RightHand"
            })]
    public class XRSamplesHandDevice : TrackedDevice
    {
        private const string _kDeviceProductName = "XRSamplesHandDevice";

        // Palm facing user gesture computed by using the dot product of the vector from the
        // hand to the head's position, and the palm's up vector. If the dot product falls under
        // this threshold, it means the palm is facing the user.
        private const float _kPalmFacingUserThreshold = -0.75f;

        private Handedness _handedness;
        private bool _wasValid;

        private InputAction _headPositionAction = new InputAction();
        private InputAction _pinchValueAction = new InputAction();

        static XRSamplesHandDevice()
        {
            Initialize();
        }

        /// <summary>
        /// Gets the static instance of the left hand device.
        /// </summary>
        public static XRSamplesHandDevice LeftHand
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the static instance of the right hand device.
        /// </summary>
        public static XRSamplesHandDevice RightHand
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets pinch status of the thumb and index finger when hands facing away from user.
        /// </summary>
        [Preserve]
        [InputControl(offset = 0)]
        public ButtonControl IndexPinching
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets pinch status of the thumb and index finger when hands face user.
        /// </summary>
        [Preserve]
        [InputControl(offset = 1)]
        public ButtonControl IndexPinchingUserFacing
        {
            get; private set;
        }

        /// <summary>
        /// Gets pinch strength of the thumb and index finger.
        /// </summary>
        [Preserve]
        [InputControl]
        public AxisControl IndexPinchStrength
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a device for given handedness.
        /// </summary>
        /// <param name="subsystem">The hand subsystem.</param>
        /// <param name="handedness">HAndedness of the hand.</param>
        /// <param name="updateSuccessFlags">Update success flags.</param>
        /// <param name="updateType">Type of the update.</param>
        /// <returns>Returns the eye device.</returns>
        public static XRSamplesHandDevice Create(
                XRHandSubsystem subsystem, Handedness handedness,
                XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags,
                XRHandSubsystem.UpdateType updateType)
        {
            Debug.Log($"XRSamplesHandDevice::Create hand: {handedness}");
            InputDeviceCharacteristics extraCharacteristics =
                    handedness == Handedness.Left
                            ? InputDeviceCharacteristics.Left
                            : InputDeviceCharacteristics.Right;

            var desc =
                    new InputDeviceDescription
                    {
                            product = _kDeviceProductName,
                            capabilities =
                                    new XRDeviceDescriptor
                                    {
                                            characteristics =
                                                    InputDeviceCharacteristics.HandTracking |
                                                    InputDeviceCharacteristics.TrackedDevice |
                                                    extraCharacteristics,
                                            inputFeatures =
                                                    new List<XRFeatureDescriptor>
                                                    {
                                                            new XRFeatureDescriptor
                                                            {
                                                                    name = "index_pinching",
                                                                    featureType =
                                                                            FeatureType.Binary
                                                            },
                                                            new XRFeatureDescriptor
                                                            {
                                                                    name = "index_pinch_strength",
                                                                    featureType = FeatureType.Axis1D
                                                            },
                                                    }
                                    }.ToJson()
                    };
            var handDevice = InputSystem.AddDevice(desc) as XRSamplesHandDevice;
            if (handDevice != null)
            {
                subsystem.updatedHands += handDevice.OnUpdatedHands;
                handDevice._handedness = handedness;

                handDevice.OnUpdatedHands(subsystem, updateSuccessFlags, updateType);
            }

            return handDevice;
        }

        /// <summary>
        /// Finishes setting up the device and sets usage.
        /// </summary>
        protected override void FinishSetup()
        {
            base.FinishSetup();

            IndexPinching = GetChildControl<ButtonControl>("indexPinching");
            IndexPinchingUserFacing = GetChildControl<ButtonControl>("indexPinchingUserFacing");
            IndexPinchStrength = GetChildControl<AxisControl>("indexPinchStrength");

            XRDeviceDescriptor deviceDescriptor =
                    XRDeviceDescriptor.FromJson(description.capabilities);
            if (deviceDescriptor != null)
            {
                if ((deviceDescriptor.characteristics & InputDeviceCharacteristics.Left) != 0)
                {
                    InputSystem.SetDeviceUsage(this, CommonUsages.LeftHand);
                    _handedness = Handedness.Left;
                }
                else if ((deviceDescriptor.characteristics & InputDeviceCharacteristics.Right) != 0)
                {
                    InputSystem.SetDeviceUsage(this,
                            CommonUsages.RightHand);
                    _handedness = Handedness.Right;
                }
            }

            // Get head input action binding to be able to get head position.
            _headPositionAction.AddBinding("<XRHMD>/centerEyePosition");
            _headPositionAction.Enable();

            // Get pinch input action binding to be able to get pinch value.
            _pinchValueAction.AddBinding($"<HandInteraction>{{{_handedness}Hand}}/pinchValue");
            _pinchValueAction.Enable();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
#if ENABLE_INPUT_SYSTEM
            InputSystem.RegisterLayout<XRSamplesHandDevice>(
                    matches: new InputDeviceMatcher().WithProduct(_kDeviceProductName));
#endif // ENABLE_INPUT_SYSTEM
        }

        private void OnUpdatedHands(XRHandSubsystem subsystem,
                                    XRHandSubsystem.UpdateSuccessFlags updateSuccessFlags,
                                    XRHandSubsystem.UpdateType updateType)
        {
            XRHand hand;
            bool isValid;
            if (_handedness == Handedness.Left)
            {
                hand = subsystem.leftHand;
                XRHandSubsystem.UpdateSuccessFlags success =
                        XRHandSubsystem.UpdateSuccessFlags.LeftHandRootPose |
                        XRHandSubsystem.UpdateSuccessFlags.LeftHandJoints;
                isValid = (updateSuccessFlags & success) == success;
            }
            else
            {
                hand = subsystem.rightHand;
                XRHandSubsystem.UpdateSuccessFlags success =
                        XRHandSubsystem.UpdateSuccessFlags.RightHandRootPose |
                        XRHandSubsystem.UpdateSuccessFlags.RightHandJoints;
                isValid = (updateSuccessFlags & success) == success;
            }

            if (!_wasValid && !isValid)
            {
                return;
            }

            if (_wasValid && !isValid)
            {
                using (StateEvent.From(this, out InputEventPtr eventPtr))
                {
                    isTracked.WriteValueIntoEvent(0.0f, eventPtr);
                }

                InputSystem.QueueDeltaStateEvent(trackingState,
                        InputTrackingState.None);
                _wasValid = false;
                return;
            }

            if (!_wasValid && isValid)
            {
                using (StateEvent.From(this, out InputEventPtr eventPtr))
                {
                    isTracked.WriteValueIntoEvent(1.0f, eventPtr);
                }

                InputSystem.QueueDeltaStateEvent(
                        trackingState, InputTrackingState.Position | InputTrackingState.Rotation);
                _wasValid = true;
            }

            UpdatePinchState(hand, IndexPinching, IndexPinchingUserFacing, IndexPinchStrength);
        }

        void UpdatePinchState(XRHand hand, ButtonControl pinching,
                              ButtonControl pinchingUserFacing,
                              AxisControl pinchStrength)
        {
            bool pinch_pressed = IsIndexPinching();
            float pinch_strength = IndexPinchValue();

            if (IsPalmFacingUser(hand))
            {
                InputSystem.QueueDeltaStateEvent(pinching, false);
                InputSystem.QueueDeltaStateEvent(pinchingUserFacing, pinch_pressed);
            }
            else
            {
                InputSystem.QueueDeltaStateEvent(pinching, pinch_pressed);
                InputSystem.QueueDeltaStateEvent(pinchingUserFacing, false);
            }

            InputSystem.QueueDeltaStateEvent(pinchStrength, pinch_strength);
        }

        bool IsIndexPinching()
        {
            return _pinchValueAction.IsPressed();
        }

        float IndexPinchValue()
        {
            return _pinchValueAction.ReadValue<float>();
        }

        bool IsPalmFacingUser(XRHand hand)
        {
            if (!hand.isTracked)
            {
                return false;
            }

            if (hand.GetJoint(XRHandJointID.Palm).TryGetPose(out Pose palmPose))
            {
                var headPosition = _headPositionAction.ReadValue<Vector3>();
                Vector3 handToHeadDir = (headPosition - palmPose.position).normalized;

                // How aligned is the hand pose with the head pose.
                float dot = Vector3.Dot(handToHeadDir, palmPose.up);

                // If they are almost facing opposite directions, that means the palm
                // is facing the user.
                if (dot < _kPalmFacingUserThreshold)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
