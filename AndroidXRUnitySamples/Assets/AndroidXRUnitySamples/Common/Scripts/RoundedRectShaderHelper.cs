// <copyright file="RoundedRectShaderHelper.cs" company="Google LLC">
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
using UnityEngine.UI;

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Script for configuring shader parameters for the rounded rect shader.
    /// </summary>
    public class RoundedRectShaderHelper : MonoBehaviour
    {
        private static int _kBounds = Shader.PropertyToID("_Bounds");
        private static int _kCornerRadius = Shader.PropertyToID("_CornerRadius");

        [SerializeField] private float _cornerRadius = 15.0f;

        [ContextMenu("Refresh Material")]
        private void RefreshMaterial()
        {
            Material imageMat = new Material(GetComponent<Image>().material);
            imageMat.SetVector(_kBounds, GetComponent<RectTransform>().rect.size);
            imageMat.SetFloat(_kCornerRadius, _cornerRadius);
            GetComponent<Image>().material = imageMat;
        }

        private void Start()
        {
            RefreshMaterial();
        }
    }
}
