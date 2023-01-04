using UnityEngine;

namespace Arkayns.Reckon.HM {
	
	public class HexCell : MonoBehaviour {
		
		// -- Variables --
		public HexCoordinates coordinates;
		public RectTransform uiRect;
		public HexGridChunk chunk;

		private Color m_color;
		private int m_elevation = int.MinValue;
		[SerializeField] private HexCell[] neighbors;
		
		private bool m_hasIncomingRiver, m_hasOutgoingRiver;
		private HexDirection m_incomingRiver, m_outgoingRiver;
		
		// -- Properties --
		public Color Color {
			get => m_color;
			set {
				if (m_color == value) return;
				m_color = value;
				Refresh();
			}
		} // Color
		public int Elevation {
			get => m_elevation;
			set {
				if (m_elevation == value) return;
				m_elevation = value;
				
				var position = transform.localPosition;
				position.y = value * HexMetrics.elevationStep;
				position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
				transform.localPosition = position;

				var uiPosition = uiRect.localPosition;
				uiPosition.z = -position.y;
				uiRect.localPosition = uiPosition;
				Refresh();
			}
		} // Elevation

		public Vector3 Position => transform.localPosition;

		public bool HasIncomingRiver => m_hasIncomingRiver;
		public bool HasOutgoingRiver => m_hasOutgoingRiver;
		public HexDirection IncomingRiver => m_incomingRiver;
		public HexDirection OutgoingRiver => m_outgoingRiver;
		public bool HasRiver => m_hasIncomingRiver || m_hasOutgoingRiver;
		
		// -- Methods --
		public HexCell GetNeighbor (HexDirection direction) => neighbors[(int)direction];

		public void SetNeighbor (HexDirection direction, HexCell cell) {
			neighbors[(int)direction] = cell;
			cell.neighbors[(int)direction.Opposite()] = this;
		} // SetNeighbor

		public HexEdgeType GetEdgeType (HexDirection direction) => HexMetrics.GetEdgeType(m_elevation, neighbors[(int)direction].m_elevation);

		public HexEdgeType GetEdgeType (HexCell otherCell) => HexMetrics.GetEdgeType(m_elevation, otherCell.m_elevation);

		public bool HasRiverThroughEdge(HexDirection direction) {
			return m_hasIncomingRiver && m_incomingRiver == direction || m_hasOutgoingRiver && m_outgoingRiver == direction;
		} // HasRiverThroughEdge ()

		public void RemoveOutgoingRiver() {
			if (!m_hasOutgoingRiver) return;
			m_hasOutgoingRiver = false;
			RefreshSelfOnly();

			var neighbor = GetNeighbor(m_outgoingRiver);
			neighbor.m_hasIncomingRiver = false;
			neighbor.RefreshSelfOnly();
		} // RemoveOutgoingRiver ()

		public void RemoveIncomingRiver() {
			if(!m_hasIncomingRiver) return;
			m_hasIncomingRiver = false;
			RefreshSelfOnly();

			var neighbor = GetNeighbor(m_incomingRiver);
			neighbor.m_hasOutgoingRiver = false;
			neighbor.RefreshSelfOnly();
		} // RemoveIncomingRiver ()

		public void RemoveRiver() {
			RemoveOutgoingRiver();
			RemoveIncomingRiver();
		} // RemoveRiver ()
		
		private void Refresh () {
			if (!chunk) return;
			chunk.Refresh();
			foreach (var neighbor in neighbors) if (neighbor != null && neighbor.chunk != chunk) neighbor.chunk.Refresh();
		} // Refresh

		private void RefreshSelfOnly() {
			chunk.Refresh();
		} // RefreshSelfOnly ()
		
	} // Class HexCell
	
} // Namespace Arkayns Reckon HexMap