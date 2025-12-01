// <copyright file="FollowObjectAndFaceCamera.cs" company="Google LLC">
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
using UnityEngine.Serialization;

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// Positions and rotates this GameObject to face another object,
    /// with an optional offset and deactivation behavior when too close.
    /// </summary>
    [ExecuteInEditMode]
    public class FollowObjectAndFaceCamera : MonoBehaviour
    {
        /// <summary>The object this GameObject will face.</summary>
        [Tooltip("The object this GameObject will face.")]
        [SerializeField] public Transform ObjectToFace;

        [Tooltip("The object whose position this GameObject will follow.")]
        [SerializeField] private Transform _objectToFollow;

        [Tooltip("An offset applied to this GameObject's position relative to the object it " +
                 "follows, in its local space.")]
        [SerializeField] private Vector3 _offset;

        [Tooltip("An optional child GameObject to deactivate if this object gets too close to " +
                 "the object it's facing.")]
        [SerializeField] private GameObject _childToDeactivateIfTooClose = null;

        [Space]

        [Tooltip("The distance threshold at which the child GameObject will be deactivated.")]
        [SerializeField] private float _threshold = 0.3f;

        /// <summary>
        /// Called once per frame. Updates the position and rotation of this GameObject.
        /// </summary>
        void Update()
        {
            if (_objectToFollow == null || ObjectToFace == null)
            {
                Debug.LogWarning("FaceObject: 'Object To Follow' or 'Object To Face' is not " +
                                     "assigned. Please assign them in the inspector.",
                                 this);
                return;
            }

            float distance = Vector3.Distance(_objectToFollow.position, ObjectToFace.position);

            // Handle deactivation of child if too close
            if (_childToDeactivateIfTooClose != null)
            {
                if (distance < _threshold)
                {
                    if (_childToDeactivateIfTooClose.activeSelf)
                    {
                        _childToDeactivateIfTooClose.SetActive(false);
                    }

                    return;  // Stop updating position/rotation if too close and child is handled
                }
                else
                {
                    if (!_childToDeactivateIfTooClose.activeSelf)
                    {
                        _childToDeactivateIfTooClose.SetActive(true);
                    }
                }
            }

            // Set rotation to look from this object's position towards _objectToFace's position
            // with Vector3.up as the world up direction.
            transform.rotation =
                Quaternion.LookRotation(transform.position - ObjectToFace.position, Vector3.up);

            // Set position based on _objectToFollow's position and the local offset rotated by this
            // object's new rotation.
            transform.position =
                _objectToFollow.transform.position + (transform.rotation * _offset);
        }
    }
}
