﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Arkayns.Reckon.HM {
    
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexMesh : MonoBehaviour {
        
        // -- Variables --
        [NonSerialized] private List<Vector3> m_vertices = new();
        [NonSerialized] private List<Color> m_colors = new();
        [NonSerialized] private List<int> m_triangles = new();
        [NonSerialized] private List<Vector2> m_uvs = new();

        private Mesh m_hexMesh;
        private MeshCollider m_meshCollider;

        public bool useCollider, useColors, useUVCoordinates;
        
        // -- Built-In Methods --
        private void Awake() {
            GetComponent<MeshFilter>().mesh = m_hexMesh = new Mesh();
            if(useCollider) m_meshCollider = gameObject.AddComponent<MeshCollider>();
            m_hexMesh.name = "Hex Mesh";
        } // Awake ()
        
        // -- Methods --
        public void Clear() {
            m_hexMesh.Clear();
            m_vertices = ListPool<Vector3>.Get();
            if(useColors) m_colors = ListPool<Color>.Get();
            if (useUVCoordinates) m_uvs = ListPool<Vector2>.Get();
            m_triangles = ListPool<int>.Get();
        } // Clear ()
        
        public void Apply() {
            m_hexMesh.SetVertices(m_vertices);
            ListPool<Vector3>.Add(m_vertices);
            
            if (useColors) {
                m_hexMesh.SetColors(m_colors);
                ListPool<Color>.Add(m_colors);
            }

            if (useUVCoordinates) {
                m_hexMesh.SetUVs(0, m_uvs);
                ListPool<Vector2>.Add(m_uvs);
            }

            m_hexMesh.SetTriangles(m_triangles, 0);
            ListPool<int>.Add(m_triangles);
            
            m_hexMesh.RecalculateNormals();
            if (useCollider) m_meshCollider.sharedMesh = m_hexMesh;
        } // Apply ()

        public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
            var vertexIndex = m_vertices.Count;
            m_vertices.Add(HexMetrics.Perturb(v1));
            m_vertices.Add(HexMetrics.Perturb(v2));
            m_vertices.Add(HexMetrics.Perturb(v3));
            m_triangles.Add(vertexIndex);
            m_triangles.Add(vertexIndex + 1);
            m_triangles.Add(vertexIndex + 2);
        } // AddTriangle ()

        public void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3) {
            var vertexIndex = m_vertices.Count;
            m_vertices.Add(v1);
            m_vertices.Add(v2);
            m_vertices.Add(v3);
            m_triangles.Add(vertexIndex);
            m_triangles.Add(vertexIndex + 1);
            m_triangles.Add(vertexIndex + 2);
        } // AddTriangleUnperturbed ()

        public void AddTriangleColor(Color color) {
            m_colors.Add(color);
            m_colors.Add(color);
            m_colors.Add(color);
        } // AddTriangleColor ()

        public void AddTriangleColor(Color c1, Color c2, Color c3) {
            m_colors.Add(c1);
            m_colors.Add(c2);
            m_colors.Add(c3);
        } // AddTriangleColor ()

        public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector2 uv3) {
            m_uvs.Add(uv1);
            m_uvs.Add(uv2);
            m_uvs.Add(uv3);
        } // AddTriangleUV ()
        
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
        } // AddQuad ()

        public void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector3 uv4) {
            m_uvs.Add(uv1);
            m_uvs.Add(uv2);
            m_uvs.Add(uv3);
            m_uvs.Add(uv4);
        } // AddQuadUV ()
        
        public void AddQuadUV(float uMin, float uMax, float vMin, float vMax) {
            m_uvs.Add(new Vector2(uMin, vMin));
            m_uvs.Add(new Vector2(uMax, vMin));
            m_uvs.Add(new Vector2(uMin, vMax));
            m_uvs.Add(new Vector2(uMax, vMax));
        } // AddQuadUV ()
        
        public void AddQuadColor(Color c1, Color c2) {
            m_colors.Add(c1);
            m_colors.Add(c1);
            m_colors.Add(c2);
            m_colors.Add(c2);
        } // AddQuadColor ()

        public void AddQuadColor(Color c1, Color c2, Color c3, Color c4) {
            m_colors.Add(c1);
            m_colors.Add(c2);
            m_colors.Add(c3);
            m_colors.Add(c4);
        } //AddQuadColor ()

        public void AddQuadColor(Color color) {
            m_colors.Add(color);
            m_colors.Add(color);
            m_colors.Add(color);
            m_colors.Add(color);
        } // AddQuadColor ()

    } // Class HexMesh
    
} // Namespace Arkayns Reckon HexMap