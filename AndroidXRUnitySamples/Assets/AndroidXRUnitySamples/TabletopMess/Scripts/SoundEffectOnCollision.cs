// <copyright file="SoundEffectOnCollision.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.TabletopMess
{
    /// <summary>
    /// Triggers sound effects when collision happen with velocities above a threshold.
    /// </summary>
    public class SoundEffectOnCollision : MonoBehaviour
    {
        [SerializeField] private Vector2 _soundVelocityRange;
        [SerializeField] private AudioClipData _collisionClip;

        private Vector2 _baseVolumeRange;

        private void Start()
        {
            _baseVolumeRange = _collisionClip.VolumeRange;
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Play a sound if the colliding objects had a big impact.
            float mag = collision.relativeVelocity.magnitude;
            if (mag > _soundVelocityRange.x)
            {
                // Scale the volume of the sound according to impact strength.
                float t = Mathf.InverseLerp(_soundVelocityRange.x, _soundVelocityRange.y, mag);
                _collisionClip.VolumeRange = _baseVolumeRange * t;
                Singleton.Instance.Audio.PlayOneShot(_collisionClip, transform.position);
            }
        }
    }
}
