#nullable enable
using UnityEditor;
using UnityEngine;

namespace net.puk06.TexStackEditor.Editor.Services
{
    internal static class TSEShaderEngine
    {
        private const string TextureProcessorGuid = "9dc07817fec58924b8d721b894a1d3e9";
        private const string TextureBlenderGuid = "8e84498de768c184db955801fe3e6df8";

        internal static ComputeShader? TextureProcessorComputeShader { get; private set; }
        internal static ComputeShader? TextureBlenderComputeShader { get; private set; }

        static TSEShaderEngine() => LoadShaders();

        internal static void LoadShaders()
        {
            TextureProcessorComputeShader = Load(TextureProcessorGuid);
            TextureBlenderComputeShader = Load(TextureBlenderGuid);
        }

        private static ComputeShader? Load(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return AssetDatabase.LoadAssetAtPath<ComputeShader>(path);
        }
    }
}
