// <copyright file="EventEditor.cs" company="Google LLC">
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

using UnityEditor;
using UnityEngine;
using Event = AndroidXRUnitySamples.Events.Event;

namespace AndroidXRUnitySamples.Editor
{
    /// <summary>
    /// Implements a Custom editor for the Scriptable Object parameterless
    /// <see cref="Events.Event" /> type.
    /// <para />
    /// Displays a count of listeners subscribed to this variables' changes.
    /// </summary>
    [CustomEditor(typeof(Events.Event))]
    public class EventEditor : UnityEditor.Editor
    {
        /// <summary>
        /// <inheritdoc cref="Editor.OnInspectorGUI" />
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // Only enable invoking when playing.
            GUI.enabled = Application.isPlaying;

            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Simulate Event", EditorStyles.boldLabel);
            EventValueInputField();
            Events.Event evt = EventInvokeButton();
            GUILayout.Label($"Listener count: {evt.ListenerCount}");
        }

        private void EventValueInputField()
        {
        }

        private Events.Event EventInvokeButton()
        {
            var evt = target as Events.Event;
            if (GUILayout.Button("Invoke"))
            {
                evt.Invoke();
            }

            return evt;
        }
    }
}
