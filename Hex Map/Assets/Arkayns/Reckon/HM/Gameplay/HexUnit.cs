using System.IO;
using UnityEngine;

namespace Arkayns.Reckon.HM {

    public class HexUnit : MonoBehaviour {

        // -- Variables --
        public static HexUnit unitPrefab;
        private HexCell m_location;
        private float m_orientation;
        
        // -- Properties --
        public HexCell Location {
            get => m_location;
            set {
                if (m_location) m_location.Unit = null;
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
        public void Save (BinaryWriter writer) {
            m_location.coordinates.Save(writer);
            writer.Write(m_orientation);
        } // Save ()
        
        public static void Load (BinaryReader reader, HexGrid grid) {
            var coordinates = HexCoordinates.Load(reader);
            var orientation = reader.ReadSingle();
            grid.AddUnit(Instantiate(unitPrefab), grid.GetCell(coordinates), orientation);
        } // Load ()
        
        public void ValidateLocation () {
            transform.localPosition = m_location.Position;
        } // ValidateLocation ()
        
        public bool IsValidDestination (HexCell cell) {
            return !cell.IsUnderwater && !cell.Unit;
        } // IsValidDestination ()
        
        public void Die () {
            m_location.Unit = null;
            Destroy(gameObject);
        } // Die ()

    } // Class HexUnit

} // Namespace Arkayns Reckon HM