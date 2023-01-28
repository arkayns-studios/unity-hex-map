using UnityEngine;

namespace Arkayns.Reckon.HM {

    public class HexMapMenu : MonoBehaviour {

        //  -- Variables --
        public HexGrid hexGrid;

        // -- Methods --
        public void Open () {
            gameObject.SetActive(true);
            HexMapCamera.Locked = true;
        } // Open ()

        public void Close () {
            gameObject.SetActive(false);
            HexMapCamera.Locked = false;
        } // Close ()
        
        private void CreateMap (int x, int z) {
            hexGrid.CreateMap(x, z);
            HexMapCamera.ValidatePosition();
            Close();
        } // CreateMap ()
        
        public void CreateSmallMap () {
            CreateMap(20, 15);
        } // CreateSmallMap ()

        public void CreateMediumMap () {
            CreateMap(40, 30);
        } // CreateMediumMap ()

        public void CreateLargeMap () {
            CreateMap(80, 60);
        } // CreateLargeMap ()
        
    } // Class HexMapMenu

} // Namespace Arkayns Reckon HM