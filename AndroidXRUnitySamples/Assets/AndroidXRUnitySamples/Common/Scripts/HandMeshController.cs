// <copyright file="HandMeshController.cs" company="Google LLC">
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
using AndroidXRUnitySamples.Variables;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands.Samples.VisualizerSample;

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Manages hands when using Hand Mesh extension.
    /// </summary>
    public class HandMeshController : MonoBehaviour
    {
        [SerializeField] private MeshFilter _leftHandMesh;
        [SerializeField] private MeshFilter _rightHandMesh;
        [SerializeField] private HandVisualizer _handVisualizer;
        [SerializeField] private BoolVariable _useHandMesh;

        private XRMeshSubsystem _meshSubsystem;

        private void Start()
        {
            List<XRMeshSubsystem> meshSubsystems = new List<XRMeshSubsystem>();
            SubsystemManager.GetSubsystems(meshSubsystems);
            if (meshSubsystems.Count != 1)
            {
                Debug.LogError("Unexpected number of mesh subsystems."
                    + $"Expected 1, got {meshSubsystems.Count}.");
                _useHandMesh.Value = false;
                return;
            }

            _meshSubsystem = meshSubsystems[0];

            _useHandMesh.AddListener(HandleUseHandMeshChange);
            HandleUseHandMeshChange(_useHandMesh.Value);
        }

        private void OnDestroy()
        {
            _useHandMesh.RemoveListener(HandleUseHandMeshChange);
        }

        private void HandleUseHandMeshChange(bool enable)
        {
            _handVisualizer.drawMeshes = !enable;
            _leftHandMesh.gameObject.SetActive(enable);
            _rightHandMesh.gameObject.SetActive(enable);
        }

        private void Update()
        {
            if (_meshSubsystem == null || !_meshSubsystem.running || !_useHandMesh.Value)
            {
                return;
            }

            List<MeshInfo> meshInfos = new List<MeshInfo>();
            if (_meshSubsystem.TryGetMeshInfos(meshInfos))
            {
                if (meshInfos.Count != 2)
                {
                    Debug.LogError("Unexpected number of mesh infos from hand mesh subsystem."
                        + $" Expected 2, got {meshInfos.Count}.");
                }

                if (meshInfos[0].ChangeState == MeshChangeState.Added
                    || meshInfos[0].ChangeState == MeshChangeState.Updated)
                {
                    _meshSubsystem.GenerateMeshAsync(meshInfos[0].MeshId, _leftHandMesh.mesh,
                        null, MeshVertexAttributes.Normals, result => { });
                }

                if (meshInfos[1].ChangeState == MeshChangeState.Added
                    || meshInfos[1].ChangeState == MeshChangeState.Updated)
                {
                    _meshSubsystem.GenerateMeshAsync(meshInfos[1].MeshId, _rightHandMesh.mesh,
                        null, MeshVertexAttributes.Normals, result => { });
                }
            }
        }
    }
}
