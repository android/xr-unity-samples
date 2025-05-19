// <copyright file="MenuTutorial.cs" company="Google LLC">
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

using AndroidXRUnitySamples.InputDevices;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AndroidXRUnitySamples.MenusAndUI
{
    /// <summary>
    /// Logic for the tutorial window teaching how to summon the Menu.
    /// </summary>
    public class MenuTutorial : MonoBehaviour
    {
        [SerializeField] private InputActionProperty _menuSummonInputAction;
        [SerializeField] private InputActionProperty _leftHandPosition;
        [SerializeField] private Vector3 _leftHandOffset;
        [SerializeField] private float _filterBeta;
        [SerializeField] private float _filterMinCutoff;

        [Header("Animation")]
        [SerializeField] private CanvasGroup _canvas;
        [SerializeField] private float _showHideSpeed;
        [SerializeField] private float _showHideMinimumValue;
        [SerializeField] private AnimationCurve _showHideCurve;

        private OneEuroFilterVector3 _positionFilter;
        private State _currentState;
        private float _stateTimer;

        private enum State
        {
            TransitionToShowing,
            Showing,
            TransitionToHidden,
        }

        void Awake()
        {
            _positionFilter = new OneEuroFilterVector3();
            _positionFilter.Beta = _filterBeta;
            _positionFilter.MinCutoff = _filterMinCutoff;
            _positionFilter.Init(_leftHandPosition.action.ReadValue<Vector3>());
            _currentState = State.TransitionToShowing;
            _stateTimer = 0.0f;
            RefreshVisuals();
        }

        void Update()
        {
            switch (_currentState)
            {
            case State.TransitionToShowing:
                {
                    _stateTimer += Time.deltaTime * _showHideSpeed;
                    _stateTimer = Mathf.Min(_stateTimer, 1.0f);
                    RefreshVisuals();
                    if (_stateTimer >= 1.0f)
                    {
                        _currentState = State.Showing;
                    }

                    if (_menuSummonInputAction.action.WasPressedThisFrame())
                    {
                        _currentState = State.TransitionToHidden;
                    }
                }

                break;
            case State.Showing:
                if (_menuSummonInputAction.action.WasPressedThisFrame())
                {
                    // Yay, we did it. Hide us.
                    _currentState = State.TransitionToHidden;
                }

                break;
            case State.TransitionToHidden:
                {
                    _stateTimer -= Time.deltaTime * _showHideSpeed;
                    _stateTimer = Mathf.Max(_stateTimer, 0.0f);
                    RefreshVisuals();
                    if (_stateTimer <= 0.0f)
                    {
                        Destroy(gameObject);
                    }
                }

                break;
            }

            // Track and position next to left hand.
            Vector3 handPos = _leftHandPosition.action.ReadValue<Vector3>();

            // Put offset in hmd space.
            Transform camXf = Singleton.Instance.XROrigin.Camera.transform;
            Vector3 transformedOffset = camXf.TransformVector(_leftHandOffset);
            transform.position = _positionFilter.Step(Time.time, handPos + transformedOffset);

            // Face user.
            transform.forward = camXf.forward;
        }

        void RefreshVisuals()
        {
            // _showHideCurve modifies _stateTimer to smooth is out.
            float curveT = _showHideCurve.Evaluate(_stateTimer);

            // Map the curve value to [_showHideMinimumValue:1].
            float lerpT = Mathf.Lerp(_showHideMinimumValue, 1.0f, curveT);
            transform.localScale = Vector3.one * lerpT;
            _canvas.alpha = _stateTimer;
        }
    }
}
