using System.Linq;
using UnityEngine;

namespace Arkayns.Reckon.HM {

    public class HexCell : MonoBehaviour {

        // -- Variables --
        public HexCoordinates coordinates;
        public RectTransform uiRect;
        public HexGridChunk chunk;

        private Color color;
        private int elevation = int.MinValue;
        private bool hasIncomingRiver, hasOutgoingRiver;
        private HexDirection incomingRiver, outgoingRiver;

        [SerializeField] private HexCell[] neighbors;
        [SerializeField] private bool[] roads;
        
        // -- Properties --
        public Color Color {
            get => color;
            set {
                if (color == value) return;
                color = value;
                Refresh();
            }
        } // Color

        public int Elevation {
            get => elevation;
            set {
                if (elevation == value) return;

                elevation = value;
                var position = transform.localPosition;
                position.y = value * HexMetrics.ElevationStep;
                position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.ElevationPerturbStrength;
                transform.localPosition = position;

                var uiPosition = uiRect.localPosition;
                uiPosition.z = -position.y;
                uiRect.localPosition = uiPosition;

                if (hasOutgoingRiver && elevation < GetNeighbor(outgoingRiver).elevation) RemoveOutgoingRiver();
                if (hasIncomingRiver && elevation > GetNeighbor(incomingRiver).elevation) RemoveIncomingRiver();
                Refresh();
            }
        } // Elevation

        public bool HasIncomingRiver => hasIncomingRiver;

        public bool HasOutgoingRiver => hasOutgoingRiver;

        public bool HasRiver => hasIncomingRiver || hasOutgoingRiver;

        public bool HasRiverBeginOrEnd => hasIncomingRiver != hasOutgoingRiver;

        public HexDirection IncomingRiver => incomingRiver;

        public HexDirection OutgoingRiver => outgoingRiver;

        public Vector3 Position => transform.localPosition;

        public float RiverSurfaceY => (elevation + HexMetrics.RiverSurfaceElevationOffset) * HexMetrics.ElevationStep;

        public float StreamBedY => (elevation + HexMetrics.StreamBedElevationOffset) * HexMetrics.ElevationStep;

        public bool HasRoads => roads.Any(road => road);
        
        // -- Methods --
        public HexCell GetNeighbor(HexDirection direction) {
            return neighbors[(int)direction];
        } // GetNeighbor ()

        public void SetNeighbor(HexDirection direction, HexCell cell) {
            neighbors[(int)direction] = cell;
            cell.neighbors[(int)direction.Opposite()] = this;
        } // SetNeighbor ()
 
        public HexEdgeType GetEdgeType(HexDirection direction) {
            return HexMetrics.GetEdgeType(elevation, neighbors[(int)direction].elevation);
        } // GetEdgeType ()

        public HexEdgeType GetEdgeType(HexCell otherCell) {
            return HexMetrics.GetEdgeType(elevation, otherCell.elevation);
        } // GetEdgeType ()

        public bool HasRiverThroughEdge(HexDirection direction) {
            return hasIncomingRiver && incomingRiver == direction || hasOutgoingRiver && outgoingRiver == direction;
        } // HasRiverThroughEdge ()

        public void RemoveIncomingRiver() {
            if (!hasIncomingRiver) return;

            hasIncomingRiver = false;
            RefreshSelfOnly();

            var neighbor = GetNeighbor(incomingRiver);
            neighbor.hasOutgoingRiver = false;
            neighbor.RefreshSelfOnly();
        } // RemoveIncomingRiver ()

        public void RemoveOutgoingRiver() {
            if (!hasOutgoingRiver) return;

            hasOutgoingRiver = false;
            RefreshSelfOnly();

            var neighbor = GetNeighbor(outgoingRiver);
            neighbor.hasIncomingRiver = false;
            neighbor.RefreshSelfOnly();
        } // RemoveOutgoingRiver ()

        public void RemoveRiver() {
            RemoveOutgoingRiver();
            RemoveIncomingRiver();
        } // RemoveRiver ()

        public void SetOutgoingRiver(HexDirection direction) {
            if (hasOutgoingRiver && outgoingRiver == direction) return;

            var neighbor = GetNeighbor(direction);
            if (!neighbor || elevation < neighbor.elevation) return;

            RemoveOutgoingRiver();
            if (hasIncomingRiver && incomingRiver == direction) RemoveIncomingRiver();

            hasOutgoingRiver = true;
            outgoingRiver = direction;
            RefreshSelfOnly();

            neighbor.RemoveIncomingRiver();
            neighbor.hasIncomingRiver = true;
            neighbor.incomingRiver = direction.Opposite();
            neighbor.RefreshSelfOnly();
        } // SetOutgoingRiver ()

        public bool HasRoadThroughEdge(HexDirection direction) {
            return roads[(int)direction];
        } // HasRoadThroughEdge ()

        public void RemoveRoads() {
            for (var i = 0; i < neighbors.Length; i++) {
                if (!roads[i]) continue;
                roads[i] = false;
                neighbors[i].roads[(int)((HexDirection)i).Opposite()] = false;
                neighbors[i].RefreshSelfOnly();
                RefreshSelfOnly();
            }
        } // RemoveRoads ()
        
        private void Refresh() {
            if (!chunk) return;
            chunk.Refresh();
            foreach (var neighbor in neighbors) {
                if (neighbor != null && neighbor.chunk != chunk) neighbor.chunk.Refresh();
            }
        } // Refresh ()

        private void RefreshSelfOnly() {
            chunk.Refresh();
        } // RefreshSelfOnly ()

    } // Class HexCell

} // Namespace Arkayns Reckon HexMap