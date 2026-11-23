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
        void DrawScoreGuide()
        {using(NativeUiScope("StarHint","Star hint")){
            Shade();if(NativeUiReady)editableUI.Button(new Rect(0,0,1600,900),true,NativeMatrix);
            Sticker(new Rect(440,245,720,410),arcadeCream,true,5);
            DrawStars(new Rect(660,274,280,78),3,"HintStars",76);
            ArcadeText(new Rect(470,375,660,55),"MAKE IT A GREAT SHIFT",35,arcadeNavy,TextAnchor.MiddleCenter);
            Text(new Rect(490,449,620,75),"Finish your goals. Be quick and accurate.\nKeep the shop tidy for more stars!",26,arcadeNavy,FontStyle.Normal,TextAnchor.MiddleCenter);
            if(UiButton(new Rect(610,558,380,60),"GOT IT",arcadeGold,28)){scoreGuideOpen=false;ReleaseMobileControls();heldKeys.Clear();}
        }}
        void DrawShiftBriefing()
        {using(NativeUiScope("ShiftBrief","Shift briefing")){
            Shade();Sticker(new Rect(305,166,990,566),arcadeCream,true,5);
            ArcadeText(new Rect(340,187,920,65),"SHIFT "+CurrentLevel.ToString("00")+"  /  "+Targets.Title,42,arcadeNavy,TextAnchor.MiddleCenter);
            Text(new Rect(370,271,860,58),Targets.Hint,25,arcadeTeal,FontStyle.Bold,TextAnchor.MiddleCenter);
            int[] goal={ServeGoal,StockGoal,CleanGoal};string[] labels={"SERVE","RESTOCK","CLEAN"};string[] icons={"CheckoutIcon01","StockCard01","PuddleIcon01"};
            for(int i=0;i<3;i++){var r=new Rect(370+i*295,355,270,135);Panel(r,new Color(.90f,.91f,.83f));ArtImage(new Rect(r.x+20,r.y+25,64,64),icons[i]);ArcadeText(new Rect(r.x+99,r.y+12,150,55),goal[i].ToString(),42,arcadeNavy,TextAnchor.MiddleCenter);ArcadeText(new Rect(r.x+90,r.y+74,165,35),labels[i],25,arcadeTeal,TextAnchor.MiddleCenter);}
            ArtImage(new Rect(397,523,42,42),"TimerIcon01",uiKey:"ClosingTimerIcon");
            Text(new Rect(370,524,860,42),"CLOSING IN "+ClockText(ShiftBudget)+(mandatoryBossPending?"  /  BOSS: "+scenarioTitles[(int)MilestoneBoss(CurrentLevel)]:"  /  QUICK, ACCURATE, TIDY!"),23,arcadeNavy,FontStyle.Bold,TextAnchor.MiddleCenter);
            if(UiButton(new Rect(520,617,560,70),"LET'S OPEN!",arcadeGold,32))StartBriefedShift();
        }}
        void ResetShiftFeedback(){taskElapsed=0;taskLastAttempt=-1;lastCountdown=-1;taskFeedbackFailed=false;scoreGuideOpen=false;}
        float TaskBudget=>skillType==1?30:24;
        void RecordTaskMiss(string reason)
        {if(campaignActive)timingMistakes++;taskFeedbackFailed=true;skillFlash=Time.unscaledTime+.45f;taskFeedback=reason+". Try again!";PlaySound("Miss");}
        void TickShiftFeedback(float dt)
        {
            float left=ShiftBudget-shiftTime;
            if(skillType>0){taskElapsed+=dt;if(taskElapsed>=TaskBudget){RecordTaskMiss("Task timed out");skillType=-1;Toast("Task timed out");}}
            if(campaignActive&&left<=30&&left+dt>30)Toast("Closing in 30s");
            if(campaignActive&&left<=10&&left+dt>10)Toast("Closing in 10s");
            int seconds=Mathf.CeilToInt(skillType>0?TaskBudget-taskElapsed:encounter==Encounter.Bo&&!resolved&&!canKicked?boRemaining:left);
            int warning=skillType>0||encounter==Encounter.Bo?5:10;
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
            var progress=new Rect(417,357,26,62);
            using(NativeUiScope("","Fill"))
            {
                float amount=Mathf.Clamp01(taskCombo/4f);
                float height=progress.height*amount;
                EditableTextureUV(new Rect(progress.x,progress.yMax-height,progress.width,height),GameArtLibrary.UI("ProgressBar01a"),new Rect(0,0,1,amount));
            }
            ArtImage(new Rect(r.x+7,r.y+23,22,28),"PuddleIcon01");
        }}
        // The artwork exposes a transparent progress slot at these normalized coordinates.
        static Rect CardProgress(Rect card,float amount)
        {
            const float left=13f/221,right=208f/221,top=245f/274,bottom=259f/274;
            return new Rect(card.x+card.width*left,card.y+card.height*top,
                card.width*(right-left)*Mathf.Clamp01(amount),card.height*(bottom-top));
        }
        void HudCard(Rect r,string art,string label,int done,int goal)
        {using(NativeUiScope(label,"Task card "+label)){
            var card=new Rect(r.x,r.y,r.width,r.height-28);
            ArtImage(card,art);
            Fill(CardProgress(card,(float)done/Mathf.Max(1,goal)),done>=goal?arcadeTeal:new Color(.41f,.86f,.18f));
            ArcadeText(new Rect(r.x-10,r.yMax-24,r.width+20,24),label+" "+Mathf.Min(done,goal)+"/"+goal,14,arcadeCream,TextAnchor.MiddleCenter,true);
        }}
        void DrawNovemberHud()
        {using(NativeUiScope("ShiftHud","Shift HUD")){
            var canvas=GUI.matrix;GUI.matrix=HudMatrix(canvas,new Vector2(26,24));
            ArtImage(new Rect(26,24,238,92),"DayTimer01");ArcadeText(new Rect(190,31,48,28),CurrentLevel.ToString(),19,arcadeNavy,TextAnchor.MiddleCenter);
            float left=Mathf.Max(0,ShiftBudget-shiftTime);ArcadeText(new Rect(96,65,154,39),ClockText(left),29,left<=30?arcadeCoral:arcadeCream,TextAnchor.MiddleCenter);
            ArcadeText(new Rect(26,111,238,22),"SHIFT "+CurrentLevel.ToString("00"),15,arcadeCream,TextAnchor.MiddleCenter,true);
            HudCard(new Rect(27,135,76,123),"CheckoutCard01","SERVE",served,ServeGoal);
            HudCard(new Rect(125,135,76,123),"PuddleCard01","CLEAN",hazardsCleared,CleanGoal);
            var hazardCard=new Rect(223,135,76,95);
            ArtImage(hazardCard,"HazardCard01");Fill(CardProgress(hazardCard,chaos/100),chaos>60?arcadeCoral:arcadeGold);
            ArcadeText(new Rect(210,234,102,24),"CHAOS "+Mathf.RoundToInt(chaos)+"%",14,arcadeCream,TextAnchor.MiddleCenter,true);
            Panel(new Rect(26,274,282,50),arcadeCream);ArtImage(new Rect(34,282,33,33),"StockIcon01");ArcadeText(new Rect(76,279,225,37),"RESTOCK "+stockActions+" / "+StockGoal,20,arcadeNavy);
            GoalProgress(new Rect(34,316,266,4),stockActions,StockGoal);
            GUI.matrix=HudMatrix(canvas,new Vector2(1573,24));
            ArtImage(new Rect(1280,26,215,65),"CustomerCount01");ArcadeText(new Rect(1335,36,150,42),served+" / "+ServeGoal,25,arcadeNavy,TextAnchor.MiddleCenter);
            ArtImage(new Rect(1280,101,215,65),"MoneyCount01");ArcadeText(new Rect(1284,110,160,42),Ringgit(InvoiceTotal),22,arcadeNavy,TextAnchor.MiddleCenter);
            ArtImage(new Rect(1512,28,58,63),"PauseButton01");if(InterfaceButton(new Rect(1502,20,76,82)))SetSettingsOpen(true);
            GUI.matrix=canvas;
            bool hasNotice=Time.unscaledTime<noticeUntil&&!string.IsNullOrEmpty(notice);
            Panel(new Rect(606,23,388,49),hasNotice?arcadeNavy:arcadeCream);
            ArcadeText(new Rect(618,26,364,43),hasNotice?notice:Targets.Title,21,
                hasNotice?arcadeCream:arcadeNavy,TextAnchor.MiddleCenter);
            if(customerQueue.Count>0){using(NativeUiScope("Queue","Queue patience")){float soonest=CustomerPatience;foreach(var c in customerQueue)soonest=Mathf.Min(soonest,CustomerPatience-c.timer);Panel(new Rect(1190,218,380,48),soonest<15?arcadeCoral:arcadeNavy);ArcadeText(new Rect(1202,225,356,33),"QUEUE "+customerQueue.Count+"  /  "+Mathf.CeilToInt(Mathf.Max(0,soonest))+"s",21,arcadeCream,TextAnchor.MiddleCenter);}}
            if(encounter!=Encounter.Store){using(NativeUiScope("Encounter","Encounter status")){Panel(new Rect(430,137,740,49),resolved?arcadeTeal:arcadeCoral);ArcadeText(new Rect(445,143,710,36),resolved?"ALL CLEAR!":ActiveEncounterObjective(),21,arcadeCream,TextAnchor.MiddleCenter);}}
            GUI.matrix=HudMatrix(canvas,new Vector2(26,24));
            using(NativeUiScope("Stamina","Stamina")){Fill(new Rect(34,333,266,6),arcadeNavy);Fill(new Rect(34,333,266*Mathf.Clamp01(Stamina/maxStamina),6),arcadeGold);}
            GUI.matrix=canvas;
        }}
    }
}
