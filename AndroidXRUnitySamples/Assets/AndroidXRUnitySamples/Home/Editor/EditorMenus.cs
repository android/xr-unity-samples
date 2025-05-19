// <copyright file="EditorMenus.cs" company="Google LLC">
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
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AndroidXRUnitySamples.Home.Editor
{
    /// <summary>
    /// Editor menu entries for editor features.
    /// </summary>
    public class EditorMenus
    {
        private const string _settingsPath =
                "Assets/AndroidXRUnitySamples/Common/ScriptableObjects/Settings.asset";

        [MenuItem("AndroidXRUnitySamples/Repopulate Scene List")]
        private static void RepopulateSceneLists()
        {
            Settings settings = GetSettings();

            if (settings == null)
            {
                Debug.LogError($"Unable to find settings at \"{_settingsPath}\"");
            }

            var ilsScenes = new Dictionary<string, EditorBuildSettingsScene>();

            ilsScenes.Add(settings.HomeScenePath,
                    new EditorBuildSettingsScene(settings.HomeScenePath, true));

            bool hasError = false;

            foreach (ExperienceSettings experience in settings.Experiences)
            {
                if (!IsValidScenePath(experience.ScenePath))
                {
                    Debug.LogError(
                            $"Cannot find scene \"{experience.ScenePath}\" for "
                            + $"experience \"{experience.name}\"");
                    hasError = true;
                    continue;
                }

                if (!ilsScenes.ContainsKey(experience.ScenePath))
                {
                    ilsScenes.Add(experience.ScenePath,
                            new EditorBuildSettingsScene(experience.ScenePath, true));
                }
            }

            if (hasError)
            {
                throw new Exception("Found settings error. Check the logs above.");
            }

            var scenesToBuild = new EditorBuildSettingsScene[ilsScenes.Count];
            ilsScenes.Values.CopyTo(scenesToBuild, 0);

            EditorBuildSettings.scenes = scenesToBuild;
        }

        private static Settings GetSettings()
        {
            return AssetDatabase.LoadAssetAtPath(
                _settingsPath, typeof(Settings)) as Settings;
        }

        private static bool IsValidScenePath(string scenePath)
        {
            var scene = AssetDatabase.LoadAssetAtPath(scenePath, typeof(SceneAsset)) as SceneAsset;
            return scene != null;
        }
    }
}
