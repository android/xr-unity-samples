// <copyright file="GestureRun.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.GestureRun
{
    /// <summary>
    /// Class used to setup the GestureRun scene.
    /// </summary>
    public class GestureRun : MonoBehaviour
    {
        /// <summary>
        /// GameObject used as the room environment.
        /// </summary>
        public GameObject RoomEnvironment;

        void Start()
        {
            Singleton.Instance.OriginManager.EnablePassthrough = true;

#if !UNITY_EDITOR
            RoomEnvironment.SetActive(false);
#endif

            Reposition();
        }

        void Reposition()
        {
            var camera = Singleton.Instance.Camera;
            Vector3 pos = camera.transform.position;
            Vector3 offset = camera.transform.forward;
            offset.y = 0.0f;
            offset.Normalize();

            transform.position = pos;
            transform.forward = offset;
        }
    }
}
