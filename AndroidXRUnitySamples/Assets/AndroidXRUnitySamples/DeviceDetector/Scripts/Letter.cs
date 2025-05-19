// <copyright file="Letter.cs" company="Google LLC">
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

using System;
using UnityEngine;

namespace AndroidXRUnitySamples.DeviceDetector
{
    /// <summary>
    /// Class used to control balloon letter.
    /// </summary>
    public class Letter : MonoBehaviour
    {
        [SerializeField] private Mesh[] _letterMeshes;
        [SerializeField] private ColorPair[] _letterColors;
        [SerializeField] private float _baseScale;
        [SerializeField] private AnimationCurve _scaleCurveX;
        [SerializeField] private AnimationCurve _scaleCurveY;
        [SerializeField] private AnimationCurve _scaleCurveZ;
        [SerializeField] private float _scaleSpeed;
        [SerializeField] private Vector2 _liftRange;
        [SerializeField] private Vector2 _lifeDurationRange;
        [SerializeField] private GameObject _explodePrefab;
        [SerializeField] private AudioClipData _spawnClip;
        [SerializeField] private AudioClipData _popClip;

        private float _lifeTimer;
        private float _scaleTimer;
        private Vector3 _lift;

        private int _propRimColor = Shader.PropertyToID("_RimColor");
        private int _propTint = Shader.PropertyToID("_Tint");

        /// <summary>Function to initialize the letter visuals.</summary>
        /// <param name="letterIndex">Alphabet index (0 based) for the letter.</param>
        public void Init(int letterIndex)
        {
            GetComponentInChildren<MeshFilter>().mesh = _letterMeshes[letterIndex];

            int colorIndex = UnityEngine.Random.Range(0, _letterColors.Length);
            MeshRenderer mr = GetComponentInChildren<MeshRenderer>();
            mr.material.SetColor(_propRimColor, _letterColors[colorIndex].First);
            mr.material.SetColor(_propTint, _letterColors[colorIndex].Second);

            _lift = Vector3.up * UnityEngine.Random.Range(_liftRange.x, _liftRange.y);
        }

        private void Start()
        {
            _lifeTimer = UnityEngine.Random.Range(_lifeDurationRange.x, _lifeDurationRange.y);
            _scaleTimer = 0.0f;
            transform.localScale = Vector3.zero;
            Singleton.Instance.Audio.PlayOneShot(_spawnClip, transform.position);
        }

        private void Update()
        {
            if (_scaleTimer < 1.0f)
            {
                _scaleTimer += Time.deltaTime * _scaleSpeed;
                _scaleTimer = Mathf.Min(_scaleTimer, 1.0f);
                Vector3 scale;
                scale.x = _scaleCurveX.Evaluate(_scaleTimer) * _baseScale;
                scale.y = _scaleCurveY.Evaluate(_scaleTimer) * _baseScale;
                scale.z = _scaleCurveZ.Evaluate(_scaleTimer) * _baseScale;
                transform.localScale = scale;
            }

            _lifeTimer -= Time.deltaTime;
            if (_lifeTimer <= 0.0f)
            {
                Instantiate(_explodePrefab, transform.position, transform.rotation);
                Singleton.Instance.Audio.PlayOneShot(_popClip, transform.position);
                Destroy(gameObject);
            }
        }

        private void FixedUpdate()
        {
            GetComponent<Rigidbody>().AddForce(_lift, ForceMode.Force);
        }
    }

    /// <summary>
    /// Class used to for serializing a pair of colors.
    /// </summary>
    [Serializable]
    public class ColorPair
    {
        /// <summary>
        /// First color in the pair.
        /// </summary>
        public Color First;

        /// <summary>
        /// Second color in the pair.
        /// </summary>
        public Color Second;
    }
}
