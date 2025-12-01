// <copyright file="CameraFrameData.cs" company="Google LLC">
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
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Serialization;

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// Represents a camera frame with managed image data and metadata.
    /// </summary>
    public struct CameraFrameData
    {
        /// <summary>
        /// Creates a new CameraFrameData instance.
        /// </summary>
        /// <param name="data">Raw image data bytes.</param>
        /// <param name="width">Width of the image in pixels.</param>
        /// <param name="height">Height of the image in pixels.</param>
        /// <param name="format">Format of the image data.</param>
        /// <param name="timestamp">Capture timestamp in milliseconds.</param>
        private CameraFrameData(byte[] data, int width, int height, string format, long timestamp)
        {
            ImageData = data;
            Width = width;
            Height = height;
            Format = format;
            Timestamp = timestamp;
        }

        /// <summary>
        /// Gets the raw image data bytes.
        /// </summary>
        public byte[] ImageData { get; private set; }

        /// <summary>
        /// Gets the width of the image in pixels.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the height of the image in pixels.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets the format of the image data.
        /// </summary>
        public string Format { get; private set; }

        /// <summary>
        /// Gets the capture timestamp in milliseconds.
        /// </summary>
        public long Timestamp { get; private set; }

        /// <summary>
        /// Creates CameraFrameData by copying data from the native pointer.
        /// </summary>
        /// <param name="baseEventData">Data received from Java containing the pointer.</param>
        /// <returns>CameraFrameData with copied byte array, or null if copy fails.</returns>
        public static CameraFrameData? CreateFromNative(
            CameraFrameDataBase baseEventData)
        {
            if (baseEventData.ImageDataPtr == 0 || baseEventData.ImageSize <= 0)
            {
                Debug.LogError("Cannot create CameraFrameData: Invalid pointer or size.");
                return null;
            }

            byte[] managedData = new byte[baseEventData.ImageSize];
            try
            {
                var ptr = new IntPtr(baseEventData.ImageDataPtr);
                Marshal.Copy(ptr, managedData, 0, baseEventData.ImageSize);

                if (CameraCaptureBridge.EnableVerboseLogging)
                {
                    Debug.Log($"Successfully copied {baseEventData.ImageSize} "
                            + $"bytes from native pointer " +
                        $"{baseEventData.ImageDataPtr}.");
                }

                return new CameraFrameData(
                    managedData,
                    baseEventData.Width,
                    baseEventData.Height,
                    baseEventData.Format,
                    baseEventData.Timestamp);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to copy data from native pointer "
                             + $"{baseEventData.ImageDataPtr}: " +
                    $"{e.Message}\n{e.StackTrace}");
                return null;
            }
        }
    }

    /// <summary>
    /// Base class for camera frame data events received from the native plugin.
    /// Contains raw pointer and metadata about the image.
    /// </summary>
    [Serializable]
    public class CameraFrameDataBase : BasePluginEvent
    {
        /// <summary>
        /// Pointer to the native image data buffer.
        /// </summary>
        public long ImageDataPtr;

        /// <summary>
        /// Size of the image data in bytes.
        /// </summary>
        public int ImageSize;

        /// <summary>
        /// Width of the image in pixels.
        /// </summary>
        public int Width;

        /// <summary>
        /// Height of the image in pixels.
        /// </summary>
        public int Height;

        /// <summary>
        /// Format of the image data (e.g., "JPEG").
        /// </summary>
        public string Format;
    }
}
