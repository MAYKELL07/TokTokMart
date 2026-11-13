using System.Collections.Generic;
using UnityEngine;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        int timingMistakes,chosenLevel,mapPage,mapSelection=1,lastCountdown=-1;
        bool milestoneCleared,levelMapOpen,scoreGuideOpen,taskFeedbackFailed;
        float taskElapsed,taskLastAttempt=-1;
        const int LevelsPerPage=20;
        int HighestUnlocked=>Mathf.Max(1,PlayerPrefs.GetInt(UnlockSave,PlayerPrefs.GetInt(LevelSave,1)));
        string BestKey(int level,string value)=>"TokMart.Level."+level+"."+value;
        ShiftScore Rating(bool complete)=>new ShiftScore(Targets,served,stockActions,hazardsCleared,CurrentLevel%5==0,milestoneCleared,priceMistakes,timingMistakes,missedCustomers,shiftTime,chaos,complete);
        void SaveLevelBest()
        {
            var score=Rating(true);
            PlayerPrefs.SetInt(BestKey(CurrentLevel,"Stars"),Mathf.Max(PlayerPrefs.GetInt(BestKey(CurrentLevel,"Stars")),score.stars));
            PlayerPrefs.SetInt(BestKey(CurrentLevel,"Score"),Mathf.Max(PlayerPrefs.GetInt(BestKey(CurrentLevel,"Score")),score.total));
            PlayerPrefs.Save();
        }
        void OpenLevelMap()
        {chosenLevel=0;mapSelection=Mathf.Clamp(PlayerPrefs.GetInt(LevelSave,1),1,HighestUnlocked);mapPage=(mapSelection-1)/LevelsPerPage;levelMapOpen=true;}
        void DrawStars(Rect r,int earned,string id,int size=50)
        {using(NativeUiScope(id,"Three stars")){for(int i=0;i<3;i++){GUI.color=i<earned?Color.white:new Color(.25f,.30f,.29f,.45f);EditableTexture(new Rect(r.x+i*(size+9),r.y,size,size),InvoiceStar(),ScaleMode.ScaleToFit);}GUI.color=Color.white;}}
        static string ClockText(float seconds){int n=Mathf.CeilToInt(Mathf.Max(0,seconds));return (n/60)+":"+(n%60).ToString("00");}
        void DrawLevelMap()
        {using(NativeUiScope("November11LevelMap","Choose a level")){
            var bg=GameArtLibrary.UI("MenuBackground");if(bg)EditableTexture(new Rect(0,0,1600,900),bg,ScaleMode.ScaleAndCrop);
            Fill(new Rect(0,0,1600,900),new Color(.04f,.14f,.16f,.91f));
            ArcadeText(new Rect(75,33,950,80),"CHOOSE YOUR SHIFT",55,arcadeCream);
            Text(new Rect(80,117,1070,35),"Earn up to 3 stars. Clear a shift to unlock the next. Replay to beat your best!",23,arcadeCream);
            if(UiButton(new Rect(1295,53,220,60),"MAIN MENU",arcadeCream,23))levelMapOpen=false;
            int first=mapPage*LevelsPerPage+1;
            for(int i=0;i<LevelsPerPage;i++)
            {using(NativeUiScope("Level"+i,"Level tile "+(first+i))){
                int level=first+i;bool unlocked=level<=HighestUnlocked;var r=new Rect(80+(i%5)*196,185+(i/5)*146,178,129);
                Sticker(r,level==mapSelection?arcadeGold:unlocked?arcadeCream:new Color(.22f,.32f,.32f),true,4);
                ArcadeText(new Rect(r.x+12,r.y+8,154,40),unlocked?"SHIFT "+level.ToString("00"):"LOCKED",26,unlocked?arcadeNavy:muted,TextAnchor.MiddleCenter);
                DrawStars(new Rect(r.x+30,r.y+49,135,38),unlocked?PlayerPrefs.GetInt(BestKey(level,"Stars"),0):0,"TileStars",34);
                ArcadeText(new Rect(r.x+8,r.y+94,162,25),level%5==0?"BOSS SHIFT":unlocked?(PlayerPrefs.GetInt(BestKey(level,"Score"),0)>0?"BEST "+PlayerPrefs.GetInt(BestKey(level,"Score")):"READY TO OPEN"):"CLEAR SHIFT "+(level-1),14,unlocked?arcadeNavy:muted,TextAnchor.MiddleCenter);
                GUI.enabled=unlocked;if(InterfaceButton(r))mapSelection=level;GUI.enabled=true;
            }}
            Sticker(new Rect(1100,185,420,565),arcadeCream,true,4);
            var t=new ShiftTargets(mapSelection);int baseScore=ShiftScore.Base(t,mapSelection%5==0);
            ArcadeText(new Rect(1120,204,380,55),"SHIFT "+mapSelection.ToString("00"),37,arcadeNavy,TextAnchor.MiddleCenter);
            Text(new Rect(1130,273,360,92),"Serve "+t.serve+" customers  /  100 pts each\nRestock "+t.stock+" shelves  /  150 pts each\nClean "+t.clean+" spills  /  100 pts each",22,arcadeNavy);
            Text(new Rect(1130,376,360,51),mapSelection%5==0?"Boss: "+scenarioTitles[(int)MilestoneBoss(mapSelection)]+" (+200 pts)":"Time limit: "+ClockText(t.duration),21,arcadeTeal,FontStyle.Bold);
            for(int i=0;i<3;i++){DrawStars(new Rect(1134,443+i*56,112,30),i+1,"Requirement"+i,26);ArcadeText(new Rect(1260,440+i*56,220,34),(baseScore+(i==0?0:i==1?300:450))+" POINTS",23,arcadeNavy);}
            Text(new Rect(1130,624,360,36),"All objectives required for any star.",19,arcadeNavy);
            if(UiButton(new Rect(1130,677,360,52),"PLAY SHIFT "+mapSelection,arcadeGold,25)){chosenLevel=mapSelection;levelMapOpen=false;mainMenu=false;selected=false;}
            GUI.enabled=mapPage>0;if(UiButton(new Rect(80,798,180,55),"PREVIOUS",arcadeCream,22)){mapPage--;mapSelection=mapPage*20+1;}GUI.enabled=true;
            ArcadeText(new Rect(285,794,545,60),"SHIFTS "+first+" - "+(first+19)+"  /  UNLOCKED "+HighestUnlocked,23,arcadeCream,TextAnchor.MiddleCenter);
            GUI.enabled=first+20<=HighestUnlocked;if(UiButton(new Rect(860,798,180,55),"NEXT",arcadeCream,22)){mapPage++;mapSelection=mapPage*20+1;}GUI.enabled=true;
            if(UiButton(new Rect(1100,798,420,55),"HOW TO GET A PERFECT SCORE",arcadeGold,20))scoreGuideOpen=true;
            if(scoreGuideOpen)DrawScoreGuide();
        }}
        void DrawScoreGuide()
        {using(NativeUiScope("November11ScoreGuide","Scoring rules")){
            Shade();if(NativeUiReady)editableUI.Button(new Rect(0,0,1600,900),true,NativeMatrix);Sticker(new Rect(300,95,1000,708),arcadeCream,true,5);
            ArcadeText(new Rect(340,119,920,65),"THE ROAD TO 3 STARS",45,arcadeNavy,TextAnchor.MiddleCenter);
            Text(new Rect(360,206,880,69),"Clear every target and the required boss before closing to earn stars.\nMoney is separate. Only required tasks earn score.",23,arcadeNavy);
            string[] title={"OBJECTIVES","ACCURACY  /  UP TO 200","SPEED  /  UP TO 200","SHOP CONTROL  /  UP TO 100"};
            string[] body={"100 per customer + 150 per restock + 100 per cleanup; boss +200.","Start at 200. Wrong total or missed timing: -25. Lost customer: -100.","Finish within 70% of the shift: +200. Within 85%: +100. Later: +0.","End with chaos 0-15%: +100. Chaos 16-40%: +50. Higher: +0."};
            for(int i=0;i<4;i++){ArcadeText(new Rect(360,294+i*82,880,32),title[i],24,arcadeTeal);Text(new Rect(360,329+i*82,880,39),body[i],22,arcadeNavy);}
            Text(new Rect(360,631,880,48),"1 star = objective points. 2 stars = objectives +300. 3 stars = objectives +450.\nPERFECT = all objectives +500: no errors, finish early, chaos at 15% or less.",21,arcadeNavy,FontStyle.Bold,TextAnchor.MiddleCenter);
            if(UiButton(new Rect(555,719,490,57),"GOT IT - BACK TO THE SHOP",arcadeGold,25)){scoreGuideOpen=false;ReleaseMobileControls();heldKeys.Clear();}
        }}
        void DrawShiftBriefing()
        {using(NativeUiScope("November11Briefing","Shift targets and star requirements")){
            Shade();Sticker(new Rect(305,90,990,720),arcadeCream,true,5);
            ArcadeText(new Rect(340,108,920,75),"SHIFT "+CurrentLevel.ToString("00")+"  /  MAKE IT COUNT!",44,arcadeNavy,TextAnchor.MiddleCenter);
            Text(new Rect(370,199,860,50),"COMPLETE ALL TARGETS  /  CLOSING IN "+ClockText(ShiftBudget),25,arcadeTeal,FontStyle.Bold,TextAnchor.MiddleCenter);
            int[] goal={ServeGoal,StockGoal,CleanGoal};string[] labels={"SERVE","RESTOCK","CLEAN"};string[] icons={"CheckoutIcon01","stock","PuddleIcon01"};
            for(int i=0;i<3;i++){var r=new Rect(370+i*295,267,270,155);Panel(r,new Color(.90f,.91f,.83f));if(i==1)Icon(new Rect(r.x+15,r.y+25,64,64),"stock",arcadeTeal);else ArtImage(new Rect(r.x+15,r.y+25,64,64),icons[i]);ArcadeText(new Rect(r.x+90,r.y+16,165,55),goal[i]+" "+labels[i],26,arcadeNavy);ArcadeText(new Rect(r.x+15,r.y+104,240,35),i==1?"150 POINTS EACH":"100 POINTS EACH",20,arcadeTeal,TextAnchor.MiddleCenter);}
            var score=Rating(false);int[] thresholds={score.oneStar,score.twoStars,score.threeStars};
            for(int i=0;i<3;i++){DrawStars(new Rect(375+i*295,449,135,35),i+1,"BriefStars"+i,32);ArcadeText(new Rect(370+i*295,496,270,36),thresholds[i]+" POINTS",26,arcadeNavy,TextAnchor.MiddleCenter);}
            Text(new Rect(370,551,860,61),"PERFECT "+score.maximum+": no mistakes; finish by "+ClockText(Mathf.Floor(ShiftBudget*.70f))+" elapsed; chaos <=15%.\nPartner sales count. Complete the restock and cleanup targets yourself.",21,arcadeNavy,FontStyle.Normal,TextAnchor.MiddleCenter);
            Text(new Rect(370,626,860,36),mandatoryBossPending?"BOSS SHIFT: "+scenarioTitles[(int)MilestoneBoss(CurrentLevel)]+" - defeat the boss too (+200).":"Watch patience bars. Clean spills and serve customers to lower chaos.",20,arcadeTeal,FontStyle.Bold,TextAnchor.MiddleCenter);
            if(UiButton(new Rect(365,705,300,61),"SCORING RULES",arcadeCream,24))scoreGuideOpen=true;
            if(UiButton(new Rect(690,705,540,61),"OPEN SHIFT",arcadeGold,30))StartBriefedShift();
        }}
        void DrawArcadeResults()
        {using(NativeUiScope("November11Results","Shift score and stars")){
            Fill(new Rect(0,0,1600,900),new Color(.07f,.24f,.27f));
            ArcadeText(new Rect(180,20,1240,90),levelWon?"SHIFT COMPLETE!":"LET'S TRY THAT AGAIN",59,levelWon?arcadeGold:arcadeCoral,TextAnchor.MiddleCenter);
            var s=Rating(levelWon);Sticker(new Rect(255,125,1090,600),arcadeCream,true,5);
            DrawStars(new Rect(350,153,300,85),invoiceStars,"ResultStars",82);
            ArcadeText(new Rect(780,148,510,60),s.total+" / "+s.maximum+" POINTS",36,arcadeNavy,TextAnchor.MiddleRight);
            ArcadeText(new Rect(780,208,510,35),"SHIFT "+CurrentLevel+"  /  BEST "+PlayerPrefs.GetInt(BestKey(CurrentLevel,"Score"),0),22,arcadeTeal,TextAnchor.MiddleRight);
            string[] rows={"Objectives (capped at targets)","Accuracy: "+priceMistakes+" wrong totals, "+timingMistakes+" timing misses, "+missedCustomers+" lost","Speed: finished in "+ClockText(shiftTime)+" / "+ClockText(ShiftBudget),"Shop control: "+Mathf.RoundToInt(chaos)+"% chaos"};
            int[] amounts={s.objectives,levelWon?s.accuracy:0,levelWon?s.speed:0,levelWon?s.control:0};
            for(int i=0;i<4;i++){Text(new Rect(305,284+i*58,820,43),rows[i],23,arcadeNavy);ArcadeText(new Rect(1140,279+i*58,155,43),"+"+amounts[i],27,arcadeTeal,TextAnchor.MiddleRight);}
            for(int i=0;i<3;i++){DrawStars(new Rect(310+i*335,543,117,32),i+1,"ResultRequirements"+i,28);ArcadeText(new Rect(445+i*335,540,165,37),(i==0?s.oneStar:i==1?s.twoStars:s.threeStars)+" PTS",22,arcadeNavy);}
            string tip=!levelWon?levelFailure:s.total==s.maximum?"PERFECT SHIFT! All 500 bonus points earned.":s.speed<200?"Next time: finish by "+ClockText(Mathf.Floor(ShiftBudget*.70f))+" elapsed for the full speed bonus.":s.accuracy<200?"Next time: check receipt totals and hit the green zone. Accuracy starts at 200.":"Next time: clean spills and serve customers. End with chaos <=15% for +100.";
            Text(new Rect(305,604,990,48),tip,23,arcadeNavy,FontStyle.Bold,TextAnchor.MiddleCenter);
            Text(new Rect(305,668,990,32),"SHOP TAKINGS  "+Ringgit(InvoiceTotal)+"  /  Money does not change your stars.",21,arcadeTeal,FontStyle.Normal,TextAnchor.MiddleCenter);
            if(UiButton(new Rect(255,764,460,66),levelWon?"NEXT SHIFT "+(CurrentLevel+1):"RETRY SHIFT "+CurrentLevel,arcadeGold,27))BeginCampaignLevel(levelWon?CurrentLevel+1:CurrentLevel,campaignPersistence);
            if(UiButton(new Rect(735,764,285,66),"REPLAY",arcadeCream,26))BeginCampaignLevel(CurrentLevel,campaignPersistence);
            if(UiButton(new Rect(1040,764,305,66),"LEVEL SELECT",arcadeCream,25)){ReturnToMainMenu();OpenLevelMap();}
        }}
        void ResetShiftFeedback(){taskElapsed=0;taskLastAttempt=-1;lastCountdown=-1;taskFeedbackFailed=false;scoreGuideOpen=false;}
        float TaskBudget=>skillType==1?30:skillType==2?24:20;
        void RecordTaskMiss(string reason)
        {if(campaignActive)timingMistakes++;taskFeedbackFailed=true;skillFlash=Time.unscaledTime+.45f;taskFeedback=reason+". -25 accuracy points; try again.";PlaySound("Miss");}
        void TickShiftFeedback(float dt)
        {
            float left=ShiftBudget-shiftTime;
            if(skillType>=0){taskElapsed+=dt;if(taskElapsed>=TaskBudget){RecordTaskMiss("Task timed out");skillType=-1;Toast("Task timed out! -25 accuracy. Crate and spill kept; try again.");}}
            if(campaignActive&&left<=30&&left+dt>30)Toast("30 SECONDS TO CLOSING - finish your remaining targets!");
            if(campaignActive&&left<=10&&left+dt>10)Toast("FINAL 10 SECONDS!");
            int seconds=Mathf.CeilToInt(skillType>=0?TaskBudget-taskElapsed:encounter==Encounter.Bo&&!resolved&&!canKicked?boRemaining:left);
            int warning=skillType>=0||encounter==Encounter.Bo?5:10;
            if(seconds>0&&seconds<=warning&&seconds!=lastCountdown)PlaySound("Click",.65f);
            lastCountdown=seconds;
        }
        void DrawTaskCountdown(Rect r)
        {using(NativeUiScope("TaskDeadline","Task time remaining")){
            float left=Mathf.Max(0,TaskBudget-taskElapsed);Fill(r,new Color(.14f,.22f,.23f));Fill(new Rect(r.x,r.y,r.width*left/TaskBudget,r.height),left<=5?arcadeCoral:arcadeTeal);
            ArcadeText(new Rect(r.x,r.y-1,r.width,r.height+2),"TIME LEFT "+Mathf.CeilToInt(left)+"s",16,arcadeCream,TextAnchor.MiddleCenter);
        }}
        void DrawTaskDrive()
        {using(NativeUiScope("TaskDrive","Task progress and icon")){
            var r=new Rect(412,351,36,74);ArtImage(r,"ProgressBar01");
            float progress=(float)taskCombo/(skillType==0?3:4);Fill(new Rect(r.x+5,r.yMax-6-(r.height-12)*progress,r.width-10,(r.height-12)*progress),new Color(.41f,.86f,.18f));
            ArtImage(new Rect(r.x+7,r.y+23,22,28),skillType==0?"CheckoutIcon01":"PuddleIcon01");
        }}
        void HudCard(Rect r,string art,string label,int done,int goal)
        {using(NativeUiScope(label,"Task card "+label)){
            ArtImage(new Rect(r.x,r.y,r.width,r.height-28),art);Fill(new Rect(r.x+9,r.y+r.height-46,(r.width-18)*Mathf.Clamp01((float)done/Mathf.Max(1,goal)),9),done>=goal?arcadeTeal:new Color(.41f,.86f,.18f));
            ArcadeText(new Rect(r.x-10,r.yMax-24,r.width+20,24),label+" "+Mathf.Min(done,goal)+"/"+goal,14,arcadeCream,TextAnchor.MiddleCenter,true);
        }}
        void DrawNovemberHud()
        {using(NativeUiScope("November11HUD","Shift HUD")){
            var canvas=GUI.matrix;GUI.matrix=HudMatrix(canvas,new Vector2(26,24));
            ArtImage(new Rect(26,24,238,92),"DayTimer01");ArcadeText(new Rect(190,31,48,28),CurrentLevel.ToString(),19,arcadeNavy,TextAnchor.MiddleCenter);
            float left=Mathf.Max(0,ShiftBudget-shiftTime);ArcadeText(new Rect(96,65,154,39),ClockText(left),29,left<=30?arcadeCoral:arcadeCream,TextAnchor.MiddleCenter);
            ArcadeText(new Rect(26,111,238,22),"SHIFT "+CurrentLevel.ToString("00"),15,arcadeCream,TextAnchor.MiddleCenter,true);
            HudCard(new Rect(27,135,76,123),"CheckoutCard01","SERVE",served,ServeGoal);
            HudCard(new Rect(125,135,76,123),"PuddleCard01","CLEAN",hazardsCleared,CleanGoal);
            ArtImage(new Rect(223,135,76,95),"HazardCard01");Fill(new Rect(232,211,58*Mathf.Clamp01(chaos/100),9),chaos>60?arcadeCoral:arcadeGold);
            ArcadeText(new Rect(210,234,102,24),"CHAOS "+Mathf.RoundToInt(chaos)+"%",14,arcadeCream,TextAnchor.MiddleCenter,true);
            Panel(new Rect(26,274,282,50),arcadeCream);Icon(new Rect(34,282,33,33),"stock",arcadeTeal);ArcadeText(new Rect(76,279,225,37),"RESTOCK "+stockActions+" / "+StockGoal,20,arcadeNavy);
            GoalProgress(new Rect(34,316,266,4),stockActions,StockGoal);
            GUI.matrix=HudMatrix(canvas,new Vector2(1573,24));
            ArtImage(new Rect(1280,26,215,65),"CustomerCount01");ArcadeText(new Rect(1335,36,150,42),served+" / "+ServeGoal,25,arcadeNavy,TextAnchor.MiddleCenter);
            ArtImage(new Rect(1280,101,215,65),"MoneyCount01");ArcadeText(new Rect(1284,110,160,42),Ringgit(InvoiceTotal),22,arcadeNavy,TextAnchor.MiddleCenter);
            ArtImage(new Rect(1512,28,58,63),"PauseButton01");if(InterfaceButton(new Rect(1502,20,76,82)))SetSettingsOpen(true);
            GUI.matrix=canvas;
            Panel(new Rect(567,24,466,92),arcadeCream);var score=Rating(false);
            ArcadeText(new Rect(583,27,434,35),"SCORE "+score.objectives+"  /  3 STARS "+score.threeStars,24,arcadeNavy,TextAnchor.MiddleCenter);
            ArcadeText(new Rect(583,66,434,30),"+ ACCURACY, SPEED & CHAOS BONUSES  /  ?",14,arcadeTeal,TextAnchor.MiddleCenter);
            if(InterfaceButton(new Rect(567,24,466,92))){scoreGuideOpen=true;ReleaseMobileControls();heldKeys.Clear();}
            if(customerQueue.Count>0){float soonest=CustomerPatience;foreach(var c in customerQueue)soonest=Mathf.Min(soonest,CustomerPatience-c.timer);Panel(new Rect(1190,194,380,48),soonest<15?arcadeCoral:arcadeNavy);ArcadeText(new Rect(1202,201,356,33),"QUEUE "+customerQueue.Count+"  /  PATIENCE "+Mathf.CeilToInt(Mathf.Max(0,soonest))+"s",21,arcadeCream,TextAnchor.MiddleCenter);}
            if(encounter!=Encounter.Store){Panel(new Rect(430,137,1140,49),resolved?arcadeTeal:arcadeCoral);ArcadeText(new Rect(445,143,1110,36),resolved?"ALL CLEAR!":encounter==Encounter.Bo?"KICK THE CAN! "+boRemaining.ToString("0.0")+"s":ObjectiveText(),21,arcadeCream,TextAnchor.MiddleCenter);}
            if(!showTouchControls){Sticker(new Rect(520,804,560,61),arcadeCream);Keycap(new Rect(531,812,46,43),"E");ArcadeText(new Rect(591,811,473,43),InteractionLabel(),22,arcadeNavy);if(InterfaceButton(new Rect(520,804,560,61)))ActionInput();}
            else{Panel(new Rect(548,817,504,50),arcadeNavy);ArcadeText(new Rect(561,821,478,40),InteractionLabel(),20,arcadeCream,TextAnchor.MiddleCenter);}
            Fill(new Rect(640,882,320,7),arcadeNavy);Fill(new Rect(640,882,320*Mathf.Clamp01(Stamina/maxStamina),7),arcadeGold);
        }}
    }
}
