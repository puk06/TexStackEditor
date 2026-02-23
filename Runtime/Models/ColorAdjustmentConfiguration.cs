namespace net.puk06.TexStackEditor.Models
{
    [System.Serializable]
    public class ColorAdjustmentConfiguration
    {
        public bool IsEnabled = false;

        public ColorAdjustmentMode Mode = ColorAdjustmentMode.Normal;

        public NormalColorSettings NormalColorSettings = new();
        public V1ColorSettings V1ColorSettings = new();
        public V2ColorSettings V2ColorSettings = new();
        public V3ColorSettings V3ColorSettings = new();
    }
}
