using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace GokilKisah.Editor
{
    public static class StorageRoomBuilder
    {
        static GameObject Group(Transform parent,string name,Vector2 position){var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.position=position;return go;}
        static SpriteRenderer Art(Transform parent,string name,string spriteName,Vector2 position,Vector2 size,int depth)
        {
            var go=Group(parent,name,position);var renderer=go.AddComponent<SpriteRenderer>();renderer.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GokilKisah/Resources/OnlineArt/"+spriteName+".png");
            if(!renderer.sprite)throw new InvalidOperationException("Missing online room artwork "+spriteName);
            go.transform.localScale=new Vector3(size.x/renderer.sprite.bounds.size.x,size.y/renderer.sprite.bounds.size.y,1);renderer.sortingOrder=depth;return renderer;
        }
        static void Wall(Transform root,string name,Vector2 center,Vector2 size)
        {
            var go=Group(root,name,center);var collider=go.AddComponent<BoxCollider2D>();collider.size=size;go.AddComponent<ShopObstacle>().shapes=new Collider2D[]{collider};
            Art(root,name+" art","Wall",center+Vector2.up*.18f,new Vector2(size.x+.12f,size.y+.30f),-700);
        }
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode to add storage.");
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();if(!shop)throw new InvalidOperationException("Open store scene first.");
            if(shop.storageArea)return;
            Undo.RegisterFullObjectHierarchyUndo(shop.gameObject,"Add walkable storage room");
            var root=Group(shop.transform,"Storage room",Vector2.zero).transform;
            var area=Group(root,"Walkable storage floor",new Vector2(1.05f,-8.95f));shop.storageArea=area.AddComponent<BoxCollider2D>();shop.storageArea.size=new Vector2(5.4f,4.3f);shop.storageArea.isTrigger=true;
            // Native floor tiles use the downloaded Kenney texture.
            for(int y=0;y<6;y++)for(int x=0;x<8;x++)Art(root,"Storage floor "+x+","+y,"StorageFloor",new Vector2(-1.65f+(x+.5f)*5.4f/8,-6.8f-(y+.5f)*4.3f/6),new Vector2(5.4f/8,4.3f/6),-1000);
            Wall(root,"Storage north wall",new Vector2(1.05f,-6.8f),new Vector2(5.4f,.13f));
            Wall(root,"Storage south wall",new Vector2(1.05f,-11.1f),new Vector2(5.4f,.13f));
            Wall(root,"Storage west wall",new Vector2(-1.65f,-8.95f),new Vector2(.13f,4.3f));
            Wall(root,"Storage east wall above door",new Vector2(3.42f,-7.7f),new Vector2(.14f,1.8f));
            Wall(root,"Storage east wall below door",new Vector2(3.42f,-10.45f),new Vector2(.14f,1.3f));
            shop.storageDoor=Group(root,"Storage doorway",new Vector2(3.42f,-9.2f)).transform;
            // Clear the visual wall tiles across the doorway while preserving the original scene art.
            foreach(var renderer in shop.GetComponentsInChildren<SpriteRenderer>())if(renderer.name=="Left wall"&&renderer.transform.position.y> -10.05f&&renderer.transform.position.y< -8.35f)renderer.gameObject.SetActive(false);
            Art(root,"Door threshold","Floor",new Vector2(3.45f,-9.2f),new Vector2(.55f,1.15f),-990);
            Art(root,"Dry-goods backstock","ShelfStock",new Vector2(-.9f,-7.65f),new Vector2(1.3f,1.15f),785);
            Art(root,"Drinks backstock","ShelfBags",new Vector2(1.1f,-7.65f),new Vector2(1.3f,1.15f),785);
            Wall(root,"Storage rack feet left",new Vector2(-.9f,-8.05f),new Vector2(1.10f,.24f));
            Wall(root,"Storage rack feet right",new Vector2(1.1f,-8.05f),new Vector2(1.10f,.24f));
            Art(root,"Fruit delivery","Fruit",new Vector2(-.90f,-10.4f),new Vector2(1.05f,.85f),1040);
            Art(root,"Bread delivery","Bread",new Vector2(1.05f,-10.4f),new Vector2(1.05f,.85f),1040);
            shop.stockroom.transform.SetParent(root,true);shop.stockroom.transform.position=new Vector3(.3f,-8.8f,0);shop.stockroom.name="Take stock baskets";
            foreach(var renderer in shop.stockroom.GetComponentsInChildren<SpriteRenderer>()){renderer.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GokilKisah/Resources/OnlineArt/Crate.png");renderer.transform.localPosition=new Vector3(0,.28f,0);renderer.transform.localScale=new Vector3(.45f,.38f,1);}
            if(shop.stockroom.approachPoint)shop.stockroom.approachPoint.position=shop.stockroom.transform.position;
            shop.schemaVersion=6;EditorUtility.SetDirty(shop);EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Debug.Log("Storage room with an open doorway and stock pickup saved.");
        }
    }
}
