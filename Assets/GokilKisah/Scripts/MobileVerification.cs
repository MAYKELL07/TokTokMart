using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        Vector2 CanvasToScreen(Vector2 p)
        {
            float scale=Mathf.Min(Screen.width/1600f,Screen.height/900f);
            return new Vector2((Screen.width-1600*scale)/2+p.x*scale,Screen.height-((Screen.height-900*scale)/2+p.y*scale));
        }
        IEnumerator TapInterface(Touchscreen device,Vector2 point)
        {
            var position=CanvasToScreen(point);
            InputSystem.QueueStateEvent(device,new TouchState{touchId=502,phase=UnityEngine.InputSystem.TouchPhase.Began,position=position});
            yield return null;yield return null;
            InputSystem.QueueStateEvent(device,new TouchState{touchId=502,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=position});
            yield return null;yield return null;yield return new WaitForEndOfFrame();
            // Retained Canvas binds the new screen during Repaint, after input dispatch.
            // Wait for that visible layout before the next simulated pointer-down.
            yield return null;yield return new WaitForEndOfFrame();
        }
        public void BeginMobileVerification(string directory)=>StartCoroutine(MobileVerification(directory));
        IEnumerator MobileVerification(string directory)
        {
            Directory.CreateDirectory(directory);var checks=new List<string>();
            Action<bool,string> check=(ok,label)=>checks.Add((ok?"PASS ":"FAIL ")+label);
            testing=true;verificationRunning=true;autoPaused=false;settingsOpen=false;campaignPersistence=false;
            // Keep the user's settings intact after testing persistence and layout extremes.
            var saved=new[]{leftControlScale,rightControlScale,leftControlPadding,rightControlPadding,leftControlLift,rightControlLift};
            bool savedTouch=showTouchControls;int tab=settingsTab;
            var screen=InputSystem.AddDevice<Touchscreen>();
            ReturnToMainMenu();yield return null;
            yield return TapInterface(screen,new Vector2(325,500));
            check(settingsOpen,"Input System touch opens menu settings without legacy IMGUI input");
            yield return TapInterface(screen,new Vector2(410,510));
            check(settingsTab==4,"Touch selects Controls settings tab");
            yield return TapInterface(screen,new Vector2(1050,300));
            check(Mathf.Abs(leftControlScale-1.045f)<.03f,"Touch updates the joystick scale slider");
            yield return TapInterface(screen,new Vector2(1250,155));
            check(!settingsOpen,"Touch closes settings");
            yield return TapInterface(screen,new Vector2(325,405));
            check(!mainMenu&&!selected,"Touch opens character selection");
            yield return TapInterface(screen,new Vector2(560,638));
            check(selected&&!zack,"Touch selects Sam");
            InputSystem.RemoveDevice(screen);
            foreach(string name in new[]{"CheckoutIcon01","CustomerCount01","DayTimer01","HazardIcon01","IndicatorContainer","InteractButton01","MoneyCount01","PauseButton01","PuddleIcon01","SprintButton01","JoystickBase","JoystickKnob","Logo","MenuBackground"})
                check(NovemberArt.UI(name),"Artist UI loaded: "+name);
            foreach(string skin in new[]{"Sam","Zack","Toyol"})for(int i=0;i<9;i++)
            {var sprite=NovemberArt.Walk(skin,i);check(sprite&&sprite.pivot.y==0,"Walk frame with foot pivot: "+skin+i);}
            ReturnToMainMenu();yield return new WaitForSecondsRealtime(.3f);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"menu.png"));
            BeginCampaignLevel(1,false);selected=true;showTouchControls=true;noticeUntil=0;
            yield return new WaitForSecondsRealtime(.5f);
            ControlRects(out var stick,out var action,out var sprint);
            MobilePointer(101,stick.center+new Vector2(stick.width*.3f,0),true,true);
            MobilePointer(102,sprint.center,true,true);
            check(touchMove.x>.7f&&touchSprint,"Independent joystick and held sprint fingers");
            var before=player;ApplyResponsiveMovement(touchMove,MovementSpeed(touchSprint,true),.016f);
            check(player.x>before.x,"Joystick moves player through normal collision path");
            var shopBefore=stockActions;player=supply;carryingCrate=false;skillType=-1;
            MobilePointer(103,action.center,true,true);check(carryingCrate,"Third finger interacts while joystick and sprint are held");
            MobilePointer(101,Vector2.zero,false,false);MobilePointer(102,Vector2.zero,false,false);
            check(touchMove==Vector2.zero&&!touchSprint,"Release clears movement and sprint");
            MobilePointer(101,stick.center,true,true);MobilePointer(102,sprint.center,true,true);SetSettingsOpen(true);
            check(stickFinger<0&&sprintFinger<0&&!touchSprint&&touchMove==Vector2.zero,"Pause cancels held touch input");SetSettingsOpen(false);
            leftControlScale=1.4f;rightControlScale=.7f;leftControlPadding=260;rightControlPadding=190;leftControlLift=220;rightControlLift=160;
            SaveControlSettings();leftControlScale=rightControlScale=1;leftControlPadding=rightControlPadding=0;leftControlLift=rightControlLift=0;LoadControlSettings();
            check(leftControlScale==1.4f&&rightControlScale==.7f&&leftControlPadding==260&&rightControlPadding==190&&leftControlLift==220&&rightControlLift==160,"Scale, both paddings and both heights survive preferences reload");
            ControlRects(out stick,out action,out sprint);var safe=ControlSafeArea();
            check(safe.Contains(stick.min)&&safe.Contains(stick.max-Vector2.one*.01f)&&safe.Contains(action.min)&&safe.Contains(action.max-Vector2.one*.01f),"Adjusted controls stay in screen safe area");
            settingsTab=4;SetSettingsOpen(true);yield return new WaitForSecondsRealtime(.3f);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"settings-adjusted.png"));
            leftControlScale=rightControlScale=1;leftControlPadding=rightControlPadding=42;leftControlLift=rightControlLift=38;
            SetSettingsOpen(false);player=playerStart;helper=player+new Vector2(120,0);cameraInitialized=false;
            yield return new WaitForSecondsRealtime(.5f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"mobile-gameplay.png"));
            ResetRun(Encounter.Goblins);selected=true;mainMenu=false;yield return new WaitForSecondsRealtime(.3f);
            check(shop.goblinTemplate.smoothSkin=="Toyol","Goblin encounter uses supplied Toyol animation");
            foreach(var view in goblinViews)check(view.artwork.sprite&&view.artwork.sprite.name.StartsWith("Toyol"),"Live Toyol encounter view");
            foreach(var actor in new[]{shop.playerCharacter,shop.partnerCharacter})
            {
                string skin=actor==shop.playerCharacter?(zack?"Zack":"Sam"):(zack?"Sam":"Zack");
                var origin=actor.LastGamePosition;actor.ResetLocomotion(origin);
                actor.Apply(origin,24,skin=="Sam"?0:6,Vector2.zero,0);
                var first=actor.artwork.sprite;
                actor.Apply(origin+Vector2.right*(actor.walkCycleDistance/NovemberArt.WalkFrameCount(skin)*1.1f),24,skin=="Sam"?0:6,Vector2.one,.1f);
                check(first==NovemberArt.Idle(skin)&&actor.artwork.sprite==NovemberArt.Walk(skin,1),"Keeper advances supplied animation frames: "+actor.name);
            }
            player=ShopLayout.ToGame(shop.goblinSpawns[0].position);cameraInitialized=false;
            yield return new WaitForSecondsRealtime(.3f);yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"toyol-encounter.png"));
            leftControlScale=saved[0];rightControlScale=saved[1];leftControlPadding=saved[2];rightControlPadding=saved[3];leftControlLift=saved[4];rightControlLift=saved[5];showTouchControls=savedTouch;settingsTab=tab;SaveSettings();
            File.WriteAllLines(Path.Combine(directory,"results.txt"),checks);
            testing=false;verificationRunning=false;ReturnToMainMenu();
        }
    }
}
