///<summary>
/// Author: Halen
/// 
/// 
/// 
///</summary>

using System.Collections.Generic;
using UnityEngine;

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

        public void Awake()
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
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                Triangulate(d, cell);
            }
        }

        private void Triangulate(HexDirection direction, HexCell cell)
        {
            Vector3 center = cell.transform.localPosition;
            Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
            Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);

            AddTriangle(center, v1, v2);
            AddTriangleColour(cell.colour);

            if (direction <= HexDirection.SE)
                TriangulateConnection(direction, cell, v1, v2);
        }

        private void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2)
        {
            HexCell neighbour = cell.GetNeighbour(direction);

            if (neighbour == null)
                return;

            Vector3 bridge = HexMetrics.GetBridge(direction); ;
            Vector3 v3 = v1 + bridge;
            Vector3 v4 = v2 + bridge;
            v3.y = v4.y = neighbour.Elevation * HexMetrics.elevationStep;

            if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
            {
                TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbour);
            }
            else
            {
                AddQuad(v1, v2, v3, v4);
                AddQuadColour(cell.colour, neighbour.colour);
            }

            HexCell nextNeighbour = cell.GetNeighbour(direction.Next());
            if (direction <= HexDirection.E && nextNeighbour != null)
            {
                Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
                v5.y = nextNeighbour.Elevation * HexMetrics.elevationStep;

                // rotate triangle correctly based on 3 surrounding cell's elevation
                if (cell.Elevation <= neighbour.Elevation)
                {
                    if (cell.Elevation <= nextNeighbour.Elevation)
                    {
                        TriangulateCorner(v2, cell, v4, neighbour, v5, nextNeighbour);
                    }
                    else
                    {
                        TriangulateCorner(v5, nextNeighbour, v2, cell, v4, neighbour);
                    }
                }
                else if (neighbour.Elevation <= nextNeighbour.Elevation)
                {
                    TriangulateCorner(v4, neighbour, v5, nextNeighbour, v2, cell);
                }
                else
                {
                    TriangulateCorner(v5, nextNeighbour, v2, cell, v4, neighbour);
                }
            }
        }

        private void TriangulateEdgeTerraces(Vector3 beginLeft, Vector3 beginRight, HexCell beginCell,
                                             Vector3 endLeft, Vector3 endRight, HexCell endCell)
        {
            Vector3 v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, 1);
            Vector3 v4 = HexMetrics.TerraceLerp(beginRight, endRight, 1);
            Color c2 = HexMetrics.TerraceLerp(beginCell.colour, endCell.colour, 1);

            // first step
            AddQuad(beginLeft, beginRight, v3, v4);
            AddQuadColour(beginCell.colour, c2);

            for (int i = 2; i < HexMetrics.terraceSteps; i++)
            {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c2;
                v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
                v4 = HexMetrics.TerraceLerp(beginRight, endRight, i);
                c2 = HexMetrics.TerraceLerp(beginCell.colour, endCell.colour, i);
                AddQuad(v1, v2, v3, v4);
                AddQuadColour(c1, c2);
            }

            // last step
            AddQuad(v3, v4, endLeft, endRight);
            AddQuadColour(c2, endCell.colour);
        }

        private void TriangulateCorner(Vector3 bottom, HexCell bottomCell,
                                       Vector3 left, HexCell leftCell,
                                       Vector3 right, HexCell rightCell)
        {
            HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
            HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

            if (leftEdgeType == HexEdgeType.Slope)
            {
                if (rightEdgeType == HexEdgeType.Slope)
                {
                    TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                }
                else if (rightEdgeType == HexEdgeType.Flat)
                {
                    TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
                }
                else
                {
                    TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
                }
            }
            else if (rightEdgeType == HexEdgeType.Slope)
            {
                if (leftEdgeType == HexEdgeType.Flat)
                {
                    TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                }
                else
                {
                    TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                }
            }
            else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                if (leftCell.Elevation < rightCell.Elevation)
                {
                    TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                }
                else
                {
                    TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
                }
            }
            else
            {
                AddTriangle(bottom, left, right);
                AddTriangleColour(bottomCell.colour, leftCell.colour, rightCell.colour);
            }
        }

        private void TriangulateCornerTerraces(Vector3 begin, HexCell beginCell,
                                               Vector3 left, HexCell leftCell,
                                               Vector3 right, HexCell rightCell)
        {
            Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
            Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
            Color c3 = HexMetrics.TerraceLerp(beginCell.colour, leftCell.colour, 1);
            Color c4 = HexMetrics.TerraceLerp(beginCell.colour, rightCell.colour, 1);

            // first triangle step
            AddTriangle(begin, v3, v4);
            AddTriangleColour(beginCell.colour, c3, c4);

            // in-between quad steps
            for (int i = 2; i < HexMetrics.terraceSteps; i++)
            {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c3;
                Color c2 = c4;
                v3 = HexMetrics.TerraceLerp(begin, left, i);
                v4 = HexMetrics.TerraceLerp(begin, right, i);
                c3 = HexMetrics.TerraceLerp(beginCell.colour, leftCell.colour, i);
                c4 = HexMetrics.TerraceLerp(beginCell.colour, rightCell.colour, i);
                AddQuad(v1, v2, v3, v4);
                AddQuadColour(c1, c2, c3, c4);
            }


            // last quad step
            AddQuad(v3, v4, left, right);
            AddQuadColour(c3, c4, leftCell.colour, rightCell.colour);
        }

        private void TriangulateCornerTerracesCliff(Vector3 begin, HexCell beginCell,
                                                    Vector3 left, HexCell leftCell,
                                                    Vector3 right, HexCell rightCell)
        {
            float b = 1f / (rightCell.Elevation - beginCell.Elevation);
            b = Mathf.Abs(b);
            Vector3 boundary = Vector3.Lerp(begin, right, b);
            Color boundaryColour = Color.Lerp(beginCell.colour, rightCell.colour, b);

            TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColour);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColour);
            }
            else
            {
                AddTriangle(left, right, boundary);
                AddTriangleColour(leftCell.colour, rightCell.colour, boundaryColour);
            }
        }

        private void TriangulateCornerCliffTerraces(Vector3 begin, HexCell beginCell,
                                                    Vector3 left, HexCell leftCell,
                                                    Vector3 right, HexCell rightCell)
        {
            float b = 1f / (leftCell.Elevation - beginCell.Elevation);
            b = Mathf.Abs(b);
            Vector3 boundary = Vector3.Lerp(begin, left, b);
            Color boundaryColour = Color.Lerp(beginCell.colour, leftCell.colour, b);

            TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColour);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColour);
            }
            else
            {
                AddTriangle(left, right, boundary);
                AddTriangleColour(leftCell.colour, rightCell.colour, boundaryColour);
            }
        }

        private void TriangulateBoundaryTriangle(Vector3 begin, HexCell beginCell,
                                                 Vector3 left, HexCell leftCell,
                                                 Vector3 boundary, Color boundaryColour)
        {
            Vector3 v2 = HexMetrics.TerraceLerp(begin, left, 1);
            Color c2 = HexMetrics.TerraceLerp(beginCell.colour, leftCell.colour, 1);

            // first collapsing step
            AddTriangle(begin, v2, boundary);
            AddTriangleColour(beginCell.colour, c2, boundaryColour);

            // in-between collpasing step
            for (int i = 2; i < HexMetrics.terraceSteps; i++)
            {
                Vector3 v1 = v2;
                Color c1 = c2;
                v2 = HexMetrics.TerraceLerp(begin, left, i);
                c2 = HexMetrics.TerraceLerp(beginCell.colour, leftCell.colour, i);
                AddTriangle(v1, v2, boundary);
                AddTriangleColour(c1, c2, boundaryColour);
            }

            // last collapsing step
            AddTriangle(v2, left, boundary);
            AddTriangleColour(c2, leftCell.colour, boundaryColour);
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

        private void AddTriangleColour(Color c1, Color c2, Color c3)
        {
            m_colours.Add(c1);
            m_colours.Add(c2);
            m_colours.Add(c3);
        }

        private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            int vertexIndex = m_vertices.Count;
            m_vertices.Add(v1);
            m_vertices.Add(v2);
            m_vertices.Add(v3);
            m_vertices.Add(v4);
            m_triangles.Add(vertexIndex);
            m_triangles.Add(vertexIndex + 2);
            m_triangles.Add(vertexIndex + 1);
            m_triangles.Add(vertexIndex + 1);
            m_triangles.Add(vertexIndex + 2);
            m_triangles.Add(vertexIndex + 3);
        }

        void AddQuadColour(Color c1, Color c2)
        {
            m_colours.Add(c1);
            m_colours.Add(c1);
            m_colours.Add(c2);
            m_colours.Add(c2);
        }

        void AddQuadColour(Color c1, Color c2, Color c3, Color c4)
        {
            m_colours.Add(c1);
            m_colours.Add(c2);
            m_colours.Add(c3);
            m_colours.Add(c4);
        }
    }
}
