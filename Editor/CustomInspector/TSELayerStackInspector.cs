#nullable enable
using net.puk06.TexStackEditor.Editor.Models;
using net.puk06.TexStackEditor.Editor.Services;
using net.puk06.TexStackEditor.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace net.puk06.TexStackEditor.Editor
{
    [CustomEditor(typeof(TSELayerStack))]
    internal class TSELayerStackInspector : TSEPreviewableInspector
    {
        public override ExtendedRenderTexture? GeneratePreview()
        {
            TSELayerStack? component = target as TSELayerStack;
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

            UnityService.DrawSectionHeader(LocalizationUtils.Localize("Inspector.Stack.Section.OutputTexture"));
            EditorGUILayout.HelpBox(LocalizationUtils.Localize("Inspector.Stack.OutputTextureDescription"), MessageType.Info);
            RenderOutputTextureSection();

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

        private void RenderOutputTextureSection()
        {
            if (GUILayout.Button(LocalizationUtils.Localize("Inspector.Stack.Button.OutputTexture")))
            {
                TSELayerStack? component = target as TSELayerStack;
                if (component == null) return;

                ExtendedRenderTexture? outputTexture = TextureBuilder.Build(component);
                if (outputTexture != null)
                {
                    string path = EditorUtility.SaveFilePanel(LocalizationUtils.Localize("Inspector.Stack.SaveTextureDialog.Title"), Application.dataPath, "OutputTexture.png", "png");
                    if (!string.IsNullOrEmpty(path))
                    {
                        Texture2D outputTexture2D = outputTexture.ToTexture2D();
                        byte[] pngData = outputTexture2D.EncodeToPNG();
                        System.IO.File.WriteAllBytes(path, pngData);
                        AssetDatabase.Refresh();
                    }
                }
            }
        }
    }
}
