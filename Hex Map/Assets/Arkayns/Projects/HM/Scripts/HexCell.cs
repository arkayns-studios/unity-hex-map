using UnityEngine;

namespace Arkayns.HM {
    
    public class HexCell : MonoBehaviour {
        public HexCoordinates coordinates;
        public Color color;
        public int elevation;
        [SerializeField] private HexCell[] m_neighbors;

        public HexCell GetNeighbor(HexDirection direction) {
            return m_neighbors[(int) direction];
        } // GetNeighbor
        
        public void SetNeighbor(HexDirection direction, HexCell cell) {
            m_neighbors[(int) direction] = cell;
            cell.m_neighbors[(int) direction.Opposite()] = this;
        } // GetNeighbor
        
    } // Class HexCell
    
} // Namespace Arkayns HM
