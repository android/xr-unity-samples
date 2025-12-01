// <copyright file="FeatureTagCatalogEditor.cs" company="Google LLC">
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

using System;
using UnityEditor;
using UnityEngine;

namespace AndroidXRUnitySamples.Editor
{
    /// <summary>
    /// Custom editor for the FeatureTagCatalog.
    /// </summary>
    [CustomEditor(typeof(FeatureTagCatalog))]
    public class FeatureTagCatalogEditor : UnityEditor.Editor
    {
        private SerializedProperty _tagsProperty;
        private string[] _featureTypeNames;

        /// <summary>
        /// Makes the selection dropdown human readable.
        /// </summary>
        public override void OnInspectorGUI()
        {
            // Update the serialized object's representation
            serializedObject.Update();

            // Ensure the array size matches the enum count.
            // This is a second check in case OnValidate() hasn't run yet.
            if (_tagsProperty.arraySize != _featureTypeNames.Length)
            {
                _tagsProperty.arraySize = _featureTypeNames.Length;
            }

            EditorGUILayout.LabelField("Feature Tags", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            for (int i = 0; i < _featureTypeNames.Length; i++)
            {
                SerializedProperty element = _tagsProperty.GetArrayElementAtIndex(i);
                SerializedProperty nameProp = element.FindPropertyRelative("FeatureName");
                if (string.IsNullOrEmpty(nameProp.stringValue))
                {
                    nameProp.stringValue = _featureTypeNames[i];
                }

                // Draw the property field using the enum name as the label
                EditorGUILayout.PropertyField(element, new GUIContent(_featureTypeNames[i]), true);
            }

            EditorGUI.indentLevel--;

            // Apply any changes made in the Inspector
            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _tagsProperty = serializedObject.FindProperty("Tags");
            _featureTypeNames = Enum.GetNames(typeof(FeatureTagCatalog.Feature));
        }
    }
}
