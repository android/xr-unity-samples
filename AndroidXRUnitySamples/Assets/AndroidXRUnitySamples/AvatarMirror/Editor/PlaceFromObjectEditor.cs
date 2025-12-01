// <copyright file="PlaceFromObjectEditor.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.AvatarMirror.Editor
{
    /// <summary>
    /// Custom editor for the <see cref="PlaceFromObject"/> component,
    /// providing a "Place Now" button in the Inspector for immediate positioning.
    /// </summary>
    [CustomEditor(typeof(PlaceFromObject))]
    public class PlaceFromObjectEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Draws the custom inspector GUI for the <see cref="PlaceFromObject"/> component.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Get the target component as PlaceFromObject
            PlaceFromObject myComponent = (PlaceFromObject)target;

            // Draw the default inspector fields
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();

            // Add a button
            if (GUILayout.Button("Place Now"))
            {
                myComponent.PlaceNow();
            }
        }
    }
}
