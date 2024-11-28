///<summary>
/// Author: Halen
/// 
/// 
/// 
///</summary>

using UnityEngine;
using UnityEngine.InputSystem;

namespace CardGame.HexMap
{
    public class HexMapCamera : MonoBehaviour
    {
        [SerializeField] private HexMap m_map;
        
        [Header("Zoom Properties")]
        [SerializeField, Min(0)] private float m_zoomModifier;

        [Space]

        [SerializeField] private float m_stickMinZoom;
        [SerializeField] private float m_stickMaxZoom;

        [Space]

        [SerializeField] private float m_swivelMinZoom;
        [SerializeField] private float m_swivelMaxZoom;
        
        private Transform m_swivel, m_stick;
        private float m_currentZoom;

        [Header("Movement")]
        [SerializeField, Min(0)] private float m_moveSpeedMinZoom;
        [SerializeField, Min(0)] private float m_moveSpeedMaxZoom;

        private Vector2 m_moveInput;

        [Header("Rotation")]
        [SerializeField] private float m_rotationSpeed;

        private float m_currentRotation;
        private float m_rotateInput;

        private void Awake()
        {
            m_swivel = transform.GetChild(0);
            m_stick = m_swivel.GetChild(0);

            AdjustZoom(0);
        }

        private void Update()
        {
            if (m_moveInput.sqrMagnitude != 0)
            {
                AdjustPosition();
            }

            if (m_rotateInput != 0)
            {
                AdjustRotation();
            }
        }

        #region Zooming
        public void Zoom(InputAction.CallbackContext context)
        {
            AdjustZoom(context.ReadValue<float>() / m_zoomModifier);
        }

        private void AdjustZoom(float delta)
        {
            m_currentZoom = Mathf.Clamp01(m_currentZoom + delta);

            float distance = Mathf.Lerp(m_stickMinZoom, m_stickMaxZoom, m_currentZoom);
            m_stick.localPosition = new Vector3(0, 0, distance);

            float angle = Mathf.Lerp(m_swivelMinZoom, m_swivelMaxZoom, m_currentZoom);
            m_swivel.localRotation = Quaternion.Euler(angle, 0, 0);
        }
        #endregion

        #region Movement
        public void Move(InputAction.CallbackContext context)
        {
            Vector2 value = context.ReadValue<Vector2>();
            m_moveInput = value;
        }

        private void AdjustPosition()
        {
            Vector3 direction = transform.localRotation * new Vector3(m_moveInput.x, 0, m_moveInput.y).normalized;
            float distance = Mathf.Lerp(m_moveSpeedMinZoom, m_moveSpeedMaxZoom, m_currentZoom) * Time.deltaTime;
            
            Vector3 position = transform.localPosition;
            position += direction * distance;
            transform.localPosition = ClampPosition(position);
        }

        private Vector3 ClampPosition(Vector3 position)
        {
            float xMax = (m_map.chunkCountX * HexMetrics.chunkSizeX - 0.5f) * 2f * HexMetrics.innerRadius;
            position.x = Mathf.Clamp(position.x, 0, xMax);

            float zMax = (m_map.chunkCountZ * HexMetrics.chunkSizeZ - 1) * 1.5f * HexMetrics.outerRadius;
            position.z = Mathf.Clamp(position.z, 0, zMax);

            return position;
        }
        #endregion

        #region Rotation
        public void Rotate(InputAction.CallbackContext context)
        {
            m_rotateInput = context.ReadValue<float>();
            print(m_rotateInput);
        }

        private void AdjustRotation()
        {
            m_currentRotation += m_rotateInput * m_rotationSpeed * Time.deltaTime;

            m_currentRotation = WrapFloat(m_currentRotation, 0, 360);

            transform.localRotation = Quaternion.Euler(0, m_currentRotation, 0);
        }

        private static float WrapFloat(float value, float min, float max)
        {
            if (value < min)
                value += max;
            else if (value >= max)
                value -= max;

            return value;
        }
        #endregion
    }
}
