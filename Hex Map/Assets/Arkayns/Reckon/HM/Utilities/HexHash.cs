using UnityEngine;

namespace Arkayns.Reckon.HM {

    public struct HexHash {

        // -- Variables --
        public float a, b, c;
        
        // -- Method --
        public static HexHash Create () {
            HexHash hash;
            hash.a = Random.value * 0.999f;
            hash.b = Random.value * 0.999f;
            hash.c = Random.value * 0.999f;
            return hash;
        } // Create ()
        
    } // Struct HexHash

} // Namespace Arkayns Reckon HM