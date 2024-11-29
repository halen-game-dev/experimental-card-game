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
        [SerializeField] private HexCell[] m_neighbours;

        public HexCoordinates coordinates;
        private Color m_colour;
        private int m_elevation = int.MinValue;

        public HexMapChunk chunk;
        public RectTransform labelRect;

        public Vector3 Position => transform.localPosition;
        public Color Colour
        {
            get { return m_colour; }
            set
            {
                if (m_colour == value)
                    return;

                m_colour = value;
                Refresh();
            }
        }

        public int Elevation
        {
            get { return m_elevation; }
            set
            {
                if (m_elevation == value)
                    return;

                m_elevation = value;
                Vector3 position = transform.localPosition;
                position.y = value * HexMetrics.elevationStep;
                position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
                transform.localPosition = position;

                Vector3 labelPosition = labelRect.localPosition;
                labelPosition.z = -position.y;
                labelRect.localPosition = labelPosition;

                // remove any rivers that now flow uphill
                if (m_hasOutgoingRiver && m_elevation < GetNeighbour(m_outgoingRiver).m_elevation)
                    RemoveOutgoingRiver();
                if (m_hasIncomingRiver && m_elevation > GetNeighbour(m_incomingRiver).m_elevation)
                    RemoveIncomingRiver();

                Refresh();
            }
        }
        public float StreamBedY => (m_elevation + HexMetrics.streamBedElevationOffset) * HexMetrics.elevationStep;

        // rivers
        private bool m_hasIncomingRiver;
        private bool m_hasOutgoingRiver;
        private HexDirection m_incomingRiver, m_outgoingRiver;

        public bool HasRiver => m_hasIncomingRiver || m_hasOutgoingRiver;
        public bool HasRiverBeginOrEnd => m_hasIncomingRiver != m_hasOutgoingRiver;
        public bool HasIncomingRiver => m_hasIncomingRiver;
        public bool HasOutgoingRiver => m_hasOutgoingRiver;
        public HexDirection IncomingRiver => m_incomingRiver;
        public HexDirection OutgoingRiver => m_outgoingRiver;


        private void Awake()
        {
            m_neighbours = new HexCell[6];
        }

        #region Initialisation
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
        #endregion

        #region Rivers
        public bool HasRiverThroughEdge(HexDirection direction)
        {
            return m_hasIncomingRiver && m_incomingRiver == direction ||
                m_hasOutgoingRiver && m_outgoingRiver == direction;
        }

        public void SetOutgoingRiver(HexDirection direction)
        {
            // if river already exists
            if (m_hasOutgoingRiver && m_outgoingRiver == direction)
                return;

            // stop river from flowing uphill
            HexCell neighbour = GetNeighbour(direction);
            if (!neighbour || m_elevation < neighbour.m_elevation)
                return;

            // remove old outgoing river and remove incoming river if it will overlap
            RemoveOutgoingRiver();
            if (m_hasIncomingRiver && m_incomingRiver == direction)
                RemoveIncomingRiver();

            // set the new outgoing river
            m_hasOutgoingRiver = true;
            m_outgoingRiver = direction;
            RefreshSelfOnly();

            // update cell river is connected to
            neighbour.RemoveIncomingRiver();
            neighbour.m_hasIncomingRiver = true;
            neighbour.m_incomingRiver = direction.Opposite();
            neighbour.RefreshSelfOnly();
        }

        public void RemoveRiver()
        {
            RemoveOutgoingRiver();
            RemoveOutgoingRiver();
        }

        public void RemoveOutgoingRiver()
        {
            if (!m_hasOutgoingRiver)
                return;
            m_hasOutgoingRiver = false;
            RefreshSelfOnly();

            HexCell neighbour = GetNeighbour(m_outgoingRiver);
            neighbour.m_hasIncomingRiver = false;
            neighbour.RefreshSelfOnly();
        }

        public void RemoveIncomingRiver()
        {
            if (!m_hasIncomingRiver)
                return;

            m_hasIncomingRiver = false;
            RefreshSelfOnly();

            HexCell neighbour = GetNeighbour(m_incomingRiver);
            neighbour.m_hasOutgoingRiver = false;
            neighbour.RefreshSelfOnly();
        }
        #endregion

        private void Refresh()
        {
            if (chunk)
            {
                chunk.Refresh();
                for (int n = 0; n < m_neighbours.Length; n++)
                {
                    HexCell neighbour = m_neighbours[n];
                    if (neighbour != null && neighbour.chunk != chunk)
                    {
                        neighbour.chunk.Refresh();
                    }
                }
            }
        }

        private void RefreshSelfOnly()
        {
            chunk.Refresh();
        }
    }
}
