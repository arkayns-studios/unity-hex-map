namespace Arkayns.HM {
    
    public enum HexDirection {
        NE, E, SE,
        SW, W, NW    
    } // Enum HexDirection
    
    public static class HexDirectionExtensions {
        public static HexDirection Opposite (this HexDirection direction) {
            return (int)direction < 3 ? (direction + 3) : (direction - 3);
        } // Static Opposite
        
        public static HexDirection Previous (this HexDirection direction) {
            return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
        } // Static Previous

        public static HexDirection Next (this HexDirection direction) {
            return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
        } // Static Next
        
    } // Static Class HexDirectionExtensions
    
} // Namespace Arkayns HM