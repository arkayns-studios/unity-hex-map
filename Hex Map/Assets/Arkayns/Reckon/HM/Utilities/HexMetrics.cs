using UnityEngine;

namespace Arkayns.Reckon.HM {

	public static class HexMetrics {

		// -- Constants --
		public const float OuterToInner = 0.866025404f;
		public const float InnerToOuter = 1f / OuterToInner;

		public const float OuterRadius = 10f;

		public const float InnerRadius = OuterRadius * OuterToInner;

		public const float SolidFactor = 0.8f;

		public const float BlendFactor = 1f - SolidFactor;

		public const float ElevationStep = 3f;

		public const int TerracesPerSlope = 2;

		public const int TerraceSteps = TerracesPerSlope * 2 + 1;

		public const float HorizontalTerraceStepSize = 1f / TerraceSteps;

		public const float VerticalTerraceStepSize = 1f / (TerracesPerSlope + 1);

		public const float CellPerturbStrength = 4f;

		public const float ElevationPerturbStrength = 1.5f;

		public const float StreamBedElevationOffset = -1.75f;

		public const float WaterElevationOffset = -0.5f;

		public const float WaterFactor = 0.6f;
		
		public const float WaterBlendFactor = 1f - WaterFactor;
		
		public const float NoiseScale = 0.003f;

		public const int ChunkSizeX = 5, ChunkSizeZ = 5;

		public static Texture2D noiseSource;

		public const int HashGridSize = 256;
		public const float HashGridScale = 0.25f;
		
		public const float WallHeight = 4f;
		public const float WallThickness = 0.75f;
		public const float WallTowerThreshold = 0.5f;
		public const float WallYOffset = -1f;
		
		public const float WallElevationOffset = VerticalTerraceStepSize;
		
		private static HexHash[] m_hashGrid;

		private static Vector3[] m_corners = {
			new (0f, 0f, OuterRadius),
			new (InnerRadius, 0f, 0.5f * OuterRadius),
			new (InnerRadius, 0f, -0.5f * OuterRadius),
			new (0f, 0f, -OuterRadius),
			new (-InnerRadius, 0f, -0.5f * OuterRadius),
			new (-InnerRadius, 0f, 0.5f * OuterRadius),
			new (0f, 0f, OuterRadius)
		};

		private static float[][] m_featureThresholds = {
			new float[] {0.0f, 0.0f, 0.4f},
			new float[] {0.0f, 0.4f, 0.6f},
			new float[] {0.4f, 0.6f, 0.8f}
		};

		// -- Methods --
		public static Vector4 SampleNoise (Vector3 position) {
			return noiseSource.GetPixelBilinear(position.x * NoiseScale, position.z * NoiseScale);
		} // SampleNoise ()

		public static Vector3 GetFirstCorner (HexDirection direction) {
			return m_corners[(int)direction];
		} // GetFirstCorner ()

		public static Vector3 GetSecondCorner (HexDirection direction) {
			return m_corners[(int)direction + 1];
		} // GetSecondCorner ()

		public static Vector3 GetFirstSolidCorner (HexDirection direction) {
			return m_corners[(int)direction] * SolidFactor;
		} // GetFirstSolidCorner ()

		public static Vector3 GetSecondSolidCorner (HexDirection direction) {
			return m_corners[(int)direction + 1] * SolidFactor;
		} // GetSecondSolidCorner ()

		public static Vector3 GetSolidEdgeMiddle (HexDirection direction) {
			return (m_corners[(int)direction] + m_corners[(int)direction + 1]) * (0.5f * SolidFactor);
		} // GetSolidEdgeMiddle ()

		public static Vector3 GetFirstWaterCorner (HexDirection direction) {
			return m_corners[(int)direction] * WaterFactor;
		} // GetFirstWaterCorner ()

		public static Vector3 GetSecondWaterCorner (HexDirection direction) {
			return m_corners[(int)direction + 1] * WaterFactor;
		} // GetSecondWaterCorner ()
		
		public static Vector3 GetWaterBridge (HexDirection direction) {
			return (m_corners[(int)direction] + m_corners[(int)direction + 1]) * WaterBlendFactor;
		} // GetWaterBridge ()
		
		public static Vector3 GetBridge (HexDirection direction) {
			return (m_corners[(int)direction] + m_corners[(int)direction + 1]) * BlendFactor;
		} // GetBridge ()

		public static Vector3 TerraceLerp (Vector3 a, Vector3 b, int step) {
			var h = step * HorizontalTerraceStepSize;
			a.x += (b.x - a.x) * h;
			a.z += (b.z - a.z) * h;
			var v = ((step + 1) / 2) * VerticalTerraceStepSize;
			a.y += (b.y - a.y) * v;
			return a;
		} // TerraceLerp ()

		public static Color TerraceLerp (Color a, Color b, int step) {
			var h = step * HorizontalTerraceStepSize;
			return Color.Lerp(a, b, h);
		} // TerraceLerp ()

		public static HexEdgeType GetEdgeType (int elevation1, int elevation2) {
			if (elevation1 == elevation2) return HexEdgeType.Flat;
			
			var delta = elevation2 - elevation1;
			return delta is 1 or -1 ? HexEdgeType.Slope : HexEdgeType.Cliff;
		} // GetEdgeType ()

		public static Vector3 Perturb (Vector3 position) {
			var sample = SampleNoise(position);
			position.x += (sample.x * 2f - 1f) * CellPerturbStrength;
			position.z += (sample.z * 2f - 1f) * CellPerturbStrength;
			return position;
		} // Perturb ()
		
		public static void InitializeHashGrid (int seed) {
			m_hashGrid = new HexHash[HashGridSize * HashGridSize];
			var currentState = Random.state;
			Random.InitState(seed);
			for (var i = 0; i < m_hashGrid.Length; i++) m_hashGrid[i] = HexHash.Create();
			Random.state = currentState;
		} // InitializeHashGrid ()
		
		public static float[] GetFeatureThresholds (int level) {
			return m_featureThresholds[level];
		} // GetFeatureThresholds ()
		
		public static HexHash SampleHashGrid (Vector3 position) {
			var x = (int)(position.x * HashGridScale) % HashGridSize;
			if (x < 0) x += HashGridSize;
			
			var z = (int)(position.z * HashGridScale) % HashGridSize;
			if (z < 0) z += HashGridSize;
			
			return m_hashGrid[x + z * HashGridSize];
		} // SampleHashGrid ()
		
		public static Vector3 WallThicknessOffset (Vector3 near, Vector3 far) {
			Vector3 offset;
			offset.x = far.x - near.x;
			offset.y = 0f;
			offset.z = far.z - near.z;
			return offset.normalized * (WallThickness * 0.5f);
		} // WallThicknessOffset ()
		
		public static Vector3 WallLerp (Vector3 near, Vector3 far) {
			near.x += (far.x - near.x) * 0.5f;
			near.z += (far.z - near.z) * 0.5f;
			var v = near.y < far.y ? WallElevationOffset : (1f - WallElevationOffset);
			near.y += (far.y - near.y) * v + WallYOffset;
			return near;
		} // WallLerp ()
		
	} // Class HexMetrics

} // Namespace Arkayns Reckon HM