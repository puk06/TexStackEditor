#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using nadena.dev.ndmf;
using net.puk06.TexStackEditor.Editor.Extension;
using net.puk06.TexStackEditor.Editor.Models;
using net.puk06.TexStackEditor.Editor.Services;
using UnityEngine;

namespace net.puk06.TexStackEditor.Editor.Ndmf
{
    internal class NdmfProcessor
    {
        internal static Dictionary<Texture2D, ExtendedRenderTexture> ProcessAllComponents(IEnumerable<TSELayerStack> components, Action<TSELayerStack>? onSuccess = null, Action<TSELayerStack>? onFailed = null)
        {
            var result = new Dictionary<Texture2D, ExtendedRenderTexture>();

            foreach (var component in components)
            {
                if (component.TargetTexture == null || result.ContainsKey(component.TargetTexture))
                {
                    onFailed?.Invoke(component);
                    continue;
                }

                var processedRenderTexture = TextureBuilder.Build(component);
                if (processedRenderTexture == null)
                {
                    onFailed?.Invoke(component);
                    continue;
                }

                result.Add(component.TargetTexture, processedRenderTexture);
                onSuccess?.Invoke(component);
            }

            return result;
        }

        internal static Dictionary<Texture2D, Texture2D> ConvertToTexture2DDictionary(Dictionary<Texture2D, ExtendedRenderTexture> processedTexturesDictionary)
        {
            var result = new Dictionary<Texture2D, Texture2D>();

            foreach (var processedKpv in processedTexturesDictionary)
            {
                var convertedTexture = processedKpv.Value.ToTexture2D();
                processedKpv.Value.Dispose();

                result.Add(processedKpv.Key, convertedTexture);
            }

            return result;
        }

        internal static void ReplaceTexturesInRenderers(IEnumerable<Renderer> renderers, Dictionary<Texture2D, Texture2D> processedTexturesDictionary)
        {
            if (processedTexturesDictionary.Count == 0) return;

            Dictionary<Material, Material> materialMap = new();
            
            foreach (Renderer renderer in renderers)
            {
                Material?[] materials = renderer.sharedMaterials;
                bool changed = false;

                foreach (ref Material? material in materials.AsSpan())
                {
                    if (material == null) continue;
                    if (materialMap.TryGetValue(material, out Material? cloned))
                    {
                        material = cloned;
                        changed = true;
                    }
                    else
                    {
                        var newMaterial = GetProcessedMaterial(material, processedTexturesDictionary);
                        if (newMaterial == material) continue;

                        ObjectRegistry.RegisterReplacedObject(material, newMaterial!);
                        materialMap.Add(material, newMaterial!);
                        material = newMaterial;
                        changed = true;
                    }
                }

                if (changed) renderer.sharedMaterials = materials;
            }
        }

        [return:NotNullIfNotNull("material")]
        internal static Material? GetProcessedMaterial<T>(Material? material, Dictionary<Texture2D, T> processedTextures)
            where T : Texture
        {
            if (material == null) return null;

            Material? newMaterial = null;

            material.ForEachTexture((texture, propName) =>
            {
                if (texture is not Texture2D originalTexture || !processedTextures.TryGetValue(originalTexture, out T processedTexture)) return;
                if (newMaterial == null) newMaterial = UnityEngine.Object.Instantiate(material);
                newMaterial.SetTexture(propName, processedTexture);
            });

            if (newMaterial != null) return newMaterial;
            return material;
        }
    }
}
