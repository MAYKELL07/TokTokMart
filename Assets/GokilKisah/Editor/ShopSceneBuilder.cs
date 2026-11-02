using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace GokilKisah.Editor
{
    public static class ShopSceneBuilder
    {
        const string Folder="Assets/GokilKisah/SceneSprites";
        static Sprite Tile(string atlas,int x,int y,int w=1,int h=1)
        {
            string path=Folder+"/"+atlas+"_"+x+"_"+y+"_"+w+"_"+h+".asset";
            var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(sprite)return sprite;
            var texture=Resources.Load<Texture2D>("ArtistArt/"+atlas);
            sprite=Sprite.Create(texture,new Rect(x*17,texture.height-y*17-(h*17-1),w*17-1,h*17-1),new Vector2(.5f,.5f),100);
            AssetDatabase.CreateAsset(sprite,path);return sprite;
        }
        static Transform Group(Transform parent,string name)
        {var go=new GameObject(name);go.transform.SetParent(parent,false);return go.transform;}
        static Transform Marker(Transform parent,string name,Vector2 p)
        {var t=Group(parent,name);t.position=ShopLayout.ToWorld(p);return t;}
        static SpriteRenderer Place(Transform parent,string name,Sprite sprite,Rect area,int depth)
        {
            var root=Group(parent,name);root.position=new Vector3(area.center.x/100,-area.center.y/100,0);
            root.localScale=new Vector3(area.width/sprite.rect.width,area.height/sprite.rect.height,1);
            var renderer=root.gameObject.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=depth;return renderer;
        }
        static FootDepthSorter Sort(Transform root,Transform foot)
        {
            var s=root.GetComponent<FootDepthSorter>()??root.gameObject.AddComponent<FootDepthSorter>();
            s.feet=foot;s.artwork=root.GetComponentsInChildren<SpriteRenderer>();return s;
        }
        static void PivotAt(Transform root,Vector3 p)
        {
            var children=new List<Transform>();var positions=new List<Vector3>();
            foreach(Transform c in root){children.Add(c);positions.Add(c.position);}
            root.position=p;for(int i=0;i<children.Count;i++)children[i].position=positions[i];
        }
        static CharacterSpriteAnimator Character(Transform parent,string name,Vector2 foot,int row,SpriteRenderer existing=null)
        {
            var root=Marker(parent,name,foot);
            var art=existing?existing:Place(root,"Artwork",Tile("Urban",24,row),new Rect(foot.x-32,foot.y-58,64,64),(int)foot.y);
            art.transform.SetParent(root,true);art.name="Artwork";
            var view=root.gameObject.AddComponent<CharacterSpriteAnimator>();view.artwork=art;
            view.atlas=Resources.Load<Texture2D>("ArtistArt/Urban");view.skinRow=row;Sort(root,root);return view;
        }
        static ShopStation Station(Transform parent,string artName,string name,Vector2 foot,ShopStation.StationType type)
        {
            var art=parent.Find(artName);var root=Marker(parent,name,foot);
            art.SetParent(root,true);art.name="Artwork";var station=root.gameObject.AddComponent<ShopStation>();
            station.stationType=type;station.approachPoint=Marker(root,"Approach point",foot);Sort(root,root);return station;
        }
        static BoxCollider2D Box(Transform root,string name,Vector2 point,Vector2 size)
        {var t=Marker(root,name,point);var box=t.gameObject.AddComponent<BoxCollider2D>();box.size=size;box.isTrigger=false;return box;}
        static void AddAccessories(ShopLayout layout)
        {
            if(layout.fluorescentLights==null||layout.fluorescentLights.Length==0)
            {
                var group=Group(layout.transform,"Fluorescent lights");layout.fluorescentLights=new SpriteRenderer[4];
                for(int i=0;i<4;i++)layout.fluorescentLights[i]=Place(group,"Ceiling fixture "+(i+1),Tile("Urban",1,11),new Rect(485+i*225,190,96,16),1000);
                EditorUtility.SetDirty(layout);
            }
            foreach(var character in new[]{layout.playerCharacter,layout.customerTemplate})
            {
                if(character.heldStock)continue;
                Vector2 p=ShopLayout.ToGame(character.transform.position);
                var held=Place(character.transform,"Carried stock",Tile("Urban",3,10),new Rect(p.x+10,p.y-30,27,27),(int)p.y);
                character.heldStock=held.transform;Sort(character.transform,character.transform);held.gameObject.SetActive(false);EditorUtility.SetDirty(character);
            }
            if(!layout.playerCharacter.heldGun)
            {
                Vector2 p=ShopLayout.ToGame(layout.playerCharacter.transform.position);
                var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Folder+"/NailGun.asset");
                var held=Place(layout.playerCharacter.transform,"Held nail gun",sprite,new Rect(p.x+13,p.y-28,34,13),(int)p.y);
                layout.playerCharacter.heldGun=held.transform;
                Sort(layout.playerCharacter.transform,layout.playerCharacter.transform);held.gameObject.SetActive(false);EditorUtility.SetDirty(layout.playerCharacter);
            }
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        [MenuItem("Gokil Kisah/Upgrade Scene to Editable Gameplay")]
        public static void Populate()
        {
            if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play Mode before upgrading.");
            var layout=Object.FindFirstObjectByType<ShopLayout>();
            if(!layout)throw new System.InvalidOperationException("Open the existing pitch scene before upgrading.");
            var controller=Object.FindFirstObjectByType<StoreGameController>();
            if(controller){controller.name="Store Game Controller";controller.shop=layout;EditorUtility.SetDirty(controller);}
            if(layout.schemaVersion==2)
            {
                Physics2D.SyncTransforms();
                foreach(var shelf in layout.Shelves)
                {
                    var bounds=shelf.collisionBase.bounds;
                    var target=new Vector3(bounds.center.x,bounds.min.y-.22f,0);
                    shelf.shoppingPoint.position=target;shelf.interactionZone.transform.position=target;
                }
                layout.schemaVersion=3;EditorUtility.SetDirty(layout);
            }
            if(layout.schemaVersion==3)
            {
                foreach(var actor in new[]{layout.playerCharacter,layout.partnerCharacter})
                {
                    PivotAt(actor.transform,actor.artwork.transform.position+Vector3.down*.26f);
                    if(actor.heldStock)actor.heldStock.localPosition=new Vector3(.235f,.165f,0);
                    if(actor.heldGun)actor.heldGun.localPosition=new Vector3(.3f,.215f,0);
                }
                layout.schemaVersion=4;EditorUtility.SetDirty(layout);
            }
            if(layout.schemaVersion>=4){AddAccessories(layout);return;}
            var root=layout.transform;root.name="Shop";
            var fixtures=root.Find("Shelves and stations");var floor=root.Find("Floor and entrance");
            foreach(var r in floor.GetComponentsInChildren<SpriteRenderer>())r.sortingOrder=-1000;
            layout.walkableArea=Box(root,"Walkable floor area",new Vector2(836.5f,479),new Vector2(10.07f,5.12f));layout.walkableArea.isTrigger=true;
            int[] stock={0,4,2,4,0,3};
            for(int i=0;i<6;i++)
            {
                var shelf=fixtures.Find("Shelf "+(i+1));var box=shelf.GetComponentInChildren<BoxCollider2D>();box.isTrigger=true;
                Physics2D.SyncTransforms();var foot=new Vector3(box.bounds.center.x,box.bounds.min.y,0);PivotAt(shelf,foot);
                var inventory=shelf.gameObject.AddComponent<ShelfInventory>();inventory.collisionBase=box;inventory.initialStock=stock[i];
                inventory.stockSlots=shelf.GetComponentsInChildren<SpriteRenderer>();inventory.stockedSprite=inventory.stockSlots[0].sprite;inventory.emptySprite=Tile("Indoors",3,15);
                inventory.shoppingPoint=Marker(shelf,"Shopping point",ShopLayout.ToGame(foot)+Vector2.up*22);
                inventory.interactionZone=Box(shelf,"Restock interaction zone",ShopLayout.ToGame(inventory.shoppingPoint.position),new Vector2(box.size.x,.4f));inventory.interactionZone.isTrigger=true;
                shelf.gameObject.AddComponent<ShopObstacle>().shapes=new Collider2D[]{box};Sort(shelf,shelf);inventory.ShowStock(stock[i]);
            }
            layout.stockroom=Station(fixtures,"Stock crate","Stockroom",new Vector2(380,660),ShopStation.StationType.Stockroom);
            layout.register=Station(fixtures,"Checkout","Checkout station",new Vector2(1230,430),ShopStation.StationType.Checkout);
            fixtures.Find("Register").SetParent(layout.register.transform,true);
            var registerBox=Box(layout.register.transform,"Feet collision",new Vector2(1230,429),new Vector2(.56f,.3f));
            layout.register.gameObject.AddComponent<ShopObstacle>().shapes=new Collider2D[]{registerBox};
            layout.register.approachPoint.position=ShopLayout.ToWorld(new Vector2(1165,430));
            Sort(layout.register.transform,Marker(layout.register.transform,"Sort feet",new Vector2(1230,444)));
            layout.wallPhone=Station(fixtures,"Wall phone","Wall phone station",new Vector2(360,290),ShopStation.StationType.Phone);
            var gun=Marker(fixtures,"Nail gun station",new Vector2(1210,215));var tex=Resources.Load<Texture2D>("ArtistArt/NailGun");
            var gunSprite=AssetDatabase.LoadAssetAtPath<Sprite>(Folder+"/NailGun.asset");
            if(!gunSprite){gunSprite=Sprite.Create(tex,new Rect(0,0,tex.width,tex.height),new Vector2(.5f,.5f),100);AssetDatabase.CreateAsset(gunSprite,Folder+"/NailGun.asset");}
            Place(gun,"Artwork",gunSprite,new Rect(1180,204,60,23),215);layout.nailGun=gun.gameObject.AddComponent<ShopStation>();layout.nailGun.stationType=ShopStation.StationType.NailGun;
            var backstock=Group(fixtures,"Backstock");
            for(int j=0;j<3;j++)Place(backstock,"Stock "+j,Tile("Indoors",5+j%2,15),new Rect(381+j*48,208,48,64),272);
            var backBox=Box(backstock,"Feet collision",new Vector2(450,260),new Vector2(1.48f,.24f));backstock.gameObject.AddComponent<ShopObstacle>().shapes=new Collider2D[]{backBox};
            var people=root.Find("Character starting positions");var oldPeople=people.GetComponentsInChildren<SpriteRenderer>();
            Vector2 playerFoot=ShopLayout.ToGame(oldPeople[0].transform.position)+Vector2.up*26;
            Vector2 helperFoot=ShopLayout.ToGame(oldPeople[1].transform.position)+Vector2.up*26;
            layout.playerCharacter=Character(people,"Player",playerFoot,0,oldPeople[0]);layout.partnerCharacter=Character(people,"Partner",helperFoot,6,oldPeople[1]);
            var routes=Group(root,"Customer routes");layout.customerDoor=Marker(routes,"Door",new Vector2(790,710));
            layout.customerEntryTarget=Marker(routes,"Inside entrance",new Vector2(790,653));
            layout.queueFront=Marker(layout.register.transform,"Queue front",new Vector2(1268,487));
            layout.partnerCheckoutSpot=Marker(layout.register.transform,"Partner checkout spot",new Vector2(1230,355));
            var hazardArt=floor.Find("Initial mop hazard").GetComponent<SpriteRenderer>();hazardArt.sortingOrder=-900;
            var hazard=hazardArt.gameObject.AddComponent<SpillHazard>();hazard.artwork=hazardArt;
            var outside=Group(root,"Street and Bo car");
            for(int y=224;y<743;y+=48)for(int x=1390;x<1555;x+=48)Place(outside,"Street",Tile("Urban",9,4),new Rect(x,y,Mathf.Min(48,1555-x),Mathf.Min(48,743-y)),-1000);
            var car=Group(outside,"Bo car");Place(car,"Car top",Tile("Urban",16,14),new Rect(1413,451,64,64),579);
            Place(car,"Car bottom",Tile("Urban",16,15),new Rect(1413,515,64,64),579);Place(car,"Bo",Tile("Urban",24,15),new Rect(1491,509,45,55),579);
            layout.boCarTarget=Marker(car,"Can target",new Vector2(1420,570));
            var locations=Group(root,"Encounter locations");layout.salmahSpawn=Marker(locations,"Salmah start",new Vector2(1080,575));
            layout.rayDestination=Marker(locations,"Ray pickup point",new Vector2(470,700));
            Vector2[] switches={new Vector2(350,450),new Vector2(1080,190),new Vector2(1300,660),new Vector2(730,190)};
            Vector2[] cans={new Vector2(425,495),new Vector2(810,670),new Vector2(1030,245)};
            layout.switchLocations=new Transform[switches.Length];layout.canLocations=new Transform[cans.Length];
            for(int i=0;i<switches.Length;i++)layout.switchLocations[i]=Marker(locations,"Switch location "+(i+1),switches[i]);
            for(int i=0;i<cans.Length;i++)layout.canLocations[i]=Marker(locations,"Can location "+(i+1),cans[i]);
            layout.goblinSpawns=new Transform[3];layout.idolSpawns=new Transform[3];
            for(int i=0;i<3;i++){layout.goblinSpawns[i]=Marker(locations,"Goblin start "+(i+1),new Vector2(433+i*315,420));layout.idolSpawns[i]=Marker(locations,"Idol start "+(i+1),new Vector2(440+i*310,385));}
            layout.lightSwitch=Place(locations,"Light switch",Tile("Urban",13,12),new Rect(322,420,56,56),450);
            layout.counterCan=Place(locations,"Counter can",Tile("Urban",10,9),new Rect(397,465,56,56),495);
            layout.rayCharacter=Character(locations,"Uncle Ray",new Vector2(1490,700),12);layout.salmahCharacter=Character(locations,"Miss Salmah",new Vector2(1080,575),3);
            var templates=Group(root,"Spawn templates");layout.customerTemplate=Character(templates,"Customer template",Vector2.zero,9);
            layout.goblinTemplate=Character(templates,"Goblin template",Vector2.zero,0);layout.goblinTemplate.animate=false;layout.goblinTemplate.artwork.sprite=Tile("Characters",0,3);
            layout.idolTemplate=Character(templates,"Idol template",Vector2.zero,0);layout.idolTemplate.animate=false;layout.idolTemplate.artwork.sprite=Tile("Characters",0,11);
            var spill=Object.Instantiate(hazard.gameObject,templates);spill.name="Spill template";layout.spillTemplate=spill.GetComponent<SpillHazard>();templates.gameObject.SetActive(false);
            layout.worldCamera=Object.FindFirstObjectByType<Camera>();layout.worldCamera.name="Shop Camera";
            layout.worldCamera.transform.position=new Vector3(9.364815f,-4.627407f,-10);layout.worldCamera.orthographic=true;layout.worldCamera.orthographicSize=2.953704f;
            layout.schemaVersion=4;AddAccessories(layout);EditorUtility.SetDirty(layout);EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            Selection.activeGameObject=layout.gameObject;
            if(SceneView.lastActiveSceneView){SceneView.lastActiveSceneView.in2DMode=true;SceneView.lastActiveSceneView.FrameSelected();}
        }
    }
}

