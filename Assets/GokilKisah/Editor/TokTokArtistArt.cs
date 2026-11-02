using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GokilKisah.Editor
{
    // Source PNGs remain untouched. Cropped Sprite assets remove only transparent export margins.
    public static class TokTokArtistArt
    {
        const string Source = "Assets/GokilKisah/ArtSources/TokTokMart/";
        const string Sprites = "Assets/GokilKisah/SceneSprites/TokTokMart/";
        static readonly string[] Names = {"ExtWall01", "Floor01", "Fridge01", "IntWall01", "Shelf01", "Shelf02", "StorageDoor01", "StorageRoom01"};
        static string PathFor(string name) => Source + "Art_TTM_Env_" + name + ".png";

        static Sprite Import(string name)
        {
            string path = PathFor(name);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!importer) throw new InvalidOperationException("Missing artist source: " + path);
            var cached=AssetDatabase.LoadAssetAtPath<Sprite>(Sprites+name+".asset");
            if(cached && !importer.isReadable && importer.npotScale==TextureImporterNPOTScale.None && importer.filterMode==FilterMode.Bilinear)return cached;
            importer.textureType = TextureImporterType.Default;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = name.Contains("Wall") ? 8192 : 4096;
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = true;
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            var pixels = texture.GetPixels32();
            int left=texture.width, right=-1, bottom=texture.height, top=-1;
            for(int y=0; y<texture.height; y++) for(int x=0; x<texture.width; x++)
                if(pixels[y*texture.width+x].a>8) {left=Math.Min(left,x);right=Math.Max(right,x);bottom=Math.Min(bottom,y);top=Math.Max(top,y);}
            if(right<left) throw new InvalidOperationException("Artist source is transparent: " + name);
            var sprite = Sprite.Create(texture, new Rect(left,bottom,right-left+1,top-bottom+1), new Vector2(.5f,.5f), 100, 0, SpriteMeshType.FullRect);
            sprite.name = "Art_TTM_Env_" + name;
            string output = Sprites + name + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(output);
            if(existing) {EditorUtility.CopySerialized(sprite,existing); UnityEngine.Object.DestroyImmediate(sprite); sprite=existing;}
            else AssetDatabase.CreateAsset(sprite,output);
            importer.isReadable=false;
            importer.SaveAndReimport();
            return sprite;
        }

        static SpriteRenderer Place(Transform root, string name, Sprite sprite, Vector2 center, Vector2 size, int order)
        {
            var child=root.Find(name);
            if(!child) {child=new GameObject(name).transform;child.SetParent(root,false);}
            var renderer=child.GetComponent<SpriteRenderer>();
            if(!renderer)renderer=child.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite=sprite; renderer.color=Color.white; renderer.sortingOrder=order;
            child.position=new Vector3(center.x,center.y,0);
            var parentScale=root.lossyScale;
            child.localScale=new Vector3(size.x/sprite.bounds.size.x/parentScale.x,size.y/sprite.bounds.size.y/parentScale.y,1);
            return renderer;
        }
        static Bounds Combined(IEnumerable<SpriteRenderer> renderers)
        {
            var items=renderers.ToArray();
            if(items.Length==0)throw new InvalidOperationException("Expected existing scene art was not found.");
            var bounds=items[0].bounds;foreach(var r in items)bounds.Encapsulate(r.bounds);return bounds;
        }
        static void Replace(SpriteRenderer renderer, Sprite sprite)
        {
            var bounds=renderer.bounds;
            renderer.sprite=sprite;
            var scale=renderer.transform.parent?renderer.transform.parent.lossyScale:Vector3.one;
            renderer.transform.localScale=new Vector3(bounds.size.x/sprite.bounds.size.x/scale.x,bounds.size.y/sprite.bounds.size.y/scale.y,1);
            renderer.color=Color.white;
        }
        static void SideWall(Transform root,string name,Sprite sprite,Vector2 center,Vector2 size)
        {
            var renderer=Place(root,name,sprite,center,new Vector2(size.y,size.x),-700);
            renderer.transform.rotation=Quaternion.Euler(0,0,90);
        }
        static Sprite Threshold(Sprite floor)
        {
            string path=Sprites+"FloorThreshold.asset";
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(sprite)return sprite;
            sprite=Sprite.Create(floor.texture,new Rect(floor.rect.x+8,floor.rect.y+8,16,16),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
            AssetDatabase.CreateAsset(sprite,path);return sprite;
        }

        [MenuItem("Gokil Kisah/Apply TokTokMart Artist Assets")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode before applying artist assets.");
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();
            if(!shop || !shop.storageArea)throw new InvalidOperationException("Open the expanded TokMartPitch scene first.");
            Directory.CreateDirectory(Sprites);AssetDatabase.Refresh();
            var art=new Dictionary<string,Sprite>();foreach(var name in Names)art[name]=Import(name);
            Undo.RegisterFullObjectHierarchyUndo(shop.gameObject,"Apply TokTokMart artist environment");
            var visuals=shop.GetComponentsInChildren<SpriteRenderer>(true);
            var root=shop.transform.Find("TokTokMart artist environment");
            if(!root){root=new GameObject("TokTokMart artist environment").transform;root.SetParent(shop.transform,false);}

            // One full floor export preserves its authored tile pattern and avoids hundreds of seams.
            var floorTiles=visuals.Where(r=>r.name.StartsWith("Floor ")).ToArray();
            var floor=Combined(floorTiles);
            foreach(var r in floorTiles)r.enabled=false;
            Place(root,"Artist shop floor",art["Floor01"],floor.center,floor.size,-1000);
            foreach(var r in visuals.Where(r=>r.name.StartsWith("Storage floor ")||r.name=="Door threshold"))r.enabled=false;
            var storage=shop.storageArea.bounds;
            Place(root,"Artist storage floor",art["Floor01"],storage.center,storage.size,-1000);
            Place(root,"Open stockroom threshold",Threshold(art["Floor01"]),shop.storageDoor.position,new Vector2(.65f,1.2f),-990);

            var back=visuals.Where(r=>r.name.Contains("Back wall")).ToArray();
            var backBounds=Combined(back);
            foreach(var r in back)r.enabled=false;
            Place(root,"Artist interior wall",art["IntWall01"],backBounds.center,new Vector2(backBounds.size.x,backBounds.size.x*art["IntWall01"].rect.height/art["IntWall01"].rect.width),-700);
            var leftWalls=visuals.Where(r=>r.name=="Left wall").ToArray();
            var rightWalls=visuals.Where(r=>r.name=="Right wall").ToArray();
            var leftBounds=Combined(leftWalls);var rightBounds=Combined(rightWalls);
            foreach(var r in leftWalls.Concat(rightWalls))r.enabled=false;
            float doorwayTop=shop.storageDoor.position.y+.60f,doorwayBottom=shop.storageDoor.position.y-.60f;
            SideWall(root,"Artist west wall upper",art["IntWall01"],new Vector2(leftBounds.center.x,(leftBounds.max.y+doorwayTop)/2),new Vector2(leftBounds.size.x,leftBounds.max.y-doorwayTop));
            SideWall(root,"Artist west wall lower",art["IntWall01"],new Vector2(leftBounds.center.x,(leftBounds.min.y+doorwayBottom)/2),new Vector2(leftBounds.size.x,doorwayBottom-leftBounds.min.y));
            SideWall(root,"Artist east wall",art["IntWall01"],rightBounds.center,rightBounds.size);
            foreach(var r in visuals.Where(r=>r.name.Contains("entrance")||r.name.Contains("Entrance")))r.enabled=false;
            float south=floor.min.y;
            float doorX=shop.customerDoor.position.x;
            const float opening=1.7f;
            float leftWidth=doorX-opening/2-floor.min.x;
            float rightWidth=floor.max.x-(doorX+opening/2);
            Place(root,"Artist exterior wall west",art["ExtWall01"],new Vector2(floor.min.x+leftWidth/2,south),new Vector2(leftWidth,.62f),-700);
            Place(root,"Artist exterior wall east",art["ExtWall01"],new Vector2(doorX+opening/2+rightWidth/2,south),new Vector2(rightWidth,.62f),-700);

            int shelfIndex=0;
            foreach(var shelf in shop.Shelves)
            {
                // Old expansion duplicated the middle aisle directly over the right aisle.
                // Position the whole inventory root, including its triggers and collision feet.
                float targetX=shelf.name.StartsWith("Middle aisle")?11.14f:shelf.name=="Shelf 1"||shelf.name=="Shelf 3"||shelf.name=="Shelf 5"?6.14f:16.14f;
                var currentCenter=shelf.transform.Find("Artist shelf fixture");
                float centerX=currentCenter?currentCenter.GetComponent<SpriteRenderer>().bounds.center.x:Combined(shelf.stockSlots.Where(r=>r)).center.x;
                shelf.transform.position+=new Vector3(targetX-centerX,0,0);
                var existingFixture=shelf.transform.Find("Artist shelf fixture");
                var oldBounds=existingFixture?existingFixture.GetComponent<SpriteRenderer>().bounds:Combined(shelf.stockSlots.Where(r=>r));
                var fixture=Place(shelf.transform,"Artist shelf fixture",art[shelfIndex++%2==0?"Shelf01":"Shelf02"],oldBounds.center,new Vector2(oldBounds.size.x,1.35f),Mathf.RoundToInt(-shelf.transform.position.y*100));
                // Existing five slots now show products on the authored empty shelving.
                shelf.stockedSprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GokilKisah/Resources/OnlineArt/Crate.png");
                shelf.emptySprite=null;
                for(int i=0;i<shelf.stockSlots.Length;i++)
                {
                    var slot=shelf.stockSlots[i];slot.sprite=shelf.stockedSprite;slot.enabled=true;slot.color=Color.white;
                    slot.transform.position=new Vector3(oldBounds.center.x+(i-2)*oldBounds.size.x/6,fixture.bounds.min.y+.28f,0);
                    var parentScale=slot.transform.parent.lossyScale;
                    slot.transform.localScale=new Vector3(.30f/slot.sprite.bounds.size.x/parentScale.x,.24f/slot.sprite.bounds.size.y/parentScale.y,1);
                }
                var sorter=shelf.GetComponent<FootDepthSorter>();
                if(sorter)sorter.artwork=shelf.GetComponentsInChildren<SpriteRenderer>(true);
                shelf.ShowStock(shelf.initialStock==0?1:0);shelf.ShowStock(shelf.initialStock);EditorUtility.SetDirty(shelf);
            }
            // Preserve the existing backstock collision bases and navigation markers.
            foreach(var r in visuals.Where(r=>r.name=="Stock 0"||r.name=="Stock 1"||r.name=="Stock 2"))
            {
                bool alreadyApplied=r.sprite && r.sprite.texture==art["Fridge01"].texture;
                var oldCenter=r.bounds.center;Replace(r,art["Fridge01"]);
                float h=1.45f,w=h*art["Fridge01"].rect.width/art["Fridge01"].rect.height;
                var parentScale=r.transform.parent.lossyScale;
                r.transform.localScale=new Vector3(w/art["Fridge01"].bounds.size.x/parentScale.x,h/art["Fridge01"].bounds.size.y/parentScale.y,1);
                r.transform.position=new Vector3(oldCenter.x,oldCenter.y+(alreadyApplied?0:.30f),0);
            }
            foreach(var r in visuals.Where(r=>r.name=="Dry-goods backstock"||r.name=="Drinks backstock"))Replace(r,art[r.name.StartsWith("Dry")?"Shelf01":"Shelf02"]);
            foreach(var r in visuals.Where(r=>r.name=="Storage rack feet left art"||r.name=="Storage rack feet right art"))r.enabled=false;
            foreach(var r in visuals.Where(r=>r.name.StartsWith("Storage ")&&r.name.EndsWith("wall art")&&r.name!="Storage north wall art"||r.name=="Storage east wall above door art"||r.name=="Storage east wall below door art"))
            {
                if(r.name.StartsWith("Storage south")){Replace(r,art["IntWall01"]);continue;}
                var bounds=r.bounds;r.enabled=false;
                SideWall(root,"Artist "+r.name,art["IntWall01"],bounds.center,bounds.size);
            }
            foreach(var r in visuals.Where(r=>r.name=="Storage north wall art"))
            {
                r.enabled=false;
                Place(root,"Artist storage room wall",art["StorageRoom01"],new Vector2(storage.center.x,storage.max.y+.75f),new Vector2(storage.size.x,2.2f),-700);
            }
            // The open leaf sits above the walkable doorway, so the original route stays clear.
            Place(root,"Artist storage door open leaf",art["StorageDoor01"],(Vector2)shop.storageDoor.position+new Vector2(-.10f,1.12f),new Vector2(.65f,1.25f),-650);
            EditorUtility.SetDirty(shop);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
            Audit();
            Debug.Log("TokTokMart: all eight supplied environment assets applied; gameplay layout preserved.");
        }

        public static void Audit()
        {
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();
            var lines=new List<string>();
            foreach(var r in shop.GetComponentsInChildren<SpriteRenderer>(true))
                if(r.enabled&&r.sprite)lines.Add(r.name+" | "+AssetDatabase.GetAssetPath(r.sprite.texture)+" | "+r.bounds);
            Directory.CreateDirectory("Verification/ArtistArt");
            File.WriteAllLines("Verification/ArtistArt/scene-art-audit.txt",lines);
            foreach(var name in Names)if(!lines.Any(l=>l.Contains("Art_TTM_Env_"+name+".png")))throw new InvalidOperationException("Artist asset not used: "+name);
        }
    }
}
