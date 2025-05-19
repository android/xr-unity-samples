// <copyright file="SceneManager.cs" company="Google LLC">
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Manages the scene loading flow in AndroidXRUnitySamples.
    /// </summary>
    public class SceneManager : MonoBehaviour
    {
        /// <summary>
        /// Observable container of scene loading state data.
        /// </summary>
        public SceneLoadingState SceneLoadingState;

        private readonly LoadingState _state = new LoadingState();

        /// <summary>
        /// Is the current scene the Home scene.
        /// </summary>
        public bool IsAtHome =>
                UnitySceneManager.GetActiveScene().path
             == Singleton.Instance.Settings.HomeScenePath;

        /// <summary>
        /// Loads the Home scene.
        /// </summary>
        public void LoadHomeScene()
        {
            LoadScene(Singleton.Instance.Settings.HomeScenePath);
        }

        /// <summary>
        /// Loads a scene by path.
        /// </summary>
        /// <param name="scenePath">Asset path of the scene to load.</param>
        public void LoadScene(string scenePath)
        {
            if (_state.IsLoading)
            {
                Debug.LogError(
                        $"Request to load scene \"{scenePath}\" while load operation "
                      + "in progress - ignoring new request");
                return;
            }

            StartCoroutine(LoadSceneAsync(scenePath));
        }

        /// <summary>
        /// Unloads all scenes except the active scene.
        /// </summary>
        public void UnloadExtraScenes()
        {
            var scenesToUnload = new Queue<Scene>();

            for (int i = 0; i < UnitySceneManager.loadedSceneCount; i++)
            {
                Scene scene = UnitySceneManager.GetSceneAt(i);
                if (scene != UnitySceneManager.GetActiveScene())
                {
                    scenesToUnload.Enqueue(scene);
                }
            }

            while (scenesToUnload.Count > 0)
            {
                Scene scene = scenesToUnload.Dequeue();
                Debug.Log($"Unloading scene: {scene.path}");
                UnitySceneManager.UnloadSceneAsync(scene.path);
            }
        }

        private IEnumerator LoadSceneAsync(string scenePath)
        {
            Debug.Log($"Loading scene [path: {scenePath}]");

            _state.IsLoading = true;
            _state.LoadProgress = 0f;
            SceneLoadingState.Value = _state;

            // At any given time, there should be at most 1 stand alone scene or 1 stand alone
            // plus one additive scene loaded. This method detects the latter and unloads the
            // additive scene.
            UnloadExtraScenes();

            AsyncOperation loadingOperation = UnitySceneManager.LoadSceneAsync(
                scenePath, LoadSceneMode.Single);

            // Block activation of new scene.
            loadingOperation.allowSceneActivation = false;

            while (!loadingOperation.isDone)
            {
                Debug.Log($"Load progress: {loadingOperation.progress}");

                _state.LoadProgress = Mathf.Clamp01(loadingOperation.progress);
                SceneLoadingState.Value = _state;

                // Unblock activation of new scene. The 0.9f value came from the Unity
                // documentation:
                // https://docs.unity3d.com/ScriptReference/AsyncOperation-allowSceneActivation.html
                if (loadingOperation.progress >= 0.9f)
                {
                    _state.IsLoading = false;
                    SceneLoadingState.Value = _state;

                    loadingOperation.allowSceneActivation = true;
                }

                yield return null;
            }

            Debug.Log($"Loading scene \"{scenePath}\" done");
        }
    }
}
