// <copyright file="MenuManager.cs" company="Google LLC">
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
using System.Collections.Generic;
using AndroidXRUnitySamples.Variables;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace AndroidXRUnitySamples.MenusAndUI
{
    /// <summary>
    /// Manages navigation throughout the application.
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        /// <summary>An event that fires when an experience button has been pressed.</summary>
        [HideInInspector] public Action StartTransitionToExperience;

        [SerializeField] private GameObject _mesh;
        [SerializeField] private CanvasGroup _canvas;
        [SerializeField] private GameObject _experiencesMenu;
        [SerializeField] private GameObject _settingsMenu;
        [SerializeField] private GameObject _inputCapture;
        [SerializeField] private BoolVariable _menuIsOpen;
        [SerializeField] private InputModeVariable _inputMode;

        [Header("Navigation Bar")]
        [SerializeField] private ShadowButton _experiencesButton;
        [SerializeField] private ShadowButton _settingsButton;
        [SerializeField] private ShadowButton _closeButton;

        [Header("Experience Menu")]
        [SerializeField] private Transform _experiencesScrollParent;
        [SerializeField] private Settings _settingsResource;
        [SerializeField] private GameObject _experienceButtonGroupPrefab;
        [SerializeField] private GameObject _experienceButtonPrefab;
        [SerializeField] private Color[] _experienceButtonGroupColors;

        [Header("Settings Menu")]
        [SerializeField] private Slider _foveationLevelSlider;
        [SerializeField] private Toggle _debugModeToggle;
        [SerializeField] private BoolVariable _debugMode;
        [SerializeField] private Toggle _useHandMeshToggle;
        [SerializeField] private BoolVariable _useHandMesh;
        [SerializeField] private Toggle _showPerformanceMetricsToggle;
        [SerializeField] private ShadowButton _exitAppButton;
        [SerializeField] private GameObject _performanceMetricsOverlay;
        [SerializeField] private ShadowButton _homeButton;
        [SerializeField] private ExperienceSettings _homeSettings;

        [Header("Settings Status")]
        [SerializeField] private Image _leftHandTrackingVisual;
        [SerializeField] private Image _rightHandTrackingVisual;
        [SerializeField] private Image _eyeTrackingVisual;
        [SerializeField] private TMP_Text _leftHandTrackingText;
        [SerializeField] private TMP_Text _rightHandTrackingText;
        [SerializeField] private TMP_Text _eyeTrackingText;
        [SerializeField] private Color _trackingOnColor;
        [SerializeField] private Color _trackingOffColor;
        [SerializeField] private InputActionProperty _leftHandTrackedAction;
        [SerializeField] private InputActionProperty _rightHandTrackedAction;
        [SerializeField] private InputActionProperty _eyeTrackedAction;

        [Header("Animation")]
        [SerializeField] private float _showHideSpeed;
        [SerializeField] private float _showHideMinimumValue;
        [SerializeField] private AnimationCurve _showHideCurve;

        [Header("Behaviour")]
        [SerializeField] private float _spawnDistance;
        [SerializeField] private DominantHandManager _dominantHandManager;

        [Header("Popups")]
        [SerializeField] private GameObject _experienceConfirmationPrefab;
        [SerializeField] private GameObject _quitConfirmationPrefab;

        private float _showHideT;
        private VisibilityState _currentVisibilityState;
        private MenuState _currentMenuState;
        private GameObject _activeConfirmation;
        private ExperienceSettings _loadExperienceSettings;

        /// <summary>State of the menu.</summary>
        public enum MenuState
        {
            /// <summary>Experiences menu.</summary>
            Experiences = 0,

            /// <summary>Settings menu.</summary>
            Settings
        }

        private enum VisibilityState
        {
            Hidden,
            TransitionToShowing,
            Showing,
            TransitionToHidden,
            TransitionToExperience,
        }

        /// <summary>
        /// Returns true if the Menu is active in the scene.
        /// </summary>
        public bool Active => _currentVisibilityState != VisibilityState.Hidden;

        private bool _loadingExperience =>
            _currentVisibilityState == VisibilityState.TransitionToExperience;

        /// <summary>
        /// Shows a menu.
        /// </summary>
        /// <param name="state">The menu enum to show.</param>
        public void SetMenuState(MenuState state)
        {
            SetMenuVisible(true);
            if (_currentMenuState == state)
            {
                return;
            }

            _currentMenuState = state;
            RefreshMenuVisuals();
        }

        /// <summary>
        /// Closes a menu.
        /// </summary>
        public void CloseMenu()
        {
            SetMenuVisible(false);
        }

        /// <summary>
        /// Creates a confirmation popup for exiting the application.
        /// </summary>
        public void ExitApp()
        {
            if (_activeConfirmation == null)
            {
                _activeConfirmation = Instantiate(
                    _quitConfirmationPrefab, _canvas.transform);
                MenuConfirmation mc = _activeConfirmation.GetComponent<MenuConfirmation>();
                mc.AddApproveCallback(
                    delegate
                    {
                        Application.Quit();
                    });
            }
        }

        /// <summary>
        /// Creates a confirmatin popup for launching the home experience.
        /// </summary>
        public void HomePressed()
        {
            ExperienceButtonClicked(_homeSettings);
        }

        /// <summary>Handles UI change of debug mode.</summary>
        /// <param name="on">Turn debug mode on.</param>
        public void HandleDebugMode(bool on)
        {
            _debugMode.Value = on;
        }

        /// <summary>Handles UI change of use hand mesh.</summary>
        /// <param name="on">Turn hand mesh on.</param>
        public void HandleUseHandMesh(bool on)
        {
            _useHandMesh.Value = on;
        }

        /// <summary>ExperienceButton calls this when clicked.</summary>
        /// <param name="settings">Settings of button who clicked.</param>
        public void ExperienceButtonClicked(ExperienceSettings settings)
        {
            if (_activeConfirmation == null)
            {
                _activeConfirmation = Instantiate(
                    _experienceConfirmationPrefab, _canvas.transform);
                MenuConfirmation mc = _activeConfirmation.GetComponent<MenuConfirmation>();
                mc.SetTitleText(settings.ExperienceName);
                mc.SetDescriptionText(settings.Description);
                mc.SetImage(settings.Sprite);
                mc.AddFeatures(settings.Features);
                mc.AddApproveCallback(
                    delegate
                    {
                        InitializeExperienceLoad(settings);
                    });
            }
        }

        private void HandleDebugModeVariableChange(bool on)
        {
            _debugModeToggle.isOn = on;
        }

        private void HandleUseHandMeshVariableChange(bool on)
        {
            _useHandMeshToggle.isOn = on;
        }

        private void HandPerformanceMetricsToggle(bool on)
        {
            _performanceMetricsOverlay.SetActive(on);
        }

        private int FoveationLevelToInt(float level)
        {
            switch (level)
            {
                case 0f:
                    return 0;
                case 0.25f:
                    return 1;
                case 0.5f:
                    return 2;
                case 1f:
                    return 3;
            }

            Debug.LogError($"Unknown foveation level: {level}");
            return 0;
        }

        private float FoveationLevelFromInt(int level)
        {
            switch (level)
            {
                case 0:
                    return 0f;
                case 1:
                    return 0.25f;
                case 2:
                    return 0.5f;
                case 3:
                    return 1f;
            }

            Debug.LogError($"Invalid foveation level: {level}");
            return 0;
        }

        private void HandleFoveationLevelSliderChange(float sliderValue)
        {
            float level = FoveationLevelFromInt((int)sliderValue);
            Singleton.Instance.OriginManager.FoveationController.FoveationLevel = level;
        }

        private void Start()
        {
            // Setup button actions.
            _experiencesButton.OnPress.AddListener(
                delegate { SetMenuState(MenuState.Experiences); });
            _settingsButton.OnPress.AddListener(
                delegate { SetMenuState(MenuState.Settings); });

            _closeButton.OnPress.AddListener(CloseMenu);
            _exitAppButton.OnPress.AddListener(ExitApp);
            _homeButton.OnPress.AddListener(HomePressed);

            // Create a list of groups.
            List<string> groups = new List<string>();
            for (int i = 0; i < _settingsResource.Experiences.Length; ++i)
            {
                if (!groups.Contains(_settingsResource.Experiences[i].ExperienceGroup))
                {
                    groups.Add(_settingsResource.Experiences[i].ExperienceGroup);
                }
            }

            // Create groups and buttons associated with the groups.
            for (int i = 0; i < groups.Count; ++i)
            {
                // Create the group.
                GameObject groupObject = Instantiate(
                    _experienceButtonGroupPrefab, _experiencesScrollParent.transform);
                ExperienceButtonGroup group = groupObject.GetComponent<ExperienceButtonGroup>();
                group.SetTitleText(groups[i]);
                int colorIndex = i % _experienceButtonGroupColors.Length;
                group.SetColor(_experienceButtonGroupColors[colorIndex]);

                // Add all associated buttons.
                for (int j = 0; j < _settingsResource.Experiences.Length; ++j)
                {
                    ExperienceSettings exp = _settingsResource.Experiences[j];
                    if (exp.ExperienceGroup == groups[i])
                    {
                        GameObject buttonObject = Instantiate(
                            _experienceButtonPrefab, _experiencesScrollParent.transform);
                        buttonObject.name = exp.ExperienceName;
                        group.AddChildButton(buttonObject.GetComponent<RectTransform>());

                        ExperienceButton eb = buttonObject.GetComponent<ExperienceButton>();
                        eb.Init(this, exp);
                    }
                }
            }

            // Default to experiences state, and closed.
            _currentMenuState = MenuState.Experiences;
            RefreshMenuVisuals();
            _showHideT = 0.0f;
            SetMenuVisibility(_showHideT);
            _currentVisibilityState = VisibilityState.Hidden;
            _mesh.SetActive(false);
            _canvas.interactable = false;
            _canvas.blocksRaycasts = false;
            _menuIsOpen.Value = false;

            // Init toggle states.
            // Debug mode.
            _debugModeToggle.isOn = _debugMode.Value;
            _debugModeToggle.onValueChanged.AddListener(HandleDebugMode);
            _debugMode.AddListener(HandleDebugModeVariableChange);

            // Use hand mesh.
            _useHandMeshToggle.isOn = _useHandMesh.Value;
            _useHandMeshToggle.onValueChanged.AddListener(HandleUseHandMesh);
            _useHandMesh.AddListener(HandleUseHandMeshVariableChange);

            // Performance metrics.
            _showPerformanceMetricsToggle.isOn = false;
            _showPerformanceMetricsToggle.onValueChanged.AddListener(HandPerformanceMetricsToggle);
            _performanceMetricsOverlay.SetActive(false);

            // Init slider state.
            _foveationLevelSlider.onValueChanged.AddListener(HandleFoveationLevelSliderChange);
            _foveationLevelSlider.value = FoveationLevelToInt(
                Singleton.Instance.OriginManager.FoveationController.FoveationLevel);
            HandleFoveationLevelSliderChange(_foveationLevelSlider.value);

            // Update available experiences based on input mode.
            XRInputModalityManager.currentInputMode.SubscribeAndUpdate(HandleInputModeChange);
            HandleInputModeChange(XRInputModalityManager.currentInputMode.Value);
        }

        private void OnDestroy()
        {
            _debugMode.RemoveListener(HandleDebugModeVariableChange);
            _debugModeToggle.onValueChanged.RemoveListener(HandleDebugMode);
            _useHandMesh.RemoveListener(HandleUseHandMeshVariableChange);
            _useHandMeshToggle.onValueChanged.RemoveListener(HandleUseHandMesh);
            _showPerformanceMetricsToggle.onValueChanged.RemoveListener(
                HandleUseHandMeshVariableChange);
        }

        private void Update()
        {
            // Poll for summons.
            if (_dominantHandManager.GetMenuButtonPressedThisFrame() && !_loadingExperience)
            {
                SetMenuVisible(true);

                // Requesting the menu while visible always repositions it.
                RepositionMenu();
            }

            switch (_currentVisibilityState)
            {
            case VisibilityState.TransitionToShowing:
                {
                    // _showHideValue range is [0:1]
                    _showHideT += Time.deltaTime * _showHideSpeed;
                    _showHideT = Mathf.Min(_showHideT, 1.0f);

                    // _showHideCurve modifies _showHideValue to smooth is out.
                    float curveT = _showHideCurve.Evaluate(_showHideT);

                    // Finally, map the curve value to [_showHideMinimumValue:1].
                    float lerpT = Mathf.Lerp(_showHideMinimumValue, 1.0f, curveT);
                    SetMenuVisibility(_showHideT);
                    transform.localScale = Vector3.one * lerpT;
                    if (_showHideT >= 1.0f)
                    {
                        _currentVisibilityState = VisibilityState.Showing;
                        _canvas.interactable = true;
                        _canvas.blocksRaycasts = true;
                        _menuIsOpen.Value = true;
                    }

                    HandleInputModeChange(_inputMode.Value);
                }

                break;
            case VisibilityState.Showing:
                {
                    // Update tracking "lights".
                    bool l = _leftHandTrackedAction.action.IsPressed();
                    _leftHandTrackingVisual.color = l ? _trackingOnColor : _trackingOffColor;
                    _leftHandTrackingText.text = $"Left hand {(l ? "" : "not")} tracked";

                    bool r = _rightHandTrackedAction.action.IsPressed();
                    _rightHandTrackingVisual.color = r ? _trackingOnColor : _trackingOffColor;
                    _rightHandTrackingText.text = $"Right hand {(r ? "" : "not")} tracked";

                    bool e = (InputTrackingState)_eyeTrackedAction.action.ReadValue<int>() !=
                        InputTrackingState.None;
                    _eyeTrackingVisual.color = e ? _trackingOnColor : _trackingOffColor;
                    _eyeTrackingText.text = $"Eyes {(e ? "" : "not")} tracked";
                }

                break;
            case VisibilityState.TransitionToExperience:
            case VisibilityState.TransitionToHidden:
                {
                    // Always keep the menu not interactable when it's hiding.
                    // This prevents misclicks during animations.
                    _canvas.interactable = false;
                    _canvas.blocksRaycasts = false;
                    _showHideT -= Time.deltaTime * _showHideSpeed;
                    _showHideT = Mathf.Max(_showHideT, 0.0f);
                    float curveT = _showHideCurve.Evaluate(_showHideT);
                    float lerpT = Mathf.Lerp(_showHideMinimumValue, 1.0f, curveT);
                    SetMenuVisibility(_showHideT);
                    transform.localScale = Vector3.one * lerpT;
                    if (_showHideT <= 0.0f)
                    {
                        // After the menu is dismissed, load our scene.
                        if (_loadingExperience)
                        {
                            Singleton.Instance.SceneManager.LoadScene(
                                _loadExperienceSettings.ScenePath);
                        }

                        _currentVisibilityState = VisibilityState.Hidden;
                        _mesh.SetActive(false);
                        _menuIsOpen.Value = false;
                    }
                }

                break;
            }

            if (Keyboard.current[Key.D].wasPressedThisFrame)
            {
                _debugMode.Value = !_debugMode.Value;
            }
        }

        private void SetMenuVisibility(float t)
        {
            _canvas.alpha = t;
            Color c = _mesh.GetComponent<Renderer>().material.color;
            c.a = t;
            _mesh.GetComponent<Renderer>().material.color = c;
        }

        private void SetMenuVisible(bool visible)
        {
            if (visible)
            {
                if (_currentVisibilityState == VisibilityState.Hidden ||
                    _currentVisibilityState == VisibilityState.TransitionToHidden)
                {
                    Singleton.Instance.Audio.PlayMenuSummon(transform.position);
                    _currentVisibilityState = VisibilityState.TransitionToShowing;
                    _mesh.SetActive(true);
                    _inputCapture.SetActive(false);
                    _homeButton.SetButtonDisabled(Singleton.Instance.SceneManager.IsAtHome);
                }
            }
            else
            {
                if (_currentVisibilityState == VisibilityState.Showing ||
                    _currentVisibilityState == VisibilityState.TransitionToShowing)
                {
                    Singleton.Instance.Audio.PlayMenuDismiss(transform.position);
                    _currentVisibilityState = VisibilityState.TransitionToHidden;
                }
            }
        }

        private void RefreshMenuVisuals()
        {
            // Default to everything off.
            _experiencesMenu.SetActive(false);
            _settingsMenu.SetActive(false);

            switch (_currentMenuState)
            {
            case MenuState.Experiences:
                _experiencesMenu.SetActive(true);
                break;
            case MenuState.Settings:
                _settingsMenu.SetActive(true);
                break;
            }
        }

        private void RepositionMenu()
        {
            Transform camXf = Singleton.Instance.XROrigin.Camera.transform;
            Vector3 menuPos = camXf.position;
            Vector3 menuOffset = camXf.forward;
            menuOffset.y = 0.0f;
            menuOffset.Normalize();

            transform.position = menuPos + menuOffset * _spawnDistance;
            transform.forward = menuOffset;
        }

        private void InitializeExperienceLoad(ExperienceSettings settings)
        {
            _currentVisibilityState = VisibilityState.TransitionToExperience;
            _loadExperienceSettings = settings;
            _inputCapture.SetActive(true);
            if (StartTransitionToExperience != null)
            {
                StartTransitionToExperience.Invoke();
            }
        }

        private void HandleInputModeChange(XRInputModalityManager.InputMode inputMode)
        {
            if (inputMode == _inputMode.Value)
            {
                return;
            }

            Debug.Log($"Input mode changed to: {inputMode}");
            _inputMode.Value = inputMode;
        }
    }
}
