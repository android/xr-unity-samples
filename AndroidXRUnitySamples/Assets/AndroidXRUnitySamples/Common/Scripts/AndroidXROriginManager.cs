// <copyright file="AndroidXROriginManager.cs" company="Google LLC">
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
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR.NativeTypes;

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Class used to manage the AndroidXROrigin prefab functionality.
    /// </summary>
    public class AndroidXROriginManager : MonoBehaviour
    {
        /// <summary>
        /// Controller for the blend mode.
        /// </summary>
        public BlendModeController BlendController;

        /// <summary>
        /// ARPlaneManager on the origin.
        /// </summary>
        public ARPlaneManager ARPlaneManager;

        /// <summary>
        /// ARTrackedObjectManager on the origin.
        /// </summary>
        public ARTrackedObjectManager ARTrackedObjectManager;

        /// <summary>
        /// ARPlaneManager on the origin.
        /// </summary>
        public ARSession ARSession;

        /// <summary>
        /// All GameObjects associated with gaze interactions.
        /// </summary>
        public GameObject[] GazeInteractionObjects;

        /// <summary>
        /// All hand-related interactors and associated objects under this origin.
        /// </summary>
        public GameObject[] HandRaysInteractionObjects;

        /// <summary>
        /// ARCameraManager on the origin.
        /// </summary>
        public ARCameraManager ARCameraManager;

        /// <summary>
        /// Controller for foveation.
        /// </summary>
        public FoveationController FoveationController;

        /// <summary>
        /// ARFaceManager on the origin.
        /// </summary>
        public ARFaceManager ARFaceManager;

        /// <summary>
        /// AROcclusionManager on the camera.
        /// </summary>
        public AROcclusionManager AROcclusionManager;

        /// <summary>
        /// ARShaderOcclusion on the camera.
        /// </summary>
        public ARShaderOcclusion ARShaderOcclusion;

        private bool _passthroughEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether passthrough is enabled.
        /// </summary>
        public bool EnablePassthrough
        {
            get =>
                    _passthroughEnabled;

            set
            {
                _passthroughEnabled = value;
                if (_passthroughEnabled)
                {
                    BlendController.SetBlendMode(XrEnvironmentBlendMode.AlphaBlend);
                    Singleton.Instance.Camera.clearFlags = CameraClearFlags.SolidColor;
                }
                else
                {
                    BlendController.SetBlendMode(XrEnvironmentBlendMode.Opaque);
                    Singleton.Instance.Camera.clearFlags = CameraClearFlags.Skybox;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether plane detection is enabled.
        /// </summary>
        public bool EnablePlaneDetection
        {
            get =>
                    ARPlaneManager.enabled;

            set
            {
                ARPlaneManager.enabled = value;
                UpdateSessionEnabled();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the plane detection mode to use.
        /// </summary>
        public PlaneDetectionMode PlaneDetectionMode
        {
            get =>
                    ARPlaneManager.requestedDetectionMode;

            set =>
                    ARPlaneManager.requestedDetectionMode = value;
        }

        /// <summary>
        /// Gets or sets a value indicating the plane prefab to use.
        /// </summary>
        public GameObject PlanePrefab
        {
            get =>
                    ARPlaneManager.planePrefab;

            set =>
                    ARPlaneManager.planePrefab = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether gaze interaction is enabled or not.
        /// Enables/disables interactors for gaze input when set.
        /// </summary>
        public bool EnableGazeInteraction
        {
            get
            {
                foreach (GameObject g in GazeInteractionObjects)
                {
                    if (g.activeSelf)
                    {
                        return true;
                    }
                }

                return false;
            }

            set
            {
                foreach (GameObject g in GazeInteractionObjects)
                {
                    g.SetActive(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether hands interaction is enabled or not.
        /// Enables/disables interactors for hands input when set.
        /// </summary>
        public bool EnableHandRaysInteraction
        {
            get
            {
                foreach (GameObject g in HandRaysInteractionObjects)
                {
                    if (g.activeSelf)
                    {
                        return true;
                    }
                }

                return false;
            }

            set
            {
                foreach (GameObject g in HandRaysInteractionObjects)
                {
                    g.SetActive(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether object detection is enabled.
        /// </summary>
        public bool EnableObjectTracking
        {
            get =>
                    ARTrackedObjectManager.enabled;

            set
            {
                ARTrackedObjectManager.enabled = value;
                UpdateSessionEnabled();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the object tracking reference library to use.
        /// </summary>
        public XRReferenceObjectLibrary ObjectTrackingReferenceLibrary
        {
            get =>
                    ARTrackedObjectManager.referenceLibrary;

            set =>
                    ARTrackedObjectManager.referenceLibrary = value;
        }

        /// <summary>
        /// Gets or sets a value indicating the tracked object prefab to use.
        /// </summary>
        public GameObject ObjectTrackingObjectPrefab
        {
            get =>
                    ARTrackedObjectManager.trackedObjectPrefab;

            set =>
                    ARTrackedObjectManager.trackedObjectPrefab = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether light estimation is enabled.
        /// </summary>
        public bool EnableLightEstimation
        {
            get =>
                    ARCameraManager.enabled;

            set =>
                    ARCameraManager.enabled = value;
        }

        /// <summary>
        /// Gets or sets a value indicating the light estimation mode to use.
        /// </summary>
        public LightEstimation LightEstimationMode
        {
            get =>
                    ARCameraManager.requestedLightEstimation;

            set =>
                    ARCameraManager.requestedLightEstimation = value;
        }

        /// <summary>
        /// Gets or sets a value indicating the foveation level to use.
        /// </summary>
        public float FoveationLevel
        {
            get =>
                    FoveationController.FoveationLevel;

            set =>
                    FoveationController.FoveationLevel = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether face manager is enabled.
        /// </summary>
        public bool EnableFaceManager
        {
            get =>
                    ARFaceManager.enabled;

            set
            {
                ARFaceManager.enabled = value;
                UpdateSessionEnabled();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether depth texture is enabled.
        /// </summary>
        public bool EnableDepthTexture
        {
            get =>
                    AROcclusionManager.enabled;

            set
            {
                AROcclusionManager.enabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether shader occlusion is enabled.
        /// </summary>
        public bool EnableShaderOcclusion
        {
            get =>
                    ARShaderOcclusion.enabled;

            set
            {
                ARShaderOcclusion.enabled = value;
            }
        }

        private void UpdateSessionEnabled()
        {
            ARSession.enabled = EnablePlaneDetection || EnableObjectTracking || EnableFaceManager;
        }
    }
}
