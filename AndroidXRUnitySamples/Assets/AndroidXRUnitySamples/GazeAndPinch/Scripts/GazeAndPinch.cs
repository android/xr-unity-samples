// <copyright file="GazeAndPinch.cs" company="Google LLC">
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

using AndroidXRUnitySamples.Variables;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARSubsystems;

namespace AndroidXRUnitySamples.GazeAndPinch
{
    /// <summary>
    /// Class used to manage the GazeAndPinch scene.
    /// </summary>
    public class GazeAndPinch : MonoBehaviour
    {
        /// <summary>
        /// SceneState indicating the state of the scene.
        /// </summary>
        public SceneState SceneState;

        /// <summary>
        /// Prefab used for detected planes.
        /// </summary>
        public GameObject PlanePrefab;

        /// <summary>
        /// Prefab used for displaying a plane in the Editor used for debugging.
        /// </summary>
        public GameObject EditorPlanePrefab;

        /// <summary>
        /// GameObject used as the root for the scene contents.
        /// </summary>
        public GameObject SceneContainer;

        /// <summary>
        /// UI GameObject shown when we're looking for a plane to select.
        /// </summary>
        public GameObject SearchingForPlanePrefab;

        /// <summary>
        /// UI GameObject shown when we've found a valid plane.
        /// </summary>
        public GameObject PlaneIdentifiedPrefab;

        /// <summary>
        /// Minimum scale to use for the scene contents based on plane size.
        /// </summary>
        public float MinSceneScale = 0.2f;

        /// <summary>
        /// Maximum scale to use for the scene contents based on plane size.
        /// </summary>
        public float MaxSceneScale = 1f;

        /// <summary>
        /// Size of the scene contents. Used to scale things based on plane size.
        /// </summary>
        public Vector2 SceneContentsSize;

        /// <summary>
        /// The catapults in the scene.
        /// </summary>
        public CatapultController[] Catapults;

        /// <summary>
        /// Bounds used to lerp catapult launch forces.
        /// </summary>
        public Vector2 CatapultForceLerpBounds = new Vector2(0, 1);

        /// <summary>
        /// BoolVariable indicating the state of the main menu.
        /// </summary>
        public BoolVariable MenuIsOpen;

#if UNITY_EDITOR
        float _editorExtents = 1f;
#endif

        private ScaleOnEnable _searchingForPlane;
        private ScaleOnEnable _planeIdentified;

        void Start()
        {
            Singleton.Instance.OriginManager.EnablePassthrough = true;
            Singleton.Instance.OriginManager.EnablePlaneDetection = true;
            Singleton.Instance.OriginManager.PlaneDetectionMode =
                PlaneDetectionMode.Horizontal;
            Singleton.Instance.OriginManager.PlanePrefab = PlanePrefab;
            Singleton.Instance.OriginManager.EnableGazeInteraction = true;

            SceneState.Phase.Value = Phase.SearchingForPlane;
            SceneState.ScenePlane.Value = null;

            SceneState.Phase.AddListener(HandleSceneStateChange);
            HandleSceneStateChange(SceneState.Phase.Value);

            SceneState.ScenePlane.AddListener(HandleScenePlaneChange);
            HandleScenePlaneChange(SceneState.ScenePlane.Value);

            MenuIsOpen.AddListener(HandleMenuOpen);
            HandleMenuOpen(MenuIsOpen.Value);
        }

        void OnDestroy()
        {
            SceneState.Phase.RemoveListener(HandleSceneStateChange);
            SceneState.ScenePlane.RemoveListener(HandleScenePlaneChange);
            MenuIsOpen.RemoveListener(HandleMenuOpen);
        }

        void HandleSceneStateChange(Phase phase)
        {
            bool noPlane = (SceneState.ScenePlane.Value == null);

            switch (phase)
            {
                case Phase.SearchingForPlane:
                    SceneContainer.SetActive(false);
                    break;
                case Phase.Shooting:
                    if (noPlane)
                    {
                        Debug.LogError("Scene in Shooting state but ScenePlane is null");
                        return;
                    }

                    // Only activate scene contents once.
                    if (!SceneContainer.activeSelf)
                    {
                        SceneContainer.SetActive(true);
                        SceneContainer.transform.position =
                            SceneState.ScenePlane.Value.ARPlane.center;

                        float scale;
                        float rotation;
                        GetSceneScaleAndRotation(
                            SceneState.ScenePlane.Value, out scale, out rotation);
                        SceneContainer.transform.localScale = new Vector3(
                            scale, scale, scale);
                        SceneContainer.transform.rotation = Quaternion.Euler(0, rotation, 0);

                        // Scale launch forces on the catapults.
                        ScaleCatapultLaunchForces(scale);
                    }

                    break;
            }

            UpdateUI();
        }

        void HandleScenePlaneChange(Plane plane)
        {
            HandleSceneStateChange(SceneState.Phase.Value);
        }

        void UpdateUI()
        {
            if (SceneState.Phase.Value == Phase.SearchingForPlane || MenuIsOpen.Value)
            {
                if (SceneState.ScenePlane.Value == null)
                {
                    // No plane identified. Show searching panel message and make sure our other
                    // UI messages are cleaned up.
                    if (_searchingForPlane == null)
                    {
                        GameObject g = Instantiate(SearchingForPlanePrefab);
                        _searchingForPlane = g.GetComponent<ScaleOnEnable>();
                    }

                    DestroyPlaneIdentifiedPanel();
                }
                else
                {
                    // Plane identified. Show info message and make sure the searching panel
                    // is cleaned up.
                    if (_planeIdentified == null)
                    {
                        GameObject g = Instantiate(PlaneIdentifiedPrefab);
                        _planeIdentified = g.GetComponent<ScaleOnEnable>();
                    }

                    DestroySearchingPanel();

                    bool planeValid = IsSuitablePlane(SceneState.ScenePlane.Value);
                    _planeIdentified.GetComponent<PlaneIdentifiedPanel>().SetValid(planeValid);
                }
            }
            else
            {
                // Clean up everything if we're not searching.
                DestroySearchingPanel();
                DestroyPlaneIdentifiedPanel();
            }
        }

        void DestroySearchingPanel()
        {
            if (_searchingForPlane != null)
            {
                _searchingForPlane.ScaleDownAndDestroy();
                _searchingForPlane = null;
            }
        }

        void DestroyPlaneIdentifiedPanel()
        {
            if (_planeIdentified != null)
            {
                _planeIdentified.ScaleDownAndDestroy();
                _planeIdentified = null;
            }
        }

#if UNITY_EDITOR
        // Used for fine tuning scaling of the scene based on plane size.
        void Update()
        {
            if (Keyboard.current[Key.P].wasPressedThisFrame)
            {
                GameObject p = Instantiate(EditorPlanePrefab);
                Vector3 offset = new Vector3(0, -0.5f, 1.9f);
                p.transform.position = Singleton.Instance.Camera.transform.position + offset;
                p.transform.rotation = Quaternion.Euler(90, 0, 0);
                p.transform.localScale = new Vector3(
                    _editorExtents, _editorExtents, _editorExtents);

                Plane plane = p.GetComponent<Plane>();
                SceneState.ScenePlane.Value = plane;
            }

            if (Keyboard.current[Key.Y].wasPressedThisFrame)
            {
                SceneState.Phase.Value = Phase.Shooting;
            }

            if (Keyboard.current[Key.B].wasPressedThisFrame)
            {
                _editorExtents += 0.1f;
                SceneState.ScenePlane.Value.transform.localScale = new Vector3(
                    _editorExtents, _editorExtents, _editorExtents);
                SceneState.ScenePlane.Value = SceneState.ScenePlane.Value;
            }

            if (Keyboard.current[Key.S].wasPressedThisFrame)
            {
                _editorExtents -= 0.1f;
                SceneState.ScenePlane.Value.transform.localScale = new Vector3(
                    _editorExtents, _editorExtents, _editorExtents);
                SceneState.ScenePlane.Value = SceneState.ScenePlane.Value;
            }
        }
#endif

        void GetSceneScaleAndRotation(Plane plane, out float scale, out float rotation)
        {
            if (plane == null)
            {
                Debug.LogError("Trying to get scene size with no active plane");
                scale = 1f;
                rotation = 0;
                return;
            }

            Vector2 extents = GetPlaneExtents(plane);

            // Try with rotation 0;
            float widthFactor = extents.x / SceneContentsSize.x;
            float lengthFactor = extents.y / SceneContentsSize.y;

            float scaleFactor = Mathf.Min(widthFactor, lengthFactor);

            // Try with rotation 90;
            widthFactor = extents.x / SceneContentsSize.y;
            lengthFactor = extents.y / SceneContentsSize.x;

            float scaleFactorRotated = Mathf.Min(widthFactor, lengthFactor);

            // Find the best fit.
            if (scaleFactorRotated > scaleFactor)
            {
                rotation = 90;
                scale = Mathf.Clamp(scaleFactorRotated, MinSceneScale, MaxSceneScale);
            }
            else
            {
                rotation = 0;
                scale = Mathf.Clamp(scaleFactor, MinSceneScale, MaxSceneScale);
            }
        }

        bool IsSuitablePlane(Plane plane)
        {
            Vector2 minPlaneSize = MinSceneScale * SceneContentsSize;
            Vector2 planeSize = GetPlaneExtents(plane);

            // See if the scene contents fit as is, or via 90 degree rotation.
            return (planeSize.x >= minPlaneSize.x && planeSize.y >= minPlaneSize.y) ||
                   (planeSize.x >= minPlaneSize.y && planeSize.y >= minPlaneSize.x);
        }

        void ScaleCatapultLaunchForces(float scaleFactor)
        {
            float scale = Mathf.Lerp(
                CatapultForceLerpBounds.x, CatapultForceLerpBounds.y, scaleFactor);
            foreach (CatapultController catapult in Catapults)
            {
                catapult.LaunchForceScale = scale;
            }
        }

        void HandleMenuOpen(bool isOpen)
        {
            Singleton.Instance.OriginManager.EnableGazeInteraction = !isOpen;

            if (isOpen)
            {
                DestroySearchingPanel();
                DestroyPlaneIdentifiedPanel();
            }
            else
            {
                UpdateUI();
            }
        }

        Vector2 GetPlaneExtents(Plane plane)
        {
#if UNITY_EDITOR
            return new Vector2(_editorExtents, _editorExtents);
#else
            return plane.ARPlane.extents * 2;
#endif
        }
    }
}
