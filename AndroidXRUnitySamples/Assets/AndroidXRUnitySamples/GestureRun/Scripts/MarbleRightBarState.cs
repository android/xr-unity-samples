// <copyright file="MarbleRightBarState.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.GestureRun
{
    /// <summary>
    /// Script to handle when the marble animation is in the "Right Bar" state.
    /// </summary>
    public class MarbleRightBarState : StateMachineBehaviour
    {
        MarbleController _marbleController;

        /// <summary>
        /// Handler for when the state is entered.
        /// </summary>
        /// <param name="animator">Instance of the animator.</param>
        /// <param name="stateInfo">Information about the state.</param>
        /// <param name="layerIndex">Layer index of the state.</param>
        public override void OnStateEnter(
            Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _marbleController = animator.GetComponentInParent<MarbleController>();
            _marbleController.SetMarbleSide(MarbleSide.Right);
        }

        /// <summary>
        /// Handler for when the state is exited.
        /// </summary>
        /// <param name="animator">Instance of the animator.</param>
        /// <param name="stateInfo">Information about the state.</param>
        /// <param name="layerIndex">Layer index of the state.</param>
        public override void OnStateExit(
            Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _marbleController.SetMarbleSide(MarbleSide.None);
        }
    }
}
