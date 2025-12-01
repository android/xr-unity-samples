// <copyright file="InjectManifestPermissions.cs" company="Google LLC">
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Google.XR.Extensions;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AndroidXRUnitySamples.Gemini.Editor
{
    internal class InjectManifestPermissions : IPostGenerateGradleAndroidProject
    {
        private readonly HashSet<string> _requiredPermissions = new HashSet<string>();

        [SuppressMessage("UnityRules.UnityStyleRules",
                         "US1109:PublicPropertiesMustBeUpperCamelCase",
                         Justification = "Overridden property.")]
        public int callbackOrder => 999;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            _requiredPermissions.Clear();

            List<string> buildScenes = EditorBuildSettings.scenes.Where(scene => scene.enabled)
                                           .Select(scene => scene.path)
                                           .ToList();

            if (buildScenes.Count == 0)
            {
                Debug.LogWarning("InjectManifestPermission: " +
                                 "No enabled scenes found in build settings.");
                return;
            }

            foreach (string scenePath in buildScenes)
            {
                try
                {
                    ScanSceneForPermissions(scenePath);
                }
                catch (Exception e)
                {
                    Debug.LogError("InjectManifestPermission: " +
                                   $"Error scanning scene {scenePath}: {e}");
                }
            }

            if (_requiredPermissions.Count == 0)
            {
                Debug.Log("InjectManifestPermission: " +
                          "No permissions found in any scene. No permission injection needed.");
                return;
            }

            string manifestPath = GetManifestPath(path);
            if (string.IsNullOrEmpty(manifestPath) || !File.Exists(manifestPath))
            {
                Debug.LogError(
                    "InjectManifestPermission: " +
                    $"Could not find AndroidManifest.xml in build output path: {manifestPath}");
                return;
            }

            try
            {
                XDocument manifestDoc = XDocument.Load(manifestPath);
                XNamespace androidNs = "http://schemas.android.com/apk/res/android";

                XElement manifestElement = manifestDoc.Root;
                if (manifestElement == null)
                {
                    Debug.LogError("InjectManifestPermission: " +
                                   "Invalid AndroidManifest.xml structure (no root element).");
                    return;
                }

                HashSet<string> existingPermissions =
                    manifestElement.Elements("uses-permission")
                        .Select(elem => elem.Attribute(androidNs + "name")?.Value)
                        .Where(name => name != null)
                        .ToHashSet();

                foreach (string permission in _requiredPermissions)
                {
                    if (!existingPermissions.Contains(permission))
                    {
                        var permissionElement = new XElement(
                            "uses-permission", new XAttribute(androidNs + "name", permission));

                        XElement applicationElement = manifestElement.Element("application");
                        if (applicationElement == null)
                        {
                            manifestElement.Add(permissionElement);
                        }
                        else
                        {
                            applicationElement.AddBeforeSelf(permissionElement);
                        }

                        Debug.Log("InjectManifestPermission: " +
                                  $"Added permission '{permission}' to manifest.");
                    }
                }

                manifestDoc.Save(manifestPath);
                Debug.Log("InjectManifestPermission: " +
                          "Successfully updated manifest with required permissions.");
            }
            catch (Exception e)
            {
                Debug.LogError("InjectManifestPermission: " +
                               $"Error processing manifest file: {e}");
            }
        }

        private string GetManifestPath(string buildOutputPath)
        {
            string gradleProjectPath = buildOutputPath;

            string[] possibleManifestPaths =
            {
                Path.Combine(gradleProjectPath, "src", "main", "AndroidManifest.xml"),
                Path.Combine(gradleProjectPath, "launcher", "src",
                        "main", "AndroidManifest.xml"),
                Path.Combine(gradleProjectPath, "unityLibrary", "src",
                        "main", "AndroidManifest.xml")
            };

            foreach (string path in possibleManifestPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            if (File.Exists(Path.Combine(buildOutputPath, "AndroidManifest.xml")))
            {
                return Path.Combine(buildOutputPath, "AndroidManifest.xml");
            }

            Debug.LogWarning(
                "InjectManifestPermission: Could not find AndroidManifest.xml in expected " +
                $"Gradle project locations within '{buildOutputPath}'.");
            return null;
        }

        private void ScanSceneForPermissions(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            List<AndroidXRPermissionUtil> permissionUtils =
                scene.GetRootGameObjects()
                    .SelectMany(go => go.GetComponentsInChildren<AndroidXRPermissionUtil>(true))
                    .ToList();

            foreach (AndroidXRPermissionUtil util in permissionUtils)
            {
                foreach (AndroidXRPermission xrPermission in util.AndroidXRPermissions)
                {
                    _requiredPermissions.Add(xrPermission.ToPermissionString());
                }

                foreach (string androidPermission in util.GenernalAndroidPermissions)
                {
                    _requiredPermissions.Add(androidPermission);
                }
            }

            EditorSceneManager.CloseScene(scene, false);
        }
    }
}
