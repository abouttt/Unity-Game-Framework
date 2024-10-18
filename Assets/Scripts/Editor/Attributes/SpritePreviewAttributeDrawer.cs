using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SpritePreviewAttribute))]
public class SpritePreviewAttributeDrawer : PropertyDrawer
{
    private readonly float Spacing = 5f; // 필드와 아이콘 간의 여백

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float previewSize = ((SpritePreviewAttribute)attribute).Size;
        float singleLineHeight = EditorGUIUtility.singleLineHeight;

        // 직렬화된 필드와 아이콘 미리보기를 위한 영역
        var fieldRect = new Rect
        {
            x = position.x,
            y = position.y,
            width = position.width,
            height = singleLineHeight
        };

        var spriteRect = new Rect
        {
            x = position.x + (position.width - previewSize) * 0.5f,
            y = position.y + singleLineHeight + Spacing,
            width = previewSize,
            height = previewSize,
        };

        // 직렬화된 필드를 렌더링
        EditorGUI.PropertyField(fieldRect, property, label);

        // 아이콘이 설정된 경우 미리보기를 렌더링
        if (property.objectReferenceValue is Sprite sprite)
        {
            GUI.DrawTexture(spriteRect, sprite.texture, ScaleMode.ScaleToFit, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight + ((SpritePreviewAttribute)attribute).Size + Spacing;
    }
}
