using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace GokilKisah.Editor
{
    public static class ShiftSceneUpgrade
    {
        static Sprite Art(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GokilKisah/Resources/SmoothArt/"+name+".png");
        static void Replace(SpriteRenderer renderer,string name,Vector2 size)
        {
            var sprite=Art(name);if(!sprite)throw new InvalidOperationException("Missing smooth sprite: "+name);
            renderer.sprite=sprite;
            Vector3 parentScale=renderer.transform.parent?renderer.transform.parent.lossyScale:Vector3.one;
            renderer.transform.localScale=new Vector3(size.x/sprite.bounds.size.x/parentScale.x,size.y/sprite.bounds.size.y/parentScale.y,1);
        }
        [MenuItem("Gokil Kisah/Apply Larger Smooth Store Upgrade")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/GokilKisah/Resources/SmoothArt"}))
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=256;
                importer.filterMode=FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.SaveAndReimport();
            }
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();
            if(!shop)throw new InvalidOperationException("Open TokMartPitch first.");
            Undo.RegisterFullObjectHierarchyUndo(shop.gameObject,"Larger smooth store");
            bool expanding=shop.schemaVersion<5;
            if(expanding)
            {
                var transforms=shop.GetComponentsInChildren<Transform>(true);
                var positions=new Dictionary<Transform,Vector3>();foreach(var t in transforms)positions[t]=t.position;
                foreach(var t in transforms)if(t!=shop.transform){var p=positions[t];t.position=new Vector3(3.2f+(p.x-3.2f)*1.65f,-1.87f+(p.y+1.87f)*1.8f,p.z);}
                foreach(var collider in shop.GetComponentsInChildren<BoxCollider2D>(true))collider.size=Vector2.Scale(collider.size,new Vector2(1.65f,1.8f));
                shop.schemaVersion=5;
                // Roomier aisles retain familiar routes while adding a third column of shelves.
                var originals=shop.Shelves;
                foreach(var original in originals)
                {
                    if(original.transform.position.x>11)continue;
                    var shelf=UnityEngine.Object.Instantiate(original,original.transform.parent);
                    shelf.name="Middle aisle "+original.name;shelf.transform.position+=new Vector3(5.1f,0,0);shelf.initialStock=3;
                }
            }
            foreach(var renderer in shop.GetComponentsInChildren<SpriteRenderer>(true))
            {
                Vector2 size=renderer.bounds.size;string name=renderer.name;string art="Crate";
                var actor=renderer.GetComponentInParent<CharacterSpriteAnimator>(true);
                if(actor&&renderer==actor.artwork)
                {
                    actor.smoothSkin=actor==shop.playerCharacter||actor==shop.partnerCharacter?"Keeper":actor==shop.rayCharacter?"Ray":actor==shop.salmahCharacter?"Salmah":actor==shop.goblinTemplate?"Goblin":actor==shop.idolTemplate?"Idol":"Customer";
                    art=(actor==shop.partnerCharacter?"Zack":actor.smoothSkin=="Keeper"?"Sam":actor.smoothSkin)+"_3_0";
                    Replace(renderer,art,new Vector2(.70f,.86f));renderer.transform.position=actor.transform.position+new Vector3(0,.36f,0);EditorUtility.SetDirty(actor);continue;
                }
                if(actor){art=name.Contains("gun")?"Gun":"Crate";Replace(renderer,art,new Vector2(.30f,.28f));continue;}
                var station=renderer.GetComponentInParent<ShopStation>(true);
                if(station)art=station.stationType==ShopStation.StationType.Checkout?"Checkout":station.stationType==ShopStation.StationType.Phone?"Phone":station.stationType==ShopStation.StationType.NailGun?"Gun":"Crate";
                else if(name.StartsWith("Floor "))
                {
                    art="Floor";var coordinates=name.Substring(6).Split(',');
                    float x=float.Parse(coordinates[0]),y=float.Parse(coordinates[1]);
                    size=new Vector2(Mathf.Min(48,1355-x)/100*1.65f,Mathf.Min(48,735-y)/100*1.8f);
                }
                else if(name.Contains("wall")||name.Contains("Wall")){art="Wall";size=name.Contains("Back")?new Vector2(.48f*1.65f,.36f*1.8f):new Vector2(.26f*1.65f,.48f*1.8f);}
                else if(name=="Street")
                {
                    art="Street";var center=ShopLayout.ToGame(renderer.transform.position);
                    float x=320+(center.x-320)/1.65f,y=187+(center.y-187)/1.8f;
                    size=new Vector2((x>1534?21:48)/100f*1.65f,(y>704?39:48)/100f*1.8f);
                }
                else if(renderer.GetComponentInParent<ShelfInventory>(true)||name.StartsWith("Stock "))art="ShelfStock";
                else if(renderer.GetComponent<SpillHazard>()||name.Contains("hazard")||name.Contains("Spill")){art="Spill";size=new Vector2(.82f,.4f);}
                else if(name.Contains("fixture"))art="Lamp";
                else if(renderer==shop.lightSwitch)art="Switch";
                else if(renderer==shop.counterCan)art="Can";
                else if(name.StartsWith("Car"))art="Car";
                else if(name=="Bo")art="Ray_3_0";
                else if(name.Contains("entrance")||name.Contains("Entrance"))art="Wall";
                Replace(renderer,art,size);
            }
            foreach(var shelf in shop.Shelves)
            {
                shelf.stockedSprite=Art("ShelfStock");shelf.emptySprite=Art("ShelfEmpty");
                foreach(var slot in shelf.stockSlots)Replace(slot,"ShelfStock",new Vector2(.685f,.80f));
                shelf.ShowStock(shelf.initialStock==0?1:0);shelf.ShowStock(shelf.initialStock);EditorUtility.SetDirty(shelf);
            }
            foreach(var renderer in shop.register.GetComponentsInChildren<SpriteRenderer>())
            {if(renderer.name=="Register")renderer.gameObject.SetActive(false);else Replace(renderer,"Checkout",new Vector2(.95f,.95f));}
            // Keep encounter targets close enough to reach from the larger floor.
            Physics2D.SyncTransforms();var floor=shop.walkableArea.bounds;
            foreach(var marker in shop.switchLocations){var p=marker.position;p.x=Mathf.Clamp(p.x,floor.min.x+.35f,floor.max.x-.35f);p.y=Mathf.Clamp(p.y,floor.min.y+.35f,floor.max.y-.35f);marker.position=p;}
            shop.frameCameraToInterface=true;shop.worldCamera.orthographicSize=2.35f;shop.worldCamera.transform.position=shop.playerCharacter.transform.position+new Vector3(0,.22f,-10);
            EditorUtility.SetDirty(shop);EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
            Debug.Log("Tok Mart: larger store, smooth art and follow camera applied.");
        }
    }
}
