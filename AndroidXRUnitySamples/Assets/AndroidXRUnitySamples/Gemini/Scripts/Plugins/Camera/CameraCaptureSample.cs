// <copyright file="CameraCaptureSample.cs" company="Google LLC">
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
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// Sample MonoBehaviour demonstrating the usage of the Camera Capture plugin.
    /// Handles UI interactions, permissions, and displaying captured images.
    /// </summary>
    public class CameraCaptureSample : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button _captureButtonSingle;

        [SerializeField] private Button _captureButtonStream;
        [SerializeField] private Button _captureButtonStop;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private RawImage _displayImage;
        [SerializeField] private Toggle _debugSaveToggle;
        [SerializeField] private Toggle _verboseLogToggle;

        [Header("Texture Management")]
        [Tooltip("Reuse texture to avoid GC allocs, but ensure dimensions match or handle resize.")]
        [SerializeField] private bool _reuseTexture = true;

        private CameraCaptureBridge _cameraBridge;
        private bool _isReady;
        private Texture2D _displayTexture;

        private void Start()
        {
            _statusText.text = "Initializing Camera Sample...";
            if (_captureButtonSingle != null)
            {
                _captureButtonSingle.onClick.AddListener(OnCaptureSingleButtonClicked);
                _captureButtonSingle.interactable = false;
            }

            if (_captureButtonStream != null)
            {
                _captureButtonStream.onClick.AddListener(OnCaptureStreamButtonClicked);
                _captureButtonStream.interactable = false;
            }

            if (_captureButtonStop != null)
            {
                _captureButtonStop.onClick.AddListener(OnCaptureStopButtonClicked);
                _captureButtonStop.interactable = false;
            }

            if (_debugSaveToggle != null)
            {
                _debugSaveToggle.onValueChanged.AddListener(OnDebugSaveToggleChanged);
                _debugSaveToggle.interactable = false;
            }

            if (_verboseLogToggle != null)
            {
                _verboseLogToggle.onValueChanged.AddListener(OnVerboseLogToggleChanged);
                _verboseLogToggle.interactable = false;
            }

            if (_displayImage != null)
            {
                _displayImage.enabled = false;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                RequestPermissionsAndInitialize();
            }
            else
            {
                _statusText.text = "Camera Capture only on Android.";
                if (_captureButtonSingle != null)
                {
                    _captureButtonSingle.interactable = false;
                }

                if (_captureButtonStream != null)
                {
                    _captureButtonStream.interactable = false;
                }

                if (_captureButtonStop != null)
                {
                    _captureButtonStop.interactable = false;
                }

                if (_debugSaveToggle != null)
                {
                    _debugSaveToggle.interactable = false;
                }

                if (_verboseLogToggle != null)
                {
                    _verboseLogToggle.interactable = false;
                }
            }
        }

        private async void RequestPermissionsAndInitialize()
        {
            bool hasCamPerm = Permission.HasUserAuthorizedPermission(Permission.Camera);

            if (!hasCamPerm)
            {
                _statusText.text = "Requesting Permissions...";
                var permissionsToRequest = new List<string>();
                if (!hasCamPerm)
                {
                    permissionsToRequest.Add(Permission.Camera);
                }

                var tcs = new TaskCompletionSource<bool>();
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionGranted += perm =>
                {
                    if (Permission.HasUserAuthorizedPermission(Permission.Camera))
                    {
                        tcs.TrySetResult(true);
                    }
                };
                callbacks.PermissionDenied += perm =>
                {
                    bool shouldShowRationale = Permission.ShouldShowRequestPermissionRationale(
                        Permission.Camera);
                    _statusText.text = shouldShowRationale ?
                        "Permission denied. Please grant camera access in settings." :
                        "Permission denied. Camera access is required.";
                    tcs.TrySetResult(false);
                };

                Permission.RequestUserPermissions(permissionsToRequest.ToArray(), callbacks);

                bool granted = await tcs.Task;

                if (!granted)
                {
                    _statusText.text = "Permissions Denied.";
                    Debug.LogError("Required permissions were denied.");
                    return;
                }

                Debug.Log("Permissions granted after request.");
            }

            InitializeCameraBridge();
        }

        private void InitializeCameraBridge()
        {
            if (_cameraBridge != null)
            {
                return;
            }

            _statusText.text = "Getting Camera Bridge...";
            _cameraBridge = PluginRegistry.Instance.Get<CameraCaptureBridge>();

            if (_cameraBridge != null)
            {
                _cameraBridge.OnCameraReady += HandleCameraReady;
                _cameraBridge.OnFrameDataReceived += HandleFrameDataReceived;
                _cameraBridge.OnError += HandleError;
                _cameraBridge.OnDebugLog += HandleDebugLog;
                _cameraBridge.OnCaptureComplete += HandleCaptureCompleteDebug;

                _statusText.text = "Waiting for Camera Ready event...";
            }
            else
            {
                _statusText.text = "Failed to get Camera Bridge!";
                Debug.LogError("Failed to get or create CameraCaptureBridge from registry.");
            }
        }

        private void OnCaptureSingleButtonClicked()
        {
            if (!_isReady || _cameraBridge == null)
            {
                _statusText.text = "Camera not ready.";
                return;
            }

            _captureButtonSingle.interactable = false;
            _captureButtonStream.interactable = false;
            _captureButtonStop.interactable = false;
            _statusText.text = "Capturing...";
            if (_displayImage != null)
            {
                _displayImage.enabled = false;
            }

            string debugSavePath = Path.Combine(Application.persistentDataPath,
                    "TestCameraCaptures", $"DebugCapture_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");

            if (CameraCaptureBridge.EnableVerboseLogging)
            {
                Debug.Log($"Requesting capture. Debug path (if enabled): {debugSavePath}");
            }

            _cameraBridge.CaptureSingleFrame(0, 1024, 1024, debugSavePath);
        }

        private void OnCaptureStreamButtonClicked()
        {
            if (!_isReady || _cameraBridge == null)
            {
                _statusText.text = "Camera not ready.";
                return;
            }

            _captureButtonSingle.interactable = false;
            _captureButtonStream.interactable = false;
            _captureButtonStop.interactable = true;
            _statusText.text = "Capturing...";
            if (_displayImage != null)
            {
                _displayImage.enabled = false;
            }

            string debugSavePath = Path.Combine(Application.persistentDataPath,
                    "TestCameraCaptures", $"DebugCapture_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");

            if (CameraCaptureBridge.EnableVerboseLogging)
            {
                Debug.Log($"Requesting streamed capture. Debug path (if enabled): {debugSavePath}");
            }

            _cameraBridge.CaptureFrameStream(0, 1024, 1024);
        }

        private void OnCaptureStopButtonClicked()
        {
            if (!_isReady || _cameraBridge == null)
            {
                _statusText.text = "Camera not ready.";
                return;
            }

            _captureButtonSingle.interactable = true;
            _captureButtonStream.interactable = true;
            _captureButtonStop.interactable = false;
            _statusText.text = "Stopping...";
            if (_displayImage != null)
            {
                _displayImage.enabled = false;
            }

            if (CameraCaptureBridge.EnableVerboseLogging)
            {
                Debug.Log("Requesting streamed capture stop.");
            }

            _cameraBridge.CaptureFrameStreamStop();
        }

        private void OnDebugSaveToggleChanged(bool isOn)
        {
            if (_isReady && _cameraBridge != null)
            {
                _cameraBridge.SetDebugSave(isOn);
                _statusText.text = $"Debug Save: {(isOn ? "ON" : "OFF")}";
            }
        }

        private void OnVerboseLogToggleChanged(bool isOn)
        {
            if (_isReady && _cameraBridge != null)
            {
                _cameraBridge.SetVerboseLogging(isOn);
                Debug.Log($"Verbose Logging set to: {isOn}");
            }
        }

        private void HandleCameraReady()
        {
            if (CameraCaptureBridge.EnableVerboseLogging)
            {
                Debug.Log("Event: Camera Ready");
            }

            _isReady = true;
            _statusText.text = "Camera Ready";
            if (_captureButtonSingle != null)
            {
                _captureButtonSingle.interactable = true;
            }

            if (_captureButtonStream != null)
            {
                _captureButtonStream.interactable = true;
            }

            if (_captureButtonStop != null)
            {
                _captureButtonStop.interactable = false;
            }

            if (_debugSaveToggle != null)
            {
                _debugSaveToggle.interactable = true;
                _debugSaveToggle.isOn = CameraCaptureBridge.SaveToFileForDebug;
            }

            if (_verboseLogToggle != null)
            {
                _verboseLogToggle.interactable = true;
                _verboseLogToggle.isOn = CameraCaptureBridge.EnableVerboseLogging;
            }
        }

        private void HandleFrameDataReceived(CameraFrameData frameData)
        {
            if (CameraCaptureBridge.EnableVerboseLogging)
            {
                Debug.Log($"Event: Frame Data Received! Size: "
                        + $"{frameData.ImageData?.Length ?? 0}, " +
                    $"Format: {frameData.Format}");
            }

            _statusText.text = $"Image Received ({frameData.Width}x{frameData.Height})";

            if (frameData.ImageData != null && _displayImage != null)
            {
                LoadBytesToDisplay(frameData.ImageData, frameData.Width, frameData.Height);
            }

            if (_cameraBridge.IsReady)
            {
                if (_captureButtonSingle != null)
                {
                    _captureButtonSingle.interactable = true;
                }

                if (_captureButtonStream != null)
                {
                    _captureButtonStream.interactable = true;
                }

                if (_captureButtonStop != null)
                {
                    _captureButtonStop.interactable = false;
                }
            }
        }

        private void HandleCaptureCompleteDebug(CameraCaptureResult result)
        {
            Debug.Log($"[Debug] Event: Capture Saved To File! Path: {result.ImagePath}");
        }

        private void HandleError(CameraCaptureError error)
        {
            Debug.LogError($"Event: Camera Error: {error.Error}");
            _statusText.text = $"Error: {error.Error}";
            _isReady = _cameraBridge?.IsReady ?? false;
            if (_captureButtonSingle != null)
            {
                _captureButtonSingle.interactable = _isReady;
            }

            if (_debugSaveToggle != null)
            {
                _debugSaveToggle.interactable = _isReady;
            }
        }

        private void HandleDebugLog(string message)
        {
            Debug.Log($"[Camera Bridge]: {message}");
        }

        private void LoadBytesToDisplay(byte[] imageData, int width, int height)
        {
            if (_displayImage == null || imageData == null || imageData.Length == 0)
            {
                if (_displayImage != null)
                {
                    _displayImage.enabled = false;
                }

                return;
            }

            try
            {
                if (_displayTexture == null || !_reuseTexture || _displayTexture.width != width
                 || _displayTexture.height != height)
                {
                    if (_displayTexture != null)
                    {
                        Destroy(_displayTexture);
                        _displayTexture = null;
                    }

                    _displayTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    if (CameraCaptureBridge.EnableVerboseLogging)
                    {
                        Debug.Log($"Created new Texture2D {width}x{height}");
                    }
                }

                if (_displayTexture.LoadImage(imageData))
                {
                    _displayImage.texture = _displayTexture;
                    _displayImage.enabled = true;
                }
                else
                {
                    Debug.LogError("Failed to load image data into texture.");
                    _displayImage.enabled = false;
                    if (_displayTexture != null && !_reuseTexture)
                    {
                        Destroy(_displayTexture);
                        _displayTexture = null;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading image bytes to texture: {e.Message}");
                _displayImage.enabled = false;
                if (_displayTexture != null && !_reuseTexture)
                {
                    Destroy(_displayTexture);
                    _displayTexture = null;
                }
            }
        }

        private void OnDestroy()
        {
            if (_cameraBridge != null)
            {
                _cameraBridge.OnCameraReady -= HandleCameraReady;
                _cameraBridge.OnFrameDataReceived -= HandleFrameDataReceived;
                _cameraBridge.OnError -= HandleError;
                _cameraBridge.OnDebugLog -= HandleDebugLog;
                _cameraBridge.OnCaptureComplete -= HandleCaptureCompleteDebug;
            }

            if (_displayTexture != null)
            {
                Destroy(_displayTexture);
                _displayTexture = null;
            }
        }
    }
}
