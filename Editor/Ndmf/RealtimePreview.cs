#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using nadena.dev.ndmf.preview;
using net.puk06.TexStackEditor.Editor.Extension;
using net.puk06.TexStackEditor.Editor.Models;
using net.puk06.TexStackEditor.Editor.Utils;
using net.puk06.TexStackEditor.Models;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace net.puk06.TexStackEditor.Editor.Ndmf
{
    internal class RealtimePreview : IRenderFilter
    {
        public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
        {
            ImmutableList<GameObject> avatarGameObjects = context.GetAvatarRoots();

            List<RenderGroup> targetRenderGroups = new();

            foreach (GameObject avatarGameObject in avatarGameObjects)
            {
                try
                {
                    TSETextureLayerStack[] parentComponents = context.GetComponentsInChildren<TSETextureLayerStack>(avatarGameObject, true);
                    if (parentComponents.Length == 0) continue;

                    TSELayerNode[] childNodeComponents = context.GetComponentsInChildren<TSELayerNode>(avatarGameObject, true);

                    IEnumerable<Texture2D?> targetTextures = parentComponents
                        .Select(c => context.Observe(c, c => c.TargetTexture))
                        .Distinct();

                    List<Renderer> targetRenderers = new();
                    foreach (Renderer avatarRenderer in avatarGameObject.GetComponentsInChildren<Renderer>().Where(r => r is MeshRenderer or SkinnedMeshRenderer))
                    {
                        Material[] materials = avatarRenderer.sharedMaterials;
                        if (materials == null) continue;

                        if (materials.Any(material => targetTextures.Any(targetTexture => targetTexture != null && material.HasTexture(targetTexture))))
                        {
                            targetRenderers.Add(avatarRenderer);
                        }
                    }

                    if (targetRenderers.Count > 0)
                    {
                        targetRenderGroups.Add(RenderGroup.For(targetRenderers).WithData((parentComponents, childNodeComponents)));
                    }
                }
                catch (Exception ex)
                {
                    LogUtils.LogError($"Failed to add renderer for avatar: '{avatarGameObject.name}'.\n{ex}");
                }
            }

            return targetRenderGroups.ToImmutableList();
        }

        public Task<IRenderFilterNode> Instantiate(RenderGroup group, IEnumerable<(Renderer, Renderer)> proxyPairs, ComputeContext context)
        {
            Dictionary<Texture2D, ExtendedRenderTexture>? processedTexturesDictionary = null;
            Dictionary<Renderer, Material?[]>? processedMaterialDictionary = new();

            try
            {
                (TSETextureLayerStack[] parentComponents, TSELayerNode[] childNodeComponents) = group.GetData<(TSETextureLayerStack[], TSELayerNode[])>();

                foreach (TSETextureLayerStack parentComponent in parentComponents) context.Observe(parentComponent);

                foreach (TSELayerNode childNodeComponent in childNodeComponents)
                {
                    context.Observe(childNodeComponent);
                    context.ActiveInHierarchy(childNodeComponent.gameObject);
                }

                IEnumerable<TSETextureLayerStack> enabledParentComponents = parentComponents.Where(i => context.ActiveInHierarchy(i.gameObject));
                processedTexturesDictionary = NdmfProcessor.ProcessAllComponents(enabledParentComponents);
                ObjectReferenceService.RegisterReplacements(processedTexturesDictionary);

                foreach ((Renderer original, Renderer proxy) in proxyPairs)
                {
                    processedMaterialDictionary[original] = proxy.sharedMaterials.Select(mat => NdmfProcessor.GetProcessedMaterial(mat, processedTexturesDictionary)).ToArray();
                }
                
                return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(processedMaterialDictionary, processedTexturesDictionary.Values));
            }
            catch (Exception ex)
            {
                LogUtils.LogError($"Failed to instantiate.\n{ex}");
                if (processedTexturesDictionary != null)
                {
                    foreach (ExtendedRenderTexture texture in processedTexturesDictionary.Values)
                        texture.Dispose();
                    processedTexturesDictionary.Clear();
                    processedTexturesDictionary = null;
                }

                if (processedMaterialDictionary != null)
                {
                    foreach (Material?[] materials in processedMaterialDictionary.Values)
                        foreach (Material? material in materials)
                            if (material != null) Object.DestroyImmediate(material);
                    processedMaterialDictionary.Clear();
                    processedMaterialDictionary = null;
                }
                return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(null, null));
            }
        }

        private class TextureReplacerNode : IRenderFilterNode, IDisposable
        {
            private IEnumerable<ExtendedRenderTexture>? _processedTextures;
            private Dictionary<Renderer, Material?[]>? _processedMaterialDictionary;

            public RenderAspects WhatChanged { get; private set; } = RenderAspects.Texture & RenderAspects.Material;

            public TextureReplacerNode(Dictionary<Renderer, Material?[]>? processedMaterialDictionary, IEnumerable<ExtendedRenderTexture>? processedTexturesDictionary)
            {
                _processedMaterialDictionary = processedMaterialDictionary;
                _processedTextures = processedTexturesDictionary;
            }

            public void OnFrame(Renderer original, Renderer proxy)
            {
                try
                {
                    if (_processedMaterialDictionary?.TryGetValue(original, out Material?[] processedMaterials) ?? false)
                    {
                        proxy.sharedMaterials = processedMaterials;
                    }
                }
                catch (Exception ex)
                {
                    LogUtils.LogError("Error occurred while rendering proxy.\n" + ex);
                }
            }

            public void Dispose()
            {
                if (_processedTextures != null)
                {
                    foreach (ExtendedRenderTexture texture in _processedTextures)
                        texture.Dispose();
                    _processedTextures = null;
                }

                if (_processedMaterialDictionary != null)
                {
                    foreach (Material?[] materials in _processedMaterialDictionary.Values)
                        foreach (Material? material in materials)
                            if (material != null) Object.DestroyImmediate(material);
                    _processedMaterialDictionary.Clear();
                    _processedMaterialDictionary = null;
                }
            }
        }
    }
}
