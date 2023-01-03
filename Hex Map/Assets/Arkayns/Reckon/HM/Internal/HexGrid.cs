using UnityEngine;
using UnityEngine.UI;

namespace Arkayns.Reckon.HM {
	
	public class HexGrid : MonoBehaviour {

		// -- Variables --
		public int chunkCountX = 4, chunkCountZ = 3;
		public Color defaultColor = Color.white;

		public HexCell cellPrefab;
		public Text cellLabelPrefab;
		public HexGridChunk chunkPrefab;

		public Texture2D noiseSource;

		private HexGridChunk[] m_chunks;
		private HexCell[] m_cells;
		private int m_cellCountX, m_cellCountZ;

		
		// -- Built-In Methods --
		private void Awake () {
			HexMetrics.NoiseSource = noiseSource;

			m_cellCountX = chunkCountX * HexMetrics.chunkSizeX;
			m_cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;

			CreateChunks();
			CreateCells();
		} // Awake

		private void OnEnable () {
			HexMetrics.NoiseSource = noiseSource;
		} // OnEnable
		
		
		// -- Methods --
		private void CreateChunks () {
			m_chunks = new HexGridChunk[chunkCountX * chunkCountZ];
			for (int z = 0, i = 0; z < chunkCountZ; z++) {
				for (var x = 0; x < chunkCountX; x++) {
					var chunk = m_chunks[i++] = Instantiate(chunkPrefab);
					chunk.transform.SetParent(transform);
				}
			}
		} // CreateChunks

		private void CreateCells () {
			m_cells = new HexCell[m_cellCountZ * m_cellCountX];
			for (int z = 0, i = 0; z < m_cellCountZ; z++) 
				for (var x = 0; x < m_cellCountX; x++) CreateCell(x, z, i++);
		} // CreateCells

		public HexCell GetCell (Vector3 position) {
			position = transform.InverseTransformPoint(position);
			var coordinates = HexCoordinates.FromPosition(position);
			var index = coordinates.X + coordinates.Z * m_cellCountX + coordinates.Z / 2;
			return m_cells[index];
		} // GetCell
		
		public HexCell GetCell (HexCoordinates coordinates) {
			var z = coordinates.Z;
			if (z < 0 || z >= m_cellCountZ) return null;
			var x = coordinates.X + z / 2;
			if (x < 0 || x >= m_cellCountX) return null;
			return m_cells[x + z * m_cellCountX];
		} // GetCell

		private void CreateCell (int x, int z, int i) {
			Vector3 position;
			// ReSharper disable once PossibleLossOfFraction
			position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
			position.y = 0f;
			position.z = z * (HexMetrics.outerRadius * 1.5f);

			var cell = m_cells[i] = Instantiate(cellPrefab);
			cell.transform.localPosition = position;
			cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
			cell.Color = defaultColor;

			if (x > 0) cell.SetNeighbor(HexDirection.W, m_cells[i - 1]);
			
			if (z > 0) {
				if ((z & 1) == 0) {
					cell.SetNeighbor(HexDirection.SE, m_cells[i - m_cellCountX]);
					if (x > 0) cell.SetNeighbor(HexDirection.SW, m_cells[i - m_cellCountX - 1]);
				} else {
					cell.SetNeighbor(HexDirection.SW, m_cells[i - m_cellCountX]);
					if (x < m_cellCountX - 1) cell.SetNeighbor(HexDirection.SE, m_cells[i - m_cellCountX + 1]);
				}
			}

			var label = Instantiate(cellLabelPrefab);
			label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
			label.text = cell.coordinates.ToStringOnSeparateLines();
			cell.uiRect = label.rectTransform;
			cell.Elevation = 0;

			AddCellToChunk(x, z, cell);
		} // CreateCell

		private void AddCellToChunk (int x, int z, HexCell cell) {
			var chunkX = x / HexMetrics.chunkSizeX;
			var chunkZ = z / HexMetrics.chunkSizeZ;
			var chunk = m_chunks[chunkX + chunkZ * chunkCountX];

			var localX = x - chunkX * HexMetrics.chunkSizeX;
			var localZ = z - chunkZ * HexMetrics.chunkSizeZ;
			chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
		} // AddCellToChunk
		
	} // Class HexGrid
	
} // Namespace Arkayns Reckon HexMap