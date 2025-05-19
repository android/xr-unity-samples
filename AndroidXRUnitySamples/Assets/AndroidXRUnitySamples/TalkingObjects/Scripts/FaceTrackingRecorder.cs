// <copyright file="FaceTrackingRecorder.cs" company="Google LLC">
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

using System.Collections.Generic;
using Google.XR.Extensions;
using UnityEngine;

using Face = Google.XR.Extensions.XRFaceParameterIndices;

namespace AndroidXRUnitySamples.TalkingObjects
{
    /// <summary>
    /// Records and serves face blend shape parameter values.
    /// </summary>
    public class FaceTrackingRecorder : MonoBehaviour
    {
        [SerializeField] private XRFaceTrackingManager _faceManager;
        [SerializeField] private float _recordDuration;
        [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer;
        [SerializeField] private bool _mirrorBlendShapes;
        [SerializeField] private bool _pingPongPlayback;

        private bool _useLiveData;
        private float _liveDataWeight;
        private float _playbackTimer;
        private bool _pingPongForward;
        private int[] _mirrorIndexes;
        private bool _recording;
        private float _recordingTime;
        private List<FaceSnapshot> _recordedSnapshots;

        /// <summary> True if recording.</summary>
        public bool Recording => _recording;

        /// <summary> True if playing back live data.</summary>
        public bool UsingLiveData => _useLiveData;

        /// <summary> True if there is some recorded data stored.</summary>
        public bool HasRecordedData => _recordedSnapshots.Count > 0;

        /// <summary> Percent (0:1) progressing for recording.</summary>
        public float RecordingPercent => _recordingTime / _recordDuration;

        /// <summary> Percent (0:1) progressing for playback.</summary>
        public float PlaybackPercent => _playbackTimer / _recordDuration;

        /// <summary>Sets the mesh that'll be affected by the face tracking.</summary>
        /// <param name="smr">The target SkinnedMeshRenderer.</param>
        public void SetTargetSkinnedMeshRenderer(SkinnedMeshRenderer smr)
        {
            _skinnedMeshRenderer = smr;
        }

        /// <summary>Sets the playback to use data from a recorder or live data.</summary>
        /// <param name="useLive">Whether to use recorded or use live data.</param>
        public void UseLiveData(bool useLive)
        {
            _useLiveData = useLive;
            _playbackTimer = 0.0f;
        }

        /// <summary>Sets a scalar for live data.</summary>
        /// <param name="weight">Weight [0:1] of live data.</param>
        public void SetLiveDataWeight(float weight)
        {
            _liveDataWeight = Mathf.Clamp01(weight) * 100.0f;
        }

        /// <summary>Begins face tracking recording.</summary>
        public void BeginRecording()
        {
            _recordedSnapshots.Clear();
            _recordingTime = 0.0f;
            _recording = true;
            UseLiveData(true);
        }

        private void Awake()
        {
            _useLiveData = true;
            SetLiveDataWeight(1.0f);
            _recordedSnapshots = new List<FaceSnapshot>();
            CreateMirrorIndexes();
        }

        private void Update()
        {
            if (XRFaceTrackingFeature.IsFaceTrackingExtensionEnabled == null)
            {
                // XrInstance hasn't been initialized.
                return;
            }

            if (!XRFaceTrackingFeature.IsFaceTrackingExtensionEnabled.Value)
            {
                // XR_ANDROID_face_tracking is not enabled.
                return;
            }

            if (_recording)
            {
                _recordingTime += Time.deltaTime;
                if (_recordingTime >= _recordDuration)
                {
                    // If we've recorded the full duration, turn off recording and switch
                    // back to data playback (instead of live data).
                    _recording = false;
                    UseLiveData(false);
                }
                else
                {
                    FaceSnapshot newSnapshot = new FaceSnapshot();
                    newSnapshot.TimeStamp = _recordingTime;
                    newSnapshot.Params = new float[(int)Face.UpperLipRaiserR + 1];
                    for (int i = 0; i < _faceManager.Face.Parameters.Length; ++i)
                    {
                        newSnapshot.Params[i] = _faceManager.Face.Parameters[i];
                    }

                    _recordedSnapshots.Add(newSnapshot);
                }
            }

            if (_pingPongPlayback)
            {
                if (_pingPongForward)
                {
                    _playbackTimer += Time.deltaTime;
                    if (_playbackTimer >= _recordDuration)
                    {
                        _playbackTimer = _recordDuration;
                        _pingPongForward = false;
                    }
                }
                else
                {
                    _playbackTimer -= Time.deltaTime;
                    if (_playbackTimer <= 0.0f)
                    {
                        _playbackTimer = 0.0f;
                        _pingPongForward = true;
                    }
                }
            }
            else
            {
                _playbackTimer += Time.deltaTime;
                _playbackTimer %= _recordDuration;
            }

            float[] data = _useLiveData ? _faceManager.Face.Parameters :
                GetSnapshotParamsFromTimestamp(_playbackTimer);

            if (_mirrorBlendShapes)
            {
                for (int i = 0; i < _faceManager.Face.Parameters.Length; ++i)
                {
                    _skinnedMeshRenderer.SetBlendShapeWeight(
                        _mirrorIndexes[i], data[i] * _liveDataWeight);
                }
            }
            else
            {
                for (int i = 0; i < _faceManager.Face.Parameters.Length; ++i)
                {
                    _skinnedMeshRenderer.SetBlendShapeWeight(i, data[i] * _liveDataWeight);
                }
            }
        }

        private float[] GetSnapshotParamsFromTimestamp(float time)
        {
            if (_recordedSnapshots.Count <= 0)
            {
                return null;
            }

            if (_recordedSnapshots.Count == 1)
            {
                return _recordedSnapshots[0].Params;
            }

            // Run through our list of snapshots and find the boundaries to interpolate between.
            int index;
            if (time <= _recordedSnapshots[0].TimeStamp)
            {
                index = 0;
            }
            else
            {
                for (index = 0; index < _recordedSnapshots.Count - 1; ++index)
                {
                    if (time >= _recordedSnapshots[index].TimeStamp &&
                        time <= _recordedSnapshots[index + 1].TimeStamp)
                    {
                        break;
                    }
                }
            }

            // Find our lerp t.
            float t = Mathf.InverseLerp(_recordedSnapshots[index].TimeStamp,
                _recordedSnapshots[index + 1].TimeStamp, time);
            float[] lerpedParams = new float[_recordedSnapshots[index].Params.Length];
            for (int i = 0; i < lerpedParams.Length; ++i)
            {
                lerpedParams[i] = Mathf.Lerp(_recordedSnapshots[index].Params[i],
                    _recordedSnapshots[index + 1].Params[i], t);
            }

            return lerpedParams;
        }

        private void CreateMirrorIndexes()
        {
            _mirrorIndexes = new int[(int)Face.UpperLipRaiserR + 1];
            _mirrorIndexes[(int)Face.BrowLowererL] = (int)Face.BrowLowererR;
            _mirrorIndexes[(int)Face.BrowLowererR] = (int)Face.BrowLowererL;
            _mirrorIndexes[(int)Face.CheekPuffL] = (int)Face.CheekPuffR;
            _mirrorIndexes[(int)Face.CheekPuffR] = (int)Face.CheekPuffL;
            _mirrorIndexes[(int)Face.CheekRaiserL] = (int)Face.CheekRaiserR;
            _mirrorIndexes[(int)Face.CheekRaiserR] = (int)Face.CheekRaiserL;
            _mirrorIndexes[(int)Face.CheekSuckL] = (int)Face.CheekSuckR;
            _mirrorIndexes[(int)Face.CheekSuckR] = (int)Face.CheekSuckL;
            _mirrorIndexes[(int)Face.ChinRaiserB] = (int)Face.ChinRaiserB;
            _mirrorIndexes[(int)Face.ChinRaiserT] = (int)Face.ChinRaiserT;
            _mirrorIndexes[(int)Face.DimplerL] = (int)Face.DimplerR;
            _mirrorIndexes[(int)Face.DimplerR] = (int)Face.DimplerL;
            _mirrorIndexes[(int)Face.EyesClosedL] = (int)Face.EyesClosedR;
            _mirrorIndexes[(int)Face.EyesClosedR] = (int)Face.EyesClosedL;
            _mirrorIndexes[(int)Face.EyesLookDownL] = (int)Face.EyesLookDownR;
            _mirrorIndexes[(int)Face.EyesLookDownR] = (int)Face.EyesLookDownL;
            _mirrorIndexes[(int)Face.EyesLookLeftL] = (int)Face.EyesLookLeftR;
            _mirrorIndexes[(int)Face.EyesLookLeftR] = (int)Face.EyesLookLeftL;
            _mirrorIndexes[(int)Face.EyesLookRightL] = (int)Face.EyesLookRightR;
            _mirrorIndexes[(int)Face.EyesLookRightR] = (int)Face.EyesLookRightL;
            _mirrorIndexes[(int)Face.EyesLookUpL] = (int)Face.EyesLookUpR;
            _mirrorIndexes[(int)Face.EyesLookUpR] = (int)Face.EyesLookUpL;
            _mirrorIndexes[(int)Face.InnerBrowRaiserL] = (int)Face.InnerBrowRaiserR;
            _mirrorIndexes[(int)Face.InnerBrowRaiserR] = (int)Face.InnerBrowRaiserL;
            _mirrorIndexes[(int)Face.JawDrop] = (int)Face.JawDrop;
            _mirrorIndexes[(int)Face.JawSidewaysLeft] = (int)Face.JawSidewaysRight;
            _mirrorIndexes[(int)Face.JawSidewaysRight] = (int)Face.JawSidewaysLeft;
            _mirrorIndexes[(int)Face.JawThrust] = (int)Face.JawThrust;
            _mirrorIndexes[(int)Face.LidTightenerL] = (int)Face.LidTightenerR;
            _mirrorIndexes[(int)Face.LidTightenerR] = (int)Face.LidTightenerL;
            _mirrorIndexes[(int)Face.LipCornerDepressorL] = (int)Face.LipCornerDepressorR;
            _mirrorIndexes[(int)Face.LipCornerDepressorR] = (int)Face.LipCornerDepressorL;
            _mirrorIndexes[(int)Face.LipCornerPullerL] = (int)Face.LipCornerPullerR;
            _mirrorIndexes[(int)Face.LipCornerPullerR] = (int)Face.LipCornerPullerL;
            _mirrorIndexes[(int)Face.LipFunnelerLB] = (int)Face.LipFunnelerRB;
            _mirrorIndexes[(int)Face.LipFunnelerLT] = (int)Face.LipFunnelerRT;
            _mirrorIndexes[(int)Face.LipFunnelerRB] = (int)Face.LipFunnelerLB;
            _mirrorIndexes[(int)Face.LipFunnelerRT] = (int)Face.LipFunnelerLT;
            _mirrorIndexes[(int)Face.LipPressorL] = (int)Face.LipPressorR;
            _mirrorIndexes[(int)Face.LipPressorR] = (int)Face.LipPressorL;
            _mirrorIndexes[(int)Face.LipPuckerL] = (int)Face.LipPuckerR;
            _mirrorIndexes[(int)Face.LipPuckerR] = (int)Face.LipPuckerL;
            _mirrorIndexes[(int)Face.LipStretcherL] = (int)Face.LipStretcherR;
            _mirrorIndexes[(int)Face.LipStretcherR] = (int)Face.LipStretcherL;
            _mirrorIndexes[(int)Face.LipSuckLB] = (int)Face.LipSuckRB;
            _mirrorIndexes[(int)Face.LipSuckLT] = (int)Face.LipSuckRT;
            _mirrorIndexes[(int)Face.LipSuckRB] = (int)Face.LipSuckLB;
            _mirrorIndexes[(int)Face.LipSuckRT] = (int)Face.LipSuckLT;
            _mirrorIndexes[(int)Face.LipTightenerL] = (int)Face.LipTightenerR;
            _mirrorIndexes[(int)Face.LipTightenerR] = (int)Face.LipTightenerL;
            _mirrorIndexes[(int)Face.LipsToward] = (int)Face.LipsToward;
            _mirrorIndexes[(int)Face.LowerLipDepressorL] = (int)Face.LowerLipDepressorR;
            _mirrorIndexes[(int)Face.LowerLipDepressorR] = (int)Face.LowerLipDepressorL;
            _mirrorIndexes[(int)Face.MouthLeft] = (int)Face.MouthRight;
            _mirrorIndexes[(int)Face.MouthRight] = (int)Face.MouthLeft;
            _mirrorIndexes[(int)Face.NoseWrinklerL] = (int)Face.NoseWrinklerR;
            _mirrorIndexes[(int)Face.NoseWrinklerR] = (int)Face.NoseWrinklerL;
            _mirrorIndexes[(int)Face.OuterBrowRaiserL] = (int)Face.OuterBrowRaiserR;
            _mirrorIndexes[(int)Face.OuterBrowRaiserR] = (int)Face.OuterBrowRaiserL;
            _mirrorIndexes[(int)Face.UpperLidRaiserL] = (int)Face.UpperLidRaiserR;
            _mirrorIndexes[(int)Face.UpperLidRaiserR] = (int)Face.UpperLidRaiserL;
            _mirrorIndexes[(int)Face.UpperLipRaiserL] = (int)Face.UpperLipRaiserR;
            _mirrorIndexes[(int)Face.UpperLipRaiserR] = (int)Face.UpperLipRaiserL;
        }

        private class FaceSnapshot
        {
            public float[] Params;
            public float TimeStamp;
        }
    }
}
