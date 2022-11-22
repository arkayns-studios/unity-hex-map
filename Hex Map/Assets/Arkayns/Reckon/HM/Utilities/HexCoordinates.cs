using UnityEngine;

namespace Arkayns.Reckon.HM {
	
	[System.Serializable]
	public struct HexCoordinates {

		// -- Variables --
		[SerializeField] private int x, z;

		
		// -- Properties --
		public int X => x;
		
		public int Z => z;
		
		public int Y => -X - Z;

		
		// -- Constructor --
		public HexCoordinates (int x, int z) {
			this.x = x;
			this.z = z;
		} // Constructor HexCoordinates

		
		// -- Methods --
		public static HexCoordinates FromOffsetCoordinates (int x, int z) => new HexCoordinates(x - z / 2, z);

		public static HexCoordinates FromPosition (Vector3 position) {
			var x = position.x / (HexMetrics.innerRadius * 2f);
			var y = -x;

			var offset = position.z / (HexMetrics.outerRadius * 3f);
			x -= offset;
			y -= offset;

			var iX = Mathf.RoundToInt(x);
			var iY = Mathf.RoundToInt(y);
			var iZ = Mathf.RoundToInt(-x -y);

			if (iX + iY + iZ == 0) return new HexCoordinates(iX, iZ);
			var dX = Mathf.Abs(x - iX);
			var dY = Mathf.Abs(y - iY);
			var dZ = Mathf.Abs(-x -y - iZ);

			if (dX > dY && dX > dZ) iX = -iY - iZ;
			else if (dZ > dY) iZ = -iX - iY;
			return new HexCoordinates(iX, iZ);
		} // FromPosition

		public override string ToString () => $"[{X.ToString()}, {Y.ToString()}, {Z.ToString()}]";

		public string ToStringOnSeparateLines () => $"({X.ToString()}\n{Y.ToString()}\n{Z.ToString()})";
		
	} // Class HexCoordinates
	
} // Namespace Arkayns Reckon HexMap