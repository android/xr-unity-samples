// <copyright file="CreatureGaze.cs" company="Google LLC">
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
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.OpenXR.Features.Android;

namespace AndroidXRUnitySamples.CreatureGaze
{
    /// <summary>
    /// Class used to setup the Creature Gaze scene.
    /// </summary>
    public class CreatureGaze : MonoBehaviour
    {
        [SerializeField] private GameObject _creaturePrefab;
        [SerializeField] private int _numCreatures;
        [SerializeField] private Vector2 _creatureSpawnDistRange;
        [SerializeField] private float _creatureSpawnY;

        [Header("Creature physics")]
        [SerializeField] private float _creatureRadius;
        [SerializeField] private float _creatureForceScalar;

        [Header("User")]
        [SerializeField] private Transform _leftHand;
        [SerializeField] private Transform _rightHand;

        private EyeCreature[] _creatures;
        private bool _menuWasOpen;

        private void Start()
        {
            Singleton.Instance.OriginManager.EnablePassthrough = true;
            Singleton.Instance.OriginManager.EnableFaceManager = true;

            Vector3 camPos = Singleton.Instance.Camera.transform.position;

            _creatures = new EyeCreature[_numCreatures];
            for (int i = 0; i < _numCreatures; ++i)
            {
                Vector3 spawnPos;
                float dist = Random.Range(_creatureSpawnDistRange.x, _creatureSpawnDistRange.y);

                // Spawn our first little buddy front and center.
                if (i == 0)
                {
                    Vector3 camFwd = Singleton.Instance.Camera.transform.forward;
                    camFwd.y = 0.0f;
                    spawnPos = camPos + camFwd.normalized * dist;
                }
                else
                {
                    Vector3 spawnOffset = Random.insideUnitSphere;
                    spawnOffset.y = 0.0f;
                    spawnPos = camPos + spawnOffset.normalized * dist;
                }

                spawnPos.y += Random.Range(-_creatureSpawnY, _creatureSpawnY);

                GameObject go = Instantiate(_creaturePrefab, spawnPos, Quaternion.identity);
                _creatures[i] = go.GetComponent<EyeCreature>();
            }

            // We want the first creature to show up right away. The others will follow randomly.
            _creatures[0].SetSpawnDelay(0.5f);

            _menuWasOpen = false;
        }

        private void Update()
        {
            Vector3 headPos = Singleton.Instance.Camera.transform.position;
            Quaternion invHeadRot =
                Quaternion.Inverse(Singleton.Instance.Camera.transform.rotation);

            for (int i = 0; i < _numCreatures; ++i)
            {
                for (int j = i + 1; j < _numCreatures; ++j)
                {
                    CreatureDepenetration(_creatures[i], _creatures[j]);
                }

                // Move away from head and hands.
                PointDepenetration(_creatures[i], headPos);
                PointDepenetration(_creatures[i], _leftHand.position);
                PointDepenetration(_creatures[i], _rightHand.position);
            }

            Quaternion leftEyeRotation = Quaternion.identity;
            Quaternion rightEyeRotation = Quaternion.identity;

            ARFaceManager faceManager = Singleton.Instance.OriginManager.ARFaceManager;
            foreach (ARFace face in faceManager.trackables)
            {
                // FIXME: for now the ARFace trackingState is always None so we use the extension
                // method. Once it is fixed we can directly check face.trackingState.
                if (face.TryGetAndroidOpenXRFaceTrackingStates(
                        out AndroidOpenXRFaceTrackingStates states))
                {
                    if ((states & AndroidOpenXRFaceTrackingStates.LeftEyePoseValid) != 0)
                    {
                        Quaternion eyeWS = face.leftEye.rotation;
                        leftEyeRotation = eyeWS * invHeadRot;
                    }

                    if ((states & AndroidOpenXRFaceTrackingStates.RightEyePoseValid) != 0)
                    {
                        Quaternion eyeWS = face.rightEye.rotation;
                        rightEyeRotation = eyeWS * invHeadRot;
                    }
                }

                break;
            }

            for (int i = 0; i < _numCreatures; ++i)
            {
                _creatures[i].UpdatePhysics();
                _creatures[i].UpdateEyes(leftEyeRotation, rightEyeRotation);
            }

            // Hide the creatures when the menu is open.
            bool menuIsOpen = Singleton.Instance.Menu.Active;
            if (menuIsOpen != _menuWasOpen)
            {
                for (int i = 0; i < _numCreatures; ++i)
                {
                    _creatures[i].HideForMenu(menuIsOpen);
                }
            }

            _menuWasOpen = menuIsOpen;
        }

        private void CreatureDepenetration(EyeCreature a, EyeCreature b)
        {
            Vector3 aToB = b.transform.position - a.transform.position;
            float maxExtent = _creatureRadius * 2.0f;

            if (aToB.sqrMagnitude < maxExtent * maxExtent)
            {
                float aToBDist = aToB.magnitude;

                // If the two creatures are on top of eachother, just pick an
                // arbitrary vector for depenetration.
                if (aToBDist <= 0.0f)
                {
                    aToB = Random.insideUnitSphere;
                }

                float depenetrationAmount = (maxExtent - aToBDist) / maxExtent;
                Vector3 force = aToB.normalized *
                    depenetrationAmount * _creatureForceScalar * Time.deltaTime;
                a.AddToVelocity(-force);
                b.AddToVelocity(force);
            }
        }

        private void PointDepenetration(EyeCreature a, Vector3 point)
        {
            Vector3 aToPoint = point - a.transform.position;
            float maxExtent = _creatureRadius * 2.0f;

            if (aToPoint.sqrMagnitude < maxExtent * maxExtent)
            {
                float aToPointDist = aToPoint.magnitude;

                // If the creature and point are on top of eachother, just pick an
                // arbitrary vector for depenetration.
                if (aToPointDist <= 0.0f)
                {
                    aToPoint = Random.insideUnitSphere;
                }

                float depenetrationAmount = (maxExtent - aToPointDist) / maxExtent;
                Vector3 force = aToPoint.normalized *
                    depenetrationAmount * _creatureForceScalar * Time.deltaTime;
                a.AddToVelocity(-force);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_creatures != null)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < _creatures.Length; ++i)
                {
                    Gizmos.DrawWireSphere(_creatures[i].transform.position, _creatureRadius);
                    Gizmos.DrawSphere(_creatures[i].SpringHome, 0.025f);
                }
            }
        }
    }
}
