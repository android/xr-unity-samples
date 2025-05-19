// <copyright file="TalkingObjects.cs" company="Google LLC">
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

using AndroidXRUnitySamples.MenusAndUI;
using UnityEngine;
using UnityEngine.UI;

namespace AndroidXRUnitySamples.TalkingObjects
{
    /// <summary>
    /// Controller of the TalkingObjects scene.
    /// </summary>
    public class TalkingObjects : MonoBehaviour
    {
        [SerializeField] private Transform _carousel;
        [SerializeField] private ShadowButton _nextPuppet;
        [SerializeField] private ShadowButton _prevPuppet;
        [SerializeField] private Transform[] _puppets;
        [SerializeField] private FaceTrackingRecorder _faceRecorder;

        [Header("Transitions")]
        [SerializeField] private float _transitionSpeed;
        [SerializeField] private float _trackingDelay;
        [SerializeField] private AnimationCurve _transitionCurve;
        [SerializeField] private AnimationCurve _scaleCurve;
        [SerializeField] private Transform _baseXf;
        [SerializeField] private Transform _prevXf;
        [SerializeField] private Transform _nextXf;

        [Header("Recording")]
        [SerializeField] private ShadowButton _record;
        [SerializeField] private ShadowButton _play;
        [SerializeField] private ShadowButton _stop;
        [SerializeField] private RectTransform _playbackBar;
        [SerializeField] private Image _playbackProgress;
        [SerializeField] private Image _playbackHead;
        [SerializeField] private GameObject _recordActiveIcon;
        [SerializeField] private Color _recordColor;
        [SerializeField] private Color _playbackColor;
        [SerializeField] private Color _disabledColor;

        private int _activePuppetIndex;
        private int _transitionPuppetIndex;
        private bool _transitionNext;
        private State _currentState;
        private float _stateTimer;

        private enum State
        {
            TrackingDelay,
            Tracking,
            Transitioning,
        }

        private void Start()
        {
            Singleton.Instance.OriginManager.EnablePassthrough = true;

            _nextPuppet.OnPress.AddListener(NextPuppet);
            _prevPuppet.OnPress.AddListener(PreviousPuppet);

            _record.OnPress.AddListener(StartRecording);
            _play.OnPress.AddListener(StartPlayback);
            _stop.OnPress.AddListener(StopPlayback);

            _currentState = State.TrackingDelay;
            _activePuppetIndex = 0;
            _transitionPuppetIndex = 0;
            SetPuppetActive(_activePuppetIndex);
        }

        private void SetPuppetActive(int index)
        {
            // Disable all puppets except the one we want.
            for (int i = 0; i < _puppets.Length; ++i)
            {
                _puppets[i].gameObject.SetActive(i == index);
            }

            // Set the puppet to be updated by the face tracker.
            SkinnedMeshRenderer smr =
                _puppets[index].GetComponentInChildren<SkinnedMeshRenderer>();
            _faceRecorder.SetTargetSkinnedMeshRenderer(smr);

            // Place puppet.
            _puppets[index].localPosition = _baseXf.localPosition;
            _puppets[index].localRotation = _baseXf.localRotation;
        }

        private void NextPuppet()
        {
            if (_currentState != State.Transitioning)
            {
                _transitionPuppetIndex = _activePuppetIndex + 1;
                _transitionPuppetIndex %= _puppets.Length;
                _stateTimer = 0.0f;
                _puppets[_transitionPuppetIndex].gameObject.SetActive(true);
                _puppets[_transitionPuppetIndex].localScale = Vector3.zero;
                _transitionNext = true;
                _currentState = State.Transitioning;
            }
        }

        private void PreviousPuppet()
        {
            if (_currentState != State.Transitioning)
            {
                _transitionPuppetIndex = _activePuppetIndex - 1;
                if (_transitionPuppetIndex < 0)
                {
                    _transitionPuppetIndex = _puppets.Length - 1;
                }

                _puppets[_transitionPuppetIndex].gameObject.SetActive(true);
                _puppets[_transitionPuppetIndex].localScale = Vector3.zero;
                _transitionNext = false;
                _stateTimer = 0.0f;
                _currentState = State.Transitioning;
            }
        }

        private void Update()
        {
            switch (_currentState)
            {
            case State.TrackingDelay:
                _stateTimer += Time.deltaTime;
                _stateTimer = Mathf.Min(_stateTimer, _trackingDelay);
                _faceRecorder.SetLiveDataWeight(_stateTimer / _trackingDelay);
                if (_stateTimer >= _trackingDelay)
                {
                    _currentState = State.Tracking;
                }

                break;
            case State.Transitioning:
                _stateTimer += Time.deltaTime * _transitionSpeed;
                _stateTimer = Mathf.Clamp01(_stateTimer);

                float t = _transitionCurve.Evaluate(_stateTimer);
                float scaleT = _scaleCurve.Evaluate(_stateTimer);
                float scale1mT = _scaleCurve.Evaluate(1.0f - _stateTimer);
                if (_transitionNext)
                {
                    _puppets[_activePuppetIndex].localPosition =
                        Vector3.Lerp(_baseXf.localPosition, _prevXf.localPosition, t);
                    _puppets[_activePuppetIndex].localRotation =
                        Quaternion.Lerp(_baseXf.localRotation, _prevXf.localRotation, t);
                    _puppets[_activePuppetIndex].localScale = Vector3.one * scale1mT;

                    _puppets[_transitionPuppetIndex].localPosition =
                        Vector3.Lerp(_nextXf.localPosition, _baseXf.localPosition, t);
                    _puppets[_transitionPuppetIndex].localRotation =
                        Quaternion.Lerp(_nextXf.localRotation, _baseXf.localRotation, t);
                    _puppets[_transitionPuppetIndex].localScale = Vector3.one * scaleT;
                }
                else
                {
                    _puppets[_activePuppetIndex].localPosition =
                        Vector3.Lerp(_baseXf.localPosition, _nextXf.localPosition, t);
                    _puppets[_activePuppetIndex].localRotation =
                        Quaternion.Lerp(_baseXf.localRotation, _nextXf.localRotation, t);
                    _puppets[_activePuppetIndex].localScale = Vector3.one * scale1mT;

                    _puppets[_transitionPuppetIndex].localPosition =
                        Vector3.Lerp(_prevXf.localPosition, _baseXf.localPosition, t);
                    _puppets[_transitionPuppetIndex].localRotation =
                        Quaternion.Lerp(_prevXf.localRotation, _baseXf.localRotation, t);
                    _puppets[_transitionPuppetIndex].localScale = Vector3.one * scaleT;
                }

                if (_stateTimer >= 1.0f)
                {
                    _stateTimer = 0.0f;
                    _activePuppetIndex = _transitionPuppetIndex;
                    SetPuppetActive(_activePuppetIndex);
                    _currentState = State.TrackingDelay;
                }

                break;
            }

            // Update controls UI status by polling face recording state.
            _nextPuppet.SetButtonDisabled(_faceRecorder.Recording);
            _prevPuppet.SetButtonDisabled(_faceRecorder.Recording);
            _record.SetButtonDisabled(_faceRecorder.Recording);
            _play.SetButtonDisabled(!_faceRecorder.HasRecordedData || _faceRecorder.Recording ||
                (!_faceRecorder.Recording && !_faceRecorder.UsingLiveData));
            _stop.SetButtonDisabled(_faceRecorder.UsingLiveData || _faceRecorder.Recording);
            _recordActiveIcon.SetActive(_faceRecorder.Recording);

            Color barColor = _disabledColor;
            float progress = 0.0f;
            if (_faceRecorder.Recording)
            {
                progress = _faceRecorder.RecordingPercent;
                barColor = _recordColor;
            }
            else if (!_faceRecorder.UsingLiveData)
            {
                progress = _faceRecorder.PlaybackPercent;
                barColor = _playbackColor;
            }

            Vector2 barSize = _playbackProgress.rectTransform.sizeDelta;
            barSize.x = _playbackBar.sizeDelta.x * progress;
            _playbackProgress.rectTransform.sizeDelta = barSize;

            Vector2 headPos = _playbackHead.rectTransform.anchoredPosition;
            headPos.x = _playbackBar.sizeDelta.x * progress;
            _playbackHead.rectTransform.anchoredPosition = headPos;

            _playbackProgress.color = barColor;
            _playbackHead.color = barColor;
        }

        private void StartRecording()
        {
            _faceRecorder.BeginRecording();
        }

        private void StopPlayback()
        {
            _faceRecorder.UseLiveData(true);
        }

        private void StartPlayback()
        {
            _faceRecorder.UseLiveData(false);
        }
    }
}
