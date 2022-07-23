using UnityEngine;
using UnityEngine.UI;

namespace Arkayns.HM {
    
    public class HexGridChunk : MonoBehaviour{
        
        private HexCell[] m_cells;

        private HexMesh m_hexMesh;
        private Canvas m_gridCanvas;

        private void Awake () {
            m_gridCanvas = GetComponentInChildren<Canvas>();
            m_hexMesh = GetComponentInChildren<HexMesh>();

            m_cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
        } // Awake
	
        private void Start () {
            m_hexMesh.Triangulate(m_cells);
        } // Start
        
        public void AddCell (int index, HexCell cell) {
            m_cells[index] = cell;
            cell.transform.SetParent(transform, false);
            cell.uiRect.SetParent(m_gridCanvas.transform, false);
        } // AddCell
        
    } // Class HexGridChunk
    
} // Namespace Arkayns HM