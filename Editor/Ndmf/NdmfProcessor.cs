#nullable enable
using System;
using System.Collections.Generic;
using nadena.dev.ndmf;
using net.puk06.TexStackEditor.Editor.Extension;
using net.puk06.TexStackEditor.Editor.Models;
using net.puk06.TexStackEditor.Editor.Services;
using UnityEngine;

namespace net.puk06.TexStackEditor.Editor.Ndmf
{
    internal class NdmfProcessor
    {
        internal static Dictionary<Texture2D, ExtendedRenderTexture> ProcessAllComponents(IEnumerable<TSETextureLayerStack> components, Action<TSETextureLayerStack>? onSuccess = null, Action<TSETextureLayerStack>? onFailed = null)
        {
            Dictionary<Texture2D, ExtendedRenderTexture> result = new();

            foreach (TSETextureLayerStack component in components)
            {
                if (component.TargetTexture == null || result.ContainsKey(component.TargetTexture))
                {
                    onFailed?.Invoke(component);
                    continue;
                }

                ExtendedRenderTexture? processedRenderTexture = TextureBuilder.Build(component);
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
            Dictionary<Texture2D, Texture2D> result = new();

            foreach (KeyValuePair<Texture2D, ExtendedRenderTexture> processedKpv in processedTexturesDictionary)
            {
                Texture2D convertedTexture = processedKpv.Value.ToTexture2D();
                processedKpv.Value.Dispose();

                result.Add(processedKpv.Key, convertedTexture);
            }

            return result;
        }

        internal static void ReplaceTexturesInRenderers(IEnumerable<Renderer> renderers, Dictionary<Texture2D, Texture2D> processedTexturesDictionary)
        {
            foreach (Renderer renderer in renderers)
            {
                Material?[] materials = renderer.sharedMaterials;
                Material?[] newMaterials = new Material[materials.Length];

                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == null) continue;
                    newMaterials[i] = GetProcessedMaterial(materials[i], processedTexturesDictionary);

                    ObjectRegistry.RegisterReplacedObject(materials[i], newMaterials[i]);
                }

                renderer.sharedMaterials = newMaterials;
            }
        }

        internal static Material? GetProcessedMaterial<T>(Material? material, Dictionary<Texture2D, T> processedTextures)
            where T : Texture
        {
            if (material == null) return null;

            Material newMaterial = UnityEngine.Object.Instantiate(material);

            newMaterial.ForEachTexture((texture, propName) =>
            {
                if (texture is not Texture2D originalTexture || !processedTextures.TryGetValue(originalTexture, out T processedTexture)) return;
                newMaterial.SetTexture(propName, processedTexture);
            });

            return newMaterial;
        }
    }
}
