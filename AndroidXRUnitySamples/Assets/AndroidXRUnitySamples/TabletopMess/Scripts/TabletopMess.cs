// <copyright file="TabletopMess.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.TabletopMess
{
    /// <summary>
    /// Scene loader for the TabletopMess experience.
    /// </summary>
    public class TabletopMess : MonoBehaviour
    {
        [SerializeField] private SceneState _sceneState;
        [SerializeField] private GameObject _planePrefab;

        [Header("UI")]
        [SerializeField] private GameObject _lookingForPlanePrefab;
        [SerializeField] private float _planeLostDuration;
        [SerializeField] private GameObject _objectPlacementPanelPrefab;

        private State _currentState;
        private float _planeLostTimer;
        private GameObject _lookingForPlane;
        private GameObject _objectPlacementPanel;

        private enum State
        {
            LookingForPlane,
            PlaneFound,
        }

        private void Start()
        {
            Singleton.Instance.OriginManager.EnablePlaneDetection = true;
            Singleton.Instance.OriginManager.PlanePrefab = _planePrefab;

            _sceneState.OnSpawnNewObject.AddListener(HandleSpawnNewObject);

            _currentState = State.LookingForPlane;
        }

        private void OnDestroy()
        {
            _sceneState.OnSpawnNewObject.RemoveListener(HandleSpawnNewObject);
        }

        private void Update()
        {
            switch (_currentState)
            {
            case State.LookingForPlane:
                if (_sceneState.PlaneTracked.Value)
                {
                    _currentState = State.PlaneFound;

                    _lookingForPlane.GetComponent<ScaleOnEnable>().ScaleDownAndDestroy();
                    _lookingForPlane = null;
                }
                else
                {
                    // Hide the "looking" message when the main menu is active.
                    if (Singleton.Instance.Menu.Active)
                    {
                        if (_lookingForPlane != null)
                        {
                            _lookingForPlane.GetComponent<ScaleOnEnable>().ScaleDownAndDestroy();
                            _lookingForPlane = null;
                        }
                    }
                    else
                    {
                        if (_lookingForPlane == null)
                        {
                            _lookingForPlane = Instantiate(_lookingForPlanePrefab);
                        }
                    }
                }

                break;
            case State.PlaneFound:
                if (!_sceneState.PlaneTracked.Value)
                {
                    // No plane tracked. Count down our timer.
                    _planeLostTimer -= Time.deltaTime;
                    if (_planeLostTimer <= 0.0f)
                    {
                        _objectPlacementPanel.GetComponent<ScaleOnEnable>().ScaleDownAndDestroy();
                        _objectPlacementPanel = null;
                        _currentState = State.LookingForPlane;
                    }
                }
                else
                {
                    // Hide the object placement UI when the main menu is active.
                    if (Singleton.Instance.Menu.Active)
                    {
                        if (_objectPlacementPanel != null)
                        {
                            ScaleOnEnable s = _objectPlacementPanel.GetComponent<ScaleOnEnable>();
                            s.ScaleDownAndDestroy();
                            _objectPlacementPanel = null;
                        }
                    }
                    else
                    {
                        if (_objectPlacementPanel == null)
                        {
                            _objectPlacementPanel = Instantiate(_objectPlacementPanelPrefab);
                        }
                    }
                }

                break;
            }
        }

        private void HandleSpawnNewObject(SpawnNewObjectEvent.Info info)
        {
            GameObject prefab = _sceneState.GetObjectPrefab(info.Type);
            GameObject o = Instantiate(prefab);
            o.transform.position = info.Origin;
        }
    }
}
