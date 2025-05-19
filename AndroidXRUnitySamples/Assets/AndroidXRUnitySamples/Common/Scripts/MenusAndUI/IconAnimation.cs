// <copyright file="IconAnimation.cs" company="Google LLC">
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
using UnityEngine.UI;

namespace AndroidXRUnitySamples.MenusAndUI
{
    /// <summary>
    /// A script to play an animation on a sprite with multiple textures.
    /// </summary>
    public class IconAnimation : MonoBehaviour
    {
        [SerializeField] private Image _targetImage;
        [SerializeField] private Sprite[] _frames;
        [SerializeField] private float _intervalDuration;

        private float _timer;
        private int _frameIndex;

        void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0.0f)
            {
                _targetImage.sprite = _frames[_frameIndex];
                ++_frameIndex;
                _frameIndex %= _frames.Length;

                _timer %= _intervalDuration;
                _timer += _intervalDuration;
            }
        }
    }
}
