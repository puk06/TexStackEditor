#nullable enable
using UnityEngine;

namespace net.puk06.TexStackEditor.Models
{
    [System.Serializable]
    public class LayerNodeConfiguration
    {
        public bool IsVisible = true;
        [Range(0f, 1f)] public float Opacity = 1f;
    }
}
