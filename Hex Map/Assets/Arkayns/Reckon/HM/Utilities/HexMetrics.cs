using UnityEngine;

// ReSharper disable PossibleLossOfFraction

namespace Arkayns.Reckon.HM {
	
	public static class HexMetrics {
		
		// -- Variables --
		public const float outerToInner = 0.866025404f;
		public const float innerToOuter = 1f / outerToInner;
		
		public const float outerRadius = 10f;
		public const float innerRadius = outerRadius * outerToInner;

		public const float solidFactor = 0.8f;
		public const float blendFactor = 1f - solidFactor;

		public const float elevationStep = 3f;
		public const float elevationPerturbStrength = 1.5f;
		public const int terracesPerSlope = 2;
		public const int terraceSteps = terracesPerSlope * 2 + 1;

		public const float horizontalTerraceStepSize = 1f / terraceSteps;
		public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);
		public const float cellPerturbStrength = 0f; // 4f;

		public const float noiseScale = 0.003f;
		public const int chunkSizeX = 5, chunkSizeZ = 5;

		public static Texture2D NoiseSource;
		
		private static Vector3[] m_corners = {
			new (0f, 0f, outerRadius),
			new (innerRadius, 0f, 0.5f * outerRadius),
			new (innerRadius, 0f, -0.5f * outerRadius),
			new (0f, 0f, -outerRadius),
			new (-innerRadius, 0f, -0.5f * outerRadius),
			new (-innerRadius, 0f, 0.5f * outerRadius),
			new (0f, 0f, outerRadius)
		};

		public const float streamBedElevationOffset = -1f;

		// -- Methods --
		public static Vector4 SampleNoise (Vector3 position) => NoiseSource.GetPixelBilinear(position.x * noiseScale, position.z * noiseScale);

		public static Vector3 GetFirstCorner (HexDirection direction) => m_corners[(int)direction];

		public static Vector3 GetSecondCorner (HexDirection direction) => m_corners[(int)direction + 1];

		public static Vector3 GetFirstSolidCorner (HexDirection direction) => m_corners[(int)direction] * solidFactor;

		public static Vector3 GetSecondSolidCorner (HexDirection direction) => m_corners[(int)direction + 1] * solidFactor;

		public static Vector3 GetSolidEdgeMiddle(HexDirection direction) {
			return (m_corners[(int)direction] + m_corners[(int)direction + 1]) * (0.5f * solidFactor);
		} // GetSolidEdgeMiddle () ()
		
		public static Vector3 GetBridge (HexDirection direction) => (m_corners[(int)direction] + m_corners[(int)direction + 1]) * blendFactor;

		public static Vector3 TerraceLerp (Vector3 a, Vector3 b, int step) {
			var h = step * horizontalTerraceStepSize;
			a.x += (b.x - a.x) * h;
			a.z += (b.z - a.z) * h;
			var v = ((step + 1) / 2) * verticalTerraceStepSize;
			a.y += (b.y - a.y) * v;
			return a;
		} // TerraceLerp ()

		public static Color TerraceLerp (Color a, Color b, int step) {
			var h = step * horizontalTerraceStepSize;
			return Color.Lerp(a, b, h);
		} // TerraceLerp ()

		public static HexEdgeType GetEdgeType (int elevation1, int elevation2) {
			if (elevation1 == elevation2) return HexEdgeType.Flat;
			var delta = elevation2 - elevation1;
			return delta is 1 or -1 ? HexEdgeType.Slope : HexEdgeType.Cliff;
		} // GetEdgeType ()
		
		public static Vector3 Perturb(Vector3 position) {
			var sample = SampleNoise(position);
			position.x += (sample.x * 2f - 1f) * cellPerturbStrength;
			position.z += (sample.z * 2f - 1f) * cellPerturbStrength;
			return position;
		} // Perturb ()
		
	} // Class HexMetrics
	
} // Namespace Arkayns Reckon HexMap