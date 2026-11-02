using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        public void BeginPolishVerification(string directory)=>StartCoroutine(VerifyMovementAndShifts(directory));
        bool FindPreviewRoute(out Vector2 start)
        {
            var bounds=shop.walkableArea.bounds;
            for(float y=bounds.min.y+1.8f;y<bounds.max.y-.4f;y+=.35f)
                for(float x=bounds.min.x+.4f;x+7.2f<bounds.max.x-.3f;x+=.5f)
                {
                    bool clear=true;
                    for(int i=0;i<=72&&clear;i++)clear=Walkable(ShopLayout.ToGame(new Vector3(x+i*.1f,y,0)));
                    if(clear){start=ShopLayout.ToGame(new Vector3(x,y,0));return true;}
                }
            start=playerStart;return false;
        }
        IEnumerator VerifyMovementAndShifts(string directory)
        {
            Directory.CreateDirectory(directory);var results=new List<string>();
            Action<bool,string> check=(ok,label)=>results.Add((ok?"PASS ":"FAIL ")+label);
            var randomState=UnityEngine.Random.state;bool savedTouch=showTouchControls;
            testing=true;verificationRunning=true;settingsOpen=false;autoPaused=false;shiftBriefOpen=false;Time.timeScale=1;
            var probe=Instantiate(shop.playerCharacter,runtimeObjects);probe.name="Locomotion check";probe.smoothSkin="Sam";
            foreach(string skin in new[]{"Sam","Zack","Toyol"})
            {
                bool valid=true;int count=NovemberArt.WalkFrameCount(skin);
                for(int i=0;i<count;i++)
                {
                    var sprite=NovemberArt.Walk(skin,i);
                    valid&=sprite&&sprite.pivot.y==0&&sprite.rect.xMax<=sprite.texture.width&&sprite.rect.yMax<=sprite.texture.height;
                }
                check(valid&&NovemberArt.Idle(skin),"Full high-frame-rate sequence and idle pose: "+skin+" ("+count+" frames)");
            }
            float referencePhase=0,walkDistance=0;
            foreach(int fps in new[]{30,60,120})
            {
                probe.ResetLocomotion(Vector2.zero);
                for(int i=1;i<=fps;i++)probe.Apply(Vector2.right*(260f*i/fps),26,0,Vector2.right,100+i);
                check(Mathf.Abs(probe.TravelledDistance-260)<.02f&&probe.FootstepContacts==2,"One second of walking has consistent travel and footfalls at "+fps+" fps");
                if(fps==30){referencePhase=probe.CyclePhase;walkDistance=probe.TravelledDistance;}
                else check(Mathf.Abs(referencePhase-probe.CyclePhase)<.0001f,"Gait phase agrees with 30 fps at "+fps+" fps");
            }
            probe.ResetLocomotion(Vector2.zero);
            for(int i=1;i<=60;i++)probe.Apply(Vector2.right*(260*sprintMultiplier*i/60),26,0,Vector2.right,i);
            check(Mathf.Abs(probe.TravelledDistance/walkDistance-sprintMultiplier)<.001f&&probe.FootstepContacts==4,"Sprint cadence and footfalls follow the actual 1.65x ground speed");
            float phase=probe.CyclePhase,travel=probe.TravelledDistance;
            for(int i=0;i<60;i++)probe.Apply(Vector2.right*travel,26,0,Vector2.one,i*10);
            check(probe.TravelledDistance==travel&&Mathf.Abs(probe.CyclePhase-phase)<.0001f&&probe.artwork.sprite==NovemberArt.Idle("Sam"),"Stopped or wall-blocked movement holds gait phase and shows a proper idle pose");
            probe.ResetLocomotion(Vector2.zero);probe.Apply(Vector2.right*60,26,0,Vector2.right,0);phase=probe.CyclePhase;
            probe.Apply(Vector2.right*70,26,0,Vector2.right,500);
            check(Mathf.Abs(probe.CyclePhase-phase-10/probe.walkCycleDistance)<.0001f,"Changing speed or global clock keeps the current stride phase");
            Time.timeScale=0;phase=probe.CyclePhase;var pausedSprite=probe.artwork.sprite;
            probe.Apply(Vector2.right*70,26,0,Vector2.one,900);
            check(probe.CyclePhase==phase&&probe.artwork.sprite==pausedSprite,"Pause freezes the current animation frame");Time.timeScale=1;
            probe.Apply(Vector2.right*400,26,0,Vector2.one,901);
            check(probe.TravelledDistance==0&&probe.FootstepContacts==0,"Teleport does not produce false strides or footstep sounds");
            probe.Apply(Vector2.right*410,26,0,Vector2.one,0);check(probe.artwork.flipX,"Rightward travel mirrors the supplied walk");
            probe.Apply(Vector2.right*400,23,0,Vector2.one,0);check(!probe.artwork.flipX,"Leftward travel restores the supplied facing");Destroy(probe.gameObject);

            var curve=new List<string>{"shift,customers,restocks,cleanups,seconds,required_work_per_second"};float pressure=0;int previousServed=0;
            for(int level=1;level<=40;level++)
            {
                var target=new ShiftTargets(level);float nextPressure=target.workloadSeconds/target.duration;
                check(target.serve>previousServed&&nextPressure>pressure,"Shift "+level+" raises its customer goal and workload per available second");
                pressure=nextPressure;previousServed=target.serve;
                if(level<=20)curve.Add(level+","+target.serve+","+target.stock+","+target.clean+","+target.duration+","+nextPressure.ToString("F4",System.Globalization.CultureInfo.InvariantCulture));
                BeginCampaignLevel(level,false);
                check(spills.Count>=Mathf.Min(2,CleanGoal),"Shift "+level+" opens with reachable cleanup work");
            }
            File.WriteAllLines(Path.Combine(directory,"shift-curve.csv"),curve);
            BeginCampaignLevel(13,false);bool cleanupAvailable=true;
            while(hazardsCleared<CleanGoal)
            {
                if(spills.Count==0){cleanupAvailable=false;break;}
                spills.RemoveAt(0);hazardsCleared++;EnsureShiftCleanupWork();
            }
            check(cleanupAvailable&&spills.Count==0,"Later cleanup goals replenish reachable work and stop replenishing once complete");
            BeginCampaignLevel(4,false);int initialStock=TotalStock();helper=ShopLayout.ToGame(shop.partnerCheckoutSpot.position);
            for(int i=0;i<80;i++)UpdateAI(.1f);
            check(TotalStock()==initialStock&&aiStatus.Contains("checkout"),"Partner covers checkout and preserves required player restock work");
            stockActions=StockGoal;for(int i=0;i<1000;i++)UpdateAI(.1f);
            check(TotalStock()>initialStock,"Partner resumes stocking after the player's restock target is met");
            for(int missing=0;missing<3;missing++)
            {
                BeginCampaignLevel(2,false);served=ServeGoal;stockActions=StockGoal;hazardsCleared=CleanGoal;
                if(missing==0)served--;else if(missing==1)stockActions--;else hazardsCleared--;
                TickCampaign(0,true);check(!levelResults,"Shift cannot complete with missing "+new[]{"customers","restocks","cleanup"}[missing]);
            }
            BeginCampaignLevel(2,false);served=ServeGoal;stockActions=StockGoal;hazardsCleared=CleanGoal;TickCampaign(0,true);
            check(levelResults&&levelWon,"All shift goals open the winning results");
            BeginCampaignLevel(1,false);showTouchControls=true;shiftBriefOpen=true;noticeUntil=0;
            testing=false;verificationRunning=false;yield return new WaitForSecondsRealtime(.2f);
            check(shiftTime==0&&Time.timeScale==0,"Briefing freezes the shift timer before opening");
            player=supply;ActionInput();check(!carryingCrate,"Briefing blocks gameplay input");player=playerStart;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"shift-one-briefing.png"));
            testing=true;verificationRunning=true;var touch=InputSystem.AddDevice<Touchscreen>();
            yield return TapInterface(touch,new Vector2(800,705));
            check(!shiftBriefOpen&&Time.timeScale==1,"Touch opens the briefed shift");InputSystem.RemoveDevice(touch);
            BeginCampaignLevel(10,false);shiftBriefOpen=true;noticeUntil=0;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"shift-ten-briefing.png"));
            StartBriefedShift();BeginCampaignLevel(1,false);showTouchControls=true;noticeUntil=0;
            bool routeFound=FindPreviewRoute(out var previewStart);player=previewStart;helper=helperStart;cameraInitialized=false;
            lastPlayer=player;playerMotion=Vector2.zero;playerFacing=26;
            check(routeFound&&Walkable(player),"Recorded movement starts on a clear route inside the actual shop geometry");
            shop.playerCharacter.ResetLocomotion(player);lastFootstepContact=0;playerVelocity=Vector2.zero;
            var frameDir=Path.Combine(directory,"frames");Directory.CreateDirectory(frameDir);
            int animatedFrames=0;
            for(int i=0;i<100;i++)
            {
                float speed=i<30?tuning.moveSpeed:i<60?tuning.moveSpeed*sprintMultiplier:0;
                ApplyResponsiveMovement(speed>0?Vector2.right:Vector2.zero,speed,1f/30);IsSprinting=i>=30&&i<60;
                TrackArtistMotion();yield return new WaitForEndOfFrame();
                if(i<60&&shop.playerCharacter.artwork.sprite!=NovemberArt.Idle(zack?"Zack":"Sam"))animatedFrames++;
                ScreenCapture.CaptureScreenshot(Path.Combine(frameDir,i.ToString("D3")+".png"));yield return null;
            }
            check(Vector2.Distance(player,previewStart)>650&&Walkable(player)&&animatedFrames>=58,"Live recorded walk/run covers the expected ground distance with animated poses");
            File.WriteAllLines(Path.Combine(directory,"results.txt"),results);
            showTouchControls=savedTouch;UnityEngine.Random.state=randomState;testing=false;verificationRunning=false;ReturnToMainMenu();
        }
    }
}
