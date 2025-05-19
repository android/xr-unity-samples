// <copyright file="EyeCreature.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.CreatureGaze
{
    /// <summary>
    /// Controller for eye creature character logic.
    /// </summary>
    public class EyeCreature : MonoBehaviour
    {
        [Header("Eyes")]
        [SerializeField] private Transform _leftEye;
        [SerializeField] private Transform _rightEye;
        [SerializeField] private Transform _leftEyeAttach;
        [SerializeField] private Transform _rightEyeAttach;
        [SerializeField] private float _maxAngleFromForward;

        [Header("Animation")]
        [SerializeField] private Animator _animator;

        [Header("Physics")]
        [SerializeField] private float _springK;
        [SerializeField] private float _springDampen;
        [SerializeField] private float _homeAdjustSpeed;

        [Header("Blink")]
        [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
        [SerializeField] private Vector2 _blinkIntervalRange;
        [SerializeField] private float _blinkSpeed;

        [Header("Look Customization")]
        [SerializeField] private Transform[] _scaleTransforms;
        [SerializeField] private DistributionRange _scaleDelay;
        [SerializeField] private AnimationCurve _scaleInCurve;
        [SerializeField] private float _scaleInSpeed;
        [SerializeField] private float _scaleOutSpeed;
        [SerializeField] private DistributionRange _scaleRange;
        [SerializeField] private DistributionRange _eyeTrackSpeedRange;
        [SerializeField] private DistributionRange _eyeTrackDelayRange;

        [Header("Color Customization")]
        [SerializeField] private Colorway[] _colorWays;
        [SerializeField] private Color[] _eyeColors;
        [SerializeField] private Renderer _bodyRenderer;
        [SerializeField] private int _bodyMaterialIndex;
        [SerializeField] private Renderer[] _wingRenderers;
        [SerializeField] private Renderer[] _eyeRenderers;
        [SerializeField] private int _eyeMaterialIndex;

        [Header("Audio")]
        [SerializeField] private AudioClipData _spawnAudio;
        [SerializeField] private EyeCreatureAnimationAudio _wingFlapAudio;

        private State _currentState;
        private float _scaleTimer;

        private Quaternion[] _previousEyeRotationsLeft;
        private Quaternion[] _previousEyeRotationsRight;
        private int _previousEyeTargetIndex;
        private Quaternion _eyeRotationLeftLS;
        private Quaternion _eyeRotationRightLS;
        private float _eyeTrackSpeed;
        private float _eyeTrackDelay;

        private bool _blinking;
        private float _blinkTimer;
        private float _blinkValue;

        private Vector3 _velocity;
        private Vector3 _springHome;

        private enum State
        {
            Hidden,
            WaitingToShow,
            Showing,
            Visible,
            Hiding,
        }

        /// <summary>
        /// Accessor for getting the spring home.
        /// </summary>
        public Vector3 SpringHome => _springHome;

        /// <summary>
        /// Function for adjusting the velocity.
        /// </summary>
        /// <param name="vel">Value to add to velocity.</param>
        public void AddToVelocity(Vector3 vel)
        {
            _velocity += vel;
        }

        /// <summary>
        /// Function for updating physics. This is called from our scene manager so all
        /// creatures are guaranteed to be updated at the same time.
        /// </summary>
        public void UpdatePhysics()
        {
            Vector3 toHome = _springHome - transform.position;
            Vector3 dampenedVel = _velocity * _springDampen;
            Vector3 springForce = (toHome * _springK) - dampenedVel;
            _velocity += springForce;

            Vector3 pos = transform.position;
            pos += (_velocity * Time.deltaTime);
            transform.position = pos;

            // Slowwwwly move our home toward our current position.
            Vector3 homeToUs = pos - _springHome;
            _springHome += homeToUs.normalized * _homeAdjustSpeed * Time.deltaTime;
        }

        /// <summary>
        /// Function for updating eye orientations from ARFaceManager.
        /// </summary>
        /// <param name="leftEyeRotation">Rotations for the left eye in local space.</param>
        /// <param name="rightEyeRotation">Rotations for the right eye in local space.</param>
        public void UpdateEyes(Quaternion leftEyeRotation, Quaternion rightEyeRotation)
        {
            _eyeRotationLeftLS = leftEyeRotation;
            _eyeRotationRightLS = rightEyeRotation;
        }

        /// <summary>
        /// Function for hard setting the spawn delay.
        /// </summary>
        /// <param name="delay">The delay value.</param>
        public void SetSpawnDelay(float delay)
        {
            _scaleTimer = delay;
        }

        /// <summary>
        /// Function for showing and hiding the creature when the menu is enabled.
        /// </summary>
        /// <param name="hide">Whether to hide or not.</param>
        public void HideForMenu(bool hide)
        {
            if (hide)
            {
                if (_currentState == State.WaitingToShow)
                {
                    _currentState = State.Hidden;
                }

                if (_currentState == State.Showing || _currentState == State.Visible)
                {
                    _currentState = State.Hiding;
                }
            }
            else
            {
                if (_currentState == State.Hiding)
                {
                    _currentState = State.Showing;
                }
                else if (_currentState == State.Hidden)
                {
                    _scaleTimer = _scaleDelay.GetValue() * 0.5f;
                    _currentState = State.WaitingToShow;
                }
            }
        }

        private void Awake()
        {
            _springHome = transform.position;

            // Randomize animation start time.
            _animator.SetFloat("Offset", UnityEngine.Random.value);

            // Pick a random scale and set our scale to 0 for now.
            float scale = _scaleRange.GetValue();
            for (int i = 0; i < _scaleTransforms.Length; ++i)
            {
                _scaleTransforms[i].localScale *= scale;
            }

            transform.localScale = Vector3.zero;
            _scaleTimer = _scaleDelay.GetValue();

            // Animation speed is inversely proporational to scale.
            // (little guys flap their wings fast, big ones flap slow)
            _animator.speed = 1.0f / scale;
            _animator.speed = Mathf.Pow(_animator.speed, 1.75f);

            // Pitch wing flap up a little if we're smaller.
            float scaleT = Mathf.InverseLerp(_scaleRange.Range.x, _scaleRange.Range.y, scale);
            _wingFlapAudio.SetPitch(Mathf.Lerp(1.5f, 1.0f, scaleT));

            // Randomize eye track speed and delay.
            _eyeTrackSpeed = _eyeTrackSpeedRange.GetValue();
            _eyeTrackDelay = _eyeTrackDelayRange.GetValue();
            InitializeDelay();

            // Randomize body color.
            Colorway c = _colorWays[UnityEngine.Random.Range(0, _colorWays.Length)];
            Material[] bodyMats = _bodyRenderer.materials;
            bodyMats[_bodyMaterialIndex].color = c.Body;
            _bodyRenderer.materials = bodyMats;
            for (int i = 0; i < _wingRenderers.Length; ++i)
            {
                _wingRenderers[i].material.color = c.Wings;
            }

            // Randomize eye color.
            Color eyeColor = _eyeColors[UnityEngine.Random.Range(0, _eyeColors.Length)];
            for (int i = 0; i < _eyeRenderers.Length; ++i)
            {
                Material[] eyeMats = _eyeRenderers[i].materials;
                eyeMats[_eyeMaterialIndex].color = eyeColor;
                _eyeRenderers[i].materials = eyeMats;
            }

            // Start blink timer.
            _blinkTimer = UnityEngine.Random.Range(
                _blinkIntervalRange.x, _blinkIntervalRange.y);

            _wingFlapAudio.SetEnabled(false);

            _currentState = State.WaitingToShow;
        }

        private void InitializeDelay()
        {
            // Assume we're running at 90hz to calculate how many previous frames we need.
            int numPreviousSteps = Mathf.Max((int)(_eyeTrackDelay / (1.0f / 90.0f)), 1);
            _previousEyeRotationsLeft = new Quaternion[numPreviousSteps];
            _previousEyeRotationsRight = new Quaternion[numPreviousSteps];
            _previousEyeTargetIndex = 0;
        }

        private void LateUpdate()
        {
            // Scaling state machine.
            switch (_currentState)
            {
            case State.WaitingToShow:
                _scaleTimer -= Time.deltaTime;
                if (_scaleTimer <= 0.0f)
                {
                    _currentState = State.Showing;
                    _scaleTimer = 0.0f;
                    Singleton.Instance.Audio.PlayOneShot(_spawnAudio, transform.position);
                }

                break;
            case State.Showing:
                {
                    _scaleTimer += Time.deltaTime * _scaleInSpeed;
                    _scaleTimer = Mathf.Min(_scaleTimer, 1.0f);
                    if (_scaleTimer >= 1.0f)
                    {
                        _currentState = State.Visible;
                        _wingFlapAudio.SetEnabled(true);
                    }

                    float scaleT = _scaleInCurve.Evaluate(_scaleTimer);
                    transform.localScale = Vector3.one * scaleT;
                }

                break;
            case State.Hiding:
                {
                    _scaleTimer -= Time.deltaTime * _scaleOutSpeed;
                    _scaleTimer = Mathf.Max(_scaleTimer, 0.0f);
                    if (_scaleTimer <= 0.0f)
                    {
                        _currentState = State.Hidden;
                        _wingFlapAudio.SetEnabled(false);
                    }

                    transform.localScale = Vector3.one * _scaleTimer;
                }

                break;
            }

            // Keep eyes in the sockets.
            _leftEye.position = _leftEyeAttach.position;
            _rightEye.position = _rightEyeAttach.position;

            // Store current target in our queue and pluck off the next in line for acting on.
            _previousEyeRotationsLeft[_previousEyeTargetIndex] = _eyeRotationLeftLS;
            _previousEyeRotationsRight[_previousEyeTargetIndex] = _eyeRotationRightLS;
            ++_previousEyeTargetIndex;
            _previousEyeTargetIndex %= _previousEyeRotationsLeft.Length;
            Quaternion frameRotationLeftLS = _previousEyeRotationsLeft[_previousEyeTargetIndex];
            Quaternion frameRotationRightLS = _previousEyeRotationsRight[_previousEyeTargetIndex];

            // Move eyes.
            Vector3 desiredLeftFwd = frameRotationLeftLS * Vector3.forward;
            TrackTowardDesiredForward(_leftEye, desiredLeftFwd);

            Vector3 desiredRightFwd = frameRotationRightLS * Vector3.forward;
            TrackTowardDesiredForward(_rightEye, desiredRightFwd);

            // Update blink logic.
            if (_blinking)
            {
                _blinkValue += Time.deltaTime * _blinkSpeed;
                _blinkValue = Mathf.Clamp01(_blinkValue);
                _skinnedMeshRenderer.SetBlendShapeWeight(
                    0, Mathf.Sin(_blinkValue * Mathf.PI) * 100.0f);

                if (_blinkValue >= 1.0f)
                {
                    _blinking = false;
                    _blinkTimer = UnityEngine.Random.Range(
                        _blinkIntervalRange.x, _blinkIntervalRange.y);
                }
            }
            else
            {
                _blinkTimer -= Time.deltaTime;
                if (_blinkTimer <= 0.0f)
                {
                    _blinking = true;
                    _blinkValue = 0.0f;
                }
            }

            // Face user.
            Vector3 toUserNoY = Singleton.Instance.Camera.transform.position - transform.position;
            toUserNoY.y = 0.0f;
            transform.rotation = Quaternion.LookRotation(toUserNoY.normalized);
        }

        private void TrackTowardDesiredForward(Transform eyeXf, Vector3 forwardLS)
        {
            // Clamp desired forward.
            float angleToFwd = Vector3.Angle(Vector3.forward, forwardLS);
            if (angleToFwd > _maxAngleFromForward)
            {
                Vector3 rotationAxis = Vector3.Cross(Vector3.forward, forwardLS);
                Quaternion clampedRot = Quaternion.AngleAxis(_maxAngleFromForward, rotationAxis);
                forwardLS = clampedRot * Vector3.forward;
            }

            Vector3 forwardWS = transform.TransformDirection(forwardLS);
            float angleToDesired = Vector3.Angle(eyeXf.forward, forwardWS);
            float trackStep = _eyeTrackSpeed * Time.deltaTime;
            if (trackStep >= angleToDesired)
            {
                eyeXf.forward = forwardWS;
            }
            else
            {
                float stepInRads = Mathf.Deg2Rad * trackStep;
                Vector3 newFwd = Vector3.RotateTowards(eyeXf.forward, forwardWS, stepInRads, 1.0f);
                eyeXf.forward = newFwd.normalized;
            }
        }
    }

    /// <summary>
    /// Class for defining a range and a distribution map across that range.
    /// </summary>
    [Serializable]
    public class DistributionRange
    {
        /// <summary> The value range. </summary>
        public Vector2 Range;

        /// <summary> The distribution curve. </summary>
        public AnimationCurve Distribution;

        /// <summary> Method for getting a new value. </summary>
        /// <returns> A new, randomly distributed value. </summary>
        public float GetValue()
        {
            float t = UnityEngine.Random.value;
            float distributedT = Distribution.Evaluate(t);
            return Mathf.Lerp(Range.x, Range.y, distributedT);
        }
    }

    /// <summary>
    /// Class for a creature color way.
    /// </summary>
    [Serializable]
    public class Colorway
    {
        /// <summary> The color of the body. </summary>
        public Color Body;

        /// <summary> The color of the wings. </summary>
        public Color Wings;
    }
}
