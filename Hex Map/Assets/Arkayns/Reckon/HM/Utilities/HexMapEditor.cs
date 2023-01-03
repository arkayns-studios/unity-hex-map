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

		// -- Built-In Methods --
		private void Awake () {
			SelectColor(-1);
			SetApplyElevation(true);
		} // Awake ()

		private void Update () {
			if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) HandleInput();
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
		
		private void HandleInput () {
			if (Camera.main == null) return;
			var inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(inputRay, out var hit)) EditCells(hexGrid.GetCell(hit.point));
		} // HandleInput ()

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
		} // EditCell ()
		
		public void ShowUI(bool visible) {
			hexGrid.ShowUI(visible);
		} // ShowUI ()
		
	} // Class HexMapEditor
	
} // Namespace Arkayns Reckon HexMap