// <copyright file="SimpleDestroyWithoutChildren.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples
{
    /// <summary>
    /// Simple script for destroying an object when there are no children.
    /// This is useful for parents of collections of particle systems that
    /// all destroy themselves when they're done emitting.
    /// </summary>
    public class SimpleDestroyWithoutChildren : MonoBehaviour
    {
        private void Update()
        {
            if (transform.childCount <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
