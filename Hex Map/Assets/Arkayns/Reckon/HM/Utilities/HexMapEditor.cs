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
		
		private void HandleInput () {
			Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(inputRay, out hit)) {
				EditCell(hexGrid.GetCell(hit.point));
			}
		} // HandleInput ()

		private void EditCell (HexCell cell) {
			if (applyColor) cell.Color = m_activeColor;
			if (applyElevation) cell.Elevation = m_activeElevation;
		} // EditCell ()
		
	} // Class HexMapEditor
	
} // Namespace Arkayns Reckon HexMap