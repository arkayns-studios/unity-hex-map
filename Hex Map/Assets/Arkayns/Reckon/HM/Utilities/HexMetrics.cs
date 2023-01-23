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

		public const float NoiseScale = 0.003f;

		public const int ChunkSizeX = 5, ChunkSizeZ = 5;

		private static Vector3[] m_corners = {
			new (0f, 0f, OuterRadius),
			new (InnerRadius, 0f, 0.5f * OuterRadius),
			new (InnerRadius, 0f, -0.5f * OuterRadius),
			new (0f, 0f, -OuterRadius),
			new (-InnerRadius, 0f, -0.5f * OuterRadius),
			new (-InnerRadius, 0f, 0.5f * OuterRadius),
			new (0f, 0f, OuterRadius)
		};

		public static Texture2D noiseSource;

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
		
	} // Class HexMetrics

} // Namespace Arkayns Reckon HM