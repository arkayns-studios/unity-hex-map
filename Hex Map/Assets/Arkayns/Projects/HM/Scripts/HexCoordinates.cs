namespace Arkayns.HM {
    
    [System.Serializable]
    public struct HexCoordinates {
        public  int X { get; private set; }
        public  int Z { get; private set; }
        public int Y => -X - Z;
        
        public HexCoordinates(int x, int z) {
            X = x;
            Z = z;
        } // Constructor HexCoordinates
        
        public static HexCoordinates FromOffsetCoordinates (int x, int z) {
            return new HexCoordinates(x -z / 2, z);
        } // FromOffsetCoordinates
        
        public string ToStringOnSeparateLines () {
            return $"{X}\n{Y}\n{Z}";
        } // ToStringOnSeparateLines
        
        public override string ToString () {
            return $"({X}, {Y}, {Z})";
        } // Override ToString
        
    } // Struct HexCoordinates
    
} // Namespace Arkayns HM