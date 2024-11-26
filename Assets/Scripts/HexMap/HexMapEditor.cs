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
            cell.colour = m_activeColour;
            cell.Elevation = m_activeElevation;
            m_hexMap.Refresh();
        }

        public void SelectColour(int index)
        {
            m_activeColour = m_colours[index];
        }

        public void SelectElevation(float elevation)
        {
            m_activeElevation = (int)elevation;
        }
    }
}
