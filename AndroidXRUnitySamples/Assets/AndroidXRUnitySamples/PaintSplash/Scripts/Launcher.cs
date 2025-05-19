// <copyright file="Launcher.cs" company="Google LLC">
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
using UnityEngine.InputSystem;
using UnityEngine.Pool;

namespace AndroidXRUnitySamples.PaintSplash
{
    /// <summary>
    /// Class used to manage a projectile launcher with an object pool.
    /// </summary>
    public class Launcher : MonoBehaviour
    {
        private const string _kLaunchParam = "Launching";
        private const string _kColorParam = "_BaseColor";
        private static readonly int _launchParamId = Animator.StringToHash(_kLaunchParam);

        [SerializeField]
        private GameObject _projectilePrefab;
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private Vector3 _launchForce;
        [SerializeField]
        private Vector3 _launchPositionOffset;

        [Space]
        [SerializeField]
        private InputAction _positionAction;
        [SerializeField]
        private InputAction _rotationAction;
        [SerializeField]
        private InputAction _launchAction;

        [Space]
        [SerializeField]
        private BoolVariable _menuIsOpen;

        private ObjectPool<Projectile> _projectilePool;
        [SerializeField]
        private List<Color> _colors;
        private int _nextColorIndex;

        /// <summary>
        /// Launches a single projectile. Called from animation.
        /// </summary>
        public void Launch()
        {
            if (!enabled || _menuIsOpen.Value)
            {
                return;
            }

            var projectile = _projectilePool.Get();
            if (projectile.Mesh
                .TryGetComponent<MeshRenderer>(out var meshRenderer))
            {
                meshRenderer.material.SetColor(_kColorParam, _colors[_nextColorIndex]);
                _nextColorIndex = (_nextColorIndex + 1) % _colors.Count;
            }

            projectile.Launch(_launchForce,
                              _launchPositionOffset);
        }

        private void Start()
        {
            _animator.SetBool(_launchParamId, false);

            _positionAction.Enable();
            _rotationAction.Enable();
            _launchAction.Enable();

            _projectilePool = new ObjectPool<Projectile>(
                    createFunc: () =>
                    {
                        var instance = Instantiate(_projectilePrefab);
                        var projectile = instance.GetComponent<Projectile>();
                        projectile.SetLauncher(transform);
                        projectile.SetReleaseAction(() => _projectilePool.Release(projectile));
                        return projectile;
                    },
                    actionOnGet: projectile =>
                    {
                        projectile.gameObject.SetActive(true);
                    },
                    actionOnRelease: projectile =>
                    {
                        projectile.gameObject.SetActive(false);
                    },
                    actionOnDestroy: Destroy);
        }

        private void OnDestroy()
        {
            _projectilePool.Clear();
        }

        private void Update()
        {
            transform.position = _positionAction.ReadValue<Vector3>();
            transform.rotation = _rotationAction.ReadValue<Quaternion>();

            _animator.SetBool(_launchParamId, _launchAction.IsPressed());
        }
    }
}
