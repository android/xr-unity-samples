// <copyright file="MarkerMaze.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.MarkerMaze
{
    /// <summary>
    /// Class used to manage the Marker Maze scene.
    /// </summary>
    public class MarkerMaze : MonoBehaviour
    {
        [SerializeField] private ARTrackedImageManager _arTrackedObjectManager;
        [SerializeField] private GameObject _lookingForMarkerPrefab;
        [SerializeField] private float _markerLostDuration;

        private State _currentState;
        private float _markerLostTimer;
        private GameObject _lookingForMarker;
        private int _trackedMarkerCount;
        private bool _atLeastOneMarkerTracked;

        private enum State
        {
            LookingForMarker,
            MarkerFound,
        }

        private void Start()
        {
            _arTrackedObjectManager.trackablesChanged.AddListener(ProcessTrackedObjectChanges);
            _atLeastOneMarkerTracked = false;
            _trackedMarkerCount = 0;
            _currentState = State.LookingForMarker;
            CreateLookingForMarker();
        }

        private void OnDestroy()
        {
            _arTrackedObjectManager.trackablesChanged.RemoveListener(ProcessTrackedObjectChanges);
        }

        private void Update()
        {
            switch (_currentState)
            {
            case State.LookingForMarker:
                if (_atLeastOneMarkerTracked)
                {
                    _currentState = State.MarkerFound;
                    _lookingForMarker.GetComponent<ScaleOnEnable>().ScaleDownAndDestroy();
                    _lookingForMarker = null;
                    _markerLostTimer = _markerLostDuration;
                }
                else
                {
                    // Hide the "looking" message when the main menu is active.
                    if (Singleton.Instance.Menu.Active)
                    {
                        if (_lookingForMarker != null)
                        {
                            ScaleOnEnable son = _lookingForMarker.GetComponent<ScaleOnEnable>();
                            son.ScaleDownAndDestroy();
                            _lookingForMarker = null;
                        }
                    }
                    else
                    {
                        if (_lookingForMarker == null)
                        {
                            _lookingForMarker = Instantiate(_lookingForMarkerPrefab);
                        }
                    }
                }

                break;
            case State.MarkerFound:
                if (_atLeastOneMarkerTracked)
                {
                    // Update marker state.
                    _markerLostTimer = _markerLostDuration;
                }
                else
                {
                    // No marker tracked. Count down our timer.
                    _markerLostTimer -= Time.deltaTime;
                    if (_markerLostTimer <= 0.0f)
                    {
                        CreateLookingForMarker();
                        _currentState = State.LookingForMarker;
                    }
                }

                break;
            }
        }

        private void CreateLookingForMarker()
        {
            _lookingForMarker = Instantiate(_lookingForMarkerPrefab);
        }

        private void ProcessTrackedObjectChanges(
                ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
        {
            _trackedMarkerCount += eventArgs.added.Count;
            _trackedMarkerCount -= eventArgs.removed.Count;
            _atLeastOneMarkerTracked = _trackedMarkerCount > 0;
        }
    }
}
