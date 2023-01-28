using System;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace Arkayns.Reckon.HM {

    public class HexMapSaveLoad : MonoBehaviour {

        // -- Variables --
        public HexGrid hexGrid;
        public Text menuLabel, actionButtonLabel;
        public InputField nameInput;
        private bool saveMode;
        public RectTransform listContent;
        public HexMapSaveLoadItem itemPrefab;
        
        // -- Methods --
        public void Open (bool saveMode) {
            this.saveMode = saveMode;
            
            if (saveMode) {
                menuLabel.text = "Save Map";
                actionButtonLabel.text = "Save";
            }
            else {
                menuLabel.text = "Load Map";
                actionButtonLabel.text = "Load";
            }
            
            FillList();
            gameObject.SetActive(true);
            HexMapCamera.Locked = true;
        } // Open ()
        
        public void Close () {
            gameObject.SetActive(false);
            HexMapCamera.Locked = false;
        } // Close ()
        
        public void Action () {
            var path = GetSelectedPath();
            if (path == null) return;
            
            if (saveMode) {
                Save(path);
            } else {
                Load(path);
            }
            
            Close();
        } // Action ()
        
        public void Delete () {
            var path = GetSelectedPath();
            if (path == null) return;
            if (File.Exists(path)) File.Delete(path);
            nameInput.text = "";
            FillList();
        } // Delete ()
        
        public void SelectItem (string name) {
            nameInput.text = name;
        } // SelectItem ()
        
        private void FillList () {
            for (var i = 0; i < listContent.childCount; i++) {
                Destroy(listContent.GetChild(i).gameObject);
            }
            
            var paths = Directory.GetFiles(Application.persistentDataPath, "*.map");
            Array.Sort(paths);
            
            foreach (var path in paths) {
                var item = Instantiate(itemPrefab, listContent, false);
                item.menu = this;
                item.MapName = Path.GetFileNameWithoutExtension(path);
            }
        } // FillList ()
        
        private void Save (string path) {
            using var writer = new BinaryWriter(File.Open(path, FileMode.Create));
            writer.Write(1);
            hexGrid.Save(writer);
        } // Save ()

        private void Load (string path) {
            if (!File.Exists(path)) {
                Debug.LogError("File does not exist " + path);
                return;
            }
            
            using var reader = new BinaryReader(File.OpenRead(path));
            var header = reader.ReadInt32();
            if (header <= 1) {
                hexGrid.Load(reader, header);
                HexMapCamera.ValidatePosition();
            } else {
                Debug.LogWarning("Unknown map format " + header);
            }
        } // Load ()
        
        private string GetSelectedPath () {
            var mapName = nameInput.text;
            return mapName.Length == 0 ? null : Path.Combine(Application.persistentDataPath, mapName + ".map");
        } // GetSelectedPath ()
        
    } // Class HexMapSaveLoad

} // Namespace Arkayns Reckon HM