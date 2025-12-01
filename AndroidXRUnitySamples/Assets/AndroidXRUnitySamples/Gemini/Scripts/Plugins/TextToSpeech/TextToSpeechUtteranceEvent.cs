// <copyright file="TextToSpeechUtteranceEvent.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// TTS utterance event wrapper.
    /// </summary>
    [Serializable]
    public class TextToSpeechUtteranceEvent : BasePluginEvent
    {
        /// <summary>
        /// The id of the utterance.
        /// </summary>
        public string UtteranceId;

        /// <summary>
        /// Error string in case of error.
        /// </summary>
        public string Error;

        /// <summary>
        /// Error string in case of general error.
        /// </summary>
        public string FailedAction;
    }
}
