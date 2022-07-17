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
            Vector3 center = cell.transform.localPosition;

            Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
            Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);
            
            AddTriangle(center, v1, v2);
            AddTriangleColor(cell.color);
            
            if (direction <= HexDirection.SE)
                TriangulateConnection(direction, cell, v1, v2);
        } // Triangulate

        private void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2) {
            HexCell neighbor = cell.GetNeighbor(direction);
            if(neighbor == null) return;
		
            Vector3 bridge = HexMetrics.GetBridge(direction);
            Vector3 v3 = v1 + bridge;
            Vector3 v4 = v2 + bridge;
            v3.y = v4.y = neighbor.Elevation * HexMetrics.ElevationStep;
            
            HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
            if (direction <= HexDirection.E && nextNeighbor != null) {
                Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
                v5.y = nextNeighbor.Elevation * HexMetrics.ElevationStep;
                AddTriangle(v2, v4, v5);
                AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
            }

            if (cell.GetEdgeType(direction) == HexEdgeType.Slope) {
                TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbor);
            } else {
                AddQuad(v1, v2, v3, v4);
                AddQuadColor(cell.color, neighbor.color);
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
        
        /// <summary> Add vertices in order, it also adds the indices of those vertices to form a triangle </summary>
        private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
            int vertexIndex = m_vertices.Count;
            m_vertices.Add(v1);
            m_vertices.Add(v2);
            m_vertices.Add(v3);
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

        
        private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
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
        
    } // Class HexMesh
    
} // Namespace Arkayns HM