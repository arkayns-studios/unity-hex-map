using UnityEngine;
using UnityEngine.EventSystems;

namespace Arkayns.Reckon.HM {

    public class HexMapEditor : MonoBehaviour {

        // -- Variables ---
        public Color[] colors;
        public HexGrid hexGrid;

        private int m_activeElevation;
        private int m_activeWaterLevel;
        private Color m_activeColor;
        private int m_brushSize;
        private int m_activeUrbanLevel;

        private bool m_applyColor;
        private bool m_applyElevation = true;
        private bool m_applyWaterLevel = true;
        private bool m_applyUrbanLevel;

        private OptionalToggle m_riverMode, m_roadMode;

        private bool isDrag;
        private HexDirection m_dragDirection;
        private HexCell m_previousCell;

        // -- Built-In Methods --
        private void Awake() {
            SelectColor(-1);
        } // Awake ()

        private void Update() {
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()) HandleInput();
            else m_previousCell = null;
        } // Update ()

        // -- Methods --
        private void HandleInput() {
            if (Camera.main == null) return;
            var inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(inputRay, out var hit)) {
                var currentCell = hexGrid.GetCell(hit.point);
                if (m_previousCell && m_previousCell != currentCell) ValidateDrag(currentCell);
                else isDrag = false;

                EditCells(currentCell);
                m_previousCell = currentCell;
            } else m_previousCell = null;
        } // HandleInput ()

        private void ValidateDrag(HexCell currentCell) {
            for (m_dragDirection = HexDirection.NE; m_dragDirection <= HexDirection.NW; m_dragDirection++) {
                if (m_previousCell.GetNeighbor(m_dragDirection) != currentCell) continue;
                isDrag = true;
                return;
            }

            isDrag = false;
        } // ValidateDrag ()

        public void SetApplyUrbanLevel (bool toggle) {
            m_applyUrbanLevel = toggle;
        } // SetApplyUrbanLevel ()
	
        public void SetUrbanLevel (float level) {
            m_activeUrbanLevel = (int)level;
        } // SetUrbanLevel ()
        
        private void EditCells(HexCell center) {
            var centerX = center.coordinates.X;
            var centerZ = center.coordinates.Z;

            for (int r = 0, z = centerZ - m_brushSize; z <= centerZ; z++, r++) {
                for (var x = centerX - r; x <= centerX + m_brushSize; x++) 
                    EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }

            for (int r = 0, z = centerZ + m_brushSize; z > centerZ; z--, r++) {
                for (var x = centerX - m_brushSize; x <= centerX + r; x++) 
                    EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        } // EditCells ()

        private void EditCell(HexCell cell) {
            if (!cell) return;
            
            if (m_applyColor) cell.Color = m_activeColor;
            if (m_applyElevation) cell.Elevation = m_activeElevation;
            if (m_applyWaterLevel) cell.WaterLevel = m_activeWaterLevel;
            if (m_applyUrbanLevel) cell.UrbanLevel = m_activeUrbanLevel;

            if (m_riverMode == OptionalToggle.No) cell.RemoveRiver();
            if (m_roadMode == OptionalToggle.No) cell.RemoveRoads();

            if (isDrag) {
                var otherCell = cell.GetNeighbor(m_dragDirection.Opposite());
                if (!otherCell) return;
                if(m_riverMode == OptionalToggle.Yes) otherCell.SetOutgoingRiver(m_dragDirection);
                if (m_roadMode == OptionalToggle.Yes) otherCell.AddRoad(m_dragDirection);
            }
            
        } // EditCell ()

        public void SelectColor(int index) {
            m_applyColor = index >= 0;
            if (m_applyColor) m_activeColor = colors[index];
        } // SelectColor ()

        public void SetApplyElevation(bool toggle) {
            m_applyElevation = toggle;
        } // SetApplyElevation ()

        public void SetElevation(float elevation) {
            m_activeElevation = (int)elevation;
        } // SetElevation ()

        public void SetApplyWaterLevel (bool toggle) {
            m_applyWaterLevel = toggle;
        } // SetApplyWaterLevel ()
	
        public void SetWaterLevel (float level) {
            m_activeWaterLevel = (int)level;
        } // SetWaterLevel ()
        
        public void SetBrushSize(float size) {
            m_brushSize = (int)size;
        } // SetBrushSize ()

        public void SetRiverMode(int mode) {
            m_riverMode = (OptionalToggle)mode;
        } // SetRiverMode ()

        public void SetRoadMode(int mode) {
            m_roadMode = (OptionalToggle)mode;
        } // SetRoadMode ()
        
        public void ShowUI(bool visible) {
            hexGrid.ShowUI(visible);
        } // ShowUI ()
        
    } // Class HexMapEditor

} // Namespace Arkayns Reckon HM