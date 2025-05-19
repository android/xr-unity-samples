// <copyright file="Timer.cs" company="Google LLC">
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
using UnityEngine.Events;

namespace AndroidXRUnitySamples.SurfaceCanvas
{
    /// <summary>
    /// A simple Timer component that can countdown in different timer modes and fire events on
    /// timer start, stop, completion, and progress.
    /// </summary>
    public class Timer : MonoBehaviour
    {
        [SerializeField]
        private float _timerDuration;

        [SerializeField]
        private TimerMode _mode;

        [SerializeField]
        private bool _countDown;

        [SerializeField]
        private UnityEvent _onTimerStarted;

        [SerializeField]
        private UnityEvent<float> _onTimerProgress;

        [SerializeField]
        private UnityEvent<float> _onTimerProgressPercent;

        [SerializeField]
        private UnityEvent _onTimerStopped;

        [SerializeField]
        private UnityEvent _onTimerComplete;

        private bool _timerIsActive;

        private enum TimerMode
        {
            DeltaTime,
            UnscaledDeltaTime,
            FixedDeltaTime,
            UnscaledFixedDeltaTime,
            FrameCount
        }

        /// <summary>
        /// Gets the timer's elapsed time.
        /// </summary>
        public float ElapsedTimeSeconds
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the timer's elapsed time percentage.
        /// </summary>
        public float ElapsedTimePercent => ElapsedTimeSeconds / _timerDuration;

        private bool _timerElapsedFull =>
                _countDown ? ElapsedTimeSeconds <= 0 : ElapsedTimeSeconds >= _timerDuration;

        /// <summary>
        /// Starts this timer.
        /// </summary>
        public void StartTimer()
        {
            TimerStart();
            _onTimerStarted?.Invoke();
        }

        /// <summary>
        /// Pauses this timer.
        /// </summary>
        public void Pause()
        {
            TimerPause();
        }

        /// <summary>
        /// Stops this timer.
        /// </summary>
        public void Stop()
        {
            TimerStop();
            _onTimerStopped?.Invoke();
        }

        private void ResetElapsedTime()
        {
            ElapsedTimeSeconds = _countDown ? _timerDuration : 0;
        }

        private void TimerStart()
        {
            ResetElapsedTime();
            _timerIsActive = true;
        }

        private void TimerPause()
        {
            _timerIsActive = false;
        }

        private void TimerStop()
        {
            ResetElapsedTime();
            _timerIsActive = false;
        }

        private void TimerProgress()
        {
            float dT = 0;
            switch (_mode)
            {
                case TimerMode.DeltaTime:
                    dT += Time.deltaTime;
                    break;
                case TimerMode.UnscaledDeltaTime:
                    dT += Time.unscaledDeltaTime;
                    break;
                case TimerMode.FixedDeltaTime:
                    dT += Time.fixedDeltaTime;
                    break;
                case TimerMode.UnscaledFixedDeltaTime:
                    dT += Time.fixedDeltaTime;
                    break;
                case TimerMode.FrameCount:
                    dT += 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ElapsedTimeSeconds += _countDown ? -dT : dT;

            _onTimerProgress?.Invoke(ElapsedTimeSeconds);
            _onTimerProgressPercent?.Invoke(ElapsedTimePercent);
        }

        private void TimerComplete()
        {
            TimerStop();
            _onTimerComplete?.Invoke();
        }

        private void Update()
        {
            if (_timerIsActive)
            {
                TimerProgress();
                if (_timerElapsedFull)
                {
                    TimerComplete();
                }
            }
        }
    }
}
