// <copyright file="DeviceDetector.cs" company="Google LLC">
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
using UnityEngine.XR.ARSubsystems;

namespace AndroidXRUnitySamples.DeviceDetector
{
    /// <summary>
    /// Class used to manage the Device Detector scene.
    /// </summary>
    public class DeviceDetector : MonoBehaviour
    {
        [SerializeField] private XRReferenceObjectLibrary _objectLibrary;
        [SerializeField] private GameObject _trackedObjectPrefab;
        [SerializeField] private LetterSpawner _letterSpawner;
        [SerializeField] private GameObject _lookingForKeyboardPrefab;
        [SerializeField] private SceneState _sceneState;
        [SerializeField] private float _keyboardLostDuration;
        [SerializeField] private AudioClipData _keyboardDetectedClip;

        private State _currentState;
        private float _keyboardLostTimer;
        private GameObject _lookingForKeyboard;

        private enum State
        {
            LookingForKeyboard,
            KeyboardFound,
        }

        private void Start()
        {
            Singleton.Instance.OriginManager.EnablePassthrough = true;

            // Prefab and library have to be set first before enabling the manager.
            Singleton.Instance.OriginManager.ObjectTrackingReferenceLibrary =
                _objectLibrary;
            Singleton.Instance.OriginManager.ObjectTrackingObjectPrefab =
                _trackedObjectPrefab;
            Singleton.Instance.OriginManager.EnableObjectTracking = true;

            _currentState = State.LookingForKeyboard;
            CreateLookingForKeyboard();
            _letterSpawner.gameObject.SetActive(false);
        }

        private void Update()
        {
            switch (_currentState)
            {
            case State.LookingForKeyboard:
                if (_sceneState.KeyboardTracked.Value)
                {
                    _currentState = State.KeyboardFound;
                    _letterSpawner.InitBounds(
                        _sceneState.KeyboardPos.Value,
                        _sceneState.KeyboardRot.Value);
                    _letterSpawner.gameObject.SetActive(true);
                    Singleton.Instance.Audio.PlayOneShot(
                        _keyboardDetectedClip, _sceneState.KeyboardPos.Value);
                    _lookingForKeyboard.GetComponent<ScaleOnEnable>().ScaleDownAndDestroy();
                    _lookingForKeyboard = null;
                    _keyboardLostTimer = _keyboardLostDuration;
                }
                else
                {
                    // Hide the "looking" message when the main menu is active.
                    if (Singleton.Instance.Menu.Active)
                    {
                        if (_lookingForKeyboard != null)
                        {
                            ScaleOnEnable son = _lookingForKeyboard.GetComponent<ScaleOnEnable>();
                            son.ScaleDownAndDestroy();
                            _lookingForKeyboard = null;
                        }
                    }
                    else
                    {
                        if (_lookingForKeyboard == null)
                        {
                            _lookingForKeyboard = Instantiate(_lookingForKeyboardPrefab);
                        }
                    }
                }

                break;
            case State.KeyboardFound:
                if (_sceneState.KeyboardTracked.Value)
                {
                    // Update keyboard state.
                    _keyboardLostTimer = _keyboardLostDuration;
                    _letterSpawner.UpdateAlpha(1.0f);
                    _letterSpawner.UpdateTargetTransformAndExtents(
                        _sceneState.KeyboardPos.Value,
                        _sceneState.KeyboardRot.Value,
                        _sceneState.KeyboardExtents.Value);
                }
                else
                {
                    // No keyboard tracked. Count down our timer.
                    _keyboardLostTimer -= Time.deltaTime;
                    _letterSpawner.UpdateAlpha(_keyboardLostTimer / _keyboardLostDuration);
                    if (_keyboardLostTimer <= 0.0f)
                    {
                        _letterSpawner.gameObject.SetActive(false);
                        CreateLookingForKeyboard();
                        _currentState = State.LookingForKeyboard;
                    }
                }

                break;
            }

#if UNITY_EDITOR
            if (Keyboard.current[Key.K].wasPressedThisFrame)
            {
                // Fake detect a keyboard.
                _sceneState.KeyboardTracked.Value = true;

                Transform t =  Singleton.Instance.Camera.transform;
                _sceneState.KeyboardPos.Value = t.position + t.forward;
                _sceneState.KeyboardRot.Value = t.rotation;

                _sceneState.KeyboardExtents.Value = Vector3.one;
            }
#endif  // UNITY_EDITOR
        }

        private void CreateLookingForKeyboard()
        {
            _lookingForKeyboard = Instantiate(_lookingForKeyboardPrefab);
        }
    }
}
