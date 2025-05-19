// <copyright file="EyeCreatureAnimationAudio.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.CreatureGaze
{
    /// <summary>
    /// Script for receiving callbacks from the animation to trigger sound effects.
    /// </summary>
    public class EyeCreatureAnimationAudio : MonoBehaviour
    {
        [SerializeField] private AudioClipData _wingFlapAudio;

        private bool _enabled;

        /// <summary>
        /// Used to disable the wing flap from playing.
        /// </summary>
        /// <param name="enable">Boolean for flagging as enabled.</param>
        public void SetEnabled(bool enable)
        {
            _enabled = enable;
        }

        /// <summary>
        /// Used to adjust the pitch of the wing flap.
        /// </summary>
        /// <param name="newPitch">New value for pitch.</param>
        public void SetPitch(float newPitch)
        {
            _wingFlapAudio.PitchRange = new Vector2(newPitch, newPitch);
        }

        /// <summary>
        /// Called from the animation to trigger a wing flap sound.
        /// </summary>
        public void TriggerWingFlapSound()
        {
            if (_enabled)
            {
                Singleton.Instance.Audio.PlayOneShot(_wingFlapAudio, transform.position);
            }
        }
    }
}
