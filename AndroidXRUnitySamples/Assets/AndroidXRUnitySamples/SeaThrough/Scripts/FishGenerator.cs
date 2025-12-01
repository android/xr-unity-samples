// <copyright file="FishGenerator.cs" company="Google LLC">
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
namespace AndroidXRUnitySamples.SeaThrough
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Generates a fish mesh based on various parameters.
    /// </summary>
    public class FishGenerator : MonoBehaviour
    {
        [Header("Fish Properties (Overrides)")]

        /// <summary>
        /// Color of the fish body.
        /// </summary>
        public Color BodyColor = Color.white;

        /// <summary>
        /// Pattern on the fish.
        /// </summary>
        public FishPreset.PatternType Pattern = FishPreset.PatternType.None;

        /// <summary>
        /// Color of the fish pattern.
        /// </summary>
        public Color PatternColor = Color.black;

        /// <summary>
        /// Length of the fish body.
        /// </summary>
        [Range(5f, 15f)] public float BodyLength = 10f;

        /// <summary>
        /// Height of the fish body.
        /// </summary>
        [Range(1f, 5f)] public float BodyHeight = 3f;

        /// <summary>
        /// Size of the fish tail.
        /// </summary>
        [Range(1f, 5f)] public float TailSize = 2.2f;

        /// <summary>
        /// Size of the fish dorsal fin.
        /// </summary>
        [Range(1f, 5f)] public float DorsalFinSize = 3f;

        [Header("Preset & Materials")]

        /// <summary>
        /// Preset to load values from.
        /// </summary>
        [Tooltip("Assign a preset to load its values. You can then override them above.")]
        public FishPreset Preset;

        /// <summary>
        /// Material for the fish body.
        /// </summary>
        public Material BodyMaterial;

        /// <summary>
        /// Material for the fish fins.
        /// </summary>
        public Material FinMaterial;

        /// <summary>
        /// Material for the fish eyes.
        /// </summary>
        public Material EyeMaterial;

        /// <summary>
        /// Material for the fish pattern.
        /// </summary>
        public Material PatternMaterial;

        private const float _scale = 0.01f;

        private static readonly int _localTransformProperty =
            Shader.PropertyToID("_LocalTransform");

        private static readonly int _localTransformInverseProperty =
            Shader.PropertyToID("_LocalTransformInverse");

        private static readonly int _colorProperty = Shader.PropertyToID("_Color");

        [Header("Body Mesh Settings")]
        [Tooltip("The number of subdivisions along the length of the fish body.")]
        [SerializeField, Range(5, 50)] private int _bodyLengthResolution = 20;

        [Tooltip("The number of subdivisions along the height of the fish body.")]
        [SerializeField, Range(1, 20)] private int _bodyHeightSubdivisions = 5;

        private MaterialPropertyBlock _materialPropertyBlock = null;

        /// <summary>
        /// Generate a new fish.
        /// </summary>
        /// <returns>Gameobject containing the fish body parts.</returns>
        [ContextMenu("Generate Fish")]
        public GameObject GenerateFish()
        {
            var fish = new GameObject("fish");

            GameObject body = CreateBody();
            body.transform.SetParent(fish.transform, false);

            GameObject tail = CreateTailFin();
            tail.transform.SetParent(fish.transform, false);

            GameObject dorsalFin = CreateDorsalFin();
            dorsalFin.transform.SetParent(fish.transform, false);

            CreatePectoralFins(fish.transform);
            CreateEyes(fish.transform);

            if (Pattern == FishPreset.PatternType.Stripes)
            {
                CreateStripes(fish.transform);
            }
            else if (Pattern == FishPreset.PatternType.Spots)
            {
                CreateSpots(fish.transform);
            }

            foreach (Transform child in fish.transform)
            {
                if (child.TryGetComponent<MeshRenderer>(out var renderer))
                {
                    _materialPropertyBlock.Clear();
                    renderer.GetPropertyBlock(_materialPropertyBlock);
                    var transformMatrix = Matrix4x4.TRS(
                        child.localPosition, child.localRotation, child.localScale);
                    _materialPropertyBlock.SetMatrix(_localTransformProperty, transformMatrix);
                    Matrix4x4 inverseTransformMatrix = Matrix4x4.identity;
                    Matrix4x4.Inverse3DAffine(transformMatrix, ref inverseTransformMatrix);
                    _materialPropertyBlock.SetMatrix(
                        _localTransformInverseProperty, inverseTransformMatrix);
                    renderer.SetPropertyBlock(_materialPropertyBlock);
                }
            }

            return fish;
        }

        /// <summary>
        /// Load values from a preset.
        /// </summary>
        [ContextMenu("Load Values From Preset")]
        public void LoadFromPreset()
        {
            if (Preset == null)
            {
                Debug.LogWarning("No preset assigned to load values from.", this);
                return;
            }

            BodyColor = Preset.BodyColor;
            Pattern = Preset.Pattern;
            PatternColor = Preset.PatternColor;
            BodyLength = Preset.BodyLength;
            BodyHeight = Preset.BodyHeight;
            TailSize = Preset.TailSize;
            DorsalFinSize = Preset.DorsalFinSize;
        }

        private void Awake()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }

        #region Mesh Creation Methods

        private GameObject CreateBody()
        {
            float l = BodyLength * _scale;
            float h = BodyHeight * _scale;
            float depth = 2.5f * _scale;

            // Create the subdivided body mesh using the new method
            Mesh mesh = CreateBodyMesh(
                l, h, depth, _bodyLengthResolution, _bodyHeightSubdivisions);

            // Create the GameObject for the body
            GameObject body = CreateMeshObject("Body", mesh, BodyMaterial, BodyColor);

            // Center the body
            body.transform.localPosition = new Vector3(-BodyLength * _scale / 2f, 0, 0);

            return body;
        }

        private Mesh CreateBodyMesh(
            float length, float height, float depth, int lengthResolution, int heightSubdivisions)
        {
            // Define the bezier curve control points for the body shape
            Vector2 p0 = new Vector2(0, 0);
            Vector2 p1Top = new Vector2(length * 0.3f, height);
            Vector2 p2Top = new Vector2(length * 0.7f, height * 0.8f);
            Vector2 p3 = new Vector2(length, 0);
            Vector2 p1Bottom = new Vector2(length * 0.3f, -height);
            Vector2 p2Bottom = new Vector2(length * 0.7f, -height * 0.8f);

            // Generate lists of points for the top and bottom edges of the fish
            var topPoints = new List<Vector2>();
            var bottomPoints = new List<Vector2>();
            for (int i = 0; i <= lengthResolution; i++)
            {
                float t = (float)i / lengthResolution;
                topPoints.Add(GetPointOnBezierCurve(p0, p1Top, p2Top, p3, t));
                bottomPoints.Add(GetPointOnBezierCurve(p0, p1Bottom, p2Bottom, p3, t));
            }

            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            int vertsPerColumn = heightSubdivisions + 1;
            int vertsPerRow = lengthResolution + 1;
            int frontFaceVertCount = vertsPerRow * vertsPerColumn;

            // Generate vertices for the front face by interpolating between top and bottom points
            for (int i = 0; i < vertsPerRow; i++)
            {
                for (int j = 0; j < vertsPerColumn; j++)
                {
                    float t = (float)j / heightSubdivisions;
                    Vector2 point = Vector2.Lerp(bottomPoints[i], topPoints[i], t);
                    vertices.Add(new Vector3(point.x, point.y, -depth / 2f));
                }
            }

            // Generate vertices for the back face
            for (int i = 0; i < vertsPerRow; i++)
            {
                for (int j = 0; j < vertsPerColumn; j++)
                {
                    float t = (float)j / heightSubdivisions;
                    Vector2 point = Vector2.Lerp(bottomPoints[i], topPoints[i], t);
                    vertices.Add(new Vector3(point.x, point.y, depth / 2f));
                }
            }

            // Generate triangles for the front and back faces
            for (int i = 0; i < lengthResolution; i++)
            {
                for (int j = 0; j < heightSubdivisions; j++)
                {
                    int bottomLeft = i * vertsPerColumn + j;
                    int bottomRight = (i + 1) * vertsPerColumn + j;
                    int topLeft = bottomLeft + 1;
                    int topRight = bottomRight + 1;

                    // Front face quad
                    triangles.AddRange(new int[]
                    {
                        bottomLeft, topLeft, bottomRight, bottomRight, topLeft, topRight
                    });

                    // Back face quad (reversed winding)
                    int backBottomLeft = frontFaceVertCount + bottomLeft;
                    int backBottomRight = frontFaceVertCount + bottomRight;
                    int backTopLeft = frontFaceVertCount + topLeft;
                    int backTopRight = frontFaceVertCount + topRight;
                    triangles.AddRange(new int[]
                    {
                        backBottomLeft, backBottomRight, backTopLeft,
                        backBottomRight, backTopRight, backTopLeft
                    });
                }
            }

            // Stitch the seams (top, bottom, nose, and tail edges)
            for (int i = 0; i < lengthResolution; i++)
            {
                // Top seam - looking down, vertices should be counter-clockwise
                int frontTopLeft = i * vertsPerColumn + heightSubdivisions;
                int frontTopRight = (i + 1) * vertsPerColumn + heightSubdivisions;
                int backTopLeft = frontFaceVertCount + frontTopLeft;
                int backTopRight = frontFaceVertCount + frontTopRight;
                triangles.AddRange(new int[]
                {
                    backTopLeft, backTopRight, frontTopRight,
                    backTopLeft, frontTopRight, frontTopLeft
                });

                // Bottom seam - looking up, vertices should be counter-clockwise
                int frontBottomLeft = i * vertsPerColumn;
                int frontBottomRight = (i + 1) * vertsPerColumn;
                int backBottomLeft = frontFaceVertCount + frontBottomLeft;
                int backBottomRight = frontFaceVertCount + frontBottomRight;
                triangles.AddRange(new int[]
                {
                    frontBottomLeft, frontBottomRight, backBottomRight,
                    frontBottomLeft, backBottomRight, backBottomLeft
                });
            }

            // Stitch the nose and tail seams
            for (int j = 0; j < heightSubdivisions; j++)
            {
                // Tail seam
                int frontBottomRight = lengthResolution * vertsPerColumn + j;
                int frontTopRight = frontBottomRight + 1;
                int backBottomRight = frontFaceVertCount + frontBottomRight;
                int backTopRight = frontFaceVertCount + frontTopRight;
                triangles.AddRange(new int[]
                {
                    frontBottomRight, frontTopRight, backBottomRight,
                    backBottomRight, frontTopRight, backTopRight
                });

                // Nose seam
                int frontBottomLeft = j;
                int frontTopLeft = frontBottomLeft + 1;
                int backBottomLeft = frontFaceVertCount + frontBottomLeft;
                int backTopLeft = frontFaceVertCount + frontTopLeft;
                triangles.AddRange(new int[]
                {
                    frontBottomLeft, backBottomLeft, frontTopLeft,
                    frontTopLeft, backBottomLeft, backTopLeft
                });
            }

            // Create and return the final mesh
            Mesh mesh = new Mesh { name = "SubdividedFishBody" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private GameObject CreateTailFin()
        {
            float ts = TailSize * _scale;
            Vector2[] shapePoints =
            {
                new Vector2(0, 0), new Vector2(-ts, ts),
                new Vector2(-ts * 0.8f, 0), new Vector2(-ts, -ts)
            };

            Mesh mesh = ExtrudeShape(shapePoints, 0.15f * _scale);
            GameObject tail = CreateMeshObject("TailFin", mesh, FinMaterial, BodyColor);
            tail.transform.localPosition = new Vector3(-BodyLength * _scale / 2f, 0, 0);
            return tail;
        }

        private GameObject CreateDorsalFin()
        {
            float ds = DorsalFinSize * _scale;
            Vector2[] shapePoints =
            {
                new Vector2(0, 0), new Vector2(ds * 0.5f, ds),
                new Vector2(ds * 1.5f, 0)
            };

            Mesh mesh = ExtrudeShape(shapePoints, 0.15f * _scale);
            GameObject fin = CreateMeshObject("DorsalFin", mesh, FinMaterial, BodyColor);

            Vector3 currentCenter = mesh.bounds.center;
            fin.transform.localPosition = new Vector3(
                -currentCenter.x, BodyHeight * _scale * 0.8f - currentCenter.y, 0);

            return fin;
        }

        private void CreatePectoralFins(Transform fish)
        {
            Vector2 p0 = new Vector2(0, 0);
            Vector2 p1 = new Vector2(1.5f * _scale, 0.5f * _scale);
            Vector2 p2 = new Vector2(2f * _scale, -1f * _scale);
            Vector2 p3 = new Vector2(0, -1.5f * _scale);

            List<Vector2> shapePoints = new List<Vector2>();
            int resolution = 10;
            for (int i = 0; i <= resolution; i++)
            {
                float t = i / (float)resolution;
                shapePoints.Add(GetPointOnBezierCurve(p0, p1, p2, p3, t));
            }

            Mesh mesh = ExtrudeShape(shapePoints.ToArray(), 0.1f * _scale);
            float surfaceZ = 1.5f * _scale;

            GameObject finL = CreateMeshObject("PectoralFin_L", mesh, FinMaterial, BodyColor);
            finL.transform.SetParent(fish, false);
            finL.transform.localPosition = new Vector3(0, -BodyHeight * _scale / 4f, surfaceZ);
            finL.transform.localRotation = Quaternion.Euler(0, -45, 11.25f);

            GameObject finR = CreateMeshObject("PectoralFin_R", mesh, FinMaterial, BodyColor);
            finR.transform.SetParent(fish, false);
            finR.transform.localPosition = new Vector3(0, -BodyHeight * _scale / 4f, -surfaceZ);
            finR.transform.localRotation = Quaternion.Euler(0, 45, 11.25f);
        }

        private void CreateEyes(Transform fish)
        {
            float l = BodyLength * _scale;
            float h = BodyHeight * _scale;
            float surfaceZ = 1.5f * _scale * 0.95f;
            float eyeRadius = Mathf.Max(0.1f * _scale, Mathf.Min(0.25f * _scale, h / 16f));

            GameObject eyeL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            eyeL.name = "Eye_L";
            Destroy(eyeL.GetComponent<SphereCollider>());
            eyeL.transform.SetParent(fish, false);
            eyeL.transform.localScale = Vector3.one * eyeRadius * 2;
            eyeL.transform.localPosition = new Vector3((l / 2f) * 0.75f, h * 0.3f, surfaceZ);
            AssignMaterial(eyeL, EyeMaterial, Color.black);

            GameObject eyeR = Instantiate(eyeL, fish, false);
            eyeR.name = "Eye_R";
            eyeR.transform.localPosition = new Vector3((l / 2f) * 0.75f, h * 0.3f, -surfaceZ);
            AssignMaterial(eyeR, EyeMaterial, Color.black);
        }

        private void CreateStripes(Transform fish)
        {
            float l = BodyLength * _scale;
            float h = BodyHeight * _scale;
            float bodyDepth = 2.5f * _scale;

            Vector2 p0 = new Vector2(0, 0);
            Vector2 p1 = new Vector2(l * 0.3f, h);
            Vector2 p2 = new Vector2(l * 0.7f, h * 0.8f);
            Vector2 p3 = new Vector2(l, 0);

            Vector3[] stripePositions =
            {
                new Vector3((l / 2f) * 0.5f, 0, 0),
                new Vector3(0, 0, 0),
                new Vector3((-l / 2f) * 0.5f, 0, 0)
            };

            foreach (var pos in stripePositions)
            {
                float xInCurveSpace = pos.x + l / 2f;
                float bodyHeightAtX = GetYForXOnBezier(p0, p1, p2, p3, xInCurveSpace);
                float stripeHeight = bodyHeightAtX * 2f * 0.99f;
                if (stripeHeight <= 0.1f * _scale)
                {
                    continue;
                }

                GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stripe.name = "Stripe";
                Destroy(stripe.GetComponent<BoxCollider>());
                stripe.transform.SetParent(fish, false);
                stripe.transform.localPosition = pos;
                stripe.transform.localRotation = Quaternion.Euler(0, 0, 15f);

                stripe.transform.localScale =
                    new Vector3(0.5f * _scale, stripeHeight, bodyDepth * 1.05f);

                AssignMaterial(stripe, PatternMaterial, PatternColor);
            }
        }

        private void CreateSpots(Transform fish)
        {
            float l = BodyLength * _scale;
            float h = BodyHeight * _scale;
            float surfaceZ = 1.5f * _scale;

            for (int i = 0; i < 8; i++)
            {
                float spotX = Random.Range(-l / 2f * 0.8f, l / 2f * 0.8f);
                float spotY = Random.Range(-h / 2f * 0.7f, h / 2f * 0.7f);

                GameObject spotL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                spotL.name = "Spot_L";
                Destroy(spotL.GetComponent<CapsuleCollider>());
                spotL.transform.SetParent(fish, false);
                spotL.transform.localScale =
                    new Vector3(0.5f * _scale, 0.05f * _scale, 0.5f * _scale);
                spotL.transform.localPosition =
                    new Vector3(spotX, spotY, surfaceZ - (0.01f * _scale));
                spotL.transform.localRotation = Quaternion.Euler(90, 0, 0);
                AssignMaterial(spotL, PatternMaterial, PatternColor);

                GameObject spotR = Instantiate(spotL, fish, false);
                spotR.name = "Spot_R";
                spotR.transform.localPosition =
                    new Vector3(spotX, spotY, -surfaceZ + (0.01f * _scale));
                AssignMaterial(spotR, PatternMaterial, PatternColor);
            }
        }

        #endregion

        #region Utility Methods

        private GameObject CreateMeshObject(
            string name, Mesh mesh, Material templateMaterial, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.AddComponent<MeshFilter>().sharedMesh = mesh;
            obj.AddComponent<MeshRenderer>();
            AssignMaterial(obj, templateMaterial, color);
            return obj;
        }

        private void AssignMaterial(GameObject obj, Material templateMaterial, Color color)
        {
            if (templateMaterial != null)
            {
                MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = templateMaterial;
                _materialPropertyBlock.Clear();
                _materialPropertyBlock.SetColor(_colorProperty, color);
                renderer.SetPropertyBlock(_materialPropertyBlock);
            }
        }

        private Vector2 GetPointOnBezierCurve(
            Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * oneMinusT * p0 +
                   3f * oneMinusT * oneMinusT * t * p1 +
                   3f * oneMinusT * t * t * p2 +
                   t * t * t * p3;
        }

        private float GetYForXOnBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float x)
        {
            for (float t = 0; t <= 1; t += 0.001f)
            {
                Vector2 point = GetPointOnBezierCurve(p0, p1, p2, p3, t);
                if (point.x >= x)
                {
                    return point.y;
                }
            }

            return 0;
        }

        private Mesh ExtrudeShape(Vector2[] shapePoints, float depth)
        {
            if (shapePoints.Length < 3)
            {
                return null;
            }

            Mesh mesh = new Mesh();
            int numPoints = shapePoints.Length;
            int numVerts = numPoints * 2;
            int numTris = (numPoints - 2) * 2 + numPoints * 2;

            Vector3[] vertices = new Vector3[numVerts];
            int[] triangles = new int[numTris * 3];

            for (int i = 0; i < numPoints; i++)
            {
                vertices[i] = new Vector3(shapePoints[i].x, shapePoints[i].y, -depth / 2f);
                vertices[i + numPoints] =
                    new Vector3(shapePoints[i].x, shapePoints[i].y, depth / 2f);
            }

            int triIndex = 0;

            for (int i = 1; i < numPoints - 1; i++)
            {
                triangles[triIndex++] = 0;
                triangles[triIndex++] = i;
                triangles[triIndex++] = i + 1;

                triangles[triIndex++] = numPoints;
                triangles[triIndex++] = numPoints + i + 1;
                triangles[triIndex++] = numPoints + i;
            }

            for (int i = 0; i < numPoints; i++)
            {
                int next = (i + 1) % numPoints;

                triangles[triIndex++] = i;
                triangles[triIndex++] = i + numPoints;
                triangles[triIndex++] = next;

                triangles[triIndex++] = next;
                triangles[triIndex++] = i + numPoints;
                triangles[triIndex++] = next + numPoints;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        #endregion
    }
}
