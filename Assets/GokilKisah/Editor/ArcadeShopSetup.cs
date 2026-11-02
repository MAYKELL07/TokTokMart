using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace GokilKisah.Editor
{
    public static class ArcadeShopSetup
    {
        static float MapY(float y)=>y<-.55f?-.55f+(y+.55f)*.74f:y;
        static SpriteRenderer Flat(Transform parent,string name,Vector2 center,Vector2 size,Color color,int depth)
        {
            var child=parent.Find(name);if(!child){child=new GameObject(name).transform;child.SetParent(parent,false);}
            child.position=center;var r=child.GetComponent<SpriteRenderer>();if(!r)r=child.gameObject.AddComponent<SpriteRenderer>();
            r.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GokilKisah/SceneSprites/TokTokMart/LayoutFlat.asset");
            child.localScale=new Vector3(size.x/r.sprite.bounds.size.x,size.y/r.sprite.bounds.size.y,1);r.sortingOrder=depth;r.color=color;return r;
        }
        [MenuItem("Gokil Kisah/Apply Arcade Shop Redesign")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();
            Undo.RegisterFullObjectHierarchyUndo(shop.gameObject,"Arcade shop redesign");
            var rig=shop.GetComponent<ArcadeRoomCamera>();
            if(!rig)
            {
                var transforms=shop.GetComponentsInChildren<Transform>(true);var positions=new Vector3[transforms.Length];
                for(int i=0;i<transforms.Length;i++){positions[i]=transforms[i].position;positions[i].y=MapY(positions[i].y);}
                for(int i=0;i<transforms.Length;i++)if(transforms[i]!=shop.transform)transforms[i].position=positions[i];
                foreach(var box in shop.GetComponentsInChildren<BoxCollider2D>(true))if(box.transform.position.y<-.549f)
                {box.size=new Vector2(box.size.x,box.size.y*.74f);box.offset=new Vector2(box.offset.x,box.offset.y*.74f);}
                foreach(var r in shop.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    if(r.sortingOrder>100&&r.sortingOrder<2000)r.sortingOrder=Mathf.RoundToInt(-MapY(-r.sortingOrder/100f)*100);
                    if(r.GetComponentInParent<CharacterSpriteAnimator>())continue;
                    if(r.name=="Artist shelf fixture"||r.name.StartsWith("Fridge "))r.transform.localScale*=.83f;
                    else if(r.name.StartsWith("Counter ")||r.name=="West wall"||r.name=="East wall"||r.name=="Exterior walkway"||r.name=="Outside pavement")
                    {var scale=r.transform.localScale;scale.y*=.74f;r.transform.localScale=scale;}
                }
                foreach(var shelf in shop.Shelves)
                {
                    shelf.collisionBase.size=new Vector2(shelf.collisionBase.size.x*.83f,shelf.collisionBase.size.y);
                    foreach(var stock in shelf.stockSlots)if(stock)
                    {var p=stock.transform.position;p.x=shelf.transform.position.x+(p.x-shelf.transform.position.x)*.83f;stock.transform.position=p;stock.transform.localScale*=.83f;}
                }
                shop.queueSpacing=new Vector2(0,-.44f);
                rig=shop.gameObject.AddComponent<ArcadeRoomCamera>();
            }
            rig.layout=shop;
            var root=shop.transform.Find("Reference shop layout");
            var floor=root.Find("Mart floor").GetComponent<SpriteRenderer>();
            // Replace the oversized yellow grid with clean, small shop tiles.
            const string folder="Assets/GokilKisah/ArcadeArt";Directory.CreateDirectory(folder);
            const string texturePath=folder+"/ShopTile.png";
            var texture=new Texture2D(96,96,TextureFormat.RGBA32,false);
            for(int y=0;y<96;y++)for(int x=0;x<96;x++)
                texture.SetPixel(x,y,x<1||y<1?new Color(.64f,.73f,.68f):x<3||y<3?new Color(.84f,.89f,.80f):new Color(.91f,.94f,.85f));
            texture.Apply();File.WriteAllBytes(texturePath,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(texturePath,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(texturePath);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=96;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            var floorBounds=shop.walkableArea.bounds;
            floor.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);floor.drawMode=SpriteDrawMode.Tiled;floor.transform.localScale=Vector3.one;
            floor.transform.position=floorBounds.center;floor.size=new Vector2(floorBounds.size.x+.2f,floorBounds.size.y);floor.color=Color.white;
            var outerMin=new Vector2(floorBounds.min.x-.26f,MapY(-12.9f)-.25f);var outerMax=new Vector2(floorBounds.max.x+.26f,.65f);
            Flat(root,"Arcade shop plinth",(outerMin+outerMax)*.5f,new Vector2(outerMax.x-outerMin.x,outerMax.y-outerMin.y),new Color(.09f,.18f,.22f),-1080);
            var pavement=root.Find("Outside pavement");if(pavement)pavement.GetComponent<SpriteRenderer>().color=new Color(.12f,.28f,.31f);
            var walkway=root.Find("Exterior walkway");if(walkway)walkway.GetComponent<SpriteRenderer>().color=new Color(.64f,.76f,.69f);
            foreach(var child in new[]{root.Find("West wall"),root.Find("East wall")})if(child)child.GetComponent<SpriteRenderer>().color=new Color(.14f,.43f,.43f);
            var lighting=shop.transform.Find("Cozy shop lighting");
            if(lighting)foreach(Transform t in lighting)if(t.name.StartsWith("Warm ceiling")||t.name=="Checkout task light")
            {
                float row=t.name.EndsWith("-8")?-8:-3;if(t.name=="Checkout task light")row=-5.8f;
                var p=t.position;p.y=MapY(row);t.position=p;
            }
            const string ringPath=folder+"/PlayerRing.png";
            var ringTexture=new Texture2D(128,64,TextureFormat.RGBA32,false);
            for(int y=0;y<64;y++)for(int x=0;x<128;x++)
            {
                float radius=new Vector2((x+.5f-64)/60,(y+.5f-32)/27).magnitude;
                ringTexture.SetPixel(x,y,new Color(1,.82f,.31f,Mathf.Clamp01((.075f-Mathf.Abs(radius-.87f))*35)*.95f));
            }
            ringTexture.Apply();File.WriteAllBytes(ringPath,ringTexture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(ringTexture);
            AssetDatabase.ImportAsset(ringPath,ImportAssetOptions.ForceSynchronousImport);
            var ringImporter=(TextureImporter)AssetImporter.GetAtPath(ringPath);ringImporter.textureType=TextureImporterType.Sprite;ringImporter.spritePixelsPerUnit=160;
            ringImporter.alphaIsTransparency=true;ringImporter.mipmapEnabled=false;ringImporter.textureCompression=TextureImporterCompression.Uncompressed;ringImporter.SaveAndReimport();
            var ring=root.Find("Player floor marker");if(!ring){ring=new GameObject("Player floor marker").transform;ring.SetParent(root,false);}
            var ringRenderer=ring.GetComponent<SpriteRenderer>();if(!ringRenderer)ringRenderer=ring.gameObject.AddComponent<SpriteRenderer>();
            ringRenderer.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(ringPath);ringRenderer.sortingOrder=-810;ring.position=shop.playerCharacter.transform.position;rig.playerRing=ringRenderer;
            rig.shopFrame=new Bounds(new Vector3(10,(outerMin.y+.65f)/2,0),new Vector3(20.5f,.65f-outerMin.y,1));
            Physics2D.SyncTransforms();
            foreach(var component in shop.GetComponentsInChildren<Component>(true))if(component)EditorUtility.SetDirty(component);
            EditorSceneManager.MarkSceneDirty(shop.gameObject.scene);EditorSceneManager.SaveScene(shop.gameObject.scene);AssetDatabase.SaveAssets();
        }
    }
}
