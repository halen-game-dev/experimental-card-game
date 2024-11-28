///<summary>
/// Author: Halen
/// 
/// Defines a Hex Cell.
/// 
///</summary>

using UnityEngine;

namespace CardGame.HexMap
{
    public class HexCell : MonoBehaviour
    {
        public HexCoordinates coordinates;
        public Color colour;
        private int m_elevation;

        public RectTransform labelRect;

        public Vector3 Position => transform.localPosition;

        public int Elevation
        {
            get { return m_elevation; }
            set
            {
                m_elevation = value;
                Vector3 position = transform.localPosition;
                position.y = value * HexMetrics.elevationStep;
                position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
                transform.localPosition = position;

                Vector3 labelPosition = labelRect.localPosition;
                labelPosition.z = -position.y;
                labelRect.localPosition = labelPosition;
            }
        }

        [SerializeField] private HexCell[] m_neighbours;

        private void Awake()
        {
            m_neighbours = new HexCell[6];
        }

        public HexCell GetNeighbour(HexDirection direction)
        {
            return m_neighbours[(int)direction];
        }

        public void SetNeighbour(HexDirection direction, HexCell cell)
        {
            m_neighbours[(int)direction] = cell;
            cell.m_neighbours[(int)direction.Opposite()] = this;
        }

        public HexEdgeType GetEdgeType(HexDirection direction)
        {
            return HexMetrics.GetEdgeType(m_elevation, m_neighbours[(int)direction].m_elevation);
        }

        public HexEdgeType GetEdgeType(HexCell otherCell)
        {
            return HexMetrics.GetEdgeType(m_elevation, otherCell.m_elevation);
        }
    }
}
