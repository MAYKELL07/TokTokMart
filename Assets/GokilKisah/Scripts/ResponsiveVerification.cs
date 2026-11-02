using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        public void BeginResponsiveVerification(string directory){StartCoroutine(ResponsiveVerification(directory));}
        IEnumerator ResponsiveVerification(string directory)
        {
            Directory.CreateDirectory(directory);var checks=new List<string>();int failures=0;
            Action<bool,string> check=(ok,name)=>{checks.Add((ok?"PASS ":"FAIL ")+name);if(!ok)failures++;};
            testing=true;verificationRunning=true;settingsOpen=false;autoPaused=false;mainMenu=false;selected=true;
            BeginCampaignLevel(1,false);noticeUntil=0;
            var camera=shop.worldCamera;var rig=shop.testFollowCamera;
            check(shop.originalWorldCamera&&shop.originalWorldCamera!=camera&&!shop.originalWorldCamera.isActiveAndEnabled,"Original camera retained and inactive");
            check(rig&&rig.isActiveAndEnabled&&rig.GetComponent<Camera>()==camera,"Separate test follow camera active");
            check(UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length==1,"Only one active audio listener");
            Vector2 open=ShopLayout.ToGame(new Vector3(10,-10.2f,0));
            player=open;playerVelocity=Vector2.zero;ApplyResponsiveMovement(Vector2.right,tuning.moveSpeed,1f/60);
            check(player.x>open.x&&playerVelocity.x>0,"Movement starts on the first input frame");
            for(int i=0;i<3;i++)ApplyResponsiveMovement(Vector2.right,tuning.moveSpeed,1f/60);
            check(Mathf.Abs(playerVelocity.x-tuning.moveSpeed)<.01f,"Walking reaches full speed within 50 ms");
            var released=player;ApplyResponsiveMovement(Vector2.zero,tuning.moveSpeed,1f/60);
            check(player==released&&playerVelocity==Vector2.zero,"Releasing movement stops without drifting");
            playerVelocity=Vector2.right*tuning.moveSpeed;
            for(int i=0;i<4;i++)ApplyResponsiveMovement(Vector2.left,tuning.moveSpeed,1f/60);
            check(playerVelocity.x<-.95f*tuning.moveSpeed,"Direction reversal responds within four frames");
            player=open;playerVelocity=Vector2.zero;ApplyResponsiveMovement(Vector2.one,tuning.moveSpeed,1);
            check(playerVelocity.magnitude<=tuning.moveSpeed+.01f,"Diagonal input does not increase top speed");
            var distances=new List<float>();
            foreach(int fps in new[]{30,60,144})
            {
                player=open;playerVelocity=Vector2.zero;
                for(int i=0;i<fps;i++)ApplyResponsiveMovement(Vector2.right,tuning.moveSpeed,1f/fps);
                distances.Add(player.x-open.x);
                check(Walkable(player),"Responsive motion remains walkable at "+fps+" Hz");
            }
            check(Mathf.Abs(distances[0]-distances[2])<5,"Movement distance remains consistent across frame rates");
            player=ShopLayout.ToGame(new Vector3(.74f,-10.2f,0));playerVelocity=Vector2.zero;
            for(int i=0;i<30;i++)ApplyResponsiveMovement(new Vector2(-1,-1),tuning.moveSpeed,1f/60);
            check(Walkable(player)&&player.y<1020-40,"Collision slides along walls without penetrating");
            ResetRun(Encounter.Store);selected=true;mainMenu=false;
            player=supply+Vector2.down*76;actionInputFrame=-1;ActionInput();
            check(!carryingCrate&&bufferedActionUntil>=0,"Slightly early interaction is buffered");
            player=supply+Vector2.down*74;ProcessBufferedAction();
            check(carryingCrate&&bufferedActionUntil<0,"Buffered interaction fires as the player enters reach");
            actionInputFrame=-1;player=supply+Vector2.down*90;ActionInput();SetSettingsOpen(true);
            check(bufferedActionUntil<0,"Opening settings clears pending interaction");SetSettingsOpen(false);
            ResetRun(Encounter.Store);selected=true;mainMenu=false;
            camera.orthographicSize=gameplayZoom;
            Vector2 center=shop.walkableArea.bounds.center;
            rig.Follow(camera,center,Vector2.zero,shop.CameraBounds,1f/60,true);var start=camera.transform.position;
            for(int i=0;i<30;i++)rig.Follow(camera,center+Vector2.right*.6f,Vector2.zero,shop.CameraBounds,1f/60,false);
            check(camera.transform.position.x>start.x+.5f,"Follow camera responds to small player movements");
            var stationary=camera.transform.position;
            for(int i=0;i<90;i++)rig.Follow(camera,center+Vector2.right*.6f,Vector2.zero,shop.CameraBounds,1f/60,false);
            check(Vector3.Distance(stationary,camera.transform.position)<.01f,"Stationary camera settles without jitter");
            rig.Follow(camera,center,Vector2.zero,shop.CameraBounds,1f/60,true);
            for(int i=0;i<60;i++)rig.Follow(camera,center,Vector2.right*2.6f,shop.CameraBounds,1f/60,false);
            check(camera.transform.position.x>center.x+.3f,"Camera looks ahead along movement");
            for(int i=0;i<90;i++)rig.Follow(camera,center,Vector2.zero,shop.CameraBounds,1f/60,false);
            check(Mathf.Abs(camera.transform.position.x-center.x)<.01f,"Look-ahead recenters after stopping");
            rig.Follow(camera,new Vector2(-100,100),Vector2.zero,shop.CameraBounds,1f/60,true);
            var cb=shop.CameraBounds;float hx=camera.orthographicSize*camera.aspect;
            check(camera.transform.position.x>=cb.min.x+hx-.001f&&camera.transform.position.y<=cb.max.y-camera.orthographicSize+.001f,"Follow camera clamps its whole viewport");
            var shelf=sceneShelves[2];var fade=shelf.GetComponent<SoftOccluder>();
            player=ShopLayout.ToGame(shelf.transform.position+Vector3.up*.45f);shop.playerCharacter.Apply(player,24,0,Vector2.zero,pulse);
            fade.ResetOpacity();var body=shop.playerCharacter.artwork.bounds;var feet=(Vector2)shop.playerCharacter.transform.position;
            check(fade.DesiredOpacity(feet,body)<.3f,"Shelf detects player occlusion");
            var distantBody=body;distantBody.center+=Vector3.right*10;
            check(fade.DesiredOpacity(feet+Vector2.right*10,distantBody)>.99f,"Shelves outside the player silhouette remain solid");
            fade.Tick(feet,body,true,1f/60);check(fade.silhouette.color.a>.22f&&fade.silhouette.color.a<1,"Shelf fades progressively without an instant pop");
            for(int i=0;i<60;i++)fade.Tick(feet,body,true,1f/60);
            check(fade.silhouette.color.a<.24f,"Shelf becomes transparent enough to see the player");
            var far=feet+Vector2.down*3;
            for(int i=0;i<90;i++)fade.Tick(far,body,true,1f/60);
            check(fade.silhouette.color.a>.995f,"Shelf restores full opacity after leaving");
            var south=shop.transform.Find("Reference shop layout/Storage south wall fade").GetComponent<SoftOccluder>();
            var near=new Vector2(south.silhouette.bounds.center.x,-.1f);
            check(south.DesiredOpacity(near,body)<.25f,"South storage wall fades near the room side");
            check(south.DesiredOpacity(near+Vector2.up*3,body)>.99f,"South storage wall stays opaque when far away");
            check(south.DesiredOpacity(near+Vector2.down*2,body)>.99f,"South storage wall restores after exiting to the mart");
            for(int i=0;i<60;i++)south.Tick(near,body,true,1f/60);
            check(south.silhouette.color.a<.2f,"South wall transparency reaches its target smoothly");
            south.ResetOpacity();check(south.silhouette.color.a>.99f,"Opacity reset clears faded state");
            player=ShopLayout.ToGame(near);playerMotion=Vector2.zero;cameraInitialized=false;
            for(int i=0;i<35;i++)yield return null;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"storage-wall-near.png"));
            player=ShopLayout.ToGame(near+Vector2.up*2.3f);cameraInitialized=false;
            for(int i=0;i<70;i++)yield return null;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"storage-wall-restored.png"));
            player=ShopLayout.ToGame(shelf.transform.position+Vector3.up*.45f);cameraInitialized=false;
            for(int i=0;i<35;i++)yield return null;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"shelf-soft-fade.png"));
            player=playerStart;cameraInitialized=false;
            for(int i=0;i<10;i++)yield return null;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"follow-gameplay.png"));
            File.WriteAllText(Path.Combine(directory,"responsive-verification.txt"),string.Join("\n",checks)+"\n\n"+checks.Count+" checks; "+failures+" failures.\n");
            testing=false;verificationRunning=false;ReturnToMainMenu();
        }
    }
}
