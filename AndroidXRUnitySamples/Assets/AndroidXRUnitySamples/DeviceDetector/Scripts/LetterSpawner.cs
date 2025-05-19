// <copyright file="LetterSpawner.cs" company="Google LLC">
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
using UnityEngine.InputSystem;

namespace AndroidXRUnitySamples.DeviceDetector
{
    /// <summary>
    /// Class used to spawn balloon letters.
    /// </summary>
    public class LetterSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _letterPrefab;
        [SerializeField] private LineRenderer _boundsVisuals;
        [SerializeField] private float _adjustSmoothing;
        [SerializeField] private AnimationCurve _alphaFromLifetime;
        [SerializeField] private Transform _helpText;
        [SerializeField] private Vector3 _debugPosition;
        [SerializeField] private Vector3 _debugExtents;
        [SerializeField] private Vector3 _debugEulers;

        private Vector3 _desiredExtents;
        private Vector3 _workingExtents;
        private Vector3 _desiredPos;
        private Vector3 _workingPos;
        private Quaternion _desiredRot;
        private Quaternion _workingRot;
        private Key[] _letterKeys = new Key[]
        {
            Key.A,
            Key.B,
            Key.C,
            Key.D,
            Key.E,
            Key.F,
            Key.G,
            Key.H,
            Key.I,
            Key.J,
            Key.K,
            Key.L,
            Key.M,
            Key.N,
            Key.O,
            Key.P,
            Key.Q,
            Key.R,
            Key.S,
            Key.T,
            Key.U,
            Key.V,
            Key.W,
            Key.X,
            Key.Y,
            Key.Z,
        };

        /// <summary>
        /// Inits the bounds transform.
        /// </summary>
        /// <param name="pos">The position of the spawner.</param>
        /// <param name="rot">The rotation of the spawner.</param>
        public void InitBounds(Vector3 pos, Quaternion rot)
        {
            transform.rotation = rot;
            _workingRot = rot;
            _desiredRot = rot;

            transform.position = pos;
            _workingPos = pos;
            _desiredPos = pos;

            _workingExtents = Vector3.zero;
            _desiredExtents = Vector3.zero;

            UpdateAlpha(1.0f);
        }

        /// <summary>
        /// Updates the target transform.
        /// </summary>
        /// <param name="pos">The position of the spawner.</param>
        /// <param name="rot">The rotation of the spawner.</param>
        /// <param name="extent">The extents of the spawner.</param>
        public void UpdateTargetTransformAndExtents(Vector3 pos, Quaternion rot, Vector3 extent)
        {
            _desiredRot = rot;
            _desiredPos = pos;
            _desiredExtents = extent;
        }

        /// <summary>
        /// Updates the alpha value.
        /// </summary>
        /// <param name="percent">The percent, (0, 1), alive the spawner is.</param>
        public void UpdateAlpha(float percent)
        {
            Color col = _boundsVisuals.material.color;
            col.a = _alphaFromLifetime.Evaluate(percent);
            _boundsVisuals.material.color = col;
        }

        private void Awake()
        {
            _desiredExtents = _debugExtents;
            _workingExtents = _desiredExtents;
            _boundsVisuals.positionCount = 5;
        }

        private void Update()
        {
            float lerpT = 1.0f - Mathf.Exp(Time.deltaTime * -_adjustSmoothing);

            // Intentionally not using slerp.
            _workingRot.x = Mathf.Lerp(_workingRot.x, _desiredRot.x, lerpT);
            _workingRot.y = Mathf.Lerp(_workingRot.y, _desiredRot.y, lerpT);
            _workingRot.z = Mathf.Lerp(_workingRot.z, _desiredRot.z, lerpT);
            _workingRot.w = Mathf.Lerp(_workingRot.w, _desiredRot.w, lerpT);
            transform.rotation = _workingRot;

            _workingPos = Vector3.Lerp(_workingPos, _desiredPos, lerpT);
            transform.position = _workingPos;

            _workingExtents = Vector3.Lerp(_workingExtents, _desiredExtents, lerpT);

            int letterIndex = 0;
            foreach (Key key in _letterKeys)
            {
                if (Keyboard.current[key].wasPressedThisFrame)
                {
                    Vector3 offset = new Vector3(
                        KeyboardXOffsetForKey(key),
                        0.0f,
                        KeyboardZOffsetForKey(key));
                    offset.x *= _workingExtents.x;
                    offset.z *= _workingExtents.z;
                    Vector3 transformedPos = transform.TransformPoint(offset);

                    GameObject go = Instantiate(
                        _letterPrefab, transformedPos, transform.rotation, transform);
                    go.GetComponent<Letter>().Init(letterIndex);
                }

                letterIndex++;
            }

            Vector3 c0 = new Vector3(-_workingExtents.x, _workingExtents.y, -_workingExtents.z);
            Vector3 c1 = new Vector3(_workingExtents.x, _workingExtents.y, -_workingExtents.z);
            Vector3 c2 = new Vector3(_workingExtents.x, _workingExtents.y, _workingExtents.z);
            Vector3 c3 = new Vector3(-_workingExtents.x, _workingExtents.y, _workingExtents.z);

            _boundsVisuals.SetPosition(0, transform.TransformPoint(c0 * 0.5f));
            _boundsVisuals.SetPosition(1, transform.TransformPoint(c1 * 0.5f));
            _boundsVisuals.SetPosition(2, transform.TransformPoint(c2 * 0.5f));
            _boundsVisuals.SetPosition(3, transform.TransformPoint(c3 * 0.5f));
            _boundsVisuals.SetPosition(4, transform.TransformPoint(c0 * 0.5f));

            // Find the line segment closest to us.
            Vector3 camPos = Singleton.Instance.Camera.transform.position;
            Vector3[] boundsPositions = new Vector3[5];
            _boundsVisuals.GetPositions(boundsPositions);
            int bestIndex = 0;
            float bestDist = float.MaxValue;
            for (int i = 0; i < 4; ++i)
            {
                Vector3 segment = boundsPositions[i + 1] - boundsPositions[i];
                segment.y = 0.0f;
                segment.Normalize();

                Vector3 midpoint = (boundsPositions[i + 1] + boundsPositions[i]) * 0.5f;
                Vector3 toMid = midpoint - camPos;
                toMid.y = 0.0f;
                float distToMid = toMid.magnitude;
                toMid.Normalize();

                // Only look for the broad segments, not the glancing segments.
                if (Vector3.Angle(segment, toMid) > 45.0f)
                {
                    if (distToMid < bestDist)
                    {
                        bestDist = distToMid;
                        bestIndex = i;
                    }
                }
            }

            // Position text next to best index, with the same orientation.
            _helpText.position = boundsPositions[bestIndex];
            _helpText.right =
                (boundsPositions[bestIndex + 1] - boundsPositions[bestIndex]).normalized;
        }

        private float KeyboardXOffsetForKey(Key key)
        {
            switch (key)
            {
            case Key.Q: return -0.4f;
            case Key.W: return -0.3f;
            case Key.E: return -0.2f;
            case Key.R: return -0.1f;
            case Key.T: return -0.035f;
            case Key.Y: return 0.035f;
            case Key.U: return 0.1f;
            case Key.I: return 0.2f;
            case Key.O: return 0.3f;
            case Key.P: return 0.4f;

            case Key.A: return -0.35f;
            case Key.S: return -0.25f;
            case Key.D: return -0.15f;
            case Key.F: return -0.05f;
            case Key.G: return 0.0f;
            case Key.H: return 0.05f;
            case Key.J: return 0.15f;
            case Key.K: return 0.25f;
            case Key.L: return 0.35f;

            case Key.Z: return -0.3f;
            case Key.X: return -0.2f;
            case Key.C: return -0.1f;
            case Key.V: return 0.0f;
            case Key.B: return 0.1f;
            case Key.N: return 0.2f;
            case Key.M: return 0.3f;
            }

            return 0.0f;
        }

        private float KeyboardZOffsetForKey(Key k)
        {
            switch (k)
            {
            case Key.Q:
            case Key.W:
            case Key.E:
            case Key.R:
            case Key.T:
            case Key.Y:
            case Key.U:
            case Key.I:
            case Key.O:
            case Key.P:
                return 0.15f;

            case Key.A:
            case Key.S:
            case Key.D:
            case Key.F:
            case Key.G:
            case Key.H:
            case Key.J:
            case Key.K:
            case Key.L:
                return 0.0f;

            case Key.Z:
            case Key.X:
            case Key.C:
            case Key.V:
            case Key.B:
            case Key.N:
            case Key.M:
                return -0.15f;
            }

            return 0.0f;
        }
    }
}
