using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        public void BeginArcadeVerification(string directory){StartCoroutine(ArcadeVerification(directory));}
        IEnumerator ArcadeVerification(string directory)
        {
            Directory.CreateDirectory(directory);var checks=new List<string>();int failures=0;
            Action<bool,string> check=(ok,name)=>{checks.Add((ok?"PASS ":"FAIL ")+name);if(!ok)failures++;};
            testing=true;verificationRunning=true;autoPaused=false;settingsOpen=false;campaignPersistence=false;
            ReturnToMainMenu();noticeUntil=0;
            yield return new WaitForSecondsRealtime(.4f);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"01-main-menu.png"));
            mainMenu=false;selected=false;
            yield return new WaitForSecondsRealtime(.4f);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"02-choose-keeper.png"));
            BeginCampaignLevel(1,false);selected=true;noticeUntil=0;
            yield return new WaitForSecondsRealtime(.5f);
            var camera=shop.worldCamera;var rig=shop.GetComponent<ArcadeRoomCamera>();
            check(rig&&rig.enabled,"Arcade follow camera active");
            check(Resources.Load<Font>("Fonts/LilitaOne-Regular"),"Display typeface loads");
            check(Walkable(player)&&Walkable(ShopDoor),"Player start and shop entrance walkable");
            bool goals=true;for(int i=0;i<sceneShelves.Length;i++)goals&=Walkable(ShelfPoint(i));
            check(goals,"All shelf approach points remain walkable");
            check(Walkable(QueuePoint(9)),"Queue can accommodate ten customers");
            check(camera.orthographicSize<=3,"Camera stays close to the character");
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"03-entire-shop.png"));
            player=supply;cameraInitialized=false;
            yield return new WaitForSecondsRealtime(1);
            check(roomPresentation.InStorage&&roomPresentation.MainOpacity>.9f,"Storage transition conceals shop");
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"04-storage.png"));
            player=playerStart;cameraInitialized=false;BeginSkill(0,0);taskCombo=1;skillClock=.3f;
            yield return new WaitForSecondsRealtime(.5f);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"05-restock-task.png"));
            skillType=-1;ResetCustomers();var c=SpawnCustomer();c.position=QueuePoint(0);c.basket=2;c.items.Add(0);c.items.Add(1);c.state=ShopperState.Queuing;customerQueue.Add(c);
            BeginSkill(1,c.id);priceEntry="6.00";
            yield return new WaitForSecondsRealtime(.3f);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"06-checkout.png"));
            check(SubmitPrice(),"Receipt keypad still accepts correct price");
            SetSettingsOpen(true);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"07-settings.png"));
            SetSettingsOpen(false);ResetRun(Encounter.Store);selected=true;mainMenu=false;campaignActive=false;spills.Clear();
            int stuck=0;bool safe=true;
            for(int tick=0;tick<12000;tick++)
            {
                for(int i=0;i<shelfStock.Length;i++)shelfStock[i]=ShelfCapacity(i);
                UpdateCustomers(.05f);if(CustomerAtRegister()!=null)ServeCustomer();spills.Clear();
                foreach(var customer in customers){safe&=SceneWalkable(customer.position);if(customer.blocked>12)stuck++;}
                if(tick%200==0)yield return null;
            }
            check(safe,"Ten-minute route test stays inside geometry");
            check(served>25&&completedShoppingTrips>25,"Customers shop, pay and depart: "+served+" / "+completedShoppingTrips);
            check(stuck==0,"No prolonged customer blockage: "+stuck);
            BeginCampaignLevel(1,false);served=ServeGoal;stockActions=StockGoal;hazardsCleared=CleanGoal;FinishCampaignLevel(true,"");noticeUntil=0;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"08-shift-receipt.png"));
            File.WriteAllText(Path.Combine(directory,"results.txt"),string.Join("\n",checks)+"\n"+failures+" failures\n");
            testing=false;verificationRunning=false;ReturnToMainMenu();
        }
    }
}
