using UnityEngine.UI;
using UnityEngine;

namespace Arkayns.HM {
    
    public class HexGrid : MonoBehaviour {

        public int width = 6;
        public int height = 6;

        public Color defaultColor = Color.white;
        
        public HexCell cellPrefab;
        public Text cellLabelPrefab;
        
        private HexCell[] m_cells;
        private Canvas m_gridCanvas;
        private HexMesh m_hexMesh;
        
        public Texture2D noiseSource;
        
        private void Awake() {
            HexMetrics.NoiseSource = noiseSource;
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

        private void OnEnable () {
            HexMetrics.NoiseSource = noiseSource;
        } // OnEnable
        
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
            cell.color = defaultColor;

            if(x > 0) cell.SetNeighbor(HexDirection.W, m_cells[i -1]);
            if(z > 0) {
                if((z & 1) == 0) {
                    cell.SetNeighbor(HexDirection.SE, m_cells[i - width]);
                    if(x > 0) cell.SetNeighbor(HexDirection.SW, m_cells[i - width - 1]);
                } else {
                    cell.SetNeighbor(HexDirection.SW, m_cells[i - width]);
                    if (x < width - 1) cell.SetNeighbor(HexDirection.SE, m_cells[i - width + 1]);
                }
            }

            Text label = Instantiate<Text>(cellLabelPrefab, m_gridCanvas.transform, false);
            label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
            label.text = cell.coordinates.ToStringOnSeparateLines();

            cell.uiRect = label.rectTransform;
        } // CreateCell

        public HexCell GetCell (Vector3 position) {
            position = transform.InverseTransformPoint(position);
            HexCoordinates coordinates = HexCoordinates.FromPosition(position);
            int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
            return m_cells[index];
        } // GetCell

        public void Refresh() {
            m_hexMesh.Triangulate(m_cells);
        } // Refresh
        
    } // Class HexGrid
    
} // Namespace Arkayns HM