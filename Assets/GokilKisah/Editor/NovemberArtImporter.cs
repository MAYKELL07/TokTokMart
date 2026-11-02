using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GokilKisah.Editor
{
    public static class NovemberArtImporter
    {
        const string Source = "Assets/GokilKisah/ArtSources/November2026/";
        const string Runtime = "Assets/GokilKisah/Resources/NovemberArt/";
        static RectInt Bounds(Color32[] pixels, int width, int height)
        {
            int l=width, b=height, r=-1, t=-1;
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)
                if(pixels[y*width+x].a>8){l=Math.Min(l,x);r=Math.Max(r,x);b=Math.Min(b,y);t=Math.Max(t,y);}
            if(r<l)throw new InvalidOperationException("Empty artist image");
            return new RectInt(l,b,r-l+1,t-b+1);
        }
        static Texture2D Read(string path)
        {
            var t=new Texture2D(2,2);if(!t.LoadImage(File.ReadAllBytes(path)))throw new IOException(path);return t;
        }
        static void ImportUI(string input,string output,int max=512)
        {
            var t=Read(Source+input);var b=Bounds(t.GetPixels32(),t.width,t.height);
            var crop=new Texture2D(b.width,b.height,TextureFormat.RGBA32,false);
            crop.SetPixels(t.GetPixels(b.x,b.y,b.width,b.height));crop.Apply();
            string path=Runtime+"UI/"+output+".png";File.WriteAllBytes(path,crop.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(t);UnityEngine.Object.DestroyImmediate(crop);
            AssetDatabase.ImportAsset(path);
            var imp=(TextureImporter)AssetImporter.GetAtPath(path);
            imp.textureType=TextureImporterType.Default;imp.npotScale=TextureImporterNPOTScale.None;
            imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.isReadable=false;
            imp.filterMode=FilterMode.Bilinear;imp.wrapMode=TextureWrapMode.Clamp;
            imp.maxTextureSize=max;imp.textureCompression=TextureImporterCompression.Compressed;imp.SaveAndReimport();
        }
        static void Walk(string folder,string skin)
        {
            var t=Read(Source+"Animation/"+folder+"/"+folder+"Walk/"+folder+"Walk_Sheet_60FPS.png");
            // Full supplied sequence, top-left to bottom-right; one common crop keeps feet anchored.
            int columns=t.width/256,rows=t.height/256,count=NovemberArt.WalkFrameCount(skin);
            int l=256,b=256,r=0,top=0;
            for(int i=0;i<count;i++)
            {
                var box=Bounds(Array.ConvertAll(t.GetPixels(i%columns*256,(rows-1-i/columns)*256,256,256),c=>(Color32)c),256,256);
                l=Math.Min(l,box.x);b=Math.Min(b,box.y);r=Math.Max(r,box.xMax);top=Math.Max(top,box.yMax);
            }
            string sheetPath=Runtime+"Walk/"+skin+".png";File.WriteAllBytes(sheetPath,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);
            AssetDatabase.ImportAsset(sheetPath);var imp=(TextureImporter)AssetImporter.GetAtPath(sheetPath);
            imp.textureType=TextureImporterType.Default;imp.npotScale=TextureImporterNPOTScale.None;imp.maxTextureSize=2048;
            imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.isReadable=false;imp.filterMode=FilterMode.Bilinear;
            imp.textureCompression=TextureImporterCompression.Compressed;imp.SaveAndReimport();
            t=AssetDatabase.LoadAssetAtPath<Texture2D>(sheetPath);
            for(int i=0;i<count;i++)
            {
                string path=Runtime+"Walk/"+skin+i+".asset";
                var sprite=Sprite.Create(t,new Rect(i%columns*256+l,(rows-1-i/columns)*256+b,r-l,top-b),new Vector2(.5f,0),(top-b)/1.88f,0,SpriteMeshType.FullRect);
                sprite.name=skin+" walk "+i;var old=AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if(old){EditorUtility.CopySerialized(sprite,old);UnityEngine.Object.DestroyImmediate(sprite);}else AssetDatabase.CreateAsset(sprite,path);
            }
        }
        static void Portrait(string skin)
        {
            string path="Assets/GokilKisah/Resources/CharacterArt/Art_TTM_Char_"+skin+"01.png";
            var read=Read(path);var rect=Bounds(read.GetPixels32(),read.width,read.height);UnityEngine.Object.DestroyImmediate(read);
            var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.npotScale=TextureImporterNPOTScale.None;imp.isReadable=false;imp.SaveAndReimport();
            var t=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var sprite=Sprite.Create(t,new Rect(rect.x,rect.y,rect.width,rect.height),new Vector2(.5f,0),rect.height/1.88f,0,SpriteMeshType.FullRect);
            sprite.name=skin+"Static";
            string output="Assets/GokilKisah/Resources/PresentationSprites/"+skin+"Static.asset";
            var old=AssetDatabase.LoadAssetAtPath<Sprite>(output);
            if(old){EditorUtility.CopySerialized(sprite,old);UnityEngine.Object.DestroyImmediate(sprite);}else AssetDatabase.CreateAsset(sprite,output);
        }
        [MenuItem("Gokil Kisah/Import November Mobile Artwork")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first");
            Directory.CreateDirectory(Runtime+"UI");Directory.CreateDirectory(Runtime+"Walk");AssetDatabase.Refresh();
            foreach(string name in new[]{"CheckoutIcon01","CustomerCount01","DayTimer01","HazardIcon01","IndicatorContainer","InteractButton01","MoneyCount01","PauseButton01","PuddleIcon01","SprintButton01"})
                ImportUI("UI/Art_TTM_UI_"+name+".png",name);
            ImportUI("UI/Joystick/Art_TTM_UI_Joystick01.png","JoystickBase");
            ImportUI("UI/Joystick/Art_TTM_UI_Joystick01a.png","JoystickKnob",256);
            ImportUI("UI/Asset 11@2x.png","Logo",1024);
            ImportUI("Environment, Props/main menu background.png","MenuBackground",2048);
            Walk("Sam","Sam");Walk("Zack","Zack");Walk("Goblin","Toyol");
            Portrait("Sam");Portrait("Zack");Portrait("Toyol");
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();
            if(shop)
            {
                shop.goblinTemplate.smoothSkin="Toyol";shop.goblinTemplate.framesPerSecond=12;
                var art=shop.goblinTemplate.artwork;var parent=art.transform.parent.lossyScale;
                art.transform.localScale=new Vector3(.65f/1.88f/parent.x,.65f/1.88f/parent.y,1);
                art.transform.localPosition=Vector3.zero;art.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Runtime+"Walk/Toyol0.asset");
                EditorUtility.SetDirty(shop.goblinTemplate);EditorUtility.SetDirty(art);EditorUtility.SetDirty(art.transform);
                EditorSceneManager.MarkSceneDirty(shop.gameObject.scene);EditorSceneManager.SaveScene(shop.gameObject.scene);
            }
            AssetDatabase.SaveAssets();Debug.Log("November art: 14 UI images and 139 smooth walk sprites imported; source originals preserved.");
        }
    }
}
