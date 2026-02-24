#nullable enable
using System.Collections.Generic;
using net.puk06.TexStackEditor.Editor.Extension;
using net.puk06.TexStackEditor.Editor.Models;
using net.puk06.TexStackEditor.Editor.Utils;
using UnityEngine;

namespace net.puk06.TexStackEditor.Editor.Services
{
    internal static class TextureBuilder
    {
        internal static ExtendedRenderTexture? Build(TSELayerStack parent)
        {
            if (!parent.gameObject.activeInHierarchy) return null;
            ExtendedRenderTexture? result = null;

            foreach (TSELayerNode node in GetNodes(parent.transform))
            {
                using ExtendedRenderTexture? layerTexture = BuildNode(node);
                if (layerTexture == null) continue;

                using (layerTexture)
                {
                    if (result == null && !ExtendedRenderTexture.TryCreate(layerTexture.width, layerTexture.height, out result))
                        return null;

                    ExtendedRenderTexture? blendedTexture = Blend(result, layerTexture, node.LayerNodeConfiguration.Opacity);
                    if (blendedTexture == null)
                    {
                        result.Dispose();
                        return null;
                    }
                    result.Dispose();
                    result = blendedTexture;
                }
            }

            return result;
        }

        internal static ExtendedRenderTexture? BuildNode(TSELayerNode node)
        {
            if (node is TSELayerFolder folder) return Build(folder);
            else if (node is TSETextureLayer texture) return Build(texture);

            return null;
        }

        private static ExtendedRenderTexture? Build(TSELayerFolder folder)
        {
            if (!folder.gameObject.activeInHierarchy || !folder.LayerNodeConfiguration.IsVisible) return null;

            ExtendedRenderTexture? layerProcessedTexture = null;

            foreach (TSELayerNode node in GetNodes(folder.transform))
            {
                ExtendedRenderTexture? layerTexture = BuildNode(node);
                if (layerTexture == null) continue;

                using (layerTexture)
                {
                    if (layerProcessedTexture == null && !ExtendedRenderTexture.TryCreate(layerTexture.width, layerTexture.height, out layerProcessedTexture))
                        return null;

                    if (layerProcessedTexture == null) return null;

                    ExtendedRenderTexture? blendedTexture = Blend(layerProcessedTexture, layerTexture, node.LayerNodeConfiguration.Opacity);
                    if (blendedTexture == null)
                    {
                        layerProcessedTexture.Dispose();
                        return null;
                    }

                    layerProcessedTexture.Dispose();
                    layerProcessedTexture = blendedTexture;
                }
            }

            if (layerProcessedTexture == null) return null;

            using (layerProcessedTexture)
            {
                return Process(folder, layerProcessedTexture);
            }
        }

        private static ExtendedRenderTexture? Build(TSETextureLayer layer)
        {
            if (!layer.gameObject.activeInHierarchy || layer.LayerTexture == null || !layer.LayerNodeConfiguration.IsVisible) return null;

            if (!ExtendedRenderTexture.TryCreate(layer.LayerTexture.width, layer.LayerTexture.height, out ExtendedRenderTexture layerTexture))
                return null;
            layerTexture.Copy(layer.LayerTexture);

            using (layerTexture)
            {
                return Process(layer, layerTexture);
            }
        }

        private static IEnumerable<TSELayerNode> GetNodes(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var node = parent.GetChild(i).GetComponent<TSELayerNode>();
                if (node != null) yield return node;
            }
        }

        private static ExtendedRenderTexture? Blend(RenderTexture baseTexture, RenderTexture layerTexture, float layerOpacity)
        {
            ComputeShader? blenderShader = TSEShaderEngine.TextureBlenderComputeShader;
            if (blenderShader == null) TSEShaderEngine.LoadShaders();

            if (blenderShader == null)
            {
                LogUtils.LogError("Failed to load blender compute shader.");
                return null;
            }

            int kernel = blenderShader.FindKernel("CSMain");

            int canvasWidth = Mathf.Max(baseTexture.width, layerTexture.width);
            int canvasHeight = Mathf.Max(baseTexture.height, layerTexture.height);

            Vector2 baseScale = new((float)baseTexture.width / canvasWidth, (float)baseTexture.height / canvasHeight);
            Vector2 layerScale = new((float)layerTexture.width / canvasWidth, (float)layerTexture.height / canvasHeight);

            if (!ExtendedRenderTexture.TryCreate(canvasWidth, canvasHeight, out ExtendedRenderTexture targetTexture))
                return null;

            blenderShader.SetTexture(kernel, "_BaseTex", baseTexture);
            blenderShader.SetTexture(kernel, "_LayerTex", layerTexture);
            blenderShader.SetTexture(kernel, "_TargetTex", targetTexture);

            blenderShader.SetInts("_CanvasSize", new int[] { canvasWidth, canvasHeight });

            blenderShader.SetVector("_BaseTexScale", baseScale);
            blenderShader.SetVector("_LayerTexScale", layerScale);

            blenderShader.SetFloat("_LayerOpacity", layerOpacity);

            int threadGroupX = Mathf.CeilToInt(canvasWidth / 16.0f);
            int threadGroupY = Mathf.CeilToInt(canvasHeight / 16.0f);
            blenderShader.Dispatch(kernel, threadGroupX, threadGroupY, 1);

            return targetTexture;
        }
        private static ExtendedRenderTexture? Process(TSELayerNode node, RenderTexture sourceTexture)
        {
            ComputeShader? processorShader = TSEShaderEngine.TextureProcessorComputeShader;
            if (processorShader == null) TSEShaderEngine.LoadShaders();

            if (processorShader == null)
            {
                LogUtils.LogError("Failed to load processor compute shader.");
                return null;
            }

            if (!ExtendedRenderTexture.TryCreate(sourceTexture.width, sourceTexture.height, out ExtendedRenderTexture targetTexture))
                return null;

            static int convertColorValueToInt32(float colorValue) => Mathf.RoundToInt(colorValue * 255f);
            static int[] toInt32Array(Color color) => new int[4] { convertColorValueToInt32(color.r), convertColorValueToInt32(color.g), convertColorValueToInt32(color.b), convertColorValueToInt32(color.a) };

            int kernel = processorShader.FindKernel("CSMain");

            processorShader.SetTexture(kernel, "_SourceTex", sourceTexture);
            processorShader.SetTexture(kernel, "_TargetTex", targetTexture);

            processorShader.SetBool("_UseMask", node.MaskConfiguration.IsEnabled && node.MaskConfiguration.MaskTexture != null);

            processorShader.SetTexture(kernel, "_MaskTex", (node.MaskConfiguration.IsEnabled && node.MaskConfiguration.MaskTexture != null) ? node.MaskConfiguration.MaskTexture : DummyRenderTexture.Instance);
            
            Vector2 maskScale = node.MaskConfiguration.MaskTexture == null ? new() : new Vector2((float)node.MaskConfiguration.MaskTexture.width / sourceTexture.width, (float)node.MaskConfiguration.MaskTexture.height / sourceTexture.height);
            processorShader.SetVector("_MaskTexScale", maskScale);
            processorShader.SetInt("_MaskSelectionType", (int)node.MaskConfiguration.MaskSelectionType);
            processorShader.SetBool("_IsBackgroundTransparent", node.MaskConfiguration.MaskBlendSettings.IsBackgroundTransparent);

            processorShader.SetBool("_ColorAdjustmentConfigurationEnabled", node.ColorAdjustmentConfiguration.IsEnabled);

            processorShader.SetInt("_ColorMode", (int)node.ColorAdjustmentConfiguration.Mode);

            processorShader.SetInts("_NormalSourceColor", toInt32Array(node.ColorAdjustmentConfiguration.NormalColorSettings.SourceColor));
            processorShader.SetInts("_NormalTargetColor", toInt32Array(node.ColorAdjustmentConfiguration.NormalColorSettings.TargetColor));

            processorShader.SetFloat("_V1Weight", node.ColorAdjustmentConfiguration.V1ColorSettings.Weight);
            processorShader.SetFloat("_V1Minimum", node.ColorAdjustmentConfiguration.V1ColorSettings.Minimum);

            processorShader.SetFloat("_V2Weight", node.ColorAdjustmentConfiguration.V2ColorSettings.Weight);
            processorShader.SetFloat("_V2Radius", node.ColorAdjustmentConfiguration.V2ColorSettings.Radius);
            processorShader.SetFloat("_V2Minimum", node.ColorAdjustmentConfiguration.V2ColorSettings.Minimum);
            processorShader.SetBool("_V2IncludeOutside", node.ColorAdjustmentConfiguration.V2ColorSettings.IncludeOutside);

            Texture2D v3GradientTexture = node.ColorAdjustmentConfiguration.V3ColorSettings.Gradient.ToTexture(node.ColorAdjustmentConfiguration.V3ColorSettings.Resolution);
            processorShader.SetTexture(kernel, "_V3Gradient", v3GradientTexture);
            processorShader.SetInt("_V3GradientWidth", node.ColorAdjustmentConfiguration.V3ColorSettings.Resolution);

            processorShader.SetBool("_AdvancedColorConfigurationEnabled", node.AdvancedColorConfiguration.IsEnabled);
            processorShader.SetFloat("_Hue", node.AdvancedColorConfiguration.Hue / 360f);
            processorShader.SetFloat("_Saturation", node.AdvancedColorConfiguration.Saturation / 100f);
            processorShader.SetFloat("_Value", node.AdvancedColorConfiguration.Value / 100f);
            processorShader.SetFloat("_Brightness", node.AdvancedColorConfiguration.Brightness);
            processorShader.SetFloat("_Contrast", node.AdvancedColorConfiguration.Contrast);
            processorShader.SetFloat("_Gamma", node.AdvancedColorConfiguration.Gamma);
            processorShader.SetFloat("_Exposure", node.AdvancedColorConfiguration.Exposure);
            processorShader.SetFloat("_Transparency", node.AdvancedColorConfiguration.Transparency);

            int threadGroupX = Mathf.CeilToInt(sourceTexture.width / 16.0f);
            int threadGroupY = Mathf.CeilToInt(sourceTexture.height / 16.0f);
            processorShader.Dispatch(kernel, threadGroupX, threadGroupY, 1);

            Object.DestroyImmediate(v3GradientTexture);

            return targetTexture;
        }
    }
}
