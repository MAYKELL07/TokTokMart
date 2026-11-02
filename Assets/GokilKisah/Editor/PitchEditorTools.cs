using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GokilKisah.Editor
{
    [InitializeOnLoad]
    public static class PitchEditorTools
    {
        const string ScenePath="Assets/GokilKisah/Scenes/TokMartPitch.unity";
        static readonly string Root=Directory.GetParent(Application.dataPath).FullName;
        static string CommandPath=>Path.Combine(Root,"Temp/TokMartCommand.json");
        static string ReplyPath=>Path.Combine(Root,"Temp/TokMartReply.json");
        [Serializable] class Command {public string action;public string path;}
        [Serializable] class Reply {public string action;public string result;public bool playing;public bool compiling;public string scene;}
        static PitchEditorTools(){EditorApplication.update+=Tick;}
        static void ReplyTo(string action,string result){File.WriteAllText(ReplyPath,JsonUtility.ToJson(new Reply{action=action,result=result,playing=EditorApplication.isPlaying,compiling=EditorApplication.isCompiling,scene=SceneManager.GetActiveScene().path},true));}
        static void Tick()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            if(!File.Exists(CommandPath))return;
            Command command;
            try{command=JsonUtility.FromJson<Command>(File.ReadAllText(CommandPath));File.Delete(CommandPath);}
            catch(Exception ex){ReplyTo("read",ex.Message);return;}
            try
            {
                switch(command.action)
                {
                    case "novemberart":NovemberArtImporter.Apply();ReplyTo(command.action,"November artwork imported");break;
                    case "mobileverify":RequireGame().BeginMobileVerification(command.path);ReplyTo(command.action,"Mobile checks started");break;
                    case "androidbuild":ReplyTo(command.action,MobileDemoBuild.Build(command.path));break;
                    case "refresh":AssetDatabase.Refresh();ReplyTo(command.action,"Refreshed");break;
                    case "shelfinspect":ShelfTrafficSetup.Inspect();ReplyTo(command.action,"Shelf templates inspected");break;
                    case "shelfslotorder":ShelfTrafficSetup.OrderSlots();ReplyTo(command.action,"Stock slots ordered in hierarchy");break;
                    case "shelftemplates":ShelfTrafficSetup.Apply();ReplyTo(command.action,"Authored shelf templates and ordered slot references saved");break;
                    case "trafficverify":RequireGame().BeginTrafficVerification(command.path);ReplyTo(command.action,"Traffic, mood and customer-only entrance checks started");break;
                    case "depthwalls":DepthAssetSetup.RepairWalls();ReplyTo(command.action,"Wall proportions repaired");break;
                    case "depthassets":DepthAssetSetup.Apply();ReplyTo(command.action,"New environment assets and static Zack applied with uniform proportions");break;
                    case "depthverify":RequireGame().BeginDepthAssetVerification(command.path);ReplyTo(command.action,"Depth asset verification started");break;
                    case "closecameraverify":RequireGame().BeginCloseCameraPreview(command.path);ReplyTo(command.action,"Close follow camera check started");break;
                    case "arcadeapply":ArcadeShopSetup.Apply();ReplyTo(command.action,"Arcade room layout saved");break;
                    case "arcadeverify":RequireGame().BeginArcadeVerification(command.path);ReplyTo(command.action,"Arcade interface verification started");break;
                    case "cozylighting":CozyLightingSetup.Apply();ReplyTo(command.action,"Cozy 2D lighting and post processing saved");break;
                    case "cozypreview":RequireGame().BeginCozyLightingPreview(command.path);ReplyTo(command.action,"Lighting preview started");break;
                    case "samart":SamCharacterArt.Apply();ReplyTo(command.action,"Supplied static Sam artwork imported and scene saved");break;
                    case "sampreview":RequireGame().BeginSamArtPreview(command.path);ReplyTo(command.action,"Sam and Zack appearance preview started");break;
                    case "roomrouteverify":RequireGame().BeginRoomRouteVerification(command.path);ReplyTo(command.action,"Room visibility and customer route verification started");break;
                    case "responsiveapply":ResponsiveCameraSetup.Apply();ReplyTo(command.action,"Test follow camera enabled, original preserved inactive, smooth fades installed");break;
                    case "originalcamera":ResponsiveCameraSetup.UseOriginal();ReplyTo(command.action,"Original camera enabled");break;
                    case "testcamera":ResponsiveCameraSetup.UseTest();ReplyTo(command.action,"Test follow camera enabled");break;
                    case "responsiveverify":RequireGame().BeginResponsiveVerification(command.path);ReplyTo(command.action,"Responsive controls, follow camera and fades verification started");break;
                    case "artverify":RequireGame().BeginArtPresentationVerification(command.path);ReplyTo(command.action,"Visual presentation checks started");break;
                    case "artrepair":ArtLayoutRepair.Apply();ReplyTo(command.action,"Art proportions and scene placement repaired");break;
                    case "snapshot":EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),command.path,true);ReplyTo(command.action,"Scene snapshot saved");break;
                    case "perfverify":RequireGame().BeginPerformanceVerification(command.path);ReplyTo(command.action,"Performance capture started");break;
                    case "referencelayout":ReferenceShopLayout.Apply();ReplyTo(command.action,"Reference shop layout saved");break;
                    case "levelverify":RequireGame().BeginLevelVerification(command.path);ReplyTo(command.action,"Level progression and layout verification started");break;
                    case "artistapply":TokTokArtistArt.Apply();ReplyTo(command.action,"All supplied environment assets applied and saved");break;
                    case "artistaudit":TokTokArtistArt.Audit();ReplyTo(command.action,"All eight supplied assets are in use");break;
                    case "onlinebake":OnlineMarketArt.Bake();ReplyTo(command.action,"Downloaded market art baked");break;
                    case "onlineapply":OnlineMarketArt.Apply();StorageRoomBuilder.Apply();ReplyTo(command.action,"Online assets and storage room saved");break;
                    case "improve":ShiftSceneUpgrade.Apply();ReplyTo(command.action,"Larger store and smooth art saved");break;
                    case "upgradepreview":RequireGame().BeginUpgradePreview(command.path);ReplyTo(command.action,"Preview screens captured and cashier opened");break;
                    case "upgradeverify":RequireGame().BeginUpgradeVerification(command.path);ReplyTo(command.action,"Upgrade verification started");break;
                    case "setup":BuildScene();ReplyTo(command.action,"Prototype scene saved and opened");break;
                    case "play":if(SceneManager.GetActiveScene().path!=ScenePath)EditorSceneManager.OpenScene(ScenePath);EditorApplication.isPlaying=true;ReplyTo(command.action,"Play requested");break;
                    case "stop":EditorApplication.isPlaying=false;ReplyTo(command.action,"Stop requested");break;
                    case "show":EditorApplication.ExecuteMenuItem("Window/General/Game");ReplyTo(command.action,"Game view opened");break;
                    case "verify":RequireGame().BeginVerification(command.path);ReplyTo(command.action,"Play Mode verification started");break;
                    case "customers":RequireGame().BeginCustomerCapture(command.path);ReplyTo(command.action,"Customer journey capture started");break;
                    case "capture":RequireGame().BeginCapture(command.path);ReplyTo(command.action,"Five gameplay replay captures started");break;
                    case "preview":RequireGame().BeginPresentationCapture(command.path);ReplyTo(command.action,"Presentation capture started");break;
                    case "build":
                        if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode before building");
                        Directory.CreateDirectory(Path.GetDirectoryName(command.path));
                        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=command.path,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
                        ReplyTo(command.action,report.summary.result+" / "+report.summary.totalErrors+" errors");break;
                    default:ReplyTo(command.action,"Editor ready");break;
                }
            }catch(Exception ex){Debug.LogException(ex);ReplyTo(command.action,"ERROR: "+ex.Message);}
        }
        static StoreGameController RequireGame(){if(!EditorApplication.isPlaying)throw new InvalidOperationException("Enter Play Mode first");return UnityEngine.Object.FindFirstObjectByType<StoreGameController>()??throw new InvalidOperationException("Prototype component missing");}
        [MenuItem("Gokil Kisah/Open Pitch Prototype")]
        static void Open(){if(!File.Exists(ScenePath))BuildScene();else EditorSceneManager.OpenScene(ScenePath);}
        [MenuItem("Gokil Kisah/Set Up Editable Shop")]
        static void BuildScene()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first");
            if(File.Exists(ScenePath)){if(SceneManager.GetActiveScene().path!=ScenePath){if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Save your current scene before opening another scene.");EditorSceneManager.OpenScene(ScenePath);}ShopSceneBuilder.Populate();return;}
            if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Current scene has unsaved changes; save it before opening prototype");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Pitch Camera").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.08f,.13f,.19f);camera.orthographic=true;camera.transform.position=new Vector3(0,0,-10);camera.gameObject.AddComponent<AudioListener>();
            new GameObject("Store Game Controller").AddComponent<StoreGameController>();
            EditorSceneManager.SaveScene(scene,ScenePath);ShopSceneBuilder.Populate();
            Selection.activeObject=UnityEngine.Object.FindFirstObjectByType<StoreGameController>();
            Debug.Log("TOK MART pitch prototype ready. Choose Sam or Zack in Play Mode.");
        }
        public static void BatchBuild()
        {
            BuildScene();
            string output=null;
            foreach(string arg in Environment.GetCommandLineArgs())if(arg.StartsWith("--pitch-build="))output=arg.Substring(14);
            if(string.IsNullOrEmpty(output))throw new ArgumentException("Missing --pitch-build path");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=output,target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Pitch build failed: "+report.summary.totalErrors+" errors");
        }
    }
}



