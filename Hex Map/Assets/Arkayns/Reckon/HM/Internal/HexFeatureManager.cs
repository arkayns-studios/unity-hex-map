using UnityEngine;

namespace Arkayns.Reckon.HM {

    public class HexFeatureManager : MonoBehaviour {

        // -- Variables --
        public HexFeatureCollection[] urbanCollections, farmCollections, plantCollections;
        private Transform m_container;
        
        // -- Methods --
        public void Clear() {
            if (m_container) {
                Destroy(m_container.gameObject);
            }
            m_container = new GameObject("Features Container").transform;
            m_container.SetParent(transform, false);
        } // Clear ()

        public void Apply () {} // Apply ()

        public void AddFeature(HexCell cell, Vector3 position) {
            var hash = HexMetrics.SampleHashGrid(position);
            var prefab = PickPrefab(urbanCollections, cell.UrbanLevel, hash.a, hash.d);
            var otherPrefab = PickPrefab(farmCollections, cell.FarmLevel, hash.b, hash.d);
            
            var usedHash = hash.a;
            if (prefab) {
                if (otherPrefab && hash.b < hash.a) {
                    prefab = otherPrefab;
                    usedHash = hash.b;
                }
            } else if (otherPrefab) {
                prefab = otherPrefab;
                usedHash = hash.b;
            }
            
            otherPrefab = PickPrefab(plantCollections, cell.PlantLevel, hash.c, hash.d);
            if (prefab) {
                if (otherPrefab && hash.c < usedHash) {
                    prefab = otherPrefab;
                }
            } else if (otherPrefab) {
                prefab = otherPrefab;
            } else {
                return;
            }
            
            var instance = Instantiate(prefab);
            position.y += instance.localScale.y * 0.5f;
            instance.localPosition = HexMetrics.Perturb(position);
            instance.localRotation = Quaternion.Euler(0f, 360f * hash.e, 0f);
            instance.SetParent(m_container, false);
        } // AddFeature ()

        private Transform PickPrefab (HexFeatureCollection[] collection, int level, float hash, float choice) {
            if (level <= 0) return null;
            var thresholds = HexMetrics.GetFeatureThresholds(level - 1);
            for (var i = 0; i < thresholds.Length; i++) {
                if (hash < thresholds[i]) return collection[i].Pick(choice);
            }
            return null;
        } // PickPrefab ()
        
    } // Class HexFeatureManager

} // Namespace Arkayns Reckon HM