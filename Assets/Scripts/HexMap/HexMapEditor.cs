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
        private enum OptionalToggle { Ignore, Yes, No }
        
        [SerializeField] private HexMap m_hexMap;
        [SerializeField] private Color[] m_colours;

        private Color m_activeColour;
        private int m_activeElevation;
        private int m_brushSize;
        private OptionalToggle m_riverMode;

        private bool m_applyColour = false;
        private bool m_applyElevation = true;

        private bool m_mouseHeld = false;

        private bool m_isDrag = false;
        private HexDirection m_dragDirection;
        private HexCell m_previousDragCell;

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
                    HexCell currentCell = m_hexMap.GetCell(hit.point);

                    // check if drag is valid
                    if (m_previousDragCell && m_previousDragCell != currentCell)
                        ValidateDrag(currentCell);
                    else
                        m_isDrag = false;

                    EditCells(currentCell);
                    m_previousDragCell = currentCell;
                }
            }
        }

        public void MouseLeftButton(InputAction.CallbackContext context)
        {
            if (context.started)
                m_mouseHeld = true;
            else if (context.canceled)
            {
                m_mouseHeld = false;
                //m_previousDragCell = null;
            }
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

                if (m_riverMode == OptionalToggle.No)
                    cell.RemoveRiver();
                else if (m_isDrag && m_riverMode == OptionalToggle.Yes)
                {
                    HexCell otherCell = cell.GetNeighbour(m_dragDirection.Opposite());
                    if (otherCell)
                        otherCell.SetOutgoingRiver(m_dragDirection);
                }
            }
        }

        private void ValidateDrag(HexCell currentCell)
        {
            for (m_dragDirection = HexDirection.NE; m_dragDirection <= HexDirection.NW; m_dragDirection++)
            {
                if (m_previousDragCell.GetNeighbour(m_dragDirection) == currentCell)
                {
                    m_isDrag = true;
                    return;
                }
            }
            m_isDrag = false;
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

        public void SetRiverMode(int mode)
        {
            m_riverMode = (OptionalToggle)mode;
        }
    }
}
