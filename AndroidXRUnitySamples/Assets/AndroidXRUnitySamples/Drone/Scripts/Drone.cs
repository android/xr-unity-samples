// <copyright file="Drone.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.Drone
{
    /// <summary>
    /// State machine for the drone scene.
    /// </summary>
    public class Drone : MonoBehaviour
    {
        [SerializeField] private MeshFilter _sceneMeshPrefab;
        [SerializeField] private DronePhysics _drone;
        [SerializeField] private GameObject _howToPlayPanelPrefab;
        [SerializeField] private float _howToPlayMinShowDuration;
        [SerializeField] private InputActionReference _resetDroneAction;

        private GameObject _howToPlayPanel;
        private float _howToPlayTimer;
        private State _currentState;

        private enum State
        {
            Intro,
            Standard,
        }

        private void Start()
        {
            Singleton.Instance.OriginManager.MeshManagerMeshPrefab = _sceneMeshPrefab;
            Singleton.Instance.OriginManager.EnableMeshManager = true;
            _howToPlayTimer = _howToPlayMinShowDuration;
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
                    if (_resetDroneAction.action.triggered)
                    {
                        _drone.ResetDrone();
                        _howToPlayPanel.GetComponent<ScaleOnEnable>().ScaleDownAndDestroy();
                        _howToPlayPanel = null;
                        _currentState = State.Standard;
                    }
                }
            }
        }

        private void UpdateStandard()
        {
            if (_resetDroneAction.action.triggered)
            {
                _drone.ResetDrone();
            }
        }
    }
}
