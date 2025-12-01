// <copyright file="GeminiInteractionManager.cs" company="Google LLC">
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
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// Manages interaction flow with the Gemini API, maintains conversation context,
    /// handles request lifecycles, and exposes results.
    /// </summary>
    public class GeminiInteractionManager : MonoBehaviour
    {
        /// <summary>
        /// Settings for the Gemini API connection.
        /// </summary>
        [FormerlySerializedAs("geminiSettings")]
        [Header("Configuration")]
        [Tooltip("Assign your GeminiSettings ScriptableObject containing the API Key")]
        public GeminiAPISettings GeminiSettings;

        /// <summary>
        /// Stores the conversation history for context in future requests.
        /// </summary>
        private readonly List<Content> _conversationHistory = new List<Content>();

        /// <summary>
        /// Current status of the interaction manager.
        /// </summary>
        [Header("Status & Results")]
        [SerializeField]
        private InteractionStatus _currentStatus = InteractionStatus.Idle;

        /// <summary>
        /// Indicates if a request is currently being processed.
        /// </summary>
        private bool _isProcessing;

        /// <summary>
        /// Gets the audio data (bytes) from the most recent successful Gemini response (if any).
        /// </summary>
        /// <remarks>Requires Gemini model response to include audio data.</remarks>
        private byte[] _latestResponseAudioData;

        /// <summary>Fires when the InteractionStatus changes.</summary>
        public event Action<InteractionStatus> OnStatusChanged;

        /// <summary>Fires when a new text response is successfully received from Gemini.</summary>
        public event Action<string> OnTextResponseReceived;

        /// <summary>Fires when new image data is successfully received from Gemini.</summary>
        public event Action<byte[]> OnImageResponseReceived;

        /// <summary>Fires when an error occurs during the interaction.</summary>
        public event Action<string> OnErrorOccurred;

        /// <summary>Fires when audio data is successfully received from Gemini.</summary>
        public event Action<byte[]> OnAudioResponseReceived;

        /// <summary>
        /// Represents the current state of the API interaction.
        /// </summary>
        public enum InteractionStatus
        {
            /// <summary>
            /// The manager is idle and ready to process new requests.
            /// </summary>
            Idle,

            /// <summary>
            /// A request is being prepared and sent to the API.
            /// </summary>
            Sending,

            /// <summary>
            /// The API is processing the request.
            /// </summary>
            Processing,

            /// <summary>
            /// The request was processed successfully.
            /// </summary>
            Success,

            /// <summary>
            /// An error occurred during the request.
            /// </summary>
            Error
        }

        /// <summary>
        /// Gets the current status of the interaction manager.
        /// </summary>
        public InteractionStatus CurrentStatus => _currentStatus;

        /// <summary>
        /// Gets the text content from the most recent successful Gemini response.
        /// </summary>
        public string LatestResponseText { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the image data (bytes) from the most recent successful Gemini response (if any).
        /// </summary>
        /// <remarks>Requires Gemini model response to include image data.</remarks>
        public byte[] LatestResponseImageData { get; private set; }

        /// <summary>Gets
        /// Gets or sets the audio data (bytes) from the most recent successful Gemini response.
        /// </summary>
        /// <remarks>Requires Gemini model response to include audio data.</remarks>
        public byte[] LatestResponseAudioData
        {
            get { return _latestResponseAudioData; }
            private set { _latestResponseAudioData = value; }
        }

        /// <summary>
        /// Gets the last error message encountered.
        /// </summary>
        public string LastErrorMessage { get; private set; } = string.Empty;

        /// <summary>
        /// Sends a prompt to the Gemini API, potentially including text, image, and/or audio data.
        /// </summary>
        /// <param name="textPrompt">The text part of the prompt (optional).</param>
        /// <param name="imageData">JPEG or PNG image bytes (optional).</param>
        /// <param name="imageMimeType">The MIME type for the image
        /// data (e.g., "image/jpeg").</param>
        /// <param name="audioData">Audio bytes (optional). Requires model support.</param>
        /// <param name="audioMimeType">The MIME type for the audio
        /// data (e.g., "audio/wav").</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task SendPromptAsync(
            string textPrompt = null,
            byte[] imageData = null,
            string imageMimeType = "image/jpeg",
            byte[] audioData = null,
            string audioMimeType = null)
        {
            if (_isProcessing)
            {
                Debug.LogWarning(
                    "Gemini Interaction Manager: Request ignored,"
                  + " already processing a previous request.");
                return;
            }

            if (GeminiSettings == null || string.IsNullOrEmpty(GeminiSettings.GeminiAPIKey))
            {
                HandleError("API Key is not configured in GeminiSettings ScriptableObject.");
                return;
            }

            if (string.IsNullOrEmpty(textPrompt) && imageData == null && audioData == null)
            {
                HandleError("Cannot send prompt: No text, image, or audio data provided.");
                return;
            }

            if (imageData != null && string.IsNullOrEmpty(imageMimeType))
            {
                HandleError("Cannot send prompt: Image data provided"
                          + " but image MIME type is missing.");
                return;
            }

            if (audioData != null && string.IsNullOrEmpty(audioMimeType))
            {
                HandleError("Cannot send prompt: Audio data provided"
                          + " but audio MIME type is missing.");
                return;
            }

            _isProcessing = true;
            UpdateStatus(InteractionStatus.Sending);
            LastErrorMessage = string.Empty;

            var userParts = new List<Part>();

            // Per the tips and best practices in the documentation, place the text *after*
            // the image data: https://ai.google.dev/gemini-api/docs/image-understanding
            if (imageData != null)
            {
                try
                {
                    userParts.Add(RequestPartHelper.Image(imageMimeType, imageData));
                }
                catch (Exception e)
                {
                    HandleError($"Error encoding image data: {e.Message}");
                    _isProcessing = false;
                    return;
                }
            }

            if (audioData != null)
            {
                try
                {
                    userParts.Add(RequestPartHelper.Audio(audioMimeType, audioData));
                }
                catch (Exception e)
                {
                    HandleError($"Error encoding audio data: {e.Message}");
                    _isProcessing = false;
                    return;
                }
            }

            if (!string.IsNullOrEmpty(textPrompt))
            {
                userParts.Add(new Part { text = textPrompt });
            }

            var userContent = new Content { role = "user", parts = userParts };
            var currentTurnHistory = new List<Content>(_conversationHistory);
            currentTurnHistory.Add(userContent);
            var requestPayload = new GeminiRequest { contents = currentTurnHistory };

            GeminiResponse response = null;
            try
            {
                response = await GeminiService.GenerateContentAsync(
                    requestPayload,
                    GeminiSettings.GeminiAPIKey);
            }
            catch (Exception ex)
            {
                HandleError($"An unexpected exception occurred during the API call: {ex.Message}");
                _isProcessing = false;
                return;
            }

            UpdateStatus(InteractionStatus.Processing);

            if (response == null)
            {
                HandleError("API request failed. Check logs for details.");
            }
            else if (response.error != null && response.error.IsError)
            {
                HandleError(
                    $"Gemini API Error {response.error.code} ({response.error.status}):"
                  + $" {response.error.message}");
            }
            else if (response.promptFeedback?.blockReason != null)
            {
                HandleError($"Request blocked by API. Reason:"
                          + $" {response.promptFeedback.blockReason}");
            }
            else if (response.candidates == null || response.candidates.Count == 0)
            {
                HandleError("Request completed, but the response contained no candidates.");
                _conversationHistory.Add(userContent);
            }
            else
            {
                Candidate bestCandidate = response.candidates[0];

                if (bestCandidate.content != null && bestCandidate.content.parts != null)
                {
                    LatestResponseText = string.Join("\n", bestCandidate.content.parts
                        .Where(p => !string.IsNullOrEmpty(p.text))
                        .Select(p => p.text)).Trim();

                    // Reset audio and image data before processing new response
                    LatestResponseImageData = null;
                    LatestResponseAudioData = null;

                    Part imagePart = bestCandidate.content.parts.FirstOrDefault(p =>
                        p.inlineData != null && p.inlineData.mimeType != null &&
                        p.inlineData.mimeType.StartsWith("image/"));
                    if (imagePart != null)
                    {
                        try
                        {
                            LatestResponseImageData = Convert
                                    .FromBase64String(imagePart.inlineData.data);
                            Debug.Log($"Received image data:"
                                    + $" {LatestResponseImageData.Length} bytes");
                            OnImageResponseReceived?.Invoke(LatestResponseImageData);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Failed to"
                                         + $" decode received image data: {e.Message}");
                            LatestResponseImageData = null;
                        }
                    }

                    // Process audio response if present
                    Part audioPart = bestCandidate.content.parts.FirstOrDefault(p =>
                        p.inlineData != null && p.inlineData.mimeType != null &&
                        p.inlineData.mimeType.StartsWith("audio/"));
                    if (audioPart != null)
                    {
                        try
                        {
                            LatestResponseAudioData = Convert
                                    .FromBase64String(audioPart.inlineData.data);
                            Debug.Log($"Received audio data:"
                                    + $" {LatestResponseAudioData.Length} bytes");
                            OnAudioResponseReceived?.Invoke(LatestResponseAudioData);
                            Debug.LogWarning("Parsed audio data from response. Note: Standard"
                                + " Gemini models may not currently return audio directly"
                                + " in this format.");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Failed to"
                                         + $" decode received audio data: {e.Message}");
                            LatestResponseAudioData = null;
                        }
                    }

                    _conversationHistory.Add(userContent);
                    _conversationHistory.Add(bestCandidate.content);

                    UpdateStatus(InteractionStatus.Success);
                    OnTextResponseReceived?.Invoke(LatestResponseText);
                    Debug.Log($"Gemini Success. Response Text: '{LatestResponseText}'");
                }
                else
                {
                    HandleError("Request successful, but"
                              + " the candidate content was empty or invalid.");
                    _conversationHistory.Add(userContent);
                }

                if (bestCandidate.finishReason != "STOP"
                 && bestCandidate.finishReason != "MAX_TOKENS")
                {
                    Debug.LogWarning($"Gemini"
                                   + $" response finished with "
                                   + $"reason: {bestCandidate.finishReason}");
                }
            }

            _isProcessing = false;

            if (CurrentStatus != InteractionStatus.Error
             && CurrentStatus != InteractionStatus.Success)
            {
                UpdateStatus(InteractionStatus.Idle);
            }
        }

        /// <summary>
        /// Clears the current conversation history and resets response data.
        /// </summary>
        public void ClearConversationHistory()
        {
            _conversationHistory.Clear();
            Debug.Log("Gemini conversation history cleared.");
            LatestResponseText = string.Empty;
            LatestResponseImageData = null;
            LatestResponseAudioData = null;
            if (CurrentStatus != InteractionStatus.Idle
             && CurrentStatus != InteractionStatus.Sending)
            {
                UpdateStatus(InteractionStatus.Idle);
            }
        }

        /// <summary>
        /// Checks to see if the GeminiSettings resource is valid.
        /// </summary>
        /// <returns>A boolean, representing validity.</returns>
        public bool AreSettingsValid()
        {
            if (GeminiSettings == null)
            {
                Debug.LogError(
                    "GeminiSettings ScriptableObject is not assigned in the Inspector!",
                    this);
                return false;
            }
            else if (string.IsNullOrEmpty(GeminiSettings.GeminiAPIKey))
            {
                Debug.LogError("Gemini API Key is not set"
                             + " within the assigned GeminiSettings!\n"
                             + "If you need an API key, try: "
                             + "https://ai.google.dev/gemini-api/docs/api-key", this);
                return false;
            }

            return true;
        }

        private void UpdateStatus(InteractionStatus newStatus)
        {
            if (_currentStatus != newStatus)
            {
                _currentStatus = newStatus;
                OnStatusChanged?.Invoke(_currentStatus);
            }
        }

        private void HandleError(string errorMessage)
        {
            LastErrorMessage = errorMessage;
            Debug.LogError($"Gemini Error: {errorMessage}");
            UpdateStatus(InteractionStatus.Error);
            OnErrorOccurred?.Invoke(errorMessage);
            _isProcessing = false;
        }
    }
}
