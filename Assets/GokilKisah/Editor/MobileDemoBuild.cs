using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GokilKisah.Editor
{
    public static class MobileDemoBuild
    {
        public static bool IsBuildingDemo {get;private set;}
        [Serializable] class Toolchain {public string sdk;public string ndk;public string jdk;}
        static void ConfigureToolchain()
        {
            const string path="Tools/android-toolchain.local.json";
            if(!File.Exists(path))return;
            var config=JsonUtility.FromJson<Toolchain>(File.ReadAllText(path));
            var type=Type.GetType("UnityEditor.Android.AndroidExternalToolsSettings, UnityEditor.Android.Extensions");
            if(type==null)throw new InvalidOperationException("Restart Unity to load the newly installed Android module");
            foreach(var pair in new[]{("sdkRootPath",config.sdk),("ndkRootPath",config.ndk),("jdkRootPath",config.jdk)})
            {
                if(!Directory.Exists(pair.Item2))throw new DirectoryNotFoundException(pair.Item2);
                type.GetProperty(pair.Item1).SetValue(null,pair.Item2);
            }
        }
        [MenuItem("Gokil Kisah/Build Android Demo APK")]
        public static void MenuBuild()=>Build("Builds/Android/TokTokMart-Demo.apk");
        public static string Build(string path)
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first");
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))throw new InvalidOperationException("Android Build Support module is required");
            ConfigureToolchain();
            PlayerSettings.companyName="TokTokMart";PlayerSettings.productName="TokTokMart Demo";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.toktokmart.demo");
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
            PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevel35;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.Android.useCustomKeystore=false;
            PlayerSettings.bundleVersion="0.3.0";PlayerSettings.Android.bundleVersionCode=3;
            EditorUserBuildSettings.buildAppBundle=false;Directory.CreateDirectory(Path.GetDirectoryName(path));
            BuildReport report;
            IsBuildingDemo=true;
            try
            {
                report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                    scenes=new[]{"Assets/GokilKisah/Scenes/TokMartPitch.unity"},locationPathName=path,
                    target=BuildTarget.Android,options=BuildOptions.None});
            }
            finally {IsBuildingDemo=false;}
            string result=report.summary.result+" / "+report.summary.totalErrors+" errors / "+report.summary.totalSize+" bytes";
            string reportDirectory=Path.Combine("Verification/Builds",PlayerSettings.bundleVersion);
            Directory.CreateDirectory(reportDirectory);File.WriteAllText(Path.Combine(reportDirectory,"build-result.txt"),result);
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception(result);
            return result;
        }
        public static void BatchBuild()
        {
            string path="Builds/Android/TokTokMart-Demo.apk";
            foreach(string arg in Environment.GetCommandLineArgs())if(arg.StartsWith("--android-output="))path=arg.Substring(17);
            Build(path);
        }
    }
}
