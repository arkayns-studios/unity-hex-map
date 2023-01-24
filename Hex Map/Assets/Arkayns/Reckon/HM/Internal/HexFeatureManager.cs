using UnityEngine;

namespace Arkayns.Reckon.HM {

    public class HexFeatureManager : MonoBehaviour {

        // -- Variables --
        public Transform featurePrefab;
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

        public void AddFeature(Vector3 position) {
            var instance = Instantiate(featurePrefab);
            position.y += instance.localScale.y * 0.5f;
            instance.localPosition = HexMetrics.Perturb(position);
            instance.SetParent(m_container, false);
        } // AddFeature ()

    } // Class HexFeatureManager

} // Namespace Arkayns Reckon HM