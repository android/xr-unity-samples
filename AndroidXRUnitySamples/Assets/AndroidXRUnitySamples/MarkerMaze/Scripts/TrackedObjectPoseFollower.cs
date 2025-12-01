// <copyright file="TrackedObjectPoseFollower.cs" company="Google LLC">
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

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

namespace AndroidXRUnitySamples.MarkerMaze
{
    /// <summary>
    /// Follows the tracking pose of a tracked object, updating a target transform
    /// in tandem with the tracking pose of a tracked real world object.
    /// </summary>
    [RequireComponent(typeof(TrackableObjectSelector))]
    public class TrackedObjectPoseFollower : MonoBehaviour
    {
        /// <summary>
        /// Event that is fired when tracking of an object is paused.
        /// </summary>
        public UnityEvent TrackingPaused;

        /// <summary>
        /// Event that is fired when tracking of an object is resumed.
        /// </summary>
        public UnityEvent TrackingResumed;

        /// <summary>
        /// Apply scale to the virtual object.
        /// </summary>
        public Vector3 ObjectScale = Vector3.one;
        private const int _trackingTimoutMS = 1000;

        private DateTime _lastUpdated;

        private TrackableObjectSelector _trackedObject;

        private Vector3 _lastRawPosition;

        private Quaternion _lastRawQuaternion;

        [SerializeField]
        private Transform _followerTransform;

        [SerializeField]
        private bool _followPosition = true;

        [SerializeField]
        private Vector3 _followPositionScale = Vector3.one;

        [SerializeField]
        private bool _followRotation = true;

        [SerializeField]
        private Vector3 _followRotationScale = Vector3.one;

        [SerializeField]
        private QuaternionFilter _quaternionFilter;

        [SerializeField]
        private PositionFilter _positionFilter;

        /// <summary>
        /// Gets a value indicating whether tracking for this object is active.
        /// </summary>
        public bool TrackingActive
        {
            get;
            private set;
        }

        private void Awake()
        {
            _trackedObject = GetComponent<TrackableObjectSelector>();
        }

        private void OnEnable()
        {
            _trackedObject.ObjectAdded.AddListener(FollowTrackedObjectPose);
            _trackedObject.ObjectUpdated.AddListener(FollowTrackedObjectPose);
        }

        private void OnDisable()
        {
            _trackedObject.ObjectAdded.RemoveListener(FollowTrackedObjectPose);
            _trackedObject.ObjectUpdated.RemoveListener(FollowTrackedObjectPose);
        }

        private void Update()
        {
            UpdateFilteredPose();

            if ((DateTime.Now - _lastUpdated).TotalMilliseconds >= _trackingTimoutMS)
            {
                if (TrackingActive)
                {
                    TrackingActive = false;
                    TrackingPaused.Invoke();
                }
            }
            else if (!TrackingActive)
            {
                TrackingActive = true;
                TrackingResumed.Invoke();
            }
        }

        private void FollowTrackedObjectPose(ARTrackedImage trackedObject)
        {
            if (_lastRawPosition != trackedObject.transform.position ||
                _lastRawQuaternion != trackedObject.transform.rotation)
            {
                _lastRawPosition = trackedObject.transform.position;
                _lastRawQuaternion = trackedObject.transform.rotation;
                ObjectScale = trackedObject.extents;
                _lastUpdated = DateTime.Now;
            }
        }

        private void UpdateFilteredPose()
        {
            if (_followPosition)
            {
                Vector3 filteredPosition = _positionFilter != null
                                                   ? _positionFilter.Filter(_lastRawPosition)
                                                   : _lastRawPosition;
                filteredPosition.Scale(_followPositionScale);
                _followerTransform.position = filteredPosition;
            }

            if (_followRotation)
            {
                Quaternion filteredQuaternion =
                        _quaternionFilter != null
                                ? _quaternionFilter.Filter(_lastRawQuaternion)
                                : _lastRawQuaternion;
                Vector3 euler = filteredQuaternion.eulerAngles;
                euler.Scale(_followRotationScale);
                filteredQuaternion = Quaternion.Euler(euler);
                _followerTransform.rotation = filteredQuaternion;
            }
        }
    }
}
