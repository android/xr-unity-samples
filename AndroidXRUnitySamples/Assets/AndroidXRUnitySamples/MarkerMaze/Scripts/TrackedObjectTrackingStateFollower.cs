// <copyright file="TrackedObjectTrackingStateFollower.cs" company="Google LLC">
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
    /// Follows the tracking state of a tracked object, enabling and/or disabling a target object
    /// in tandem with the tracking lifecycle events of a tracked real world object.
    /// </summary>
    [RequireComponent(typeof(TrackableObjectSelector))]
    public class TrackedObjectTrackingStateFollower : MonoBehaviour
    {
        private TrackableObjectSelector _trackedObject;

        [SerializeField]
        private GameObject _trackedStateFollower;

        [SerializeField]
        private bool _activateWhenTrackingFound;

        [SerializeField]
        private bool _deactivateWhenTrackingLost;

        private void Awake()
        {
            _trackedObject = GetComponent<TrackableObjectSelector>();
        }

        private void OnEnable()
        {
            _trackedObject.ObjectAdded.AddListener(EnableStateFollowerObject);
            _trackedObject.ObjectUpdated.AddListener(EnableStateFollowerObject);
            _trackedObject.ObjectRemoved.AddListener(DisableStateFollowerObject);
        }

        private void OnDisable()
        {
            _trackedObject.ObjectAdded.RemoveListener(EnableStateFollowerObject);
            _trackedObject.ObjectUpdated.RemoveListener(EnableStateFollowerObject);
            _trackedObject.ObjectRemoved.RemoveListener(DisableStateFollowerObject);
        }

        private void EnableStateFollowerObject(ARTrackedImage trackedObject)
        {
            if (_activateWhenTrackingFound)
            {
                _trackedStateFollower.SetActive(true);
            }
        }

        private void DisableStateFollowerObject(ARTrackedImage trackedObject)
        {
            if (_deactivateWhenTrackingLost)
            {
                _trackedStateFollower.SetActive(false);
            }
        }
    }
}
