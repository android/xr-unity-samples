// <copyright file="TextureCreator.cs" company="Google LLC">
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

using UnityEngine;

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// Class holding a helper method for creating textures from a byte array.
    /// </summary>
    public class TextureCreator : MonoBehaviour
    {
        /// <summary>
        /// Creates a Texture2D from a byte array representing greyscale values and applies
        /// it to a material. The byte array should contain the raw pixel data, with 1 byte
        /// per pixel, arranged sequentially.
        /// </summary>
        /// <param name="byteData">The byte array containing the greyscale data..</param>
        /// <param name="width">The width of the texture in pixels.</param>
        /// <param name="height">The height of the texture in pixels.</param>
        /// <param name="targetMaterial">The material to apply the texture.</param>
        /// <returns>True on success.</returns>
        public static bool CreateAndApplyTextureFromBytes(
            byte[] byteData, int width, int height, Material targetMaterial)
        {
            if (targetMaterial == null)
            {
                Debug.LogError("[TextureCreator] Target material cannot be null.");
                return false;
            }

            if (byteData == null || byteData.Length == 0)
            {
                Debug.LogError("[TextureCreator] Input byte array is null or empty.");
                return false;
            }

            if (width <= 0 || height <= 0)
            {
                Debug.LogError("[TextureCreator] Width and height must be greater than zero.");
                return false;
            }

            // Validate array length
            int expectedDataLength = width * height;
            if (byteData.Length != expectedDataLength)
            {
                byte[] adjustedData = new byte[expectedDataLength];

                if (byteData.Length < expectedDataLength)
                {
                    Debug.LogWarning("[TextureCreator] Byte array length less than expected.");
                    for (int i = 0; i < expectedDataLength; i++)
                    {
                        adjustedData[i] = byteData[i % byteData.Length];
                    }
                }
                else
                {
                    Debug.LogWarning("[TextureCreator] Byte array length greater than expected.");
                    System.Array.Copy(byteData, 0, adjustedData, 0, expectedDataLength);
                }

                byteData = adjustedData;
            }

            byte[] tripledData = new byte[expectedDataLength * 3];
            for (int i = 0; i < expectedDataLength; ++i)
            {
                int baseIndex = i * 3;
                tripledData[baseIndex + 0] = byteData[i];
                tripledData[baseIndex + 1] = byteData[i];
                tripledData[baseIndex + 2] = byteData[i];
            }

            // TextureFormat.RGB24 uses 8 bits for each r, g, b channel.
            Texture2D generatedTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
            generatedTexture.filterMode = FilterMode.Bilinear;
            generatedTexture.wrapMode = TextureWrapMode.Repeat;
            generatedTexture.LoadRawTextureData(tripledData);
            generatedTexture.Apply();

            targetMaterial.mainTexture = generatedTexture;

            return true;
        }
    }
}
