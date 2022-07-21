using UnityEngine;

namespace Arkayns.HM {
    
    public class HexCell : MonoBehaviour {
        public HexCoordinates coordinates;
        public Color color;
        public int elevation;
        public RectTransform uiRect;
        [SerializeField] private HexCell[] m_neighbors;

        public int Elevation {
            get => elevation;
            set {
                elevation = value;
                var transformVar = transform;
                var position = transformVar.localPosition;
                position.y = value * HexMetrics.ElevationStep;
                position.y +=
                        (HexMetrics.SampleNoise(position).y * 2f - 1f) *
                        HexMetrics.ElevationPerturbStrength;
                transformVar.localPosition = position;

                var uiPosition = uiRect.localPosition;
                uiPosition.z = -position.y;
                uiRect.localPosition = uiPosition;
            }
        } // Elevation

        public Vector3 Position {
            get => transform.localPosition;
        } // Position
        
        public HexCell GetNeighbor(HexDirection direction) {
            return m_neighbors[(int) direction];
        } // GetNeighbor
        
        public void SetNeighbor(HexDirection direction, HexCell cell) {
            m_neighbors[(int) direction] = cell;
            cell.m_neighbors[(int) direction.Opposite()] = this;
        } // GetNeighbor
        
        public HexEdgeType GetEdgeType (HexDirection direction) {
            return HexMetrics.GetEdgeType(elevation, m_neighbors[(int)direction].elevation);
        } // GetEdgeType
        
        public HexEdgeType GetEdgeType (HexCell otherCell) {
            return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
        } // GetEdgeType
        
    } // Class HexCell
    
} // Namespace Arkayns HM
