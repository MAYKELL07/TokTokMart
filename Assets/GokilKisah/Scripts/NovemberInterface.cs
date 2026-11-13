using UnityEngine;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        void ArtImage(Rect area,string name,string uiKey="")
        {using(NativeUiScope(uiKey,"ArtImage: "+name)){var art=NovemberArt.UI(name);if(art)EditableTexture(area,art,ScaleMode.ScaleToFit,true);}}
        void DrawNovemberMenu()
        {using(NativeUiScope("November11Menu","Main menu")){
            if(levelMapOpen){DrawLevelMap();return;}
            var bg=NovemberArt.UI("MenuBackground");if(bg)EditableTexture(new Rect(0,0,1600,900),bg,ScaleMode.ScaleAndCrop);
            Fill(new Rect(0,0,680,900),new Color(.06f,.16f,.18f,.89f), uiKey:"NovemberInterface_424");
            ArtImage(new Rect(90,38,485,248),"Logo", uiKey:"NovemberInterface_496");
            ArcadeText(new Rect(85,295,530,42),"A LITTLE SHOP. A LOT OF CHAOS.",22,arcadeCream,TextAnchor.MiddleCenter, uiKey:"NovemberInterface_550");
            GUI.enabled=!settingsOpen;
            if(UiButton(new Rect(105,365,445,78),SavedLevel>1?"BACK TO SHIFT "+SavedLevel:"OPEN THE SHOP",arcadeGold,30, uiKey:"NovemberInterface_713")){mainMenu=false;selected=false;}
            if(UiButton(new Rect(105,468,445,66),"SETTINGS",arcadeCream,26, uiKey:"NovemberInterface_867"))SetSettingsOpen(true);
            if(UiButton(new Rect(105,558,445,60),"CHOOSE A LEVEL",arcadeGold,23, uiKey:"November11Levels"))OpenLevelMap();
            if(UiButton(new Rect(105,654,445,60),"CLOCK OUT",arcadeCoral,23, uiKey:"NovemberInterface_1187")){if(!Application.isEditor)Application.Quit();else Toast("Quit is available in the built demo.");}
            GUI.enabled=true;
            Portrait(new Rect(725,560,190,284),false, uiKey:"NovemberInterface_1389");Portrait(new Rect(1260,560,190,284),true, uiKey:"NovemberInterface_1431");
            ArcadeText(new Rect(92,790,500,45),"STOCK. SERVE. KEEP IT CLEAN.",22,arcadeCream,TextAnchor.MiddleCenter, uiKey:"NovemberInterface_1486");
            if(settingsOpen)DrawSettings();
        }}
        void DrawLegacyNovemberHud()
        {using(NativeUiScope("DrawNovemberHud","DrawNovemberHud")){
            var hudCanvas=GUI.matrix;
            GUI.matrix=HudMatrix(hudCanvas,new Vector2(26,24));
            ArtImage(new Rect(26,24,290,96),"CustomerCount01", uiKey:"NovemberInterface_1802");
            ArcadeText(new Rect(110,44,190,46),served+" / "+(campaignActive?ServeGoal:3),29,arcadeNavy,TextAnchor.MiddleCenter, uiKey:"NovemberInterface_1866");
            ArtImage(new Rect(332,24,312,96),"MoneyCount01", uiKey:"NovemberInterface_1995");
            ArcadeText(new Rect(344,46,220,43),Ringgit(campaignActive?InvoiceTotal:0),27,arcadeNavy,TextAnchor.MiddleCenter, uiKey:"NovemberInterface_2057");
            GoalProgress(new Rect(111,96,180,5),served,campaignActive?ServeGoal:3, uiKey:"NovemberInterface_2183");
            GUI.matrix=HudMatrix(hudCanvas,new Vector2(1573,19));
            ArtImage(new Rect(1265,19,218,118),"DayTimer01", uiKey:"NovemberInterface_2333");
            ArcadeText(new Rect(1420,35,40,35),CurrentLevel.ToString(),22,arcadeNavy,TextAnchor.MiddleCenter, uiKey:"NovemberInterface_2395");
            float left=campaignActive?Mathf.Max(0,ShiftBudget-shiftTime):shiftTime;
            ArcadeText(new Rect(1332,65,133,45),((int)left/60)+":"+((int)left%60).ToString("00"),30,arcadeCream,TextAnchor.MiddleCenter, uiKey:"NovemberInterface_2590");
            ArtImage(new Rect(1500,26,68,72),"PauseButton01", uiKey:"NovemberInterface_2728");
            if(InterfaceButton(new Rect(1495,20,78,86)))SetSettingsOpen(true);
            GUI.matrix=HudMatrix(hudCanvas,new Vector2(26,24));
            ArtImage(new Rect(24,132,500,117),"IndicatorContainer", uiKey:"NovemberInterface_2934");
            ArtImage(new Rect(38,150,35,35),"CheckoutIcon01", uiKey:"NovemberInterface_3003");
            ArcadeText(new Rect(83,149,190,38),"RESTOCK "+stockActions+" / "+(campaignActive?StockGoal:1),17,arcadeCream, uiKey:"NovemberInterface_3066");
            ArtImage(new Rect(278,149,25,35),"PuddleIcon01", uiKey:"NovemberInterface_3189");
            ArcadeText(new Rect(314,149,144,38),"CLEAN "+hazardsCleared+" / "+(campaignActive?CleanGoal:1),17,arcadeCream, uiKey:"NovemberInterface_3251");
            ArtImage(new Rect(475,153,36,36),"HazardIcon01", uiKey:"NovemberInterface_3375");
            ArcadeText(new Rect(465,197,55,28),Mathf.RoundToInt(chaos)+"%",14,arcadeCream,TextAnchor.MiddleCenter, uiKey:"NovemberInterface_3437");
            GoalProgress(new Rect(83,202,145,5),stockActions,campaignActive?StockGoal:1, uiKey:"NovemberInterface_3553");
            GoalProgress(new Rect(314,202,125,5),hazardsCleared,campaignActive?CleanGoal:1, uiKey:"NovemberInterface_3643");
            GUI.matrix=hudCanvas;
            if(encounter!=Encounter.Store)
            {
                Sticker(new Rect(675,140,890,62),resolved?arcadeTeal:arcadeCoral, uiKey:"NovemberInterface_3831");
                ArcadeText(new Rect(691,149,858,43),resolved?"ALL CLEAR!":ObjectiveText(),22,arcadeCream,TextAnchor.MiddleCenter, uiKey:"NovemberInterface_3914");
            }
            if(!showTouchControls)
            {
                Sticker(new Rect(480,798,104,80),arcadeCoral, uiKey:"NovemberInterface_4108");Portrait(new Rect(493,784,79,86),zack, uiKey:"NovemberInterface_4154");
                Sticker(new Rect(600,798,470,62),arcadeCream, uiKey:"NovemberInterface_4210");Keycap(new Rect(609,806,46,44),"E", uiKey:"NovemberInterface_4256");
                ArcadeText(new Rect(670,808,382,40),InteractionLabel(),22,arcadeNavy, uiKey:"NovemberInterface_4309");
                if(InterfaceButton(new Rect(600,798,470,62)))ActionInput();
            }
            else
            {
                string hint=InteractionLabel();if(hint=="Move near a station")hint="Stock. Serve. Keep it clean.";
                Panel(new Rect(548,817,504,50),new Color(.06f,.16f,.18f,.85f), uiKey:"NovemberInterface_4632");
                ArcadeText(new Rect(561,821,478,40),hint,20,arcadeCream,TextAnchor.MiddleCenter, uiKey:"NovemberInterface_4712");
            }
            Fill(new Rect(640,882,320,7),new Color(.12f,.23f,.23f), uiKey:"NovemberInterface_4820");
            Fill(new Rect(640,882,320*Mathf.Clamp01(Stamina/maxStamina),7),arcadeGold, uiKey:"NovemberInterface_4889");
        }}
    }
}
