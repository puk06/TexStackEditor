#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using nadena.dev.ndmf.preview;
using net.puk06.TexStackEditor.Editor.Extension;
using net.puk06.TexStackEditor.Editor.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace net.puk06.TexStackEditor.Editor.Ndmf
{
    internal class RealtimePreview : IRenderFilter
    {
        public ImmutableList<RenderGroup> GetTargetGroups(ComputeContext context)
        {
            var avatarGameObjects = context.GetAvatarRoots().Distinct();

            var targetRenderGroups = new List<RenderGroup>();

            foreach (var avatarGameObject in avatarGameObjects)
            {
                try
                {
                    var parentComponents = context.GetComponentsInChildren<TSELayerStack>(avatarGameObject, true);
                    if (parentComponents.Length == 0) continue;

                    foreach (var parentComponent in parentComponents)
                    {
                        context.Observe(parentComponent, c => c.TargetTexture, (a, b) => a == b);
                        context.ActiveInHierarchy(parentComponent.gameObject);
                        context.Observe(parentComponent.gameObject, go => go.tag);
                    }

                    context.GetComponentsInChildren<TSELayerNode>(avatarGameObject, true);

                    var targetTextures = parentComponents
                        .Select(c => c.TargetTexture)
                        .Distinct();

                    var targetRenderers = new List<Renderer>();
                    foreach (Renderer avatarRenderer in context.GetComponentsInChildren<Renderer>(avatarGameObject, true).Where(r => r is MeshRenderer or SkinnedMeshRenderer))
                    {
                        var materials = context.Observe(avatarRenderer, i => i.sharedMaterials, (a, b) => a != null && b != null && a.SequenceEqual(b));
                        if (materials == null) continue;

                        if (materials.Any(material => targetTextures.Any(targetTexture => targetTexture != null && material.HasTexture(targetTexture))))
                        {
                            targetRenderers.Add(avatarRenderer);
                        }
                    }

                    if (targetRenderers.Count > 0)
                    {
                        targetRenderGroups.Add(RenderGroup.For(targetRenderers).WithData(avatarGameObject));
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
            Dictionary<Texture2D, Texture2D>? processedTexturesDictionary = null;
            Dictionary<Renderer, Material?[]>? processedMaterialDictionary = new();
            Dictionary<Material, Material>? materialMap = null;

            try
            {
                var root = group.GetData<GameObject>();

                var parentComponents = root.GetComponentsInChildren<TSELayerStack>(true);
                if (parentComponents.Length == 0) return Task.FromResult<IRenderFilterNode>(new EmptyNode());

                var childNodeComponents = context.GetComponentsInChildren<TSELayerNode>(root, true);
                foreach (var childNodeComponent in childNodeComponents)
                {
                    context.Observe(childNodeComponent);
                    context.ActiveInHierarchy(childNodeComponent.gameObject);
                    context.Observe(childNodeComponent.gameObject, go => go.tag);
                }

                var processedTextures = NdmfProcessor.ProcessAllComponents(parentComponents);
                processedTexturesDictionary = NdmfProcessor.ConvertToTexture2DDictionary(processedTextures);
                ObjectReferenceService.RegisterReplacements(processedTexturesDictionary);

                materialMap = new();

                foreach ((Renderer original, Renderer proxy) in proxyPairs)
                {
                    Material?[] materials = proxy.sharedMaterials;
                    Material?[] newMaterials = (Material?[])materials.Clone();
                    bool changed = false;

                    for (int i = 0; i < materials.Length; i++)
                    {
                        var material = materials[i];
                        if (material == null) continue;

                        if (materialMap.TryGetValue(material, out var cached))
                        {
                            newMaterials[i] = cached;
                            changed = true;
                        }
                        else
                        {
                            var processed = NdmfProcessor.GetProcessedMaterial(materials[i], processedTexturesDictionary);
                            if (processed != material)
                            {
                                materialMap.Add(material, processed!);
                                newMaterials[i] = processed;
                                changed = true;
                            }
                        }
                    }

                    if (changed)
                        processedMaterialDictionary[original] = newMaterials;
                }

                return Task.FromResult<IRenderFilterNode>(new TextureReplacerNode(processedMaterialDictionary, processedTexturesDictionary.Values, materialMap.Values));
            }
            catch (Exception ex)
            {
                LogUtils.LogError($"Failed to instantiate.\n{ex}");
                if (processedTexturesDictionary != null)
                {
                    foreach (var texture in processedTexturesDictionary.Values)
                        Object.DestroyImmediate(texture);
                    processedTexturesDictionary.Clear();
                    processedTexturesDictionary = null;
                }

                if (processedMaterialDictionary != null)
                {
                    if (materialMap != null)
                    {
                        foreach (var material in materialMap.Values)
                            Object.DestroyImmediate(material);
                    }
                    processedMaterialDictionary.Clear();
                    processedMaterialDictionary = null;
                }

                return Task.FromResult<IRenderFilterNode>(new EmptyNode());
            }
        }

        private class TextureReplacerNode : IRenderFilterNode, IDisposable
        {
            private IEnumerable<Texture2D>? _processedTextures;
            private Dictionary<Renderer, Material?[]>? _processedMaterialDictionary;
            private IEnumerable<Material>? _createdMaterials;

            public RenderAspects WhatChanged { get; private set; } = RenderAspects.Texture | RenderAspects.Material;

            public TextureReplacerNode(Dictionary<Renderer, Material?[]>? processedMaterialDictionary, IEnumerable<Texture2D>? processedTextures, IEnumerable<Material>? createdMaterials)
            {
                _processedMaterialDictionary = processedMaterialDictionary;
                _processedTextures = processedTextures;
                _createdMaterials = createdMaterials;
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
                    foreach (var texture in _processedTextures)
                        Object.DestroyImmediate(texture);
                    _processedTextures = null;
                }

                if (_createdMaterials != null)
                {
                    foreach (var material in _createdMaterials)
                        Object.DestroyImmediate(material);
                    _createdMaterials = null;
                }

                if (_processedMaterialDictionary != null)
                {
                    _processedMaterialDictionary.Clear();
                    _processedMaterialDictionary = null;
                }
            }
        }
    
        private class EmptyNode : IRenderFilterNode
        {
            public RenderAspects WhatChanged => 0;

            public void OnFrame(Renderer original, Renderer proxy)
            {
                // Do nothing
            }
        }
    }
}
