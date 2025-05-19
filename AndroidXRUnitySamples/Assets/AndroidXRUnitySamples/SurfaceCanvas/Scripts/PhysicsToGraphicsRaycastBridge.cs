// <copyright file="PhysicsToGraphicsRaycastBridge.cs" company="Google LLC">
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
using System.Reflection;
using AndroidXRUnitySamples.Variables;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace AndroidXRUnitySamples.SurfaceCanvas
{
    /// <summary>
    /// Translates physics raycast hits on a mesh collider into graphics raycasts hists on a canvas
    /// being rendered by a separate camera.
    /// </summary>
    public class PhysicsToGraphicsRaycastBridge : MonoBehaviour
    {
        private static readonly int _maxRayIntersections = 10;
        private static Comparison<RaycastResult> _raycastCompareDelegate;
        private readonly RaycastHit[] _raycastHits = new RaycastHit[_maxRayIntersections];
        private PhysicsScene _localPhysicsScene;

        [SerializeField]
        private UIInputModule _inputModule;

        [SerializeField]
        private Canvas _canvas;

        // TODO: consider using a TrackedDeviceGraphicsRaycaster here
        private GraphicRaycaster _graphicRaycaster;

        private LayerMask _layerMask;

        [SerializeField]
        private BoolVariable _debugModeSetting;

        private Ray _lastRay = new Ray(Vector3.zero, Vector3.zero);

        private static Comparison<RaycastResult> _raycastComparer
        {
            get
            {
                if (_raycastCompareDelegate == null)
                {
                    _raycastCompareDelegate =
                            (Comparison<RaycastResult>)typeof(EventSystem).GetField(
                                            "s_RaycastComparer",
                                            BindingFlags.NonPublic | BindingFlags.Static)
                                    .GetValue(null);
                }

                return _raycastCompareDelegate;
            }
        }

        private static PointerEventData CopyPointerEventData(PointerEventData original)
        {
            // Create a new PointerEventData object with the same EventSystem reference
            var copy = new PointerEventData(EventSystem.current)
            {
                    // Manually copy all properties
                    pointerEnter = original.pointerEnter,
                    rawPointerPress = original.rawPointerPress,
                    pointerDrag = original.pointerDrag,
                    pointerCurrentRaycast = original.pointerCurrentRaycast,
                    pointerPressRaycast = original.pointerPressRaycast,
                    eligibleForClick = original.eligibleForClick,
                    pointerId = original.pointerId,
                    position = original.position,
                    delta = original.delta,
                    pressPosition = original.pressPosition,
                    clickTime = original.clickTime,
                    clickCount = original.clickCount,
                    scrollDelta = original.scrollDelta,
                    useDragThreshold = original.useDragThreshold,
                    dragging = original.dragging,
                    button = original.button,
                    hovered = original.hovered,

                    // Add other properties here as needed.
                    pressure = original.pressure,
                    radius = original.radius,
                    reentered = original.reentered,
                    tilt = original.tilt,
                    twist = original.twist,
                    altitudeAngle = original.altitudeAngle
            };
            return copy;
        }

        private void Awake()
        {
            _localPhysicsScene = gameObject.scene.GetPhysicsScene();
            _graphicRaycaster = _canvas.GetComponent<GraphicRaycaster>();
            _layerMask = 1 << gameObject.layer;
        }

        private void OnEnable()
        {
            _inputModule.finalizeRaycastResults += PhysicsToGraphicsRaycast;
        }

        private void OnDisable()
        {
            _inputModule.finalizeRaycastResults -= PhysicsToGraphicsRaycast;
        }

        private void PhysicsToGraphicsRaycast(PointerEventData pointerEventData,
                                              List<RaycastResult> raycastResults)
        {
            bool dirty = false;
            for (int i = raycastResults.Count - 1; i >= 0; i--)
            {
                RaycastResult raycastHit = raycastResults[i];
                if (raycastHit.module is TrackedDevicePhysicsRaycaster)
                {
                    if (raycastHit.gameObject.TryGetComponent(
                                out PhysicsToGraphicsRaycastBridge bridge))
                    {
                        if (bridge != this)
                        {
                            return;
                        }

                        List<RaycastResult> results =
                                TranslateRaycastResults(pointerEventData, raycastHit);

                        foreach (RaycastResult result in results)
                        {
                            if (_debugModeSetting.Value)
                            {
                                Debug.Log($"process canvas hit {result.gameObject}");
                            }

                            raycastResults.Add(result);
                            dirty = true;
                        }
                    }
                }
            }

            if (dirty)
            {
                raycastResults.Sort(_raycastComparer);
            }
        }

        private List<RaycastResult> TranslateRaycastResults(PointerEventData eventData,
                                                            RaycastResult raycastResult)
        {
            var results = new List<RaycastResult>();
            bool hitFound = false;
            RaycastHit meshObjHit = default;
            if (eventData is TrackedDeviceEventData trackedDeviceEventData)
            {
                if (trackedDeviceEventData.interactor is XRRayInteractor interactor)
                {
                    hitFound = interactor.TryGetCurrent3DRaycastHit(out meshObjHit);
                    Vector3 position = interactor.rayOriginTransform.position;
                    _lastRay = new Ray(position,
                            meshObjHit.point - position);
                }
            }

            if (!hitFound)
            {
                hitFound = RetraceRaycast(eventData, raycastResult, out meshObjHit);
            }

            if (hitFound)
            {
                Vector2 uv = meshObjHit.textureCoord;
                Vector3 screenPos = _canvas.worldCamera.ViewportToScreenPoint(uv);

                if (_debugModeSetting.Value)
                {
                    Debug.Log($"eventData current position: {eventData.position}");
                    Debug.Log($"Mesh UV: {uv}");
                    Debug.Log($"Translated screen position: {screenPos}");
                }

                PointerEventData translatedEventData = CopyPointerEventData(eventData);

                translatedEventData.position = screenPos;

                _graphicRaycaster.Raycast(translatedEventData, results);

                foreach (GraphicRaycaster gr in _canvas.GetComponentsInChildren<GraphicRaycaster>())
                {
                    gr.Raycast(translatedEventData, results);
                }
            }

            return results;
        }

        private bool RetraceRaycast(PointerEventData eventData, RaycastResult raycastResult,
                                    out RaycastHit hit)
        {
            Vector3 origin = default, hitPoint = default;
            if (eventData is TrackedDeviceEventData trackedDeviceEventData)
            {
                // TODO: this doesn't line up exactly with the ray display in the controller model
                origin = trackedDeviceEventData.rayPoints[trackedDeviceEventData.rayHitIndex - 1];
                hitPoint = trackedDeviceEventData.rayPoints[trackedDeviceEventData.rayHitIndex];
            }
            else
            {
                origin =
                        raycastResult.module.eventCamera.ScreenToWorldPoint(
                                eventData.pointerCurrentRaycast.screenPosition);
                hitPoint = raycastResult.worldPosition;
            }

            Vector3 direction = hitPoint - origin;
            _lastRay = new Ray(origin, direction);
            return GetMeshObjectRaycastHit(origin, direction, out hit);
        }

        private bool GetMeshObjectRaycastHit(Vector3 origin, Vector3 direction, out RaycastHit hit)
        {
            int hitCount = _localPhysicsScene.Raycast(origin, direction, _raycastHits,
                    float.PositiveInfinity, _layerMask.value);
            if (hitCount > 0)
            {
                RaycastHit meshObjHit = default;
                for (int i = 0; i < hitCount; i++)
                {
                    meshObjHit = _raycastHits[i];
                    if (meshObjHit.transform.gameObject.TryGetComponent(
                                out PhysicsToGraphicsRaycastBridge _))
                    {
                        hit = meshObjHit;
                        return true;
                    }
                }
            }

            hit = default;
            return false;
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (_debugModeSetting.Value)
            {
                if (_debugModeSetting.Value)
                {
                    Debug.DrawRay(_lastRay.origin, 1000 * _lastRay.direction, Color.red, 0f,
                            false);
                }
            }
        }
#endif
    }
}
