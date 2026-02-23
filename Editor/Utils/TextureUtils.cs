using UnityEngine;

namespace net.puk06.TexStackEditor.Editor.Utils
{
    internal static class TextureUtils
    {
        internal static void ApplyStreamingMipmaps(Texture2D texture)
        {
            UnityEditor.SerializedObject textureObject = new(texture);
            UnityEditor.SerializedProperty streamingMipmapsProperty = textureObject.FindProperty("m_StreamingMipmaps");
            if (streamingMipmapsProperty != null) streamingMipmapsProperty.boolValue = true;
            textureObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
