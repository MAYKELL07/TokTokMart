using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;

namespace GokilKisah.Editor
{
    // The demo has no inference models. The editor assistant's dependency contains a Resources
    // folder, so its otherwise unused inference shaders would be compiled into every mobile build.
    // Limit stripping to the explicit demo build helper; normal builds retain package behavior.
    public sealed class MobileDemoShaderStripper : IPreprocessShaders, IPreprocessComputeShaders
    {
        public int callbackOrder=>0;
        static bool IsUnusedInference(Object shader)
        {
            if(!MobileDemoBuild.IsBuildingDemo)return false;
            string path=AssetDatabase.GetAssetPath(shader);
            return path.StartsWith("Packages/com.unity.ai.inference/")||path.StartsWith("Packages/com.unity.sentis/");
        }
        public void OnProcessShader(Shader shader,ShaderSnippetData snippet,IList<ShaderCompilerData> data)
        {if(IsUnusedInference(shader))data.Clear();}
        public void OnProcessComputeShader(ComputeShader shader,string kernelName,IList<ShaderCompilerData> data)
        {if(IsUnusedInference(shader))data.Clear();}
    }
}
