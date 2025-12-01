// <copyright file="AvatarControllerEditor.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.AvatarMirror.Editor  // Consistent namespace
{
    /// <summary>
    /// Custom editor for the <see cref="AvatarController"/> component,
    /// providing a "Setup New Avatar" button in the Inspector.
    /// </summary>
    [CustomEditor(typeof(AvatarController))]
    public class AvatarControllerEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Draws the custom inspector GUI for the <see cref="AvatarController"/> component.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Get the target component as AvatarController
            AvatarController avatarController = (AvatarController)target;

            // Update the serialized object to ensure changes are reflected
            serializedObject.Update();

            // Draw all default inspector properties except for the 'm_Script' field,
            // which is automatically handled by Unity and doesn't need to be exposed.
            DrawPropertiesExcluding(serializedObject, "m_Script");

            // Apply any modifications made in the inspector to the serialized object
            serializedObject.ApplyModifiedProperties();

            // Add some vertical space for better layout before the button
            EditorGUILayout.Space();

            // Add a custom button to trigger the SetupNewAvatar method
            if (GUILayout.Button("Setup New Avatar"))
            {
                avatarController.SetupNewAvatar();
            }
        }
    }
}
