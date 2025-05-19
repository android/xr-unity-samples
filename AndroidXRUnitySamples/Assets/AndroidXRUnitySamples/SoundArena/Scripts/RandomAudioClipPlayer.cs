// <copyright file="RandomAudioClipPlayer.cs" company="Google LLC">
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

using System.Threading;
using UnityEngine;

namespace AndroidXRUnitySamples.SoundArena.Speakers
{
    /// <summary>
    /// Plays random audio clips from an array with customizable playback settings.
    /// </summary>
    public class RandomAudioClipPlayer : MonoBehaviour
    {
        /// <summary>
        /// The AudioSource component used for audio playback.
        /// </summary>
        [Tooltip("The AudioSource component used for audio playback.")]
        public AudioSource AudioSource;

        /// <summary>
        /// Array of audio clips to choose from for playback.
        /// </summary>
        [Tooltip("Array of audio clips to choose from for playback.")]
        public AudioClip[] AudioClips;

        /// <summary>
        /// Allows multiple audio clips to play simultaneously.
        /// </summary>
        [Tooltip("Allows multiple audio clips to play simultaneously.")]
        public bool AllowOverlap = true;

        /// <summary>
        /// Maximum number of audio clips that can be played at the same time when overlap is
        /// enabled.
        /// </summary>
        [Tooltip(
                "Maximum number of audio clips that can be played at the "
              + "same time when overlap is enabled.")]
        public int MaxSimultaneousVoices = 4;

        /// <summary>
        /// Allows the same audio clip to be played consecutively.
        /// </summary>
        [Tooltip("Allows the same audio clip to be played consecutively.")]
        public bool AllowRepeatLastPlayed = true;

        /// <summary>
        /// Minimum delay in milliseconds before playing an audio clip.
        /// </summary>
        [Tooltip("Minimum delay in milliseconds before playing an audio clip.")]
        public int MinDelayMillis;

        /// <summary>
        /// Maximum delay in milliseconds before playing an audio clip.
        /// </summary>
        [Tooltip("Maximum delay in milliseconds before playing an audio clip.")]
        public int MaxDelayMillis;

        /// <summary>
        /// Volume scale of the played audio clip when playing with overlap enabled.
        /// </summary>
        [Range(0f, 1f)]
        [Tooltip("Volume scale of the played audio clip when playing with overlap enabled.")]
        public float VolumeScale = 1f;

        private const int _maxIterations = 10000;

        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        private int _lastIndex = -1;
        private int _voiceCounter;

        private AudioClip _randomAudioClip => AudioClips[Random.Range(0, AudioClips.Length)];

        private AudioClip _randomAudioClipNoRepeat
        {
            get
            {
                int nextIndex;
                int i = 0;
                while ((nextIndex = Random.Range(0, AudioClips.Length)) == _lastIndex)
                {
                    if (i++ >= _maxIterations)
                    {
                        break;
                    }
                }

                _lastIndex = nextIndex;
                return AudioClips[nextIndex];
            }
        }

        /// <summary>
        /// Initiates the playback of a random audio clip.
        /// </summary>
        public void TriggerPlayback()
        {
            PlayRandomClip(MinDelayMillis, MaxDelayMillis, AllowOverlap, AllowRepeatLastPlayed,
                    VolumeScale);
        }

        private async void PlayRandomClip(int minDelayMs = 0, int maxDelayMs = 0,
                                          bool overlap = true, bool canRepeat = true,
                                          float volScale = 1f)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            if (_voiceCounter >= MaxSimultaneousVoices)
            {
                return;
            }

            _voiceCounter++;

            int playDelayMillis = Random.Range(minDelayMs, maxDelayMs);

            if (playDelayMillis > float.Epsilon)
            {
                await Awaitable.WaitForSecondsAsync(playDelayMillis / 1000f, _cancellation.Token);
                if (_cancellation.IsCancellationRequested)
                {
                    return;
                }
            }

            AudioClip clip;
            if (canRepeat)
            {
                clip = _randomAudioClip;
            }
            else
            {
                clip = _randomAudioClipNoRepeat;
            }

            AudioSource.enabled = true;

            if (overlap)
            {
                AudioSource.PlayOneShot(clip, volScale);
            }
            else
            {
                AudioSource.clip = clip;
                AudioSource.Play();
            }

            await Awaitable.WaitForSecondsAsync(clip.length, _cancellation.Token);
            _voiceCounter--;
        }

        private void OnDestroy()
        {
            _cancellation.Cancel();
        }
    }
}
