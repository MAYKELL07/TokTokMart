using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace GokilKisah.Editor
{
    public static class NovemberUpdateTools
    {
        const string Source="Assets/GokilKisah/ArtSources/November2026/";
        const string Runtime="Assets/GokilKisah/Resources/NovemberArt/UI/";
        static bool CopyIfChanged(string source,string destination)
        {
            if(File.Exists(destination)&&File.ReadAllBytes(source).SequenceEqual(File.ReadAllBytes(destination)))return false;
            Directory.CreateDirectory(Path.GetDirectoryName(destination));File.Copy(source,destination,true);return true;
        }
        public static void Apply(string incoming)
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first");
            if(!File.Exists(Path.Combine(incoming,"Font/Bigbesty.ttf")))throw new FileNotFoundException("Bigbesty font missing from art folder");
            var audit=new List<string>();bool environmentChanged=false;
            foreach(var file in Directory.GetFiles(incoming,"*",SearchOption.AllDirectories))
            {
                string relative=file.Substring(incoming.TrimEnd('/','\\').Length+1).Replace('\\','/');
                if(CopyIfChanged(file,Source+relative))audit.Add("Updated source: "+relative);
                if(relative.StartsWith("Environment, Props/Art_TTM_Env_")&&!relative.Contains("CounterTable")&&!relative.Contains("StorageBox"))
                    environmentChanged|=CopyIfChanged(file,"Assets/GokilKisah/ArtSources/TokTokMart/"+Path.GetFileName(file));
                if(relative.StartsWith("Chracter/Art_TTM_Char_"))CopyIfChanged(file,"Assets/GokilKisah/Resources/CharacterArt/"+Path.GetFileName(file));
            }
            Directory.CreateDirectory("Assets/GokilKisah/Resources/Fonts");CopyIfChanged(Path.Combine(incoming,"Font/Bigbesty.ttf"),"Assets/GokilKisah/Resources/Fonts/Bigbesty.ttf");AssetDatabase.Refresh();
            foreach(string name in new[]{"CheckoutIcon01","CheckoutCard01","CustomerCount01","DayTimer01","HazardIcon01","HazardCard01","IndicatorContainer","InteractButton01","MoneyCount01","PauseButton01","PuddleIcon01","PuddleCard01","SprintButton01","ProgressBar01","ProgressBar01a"})
                NovemberArtImporter.ImportUI("UI/Art_TTM_UI_"+name+".png",name);
            NovemberArtImporter.ImportUI("UI/Joystick/Art_TTM_UI_Joystick01.png","JoystickBase");NovemberArtImporter.ImportUI("UI/Joystick/Art_TTM_UI_Joystick01a.png","JoystickKnob",256);
            NovemberArtImporter.ImportUI("UI/Asset 11@2x.png","Logo",1024);NovemberArtImporter.ImportUI("UI/Asset 12.png","StartButton",1024);NovemberArtImporter.ImportUI("UI/Asset 13.png","SettingsButton");NovemberArtImporter.ImportUI("UI/Asset 14.png","LevelsButton");NovemberArtImporter.ImportUI("UI/Asset 16.png","InvoicePaper",1024);NovemberArtImporter.ImportUI("UI/Asset 17.png","Star",256);
            NovemberArtImporter.ImportUI("Environment, Props/main menu background.png","MenuBackground",2048);
            if(environmentChanged)DepthAssetSetup.Apply();
            NovemberArtImporter.ImportUI("Environment, Props/Art_TTM_Env_CounterTable01.png","CounterTable01",2048);
            NovemberArtImporter.ImportUI("Environment, Props/Art_TTM_Env_StorageBox01.png","StorageBox01",1024);
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();
            if(shop)
            {
                Undo.RegisterFullObjectHierarchyUndo(shop.gameObject,"November 11 supplied counter and storage artwork");
                var root=shop.transform.Find("Reference shop layout");
                foreach(string name in new[]{"Counter long top","Counter return","Counter front edge"}){var t=root.Find(name);if(t)t.GetComponent<SpriteRenderer>().enabled=false;}
                Place(root,"Artist counter table November11","CounterTable01",new Vector2(17.91f,-7.56f),4.3f,true);
                foreach(var old in shop.stockroom.GetComponentsInChildren<SpriteRenderer>(true))if(old.name!="Artist storage box November11")old.enabled=false;
                Place(shop.stockroom.transform,"Artist storage box November11","StorageBox01",new Vector2(shop.stockroom.transform.position.x-.6f,shop.stockroom.transform.position.y-.15f),1.45f,false);
                foreach(var t in shop.GetComponentsInChildren<Transform>(true))EditorUtility.SetDirty(t);
                EditorSceneManager.MarkSceneDirty(shop.gameObject.scene);EditorSceneManager.SaveScene(shop.gameObject.scene);
            }
            var font=AssetDatabase.LoadAssetAtPath<Font>("Assets/GokilKisah/Resources/Fonts/Bigbesty.ttf");
            var ui=UnityEngine.Object.FindFirstObjectByType<TokMartCanvas>();if(ui){foreach(var text in ui.GetComponentsInChildren<Text>(true)){text.font=font;EditorUtility.SetDirty(text);}EditorUtility.SetDirty(ui);}
            AssetDatabase.SaveAssets();audit.Add("Bigbesty font imported and applied to authored Canvas text.");audit.Add("Updated UI art, 3-star icon, counter and storage box installed; original interaction geometry retained.");
            Directory.CreateDirectory("Verification/November11");File.WriteAllLines("Verification/November11/art-import.txt",audit);Debug.Log("November 11 art refresh complete");
        }
        static void Place(Transform parent,string name,string image,Vector2 bottomLeft,float height,bool counter)
        {
            string path=Runtime+image+".png";var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=100;importer.SaveAndReimport();
            var t=parent.Find(name);if(!t){t=new GameObject(name).transform;t.SetParent(parent,false);}var r=t.GetComponent<SpriteRenderer>();if(!r)r=t.gameObject.AddComponent<SpriteRenderer>();
            r.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);r.color=Color.white;r.sortingOrder=counter?756:Mathf.RoundToInt(-bottomLeft.y*100);
            float scale=height/r.sprite.bounds.size.y;var worldScale=t.parent.lossyScale;t.localScale=new Vector3(scale/worldScale.x,scale/worldScale.y,1);
            t.position=new Vector3(bottomLeft.x+r.sprite.bounds.size.x*scale*.5f,bottomLeft.y+height*.5f,0);r.enabled=true;EditorUtility.SetDirty(r);
        }
    }
}
