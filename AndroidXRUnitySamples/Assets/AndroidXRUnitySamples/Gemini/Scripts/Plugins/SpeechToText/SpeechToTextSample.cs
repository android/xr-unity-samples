// <copyright file="SpeechToTextSample.cs" company="Google LLC">
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
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// Sample MonoBehaviour demonstrating the usage of SpeechToTextBridge.
    /// Provides a simple UI for starting/stopping speech recognition and displaying results.
    /// </summary>
    public class SpeechToTextSample : MonoBehaviour
    {
        [SerializeField] private Button _testSttButton;
        [SerializeField] private TMP_Text _resultTextUI;
        [SerializeField] private TMP_Text _statusTextUI;

        private SpeechToTextBridge _speechToTextBridge;
        private bool _permissionRequested;

        private void Start()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                _statusTextUI.text = "Initializing...";
                if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
                {
                    _statusTextUI.text = "Requesting Mic Permission...";
                    _testSttButton.interactable = false;
                    _permissionRequested = true;
                    Permission.RequestUserPermission(Permission.Microphone);
                }
                else
                {
                    InitializeBridge();
                }
            }
            else
            {
                _statusTextUI.text = "STT only on Android.";
                Debug.LogWarning("STT Plugin tests are only available on Android platform.");
                if (_testSttButton != null)
                {
                    _testSttButton.interactable = false;
                }
            }

            if (_testSttButton != null)
            {
                _testSttButton.onClick.AddListener(OnTestSTTButtonClicked);
                _testSttButton.interactable = false;
            }
        }

        private void Update()
        {
            if (_permissionRequested)
            {
                if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
                {
                    Debug.Log("Microphone Permission Granted after request.");
                    InitializeBridge();
                    _permissionRequested = false;
                }
            }
        }

        private void InitializeBridge()
        {
            if (_speechToTextBridge != null)
            {
                return;
            }

            try
            {
                _speechToTextBridge = new SpeechToTextBridge();

                _speechToTextBridge.OnReadyForSpeech += HandleReadyForSpeech;
                _speechToTextBridge.OnBeginningOfSpeech += HandleBeginningOfSpeech;
                _speechToTextBridge.OnEndOfSpeech += HandleEndOfSpeech;
                _speechToTextBridge.OnResult += HandleResult;
                _speechToTextBridge.OnError += HandleError;
                _speechToTextBridge.OnDebugLog += HandleDebugLog;

                _statusTextUI.text = "Ready (Bridge Initialized)";
                _testSttButton.interactable = true;
            }
            catch (Exception e)
            {
                _statusTextUI.text = $"Error initializing bridge: {e.Message}";
                Debug.LogError($"Failed to create SpeechToTextBridge: {e.Message}");
                _testSttButton.interactable = false;
            }
        }

        private void OnTestSTTButtonClicked()
        {
            if (_speechToTextBridge != null)
            {
                _testSttButton.interactable = false;
                _statusTextUI.text = "Requesting STT start...";
                _resultTextUI.text = string.Empty;
                _speechToTextBridge.StartRecognition();
            }
            else
            {
                _statusTextUI.text = "STT Bridge not ready.";
                Debug.LogWarning("STT Bridge not initialized.");
                if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
                {
                    InitializeBridge();
                }
                else
                {
                    _statusTextUI.text = "Mic permission needed.";
                }
            }
        }

        private void HandleReadyForSpeech()
        {
            _statusTextUI.text = "Listening...";
            _testSttButton.interactable = false;
        }

        private void HandleBeginningOfSpeech()
        {
            _statusTextUI.text = "Speech detected...";
        }

        private void HandleEndOfSpeech()
        {
            _statusTextUI.text = "Processing...";
        }

        private void HandleResult(SpeechToTextResult resultData)
        {
            _resultTextUI.text = "Result: " + resultData.Text;
            _statusTextUI.text = "Ready";
            _testSttButton.interactable = true;
        }

        private void HandleError(SpeechToTextError errorData)
        {
            _statusTextUI.text = $"Error: {errorData.Error} ({errorData.ErrorCode})";
            _resultTextUI.text = "-- Error --";
            Debug.LogError($"STT Error: {errorData.Error} (Code: {errorData.ErrorCode})");
            _testSttButton.interactable = true;
        }

        private void HandleDebugLog(string message)
        {
            Debug.Log($"[STT Bridge]: {message}");
        }

        private void OnDestroy()
        {
            if (_speechToTextBridge != null)
            {
                _speechToTextBridge.OnReadyForSpeech -= HandleReadyForSpeech;
                _speechToTextBridge.OnBeginningOfSpeech -= HandleBeginningOfSpeech;
                _speechToTextBridge.OnEndOfSpeech -= HandleEndOfSpeech;
                _speechToTextBridge.OnResult -= HandleResult;
                _speechToTextBridge.OnError -= HandleError;
                _speechToTextBridge.OnDebugLog -= HandleDebugLog;

                _speechToTextBridge.Dispose();
                _speechToTextBridge = null;
            }
        }
    }
}
