﻿using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Arkayns.Reckon.HM {

	public class HexGrid : MonoBehaviour {

		// -- Variables --
		public int cellCountX = 20;
		public int cellCountZ = 15;
		private int m_chunkCountX, m_chunkCountZ;

		public HexCell cellPrefab;
		public Text cellLabelPrefab;
		public HexGridChunk chunkPrefab;

		public Texture2D noiseSource;
		public int seed;

		private HexGridChunk[] m_gridChunks;
		private HexCell[] m_cells;

		// -- Built-In Methods --
		private void Awake () {
			HexMetrics.noiseSource = noiseSource;
			HexMetrics.InitializeHashGrid(seed);
			CreateMap(cellCountX, cellCountZ);
		} // Awake ()
		
		private void OnEnable () {
			if (!HexMetrics.noiseSource) {
				HexMetrics.noiseSource = noiseSource;
				HexMetrics.InitializeHashGrid(seed);
			}
		} // OnEnable ()

		// -- Methods --
		public void Save (BinaryWriter writer) {
			writer.Write(cellCountX);
			writer.Write(cellCountZ);
			foreach (var cell in m_cells) cell.Save(writer);
		} // Save ()

		public void Load (BinaryReader reader, int header) {
			int x = 20, z = 15;
			if (header >= 1) {
				x = reader.ReadInt32();
				z = reader.ReadInt32();
			}
			
			if (x != cellCountX || z != cellCountZ)
				if (!CreateMap(x, z)) return;
			foreach (var cell in m_cells) cell.Load(reader);
			foreach (var chunk in m_gridChunks) chunk.Refresh();
		} // Load ()

		public bool  CreateMap (int x, int z) {
			if (x <= 0 || x % HexMetrics.ChunkSizeX != 0 || z <= 0 || z % HexMetrics.ChunkSizeZ != 0) {
				Debug.LogError("Unsupported map size.");
				return false;
			}
			
			if (m_gridChunks != null) {
				foreach (var t in m_gridChunks) 
					Destroy(t.gameObject);
			}
			
			cellCountX = x;
			cellCountZ = z;
			m_chunkCountX = cellCountX / HexMetrics.ChunkSizeX;
			m_chunkCountZ = cellCountZ / HexMetrics.ChunkSizeZ;

			CreateChunks();
			CreateCells();
			
			return true;
		} // CreateMap ()
		
		private void CreateChunks () {
			m_gridChunks = new HexGridChunk[m_chunkCountX * m_chunkCountZ];

			for (int z = 0, i = 0; z < m_chunkCountZ; z++) {
				for (var x = 0; x < m_chunkCountX; x++) {
					var chunk = m_gridChunks[i++] = Instantiate(chunkPrefab);
					chunk.transform.SetParent(transform);
				}
			}
		} // CreateChunks ()

		private void CreateCells () {
			m_cells = new HexCell[cellCountZ * cellCountX];
			for (int z = 0, i = 0; z < cellCountZ; z++) 
				for (var x = 0; x < cellCountX; x++) CreateCell(x, z, i++);
		} // CreateCells ()
		
		public HexCell GetCell (Vector3 position) {
			position = transform.InverseTransformPoint(position);
			var coordinates = HexCoordinates.FromPosition(position);
			var index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
			return m_cells[index];
		} // GetCell ()

		public HexCell GetCell (HexCoordinates coordinates) {
			var z = coordinates.Z;
			if (z < 0 || z >= cellCountZ) return null;
			
			var x = coordinates.X + z / 2;
			if (x < 0 || x >= cellCountX) return null;
			
			return m_cells[x + z * cellCountX];
		} // GetCell ()

		public void ShowUI (bool visible) {
			foreach (var chunk in m_gridChunks) chunk.ShowUI(visible);
		} // ShowUI ()

		private void CreateCell (int x, int z, int i) {
			Vector3 position;
			position.x = (x + z * 0.5f - z / 2) * (HexMetrics.InnerRadius * 2f);
			position.y = 0f;
			position.z = z * (HexMetrics.OuterRadius * 1.5f);

			var cell = m_cells[i] = Instantiate<HexCell>(cellPrefab);
			cell.transform.localPosition = position;
			cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

			if (x > 0) cell.SetNeighbor(HexDirection.W, m_cells[i - 1]);
			
			if (z > 0) {
				if ((z & 1) == 0) {
					cell.SetNeighbor(HexDirection.SE, m_cells[i - cellCountX]);
					if (x > 0) cell.SetNeighbor(HexDirection.SW, m_cells[i - cellCountX - 1]);
				} else {
					cell.SetNeighbor(HexDirection.SW, m_cells[i - cellCountX]);
					if (x < cellCountX - 1) cell.SetNeighbor(HexDirection.SE, m_cells[i - cellCountX + 1]);
				}
			}

			var label = Instantiate(cellLabelPrefab);
			label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
			label.text = cell.coordinates.ToStringOnSeparateLines();
			cell.uiRect = label.rectTransform;

			cell.Elevation = 0;

			AddCellToChunk(x, z, cell);
		} // CreateCell ()

		private void AddCellToChunk (int x, int z, HexCell cell) {
			var chunkX = x / HexMetrics.ChunkSizeX;
			var chunkZ = z / HexMetrics.ChunkSizeZ;
			var chunk = m_gridChunks[chunkX + chunkZ * m_chunkCountX];

			var localX = x - chunkX * HexMetrics.ChunkSizeX;
			var localZ = z - chunkZ * HexMetrics.ChunkSizeZ;
			chunk.AddCell(localX + localZ * HexMetrics.ChunkSizeX, cell);
		} // AddCellToChunk ()
		
	} // Class HexGrid

} // Namespace Arkayns Reckon HM