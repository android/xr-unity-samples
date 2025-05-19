// <copyright file="MarbleController.cs" company="Google LLC">
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

using System.Drawing;
using UnityEngine;
using UnityEngine.XR.Hands;

namespace AndroidXRUnitySamples.GestureRun
{
    /// <summary>
    /// Class used to control the marble.
    /// </summary>
    public class MarbleController : MonoBehaviour
    {
        /// <summary>
        /// Animator component of the marble.
        /// </summary>
        public Animator Animator;

        /// <summary>
        /// GestureDetector component of the scene.
        /// </summary>
        public GestureDetector GestureDetector;

        /// <summary>
        /// Transform used for positiong the particle effects.
        /// </summary>
        public Transform ParticleParent;

        /// <summary>
        /// Particle system used for dust effects.
        /// </summary>
        public ParticleSystem DustParticles;

        /// <summary>
        /// Particle system used for burst effects.
        /// </summary>
        public ParticleSystem BurstParticles;

        /// <summary>
        /// Trail renderer used for effects.
        /// </summary>
        public GameObject Trail;

        /// <summary>
        /// Animator component of the help icon.
        /// </summary>
        public Animator HelpIconAnimator;

        /// <summary>
        /// Flag specifying the marble is ready for new input action.
        /// </summary>
        public bool IsReady = false;

        /// <summary>
        /// Current side the marble is on.
        /// </summary>
        public MarbleSide Side = MarbleSide.None;

        /// <summary>
        /// Sound effect to play when jumping.
        /// </summary>
        public AudioClipData JumpClip;

        /// <summary>
        /// Sound effect to play when switching sides.
        /// </summary>
        public AudioClipData SwitchClip;

        const string _jumpTrigger = "Jump";
        const string _switchTrigger = "Switch";

        /// <summary>
        /// Set the marble side parameter.
        /// </summary>
        /// <param name="side">The side the marble is to be set on.</param>
        public void SetMarbleSide(MarbleSide side)
        {
            Side = side;
        }

        void Update()
        {
            if (!IsReady || Side == MarbleSide.None)
            {
                return;
            }

            Handedness handedness = Side == MarbleSide.Left ? Handedness.Left : Handedness.Right;
            if (GestureDetector.HasGesture(handedness, Gesture.IndexPinch))
            {
                Switch();
            }
            else if (GestureDetector.HasGesture(handedness, Gesture.OpenPalm))
            {
                Jump();
            }
        }

        void Jump()
        {
            if (!IsReady)
            {
                return;
            }

            Singleton.Instance.Audio.PlayOneShot(JumpClip, transform.position);
            Animator.SetTrigger(_jumpTrigger);
        }

        void Switch()
        {
            if (!IsReady)
            {
                return;
            }

            Singleton.Instance.Audio.PlayOneShot(SwitchClip, transform.position);
            Animator.SetTrigger(_switchTrigger);
            Animator.SetTrigger(_jumpTrigger);
            HelpIconAnimator.SetTrigger(_switchTrigger);
        }
    }
}
