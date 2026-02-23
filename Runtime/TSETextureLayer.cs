#nullable enable
using UnityEngine;

namespace net.puk06.TexStackEditor
{
    [DisallowMultipleComponent]
    [System.Serializable]
    public class TSETextureLayer : TSELayerNode, VRC.SDKBase.IEditorOnly
    {
        public Texture2D? LayerTexture;
    }
}
