using UnityEngine;
using UnityEngine.EventSystems;

namespace Arkayns.Reckon.HM {
	public class HexMapEditor : MonoBehaviour {

		// -- Variables --
		public Color[] colors;
		public HexGrid hexGrid;
		private int m_activeElevation;
		private Color m_activeColor;
		private bool applyColor;
		private bool applyElevation;
		private int brushSize;

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
			applyColor = index >= 0;
			if (applyColor) m_activeColor = colors[index];
		} // SelectColor ()
		
		public void SetApplyElevation(bool toggle) {
			applyElevation = toggle;
		} // SetApplyElevation ()

		public void SetElevation(float elevation) {
			if (applyElevation) m_activeElevation = (int)elevation;
		} // SetElevation ()

		public void SetBrushSize(float size) {
			brushSize = (int)size;
		} // SetBrushSize ()
		
		private void HandleInput () {
			var inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(inputRay, out hit)) {
				EditCells(hexGrid.GetCell(hit.point));
			}
		} // HandleInput ()

		private void EditCells(HexCell center) {
			var centerX = center.coordinates.X;
			var centerZ = center.coordinates.Z;

			for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++) {
				for (var x = centerX - r; x <= centerX + brushSize; x++) {
					EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
				}
			}
			
			for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
				for (var x = centerX - brushSize; x <= centerX + r; x++) {
					EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
				}
			}

		} // EditCells ()
		
		private void EditCell (HexCell cell) {
			if (!cell) return;
			if (applyColor) cell.Color = m_activeColor;
			if (applyElevation) cell.Elevation = m_activeElevation;
		} // EditCell ()
		
	} // Class HexMapEditor
	
} // Namespace Arkayns Reckon HexMap