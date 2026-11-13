using System.IO;
using UnityEditor;
using UnityEngine;
namespace GokilKisah.Editor
{
    [InitializeOnLoad]
    public static class AuthoredUIBatchValidation
    {
        const string Folder="Verification/AuthoredUI";
        static double started;
        static AuthoredUIBatchValidation(){EditorApplication.update+=Tick;}
        public static void Run(){AuthoredUIProjectTools.Prepare();Directory.CreateDirectory(Folder);if(File.Exists(Folder+"/canvas-verification.txt"))File.Delete(Folder+"/canvas-verification.txt");SessionState.SetInt("AuthoredUIValidation",1);EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.isPlaying=true;}
        static void Tick()
        {
            int phase=SessionState.GetInt("AuthoredUIValidation",0);if(phase==0)return;
            if(started==0)started=EditorApplication.timeSinceStartup;
            if(EditorApplication.timeSinceStartup-started>180){SessionState.SetInt("AuthoredUIValidation",0);EditorApplication.Exit(2);return;}
            if(phase==1&&EditorApplication.isPlaying&&!EditorApplication.isCompiling)
            {
                var game=Object.FindFirstObjectByType<StoreGameController>();if(!game)return;
                game.BeginCanvasVerification(Folder);SessionState.SetInt("AuthoredUIValidation",2);
            }
            else if(phase==2&&File.Exists(Folder+"/canvas-verification.txt"))
            {
                string report=File.ReadAllText(Folder+"/canvas-verification.txt");Debug.Log(report);
                SessionState.SetInt("AuthoredUIValidation",0);EditorApplication.isPlaying=false;EditorApplication.Exit(report.Contains("FAIL ")?1:0);
            }
        }
    }
}
