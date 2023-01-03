using UnityEngine;

namespace Arkayns.Reckon.HM {
    
    public class HexMapCamera : MonoBehaviour {
    
        // -- Variables --
        private Transform m_swivel, m_stick;
        public float m_stickMinZoom, m_stickMaxZoom;
        public float m_swivelMinZoom, m_swivelMaxZoom;
        private float m_zoom = 1f;
        public float moveSpeedMinZoom;
        public float moveSpeedMaxZoom;
        public float rotationSpeed;
        private float rotationAngle = 0f;
        public HexGrid grid;

        // -- Built-In Methods --
        private void Awake() {
            m_swivel = transform.GetChild(0);
            m_stick = m_swivel.GetChild(0);
        } // Awake ()

        private void Update() {
            var zoomDelta = Input.GetAxis("Mouse ScrollWheel");
            var xDelta = Input.GetAxis("Horizontal");
            var zDelta = Input.GetAxis("Vertical");
            var rotationDelta = Input.GetAxis("Rotation");
            
            if (zoomDelta != 0f) HandleZoom(zoomDelta);
            if (xDelta != 0f || zDelta != 0f) HandlePosition(xDelta, zDelta);
            if (rotationDelta != 0f) HandleRotation(rotationDelta);

        } // Update ()
        
        // -- Methods --
        private void HandleZoom(float _delta) {
            m_zoom = Mathf.Clamp01(m_zoom + _delta);
            
            var distance = Mathf.Lerp(m_stickMinZoom, m_stickMaxZoom, m_zoom);
            m_stick.localPosition = new Vector3(0f, 0f, distance);
            
            var angle = Mathf.Lerp(m_swivelMinZoom, m_swivelMaxZoom, m_zoom);
            m_swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
        } // HandleZoom ()

        private void HandlePosition(float _xDelta, float _zDelta) {
            var direction = transform.localRotation * new Vector3(_xDelta, 0f, _zDelta).normalized;
            var damping = Mathf.Max(Mathf.Abs(_xDelta), Mathf.Abs(_zDelta));
            var distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, m_zoom) * damping * Time.deltaTime;
            
            var position = transform.localPosition;
            position += direction * distance;
            transform.localPosition = ClampPosition(position);
        } // HandlePosition ()

        private void HandleRotation(float _delta) {
            rotationAngle += _delta * rotationSpeed * Time.deltaTime;
            switch (rotationAngle) {
                case < 0f: rotationAngle += 360f; break;
                case >= 360f: rotationAngle -= 360f; break;
            }
            transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
        } // HandleRotation ()
        
        private Vector3 ClampPosition(Vector3 _position) {
            const float halfCell = 0.5f; const float fullCell = 1f;
            var xMax = (grid.chunkCountX * HexMetrics.chunkSizeX - halfCell) * (2f * HexMetrics.innerRadius);
            _position.x = Mathf.Clamp(_position.x, 0f, xMax);
            
            var zMax = (grid.chunkCountZ * HexMetrics.chunkSizeZ - fullCell) * (2f * HexMetrics.outerRadius);
            _position.z = Mathf.Clamp(_position.z, 0f, zMax);
            
            return _position;
        } // ClampPosition ()
        
    } // Class HexMapCamera
    
} // Namespace Arkayns Reckon HexMap