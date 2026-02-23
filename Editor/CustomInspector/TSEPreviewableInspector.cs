#nullable enable
using net.puk06.TexStackEditor.Editor.Models;
using UnityEngine;

namespace net.puk06.TexStackEditor.Editor
{
    internal abstract class TSEPreviewableInspector : UnityEditor.Editor
    {
        private ExtendedRenderTexture? _previewTexture;
        private void OnEnable() => UpdatePreview();
        private void OnDisable() => DisposeTexture();
        private void OnValidate() => ReloadPreviewInstances();

        public override bool HasPreviewGUI() => true;
        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            if (_previewTexture != null)
            {
                GUI.DrawTexture(rect, _previewTexture, ScaleMode.ScaleToFit);
            }
        }

        public abstract ExtendedRenderTexture? GeneratePreview();
        
        public void DisposeTexture()
        {
            if (_previewTexture != null) _previewTexture.Dispose();
        }

        public void UpdatePreview()
        {
            DisposeTexture();
            _previewTexture = GeneratePreview();
        }
    }
}
