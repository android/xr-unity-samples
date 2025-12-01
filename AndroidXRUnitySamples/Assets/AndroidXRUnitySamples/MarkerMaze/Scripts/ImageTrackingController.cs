// <copyright file="ImageTrackingController.cs" company="Google LLC">
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
using System.Linq;
using System.Text;
using Google.XR.Extensions;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.OpenXR;
#if XR_FEATURE_RUNTIME_TEST
    using Google.XR.Extensions.Internal;
#endif

namespace AndroidXRUnitySamples.MarkerMaze
{
    /// <summary>
    /// An sample of image tracking with <see cref="ARTrackedImageManager"/>
    /// </summary>
    [RequireComponent(typeof(AndroidXRPermissionUtil))]
    public class ImageTrackingController : MonoBehaviour
    {
        /// <summary>
        /// Text mesh to display debug information.
        /// </summary>
        public TextMesh DebugText;

        /// <summary>
        /// The <see cref="ARTrackedImageManager"/> component in the scene.
        /// </summary>
        public ARTrackedImageManager ImageManager;

        private List<TrackableId> _images = new List<TrackableId>();
        private int _imageAdded = 0;
        private int _imageUpdated = 0;
        private int _imageRemoved = 0;

        private AndroidXRPermissionUtil _permissionUtil;
        private StringBuilder _stringBuilder = new StringBuilder();

        /// <summary>
        /// Called from AR Tracked Image Manager component callback.
        /// </summary>
        /// <param name="eventArgs">Image change event arguments.</param>
        public void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
        {
            foreach (ARTrackedImage trackedImage in eventArgs.added)
            {
                Debug.LogFormat("Adding tracked image at {0}\n{1}",
                    Time.frameCount,
                    GetImageDebugInfo(trackedImage));
            }

            _imageAdded = eventArgs.added.Count;
            _imageUpdated = eventArgs.updated.Count;
            _imageRemoved = eventArgs.removed.Count;
            _images = eventArgs.updated.Select(image => image.trackableId).ToList();
        }

        private void OnEnable()
        {
            _permissionUtil = GetComponent<AndroidXRPermissionUtil>();
            if (ImageManager == null)
            {
                Debug.LogError("ARTrackedImageManager is null!");
            }
        }

        private void OnDisable()
        {
            _images.Clear();
        }

        // Update is called once per frame
        private void Update()
        {
            if (!_permissionUtil.AllPermissionGranted())
            {
                return;
            }

            _stringBuilder.Clear();
            UpdateImages();
            if (DebugText != null && _stringBuilder.Length > 0)
            {
                DebugText.text = _stringBuilder.ToString();
            }
        }

        private void UpdateImages()
        {
            if (XRQrCodeTrackingFeature.IsExtensionEnabled == null)
            {
                return;
            }
            else if (!XRQrCodeTrackingFeature.IsExtensionEnabled.Value)
            {
                _stringBuilder.Append("XR_ANDROID_trackables_qr_code is not enabled.\n");
            }
            else if (ImageManager == null || ImageManager.subsystem == null)
            {
                _stringBuilder.Append("Cannnot find ARTrackedImageManager.\n");
            }
            else
            {
                _stringBuilder.Append($"{ImageManager.subsystem.GetType()}\n");
                _stringBuilder.AppendFormat(
                    $"Images: ({_imageAdded}, {_imageUpdated}, {_imageRemoved})\n" +
                    $"{string.Join("\n", _images.Select(id => id.subId1).ToArray())}\n");
            }
        }

        private string GetImageDebugInfo(ARTrackedImage trackable)
        {
            if (trackable == null)
            {
                return string.Empty;
            }

            string reference =
                trackable.referenceImage == null ? "null" : trackable.referenceImage.name;
            string data = "No data.";
            if (trackable.IsQrCode())
            {
                trackable.TryGetQrCodeData(out data);
            }
            else if (trackable.IsMarker())
            {
                trackable.TryGetMarkerData(out XRMarkerDictionary dictionary, out int id);
                data = $"dictionary={dictionary}, id={id}";
            }

            return string.Format(
                $"Image: {trackable.trackableId.subId1}-{trackable.trackableId.subId2}\n" +
                $"  Reference: {reference}\n" +
                $"  NativePtr: {trackable.nativePtr}\n" +
                $"  Tracking: {trackable.trackingState}\n" +
                $"  Pose: {trackable.transform.position}-{trackable.transform.rotation}\n" +
                $"  Size: {trackable.size}\n" +
                $"  Data: {data}");
        }
    }
}
