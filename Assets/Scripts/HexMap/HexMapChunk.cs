///<summary>
/// Author: Halen
/// 
/// 
/// 
///</summary>

using UnityEngine;
using UnityEngine.UI;

namespace CardGame.HexMap
{
    public class HexMapChunk : MonoBehaviour
    {
        private HexCell[] m_cells;

        private HexMesh m_mesh;
        Canvas m_mapCanvas;

        private void Awake()
        {
            m_mapCanvas = GetComponentInChildren<Canvas>();
            m_mesh = GetComponentInChildren<HexMesh>();

            m_cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
        }

        private void Start()
        {
            m_mesh.Triangulate(m_cells);
        }

        public void AddCell(int index, HexCell cell)
        {
            m_cells[index] = cell;
            cell.transform.SetParent(transform, false);
            cell.labelRect.SetParent(m_mapCanvas.transform, false);
        }
    }
}
