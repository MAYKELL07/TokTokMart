using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace GokilKisah.Editor
{
    public static class DepthAssetSetup
    {
        const string Source="Assets/GokilKisah/ArtSources/TokTokMart/";
        const string Sprites="Assets/GokilKisah/SceneSprites/TokTokMart/";
        static readonly string[] Names={"ExtWall01","Floor01","Fridge01","IntWall01","Shelf01","Shelf02","StorageDoor01","StorageRoom01","EntranceDoor01"};
        static Texture2D Read(string path,int maximum)
        {
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;
            importer.maxTextureSize=maximum;importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.isReadable=true;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static Rect AlphaRect(Texture2D texture)
        {
            var pixels=texture.GetPixels32();int left=texture.width,right=-1,bottom=texture.height,top=-1;
            for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)if(pixels[y*texture.width+x].a>8)
            {left=Math.Min(left,x);right=Math.Max(right,x);bottom=Math.Min(bottom,y);top=Math.Max(top,y);}
            if(right<left)throw new InvalidOperationException("Empty artwork: "+texture.name);
            return new Rect(left,bottom,right-left+1,top-bottom+1);
        }
        static Sprite Save(string path,Texture2D texture,Rect rect,Vector2 pivot,float ppu=100)
        {
            var created=Sprite.Create(texture,rect,pivot,ppu,0,SpriteMeshType.FullRect);created.name=Path.GetFileNameWithoutExtension(path);
            var saved=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(saved){EditorUtility.CopySerialized(created,saved);UnityEngine.Object.DestroyImmediate(created);return saved;}
            AssetDatabase.CreateAsset(created,path);return created;
        }
        static Rect Cover(Rect source,float aspect,bool top=false)
        {
            if(source.width/source.height>aspect){float w=source.height*aspect;return new Rect(source.center.x-w/2,source.y,w,source.height);}
            float h=source.width/aspect;return new Rect(source.x,top?source.yMax-h:source.center.y-h/2,source.width,h);
        }
        static void Scale(SpriteRenderer r,Sprite sprite,float worldScale)
        {
            r.sprite=sprite;r.drawMode=SpriteDrawMode.Simple;r.color=Color.white;r.transform.rotation=Quaternion.identity;
            var parent=r.transform.parent?r.transform.parent.lossyScale:Vector3.one;
            r.transform.localScale=new Vector3(worldScale/parent.x,worldScale/parent.y,1);
        }
        static void WidthAtBottom(SpriteRenderer r,Sprite sprite,float width,Vector2 bottom)
        {
            float scale=width/sprite.bounds.size.x;Scale(r,sprite,scale);
            r.transform.position=new Vector3(bottom.x,bottom.y+sprite.bounds.size.y*scale*.5f,0);
        }
        static SpriteRenderer Renderer(Transform root,string name)
        {
            var t=root.Find(name);if(!t){t=new GameObject(name).transform;t.SetParent(root,false);}
            var r=t.GetComponent<SpriteRenderer>();return r?r:t.gameObject.AddComponent<SpriteRenderer>();
        }
        static void Wall(SpriteRenderer r,Sprite sprite,float width)
        {
            float scale=1.55f/sprite.bounds.size.y;Scale(r,sprite,scale);r.drawMode=SpriteDrawMode.Tiled;
            r.size=new Vector2(width/scale,1.55f/scale);r.transform.position=new Vector3(r.transform.position.x,.225f,0);
            var parent=r.transform.parent?r.transform.parent.lossyScale:Vector3.one;
            r.transform.localScale=new Vector3(scale/parent.x,scale/parent.y,1);
        }
        public static void RepairWalls()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();var root=shop.transform.Find("Reference shop layout");
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Sprites+"InteriorWallRepeat.asset");
            foreach(var r in root.GetComponentsInChildren<SpriteRenderer>(true))
                if(r.name.StartsWith("North wall")&&r.sprite.texture.name.StartsWith("Art_TTM_Env_")||r.name=="Storage south wall fade")
                {float width=r.bounds.size.x;Wall(r,sprite,width);EditorUtility.SetDirty(r);EditorUtility.SetDirty(r.transform);}
            EditorSceneManager.MarkSceneDirty(shop.gameObject.scene);EditorSceneManager.SaveScene(shop.gameObject.scene);
        }
        static Rect DoorHole(Texture2D texture,Rect full)
        {
            var pixels=texture.GetPixels32();int start=-1,bestStart=-1,bestWidth=0;
            int from=(int)(full.x+full.width*.6f),to=(int)(full.xMax-2);
            for(int x=from;x<=to;x++)
            {
                int clear=0;for(int n=0;n<12;n++){int y=(int)(full.y+full.height*(.08f+n*.045f));if(pixels[y*texture.width+x].a<16)clear++;}
                if(clear>=11){if(start<0)start=x;}
                else if(start>=0){if(x-start>bestWidth){bestStart=start;bestWidth=x-start;}start=-1;}
            }
            if(bestWidth<full.width*.02f)return new Rect(full.x+full.width*.729f,full.y,full.width*.171f,full.height*.83f);
            return new Rect(bestStart,full.y,bestWidth,full.height*.83f);
        }
        [MenuItem("Gokil Kisah/Apply New Depth Assets and Zack")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();var root=shop.transform.Find("Reference shop layout");
            Undo.RegisterFullObjectHierarchyUndo(shop.gameObject,"New depth artwork and character proportions");
            var before=shop.GetComponentsInChildren<SpriteRenderer>(true).ToDictionary(r=>r,r=>r.bounds);
            var art=new Dictionary<string,Sprite>();var textures=new Dictionary<string,Texture2D>();var audit=new List<string>();
            foreach(string name in Names)
            {
                var t=Read(Source+"Art_TTM_Env_"+name+".png",name.Contains("Wall")?8192:4096);textures[name]=t;
                art[name]=Save(Sprites+name+".asset",t,AlphaRect(t),Vector2.one*.5f);
                audit.Add(name+" "+t.width+"x"+t.height+" crop "+art[name].rect);
            }
            // Every fixture uses one scale in both axes; transparent export borders do not affect sizing.
            foreach(var shelf in shop.Shelves)
            {
                var r=shelf.transform.Find("Artist shelf fixture").GetComponent<SpriteRenderer>();var b=before[r];
                WidthAtBottom(r,art[shelf.name.StartsWith("Middle aisle")?"Shelf02":"Shelf01"],b.size.x,new Vector2(b.center.x,b.min.y));
                for(int i=0;i<shelf.stockSlots.Length;i++)if(shelf.stockSlots[i])
                {
                    var slot=shelf.stockSlots[i];var original=slot.sprite;var sizing=original?original:shelf.stockedSprite;
                    if(sizing){Scale(slot,sizing,.18f/sizing.bounds.size.y);slot.sprite=original;}
                    slot.transform.position=new Vector3(b.center.x+(i-2)*b.size.x/6,r.bounds.min.y+.2f,0);
                }
                EditorUtility.SetDirty(shelf);
            }
            for(int i=1;i<=4;i++)
            {
                var r=root.Find("Fridge "+i).GetComponent<SpriteRenderer>();var b=before[r];
                WidthAtBottom(r,art["Fridge01"],b.size.x,new Vector2(b.center.x,b.min.y));
                r.sortingOrder=Mathf.RoundToInt(-(b.min.y+.1f)*100);
            }
            var wallRect=art["IntWall01"].rect;wallRect.width*=.70f;
            var repeat=Save(Sprites+"InteriorWallRepeat.asset",textures["IntWall01"],wallRect,Vector2.one*.5f);
            foreach(var r in root.GetComponentsInChildren<SpriteRenderer>(true))if(r.name.StartsWith("North wall")||r.name=="Storage south wall fade")Wall(r,repeat,before[r].size.x);
            var door=root.Find("Storage door beside opening").GetComponent<SpriteRenderer>();
            float doorWidth=art["StorageDoor01"].bounds.size.x*1.5f/art["StorageDoor01"].bounds.size.y;
            WidthAtBottom(door,art["StorageDoor01"],doorWidth,new Vector2(shop.storageDoor.position.x-.7f,-.55f));
            var room=shop.storageArea.bounds;
            var storageSprite=Save(Sprites+"StorageRoomFit.asset",textures["StorageRoom01"],Cover(art["StorageRoom01"].rect,room.size.x/room.size.y,true),Vector2.one*.5f);
            var storage=root.Find("Storage floor").GetComponent<SpriteRenderer>();Scale(storage,storageSprite,room.size.x/storageSprite.bounds.size.x);storage.transform.position=room.center;
            var bounds=shop.walkableArea.bounds;
            var floorSprite=Save(Sprites+"DepthShopFloor.asset",textures["Floor01"],Cover(art["Floor01"].rect,bounds.size.x/bounds.size.y),Vector2.one*.5f);
            var floor=root.Find("Mart floor").GetComponent<SpriteRenderer>();Scale(floor,floorSprite,bounds.size.x/floorSprite.bounds.size.x);floor.transform.position=bounds.center;
            var facade=Renderer(root,"Facade west");
            float width=bounds.size.x+.4f,bottom=bounds.min.y-.25f;
            WidthAtBottom(facade,art["ExtWall01"],width,new Vector2(bounds.center.x,bottom));facade.enabled=true;
            foreach(string name in new[]{"Facade east","Front facade"}){var t=root.Find(name);if(t)t.GetComponent<SpriteRenderer>().enabled=false;}
            var hole=DoorHole(textures["ExtWall01"],art["ExtWall01"].rect);float unit=width/art["ExtWall01"].rect.width;
            float entranceX=facade.bounds.min.x+(hole.center.x-art["ExtWall01"].rect.x)*unit;
            shop.customerDoor.position=new Vector3(entranceX,shop.customerDoor.position.y,0);shop.customerEntryTarget.position=new Vector3(entranceX,shop.customerEntryTarget.position.y,0);
            var entrance=root.Find("Artist entrance door");if(!entrance){entrance=new GameObject("Artist entrance door").transform;entrance.SetParent(root,false);}
            var left=Renderer(entrance,"Entrance left leaf");var right=Renderer(entrance,"Entrance right leaf");
            float leafWidth=hole.width*unit*.5f-.015f;
            WidthAtBottom(left,art["EntranceDoor01"],leafWidth,new Vector2(entranceX-leafWidth*.5f,bottom+.025f));
            WidthAtBottom(right,art["EntranceDoor01"],leafWidth,new Vector2(entranceX+leafWidth*.5f,bottom+.025f));
            left.sortingOrder=facade.sortingOrder+1;right.sortingOrder=facade.sortingOrder+1;
            var sliding=entrance.GetComponent<EntranceDoorPresentation>();if(!sliding)sliding=entrance.gameObject.AddComponent<EntranceDoorPresentation>();
            sliding.layout=shop;sliding.leftLeaf=left.transform;sliding.rightLeaf=right.transform;sliding.travel=leafWidth*.88f;
            // Both supplied keeper drawings share a normalized foot pivot and intrinsic height.
            foreach(string name in new[]{"Sam","Zack"})
            {
                var t=Read("Assets/GokilKisah/Resources/CharacterArt/Art_TTM_Char_"+name+"01.png",2048);var rect=AlphaRect(t);
                Save("Assets/GokilKisah/Resources/PresentationSprites/"+name+"Static.asset",t,rect,new Vector2(.5f,0),rect.height/1.88f);
            }
            foreach(var actor in shop.GetComponentsInChildren<CharacterSpriteAnimator>(true))
            {
                string skin=actor.smoothSkin=="Keeper"?(actor==shop.playerCharacter?"Sam":"Zack"):actor.smoothSkin;
                var sprite=SmoothArt.Load(skin+"_3_0");if(!sprite)continue;
                Scale(actor.artwork,sprite,(skin=="Sam"||skin=="Zack"?1.05f:.95f)/sprite.bounds.size.y);
                actor.artwork.transform.localPosition=Vector3.zero;
            }
            Physics2D.SyncTransforms();
            foreach(var component in shop.GetComponentsInChildren<Component>(true))if(component)EditorUtility.SetDirty(component);
            EditorSceneManager.MarkSceneDirty(shop.gameObject.scene);EditorSceneManager.SaveScene(shop.gameObject.scene);AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Verification/DepthAssets");
            audit.Add("Sam and Zack: 1.05 world units tall; one uniform scale, foot pivot at ground.");audit.Add("Entrance aligned to authored facade opening at x="+entranceX);
            File.WriteAllLines("Verification/DepthAssets/import.txt",audit);
            foreach(var t in textures.Values){var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(t));importer.isReadable=false;importer.SaveAndReimport();}
        }
    }
}
