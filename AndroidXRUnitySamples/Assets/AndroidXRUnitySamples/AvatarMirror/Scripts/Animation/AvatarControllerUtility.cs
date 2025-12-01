// <copyright file="AvatarControllerUtility.cs" company="Google LLC">
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
using UnityEngine;
using UnityEngine.XR.Hands;  // Required for XRHandJointID

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// Provides utility methods for setting up and configuring an <see cref="AvatarController"/>
    /// with an avatar rig.
    /// </summary>
    public static class AvatarControllerUtility
    {
        /// <summary>
        /// Automatically configures an <see cref="AvatarController"/> by searching for specific
        /// GameObjects and components within a provided avatar rig based on their names.
        /// </summary>
        /// <param name="controller">The <see cref="AvatarController"/> instance to set up.</param>
        /// <param name="rig">The root <see cref="Transform"/> of the avatar's skeletal rig.</param>
        public static void SetupNewAvatar(AvatarController controller, Transform rig)
        {
            // find all children of the rig
            List<Transform> allDiscendants = FindAllDescendants(rig);

            controller._lfAvatarHandRoot = null;
            controller._rtAvatarHandRoot = null;

            controller._avatarRoot = null;
            controller._lfEye = null;
            controller._rtEye = null;
            controller._mainMesh = null;

            foreach (Transform discendant in allDiscendants)
            {
                //// body

                Debug.Log(discendant.name);

                if (discendant.name.Contains("pelvis"))
                {
                    controller._avatarRoot = discendant.gameObject;
                    Debug.Log($"Assingned {discendant.name} to avatar root");
                }

                if (discendant.name.Contains("lf_eye"))
                {
                    controller._lfEye = discendant.gameObject;
                    Debug.Log($"Assingned {discendant.name} to avatar _lfEye");
                }

                if (discendant.name.Contains("rt_eye"))
                {
                    controller._rtEye = discendant.gameObject;
                    Debug.Log($"Assingned {discendant.name} to avatar _rtEye");
                }

                if (discendant.name.Contains("avatar_geo"))
                {
                    controller._mainMesh =
                        discendant.gameObject.GetComponent<SkinnedMeshRenderer>();
                    Debug.Log($"Assingned {discendant.name} to avatar _mainMesh");
                }

                //// hands

                if (discendant.name.Contains("lf_wrist") && !discendant.name.Contains("twist"))
                {
                    controller._lfAvatarHandRoot = discendant;
                    Debug.Log($"Assingned {discendant.name} to avatar _lfAvatarHandRoot");
                }

                if (discendant.name.Contains("rt_wrist") && !discendant.name.Contains("twist"))
                {
                    controller._rtAvatarHandRoot = discendant;
                    Debug.Log($"Assingned {discendant.name} to avatar _rtAvatarHandRoot");
                }
            }

            if (controller._lfAvatarHandRoot == null || controller._rtAvatarHandRoot == null)
            {
                Debug.Log($"Left or Right Wrist not found. Avatar hands won't be initialized.");
                return;
            }

            //// fingers

            AssignHandJoints(Handedness.Left, controller);
            AssignHandJoints(Handedness.Right, controller);
        }

        /// <summary>
        /// Finds all descendant Transforms of a given parent Transform, including the parent
        /// itself.
        /// </summary>
        /// <param name="parentTransform">The parent Transform to start the search from.</param>
        /// <returns>A List of all descendant Transforms, including the parent.</returns>
        public static List<Transform> FindAllDescendants(Transform parentTransform)
        {
            List<Transform> allDescendants = new ();

            if (parentTransform == null)
            {
                Debug.LogWarning(
                    "FindAllDescendants: Provided parent Transform is null. Returning empty list.");
                return allDescendants;
            }

            Queue<Transform> queue = new ();
            queue.Enqueue(parentTransform);
            allDescendants.Add(parentTransform);

            while (queue.Count > 0)
            {
                Transform currentTransform = queue.Dequeue();
                foreach (Transform child in currentTransform)
                {
                    allDescendants.Add(child);
                    queue.Enqueue(child);
                }
            }

            Debug.Log(
                $"FindAllDescendants: Found {allDescendants.Count} transforms (including self) for "
              + $"'{parentTransform.name}'.");
            return allDescendants;
        }

        private static void AssignHandJoints(Handedness hand, AvatarController controller)
        {
            // find all children of the rig
            List<Transform> allDiscendants = new ();
            string prefix = string.Empty;
            List<AvatarJointIDToTransform> jointIDToTransforms = new ();

            if (hand == Handedness.Left)
            {
                prefix = "lf";
                allDiscendants = FindAllDescendants(controller._lfAvatarHandRoot);
                jointIDToTransforms = controller._lfAvatarJointIDToTransforms;
            }
            else if (hand == Handedness.Right)
            {
                prefix = "rt";
                allDiscendants = FindAllDescendants(controller._rtAvatarHandRoot);
                jointIDToTransforms = controller._rtAvatarJointIDToTransforms;
            }

            jointIDToTransforms.Clear();

            foreach (Transform discendant in allDiscendants)
            {
                if (discendant.name.Contains(prefix + "_thumb"))
                {
                    AvatarJointIDToTransform newFingerJoint = new ();

                    if (discendant.name.Contains("01"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.ThumbMetacarpal;
                    }

                    if (discendant.name.Contains("02"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.ThumbProximal;
                    }

                    if (discendant.name.Contains("03"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.ThumbDistal;
                    }

                    if (discendant.name.Contains("04"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.ThumbTip;
                    }

                    if (newFingerJoint.XRHandJointID != 0)
                    {
                        newFingerJoint.JointTransform = discendant.transform;
                        jointIDToTransforms.Add(newFingerJoint);
                    }

                    Debug.Log($"Added {discendant.name} to avatar {jointIDToTransforms}");
                }

                if (discendant.name.Contains(prefix + "_index"))
                {
                    AvatarJointIDToTransform newFingerJoint = new ();

                    if (discendant.name.Contains("00"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.IndexMetacarpal;
                    }

                    if (discendant.name.Contains("01"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.IndexProximal;
                    }

                    if (discendant.name.Contains("02"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.IndexIntermediate;
                    }

                    if (discendant.name.Contains("03"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.IndexDistal;
                    }

                    if (discendant.name.Contains("04"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.IndexTip;
                    }

                    if (newFingerJoint.XRHandJointID != 0)
                    {
                        newFingerJoint.JointTransform = discendant.transform;
                        jointIDToTransforms.Add(newFingerJoint);
                    }

                    Debug.Log($"Added {discendant.name} to avatar {jointIDToTransforms}");
                }

                if (discendant.name.Contains(prefix + "_middle"))
                {
                    AvatarJointIDToTransform newFingerJoint = new ();

                    if (discendant.name.Contains("00"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.MiddleMetacarpal;
                    }

                    if (discendant.name.Contains("01"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.MiddleProximal;
                    }

                    if (discendant.name.Contains("02"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.MiddleIntermediate;
                    }

                    if (discendant.name.Contains("03"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.MiddleDistal;
                    }

                    if (discendant.name.Contains("04"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.MiddleTip;
                    }

                    if (newFingerJoint.XRHandJointID != 0)
                    {
                        newFingerJoint.JointTransform = discendant.transform;
                        jointIDToTransforms.Add(newFingerJoint);
                    }

                    Debug.Log($"Added {discendant.name} to avatar {jointIDToTransforms}");
                }

                if (discendant.name.Contains(prefix + "_ring"))
                {
                    AvatarJointIDToTransform newFingerJoint = new ();

                    if (discendant.name.Contains("00"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.RingMetacarpal;
                    }

                    if (discendant.name.Contains("01"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.RingProximal;
                    }

                    if (discendant.name.Contains("02"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.RingIntermediate;
                    }

                    if (discendant.name.Contains("03"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.RingDistal;
                    }

                    if (discendant.name.Contains("04"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.RingTip;
                    }

                    if (newFingerJoint.XRHandJointID != 0)
                    {
                        newFingerJoint.JointTransform = discendant.transform;
                        jointIDToTransforms.Add(newFingerJoint);
                    }

                    Debug.Log($"Added {discendant.name} to avatar {jointIDToTransforms}");
                }

                if (discendant.name.Contains(prefix + "_pinky"))
                {
                    AvatarJointIDToTransform newFingerJoint = new ();

                    if (discendant.name.Contains("00"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.LittleMetacarpal;
                    }

                    if (discendant.name.Contains("01"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.LittleProximal;
                    }

                    if (discendant.name.Contains("02"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.LittleIntermediate;
                    }

                    if (discendant.name.Contains("03"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.LittleDistal;
                    }

                    if (discendant.name.Contains("04"))
                    {
                        newFingerJoint.XRHandJointID = XRHandJointID.LittleTip;
                    }

                    if (newFingerJoint.XRHandJointID != 0)
                    {
                        newFingerJoint.JointTransform = discendant.transform;
                        jointIDToTransforms.Add(newFingerJoint);
                    }

                    Debug.Log($"Added {discendant.name} to avatar {jointIDToTransforms}");
                }
            }
        }

        /// <summary>
        /// Checks if a given string value starts with or ends with a specified search term,
        /// ignoring case, and also checks for an appended "_jnt" suffix.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <param name="searchTerm">The term to search for (e.g., an XRHandJointID.ToString()
        /// value).</param> <returns>True if the value matches the searchTerm (or searchTerm +
        /// "_jnt"), false otherwise.</returns>
        private static bool StartsOrEndsWith(string value, string searchTerm)
        {
            bool directMatch =
                value.StartsWith(searchTerm, StringComparison.InvariantCultureIgnoreCase) ||
                value.EndsWith(searchTerm, StringComparison.InvariantCultureIgnoreCase);

            bool jntSuffixMatch =
                value.StartsWith(searchTerm + "_jnt",
                                 StringComparison.InvariantCultureIgnoreCase) ||
                value.EndsWith(searchTerm + "_jnt", StringComparison.InvariantCultureIgnoreCase);

            return directMatch || jntSuffixMatch;
        }
    }
}
