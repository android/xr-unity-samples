// <copyright file="SimpleInDisableWhenNotFacingCamera.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Simple behavior for disabling an object when not facing the camera.
    /// </summary>
    public class SimpleInDisableWhenNotFacingCamera : MonoBehaviour
    {
        [SerializeField] private GameObject _object;

        private void Update()
        {
            Transform camXf = Singleton.Instance.Camera.transform;
            Vector3 camToUs = transform.position - camXf.position;
            _object.SetActive(Vector3.Dot(camToUs.normalized, _object.transform.forward) > 0.0f);
        }
    }
}
