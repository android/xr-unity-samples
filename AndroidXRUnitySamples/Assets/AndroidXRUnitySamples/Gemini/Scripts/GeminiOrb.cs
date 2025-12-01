// <copyright file="GeminiOrb.cs" company="Google LLC">
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
using System.Collections;
using TMPro;
using UnityEngine;

namespace AndroidXRUnitySamples.Gemini
{
    /// <summary>
    /// State machine for Gemini Orb object.
    /// </summary>
    public class GeminiOrb : MonoBehaviour
    {
        [SerializeField] private Animator _animatorMiniSpheres;
        [SerializeField] private Animator _animatorBehvaior;
        [SerializeField] private Renderer[] _orbMeshes;
        [SerializeField] private Transform _animParent;
        [SerializeField] private SimpleScalingHelper _chefsHat;

        [Header("Behaviors")]
        [SerializeField] private Color[] _behaviorColors;
        [SerializeField] private float _colorChangeSpeed;

        [Header("Hover")]
        [SerializeField] private Transform[] _hoverMeshes;
        [SerializeField] private float _sizeChangeSpeed;
        [SerializeField] private Vector2 _mainAnimHoverOnOffSpeed;
        [SerializeField] private Vector2 _mainAnimHoverOnOffSize;

        [Header("Dynamic material object")]
        [SerializeField] private Material _dynamicMaterial;
        [SerializeField] private GameObject _dynamicMaterialCubePrefab;
        [SerializeField] private Transform _dynamicMaterialCubeParent;
        [SerializeField] private TMP_Text _dynamicMaterialPhrase;
        [SerializeField] private float _writePhraseDuration;
        [SerializeField] private float _erasePhraseDuration;

        private Material _localMaterial;
        private GameObject _dynamicMaterialCube;

        private Color _currentColor;
        private Color _desiredColor;
        private float _currentSize;
        private float _desiredSize;

        /// <summary>
        /// Update animations, behaviors, and colors.
        /// </summary>
        /// <param name="currentMode">The current mode.</param>
        /// <param name="prevState">The state we're leaving.</param>
        /// <param name="newState">The state we're entering.</param>
        public void SetState(
            GeminiMaterials.Mode currentMode,
            GeminiMaterials.State prevState,
            GeminiMaterials.State newState)
        {
            // Exit state.
            switch (prevState)
            {
            case GeminiMaterials.State.DoneResponding:
                if (_dynamicMaterialCube != null)
                {
                    _dynamicMaterialCube.GetComponent<ScaleOnEnable>().ScaleDownAndDestroy();
                    _dynamicMaterialCube = null;
                }

                if (_dynamicMaterialPhrase.gameObject.activeSelf)
                {
                    StartCoroutine(EraseDynamicMaterialPhrase());
                }

                _chefsHat.ScaleDownAndDisable();
                break;
            }

            // Init state is -1, but we want our color and animation state at 0 during Init.
            _desiredColor = _behaviorColors[Mathf.Max((int)newState, 0)];
            _animatorBehvaior.SetInteger("Behavior", Mathf.Max((int)newState, 0));
            SetHovering(false);

            // Enter state.
            switch (newState)
            {
            case GeminiMaterials.State.Listening:
                if (currentMode == GeminiMaterials.Mode.Conversation)
                {
                    _chefsHat.gameObject.SetActive(true);
                    _chefsHat.ScaleUp();
                }

                break;
            case GeminiMaterials.State.Error:
                _chefsHat.ScaleDownAndDisable();
                break;
            }
        }

        /// <summary>
        /// Update visuals for hovering or not.
        /// </summary>
        /// <param name="hovering">Are we hovering or not.</param>
        public void SetHovering(bool hovering)
        {
            _animatorMiniSpheres.SetFloat("Speed",
                (hovering ? _mainAnimHoverOnOffSpeed.x : _mainAnimHoverOnOffSpeed.y));
            _desiredSize = hovering ? _mainAnimHoverOnOffSize.x : _mainAnimHoverOnOffSize.y;
        }

        /// <summary>
        /// Preps the material to be set from a json string.
        /// </summary>
        /// <param name="s">String containing json info.</param>
        /// <returns>The summary string from the json.</returns>
        public string SetMaterialFromJson(string s)
        {
            // "A glowing star"
            /*
            const debugTestMaterialJsonString =
                "{ \"MainColor\":[255, 255, 200],\"MetallicAmount\": 0.0,\"Smoothness\":0.2," +
                "\"Emissiveness\":true,\"EmissiveColor\":[255, 230, 100],\"MainTexture\":[160, " +
                "165, 170, 175, 170, 165, 160, 155, 150, 145, 150, 155, 160, 165, 170, 160, " +
                "155, 160, 165, 170, 165, 160, 155, 150, 145, 140, 145, 150, 155, 160, 165, " +
                "155, 150, 155, 160, 165, 160, 155, 150, 145, 140, 135, 140, 145, 150, 155, " +
                "160, 150, 145, 150, 155, 160, 155, 150, 145, 140, 135, 130, 135, 140, 145, " +
                "150, 155, 145, 150, 155, 160, 155, 150, 145, 140, 135, 140, 145, 150, 155, " +
                "160, 155, 150, 150, 155, 160, 165, 160, 155, 150, 145, 140, 145, 150, 155, " +
                "160, 165, 160, 155, 155, 160, 165, 170, 165, 160, 155, 150, 145, 150, 155, " +
                "160, 165, 170, 165, 160, 160, 165, 170, 175, 170, 165, 160, 155, 150, 155, " +
                "160, 165, 170, 175, 170, 165, 165, 170, 175, 180, 175, 170, 165, 160, 155, " +
                "160, 165, 170, 175, 180, 175, 170, 170, 165, 170, 175, 170, 165, 160, 155, " +
                "150, 155, 160, 165, 170, 175, 170, 165, 165, 160, 165, 170, 165, 160, 155, " +
                "150, 145, 150, 155, 160, 165, 170, 165, 160, 160, 155, 160, 165, 160, 155, " +
                "150, 145, 140, 145, 150, 155, 160, 165, 160, 155, 155, 150, 155, 160, 155, " +
                "150, 145, 140, 135, 140, 145, 150, 155, 160, 155, 150, 150, 145, 150, 155, " +
                "150, 145, 140, 135, 130, 135, 140, 145, 150, 155, 150, 145, 145, 152, 158, " +
                "162, 158, 152, 148, 142, 138, 142, 148, 152, 158, 162, 158, 152, 152, 160, " +
                "165, 170, 175, 170, 165, 160, 155, 150, 145, 150, 155, 160, 165, 170, 160],
                "\"Summary\":\"A glowing star\"}";
            */

            DynamicMaterialProps jsonMat;
            try
            {
                // Try cleaning the input string in case Gemini added some spice.
                int firstOpeningBracket = s.IndexOf("{");
                int lastClosingBracket = s.LastIndexOf("}");
                string cleanedString =
                    s.Substring(firstOpeningBracket, lastClosingBracket - firstOpeningBracket + 1);
                jsonMat = JsonUtility.FromJson<DynamicMaterialProps>(cleanedString);
            }
            catch (Exception)
            {
                return "Sorry, I made a mistake.";
            }

            // Verify our json wasn't malformed.
            if (jsonMat == null ||
                jsonMat.MainColor == null ||
                jsonMat.EmissiveColor == null ||
                jsonMat.MainTexture == null)
            {
                Debug.LogError("Malformed json in SetMaterialFromJson");
                return string.Empty;
            }

            TextureCreator.CreateAndApplyTextureFromBytes(
                jsonMat.MainTexture, 16, 16, _localMaterial);
            _localMaterial.color =
                new Color(
                    jsonMat.MainColor[0] / 255.0f,
                    jsonMat.MainColor[1] / 255.0f,
                    jsonMat.MainColor[2] / 255.0f);
            _localMaterial.SetFloat("_Glossiness", jsonMat.Smoothness);
            _localMaterial.SetFloat("_Metallic", jsonMat.MetallicAmount);
            if (jsonMat.Emissiveness)
            {
                _localMaterial.EnableKeyword("_EMISSION");
                _localMaterial.SetColor("_EmissionColor",
                    new Color(
                        jsonMat.EmissiveColor[0] / 255.0f,
                        jsonMat.EmissiveColor[1] / 255.0f,
                        jsonMat.EmissiveColor[2] / 255.0f));
            }
            else
            {
                _localMaterial.DisableKeyword("_EMISSION");
            }

            // Create the dynamic material cube if it doesn't already exist.
            if (_dynamicMaterialCube == null)
            {
                _dynamicMaterialCube =
                    Instantiate(_dynamicMaterialCubePrefab, _dynamicMaterialCubeParent);
            }

            _dynamicMaterialCube.GetComponentInChildren<Renderer>().material = _localMaterial;
            _dynamicMaterialPhrase.gameObject.SetActive(true);
            StartCoroutine(WriteOutDynamicMaterialPhrase(jsonMat.Summary));

            return jsonMat.Summary;
        }

        private void Awake()
        {
            _localMaterial = new Material(_dynamicMaterial);
            _currentSize = _mainAnimHoverOnOffSize.y;
            _desiredSize = _currentSize;
            _dynamicMaterialPhrase.gameObject.SetActive(false);
            _currentColor = _desiredColor = _behaviorColors[0];
            _chefsHat.gameObject.SetActive(false);
        }

        private void Update()
        {
            // Update color.
            float colorChangeRate = Time.deltaTime * _colorChangeSpeed;
            _currentColor.r = MoveTowards(_currentColor.r, _desiredColor.r, colorChangeRate);
            _currentColor.g = MoveTowards(_currentColor.g, _desiredColor.g, colorChangeRate);
            _currentColor.b = MoveTowards(_currentColor.b, _desiredColor.b, colorChangeRate);
            for (int i = 0; i < _orbMeshes.Length; ++i)
            {
                _orbMeshes[i].material.SetColor("_Tint", _currentColor);
            }

            // Update size.
            if (_desiredSize != _currentSize)
            {
                float sizeChangeRate = Time.deltaTime * _sizeChangeSpeed;
                _currentSize = MoveTowards(_currentSize, _desiredSize, sizeChangeRate);
                for (int i = 0; i < _hoverMeshes.Length; ++i)
                {
                    _hoverMeshes[i].localScale = Vector3.one * _currentSize;
                }
            }
        }

        private float MoveTowards(float currentValue, float desiredValue, float rate)
        {
            if (currentValue < desiredValue)
            {
                return Mathf.Min(currentValue + rate, desiredValue);
            }
            else if (currentValue > desiredValue)
            {
                return Mathf.Max(currentValue - rate, desiredValue);
            }

            return desiredValue;
        }

        private IEnumerator WriteOutDynamicMaterialPhrase(string s)
        {
            float time = 0.0f;
            int totalCharacters = s.Length;

            while (time < _writePhraseDuration)
            {
                float floatCharaters = totalCharacters * (time / _writePhraseDuration);
                int numToWrite = Mathf.Min((int)Mathf.Ceil(floatCharaters), totalCharacters);
                _dynamicMaterialPhrase.text = s.Substring(0, numToWrite);
                time += Time.deltaTime;
                yield return null;
            }

            _dynamicMaterialPhrase.text = s;
        }

        private IEnumerator EraseDynamicMaterialPhrase()
        {
            float time = 0.0f;
            string s = _dynamicMaterialPhrase.text;
            int totalCharacters = s.Length;

            while (time < _erasePhraseDuration)
            {
                float floatCharaters = totalCharacters * (1.0f - (time / _erasePhraseDuration));
                int numToWrite = Mathf.Min((int)Mathf.Ceil(floatCharaters), totalCharacters);
                _dynamicMaterialPhrase.text = s.Substring(0, numToWrite);
                time += Time.deltaTime;
                yield return null;
            }

            _dynamicMaterialPhrase.text = string.Empty;
            _dynamicMaterialPhrase.gameObject.SetActive(false);
        }
    }
}
