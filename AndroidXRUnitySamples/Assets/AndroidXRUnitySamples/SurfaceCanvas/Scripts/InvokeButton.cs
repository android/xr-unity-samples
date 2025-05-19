// <copyright file="InvokeButton.cs" company="Google LLC">
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
using UnityEngine.UI;

namespace AndroidXRUnitySamples.SurfaceCanvas
{
    /// <summary>
    /// Simple shorthand for invoking a button's onClick event directly from another event trigger.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class InvokeButton : MonoBehaviour
    {
        private Button _button;

        /// <summary>
        /// Invokes this button's onClick event handler.
        /// </summary>
        public void Invoke()
        {
            if (_button != null && _button.onClick != null)
            {
                _button.onClick.Invoke();
            }
        }

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
        }
    }
}
