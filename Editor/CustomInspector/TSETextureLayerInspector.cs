#nullable enable
using net.puk06.TexStackEditor.Editor.Services;
using net.puk06.TexStackEditor.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace net.puk06.TexStackEditor.Editor
{
    [CustomEditor(typeof(TSETextureLayer))]
    internal class TSETextureLayerInspector : TSELayerNodeInspector
    {
        public override void OnInspectorGUI()
        {
            UpdateChecker.GenerateVersionLabel();
            LocalizationUtils.DrawLanguageSelectionPopup();
            EditorGUILayout.HelpBox(LocalizationUtils.Localize("Inspector.TextureLayer.Description"), MessageType.Info);

            UnityService.DrawSectionHeader(LocalizationUtils.Localize("Inspector.TextureLayer.Section.LayerTexture"));
            RenderLayerTextureSection();
            
            DrawInspectorGUI();

            bool changed = serializedObject.ApplyModifiedProperties();
            if (changed) UpdatePreview();
        }

        private void RenderLayerTextureSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.indentLevel = 1;

            SerializedProperty layerTextureProperty = serializedObject.FindProperty("LayerTexture");
            layerTextureProperty.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField(LocalizationUtils.Localize("Inspector.TextureLayer.Component.LayerTexture"), (Texture2D)layerTextureProperty.objectReferenceValue, typeof(Texture2D), true);
        
            EditorGUI.indentLevel = 0;

            EditorGUILayout.EndVertical();
        }
    }
}
