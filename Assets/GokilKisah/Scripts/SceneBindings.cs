using System;
using System.Collections.Generic;
using UnityEngine;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        [Header("Editable shop scene")]
        public ShopLayout shop;
        ShelfInventory[] sceneShelves=Array.Empty<ShelfInventory>();
        ShopObstacle[] sceneObstacles=Array.Empty<ShopObstacle>();
        Collider2D[] blockingShapes=Array.Empty<Collider2D>();
        SpillHazard[] authoredSpills=Array.Empty<SpillHazard>();
        Vector2 playerStart,helperStart,rayStart;
        [SerializeField,HideInInspector] List<Vector2> initialSpillPositions=new List<Vector2>();
        readonly List<SpillHazard> spillViews=new List<SpillHazard>();
        readonly Dictionary<int,CharacterSpriteAnimator> shopperViews=new Dictionary<int,CharacterSpriteAnimator>();
        readonly HashSet<int> aliveShopperIds=new HashSet<int>();
        readonly List<int> removedShopperIds=new List<int>();
        static readonly int[] customerSkinRows={9,12,3};
        static readonly string[] customerSkins={"Girl","Zomboy","Cat","Zomgirl"};
        SoftOccluder[] softOccluders=Array.Empty<SoftOccluder>();
        readonly List<CharacterSpriteAnimator> goblinViews=new List<CharacterSpriteAnimator>(),idolViews=new List<CharacterSpriteAnimator>();
        Transform runtimeObjects;
        bool sceneInitialized, releaseArtApplied;
        RoomPresentation roomPresentation;
        void InitializeSceneBindings()
        {
            if(!shop)shop=FindFirstObjectByType<ShopLayout>();
            if(!shop||shop.schemaVersion<2)return;
            if(!roomPresentation)
            {
                var visual=new GameObject("Room visibility and contact shadows");visual.transform.SetParent(shop.transform,false);
                roomPresentation=visual.AddComponent<RoomPresentation>();roomPresentation.Initialize(shop);
            }
            if(!sceneInitialized||initialSpillPositions.Count!=authoredSpills.Length)
            {
                playerStart=ShopLayout.ToGame(shop.playerCharacter.transform.position);
                helperStart=ShopLayout.ToGame(shop.partnerCharacter.transform.position);
                rayStart=ShopLayout.ToGame(shop.rayCharacter.transform.position);
                authoredSpills=shop.StartingSpills;initialSpillPositions.Clear();
                foreach(var spill in authoredSpills)initialSpillPositions.Add(spill.Position);
                runtimeObjects=shop.transform.Find("Runtime customers and hazards");
                if(!runtimeObjects){var go=new GameObject("Runtime customers and hazards");go.transform.SetParent(shop.transform,false);runtimeObjects=go.transform;}
                sceneInitialized=true;
            }
            if(shop.spillTemplate)shop.spillTemplate.gameObject.SetActive(false);
            if(runtimeObjects){foreach(Transform child in runtimeObjects)if(child.GetComponent<CharacterSpriteAnimator>()||child.GetComponent<SpillHazard>()){child.gameObject.SetActive(false);Destroy(child.gameObject);}}
            shopperViews.Clear();goblinViews.Clear();idolViews.Clear();spillViews.Clear();
            RefreshSceneGeometry();
            if(!releaseArtApplied){ReleaseShopArt.Apply(shop,sceneShelves);releaseArtApplied=true;}
        }
        void RefreshSceneGeometry()
        {
            if(!shop||shop.schemaVersion<2)return;
            Physics2D.SyncTransforms();
            var previousShelves=sceneShelves;var previousStock=shelfStock;
            var nextShelves=shop.Shelves;
            bool topologyChanged=nextShelves.Length!=previousShelves.Length;
            if(!topologyChanged)for(int i=0;i<nextShelves.Length;i++)if(nextShelves[i]!=previousShelves[i]){topologyChanged=true;break;}
            if(topologyChanged)
            {
                var values=new int[nextShelves.Length];
                for(int i=0;i<nextShelves.Length;i++)
                {
                    int previous=Array.IndexOf(previousShelves,nextShelves[i]);
                    values[i]=previous>=0&&previous<previousStock.Length?previousStock[previous]:nextShelves[i].initialStock;
                }
                shelfStock=values;
                foreach(var c in customers)
                {
                    if(c.shelf<0)continue;
                    var old=c.shelf<previousShelves.Length?previousShelves[c.shelf]:null;
                    c.shelf=old?Array.IndexOf(nextShelves,old):-1;
                    if(c.shelf<0){c.shelf=-1;c.state=ShopperState.Entering;c.goal=ShopDoor;}
                }
                if(skillType==0)skillType=-1;
            }
            sceneShelves=nextShelves;sceneObstacles=shop.Obstacles;blockingShapes=shop.GetComponentsInChildren<Collider2D>();
            foreach(var occluder in softOccluders)if(occluder)occluder.ResetOpacity();
            softOccluders=shop.GetComponentsInChildren<SoftOccluder>();
            shelves.Clear();
            foreach(var shelf in sceneShelves)
            {
                var b=shelf.collisionBase.bounds;
                shelves.Add(new Rect(b.min.x*100,-b.max.y*100-28,b.size.x*100,b.size.y*100+28));
            }
            if(shelfStock.Length!=sceneShelves.Length)Array.Resize(ref shelfStock,sceneShelves.Length);
            supply=shop.stockroom.Position;checkout=shop.register.Position;phone=shop.wallPhone.Position;nailGun=shop.nailGun.Position;
            switches=Positions(shop.switchLocations);cans=Positions(shop.canLocations);
            if(switchIndex>=0&&switchIndex<switches.Length)lightSwitch=switches[switchIndex];
            if(canIndex>=0&&canIndex<cans.Length&&!canKicked)can=cans[canIndex];
            CacheCollisionGeometry();
        }
        Vector2[] Positions(Transform[] items)
        {var points=new Vector2[items.Length];for(int i=0;i<items.Length;i++)points[i]=ShopLayout.ToGame(items[i].position);return points;}
        void ReadManualSceneEdits()
        {
            if(!sceneInitialized||initialSpillPositions.Count!=authoredSpills.Length)return;
            if(GeometryWasEdited())RefreshSceneGeometry();
            if(shop.playerCharacter.WasMovedInScene())player=ShopLayout.ToGame(shop.playerCharacter.transform.position);
            if(shop.partnerCharacter.WasMovedInScene())helper=ShopLayout.ToGame(shop.partnerCharacter.transform.position);
            // Only intentional transform edits flow back into simulation. A stale visual must
            // never overwrite the next spill after a removal or cleanup replenishment.
            if(spillViews.Count==spills.Count)for(int i=0;i<spills.Count;i++)
                if(spillViews[i]&&spillViews[i].gameObject.activeSelf&&spillViews[i].HasManualEdit)
                    spills[i]=spillViews[i].Position;
        }
        bool SceneWalkable(Vector2 p)
        {
            return CachedSceneWalkable(p);
        }
        int NearestShelf()
        {
            int best=-1;
            float nearest=float.PositiveInfinity;
            for(int i=0;i<sceneShelves.Length;i++)
            {
                if(!sceneShelves[i]||!sceneShelves[i].CanInteract(ShopLayout.ToWorld(player)))continue;
                Vector2 approach=ShelfPoint(i);
                float distance=(approach-player).sqrMagnitude;
                if(distance>=nearest||!StaticSegmentClear(player,approach))continue;
                best=i;
                nearest=distance;
            }
            return best;
        }
        int NearestSpill()
        {
            int best=-1;float nearest=(shop?shop.spillInteractionRadius:.65f)*100;
            for(int i=0;i<spills.Count;i++){float d=Vector2.Distance(player,spills[i]);if(d<nearest&&StaticSegmentClear(player,spills[i])){nearest=d;best=i;}}
            return best;
        }
        int ShelfCapacity(int i)=>sceneInitialized&&i<sceneShelves.Length?sceneShelves[i].capacity:5;
        void ResetSceneState()
        {
            if(!sceneInitialized||initialSpillPositions.Count!=authoredSpills.Length)return;
            RefreshSceneGeometry();player=playerStart;helper=helperStart;salmah=ShopLayout.ToGame(shop.salmahSpawn.position);
            shop.playerCharacter.ResetLocomotion(player);shop.partnerCharacter.ResetLocomotion(helper);lastFootstepContact=0;
            rayX=rayStart.x;shelfStock=new int[sceneShelves.Length];
            for(int i=0;i<sceneShelves.Length;i++)shelfStock[i]=sceneShelves[i].initialStock;
            spills.Clear();spills.AddRange(initialSpillPositions);
            foreach(var view in spillViews)RetireSpillView(view);
            spillViews.Clear();
            for(int i=0;i<authoredSpills.Length;i++)if(authoredSpills[i]){authoredSpills[i].ApplyPosition(initialSpillPositions[i]);authoredSpills[i].gameObject.SetActive(true);spillViews.Add(authoredSpills[i]);}
            CollapseStackedSpills();if(shop.spillTemplate)shop.spillTemplate.gameObject.SetActive(false);
        }
        bool ClearSpill(int index)
        {
            if(index<0||index>=spills.Count)return false;
            spills.RemoveAt(index);
            // Remove the matching view immediately, including the authored opening hazard.
            if(index<spillViews.Count){RetireSpillView(spillViews[index]);spillViews.RemoveAt(index);}
            return true;
        }
        void RetireSpillView(SpillHazard view)
        {if(!view)return;view.gameObject.SetActive(false);if(view.runtimeSpawned)Destroy(view.gameObject);}
        void SyncSpillViews()
        {
            while(spillViews.Count>spills.Count){int last=spillViews.Count-1;RetireSpillView(spillViews[last]);spillViews.RemoveAt(last);}
            while(spillViews.Count<spills.Count)
            {
                var view=Instantiate(shop.spillTemplate,runtimeObjects);view.name="Water spill";view.runtimeSpawned=true;view.gameObject.SetActive(true);
                if(view.artwork)view.artwork.sortingOrder=-900;spillViews.Add(view);
            }
            for(int i=0;i<spills.Count;i++)if(spillViews[i])spillViews[i].ApplyPosition(spills[i]);
        }
        CharacterSpriteAnimator SpawnView(CharacterSpriteAnimator template,string title)
        {var view=Instantiate(template,runtimeObjects);view.name=title;view.gameObject.SetActive(true);return view;}
        bool drawingWorldOverlay;
        Rect OverlayViewport(){var p=shop.worldCamera.pixelRect;return new Rect(p.x,Screen.height-p.yMax,p.width,p.height);}
        bool WorldOverlayVisible(Rect rect)
        {
            var viewport=OverlayViewport();var min=GUI.matrix.MultiplyPoint3x4(new Vector3(rect.xMin,rect.yMin));var max=GUI.matrix.MultiplyPoint3x4(new Vector3(rect.xMax,rect.yMax));
            return min.x>=viewport.xMin&&min.y>=viewport.yMin&&max.x<=viewport.xMax&&max.y<=viewport.yMax;
        }
        Rect ClipWorldOverlay(Rect rect)
        {
            var v=OverlayViewport();var inverse=GUI.matrix.inverse;var min=inverse.MultiplyPoint3x4(new Vector3(v.xMin,v.yMin));var max=inverse.MultiplyPoint3x4(new Vector3(v.xMax,v.yMax));
            return Rect.MinMaxRect(Mathf.Max(rect.xMin,min.x),Mathf.Max(rect.yMin,min.y),Mathf.Min(rect.xMax,max.x),Mathf.Min(rect.yMax,max.y));
        }
        Matrix4x4 SceneOverlayMatrix()
        {
            var camera=shop.worldCamera;
            Vector3 origin=camera.WorldToScreenPoint(ShopLayout.ToWorld(Vector2.zero));
            Vector3 x=camera.WorldToScreenPoint(ShopLayout.ToWorld(new Vector2(100,0)));
            Vector3 y=camera.WorldToScreenPoint(ShopLayout.ToWorld(new Vector2(0,100)));
            var matrix=Matrix4x4.identity;
            matrix.m00=(x.x-origin.x)/100;matrix.m10=-(x.y-origin.y)/100;
            matrix.m01=(y.x-origin.x)/100;matrix.m11=-(y.y-origin.y)/100;
            matrix.m03=origin.x;matrix.m13=Screen.height-origin.y;return matrix;
        }
        void ConfigureWorldCamera()
        {
            if(!sceneInitialized||!shop.worldCamera)return;
            float scale=Mathf.Min(Screen.width/1600f,Screen.height/900f);
            float ox=(Screen.width-1600*scale)/2,oy=(Screen.height-900*scale)/2;
            var camera=shop.worldCamera;
            if(cleanCaptureMode)
            {
                camera.rect=new Rect(0,0,1,1);
                camera.backgroundColor=new Color(.24f,.32f,.29f);camera.clearFlags=CameraClearFlags.SolidColor;
                camera.transform.position=new Vector3(9.364815f,-4.627407f,-10);
                camera.orthographic=true;camera.orthographicSize=Mathf.Max(2.953704f,6.45f/camera.aspect);
                return;
            }
            camera.rect=new Rect(ox/Screen.width,oy/Screen.height,1600*scale/Screen.width,900*scale/Screen.height);
            camera.backgroundColor=new Color(.10f,.25f,.28f);camera.clearFlags=CameraClearFlags.SolidColor;
            var arcade=shop.GetComponent<ArcadeRoomCamera>();
            if(arcade&&arcade.enabled){arcade.Frame(camera,ShopLayout.ToWorld(player),Time.unscaledDeltaTime,!cameraInitialized);cameraInitialized=true;return;}
            if(shop.frameCameraToInterface)
            {
                if(shop.testFollowCamera&&shop.testFollowCamera.isActiveAndEnabled&&camera==shop.testFollowCamera.GetComponent<Camera>())
                {
                    camera.orthographic=true;camera.orthographicSize=gameplayZoom;
                    var motion=selected&&!mainMenu&&!settingsOpen&&!autoPaused&&skillType<0?playerMotion:Vector2.zero;
                    var velocity=Time.deltaTime>.00001f?new Vector2(motion.x,-motion.y)/(100*Time.deltaTime):Vector2.zero;
                    shop.testFollowCamera.Follow(camera,ShopLayout.ToWorld(player),velocity,shop.CameraBounds,Time.unscaledDeltaTime,!cameraInitialized);
                    cameraCenter=camera.transform.position;cameraInitialized=true;
                }
                else FollowPlayerCamera(camera);
            }
        }
        void LateUpdate()
        {
            if(!sceneInitialized||initialSpillPositions.Count!=authoredSpills.Length)return;
            ConfigureWorldCamera();
            if(shop.fluorescentLights!=null)foreach(var lamp in shop.fluorescentLights)if(lamp){lamp.gameObject.SetActive(encounter==Encounter.KKung);lamp.color=Color.Lerp(new Color(.5f,.5f,.6f),Color.white,lightsOn?1:(float)circuitStep/CircuitGoal);}
            if(shop.playerCharacter.heldStock)shop.playerCharacter.heldStock.gameObject.SetActive(carryingCrate);
            if(shop.playerCharacter.heldGun)shop.playerCharacter.heldGun.gameObject.SetActive(hasGun);
            shop.boCarTarget.parent.gameObject.SetActive(encounter==Encounter.Bo);
            shop.playerCharacter.Apply(player,playerFacing,zack?6:0,playerMotion,pulse);
            SyncPlayerFootsteps();
            shop.partnerCharacter.Apply(helper,helperFacing,zack?0:6,helperMotion,pulse);
            for(int i=0;i<sceneShelves.Length;i++)if(sceneShelves[i])sceneShelves[i].ShowStock(shelfStock[i]);
            foreach(var occluder in softOccluders)if(occluder&&occluder.isActiveAndEnabled)
                occluder.Tick(shop.playerCharacter.transform.position,shop.playerCharacter.artwork.bounds,selected&&!mainMenu,Time.unscaledDeltaTime);
            var alive=aliveShopperIds;alive.Clear();
            foreach(var c in customers)
            {
                alive.Add(c.id);
                if(!shopperViews.TryGetValue(c.id,out var view)||!view)
                {
                    view=SpawnView(shop.customerTemplate,"Customer "+c.id);view.gameObject.AddComponent<CustomerMoodPresentation>();shopperViews[c.id]=view;
                    view.smoothSkin=customerSkins[(c.id-1)%customerSkins.Length];
                    var art=view.artwork;var scale=art.transform.parent.lossyScale;float height=view.smoothSkin=="Cat"?.62f:.96f;
                    art.transform.localScale=new Vector3(height/1.88f/scale.x,height/1.88f/scale.y,1);art.transform.localPosition=Vector3.zero;
                }
                view.GetComponent<CustomerMoodPresentation>().SetFrustration(CustomerFrustration(c),c.state==ShopperState.Queuing);
                if(view.heldStock)view.heldStock.gameObject.SetActive(c.basket>0);
                view.Apply(c.position,c.facing,customerSkinRows[c.id%3],c.motion,pulse+c.id*.14f);
            }
            var removed=removedShopperIds;removed.Clear();foreach(var pair in shopperViews)if(!alive.Contains(pair.Key)){if(pair.Value)Destroy(pair.Value.gameObject);removed.Add(pair.Key);}
            foreach(int id in removed)shopperViews.Remove(id);
            SyncSpillViews();
            foreach(var item in droppedStock)if(item.view)PresentDroppedStock(item);
            PresentActiveEncounters();
            shop.nailGun.gameObject.SetActive(encounter==Encounter.Salmah&&!hasGun);
            shop.lightSwitch.gameObject.SetActive(encounter==Encounter.KKung);shop.lightSwitch.transform.position=ShopLayout.ToWorld(lightSwitch);shop.lightSwitch.sortingOrder=(int)lightSwitch.y;
            shop.counterCan.gameObject.SetActive(encounter==Encounter.Bo&&(!canKicked||canFlying));shop.counterCan.transform.position=ShopLayout.ToWorld(canFlying?flyingCan:can);shop.counterCan.sortingOrder=(int)(canFlying?flyingCan.y:can.y);
            shop.counterCan.transform.localRotation=Quaternion.Euler(0,0,canFlying?pulse*600:boSpawnDelay>0?Mathf.Sin(pulse*18)*15:0);
            roomPresentation.Tick(shop.playerCharacter.transform.position,selected&&!mainMenu,Time.unscaledDeltaTime);
        }
    }
}


