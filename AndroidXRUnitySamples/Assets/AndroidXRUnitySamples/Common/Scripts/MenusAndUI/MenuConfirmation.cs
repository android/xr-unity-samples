// <copyright file="MenuConfirmation.cs" company="Google LLC">
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
using UnityEngine.Events;
using UnityEngine.UI;

namespace AndroidXRUnitySamples.MenusAndUI
{
    /// <summary>
    /// Controls state and callbacks for MenuManager confirmation popups.
    /// </summary>
    public class MenuConfirmation : MonoBehaviour
    {
        [Header("Objects")]
        [SerializeField] private CanvasGroup _shadow;
        [SerializeField] private RectTransform _dialogXf;
        [SerializeField] private CanvasGroup _dialogGroup;
        [SerializeField] private ShadowButton _approve;
        [SerializeField] private ShadowButton _decline;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private Image _image;

        [Header("Tunables")]
        [SerializeField] private float _animSpeed;
        [SerializeField] private AnimationCurve _shadowCurve;
        [SerializeField] private AnimationCurve _dialogCurve;
        [SerializeField] private float _dialogMinScale;

        private float _animT;
        private bool _show;

        /// <summary>Sets the name on the confirmation dialog.</summary>
        /// <param name="text">The text.</param>
        public void SetTitleText(string text)
        {
            _titleText.text = text;
        }

        /// <summary>Sets the description text on the confirmation dialog.</summary>
        /// <param name="text">The text.</param>
        public void SetDescriptionText(string text)
        {
            _descriptionText.text = text;
        }

        /// <summary>Sets the sprite image on the confirmation dialog.</summary>
        /// <param name="sp">The sprite.</param>
        public void SetImage(Sprite sp)
        {
            _image.sprite = sp;
        }

        /// <summary>Adds a callback to the approve button.</summary>
        /// <param name="call">The callback.</param>
        public void AddApproveCallback(UnityAction call)
        {
            _approve.OnPress.AddListener(call);
        }

        void Awake()
        {
            _show = true;
            _animT = 0.0f;
            _dialogXf.localScale = Vector3.zero;
            _shadow.alpha = 0.0f;
            _dialogGroup.alpha = 0.0f;
            _approve.OnPress.AddListener(Dismiss);
            _decline.OnPress.AddListener(Dismiss);
        }

        void Update()
        {
            if (_show)
            {
                if (_animT < 1.0f)
                {
                    _animT += Time.deltaTime * _animSpeed;
                    _animT = Mathf.Clamp01(_animT);
                    RefreshVisuals();
                }
            }
            else
            {
                _animT -= Time.deltaTime * _animSpeed;
                _animT = Mathf.Clamp01(_animT);
                if (_animT <= 0.0f)
                {
                    Destroy(gameObject);
                }
                else
                {
                    RefreshVisuals();
                }
            }
        }

        void RefreshVisuals()
        {
            _shadow.alpha = _shadowCurve.Evaluate(_animT);

            float dialogValue = _dialogCurve.Evaluate(_animT);
            _dialogGroup.alpha = dialogValue;
            _dialogXf.localScale =
                Vector3.one * Mathf.Lerp(_dialogMinScale, 1.0f, dialogValue);
        }

        void Dismiss()
        {
            _show = false;
        }
    }
}
