using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Arkayns.Reckon.HM {

    public class HexUnit : MonoBehaviour {

        // -- Variables --
        public static HexUnit unitPrefab;
        private HexCell m_location;
        private float m_orientation;
        private List<HexCell> m_pathToTravel;
        private const float TravelSpeed = 4f;
        
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
        
        // -- Built-In Methods --
        private void OnDrawGizmos () {
            if (m_pathToTravel == null || m_pathToTravel.Count == 0) return;
            for (var i = 1; i < m_pathToTravel.Count; i++) {
                var a = m_pathToTravel[i - 1].Position;
                var b = m_pathToTravel[i].Position;
                for (var t = 0f; t < 1f; t += 0.1f) {
                    Gizmos.DrawSphere(Vector3.Lerp(a, b, t), 2f);
                }
            }
        } // OnDrawGizmos()
        
        private void OnEnable () {
            if (m_location) transform.localPosition = m_location.Position;
        } // OnEnable ()
        
        // -- Custom Methods --
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
        
        public void Travel (List<HexCell> path) {
            Location = path[^1];
            m_pathToTravel = path;
            StopAllCoroutines();
            StartCoroutine(TravelPath());
        } // Travel ()
        
        private IEnumerator TravelPath () {
            for (var i = 1; i < m_pathToTravel.Count; i++) {
                var a = m_pathToTravel[i - 1].Position;
                var b = m_pathToTravel[i].Position;
                for (var t = 0f; t < 1f; t += Time.deltaTime * TravelSpeed) {
                    transform.localPosition = Vector3.Lerp(a, b, t);
                    yield return null;
                }
            }
        } // TravelPath ()

        public void Die () {
            m_location.Unit = null;
            Destroy(gameObject);
        } // Die ()

    } // Class HexUnit

} // Namespace Arkayns Reckon HM