// <copyright file="SceneContextStateBehaviour.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.StateMachine
{
    /// <summary>
    /// Base class for state machine behaviours that automatically initializes a reference to a
    /// context object.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the context component. Must derive from
    /// <see
    ///     cref="StateMachineSceneContext" />
    /// .
    /// </typeparam>
    public abstract class SceneContextStateBehaviour<T> : StateMachineBehaviour
            where T : StateMachineSceneContext
    {
        //// Disabling warning here since it's a readonly and number of
        //// derived types won't scale, not a huge problem.
        //// ReSharper disable once StaticMemberInGenericType

        /// <summary>
        /// Hash for accessing the 'advance state' animator trigger.
        /// </summary>
        public static readonly int AdvanceStateTriggerHash =
                Animator.StringToHash("advance state");

        /// <summary>
        /// The context object.
        /// </summary>
        protected T _context;

        /// <summary>
        /// May be used internally to indicate when the state starts transitioning,
        /// e.g., to ensure a certain part of the code is only executed once, etc..
        /// </summary>
        protected bool _stateTransitioning = false;

        /// <inheritdoc />
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo,
                                          int layerIndex)
        {
            if (_context == null)
            {
                if (!animator.TryGetComponent(out _context))
                {
                    Debug.LogError(
                            $"Animator State Machine {animator.name} needs to " +
                            $"have a Context component of type {typeof(T).Name} attached.",
                            animator.gameObject);
                }
            }
        }
    }
}
