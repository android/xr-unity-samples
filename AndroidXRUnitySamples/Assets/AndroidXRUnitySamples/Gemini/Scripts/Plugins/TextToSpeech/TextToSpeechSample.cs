// <copyright file="TextToSpeechSample.cs" company="Google LLC">
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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// Sample MonoBehaviour demonstrating the usage of Text-to-Speech functionality.
    /// Provides a UI for text input, language selection, and speech parameter controls.
    /// </summary>
    public class TextToSpeechSample : MonoBehaviour
    {
        private readonly Dictionary<string, string> _languages = new Dictionary<string, string>
        {
                {
                        "English (US)", "en-US"
                },
                {
                        "English (UK)", "en-GB"
                },
                {
                        "French (France)", "fr-FR"
                },
                {
                        "German (Germany)", "de-DE"
                },
                {
                        "Spanish (Spain)", "es-ES"
                }
        };

        [SerializeField]
        private TMP_InputField _textToSpeakInput;

        [SerializeField]
        private Button _speakButton;

        [SerializeField]
        private Button _stopButton;

        [SerializeField]
        private TMP_Text _statusText;

        [SerializeField]
        private Slider _pitchSlider;

        [SerializeField]
        private Slider _rateSlider;

        [SerializeField]
        private TMP_Dropdown _languageDropdown;

        private TextToSpeechBridge _ttsBridge;
        private bool _isReady;

        private void Start()
        {
            _statusText.text = "Initializing TTS Sample...";
            SetupUI();

            if (Application.platform == RuntimePlatform.Android)
            {
                InitializeTTSBridge();
            }
            else
            {
                _statusText.text = "TTS only on Android.";
                DisableUI();
            }
        }

        private void SetupUI()
        {
            _speakButton?.onClick.AddListener(OnSpeakButtonClicked);
            _stopButton?.onClick.AddListener(OnStopButtonClicked);
            _pitchSlider?.onValueChanged.AddListener(OnPitchChanged);
            _rateSlider?.onValueChanged.AddListener(OnRateChanged);
            _languageDropdown?.onValueChanged.AddListener(OnLanguageChanged);

            if (_languageDropdown != null)
            {
                _languageDropdown.ClearOptions();
                _languageDropdown.AddOptions(new List<string>(_languages.Keys));
                _languageDropdown.value = 0;
                _languageDropdown.RefreshShownValue();
            }

            DisableUI();
        }

        private void DisableUI()
        {
            _speakButton.interactable = false;
            _stopButton.interactable = false;
            _pitchSlider.interactable = false;
            _rateSlider.interactable = false;
            _languageDropdown.interactable = false;
            _textToSpeakInput.interactable = false;
        }

        private void EnableUI()
        {
            _speakButton.interactable = true;
            _stopButton.interactable = true;
            _pitchSlider.interactable = true;
            _rateSlider.interactable = true;
            _languageDropdown.interactable = true;
            _textToSpeakInput.interactable = true;
        }

        private void InitializeTTSBridge()
        {
            if (_ttsBridge != null)
            {
                return;
            }

            _statusText.text = "Getting TTS Bridge...";
            _ttsBridge = PluginRegistry.Instance.Get<TextToSpeechBridge>();

            if (_ttsBridge != null)
            {
                _ttsBridge.OnInitResult += HandleInitResult;
                _ttsBridge.OnSpeakStart += HandleSpeakStart;
                _ttsBridge.OnSpeakDone += HandleSpeakDone;
                _ttsBridge.OnSpeakError += HandleSpeakError;
                _ttsBridge.OnDebugLog += HandleDebugLog;

                _statusText.text = "Waiting for TTS Init event...";
            }
            else
            {
                _statusText.text = "Failed to get TTS Bridge!";
                Debug.LogError("Failed to get or create TextToSpeechBridge from registry.");
            }
        }

        private void OnSpeakButtonClicked()
        {
            if (!_isReady || _ttsBridge == null)
            {
                return;
            }

            string text = _textToSpeakInput.text;
            if (!string.IsNullOrEmpty(text))
            {
                _statusText.text = "Speaking...";
                _speakButton.interactable = false;
                _stopButton.interactable = true;
                _ttsBridge.Speak(text);
            }
        }

        private void OnStopButtonClicked()
        {
            if (!_isReady || _ttsBridge == null)
            {
                return;
            }

            _statusText.text = "Stopping...";
            _ttsBridge.Stop();

            _speakButton.interactable = true;
            _stopButton.interactable = false;
        }

        private void OnPitchChanged(float value)
        {
            if (!_isReady || _ttsBridge == null)
            {
                return;
            }

            _ttsBridge.SetPitch(value);
        }

        private void OnRateChanged(float value)
        {
            if (!_isReady || _ttsBridge == null)
            {
                return;
            }

            _ttsBridge.SetSpeechRate(value);
        }

        private void OnLanguageChanged(int index)
        {
            if (!_isReady || _ttsBridge == null || _languageDropdown == null)
            {
                return;
            }

            string selectedKey = _languageDropdown.options[index].text;
            if (_languages.TryGetValue(selectedKey, out string langTag))
            {
                _statusText.text = $"Setting lang to {langTag}...";
                _ttsBridge.SetLanguage(langTag);
            }
        }

        private void HandleInitResult(TextToSpeechStatus result)
        {
            if (result.Event == "TTS_InitSuccess")
            {
                Debug.Log($"Event: TTS Initialized: {result.Message}");
                _isReady = true;
                _statusText.text = "TTS Ready";
                EnableUI();

                if (_pitchSlider != null)
                {
                    _ttsBridge?.SetPitch(_pitchSlider.value);
                }

                if (_rateSlider != null)
                {
                    _ttsBridge?.SetSpeechRate(_rateSlider.value);
                }

                if (_languageDropdown != null)
                {
                    OnLanguageChanged(_languageDropdown.value);
                }
            }
            else
            {
                _isReady = false;
                Debug.LogError($"Event: TTS Init Error: {result.Message}");
                _statusText.text = $"TTS Init Error: {result.Message}";
                DisableUI();
            }
        }

        private void HandleSpeakStart(TextToSpeechUtteranceEvent utteranceEvent)
        {
            Debug.Log($"Event: Speak Start (ID: {utteranceEvent.UtteranceId})");
            _statusText.text = "Speaking...";
            _speakButton.interactable = false;
            _stopButton.interactable = true;
        }

        private void HandleSpeakDone(TextToSpeechUtteranceEvent utteranceEvent)
        {
            Debug.Log($"Event: Speak Done (ID: {utteranceEvent.UtteranceId})");
            if (_statusText.text == "Speaking...")
            {
                _statusText.text = "TTS Ready";
            }

            _speakButton.interactable = true;
            _stopButton.interactable = false;
        }

        private void HandleSpeakError(TextToSpeechUtteranceEvent utteranceEvent)
        {
            Debug.LogError(
                    $"Event: Speak Error "
                  + $"(ID: {utteranceEvent.UtteranceId}): {utteranceEvent.Error}");
            _statusText.text = $"TTS Error: {utteranceEvent.Error}";
            _speakButton.interactable = true;
            _stopButton.interactable = false;
        }

        private void HandleDebugLog(string message)
        {
            Debug.Log($"[TTS Bridge]: {message}");
        }

        private void OnDestroy()
        {
            if (_ttsBridge != null)
            {
                _ttsBridge.OnInitResult -= HandleInitResult;
                _ttsBridge.OnSpeakStart -= HandleSpeakStart;
                _ttsBridge.OnSpeakDone -= HandleSpeakDone;
                _ttsBridge.OnSpeakError -= HandleSpeakError;
                _ttsBridge.OnDebugLog -= HandleDebugLog;
            }
        }
    }
}
