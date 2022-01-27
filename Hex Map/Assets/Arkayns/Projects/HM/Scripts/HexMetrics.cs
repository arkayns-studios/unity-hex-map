using UnityEngine;

namespace Arkayns.HM {
    
    public static class HexMetrics {
        public const float OuterRadius = 10f;
        public const float InnerRadius = OuterRadius * 0.866025404f; // √e2−(e2)2 = √3e24 = e√32 ≈ 0.886e
        
        private static readonly Vector3[] Corners = {
            new Vector3(0f, 0f, OuterRadius), 
            new Vector3(InnerRadius, 0f, 0.5f * OuterRadius), 
            new Vector3(InnerRadius, 0f, -0.5f * OuterRadius),
            new Vector3(0f, 0f, -OuterRadius), 
            new Vector3(-InnerRadius, 0f, -0.5f * OuterRadius), 
            new Vector3(-InnerRadius, 0f, 0.5f * OuterRadius),
            new Vector3(0f, 0f, OuterRadius)
        };

        public static Vector3 GetFirstCorner(HexDirection direction) {
            return Corners[(int) direction];
        } // Static GetFirstCorner
        
        public static Vector3 GetSecondCorner(HexDirection direction) {
            return Corners[(int) direction + 1];
        } // Static GetSecondCorner
        
    } // Class HexMetrics
    
} // Namespace 

