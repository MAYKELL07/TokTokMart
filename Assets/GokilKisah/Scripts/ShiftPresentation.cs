using UnityEngine;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        bool shiftBriefOpen;
        int notifiedShiftGoals,lastFootstepContact;
        void StartBriefedShift()
        {
            shiftBriefOpen=false;noticeUntil=0;bufferedActionUntil=-1;
            ReleaseMobileControls();heldKeys.Clear();Time.timeScale=settingsOpen||autoPaused?0:1;
            PlaySound("Confirm");
        }
        void DrawShiftBriefing()
        {
            Shade();Sticker(new Rect(345,128,910,640),arcadeCream,true,6);
            Panel(new Rect(351,134,898,111),arcadeTeal);
            ArcadeText(new Rect(380,145,840,74),"SHIFT "+CurrentLevel.ToString("00"),52,arcadeCream,TextAnchor.MiddleCenter);
            string[] labels={"SERVE CUSTOMERS","RESTOCK SHELVES","CLEAN SPILLS"};
            string[] icons={"CustomerCount01","CheckoutIcon01","PuddleIcon01"};int[] goals={ServeGoal,StockGoal,CleanGoal};
            for(int i=0;i<3;i++)
            {
                float y=277+i*90;
                if(i==0)GUI.DrawTextureWithTexCoords(new Rect(398,y,54,64),NovemberArt.UI("CustomerCount01"),new Rect(0,0,.28f,1));
                else ArtImage(new Rect(393,y,64,64),icons[i]);
                ArcadeText(new Rect(480,y,555,60),labels[i],28,arcadeNavy);
                ArcadeText(new Rect(1060,y,135,60),goals[i].ToString(),40,arcadeTeal,TextAnchor.MiddleCenter);
            }
            ArcadeText(new Rect(390,556,820,36),"CLOSING IN  "+((int)ShiftBudget/60)+":"+((int)ShiftBudget%60).ToString("00"),26,arcadeNavy,TextAnchor.MiddleCenter);
            string note=mandatoryBossPending?"BOSS SHIFT: "+scenarioTitles[(int)MilestoneBoss(CurrentLevel)]+" — clear the encounter too.":"Your partner covers checkout while you tackle restocking.";
            Text(new Rect(390,602,820,48),note,20,arcadeNavy,FontStyle.Normal,TextAnchor.MiddleCenter);
            if(UiButton(new Rect(448,675,704,65),"OPEN SHIFT",arcadeGold,30))StartBriefedShift();
        }
        void EnsureShiftCleanupWork()
        {
            if(!campaignActive||sceneShelves.Length==0)return;
            int desired=Mathf.Min(2,Mathf.Max(0,CleanGoal-hazardsCleared));
            for(int i=0;i<sceneShelves.Length*3&&spills.Count<desired;i++)
            {
                var offset=i%3==0?new Vector2(0,70):i%3==1?new Vector2(-70,45):new Vector2(70,45);
                var p=ShelfPoint((i/3+hazardsCleared)%sceneShelves.Length)+offset;
                bool clear=Walkable(p);
                foreach(var spill in spills)if(Vector2.Distance(p,spill)<65)clear=false;
                if(clear)spills.Add(p);
            }
        }
        void NotifyShiftGoals()
        {
            int mask=(served>=ServeGoal?1:0)|(stockActions>=StockGoal?2:0)|(hazardsCleared>=CleanGoal?4:0);
            int fresh=mask&~notifiedShiftGoals;notifiedShiftGoals|=mask;
            if(fresh==0||testing||verificationRunning)return;
            PlaySound("Success",.55f);
            Toast((fresh&4)!=0?"Cleanup target complete!":(fresh&2)!=0?"Restock target complete!":"Customer target complete!");
        }
        void SyncPlayerFootsteps()
        {
            int contact=shop.playerCharacter.FootstepContacts;
            if(contact>lastFootstepContact&&selected&&!mainMenu&&!settingsOpen&&!autoPaused&&!shiftBriefOpen&&skillType<0&&!levelResults)
                PlaySound("Footstep",IsSprinting?.24f:.18f);
            lastFootstepContact=contact;
        }
        void GoalProgress(Rect r,int done,int target)
        {
            Fill(r,new Color(.13f,.21f,.22f));
            Fill(new Rect(r.x,r.y,r.width*Mathf.Clamp01((float)done/Mathf.Max(1,target)),r.height),done>=target?arcadeTeal:arcadeGold);
        }
    }
}
