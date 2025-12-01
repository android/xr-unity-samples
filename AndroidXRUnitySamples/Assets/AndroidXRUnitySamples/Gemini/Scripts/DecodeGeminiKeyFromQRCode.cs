// <copyright file="DecodeGeminiKeyFromQRCode.cs" company="Google LLC">
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
using Google.XR.Extensions;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// Decodes a Gemini API key from a QR code and saves it to the GeminiAPISettings.
    /// </summary>
    public class DecodeGeminiKeyFromQRCode : MonoBehaviour
    {
        [SerializeField] private GeminiInteractionManager _geminiInteractionManager;
        [SerializeField] private AndroidXRPermissionUtil _permissionUtil;
        [SerializeField] private ARTrackedImageManager _imageManager;

        [Header("Status visuals")]
        [SerializeField] private GameObject _infoVisuals;
        [SerializeField] private GameObject _scanningVisuals;
        [SerializeField] private GameObject _invalidVisuals;
        [SerializeField] private GameObject _errorVisuals;

        /// <summary>
        /// Callback for when a QR Code is successfully read.
        /// </summary>
        public event Action OnQrCodeSuccessfullyDecoded;

        private enum VisualsType
        {
            Info,
            Scanning,
            Invalid,
            Error
        }

        /// <summary>
        /// Starts the QR code decoding subsystem.
        /// </summary>
        public void StartDecoding()
        {
            if (_imageManager != null && _imageManager.subsystem != null)
            {
                _imageManager.subsystem.Start();
                _imageManager.trackablesChanged.AddListener(OnImageChanged);
                SetVisuals(VisualsType.Scanning);
            }
            else
            {
                Debug.LogError("ARTrackedImageManager invalid. Can't start tracking.");
                SetVisuals(VisualsType.Error);
            }
        }

        /// <summary>
        /// Stops the QR code decoding subsystem.
        /// </summary>
        public void StopDecoding()
        {
            if (_imageManager != null && _imageManager.subsystem != null)
            {
                _imageManager.trackablesChanged.RemoveListener(OnImageChanged);
                _imageManager.subsystem.Stop();
            }

            SetVisuals(VisualsType.Info);
        }

        private void Start()
        {
            SetVisuals(VisualsType.Info);
        }

        private void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
        {
            if (!_permissionUtil.AllPermissionGranted())
            {
                return;
            }

            foreach (ARTrackedImage trackedImage in eventArgs.added)
            {
                HandleTrackedImage(trackedImage);
            }

            foreach (ARTrackedImage trackedImage in eventArgs.updated)
            {
                HandleTrackedImage(trackedImage);
            }
        }

        private void HandleTrackedImage(ARTrackedImage trackedImage)
        {
            if (trackedImage.trackingState >= TrackingState.Limited && trackedImage.IsQrCode())
            {
                SetVisuals(VisualsType.Scanning);
                if (trackedImage.TryGetQrCodeData(out string qrCodeData) &&
                    !string.IsNullOrEmpty(qrCodeData) && ValidateAPIKey(qrCodeData))
                {
                    Debug.Log("Found and set Gemini API key from QR code.");
                    _geminiInteractionManager.GeminiSettings.GeminiAPIKey = qrCodeData;
                    StopDecoding();
                    OnQrCodeSuccessfullyDecoded?.Invoke();
                    GetComponent<SimpleScalingHelper>().ScaleDownAndDisable();
                }
                else
                {
                    SetVisuals(VisualsType.Invalid);
                }
            }
        }

        private void SetVisuals(VisualsType type)
        {
            _infoVisuals.SetActive(type == VisualsType.Info);
            _scanningVisuals.SetActive(type == VisualsType.Scanning);
            _invalidVisuals.SetActive(type == VisualsType.Invalid);
            _errorVisuals.SetActive(type == VisualsType.Error);
        }

        private bool ValidateAPIKey(string key)
        {
            if (string.IsNullOrEmpty(key) || key.Length != 39)
            {
                return false;
            }

            foreach (char c in key)
            {
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
