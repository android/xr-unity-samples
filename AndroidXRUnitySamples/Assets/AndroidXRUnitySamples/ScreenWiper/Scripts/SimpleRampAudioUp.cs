// <copyright file="SimpleRampAudioUp.cs" company="Google LLC">
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

using System.Collections;
using UnityEngine;

namespace AndroidXRUnitySamples.ScreenWiper
{
    /// <summary>
    /// A simple script for ramping audio volume up on instantiation.
    /// </summary>
    public class SimpleRampAudioUp : MonoBehaviour
    {
        [SerializeField] private AudioSource _audio;
        [SerializeField] private float _delay;
        [SerializeField] private float _speed;
        [SerializeField] private float _targetVolume;

        private void Start()
        {
            if (_speed <= 0.0f)
            {
                Debug.LogError("SimpleRampAudioUp._speed should be > 0.");
                _speed = 0.01f;
            }

            StartCoroutine(RampUp());
        }

        private IEnumerator RampUp()
        {
            _audio.volume = 0.0f;
            yield return new WaitForSeconds(_delay);

            float t = 0.0f;
            while (t < 1.0f)
            {
                t += Time.deltaTime * _speed;
                t = Mathf.Clamp01(t);
                _audio.volume = t * _targetVolume;
                yield return null;
            }
        }
    }
}
