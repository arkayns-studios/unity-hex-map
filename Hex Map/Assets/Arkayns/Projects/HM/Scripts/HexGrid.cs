using UnityEngine;
using UnityEngine.UI;

namespace Arkayns.HM {
    
    public class HexGrid : MonoBehaviour {
        public int width = 6;
        public int height = 6;

        public HexCell cellPrefab;
        private HexCell[] m_cells;

        public Text cellLabelPrefab;
        private Canvas m_gridCanvas;
        
        private void Awake() {
            m_gridCanvas = GetComponentInChildren<Canvas>();
            m_cells = new HexCell[height * width];

            for (int z = 0, i = 0; z < height; z++) {
                for (int x = 0; x < width; x++) 
                    CreateCell(x, z, i++);
            }
        } // Awake

        private void CreateCell(int x, int z, int i) {
            Vector3 position;
            position.x = x * 10f;
            position.y = 0f;
            position.z = z * 10f;

            HexCell cell = m_cells[i] = Instantiate<HexCell>(cellPrefab, transform, false);
            cell.transform.localPosition = position;

            Text label = Instantiate<Text>(cellLabelPrefab, m_gridCanvas.transform, false);
            label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
            label.text = $"{x}\n{z}";
        } // CreateCell

    } // Class HexGrid
    
} // Namespace Arkayns HM