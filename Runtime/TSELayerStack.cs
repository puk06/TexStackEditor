#nullable enable
using UnityEngine;

namespace net.puk06.TexStackEditor
{
    [DisallowMultipleComponent]
    [System.Serializable]
    public class TSELayerStack : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        public Texture2D? TargetTexture;
    }
}
