// <copyright file="GeminiPrototypeUI.cs" company="Google LLC">
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
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// A simple UI prototype for testing the GeminiService.
    /// Provides input field for prompts, button to send requests, and text areas for status and responses.
    /// </summary>
    public class GeminiPrototypeUI : MonoBehaviour
    {
        private readonly List<Content> _conversationHistory = new List<Content>();

        [Header("UI References")]
        [SerializeField]
        private TMP_InputField _promptInputField;

        [SerializeField]
        private Button _sendButton;

        [SerializeField]
        private TextMeshProUGUI _statusText;

        [SerializeField]
        private TextMeshProUGUI _responseText;

        [SerializeField]
        private ScrollRect _responseScrollRect;

        [Header("Settings")]
        [SerializeField]
        private GeminiAPISettings _geminiSettings;

        [SerializeField]
        private bool _clearResponseOnNewRequest = true;

        [SerializeField]
        private bool _autoScrollToBottom = true;

        private bool _isProcessing;

        private void Start()
        {
            if (_promptInputField == null || _sendButton == null || _statusText == null
             || _responseText == null)
            {
                Debug.LogError("GeminiPrototypeUI: Missing required UI references!");
                return;
            }

            _sendButton.onClick.AddListener(OnSendButtonClicked);
            _promptInputField.onSubmit.AddListener(_ => OnSendButtonClicked());

            UpdateStatus("Ready");
            _responseText.text = "Enter a prompt and click 'Send Prompt' to start.";
        }

        private void OnDestroy()
        {
            if (_sendButton != null)
            {
                _sendButton.onClick.RemoveListener(OnSendButtonClicked);
            }

            if (_promptInputField != null)
            {
                _promptInputField.onSubmit.RemoveListener(_ => OnSendButtonClicked());
            }
        }

        private async void OnSendButtonClicked()
        {
            if (_isProcessing)
            {
                Debug.Log("Already processing a request. Please wait.");
                return;
            }

            string prompt = _promptInputField.text.Trim();
            if (string.IsNullOrEmpty(prompt))
            {
                Debug.Log("Prompt is empty. Please enter text.");
                return;
            }

            if (_geminiSettings == null || string.IsNullOrEmpty(_geminiSettings.GeminiAPIKey))
            {
                UpdateStatus("Error: Missing API key. Please assign GeminiAPISettings.");
                return;
            }

            if (_clearResponseOnNewRequest)
            {
                _responseText.text = string.Empty;
            }

            _isProcessing = true;
            _sendButton.interactable = false;
            _promptInputField.interactable = false;
            UpdateStatus("Processing...");

            try
            {
                GeminiRequest requestPayload = CreateRequestPayload(prompt);

                Debug.Log($"Sending request with {requestPayload.contents.Count} content items");
                foreach (Content content in requestPayload.contents)
                {
                    Debug.Log(
                            $"Content role: {content.role}, Parts count: "
                          + $"{content.parts?.Count ?? 0}");
                    if (content.parts != null)
                    {
                        foreach (Part part in content.parts)
                        {
                            if (!string.IsNullOrEmpty(part.text))
                            {
                                var dbgStr = part
                                             .text
                                             .Substring(0,
                                                     Math.Min(50, part.text.Length));
                                Debug.Log(
                                        $"Part text: "
                                      + $"{dbgStr}...");
                            }
                        }
                    }
                }

                GeminiResponse response =
                        await GeminiService.GenerateContentAsync(requestPayload,
                                _geminiSettings.GeminiAPIKey);
                ProcessResponse(response);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing Gemini request: {ex.Message}");
                UpdateStatus($"Error: {ex.Message}");
                AppendToResponse($"\n<color=red>Error: {ex.Message}</color>");
            }
            finally
            {
                _isProcessing = false;
                _sendButton.interactable = true;
                _promptInputField.interactable = true;
                _promptInputField.text = string.Empty;
                _promptInputField.ActivateInputField();
            }
        }

        private GeminiRequest CreateRequestPayload(string prompt)
        {
            var userContent = new Content
            {
                    role = "user",
                    parts = new List<Part>
                    {
                            RequestPartHelper.Text(prompt)
                    }
            };

            _conversationHistory.Add(userContent);

            return new GeminiRequest
            {
                    contents = new List<Content>(_conversationHistory),
                    generationConfig = new GenerationConfig
                    {
                            temperature = 0.7f, topP = 0.8f, topK = 40, maxOutputTokens = 1024
                    }
            };
        }

        private void ProcessResponse(GeminiResponse response)
        {
            if (response == null)
            {
                UpdateStatus("Error: No response received");
                AppendToResponse(
                        "\n<color=red>Error: No response received from Gemini API</color>");
                return;
            }

            if (response.error != null)
            {
                Debug.Log(
                        $"Error details - Code: {response.error.code}, Message: "
                      + $"'{response.error.message}', Status: '{response.error.status}'");
                Debug.Log($"IsError property: {response.error.IsError}");
            }

            if (response.error != null && response.error.IsError)
            {
                string errorMessage = !string.IsNullOrEmpty(response.error.message)
                                              ? response.error.message
                                              : "Unknown error";

                string errorStatus = !string.IsNullOrEmpty(response.error.status)
                                             ? response.error.status
                                             : "Unknown status";

                UpdateStatus($"API Error: {errorMessage}");
                AppendToResponse(
                        $"\n<color=red>API Error: {errorMessage} (Status: {errorStatus})</color>");
                return;
            }

            if (response.candidates == null || response.candidates.Count == 0)
            {
                UpdateStatus("Error: No candidates in response");
                AppendToResponse("\n<color=red>Error: No candidates in response</color>");
                return;
            }

            Candidate candidate = response.candidates[0];
            if (candidate.content == null || candidate.content.parts == null
                                          || candidate.content.parts.Count == 0)
            {
                UpdateStatus("Error: Empty content in response");
                AppendToResponse("\n<color=red>Error: Empty content in response</color>");
                return;
            }

            string responseText = string.Empty;
            foreach (Part part in candidate.content.parts)
            {
                if (!string.IsNullOrEmpty(part.text))
                {
                    responseText += part.text;
                }
            }

            _conversationHistory.Add(candidate.content);

            UpdateStatus($"Response received (finish reason: {candidate.finishReason})");
            AppendToResponse(
                    $"\n<color=blue>You:</color> {_promptInputField.text}\n"
                  + $"<color=green>Gemini:</color> {responseText}");

            if (_autoScrollToBottom && _responseScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                _responseScrollRect.normalizedPosition = new Vector2(0, 1);
            }
        }

        private void UpdateStatus(string status)
        {
            if (_statusText != null)
            {
                _statusText.text = status;
            }
        }

        private void AppendToResponse(string text)
        {
            if (_responseText != null)
            {
                _responseText.text += text;
            }
        }
    }
}
