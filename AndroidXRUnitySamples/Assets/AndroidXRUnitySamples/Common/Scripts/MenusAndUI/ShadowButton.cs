// <copyright file="ShadowButton.cs" company="Google LLC">
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
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AndroidXRUnitySamples.MenusAndUI
{
    /// <summary>
    /// A shadow button.
    /// </summary>
    public class ShadowButton : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>The press event.</summary>
        public Button.ButtonClickedEvent OnPress;

        [SerializeField] private float _animateSpeed;
        [SerializeField] private CanvasGroup _hoverGroup;
        [SerializeField] private CanvasGroup _innerShadows;
        [SerializeField] private CanvasGroup _outerShadows;
        [SerializeField] private Transform _iconParent;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _iconText;
        [SerializeField] private Vector3 _iconShift;
        [SerializeField] private Color _iconBaseColor;
        [SerializeField] private Color _iconPressColor;
        [SerializeField] private Color _iconDisabledColor;

        private bool _pressed;
        private float _pressValue;
        private bool _hover;
        private float _hoverValue;
        private bool _disabled;
        private Vector3 _iconLocalPos;

        /// <summary>Handler for when there is a click on this.</summary>
        /// <param name="eventData">Info about the event.</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_disabled)
            {
                _pressed = true;
            }
        }

        /// <summary>Handler for when there is a click release on this.</summary>
        /// <param name="eventData">Info about the event.</param>
        public void OnPointerUp(PointerEventData eventData)
        {
            _pressed = false;
        }

        /// <summary>Handler for when the pointer enters this.</summary>
        /// <param name="eventData">Info about the event.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_disabled)
            {
                _hover = true;
                PlayHoverSound();
            }
        }

        /// <summary>Handler for when the pointer exits this.</summary>
        /// <param name="eventData">Info about the event.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            _hover = false;
        }

        /// <summary>Function to set the button's caption.</summary>
        /// <param name="s">Button caption.</param>
        public void SetButtonText(string s)
        {
            if (_iconText != null)
            {
                _iconText.text = s;
            }
        }

        /// <summary>Function to set the button's icon.</summary>
        /// <param name="icon">Sprite icon for button.</param>
        public void SetButtonIcon(Sprite icon)
        {
            _iconImage.sprite = icon;
        }

        /// <summary>Function to set whether the button is disabled or not.</summary>
        /// <param name="disabled">Boolean for disabled or not.</param>
        public void SetButtonDisabled(bool disabled)
        {
            if (_disabled != disabled)
            {
                _disabled = disabled;
                if (_iconImage != null)
                {
                    _iconImage.color = disabled ? _iconDisabledColor : _iconBaseColor;
                }

                if (_iconText != null)
                {
                    _iconText.color = disabled ? _iconDisabledColor : _iconBaseColor;
                }

                if (_disabled)
                {
                    _pressed = false;
                    _hover = false;
                }
            }
        }

        /// <summary>Function to trigger a press.</summary>
        public void TriggerPress()
        {
            // TriggerPress is overloaded and can be called by radio buttons.
            if (_pressed)
            {
                Singleton.Instance.Audio.PlayButtonPress(transform.position);
            }

            OnPress.Invoke();
        }

        /// <summary>Function to reset the pressed and hover visuals.</summary>
        public void Reset()
        {
            _pressed = false;
            _pressValue = 0.0f;
            _hover = false;
            _hoverValue = 0.0f;
            RefreshShadowVisuals();
            SetShadowHoverProperties();
            SetIconOffset(0.0f);
        }

        /// <summary>Overridable function for accessing pressed state.</summary>
        /// <returns>If button is pressed.</returns>
        protected virtual bool ButtonIsPressed()
        {
            return _pressed;
        }

        /// <summary>Overridable function for triggering hover sound.</summary>
        protected virtual void PlayHoverSound()
        {
            Singleton.Instance.Audio.PlayButtonHover(transform.position);
        }

        void Awake()
        {
            _iconLocalPos = _iconParent.localPosition;
            Reset();
        }

        void OnEnable()
        {
            Reset();
        }

        void Update()
        {
            bool buttonIsPressed = ButtonIsPressed();

            float prevHoverValue = _hoverValue;
            if (_pressValue <= 0.0f && _hover)
            {
                _hoverValue += Time.deltaTime * _animateSpeed;
                _hoverValue = Mathf.Min(_hoverValue, 1.0f);
            }
            else
            {
                // If we're pressing, slam the hover value to zero.
                if (buttonIsPressed)
                {
                    _hoverValue = 0.0f;
                }
                else
                {
                    _hoverValue -= Time.deltaTime * _animateSpeed;
                    _hoverValue = Mathf.Max(_hoverValue, 0.0f);
                }
            }

            if (prevHoverValue != _hoverValue)
            {
                SetShadowHoverProperties();
            }

            if (buttonIsPressed && _pressValue < 1.0f)
            {
                float prevValue = _pressValue;
                _pressValue += Time.deltaTime * _animateSpeed;
                _pressValue = Mathf.Min(_pressValue, 1.0f);
                RefreshShadowVisuals();
                if (prevValue < 0.5f && _pressValue >= 0.5f)
                {
                    TriggerPress();
                }

                SetIconOffset(_pressValue);
            }
            else if (!buttonIsPressed && _pressValue > 0.0f)
            {
                _pressValue -= Time.deltaTime * _animateSpeed;
                _pressValue = Mathf.Max(_pressValue, 0.0f);
                RefreshShadowVisuals();
                SetIconOffset(_pressValue);
            }
        }

        void RefreshShadowVisuals()
        {
            _innerShadows.alpha = Mathf.Max(_pressValue - 0.5f) * 2.0f;
            _outerShadows.alpha = 1.0f - (Mathf.Min(_pressValue, 0.5f) * 2.0f);
        }

        void SetIconOffset(float percent)
        {
            if (_iconParent != null)
            {
                _iconParent.localPosition = _iconLocalPos + _iconShift * percent;
                Color col = Color.Lerp(_iconBaseColor, _iconPressColor, percent);
                if (_disabled)
                {
                    col = _iconDisabledColor;
                }

                if (_iconImage != null)
                {
                    _iconImage.color = col;
                }

                if (_iconText != null)
                {
                    _iconText.color = col;
                }
            }
        }

        void SetShadowHoverProperties()
        {
            _hoverGroup.alpha = _hoverValue;
        }
    }
}
