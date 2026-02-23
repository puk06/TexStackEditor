#nullable enable
using net.puk06.TexStackEditor.Editor.Models;
using net.puk06.TexStackEditor.Editor.Services;
using net.puk06.TexStackEditor.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace net.puk06.TexStackEditor.Editor
{
    [CustomEditor(typeof(TSETextureLayerStack))]
    internal class TSETextureLayerStackInspector : TSEPreviewableInspector
    {
        public override ExtendedRenderTexture? GeneratePreview()
        {
            TSETextureLayerStack? component = target as TSETextureLayerStack;
            if (component == null || component.TargetTexture == null) return null;
            
            return TextureBuilder.Build(component);
        }

        public override void OnInspectorGUI()
        {
            UpdateChecker.GenerateVersionLabel();
            LocalizationUtils.DrawLanguageSelectionPopup();
            EditorGUILayout.HelpBox(LocalizationUtils.Localize("Inspector.Stack.Description"), MessageType.Info);
            UnityService.DrawSectionHeader(LocalizationUtils.Localize("Inspector.Stack.Section.TargetTexture"));
            RenderTargetTextureSection();

            bool changed = serializedObject.ApplyModifiedProperties();
            if (changed) UpdatePreview();
        }

        private void RenderTargetTextureSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.indentLevel = 1;

            SerializedProperty targetTextureProperty = serializedObject.FindProperty("TargetTexture");
            targetTextureProperty.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField(LocalizationUtils.Localize("Inspector.Stack.Component.TargetTexture"), (Texture2D)targetTextureProperty.objectReferenceValue, typeof(Texture2D), true);
        
            EditorGUI.indentLevel = 0;

            EditorGUILayout.EndVertical();
        }
    }
}
