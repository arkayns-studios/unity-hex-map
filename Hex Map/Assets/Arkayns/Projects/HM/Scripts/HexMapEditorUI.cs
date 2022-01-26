using UnityEngine.EventSystems;
using UnityEngine;

namespace Arkayns.HM {
    
    public class HexMapEditorUI : MonoBehaviour {

        public HexGrid hexGrid;
        
        public Color[] colors;
        private Color m_activeColor;


        private void Awake() {
            SelectColor(0);
        } // Awake

        private void Update () {
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) HandleInput();
        } // Update

        
        private void HandleInput () {
            Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(inputRay, out RaycastHit hit)) hexGrid.ColorCell(hit.point, m_activeColor);
        } // HandleInput

        public void SelectColor(int index) {
            m_activeColor = colors[index];
        } // SelectColor
        
    } // Class HexMapEditorUI
    
} // Namespace Arkayns HM