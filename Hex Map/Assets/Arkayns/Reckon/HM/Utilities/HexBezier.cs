using UnityEngine;

namespace Arkayns.Reckon.HM {

    public static class HexBezier {

        // -- Methods --
        public static Vector3 GetPoint (Vector3 a, Vector3 b, Vector3 c, float t) {
            var r = 1f - t;
            return r * r * a + 2f * r * t * b + t * t * c;
        } // GetPoint ()

    } // Class HexBezier

} // Namespace Arkayns Reckon HM