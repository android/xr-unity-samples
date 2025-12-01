// <copyright file="TrackableObjectSelector.cs" company="Google LLC">
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

using System.Collections.Generic;
using System.Linq;
using Google.XR.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

namespace AndroidXRUnitySamples.MarkerMaze
{
    /// <summary>
    /// Observes changes to the list of currently tracked objects, and filters for specific tracked
    /// objects changing tracking state on the list, emitting tracking lifecycle events as Unity
    /// events for that specific object.
    /// </summary>
    public class TrackableObjectSelector : MonoBehaviour
    {
        /// <summary>
        /// Event that is fired when an object is added.
        /// </summary>
        public UnityEvent<ARTrackedImage> ObjectAdded;

        /// <summary>
        /// Event that is fired when an object is updated.
        /// </summary>
        public UnityEvent<ARTrackedImage> ObjectUpdated;

        /// <summary>
        /// Event that is fired when an object is removed.
        /// </summary>
        public UnityEvent<ARTrackedImage> ObjectRemoved;

        [SerializeField]
        private ARTrackedImageManager _arTrackedObjectManager;

        [SerializeField]
        private int _trackableId;

        [SerializeField]
        private bool _trackQrCodes;

        [SerializeField]
        private float _timeUntilTrackingLostMillis = 1000;

        private bool _didReceiveTrackingUpdate;

        private float _timeSinceLastTrackingUpdateMillis;

        private ARTrackedImage _trackedObject;

        private void OnEnable()
        {
            if (_trackQrCodes)
            {
                _trackableId = "qrCode".GetHashCode();
            }

            _arTrackedObjectManager.trackablesChanged.AddListener(ProcessTrackedObjectChanges);
        }

        private void OnDisable()
        {
            _arTrackedObjectManager.trackablesChanged.RemoveListener(ProcessTrackedObjectChanges);
        }

        private void Update()
        {
            if (_didReceiveTrackingUpdate)
            {
                if (_timeSinceLastTrackingUpdateMillis > 0)
                {
                    ObjectAdded.Invoke(_trackedObject);
                }

                _timeSinceLastTrackingUpdateMillis = 0;
                _didReceiveTrackingUpdate = false;
            }
            else
            {
                _timeSinceLastTrackingUpdateMillis += Time.deltaTime;
                if (_timeUntilTrackingLostMillis > 0
                 && _timeSinceLastTrackingUpdateMillis > _timeUntilTrackingLostMillis)
                {
                    ObjectRemoved.Invoke(_trackedObject);
                }
            }
        }

        private void ProcessTrackedObjectChanges(
                ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
        {
            ARTrackedImage objAdded = SelectMatchingObject(eventArgs.added);
            if (objAdded != null)
            {
                _trackedObject = objAdded;
                ObjectAdded.Invoke(objAdded);
            }

            ARTrackedImage objUpdated = SelectMatchingObject(eventArgs.updated);
            if (objUpdated != null)
            {
                _trackedObject = objUpdated;
                _didReceiveTrackingUpdate = true;
                ObjectUpdated.Invoke(objUpdated);
            }

            ARTrackedImage objRemoved =
                    SelectMatchingObject(eventArgs.removed.Select(kvp => kvp.Value));
            if (objRemoved != null)
            {
                _trackedObject = objRemoved;
                ObjectRemoved.Invoke(objRemoved);
            }
        }

        private ARTrackedImage SelectMatchingObject(IEnumerable<ARTrackedImage> objects)
        {
            return objects.FirstOrDefault(o => GetTrackableID(o) == _trackableId);
        }

        private int GetTrackableID(ARTrackedImage trackedObject)
        {
            return trackedObject.IsMarker()
                           ? trackedObject.TryGetMarkerData(out XRMarkerDictionary dictionary,
                                     out int id)
                                     ? id
                                     : -1
                           : "qrCode".GetHashCode();
        }
    }
}
