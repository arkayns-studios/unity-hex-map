using UnityEngine;

namespace Arkayns.Reckon.HM {

    public class HexUnit : MonoBehaviour {

        // -- Variables --
        private HexCell m_location;

        private float m_orientation;
        
        // -- Properties --
        public HexCell Location {
            get => m_location;
            set {
                m_location = value;
                value.Unit = this;
                transform.localPosition = value.Position;
            }
        } // Location
        
        public float Orientation {
            get => m_orientation;
            set {
                m_orientation = value;
                transform.localRotation = Quaternion.Euler(0f, value, 0f);
            }
        } // Orientation
        
        // -- Methods --
        public void ValidateLocation () {
            transform.localPosition = m_location.Position;
        } // ValidateLocation ()
        
        public void Die () {
            m_location.Unit = null;
            Destroy(gameObject);
        } // Die ()
        
    } // Class HexUnit

} // Namespace Arkayns Reckon HM