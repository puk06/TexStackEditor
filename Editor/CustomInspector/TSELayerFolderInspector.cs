#nullable enable
using net.puk06.TexStackEditor.Editor.Services;
using net.puk06.TexStackEditor.Editor.Utils;
using UnityEditor;

namespace net.puk06.TexStackEditor.Editor
{
    [CustomEditor(typeof(TSELayerFolder))]
    internal class TSELayerFolderInspector : TSELayerNodeInspector
    {
        public override void OnInspectorGUI()
        {
            UpdateChecker.GenerateVersionLabel();
            LocalizationUtils.DrawLanguageSelectionPopup();
            EditorGUILayout.HelpBox(LocalizationUtils.Localize("Inspector.LayerFolder.Description"), MessageType.Info);

            DrawInspectorGUI();

            bool changed = serializedObject.ApplyModifiedProperties();
            if (changed) UpdatePreview();
        }
    }
}
