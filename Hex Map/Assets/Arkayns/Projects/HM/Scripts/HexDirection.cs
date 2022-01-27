namespace Arkayns.HM {
    
    public enum HexDirection {
        NE, E, SE,
        SW, W, NW    
    } // Enum HexDirection
    
    public static class HexDirectionExtensions {
        public static HexDirection Opposite (this HexDirection direction) {
            return (int)direction < 3 ? (direction + 3) : (direction - 3);
        } // Static Opposite
    } // Static Class HexDirectionExtensions
    
} // Namespace Arkayns HM