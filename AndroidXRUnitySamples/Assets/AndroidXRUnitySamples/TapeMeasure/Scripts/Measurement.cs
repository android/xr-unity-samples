// <copyright file="Measurement.cs" company="Google LLC">
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

using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace AndroidXRUnitySamples.TapeMeasure
{
    /// <summary>
    /// Class used to control the measurements in TapeMeasure.
    /// </summary>
    public class Measurement : MonoBehaviour
    {
        [SerializeField] private GameObject _startPoint;
        [SerializeField] private GameObject _endPoint;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private Transform _caption;
        [SerializeField] private Transform _textBackground;
        [SerializeField] private float _backgroundWidthScalar;
        [SerializeField] private TextMeshPro[] _texts;
        [SerializeField] private Gradient _gradient;
        [SerializeField] private float _maxGradientLength = 3f;
        [SerializeField] private MeshRenderer _startPointRenderer;
        [SerializeField] private MeshRenderer _endPointRenderer;

        private State _state = State.SelectingFirst;

        private enum State
        {
            SelectingFirst,
            SelectingSecond,
            Done,
        }

        /// <summary>
        /// Gets a value indicating whether this measurement is done or not.
        /// </summary>
        public bool IsDone
        {
            get
            {
                return _state == State.Done;
            }
        }

        /// <summary>
        /// Moves the current position (start or end) to follow provided value.
        /// </summary>
        /// <param name="position">Position to set current psotion to.</param>
        public void UpdatePosition(Vector3 position)
        {
            switch (_state)
            {
            case State.SelectingFirst:
                _startPoint.transform.position = position;
                break;
            case State.SelectingSecond:
                _endPoint.transform.position = position;
                UpdateVisuals();
                break;
            case State.Done:
                Debug.LogError("Trying to update position of done marker", this);
                break;
            }
        }

        /// <summary>
        /// Fixes current position and moves on to the next point.
        /// </summary>
        public void FixatePosition()
        {
            switch (_state)
            {
            case State.SelectingFirst:
                _state = State.SelectingSecond;
                EnableVisuals(true, true);
                AddAnchor(_startPoint);
                break;
            case State.SelectingSecond:
                _state = State.Done;
                AddAnchor(_endPoint);
                break;
            case State.Done:
                Debug.LogError("Trying to fixate position of done marker", this);
                break;
            }
        }

        private void EnableVisuals(bool enableFirstPoint, bool enableSecondPoint)
        {
            _startPoint.SetActive(enableFirstPoint);
            _endPoint.SetActive(enableSecondPoint);
            _lineRenderer.enabled = enableSecondPoint;
            _caption.gameObject.SetActive(enableSecondPoint);
        }

        private void UpdateVisuals()
        {
            var positions = new Vector3[2];
            positions[0] = _startPoint.transform.position;
            positions[1] = _endPoint.transform.position;

            _lineRenderer.positionCount = positions.Length;
            _lineRenderer.SetPositions(positions);

            float length = Vector3.Distance(positions[0], positions[1]);
            string dist = (length * 100f).ToString("0.00");
            for (int i = 0; i < _texts.Length; ++i)
            {
                _texts[i].text = $"{dist} cm";
            }

            Vector3 localScale = _textBackground.localScale;
            localScale.x = _texts[0].preferredWidth * _backgroundWidthScalar;
            _textBackground.localScale = localScale;
            Vector3 localPos = _textBackground.localPosition;
            localPos.z = localScale.x * 0.5f;
            _textBackground.localPosition = localPos;

            _caption.position = positions[0];
            Vector3 toEnd = positions[1] - positions[0];
            _caption.rotation = Quaternion.LookRotation(toEnd.normalized, Vector3.up);

            Color color = _gradient.Evaluate(length / _maxGradientLength);
            _lineRenderer.material.SetColor("_Color", color);
            _startPointRenderer.material.color = color;
            _endPointRenderer.material.color = color;
            _textBackground.GetComponent<Renderer>().material.SetColor("_Color", color);
        }

        private void AddAnchor(GameObject point)
        {
            point.AddComponent<ARAnchor>();
        }
    }
}
