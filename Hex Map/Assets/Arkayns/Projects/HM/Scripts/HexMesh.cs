using System.Collections.Generic;
using UnityEngine;

namespace Arkayns.HM {
    
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexMesh : MonoBehaviour {
        private Mesh m_hexMesh;
        private List<Vector3> m_vertices;
        private List<int> m_triangles;

        private void Awake() {
            GetComponent<MeshFilter>().mesh = m_hexMesh = new Mesh();
            m_hexMesh.name = "Hex Mesh";
            m_vertices = new List<Vector3>();
            m_triangles = new List<int>();
        } // Awake

        /// <summary> Assign the generated vertices and triangles to the mesh, and recalculate the mesh normals </summary>
        public void Triangulate(HexCell[] cells) {
            m_hexMesh.Clear();
            m_vertices.Clear();
            m_triangles.Clear();

            for (int i = 0; i < cells.Length; i++) 
                Triangulate(cells[i]);

            m_hexMesh.vertices = m_vertices.ToArray();
            m_hexMesh.triangles = m_triangles.ToArray();
            m_hexMesh.RecalculateNormals();
        } // Triangulate

        /// <summary> Loop through all six triangles </summary>
        private void Triangulate(HexCell cell) {
            Vector3 center = cell.transform.localPosition;
            for (int i = 0; i < 6; i++)
                AddTriangle(center, center + HexMetrics.Corners[i], center + HexMetrics.Corners[i + 1]);
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
        
    } // Class HexMesh
    
} // Namespace Arkayns HM