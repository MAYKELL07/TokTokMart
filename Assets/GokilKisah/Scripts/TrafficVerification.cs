using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        public void BeginTrafficVerification(string directory){StartCoroutine(TrafficVerification(directory));}
        IEnumerator TrafficVerification(string directory)
        {
            Directory.CreateDirectory(directory);var checks=new List<string>();
            Action<bool,string> check=(ok,name)=>checks.Add((ok?"PASS ":"FAIL ")+name);
            testing=true;verificationRunning=true;autoPaused=false;settingsOpen=false;
            BeginCampaignLevel(1,false);noticeUntil=0;
            var regular=sceneShelves.Single(s=>s.name=="Shelf 5");var middle=sceneShelves.Single(s=>s.name=="Middle aisle Shelf 1");
            foreach(var s in sceneShelves)
            {
                var source=s.name.StartsWith("Middle aisle")?middle:regular;
                bool matches=s.capacity==source.stockSlots.Length&&s.stockSlots.Length==source.stockSlots.Length;
                for(int i=0;i<s.stockSlots.Length;i++)
                {
                    matches&=s.stockSlots[i].name=="Stock slot "+(i+1).ToString("00");
                    matches&=Vector3.Distance(s.stockSlots[i].transform.position-s.transform.position,source.stockSlots[i].transform.position-source.transform.position)<.01f;
                }
                check(matches,"Template slots, names and capacity: "+s.name+" ("+s.capacity+")");
                s.ShowStock(s.capacity);check(s.stockSlots.All(r=>r.sprite==s.stockedSprite),"Every authored slot fills: "+s.name);s.ShowStock(shelfStock[Array.IndexOf(sceneShelves,s)]);
            }
            check(customerArrival>=3.9f,"Quiet opening has a four-second first-arrival delay");
            float opening=CurrentArrivalInterval;int firstLimit=CustomerLimit;
            shiftTime=ShiftBudget;float closing=CurrentArrivalInterval;int lastLimit=CustomerLimit;
            check(closing<opening*.4f&&lastLimit>firstLimit,"Level 1 ramps from "+firstLimit+" to "+lastLimit+" customers, intervals "+opening.ToString("F1")+"s to "+closing.ToString("F1")+"s");
            BeginCampaignLevel(12,false);float laterOpening=CurrentArrivalInterval;int laterLimit=CustomerLimit;shiftTime=ShiftBudget;
            check(laterOpening<opening&&CurrentArrivalInterval<closing&&laterLimit>firstLimit&&CustomerLimit>lastLimit,"Later levels raise both opening and closing traffic");
            BeginCampaignLevel(1,false);player=helper=ShopDoor;customerArrival=999;
            var door=shop.GetComponentInChildren<EntranceDoorPresentation>();
            UpdateCustomers(.6f);check(!CustomerRequestsEntrance&&door.OpenAmount==0,"Player and partner do not open customer entrance");
            var entering=SpawnCustomer();var initial=entering.position;
            UpdateCustomers(.1f);check(door.OpenAmount>0&&door.OpenAmount<1&&entering.position==initial,"Customer triggers tween and waits for leaf clearance");
            UpdateCustomers(.35f);check(door.IsPassable,"Customer entrance fully opens");
            player=ShopDoor+new Vector2(-70,-65);cameraInitialized=false;
            yield return new WaitForSecondsRealtime(.5f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"customer-entrance-open.png"));
            customers.Clear();UpdateCustomers(.2f);check(door.IsPassable,"Door holds open briefly after customer clears");
            UpdateCustomers(.3f);UpdateCustomers(.5f);check(door.OpenAmount==0,"Door tweens closed after customer clears");
            var queued=SpawnCustomer();queued.state=ShopperState.Queuing;queued.position=QueuePoint(0);queued.basket=1;queued.items.Add(0);queued.timer=CustomerPatience*.82f;customerQueue.Add(queued);
            check(CustomerFrustration(queued)>.75f,"Long wait produces angry mood before abandonment");
            player=QueuePoint(0)+new Vector2(-90,45);cameraInitialized=false;
            yield return new WaitForSecondsRealtime(.5f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"angry-customer.png"));
            queued.timer=CustomerPatience+1;UpdateCustomers(.05f);
            check(queued.angry&&queued.state==ShopperState.Leaving&&!customerQueue.Contains(queued)&&missedCustomers==1&&chaos==8,"Angry unpaid customer leaves queue with one penalty");
            ResetRun(Encounter.Store);selected=true;mainMenu=false;customerArrival=999;
            int stuck=0;bool safe=true;
            for(int tick=0;tick<12000;tick++)
            {
                shiftTime=tick*.05f;for(int i=0;i<shelfStock.Length;i++)shelfStock[i]=ShelfCapacity(i);
                if(tick==0)customerArrival=4;
                UpdateCustomers(.05f);if(CustomerAtRegister()!=null)ServeCustomer();spills.Clear();
                foreach(var c in customers){safe&=SceneWalkable(c.position);if(c.blocked>12)stuck++;}
                if(tick%200==0)yield return null;
            }
            check(safe&&stuck==0&&served>25,"Ten-minute ramp, entrance and register regression: "+served+" served, "+completedShoppingTrips+" departed, "+stuck+" prolonged blocks");
            File.WriteAllLines(Path.Combine(directory,"results.txt"),checks);
            testing=false;verificationRunning=false;ReturnToMainMenu();
        }
    }
}
