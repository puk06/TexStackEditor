using UnityEngine;

namespace net.puk06.TexStackEditor.Editor.Utils
{
    internal static class TextureUtils
    {
        internal static void ApplyStreamingMipmaps(Texture2D texture)
        {
            var textureObject = new UnityEditor.SerializedObject(texture);
            var streamingMipmapsProperty = textureObject.FindProperty("m_StreamingMipmaps");
            if (streamingMipmapsProperty != null) streamingMipmapsProperty.boolValue = true;
            textureObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
