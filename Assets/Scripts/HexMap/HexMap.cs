///<summary>
/// Author: Halen
/// 
/// Defines a grid of HexCells.
/// 
///</summary>

using UnityEngine;
using TMPro;

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

        [Header("Cell Labels")]
        [SerializeField] private TextMeshProUGUI m_cellLabelPrefab;

        [Header("Irregularity")]
        public Texture2D noiseSource;

        private HexCell[] m_cells;
        private Canvas m_mapCanvas;
        private HexMesh m_hexMesh;

        private void OnEnable()
        {
            HexMetrics.noiseSource = noiseSource;
        }

        private void Awake()
        {
            HexMetrics.noiseSource = noiseSource;
            
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

        public void Refresh()
        {
            m_hexMesh.Triangulate(m_cells);
        }

        public HexCell GetCell(Vector3 position)
        {
            position = transform.InverseTransformPoint(position);
            HexCoordinates coordinates = HexCoordinates.FromPosition(position);
            int index = coordinates.X + coordinates.Z * m_width + coordinates.Z / 2;
            return m_cells[index];
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

            if (x > 0)
            {
                cell.SetNeighbour(HexDirection.W, m_cells[i - 1]);
            }
            if (z > 0)
            {
                if ((z & 1) == 0)
                {
                    cell.SetNeighbour(HexDirection.SE, m_cells[i - m_width]);
                    if (x > 0)
                    {
                        cell.SetNeighbour(HexDirection.SW, m_cells[i - m_width - 1]);
                    }
                }
                else
                {
                    cell.SetNeighbour(HexDirection.SW, m_cells[i - m_width]);
                    if (x < m_width - 1)
                    {
                        cell.SetNeighbour(HexDirection.SE, m_cells[i - m_width + 1]);
                    }
                }
            }

            var label = Instantiate(m_cellLabelPrefab);
            label.rectTransform.SetParent(m_mapCanvas.transform, false);
            label.rectTransform.anchoredPosition = new(positon.x, positon.z);
            label.text = cell.coordinates.ToString();
            cell.labelRect = label.rectTransform;

            cell.Elevation = 0;
        }
    }
}
