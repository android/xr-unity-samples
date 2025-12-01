// <copyright file="CameraCaptureBridge.cs" company="Google LLC">
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
    /// C# Bridge for interacting with the native Android CameraCapturePluginImpl.
    /// </summary>
    public class CameraCaptureBridge : IDisposable
    {
        private const string _javaPluginClassName =
                "com.google.xr.androidxrunitysamples.java.CameraCapturePlugin";

        private const string _javaCallbackInterfaceName =
                "com.google.xr.androidxrunitysamples.java.IPluginCallback";

        private const string _actionCaptureSingleFrame = "captureSingleFrame";
        private const string _actionCaptureFrameStream = "startContinuousCapture";
        private const string _actionCaptureFrameStreamStop = "stopContinuousCapture";
        private const string _actionSetDebugSave = "setDebugSave";
        private const string _actionSetVerboseLogging = "setVerboseLogging";

        private const string _eventCaptureComplete = "Camera_CaptureComplete";
        private const string _eventCaptureError = "Camera_CaptureError";
        private const string _eventCameraReady = "Camera_Ready";
        private const string _eventFrameDataAvailable = "Camera_FrameDataAvailable";

        private AndroidJavaObject _pluginInstance;
        private PluginCallbackProxy _callbackProxy;
        private bool _isInitialized;
        private bool _isDisposed;
        private bool _isStreamingCapture;

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraCaptureBridge"/> class.
        /// Sets up the bridge between Unity and the native Android camera capture plugin.
        /// </summary>
        public CameraCaptureBridge()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                Debug.LogError("CameraCaptureBridge is only supported on Android.");
                return;
            }

            try
            {
                _callbackProxy = new PluginCallbackProxy(this);
                using (AndroidJavaClass unityPlayer =
                       new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject activity = unityPlayer
                                   .GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        _pluginInstance = new AndroidJavaObject(_javaPluginClassName);
                        if (_pluginInstance == null)
                        {
                            throw new Exception("Failed to instantiate "
                                              + $"Java class: {_javaPluginClassName}");
                        }

                        _pluginInstance.Call("initialize", activity, _callbackProxy);
                        OnDebugLog?.Invoke(
                                "CameraCaptureBridge instantiation complete. "
                              + "Waiting for Java initialization...");
                        Debug.Log(
                                "CameraCaptureBridge instantiation complete. "
                              + "Waiting for Java initialization...");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(
                        "CameraCaptureBridge "
                      + $"instantiation failed: {e.Message}\n{e.StackTrace}");
                OnError?.Invoke(new CameraCaptureError
                {
                        Error = $"Initialization failed: {e.Message}"
                });
                Dispose();
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="CameraCaptureBridge"/> class.
        /// </summary>
        ~CameraCaptureBridge()
        {
            Dispose(false);
        }

        /// <summary>
        /// Event triggered when new frame data is available from the camera.
        /// </summary>
        public event Action<CameraFrameData> OnFrameDataReceived;

        /// <summary>
        /// Event triggered when the camera is ready for capture operations.
        /// </summary>
        public event Action OnCameraReady;

        /// <summary>
        /// Event triggered when a capture operation completes successfully.
        /// </summary>
        public event Action<CameraCaptureResult> OnCaptureComplete;

        /// <summary>
        /// Event triggered when an error occurs during camera operations.
        /// </summary>
        public event Action<CameraCaptureError> OnError;

        /// <summary>
        /// Event triggered for debug log messages from the camera bridge.
        /// </summary>
        public event Action<string> OnDebugLog;

        /// <summary>
        /// Gets a value indicating whether debug file saving is enabled.
        /// </summary>
        public static bool SaveToFileForDebug
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether verbose logging is enabled.
        /// </summary>
        public static bool EnableVerboseLogging
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the camera bridge is ready for capture operations.
        /// </summary>
        public bool IsReady => _isInitialized && !_isDisposed && !_isStreamingCapture;

        /// <summary>
        /// Requests a single frame capture.
        /// Ensure Camera and Write External Storage permissions are granted first.
        /// </summary>
        /// <param name="cameraIndex">0 for default back camera, 1 for default front, etc.</param>
        /// <param name="width">Desired width (plugin will choose optimal size near this).</param>
        /// <param name="height">Desired height.</param>
        /// <param name="savePath">
        /// Full path including
        /// filename where the JPEG image should be saved.
        /// </param>
        public void CaptureSingleFrame(int cameraIndex, int width, int height, string savePath)
        {
            if (!IsReady)
            {
                Debug.LogError("CameraCaptureBridge is "
                             + "not initialized or has been disposed.");
                OnError?.Invoke(new CameraCaptureError
                {
                        Error = "Bridge not initialized/disposed"
                });
                return;
            }

            if (string.IsNullOrEmpty(savePath))
            {
                Debug.LogError("Save path cannot be null or empty.");
                OnError?.Invoke(new CameraCaptureError
                {
                        Error = "Invalid save path provided."
                });
                return;
            }

            try
            {
                string jsonArgs =
                        $"{{\"cameraIndex\":{cameraIndex}, \"width\":{width}, "
                      + $"\"height\":{height}, \"savePath\":\"{EscapeJsonString(savePath)}\"}}";

                OnDebugLog?.Invoke($"Requesting CaptureSingleFrame with args: {jsonArgs}");
                _pluginInstance.Call("callAction",
                        _actionCaptureSingleFrame, jsonArgs);
            }
            catch (Exception e)
            {
                Debug.LogError("Error calling "
                             + $"CaptureSingleFrame: {e.Message}\n{e.StackTrace}");
                OnError?.Invoke(new CameraCaptureError
                {
                        Error = $"CaptureSingleFrame call failed: {e.Message}"
                });
            }
        }

        /// <summary>
        /// Requests a streamed frames capture.
        /// Ensure Camera permissions are granted first.
        /// </summary>
        /// <param name="cameraIndex">0 for default back camera, 1 for default front, etc.</param>
        /// <param name="width">Desired width (plugin will choose optimal size near this).</param>
        /// <param name="height">Desired height.</param>
        public void CaptureFrameStream(int cameraIndex, int width, int height)
        {
            if (!IsReady)
            {
                Debug.LogError("CameraCaptureBridge is "
                             + "not initialized or has been disposed.");
                OnError?.Invoke(new CameraCaptureError
                {
                        Error = "Bridge not initialized/disposed"
                });
                return;
            }

            try
            {
                string jsonArgs =
                        $"{{\"cameraIndex\":{cameraIndex}, "
                      + $"\"width\":{width}, \"height\":{height}}}";

                OnDebugLog?.Invoke($"Requesting CaptureFrameStream with args: {jsonArgs}");
                _isStreamingCapture = true;
                _pluginInstance.Call("callAction",
                        _actionCaptureFrameStream, jsonArgs);
            }
            catch (Exception e)
            {
                Debug.LogError("Error calling "
                             + $"CaptureFrameStream: {e.Message}\n{e.StackTrace}");
                _isStreamingCapture = false;
                OnError?.Invoke(new CameraCaptureError
                {
                        Error = $"CaptureFrameStream call failed: {e.Message}"
                });
            }
        }

        /// <summary>
        /// Requests a streamed frames capture stop.
        /// </summary>
        public void CaptureFrameStreamStop()
        {
            if (!_isStreamingCapture)
            {
                Debug.LogError("CameraCaptureBridge is not streaming capture.");
                OnError?.Invoke(new CameraCaptureError
                {
                        Error = "Not currently streaming"
                });
                return;
            }

            try
            {
                OnDebugLog?.Invoke("Requesting CaptureFrameStreamStop");
                _pluginInstance.Call("callAction",
                        _actionCaptureFrameStreamStop, "{}");
                _isStreamingCapture = false;
            }
            catch (Exception e)
            {
                Debug.LogError(
                        "Error calling CaptureFrameStreamStop:"
                      + $" {e.Message}\n{e.StackTrace}");
                _isStreamingCapture = false;
                OnError?.Invoke(new CameraCaptureError
                {
                        Error = $"CaptureFrameStreamStop call failed: {e.Message}"
                });
            }
        }

        /// <summary>
        /// Enables or disables debug file saving functionality.
        /// </summary>
        /// <param name="enabled">True to enable debug file saving, false to disable.</param>
        public void SetDebugSave(bool enabled)
        {
            try
            {
                string jsonArgs = $"{{\"enabled\":{enabled.ToString().ToLower()}}}";
                OnDebugLog?.Invoke($"Setting debug save to: {enabled}");
                _pluginInstance?.Call("callAction",
                        _actionSetDebugSave, jsonArgs);
                SaveToFileForDebug = enabled;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error setting debug save: {e.Message}");
                SaveToFileForDebug = !enabled;
            }
        }

        /// <summary>
        /// Enables or disables verbose logging functionality.
        /// </summary>
        /// <param name="enabled">True to enable verbose logging, false to disable.</param>
        public void SetVerboseLogging(bool enabled)
        {
            try
            {
                string jsonArgs = $"{{\"enabled\":{enabled.ToString().ToLower()}}}";
                OnDebugLog?.Invoke($"Setting verbose logging to: {enabled}");
                _pluginInstance?.Call("callAction",
                        _actionSetVerboseLogging, jsonArgs);
                EnableVerboseLogging = enabled;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error setting verbose logging: {e.Message}");
                EnableVerboseLogging = !enabled;
            }
        }

        /// <summary>
        /// Implements the IDisposable pattern to dispose of managed and unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern.
        /// </summary>
        /// <param name="disposing">True if disposing managed resources,
        /// false if finalizing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
            }

            if (_pluginInstance != null)
            {
                try
                {
                    if (_isInitialized)
                    {
                        OnDebugLog?.Invoke("Disposing CameraCaptureBridge...");
                        Debug.Log("Disposing CameraCaptureBridge...");
                        _pluginInstance.Call("destroy");
                    }

                    _pluginInstance.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogError("Error during CameraCaptureBridge"
                                 + $" disposal: {e.Message}");
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

            return str.Replace("\\", "\\\\")
                      .Replace("\"", "\\\"")
                      .Replace("\n", "\\n")
                      .Replace("\r", "\\r")
                      .Replace("\t", "\\t")
                      .Replace("\b", "\\b")
                      .Replace("\f", "\\f");
        }

        private class PluginCallbackProxy : AndroidJavaProxy
        {
            private readonly CameraCaptureBridge _owner;

            public PluginCallbackProxy(CameraCaptureBridge owner) : base(_javaCallbackInterfaceName)
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
                    Debug.LogWarning("Received null or empty event payload.");
                    return;
                }

                try
                {
                    var baseEvent = JsonUtility.FromJson<BasePluginEvent>(jsonPayload);
                    if (baseEvent == null || string.IsNullOrEmpty(baseEvent.Event))
                    {
                        Debug.LogError($"Failed to parse base event data: {jsonPayload}");
                        return;
                    }

                    switch (baseEvent.Event)
                    {
                        case _eventCameraReady:
                            _owner._isInitialized = true;
                            _owner.OnDebugLog?.Invoke(
                                    "Camera Plugin Ready event received from Java.");
                            _owner.OnCameraReady?.Invoke();
                            break;
                        case _eventFrameDataAvailable:
                            var frameDataBase =
                                    JsonUtility.FromJson<CameraFrameDataBase>(jsonPayload);
                            if (frameDataBase != null)
                            {
                                CameraFrameData? frameData =
                                        CameraFrameData.CreateFromNative(frameDataBase);
                                if (frameData.HasValue)
                                {
                                    _owner.OnFrameDataReceived?.Invoke(frameData.Value);
                                }
                            }
                            else
                            {
                                Debug.LogError(
                                        "Failed to parse "
                                      + $"{_eventFrameDataAvailable}: {jsonPayload}");
                            }

                            break;
                        case _eventCaptureComplete:
                            _owner._isStreamingCapture = false;
                            var result = JsonUtility.FromJson<CameraCaptureResult>(jsonPayload);
                            if (result != null)
                            {
                                _owner.OnCaptureComplete?.Invoke(result);
                            }
                            else
                            {
                                Debug.LogError(
                                        "Failed to parse "
                                      + $"Camera_CaptureComplete: {jsonPayload}");
                            }

                            break;
                        case _eventCaptureError:
                            _owner._isStreamingCapture = false;
                            var error = JsonUtility.FromJson<CameraCaptureError>(jsonPayload);
                            if (error != null)
                            {
                                _owner.OnError?.Invoke(error);
                            }
                            else
                            {
                                Debug.LogError(
                                        $"Failed to parse Camera_CaptureError: {jsonPayload}");
                            }

                            break;
                        default:
                            Debug.LogWarning(
                                    $"Received unhandled camera event: {baseEvent.Event}");
                            _owner.OnDebugLog?.Invoke($"Unhandled event: {baseEvent.Event}");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(
                            "Error processing camera event JSON:"
                          + $" {jsonPayload}\nError: {e.Message}\n{e.StackTrace}");
                    _owner.OnError?.Invoke(new CameraCaptureError
                    {
                            Error = $"JSON Processing Error: {e.Message}"
                    });
                }
            }
        }
    }
}
