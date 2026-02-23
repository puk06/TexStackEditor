using UnityEngine;

namespace net.puk06.TexStackEditor.Editor.Extension
{
    internal static class GradientExtensions
    {
        internal static Texture2D ToTexture(this Gradient gradient, int width, int height = 1)
        {
            Texture2D texture = new(width, height, TextureFormat.RGBA32, false, true)
            {
                wrapMode = TextureWrapMode.Clamp
            };

            for (int i = 0; i < width; i++)
            {
                float value = i / (float)(width - 1);
                Color color = gradient.Evaluate(value); // ここではGamma値を返す
                texture.SetPixel(i, 0, color.linear); // LinearをTextureにはセットしてあげる
            }

            texture.Apply();

            return texture;
        }
    }
}
