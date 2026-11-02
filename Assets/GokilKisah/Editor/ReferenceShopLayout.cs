using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GokilKisah.Editor
{
    public static class ReferenceShopLayout
    {
        const string ArtRoot="Assets/GokilKisah/SceneSprites/TokTokMart/";
        static Sprite Art(string name)=>AssetDatabase.LoadAssetAtPath<Sprite>(ArtRoot+name+".asset");
        static Transform Group(Transform parent,string name)
        {var t=parent.Find(name);if(!t){t=new GameObject(name).transform;t.SetParent(parent,false);}t.gameObject.SetActive(true);return t;}
        static Sprite Flat()
        {
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(ArtRoot+"LayoutFlat.asset");if(sprite)return sprite;
            var texture=new Texture2D(4,4,TextureFormat.RGBA32,false);texture.SetPixels(Enumerable.Repeat(Color.white,16).ToArray());texture.Apply();
            AssetDatabase.CreateAsset(texture,ArtRoot+"LayoutFlatTexture.asset");
            sprite=Sprite.Create(texture,new Rect(0,0,4,4),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);AssetDatabase.CreateAsset(sprite,ArtRoot+"LayoutFlat.asset");return sprite;
        }
        static SpriteRenderer Visual(Transform root,string name,Sprite sprite,Vector2 center,Vector2 size,int depth,Color? color=null)
        {
            var t=Group(root,name);t.position=new Vector3(center.x,center.y,0);t.rotation=Quaternion.identity;
            t.localScale=Vector3.one;var r=t.GetComponent<SpriteRenderer>();if(!r)r=t.gameObject.AddComponent<SpriteRenderer>();
            r.enabled=true;r.drawMode=SpriteDrawMode.Simple;r.sprite=sprite;r.color=color??Color.white;r.sortingOrder=depth;
            var p=root.lossyScale;
            if(sprite==Flat())t.localScale=new Vector3(size.x/sprite.bounds.size.x/p.x,size.y/sprite.bounds.size.y/p.y,1);
            else {float scale=Mathf.Min(size.x/sprite.bounds.size.x,size.y/sprite.bounds.size.y);t.localScale=new Vector3(scale/p.x,scale/p.y,1);}
            return r;
        }
        static void Obstacle(Transform root,string name,Vector2 center,Vector2 size)
        {
            var t=Group(root,name);t.position=center;t.localScale=Vector3.one;
            var c=t.GetComponent<BoxCollider2D>();if(!c)c=t.gameObject.AddComponent<BoxCollider2D>();c.size=size;c.offset=Vector2.zero;c.isTrigger=false;
            var o=t.GetComponent<ShopObstacle>();if(!o)o=t.gameObject.AddComponent<ShopObstacle>();o.shapes=new Collider2D[]{c};
        }
        static void Marker(Transform t,Vector2 p){t.position=new Vector3(p.x,p.y,0);}
        static void Station(ShopStation station,Transform root,Vector2 p)
        {station.transform.SetParent(root,true);station.transform.position=p;station.transform.localScale=Vector3.one;station.gameObject.SetActive(true);if(station.approachPoint)station.approachPoint.position=p;}

        [MenuItem("Gokil Kisah/Apply Reference Shop Layout")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode before arranging the shop.");
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();if(!shop)throw new InvalidOperationException("Open TokMartPitch first.");
            if(!Art("Shelf01"))throw new InvalidOperationException("Import the supplied artist assets first.");
            Undo.RegisterFullObjectHierarchyUndo(shop.gameObject,"Match reference shop layout");
            var root=Group(shop.transform,"Reference shop layout");
            var inventories=shop.GetComponentsInChildren<ShelfInventory>(true).OrderBy(s=>s.name,StringComparer.Ordinal).ToArray();
            var green=inventories.Where(s=>!s.name.StartsWith("Middle aisle")).ToArray();
            var red=inventories.Where(s=>s.name.StartsWith("Middle aisle")).ToArray();
            if(green.Length!=6||red.Length!=3)throw new InvalidOperationException("Expected the original six shelves and three expansion shelves.");
            // Reparent functional objects before hiding the former structural groups.
            Station(shop.stockroom,root,new Vector2(16.3f,1.7f));
            Station(shop.register,root,new Vector2(18.35f,-5.9f));
            Station(shop.wallPhone,root,new Vector2(18.35f,-2.4f));
            Station(shop.nailGun,root,new Vector2(12.4f,-.95f));
            foreach(var shelf in inventories)shelf.transform.SetParent(root,true);
            foreach(var spill in shop.StartingSpills){spill.transform.SetParent(root,true);spill.transform.position=new Vector3(10,-9.3f,0);}
            foreach(string name in new[]{"Floor and entrance","Walls","Storage room","TokTokMart artist environment","Shelves and stations"})
            {var old=shop.transform.Find(name);if(old)old.gameObject.SetActive(false);}

            shop.walkableArea.transform.position=new Vector3(10,-6.375f,0);shop.walkableArea.size=new Vector2(18.8f,11.65f);shop.walkableArea.isTrigger=true;
            shop.storageArea.transform.SetParent(root,true);shop.storageArea.gameObject.SetActive(true);shop.storageArea.transform.position=new Vector3(16.8f,1.175f,0);shop.storageArea.size=new Vector2(5.2f,4.05f);shop.storageArea.isTrigger=true;
            shop.storageDoor.SetParent(root,true);Marker(shop.storageDoor,new Vector2(18.45f,-.55f));
            Visual(root,"Mart floor",Art("Floor01"),new Vector2(10,-5.65f),new Vector2(19.1f,10.4f),-1000);
            Visual(root,"Exterior walkway",Flat(),new Vector2(10,-11.5f),new Vector2(19.1f,1.3f),-1000,new Color(.36f,.70f,.59f));
            Visual(root,"Storage floor",Flat(),new Vector2(16.8f,1.175f),new Vector2(5.2f,4.05f),-1000,new Color(.76f,.78f,.78f));
            Visual(root,"Storage backdrop",Art("StorageRoom01"),new Vector2(16.8f,3.05f),new Vector2(5.2f,1.4f),-700);
            Visual(root,"Storage west edge",Flat(),new Vector2(14.2f,1.3f),new Vector2(.13f,3.8f),-650,new Color(.22f,.25f,.25f));
            Visual(root,"Storage east edge",Flat(),new Vector2(19.4f,1.3f),new Vector2(.13f,3.8f),-650,new Color(.22f,.25f,.25f));
            Visual(root,"Storage north edge",Flat(),new Vector2(16.8f,3.2f),new Vector2(5.2f,.13f),-650,new Color(.22f,.25f,.25f));
            Visual(root,"Storage door beside opening",Art("StorageDoor01"),new Vector2(19.05f,-.1f),new Vector2(.72f,1.15f),-650);
            Visual(root,"North wall frieze",Art("IntWall01"),new Vector2(9.05f,-.1f),new Vector2(17.3f,1.1f),-700);
            Visual(root,"North wall end",Flat(),new Vector2(19.3f,-.1f),new Vector2(.5f,1.1f),-700,new Color(.51f,.72f,.56f));
            Visual(root,"West wall",Flat(),new Vector2(.45f,-6.2f),new Vector2(.25f,12.4f),-650,new Color(.36f,.70f,.59f));
            Visual(root,"East wall",Flat(),new Vector2(19.55f,-6.2f),new Vector2(.25f,12.4f),-650,new Color(.36f,.70f,.59f));
            Visual(root,"Front facade",Art("ExtWall01"),new Vector2(10,-12.15f),new Vector2(19.1f,.78f),-650);
            Visual(root,"Outside pavement",Flat(),new Vector2(10,-12.9f),new Vector2(21,1.4f),-1100,new Color(.73f,.75f,.74f));
            Obstacle(root,"North partition west",new Vector2(9.15f,-.55f),new Vector2(17.1f,.16f));
            Obstacle(root,"North partition east",new Vector2(19.25f,-.55f),new Vector2(.3f,.16f));
            Obstacle(root,"Storage west partition",new Vector2(14.2f,1.325f),new Vector2(.16f,3.75f));
            Obstacle(root,"Storage north partition",new Vector2(16.8f,3.2f),new Vector2(5.2f,.16f));
            Obstacle(root,"Storage east partition",new Vector2(19.4f,1.325f),new Vector2(.16f,3.75f));
            // Shelves: three columns, two rows, plus the two red back-wall units.
            for(int i=0;i<6;i++)SetShelf(green[i],new Vector2(new[]{5f,10f,14.7f}[i%3],i<3?-4.3f:-7.45f),Art("Shelf01"),new Vector2(3.95f,1.8f),i%3==0?0:3);
            for(int i=0;i<2;i++)SetShelf(red[i],new Vector2(5.6f+i*4.7f,-1.65f),Art("Shelf02"),new Vector2(4.2f,2f),3);
            red[2].gameObject.SetActive(false);
            for(int i=0;i<4;i++)Visual(root,"Fridge "+(i+1),Art("Fridge01"),new Vector2(14.1f+i*.98f,-1.5f),new Vector2(.98f,1.5f),240);
            Obstacle(root,"Fridge feet",new Vector2(15.57f,-2.08f),new Vector2(3.9f,.26f));
            // The turquoise L counter follows the reference, with the till and staff behind it.
            var counter=new Color(.33f,.72f,.69f);
            Visual(root,"Counter long top",Flat(),new Vector2(18.45f,-5.3f),new Vector2(1.08f,3.5f),530,counter);
            Visual(root,"Counter return",Flat(),new Vector2(18.82f,-7.12f),new Vector2(1.8f,.85f),755,counter);
            Visual(root,"Counter front edge",Flat(),new Vector2(18.82f,-7.5f),new Vector2(1.8f,.12f),756,new Color(.16f,.48f,.47f));
            foreach(var c in shop.register.GetComponentsInChildren<BoxCollider2D>(true)){c.transform.position=new Vector3(18.45f,-5.3f,0);c.transform.localScale=Vector3.one;c.size=new Vector2(.8f,3.5f);c.offset=Vector2.zero;}
            Obstacle(root,"Counter return feet",new Vector2(18.82f,-7.3f),new Vector2(1.8f,.35f));
            foreach(var r in shop.register.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if(!r.gameObject.activeSelf)continue;
                r.transform.position=new Vector3(18.4f,-6.55f,0);var size=r.sprite.bounds.size;
                r.transform.localScale=new Vector3(.9f/size.x,.8f/size.y,1);
            }
            Marker(shop.register.approachPoint,new Vector2(17.3f,-5.9f));Marker(shop.queueFront,new Vector2(17.3f,-6.2f));
            Marker(shop.partnerCheckoutSpot,new Vector2(19.05f,-5.3f));shop.queueSpacing=new Vector2(0,-.43f);
            Marker(shop.customerDoor,new Vector2(14.8f,-11.65f));Marker(shop.customerEntryTarget,new Vector2(14.8f,-10.2f));
            Marker(shop.playerCharacter.transform,new Vector2(8,-9.3f));Marker(shop.partnerCharacter.transform,new Vector2(17.2f,-3.05f));
            Marker(shop.rayCharacter.transform,new Vector2(20.4f,-11.65f));Marker(shop.rayDestination,new Vector2(13.4f,-10.1f));
            Marker(shop.salmahSpawn,new Vector2(11.8f,-9.3f));
            var car=shop.boCarTarget.parent;car.position+=new Vector3(20.3f,-10.4f,0)-shop.boCarTarget.position;
            var street=shop.transform.Find("Street and Bo car");if(street)foreach(var r in street.GetComponentsInChildren<SpriteRenderer>(true))if(r.name=="Street")r.enabled=false;
            Vector2[] switchPoints={new Vector2(18.5f,1.5f),new Vector2(17.5f,2.4f),new Vector2(15.1f,1.0f),new Vector2(16.5f,2.0f)};
            Vector2[] canPoints={new Vector2(16.8f,-9.5f),new Vector2(11.5f,-10.2f),new Vector2(3,-6.1f)};
            for(int i=0;i<shop.switchLocations.Length;i++)Marker(shop.switchLocations[i],switchPoints[i%switchPoints.Length]);
            for(int i=0;i<shop.canLocations.Length;i++)Marker(shop.canLocations[i],canPoints[i%canPoints.Length]);
            for(int i=0;i<shop.goblinSpawns.Length;i++)Marker(shop.goblinSpawns[i],new Vector2(4+i*5,-6.25f));
            for(int i=0;i<shop.idolSpawns.Length;i++)Marker(shop.idolSpawns[i],new Vector2(4+i*5,-3.1f));
            if(shop.fluorescentLights!=null)for(int i=0;i<shop.fluorescentLights.Length;i++)Marker(shop.fluorescentLights[i].transform,new Vector2(4+i*4.4f,-.7f));
            shop.schemaVersion=7;shop.frameCameraToInterface=true;
            var controller=UnityEngine.Object.FindFirstObjectByType<StoreGameController>();controller.gameplayZoom=2.35f;EditorUtility.SetDirty(controller);
            foreach(var component in shop.GetComponentsInChildren<MonoBehaviour>(true))EditorUtility.SetDirty(component);
            Physics2D.SyncTransforms();EditorUtility.SetDirty(shop);EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
            Debug.Log("TokTokMart reference layout saved: six green shelves, two red shelves, rear fridges, right counter, top-right storage.");
        }
        static void SetShelf(ShelfInventory shelf,Vector2 center,Sprite sprite,Vector2 size,int initial)
        {
            shelf.gameObject.SetActive(true);shelf.transform.localScale=Vector3.one;
            float fit=Mathf.Min(size.x/sprite.bounds.size.x,size.y/sprite.bounds.size.y);
            size=(Vector2)sprite.bounds.size*fit;
            shelf.transform.position=new Vector3(center.x,center.y-size.y/2+.12f,0);
            var r=Visual(shelf.transform,"Artist shelf fixture",sprite,center,size,Mathf.RoundToInt(-shelf.transform.position.y*100));
            shelf.collisionBase.transform.position=shelf.transform.position;shelf.collisionBase.transform.localScale=Vector3.one;shelf.collisionBase.size=new Vector2(size.x-.18f,.26f);shelf.collisionBase.offset=Vector2.zero;shelf.collisionBase.isTrigger=false;
            var approach=(Vector2)shelf.transform.position+Vector2.down*.4f;
            Marker(shelf.shoppingPoint,approach);shelf.interactionZone.transform.position=approach;shelf.interactionZone.transform.localScale=Vector3.one;shelf.interactionZone.size=new Vector2(size.x,.65f);shelf.interactionZone.isTrigger=true;
            shelf.initialStock=initial;shelf.emptySprite=null;shelf.stockedSprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GokilKisah/Resources/OnlineArt/Crate.png");
            for(int i=0;i<shelf.stockSlots.Length;i++)
            {
                var slot=shelf.stockSlots[i];slot.transform.position=new Vector3(center.x+(i-2)*size.x/6,center.y-size.y/2+.28f,0);
                slot.transform.rotation=Quaternion.identity;slot.transform.localScale=new Vector3(.3f/shelf.stockedSprite.bounds.size.x,.24f/shelf.stockedSprite.bounds.size.y,1);slot.enabled=true;
            }
            var sorter=shelf.GetComponent<FootDepthSorter>();if(sorter){sorter.feet=shelf.transform;sorter.artwork=shelf.GetComponentsInChildren<SpriteRenderer>(true);}
            shelf.ShowStock(initial==0?1:0);shelf.ShowStock(initial);
        }
    }
}
