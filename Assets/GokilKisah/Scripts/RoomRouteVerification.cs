using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        public void BeginRoomRouteVerification(string directory){StartCoroutine(RoomRouteVerification(directory));}
        IEnumerator RoomRouteVerification(string directory)
        {
            Directory.CreateDirectory(directory);var checks=new List<string>();int failures=0;
            Action<bool,string> check=(ok,name)=>{checks.Add((ok?"PASS ":"FAIL ")+name);if(!ok)failures++;};
            testing=true;verificationRunning=true;settingsOpen=false;autoPaused=false;
            ResetRun(Encounter.Store);selected=true;mainMenu=false;spills.Clear();
            foreach(var fade in softOccluders)if(fade.kind!=SoftOccluder.OccluderKind.Shelf)
            {
                var b=fade.silhouette.bounds;var near=new Vector2(b.center.x,b.min.y+.15f);
                var body=new Bounds((Vector3)near+Vector3.up*.5f,new Vector3(.65f,1,0));
                check(fade.DesiredOpacity(near,body)<.3f,"Wall fades: "+fade.name);
                body.center+=Vector3.right*40;
                float restored=fade.DesiredOpacity(near+Vector2.right*40,body);
                check(restored>.99f,"Wall restores: "+fade.name+" opacity "+restored+" bounds "+b);
            }
            player=ShopLayout.ToGame(shop.storageArea.bounds.center);cameraInitialized=false;
            yield return new WaitForSecondsRealtime(1.5f);
            check(roomPresentation.InStorage&&roomPresentation.MainOpacity>.9f&&roomPresentation.StorageOpacity<.01f,"Inside storage darkens main room only");
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"inside-storage.png"));
            player=ShopLayout.ToGame(new Vector3(18.45f,-1.05f,0));cameraInitialized=false;
            yield return new WaitForSecondsRealtime(1.5f);
            check(!roomPresentation.InStorage&&roomPresentation.StorageOpacity>.9f&&roomPresentation.MainOpacity<.01f,"Outside storage darkens storage only: "+roomPresentation.InStorage+" / "+roomPresentation.StorageOpacity+" / "+roomPresentation.MainOpacity);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"outside-storage.png"));
            player=ShopLayout.ToGame(new Vector3(14.8f,-11.15f,0));cameraInitialized=false;
            for(int i=0;i<45;i++)yield return null;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"south-wall.png"));
            ResetCustomers();spills.Clear();
            var paid=SpawnCustomer();paid.position=QueuePoint(0);paid.basket=1;paid.state=ShopperState.Queuing;customerQueue.Add(paid);
            for(int i=1;i<6;i++){var c=SpawnCustomer();c.position=QueuePoint(i);c.basket=1;c.state=ShopperState.Queuing;customerQueue.Add(c);}
            customerArrival=999;ServeCustomer();
            for(int tick=0;tick<1200;tick++){UpdateCustomers(.05f);if(tick%100==0)yield return null;}
            check(!customers.Contains(paid),"Paid customer leaves through exit with five customers queued");
            for(int tick=0;tick<6000;tick++)
            {
                UpdateCustomers(.05f);if(tick%10==0&&CustomerAtRegister()!=null)ServeCustomer();
                if(tick%100==0)yield return null;
            }
            check(customerQueue.Count==0&&customers.Count==0,"All six queued customers complete checkout and depart");
            ResetRun(Encounter.Store);selected=true;mainMenu=false;spills.Clear();
            bool safe=true;int maxStuck=0;
            for(int tick=0;tick<12000;tick++)
            {
                for(int s=0;s<shelfStock.Length;s++)shelfStock[s]=ShelfCapacity(s);
                UpdateCustomers(.05f);if(CustomerAtRegister()!=null)ServeCustomer();spills.Clear();
                foreach(var c in customers){if(!SceneWalkable(c.position))safe=false;if(c.blocked>12)maxStuck++;}
                if(tick%200==0)yield return null;
            }
            check(safe,"Ten-minute customer simulation stays inside walkable geometry");
            check(served>25&&completedShoppingTrips>25,"Repeated shopping, checkout and exit: "+served+" served / "+completedShoppingTrips+" departed");
            checks.Add("Long blocked samples: "+maxStuck);
            foreach(var c in customers)checks.Add("Customer "+c.id+" "+c.state+" position "+c.position+" goal "+c.goal+" blocked "+c.blocked);
            player=playerStart;cameraInitialized=false;
            for(int i=0;i<30;i++)yield return null;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"shadows-gameplay.png"));
            File.WriteAllText(Path.Combine(directory,"results.txt"),string.Join("\n",checks)+"\n"+failures+" failures\n");
            testing=false;verificationRunning=false;ReturnToMainMenu();
        }
    }
}
