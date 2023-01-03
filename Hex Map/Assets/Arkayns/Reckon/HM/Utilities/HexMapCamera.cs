using UnityEngine;

namespace Arkayns.Reckon.HM {
    
    public class HexMapCamera : MonoBehaviour {
    
        // -- Variables --
        private Transform m_swivel, m_stick;
        public float m_stickMinZoom, m_stickMaxZoom;
        public float m_swivelMinZoom, m_swivelMaxZoom;
        private float m_zoom = 1f;

        // -- Built-In Methods --
        private void Awake() {
            m_swivel = transform.GetChild(0);
            m_stick = m_swivel.GetChild(0);
        } // Awake ()

        private void Update() {
            var zoomDelta = Input.GetAxis("Mouse ScrollWheel");
            
            if (zoomDelta == 0) return;
            AdjustZoom(zoomDelta);
            
            var distance = Mathf.Lerp(m_stickMinZoom, m_stickMaxZoom, m_zoom);
            m_stick.localPosition = new Vector3(0f, 0f, distance);
            
            var angle = Mathf.Lerp(m_swivelMinZoom, m_swivelMaxZoom, m_zoom);
            m_swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
        } // Update ()
        
        // -- Methods --
        private void AdjustZoom(float _delta) {
            m_zoom = Mathf.Clamp01(m_zoom + _delta);
        } // AdjustZoom ()

    } // Class HexMapCamera
    
} // Namespace Arkayns Reckon HexMap