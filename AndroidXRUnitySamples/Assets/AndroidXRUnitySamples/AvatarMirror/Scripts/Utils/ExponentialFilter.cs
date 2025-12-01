// <copyright file="ExponentialFilter.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// Implements an exponential filter following the rule:
    /// `x[t] = alpha * val[t] + (1 - alpha) * x[t-1]`
    /// where `alpha` is the filter weight.
    /// </summary>
    public class ExponentialFilter
    {
        private readonly float _weight;
        private readonly bool _initializeOnFirstUpdate;
        private bool _initialized;

        private float _currentValueFloat;
        private Vector3 _currentValueV3;
        private Quaternion _currentValueQuat;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialFilter"/> for float values.
        /// </summary>
        /// <param name="settings">The filter settings for float values.</param>
        public ExponentialFilter(ExponentialFilterSettingsFloat settings)
        {
            _weight = settings.Weight;
            _initializeOnFirstUpdate = settings.InitializeOnFirstUpdate;
            _currentValueFloat = settings.InitialValue;
            _initialized = !_initializeOnFirstUpdate;  // If not initialized on first update, it's
                                                       // considered initialized with InitialValue
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialFilter"/> for <see
        /// cref="Vector3"/> values.
        /// </summary>
        /// <param name="settings">The filter settings for Vector3 values.</param>
        public ExponentialFilter(ExponentialFilterSettingsV3 settings)
        {
            _weight = settings.Weight;
            _initializeOnFirstUpdate = settings.InitializeOnFirstUpdate;
            _currentValueV3 = settings.InitialValue;
            _initialized = !_initializeOnFirstUpdate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialFilter"/> for <see
        /// cref="Quaternion"/> values.
        /// </summary>
        /// <param name="settings">The filter settings for Quaternion values.</param>
        public ExponentialFilter(ExponentialFilterSettingsQuat settings)
        {
            _weight = settings.Weight;
            _initializeOnFirstUpdate = settings.InitializeOnFirstUpdate;
            _currentValueQuat = settings.InitialValue;
            _initialized = !_initializeOnFirstUpdate;
        }

        /// <summary>
        /// Resets the filter's current value to a specified float value.
        /// </summary>
        /// <param name="value">The value to reset to.</param>
        public void ResetValue(float value)
        {
            _currentValueFloat = value;
            _initialized = true;  // Mark as initialized after reset
        }

        /// <summary>
        /// Resets the filter's current value to a specified <see cref="Vector3"/> value.
        /// </summary>
        /// <param name="value">The value to reset to.</param>
        public void ResetValue(Vector3 value)
        {
            _currentValueV3 = value;
            _initialized = true;
        }

        /// <summary>
        /// Resets the filter's current value to a specified <see cref="Quaternion"/> value.
        /// </summary>
        /// <param name="value">The value to reset to.</param>
        public void ResetValue(Quaternion value)
        {
            _currentValueQuat = value;
            _initialized = true;
        }

        /// <summary>
        /// Applies the exponential filter to a float input value.
        /// </summary>
        /// <param name="input">The new input value.</param>
        /// <returns>The filtered float value.</returns>
        public float Filter(float input)
        {
            if (!_initialized)
            {
                _currentValueFloat = input;
                _initialized = true;
            }
            else
            {
                _currentValueFloat = (_weight * input) + ((1f - _weight) * _currentValueFloat);
            }

            return _currentValueFloat;
        }

        /// <summary>
        /// Applies the exponential filter to a <see cref="Vector3"/> input value.
        /// </summary>
        /// <param name="input">The new input value.</param>
        /// <returns>The filtered <see cref="Vector3"/> value.</returns>
        public Vector3 Filter(Vector3 input)
        {
            if (!_initialized)
            {
                _currentValueV3 = input;
                _initialized = true;
            }
            else
            {
                _currentValueV3 = Vector3.Lerp(_currentValueV3, input, _weight);
            }

            return _currentValueV3;
        }

        /// <summary>
        /// Applies the exponential filter to a <see cref="Quaternion"/> input value using spherical
        /// linear interpolation (Slerp).
        /// </summary>
        /// <param name="input">The new input value.</param>
        /// <returns>The filtered <see cref="Quaternion"/> value.</returns>
        public Quaternion Filter(Quaternion input)
        {
            if (!_initialized)
            {
                _currentValueQuat = input;
                _initialized = true;
            }
            else
            {
                _currentValueQuat = Quaternion.Slerp(_currentValueQuat, input, _weight);
            }

            return _currentValueQuat;
        }
    }

    /// <summary>
    /// Base settings for an exponential filter.
    /// </summary>
    [System.Serializable]
    public abstract class ExponentialFilterSettingsBase
    {
        /// <summary>The weight (alpha) of the filter. A higher weight means less smoothing
        /// (closer to input).</summary>
        [Tooltip("The weight (alpha) of the filter. A higher weight means less smoothing (closer " +
                 "to input).")]
        [Range(0.01f, 0.99f)]  // Restrict weight to a reasonable range for filtering
        public float Weight;

        /// <summary>If true, the filter's initial value will be set to the first input value
        /// received.</summary>
        [Tooltip(
            "If true, the filter's initial value will be set to the first input value received.")]
        public bool InitializeOnFirstUpdate;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialFilterSettingsBase"/> class.
        /// </summary>
        /// <param name="weight">The weight of the filter (0.01-0.99).</param>
        /// <param name="initializeOnFirstUpdate">Whether to initialize the filter on the first
        /// update.</param>
        protected ExponentialFilterSettingsBase(float weight, bool initializeOnFirstUpdate)
        {
            Weight = weight;
            InitializeOnFirstUpdate = initializeOnFirstUpdate;
        }
    }

    /// <summary>
    /// Settings for an exponential filter that operates on a float.
    /// </summary>
    [System.Serializable]
    public class ExponentialFilterSettingsFloat : ExponentialFilterSettingsBase
    {
        /// <summary>The initial value of the filter if InitializeOnFirstUpdate is false.</summary>
        [Tooltip("The initial value of the filter if InitializeOnFirstUpdate is false.")]
        public float InitialValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialFilterSettingsFloat"/> class.
        /// </summary>
        /// <param name="weight">The weight of the filter (0.01-0.99).</param>
        /// <param name="initialValue">The initial value of the filter.</param>
        /// <param name="initializeOnFirstUpdate">Whether to initialize the filter on the first
        /// update.</param>
        public ExponentialFilterSettingsFloat(float weight = 0.9f, float initialValue = 0f,
                                              bool initializeOnFirstUpdate = true)
            : base(weight, initializeOnFirstUpdate)
        {
            InitialValue = initialValue;
        }
    }

    /// <summary>
    /// Settings for an exponential filter that operates on a <see cref="Vector3"/>.
    /// </summary>
    [System.Serializable]
    public class ExponentialFilterSettingsV3 : ExponentialFilterSettingsBase
    {
        /// <summary>The initial value of the filter if InitializeOnFirstUpdate is false.</summary>
        [Tooltip("The initial value of the filter if InitializeOnFirstUpdate is false.")]
        public Vector3 InitialValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialFilterSettingsV3"/> class.
        /// </summary>
        /// <param name="weight">The weight of the filter (0.01-0.99).</param>
        /// <param name="initialValue">The initial value of the filter.</param>
        /// <param name="initializeOnFirstUpdate">Whether to initialize the filter on the first
        /// update.</param>
        public ExponentialFilterSettingsV3(float weight = 0.9f, Vector3 initialValue = default,
                                           bool initializeOnFirstUpdate = true)
            : base(weight, initializeOnFirstUpdate)
        {
            InitialValue = initialValue;
        }
    }

    /// <summary>
    /// Settings for an exponential filter that operates on a <see cref="Quaternion"/>.
    /// </summary>
    [System.Serializable]
    public class ExponentialFilterSettingsQuat : ExponentialFilterSettingsBase
    {
        /// <summary>The initial value of the filter if InitializeOnFirstUpdate is false.</summary>
        [Tooltip("The initial value of the filter if InitializeOnFirstUpdate is false.")]
        public Quaternion InitialValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExponentialFilterSettingsQuat"/> class.
        /// </summary>
        /// <param name="weight">The weight of the filter (0.01-0.99).</param>
        /// <param name="initialValue">The initial value of the filter.</param>
        /// <param name="initializeOnFirstUpdate">Whether to initialize the filter on the first
        /// update.</param>
        public ExponentialFilterSettingsQuat(float weight = 0.9f, Quaternion initialValue = default,
                                             bool initializeOnFirstUpdate = true)
            : base(weight, initializeOnFirstUpdate)
        {
            InitialValue = initialValue;
        }
    }
}
