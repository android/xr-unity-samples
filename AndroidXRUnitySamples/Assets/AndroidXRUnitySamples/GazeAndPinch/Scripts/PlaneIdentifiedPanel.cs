// <copyright file="PlaneIdentifiedPanel.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.GazeAndPinch
{
    /// <summary>
    /// Class used for controlling info message on plane identified panel.
    /// </summary>
    public class PlaneIdentifiedPanel : MonoBehaviour
    {
        /// <summary>
        /// Object to enable when identified plane is valid.
        /// </summary>
        public GameObject ValidObjects;

        /// <summary>
        /// Object to enable when identified plane is invalid.
        /// </summary>
        public GameObject InvalidObjects;

        /// <summary>Function to toggle objects based on validity.</summary>
        /// <param name="valid">If we should show the valid elements.</param>
        public void SetValid(bool valid)
        {
            ValidObjects.SetActive(valid);
            InvalidObjects.SetActive(!valid);
        }
    }
}
