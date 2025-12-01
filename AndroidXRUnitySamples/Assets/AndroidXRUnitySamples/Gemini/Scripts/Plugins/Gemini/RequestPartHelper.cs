// <copyright file="RequestPartHelper.cs" company="Google LLC">
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
using UnityEngine.Serialization;

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// Helper class for creating request parts with different content types.
    /// </summary>
    public static class RequestPartHelper
    {
        /// <summary>
        /// Creates a text part for a request.
        /// </summary>
        /// <param name="text">The text content to include.</param>
        /// <returns>A Part object containing the text.</returns>
        public static Part Text(string text)
        {
            return new Part { text = text };
        }

        /// <summary>
        /// Creates an image part for a request.
        /// </summary>
        /// <param name="mimeType">The MIME type of the image.</param>
        /// <param name="data">The raw image data.</param>
        /// <returns>A Part object containing the image data.</returns>
        public static Part Image(string mimeType, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (string.IsNullOrEmpty(mimeType))
            {
                throw new ArgumentException("MIME type cannot be empty.", nameof(mimeType));
            }

            return new Part
            {
                inlineData = new InlineData
                {
                    mimeType = mimeType,
                    data = Convert.ToBase64String(data)
                }
            };
        }

        /// <summary>
        /// Creates an audio part for a request.
        /// </summary>
        /// <param name="mimeType">The MIME type of the audio.</param>
        /// <param name="data">The raw audio data.</param>
        /// <returns>A Part object containing the audio data.</returns>
        public static Part Audio(string mimeType, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (string.IsNullOrEmpty(mimeType))
            {
                throw new ArgumentException("MIME type cannot be empty.", nameof(mimeType));
            }

            return new Part
            {
                inlineData = new InlineData
                {
                    mimeType = mimeType,
                    data = Convert.ToBase64String(data)
                }
            };
        }

        /// <summary>
        /// Creates a function response part.
        /// </summary>
        /// <param name="name">The name of the function that was called.</param>
        /// <param name="responseJson">The JSON response from the function.</param>
        /// <returns>A Part object containing the function response.</returns>
        public static Part FunctionResponse(string name, string responseJson)
        {
            return new Part
            {
                functionResponse = new FunctionResponse
                {
                    name = name,
                    responseJsonString = responseJson
                }
            };
        }
    }

    /// <summary>
    /// Represents a response from the Gemini API, containing generated content candidates and feedback.
    /// </summary>
    [Serializable]
    public class GeminiResponse
    {
        /// <summary>
        /// A list of generated response candidates. Populated on success.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public List<Candidate> candidates;

        /// <summary>
        /// Feedback regarding the prompt, including safety ratings and potential block reasons.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public PromptFeedback promptFeedback;

        /// <summary>
        /// If the request failed at the API level (e.g., invalid API key, malformed request),
        /// this field may be populated. Check this *before* checking candidates.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public GeminiErrorData error;
    }

    /// <summary>
    /// Represents a single generated response candidate from the Gemini model.
    /// </summary>
    [Serializable]
    public class Candidate
    {
        /// <summary>
        /// The generated content from the model for this candidate.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public Content content;

        /// <summary>
        /// The reason the model stopped generating content.
        /// Possible values: "STOP", "MAX_TOKENS", "SAFETY", "RECITATION", "OTHER".
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public string finishReason;

        /// <summary>
        /// The index of this candidate in the list of generated candidates.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public int index;

        /// <summary>
        /// List of safety ratings for the content generated by this candidate.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public List<SafetyRating> safetyRatings;

        /// <summary>
        /// Citation metadata if the model's response includes cited text.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public CitationMetadata citationMetadata;

        /// <summary>
        /// Total number of tokens calculated for this candidate.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public int tokenCount;
    }

    /// <summary>
    /// Represents a content turn in the conversation, containing one or more parts and a role identifier.
    /// </summary>
    [Serializable]
    public class Content
    {
        /// <summary>
        /// The actual content of the turn, broken down into one or more parts.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public List<Part> parts;

        /// <summary>
        /// The role of the entity that generated this content.
        /// Typically "user" for requests and "model" for responses.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public string role;
    }

    /// <summary>
    /// Represents a single part of content, which can be text, inline data, or function-related.
    /// </summary>
    [Serializable]
    public class Part
    {
        /// <summary>
        /// Plain text content.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public string text;

        /// <summary>
        /// Inline data, such as an image or audio blob. Used for multimodal input/output.
        /// Requires `gemini-pro-vision` or compatible model.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public InlineData inlineData;

        /// <summary>
        /// A request from the model to call a specific function. (Model Output)
        /// Requires Function Calling setup (Tools).
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public FunctionCall functionCall;

        /// <summary>
        /// The response provided by the client after executing a function requested by the model.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public FunctionResponse functionResponse;
    }

    /// <summary>
    /// Represents inline binary data with its MIME type, used for multimodal content.
    /// </summary>
    [Serializable]
    public class InlineData
    {
        /// <summary>
        /// The IANA standard MIME type of the data (e.g., "image/jpeg", "image/png", "audio/wav").
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public string mimeType;

        /// <summary>
        /// The binary data encoded as a Base64 string.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public string data;
    }

    /// <summary>
    /// Represents a function call request from the model.
    /// </summary>
    [Serializable]
    public class FunctionCall
    {
        /// <summary>
        /// The name of the function the model wants to call.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public string name;

        /// <summary>
        /// Arguments for the function call, represented as a single JSON string.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public string argsJsonString;
    }

    /// <summary>
    /// Represents a response to a function call made by the model.
    /// </summary>
    [Serializable]
    public class FunctionResponse
    {
        /// <summary>
        /// The name of the function that was called by the client.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public string name;

        /// <summary>
        /// The result of the function call, represented as a single JSON string.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public string responseJsonString;
    }

    /// <summary>
    /// Represents a safety rating for content, indicating potential harmful content categories.
    /// </summary>
    [Serializable]
    public class SafetyRating
    {
        /// <summary>
        /// The category of harmful content.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public string category;

        /// <summary>
        /// The assessed probability of the content belonging to this category.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public string probability;
    }

    /// <summary>
    /// Contains feedback about the prompt, including safety ratings and potential block reasons.
    /// </summary>
    [Serializable]
    public class PromptFeedback
    {
        /// <summary>
        /// If the prompt or response was blocked, this indicates the reason.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public string blockReason;

        /// <summary>
        /// Safety ratings associated with the prompt itself.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public List<SafetyRating> safetyRatings;
    }

    /// <summary>
    /// Contains metadata about citations in the model's response.
    /// </summary>
    [Serializable]
    public class CitationMetadata
    {
        /// <summary>
        /// List of citation sources referenced in the response.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public List<CitationSource> citationSources;
    }

    /// <summary>
    /// Represents a single citation source with its location and metadata.
    /// </summary>
    [Serializable]
    public class CitationSource
    {
        /// <summary>
        /// The starting index of the cited text in the response.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public int startIndex;

        /// <summary>
        /// The ending index of the cited text in the response.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public int endIndex;

        /// <summary>
        /// The URI of the cited source.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public string uri;

        /// <summary>
        /// The license associated with the cited content.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public string license;
    }

    /// <summary>
    /// Contains error information when a Gemini API request fails.
    /// </summary>
    [Serializable]
    public class GeminiErrorData
    {
        /// <summary>
        /// The error code returned by the API.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public int code;

        /// <summary>
        /// A human-readable error message.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public string message;

        /// <summary>
        /// The status of the error.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public string status;

        /// <summary>
        /// Gets whether this represents an error condition.
        /// </summary>
        public bool IsError =>
            code != 0 ||
            !string.IsNullOrEmpty(message) ||
            !string.IsNullOrEmpty(status);
    }

    /// <summary>
    /// Represents a request to the Gemini API.
    /// </summary>
    [Serializable]
    public class GeminiRequest
    {
        /// <summary>
        /// The conversation history and current prompt.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public List<Content> contents;

        /// <summary>
        /// Optional safety settings to control content filtering.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public List<SafetySetting> safetySettings;

        /// <summary>
        /// Optional configuration for text generation.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public GenerationConfig generationConfig;
    }

    /// <summary>
    /// Represents a safety setting for content filtering.
    /// </summary>
    [Serializable]
    public class SafetySetting
    {
        /// <summary>
        /// The category of harmful content to filter.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public string category;

        /// <summary>
        /// The threshold level for filtering this category.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public string threshold;
    }

    /// <summary>
    /// Configuration options for text generation.
    /// </summary>
    [Serializable]
    public class GenerationConfig
    {
        /// <summary>
        /// Number of response candidates to generate.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public int candidateCount;

        /// <summary>
        /// Sequences that will cause the model to stop generating further tokens.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public List<string> stopSequences;

        /// <summary>
        /// The maximum number of tokens to generate.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public int maxOutputTokens;

        /// <summary>
        /// Controls randomness in the output. Higher values make the output more random.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public float temperature = -1f;

        /// <summary>
        /// Controls diversity via nucleus sampling.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public float topP = -1f;

        /// <summary>
        /// Controls diversity via top-k sampling.
        /// </summary>
        [SuppressMessage("UnityRules.UnityStyleRules",
                "US1107:PublicFieldsMustBeUpperCamelCase",
                Justification = "JSON property.")]
        public int topK = -1;
    }
}
