// <copyright file="TextToSpeechBridge.cs" company="Google LLC">
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
using System.Globalization;
using UnityEngine;

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// Bridge class for Text-to-Speech functionality.
    /// Provides an interface between Unity and Android's Text-to-Speech capabilities.
    /// </summary>
    public class TextToSpeechBridge : IDisposable
    {
        private const string _javaPluginClassName =
                "com.google.xr.androidxrunitysamples.java.TextToSpeechPlugin";

        private const string _javaCallbackInterfaceName =
                "com.google.xr.androidxrunitysamples.java.IPluginCallback";

        private const string _actionSpeak = "speak";
        private const string _actionStop = "stop";
        private const string _actionSetLang = "setLanguage";
        private const string _actionSetPitch = "setPitch";
        private const string _actionSetRate = "setSpeechRate";

        private const string _eventInitSuccess = "TTS_InitSuccess";
        private const string _eventInitError = "TTS_InitError";
        private const string _eventSpeakStart = "TTS_SpeakStart";
        private const string _eventSpeakDone = "TTS_SpeakDone";
        private const string _eventSpeakError = "TTS_SpeakError";
        private const string _eventLangChanged = "TTS_LangChanged";
        private const string _eventPitchChanged = "TTS_PitchChanged";
        private const string _eventRateChanged = "TTS_RateChanged";

        private AndroidJavaObject _pluginInstance;
        private PluginCallbackProxy _callbackProxy;
        private bool _isInitialized;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextToSpeechBridge"/> class.
        /// Sets up the bridge between Unity and Android's Text-to-Speech functionality.
        /// </summary>
        public TextToSpeechBridge()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                Debug.LogError("TextToSpeechBridge is only supported on Android.");
                return;
            }

            try
            {
                _callbackProxy = new PluginCallbackProxy(this);
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (var activity = unityPlayer.GetStatic<AndroidJavaObject>(
                                   "currentActivity"))
                    {
                        _pluginInstance = new AndroidJavaObject(_javaPluginClassName);
                        if (_pluginInstance == null)
                        {
                            throw new Exception(
                                    $"Failed to instantiate Java class: "
                                  + $"{_javaPluginClassName}");
                        }

                        _pluginInstance.Call("initialize",
                                activity, _callbackProxy);
                        OnDebugLog?.Invoke(
                                "TextToSpeechBridge instantiation complete. "
                              + "Waiting for Java initialization...");
                        Debug.Log(
                                "TextToSpeechBridge instantiation complete. "
                              + "Waiting for Java initialization...");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(
                        $"TextToSpeechBridge instantiation "
                      + $"failed: {e.Message}\n{e.StackTrace}");
                Dispose();
            }
        }

        ~TextToSpeechBridge()
        {
            Dispose(false);
        }

        /// <summary>
        /// Event fired when initialization is complete, with success or failure status.
        /// </summary>
        public event Action<TextToSpeechStatus> OnInitResult;

        /// <summary>
        /// Event fired when speech synthesis begins.
        /// </summary>
        public event Action<TextToSpeechUtteranceEvent> OnSpeakStart;

        /// <summary>
        /// Event fired when speech synthesis completes successfully.
        /// </summary>
        public event Action<TextToSpeechUtteranceEvent> OnSpeakDone;

        /// <summary>
        /// Event fired when an error occurs during speech synthesis.
        /// </summary>
        public event Action<TextToSpeechUtteranceEvent> OnSpeakError;

        /// <summary>
        /// Event fired when the language setting changes.
        /// </summary>
        public event Action<TextToSpeechStatus> OnLanguageChanged;

        /// <summary>
        /// Event fired when the pitch setting changes.
        /// </summary>
        public event Action<TextToSpeechStatus> OnPitchChanged;

        /// <summary>
        /// Event fired when the speech rate setting changes.
        /// </summary>
        public event Action<TextToSpeechStatus> OnRateChanged;

        /// <summary>
        /// Event fired for debug logging purposes.
        /// </summary>
        public event Action<string> OnDebugLog;

        /// <summary>
        /// Gets a value indicating whether the bridge is ready for use.
        /// </summary>
        public bool IsReady => _isInitialized && !_isDisposed;

        /// <summary>
        /// Send a string of text to TTS plugin.
        /// </summary>
        /// <param name="text">The text to be spoken by the Text-to-Speech engine.</param>
        public void Speak(string text)
        {
            if (!IsReady)
            {
                LogErrorAndCallback("Cannot Speak: Bridge not ready.");
                return;
            }

            if (string.IsNullOrEmpty(text))
            {
                LogErrorAndCallback("Cannot Speak: Text is null or empty.");
                return;
            }

            try
            {
                string jsonArgs = $"{{\"text\":\"{EscapeJsonString(text)}\"}}";
                OnDebugLog?.Invoke($"Requesting Speak with args: {jsonArgs}");
                _pluginInstance.Call("callAction", _actionSpeak, jsonArgs);
            }
            catch (Exception e)
            {
                LogErrorAndCallback($"Speak call failed: {e.Message}", e);
            }
        }

        /// <summary>
        /// Stops speech.
        /// </summary>
        public void Stop()
        {
            if (!IsReady)
            {
                LogErrorAndCallback("Cannot Stop: Bridge not ready.");
                return;
            }

            try
            {
                OnDebugLog?.Invoke("Requesting Stop");
                _pluginInstance.Call("callAction", _actionStop, "{}");
            }
            catch (Exception e)
            {
                LogErrorAndCallback($"Stop call failed: {e.Message}", e);
            }
        }

        /// <summary>
        /// Set plugin language.
        /// </summary>
        /// <param name="languageTag">The BCP 47 language tag (e.g., "en-US", "fr-FR").</param>
        public void SetLanguage(string languageTag)
        {
            if (!IsReady)
            {
                LogErrorAndCallback("Cannot SetLanguage: Bridge not ready.");
                return;
            }

            if (string.IsNullOrEmpty(languageTag))
            {
                LogErrorAndCallback("Cannot SetLanguage: Language tag is null or empty.");
                return;
            }

            try
            {
                string jsonArgs = $"{{\"language\":\"{EscapeJsonString(languageTag)}\"}}";
                OnDebugLog?.Invoke($"Requesting SetLanguage with args: {jsonArgs}");
                _pluginInstance.Call("callAction", _actionSetLang, jsonArgs);
            }
            catch (Exception e)
            {
                LogErrorAndCallback($"SetLanguage call failed: {e.Message}", e);
            }
        }

        /// <summary>
        /// Set utterance pitch.
        /// </summary>
        /// <param name="pitch">The pitch value between 0.0 and 2.0,
        /// where 1.0 is normal pitch.</param>
        public void SetPitch(float pitch)
        {
            if (!IsReady)
            {
                LogErrorAndCallback("Cannot SetPitch: Bridge not ready.");
                return;
            }

            try
            {
                string jsonArgs = $"{{\"pitch\":{pitch.ToString(CultureInfo.InvariantCulture)}}}";
                OnDebugLog?.Invoke($"Requesting SetPitch with args: {jsonArgs}");
                _pluginInstance.Call("callAction", _actionSetPitch, jsonArgs);
            }
            catch (Exception e)
            {
                LogErrorAndCallback($"SetPitch call failed: {e.Message}", e);
            }
        }

        /// <summary>
        /// Set utterance speech rate.
        /// </summary>
        /// <param name="rate">The speech rate between 0.0 and 2.0, where 1.0
        /// is normal speed.</param>
        public void SetSpeechRate(float rate)
        {
            if (!IsReady)
            {
                LogErrorAndCallback("Cannot SetSpeechRate: Bridge not ready.");
                return;
            }

            try
            {
                string jsonArgs = $"{{\"rate\":{rate.ToString(CultureInfo.InvariantCulture)}}}";
                OnDebugLog?.Invoke($"Requesting SetSpeechRate with args: {jsonArgs}");
                _pluginInstance.Call("callAction", _actionSetRate, jsonArgs);
            }
            catch (Exception e)
            {
                LogErrorAndCallback($"SetSpeechRate call failed: {e.Message}", e);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(),
        /// false if called from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (_pluginInstance != null)
            {
                try
                {
                    OnDebugLog?.Invoke("Disposing TextToSpeechBridge...");
                    Debug.Log("Disposing TextToSpeechBridge...");
                    _pluginInstance.Call("destroy");
                    _pluginInstance.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error during TextToSpeechBridge disposal: {e.Message}");
                }
                finally
                {
                    _pluginInstance = null;
                }
            }

            _isInitialized = false;
            _callbackProxy = null;
            _isDisposed = true;
        }

        private string EscapeJsonString(string str)
        {
            if (str == null)
            {
                return string.Empty;
            }

            str = str.Replace("\\", "\\\\");
            str = str.Replace("\"", "\\\"");
            return str;
        }

        private void LogErrorAndCallback(string message, Exception ex = null)
        {
            Debug.LogError(ex != null ? $"{message}\n{ex.StackTrace}" : message);

            OnSpeakError?.Invoke(new TextToSpeechUtteranceEvent
            {
                    Error = message, Event = _eventSpeakError
            });
        }

        private class PluginCallbackProxy : AndroidJavaProxy
        {
            private readonly TextToSpeechBridge _owner;

            public PluginCallbackProxy(TextToSpeechBridge owner) : base(_javaCallbackInterfaceName)
            {
                _owner = owner;
            }

            public void OnEvent(string jsonPayload)
            {
                UnityMainThreadDispatcher.RunOnMainThread(() => ProcessEvent(jsonPayload));
            }

            private void ProcessEvent(string jsonPayload)
            {
                if (_owner._isDisposed)
                {
                    return;
                }

                if (string.IsNullOrEmpty(jsonPayload))
                {
                    Debug.LogWarning("Received null/empty TTS event.");
                    return;
                }

                try
                {
                    var baseEvent = JsonUtility.FromJson<BasePluginEvent>(jsonPayload);
                    if (baseEvent == null || string.IsNullOrEmpty(baseEvent.Event))
                    {
                        Debug.LogError($"Failed to parse base TTS event: {jsonPayload}");
                        return;
                    }

                    switch (baseEvent.Event)
                    {
                        case _eventInitSuccess:
                            _owner._isInitialized = true;
                            var initSuccess = JsonUtility.FromJson<TextToSpeechStatus>(jsonPayload);
                            _owner.OnDebugLog?.Invoke($"TTS Init Success: {initSuccess?.Message}");
                            _owner.OnInitResult?.Invoke(initSuccess);
                            break;
                        case _eventInitError:
                            _owner._isInitialized = false;
                            var initError = JsonUtility.FromJson<TextToSpeechStatus>(jsonPayload);
                            _owner.OnDebugLog?.Invoke($"TTS Init Error: {initError?.Message}");
                            _owner.OnInitResult?.Invoke(initError);
                            break;

                        case _eventSpeakStart:
                            var startEvent =
                                    JsonUtility.FromJson<TextToSpeechUtteranceEvent>(jsonPayload);
                            if (startEvent != null)
                            {
                                _owner.OnSpeakStart?.Invoke(startEvent);
                            }
                            else
                            {
                                Debug.LogError(
                                        $"Failed to parse {_eventSpeakStart}: {jsonPayload}");
                            }

                            break;
                        case _eventSpeakDone:
                            var doneEvent =
                                    JsonUtility.FromJson<TextToSpeechUtteranceEvent>(jsonPayload);
                            if (doneEvent != null)
                            {
                                _owner.OnSpeakDone?.Invoke(doneEvent);
                            }
                            else
                            {
                                Debug.LogError($"Failed to parse "
                                             + $"{_eventSpeakDone}: {jsonPayload}");
                            }

                            break;
                        case _eventSpeakError:
                            var errorEvent =
                                    JsonUtility.FromJson<TextToSpeechUtteranceEvent>(jsonPayload);
                            if (errorEvent != null)
                            {
                                _owner.OnSpeakError?.Invoke(errorEvent);
                            }
                            else
                            {
                                Debug.LogError(
                                        $"Failed to parse {_eventSpeakError}: {jsonPayload}");
                            }

                            break;

                        case _eventLangChanged:
                            var langEvent = JsonUtility.FromJson<TextToSpeechStatus>(jsonPayload);
                            if (langEvent != null)
                            {
                                _owner.OnLanguageChanged?.Invoke(langEvent);
                            }
                            else
                            {
                                Debug.LogError(
                                        $"Failed to parse {_eventLangChanged}: {jsonPayload}");
                            }

                            break;
                        case _eventPitchChanged:
                            var pitchEvent = JsonUtility.FromJson<TextToSpeechStatus>(jsonPayload);
                            if (pitchEvent != null)
                            {
                                _owner.OnPitchChanged?.Invoke(pitchEvent);
                            }
                            else
                            {
                                Debug.LogError(
                                        $"Failed to parse {_eventPitchChanged}: {jsonPayload}");
                            }

                            break;
                        case _eventRateChanged:
                            var rateEvent = JsonUtility.FromJson<TextToSpeechStatus>(jsonPayload);
                            if (rateEvent != null)
                            {
                                _owner.OnRateChanged?.Invoke(rateEvent);
                            }
                            else
                            {
                                Debug.LogError(
                                        $"Failed to parse {_eventRateChanged}: {jsonPayload}");
                            }

                            break;

                        default:
                            Debug.LogWarning($"Received unhandled TTS event: {baseEvent.Event}");
                            _owner.OnDebugLog?.Invoke($"Unhandled event: {baseEvent.Event}");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(
                            $"Error processing TTS event JSON: "
                          + $"{jsonPayload}\nError: {e.Message}\n{e.StackTrace}");
                    _owner.OnSpeakError?.Invoke(new TextToSpeechUtteranceEvent
                    {
                            Error = $"JSON Processing Error: {e.Message}", Event = _eventSpeakError
                    });
                }
            }
        }
    }
}
