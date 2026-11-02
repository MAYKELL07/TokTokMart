using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        public void BeginArtPresentationVerification(string directory){StartCoroutine(ArtPresentationVerification(directory));}
        IEnumerator ArtPresentationVerification(string directory)
        {
            Directory.CreateDirectory(directory);var checks=new List<string>();int failures=0;
            Action<bool,string> check=(ok,name)=>{checks.Add((ok?"PASS ":"FAIL ")+name);if(!ok)failures++;};
            testing=true;verificationRunning=true;settingsOpen=false;autoPaused=false;mainMenu=false;selected=true;
            BeginCampaignLevel(1,false);noticeUntil=0;
            var root=shop.transform.Find("Reference shop layout");
            foreach(var shelf in sceneShelves)
            {
                var fixture=shelf.transform.Find("Artist shelf fixture").GetComponent<SpriteRenderer>();var scale=fixture.transform.lossyScale;
                check(Mathf.Abs(scale.x-scale.y)<.0001f,"Original aspect ratio: "+shelf.name);
                foreach(var slot in shelf.stockSlots)if(slot.sprite)check(slot.sortingOrder>fixture.sortingOrder,"Stock draws above shelf: "+slot.name);
            }
            var floor=root.Find("Mart floor").GetComponent<SpriteRenderer>();
            check(floor.drawMode==SpriteDrawMode.Tiled&&Mathf.Abs(floor.transform.lossyScale.x-floor.transform.lossyScale.y)<.0001f,"Floor tiles repeat without stretching");
            foreach(var actor in new[]{shop.playerCharacter,shop.partnerCharacter,shop.customerTemplate})
            {var scale=actor.artwork.transform.lossyScale;check(Mathf.Abs(scale.x-scale.y)<.0001f&&actor.artwork.sprite.pivot.y==0,"Uniform character scale and foot pivot: "+actor.name);}
            var bodies=new List<SpriteRenderer>();foreach(var shelf in sceneShelves)bodies.Add(shelf.transform.Find("Artist shelf fixture").GetComponent<SpriteRenderer>());
            for(int i=0;i<bodies.Count;i++)for(int j=i+1;j<bodies.Count;j++)check(!bodies[i].bounds.Intersects(bodies[j].bounds),"Shelf artwork does not overlap: "+sceneShelves[i].name+" / "+sceneShelves[j].name);
            gameplayZoom=8.6f;player=playerStart;cameraInitialized=false;
            yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"overview.png"));
            gameplayZoom=2.35f;player=playerStart;cameraInitialized=false;
            yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"gameplay.png"));
            var targetShelf=sceneShelves[2];var shelfArt=targetShelf.transform.Find("Artist shelf fixture").GetComponent<SpriteRenderer>();
            player=ShopLayout.ToGame(targetShelf.transform.position+Vector3.up*.45f);cameraInitialized=false;
            yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();check(shop.playerCharacter.artwork.sortingOrder<shelfArt.sortingOrder,"Player is occluded when behind shelf");ScreenCapture.CaptureScreenshot(Path.Combine(directory,"behind-shelf.png"));
            player=ShopLayout.ToGame(targetShelf.transform.position+Vector3.down*.45f);cameraInitialized=false;
            yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();check(shop.playerCharacter.artwork.sortingOrder>shelfArt.sortingOrder,"Player draws in front when below shelf");ScreenCapture.CaptureScreenshot(Path.Combine(directory,"front-of-shelf.png"));
            helper=ShopLayout.ToGame(shop.partnerCheckoutSpot.position);player=ShopLayout.ToGame(shop.register.approachPoint.position);cameraInitialized=false;
            yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();
            check(!shop.partnerCharacter.artwork.bounds.Intersects(root.Find("Counter long top").GetComponent<SpriteRenderer>().bounds),"Staff sprite fits behind counter without intersecting tabletop");
            ScreenCapture.CaptureScreenshot(Path.Combine(directory,"counter.png"));
            player=supply+new Vector2(0,60);cameraInitialized=false;
            yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"storage.png"));
            player=ShopDoor;cameraInitialized=false;
            yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"entrance.png"));
            check(Walkable(ShopDoor),"Entrance remains walkable");
            File.WriteAllText(Path.Combine(directory,"art-verification.txt"),string.Join("\n",checks)+"\n\n"+checks.Count+" checks; "+failures+" failures.\n");
            gameplayZoom=2.35f;ReturnToMainMenu();testing=false;verificationRunning=false;
        }
    }
}
