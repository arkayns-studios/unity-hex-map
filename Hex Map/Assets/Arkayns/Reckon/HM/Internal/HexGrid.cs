using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable once PossibleLossOfFraction

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
		
		private HexCellPriorityQueue m_searchFrontier;
		private int m_searchFrontierPhase;

		private HexCell m_currentPathFrom, m_currentPathTo;
		private bool m_currentPathExists;
		
		private List<HexUnit> m_units = new ();
		public HexUnit unitPrefab;
		
		private HexCellShaderData m_cellShaderData;
		
		// -- Property --
		public bool HasPath => m_currentPathExists;

		// -- Built-In Methods --
		private void Awake () {
			HexMetrics.noiseSource = noiseSource;
			HexMetrics.InitializeHashGrid(seed);
			HexUnit.unitPrefab = unitPrefab;
			m_cellShaderData = gameObject.AddComponent<HexCellShaderData>();
			CreateMap(cellCountX, cellCountZ);
		} // Awake ()
		
		private void OnEnable () {
			if (HexMetrics.noiseSource) return;
			HexMetrics.noiseSource = noiseSource;
			HexMetrics.InitializeHashGrid(seed);
			HexUnit.unitPrefab = unitPrefab;
		} // OnEnable ()

		// -- Methods --
		public void Save (BinaryWriter writer) {
			writer.Write(cellCountX);
			writer.Write(cellCountZ);
			foreach (var cell in m_cells) cell.Save(writer);
			
			writer.Write(m_units.Count);
			foreach (var unit in m_units) unit.Save(writer);
		} // Save ()

		public void Load (BinaryReader reader, int header) {
			ClearPath();
			ClearUnits();
			int x = 20, z = 15;
			if (header >= 1) {
				x = reader.ReadInt32();
				z = reader.ReadInt32();
			}
			
			if (x != cellCountX || z != cellCountZ)
				if (!CreateMap(x, z)) return;
			foreach (var cell in m_cells) cell.Load(reader);
			foreach (var chunk in m_gridChunks) chunk.Refresh();

			if (header >= 2) {
				var unitCount = reader.ReadInt32();
				for (var i = 0; i < unitCount; i++) 
					HexUnit.Load(reader, this);
			}
		} // Load ()

		public bool  CreateMap (int x, int z) {
			if (x <= 0 || x % HexMetrics.ChunkSizeX != 0 || z <= 0 || z % HexMetrics.ChunkSizeZ != 0) {
				Debug.LogError("Unsupported map size.");
				return false;
			}
			
			ClearPath();
			ClearUnits();
			if (m_gridChunks != null) {
				foreach (var t in m_gridChunks) Destroy(t.gameObject);
			}
			
			cellCountX = x;
			cellCountZ = z;
			m_chunkCountX = cellCountX / HexMetrics.ChunkSizeX;
			m_chunkCountZ = cellCountZ / HexMetrics.ChunkSizeZ;
			m_cellShaderData.Initialize(cellCountX, cellCountZ);
			
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

		private void AddCellToChunk (int x, int z, HexCell cell) {
			var chunkX = x / HexMetrics.ChunkSizeX;
			var chunkZ = z / HexMetrics.ChunkSizeZ;
			var chunk = m_gridChunks[chunkX + chunkZ * m_chunkCountX];

			var localX = x - chunkX * HexMetrics.ChunkSizeX;
			var localZ = z - chunkZ * HexMetrics.ChunkSizeZ;
			chunk.AddCell(localX + localZ * HexMetrics.ChunkSizeX, cell);
		} // AddCellToChunk ()

		private void CreateCells () {
			m_cells = new HexCell[cellCountZ * cellCountX];
			for (int z = 0, i = 0; z < cellCountZ; z++) 
				for (var x = 0; x < cellCountX; x++) CreateCell(x, z, i++);
		} // CreateCells ()
		
		private void CreateCell (int x, int z, int i) {
			Vector3 position;
			position.x = (x + z * 0.5f - z / 2) * (HexMetrics.InnerRadius * 2f);
			position.y = 0f;
			position.z = z * (HexMetrics.OuterRadius * 1.5f);

			var cell = m_cells[i] = Instantiate(cellPrefab);
			cell.transform.localPosition = position;
			cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
			cell.Index = i;
			cell.ShaderData = m_cellShaderData;

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
			cell.uiRect = label.rectTransform;

			cell.Elevation = 0;

			AddCellToChunk(x, z, cell);
		} // CreateCell ()

		private void ClearUnits () {
			foreach (var unit in m_units) unit.Die();
			m_units.Clear();
		} // ClearUnits ()
		
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

		public HexCell GetCell (Ray ray) {
			return Physics.Raycast(ray, out var hit) ? GetCell(hit.point) : null;
		} // GetCell ()
		
		public void ClearPath () {
			if (m_currentPathExists) {
				var current = m_currentPathTo;
				while (current != m_currentPathFrom) {
					current.SetLabel(null);
					current.DisableHighlight();
					current = current.PathFrom;
				}
				current.DisableHighlight();
				m_currentPathExists = false;
			} else if (m_currentPathFrom) {
				m_currentPathFrom.DisableHighlight();
				m_currentPathTo.DisableHighlight();
			}
			m_currentPathFrom = m_currentPathTo = null;
		} // ClearPath ()
		
		public void FindPath (HexCell fromCell, HexCell toCell, int speed) {
			ClearPath();
			m_currentPathFrom = fromCell;
			m_currentPathTo = toCell;
			m_currentPathExists = Search(fromCell, toCell, speed);
			ShowPath(speed);
		} // FindPath ()

		public List<HexCell> GetPath () {
			if (!m_currentPathExists) return null;
			var path = ListPool<HexCell>.Get();
			for (var c = m_currentPathTo; c != m_currentPathFrom; c = c.PathFrom) path.Add(c);
			path.Add(m_currentPathFrom);
			path.Reverse();
			return path;
		} // GetPath ()
		
		private bool Search (HexCell fromCell, HexCell toCell, int speed) {
			m_searchFrontierPhase += 2;
			if (m_searchFrontier == null) m_searchFrontier = new HexCellPriorityQueue();
			else m_searchFrontier.Clear();
			
			fromCell.SearchPhase = m_searchFrontierPhase;
			fromCell.Distance = 0;
			m_searchFrontier.Enqueue(fromCell);
			
			while (m_searchFrontier.Count > 0) {
				var current = m_searchFrontier.Dequeue();
				current.SearchPhase += 1;

				if (current == toCell) return true;
				var currentTurn = (current.Distance - 1) / speed;
				
				for (var d = HexDirection.NE; d <= HexDirection.NW; d++) {
					var neighbor = current.GetNeighbor(d);

					if (neighbor == null || neighbor.SearchPhase > m_searchFrontierPhase) continue;
					if (neighbor.IsUnderwater || neighbor.Unit) continue;
					
					var edgeType = current.GetEdgeType(neighbor);
					if (edgeType == HexEdgeType.Cliff) continue;

					int moveCost;
					if (current.HasRoadThroughEdge(d)) {
						moveCost = 1;
					} else if (current.Walled != neighbor.Walled) {
						continue;
					} else {
						moveCost = edgeType == HexEdgeType.Flat ? 5 : 10;
						moveCost += neighbor.UrbanLevel + neighbor.FarmLevel + neighbor.PlantLevel;
					}
					
					var distance = current.Distance + moveCost;
					var turn = distance / speed;
					if (turn > currentTurn) {
						distance = turn * speed + moveCost;
					}
					
					if (neighbor.SearchPhase < m_searchFrontierPhase) {
						neighbor.SearchPhase = m_searchFrontierPhase;
						neighbor.Distance = distance;
						neighbor.PathFrom = current;
						neighbor.SearchHeuristic = neighbor.coordinates.DistanceTo(toCell.coordinates);
						m_searchFrontier.Enqueue(neighbor);
					} else if (distance < neighbor.Distance) {
						var oldPriority = neighbor.SearchPriority;
						neighbor.Distance = distance;
						neighbor.PathFrom = current;
						m_searchFrontier.Change(neighbor, oldPriority);
					}
				}
			}
			return false;
		} // Search ()

		private void ShowPath (int speed) {
			if (m_currentPathExists) {
				var current = m_currentPathTo;
				while (current != m_currentPathFrom) {
					var turn = (current.Distance - 1) / speed;
					current.SetLabel(turn.ToString());
					current.EnableHighlight(Color.white);
					current = current.PathFrom;
				}
			}
			m_currentPathFrom.EnableHighlight(Color.blue);
			m_currentPathTo.EnableHighlight(Color.red);
		} // ShowPath ()
		
		public void AddUnit (HexUnit unit, HexCell location, float orientation) {
			m_units.Add(unit);
			unit.transform.SetParent(transform, false);
			unit.Location = location;
			unit.Orientation = orientation;
		} // AddUnit ()
		
		public void RemoveUnit (HexUnit unit) {
			m_units.Remove(unit);
			unit.Die();
		} // RemoveUnit ()
		
		public void ShowUI (bool visible) {
			foreach (var chunk in m_gridChunks) chunk.ShowUI(visible);
		} // ShowUI ()
		
	} // Class HexGrid

} // Namespace Arkayns Reckon HM