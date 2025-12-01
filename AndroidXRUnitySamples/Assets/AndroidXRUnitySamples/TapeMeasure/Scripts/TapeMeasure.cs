// <copyright file="TapeMeasure.cs" company="Google LLC">
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AndroidXRUnitySamples.TapeMeasure
{
    /// <summary>
    /// Class used to control the tape measure experience.
    /// </summary>
    public class TapeMeasure : MonoBehaviour
    {
        [SerializeField] private InputActionProperty _handPositionAction;
        [SerializeField] private InputActionProperty _handRotationAction;
        [SerializeField] private InputActionProperty _selectActionRight;
        [SerializeField] private GameObject _measurementPrefab;
        [SerializeField] private GameObject _markerVisual;
        [SerializeField] private AnimationCurve _markerVisualHideCurve;
        [SerializeField] private float _markerVisualHideSpeed;
        [SerializeField] private GameObject _howToPlayPanelPrefab;
        [SerializeField] private float _howToPlayMinShowDuration;
        [SerializeField] private float _initialMeasuringBlockDuration;
        [SerializeField] private float _handCastOffset;

        [Tooltip("The max distance to raycast.")]
        [SerializeField] private float _raycastMaxDistance = 100f;

        [Tooltip("Game objects of the selected layers are raycasted on.")]
        [SerializeField] private LayerMask _raycastLayerMask;

        [SerializeField] private MeshFilter _sceneMeshPrefab;

        /// <summary>
        /// Whether the marker position was valid at the previous frame.
        /// </summary>
        private bool _markerPositionWasValid = false;
        private Measurement _activeMeasurement;
        private List<GameObject> _measurements;
        private Coroutine _markerScalingRoutine;
        private GameObject _howToPlayPanel;
        private float _howToPlayTimer;
        private State _currentState;
        private float _measuringBlockTimer;
        private bool _menuWasActive;

        private enum State
        {
            Intro,
            Standard,
        }

        private void Start()
        {
            Singleton.Instance.OriginManager.MeshManagerMeshPrefab = _sceneMeshPrefab;
            Singleton.Instance.OriginManager.EnableMeshManager = true;

            _markerVisual.transform.localScale = Vector3.zero;
            EnableMarker(false);
            _measurements = new List<GameObject>();

            _howToPlayTimer = _howToPlayMinShowDuration;
            _measuringBlockTimer = _initialMeasuringBlockDuration;
        }

        private void Update()
        {
            switch (_currentState)
            {
            case State.Intro:
                UpdateIntro();
                break;
            case State.Standard:
                UpdateStandard();
                break;
            }
        }

        private void UpdateIntro()
        {
            // Hide the "how to play" message when the main menu is active.
            if (Singleton.Instance.Menu.Active)
            {
                if (_howToPlayPanel != null)
                {
                    _howToPlayPanel.GetComponent<ScaleOnEnable>().ScaleDownAndDestroy();
                    _howToPlayPanel = null;
                }
            }
            else
            {
                if (_howToPlayPanel == null)
                {
                    _howToPlayPanel = Instantiate(_howToPlayPanelPrefab);
                }

                _howToPlayTimer -= Time.deltaTime;
                if (_howToPlayTimer <= 0.0f)
                {
                    if (_selectActionRight.action.IsPressed())
                    {
                        _howToPlayPanel.GetComponent<ScaleOnEnable>().ScaleDownAndDestroy();
                        _howToPlayPanel = null;
                        _currentState = State.Standard;
                        _menuWasActive = false;
                        EnableMarker(true);
                    }
                }
            }
        }

        private void UpdateStandard()
        {
            // Compute a marker position.
            bool markerPositionIsValid = GetMarkerPosition(out Vector3 markerPosition);

            // Manage visuals when the main menu is active.
            bool menuActive = Singleton.Instance.Menu.Active;
            if (menuActive != _menuWasActive)
            {
                for (int i = 0; i < _measurements.Count; ++i)
                {
                    _measurements[i].SetActive(!menuActive);
                }

                EnableMarker(!menuActive);

                if (menuActive && _activeMeasurement != null)
                {
                    _activeMeasurement.FixatePosition();
                    _activeMeasurement = null;
                }

                // If the menu just closed, disregard input for a little bit so the menu
                // close input doesn't trigger a measurement.
                if (!menuActive)
                {
                    _measuringBlockTimer = 0.5f;
                }

                _menuWasActive = menuActive;
            }

            if (!menuActive)
            {
                _measuringBlockTimer -= Time.deltaTime;
                if (_measuringBlockTimer <= 0.0f)
                {
                    if (_activeMeasurement == null)
                    {
                        if (_selectActionRight.action.IsPressed())
                        {
                            if (markerPositionIsValid)
                            {
                                // Create a new measurement and set the first position.
                                _activeMeasurement = CreateMeasurement();
                                _activeMeasurement.UpdatePosition(markerPosition);
                                _activeMeasurement.FixatePosition();
                                EnableMarker(false);
                            }
                        }
                        else
                        {
                            // Enable the marker when the marker position becomes valid, or
                            // disable the marker when the marker position becomes invalid.
                            if (markerPositionIsValid != _markerPositionWasValid)
                            {
                                EnableMarker(markerPositionIsValid);
                                _markerPositionWasValid = markerPositionIsValid;
                            }
                        }
                    }
                    else
                    {
                        if (!_selectActionRight.action.IsPressed())
                        {
                            if (markerPositionIsValid)
                            {
                                // Set the second position of the active measurement.
                                _activeMeasurement.FixatePosition();
                                _activeMeasurement = null;
                                EnableMarker(true);
                            }
                            else
                            {
                                // Cannot set the second position of the active measurement.
                                // Remove and destroy the active measurement.
                                RemoveMeasurement(_activeMeasurement);
                                Destroy(_activeMeasurement.gameObject);
                                _activeMeasurement = null;
                            }
                        }
                    }

                    if (_activeMeasurement != null && markerPositionIsValid)
                    {
                        _activeMeasurement.UpdatePosition(markerPosition);
                    }
                }

                if (markerPositionIsValid)
                {
                    // Update the position and the forward direction of the marker.
                    _markerVisual.transform.position = markerPosition;
                    _markerVisual.transform.forward = GetMarkerForward();
                }
            }
        }

        private Measurement CreateMeasurement()
        {
            var obj = Instantiate(_measurementPrefab);
            _measurements.Add(obj);
            return obj.GetComponent<Measurement>();
        }

        private void RemoveMeasurement(Measurement measurement)
        {
            _measurements.Remove(measurement.gameObject);
        }

        private void EnableMarker(bool enable)
        {
            if (_markerScalingRoutine != null)
            {
                StopCoroutine(_markerScalingRoutine);
            }

            float targetScale = enable ? 1.0f : 0.0f;
            _markerScalingRoutine = StartCoroutine(SetMarkerTargetScaleRoutine(targetScale));
        }

        /// <summary>
        /// Compute a marker position in the world space at the center of the view.
        /// </summary>
        /// <param name="markerPosition">The output marker position.</param>
        /// <returns>Whether the output position is valid.</returns>
        private bool GetMarkerPosition(out Vector3 markerPosition)
        {
            var handPosition = _handPositionAction.action.ReadValue<Vector3>();
            var handRotation = _handRotationAction.action.ReadValue<Quaternion>();
            var handForward = handRotation * Vector3.forward;
            handPosition += handForward * _handCastOffset;

            // Use the raycasting hit position as the marker position.
            if (Physics.Raycast(handPosition, handForward, out var raycastHit, _raycastMaxDistance,
                                _raycastLayerMask))
            {
                // Raycasting hit a collider. Compute the hit position.
                markerPosition = handPosition + raycastHit.distance * handForward;
                return true;
            }
            else
            {
                // Raycasting did not hit any collider.
                markerPosition = Vector3.zero;
                return false;
            }
        }

        private Vector3 GetMarkerForward()
        {
            var rot = _handRotationAction.action.ReadValue<Quaternion>();
            return rot * Vector3.forward;
        }

        private IEnumerator SetMarkerTargetScaleRoutine(float targetScale)
        {
            _markerVisual.SetActive(true);

            float currentScale = _markerVisual.transform.localScale.x;
            float stepDirection = currentScale > targetScale ? -1.0f : 1.0f;
            while (currentScale != targetScale)
            {
                float step = _markerVisualHideSpeed * Time.deltaTime * stepDirection;
                float toTarget = Mathf.Abs(targetScale - currentScale);
                if (Mathf.Abs(step) > toTarget)
                {
                    currentScale = targetScale;
                }
                else
                {
                    currentScale += step;
                }

                float curveScale = _markerVisualHideCurve.Evaluate(currentScale);
                _markerVisual.transform.localScale = Vector3.one * curveScale;
                yield return null;
            }

            _markerVisual.SetActive(currentScale > 0.0f);
            _markerScalingRoutine = null;
        }
    }
}
