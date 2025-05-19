// <copyright file="AnimationEventDispatcher.cs" company="Google LLC">
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

using System.Collections.Generic;
using UnityEngine;

namespace AndroidXRUnitySamples.Animation
{
    /// <summary>
    /// Dispatches animation events to registered bindings.
    /// Because Animation Events in Unity are implemented via calling SendMessage() on the
    /// object that contains the Animator component, it is a good practice performance-wise to
    /// keep the component count of gameObjects with animators that respond to animation events
    /// to a minimum. This class allows us to centralize all handling of animation events, which
    /// can then be dispatched to handlers attached to different gameObjects. It also improves
    /// the maintainability of animation clips that use animation events, since its use allows
    /// for every animation clip to map to the same method call, making refactoring less prone
    /// to silently break things.
    /// </summary>
    public class AnimationEventDispatcher : MonoBehaviour
    {
        private readonly Dictionary<string, List<AnimationEventBinding>> _dispatchLookup =
                new Dictionary<string, List<AnimationEventBinding>>();

        /// <summary>
        /// Dispatches an animation event to all registered bindings for the specified event
        /// name. This is the method that will be set in the animation clip as the 'Function'
        /// string. The animation clip should also send a string containing the animation event
        /// name under which its handlers will be registered.
        /// </summary>
        /// <param name="animationEventName">The name of the animation event to dispatch.</param>
        public void DispatchAnimationEvent(string animationEventName)
        {
            if (!_dispatchLookup.TryGetValue(animationEventName,
                        out List<AnimationEventBinding> handlers))
            {
                return;
            }

            for (int i = handlers.Count - 1; i >= 0; i--)
            {
                handlers[i].EventCallback.Invoke();
            }
        }

        /// <summary>
        /// Registers an <see cref="AnimationEventBinding" /> for a specific animation event.
        /// </summary>
        /// <param name="binding">The binding to register.</param>
        public void AddAnimationEventBinding(AnimationEventBinding binding)
        {
            if (!_dispatchLookup.TryGetValue(binding.AnimationEventName,
                        out List<AnimationEventBinding> handlers))
            {
                handlers = new List<AnimationEventBinding>();
                _dispatchLookup.Add(binding.AnimationEventName, handlers);
            }

            handlers.Add(binding);
        }
    }
}
