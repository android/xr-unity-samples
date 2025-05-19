// <copyright file="ReserializeAssets.cs" company="Google LLC">
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
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AndroidXRUnitySamples.Editor
{
    /// <summary>
    /// Utilities for manually triggering reserialization of assets in the project.
    /// </summary>
    public static class ReserializeAssets
    {
        /// <summary>
        /// Forces reserialization of all project assets. Use this after using the
        /// refactoring, to write the serialized values under the new name, and you should be able
        /// can cause changes to every single file in the project.
        /// </summary>
        [MenuItem("AndroidXRUnitySamples/Project/Reserialize All Assets")]
        public static void ReserializeAllProjectAssets()
        {
            float startTime = Time.realtimeSinceStartup;
            Debug.Log("Generating asset paths list... ");

            string[] assetsFolderSearchFilter =
                    new[]
                            {
                                    "Assets"
                            }
                            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                            .ToArray();

            // Gets the paths only from assets in the Assets folder.
            string[] paths = AssetDatabase.FindAssets(string.Empty, assetsFolderSearchFilter)
                                          .ToArray();

            float endTime = Time.realtimeSinceStartup;
            Debug.Log($"Asset paths list generated in {endTime - startTime} seconds, " +
                      $"{paths.Length} assets to reserialize.");

            startTime = Time.realtimeSinceStartup;
            Debug.Log("Begin reserialization of all project assets... " +
                      $"{paths.Length} assets to reserialize.");

            AssetDatabase.ForceReserializeAssets(paths);

            endTime = Time.realtimeSinceStartup;
            Debug.Log("Reserialization of all project assets completed in " +
                      $"{endTime - startTime} seconds for {paths.Length} assets.");
        }

        /// <summary>
        /// Reserializes the selected project assets. Use this after renaming a Unity serialized
        /// field and you should be able to remove the FormerlySerializedAs attributes.
        /// </summary>
        [MenuItem("Assets/Reserialize Assets")]
        public static void ReserializeProjectAssets()
        {
            Object[] assets = Selection.objects;
            float startTime = Time.realtimeSinceStartup;
            Debug.Log("Reserializing assets... " + $"{assets.Count()} assets to reserialize.");
            IEnumerable<string> paths = assets.Select(AssetDatabase.GetAssetPath);
            AssetDatabase.ForceReserializeAssets(paths);

            float endTime = Time.realtimeSinceStartup;
            Debug.Log("Reserialization of all project assets completed in " +
                      $"{endTime - startTime} seconds for {assets.Count()} assets.");
        }
    }
}
