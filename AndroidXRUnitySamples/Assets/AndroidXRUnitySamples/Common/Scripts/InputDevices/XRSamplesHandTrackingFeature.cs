// <copyright file="XRSamplesHandTrackingFeature.cs" company="Google LLC">
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

using System.Linq;
using Google.XR.Extensions;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.InputSystem;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR.Features;

namespace AndroidXRUnitySamples.InputDevices
{
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.XR.OpenXR.Features;
#endif

    /// <summary>
    /// Feature used to enable the XRSamplesHandDevice. Requires the `Hand Tracking Subsystem` to
    /// also be enabled.
    /// </summary>
#if UNITY_EDITOR
    [OpenXRFeature(UiName = "Android XR Unity Samples Hand Tracking",
            BuildTargetGroups =
                    new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android },
            Company = "Google", Desc = "Android XR Unity Samples hand tracking feature",
            Version = "1.0.0", Category = FeatureCategory.Feature, FeatureId = FeatureId)]
#endif
    public class XRSamplesHandTrackingFeature : OpenXRFeature
    {
        /// <summary>
        /// The feature ID string.
        /// </summary>
        public const string FeatureId = "com.google.xr.androidxr_unity_samples.hand_tracking";

        private readonly AndroidXRPermission[] _androidPermissions =
                new AndroidXRPermission[] { AndroidXRPermission.HandTracking, };

        /// <summary>
        /// Invoked when the system is started. Used for creating hand devices.
        /// </summary>
        protected override void OnSubsystemStart()
        {
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += OnPermissionsDenied;
            callbacks.PermissionGranted += OnPermissionsGranted;
            var permissionStrings = _androidPermissions.Select(p => p.ToPermissionString());
            Permission.RequestUserPermissions(permissionStrings.ToArray(), callbacks);
        }

        /// <summary>
        /// Invoked when the system is stopped. Used for destroying hand devices.
        /// </summary>
        protected override void OnSubsystemStop()
        {
            var subsystem = XRGeneralSettings.Instance?.Manager?.activeLoader
                                             ?.GetLoadedSubsystem<XRHandSubsystem>();
            if (subsystem != null)
            {
                DestroyHands();
            }
        }

        private static void OnPermissionsGranted(string permission)
        {
            InitializeSubsystemDevice();
        }

        private static void OnPermissionsDenied(string permission)
        {
            string msg = $"Permission {permission} denied for feature {FeatureId}";
            Debug.LogError(msg);
        }

        private static void InitializeSubsystemDevice()
        {
            var subsystem = XRGeneralSettings.Instance?.Manager?.activeLoader
                                             ?.GetLoadedSubsystem<XRHandSubsystem>();
            if (subsystem != null)
            {
                CreateHands(subsystem);
            }
        }

        private static void CreateHands(XRHandSubsystem subsystem)
        {
            if (XRSamplesHandDevice.LeftHand == null)
            {
                XRSamplesHandDevice.LeftHand = XRSamplesHandDevice.Create(
                        subsystem, Handedness.Left, XRHandSubsystem.UpdateSuccessFlags.None,
                        XRHandSubsystem.UpdateType.Dynamic);
            }

            if (XRSamplesHandDevice.RightHand == null)
            {
                XRSamplesHandDevice.RightHand = XRSamplesHandDevice.Create(
                        subsystem, Handedness.Right, XRHandSubsystem.UpdateSuccessFlags.None,
                        XRHandSubsystem.UpdateType.Dynamic);
            }
        }

        private void DestroyHands()
        {
            if (XRSamplesHandDevice.LeftHand != null)
            {
                InputSystem.RemoveDevice(XRSamplesHandDevice.LeftHand);
                XRSamplesHandDevice.LeftHand = null;
            }

            if (XRSamplesHandDevice.RightHand != null)
            {
                InputSystem.RemoveDevice(XRSamplesHandDevice.RightHand);
                XRSamplesHandDevice.RightHand = null;
            }
        }
    }
}
