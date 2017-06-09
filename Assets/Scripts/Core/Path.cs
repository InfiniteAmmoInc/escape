using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using System;

namespace Scape
{
    public class Path : MonoBehaviour
    {
        [System.Serializable]
        public class Node
        {
            public Node(Vector3 point)
            {
                this.point = point;
            }

            public Vector3 point;
        }

        [System.Serializable]
        public class SpawnSet
        {
            public string name;
            public GameObject[] prefabs;
            public float spacing;
            public float minScale, maxScale;
            public enum Comparison
            {
                None,
                GreaterThan,
                LessThan,
                EqualTo,
            }

            public Comparison upYComparison;
            public float upY;
            public float minNormalOffset, maxNormalOffset;
        }

        public Color color = Color.white;
        public bool generateCollision;
        public bool generateFillMesh;
        public float fillMeshZ;
        public Material fillMaterial;
        public GameObject prefabSpawnAlong;
        //public bool showCollision;
        public float boxExtrude;
        public SpawnSet[] spawnSets;
        public Node[] nodes;

        void Awake()
        {
            if (generateCollision)
                GenerateCollision();
            if (generateFillMesh)
                GenerateFillMesh();
            SpawnSpawnSets();
        }

        void GenerateCollision()
        {
            for (int i = 0; i < nodes.Length - 1; i++)
            {
                var diff = nodes[i + 1].point - nodes[i].point;
                var perp = new Vector3(diff.y, -diff.x, 0f);
                var newGameObject = new GameObject("Box");
                newGameObject.AddComponent<BoxCollider>();
                newGameObject.transform.parent = transform;
                newGameObject.transform.localScale = new Vector3(diff.magnitude, Mathf.Abs(boxExtrude), 1f);
                newGameObject.transform.right = diff.normalized;
                newGameObject.transform.localPosition = diff / 2f + nodes[i].point + perp.normalized * boxExtrude / 2f;
                newGameObject.layer = LayerMask.NameToLayer("Obstruction");

                if (prefabSpawnAlong != null)
                {
                    var newSpawn = (GameObject)Instantiate(prefabSpawnAlong, newGameObject.transform.position, newGameObject.transform.rotation);
                    newSpawn.transform.parent = this.transform;
                    newSpawn.transform.localScale = newGameObject.transform.localScale;
                }
            }
        }

        void GenerateFillMesh()
        {
            var meshFilter = gameObject.AddComponent<MeshFilter>();
            var mesh = new Mesh();
            var meshRenderer = gameObject.AddComponent<MeshRenderer>();

            Vector2[] points = new Vector2[nodes.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = nodes[i].point;
            }

            List<Vector3> points3d = new List<Vector3>();
            for (int i = 0; i < nodes.Length; i++)
            {
                points3d.Add(new Vector3(nodes[i].point.x, nodes[i].point.y, fillMeshZ));
            }

            mesh.SetVertices(points3d);

            var t = new Triangulator(points);
            mesh.SetIndices(t.Triangulate(), MeshTopology.Triangles, 0);
            meshFilter.mesh = mesh;

            List<Vector2> uvs = new List<Vector2>();
            for (int i = 0; i < nodes.Length; i++)
            {
                uvs.Add(new Vector2(nodes[i].point.x, nodes[i].point.y));
            }
            mesh.SetUVs(0, uvs);

            meshRenderer.material = fillMaterial;
        }

        void SpawnSpawnSets()
        {
            for (int i = 0; i < nodes.Length - 1; i++)
            {
                var diff = nodes[i + 1].point - nodes[i].point;
                var normal = new Vector3(-diff.y, diff.x, 0f);
                normal = normal.normalized;

                float localX = diff.magnitude;
                float localY = Mathf.Abs(boxExtrude);
                float length = diff.magnitude;
                var right = diff.normalized;
                var midPoint = diff / 2f + nodes[i].point + normal * boxExtrude / 2f;

                foreach (var spawnSet in spawnSets)
                {
                    if (spawnSet.spacing != 0f)
                    {
                        bool doSpawn = true;
                        switch (spawnSet.upYComparison)
                        {
                            case SpawnSet.Comparison.GreaterThan:
                                doSpawn = normal.y > spawnSet.upY;
                                break;
                            case SpawnSet.Comparison.LessThan:
                                doSpawn = normal.y < spawnSet.upY;
                                break;
                            case SpawnSet.Comparison.EqualTo:
                                doSpawn = normal.y == spawnSet.upY;
                                break;
                        }

                        if (doSpawn)
                        {
                            int numToSpawn = (int)(length / spawnSet.spacing);
                            for (int j = 0; j < numToSpawn; j++)
                            {
                                var newGameObject = (GameObject)Instantiate(spawnSet.prefabs[UnityEngine.Random.Range(0, spawnSet.prefabs.Length)]);
                                newGameObject.transform.position = midPoint + right * UnityEngine.Random.Range(-localX / 2f, localX / 2f);
                                newGameObject.transform.position += normal * UnityEngine.Random.Range(spawnSet.minNormalOffset, spawnSet.maxNormalOffset);
                                newGameObject.transform.up = normal;
                                if (spawnSet.minScale != 0f || spawnSet.maxScale != 0f)
                                {
                                    newGameObject.transform.localScale = Vector3.one * UnityEngine.Random.Range(spawnSet.minScale, spawnSet.maxScale);
                                }
                            }
                        }
                    }
                }
            }
        }

        void OnDrawGizmos()
        {
            if (nodes != null)
            {
                Gizmos.color = color;
                for (int i = 0; i < nodes.Length - 1; i++)
                {
                    if (boxExtrude != 0f)
                        DrawLine(transform.TransformPoint(nodes[i].point), transform.TransformPoint(nodes[i + 1].point), boxExtrude);
                    else
                        Gizmos.DrawLine(transform.TransformPoint(nodes[i].point), transform.TransformPoint(nodes[i + 1].point));

                    /*


                    if (boxExtrude != 0f)
                    {
                        Vector3 diff = nodes[i+1].point - nodes[i].point;
                        diff.Normalize();
                        diff = new Vector3(diff.y, -diff.x, 0f);
                        Gizmos.DrawLine(transform.TransformPoint(nodes[i].point) + diff * boxExtrude, transform.TransformPoint(nodes[i+1].point) + diff * boxExtrude);
                    }
                    */
                }
            }
        }

        static void DrawLine(Vector3 p1, Vector3 p2, float width)
        {
            const float factor = 20f;
            int count = Mathf.CeilToInt(width * factor); // how many lines are needed.
            if (count == 1)
                Gizmos.DrawLine(p1, p2);
            else
            {
                Camera c = Camera.current;
                if (c == null)
                {
                    Debug.LogError("Camera.current is null");
                    return;
                }
                Vector3 v1 = (p2 - p1).normalized; // line direction
                Vector3 v2 = (c.transform.position - p1).normalized; // direction to camera
                Vector3 n = Vector3.Cross(v1, v2); // normal vector
                for (int i = 0; i < count; i++)
                {
                    Vector3 o = n * width * factor * ((float)i / (count - 1) - 1f) / factor;
                    Gizmos.DrawLine(p1 + o, p2 + o);
                }
            }
        }

        void OnDrawGimosSelected()
        {
            Gizmos.color = Color.white;

            if (nodes != null)
            {
                for (int i = 0; i < nodes.Length - 1; i++)
                {
                    Vector3 a = transform.TransformPoint(nodes[i].point);
                    Vector3 b = transform.TransformPoint(nodes[i + 1].point);
                    Gizmos.DrawLine(a, b);

                    /*
                    Util.DrawArrowHead((b-a).normalized, b);
                    */

                    if (boxExtrude != 0f)
                    {
                        Vector3 diff = nodes[i + 1].point - nodes[i].point;
                        diff.Normalize();
                        diff = new Vector3(diff.y, -diff.x, 0f);
                        Gizmos.DrawLine(a + diff * boxExtrude, b + diff * boxExtrude);
                    }
                }
            }
        }
    }
}