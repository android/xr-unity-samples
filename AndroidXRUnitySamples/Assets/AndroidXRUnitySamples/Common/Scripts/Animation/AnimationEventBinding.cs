// <copyright file="AnimationEventBinding.cs" company="Google LLC">
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
using UnityEngine.Events;

namespace AndroidXRUnitySamples.Animation
{
    /// <summary>
    /// Binds Unity animation events to specific callbacks, managed by an
    /// <see cref="AnimationEventDispatcher" />.
    /// </summary>
    public class AnimationEventBinding : MonoBehaviour
    {
        /// <summary>
        /// The animation event name under which this binding will be registered. Animation
        /// clips using <see cref="AnimationEventDispatcher" /> to send events should send a
        /// string parameter in the event, containing the event name for which corresponding
        /// bindings will be invoked.
        /// </summary>
        [Tooltip(
                "The animation event name under which this binding will be registered.\n" +
                "Animation clips using an AnimationEventDispatcher to send events " +
                "should send a string parameter\n" +
                "in the event with the event name for which "
              + "corresponding bindings will be invoked.")]
        public string AnimationEventName;

        /// <summary>
        /// The UnityEvent that will be triggered in response to the animation event.
        /// </summary>
        [Tooltip("The UnityEvent that will be triggered in response to the animation event.")]
        public UnityEvent EventCallback;

        /// <summary>
        /// The dispatcher this object binds to.
        /// </summary>
        [SerializeField]
        protected AnimationEventDispatcher _animationEventDispatcher;

        private void Awake()
        {
            if (_animationEventDispatcher == null)
            {
                Debug.LogError(
                        $"AnimationEventBinding on gameObject {gameObject.name} "
                      + "needs a reference to an"
                      + "AnimationEventDispatcher component in the scene, but none was set.",
                        this);
                return;
            }

            _animationEventDispatcher.AddAnimationEventBinding(this);
        }
    }
}
