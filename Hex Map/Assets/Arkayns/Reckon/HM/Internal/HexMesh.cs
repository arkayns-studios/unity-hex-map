using System.Collections.Generic;
using UnityEngine;

namespace Arkayns.Reckon.HM {
    
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexMesh : MonoBehaviour {
        
        // -- Variables --
        private static List<Vector3> m_vertices = new();
        private static List<Color> m_colors = new();
        private static List<int> m_triangles = new();

        private Mesh m_hexMesh;
        private MeshCollider m_meshCollider;
        
        // -- Built-In Methods --
        private void Awake() {
            GetComponent<MeshFilter>().mesh = m_hexMesh = new Mesh();
            m_meshCollider = gameObject.AddComponent<MeshCollider>();
            m_hexMesh.name = "Hex Mesh";
        } // Awake
        
        // -- Methods --
        public void Triangulate(HexCell[] cells) {
            m_hexMesh.Clear();
            m_vertices.Clear();
            m_colors.Clear();
            m_triangles.Clear();
            foreach (var cell in cells) Triangulate(cell);
            m_hexMesh.vertices = m_vertices.ToArray();
            m_hexMesh.colors = m_colors.ToArray();
            m_hexMesh.triangles = m_triangles.ToArray();
            m_hexMesh.RecalculateNormals();
            m_meshCollider.sharedMesh = m_hexMesh;
        } // Triangulate

        private void Triangulate(HexCell cell) {
            for (var d = HexDirection.NE; d <= HexDirection.NW; d++) Triangulate(d, cell);
        } // Triangulate

        private void Triangulate(HexDirection direction, HexCell cell) {
            var center = cell.Position;
            var e = new EdgeVertices(center + HexMetrics.GetFirstSolidCorner(direction), center + HexMetrics.GetSecondSolidCorner(direction));

            if (cell.HasRiver) {
                if (cell.HasRiverThroughEdge(direction)) {
                    e.v3.y = cell.StreamBedY;
                    if (cell.HasRiverBeginOrEnd) TriangulateWithRiverBeginOrEnd(direction, cell, center, e);
                    else TriangulateWithRiver(direction, cell, center, e);
                } else TriangulateAdjacentToRiver(direction, cell, center, e);
            } else TriangulateEdgeFan(center, e, cell.Color);
            
            if (direction <= HexDirection.SE) TriangulateConnection(direction, cell, e);
        } // Triangulate ()

        private void TriangulateWithRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e) {
            Vector3 centerL, centerR;
            if (cell.HasRiverThroughEdge(direction.Opposite())) {
                centerL = center + HexMetrics.GetFirstSolidCorner(direction.Next()) * 0.25f;
                centerR = center + HexMetrics.GetSecondSolidCorner(direction.Next()) * 0.25f;
                center = Vector3.Lerp(centerL, centerR, 0.5f);
            } else if (cell.HasRiverThroughEdge(direction.Next())) {
                centerL = center;
                centerR = Vector3.Lerp(center, e.v5, 2f / 3f);
            } else if (cell.HasRiverThroughEdge(direction.Previous())) {
                centerL = Vector3.Lerp(center, e.v1, 2f / 3f);
                centerR = center;
            } else if (cell.HasRiverThroughEdge(direction.Next2())) {
                centerL = center;
                centerR = center + HexMetrics.GetSolidEdgeMiddle(direction.Next()) * (0.5f * HexMetrics.innerToOuter);
            } else {
                centerL = center + HexMetrics.GetSolidEdgeMiddle(direction.Previous()) * (0.5f * HexMetrics.innerToOuter);
                centerR = center;
            }
            
            var m = new EdgeVertices(Vector3.Lerp(centerL, e.v1, 0.5f), Vector3.Lerp(centerR, e.v5, 0.5f), 1f / 6f);
            m.v3.y = center.y = e.v3.y;
            
            TriangulateEdgeStrip(m, cell.Color, e, cell.Color);
            
            AddTriangle(centerL, m.v1, m.v2);
            AddTriangleColor(cell.Color);
            AddQuad(centerL, center, m.v2, m.v3);
            AddQuadColor(cell.Color);
            AddQuad(center, centerR, m.v3, m.v4);
            AddQuadColor(cell.Color);
            AddTriangle(centerR, m.v4, m.v5);
            AddTriangleColor(cell.Color);
        } // TriangulateWithRiver ()

        private void TriangulateWithRiverBeginOrEnd(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e) {
            var m = new EdgeVertices(Vector3.Lerp(center, e.v1, 0.5f), Vector3.Lerp(center, e.v5, 0.5f));
            m.v3.y = e.v3.y;
            
            TriangulateEdgeStrip(m, cell.Color, e, cell.Color);
            TriangulateEdgeFan(center, m, cell.Color);
        } // TriangulateWithRiverBeginOrEnd ()

        private void TriangulateAdjacentToRiver(HexDirection direction, HexCell cell, Vector3 center, EdgeVertices e) {
            var m = new EdgeVertices(Vector3.Lerp(center, e.v1, 0.5f), Vector3.Lerp(center, e.v5, 0.5f));
            TriangulateEdgeStrip(m, cell.Color,e, cell.Color);
            TriangulateEdgeFan(center, m, cell.Color);
        } // TriangulateAdjacentToRiver ()
        
        private void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices e1) {
            var neighbor = cell.GetNeighbor(direction);
            if (neighbor == null) return;

            var bridge = HexMetrics.GetBridge(direction);
            bridge.y = neighbor.Position.y - cell.Position.y;
            var e2 = new EdgeVertices(e1.v1 + bridge, e1.v5 + bridge);

            if (cell.HasRiverThroughEdge(direction)) e2.v3.y = neighbor.StreamBedY;
            
            if (cell.GetEdgeType(direction) == HexEdgeType.Slope) TriangulateEdgeTerraces(e1, cell, e2, neighbor);
            else TriangulateEdgeStrip(e1, cell.Color, e2, neighbor.Color);

            var nextNeighbor = cell.GetNeighbor(direction.Next());
            if (direction > HexDirection.E || nextNeighbor == null) return;

            var v5 = e1.v5 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.Position.y;

            if (cell.Elevation <= neighbor.Elevation) {
                if (cell.Elevation <= nextNeighbor.Elevation) {
                    TriangulateCorner(e1.v5, cell, e2.v5, neighbor, v5, nextNeighbor);
                } else {
                    TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
                }
            } else if (neighbor.Elevation <= nextNeighbor.Elevation) {
                TriangulateCorner(e2.v5, neighbor, v5, nextNeighbor, e1.v5, cell);
            } else {
                TriangulateCorner(v5, nextNeighbor, e1.v5, cell, e2.v5, neighbor);
            }
        } // TriangulateConnection

        private void TriangulateCorner(Vector3 bottom, HexCell bottomCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
            var leftEdgeType = bottomCell.GetEdgeType(leftCell);
            var rightEdgeType = bottomCell.GetEdgeType(rightCell);

            if (leftEdgeType == HexEdgeType.Slope) {
                if (rightEdgeType == HexEdgeType.Slope) {
                    TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                } else if (rightEdgeType == HexEdgeType.Flat) {
                    TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
                } else {
                    TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
                }
            } else if (rightEdgeType == HexEdgeType.Slope) {
                if (leftEdgeType == HexEdgeType.Flat) {
                    TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                } else {
                    TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                }
            } else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                if (leftCell.Elevation < rightCell.Elevation) {
                    TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                } else {
                    TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
                }
            } else {
                AddTriangle(bottom, left, right);
                AddTriangleColor(bottomCell.Color, leftCell.Color, rightCell.Color);
            }
        } // TriangulateCorner

        private void TriangulateEdgeTerraces(EdgeVertices begin, HexCell beginCell, EdgeVertices end, HexCell endCell) {
            var e2 = EdgeVertices.TerraceLerp(begin, end, 1);
            var c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, 1);

            TriangulateEdgeStrip(begin, beginCell.Color, e2, c2);

            for (var i = 2; i < HexMetrics.terraceSteps; i++) {
                var e1 = e2;
                var c1 = c2;
                e2 = EdgeVertices.TerraceLerp(begin, end, i);
                c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i);
                TriangulateEdgeStrip(e1, c1, e2, c2);
            }

            TriangulateEdgeStrip(e2, c2, end, endCell.Color);
        } // TriangulateEdgeTerraces

        private void TriangulateCornerTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
            var v3 = HexMetrics.TerraceLerp(begin, left, 1);
            var v4 = HexMetrics.TerraceLerp(begin, right, 1);
            var c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);
            var c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, 1);

            AddTriangle(begin, v3, v4);
            AddTriangleColor(beginCell.Color, c3, c4);

            for (var i = 2; i < HexMetrics.terraceSteps; i++) {
                var v1 = v3;
                var v2 = v4;
                var c1 = c3;
                var c2 = c4;
                v3 = HexMetrics.TerraceLerp(begin, left, i);
                v4 = HexMetrics.TerraceLerp(begin, right, i);
                c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
                c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, i);
                AddQuad(v1, v2, v3, v4);
                AddQuadColor(c1, c2, c3, c4);
            }

            AddQuad(v3, v4, left, right);
            AddQuadColor(c3, c4, leftCell.Color, rightCell.Color);
        } // TriangulateCornerTerraces

        private void TriangulateCornerTerracesCliff(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
            var b = 1f / (rightCell.Elevation - beginCell.Elevation);
            if (b < 0) {
                b = -b;
            }

            var boundary = Vector3.Lerp(Perturb(begin), Perturb(right), b);
            var boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);

            TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
            }else {
                AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
                AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
            }
        } // TriangulateCornerTerracesCliff

        private void TriangulateCornerCliffTerraces(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
            var b = 1f / (leftCell.Elevation - beginCell.Elevation);
            if (b < 0) {
                b = -b;
            }

            var boundary = Vector3.Lerp(Perturb(begin), Perturb(left), b);
            var boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, b);

            TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);

            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
            } else {
                AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
                AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
            }
        } // TriangulateCornerCliffTerraces

        private void TriangulateBoundaryTriangle(Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 boundary, Color boundaryColor) {
            var v2 = Perturb(HexMetrics.TerraceLerp(begin, left, 1));
            var c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);

            AddTriangleUnperturbed(Perturb(begin), v2, boundary);
            AddTriangleColor(beginCell.Color, c2, boundaryColor);

            for (var i = 2; i < HexMetrics.terraceSteps; i++) {
                var v1 = v2;
                var c1 = c2;
                v2 = Perturb(HexMetrics.TerraceLerp(begin, left, i));
                c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
                AddTriangleUnperturbed(v1, v2, boundary);
                AddTriangleColor(c1, c2, boundaryColor);
            }

            AddTriangleUnperturbed(v2, Perturb(left), boundary);
            AddTriangleColor(c2, leftCell.Color, boundaryColor);
        } // TriangulateBoundaryTriangle

        private void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color color) {
            AddTriangle(center, edge.v1, edge.v2);
            AddTriangleColor(color);
            AddTriangle(center, edge.v2, edge.v3);
            AddTriangleColor(color);
            AddTriangle(center, edge.v3, edge.v4);
            AddTriangleColor(color);
            AddTriangle(center, edge.v4, edge.v5);
            AddTriangleColor(color);
        } // TriangulateEdgeFan

        private void TriangulateEdgeStrip(EdgeVertices e1, Color c1, EdgeVertices e2, Color c2) {
            AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
            AddQuadColor(c1, c2);
            AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
            AddQuadColor(c1, c2);
            AddQuad(e1.v3, e1.v3, e2.v3, e2.v3);
            AddQuadColor(c1, c2);
            AddQuad(e1.v4, e1.v5, e2.v4, e2.v5);
            AddQuadColor(c1, c2);
        } // TriangulateEdgeStrip

        private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
            var vertexIndex = m_vertices.Count;
            m_vertices.Add(Perturb(v1));
            m_vertices.Add(Perturb(v2));
            m_vertices.Add(Perturb(v3));
            m_triangles.Add(vertexIndex);
            m_triangles.Add(vertexIndex + 1);
            m_triangles.Add(vertexIndex + 2);
        } // AddTriangle

        private void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3) {
            var vertexIndex = m_vertices.Count;
            m_vertices.Add(v1);
            m_vertices.Add(v2);
            m_vertices.Add(v3);
            m_triangles.Add(vertexIndex);
            m_triangles.Add(vertexIndex + 1);
            m_triangles.Add(vertexIndex + 2);
        } // AddTriangleUnperturbed

        private void AddTriangleColor(Color color) {
            m_colors.Add(color);
            m_colors.Add(color);
            m_colors.Add(color);
        } // AddTriangleColor

        private void AddTriangleColor(Color c1, Color c2, Color c3) {
            m_colors.Add(c1);
            m_colors.Add(c2);
            m_colors.Add(c3);
        } // AddTriangleColor

        private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
            var vertexIndex = m_vertices.Count;
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
        } // AddQuad

        private void AddQuadColor(Color c1, Color c2) {
            m_colors.Add(c1);
            m_colors.Add(c1);
            m_colors.Add(c2);
            m_colors.Add(c2);
        } // AddQuadColor

        private void AddQuadColor(Color c1, Color c2, Color c3, Color c4) {
            m_colors.Add(c1);
            m_colors.Add(c2);
            m_colors.Add(c3);
            m_colors.Add(c4);
        } //AddQuadColor

        private void AddQuadColor(Color color) {
            m_colors.Add(color);
            m_colors.Add(color);
            m_colors.Add(color);
            m_colors.Add(color);
        } // AddQuadColor ()
        
        private Vector3 Perturb(Vector3 position) {
            var sample = HexMetrics.SampleNoise(position);
            position.x += (sample.x * 2f - 1f) * HexMetrics.cellPerturbStrength;
            position.z += (sample.z * 2f - 1f) * HexMetrics.cellPerturbStrength;
            return position;
        } // Perturb
        
    } // Class HexMesh
    
} // Namespace Arkayns Reckon HexMap