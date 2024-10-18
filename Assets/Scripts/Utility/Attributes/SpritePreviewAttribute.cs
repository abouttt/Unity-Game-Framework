using UnityEngine;

public class SpritePreviewAttribute : PropertyAttribute
{
    public float Size;

    public SpritePreviewAttribute(float size = 50f)
    {
        Size = size;
    }
}
