using UnityEngine;
using UnityEngine.EventSystems;

namespace Arkayns.Reckon.HM {
	public class HexMapEditor : MonoBehaviour {

		// -- Variables --
		public Color[] colors;
		public HexGrid hexGrid;
		private int m_activeElevation;
		private Color m_activeColor;
		private bool m_applyColor;
		private bool m_applyElevation;
		private int m_brushSize;
		private OptionalToggle m_riverMode;
		private bool m_isDrag;
		private HexDirection m_dragDirection;
		private HexCell m_previousCell;
		
		// -- Built-In Methods --
		private void Awake () {
			SelectColor(-1);
			SetApplyElevation(true);
		} // Awake ()

		private void Update () {
			if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) HandleInput();
			else m_previousCell = null;
		} // Update ()

		// -- Methods --
		public void SelectColor(int index) {
			m_applyColor = index >= 0;
			if (m_applyColor) m_activeColor = colors[index];
		} // SelectColor ()
		
		public void SetApplyElevation(bool toggle) {
			m_applyElevation = toggle;
		} // SetApplyElevation ()

		public void SetElevation(float elevation) {
			if (m_applyElevation) m_activeElevation = (int)elevation;
		} // SetElevation ()

		public void SetBrushSize(float size) {
			m_brushSize = (int)size;
		} // SetBrushSize ()

		public void SetRiverMode(int mode) {
			m_riverMode = (OptionalToggle)mode;
		} // SetRiverMode ()
		
		private void HandleInput () {
			if (Camera.main == null) return;
			var inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(inputRay, out var hit)) {
				var currentCell = hexGrid.GetCell(hit.point);

				if (m_previousCell && m_previousCell != currentCell) ValidateDrag(currentCell);
				else m_isDrag = false;
				
				EditCells(currentCell);
				m_previousCell = currentCell;
			} else m_previousCell = null;
		} // HandleInput ()

		private void ValidateDrag(HexCell currentCell) {
			for (m_dragDirection = HexDirection.NE; m_dragDirection <= HexDirection.NW; m_dragDirection++) {
				if (m_previousCell.GetNeighbor(m_dragDirection) != currentCell) continue;
				m_isDrag = true;
				return;
			}

			m_isDrag = false;
		} // ValidateDrag
		
		private void EditCells(HexCell center) {
			var centerX = center.coordinates.X;
			var centerZ = center.coordinates.Z;

			for (int r = 0, z = centerZ - m_brushSize; z <= centerZ; z++, r++) {
				for (var x = centerX - r; x <= centerX + m_brushSize; x++) {
					EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
				}
			}
			
			for (int r = 0, z = centerZ + m_brushSize; z > centerZ; z--, r++) {
				for (var x = centerX - m_brushSize; x <= centerX + r; x++) {
					EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
				}
			}

		} // EditCells ()
		
		private void EditCell (HexCell cell) {
			if (!cell) return;
			if (m_applyColor) cell.Color = m_activeColor;
			if (m_applyElevation) cell.Elevation = m_activeElevation;
			if(m_riverMode == OptionalToggle.No) cell.RemoveRiver();
			else if(m_isDrag && m_riverMode == OptionalToggle.Yes) {
				var otherCell = cell.GetNeighbor(m_dragDirection.Opposite());
				if(otherCell) otherCell.SetOutgoingRiver(m_dragDirection);
			}
		} // EditCell ()
		
		public void ShowUI(bool visible) {
			hexGrid.ShowUI(visible);
		} // ShowUI ()
		
	} // Class HexMapEditor
	
} // Namespace Arkayns Reckon HexMap