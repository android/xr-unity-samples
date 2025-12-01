// <copyright file="SpeechToTextBridge.cs" company="Google LLC">
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
using UnityEngine;

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// C# Bridge for interacting with the native Android SpeechToTextPluginImpl.
    /// Handles plugin lifecycle, method calls, and event callbacks.
    /// </summary>
    public class SpeechToTextBridge : IDisposable
    {
        private const string _javaPluginClassName =
            "com.google.xr.androidxrunitysamples.java.SpeechToTextPlugin";

        private const string _javaCallbackInterfaceName =
            "com.google.xr.androidxrunitysamples.java.IPluginCallback";

        private const string _actionStartStt = "startSpeechToText";

        private const string _eventResult = "STT_Result";
        private const string _eventError = "STT_Error";
        private const string _eventReady = "STT_Ready";
        private const string _eventBeginning = "STT_Beginning";
        private const string _eventEnd = "STT_End";

        private AndroidJavaObject _pluginInstance;
        private PluginCallbackProxy _callbackProxy;
        private bool _isInitialized;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeechToTextBridge"/> class.
        /// Sets up the bridge between Unity and Android's Speech-to-Text functionality.
        /// </summary>
        public SpeechToTextBridge()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                Debug.LogError("SpeechToTextBridge is only supported on Android.");
                return;
            }

            try
            {
                _callbackProxy = new PluginCallbackProxy(this);
                using (var unityPlayer = new AndroidJavaClass(
                    "com.unity3d.player.UnityPlayer"))
                {
                    using (var activity = unityPlayer.GetStatic<AndroidJavaObject>(
                        "currentActivity"))
                    {
                        _pluginInstance = new AndroidJavaObject(_javaPluginClassName);
                        if (_pluginInstance == null)
                        {
                            throw new Exception(
                                $"Failed to instantiate Java class: {_javaPluginClassName}");
                        }

                        _pluginInstance.Call("initialize", activity, _callbackProxy);
                        _isInitialized = true;
                        OnDebugLog?.Invoke("SpeechToTextBridge initialized successfully.");
                        Debug.Log("SpeechToTextBridge initialized successfully.");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"SpeechToTextBridge initialization failed: {e.Message}\n{e.StackTrace}");
                OnError?.Invoke(new SpeechToTextError
                {
                    Error = $"Initialization failed: {e.Message}",
                    ErrorCode = -1
                });
                Dispose();
            }
        }

        ~SpeechToTextBridge()
        {
            Dispose(false);
        }

        /// <summary>
        /// Event fired when the speech recognizer is ready to begin listening.
        /// </summary>
        public event Action OnReadyForSpeech;

        /// <summary>
        /// Event fired when the user starts speaking.
        /// </summary>
        public event Action OnBeginningOfSpeech;

        /// <summary>
        /// Event fired when the user stops speaking.
        /// </summary>
        public event Action OnEndOfSpeech;

        /// <summary>
        /// Event fired when speech recognition results are available.
        /// </summary>
        public event Action<SpeechToTextResult> OnResult;

        /// <summary>
        /// Event fired when an error occurs during speech recognition.
        /// </summary>
        public event Action<SpeechToTextError> OnError;

        /// <summary>
        /// Event fired for debug logging purposes.
        /// </summary>
        public event Action<string> OnDebugLog;

        /// <summary>
        /// Starts the speech recognition process.
        /// Make sure RECORD_AUDIO permission is granted before calling this.
        /// </summary>
        public void StartRecognition()
        {
            if (!_isInitialized || _isDisposed)
            {
                Debug.LogError("SpeechToTextBridge is not initialized or has been disposed.");
                OnError?.Invoke(new SpeechToTextError
                {
                        Error = "Bridge not initialized/disposed", ErrorCode = -1
                });
                return;
            }

            try
            {
                OnDebugLog?.Invoke("Requesting StartRecognition...");

                _pluginInstance.Call("callAction", _actionStartStt, "{}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error calling StartRecognition: {e.Message}\n{e.StackTrace}");
                OnError?.Invoke(new SpeechToTextError
                {
                        Error = $"StartRecognition call failed: {e.Message}", ErrorCode = -1
                });
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(),
        /// if called from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (_isInitialized && _pluginInstance != null)
            {
                try
                {
                    OnDebugLog?.Invoke("Disposing SpeechToTextBridge...");
                    Debug.Log("Disposing SpeechToTextBridge...");
                    _pluginInstance.Call("destroy");
                    _pluginInstance.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error during SpeechToTextBridge disposal: {e.Message}");
                }
                finally
                {
                    _pluginInstance = null;
                    _isInitialized = false;
                }
            }

            _callbackProxy = null;

            _isDisposed = true;
        }

        private class PluginCallbackProxy : AndroidJavaProxy
        {
            private readonly SpeechToTextBridge _owner;

            public PluginCallbackProxy(SpeechToTextBridge owner) : base(_javaCallbackInterfaceName)
            {
                _owner = owner;
            }

            public void OnEvent(string jsonPayload)
            {
                UnityMainThreadDispatcher.Instance?.Enqueue(() => ProcessEvent(jsonPayload));
            }

            private void ProcessEvent(string jsonPayload)
            {
                if (_owner._isDisposed)
                {
                    return;
                }

                if (string.IsNullOrEmpty(jsonPayload))
                {
                    Debug.LogWarning("Received null or empty event payload on main thread.");
                    return;
                }

                try
                {
                    var baseEvent = JsonUtility.FromJson<BasePluginEvent>(jsonPayload);

                    if (baseEvent == null || string.IsNullOrEmpty(baseEvent.Event))
                    {
                        Debug.LogError(
                                $"Failed to parse base event data or missing "
                              + $"'event' field: {jsonPayload}");
                        _owner.OnError?.Invoke(new SpeechToTextError
                        {
                                Error = "Invalid event payload received", ErrorCode = -1
                        });
                        return;
                    }

                    switch (baseEvent.Event)
                    {
                        case _eventReady:
                            _owner.OnReadyForSpeech?.Invoke();
                            break;
                        case _eventBeginning:
                            _owner.OnBeginningOfSpeech?.Invoke();
                            break;
                        case _eventEnd:
                            _owner.OnEndOfSpeech?.Invoke();
                            break;
                        case _eventResult:
                            var resultData = JsonUtility.FromJson<SpeechToTextResult>(jsonPayload);
                            if (resultData != null)
                            {
                                _owner.OnResult?.Invoke(resultData);
                            }
                            else
                            {
                                Debug.LogError($"Failed to parse STT_Result: {jsonPayload}");
                            }

                            break;
                        case _eventError:
                            var errorData = JsonUtility.FromJson<SpeechToTextError>(jsonPayload);
                            if (errorData != null)
                            {
                                _owner.OnError?.Invoke(errorData);
                            }
                            else
                            {
                                Debug.LogError($"Failed to parse STT_Error: {jsonPayload}");
                            }

                            break;
                        default:
                            Debug.LogWarning(
                                    $"Received unhandled plugin event type: {baseEvent.Event}");
                            _owner.OnDebugLog?.Invoke($"Unhandled event: {baseEvent.Event}");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(
                            $"Error processing plugin event JSON "
                          + $"on main thread: {jsonPayload}\nError: {e.Message}\n{e.StackTrace}");
                    _owner.OnError?.Invoke(new SpeechToTextError
                    {
                            Error = $"JSON Processing Error: {e.Message}", ErrorCode = -1
                    });
                }
            }
        }
    }
}
