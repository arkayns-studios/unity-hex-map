using UnityEngine;
using UnityEngine.EventSystems;

namespace Arkayns.Reckon.HM {
	public class HexMapEditor : MonoBehaviour {

		// -- Variables --
		public Color[] colors;
		public HexGrid hexGrid;
		private int m_activeElevation;
		private Color m_activeColor;

		// -- Methods --
		private void Awake () {
			SelectColor(0);
		} // Awake

		private void Update () {
			if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) {
				HandleInput();
			}
		} // Update
		
		public void SelectColor (int index) => m_activeColor = colors[index];

		public void SetElevation (float elevation) => m_activeElevation = (int)elevation;
		
		
		// -- Methods --
		private void HandleInput () {
			Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(inputRay, out hit)) {
				EditCell(hexGrid.GetCell(hit.point));
			}
		} // HandleInput

		private void EditCell (HexCell cell) {
			cell.Color = m_activeColor;
			cell.Elevation = m_activeElevation;
		} // EditCell
		
	} // Class HexMapEditor
	
} // Namespace Arkayns Reckon HexMap