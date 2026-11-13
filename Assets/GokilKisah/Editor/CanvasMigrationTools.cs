using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace GokilKisah.Editor
{
    [InitializeOnLoad]
    public static class CanvasMigrationTools
    {
        static string Root=>Directory.GetParent(Application.dataPath).FullName;
        static string Request=>Path.Combine(Root,"Temp/CanvasMigration.request");
        static string Status=>Path.Combine(Root,"Temp/CanvasMigration.status");
        const string PrefabPath="Assets/GokilKisah/Resources/TokMartEditableUI.prefab";
        static int phase {get=>SessionState.GetInt("TokMart.CanvasMigration.Phase",0);set=>SessionState.SetInt("TokMart.CanvasMigration.Phase",value);}
        static CanvasMigrationTools(){EditorApplication.update+=Tick;}
        public static void RequestBuild(){File.WriteAllText(Request,"build");}
        static void Tick()
        {
            if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
            try
            {
                if(phase==0&&File.Exists(Request))
                {
                    File.Delete(Request);File.WriteAllText(Status,"Preparing editable Canvas");
                    if(!EditorApplication.isPlaying){Directory.CreateDirectory(Path.Combine(Root,"UI-Backups"));EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"UI-Backups/Before-Canvas-Install-"+System.DateTime.Now.ToString("yyyyMMdd-HHmmss")+".unity",true);EditorApplication.isPlaying=true;}
                    phase=1;return;
                }
                if(phase==1&&EditorApplication.isPlaying&&!EditorApplication.isPlayingOrWillChangePlaymode)return;
                if(phase==1&&EditorApplication.isPlaying)
                {
                    var game=Object.FindFirstObjectByType<StoreGameController>();if(!game)return;game.BakeEditableScreens();phase=2;return;
                }
                if(phase==2)
                {
                    var game=Object.FindFirstObjectByType<StoreGameController>();if(!game||!game.CanvasBakeComplete)return;
                    foreach(var e in game.editableUI.GetComponentsInChildren<TokMartCanvasElement>(true))e.ResetBindingPreview();
                    PersistArtwork(game.editableUI);PrefabUtility.SaveAsPrefabAsset(game.editableUI.gameObject,PrefabPath);
                    File.WriteAllText(Status,"Prefab saved; installing scene instance");phase=3;EditorApplication.isPlaying=false;return;
                }
                if(phase==3&&!EditorApplication.isPlaying&&!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    var game=Object.FindFirstObjectByType<StoreGameController>();if(!game)throw new System.Exception("No StoreGameController in scene");
                    var ui=Object.FindFirstObjectByType<TokMartCanvas>();if(!ui)ui=((GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath))).GetComponent<TokMartCanvas>();
                    game.editableUI=ui;EditorUtility.SetDirty(game);EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);
                    Selection.activeGameObject=ui.gameObject;File.WriteAllText(Status,"Complete: editable prefab installed in scene; "+ui.GetComponentsInChildren<TokMartCanvasElement>(true).Length+" editable elements");phase=0;
                }
            }
            catch(System.Exception ex){File.WriteAllText(Status,"ERROR: "+ex);Debug.LogException(ex);phase=0;}
        }
        static void PersistArtwork(TokMartCanvas ui)
        {
            const string folder="Assets/GokilKisah/Resources/CanvasGenerated";Directory.CreateDirectory(folder);
            var textures=new System.Collections.Generic.Dictionary<Texture,Texture2D>();
            Texture2D Save(Texture texture)
            {
                if(!texture)return null;if(AssetDatabase.Contains(texture))return texture as Texture2D;
                if(textures.TryGetValue(texture,out var cached))return cached;
                var rt=RenderTexture.GetTemporary(texture.width,texture.height,0);var old=RenderTexture.active;Graphics.Blit(texture,rt);RenderTexture.active=rt;
                var copy=new Texture2D(texture.width,texture.height,TextureFormat.RGBA32,false);copy.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);copy.Apply();RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);
                var bytes=copy.EncodeToPNG();string hash;using(var sha=System.Security.Cryptography.SHA256.Create())hash=System.BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").Substring(0,24);
                string path=folder+"/Texture-"+hash+".png";File.WriteAllBytes(path,bytes);Object.DestroyImmediate(copy);AssetDatabase.ImportAsset(path);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spriteBorder=new Vector4(10,10,10,10);importer.alphaIsTransparency=true;importer.SaveAndReimport();
                var saved=AssetDatabase.LoadAssetAtPath<Texture2D>(path);textures[texture]=saved;return saved;
            }
            foreach(var raw in ui.GetComponentsInChildren<RawImage>(true)){raw.texture=Save(raw.texture);var e=raw.GetComponent<TokMartCanvasElement>();if(e)e.originalTexture=raw.texture;}
            foreach(var image in ui.GetComponentsInChildren<Image>(true))if(image.sprite&&!AssetDatabase.Contains(image.sprite)){var texture=Save(image.sprite.texture);image.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(texture));}
            AssetDatabase.SaveAssets();
        }
    }
}
