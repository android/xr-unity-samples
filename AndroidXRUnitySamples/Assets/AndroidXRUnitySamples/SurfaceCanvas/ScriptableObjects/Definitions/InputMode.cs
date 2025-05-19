// <copyright file="InputMode.cs" company="Google LLC">
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

using AndroidXRUnitySamples.Variables;
using UnityEngine;

namespace AndroidXRUnitySamples.SurfaceCanvas
{
    /// <summary>
    /// <inheritdoc cref="Variable{T}" />
    /// <para />
    /// This is the <see cref="Variable{T}" /> extension for InputMode in the SurfaceCanvas scene.
    /// </summary>
    [CreateAssetMenu(menuName = "AndroidXRUnitySamples/SurfaceCanvas/InputMode")]
    public class InputMode : Variable<InputMode.XruiInputMode>
    {
        /// <summary>
        /// An enum type describing the possible input modes.
        /// </summary>
        public enum XruiInputMode
        {
            /// <summary>
            /// Hands tracking input mode.
            /// </summary>
            HandsOnly,

            /// <summary>
            /// Eye tracking and Hands tracking input mode.
            /// </summary>
            EyesAndHands,

            /// <summary>
            /// Eye tracking input mode.
            /// </summary>
            EyesOnly
        }
    }
}
