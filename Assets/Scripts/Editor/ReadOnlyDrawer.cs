using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute), true)]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var readOnlyAttribute = (ReadOnlyAttribute)attribute;

        if (!Application.isPlaying && readOnlyAttribute.RuntimeOnly)
        {
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }

        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
