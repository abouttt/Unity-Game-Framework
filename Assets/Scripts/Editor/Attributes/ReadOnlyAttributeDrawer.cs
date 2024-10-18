using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute), true)]
public class ReadOnlyAttributeDrawer : PropertyDrawer
{
    // 비활성화된 속성 필드 그리기
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = !Application.isPlaying && ((ReadOnlyAttribute)attribute).RuntimeOnly;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }

    // 일부 속성은 해당 콘텐츠보다 작게 축소되는 경향이 있으므로 필요
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
