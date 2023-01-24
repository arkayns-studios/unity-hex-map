using UnityEngine;

namespace Arkayns.Reckon.HM {

    [System.Serializable]
    public struct HexFeatureCollection {

        // -- Variables --
        public Transform[] prefabs;

        // -- Method --
        public Transform Pick (float choice) {
            return prefabs[(int)(choice * prefabs.Length)];
        } // Pick ()

    } // Struct HexFeatureCollection

} // Namespace Arkayns Reckon HM