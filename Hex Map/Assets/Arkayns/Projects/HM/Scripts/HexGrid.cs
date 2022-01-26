using System;
using UnityEngine;

namespace Arkayns.HM {
    
    public class HexGrid : MonoBehaviour {
        public int width = 6;
        public int height = 6;

        public HexCell cellPrefab;
        private HexCell[] m_cells;

        private void Awake() {
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

            HexCell cell = m_cells[i] = Instantiate<HexCell>(cellPrefab);
            cell.transform.SetParent(transform, false);
            cell.transform.localPosition = position;
        } // CreateCell

    } // Class HexGrid
    
} // Namespace Arkayns HM