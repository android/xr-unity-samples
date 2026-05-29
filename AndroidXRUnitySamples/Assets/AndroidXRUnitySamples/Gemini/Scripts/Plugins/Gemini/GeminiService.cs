// <copyright file="GeminiService.cs" company="Google LLC">
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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// Static service class that handles communication with the Gemini API.
    /// Provides methods for sending requests and processing responses.
    /// </summary>
    public static class GeminiService
    {
        private const string _geminiUrlFormat =
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.1-flash-lite"
            + ":generateContent?key={0}";

        /// <summary>
        /// Sends a pre-constructed Gemini request payload to the API.
        /// </summary>
        /// <param name="requestPayload">
        /// The GeminiRequest object containing the full conversation history and
        /// latest prompt.
        /// </param>
        /// <param name="apiKey">Your Google AI Studio API Key.</param>
        /// <returns>A Task resulting in the parsed GeminiResponse or null on failure.</returns>
        public static async Task<GeminiResponse> GenerateContentAsync(
            GeminiRequest requestPayload, string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("Gemini API Key is missing!");
                return null;
            }

            if (requestPayload == null || requestPayload.contents == null
                                       || requestPayload.contents.Count == 0)
            {
                Debug.LogError("Gemini request payload or contents are null/empty.");
                return null;
            }

            string apiUrl = string.Format(_geminiUrlFormat, apiKey);

            string jsonRequestBody;
            try
            {
                jsonRequestBody = BuildJsonRequestManually(requestPayload);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to serialize Gemini request: {e.Message}");
                return null;
            }

            if (string.IsNullOrEmpty(jsonRequestBody))
            {
                Debug.LogError("Failed to create JSON request body.");
                return null;
            }

            Debug.Log($"Gemini Request URL: {apiUrl}");
            Debug.Log($"Gemini Request Body: {jsonRequestBody}");

            using (var request = new UnityWebRequest(apiUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequestBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                try
                {
                    UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
                    while (!asyncOperation.isDone)
                    {
                        await Task.Yield();
                    }

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError(
                                $"Gemini API Request Failed: {request.error} "
                              + $"- HTTP {request.responseCode}");
                        Debug.LogError($"Response Body: {request.downloadHandler.text}");
                        try
                        {
                            var errorResponse =
                                    JsonUtility.FromJson<GeminiResponse>(request.downloadHandler
                                            .text);
                            if (errorResponse?.error != null)
                            {
                                return errorResponse;
                            }

                            if (request.downloadHandler.text.Contains("\"error\""))
                            {
                                Debug.LogWarning(
                                        "Could not fully parse error response "
                                      + "with JsonUtility, but error field detected.");
                            }
                        }
                        catch (Exception parseEx)
                        {
                            Debug.LogError(
                                    $"Failed to parse even the "
                                  + $"error response: {parseEx.Message}");
                        }

                        return null;
                    }

                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log($"Gemini Response Body: {jsonResponse}");

                    try
                    {
                        var response = JsonUtility.FromJson<GeminiResponse>(jsonResponse);

                        if (response == null)
                        {
                            Debug.LogError(
                                    "Failed to parse Gemini response JSON using JsonUtility.");
                            return null;
                        }

                        if (response.error != null && response.error.IsError)
                        {
                            Debug.LogError(
                                    $"Gemini API Error (in 200 OK response): "
                                  + $"{response.error.message} (Status: {response.error.status})");
                        }
                        else if (response.candidates == null || response.candidates.Count == 0)
                        {
                            if (response.promptFeedback?.blockReason != null)
                            {
                                Debug.LogWarning(
                                        $"Gemini request blocked. Reason: "
                                      + $"{response.promptFeedback.blockReason}");
                            }
                            else
                            {
                                Debug.LogWarning(
                                        "Gemini response contained no candidates "
                                      + "and no specific block reason/error.");
                            }
                        }

                        return response;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(
                                $"Error parsing Gemini response JSON: "
                              + $"{e.Message}\nJSON: {jsonResponse}\n{e.StackTrace}");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                            $"Exception during Gemini request: {ex.Message}\n{ex.StackTrace}");
                    return null;
                }
            }
        }

        private static string BuildJsonRequestManually(GeminiRequest requestPayload)
        {
            var sb = new StringBuilder();
            sb.Append("{");

            // Contents array
            sb.Append("\"contents\":[");
            for (int i = 0; i < requestPayload.contents.Count; i++)
            {
                var content = requestPayload.contents[i];
                if (i > 0)
                {
                    sb.Append(",");
                }

                sb.Append("{");
                sb.Append($"\"role\":\"{EscapeJsonString(content.role)}\"");

                if (content.parts != null && content.parts.Count > 0)
                {
                    sb.Append(",\"parts\":[");
                    for (int j = 0; j < content.parts.Count; j++)
                    {
                        var part = content.parts[j];
                        if (j > 0)
                        {
                            sb.Append(",");
                        }

                        sb.Append("{");
                        if (!string.IsNullOrEmpty(part.text))
                        {
                            sb.Append($"\"text\":\"{EscapeJsonString(part.text)}\"");
                        }
                        else if (part.inlineData != null)
                        {
                            sb.Append("\"inline_data\":{");
                            sb.Append(
                                $"\"mime_type\":\"{EscapeJsonString(part.inlineData.mimeType)}\"");
                            sb.Append($",\"data\":\"{EscapeJsonString(part.inlineData.data)}\"");
                            sb.Append("}");
                        }
                        else if (part.functionCall != null)
                        {
                            sb.Append("\"function_call\":{");
                            sb.Append($"\"name\":\"{EscapeJsonString(part.functionCall.name)}\"");
                            if (!string.IsNullOrEmpty(part.functionCall.argsJsonString))
                            {
                                sb.Append($",\"args\":{part.functionCall.argsJsonString}");
                            }

                            sb.Append("}");
                        }
                        else if (part.functionResponse != null)
                        {
                            sb.Append("\"function_response\":{");
                            sb.Append(
                                $"\"name\":\"{EscapeJsonString(part.functionResponse.name)}\"");
                            if (!string.IsNullOrEmpty(part.functionResponse.responseJsonString))
                            {
                                sb.Append(
                                    $",\"response\":{part.functionResponse.responseJsonString}");
                            }
                            else
                            {
                                sb.Append($",\"response\":\"\"");
                            }

                            sb.Append("}");
                        }

                        sb.Append("}");
                    }

                    sb.Append("]");
                }

                sb.Append("}");
            }

            sb.Append("]");

            // Optional generation config
            if (requestPayload.generationConfig != null)
            {
                sb.Append(",\"generationConfig\":{");
                if (requestPayload.generationConfig.candidateCount > 0)
                {
                    sb.Append(
                        $"\"candidateCount\":{requestPayload.generationConfig.candidateCount}");
                }

                if (requestPayload.generationConfig.stopSequences != null &&
                    requestPayload.generationConfig.stopSequences.Count > 0)
                {
                    if (requestPayload.generationConfig.candidateCount > 0)
                    {
                        sb.Append(",");
                    }

                    sb.Append("\"stopSequences\":[");
                    for (int i = 0; i < requestPayload.generationConfig.stopSequences.Count; i++)
                    {
                        if (i > 0)
                        {
                            sb.Append(",");
                        }

                        sb.Append(
                    $"\"{EscapeJsonString(requestPayload.generationConfig.stopSequences[i])}\"");
                    }

                    sb.Append("]");
                }

                if (requestPayload.generationConfig.maxOutputTokens > 0)
                {
                    if (requestPayload.generationConfig.candidateCount > 0 ||
                        (requestPayload.generationConfig.stopSequences != null &&
                         requestPayload.generationConfig.stopSequences.Count > 0))
                    {
                        sb.Append(",");
                    }

                    sb.Append($"\"maxOutputTokens\""
                            + $":{requestPayload.generationConfig.maxOutputTokens}");
                }

                if (requestPayload.generationConfig.temperature >= 0)
                {
                    if (requestPayload.generationConfig.candidateCount > 0 ||
                        (requestPayload.generationConfig.stopSequences != null &&
                         requestPayload.generationConfig.stopSequences.Count > 0) ||
                        requestPayload.generationConfig.maxOutputTokens > 0)
                    {
                        sb.Append(",");
                    }

                    sb.Append($"\"temperature\":{requestPayload.generationConfig.temperature}");
                }

                if (requestPayload.generationConfig.topP >= 0)
                {
                    if (requestPayload.generationConfig.candidateCount > 0 ||
                        (requestPayload.generationConfig.stopSequences != null &&
                         requestPayload.generationConfig.stopSequences.Count > 0) ||
                        requestPayload.generationConfig.maxOutputTokens > 0 ||
                        requestPayload.generationConfig.temperature >= 0)
                    {
                        sb.Append(",");
                    }

                    sb.Append($"\"topP\":{requestPayload.generationConfig.topP}");
                }

                if (requestPayload.generationConfig.topK >= 0)
                {
                    if (requestPayload.generationConfig.candidateCount > 0 ||
                        (requestPayload.generationConfig.stopSequences != null &&
                         requestPayload.generationConfig.stopSequences.Count > 0) ||
                        requestPayload.generationConfig.maxOutputTokens > 0 ||
                        requestPayload.generationConfig.temperature >= 0 ||
                        requestPayload.generationConfig.topP >= 0)
                    {
                        sb.Append(",");
                    }

                    sb.Append($"\"topK\":{requestPayload.generationConfig.topK}");
                }

                sb.Append("}");
            }

            // Optional safety settings
            if (requestPayload.safetySettings != null && requestPayload.safetySettings.Count > 0)
            {
                sb.Append(",\"safetySettings\":[");
                for (int i = 0; i < requestPayload.safetySettings.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }

                    var setting = requestPayload.safetySettings[i];
                    sb.Append("{");
                    sb.Append($"\"category\":\"{EscapeJsonString(setting.category)}\"");
                    sb.Append($",\"threshold\":\"{EscapeJsonString(setting.threshold)}\"");
                    sb.Append("}");
                }

                sb.Append("]");
            }

            sb.Append("}");
            return sb.ToString();
        }

        private static string EscapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                switch (c)
                {
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    default:
                        if (c < ' ')
                        {
                            sb.Append($"\\u{(int)c:X4}");
                        }
                        else
                        {
                            sb.Append(c);
                        }

                        break;
                }
            }

            return sb.ToString();
        }
    }
}
