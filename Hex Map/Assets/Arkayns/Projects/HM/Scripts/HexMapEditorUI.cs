using UnityEngine.EventSystems;
using UnityEngine;

namespace Arkayns.HM {
    
    public class HexMapEditorUI : MonoBehaviour {

        public HexGrid hexGrid;
        public Color[] colors;
        
        private Color m_activeColor;
        private int m_activeElevation;


        private void Awake() {
            SelectColor(0);
        } // Awake

        private void Update () {
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) HandleInput();
        } // Update

        
        private void HandleInput () {
            if (Camera.main == null) return;
            Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(inputRay, out RaycastHit hit)) EditCell(hexGrid.GetCell(hit.point));
        } // HandleInput

        private void EditCell(HexCell cell) {
            cell.color = m_activeColor;
            cell.Elevation = m_activeElevation;
            hexGrid.Refresh();
        } // EditCell
        
        public void SetElevation (float elevation) {
            m_activeElevation = (int)elevation;
        } // SetElevation
        
        public void SelectColor(int index) {
            m_activeColor = colors[index];
        } // SelectColor
        
    } // Class HexMapEditorUI
    
} // Namespace Arkayns HM