using UnityEngine;

namespace Arkayns.Reckon.HM {

    public struct HexHash {

        // -- Variables --
        public float a, b;
        
        // -- Method --
        public static HexHash Create () {
            HexHash hash;
            hash.a = Random.value;
            hash.b = Random.value;
            return hash;
        } // Create ()
        
    } // Struct HexHash

} // Namespace Arkayns Reckon HM