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
        [SerializeField] private InputActionProperty _headPositionAction;
        [SerializeField] private InputActionProperty _headRotationAction;
        [SerializeField] private InputActionProperty _selectActionLeft;
        [SerializeField] private InputActionProperty _selectActionRight;
        [SerializeField] private DepthProvider _depthProvider;
        [SerializeField] private GameObject _measurementPrefab;
        [SerializeField] private GameObject _markerVisual;
        [SerializeField] private AnimationCurve _markerVisualHideCurve;
        [SerializeField] private float _markerVisualHideSpeed;
        [SerializeField] private GameObject _howToPlayPanelPrefab;
        [SerializeField] private float _howToPlayMinShowDuration;
        [SerializeField] private float _initialMeasuringBlockDuration;

        private Vector2 _offset = new Vector2(0.5f, 0.5f);
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
            Singleton.Instance.OriginManager.EnablePassthrough = true;
            Singleton.Instance.OriginManager.EnableDepthTexture = true;

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
                    if (_selectActionLeft.action.IsPressed() ||
                        _selectActionRight.action.IsPressed())
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
            Vector3 markerPosition = GetMarkerPosition();

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
                        if (_selectActionLeft.action.IsPressed() ||
                            _selectActionRight.action.IsPressed())
                        {
                            _activeMeasurement = CreateMeasurement();
                            _activeMeasurement.UpdatePosition(markerPosition);
                            _activeMeasurement.FixatePosition();
                            EnableMarker(false);
                        }
                    }
                    else
                    {
                        if (!_selectActionLeft.action.IsPressed() &&
                            !_selectActionRight.action.IsPressed())
                        {
                            _activeMeasurement.FixatePosition();
                            _activeMeasurement = null;
                            EnableMarker(true);
                        }
                    }

                    if (_activeMeasurement != null)
                    {
                        _activeMeasurement.UpdatePosition(markerPosition);
                    }
                }

                _markerVisual.transform.position = markerPosition;
                _markerVisual.transform.forward = GetMarkerForward();
            }
        }

        private Measurement CreateMeasurement()
        {
            var obj = Instantiate(_measurementPrefab);
            _measurements.Add(obj);
            return obj.GetComponent<Measurement>();
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

        private Vector3 GetMarkerPosition()
        {
            if (_depthProvider._depthTexture == null)
            {
                Debug.LogError("Depth provider has no depth texture");
                return Vector3.zero;
            }

            // Compute depth position.
            var uv = new Vector2(_offset.x, 1f - _offset.y);
            var depth = _depthProvider._depthTexture.GetPixelBilinear(uv.x, uv.y).r;

            var pos = _headPositionAction.action.ReadValue<Vector3>();
            var rot = _headRotationAction.action.ReadValue<Quaternion>();

            Vector3 fwd = rot * Vector3.forward;
            return pos + depth * fwd;
        }

        private Vector3 GetMarkerForward()
        {
            var rot = _headRotationAction.action.ReadValue<Quaternion>();
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
