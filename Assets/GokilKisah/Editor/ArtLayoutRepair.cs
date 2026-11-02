using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace GokilKisah.Editor
{
    public static class ArtLayoutRepair
    {
        const string ArtRoot="Assets/GokilKisah/SceneSprites/TokTokMart/";
        const string Frames="Assets/GokilKisah/Resources/PresentationSprites/";
        static Sprite Art(string n)=>AssetDatabase.LoadAssetAtPath<Sprite>(ArtRoot+n+".asset");
        static Sprite SaveSprite(string path,Texture2D texture,Rect rect,Vector2 pivot)
        {
            var created=Sprite.Create(texture,rect,pivot,100,0,SpriteMeshType.FullRect);created.name=Path.GetFileNameWithoutExtension(path);
            var saved=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(saved){EditorUtility.CopySerialized(created,saved);UnityEngine.Object.DestroyImmediate(created);return saved;}
            AssetDatabase.CreateAsset(created,path);return created;
        }
        static Rect AlphaRect(Texture2D texture)
        {
            var pixels=texture.GetPixels32();int left=texture.width,right=-1,bottom=texture.height,top=-1;
            for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)if(pixels[y*texture.width+x].a>8)
            {left=Math.Min(left,x);right=Math.Max(right,x);bottom=Math.Min(bottom,y);top=Math.Max(top,y);}
            return right<left?new Rect(0,0,texture.width,texture.height):new Rect(left,bottom,right-left+1,top-bottom+1);
        }
        static Texture2D Readable(string path,bool readable)
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            if(importer.isReadable!=readable){importer.isReadable=readable;importer.SaveAndReimport();}
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static void PrepareFrames()
        {
            Directory.CreateDirectory(Frames);AssetDatabase.Refresh();
            var skins=new[]{"Sam","Zack","Customer","Goblin","Idol","Ray","Salmah"};
            var required=skins.SelectMany(s=>Directory.GetFiles("Assets/GokilKisah/Resources/OnlineArt",s+"_*.png")).Concat(new[]{"Checkout","Crate","Fruit","Bread","Car","Gun","Phone","Switch","Can","Lamp"}.Select(s=>"Assets/GokilKisah/Resources/OnlineArt/"+s+".png").Where(File.Exists));
            if(required.All(p=>File.Exists(Frames+Path.GetFileNameWithoutExtension(p)+".asset")))return;
            foreach(string skin in new[]{"Sam","Zack","Customer","Goblin","Idol","Ray","Salmah"})
            {
                var paths=Directory.GetFiles("Assets/GokilKisah/Resources/OnlineArt",skin+"_*.png").OrderBy(p=>p).ToArray();
                var textures=new List<Texture2D>();Rect union=new Rect();bool first=true;
                foreach(string path in paths){var t=Readable(path,true);textures.Add(t);var r=AlphaRect(t);union=first?r:Rect.MinMaxRect(Mathf.Min(union.xMin,r.xMin),Mathf.Min(union.yMin,r.yMin),Mathf.Max(union.xMax,r.xMax),Mathf.Max(union.yMax,r.yMax));first=false;}
                for(int i=0;i<paths.Length;i++)SaveSprite(Frames+Path.GetFileNameWithoutExtension(paths[i])+".asset",textures[i],union,new Vector2(.5f,0));
                foreach(string path in paths)Readable(path,false);
            }
            foreach(string prop in new[]{"Checkout","Crate","Fruit","Bread","Car","Gun","Phone","Switch","Can","Lamp"})
            {
                string path="Assets/GokilKisah/Resources/OnlineArt/"+prop+".png";if(!File.Exists(path))continue;
                var texture=Readable(path,true);SaveSprite(Frames+prop+".asset",texture,AlphaRect(texture),new Vector2(.5f,.5f));Readable(path,false);
            }
            AssetDatabase.SaveAssets();
        }
        static void Fit(SpriteRenderer r,Sprite sprite,float width,float height)
        {
            r.sprite=sprite;r.transform.rotation=Quaternion.identity;var p=r.transform.parent?r.transform.parent.lossyScale:Vector3.one;
            float scale=Mathf.Min(width/sprite.bounds.size.x,height/sprite.bounds.size.y);r.transform.localScale=new Vector3(scale/p.x,scale/p.y,1);r.enabled=true;
        }
        static SpriteRenderer Panel(Transform root,string name,Sprite sprite,Vector2 center,Vector2 size,int depth,bool uniform=true)
        {
            var t=root.Find(name);if(!t){t=new GameObject(name).transform;t.SetParent(root,false);}t.position=center;t.localScale=Vector3.one;t.rotation=Quaternion.identity;
            var r=t.GetComponent<SpriteRenderer>();if(!r)r=t.gameObject.AddComponent<SpriteRenderer>();r.drawMode=SpriteDrawMode.Simple;r.color=Color.white;
            if(uniform)Fit(r,sprite,size.x,size.y);else {r.sprite=sprite;t.localScale=new Vector3(size.x/sprite.bounds.size.x,size.y/sprite.bounds.size.y,1);}
            r.sortingOrder=depth;return r;
        }
        static void FixedDepth(Transform root,int depth)
        {
            var sorter=root.GetComponent<FootDepthSorter>();if(sorter)sorter.automaticSorting=false;
            foreach(var r in root.GetComponentsInChildren<SpriteRenderer>(true))r.sortingOrder=depth;
        }
        static Sprite FloorTile()
        {
            string path=ArtRoot+"FloorRepeat.asset";var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(existing)return existing;
            var source=Art("Floor01");var texture=Readable(AssetDatabase.GetAssetPath(source.texture),true);
            // Find one authored grid period from vertical lines rather than stretching the full export.
            var pixels=texture.GetPixels32();int y=texture.height/3;var lines=new List<int>();bool inLine=false;
            for(int x=1;x<texture.width-1;x++)
            {
                var c=pixels[y*texture.width+x];bool dark=c.r<220&&c.g<210&&c.b<180;
                if(dark&&!inLine)lines.Add(x);inLine=dark;
            }
            var gaps=new List<int>();for(int i=1;i<lines.Count;i++)if(lines[i]-lines[i-1]>30)gaps.Add(lines[i]-lines[i-1]);
            int period=gaps.Count>0?gaps.OrderBy(v=>v).ElementAt(gaps.Count/2):texture.width/13;
            int start=lines.Count>1?lines[1]:0;int side=Math.Min(period*2,Math.Min(texture.width-start,texture.height));
            var sprite=SaveSprite(path,texture,new Rect(start,Mathf.Max(0,texture.height/2-side/2),side,side),new Vector2(.5f,.5f));
            Readable(AssetDatabase.GetAssetPath(source.texture),false);return sprite;
        }
        static void SortAtFeet(SpriteRenderer r,float footY,int offset=0)
        {
            r.sortingOrder=Mathf.RoundToInt(-footY*100)+offset;
            var sorter=r.GetComponent<FootDepthSorter>();if(sorter)sorter.automaticSorting=false;
        }
        [MenuItem("Gokil Kisah/Repair Art Proportions and Layout")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            PrepareFrames();ReferenceShopLayout.Apply();
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();var root=shop.transform.Find("Reference shop layout");
            var tile=FloorTile();var floor=root.Find("Mart floor").GetComponent<SpriteRenderer>();floor.sprite=tile;floor.drawMode=SpriteDrawMode.Tiled;
            float floorScale=3.2f/tile.bounds.size.x;floor.transform.localScale=new Vector3(floorScale,floorScale,1);floor.size=new Vector2(19.1f/floorScale,10.4f/floorScale);floor.transform.position=new Vector3(10,-5.65f,0);
            root.Find("Storage backdrop").gameObject.SetActive(false);
            var source=Art("StorageRoom01");var rect=source.rect;float cropH=rect.width/(5.2f/4.05f);
            var room=SaveSprite(ArtRoot+"StorageRoomFit.asset",source.texture,new Rect(rect.x,rect.yMax-cropH,rect.width,cropH),new Vector2(.5f,.5f));
            Panel(root,"Storage floor",room,new Vector2(16.8f,1.175f),new Vector2(5.2f,4.05f),-1000);
            // Repeat a clean wall section; the full export contains door cutouts that do not match this opening.
            var wallSource=Art("IntWall01");var wallRect=wallSource.rect;
            var wallRepeat=SaveSprite(ArtRoot+"InteriorWallRepeat.asset",wallSource.texture,new Rect(wallRect.x,wallRect.y,wallRect.width*.75f,wallRect.height),new Vector2(.5f,.5f));
            var frieze=root.Find("North wall frieze").GetComponent<SpriteRenderer>();frieze.sprite=wallRepeat;frieze.drawMode=SpriteDrawMode.Tiled;
            float wallScale=1.1f/wallRepeat.bounds.size.y;frieze.transform.localScale=new Vector3(wallScale,wallScale,1);frieze.size=new Vector2(17.3f/wallScale,1.1f/wallScale);
            frieze.transform.position=new Vector3(9.05f,0,0);
            var door=root.Find("Storage door beside opening").GetComponent<SpriteRenderer>();Fit(door,Art("StorageDoor01"),.72f,1.3f);door.transform.position=new Vector3(19.25f,-.05f,0);
            // Front facade is split around a real entrance, using proportional crops of the same export.
            var facade=root.Find("Front facade").GetComponent<SpriteRenderer>();facade.enabled=false;
            var wall=Art("ExtWall01");float worldHeight=1.65f,ppu=wall.rect.height/worldHeight;
            float doorX=14.8f,opening=1.65f,leftWidth=doorX-opening/2-.45f,rightWidth=19.55f-(doorX+opening/2);
            var left=SaveSprite(ArtRoot+"FacadeWest.asset",wall.texture,new Rect(wall.rect.x,wall.rect.y,leftWidth*ppu,wall.rect.height),new Vector2(.5f,.5f));
            var right=SaveSprite(ArtRoot+"FacadeEast.asset",wall.texture,new Rect(wall.rect.x,wall.rect.y,800,wall.rect.height),new Vector2(.5f,.5f));
            Panel(root,"Facade west",left,new Vector2(.45f+leftWidth/2,-12.05f),new Vector2(leftWidth,worldHeight),1205);
            var east=Panel(root,"Facade east",right,new Vector2(doorX+opening/2+rightWidth/2,-12.05f),new Vector2(rightWidth,worldHeight),1205);
            float facadeScale=worldHeight/right.bounds.size.y;east.transform.localScale=new Vector3(facadeScale,facadeScale,1);east.drawMode=SpriteDrawMode.Tiled;east.size=new Vector2(rightWidth/facadeScale,worldHeight/facadeScale);
            // Native table panels may stretch; artist sprites and characters always use uniform scale.
            var body=root.Find("Counter long top").GetComponent<SpriteRenderer>();body.transform.position=new Vector3(18.25f,-5.3f,0);body.transform.localScale=new Vector3(.85f/body.sprite.bounds.size.x,3.5f/body.sprite.bounds.size.y,1);SortAtFeet(body,-7.05f);
            var turn=root.Find("Counter return").GetComponent<SpriteRenderer>();turn.transform.position=new Vector3(18.65f,-7.12f,0);turn.transform.localScale=new Vector3(1.5f/turn.sprite.bounds.size.x,.85f/turn.sprite.bounds.size.y,1);SortAtFeet(turn,-7.55f);
            var edge=root.Find("Counter front edge").GetComponent<SpriteRenderer>();edge.transform.position=new Vector3(18.65f,-7.5f,0);edge.transform.localScale=new Vector3(1.5f/edge.sprite.bounds.size.x,.12f/edge.sprite.bounds.size.y,1);SortAtFeet(edge,-7.56f);
            foreach(var c in shop.register.GetComponentsInChildren<BoxCollider2D>(true)){c.transform.position=new Vector3(18.25f,-5.3f,0);c.size=new Vector2(.85f,3.5f);}
            var returnFeet=root.Find("Counter return feet");returnFeet.position=new Vector3(18.65f,-7.3f,0);returnFeet.GetComponent<BoxCollider2D>().size=new Vector2(1.5f,.5f);
            var till=AssetDatabase.LoadAssetAtPath<Sprite>(Frames+"Checkout.asset");
            foreach(var r in shop.register.GetComponentsInChildren<SpriteRenderer>(true))if(r.gameObject.activeSelf){Fit(r,till,.75f,.62f);r.transform.position=new Vector3(18.25f,-6.85f,0);}
            FixedDepth(shop.register.transform,770);
            for(int i=0;i<4;i++){var fridge=root.Find("Fridge "+(i+1)).GetComponent<SpriteRenderer>();SortAtFeet(fridge,fridge.bounds.min.y+.12f);}
            shop.partnerCheckoutSpot.position=new Vector3(19.05f,-5.3f,0);
            foreach(var actor in shop.GetComponentsInChildren<CharacterSpriteAnimator>(true))
            {
                string name=actor==shop.playerCharacter?"Sam":actor==shop.partnerCharacter?"Zack":actor.smoothSkin=="Keeper"?"Sam":actor.smoothSkin;
                var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Frames+name+"_3_0.asset");if(!sprite)continue;
                Fit(actor.artwork,sprite,.68f,1.15f);actor.artwork.transform.localPosition=Vector3.zero;
                if(actor.heldStock){var r=actor.heldStock.GetComponent<SpriteRenderer>();Fit(r,AssetDatabase.LoadAssetAtPath<Sprite>(Frames+"Crate.asset"),.3f,.25f);actor.heldStock.localPosition=new Vector3(-.23f,.30f,0);}
                if(actor.heldGun){var r=actor.heldGun.GetComponent<SpriteRenderer>();Fit(r,AssetDatabase.LoadAssetAtPath<Sprite>(Frames+"Gun.asset"),.4f,.2f);actor.heldGun.localPosition=new Vector3(.34f,.42f,0);}
                var actorSort=actor.GetComponent<FootDepthSorter>();if(actorSort){actorSort.artwork=actor.GetComponentsInChildren<SpriteRenderer>(true);actorSort.artworkOffsets=actorSort.artwork.Select(r=>r==actor.artwork?0:1).ToArray();}
                EditorUtility.SetDirty(actor);
            }
            foreach(var shelf in shop.Shelves)
            {
                var fixture=shelf.transform.Find("Artist shelf fixture").GetComponent<SpriteRenderer>();
                shelf.stockedSprite=AssetDatabase.LoadAssetAtPath<Sprite>(Frames+"Crate.asset");
                foreach(var slot in shelf.stockSlots){Fit(slot,shelf.stockedSprite,.28f,.18f);var p=slot.transform.position;p.y=fixture.bounds.min.y+.26f;slot.transform.position=p;}
                var sorter=shelf.GetComponent<FootDepthSorter>();if(sorter){sorter.artwork=shelf.GetComponentsInChildren<SpriteRenderer>(true);sorter.artworkOffsets=sorter.artwork.Select(r=>r==fixture?0:1).ToArray();sorter.orderOffset=0;}
                shelf.ShowStock(shelf.initialStock==0?1:0);shelf.ShowStock(shelf.initialStock);EditorUtility.SetDirty(shelf);
            }
            // Hide standby encounter actors in Edit Mode as well as during normal gameplay.
            shop.rayCharacter.gameObject.SetActive(false);shop.salmahCharacter.gameObject.SetActive(false);shop.lightSwitch.gameObject.SetActive(false);shop.counterCan.gameObject.SetActive(false);shop.boCarTarget.parent.gameObject.SetActive(false);
            foreach(var r in shop.stockroom.GetComponentsInChildren<SpriteRenderer>()){Fit(r,AssetDatabase.LoadAssetAtPath<Sprite>(Frames+"Crate.asset"),.6f,.5f);r.transform.localPosition=new Vector3(0,.3f,0);}
            foreach(var station in new[]{shop.wallPhone,shop.nailGun})foreach(var r in station.GetComponentsInChildren<SpriteRenderer>())
            {var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Frames+(station==shop.wallPhone?"Phone":"Gun")+".asset");if(sprite)Fit(r,sprite,.45f,.55f);r.transform.localPosition=new Vector3(0,.25f,0);}
            Fit(shop.lightSwitch,AssetDatabase.LoadAssetAtPath<Sprite>(Frames+"Switch.asset"),.35f,.35f);
            Fit(shop.counterCan,AssetDatabase.LoadAssetAtPath<Sprite>(Frames+"Can.asset"),.25f,.25f);
            shop.presentationBounds=new Bounds(new Vector3(10,-4.65f,0),new Vector3(20.3f,17.1f,0));
            shop.schemaVersion=8;Physics2D.SyncTransforms();
            foreach(var c in shop.GetComponentsInChildren<MonoBehaviour>(true))EditorUtility.SetDirty(c);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
            Debug.Log("TokTokMart art repaired: uniform sprite scales, foot pivots, clear staff lane, correct counter layering, and tiled floor.");
        }
    }
}
