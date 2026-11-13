using System.Collections.Generic;
using UnityEngine;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        Font arcadeFont;
        readonly Dictionary<int,GUIStyle> arcadeStyles=new Dictionary<int,GUIStyle>();
        static readonly Color arcadeNavy=new Color(.10f,.19f,.23f),arcadeCream=new Color(1,.97f,.87f),arcadeCoral=new Color(.94f,.39f,.29f),arcadeGold=new Color(1,.79f,.27f),arcadeTeal=new Color(.13f,.57f,.51f);
        readonly Dictionary<string,Texture2D> arcadeIcons=new Dictionary<string,Texture2D>();
        void ArcadeText(Rect r,string value,int size,Color color,TextAnchor align=TextAnchor.MiddleLeft,bool outline=false,string uiKey="")
        {using(NativeUiScope(uiKey,"ArcadeText: "+value)){
            if(!arcadeFont)arcadeFont=Resources.Load<Font>("Fonts/Bigbesty") ?? Resources.Load<Font>("Fonts/LilitaOne-Regular");
            int key=size*10+(int)align;
            if(!arcadeStyles.TryGetValue(key,out var style))
            {style=new GUIStyle{font=arcadeFont?arcadeFont:boldFont,fontSize=size,alignment=align,wordWrap=false};arcadeStyles[key]=style;}
            if(outline)
            {
                style.normal.textColor=arcadeNavy;
                foreach(var d in new[]{new Vector2(-3,0),new Vector2(3,0),new Vector2(0,-3),new Vector2(0,4)})EditableLabel(new Rect(r.position+d,r.size),value,style);
            }
            style.normal.textColor=color;EditableLabel(r,value,style);
        }}
        void Sticker(Rect r,Color color,bool shadow=true,float border=4,string uiKey="")
        {using(NativeUiScope(uiKey,"Sticker")){
            if(shadow)Panel(new Rect(r.x+1,r.y+7,r.width,r.height),new Color(.04f,.10f,.13f,.28f), uiKey:"ArcadeInterface_1464");
            Panel(r,arcadeNavy, uiKey:"ArcadeInterface_1554");Panel(new Rect(r.x+border,r.y+border,r.width-border*2,r.height-border*2),color, uiKey:"ArcadeInterface_1574");
        }}
        void Keycap(Rect r,string key,string uiKey="")
        {using(NativeUiScope(uiKey,"Keycap")){Sticker(r,arcadeCream,false,3, uiKey:"ArcadeInterface_1713");ArcadeText(r,key,22,arcadeNavy,TextAnchor.MiddleCenter, uiKey:"ArcadeInterface_1744");}}
        static float IconLine(Vector2 p,Vector2 a,Vector2 b,float width)
        {var d=b-a;float t=Mathf.Clamp01(Vector2.Dot(p-a,d)/Mathf.Max(.001f,d.sqrMagnitude));return Mathf.Clamp01(width+.6f-(p-a-t*d).magnitude);}
        static float IconBox(Vector2 p,float x,float y,float w,float h)
        {return p.x>=x&&p.x<=x+w&&p.y>=y&&p.y<=y+h?1:0;}
        Texture2D ArcadeIcon(string kind)
        {
            if(arcadeIcons.TryGetValue(kind,out var texture))return texture;
            texture=new Texture2D(96,96,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear};
            for(int y=0;y<96;y++)for(int x=0;x<96;x++)
            {
                var p=new Vector2(x,y);float a=0;
                if(kind=="clock")
                {
                    a=Mathf.Clamp01(4.5f-Mathf.Abs((p-new Vector2(48,46)).magnitude-29));
                    a=Mathf.Max(a,IconLine(p,new Vector2(48,46),new Vector2(48,65),3));a=Mathf.Max(a,IconLine(p,new Vector2(48,46),new Vector2(64,37),3));
                    a=Mathf.Max(a,IconBox(p,38,82,20,6));
                }
                else if(kind=="coin")
                {
                    a=Mathf.Clamp01(4.5f-Mathf.Abs((p-new Vector2(48,48)).magnitude-32));
                    a=Mathf.Max(a,IconBox(p,44,25,8,46));a=Mathf.Max(a,IconLine(p,new Vector2(61,63),new Vector2(35,57),4));a=Mathf.Max(a,IconLine(p,new Vector2(35,57),new Vector2(61,39),4));a=Mathf.Max(a,IconLine(p,new Vector2(61,39),new Vector2(35,32),4));
                }
                else if(kind=="stock")
                {
                    a=IconLine(p,new Vector2(18,23),new Vector2(78,23),4);a=Mathf.Max(a,IconLine(p,new Vector2(18,23),new Vector2(18,72),4));a=Mathf.Max(a,IconLine(p,new Vector2(78,23),new Vector2(78,72),4));a=Mathf.Max(a,IconLine(p,new Vector2(18,72),new Vector2(78,72),4));a=Mathf.Max(a,IconBox(p,42,51,12,21));a=Mathf.Max(a,IconBox(p,29,34,19,5));
                }
                else if(kind=="clean")
                {
                    a=IconLine(p,new Vector2(37,31),new Vector2(66,79),4);a=Mathf.Max(a,IconLine(p,new Vector2(18,20),new Vector2(51,39),7));a=Mathf.Max(a,IconLine(p,new Vector2(20,15),new Vector2(55,34),4));
                    a=Mathf.Max(a,IconLine(p,new Vector2(72,20),new Vector2(72,42),2));a=Mathf.Max(a,IconLine(p,new Vector2(62,31),new Vector2(82,31),2));
                }
                else if(kind=="serve")
                {
                    a=IconLine(p,new Vector2(18,65),new Vector2(78,65),4);a=Mathf.Max(a,IconLine(p,new Vector2(25,65),new Vector2(30,35),4));a=Mathf.Max(a,IconLine(p,new Vector2(30,35),new Vector2(69,35),4));a=Mathf.Max(a,IconLine(p,new Vector2(69,35),new Vector2(77,65),4));
                    a=Mathf.Max(a,Mathf.Clamp01(7-(p-new Vector2(34,20)).magnitude));a=Mathf.Max(a,Mathf.Clamp01(7-(p-new Vector2(67,20)).magnitude));a=Mathf.Max(a,IconLine(p,new Vector2(18,65),new Vector2(10,77),4));
                }
                else if(kind=="pause")a=Mathf.Max(IconBox(p,27,23,13,50),IconBox(p,56,23,13,50));
                else if(kind=="check")a=Mathf.Max(IconLine(p,new Vector2(18,46),new Vector2(38,26),6),IconLine(p,new Vector2(38,26),new Vector2(78,72),6));
                texture.SetPixel(x,y,new Color(1,1,1,a));
            }
            texture.Apply();arcadeIcons[kind]=texture;return texture;
        }
        void Icon(Rect r,string kind,Color color,string uiKey=""){using(NativeUiScope(uiKey,"Icon: "+kind)){GUI.color=color;EditableTexture(r,ArcadeIcon(kind));GUI.color=Color.white;}}
        void Portrait(Rect area,bool isZack,string uiKey="")
        {using(NativeUiScope(uiKey,"Portrait")){
            var sprite=SmoothArt.Load(isZack?"Zack_3_0":"Sam_3_0");if(!sprite)return;
            var rect=sprite.rect;float scale=Mathf.Min(area.width/rect.width,area.height/rect.height);
            var fit=new Rect(area.center.x-rect.width*scale/2,area.yMax-rect.height*scale,rect.width*scale,rect.height*scale);
            EditableTextureUV(fit,sprite.texture,new Rect(rect.x/sprite.texture.width,rect.y/sprite.texture.height,rect.width/sprite.texture.width,rect.height/sprite.texture.height));
        }}
        void GoalTicket(Rect r,int number,string label,string icon,int done,int target,Color color,string uiKey="")
        {using(NativeUiScope(uiKey,"GoalTicket")){
            bool complete=done>=target;Sticker(r,arcadeCream,true,3, uiKey:"ArcadeInterface_5987");
            Panel(new Rect(r.x+3,r.y+3,r.width-6,29),complete?arcadeTeal:color, uiKey:"ArcadeInterface_6030");
            ArcadeText(new Rect(r.x+13,r.y+3,r.width-26,27),number.ToString("00")+"  "+label,20,Color.white, uiKey:"ArcadeInterface_6111");
            Icon(new Rect(r.x+15,r.y+42,60,60),complete?"check":icon,complete?arcadeTeal:arcadeNavy, uiKey:"ArcadeInterface_6221");
            ArcadeText(new Rect(r.x+85,r.y+43,r.width-94,48),Mathf.Min(done,target)+" / "+target,34,arcadeNavy, uiKey:"ArcadeInterface_6323");
            for(int x=12;x<r.width-12;x+=13)Fill(new Rect(r.x+x,r.y+108,6,2),new Color(.6f,.65f,.6f), uiKey:"ArcadeInterface_6468");
            ArcadeText(new Rect(r.x+13,r.y+113,r.width-26,21),complete?"ALL DONE!":icon=="serve"?"AT THE CHECKOUT":icon=="stock"?"REFILL THE SHELVES":"MOP THOSE SPILLS",13,complete?arcadeTeal:arcadeNavy, uiKey:"ArcadeInterface_6539");
            Fill(new Rect(r.x+8,r.yMax-8,r.width-16,4),new Color(.79f,.82f,.75f), uiKey:"ArcadeInterface_6744");
            Fill(new Rect(r.x+8,r.yMax-8,(r.width-16)*Mathf.Clamp01((float)done/Mathf.Max(1,target)),4),complete?arcadeTeal:color, uiKey:"ArcadeInterface_6827");
        }}
        void DrawArcadeHud()
        {
            GoalTicket(new Rect(26,18,212,148),1,"SERVE","serve",served,campaignActive?ServeGoal:3,arcadeTeal, uiKey:"ArcadeInterface_7008");
            GoalTicket(new Rect(252,18,212,148),2,"RESTOCK","stock",stockActions,campaignActive?StockGoal:1,arcadeGold*.92f, uiKey:"ArcadeInterface_7120");
            GoalTicket(new Rect(478,18,212,148),3,"CLEAN UP","clean",hazardsCleared,campaignActive?CleanGoal:1,arcadeCoral, uiKey:"ArcadeInterface_7246");
            Sticker(new Rect(1290,24,194,56),arcadeCream, uiKey:"ArcadeInterface_7371");
            ArcadeText(new Rect(1302,29,170,44),"SHIFT "+CurrentLevel.ToString("00"),27,arcadeNavy,TextAnchor.MiddleCenter, uiKey:"ArcadeInterface_7430");
            if(UiButton(new Rect(1500,24,66,56),"",arcadeCream, uiKey:"ArcadeInterface_7558"))SetSettingsOpen(true);
            Icon(new Rect(1515,35,36,36),"pause",arcadeNavy, uiKey:"ArcadeInterface_7642");
            Sticker(new Rect(26,782,310,94),arcadeGold, uiKey:"ArcadeInterface_7704");
            Icon(new Rect(40,798,65,65),"coin",arcadeNavy, uiKey:"ArcadeInterface_7761");
            ArcadeText(new Rect(119,791,196,22),campaignActive?"SHIFT TAKINGS":"CUSTOMERS SERVED",17,arcadeNavy, uiKey:"ArcadeInterface_7821");
            ArcadeText(new Rect(116,815,205,47),campaignActive?Ringgit(InvoiceTotal):served.ToString("00"),35,arcadeNavy, uiKey:"ArcadeInterface_7935");
            var danger=chaos>60?arcadeCoral:arcadeTeal;
            ArcadeText(new Rect(32,754,292,23),"SHOP CHAOS  "+Mathf.RoundToInt(chaos)+"%",16,arcadeCream,TextAnchor.MiddleLeft,true, uiKey:"ArcadeInterface_8114");
            float left=campaignActive?Mathf.Max(0,ShiftBudget-shiftTime):shiftTime;
            var clockColor=campaignActive&&left<30?Color.Lerp(arcadeCoral,arcadeGold,(Mathf.Sin(pulse*7)+1)*.5f):arcadeCream;
            Sticker(new Rect(1302,782,264,94),clockColor, uiKey:"ArcadeInterface_8458");
            Icon(new Rect(1316,800,58,58),"clock",arcadeNavy, uiKey:"ArcadeInterface_8517");
            ArcadeText(new Rect(1386,794,164,21),campaignActive?"CLOSING IN":"SHIFT TIME",16,arcadeNavy, uiKey:"ArcadeInterface_8580");
            ArcadeText(new Rect(1381,814,170,45),((int)left/60)+":"+((int)left%60).ToString("00"),42,arcadeNavy,TextAnchor.MiddleCenter, uiKey:"ArcadeInterface_8686");
            string action=skillType==1?"Enter the total":skillType>=0?"Time the task!":InteractionLabel();
            bool canAct=action!="Move near a station";
            Sticker(new Rect(480,798,104,80),zack?new Color(.54f,.7f,.95f):arcadeCoral, uiKey:"ArcadeInterface_8986");
            Portrait(new Rect(493,784,79,86),zack, uiKey:"ArcadeInterface_9075");
            Sticker(new Rect(600,798,470,62),canAct?arcadeCream:new Color(.79f,.87f,.81f), uiKey:"ArcadeInterface_9127");
            Keycap(new Rect(609,806,46,44),"E", uiKey:"ArcadeInterface_9219");
            ArcadeText(new Rect(670,808,382,40),canAct?action:"Stock. Serve. Keep it clean.",22,arcadeNavy, uiKey:"ArcadeInterface_9268");
            if(canAct&&InterfaceButton(new Rect(600,798,470,62)))Interact();
            for(int i=0;i<8;i++)Fill(new Rect(609+i*19,870,15,6),Stamina/maxStamina>(float)i/8?arcadeGold:new Color(.3f,.4f,.4f), uiKey:"ArcadeInterface_9474");
            ArcadeText(new Rect(805,864,265,22),MobileInterface?"HOLD SPRINT":"SHIFT  SPRINT",15,arcadeCream,TextAnchor.MiddleRight,true, uiKey:"ArcadeInterface_9585");
            if(encounter!=Encounter.Store)
            {
                Sticker(new Rect(945,103,620,64),resolved?arcadeTeal:arcadeCoral, uiKey:"ArcadeInterface_9785");
                string message=resolved?"ALL CLEAR!":encounter==Encounter.KKung?"LIGHTS OUT! Find the breaker":encounter==Encounter.Goblins?"GOBLINS! Call Uncle Ray":encounter==Encounter.Salmah?"MISS SALMAH! Get behind her":"BO'S HERE! "+boRemaining.ToString("0.0")+"s";
                ArcadeText(new Rect(960,114,590,41),message,25,arcadeCream,TextAnchor.MiddleCenter, uiKey:"ArcadeInterface_10139");
            }
        }
        void DrawArcadeMenu()
        {
            Fill(new Rect(0,0,1600,900),new Color(.10f,.32f,.35f), uiKey:"ArcadeInterface_10300");
            for(int i=0;i<18;i++)Panel(new Rect(570+i%6*191,24+i/6*295,110,110),new Color(.13f,.36f,.38f), uiKey:"ArcadeInterface_10389");
            var matrix=GUI.matrix;GUIUtility.RotateAroundPivot(-3,new Vector2(330,170));
            ArcadeText(new Rect(75,60,550,110),"TOKTOK",104,arcadeCream,TextAnchor.MiddleCenter,true, uiKey:"ArcadeInterface_10565");
            ArcadeText(new Rect(75,156,550,110),"MART!",104,arcadeGold,TextAnchor.MiddleCenter,true, uiKey:"ArcadeInterface_10668");GUI.matrix=matrix;
            ArcadeText(new Rect(100,283,500,30),"A LITTLE SHOP. A LOT OF CHAOS.",21,arcadeCream, uiKey:"ArcadeInterface_10788");
            GUI.enabled=!settingsOpen;
            if(UiButton(new Rect(102,354,440,79),SavedLevel>1?"BACK TO SHIFT "+SavedLevel:"OPEN THE SHOP",arcadeGold,31, uiKey:"ArcadeInterface_10928")){mainMenu=false;selected=false;}
            if(SavedLevel>1&&UiButton(new Rect(102,455,440,65),"FRESH START",arcadeCream,25, uiKey:"ArcadeInterface_11096")){PlayerPrefs.SetInt(LevelSave,1);PlayerPrefs.SetInt(BossSave,0);PlayerPrefs.Save();mainMenu=false;selected=false;}
            if(UiButton(new Rect(102,548,440,65),"SETTINGS",arcadeCream,25, uiKey:"ArcadeInterface_11290"))SetSettingsOpen(true);
            if(UiButton(new Rect(102,635,440,65),"CLOCK OUT",arcadeCoral,25, uiKey:"ArcadeInterface_11389")){if(!Application.isEditor)Application.Quit();else Toast("Quit is available in the standalone game.");}
            GUI.enabled=true;
            // An original storefront illustration built from the game's own vector shapes.
            Sticker(new Rect(690,250,760,410),new Color(.92f,.86f,.69f),true,8, uiKey:"ArcadeInterface_11688");
            Sticker(new Rect(735,405,285,215),new Color(.22f,.52f,.55f),false,6, uiKey:"ArcadeInterface_11769");
            for(int x=744;x<1010;x+=88)Fill(new Rect(x,414,3,198),arcadeCream, uiKey:"ArcadeInterface_11878");
            Fill(new Rect(744,516,268,5),arcadeCream, uiKey:"ArcadeInterface_11931");
            Sticker(new Rect(1087,405,242,246),new Color(.15f,.38f,.40f),false,7, uiKey:"ArcadeInterface_11986");
            Fill(new Rect(1100,418,214,8),new Color(.35f,.65f,.63f), uiKey:"ArcadeInterface_12069");Panel(new Rect(1289,526,12,12),arcadeGold, uiKey:"ArcadeInterface_12126");
            for(int i=0;i<12;i++)
            {var stripe=new Rect(695+i*63,280,63,105);Fill(stripe,i%2==0?arcadeCoral:arcadeCream, uiKey:"ArcadeInterface_12258");Panel(new Rect(stripe.x,stripe.yMax-17,63,31),i%2==0?arcadeCoral:arcadeCream, uiKey:"ArcadeInterface_12302");}
            Sticker(new Rect(854,183,445,110),arcadeGold,true,6, uiKey:"ArcadeInterface_12394");
            ArcadeText(new Rect(871,199,411,74),"TOKTOK MART",47,arcadeNavy,TextAnchor.MiddleCenter, uiKey:"ArcadeInterface_12460");
            Sticker(new Rect(679,650,788,34),arcadeNavy,false,0, uiKey:"ArcadeInterface_12562");
            Portrait(new Rect(690,541,162,257),false, uiKey:"ArcadeInterface_12628");Portrait(new Rect(1275,548,168,250),true, uiKey:"ArcadeInterface_12670");
            Sticker(new Rect(1012,721,200,68),arcadeCoral, uiKey:"ArcadeInterface_12725");ArcadeText(new Rect(1022,727,180,53),"OPEN!",34,arcadeCream,TextAnchor.MiddleCenter, uiKey:"ArcadeInterface_12772");
            ArcadeText(new Rect(105,802,580,40),"STOCK THE SHELVES. BEAT THE RUSH.",21,arcadeCream, uiKey:"ArcadeInterface_12870");
            if(settingsOpen)DrawSettings();
        }
        void DrawArcadeSelection()
        {using(NativeUiScope("DrawArcadeSelection","DrawArcadeSelection")){
            Shade(uiKey:"ArcadeInterface_13070");ArcadeText(new Rect(300,72,1000,85),"WHO'S ON SHIFT?",63,arcadeCream,TextAnchor.MiddleCenter,true, uiKey:"ArcadeInterface_13078");
            ArcadeText(new Rect(300,157,1000,35),"Pick your keeper. The other has your back.",24,arcadeCream,TextAnchor.MiddleCenter, uiKey:"ArcadeInterface_13190");
            foreach(bool pickZack in new[]{false,true})
            {
                var r=new Rect(pickZack?840:360,244,400,460);var color=pickZack?new Color(.38f,.61f,.87f):arcadeCoral;
                Sticker(r,arcadeCream,true,6, uiKey:"ArcadeInterface_13518");Panel(new Rect(r.x+6,r.y+6,r.width-12,222),color, uiKey:"ArcadeInterface_13548");
                Portrait(new Rect(r.x+103,r.y-24,194,252),pickZack, uiKey:"ArcadeInterface_13615");
                ArcadeText(new Rect(r.x+25,r.y+243,350,53),pickZack?"ZACK":"SAM",43,arcadeNavy,TextAnchor.MiddleCenter, uiKey:"ArcadeInterface_13684");
                ArcadeText(new Rect(r.x+25,r.y+303,350,28),pickZack?"READY FOR THE RUSH":"YOUR NEIGHBOURHOOD KEEPER",17,arcadeNavy,TextAnchor.MiddleCenter, uiKey:"ArcadeInterface_13805");
                if(UiButton(new Rect(r.x+28,r.y+363,344,64),pickZack?"PICK ZACK":"PICK SAM",color,28, uiKey:"ArcadeInterface_13965"))Choose(pickZack);
            }
            if(MobileInterface)
                ArcadeText(new Rect(300,768,1000,64),"DRAG JOYSTICK TO MOVE   /   HOLD SPRINT   /   TAP INTERACT",24,arcadeCream,TextAnchor.MiddleCenter, uiKey:"ArcadeInterface_14128");
            else
            {
            Keycap(new Rect(449,778,94,44),"WASD", uiKey:"ArcadeInterface_14310");ArcadeText(new Rect(554,779,120,42),"MOVE",19,arcadeCream, uiKey:"ArcadeInterface_14349");
            Keycap(new Rect(709,778,85,44),"SHIFT", uiKey:"ArcadeInterface_14421");ArcadeText(new Rect(807,779,125,42),"SPRINT",19,arcadeCream, uiKey:"ArcadeInterface_14461");
            Keycap(new Rect(967,778,48,44),"E", uiKey:"ArcadeInterface_14535");ArcadeText(new Rect(1028,779,150,42),"INTERACT",19,arcadeCream, uiKey:"ArcadeInterface_14571");
            }
        }}
        void DrawLegacyArcadeResults()
        {using(NativeUiScope("DrawArcadeResults","DrawArcadeResults")){
            Fill(new Rect(0,0,1600,900),new Color(.10f,.29f,.32f), uiKey:"ArcadeInterface_14715");
            ArcadeText(new Rect(200,42,1200,92),levelWon?"THAT'S A WRAP!":"WHAT A SHIFT!",68,levelWon?arcadeGold:arcadeCoral,TextAnchor.MiddleCenter,true, uiKey:"ArcadeInterface_14783");
            Sticker(new Rect(400,168,800,546),arcadeCream,true,6, uiKey:"ArcadeInterface_14939");
            Panel(new Rect(406,174,788,65),arcadeTeal, uiKey:"ArcadeInterface_15006");
            ArcadeText(new Rect(430,178,740,54),"TOKTOK MART  /  SHIFT "+CurrentLevel.ToString("00"),30,arcadeCream,TextAnchor.MiddleCenter, uiKey:"ArcadeInterface_15062");
            for(int i=0;i<4;i++){GUI.color=i<invoiceStars?arcadeGold:new Color(.74f,.78f,.73f);EditableTexture(new Rect(612+i*98,260,82,82),InvoiceStar());}GUI.color=Color.white;
            string[] labels={"Customers served","Shelves restocked","Spills cleaned","Mistakes & missed customers"};
            int[] amounts={salesCents,stockActions*75,hazardsCleared*50,-InvoicePenalty};
            for(int i=0;i<4;i++)
            {
                Text(new Rect(448,365+i*48,490,36),labels[i],23,arcadeNavy, uiKey:"ArcadeInterface_15641");
                ArcadeText(new Rect(954,365+i*48,192,36),(amounts[i]<0?"- ":"")+Ringgit(Mathf.Abs(amounts[i])),25,arcadeNavy,TextAnchor.MiddleRight, uiKey:"ArcadeInterface_15718");
            }
            for(int x=448;x<1150;x+=22)Fill(new Rect(x,573,11,3),new Color(.60f,.65f,.59f), uiKey:"ArcadeInterface_15905");
            ArcadeText(new Rect(448,591,450,62),"SHIFT TOTAL",30,arcadeNavy, uiKey:"ArcadeInterface_15971");
            ArcadeText(new Rect(900,591,246,62),(InvoiceTotal<0?"- ":"")+Ringgit(Mathf.Abs(InvoiceTotal)),38,arcadeNavy,TextAnchor.MiddleRight, uiKey:"ArcadeInterface_16049");
            if(!levelWon)ArcadeText(new Rect(260,720,1080,40),levelFailure,21,arcadeCream,TextAnchor.MiddleCenter, uiKey:"ArcadeInterface_16207");
            if(UiButton(new Rect(400,780,510,72),levelWon?"NEXT SHIFT  "+(CurrentLevel+1):"GIVE IT ANOTHER GO",arcadeGold,28, uiKey:"ArcadeInterface_16313"))BeginCampaignLevel(levelWon?CurrentLevel+1:CurrentLevel,campaignPersistence);
            if(UiButton(new Rect(930,780,270,72),"MAIN MENU",arcadeCream,24, uiKey:"ArcadeInterface_16517"))ReturnToMainMenu();
        }}
        void DrawSkill()
        {using(NativeUiScope("DrawSkill" + skillType,"DrawSkill")){
            if(skillType==1){DrawCashier();return;}
            Shade(uiKey:"ArcadeInterface_16708");Sticker(new Rect(392,227,816,443),arcadeCream,true,6, uiKey:"ArcadeInterface_16716");
            Panel(new Rect(398,233,804,90),skillType==0?arcadeTeal:arcadeCoral, uiKey:"ArcadeInterface_16783");
            Icon(new Rect(424,248,62,62),skillType==0?"stock":"clean",arcadeCream, uiKey:"ArcadeInterface_16864");
            ArcadeText(new Rect(505,245,650,67),skillType==0?"GET THOSE SHELVES FULL!":"MAKE IT SPARKLE!",36,arcadeCream, uiKey:"ArcadeInterface_16948");
            DrawTaskCountdown(new Rect(439,325,722,22));DrawTaskDrive();
            int required=skillType==0?3:4;
            for(int i=0;i<required;i++)
            {
                var r=new Rect(698+i*58-required*29,354,46,46);Sticker(r,i<taskCombo?arcadeTeal:new Color(.79f,.84f,.78f),false,3, uiKey:"ArcadeInterface_17219");
                if(i<taskCombo)Icon(new Rect(r.x+6,r.y+6,34,34),"check",arcadeCream, uiKey:"ArcadeInterface_17319");
                else ArcadeText(r,(i+1).ToString(),22,arcadeNavy,TextAnchor.MiddleCenter, uiKey:"ArcadeInterface_17395");
            }
            ArcadeText(new Rect(428,408,744,34),"Tap DO IT or press E / SPACE inside the green zone.",23,arcadeNavy,TextAnchor.MiddleCenter, uiKey:"ArcadeInterface_17491");
            Sticker(new Rect(439,465,722,43),new Color(.75f,.79f,.71f),false,4, uiKey:"ArcadeInterface_17631");
            Panel(new Rect(446+708*(SkillZoneCenter()-SkillZoneHalf()),471,708*SkillZoneHalf()*2,31),arcadeTeal, uiKey:"ArcadeInterface_17712");
            Sticker(new Rect(446+SkillMarker()*708-7,454,14,65),Time.unscaledTime<skillFlash?arcadeCoral:arcadeGold,false,3, uiKey:"ArcadeInterface_17826");
            GUI.enabled=!replay;
            if(UiButton(new Rect(439,563,487,64),"SPACE  /  DO IT!",arcadeGold,28, uiKey:"ArcadeInterface_17988"))AttemptSkill();
            if(UiButton(new Rect(946,563,215,64),"CANCEL",arcadeCream,24, uiKey:"ArcadeInterface_18087")){skillType=-1;playerVelocity=Vector2.zero;}
            GUI.enabled=true;
            if(taskFeedback.Length>0)Text(new Rect(445,524,710,28),taskFeedback,18,taskFeedbackFailed?arcadeCoral:arcadeTeal,FontStyle.Bold,TextAnchor.MiddleCenter, uiKey:"ArcadeInterface_18257");
        }}
        void DrawCashier()
        {using(NativeUiScope("DrawCashier","DrawCashier")){
            Shade(uiKey:"ArcadeInterface_18414");Sticker(new Rect(325,139,950,622),arcadeNavy,true,6, uiKey:"ArcadeInterface_18422");
            Sticker(new Rect(343,158,397,582),arcadeCream,false,3, uiKey:"ArcadeInterface_18488");
            Icon(new Rect(497,172,72,72),"serve",arcadeTeal, uiKey:"ArcadeInterface_18556");
            ArcadeText(new Rect(368,242,347,47),"CUSTOMER RECEIPT",29,arcadeNavy,TextAnchor.MiddleCenter, uiKey:"ArcadeInterface_18618");
            ArcadeText(new Rect(368,289,347,28),"ADD IT UP. RING IT THROUGH.",16,arcadeTeal,TextAnchor.MiddleCenter, uiKey:"ArcadeInterface_18725");
            for(int x=372;x<710;x+=17)Fill(new Rect(x,331,8,2),new Color(.60f,.65f,.59f), uiKey:"ArcadeInterface_18869");
            for(int i=0;receiptNames!=null&&i<receiptNames.Length;i++)
            {
                Text(new Rect(372,352+i*40,213,34),receiptNames[i],22,arcadeNavy, uiKey:"ArcadeInterface_19023");
                ArcadeText(new Rect(586,352+i*40,125,34),Ringgit(receiptPrices[i]),24,arcadeNavy,TextAnchor.MiddleRight, uiKey:"ArcadeInterface_19106");
            }
            DrawTaskCountdown(new Rect(372,558,338,16));
            if(taskFeedback.Length>0)Text(new Rect(371,608,340,66),taskFeedback,19,arcadeCoral,FontStyle.Bold, uiKey:"ArcadeInterface_19263");
            if(UiButton(new Rect(372,678,338,43),"CANCEL  /  ESC",new Color(.83f,.87f,.79f),19, uiKey:"ArcadeInterface_19353"))skillType=-1;
            Sticker(new Rect(762,158,492,582),new Color(.33f,.65f,.60f),false,3, uiKey:"ArcadeInterface_19460");
            ArcadeText(new Rect(785,174,446,42),"TOKTOK TILL",30,arcadeCream,TextAnchor.MiddleCenter, uiKey:"ArcadeInterface_19542");
            Sticker(new Rect(785,228,446,83),Time.unscaledTime<skillFlash?arcadeCoral:arcadeNavy,false,4, uiKey:"ArcadeInterface_19645");
            ArcadeText(new Rect(800,240,413,59),"RM "+(priceEntry.Length==0?"0.00":priceEntry),40,arcadeGold,TextAnchor.MiddleRight, uiKey:"ArcadeInterface_19752");
            string[] keys={"7","8","9","4","5","6","1","2","3",".","0","DEL"};GUI.enabled=!replay;
            for(int i=0;i<12;i++)if(UiButton(new Rect(785+i%3*152,331+i/3*72,141,60),keys[i],i==11?arcadeCoral:arcadeCream,29, uiKey:"ArcadeInterface_20009"))
            {if(i==9)EnterPricePoint();else if(i==11){if(priceEntry.Length>0)priceEntry=priceEntry.Substring(0,priceEntry.Length-1);}else EnterPriceDigit(keys[i][0]);}
            if(UiButton(new Rect(785,637,446,68),"CHARGE  /  ENTER",arcadeGold,28, uiKey:"ArcadeInterface_20284"))SubmitPrice();
            GUI.enabled=true;
        }}
    }
}
