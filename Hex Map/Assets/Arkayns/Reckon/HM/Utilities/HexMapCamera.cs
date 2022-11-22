using UnityEngine;

namespace Arkayns.Reckon.HM {
    
    public class HexMapCamera : MonoBehaviour {
    
        // -- Variables --
        private Transform m_swivel, m_stick;

        // -- Built-In Methods --
        private void Awake() {
            m_swivel = transform.GetChild(0);
            m_stick = m_swivel.GetChild(0);
        } // Awake
        
    } // Class HexMapCamera
    
} // Namespace Arkayns Reckon HexMap