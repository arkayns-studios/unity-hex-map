using UnityEngine;

namespace Arkayns.Reckon.HM {

    public class HexFeatureManager : MonoBehaviour {

        // -- Variables --
        public HexFeatureCollection[] urbanCollections;
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
            var prefab = PickPrefab(cell.UrbanLevel, hash.a, hash.b);
            if (!prefab) return;
            var instance = Instantiate(prefab);
            position.y += instance.localScale.y * 0.5f;
            instance.localPosition = HexMetrics.Perturb(position);
            instance.localRotation = Quaternion.Euler(0f, 360f * hash.c, 0f);
            instance.SetParent(m_container, false);
        } // AddFeature ()

        private Transform PickPrefab (int level, float hash, float choice) {
            if (level <= 0) return null;
            var thresholds = HexMetrics.GetFeatureThresholds(level - 1);
            for (var i = 0; i < thresholds.Length; i++) {
                if (hash < thresholds[i]) return urbanCollections[i].Pick(choice);
            }
            return null;
        } // PickPrefab ()
        
    } // Class HexFeatureManager

} // Namespace Arkayns Reckon HM