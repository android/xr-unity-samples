// <copyright file="VariableEditor.cs" company="Google LLC">
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

using AndroidXRUnitySamples.Variables;
using UnityEngine;

namespace AndroidXRUnitySamples.Editor
{
    /// <summary>
    /// <see cref="VariableEditor{T}" /> subclasses implement Custom editors for
    /// Scriptable Object <see cref="Variable{T}" /> types.
    /// <br />
    /// <br />
    /// Displays a count of listeners subscribed to this variables' changes.
    /// </summary>
    /// <typeparam name="T">The target type of the variable editor.</typeparam>
    public abstract class VariableEditor<T> : UnityEditor.Editor
    {
        /// <summary>
        /// <inheritdoc cref="Editor.OnInspectorGUI" />
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // Only enable invoking when playing.
            GUI.enabled = Application.isPlaying;

            var variable = target as Variable<T>;
            if (GUILayout.Button("Invoke"))
            {
                variable.Invoke(variable.Value);
            }

            GUILayout.Label($"Listener count: {variable.ListenerCount}");
        }
    }
}
