#nullable enable
using UnityEngine;

namespace net.puk06.TexStackEditor.Models
{
    public enum LayerBlendMode
    {
        Normal,
        Multiply,
        Screen,
        Add,
        Subtract,
        Darken,
        Lighten,
        Difference,
        Overlay
    }

    [System.Serializable]
    public class LayerNodeConfiguration
    {
        public bool IsVisible = true;
        [Range(0f, 1f)] public float Opacity = 1f;
        public LayerBlendMode BlendMode = LayerBlendMode.Normal;
    }
}
