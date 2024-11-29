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
        private int m_brushSize;

        private bool m_applyColour = false;
        private bool m_applyElevation = true;

        private bool m_mouseHeld = false;

        private void Update()
        {
            if (m_mouseHeld)
            {
                // if mouse is over UI, as EventSystem can only detect UI objects
                if (EventSystem.current.IsPointerOverGameObject())
                    return;

                Ray inputRay = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                if (Physics.Raycast(inputRay, out RaycastHit hit))
                {
                    EditCells(m_hexMap.GetCell(hit.point));
                }
            }
        }

        public void MouseLeftButton(InputAction.CallbackContext context)
        {
            if (context.started)
                m_mouseHeld = true;
            else if (context.canceled)
                m_mouseHeld = false;
        }

        private void EditCells(HexCell center)
        {
            int centerX = center.coordinates.X;
            int centerZ = center.coordinates.Z;

            // bottom half of brush
            for (int r = 0, z = centerZ - m_brushSize; z <= centerZ; z++, r++)
            {
                for (int x = centerX - r; x <= centerX + m_brushSize; x++)
                {
                    EditCell(m_hexMap.GetCell(new HexCoordinates(x, z)));
                }
            }

            // top half of brush
            for (int r = 0, z = centerZ + m_brushSize; z > centerZ; z--, r++)
            {
                for (int x = centerX - m_brushSize; x <= centerX + r; x++)
                {
                    EditCell(m_hexMap.GetCell(new HexCoordinates(x, z)));
                }
            }
        }

        private void EditCell(HexCell cell)
        {
            if (cell)
            {
                if (m_applyColour)
                    cell.Colour = m_activeColour;

                if (m_applyElevation)
                    cell.Elevation = m_activeElevation;
            }
        }

        public void ShowUI(bool value)
        {
            m_hexMap.ShowUI(value);
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

        public void SetBrushSize(float size)
        {
            m_brushSize = (int)size;
        }
    }
}
