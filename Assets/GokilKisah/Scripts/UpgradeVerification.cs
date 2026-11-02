using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        public void BeginUpgradePreview(string directory){StartCoroutine(CaptureUpgradePreview(directory));}
        IEnumerator CaptureUpgradePreview(string directory)
        {
            Directory.CreateDirectory(directory);ReturnToMainMenu();showPitchControls=false;showTouchControls=false;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"main-menu.png"));
            mainMenu=false;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"choose-character.png"));
            Choose(false);testing=true;player=playerStart;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"store.png"));
            player=supply+new Vector2(30,50);shop.playerCharacter.Apply(player,24,0,Vector2.zero,pulse);cameraInitialized=false;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"storage-room.png"));
            var shopper=SpawnCustomer();shopper.basket=3;shopper.items.AddRange(new[]{0,1,2});shopper.state=ShopperState.Queuing;shopper.position=QueuePoint(0);customerQueue.Add(shopper);
            player=ShopLayout.ToGame(shop.register.approachPoint.position);shop.playerCharacter.Apply(player,24,0,Vector2.zero,pulse);cameraInitialized=false;Interact();
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"cashier.png"));
            testing=false;
        }
        public void BeginUpgradeVerification(string path){StartCoroutine(VerifyUpgrade(path));}
        IEnumerator VerifyUpgrade(string path)
        {
            var checks=new List<string>();int failures=0;
            Action<bool,string> check=(ok,name)=>{checks.Add((ok?"PASS ":"FAIL ")+name);if(!ok)failures++;};
            verificationRunning=true;testing=true;settingsOpen=false;autoPaused=false;Choose(false);ResetRun(Encounter.Store);yield return null;
            check(shop.schemaVersion>=5,"Expanded scene migration saved");
            check(shop.walkableArea.bounds.size.x>15&&shop.walkableArea.bounds.size.y>8,"Store floor expanded in both dimensions");
            check(shop.storageArea&&shop.storageDoor&&shop.schemaVersion>=6,"Separate storage room and doorway are saved");
            check(shop.storageArea.OverlapPoint(ShopLayout.ToWorld(supply)),"Stock pickup is inside the storage room");
            var roomRoute=playerStart;for(int n=0;n<2000&&Vector2.Distance(roomRoute,supply)>7;n++)roomRoute=Navigate(roomRoute,supply,12);
            check(Vector2.Distance(roomRoute,supply)<7,"Player and helper can reach stock through the doorway");
            var roomWall=ShopLayout.ToGame(shop.storageDoor.position+(shop.schemaVersion>=7?Vector3.left:Vector3.up)*1.1f);check(!Walkable(roomWall)&&Walkable(ShopLayout.ToGame(shop.storageDoor.position)),"Storage partition blocks feet while doorway remains open");
            check(sceneShelves.Length==(shop.schemaVersion>=7?8:9),"All authored shelves are usable");
            check(Walkable(playerStart)&&Walkable(helperStart),"Both character spawns are walkable");
            bool reachable=true;
            foreach(var shelf in sceneShelves)
            {
                var goal=ShopLayout.ToGame(shelf.shoppingPoint.position);var p=playerStart;
                for(int n=0;n<1800&&Vector2.Distance(p,goal)>7;n++)p=Navigate(p,goal,12);
                check(Vector2.Distance(p,goal)<7&&Walkable(p),"Shelf route "+shelf.name);
                reachable&=Vector2.Distance(p,goal)<7;
            }
            foreach(var goal in new[]{supply,ShopLayout.ToGame(shop.register.approachPoint.position),ShopLayout.ToGame(shop.customerEntryTarget.position)})
            {var p=playerStart;for(int n=0;n<1800&&Vector2.Distance(p,goal)>7;n++)p=Navigate(p,goal,12);check(Vector2.Distance(p,goal)<7,"Station/entrance route "+goal);}
            check(SmoothArt.Load("Sam_3_0")&&SmoothArt.Load("ShelfStock")&&shop.playerCharacter.smoothSkin=="Keeper","Smooth character and shelf sprites installed");
            check(shop.customerTemplate.smoothSkin=="Customer"&&shop.goblinTemplate.smoothSkin=="Goblin"&&shop.idolTemplate.smoothSkin=="Idol","Inactive spawn templates use smooth artwork");
            check(shop.playerCharacter.artwork.sprite.texture.width>=256&&shop.playerCharacter.artwork.sprite.texture.filterMode==FilterMode.Bilinear,"Art uses high resolution and smooth filtering");
            check(UnityEngine.Resources.Load<Texture2D>("OnlineArt/Sam_3_0")&&UnityEngine.Resources.Load<Texture2D>("OnlineArt/ShelfStock"),"Downloaded Kenney character and market resources load");
            check(UnityEngine.Resources.Load<Texture2D>("OnlineArt/ShelfStock").width==512,"Market models are baked into high-resolution non-pixel sprites");
            check(Ringgit(350)=="RM 3.50"&&TryRinggit("3.50",out int cents)&&cents==350,"MYR amounts format and parse exact ringgit and sen");
            check(!TryRinggit("3.501",out cents)&&!TryRinggit("-1",out cents),"Price entry rejects fractional sen and negative prices");
            priceEntry="3";EnterPricePoint();EnterPriceDigit('5');EnterPriceDigit('0');EnterPriceDigit('1');check(priceEntry=="3.50","Cashier accepts at most two decimal places");EnterPricePoint();check(priceEntry=="3.50","Cashier rejects a second decimal point");
            ResetStamina();float walk=MovementSpeed(false,true),run=MovementSpeed(true,true);check(run>walk&&Mathf.Approximately(run,walk*sprintMultiplier),"Sprint increases movement speed");
            UpdateStamina(1,true);check(Stamina<maxStamina&&IsSprinting,"Moving while sprinting drains stamina");float savedStamina=Stamina;MovementSpeed(true,false);UpdateStamina(.2f,false);check(Stamina==savedStamina&&!IsSprinting,"Standing still consumes no stamina");
            MovementSpeed(true,true);UpdateStamina(20,true);check(Stamina==0&&sprintExhausted&&MovementSpeed(true,true)==walk,"Exhaustion prevents sprinting below recovery threshold");
            UpdateStamina(staminaRecoveryDelay+.1f,false);check(Stamina>0&&sprintExhausted,"Stamina recovers after delay while exhausted");UpdateStamina(5,false);check(Stamina==maxStamina&&MovementSpeed(true,true)>walk,"Rest restores stamina and allows sprinting again");
            ResetStamina();MovementSpeed(true,true);UpdateStamina(1,false);check(Stamina==maxStamina,"Running against a wall does not drain stamina");
            skillType=0;check(MovementSpeed(true,true)==walk,"Tasks block sprinting");skillType=-1;ResetStamina();
            var cam=shop.worldCamera;ConfigureWorldCamera();
            // Place camera and player away from map limits to isolate the dead zone.
            var midpoint=(Vector2)shop.walkableArea.bounds.center;cameraInitialized=true;cameraCenter=midpoint;player=ShopLayout.ToGame(midpoint-Vector2.up*.22f);FollowPlayerCamera(cam);var start=cam.transform.position;
            player+=new Vector2(20,10);FollowPlayerCamera(cam);check(Vector3.Distance(start,cam.transform.position)<.001f,"Camera stays still for movement inside center zone");
            player+=new Vector2(450,0);FollowPlayerCamera(cam);check(cam.transform.position.x>start.x,"Camera follows when player crosses zone boundary");
            player=new Vector2(-10000,-10000);FollowPlayerCamera(cam);var bounds=shop.CameraBounds;
            check(cam.transform.position.x>=bounds.min.x+cam.orthographicSize*cam.aspect-.001f&&cam.transform.position.y<=bounds.max.y-cam.orthographicSize+.001f,"Camera clamps viewport to store bounds");
            ResetRun(Encounter.Store);player=supply;Interact();check(carryingCrate,"Stockroom gives crate");player=ShelfPoint(0);Interact();check(skillType==0,"Empty shelf starts stocking task");
            skillClock=0;AttemptSkill();check(carryingCrate&&stockActions==0,"Miss preserves crate and stock");
            for(int i=0;i<3;i++){skillClock=SkillZoneCenter()/(.65f+.09f*taskCombo);AttemptSkill();}
            check(stockActions==1&&!carryingCrate&&shelfStock[0]==ShelfCapacity(0)&&skillType<0,"Three timed placements fill shelf");
            ResetRun(Encounter.Store);player=spills[0];Interact();
            float firstCenter=SkillZoneCenter();skillClock=firstCenter/.75f;AttemptSkill();check(taskCombo==1&&SkillZoneCenter()!=firstCenter,"Mop target alternates after a successful stroke");
            for(int i=1;i<4;i++){skillClock=SkillZoneCenter()/(.75f+.08f*taskCombo);AttemptSkill();}
            check(hazardsCleared==1&&spills.Count==0,"Four mop strokes clear spill");
            ResetRun(Encounter.Store);var shopper=SpawnCustomer();shopper.basket=3;shopper.state=ShopperState.Queuing;shopper.position=QueuePoint(0);customerQueue.Add(shopper);player=ShopLayout.ToGame(shop.register.approachPoint.position);Interact();
            check(skillType==1&&receiptNames.Length==3&&receiptTotal>0,"Actual customer basket creates priced receipt");
            priceEntry="1";check(!SubmitPrice()&&served==0&&skillType==1,"Wrong total keeps customer waiting");
            int stock=TotalStock();priceEntry=RinggitNumber(receiptTotal);check(SubmitPrice()&&served==1&&skillType<0&&TotalStock()==stock,"Correct total pays customer without deducting stock twice");
            priceEntry="";for(int i=0;i<10;i++)EnterPriceDigit('9');check(priceEntry.Length==8,"Cashier limits price entry length");
            ResetRun(Encounter.KKung);var old=lightSwitch;player=lightSwitch;Interact();check(lightsOn&&resolved,"Light-switch encounter resolves");ResetRun(Encounter.KKung);check(lightSwitch!=old,"Switch location changes between encounters");
            ResetRun(Encounter.Salmah);player=nailGun;Interact();player=salmah+Vector2.right*100;yield return new WaitForSeconds(2.5f);check(resolved,"Rear nail-gun attack still resolves encounter");
            ResetRun(Encounter.Bo);player=can;Interact();yield return new WaitForSeconds(4);check(resolved&&!gameOver,"Can kick reaches expanded Bo target");
            ResetRun(Encounter.Store);bool safe=true;int initialStock=TotalStock();
            for(int tick=0;tick<2400;tick++){UpdateCustomers(.05f);foreach(var c in customers)if(!Walkable(c.position)){if(safe){var wp=ShopLayout.ToWorld(c.position);string blockers="";foreach(var shape in blockingShapes)if(shape&&shape.enabled&&!shape.isTrigger&&((Vector2)shape.ClosestPoint(wp)-(Vector2)wp).sqrMagnitude<.0144f)blockers+=shape.name+" ";checks.Add("DETAIL Customer "+c.id+" at "+c.position+" state "+c.state+" blocked by "+blockers);}safe=false;}if(tick%120==0)yield return null;}
            check(safe,"Customers remain on walkable floor");check(TotalStock()<initialStock&&CustomerAtRegister()!=null,"Customers shop and physically arrive at register");
            var actual=CustomerAtRegister();if(actual!=null){player=ShopLayout.ToGame(shop.register.approachPoint.position);Interact();int expected=0;foreach(int item in actual.items)expected+=groceryPrices[item];check(receiptTotal==expected&&actual.items.Count==actual.basket,"Receipt matches the products actually collected");}
            yield return new WaitForEndOfFrame();bool sizeOK=true;foreach(var view in shopperViews.Values)if(view.artwork.bounds.size.x<.4f||view.artwork.bounds.size.y<.6f)sizeOK=false;check(sizeOK,"Spawned customers retain full character size");
            SetSettingsOpen(true);float before=shiftTime;check(Time.timeScale==0,"Settings pause the shift");SetSettingsOpen(false);
            ReturnToMainMenu();check(mainMenu&&!selected&&skillType<0,"Return to menu clears tasks and shift selection");
            Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,"TOK MART UPGRADE VERIFICATION\n"+string.Join("\n",checks)+"\n\n"+checks.Count+" checks; "+failures+" failures.\n");
            Debug.Log("Tok Mart upgrade: "+checks.Count+" checks; "+failures+" failures.");testing=false;verificationRunning=false;
        }
    }
}
