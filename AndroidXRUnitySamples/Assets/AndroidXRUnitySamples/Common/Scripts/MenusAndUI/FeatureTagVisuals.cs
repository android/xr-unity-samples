// <copyright file="FeatureTagVisuals.cs" company="Google LLC">
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

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AndroidXRUnitySamples.MenusAndUI
{
    /// <summary>
    /// Accessor class for feature tags on experience menu popups.
    /// </summary>
    public class FeatureTagVisuals : MonoBehaviour
    {
        /// <summary> Text background for tinting. </summary>
        public Image Background;

        /// <summary> Text field for showing name. </summary>
        public TMP_Text Name;
    }
}
