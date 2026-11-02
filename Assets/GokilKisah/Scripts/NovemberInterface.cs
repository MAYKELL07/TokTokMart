using UnityEngine;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        void ArtImage(Rect area,string name)
        {var art=NovemberArt.UI(name);if(art)GUI.DrawTexture(area,art,ScaleMode.ScaleToFit,true);}
        void DrawNovemberMenu()
        {
            var bg=NovemberArt.UI("MenuBackground");if(bg)GUI.DrawTexture(new Rect(0,0,1600,900),bg,ScaleMode.ScaleAndCrop);
            Fill(new Rect(0,0,680,900),new Color(.06f,.16f,.18f,.89f));
            ArtImage(new Rect(90,38,485,248),"Logo");
            ArcadeText(new Rect(85,295,530,42),"A LITTLE SHOP. A LOT OF CHAOS.",22,arcadeCream,TextAnchor.MiddleCenter);
            GUI.enabled=!settingsOpen;
            if(UiButton(new Rect(105,365,445,78),SavedLevel>1?"BACK TO SHIFT "+SavedLevel:"OPEN THE SHOP",arcadeGold,30)){mainMenu=false;selected=false;}
            if(UiButton(new Rect(105,468,445,66),"SETTINGS",arcadeCream,26))SetSettingsOpen(true);
            if(SavedLevel>1&&UiButton(new Rect(105,558,445,60),"FRESH START",arcadeCream,23))
            {PlayerPrefs.SetInt(LevelSave,1);PlayerPrefs.SetInt(BossSave,0);PlayerPrefs.Save();mainMenu=false;selected=false;}
            if(UiButton(new Rect(105,654,445,60),"CLOCK OUT",arcadeCoral,23)){if(!Application.isEditor)Application.Quit();else Toast("Quit is available in the built demo.");}
            GUI.enabled=true;
            Portrait(new Rect(725,560,190,284),false);Portrait(new Rect(1260,560,190,284),true);
            ArcadeText(new Rect(92,790,500,45),"STOCK. SERVE. KEEP IT CLEAN.",22,arcadeCream,TextAnchor.MiddleCenter);
            if(settingsOpen)DrawSettings();
        }
        void DrawNovemberHud()
        {
            ArtImage(new Rect(26,24,290,96),"CustomerCount01");
            ArcadeText(new Rect(110,44,190,46),served+" / "+(campaignActive?ServeGoal:3),29,arcadeNavy,TextAnchor.MiddleCenter);
            ArtImage(new Rect(332,24,312,96),"MoneyCount01");
            ArcadeText(new Rect(344,46,220,43),Ringgit(campaignActive?InvoiceTotal:0),27,arcadeNavy,TextAnchor.MiddleCenter);
            ArtImage(new Rect(1265,19,218,118),"DayTimer01");
            ArcadeText(new Rect(1420,35,40,35),CurrentLevel.ToString(),22,arcadeNavy,TextAnchor.MiddleCenter);
            float left=campaignActive?Mathf.Max(0,ShiftBudget-shiftTime):shiftTime;
            ArcadeText(new Rect(1332,65,133,45),((int)left/60)+":"+((int)left%60).ToString("00"),30,arcadeCream,TextAnchor.MiddleCenter);
            GoalProgress(new Rect(111,96,180,5),served,campaignActive?ServeGoal:3);
            ArtImage(new Rect(1500,26,68,72),"PauseButton01");
            if(InterfaceButton(new Rect(1495,20,78,86)))SetSettingsOpen(true);
            ArtImage(new Rect(24,132,500,117),"IndicatorContainer");
            ArtImage(new Rect(38,150,35,35),"CheckoutIcon01");
            ArcadeText(new Rect(83,149,190,38),"RESTOCK "+stockActions+" / "+(campaignActive?StockGoal:1),17,arcadeCream);
            ArtImage(new Rect(278,149,25,35),"PuddleIcon01");
            ArcadeText(new Rect(314,149,144,38),"CLEAN "+hazardsCleared+" / "+(campaignActive?CleanGoal:1),17,arcadeCream);
            ArtImage(new Rect(475,153,36,36),"HazardIcon01");
            ArcadeText(new Rect(465,197,55,28),Mathf.RoundToInt(chaos)+"%",14,arcadeCream,TextAnchor.MiddleCenter);
            GoalProgress(new Rect(83,202,145,5),stockActions,campaignActive?StockGoal:1);
            GoalProgress(new Rect(314,202,125,5),hazardsCleared,campaignActive?CleanGoal:1);
            if(encounter!=Encounter.Store)
            {
                Sticker(new Rect(675,140,890,62),resolved?arcadeTeal:arcadeCoral);
                ArcadeText(new Rect(691,149,858,43),resolved?"ALL CLEAR!":ObjectiveText(),22,arcadeCream,TextAnchor.MiddleCenter);
            }
            if(!showTouchControls)
            {
                Sticker(new Rect(480,798,104,80),arcadeCoral);Portrait(new Rect(493,784,79,86),zack);
                Sticker(new Rect(600,798,470,62),arcadeCream);Keycap(new Rect(609,806,46,44),"E");
                ArcadeText(new Rect(670,808,382,40),InteractionLabel(),22,arcadeNavy);
                if(InterfaceButton(new Rect(600,798,470,62)))ActionInput();
            }
            else
            {
                string hint=InteractionLabel();if(hint=="Move near a station")hint="Stock. Serve. Keep it clean.";
                Panel(new Rect(548,817,504,50),new Color(.06f,.16f,.18f,.85f));
                ArcadeText(new Rect(561,821,478,40),hint,20,arcadeCream,TextAnchor.MiddleCenter);
            }
            Fill(new Rect(640,882,320,7),new Color(.12f,.23f,.23f));
            Fill(new Rect(640,882,320*Mathf.Clamp01(Stamina/maxStamina),7),arcadeGold);
        }
    }
}
