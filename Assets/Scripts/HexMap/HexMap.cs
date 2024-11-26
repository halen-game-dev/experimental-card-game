///<summary>
/// Author: Halen
/// 
/// Defines a grid of HexCells.
/// 
///</summary>

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

namespace CardGame.HexMap
{
    public class HexMap : MonoBehaviour
    {
        [SerializeField] private HexCell m_cellPrefab;

        [Header("Map Properties")]
        [SerializeField] private int m_width;
        [SerializeField] private int m_height;

        [Space]

        [SerializeField] private Color m_defaultColour = Color.white;
        [SerializeField] private Color m_touchedColour = Color.magenta;

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI m_cellLabelPrefab;

        private HexCell[] m_cells;
        private Canvas m_mapCanvas;
        private HexMesh m_hexMesh;

        private void Awake()
        {
            m_mapCanvas = GetComponentInChildren<Canvas>();
            m_hexMesh = GetComponentInChildren<HexMesh>();
            
            m_cells = new HexCell[m_height * m_width];

            for (int h = 0, i = 0; h < m_height; h++)
            {
                for (int w = 0; w < m_width; w++)
                {
                    CreateCell(w, h, i++);
                }
            }
        }

        private void Start()
        {
            m_hexMesh.Triangulate(m_cells);
        }

        public void CastForHexCell(InputAction.CallbackContext context)
        {
            Ray inputRay = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(inputRay, out RaycastHit hit))
            {
                TouchCell(hit.point);
            }
        }

        private void TouchCell(Vector3 position)
        {
            position = transform.InverseTransformPoint(position);
            HexCoordinates coordinates = HexCoordinates.FromPosition(position);
            int index = coordinates.X + coordinates.Z * m_width + coordinates.Z / 2;
            HexCell cell = m_cells[index];
            cell.colour = m_touchedColour;
            m_hexMesh.Triangulate(m_cells);
        }

        /// <summary>
        /// Create a cell at index (m_x, m_z). i is the index in the array.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="i"></param>
        private void CreateCell(int x, int z, int i)
        {
            Vector3 positon = new((x + z * 0.5f - z / 2) * HexMetrics.innerRadius * 2f, 0, z * HexMetrics.outerRadius * 1.5f);

            HexCell cell = m_cells[i] = Instantiate(m_cellPrefab);
            cell.transform.SetParent(transform, false);
            cell.transform.localPosition = positon;
            cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
            cell.colour = m_defaultColour;

            var label = Instantiate(m_cellLabelPrefab);
            label.rectTransform.SetParent(m_mapCanvas.transform, false);
            label.rectTransform.anchoredPosition = new(positon.x / 10f, positon.z / 10f);
            label.text = cell.coordinates.ToString();
        }
    }
}
