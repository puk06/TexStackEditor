#nullable enable
using UnityEditor;
using UnityEngine;
using net.puk06.TexStackEditor.Models;

#if COLOR_CHANGER_FOR_UNITY
using net.puk06.ColorChanger;
#endif

namespace net.puk06.TexStackEditor.Editor
{
    internal static class TSEContextMenu
    {
        private const int Pri = 20;

        private const string MenuBasePath = "GameObject/TexStackEditor/"; // Base path for the context menu

        [MenuItem(MenuBasePath + "Layer Stack (Parent)", false, Pri)]
        private static void AddLayerStack() => AddGameObject<TSELayerStack>("[Parent] TSE LayerStack", Selection.activeGameObject);

        [MenuItem(MenuBasePath + "Add Layer/Layer Folder", false, Pri + 1)]
        private static void AddLayerFolder() => AddGameObject<TSELayerFolder>("[Layer Folder] TSE LayerFolder", Selection.activeGameObject);

        [MenuItem(MenuBasePath + "Add Layer/Texture Layer", false, Pri + 2)]
        private static void AddLayer() => AddGameObject<TSETextureLayer>("[Texture] TSE TextureLayer", Selection.activeGameObject);

        [MenuItem(MenuBasePath + "Add Layer/Base Texture Layer", false, Pri + 3)]
        private static void AddBaseLayer()
        {
            GameObject activeObject = Selection.activeGameObject;
            TSELayerStack? stackComponent = activeObject.GetComponentInParent<TSELayerStack>(true);
            if (stackComponent == null)
            {
                Debug.LogError("TSELayerStackコンポーネントが見つかりませんでした");
                return;
            }

            TSETextureLayer layerComponent = AddGameObject<TSETextureLayer>("[Base Texture] TSE Texture Layer", Selection.activeGameObject);
            layerComponent.LayerTexture = stackComponent.TargetTexture;
        }

#if COLOR_CHANGER_FOR_UNITY
        [MenuItem(MenuBasePath + "Color Changer For Unity/Import from Color Changer for Unity (CC4U)", false, Pri + 4)]
        private static void CreateFromColorChangerForUnity()
        {
            GameObject activeObject = Selection.activeGameObject;
            ColorChangerForUnity? colorChangerComponent = activeObject.GetComponent<ColorChangerForUnity>();
            if (colorChangerComponent == null)
            {
                Debug.LogError("ColorChangerForUnityコンポーネントが見つかりませんでした");
                return;
            }

            GameObject parentObject = colorChangerComponent.gameObject.transform.parent.gameObject;

            TSELayerStack stackComponent = AddGameObject<TSELayerStack>($"[Parent] TSE {colorChangerComponent.name}", parentObject);
            
            Texture2D? replacementTexture = null;
            Texture2D? targetTexture = null;

            #if COLOR_CHANGER_FOR_UNITY_V2
            replacementTexture = colorChangerComponent.ReplacementTexture;
            targetTexture = colorChangerComponent.TargetTexture;
            #endif

            #if COLOR_CHANGER_FOR_UNITY_V1
            replacementTexture = colorChangerComponent.replacementTexture;
            targetTexture = colorChangerComponent.targetTexture;
            #endif
    
            stackComponent.TargetTexture = targetTexture;

            TSELayerFolder layerFolderComponent = AddGameObject<TSELayerFolder>($"[Layer Folder (From CC4U)] {colorChangerComponent.name}", stackComponent.gameObject);

            if (replacementTexture != null)
            {
                TSETextureLayer component = AddGameObject<TSETextureLayer>($"[Replacement Texture] {replacementTexture.name}", layerFolderComponent.gameObject);
                component.LayerTexture = replacementTexture;
            }

            if (targetTexture != null)
            {
                TSETextureLayer component = AddGameObject<TSETextureLayer>($"[Base Texture] {targetTexture.name}", layerFolderComponent.gameObject);
                component.LayerTexture = targetTexture;
            }

            ConvertValueFromColorChangerForUnity(colorChangerComponent, layerFolderComponent);
        }

        [MenuItem(MenuBasePath + "Color Changer For Unity/Convert to Texture Layer", false, Pri + 5)]
        private static void ConvertToTextureLayer()
        {
            GameObject activeObject = Selection.activeGameObject;
            ColorChangerForUnity? colorChangerComponent = activeObject.GetComponent<ColorChangerForUnity>();
            if (colorChangerComponent == null)
            {
                Debug.LogError("ColorChangerForUnityコンポーネントが見つかりませんでした");
                return;
            }

            GameObject parentObject = colorChangerComponent.gameObject.transform.parent.gameObject;

            TSETextureLayer? targetTextureComponent = null;

            Texture2D? replacementTexture = null;
            Texture2D? targetTexture = null;

            #if COLOR_CHANGER_FOR_UNITY_V2
            replacementTexture = colorChangerComponent.ReplacementTexture;
            targetTexture = colorChangerComponent.TargetTexture;
            #endif

            #if COLOR_CHANGER_FOR_UNITY_V1
            replacementTexture = colorChangerComponent.replacementTexture;
            targetTexture = colorChangerComponent.targetTexture;
            #endif
    
            if (replacementTexture != null)
            {
                TSETextureLayer component = AddGameObject<TSETextureLayer>($"[Replacement Texture] {replacementTexture.name}", parentObject);
                component.LayerTexture = replacementTexture;
                targetTextureComponent = component;
            }

            if (targetTexture != null)
            {
                TSETextureLayer component = AddGameObject<TSETextureLayer>($"[Base Texture] {targetTexture.name}", parentObject);
                component.LayerTexture = targetTexture;
                if (targetTextureComponent == null) targetTextureComponent = component;
            }
            
            if (targetTextureComponent != null) ConvertValueFromColorChangerForUnity(colorChangerComponent, targetTextureComponent);
        }

        private static void ConvertValueFromColorChangerForUnity(ColorChangerForUnity component, TSELayerNode node)
        {
            #if COLOR_CHANGER_FOR_UNITY_V2
            if (component.MaskTexture != null)
            {
                node.MaskConfiguration.IsEnabled = true;
                node.MaskConfiguration.MaskTexture = component.MaskTexture;
                node.MaskConfiguration.MaskSelectionType = (MaskSelectionType)component.ImageMaskSelectionType;
                node.MaskConfiguration.MaskBlendSettings.IsBackgroundTransparent = false;
            }

            node.ColorAdjustmentConfiguration.IsEnabled = true;

            node.ColorAdjustmentConfiguration.Mode = (ColorAdjustmentMode)component.BalanceModeConfiguration.ModeVersion;

            node.ColorAdjustmentConfiguration.NormalColorSettings.SourceColor = component.SourceColor;
            node.ColorAdjustmentConfiguration.NormalColorSettings.TargetColor = component.TargetColor;

            node.ColorAdjustmentConfiguration.V1ColorSettings.Weight = component.BalanceModeConfiguration.V1Weight;
            node.ColorAdjustmentConfiguration.V1ColorSettings.Minimum = component.BalanceModeConfiguration.V1MinimumValue;

            node.ColorAdjustmentConfiguration.V2ColorSettings.Weight = component.BalanceModeConfiguration.V2Weight;
            node.ColorAdjustmentConfiguration.V2ColorSettings.Radius = component.BalanceModeConfiguration.V2Radius;
            node.ColorAdjustmentConfiguration.V2ColorSettings.Minimum = component.BalanceModeConfiguration.V2MinimumValue;
            node.ColorAdjustmentConfiguration.V2ColorSettings.IncludeOutside = component.BalanceModeConfiguration.V2IncludeOutside;

            node.ColorAdjustmentConfiguration.V3ColorSettings.Gradient = component.BalanceModeConfiguration.V3Gradient;
            node.ColorAdjustmentConfiguration.V3ColorSettings.Resolution = component.BalanceModeConfiguration.V3GradientResolution;

            node.AdvancedColorConfiguration.IsEnabled = component.AdvancedColorConfiguration.IsEnabled;
            node.AdvancedColorConfiguration.Hue = component.AdvancedColorConfiguration.Hue;
            node.AdvancedColorConfiguration.Saturation = component.AdvancedColorConfiguration.Saturation;
            node.AdvancedColorConfiguration.Value = component.AdvancedColorConfiguration.Value;
            node.AdvancedColorConfiguration.Brightness = component.AdvancedColorConfiguration.Brightness;
            node.AdvancedColorConfiguration.Contrast = component.AdvancedColorConfiguration.Contrast;
            node.AdvancedColorConfiguration.Gamma = component.AdvancedColorConfiguration.Gamma;
            node.AdvancedColorConfiguration.Exposure = component.AdvancedColorConfiguration.Exposure;
            node.AdvancedColorConfiguration.Transparency = component.AdvancedColorConfiguration.Transparency;
            #endif

            #if COLOR_CHANGER_FOR_UNITY_V1
            if (colorChangerComponent.maskTexture != null)
            {
                layerFolderComponent.MaskConfiguration.IsEnabled = true;
                layerFolderComponent.MaskConfiguration.MaskTexture = colorChangerComponent.maskTexture;
                layerFolderComponent.MaskConfiguration.MaskSelectionType = (MaskSelectionType)colorChangerComponent.imageMaskSelectionType;
                layerFolderComponent.MaskConfiguration.MaskBlendSettings.IsBackgroundTransparent = false;
            }

            layerFolderComponent.ColorAdjustmentConfiguration.IsEnabled = true;

            layerFolderComponent.ColorAdjustmentConfiguration.Mode = (ColorAdjustmentMode)colorChangerComponent.BalanceModeConfiguration.ModeVersion;

            layerFolderComponent.ColorAdjustmentConfiguration.NormalColorSettings.SourceColor = colorChangerComponent.previousColor;
            layerFolderComponent.ColorAdjustmentConfiguration.NormalColorSettings.TargetColor = colorChangerComponent.newColor;

            layerFolderComponent.ColorAdjustmentConfiguration.V1ColorSettings.Weight = colorChangerComponent.balanceModeConfiguration.V1Weight;
            layerFolderComponent.ColorAdjustmentConfiguration.V1ColorSettings.Minimum = colorChangerComponent.balanceModeConfiguration.V1MinimumValue;

            layerFolderComponent.ColorAdjustmentConfiguration.V2ColorSettings.Weight = colorChangerComponent.balanceModeConfiguration.V2Weight;
            layerFolderComponent.ColorAdjustmentConfiguration.V2ColorSettings.Radius = colorChangerComponent.balanceModeConfiguration.V2Radius;
            layerFolderComponent.ColorAdjustmentConfiguration.V2ColorSettings.Minimum = colorChangerComponent.balanceModeConfiguration.V2MinimumValue;
            layerFolderComponent.ColorAdjustmentConfiguration.V2ColorSettings.IncludeOutside = colorChangerComponent.balanceModeConfiguration.V2IncludeOutside;

            layerFolderComponent.ColorAdjustmentConfiguration.V3ColorSettings.Gradient = colorChangerComponent.balanceModeConfiguration.V3GradientColor;
            layerFolderComponent.ColorAdjustmentConfiguration.V3ColorSettings.Resolution = colorChangerComponent.balanceModeConfiguration.V3GradientBuildResolution;

            layerFolderComponent.AdvancedColorConfiguration.IsEnabled = colorChangerComponent.advancedColorConfiguration.Enabled;
            layerFolderComponent.AdvancedColorConfiguration.Brightness = colorChangerComponent.advancedColorConfiguration.Brightness;
            layerFolderComponent.AdvancedColorConfiguration.Contrast = colorChangerComponent.advancedColorConfiguration.Contrast;
            layerFolderComponent.AdvancedColorConfiguration.Gamma = colorChangerComponent.advancedColorConfiguration.Gamma;
            layerFolderComponent.AdvancedColorConfiguration.Exposure = colorChangerComponent.advancedColorConfiguration.Exposure;
            layerFolderComponent.AdvancedColorConfiguration.Transparency = colorChangerComponent.advancedColorConfiguration.Transparency;
            #endif
        }
#endif

        private static T AddGameObject<T>(string objectName, GameObject? parentObject = null)
            where T: Component
        {
            GameObject tseObject = new(objectName);
            if (parentObject != null) tseObject.transform.SetParent(parentObject.transform);

            T component = Undo.AddComponent<T>(tseObject);
            Undo.RegisterCreatedObjectUndo(tseObject , "Create TSE Object");

            PingObject(tseObject);

            return component;
        }
        
        private static void PingObject(GameObject gameObject)
        {
            if (gameObject == null) return;

            Selection.activeGameObject = gameObject;
            EditorGUIUtility.PingObject(gameObject);
        }
    }
}
