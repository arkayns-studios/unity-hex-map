using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Arkayns.HM {

    [System.Serializable]
    public struct HexCoordinates {
        [SerializeField] private int m_x, m_z;

        public int X => m_x;
        public int Y => -X - Z;
        public int Z => m_z;

        public HexCoordinates(int x, int z) {
            m_x = x;
            m_z = z;
        } // Constructor HexCoordinates

        public static HexCoordinates FromOffsetCoordinates(int x, int z) {
            return new HexCoordinates(x - z / 2, z);
        } // FromOffsetCoordinates

        public static HexCoordinates FromPosition(Vector3 position) {
            float x = position.x / (HexMetrics.InnerRadius * 2f);
            float y = -x;
            
            float offset = position.z / (HexMetrics.OuterRadius * 3f);
            x -= offset;
            y -= offset;
            
            int iX = Mathf.RoundToInt(x);
            int iY = Mathf.RoundToInt(y);
            int iZ = Mathf.RoundToInt(-x -y);

            if (iX + iY + iZ != 0) {
                float dX = Mathf.Abs(x - iX);
                float dY = Mathf.Abs(y - iY);
                float dZ = Mathf.Abs(-x -y - iZ);

                if (dX > dY && dX > dZ) iX = -iY - iZ;
                else if (dZ > dY) iZ = -iX - iY;
            }
            
            return new HexCoordinates(iX, iZ);
        } // Static FromPosition

        public string ToStringOnSeparateLines() {
            return $"{X}\n{Y}\n{Z}";
        } // ToStringOnSeparateLines

        public override string ToString() {
            return $"[{X}, {Y}, {Z}]";
        } // Override ToString

    } // Struct HexCoordinates

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(HexCoordinates))]
    public class HexCoordinatesDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            HexCoordinates coordinates = new HexCoordinates(property.FindPropertyRelative("m_x").intValue, property.FindPropertyRelative("m_z").intValue);
            GUI.Label(position, label);
            position = EditorGUI.PrefixLabel(position, label);
            GUI.Label(position, coordinates.ToString());
        } // Override OnGUI
        
    } // HexCoordinatesDrawer
#endif

} // Namespace Arkayns HM