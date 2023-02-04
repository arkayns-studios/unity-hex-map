using UnityEngine;
using UnityEngine.EventSystems;

namespace Arkayns.Reckon.HM {

    public class HexMapEditor : MonoBehaviour {

        // -- Variables ---
        public HexGrid hexGrid;
        public Material terrainMaterial;

        private int m_activeTerrainTypeIndex;
        private int m_activeElevation;
        private int m_activeWaterLevel;
        private int m_brushSize;
        private int m_activeUrbanLevel, m_activeFarmLevel, m_activePlantLevel, m_activeSpecialIndex;
        
        private bool m_applyElevation = true;
        private bool m_applyWaterLevel = true;
        private bool m_applyUrbanLevel, m_applyFarmLevel, m_applyPlantLevel, m_applySpecialIndex;
        private bool m_editMode;

        private OptionalToggle m_riverMode, m_roadMode, m_walledMode;

        private bool isDrag;
        private HexDirection m_dragDirection;
        private HexCell m_previousCell;

        // -- Built-In Methods --
        private void Awake () {
            terrainMaterial.DisableKeyword("GRID_ON");
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

                if (m_editMode) EditCells(currentCell);
                else hexGrid.FindDistancesTo(currentCell);
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
            
            if (m_activeTerrainTypeIndex >= 0) cell.TerrainTypeIndex = m_activeTerrainTypeIndex;
            if (m_applyElevation) cell.Elevation = m_activeElevation;
            if (m_applyWaterLevel) cell.WaterLevel = m_activeWaterLevel;
            if (m_applySpecialIndex) cell.SpecialIndex = m_activeSpecialIndex;
            if (m_applyUrbanLevel) cell.UrbanLevel = m_activeUrbanLevel;
            if (m_applyFarmLevel) cell.FarmLevel = m_activeFarmLevel;
            if (m_applyPlantLevel) cell.PlantLevel = m_activePlantLevel;

            if (m_riverMode == OptionalToggle.No) cell.RemoveRiver();
            if (m_roadMode == OptionalToggle.No) cell.RemoveRoads();
            if (m_walledMode != OptionalToggle.Ignore) cell.Walled = m_walledMode == OptionalToggle.Yes;

            if (isDrag) {
                var otherCell = cell.GetNeighbor(m_dragDirection.Opposite());
                if (!otherCell) return;
                if(m_riverMode == OptionalToggle.Yes) otherCell.SetOutgoingRiver(m_dragDirection);
                if (m_roadMode == OptionalToggle.Yes) otherCell.AddRoad(m_dragDirection);
            }
            
        } // EditCell ()

        public void SetTerrainTypeIndex (int index) {
            m_activeTerrainTypeIndex = index;
        } // SetTerrainTypeIndex ()
        
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
        
        public void SetApplyUrbanLevel (bool toggle) {
            m_applyUrbanLevel = toggle;
        } // SetApplyUrbanLevel ()
	
        public void SetUrbanLevel (float level) {
            m_activeUrbanLevel = (int)level;
        } // SetUrbanLevel ()
        
        public void SetApplyFarmLevel (bool toggle) {
            m_applyFarmLevel = toggle;
        } // SetApplyFarmLevel ()

        public void SetFarmLevel (float level) {
            m_activeFarmLevel = (int)level;
        } // SetFarmLevel ()

        public void SetApplyPlantLevel (bool toggle) {
            m_applyPlantLevel = toggle;
        } // SetApplyPlantLevel ()

        public void SetPlantLevel (float level) {
            m_activePlantLevel = (int)level;
        } // SetPlantLevel ()
        
        public void SetWalledMode (int mode) {
            m_walledMode = (OptionalToggle)mode;
        } // SetWalledMode ()
        
        public void SetApplySpecialIndex (bool toggle) {
            m_applySpecialIndex = toggle;
        } // SetApplySpecialIndex ()

        public void SetSpecialIndex (float index) {
            m_activeSpecialIndex = (int)index;
        } // SetSpecialIndex ()
        
        public void SetEditMode (bool toggle) {
            m_editMode = toggle;
            hexGrid.ShowUI(!toggle);
        } // SetEditMode ()

        public void ShowGrid (bool visible) {
            if (visible) terrainMaterial.EnableKeyword("GRID_ON");
            else terrainMaterial.DisableKeyword("GRID_ON");
        } // ShowGrid ()
        
    } // Class HexMapEditor

} // Namespace Arkayns Reckon HM