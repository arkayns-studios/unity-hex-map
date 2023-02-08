using System.Linq;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

namespace Arkayns.Reckon.HM {

    public class HexCell : MonoBehaviour {

        // -- Variables --
        public HexCoordinates coordinates;
        public RectTransform uiRect;
        public HexGridChunk chunk;

        private int m_terrainTypeIndex, m_waterLevel, m_elevation = int.MinValue;
        private HexDirection m_incomingRiver, m_outgoingRiver;
        private int m_urbanLevel, m_farmLevel, m_plantLevel, m_specialIndex;
        private bool m_walled;
        private int m_distance;

        private bool m_hasIncomingRiver, m_hasOutgoingRiver;

        [SerializeField] private HexCell[] neighbors;
        [SerializeField] private bool[] roads;
        
        // -- Properties --
        public int TerrainTypeIndex {
            get => m_terrainTypeIndex;
            set {
                if (m_terrainTypeIndex == value) return;
                m_terrainTypeIndex = value;
                Refresh();
            }
        } // TerrainTypeIndex
        
        /// <summary> Gets or Sets the m_elevation of the hex cell and updates the position, river and road accordingly </summary>
        public int Elevation {
            get => m_elevation;
            set {
                if (m_elevation == value) return;

                m_elevation = value;
                RefreshPosition();
                ValidateRivers();
                for (var i = 0; i < roads.Length; i++) {
                    if (roads[i] && GetElevationDifference((HexDirection)i) > 1) 
                        SetRoad(i, false);
                }
                Refresh();
            }
        } // Elevation

        /// <summary> Gets or Sets the water level of the hex cell and refresh it when its value is changed </summary>
        public int WaterLevel {
            get => m_waterLevel;
            set {
                if (m_waterLevel == value) return;
                m_waterLevel = value;
                ValidateRivers();
                RemoveRoads();
                Refresh();
            }
        } // WaterLevel
        
        /// <summary> Returns true if the water level of the hex cell is greater than its m_elevation </summary>
        public bool IsUnderwater => m_waterLevel > m_elevation;

        /// <summary> Returns true if the hex cell has an incoming river </summary>
        public bool HasIncomingRiver => m_hasIncomingRiver;

        /// <summary> Returns true if the hex cell has an outgoing river </summary>
        public bool HasOutgoingRiver => m_hasOutgoingRiver;

        /// <summary>The HasRiver property returns true if the hex has either an incoming or outgoing river </summary>
        public bool HasRiver => m_hasIncomingRiver || m_hasOutgoingRiver;

        /// <summary> Returns true if the hex cell has either an incoming or outgoing river but not both </summary>
        public bool HasRiverBeginOrEnd => m_hasIncomingRiver != m_hasOutgoingRiver;

        /// <summary> Returns the direction of the river at the beginning or end of the hex cell depending on whether it has an incoming or outgoing river </summary>
        public HexDirection RiverBeginOrEndDirection => m_hasIncomingRiver ? m_incomingRiver : m_outgoingRiver;

        /// <summary> Returns the direction of the incoming river in the hex cell </summary>
        public HexDirection IncomingRiver => m_incomingRiver;

        /// <summary> Returns the direction of the outgoing river in the hex cell </summary>
        public HexDirection OutgoingRiver => m_outgoingRiver;

        /// <summary> Returns the local position of the hex cell </summary>
        public Vector3 Position => transform.localPosition;

        /// <summary> Returns the y-coordinate of the river surface calculated by adding m_elevation, river surface m_elevation offset and m_elevation step </summary>
        public float RiverSurfaceY => (m_elevation + HexMetrics.WaterElevationOffset) * HexMetrics.ElevationStep;

        /// <summary> Returns the y-coordinate of the water surface, which is calculated by adding the water level of the hex cell and the water m_elevation offset to the m_elevation step </summary>
        public float WaterSurfaceY => (m_waterLevel + HexMetrics.WaterElevationOffset) * HexMetrics.ElevationStep;

        /// <summary> Returns the y-coordinate of the stream bed, which is calculated by adding the m_elevation of the hex cell and the stream bed m_elevation offset to the m_elevation step </summary>
        public float StreamBedY => (m_elevation + HexMetrics.StreamBedElevationOffset) * HexMetrics.ElevationStep;

        /// <summary> Returns true if the hex cell has at least one road </summary>
        public bool HasRoads => roads.Any(road => road);
        
        public int UrbanLevel {
            get => m_urbanLevel;
            set {
                if (m_urbanLevel == value) return;
                m_urbanLevel = value;
                RefreshSelfOnly();
            }
        } // UrbanLevel
        
        public int FarmLevel {
            get => m_farmLevel;
            set {
                if (m_farmLevel == value) return;
                m_farmLevel = value;
                RefreshSelfOnly();
            }
        } // FarmLevel

        public int PlantLevel {
            get => m_plantLevel;
            set {
                if (m_plantLevel == value) return;
                m_plantLevel = value;
                RefreshSelfOnly();
            }
        } // PlantLevel
        
        public bool Walled {
            get => m_walled;
            set {
                if (m_walled == value) return;
                m_walled = value;
                Refresh();
            }
        } // Walled
        
        public int SpecialIndex {
            get => m_specialIndex;
            set {
                if (m_specialIndex == value || HasRiver) return;
                m_specialIndex = value;
                RemoveRoads();
                RefreshSelfOnly();
            }
        } // SpecialIndex
        
        public bool IsSpecial => m_specialIndex > 0;

        public int Distance {
            get => m_distance;
            set {
                m_distance = value;
                UpdateDistanceLabel();
            }
        } // Distance
        
        public HexCell PathFrom { get; set; }
        
        public int SearchHeuristic { get; set; }
        
        public int SearchPriority => m_distance + SearchHeuristic;

        public HexCell NextWithSamePriority { get; set; }
        
        // -- Methods --
        public void Save (BinaryWriter writer) {
            writer.Write((byte)m_terrainTypeIndex);
            writer.Write((byte)m_elevation);
            writer.Write((byte)m_waterLevel);
            writer.Write((byte)m_urbanLevel);
            writer.Write((byte)m_farmLevel);
            writer.Write((byte)m_plantLevel);
            writer.Write((byte)m_specialIndex);
            writer.Write(m_walled);

            if (m_hasIncomingRiver) {
                writer.Write((byte)(m_incomingRiver + 128));
            } else {
                writer.Write((byte)0);
            }
            
            if (m_hasOutgoingRiver) {
                writer.Write((byte)(m_outgoingRiver + 128));
            } else {
                writer.Write((byte)0);
            }

            var roadFlags = 0;
            for (var i = 0; i < roads.Length; i++) {
                if (roads[i]) {
                    roadFlags |= 1 << i;
                }
            }
            writer.Write((byte)roadFlags);
        } // Save ()

        public void Load (BinaryReader reader) {
            // Debug.Log(Application.persistentDataPath);
            m_terrainTypeIndex = reader.ReadByte();
            m_elevation = reader.ReadByte();
            RefreshPosition();
            m_waterLevel = reader.ReadByte();
            m_urbanLevel = reader.ReadByte();
            m_farmLevel = reader.ReadByte();
            m_plantLevel = reader.ReadByte();
            m_specialIndex = reader.ReadByte();
            m_walled = reader.ReadBoolean();

            var riverData = reader.ReadByte();
            if (riverData >= 128) {
                m_hasIncomingRiver = true;
                m_incomingRiver = (HexDirection)(riverData - 128);
            } else {
                m_hasIncomingRiver = false;
            }

            riverData = reader.ReadByte();
            if (riverData >= 128) {
                m_hasOutgoingRiver = true;
                m_outgoingRiver = (HexDirection)(riverData - 128);
            } else {
                m_hasOutgoingRiver = false;
            }

            int roadFlags = reader.ReadByte();
            for (var i = 0; i < roads.Length; i++) {
                roads[i] = (roadFlags & (1 << i)) != 0;
            }
        } // Load ()
        
        public HexCell GetNeighbor(HexDirection direction) {
            return neighbors[(int)direction];
        } // GetNeighbor ()

        public void SetNeighbor(HexDirection direction, HexCell cell) {
            neighbors[(int)direction] = cell;
            cell.neighbors[(int)direction.Opposite()] = this;
        } // SetNeighbor ()
 
        public HexEdgeType GetEdgeType(HexDirection direction) {
            return HexMetrics.GetEdgeType(m_elevation, neighbors[(int)direction].m_elevation);
        } // GetEdgeType ()

        public HexEdgeType GetEdgeType(HexCell otherCell) {
            return HexMetrics.GetEdgeType(m_elevation, otherCell.m_elevation);
        } // GetEdgeType ()

        private bool IsValidRiverDestination (HexCell neighbor) {
            return neighbor && (m_elevation >= neighbor.m_elevation || m_waterLevel == neighbor.m_elevation);
        } // IsValidRiverDestination ()
        
        private void ValidateRivers () {
            if (m_hasOutgoingRiver && !IsValidRiverDestination(GetNeighbor(m_outgoingRiver))) RemoveOutgoingRiver();
            if (m_hasIncomingRiver && !GetNeighbor(m_incomingRiver).IsValidRiverDestination(this)) RemoveIncomingRiver();
        } // ValidateRivers ()
        
        public bool HasRiverThroughEdge(HexDirection direction) {
            return m_hasIncomingRiver && m_incomingRiver == direction || m_hasOutgoingRiver && m_outgoingRiver == direction;
        } // HasRiverThroughEdge ()

        public void RemoveIncomingRiver() {
            if (!m_hasIncomingRiver) return;

            m_hasIncomingRiver = false;
            RefreshSelfOnly();

            var neighbor = GetNeighbor(m_incomingRiver);
            neighbor.m_hasOutgoingRiver = false;
            neighbor.RefreshSelfOnly();
        } // RemoveIncomingRiver ()

        public void RemoveOutgoingRiver() {
            if (!m_hasOutgoingRiver) return;

            m_hasOutgoingRiver = false;
            RefreshSelfOnly();

            var neighbor = GetNeighbor(m_outgoingRiver);
            neighbor.m_hasIncomingRiver = false;
            neighbor.RefreshSelfOnly();
        } // RemoveOutgoingRiver ()

        public void RemoveRiver() {
            RemoveOutgoingRiver();
            RemoveIncomingRiver();
        } // RemoveRiver ()

        public void SetOutgoingRiver(HexDirection direction) {
            if (m_hasOutgoingRiver && m_outgoingRiver == direction) return;

            var neighbor = GetNeighbor(direction);
            if (!IsValidRiverDestination(neighbor)) return;

            RemoveOutgoingRiver();
            if (m_hasIncomingRiver && m_incomingRiver == direction) RemoveIncomingRiver();

            m_hasOutgoingRiver = true;
            m_outgoingRiver = direction;
            m_specialIndex = 0;

            neighbor.RemoveIncomingRiver();
            neighbor.m_hasIncomingRiver = true;
            neighbor.m_incomingRiver = direction.Opposite();
            neighbor.m_specialIndex = 0;

            SetRoad((int)direction, false);
        } // SetOutgoingRiver ()

        public bool HasRoadThroughEdge(HexDirection direction) {
            return roads[(int)direction];
        } // HasRoadThroughEdge ()
 
        public void AddRoad(HexDirection direction) {
            if (!roads[(int)direction] && !HasRiverThroughEdge(direction) && !IsSpecial && !GetNeighbor(direction).IsSpecial && GetElevationDifference(direction) <= 1 && !IsUnderwater) 
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
            var difference = m_elevation - GetNeighbor(direction).m_elevation;
            return difference >= 0 ? difference : -difference;
        } // GetElevationDifference ()
        
        private void UpdateDistanceLabel () {
            var label = uiRect.GetComponent<Text>();
            label.text = m_distance == int.MaxValue ? "" : m_distance.ToString();
        } // UpdateDistanceLabel ()

        public void DisableHighlight () {
            var highlight = uiRect.GetChild(0).GetComponent<Image>();
            highlight.enabled = false;
        } // DisableHighlight ()
	
        public void EnableHighlight (Color color) {
            var highlight = uiRect.GetChild(0).GetComponent<Image>();
            highlight.color = color;
            highlight.enabled = true;
        } // EnableHighlight ()
        
        private void Refresh() {
            if (!chunk) return;
            chunk.Refresh();
            foreach (var neighbor in neighbors) {
                if (neighbor != null && neighbor.chunk != chunk) neighbor.chunk.Refresh();
            }
        } // Refresh ()

        private void RefreshPosition() {
            var position = transform.localPosition;
            position.y = m_elevation * HexMetrics.ElevationStep;
            position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.ElevationPerturbStrength;
            transform.localPosition = position;

            var uiPosition = uiRect.localPosition;
            uiPosition.z = -position.y;
            uiRect.localPosition = uiPosition;
        } // RefreshPosition ()
        
        private void RefreshSelfOnly() {
            chunk.Refresh();
        } // RefreshSelfOnly ()

    } // Class HexCell

} // Namespace Arkayns Reckon HexMap