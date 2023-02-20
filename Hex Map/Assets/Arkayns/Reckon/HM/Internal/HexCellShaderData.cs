using UnityEngine;

namespace Arkayns.Reckon.HM {

    public class HexCellShaderData : MonoBehaviour {

        // -- Variables -- 
        private Texture2D m_cellTexture;
        private Color32[] m_cellTextureData;
        
        // -- Built-In Methods --
        private void LateUpdate () {
            m_cellTexture.SetPixels32(m_cellTextureData);
            m_cellTexture.Apply();
            enabled = false;
        } // LateUpdate ()
        
        // -- Methods --
        public void Initialize (int x, int z) {
            if (m_cellTexture) {
                m_cellTexture.Reinitialize(x, z);
            } else {
                m_cellTexture = new Texture2D(x, z, TextureFormat.RGBA32, false, true) {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
            }
            
            if (m_cellTextureData == null || m_cellTextureData.Length != x * z) {
                m_cellTextureData = new Color32[x * z];
            } else {
                for (var i = 0; i < m_cellTextureData.Length; i++) {
                    m_cellTextureData[i] = new Color32(0, 0, 0, 0);
                }
            }
        } // Initialize ()
        
        public void RefreshTerrain (HexCell cell) {
            m_cellTextureData[cell.Index].a = (byte)cell.TerrainTypeIndex;
            enabled = true;
        } // RefreshTerrain ()

    } // Class HexCellShaderData

} // Namespace Arkayns Reckon HexMap