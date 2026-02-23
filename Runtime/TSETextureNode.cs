#nullable enable
using UnityEngine;
using net.puk06.TexStackEditor.Models;

namespace net.puk06.TexStackEditor
{
    [DisallowMultipleComponent]
    public abstract class TSELayerNode : MonoBehaviour
    {
        public LayerNodeConfiguration LayerNodeConfiguration = new();
        public MaskConfiguration MaskConfiguration = new();

        public ColorAdjustmentConfiguration ColorAdjustmentConfiguration = new();
        public AdvancedColorConfiguration AdvancedColorConfiguration = new();
    }
}
