using UnityEngine;
using UnityEngine.UI;

namespace Arkayns.Reckon.HM {

    public class HexMapSaveLoadItem : MonoBehaviour {

        // -- Variables --
        public HexMapSaveLoad menu;
        private string mapName;
        
        // -- Property --
        public string MapName {
            get => mapName;
            set {
                mapName = value;
                transform.GetChild(0).GetComponent<Text>().text = value;
            }
        } // MapName
	
	    // -- Method --
        public void Select () {
            menu.SelectItem(mapName);
        } // Select ()

    } // Class HexMapSaveLoadItem

} // Namespace Arkayns Reckon HM