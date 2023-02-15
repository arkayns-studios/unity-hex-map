using UnityEngine;
using UnityEngine.EventSystems;

namespace Arkayns.Reckon.HM {

    public class HexGameUI : MonoBehaviour {

        // -- Variables --
        public HexGrid grid;
        private HexCell m_currentCell;
        private HexUnit m_selectedUnit;
        
        // -- Build-In Methods --
        private void Update () {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (Input.GetMouseButtonDown(0)) {
                DoSelection();
            } else if (m_selectedUnit) {
                if (Input.GetMouseButtonDown(1)) {
                    DoMove();
                } else {
                    DoPathfinding();
                }
            }
        } // Update ()
        
        // -- Custom Methods
        public void SetEditMode (bool toggle) {
            enabled = !toggle;
            grid.ShowUI(!toggle);
            grid.ClearPath();
        } // SetEditMode ()
        
        private bool UpdateCurrentCell () {
            if (Camera.main == null) return false;
            var cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
            if (cell == m_currentCell) return false;
            m_currentCell = cell;
            return true;
        } // UpdateCurrentCell ()
        
        private void DoSelection () {
            grid.ClearPath();
            UpdateCurrentCell();
            if (m_currentCell) {
                m_selectedUnit = m_currentCell.Unit;
            }
        } // DoSelection ()
        
        private void DoPathfinding () {
            if (!UpdateCurrentCell()) return;
            
            if (m_currentCell && m_selectedUnit.IsValidDestination(m_currentCell)) {
                grid.FindPath(m_selectedUnit.Location, m_currentCell, 24);
            } else {
                grid.ClearPath();
            }
        } // DoPathfinding ()
        
        private void DoMove () {
            if (!grid.HasPath) return;
            m_selectedUnit.Location = m_currentCell;
            grid.ClearPath();
        } // DoMove ()
        
    } // Class HexGameUI

} // Namespace Arkayns Reckon HM