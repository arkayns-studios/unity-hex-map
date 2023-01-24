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
        private int waterLevel;
        private bool hasIncomingRiver, hasOutgoingRiver;
        private HexDirection incomingRiver, outgoingRiver;
        private int urbanLevel;
        
        [SerializeField] private HexCell[] neighbors;
        [SerializeField] private bool[] roads;
        
        // -- Properties --
        
        /// <summary> Gets or Sets the color of the hex, it updates and refresh the hex cell if the value is different </summary>
        public Color Color {
            get => color;
            set {
                if (color == value) return;
                color = value;
                Refresh();
            }
        } // Color

        /// <summary> Gets or Sets the elevation of the hex cell and updates the position, river and road accordingly </summary>
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

                ValidateRivers();
                for (var i = 0; i < roads.Length; i++) {
                    if (roads[i] && GetElevationDifference((HexDirection)i) > 1) SetRoad(i, false);
                }
                
                Refresh();
            }
        } // Elevation

        /// <summary> Gets or Sets the water level of the hex cell and refresh it when its value is changed </summary>
        public int WaterLevel {
            get => waterLevel;
            set {
                if (waterLevel == value) return;
                waterLevel = value;
                ValidateRivers();
                Refresh();
            }
        } // WaterLevel
        
        /// <summary> Returns true if the water level of the hex cell is greater than its elevation </summary>
        public bool IsUnderwater => waterLevel > elevation;

        /// <summary> Returns true if the hex cell has an incoming river </summary>
        public bool HasIncomingRiver => hasIncomingRiver;

        /// <summary> Returns true if the hex cell has an outgoing river </summary>
        public bool HasOutgoingRiver => hasOutgoingRiver;

        /// <summary>The HasRiver property returns true if the hex has either an incoming or outgoing river </summary>
        public bool HasRiver => hasIncomingRiver || hasOutgoingRiver;

        /// <summary> Returns true if the hex cell has either an incoming or outgoing river but not both </summary>
        public bool HasRiverBeginOrEnd => hasIncomingRiver != hasOutgoingRiver;

        /// <summary> Returns the direction of the river at the beginning or end of the hex cell depending on whether it has an incoming or outgoing river </summary>
        public HexDirection RiverBeginOrEndDirection => hasIncomingRiver ? incomingRiver : outgoingRiver;

        /// <summary> Returns the direction of the incoming river in the hex cell </summary>
        public HexDirection IncomingRiver => incomingRiver;

        /// <summary> Returns the direction of the outgoing river in the hex cell </summary>
        public HexDirection OutgoingRiver => outgoingRiver;

        /// <summary> Returns the local position of the hex cell </summary>
        public Vector3 Position => transform.localPosition;

        /// <summary> Returns the y-coordinate of the river surface calculated by adding elevation, river surface elevation offset and elevation step </summary>
        public float RiverSurfaceY => (elevation + HexMetrics.WaterElevationOffset) * HexMetrics.ElevationStep;

        /// <summary> Returns the y-coordinate of the water surface, which is calculated by adding the water level of the hex cell and the water elevation offset to the elevation step </summary>
        public float WaterSurfaceY => (waterLevel + HexMetrics.WaterElevationOffset) * HexMetrics.ElevationStep;

        /// <summary> Returns the y-coordinate of the stream bed, which is calculated by adding the elevation of the hex cell and the stream bed elevation offset to the elevation step </summary>
        public float StreamBedY => (elevation + HexMetrics.StreamBedElevationOffset) * HexMetrics.ElevationStep;

        /// <summary> Returns true if the hex cell has at least one road </summary>
        public bool HasRoads => roads.Any(road => road);
        
        public int UrbanLevel {
            get => urbanLevel;
            set {
                if (urbanLevel == value) return;
                urbanLevel = value;
                RefreshSelfOnly();
            }
        } // UrbanLevel
        
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

        private bool IsValidRiverDestination (HexCell neighbor) {
            return neighbor && (elevation >= neighbor.elevation || waterLevel == neighbor.elevation);
        } // IsValidRiverDestination ()
        
        private void ValidateRivers () {
            if (hasOutgoingRiver && !IsValidRiverDestination(GetNeighbor(outgoingRiver))) RemoveOutgoingRiver();
            if (hasIncomingRiver && !GetNeighbor(incomingRiver).IsValidRiverDestination(this)) RemoveIncomingRiver();
        } // ValidateRivers ()
        
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
            if (!IsValidRiverDestination(neighbor)) return;

            RemoveOutgoingRiver();
            if (hasIncomingRiver && incomingRiver == direction) RemoveIncomingRiver();

            hasOutgoingRiver = true;
            outgoingRiver = direction;

            neighbor.RemoveIncomingRiver();
            neighbor.hasIncomingRiver = true;
            neighbor.incomingRiver = direction.Opposite();

            SetRoad((int)direction, false);
        } // SetOutgoingRiver ()

        public bool HasRoadThroughEdge(HexDirection direction) {
            return roads[(int)direction];
        } // HasRoadThroughEdge ()

        public void AddRoad(HexDirection direction) {
            if (!roads[(int)direction] && !HasRiverThroughEdge(direction) && GetElevationDifference(direction) <= 1) 
                SetRoad((int)direction, true);
        } // AddRoad ()
        
        public void RemoveRoads() {
            for (var i = 0; i < neighbors.Length; i++) {
                if (!roads[i]) continue;
                SetRoad(i, false);
            }
        } // RemoveRoads ()

        private void SetRoad(int index, bool state) {
            roads[index] = state;
            neighbors[index].roads[(int)((HexDirection)index).Opposite()] = state;
            neighbors[index].RefreshSelfOnly();
            RefreshSelfOnly();
        } // SetRoad ()

        public int GetElevationDifference(HexDirection direction) {
            var difference = elevation - GetNeighbor(direction).elevation;
            return difference >= 0 ? difference : -difference;
        } // GetElevationDifference ()
        
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