// <copyright file="InfoPanel.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// Helper script for InfoPanel object.
    /// </summary>
    public class InfoPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _infoText;

        /// <summary>
        /// Sets info text on panel.
        /// </summary>
        /// <param name="infoText">The string to set the text to.</param>
        public void SetText(string infoText)
        {
            _infoText.text = infoText;
        }
    }
}
