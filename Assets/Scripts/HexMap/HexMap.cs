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
        [SerializeField] private HexMapChunk m_chunkPrefab;
        [SerializeField] private HexCell m_cellPrefab;

        [Header("Map Properties")]
        [SerializeField] private int m_chunkCountX = 4;
        [SerializeField] private int m_chunkCountZ = 3;

        public int chunkCountX => m_chunkCountX;
        public int chunkCountZ => m_chunkCountZ;

        private int m_cellCountX, m_cellCountZ;

        [Space]

        [SerializeField] private Color m_defaultColour = Color.white;

        [Header("Cell Labels")]
        [SerializeField] private TextMeshProUGUI m_cellLabelPrefab;

        [Header("Irregularity")]
        public Texture2D noiseSource;

        // objects
        private HexMapChunk[] m_chunks;
        private HexCell[] m_cells;

        private void OnEnable()
        {
            HexMetrics.noiseSource = noiseSource;
        }

        private void Awake()
        {
            HexMetrics.noiseSource = noiseSource;

            m_cellCountX = m_chunkCountX * HexMetrics.chunkSizeX;
            m_cellCountZ = m_chunkCountZ * HexMetrics.chunkSizeZ;

            CreateChunks();
            CreateCells();
        }

        public HexCell GetCell(Vector3 position)
        {
            position = transform.InverseTransformPoint(position);
            HexCoordinates coordinates = HexCoordinates.FromPosition(position);
            int index = coordinates.X + coordinates.Z * m_cellCountX + coordinates.Z / 2;
            return m_cells[index];
        }

        private void CreateChunks()
        {
            m_chunks = new HexMapChunk[m_chunkCountX * m_chunkCountZ];

            for (int z = 0, i = 0; z < m_chunkCountZ; z++)
            {
                for (int x = 0; x < m_chunkCountX; x++)
                {
                    HexMapChunk chunk = m_chunks[i++] = Instantiate(m_chunkPrefab);
                    chunk.transform.SetParent(transform);
                }
            }
        }

        private void CreateCells()
        {
            m_cells = new HexCell[m_cellCountZ * m_cellCountX];

            for (int h = 0, i = 0; h < m_cellCountZ; h++)
            {
                for (int w = 0; w < m_cellCountX; w++)
                {
                    CreateCell(w, h, i++);
                }
            }
        }

        private void CreateCell(int x, int z, int i)
        {
            Vector3 positon = new((x + z * 0.5f - z / 2) * HexMetrics.innerRadius * 2f, 0, z * HexMetrics.outerRadius * 1.5f);

            HexCell cell = m_cells[i] = Instantiate(m_cellPrefab);
            cell.transform.localPosition = positon;
            cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
            cell.Colour = m_defaultColour;

            if (x > 0)
            {
                cell.SetNeighbour(HexDirection.W, m_cells[i - 1]);
            }
            if (z > 0)
            {
                if ((z & 1) == 0)
                {
                    cell.SetNeighbour(HexDirection.SE, m_cells[i - m_cellCountX]);
                    if (x > 0)
                    {
                        cell.SetNeighbour(HexDirection.SW, m_cells[i - m_cellCountX - 1]);
                    }
                }
                else
                {
                    cell.SetNeighbour(HexDirection.SW, m_cells[i - m_cellCountX]);
                    if (x < m_cellCountX - 1)
                    {
                        cell.SetNeighbour(HexDirection.SE, m_cells[i - m_cellCountX + 1]);
                    }
                }
            }

            var label = Instantiate(m_cellLabelPrefab);
            label.rectTransform.anchoredPosition = new(positon.x, positon.z);
            label.text = cell.coordinates.ToString();
            cell.labelRect = label.rectTransform;

            cell.Elevation = 0;

            AddCellToChunk(x, z, cell);
        }

        private void AddCellToChunk(int x, int z, HexCell cell)
        {
            int chunkX = x / HexMetrics.chunkSizeX;
            int chunkZ = z / HexMetrics.chunkSizeZ;
            HexMapChunk chunk = m_chunks[chunkX + chunkZ * m_chunkCountX];

            int localX = x - chunkX * HexMetrics.chunkSizeX;
            int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
            chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
        }
    }
}
