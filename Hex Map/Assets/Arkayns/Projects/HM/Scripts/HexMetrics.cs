using UnityEngine;

namespace Arkayns.HM {
    
    public static class HexMetrics {
        public const float OuterRadius = 10f;
        public const float InnerRadius = OuterRadius * 0.866025404f; // √e2−(e2)2 = √3e24 = e√32 ≈ 0.886e
        public const float SolidFactor = 0.75f;
        public const float BlendFactor = 1F - SolidFactor;
        public const float ElevationStep = 5f;
        public const int TerracesPerSlope = 2;
        public const int TerraceSteps = TerracesPerSlope * 2 + 1;
        public const float HorizontalTerraceStepSize = 1f / TerraceSteps;
        public const float VerticalTerraceStepSize = 1f / (TerraceSteps + 1);
        
        private static readonly Vector3[] Corners = {
            new Vector3(0f, 0f, OuterRadius), 
            new Vector3(InnerRadius, 0f, 0.5f * OuterRadius), 
            new Vector3(InnerRadius, 0f, -0.5f * OuterRadius),
            new Vector3(0f, 0f, -OuterRadius), 
            new Vector3(-InnerRadius, 0f, -0.5f * OuterRadius), 
            new Vector3(-InnerRadius, 0f, 0.5f * OuterRadius),
            new Vector3(0f, 0f, OuterRadius)
        };

        public static Vector3 GetBridge(HexDirection direction) {
            return (Corners[(int)direction] + Corners[(int)direction + 1]) * BlendFactor;
        } // Static GetBridge
        
        public static Vector3 GetFirstCorner(HexDirection direction) {
            return Corners[(int) direction];
        } // Static GetFirstCorner
        
        public static Vector3 GetSecondCorner(HexDirection direction) {
            return Corners[(int) direction + 1];
        } // Static GetSecondCorner
        
        public static Vector3 GetFirstSolidCorner (HexDirection direction) {
            return Corners[(int)direction] * SolidFactor;
        } // Static GetFirstSolidCorner

        public static Vector3 GetSecondSolidCorner (HexDirection direction) {
            return Corners[(int)direction + 1] * SolidFactor;
        } // Static GetSecondSolidCorner

        public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step) {
            float h = step * HexMetrics.HorizontalTerraceStepSize;
            a.x += (b.x - a.x) * h;
            a.z += (b.z - a.z) * h;

            float v = ((step + 1) / 2) * HexMetrics.VerticalTerraceStepSize;
            a.y += (b.y - a.y) * v;
            return a;
        } // TerraceLerp

        public static Color TerraceLerp(Color a, Color b, int step) {
            float h = step * HexMetrics.HorizontalTerraceStepSize;
            return Color.Lerp(a, b, h);
        } // TerraceLerp
        
        public static HexEdgeType GetEdgeType (int elevation1, int elevation2) {
            if (elevation1 == elevation2) 
                return HexEdgeType.Flat;
            
            int delta = elevation2 - elevation1;
            if (delta == 1 || delta == -1)
                return HexEdgeType.Slope;
            
            return HexEdgeType.Cliff;
        } // GetEdgeType
        
    } // Class HexMetrics
    
} // Namespace 

