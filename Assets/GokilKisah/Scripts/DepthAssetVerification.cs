using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        public void BeginDepthAssetVerification(string directory){StartCoroutine(DepthAssetVerification(directory));}
        IEnumerator DepthAssetVerification(string directory)
        {
            Directory.CreateDirectory(directory);var checks=new List<string>();
            Action<bool,string> check=(ok,name)=>checks.Add((ok?"PASS ":"FAIL ")+name);
            testing=true;verificationRunning=true;autoPaused=false;settingsOpen=false;campaignPersistence=false;
            ReturnToMainMenu();noticeUntil=0;
            yield return new WaitForSecondsRealtime(.4f);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"menu.png"));
            BeginCampaignLevel(1,false);selected=true;noticeUntil=0;
            yield return new WaitForSecondsRealtime(.6f);
            var root=shop.transform.Find("Reference shop layout");
            foreach(var r in root.GetComponentsInChildren<SpriteRenderer>())
            {
                if(!r.enabled||!r.sprite||!r.sprite.texture.name.StartsWith("Art_TTM_Env_"))continue;
                var s=r.transform.lossyScale;check(Mathf.Abs(s.x-s.y)<.001f,"Uniform artwork scale: "+r.name);
            }
            var floor=root.Find("Mart floor").GetComponent<SpriteRenderer>();
            check(floor.sprite.texture.name=="Art_TTM_Env_Floor01","New authored floor in scene");
            check(SmoothArt.Load("Zack_0_0")==SmoothArt.Load("Zack_3_2")&&SmoothArt.Load("Zack_0_0").name=="ZackStatic","Zack supplied still pose for every facing");
            foreach(var actor in new[]{shop.playerCharacter,shop.partnerCharacter})
                check(Mathf.Abs(actor.artwork.bounds.size.y-1.05f)<.01f&&actor.artwork.sprite.pivot.y==0,"Keeper height and foot pivot: "+actor.name);
            check(shop.worldCamera.orthographicSize<=3,"Close follow camera retained");
            check(Walkable(ShopDoor),"New facade entrance is walkable");
            bool goals=true;for(int i=0;i<sceneShelves.Length;i++)goals&=Walkable(ShelfPoint(i));
            check(goals,"Every shelf approach remains accessible");
            helper=player+new Vector2(100,0);
            yield return new WaitForSecondsRealtime(.6f);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"keepers.png"));
            player=ShopDoor;cameraInitialized=false;
            yield return new WaitForSecondsRealtime(.7f);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"entrance.png"));
            player=supply;cameraInitialized=false;
            yield return new WaitForSecondsRealtime(.8f);
            check(roomPresentation.InStorage&&roomPresentation.MainOpacity>.9f,"Storage conceals main room");
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"storage.png"));
            ResetRun(Encounter.Store);selected=true;mainMenu=false;campaignActive=false;spills.Clear();
            int stuck=0;bool safe=true;
            for(int tick=0;tick<12000;tick++)
            {
                for(int i=0;i<shelfStock.Length;i++)shelfStock[i]=ShelfCapacity(i);
                UpdateCustomers(.05f);if(CustomerAtRegister()!=null)ServeCustomer();spills.Clear();
                foreach(var customer in customers){safe&=SceneWalkable(customer.position);if(customer.blocked>12)stuck++;}
                if(tick%200==0)yield return null;
            }
            check(safe,"Customer routes stay inside geometry");
            check(served>25&&completedShoppingTrips>25,"Ten-minute shopping and departure test: "+served+" served / "+completedShoppingTrips+" trips");
            check(stuck==0,"No prolonged customer blockage");
            File.WriteAllLines(Path.Combine(directory,"results.txt"),checks);
            testing=false;verificationRunning=false;ReturnToMainMenu();
        }
    }
}
