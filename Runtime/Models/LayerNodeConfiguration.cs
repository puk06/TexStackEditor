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
        public Vector2Int CompositePosition = Vector2Int.zero;
        public Vector2 CompositeScale = Vector2.one;
        public float CompositeRotation = 0f;
    }
}
