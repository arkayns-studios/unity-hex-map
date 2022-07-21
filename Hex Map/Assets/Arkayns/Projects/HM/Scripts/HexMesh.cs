using System.Collections.Generic;
using UnityEngine;

namespace Arkayns.HM {
    
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexMesh : MonoBehaviour {
        
        private Mesh m_hexMesh;
        private MeshCollider m_meshCollider;
        
        private List<Vector3> m_vertices;
        private List<int> m_triangles;
        private List<Color> m_colors;

        
        private void Awake() {
            GetComponent<MeshFilter>().mesh = m_hexMesh = new Mesh();
            m_meshCollider = GetComponent<MeshCollider>();
            
            m_hexMesh.name = "Hex Mesh";
            m_vertices = new List<Vector3>();
            m_triangles = new List<int>();
            m_colors = new List<Color>();
        } // Awake

        
        /// <summary> Assign the generated vertices and triangles to the mesh, and recalculate the mesh normals </summary>
        public void Triangulate(HexCell[] cells) {
            m_hexMesh.Clear();
            m_vertices.Clear();
            m_triangles.Clear();
            m_colors.Clear();

            foreach (HexCell cell in cells)
                Triangulate(cell);

            m_hexMesh.vertices = m_vertices.ToArray();
            m_hexMesh.triangles = m_triangles.ToArray();
            m_hexMesh.colors = m_colors.ToArray();
            m_hexMesh.RecalculateNormals();
            
            m_meshCollider.sharedMesh = m_hexMesh;
        } // Triangulate

        /// <summary> Loop through all HexDirection </summary>
        private void Triangulate(HexCell cell) {
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++) 
                Triangulate(d, cell);
        } // Triangulate
        
        /// <summary> Get the direction and add the Triangle and Color </summary>
        private void Triangulate(HexDirection direction, HexCell cell) {
            Vector3 center = cell.Position;

            EdgeVertices e = new EdgeVertices(center + HexMetrics.GetFirstSolidCorner(direction), center + HexMetrics.GetSecondSolidCorner(direction));
            
            TriangulateEdgeFan(center, e, cell.color);
            
            if (direction <= HexDirection.SE)
                TriangulateConnection(direction, cell, e);
        } // Triangulate

        private void TriangulateConnection(HexDirection direction, HexCell cell, EdgeVertices e1) {
            HexCell neighbor = cell.GetNeighbor(direction);
            if(neighbor == null) return;
		
            Vector3 bridge = HexMetrics.GetBridge(direction);
            bridge.y = neighbor.Position.y - cell.Position.y;
            EdgeVertices e2 = new EdgeVertices(e1.v1 + bridge, e1.v4 + bridge);
            
            if (cell.GetEdgeType(direction) == HexEdgeType.Slope) TriangulateEdgeTerraces(e1.v1, e1.v4, cell, e2.v1, e2.v4, neighbor);
            else TriangulateEdgeStrip(e1, cell.color, e2, neighbor.color);
            
            HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
            if (direction <= HexDirection.E && nextNeighbor != null) {
                Vector3 v5 = e1.v4 + HexMetrics.GetBridge(direction.Next());
                v5.y = nextNeighbor.Position.y;
                
                if (cell.Elevation <= neighbor.Elevation) {
                    if (cell.Elevation <= nextNeighbor.Elevation) TriangulateCorner(e1.v4, cell, e2.v4, neighbor, v5, nextNeighbor);
                    else TriangulateCorner(v5, nextNeighbor, e1.v4, cell, e2.v4, neighbor);
                } else if (neighbor.Elevation <= nextNeighbor.Elevation) TriangulateCorner(e1.v4, neighbor, v5, nextNeighbor, e2.v4, cell);
                else TriangulateCorner(v5, nextNeighbor, e2.v4, cell, e1.v4, neighbor);
            }

            
        } // TriangulateConnection

        private void TriangulateEdgeTerraces(Vector3 beginLeft, Vector3 beginRight, HexCell beginCell, Vector3 endLeft, Vector3 endRight, HexCell endCell) {
            Vector3 v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, 1);
            Vector3 v4 = HexMetrics.TerraceLerp(beginRight, endRight, 1);
            Color c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, 1);
            
            AddQuad(beginLeft, beginRight, v3, v4);
            AddQuadColor(beginCell.color, c2);

            for (int i = 0; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c2;
                
                v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
                v4 = HexMetrics.TerraceLerp(beginRight, endRight, i);
                c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i);
                
                AddQuad(v1, v2, v3, v4);
                AddQuadColor(c1, c2);
            }
            
            AddQuad(v3, v4, endLeft, endRight);
            AddQuadColor(c2, endCell.color);
        } // TriangulateEdgeTerraces
        
        private void TriangulateEdgeFan (Vector3 center, EdgeVertices edge, Color color) {
            AddTriangle(center, edge.v1, edge.v2);
            AddTriangleColor(color);
            AddTriangle(center, edge.v2, edge.v3);
            AddTriangleColor(color);
            AddTriangle(center, edge.v3, edge.v4);
            AddTriangleColor(color);
        } // TriangulateEdgeFan
        
        private void TriangulateEdgeStrip (EdgeVertices e1, Color c1, EdgeVertices e2, Color c2) {
            AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
            AddQuadColor(c1, c2);
            AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
            AddQuadColor(c1, c2);
            AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
            AddQuadColor(c1, c2);
        } // TriangulateEdgeStrip
        
        
        /// <summary> Add vertices in order, it also adds the indices of those vertices to form a triangle </summary>
        private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
            int vertexIndex = m_vertices.Count;
            m_vertices.Add(Perturb(v1));
            m_vertices.Add(Perturb(v2));
            m_vertices.Add(Perturb(v3));
            
            m_triangles.Add(vertexIndex);
            m_triangles.Add(vertexIndex + 1);
            m_triangles.Add(vertexIndex + 2);
        } // AddTriangle

        /// <summary> Add color data for each triangle </summary>
        private void AddTriangleColor(Color color) {
            m_colors.Add(color);
            m_colors.Add(color);
            m_colors.Add(color);
        } // AddTriangleColor
        
        /// <summary> Add color data for each triangle </summary>
        private void AddTriangleColor(Color c1, Color c2, Color c3) {
            m_colors.Add(c1);
            m_colors.Add(c2);
            m_colors.Add(c3);
        } // AddTriangleColor
        
        private void TriangulateCorner (Vector3 bottom, HexCell bottomCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
            HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
            HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);
            
            if (leftEdgeType == HexEdgeType.Slope) {
                if (rightEdgeType == HexEdgeType.Slope) 
                    TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                else if (rightEdgeType == HexEdgeType.Flat)
                    TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
                else 
                    TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
                
            } else if (rightEdgeType == HexEdgeType.Slope) {
                if (leftEdgeType == HexEdgeType.Flat)
                    TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                else
                    TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                
            } else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                if (leftCell.Elevation < rightCell.Elevation) 
                    TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                else 
                    TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
            } else {
                AddTriangle(bottom, left, right);
                AddTriangleColor(bottomCell.color, leftCell.color, rightCell.color);
            }
            
        } // TriangulateCorner
        
        private void TriangulateCornerTerraces (Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
            Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
            Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
            Color c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);
            Color c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, 1);

            AddTriangle(begin, v3, v4);
            AddTriangleColor(beginCell.color, c3, c4);
            
            for (int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color c1 = c3;
                Color c2 = c4;
                v3 = HexMetrics.TerraceLerp(begin, left, i);
                v4 = HexMetrics.TerraceLerp(begin, right, i);
                c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
                c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, i);
                AddQuad(v1, v2, v3, v4);
                AddQuadColor(c1, c2, c3, c4);
            }
            
            AddQuad(v3, v4, left, right);
            AddQuadColor(c3, c4, leftCell.color, rightCell.color);
        } // TriangulateCornerTerraces
        
        private void TriangulateCornerTerracesCliff (Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
            float b = 1f / (rightCell.Elevation - beginCell.Elevation);
            if (b < 0) b = -b;
            Vector3 boundary = Vector3.Lerp(begin, right, b);
            Color boundaryColor = Color.Lerp(beginCell.color, rightCell.color, b);
            
            TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);
            
            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
            } else {
                AddTriangle(left, right, boundary);
                AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
            }
        } // TriangulateCornerTerracesCliff
        
        private void TriangulateCornerCliffTerraces (Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell) {
            float b = 1f / (leftCell.Elevation - beginCell.Elevation);
            if (b < 0) b = -b;
            Vector3 boundary = Vector3.Lerp(begin, left, b);
            Color boundaryColor = Color.Lerp(beginCell.color, leftCell.color, b);
            
            TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);
            
            if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
            } else {
                AddTriangle(left, right, boundary);
                AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
            }
        } // TriangulateCornerCliffTerraces
        
        private void TriangulateBoundaryTriangle (Vector3 begin, HexCell beginCell, Vector3 left, HexCell leftCell, Vector3 boundary, Color boundaryColor) {
            Vector3 v2 = HexMetrics.TerraceLerp(begin, left, 1);
            Color c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);

            AddTriangle(begin, v2, boundary);
            AddTriangleColor(beginCell.color, c2, boundaryColor);

            for (int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v2;
                Color c1 = c2;
                v2 = HexMetrics.TerraceLerp(begin, left, i);
                c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
                AddTriangle(v1, v2, boundary);
                AddTriangleColor(c1, c2, boundaryColor);
            }

            AddTriangle(v2, left, boundary);
            AddTriangleColor(c2, leftCell.color, boundaryColor);
        } // TriangulateBoundaryTriangle
        
        
        private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
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
        } // AddQuad

        /// <summary> Add color data for each quad </summary>
        private void AddQuadColor(Color c1, Color c2, Color c3, Color c4) {
            m_colors.Add(c1);
            m_colors.Add(c2);
            m_colors.Add(c3);
            m_colors.Add(c4);
        } // AddQuadColor

        /// <summary> Add color data for each quad </summary>
        private void AddQuadColor(Color c1, Color c2) {
            m_colors.Add(c1);
            m_colors.Add(c1);
            m_colors.Add(c2);
            m_colors.Add(c2);
        } // AddQuadColor
        
        
        private Vector3 Perturb (Vector3 position) {
            Vector4 sample = HexMetrics.SampleNoise(position);
            position.x += (sample.x * 2f - 1f) * HexMetrics.CellPerturbStrength;
            // position.y += (sample.y * 2f - 1f) * HexMetrics.CellPerturbStrength;
            position.z += (sample.z * 2f - 1f) * HexMetrics.CellPerturbStrength;
            return position;
        } // Perturb
        
    } // Class HexMesh
    
} // Namespace Arkayns HM