using UnityEngine;

namespace Arkayns.Reckon.HM {
	
	public class HexGridChunk : MonoBehaviour {

		// -- Variables --
		private HexCell[] m_cells;
		private HexMesh m_hexMesh;
		private Canvas m_gridCanvas;
		
		// -- Built-In Methods --
		private void Awake () {
			m_gridCanvas = GetComponentInChildren<Canvas>();
			m_hexMesh = GetComponentInChildren<HexMesh>();
			m_cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
			ShowUI(false);
		} // Awake

		private void LateUpdate () {
			m_hexMesh.Triangulate(m_cells);
			enabled = false;
		} // LateUpdate
		
		// -- Methods --
		public void AddCell (int index, HexCell cell) {
			m_cells[index] = cell;
			cell.chunk = this;
			cell.transform.SetParent(transform, false);
			cell.uiRect.SetParent(m_gridCanvas.transform, false);
		} // AddCell

		public void ShowUI(bool visible) {
			m_gridCanvas.gameObject.SetActive(visible);
		} // ShowUI ()
		
		public void Refresh () {
			enabled = true;
		} // Refresh

	} // Class HexGridChunk
	
} // Namespace Arkayns Reckon HexMap