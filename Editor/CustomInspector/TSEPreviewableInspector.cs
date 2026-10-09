#nullable enable
using net.puk06.TexStackEditor.Editor.Models;
using net.puk06.TexStackEditor.Editor.Utils;
using UnityEngine;
using UnityEditor;

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
                const float labelHeight = 18f;
                var labelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                // 左半分は自分のプレビュー（もしParentが無ければ自分だけ）
                if (_parentPreviewTexture != null)
                {
                    var halfWidth = rect.width / 2;
                    var leftLabelRect = new Rect(rect.x, rect.y, halfWidth, labelHeight);
                    var rightLabelRect = new Rect(rect.x + halfWidth, rect.y, halfWidth, labelHeight);
                    var leftTextureRect = new Rect(rect.x, rect.y + labelHeight, halfWidth, rect.height - labelHeight);
                    var rightTextureRect = new Rect(rect.x + halfWidth, rect.y + labelHeight, halfWidth, rect.height - labelHeight);

                    GUI.Label(leftLabelRect, LocalizationUtils.Localize("Inspector.Preview.LayerLabel"), labelStyle);
                    GUI.Label(rightLabelRect, LocalizationUtils.Localize("Inspector.Preview.ParentLabel"), labelStyle);
                    GUI.DrawTexture(leftTextureRect, _previewTexture, ScaleMode.ScaleToFit);
                    GUI.DrawTexture(rightTextureRect, _parentPreviewTexture, ScaleMode.ScaleToFit);
                }
                else
                {
                    var labelRect = new Rect(rect.x, rect.y, rect.width, labelHeight);
                    var textureRect = new Rect(rect.x, rect.y + labelHeight, rect.width, rect.height - labelHeight);

                    GUI.Label(labelRect, LocalizationUtils.Localize("Inspector.Preview.Label"), labelStyle);
                    GUI.DrawTexture(textureRect, _previewTexture, ScaleMode.ScaleToFit);
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
