///<summary>
/// Author: Halen
/// 
/// 
/// 
///</summary>

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace CardGame.HexMap
{
    public class HexMapEditor : MonoBehaviour
    {
        [SerializeField] private HexMap m_hexMap;
        [SerializeField] private Color[] m_colours;

        private Color m_activeColour;
        private int m_activeElevation;

        private bool m_applyColour = false;
        private bool m_applyElevation = true;

        private void Awake()
        {
            SelectColour(0);
        }

        public void CastForHexCell(InputAction.CallbackContext context)
        {
            // if mouse has not just been "clicked"
            if (!context.started)
                return;

            // if mouse is over UI, as EventSystem can only detect UI objects
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            Ray inputRay = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(inputRay, out RaycastHit hit))
            {
                EditCell(m_hexMap.GetCell(hit.point));
            }
        }

        private void EditCell(HexCell cell)
        {
            if (m_applyColour)
                cell.Colour = m_activeColour;

            if (m_applyElevation)
                cell.Elevation = m_activeElevation;
        }

        public void SelectColour(int index)
        {
            m_applyColour = index >= 0;
            if (m_applyColour)
                m_activeColour = m_colours[index];
        }

        public void SelectElevation(float elevation)
        {
            m_activeElevation = (int)elevation;
        }

        public void SetApplyElevation(bool value)
        {
            m_applyElevation = value;
        }
    }
}
