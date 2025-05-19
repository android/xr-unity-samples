// <copyright file="ExperienceSettings.cs" company="Google LLC">
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
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Settings for a specific experience in AndroidXRUnitySamples. An experience is the minimal unit that
    /// demonstrates a feature or an interaction paradigm in AndroidXRUnitySamples.
    /// </summary>
    [CreateAssetMenu(menuName = "AndroidXRUnitySamples/Experience Settings", order = 3)]
    public class ExperienceSettings : ScriptableObject
    {
        /// <summary>
        /// Name of the experience.
        /// </summary>
        public string ExperienceName;

        /// <summary>
        /// Name of the group, for arranging on the Menu.
        /// </summary>
        public string ExperienceGroup;

        /// <summary>
        /// A <see cref="UnityEngine.Sprite" /> image used to display an
        /// icon next to the name in the Menu.
        /// </summary>
        public Sprite Icon;

        /// <summary>
        /// A <see cref="UnityEngine.Sprite" /> image used to display an
        /// experience cover image during experience selection.
        /// </summary>
        public Sprite Sprite;

        /// <summary>
        /// A description of the experience.
        /// </summary>
        [Multiline]
        public string Description;

        /// <summary>
        /// Path to the scene asset.
        /// </summary>
        public string ScenePath;

        /// <summary>
        /// List of supported input modes for this experience.
        /// </summary>
        public List<XRInputModalityManager.InputMode> SupportedInputModes;

        /// <summary>
        /// Flag to force disable experience in the main menu.
        /// </summary>
        public bool ForceDisable;
    }
}
