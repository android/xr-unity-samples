// <copyright file="ScaleOnEnable.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Procedural animation class used for scaling on enable.
    /// </summary>
    public class ScaleOnEnable : MonoBehaviour
    {
        [SerializeField] private float _scaleUpDuration;
        [SerializeField] private float _scaleDownDuration;
        [SerializeField] private AnimationCurve _scaleUpCurve;
        [SerializeField] private AnimationCurve _scaleDownCurve;

        private Coroutine _scaleCoroutine;

        /// <summary>Function to destroy the object after an animation.</summary>
        public void ScaleDownAndDestroy()
        {
            if (_scaleCoroutine != null)
            {
                StopCoroutine(_scaleCoroutine);
            }

            _scaleCoroutine = StartCoroutine(ScaleDownCoroutine());
        }

        private void OnEnable()
        {
            if (_scaleCoroutine == null)
            {
                _scaleCoroutine = StartCoroutine(ScaleUpCoroutine());
            }
        }

        private IEnumerator ScaleUpCoroutine()
        {
            float time = 0.0f;
            Vector3 originalScale = transform.localScale;

            while (time < _scaleUpDuration)
            {
                float normalizedTime = time / _scaleUpDuration;
                float curveValue = _scaleUpCurve.Evaluate(normalizedTime);
                transform.localScale = originalScale * curveValue;

                time += Time.deltaTime;
                yield return null;
            }

            transform.localScale = originalScale;
        }

        private IEnumerator ScaleDownCoroutine()
        {
            float time = 0.0f;
            Vector3 originalScale = transform.localScale;

            while (time < _scaleDownDuration)
            {
                float normalizedTime = 1.0f - (time / _scaleDownDuration);
                float curveValue = _scaleDownCurve.Evaluate(normalizedTime);
                transform.localScale = originalScale * curveValue;

                time += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
