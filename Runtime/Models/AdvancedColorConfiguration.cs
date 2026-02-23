using UnityEngine;

namespace net.puk06.TexStackEditor.Models
{
    [System.Serializable]
    public class AdvancedColorConfiguration
    {
        public bool IsEnabled = false;
        
        [Range(0f, 360f)] public float Hue;
        [Range(0f, 100.0f)] public float Saturation;
        [Range(0f, 100.0f)] public float Value;
        public float Brightness = 1.0f;
        public float Contrast = 1.0f;
        public float Gamma = 1.0f;
        public float Exposure = 0.0f;
        [Range(0f, 1f)] public float Transparency = 0.0f;
    }
}
