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
            AddTriangle(center, center + HexMetrics.GetFirstCorner(direction), center + HexMetrics.GetSecondCorner(direction));
            AddTriangleColor(cell.color);
        } // Triangulate

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
        
    } // Class HexMesh
    
} // Namespace Arkayns HM