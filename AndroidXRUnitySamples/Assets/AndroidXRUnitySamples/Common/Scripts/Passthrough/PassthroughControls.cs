// <copyright file="PassthroughControls.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.InteractionTechniques
{
    /// <summary>
    /// Provides control of the level of passthrough applied to a GameObject with a Material.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class PassthroughControls : MonoBehaviour
    {
        private static readonly int _shaderAlphaProp = Shader.PropertyToID("_Alpha");

        [SerializeField]
        private Material _baseMaterial;

        [SerializeField]
        private Material _passthroughMaterial;

        [SerializeField]
        private bool _passthroughEnabled;
        private bool _initialPassthroughState;

        [SerializeField]
        [Range(0f, 1f)]
        private float _passthroughLevel;
        private float _initialPassthroughLevel;

        [SerializeField]
        private bool _applyToSharedMaterial = true;

        [SerializeField]
        private BoolVariable _passthroughEnabledVariable;

        [SerializeField]
        private FloatVariable _passthroughLevelVariable;

        private Renderer _ownRenderer;
        private Material _ownMaterial;

        private void Awake()
        {
            _ownRenderer = GetComponent<Renderer>();
            SetOwnMaterialReference();
        }

        private void Start()
        {
            _initialPassthroughState = _passthroughEnabledVariable.Value;
            _initialPassthroughLevel = _passthroughLevelVariable.Value;
        }

        private void OnEnable()
        {
            if (_passthroughEnabledVariable != null)
            {
                _passthroughEnabledVariable.AddListener(HandlePassthroughEnabledChanged);
            }

            if (_passthroughLevelVariable != null)
            {
                _passthroughLevelVariable.AddListener(HandlePassthroughLevelChanged);
            }
        }

        private void OnDisable()
        {
            if (_passthroughLevelVariable != null)
            {
                _passthroughEnabledVariable.RemoveListener(HandlePassthroughEnabledChanged);
            }

            if (_passthroughLevelVariable != null)
            {
                _passthroughLevelVariable.RemoveListener(HandlePassthroughLevelChanged);
            }
        }

        private void SetOwnMaterialReference()
        {
            _ownMaterial = _applyToSharedMaterial
                                   ? _ownRenderer.sharedMaterial
                                   : _ownRenderer.material;
        }

        private void HandlePassthroughEnabledChanged(bool value)
        {
            TogglePassthrough(value);
        }

        private void HandlePassthroughLevelChanged(float value)
        {
            SetPassthroughLevel(_passthroughLevelVariable.Value);
        }

        private void TogglePassthrough(bool ptEnabled)
        {
            _passthroughEnabled = ptEnabled;
            _passthroughLevel = _passthroughEnabled ? _passthroughLevel : 1f;
        }

        private void SetPassthroughLevel(float level)
        {
            _passthroughLevel = level;
        }

        private void UpdatePassthroughMode()
        {
            if (_passthroughEnabled && !Singleton.Instance.OriginManager.EnablePassthrough)
            {
                Singleton.Instance.OriginManager.EnablePassthrough = _passthroughEnabled;
            }

            Material mat = _passthroughEnabled ? _passthroughMaterial : _baseMaterial;
            if (_applyToSharedMaterial)
            {
                _ownRenderer.sharedMaterial = mat;
            }
            else
            {
                _ownRenderer.material = mat;
            }

            SetOwnMaterialReference();
        }

        private void UpdatePassthroughLevel()
        {
            if (_passthroughEnabled)
            {
                _ownMaterial.SetFloat(_shaderAlphaProp, 1 - _passthroughLevel);
            }
        }

        private void Update()
        {
            UpdatePassthroughMode();
            UpdatePassthroughLevel();
        }

        private void OnDestroy()
        {
            if (_applyToSharedMaterial)
            {
                _passthroughEnabledVariable.Value = _initialPassthroughState;
                _passthroughLevelVariable.Value = _initialPassthroughLevel;
                _passthroughLevel = _initialPassthroughLevel;
                UpdatePassthroughLevel();
            }
        }
    }
}
