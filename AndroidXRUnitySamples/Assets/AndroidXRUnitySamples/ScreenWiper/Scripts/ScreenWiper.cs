// <copyright file="ScreenWiper.cs" company="Google LLC">
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
using UnityEngine.InputSystem;

namespace AndroidXRUnitySamples.ScreenWiper
{
    /// <summary>
    /// The manager for the Screen Wiper experience.
    /// </summary>
    public class ScreenWiper : MonoBehaviour
    {
        [SerializeField] private Transform _rightHandLight;
        [SerializeField] private Transform _leftHandLight;
        [SerializeField] private InputActionProperty _rightHandRayPosition;
        [SerializeField] private InputActionProperty _rightHandRayRotation;
        [SerializeField] private InputActionProperty _leftHandRayPosition;
        [SerializeField] private InputActionProperty _leftHandRayRotation;
        [SerializeField] private WiperCursor _rightHandPainterCursor;
        [SerializeField] private WiperCursor _leftHandPainterCursor;
        [SerializeField] private Transform _passthroughSphere;
        [SerializeField] private Material _sphereMat;
        [SerializeField] private InputActionProperty _rightHandPinch;
        [SerializeField] private InputActionProperty _leftHandPinch;

        [Header("Audio")]
        [SerializeField] private AudioSource _ambientLoop;
        [SerializeField] private float _ambientLoopVolChangeSpeed;
        [SerializeField] private Vector2 _ambientLoopVolume;
        [SerializeField] private Vector2 _ambientLoopRevealRange;

        [Header("States")]
        [SerializeField] private float _introDelay;
        [SerializeField] private float _introWipeDuration;
        [SerializeField] private float _introWipeInitialOffsetY;
        [SerializeField] private AnimationCurve _introWipeCurve;
        [SerializeField] private Texture2D _introWipeTexture;

        [Header("Animation")]
        [SerializeField] private float _stampAngle;
        [SerializeField] private float _fadeSpeed;
        [SerializeField] private float _particleTriggerThreshold;

        [Header("Cursor")]
        [SerializeField] private float _cursorAdjustment;
        [SerializeField] private float _pinchStrengthMin;
        [SerializeField] private float _pinchDisabledDelay;

        [Header("Debug")]
        [SerializeField] private bool _debugSpoofRightHand;
        [SerializeField] private bool _debugGizmoDraw;

        private int _maskTextureRes;
        private Texture2D _maskTexture;
        private float[] _maskValues;
        private float _pinchDisabledTimer;

        private State _currentState;
        private float _introDelayTimer;
        private float _introWipeTimer;

        private float _revealPercent;

        private Vector3 _debugPointLeft;
        private Vector3 _debugPointRight;
        private Vector3[] _debugPointBounds;

        private enum State
        {
            Intro,
            Painting,
        }

        void Start()
        {
            _maskTextureRes = 128;
            _maskValues = new float[_maskTextureRes * _maskTextureRes];
            _maskTexture = new Texture2D(
                _maskTextureRes, _maskTextureRes, TextureFormat.RFloat, false, false);

            // Initialize texture to full opacity.
            for (int i = 0; i < _maskValues.Length; ++i)
            {
                _maskValues[i] = 1.0f;
            }

            UpdateMaskTexture();

            _passthroughSphere.position = Singleton.Instance.Camera.transform.position;
            _pinchDisabledTimer = 0.0f;
            SetState(State.Intro);

            _debugPointBounds = new Vector3[4];

            Singleton.Instance.OriginManager.EnablePassthrough = true;

            _rightHandRayPosition.action.Enable();
            _rightHandRayRotation.action.Enable();
            _leftHandRayPosition.action.Enable();
            _leftHandRayRotation.action.Enable();
        }

        void SetState(State newState)
        {
            switch (newState)
            {
            case State.Intro:
                _rightHandPainterCursor.SetCursorShowPercentNoParticles(0.0f);
                _leftHandPainterCursor.SetCursorShowPercentNoParticles(0.0f);
                _introDelayTimer = _introDelay;
                _introWipeTimer = _introWipeDuration;
                _sphereMat.SetTexture("_MaskTex", _introWipeTexture);
                _sphereMat.SetTextureOffset("_MaskTex",
                    new Vector2(0.0f, _introWipeInitialOffsetY));
                break;
            case State.Painting:
                _sphereMat.SetTexture("_MaskTex", _maskTexture);
                break;
            }

            _currentState = newState;
        }

        void Update()
        {
            switch (_currentState)
            {
            case State.Intro:
                UpdateIntro();
                break;
            case State.Painting:
                UpdatePainting();
                break;
            }
        }

        void UpdateIntro()
        {
            _introDelayTimer -= Time.deltaTime;
            if (_introDelayTimer <= 0.0f)
            {
                _introWipeTimer -= Time.deltaTime;
                Vector2 offset = Vector2.zero;

                if (_introWipeTimer <= 0.0f)
                {
                    SetState(State.Painting);
                }
                else
                {
                    float t = _introWipeCurve.Evaluate(_introWipeTimer / _introWipeDuration);
                    offset.y = Mathf.Lerp(_introWipeInitialOffsetY, 1.0f, t);

                    // In the last second of our intro, fade in the cursors.
                    if (_introWipeTimer < 1.0f)
                    {
                        float percent = 1.0f - _introWipeTimer;
                        _rightHandPainterCursor.SetCursorShowPercentNoParticles(percent);
                        _leftHandPainterCursor.SetCursorShowPercentNoParticles(percent);

                        // Place cursors in correct spots, as well.
                        Vector3 rightPosition = _rightHandRayPosition.action.ReadValue<Vector3>();
                        Quaternion rightRotation =
                            _rightHandRayRotation.action.ReadValue<Quaternion>();
                        Vector3 rightFwd = rightRotation * Vector3.forward;
                        Ray right = new Ray(rightPosition, rightFwd);

                        Vector3? rightIntersect = IntersectionPointOnSphere(right);
                        if (rightIntersect.HasValue)
                        {
                            // Place the cursor where the intersection point is.
                            Vector3 newPos = rightIntersect.Value - (rightFwd * _cursorAdjustment);
                            _rightHandPainterCursor.transform.position = newPos;
                        }

                        Vector3 leftPosition = _leftHandRayPosition.action.ReadValue<Vector3>();
                        Quaternion leftRotation =
                            _leftHandRayRotation.action.ReadValue<Quaternion>();
                        Vector3 leftFwd = leftRotation * Vector3.forward;
                        Ray left = new Ray(leftPosition, leftFwd);

                        Vector3? leftIntersect = IntersectionPointOnSphere(left);
                        if (leftIntersect.HasValue)
                        {
                            Vector3 newPos = leftIntersect.Value - (leftFwd * _cursorAdjustment);
                            _leftHandPainterCursor.transform.position = newPos;
                        }
                    }
                }

                _sphereMat.SetTextureOffset("_MaskTex", offset);
            }

            UpdateMaskTexture();
        }

        void UpdatePainting()
        {
            // Fade texture values back toward opaque.
            _pinchDisabledTimer -= Time.deltaTime;
            if (_pinchDisabledTimer <= 0.0f)
            {
                float fadeStep = _fadeSpeed * Time.deltaTime;
                for (int i = 0; i < _maskValues.Length; ++i)
                {
                    _maskValues[i] = Mathf.Min(1.0f, _maskValues[i] + fadeStep);
                }
            }

            // Update ambient loop volume with reveal percent.
            float volumeT = Mathf.InverseLerp(
                _ambientLoopRevealRange.y, _ambientLoopRevealRange.x, _revealPercent);
            float targetVolume = Mathf.Lerp(
                _ambientLoopVolume.x, _ambientLoopVolume.y, volumeT);
            float currentVolume = _ambientLoop.volume;
            float volumeStep = Time.deltaTime * _ambientLoopVolChangeSpeed;
            if (currentVolume < targetVolume)
            {
                currentVolume += volumeStep;
                currentVolume = Mathf.Min(currentVolume, targetVolume);
            }
            else if (currentVolume > targetVolume)
            {
                currentVolume -= volumeStep;
                currentVolume = Mathf.Max(currentVolume, targetVolume);
            }

            _ambientLoop.volume = currentVolume;

            if (Singleton.Instance.Menu.Active)
            {
                // "Soft pause" the experience when the main menu is open.
                _rightHandPainterCursor.RequestEnableParticles(false);
                _rightHandPainterCursor.RequestEnableAudio(false);
                _rightHandPainterCursor.SetCursorShowPercent(1.0f);
                _leftHandPainterCursor.RequestEnableParticles(false);
                _leftHandPainterCursor.RequestEnableAudio(false);
                _leftHandPainterCursor.SetCursorShowPercent(1.0f);
            }
            else
            {
                Vector3 rightHandPosition = _rightHandRayPosition.action.ReadValue<Vector3>();
                Quaternion rightHandRotation = _rightHandRayRotation.action.ReadValue<Quaternion>();
                Vector3 rightFwd = rightHandRotation * Vector3.forward;

                Vector3 leftHandPosition = _leftHandRayPosition.action.ReadValue<Vector3>();
                Quaternion leftHandRotation = _leftHandRayRotation.action.ReadValue<Quaternion>();
                Vector3 leftFwd = leftHandRotation * Vector3.forward;

                _leftHandLight.forward = leftFwd;
                _rightHandLight.forward = rightFwd;

                bool enableRightParticlesAndAudio = false;
                bool enableLeftParticlesAndAudio = false;

                // Cast ray out from ray origins to intersect with boundary sphere.
                Ray right = new Ray(rightHandPosition, rightFwd);
                Vector3? rightIntersect = IntersectionPointOnSphere(right);
                if (rightIntersect.HasValue)
                {
                    // Place the cursor where the intersection point is.
                    Vector3 prevPos = _rightHandPainterCursor.transform.position;
                    Vector3 newPos = rightIntersect.Value - (rightFwd * _cursorAdjustment);
                    _rightHandPainterCursor.transform.position = newPos;

                    // Orient the cursor so our forward is the ray cast and our right vector
                    // is the cursor movement.
                    Vector3 prevToCurrent = newPos - prevPos;
                    Vector3 cursorUp = Vector3.Cross(rightFwd, prevToCurrent.normalized);
                    Quaternion cursorRot = Quaternion.LookRotation(rightFwd, cursorUp);
                    _rightHandPainterCursor.transform.rotation = cursorRot;

                    // Get pinch strength.
                    float pinchStrength = _debugSpoofRightHand ? 1.0f :
                        _rightHandPinch.action.ReadValue<float>();

                    // The cursor should visualize the user going from "not pressing at all"
                    // to "just before the pressing begins stamping the texture".
                    float pinchCursorPercent =
                        1.0f - Mathf.Min(pinchStrength / _pinchStrengthMin, 1.0f);
                    _rightHandPainterCursor.SetCursorShowPercent(pinchCursorPercent);

                    if (pinchStrength > _pinchStrengthMin)
                    {
                        float pinchRange = 1.0f - _pinchStrengthMin;
                        float pinchPercent = (pinchStrength - _pinchStrengthMin) / pinchRange;
                        enableRightParticlesAndAudio =
                            StampMaskTextureAtPosition(rightIntersect.Value, pinchPercent) ||
                            _debugSpoofRightHand;
                        _pinchDisabledTimer = _pinchDisabledDelay;

                        _debugPointRight = rightIntersect.Value;
                    }
                }

                _rightHandPainterCursor.RequestEnableParticles(enableRightParticlesAndAudio);
                _rightHandPainterCursor.RequestEnableAudio(enableRightParticlesAndAudio);

                Ray left = new Ray(leftHandPosition, leftFwd);
                Vector3? leftIntersect = IntersectionPointOnSphere(left);
                if (leftIntersect.HasValue)
                {
                    // Place the cursor where the intersection point is.
                    Vector3 prevPos = _leftHandPainterCursor.transform.position;
                    Vector3 newPos = leftIntersect.Value - (leftFwd * _cursorAdjustment);
                    _leftHandPainterCursor.transform.position = newPos;

                    // Orient the cursor so our forward is the ray cast and our right vector
                    // is the cursor movement.
                    Vector3 prevToCurrent = newPos - prevPos;
                    Vector3 cursorUp = Vector3.Cross(leftFwd, prevToCurrent.normalized);
                    Quaternion cursorRot = Quaternion.LookRotation(leftFwd, cursorUp);
                    _leftHandPainterCursor.transform.rotation = cursorRot;

                    float pinchStrength = _leftHandPinch.action.ReadValue<float>();

                    // The cursor should visualize the user going from "not pressing at all"
                    // to "just before the pressing begins stamping the texture".
                    float pinchCursorPercent =
                        1.0f - Mathf.Min(pinchStrength / _pinchStrengthMin, 1.0f);
                    _leftHandPainterCursor.SetCursorShowPercent(pinchCursorPercent);

                    if (pinchStrength > _pinchStrengthMin)
                    {
                        float pinchRange = 1.0f - _pinchStrengthMin;
                        float pinchPercent = (pinchStrength - _pinchStrengthMin) / pinchRange;
                        enableLeftParticlesAndAudio =
                            StampMaskTextureAtPosition(leftIntersect.Value, pinchPercent);
                        _pinchDisabledTimer = _pinchDisabledDelay;

                        _debugPointLeft = leftIntersect.Value;
                    }
                }

                _leftHandPainterCursor.RequestEnableParticles(enableLeftParticlesAndAudio);
                _leftHandPainterCursor.RequestEnableAudio(enableLeftParticlesAndAudio);
            }

            UpdateMaskTexture();
        }

        void UpdateMaskTexture()
        {
            int revealCount = 0;
            Unity.Collections.NativeArray<float> fieldVals = _maskTexture.GetPixelData<float>(0);
            for (int i = 0; i < _maskValues.Length; ++i)
            {
                fieldVals[i] = _maskValues[i];
                if (_maskValues[i] < 0.5f)
                {
                    ++revealCount;
                }
            }

            _revealPercent = (float)revealCount / (float)_maskValues.Length;

            _maskTexture.SetPixelData(fieldVals, 0);
            _maskTexture.Apply();
        }

        Vector3? IntersectionPointOnSphere(Ray r)
        {
            float sphereRadius = _passthroughSphere.localScale.x;
            Vector3 rayToSphere = _passthroughSphere.position - r.origin;
            float b = Vector3.Dot(rayToSphere, r.direction);
            float c = rayToSphere.sqrMagnitude - (sphereRadius * sphereRadius);

            float discriminant = b * b - c;

            // If the discriminant is negative, there's no intersection.
            if (discriminant < 0)
            {
                return null;
            }

            // Calculate the two possible t solutions.
            float sqrtDiscriminant = (float)Mathf.Sqrt(discriminant);
            float t1 = b - sqrtDiscriminant;
            float t2 = b + sqrtDiscriminant;

            // Calculate the intersection points.
            Vector3 intersection1 = r.origin + t1 * r.direction;
            Vector3 intersection2 = r.origin + t2 * r.direction;

            // Which one is in front of the ray?
            float dot1 = Vector3.Dot(r.direction, (intersection1 - r.origin).normalized);
            float dot2 = Vector3.Dot(r.direction, (intersection1 - r.origin).normalized);
            if (dot1 > dot2)
            {
                return intersection1;
            }

            return intersection2;
        }

        Vector2 TextureCoordsFromPos(Vector3 pos)
        {
            Vector3 centerToPoint = (pos - _passthroughSphere.position).normalized;

            // Convert point into u in range [-0.5, 0.5].
            float u = Mathf.Atan2(centerToPoint.x, centerToPoint.z) / (2.0f * Mathf.PI);
            u *= -1.0f;
            u += 0.5f;
            float v = Mathf.Asin(centerToPoint.y) / Mathf.PI + .5f;
            return new Vector2(u, v);
        }

        bool StampMaskTextureAtPosition(Vector3 spherePos, float strength)
        {
            // Get the vector from sphere center to hit position and its right hand vector.
            Vector3 toSpherePos = spherePos - _passthroughSphere.position;
            toSpherePos.Normalize();
            Vector3 toSphereRight = Vector3.Cross(Vector3.up, toSpherePos);
            toSphereRight.Normalize();
            Vector3 toSphereUp = Vector3.Cross(toSpherePos, toSphereRight);
            toSphereUp.Normalize();

            // Create quats for rotating the base ray left, right, up, and down.
            float angle = _stampAngle * strength;
            Quaternion yawLeftRot = Quaternion.AngleAxis(angle, toSphereUp);
            Quaternion yawRightRot = Quaternion.AngleAxis(-angle, toSphereUp);
            Quaternion pitchUpRot = Quaternion.AngleAxis(angle, toSphereRight);
            Quaternion pitchDownRot = Quaternion.AngleAxis(-angle, toSphereRight);

            // Compute rays for finding extents of box.
            Vector3 widthLeftDir = yawLeftRot * toSpherePos;
            Vector3 widthRightDir = yawRightRot * toSpherePos;
            Vector3 heightUpDir = pitchUpRot * toSpherePos;
            Vector3 heightDownDir = pitchDownRot * toSpherePos;

            // Compute hit points on sphere from rays.
            float sphereRadius = _passthroughSphere.localScale.x;
            Vector3 hitPointLeft = _passthroughSphere.position + widthLeftDir * sphereRadius;
            Vector3 hitPointRight = _passthroughSphere.position + widthRightDir * sphereRadius;
            Vector3 hitPointUp = _passthroughSphere.position + heightUpDir * sphereRadius;
            Vector3 hitPointDown = _passthroughSphere.position + heightDownDir * sphereRadius;
            _debugPointBounds[0] = hitPointLeft;
            _debugPointBounds[1] = hitPointRight;
            _debugPointBounds[2] = hitPointUp;
            _debugPointBounds[3] = hitPointDown;

            // Get texture coords of each hit point.
            Vector2 leftUv = TextureCoordsFromPos(hitPointLeft);
            Vector2 rightUv = TextureCoordsFromPos(hitPointRight);
            Vector2 upUv = TextureCoordsFromPos(hitPointUp);
            Vector2 downUv = TextureCoordsFromPos(hitPointDown);

            // Find texture bounds from uvs.
            int minU = Mathf.FloorToInt(leftUv.x * _maskTextureRes);
            int maxU = Mathf.CeilToInt(rightUv.x * _maskTextureRes);
            int minV = Mathf.FloorToInt(upUv.y * _maskTextureRes);
            int maxV = Mathf.CeilToInt(downUv.y * _maskTextureRes);

            // Special case if we're on the U seam.
            if (minU > maxU)
            {
                maxU += _maskTextureRes;
            }

            float changeScore = 0.0f;
            float midU = (minU + maxU) * 0.5f;
            float midV = (minV + maxV) * 0.5f;
            float uHalfRange = (maxU - minU) * 0.5f;
            float vHalfRange = (maxV - minV) * 0.5f;
            for (int u = minU; u < maxU; ++u)
            {
                for (int v = minV; v < maxV; ++v)
                {
                    int wrappedU = u % _maskTextureRes;
                    int index = (v * _maskTextureRes) + wrappedU;
                    float uDistFromMid = (u - midU) / uHalfRange;
                    float vDistFromMid = (v - midV) / vHalfRange;
                    float distToCenter = new Vector2(uDistFromMid, vDistFromMid).magnitude;
                    distToCenter = Mathf.Clamp01(distToCenter);
                    float newValue = Mathf.Min(_maskValues[index], distToCenter);
                    changeScore += _maskValues[index] - newValue;
                    _maskValues[index] = newValue;
                }
            }

            return changeScore > _particleTriggerThreshold;
        }

        void OnDrawGizmos()
        {
            if (_debugGizmoDraw)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(_debugPointRight, 0.02f);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_debugPointLeft, 0.02f);
                if (_debugPointBounds != null)
                {
                    for (int i = 0; i < _debugPointBounds.Length; ++i)
                    {
                        Gizmos.DrawSphere(_debugPointBounds[i], 0.01f);
                    }
                }
            }
        }
    }
}
