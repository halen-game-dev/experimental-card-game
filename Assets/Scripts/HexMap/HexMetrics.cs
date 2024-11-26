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
        public const float outerRadius = 10f;

        public const float innerRadius = outerRadius * 0.866025404f;

        public static Vector3[] corners = {
        new(0f, 0f, outerRadius),
        new(innerRadius, 0f, 0.5f * outerRadius),
        new(innerRadius, 0f, -0.5f * outerRadius),
        new(0f, 0f, -outerRadius),
        new(-innerRadius, 0f, -0.5f * outerRadius),
        new(-innerRadius, 0f, 0.5f * outerRadius),
        new(0f, 0f, outerRadius)
        };
    }
    
}
