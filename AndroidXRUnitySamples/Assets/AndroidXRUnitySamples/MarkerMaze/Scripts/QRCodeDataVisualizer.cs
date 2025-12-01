// <copyright file="QRCodeDataVisualizer.cs" company="Google LLC">
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

using System.Text.RegularExpressions;
using Google.XR.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace AndroidXRUnitySamples.MarkerMaze
{
    /// <summary>
    ///   A visualizer to display tracked object by a cube.
    /// </summary>
    public class QRCodeDataVisualizer : MonoBehaviour
    {
        /// <summary>
        ///   Text to display additional information from the tracked image.
        /// </summary>
        public TextMeshProUGUI Info;

        /// <summary>
        ///   A threshold to render the minimal extent height.
        /// </summary>
        [Range(0.005f, 0.05f)]
        public float CubeEdge = 0.01f;

        private static readonly Regex _urlRegex = new Regex(@"(https?://[^\s<]+)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private string _qrCodeData = string.Empty;
        private ARTrackedImage _trackedImage;
        private MeshRenderer _renderer;

        private string _parsedUrl = string.Empty;

        /// <summary>
        /// Sets the tracked image.
        /// </summary>
        /// <param name="img">The tracked image to set.</param>
        public void SetTrackedImage(ARTrackedImage img)
        {
            _trackedImage = img;
        }

        /// <summary>
        /// Handler for the OnClick input event.
        /// </summary>
        public void HandleOnClick()
        {
            OpenURL(_parsedUrl);
        }

        /// <summary>
        /// Takes a url and returns a rich text formatted link.
        /// </summary>
        /// <param name="inputText">Input string containing a URL.</param>
        /// <returns>A "link" rich text formatted tag.</returns>
        public string FormatUrlsAsRichTextLinks(string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                return inputText;
            }

            string result = _urlRegex.Replace(inputText, match =>
            {
                string url = match.Value;
                string escapedUrl = url.Replace("\"", "\\\"");
                _parsedUrl = url;
                return $"<link=\"{escapedUrl}\"><color=blue><u>{url}</u></color></link>";
            });

            return result;
        }

        /// <summary>
        /// Opens URl in external browser.
        /// </summary>
        /// <param name="url">The url to open in an external browser.</param>
        public void OpenURL(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                Application.OpenURL(url);
                Debug.Log("Attempting to open URL: " + url);
            }
            else
            {
                Debug.LogError("URL to open is not set!");
            }
        }

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        private void OnDisable()
        {
            UpdateVisibility();
        }

        private void Update()
        {
            if (_trackedImage != null)
            {
                transform.localScale =
                        new Vector3(_trackedImage.size.x, CubeEdge, _trackedImage.size.y);
                UpdateVisibility();
            }
        }

        private void UpdateVisibility()
        {
            bool visible = enabled && _trackedImage?.trackingState >= TrackingState.Limited &&
                           ARSession.state > ARSessionState.Ready;

            if (visible)
            {
                if (_trackedImage.IsQrCode())
                {
                    _trackedImage.TryGetQrCodeData(out _qrCodeData);
                    if (Info != null)
                    {
                        string formattedText = FormatUrlsAsRichTextLinks(_qrCodeData);
                        Info.text = formattedText;
                    }
                }
            }

            Info?.gameObject.SetActive(visible);

            if (_renderer != null)
            {
                _renderer.enabled = visible;
            }
        }
    }
}
