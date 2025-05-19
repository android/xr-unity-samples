// <copyright file="InteractionHint.cs" company="Google LLC">
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
using UnityEngine.InputSystem;

namespace AndroidXRUnitySamples.SoundArena
{
    /// <summary>
    /// Controls visibility of a UI Canvas for hinting interactions to the user.
    /// </summary>
    public class InteractionHint : MonoBehaviour
    {
        [SerializeField]
        private InputActionProperty[] _dismissHintActions;

        [SerializeField]
        private float _initialDelay;

        private State _currentState;
        private float _stateTimer;
        private bool _dismissHint;

        private enum State
        {
            TransitionToShowing,
            Showing,
            TransitionToHidden
        }

        private void Awake()
        {
            _currentState = State.TransitionToShowing;
            _stateTimer = 0.0f;

            foreach (InputActionProperty ia in _dismissHintActions)
            {
                ia.action.performed += _ => _dismissHint = true;
            }

            _stateTimer = _initialDelay;
        }

        private void Update()
        {
            switch (_currentState)
            {
            case State.TransitionToShowing:
                _stateTimer -= Time.deltaTime;
                if (_stateTimer <= 0.0f)
                {
                    _currentState = State.Showing;
                }

                break;
            case State.Showing:
                if (_dismissHint)
                {
                    GetComponent<ScaleOnEnable>().ScaleDownAndDestroy();
                    _currentState = State.TransitionToHidden;
                }

                break;
            case State.TransitionToHidden:
                break;
            }
        }
    }
}
