// <copyright file="Home.cs" company="Google LLC">
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
using UnityEngine.InputSystem;

namespace AndroidXRUnitySamples.Home
{
    /// <summary>
    /// State machine for Home.
    /// </summary>
    public class Home : MonoBehaviour
    {
        [SerializeField] private Session _session;
        [SerializeField] private GameObject _menuTutorialPrefab;
        [SerializeField] private float _menuTutorialSpawnDelay;
        [SerializeField] private InputActionProperty _menuSummonInputAction;
        [SerializeField] private Animator _statusDashboard;

        private void Start()
        {
            StartCoroutine(SpawnMenuTutorial());
            Singleton.Instance.OriginManager.EnablePassthrough = true;
            Singleton.Instance.Menu.StartTransitionToExperience += OnStartTransitionToExperience;
        }

        private void OnDestroy()
        {
            Singleton.Instance.Menu.StartTransitionToExperience -= OnStartTransitionToExperience;
        }

        private void Update()
        {
            _statusDashboard.SetBool("Hidden", Singleton.Instance.Menu.Active);
        }

        private void OnStartTransitionToExperience()
        {
            StartCoroutine(RampAudioDownRoutine());
        }

        private IEnumerator RampAudioDownRoutine()
        {
            AudioSource audio = GetComponent<AudioSource>();
            if (audio == null)
            {
                yield break;
            }

            float baseVolume = audio.volume;
            float rampDuration = 0.25f;
            float rampTimer = rampDuration;
            while (rampTimer > 0.0f)
            {
                rampTimer -= Time.deltaTime;
                audio.volume = baseVolume * (rampTimer / rampDuration);
                yield return null;
            }
        }

        private IEnumerator SpawnMenuTutorial()
        {
            // Don't use WaitForSeconds() because we need to monitor for input.
            float delay = _menuTutorialSpawnDelay;
            while (delay > 0.0f)
            {
                if (_menuSummonInputAction.action.WasPressedThisFrame())
                {
                    // The user knows what they're doing. Don't bother showing the tutorial.
                    yield break;
                }

                delay -= Time.deltaTime;
                yield return null;
            }

            Instantiate(_menuTutorialPrefab);
        }
    }
}
