///<summary>
/// Author: Halen
/// 
/// 
/// 
///</summary>

using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

namespace CardGame.HexMap
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexMesh : MonoBehaviour
    {
        private Mesh m_mesh;
        private List<Vector3> m_vertices;
        private List<int> m_triangles;
        private List<Color> m_colours;

        private MeshCollider m_meshCollider;

        private void Awake()
        {
            GetComponent<MeshFilter>().mesh = m_mesh = new();
            m_meshCollider = gameObject.AddComponent<MeshCollider>();
            m_mesh.name = "Hex Mesh";
            m_vertices = new List<Vector3>();
            m_triangles = new List<int>();
            m_colours = new List<Color>();
        }

        public void Triangulate(HexCell[] cells)
        {
            m_mesh.Clear();
            m_vertices.Clear();
            m_triangles.Clear();
            m_colours.Clear();
            
            for (int c = 0; c < cells.Length; c++)
            {
                Triangulate(cells[c]);
            }

            m_mesh.vertices = m_vertices.ToArray();
            m_mesh.triangles = m_triangles.ToArray();
            m_mesh.colors = m_colours.ToArray();
            m_mesh.RecalculateNormals();
            m_meshCollider.sharedMesh = m_mesh;
        }

        private void Triangulate(HexCell cell)
        {
            Vector3 center = cell.transform.localPosition;
            for (int s = 0; s < 6; s++)
            {
                AddTriangle(
                    center,
                    center + HexMetrics.corners[s],
                    center + HexMetrics.corners[s + 1]
                );
                AddTriangleColour(cell.colour);
            }
        }

        private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int vertexIndex = m_vertices.Count;
            m_vertices.Add(v1);
            m_vertices.Add(v2);
            m_vertices.Add(v3);
            m_triangles.Add(vertexIndex);
            m_triangles.Add(vertexIndex + 1);
            m_triangles.Add(vertexIndex + 2);
        }

        private void AddTriangleColour(Color colour)
        {
            m_colours.Add(colour);
            m_colours.Add(colour);
            m_colours.Add(colour);
        }
    }
}
