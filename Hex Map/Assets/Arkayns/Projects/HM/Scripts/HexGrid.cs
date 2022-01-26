using System;
using UnityEngine.UI;
using UnityEngine;

namespace Arkayns.HM {
    
    public class HexGrid : MonoBehaviour {
        public int width = 6;
        public int height = 6;

        private HexCell[] m_cells;
        private Canvas m_gridCanvas;
        private HexMesh m_hexMesh;
        
        public HexCell cellPrefab;
        public Text cellLabelPrefab;
        
        private void Awake() {
            m_gridCanvas = GetComponentInChildren<Canvas>();
            m_hexMesh = GetComponentInChildren<HexMesh>();
            m_cells = new HexCell[height * width];

            for (int z = 0, i = 0; z < height; z++) {
                for (int x = 0; x < width; x++) 
                    CreateCell(x, z, i++);
            }
        } // Awake

        private void Start() {
            m_hexMesh.Triangulate(m_cells);
        } // Start

        private void CreateCell(int x, int z, int i) {
            Vector3 position;
            
            // Each row is offset along the X axis by the inner radius
            // ReSharper disable once PossibleLossOfFraction
            position.x = (x + z * 0.5f - (z / 2)) * (HexMetrics.InnerRadius * 2f); 
            position.y = 0f;
            position.z = z * (HexMetrics.OuterRadius * 1.5f);

            HexCell cell = m_cells[i] = Instantiate<HexCell>(cellPrefab, transform, false);
            cell.transform.localPosition = position;
            cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

            Text label = Instantiate<Text>(cellLabelPrefab, m_gridCanvas.transform, false);
            label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
            label.text = cell.coordinates.ToStringOnSeparateLines();
        } // CreateCell

    } // Class HexGrid
    
} // Namespace Arkayns HM