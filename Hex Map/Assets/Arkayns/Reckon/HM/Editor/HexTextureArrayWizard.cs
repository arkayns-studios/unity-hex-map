using UnityEditor;
using UnityEngine;

namespace Arkayns.Reckon.HM {

    public class HexTextureArrayWizard : ScriptableWizard {

        // -- Variables --
        public Texture2D[] textures;
        
        // -- Built-In Methods --
        private void OnWizardCreate () {
            if (textures.Length == 0) return;
            
            var path = EditorUtility.SaveFilePanelInProject("Save Texture Array", "Texture Array", "asset", "Save Texture Array");
            if (path.Length == 0) return;
            
            var texture = textures[0];
            var textureArray = new Texture2DArray(texture.width, texture.height, textures.Length, texture.format, texture.mipmapCount > 1);
            textureArray.anisoLevel = texture.anisoLevel;
            textureArray.filterMode = texture.filterMode;
            textureArray.wrapMode = texture.wrapMode;
            
            for (var i = 0; i < textures.Length; i++) {
                for (var m = 0; m < texture.mipmapCount; m++) {
                    Graphics.CopyTexture(textures[i], 0, m, textureArray, i, m);
                }
            }
            
            AssetDatabase.CreateAsset(textureArray, path);
        } // OnWizardCreate ()
        
        // -- Custom Methods --
        [MenuItem("Assets/Create/Textures Array")]
        private static void CreateWizard () {
            DisplayWizard<HexTextureArrayWizard>("Create Array of Textures", "Create");
        } // CreateWizard ()

    } // Class HexTextureArrayWizard

} // Namespace Arkayns Reckon HexMap