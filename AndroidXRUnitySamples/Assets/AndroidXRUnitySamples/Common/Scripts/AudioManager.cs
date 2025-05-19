// <copyright file="AudioManager.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Manages sfx playback.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private int _numOneShots;
        [SerializeField] private GameObject _oneShotPrefab;

        [SerializeField] private AudioClipData _menuSummon;
        [SerializeField] private AudioClipData _menuDismiss;
        [SerializeField] private AudioClipData _buttonHover;
        [SerializeField] private AudioClipData _buttonPress;
        [SerializeField] private AudioClipData _togglePress;
        [SerializeField] private AudioClipData _sliderValueChange;

        private AudioSource[] _oneShots;
        private int _oneShotIndex;

        /// <summary>Plays the menu summon sound at the specified location.</summary>
        /// <param name="pos">Where to play the sound.</param>
        public void PlayMenuSummon(Vector3 pos)
        {
            PlayOneShot(_menuSummon, pos);
        }

        /// <summary>Plays the menu summon dismiss at the specified location.</summary>
        /// <param name="pos">Where to play the sound.</param>
        public void PlayMenuDismiss(Vector3 pos)
        {
            PlayOneShot(_menuDismiss, pos);
        }

        /// <summary>Plays the button hover sound at the specified location.</summary>
        /// <param name="pos">Where to play the sound.</param>
        public void PlayButtonHover(Vector3 pos)
        {
            PlayOneShot(_buttonHover, pos);
        }

        /// <summary>Plays the button press sound at the specified location.</summary>
        /// <param name="pos">Where to play the sound.</param>
        public void PlayButtonPress(Vector3 pos)
        {
            PlayOneShot(_buttonPress, pos);
        }

        /// <summary>Plays the toggle press sound at the specified location.</summary>
        /// <param name="pos">Where to play the sound.</param>
        public void PlayTogglePress(Vector3 pos)
        {
            PlayOneShot(_togglePress, pos);
        }

        /// <summary>Plays the slider value change sound at the specified location.</summary>
        /// <param name="pos">Where to play the sound.</param>
        public void PlaySliderValueChange(Vector3 pos)
        {
            PlayOneShot(_sliderValueChange, pos);
        }

        /// <summary>Plays a sfx defined by AudioClipData at the specified location.</summary>
        /// <param name="data">The sfx clip data.</param>
        /// <param name="pos">Where to play the sound.</param>
        public void PlayOneShot(AudioClipData data, Vector3 pos)
        {
            if (Time.realtimeSinceStartup - data.LastPlayTime >= data.MinRetrigger)
            {
                data.LastPlayTime = Time.realtimeSinceStartup;

                _oneShots[_oneShotIndex].transform.position = pos;
                _oneShots[_oneShotIndex].clip =
                    data.Clips[UnityEngine.Random.Range(0, data.Clips.Length)];
                _oneShots[_oneShotIndex].pitch =
                    UnityEngine.Random.Range(data.PitchRange.x, data.PitchRange.y);
                _oneShots[_oneShotIndex].volume =
                    UnityEngine.Random.Range(data.VolumeRange.x, data.VolumeRange.y);
                _oneShots[_oneShotIndex].Play();

                ++_oneShotIndex;
                _oneShotIndex %= _numOneShots;
            }
        }

        void Awake()
        {
            _oneShotIndex = 0;
            _oneShots = new AudioSource[_numOneShots];
            for (int i = 0; i < _numOneShots; ++i)
            {
                // Parent to the AudioManager for organizational purposes.
                GameObject go = Instantiate(_oneShotPrefab, transform);
                _oneShots[i] = go.GetComponent<AudioSource>();
            }
        }
    }

    /// <summary>
    /// Data associated with an audio clip.
    /// </summary>
    [Serializable]
    public class AudioClipData
    {
        /// <summary>Audio data.</summary>
        public AudioClip[] Clips;

        /// <summary>When played, pick a random pitch in this range.</summary>
        public Vector2 PitchRange = new Vector2(1.0f, 1.0f);

        /// <summary>When played, pick a random volume in this range.</summary>
        public Vector2 VolumeRange = new Vector2(1.0f, 1.0f);

        /// <summary>Don't allow successive playback without this pause.</summary>
        public float MinRetrigger = 0.0f;

        /// <summary>For computing min retrigger.</summary>
        [HideInInspector] public float LastPlayTime;
    }
}
