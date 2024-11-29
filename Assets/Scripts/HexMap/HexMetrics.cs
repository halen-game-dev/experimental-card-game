///<summary>
/// Author: Halen
/// 
/// Defines constants for the HexMap.
/// 
///</summary>

using UnityEngine;

namespace CardGame.HexMap
{
    public static class HexMetrics
    {
        // hex cell radius
        public const float outerRadius = 10f;
        public const float innerRadius = outerRadius * 0.866025404f;

        // hex cell Colour blending
        public const float solidFactor = 0.8f;
        public const float blendFactor = 1f - solidFactor;

        // hex cell elevation
        public const float elevationStep = 3f;
        public const int terracesPerSlope = 2;
        public const int terraceSteps = terracesPerSlope * 2 + 1;
        public const float horizontalTerraceStepSize = 1f / terraceSteps;
        public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

        // mesh perturbing
        public static Texture2D noiseSource;
        public const float noiseScale = 0.003f;
        public const float cellPerturbStrength = 4f;
        public const float elevationPerturbStrength = 1.5f;

        // mesh grid chunks
        public const int chunkSizeX = 5, chunkSizeZ = 5;

        // rivers
        public const float streamBedElevationOffset = -1f;

        // hex cell corner positions
        private static Vector3[] corners = {
        new(0f, 0f, outerRadius),
        new(innerRadius, 0f, 0.5f * outerRadius),
        new(innerRadius, 0f, -0.5f * outerRadius),
        new(0f, 0f, -outerRadius),
        new(-innerRadius, 0f, -0.5f * outerRadius),
        new(-innerRadius, 0f, 0.5f * outerRadius),
        new(0f, 0f, outerRadius)
        };

        public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
        {
            // check if flat
            if (elevation1 == elevation2)
                return HexEdgeType.Flat;

            // check if slope
            int delta = elevation2 - elevation1;
            if (Mathf.Abs(delta) == 1)
                return HexEdgeType.Slope;

            // otherwise, cliff
            return HexEdgeType.Cliff;
        }

        #region MeshTriangultionMethods
        public static Vector3 GetFirstCorner(HexDirection direction)
        {
            return corners[(int)direction];
        }

        public static Vector3 GetSecondCorner(HexDirection direction)
        {
            return corners[(int)direction + 1];
        }

        public static Vector3 GetFirstSolidCorner(HexDirection direction)
        {
            return corners[(int)direction] * solidFactor;
        }

        public static Vector3 GetSecondSolidCorner(HexDirection direction)
        {
            return corners[(int)direction + 1] * solidFactor;
        }

        public static Vector3 GetBridge(HexDirection direction)
        {
            return (corners[(int)direction] + corners[(int)direction + 1]) * blendFactor;
        }

        public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
        {
            float h = step * horizontalTerraceStepSize;
            a.x += (b.x - a.x) * h;
            a.z += (b.z - a.z) * h;

            float v = ((step + 1) / 2) * verticalTerraceStepSize;
            a.y += (b.y - a.y) * v;

            return a;
        }

        public static Color TerraceLerp(Color a, Color b, int step)
        {
            float h = step * horizontalTerraceStepSize;
            return Color.Lerp(a, b, h);
        }
        #endregion

        public static Vector4 SampleNoise(Vector3 position)
        {
            return noiseSource.GetPixelBilinear(
                position.x * noiseScale,
                position.z * noiseScale
            );
        }
    }
    
}
