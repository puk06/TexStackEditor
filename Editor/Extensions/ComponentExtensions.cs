#nullable enable
using UnityEngine;

namespace net.puk06.TexStackEditor.Editor.Extension
{
    internal static class ComponentExtensions
    {
        public static bool IsActiveTSEComponent(this Component component)
        {
            if (!IsActiveComponent(component)) return false;

            if (component is TSELayerStack layerStack)
            {
                return layerStack.TargetTexture != null;
            }

            if (component is TSELayerNode layerNode)
            {
                return layerNode.LayerNodeConfiguration.IsVisible;
            }

            return false;
        }

        public static bool IsActiveComponent(this Component component)
        {
            return component.gameObject.activeInHierarchy && component.IsEditorOnly() == false;
        }

        public static bool IsEditorOnly(this Component component)
        {
            return component.CompareTag("EditorOnly");
        }
    }
}
