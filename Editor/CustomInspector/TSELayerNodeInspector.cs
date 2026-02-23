#nullable enable
using net.puk06.TexStackEditor.Editor.Models;
using net.puk06.TexStackEditor.Editor.Services;
using net.puk06.TexStackEditor.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace net.puk06.TexStackEditor.Editor
{
    internal abstract class TSELayerNodeInspector : TSEPreviewableInspector
    {
        public override ExtendedRenderTexture? GeneratePreview()
        {
            TSELayerNode? component = target as TSELayerNode;
            if (component == null) return null;

            return TextureBuilder.BuildNode(component);
        }

        public void DrawInspectorGUI()
        {
            UnityService.DrawSectionHeader(LocalizationUtils.Localize("Inspector.LayerNode.Section.LayerConfiguration"));
            RenderTextureNodeConfigurationSection();

            UnityService.DrawSectionHeader(LocalizationUtils.Localize("Inspector.LayerNode.Section.MaskConfiguration"));
            RenderMaskConfigurationSection();

            UnityService.DrawSectionHeader(LocalizationUtils.Localize("Inspector.LayerNode.Section.ColorAdjustmentConfiguration"));
            RenderColorAdjustmentConfigurationSection();

            UnityService.DrawSectionHeader(LocalizationUtils.Localize("Inspector.LayerNode.Section.AdvancedColorConfiguration"));
            RenderAdvancedColorConfigurationSection();
        }

        internal void RenderTextureNodeConfigurationSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.indentLevel = 1;
            
            SerializedProperty LayerNodeConfigurationProp = serializedObject.FindProperty("LayerNodeConfiguration");

            EditorGUILayout.PropertyField(LayerNodeConfigurationProp.FindPropertyRelative("IsVisible"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.IsVisible")));

            if (LayerNodeConfigurationProp.FindPropertyRelative("IsVisible").boolValue)
            {
                EditorGUI.indentLevel = 2;

                EditorGUILayout.PropertyField(LayerNodeConfigurationProp.FindPropertyRelative("Opacity"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.Opacity")));
                
                EditorGUI.indentLevel = 1;
            }

            EditorGUI.indentLevel = 0;

            EditorGUILayout.EndVertical();
        }

        internal void RenderMaskConfigurationSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.indentLevel = 1;
            
            SerializedProperty MaskConfigurationProp = serializedObject.FindProperty("MaskConfiguration");

            EditorGUILayout.PropertyField(MaskConfigurationProp.FindPropertyRelative("IsEnabled"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.IsEnabled")));

            if (MaskConfigurationProp.FindPropertyRelative("IsEnabled").boolValue)
            {
                EditorGUI.indentLevel = 2;

                SerializedProperty maskTextureProperty = MaskConfigurationProp.FindPropertyRelative("MaskTexture");
                maskTextureProperty.objectReferenceValue = (Texture2D)EditorGUILayout.ObjectField(LocalizationUtils.Localize("Inspector.LayerNode.Component.MaskTexture"), (Texture2D)maskTextureProperty.objectReferenceValue, typeof(Texture2D), true);

                SerializedProperty maskSelectionTypeProperty = MaskConfigurationProp.FindPropertyRelative("MaskSelectionType");
                
                string[] maskSelectionTypeLabels =
                {
                    LocalizationUtils.Localize("Inspector.LayerNode.Component.MaskSelectionType.Options.None"),
                    LocalizationUtils.Localize("Inspector.LayerNode.Component.MaskSelectionType.Options.Black"),
                    LocalizationUtils.Localize("Inspector.LayerNode.Component.MaskSelectionType.Options.White"),
                    string.Format("{0} (A = 255)", LocalizationUtils.Localize("Inspector.LayerNode.Component.MaskSelectionType.Options.Opaque")),
                    string.Format("{0} (A ≠ 0)", LocalizationUtils.Localize("Inspector.LayerNode.Component.MaskSelectionType.Options.Opaque")),
                    string.Format("{0} (A = 0)", LocalizationUtils.Localize("Inspector.LayerNode.Component.MaskSelectionType.Options.Transparent"))
                };

                maskSelectionTypeProperty.enumValueIndex = EditorGUILayout.Popup(LocalizationUtils.Localize("Inspector.LayerNode.Component.MaskSelectionType"), maskSelectionTypeProperty.enumValueIndex, maskSelectionTypeLabels);

                SerializedProperty maskBlendSettingsProperty = MaskConfigurationProp.FindPropertyRelative("MaskBlendSettings");
                EditorGUILayout.PropertyField(maskBlendSettingsProperty.FindPropertyRelative("IsBackgroundTransparent"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.MaskBlendSettings.IsBackgroundTransparent")));

                EditorGUI.indentLevel = 1;
            }

            EditorGUI.indentLevel = 0;

            EditorGUILayout.EndVertical();
        }

        internal void RenderColorAdjustmentConfigurationSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.indentLevel = 1;
            
            SerializedProperty ColorAdjustmentConfigurationProp = serializedObject.FindProperty("ColorAdjustmentConfiguration");

            EditorGUILayout.PropertyField(ColorAdjustmentConfigurationProp.FindPropertyRelative("IsEnabled"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.IsEnabled")));

            if (ColorAdjustmentConfigurationProp.FindPropertyRelative("IsEnabled").boolValue)
            {
                EditorGUI.indentLevel = 2;

                string[] colorAdjustmentModeLabels =
                {
                    LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.Mode.Options.Normal"),
                    LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.Mode.Options.V1"),
                    LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.Mode.Options.V2"),
                    LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.Mode.Options.V3"),
                };
                
                SerializedProperty modeProperty = ColorAdjustmentConfigurationProp.FindPropertyRelative("Mode");
                modeProperty.enumValueIndex = EditorGUILayout.Popup(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.Mode"), modeProperty.enumValueIndex, colorAdjustmentModeLabels);

                SerializedProperty normalColorSetingsProperty = ColorAdjustmentConfigurationProp.FindPropertyRelative("NormalColorSettings");

                switch (modeProperty.enumValueIndex)
                {
                    case 0:
                        {
                            EditorGUILayout.PropertyField(normalColorSetingsProperty.FindPropertyRelative("SourceColor"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.Normal.SourceColor")));
                            EditorGUILayout.PropertyField(normalColorSetingsProperty.FindPropertyRelative("TargetColor"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.Normal.TargetColor")));
                            break;
                        }

                    case 1:
                        {
                            EditorGUILayout.HelpBox(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.V1.Description"), MessageType.Info);
                            SerializedProperty v1ColorSetingsProperty = ColorAdjustmentConfigurationProp.FindPropertyRelative("V1ColorSettings");
                            EditorGUILayout.PropertyField(normalColorSetingsProperty.FindPropertyRelative("SourceColor"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.Normal.SourceColor")));
                            EditorGUILayout.PropertyField(normalColorSetingsProperty.FindPropertyRelative("TargetColor"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.Normal.TargetColor")));
                            EditorGUILayout.PropertyField(v1ColorSetingsProperty.FindPropertyRelative("Weight"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.V1.Weight")));
                            EditorGUILayout.PropertyField(v1ColorSetingsProperty.FindPropertyRelative("Minimum"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.V1.Minimum")));
                            break;
                        }

                    case 2:
                        {
                            EditorGUILayout.HelpBox(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.V2.Description"), MessageType.Info);
                            SerializedProperty v2ColorSetingsProperty = ColorAdjustmentConfigurationProp.FindPropertyRelative("V2ColorSettings");
                            EditorGUILayout.PropertyField(normalColorSetingsProperty.FindPropertyRelative("SourceColor"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.Normal.SourceColor")));
                            EditorGUILayout.PropertyField(normalColorSetingsProperty.FindPropertyRelative("TargetColor"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.Normal.TargetColor")));
                            EditorGUILayout.PropertyField(v2ColorSetingsProperty.FindPropertyRelative("Weight"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.V2.Weight")));
                            EditorGUILayout.PropertyField(v2ColorSetingsProperty.FindPropertyRelative("Radius"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.V2.Radius")));
                            EditorGUILayout.PropertyField(v2ColorSetingsProperty.FindPropertyRelative("Minimum"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.V2.Minimum")));
                            EditorGUILayout.PropertyField(v2ColorSetingsProperty.FindPropertyRelative("IncludeOutside"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.V2.IncludeOutside")));
                            break;
                        }

                    case 3:
                        {
                            EditorGUILayout.HelpBox(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.V3.Description"), MessageType.Info);
                            SerializedProperty v3ColorSetingsProperty = ColorAdjustmentConfigurationProp.FindPropertyRelative("V3ColorSettings");
                            EditorGUILayout.PropertyField(v3ColorSetingsProperty.FindPropertyRelative("Gradient"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.V3.Gradient")));
                            EditorGUILayout.PropertyField(v3ColorSetingsProperty.FindPropertyRelative("Resolution"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.ColorAdjustmentConfiguration.V3.Resolution")));
                            break;
                        }
                }

                EditorGUI.indentLevel = 1;
            }

            EditorGUI.indentLevel = 0;

            EditorGUILayout.EndVertical();
        }

        internal void RenderAdvancedColorConfigurationSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUI.indentLevel = 1;
            
            SerializedProperty AdvancedColorConfigurationProp = serializedObject.FindProperty("AdvancedColorConfiguration");

            EditorGUILayout.PropertyField(AdvancedColorConfigurationProp.FindPropertyRelative("IsEnabled"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.AdvancedColorConfiguration.IsEnabled")));

            if (AdvancedColorConfigurationProp.FindPropertyRelative("IsEnabled").boolValue)
            {
                EditorGUI.indentLevel = 2;

                EditorGUILayout.PropertyField(AdvancedColorConfigurationProp.FindPropertyRelative("Hue"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.AdvancedColorConfiguration.Hue")));
                EditorGUILayout.PropertyField(AdvancedColorConfigurationProp.FindPropertyRelative("Saturation"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.AdvancedColorConfiguration.Saturation")));
                EditorGUILayout.PropertyField(AdvancedColorConfigurationProp.FindPropertyRelative("Value"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.AdvancedColorConfiguration.Value")));
                EditorGUILayout.PropertyField(AdvancedColorConfigurationProp.FindPropertyRelative("Brightness"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.AdvancedColorConfiguration.Brightness")));
                EditorGUILayout.PropertyField(AdvancedColorConfigurationProp.FindPropertyRelative("Contrast"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.AdvancedColorConfiguration.Contrast")));
                EditorGUILayout.PropertyField(AdvancedColorConfigurationProp.FindPropertyRelative("Gamma"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.AdvancedColorConfiguration.Gamma")));
                EditorGUILayout.PropertyField(AdvancedColorConfigurationProp.FindPropertyRelative("Exposure"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.AdvancedColorConfiguration.Exposure")));
                EditorGUILayout.PropertyField(AdvancedColorConfigurationProp.FindPropertyRelative("Transparency"), new GUIContent(LocalizationUtils.Localize("Inspector.LayerNode.Component.AdvancedColorConfiguration.Transparency")));

                EditorGUI.indentLevel = 1;
            }

            EditorGUI.indentLevel = 0;

            EditorGUILayout.EndVertical();
        }
    }
}
