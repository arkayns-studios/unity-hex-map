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
		public static HexCoordinates FromOffsetCoordinates (int x, int z) {
			return new HexCoordinates(x - z / 2, z);
		} // FromOffsetCoordinates ()

		public static HexCoordinates FromPosition (Vector3 position) {
			var x = position.x / (HexMetrics.InnerRadius * 2f);
			var y = -x;

			var offset = position.z / (HexMetrics.OuterRadius * 3f);
			x -= offset;
			y -= offset;

			var iX = Mathf.RoundToInt(x);
			var iY = Mathf.RoundToInt(y);
			var iZ = Mathf.RoundToInt(-x -y);

			if (iX + iY + iZ != 0) {
				var dX = Mathf.Abs(x - iX);
				var dY = Mathf.Abs(y - iY);
				var dZ = Mathf.Abs(-x -y - iZ);

				if (dX > dY && dX > dZ) iX = -iY - iZ;
				else if (dZ > dY) iZ = -iX - iY;
			}

			return new HexCoordinates(iX, iZ);
		} // FromPosition ()

		public override string ToString () {
			return $"[{X.ToString()}, {Y.ToString()}, {Z.ToString()}]";
		} // ToString ()

		public string ToStringOnSeparateLines () {
			return $"{X.ToString()}\n{Y.ToString()}\n{Z.ToString()}";
		} // ToStringOnSeparateLines ()
		
	} // Struct HexCoordinates

} // Namespace Arkayns Reckon HM