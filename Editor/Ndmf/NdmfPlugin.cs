#nullable enable
using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.util;
using net.puk06.TexStackEditor.Editor.Ndmf;
using UnityEngine;

[assembly: ExportsPlugin(typeof(NdmfPlugin))]
namespace net.puk06.TexStackEditor.Editor.Ndmf
{
    internal class NdmfPlugin : Plugin<NdmfPlugin>
    {
        public override string QualifiedName => "net.puk06.tex-stack-editor";
        public override string DisplayName => "TexStackEditor";

        protected override void Configure()
        {
            InPhase(BuildPhase.Transforming)
                .AfterPlugin("net.rs64.tex-trans-tool")
                .AfterPlugin("nadena.dev.modular-avatar")
                .BeforePlugin("net.puk06.color-changer")
                .Run(BuildTextures.Instance)
#if LLC_2_4_0_OR_NEWER
                .BeforePass("io.github.azukimochi.light-limit-changer.normalize-materials")
#endif
                .PreviewingWith(new RealtimePreview());

            InPhase(BuildPhase.Optimizing)
                .AfterPlugin("net.rs64.tex-trans-tool")
                .BeforePlugin("com.anatawa12.avatar-optimizer")
                .Run(RemoveComponents.Instance);
        }
    }

    internal class BuildTextures : Pass<BuildTextures>
    {
        protected override void Execute(BuildContext context)
        {
            var avatar = context.AvatarRootObject;
            var components = avatar.GetComponentsInChildren<TSELayerStack>(false);

            var processedTexturesDictionary = NdmfProcessor.ProcessAllComponents(components,
                onSuccess: component =>
                {
                    string textureName = component.TargetTexture == null ? "Unknown Texture" : component.TargetTexture.name;
                    ErrorReport.ReportError(NdmfLocalizer.Localizer, ErrorSeverity.Information, "NdmfBuild.Processing.Success", component.AvatarRootPath(), textureName);
                },
                onFailed: component =>
                {
                    string textureName = component.TargetTexture == null ? "Unknown Texture" : component.TargetTexture.name;
                    ErrorReport.ReportError(NdmfLocalizer.Localizer, ErrorSeverity.NonFatal, "NdmfBuild.Processing.Failed", component.AvatarRootPath(), textureName);
                }
            );
            var renderers = avatar.GetComponentsInChildren<Renderer>(true).Where(r => r is MeshRenderer or SkinnedMeshRenderer);
            NdmfProcessor.ReplaceTexturesInRenderers(renderers, NdmfProcessor.ConvertToTexture2DDictionary(processedTexturesDictionary));
        }
    }

    internal class RemoveComponents : Pass<RemoveComponents>
    {
        protected override void Execute(BuildContext context)
        {
            var avatar = context.AvatarRootObject;
            var components = avatar.GetComponentsInChildren<TSELayerStack>(true);
            var childComponents = avatar.GetComponentsInChildren<TSELayerNode>(true);

            RemoveAllComponents(components);
            RemoveAllComponents(childComponents);
        }

        private void RemoveAllComponents(IEnumerable<Component> components)
        {
            foreach (var component in components)
            {
                if (component == null) continue;
                Object.DestroyImmediate(component);
            }
        }
    }
}
