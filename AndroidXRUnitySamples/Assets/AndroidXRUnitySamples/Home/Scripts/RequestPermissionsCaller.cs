// <copyright file="RequestPermissionsCaller.cs" company="Google LLC">
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

using AndroidXRUnitySamples.MenusAndUI;
using Google.XR.Extensions;
using UnityEngine;

namespace AndroidXRUnitySamples.Home
{
    [RequireComponent(typeof(ShadowButton))]
    class RequestPermissionsCaller : MonoBehaviour
    {
        [SerializeField] private AndroidXRPermission[] _permissions;
        [SerializeField] private StatusDashboard _statusDashboard;

        private void Start()
        {
            GetComponent<ShadowButton>().OnPress.AddListener(OnPress);
        }

        private void OnPress()
        {
            _statusDashboard.RequestUserPermissions(_permissions);
        }
    }
}
