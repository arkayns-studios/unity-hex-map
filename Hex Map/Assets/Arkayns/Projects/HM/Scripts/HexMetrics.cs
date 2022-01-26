using UnityEngine;

namespace Arkayns.HM {
    
    public static class HexMetrics {
        public const float outerRadius = 10f;
        public const float innerRadius = outerRadius * 0.866025404f; // √e2−(e2)2 = √3e24 = e√32 ≈ 0.886e
        
        public static Vector3[] corners = {
            new Vector3(0f, 0f, outerRadius), new Vector3(innerRadius, 0f, 0.5f * outerRadius), new Vector3(innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(0f, 0f, -outerRadius), new Vector3(-innerRadius, 0f, -0.5f * outerRadius), new Vector3(-innerRadius, 0f, 0.5f * outerRadius)
        };
        
    } // Class HexMetrics
    
} // Namespace 

