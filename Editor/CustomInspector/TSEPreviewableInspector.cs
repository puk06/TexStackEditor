#nullable enable
using net.puk06.TexStackEditor.Editor.Models;
using UnityEngine;

namespace net.puk06.TexStackEditor.Editor
{
    internal abstract class TSEPreviewableInspector : UnityEditor.Editor
    {
        private ExtendedRenderTexture? _previewTexture;
        private ExtendedRenderTexture? _parentPreviewTexture;
        private void OnEnable() => UpdatePreview();
        private void OnDisable() => DisposeTexture();
        private void OnValidate() => ReloadPreviewInstances();

        public override bool HasPreviewGUI() => true;
        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            if (_previewTexture != null)
            {
                // GUI.DrawTexture(rect, _previewTexture, ScaleMode.ScaleToFit);
                // 左半分は自分のプレビュー（もしParentが無ければ自分だけ）

                if (_parentPreviewTexture != null)
                {
                    var halfWidth = rect.width / 2;
                    var leftRect = new Rect(rect.x, rect.y, halfWidth, rect.height);
                    var rightRect = new Rect(rect.x + halfWidth, rect.y, halfWidth, rect.height);

                    GUI.DrawTexture(leftRect, _previewTexture, ScaleMode.ScaleToFit);
                    GUI.DrawTexture(rightRect, _parentPreviewTexture, ScaleMode.ScaleToFit);
                }
                else
                {
                    GUI.DrawTexture(rect, _previewTexture, ScaleMode.ScaleToFit);
                }
            }
        }

        public abstract ExtendedRenderTexture? GeneratePreview();
        public abstract ExtendedRenderTexture? GenerateParentPreview();
        
        public void DisposeTexture()
        {
            if (_previewTexture != null) _previewTexture.Dispose();
            if (_parentPreviewTexture != null) _parentPreviewTexture.Dispose();
        }

        public void UpdatePreview()
        {
            DisposeTexture();
            _previewTexture = GeneratePreview();
            _parentPreviewTexture = GenerateParentPreview();
        }
    }
}
