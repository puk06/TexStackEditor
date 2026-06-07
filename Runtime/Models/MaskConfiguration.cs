#nullable enable
using UnityEngine;

namespace net.puk06.TexStackEditor.Models
{
    [System.Serializable]
    public class MaskConfiguration : ISerializationCallbackReceiver
    {
        private const int CurrentSerializationVersion = 1;
        [SerializeField] private int SerializationVersion = 0;

        public bool IsEnabled = false;
        
        public Texture2D? MaskTexture;
        public MaskSelectionType MaskSelectionType;
        public MaskBlendSettings MaskBlendSettings = new();

        public void OnBeforeSerialize()
        {
            SerializationVersion = CurrentSerializationVersion;
        }

        public void OnAfterDeserialize()
        {
            if (SerializationVersion >= CurrentSerializationVersion) return;

            if (SerializationVersion == 0)
            {
                if ((int)MaskSelectionType == 6) MaskSelectionType = MaskSelectionType.Black;
                else if ((int)MaskSelectionType == 7) MaskSelectionType = MaskSelectionType.White;
            }

            SerializationVersion = CurrentSerializationVersion;
        }
    }
}
