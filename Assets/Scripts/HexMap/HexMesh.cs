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

        public void Awake()
        {
            GetComponent<MeshFilter>().mesh = m_mesh = new();
            m_meshCollider = gameObject.AddComponent<MeshCollider>();
            m_mesh.name = "Hex Mesh";
            m_vertices = new List<Vector3>();
            m_triangles = new List<int>();
            m_colours = new List<Color>();
        }

        # region TriangulationMethods
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
            Vector3 center = cell.Position;
            EdgeVertices e = new EdgeVertices(
                center + HexMetrics.GetFirstSolidCorner(direction),
                center + HexMetrics.GetSecondSolidCorner(direction)
            );

            TriangulateEdgeFan(center, e, cell.colour);

            if (direction <= HexDirection.SE)
                TriangulateConnection(direction, cell, e);
        }

        private void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices e1)
        {
            HexCell neighbour = cell.GetNeighbour(direction);

            if (neighbour == null)
                return;

            Vector3 bridge = HexMetrics.GetBridge(direction); ;
            bridge.y = neighbour.Position.y - cell.Position.y;
            EdgeVertices e2 = new EdgeVertices(
                e1.v1 + bridge,
                e1.v4 + bridge
            );

            if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
            {
                TriangulateEdgeTerraces(e1, cell, e2, neighbour);
            }
            else
            {
                TriangulateEdgeStrip(e1, cell.colour, e2, neighbour.colour);
            }

            HexCell nextNeighbour = cell.GetNeighbour(direction.Next());
            if (direction <= HexDirection.E && nextNeighbour != null)
            {
                Vector3 v5 = e1.v4 + HexMetrics.GetBridge(direction.Next());
                v5.y = nextNeighbour.Position.y;

                // rotate triangle correctly based on 3 surrounding cell's elevation
                if (cell.Elevation <= neighbour.Elevation)
                {
                    if (cell.Elevation <= nextNeighbour.Elevation)
                    {
                        TriangulateCorner(e1.v4, cell, e2.v4, neighbour, v5, nextNeighbour);
                    }
                    else
                    {
                        TriangulateCorner(v5, nextNeighbour, e1.v4, cell, e2.v4, neighbour);
                    }
                }
                else if (neighbour.Elevation <= nextNeighbour.Elevation)
                {
                    TriangulateCorner(e2.v4, neighbour, v5, nextNeighbour, e1.v4, cell);
                }
                else
                {
                    TriangulateCorner(v5, nextNeighbour, e1.v4, cell, e2.v4, neighbour);
                }
            }
        }

        private void TriangulateEdgeTerraces(EdgeVertices begin, HexCell beginCell,
                                             EdgeVertices end, HexCell endCell)
        {
            EdgeVertices e2 = EdgeVertices.TerraceLerp(begin, end, 1);
            Color c2 = HexMetrics.TerraceLerp(beginCell.colour, endCell.colour, 1);

            // first step
            TriangulateEdgeStrip(begin, beginCell.colour, e2, c2);

            for (int i = 2; i < HexMetrics.terraceSteps; i++)
            {
                EdgeVertices e1 = e2;
                Color c1 = c2;
                e2 = EdgeVertices.TerraceLerp(begin, end, i);
                c2 = HexMetrics.TerraceLerp(beginCell.colour, endCell.colour, i);
                TriangulateEdgeStrip(e1, c1, e2, c2);
            }

            // last step
            TriangulateEdgeStrip(e2, c2, end, endCell.colour);
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
            Vector3 boundary = Vector3.Lerp(Perturb(begin), Perturb(right), b);
            Color boundaryColour = Color.Lerp(beginCell.colour, rightCell.colour, b);

            TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColour);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColour);
            }
            else
            {
                AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
                AddTriangleColour(leftCell.colour, rightCell.colour, boundaryColour);
            }
        }

        private void TriangulateCornerCliffTerraces(Vector3 begin, HexCell beginCell,
                                                    Vector3 left, HexCell leftCell,
                                                    Vector3 right, HexCell rightCell)
        {
            float b = 1f / (leftCell.Elevation - beginCell.Elevation);
            b = Mathf.Abs(b);
            Vector3 boundary = Vector3.Lerp(Perturb(begin), Perturb(left), b);
            Color boundaryColour = Color.Lerp(beginCell.colour, leftCell.colour, b);

            TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColour);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
            {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColour);
            }
            else
            {
                AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
                AddTriangleColour(leftCell.colour, rightCell.colour, boundaryColour);
            }
        }

        private void TriangulateBoundaryTriangle(Vector3 begin, HexCell beginCell,
                                                 Vector3 left, HexCell leftCell,
                                                 Vector3 boundary, Color boundaryColour)
        {
            Vector3 v2 = Perturb(HexMetrics.TerraceLerp(begin, left, 1));
            Color c2 = HexMetrics.TerraceLerp(beginCell.colour, leftCell.colour, 1);

            // first collapsing step
            AddTriangleUnperturbed(Perturb(begin), v2, boundary);
            AddTriangleColour(beginCell.colour, c2, boundaryColour);

            // in-between collpasing step
            for (int i = 2; i < HexMetrics.terraceSteps; i++)
            {
                Vector3 v1 = v2;
                Color c1 = c2;
                v2 = Perturb(HexMetrics.TerraceLerp(begin, left, i));
                c2 = HexMetrics.TerraceLerp(beginCell.colour, leftCell.colour, i);
                AddTriangleUnperturbed(v1, v2, boundary);
                AddTriangleColour(c1, c2, boundaryColour);
            }

            // last collapsing step
            AddTriangleUnperturbed(v2, Perturb(left), boundary);
            AddTriangleColour(c2, leftCell.colour, boundaryColour);
        }

        private void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color colour)
        {
            AddTriangle(center, edge.v1, edge.v2);
            AddTriangleColour(colour);
            AddTriangle(center, edge.v2, edge.v3);
            AddTriangleColour(colour);
            AddTriangle(center, edge.v3, edge.v4);
            AddTriangleColour(colour);
        }

        private void TriangulateEdgeStrip(EdgeVertices e1, Color c1, EdgeVertices e2, Color c2)
        {
            AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
            AddQuadColour(c1, c2);
            AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
            AddQuadColour(c1, c2);
            AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
            AddQuadColour(c1, c2);
        }
        # endregion

        #region MeshCreationMethods
        private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int vertexIndex = m_vertices.Count;
            m_vertices.Add(Perturb(v1));
            m_vertices.Add(Perturb(v2));
            m_vertices.Add(Perturb(v3));
            m_triangles.Add(vertexIndex);
            m_triangles.Add(vertexIndex + 1);
            m_triangles.Add(vertexIndex + 2);
        }

        private void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3)
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
            m_vertices.Add(Perturb(v1));
            m_vertices.Add(Perturb(v2));
            m_vertices.Add(Perturb(v3));
            m_vertices.Add(Perturb(v4));
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
        #endregion

        private Vector3 Perturb(Vector3 position)
        {
            Vector4 sample = HexMetrics.SampleNoise(position);
            position.x += (sample.x * 2f - 1f) * HexMetrics.cellPerturbStrength;
            // position.y += (sample.y * 2f - 1f) * HexMetrics.cellPerturbStrength;
            position.z += (sample.z * 2f - 1f) * HexMetrics.cellPerturbStrength;
            return position;
        }
    }
}
