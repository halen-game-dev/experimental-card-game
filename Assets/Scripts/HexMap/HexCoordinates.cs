///<summary>
/// Author: Halen
/// 
/// 
/// 
///</summary>

using UnityEngine;

namespace CardGame.HexMap
{
    [System.Serializable]
    public struct HexCoordinates
    {
        [SerializeField]
        private int m_x, m_z;
        
        public int X { get { return m_x; } }
        public int Y { get { return -X - Z; } }
        public int Z { get { return m_z; } }

        public HexCoordinates(int x, int z)
        {
            this.m_x = x;
            this.m_z = z;
        }

        public static HexCoordinates FromOffsetCoordinates(int x, int z)
        {
            return new HexCoordinates(x - z / 2, z);
        }

        public static HexCoordinates FromPosition(Vector3 position)
        {
            float x = position.x / (HexMetrics.innerRadius * 2f);
            float y = -x;

            float offset = position.z / (HexMetrics.outerRadius * 3f);
            x -= offset;
            y -= offset;

            int iX = Mathf.RoundToInt(x);
            int iY = Mathf.RoundToInt(y);
            int iZ = Mathf.RoundToInt(-x - y);

            // the sum of the coordinates should always be 0
            // otherwise the coordinates are inaccurate
            if (iX + iY + iZ != 0)
            {
                // get the rounding delta
                float dX = Mathf.Abs(x - iX);
                float dY = Mathf.Abs(y - iY);
                float dZ = Mathf.Abs(-x - y - iZ);

                // deconstruct the coordinate with the highest rounding delta
                // and reconstruct it from the other two coordinates
                if (dX > dY && dX > dZ)
                {
                    iX = -iY - iZ;
                }
                else if (dZ > dY)
                {
                    iZ = -iX - iY;
                }
            }

            return new HexCoordinates(iX, iZ);
        }

        public override string ToString()
        {
            return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
        }

        public string ToStringOnSeparateLines()
        {
            return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
        }
    }
    
}
