///<summary>
/// Author: Halen
///
///
///
///</summary>

using UnityEditor;
using UnityEngine;

namespace CardGame.HexMap
{
    [CustomPropertyDrawer(typeof(HexCoordinates))]
    public class HexCoordinatesDrawer : PropertyDrawer
    {
        // Override this method to make your own IMGUI based GUI for the property.
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            HexCoordinates coordinates = new(
                property.FindPropertyRelative("m_x").intValue,
                property.FindPropertyRelative("m_z").intValue
            );

            EditorGUI.LabelField(position, label.text, coordinates.ToString());
        }

        // Override this method to specify how tall the GUI for this field is in pixels.
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}
