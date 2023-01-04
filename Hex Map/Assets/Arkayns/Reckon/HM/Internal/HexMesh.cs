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
        public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
            var vertexIndex = m_vertices.Count;
            m_vertices.Add(HexMetrics.Perturb(v1));
            m_vertices.Add(HexMetrics.Perturb(v2));
            m_vertices.Add(HexMetrics.Perturb(v3));
            m_triangles.Add(vertexIndex);
            m_triangles.Add(vertexIndex + 1);
            m_triangles.Add(vertexIndex + 2);
        } // AddTriangle

        public void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3) {
            var vertexIndex = m_vertices.Count;
            m_vertices.Add(v1);
            m_vertices.Add(v2);
            m_vertices.Add(v3);
            m_triangles.Add(vertexIndex);
            m_triangles.Add(vertexIndex + 1);
            m_triangles.Add(vertexIndex + 2);
        } // AddTriangleUnperturbed

        public void AddTriangleColor(Color color) {
            m_colors.Add(color);
            m_colors.Add(color);
            m_colors.Add(color);
        } // AddTriangleColor

        public void AddTriangleColor(Color c1, Color c2, Color c3) {
            m_colors.Add(c1);
            m_colors.Add(c2);
            m_colors.Add(c3);
        } // AddTriangleColor

        public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
            var vertexIndex = m_vertices.Count;
            m_vertices.Add(HexMetrics.Perturb(v1));
            m_vertices.Add(HexMetrics.Perturb(v2));
            m_vertices.Add(HexMetrics.Perturb(v3));
            m_vertices.Add(HexMetrics.Perturb(v4));
            m_triangles.Add(vertexIndex);
            m_triangles.Add(vertexIndex + 2);
            m_triangles.Add(vertexIndex + 1);
            m_triangles.Add(vertexIndex + 1);
            m_triangles.Add(vertexIndex + 2);
            m_triangles.Add(vertexIndex + 3);
        } // AddQuad

        public void AddQuadColor(Color c1, Color c2) {
            m_colors.Add(c1);
            m_colors.Add(c1);
            m_colors.Add(c2);
            m_colors.Add(c2);
        } // AddQuadColor

        public void AddQuadColor(Color c1, Color c2, Color c3, Color c4) {
            m_colors.Add(c1);
            m_colors.Add(c2);
            m_colors.Add(c3);
            m_colors.Add(c4);
        } //AddQuadColor

        public void AddQuadColor(Color color) {
            m_colors.Add(color);
            m_colors.Add(color);
            m_colors.Add(color);
            m_colors.Add(color);
        } // AddQuadColor ()

        public void Clear() {
            m_hexMesh.Clear();
            m_vertices.Clear();
            m_colors.Clear();
            m_triangles.Clear();
        } // Clear ()

        public void Apply() {
            m_hexMesh.SetVertices(m_vertices);
            m_hexMesh.SetColors(m_colors);
            m_hexMesh.SetTriangles(m_triangles, 0);
            m_hexMesh.RecalculateNormals();
            m_meshCollider.sharedMesh = m_hexMesh;
        } // Apply ()

    } // Class HexMesh
    
} // Namespace Arkayns Reckon HexMap