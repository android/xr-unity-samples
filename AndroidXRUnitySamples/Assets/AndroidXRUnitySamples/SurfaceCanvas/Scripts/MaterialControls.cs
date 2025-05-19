// <copyright file="MaterialControls.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.SurfaceCanvas
{
    /// <summary>
    /// Exposes some properties of an assigned Material for controlling via UI events and scripts.
    /// </summary>
    public class MaterialControls : MonoBehaviour
    {
        [SerializeField]
        private Material _material;

        [SerializeField]
        private Vector2 _clampAlphaClip = new Vector2(0, 1);

        [SerializeField]
        private Vector2 _clampAlpha = new Vector2(0, 1);

        [SerializeField]
        private Vector2 _clampHue = new Vector2(0, 1);

        [SerializeField]
        private Vector2 _clampSaturation = new Vector2(0, 1);

        [SerializeField]
        private Vector2 _clampValue = new Vector2(0, 1);

        private Color _startingColor;
        private float _startingAlphaClip;

        /// <summary>
        /// Controls the material's color hue.
        /// </summary>
        /// <param name="hue">The hue value from 0 to 1.</param>
        public void Hue(float hue)
        {
            float newHue = Mathf.Clamp(hue, _clampHue.x, _clampHue.y);
            Color.RGBToHSV(_material.color, out float h, out float s, out float v);
            Color newColor = Color.HSVToRGB(newHue, s, v);
            newColor.a = _material.color.a;
            _material.color = newColor;
        }

        /// <summary>
        /// Controls the material's color saturation.
        /// </summary>
        /// <param name="saturation">The saturation value from 0 to 1.</param>
        public void Saturation(float saturation)
        {
            float newSat = Mathf.Clamp(saturation, _clampSaturation.x, _clampSaturation.y);
            Color.RGBToHSV(_material.color, out float h, out float s, out float v);
            Color newColor = Color.HSVToRGB(h, newSat, v);
            newColor.a = _material.color.a;
            _material.color = newColor;
        }

        /// <summary>
        /// Controls the material's color value.
        /// </summary>
        /// <param name="value">The value value from 0 to 1.</param>
        public void Value(float value)
        {
            float newVal = Mathf.Clamp(value, _clampValue.x, _clampValue.y);
            Color.RGBToHSV(_material.color, out float h, out float s, out float v);
            Color newColor = Color.HSVToRGB(h, s, newVal);
            newColor.a = _material.color.a;
            _material.color = newColor;
        }

        /// <summary>
        /// Controls the material's alpha value.
        /// </summary>
        /// <param name="value">The alpha value from 0 to 1.</param>
        public void Alpha(float value)
        {
            float newA = Mathf.Clamp(value, _clampAlpha.x, _clampAlpha.y);
            Color newColor = _material.color;
            newColor.a = newA;
            _material.color = newColor;
        }

        /// <summary>
        /// Controls the material's alpha clip value.
        /// </summary>
        /// <param name="value">The alpha clip value from 0 to 1.</param>
        public void AlphaClip(float value)
        {
            float newAc = Mathf.Clamp(value, _clampAlphaClip.x, _clampAlphaClip.y);
            if (_material.HasFloat("_Alpha_Clip_Threshold"))
            {
                _material.SetFloat("_Alpha_Clip_Threshold", newAc);
            }
        }

        /// <summary>
        /// Resets all parameters in the referenced Material to their starting values.
        /// </summary>
        public void Reset()
        {
            _material.color = _startingColor;
            AlphaClip(_startingAlphaClip);
        }

        /// <summary>
        /// Toggles a preset transparency level on and off in the material's color.
        /// </summary>
        /// <param name="isOn">Whether to turn it on or off.</param>
        public void ToggleTransparency(bool isOn)
        {
            Color c = _material.color;
            float a = (1 - Convert.ToInt32(!isOn) * .5f) * _startingColor.a;
            _material.color = new Color(c.r, c.g, c.b, a);
        }

        private void Start()
        {
            _startingColor = _material.color;
            if (_material.HasFloat("_Alpha_Clip_Threshold"))
            {
                _startingAlphaClip = _material.GetFloat("_Alpha_Clip_Threshold");
            }
        }

        private void OnDestroy()
        {
            Reset();
        }
    }
}
