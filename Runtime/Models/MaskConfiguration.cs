#nullable enable
using UnityEngine;

namespace net.puk06.TexStackEditor.Models
{
    [System.Serializable]
    public class MaskConfiguration
    {
        public bool IsEnabled = false;
        
        public Texture2D? MaskTexture;
        public MaskSelectionType MaskSelectionType;
        public MaskBlendSettings MaskBlendSettings = new();
    }
}
