// <copyright file="RoomDimming.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.SoundArena
{
    /// <summary>
    /// Script for handling the room dimming.
    /// </summary>
    public class RoomDimming : MonoBehaviour
    {
        private static readonly int _shaderAlphaProp = Shader.PropertyToID("_Alpha");

        [SerializeField] private Renderer[] _roomObjects;
        [SerializeField] private float _dimSpeed;

        private float _dimAmount;

        private void Start()
        {
            Singleton.Instance.OriginManager.EnablePassthrough = true;
            _dimAmount = 0.0f;
        }

        private void Update()
        {
            if (_dimAmount < 1.0f)
            {
                _dimAmount += Time.deltaTime * _dimSpeed;
                _dimAmount = Mathf.Min(_dimAmount, 1.0f);
                for (int i = 0; i < _roomObjects.Length; ++i)
                {
                    _roomObjects[i].material.SetFloat(_shaderAlphaProp, _dimAmount);
                }
            }
        }
    }
}
