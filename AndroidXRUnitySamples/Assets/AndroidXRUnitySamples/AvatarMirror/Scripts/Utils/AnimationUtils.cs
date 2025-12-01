// <copyright file="AnimationUtils.cs" company="Google LLC">
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

namespace AndroidXRUnitySamples.AvatarMirror
{
    /// <summary>
    /// Utility class for animation-related calculations and pose transformations.
    /// </summary>
    public static class AnimationUtils
    {
        /// <summary>
        /// Defines common coordinate axes.
        /// </summary>
        public enum Axis
        {
            /// <summary>Represents the positive X axis.</summary>
            X = 1,

            /// <summary>Represents the positive Y axis.</summary>
            Y = 2,

            /// <summary>Represents the positive Z axis.</summary>
            Z = 3,

            /// <summary>Represents the negative X axis.</summary>
            negX = 4,

            /// <summary>Represents the negative Y axis.</summary>
            negY = 5,

            /// <summary>Represents the negative Z axis.</summary>
            negZ = 6
        }

        /// <summary>
        /// Calculates the pose of `thisPose` relative to `parentPose`.
        /// This is equivalent to `parentPose.Inverse() * thisPose`.
        /// </summary>
        /// <param name="thisPose">The pose to transform (child pose).</param>
        /// <param name="parentPose">The reference frame (parent pose) in world space.</param>
        /// <returns>The pose of `thisPose` expressed in `parentPose`'s local coordinate
        /// system.</returns>
        public static Pose InverseTransformedBy(this Pose thisPose, Pose parentPose)
        {
            // Calculate the inverse rotation of the parent pose.
            Quaternion inverseParentRotation = Quaternion.Inverse(parentPose.rotation);

            // Rotate the child's position by the inverse of the parent's rotation,
            // after subtracting the parent's position to make it relative to the parent's origin.
            Vector3 relativePosition =
                inverseParentRotation * (thisPose.position - parentPose.position);

            // Rotate the child's rotation by the inverse of the parent's rotation.
            Quaternion relativeRotation = inverseParentRotation * thisPose.rotation;

            return new Pose(relativePosition, relativeRotation);
        }

        /// <summary>
        /// Calculates the pose of `poseInWorldFrame` relative to `parentTransformInWorldFrame`.
        /// This is equivalent to `parentTransformInWorldFrame.localRotation.Inverse() *
        /// (poseInWorldFrame.position - parentTransformInWorldFrame.position)`.
        /// </summary>
        /// <param name="poseInWorldFrame">The pose to transform (child pose) in world
        /// space.</param> <param name="parentTransformInWorldFrame">The reference Transform
        /// (parent) in world space.</param> <returns>The pose of `poseInWorldFrame` expressed in
        /// `parentTransformInWorldFrame`'s local coordinate system.</returns>
        public static Pose InverseTransformedBy(this Pose poseInWorldFrame,
                                                Transform parentTransformInWorldFrame)
        {
            // Calculate the inverse world rotation of the parent transform.
            Quaternion parentInverseRotation =
                Quaternion.Inverse(parentTransformInWorldFrame.rotation);

            // Calculate the position of the pose relative to the parent's origin and then rotate it
            // by the inverse parent rotation.
            Vector3 positionInParentFrame =
                parentInverseRotation *
                (poseInWorldFrame.position - parentTransformInWorldFrame.position);

            // Calculate the rotation of the pose relative to the parent's rotation.
            Quaternion rotationInParentFrame = parentInverseRotation * poseInWorldFrame.rotation;

            return new Pose(positionInParentFrame, rotationInParentFrame);
        }

        /// <summary>
        /// Decomposes a quaternion into its twist and swing components.
        /// </summary>
        /// <param name="rotation">The quaternion to decompose.</param>
        /// <param name="twistAxis">The axis around which the twist occurs.</param>
        /// <returns>A tuple containing the twist and swing quaternions.</returns>
        public static (Quaternion twist, Quaternion swing)
            DecomposeTwistSwing(Quaternion rotation, Vector3 twistAxis)
        {
            // Project the rotation axis onto the twist axis.
            Vector3 compVector = new Vector3(rotation.x, rotation.y, rotation.z);

            // Calculate twist.
            Vector3 projection = Vector3.Dot(twistAxis, compVector) * twistAxis;

            Quaternion twist = Quaternion.identity;
            twist.Set(projection.x, projection.y, projection.z, rotation.w);
            twist.Normalize();

            if (projection.sqrMagnitude <= Mathf.Epsilon &&
                (rotation.w * rotation.w) <= Mathf.Epsilon)
            {
                twist = Quaternion.identity;
            }

            // Calculate the swing rotation by inverting the twist and multiplying it with the
            // original rotation.
            Quaternion swing = Quaternion.Inverse(twist) * rotation;

            return (twist, swing);
        }

        /// <summary>
        /// Calculates the signed angle of a quaternion's rotation when projected onto a plane
        /// defined by its normal. This is useful for determining angular deviation within a plane.
        /// </summary>
        /// <param name="q">The quaternion representing the rotation.</param>
        /// <param name="planeNormal">The normal vector of the plane. This should be
        /// normalized.</param> <returns>The signed angle in degrees. Returns 0 if the rotation axis
        /// is parallel to the plane normal.</returns>
        public static float SignedAngle(Quaternion q, Vector3 planeNormal)
        {
            // Normalize planeNormal just in case it isn't.
            planeNormal.Normalize();

            // Extract the rotation axis and angle from the quaternion.
            q.ToAngleAxis(out float angle, out Vector3 axis);

            // Project the rotation axis onto the plane. This gives the direction of rotation
            // *within* the plane.
            Vector3 projectedAxis = Vector3.ProjectOnPlane(axis, planeNormal).normalized;

            // If the projected axis is zero (meaning the rotation axis is parallel to the plane
            // normal), we cannot determine a unique signed angle, so return 0.
            if (projectedAxis == Vector3.zero)
            {
                return 0f;
            }

            // Create a reference vector in the plane that is perpendicular to the projected axis
            // and the plane normal. This vector will define the 'zero' angle in the plane.
            Vector3 referenceVector = Vector3.Cross(planeNormal, projectedAxis).normalized;

            // Rotate this reference vector by the original quaternion.
            Vector3 rotatedReference = q * referenceVector;

            // Project the rotated vector back onto the plane.
            Vector3 projectedRotatedReference =
                Vector3.ProjectOnPlane(rotatedReference, planeNormal).normalized;

            // Calculate the signed angle between the original reference vector and the rotated one,
            // using the plane normal to determine the sign.
            // Vector3.SignedAngle is often sufficient here, but the explicit cross product for sign
            // is robust.
            float dotProduct =
                Vector3.Dot(Vector3.Cross(referenceVector, projectedRotatedReference), planeNormal);
            float sign = Mathf.Sign(dotProduct);

            // Mathf.Acos(Vector3.Dot(referenceVector, projectedRotatedReference)) * Mathf.Rad2Deg;
            // The `angle` extracted from ToAngleAxis is always positive. We apply the sign derived
            // from the plane.
            return angle * sign;
        }
    }
}
