using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        const string LevelSave="TokMart.Campaign.NextLevel", BossSave="TokMart.Campaign.Bosses", UnlockSave="TokMart.Campaign.HighestUnlocked";
        public int CurrentLevel {get;private set;}=1;
        public int UnlockedBossMask {get;private set;}
        public bool LevelResultsVisible=>levelResults;
        public bool CampaignActive=>campaignActive;
        bool campaignActive, campaignPersistence=true, levelResults, levelWon, bossActive, mandatoryBossPending;
        int salesCents, missedCustomers, priceMistakes, randomFights, invoiceStars;
        float encounterRollClock;
        string levelFailure="";
        Texture2D invoiceStar;
        ShiftTargets Targets=>new ShiftTargets(CurrentLevel);
        public int ServeGoal=>Targets.serve;
        public int StockGoal=>Targets.stock;
        public int CleanGoal=>Targets.clean;
        public float ShiftBudget=>Targets.duration;
        public float ArrivalInterval=>Mathf.Max(1.6f,4.5f/(1+(CurrentLevel-1)*.065f));
        public float ShiftRush=>Mathf.SmoothStep(0,1,Mathf.Clamp01(shiftTime/(campaignActive?ShiftBudget:240f)));
        public float CurrentArrivalInterval=>Mathf.Max(1.4f,(campaignActive?ArrivalInterval:4.5f)*Mathf.Lerp(2.6f,.65f,ShiftRush));
        public int CustomerLimit=>Mathf.RoundToInt(Mathf.Lerp(campaignActive?Mathf.Min(5,2+(CurrentLevel-1)/5):2,campaignActive?Mathf.Min(10,6+(CurrentLevel-1)/3):6,ShiftRush));
        public float CustomerPatience=>Mathf.Max(45,95/(1+(CurrentLevel-1)*.025f));
        public float TaskDifficulty=>campaignActive?Mathf.Min(1.35f,1+(CurrentLevel-1)*.018f):1;
        public static Encounter MilestoneBoss(int level)=>(Encounter)(1+((Mathf.Max(5,level)/5-1)%4));
        int SavedLevel=>canvasPreviewLevel>0?canvasPreviewLevel:chosenLevel>0?chosenLevel:Mathf.Max(1,PlayerPrefs.GetInt(LevelSave,1));

        public void BeginCampaignLevel(int level,bool persist=true)
        {
            ResetRun(Encounter.Store);
            campaignActive=true;campaignPersistence=persist;levelResults=false;levelWon=false;
            selected=true;mainMenu=false;CurrentLevel=Mathf.Clamp(level,1,10000);
            customerArrival=Mathf.Max(1.5f,4f/(1+(CurrentLevel-1)*.065f));
            UnlockedBossMask=PlayerPrefs.GetInt(BossSave,0)&15;
            mandatoryBossPending=CurrentLevel%5==0;bossActive=false;
            salesCents=0;missedCustomers=0;priceMistakes=0;timingMistakes=0;randomFights=0;levelFailure="";milestoneCleared=false;ResetShiftFeedback();
            encounterRollClock=mandatoryBossPending?5:UnityEngine.Random.Range(24f,42f);
            tuning.boDeadline=Mathf.Max(11,21-CurrentLevel*.2f);
            tuning.rayWait=Mathf.Min(15,5+CurrentLevel*.3f);
            tuning.nailHits=3+Mathf.Min(5,CurrentLevel/10);
            EnsureShiftCleanupWork();
            notifiedShiftGoals=0;
            shiftBriefOpen=!testing&&!verificationRunning&&!replay;
            if(persist){PlayerPrefs.SetInt(UnlockSave,Mathf.Max(HighestUnlocked,CurrentLevel));PlayerPrefs.SetInt(LevelSave,CurrentLevel);PlayerPrefs.Save();}
            SetStep("LEVEL "+CurrentLevel,"Serve customers, restock shelves, and clear spills before closing.");
            if(!shiftBriefOpen)Toast(mandatoryBossPending?"Boss shift: "+scenarioTitles[(int)MilestoneBoss(CurrentLevel)]:"Shift "+CurrentLevel+" · Open for business");
            else noticeUntil=0;
        }
        void SaveCampaignProgress(int nextLevel)
        {
            if(!campaignPersistence)return;
            PlayerPrefs.SetInt(UnlockSave,Mathf.Max(HighestUnlocked,nextLevel));
            PlayerPrefs.SetInt(LevelSave,nextLevel);PlayerPrefs.SetInt(BossSave,PlayerPrefs.GetInt(BossSave,0)|UnlockedBossMask);PlayerPrefs.Save();
        }
        void StartCampaignEncounter(Encounter next)
        {
            if(next==Encounter.Store)return;
            encounter=next;resolved=false;bossActive=true;rayCalled=false;rayRemaining=0;
            lightsOn=false;nailsHit=0;nailCooldown=0;hasGun=false;canFlying=false;canKicked=false;
            boRemaining=tuning.boDeadline;nextMess=2;nextAttack=1;
            goblins=next==Encounter.Goblins?shop.goblinSpawns.Length:0;
            rayX=rayStart.x;salmah=ShopLayout.ToGame(shop.salmahSpawn.position);
            switchIndex=(switchIndex+1)%switches.Length;lightSwitch=switches[switchIndex];
            canIndex=(canIndex+1)%cans.Length;can=cans[canIndex];
            Toast(scenarioTitles[(int)next]+"! "+scenarioDescriptions[(int)next]);
            PlaySound("Impact",.5f);
        }
        Encounter PickUnlockedBoss()
        {
            var pool=new List<Encounter>();for(int i=1;i<=4;i++)if(i*5<CurrentLevel&&(UnlockedBossMask&(1<<(i-1)))!=0)pool.Add((Encounter)i);
            return pool.Count==0?Encounter.Store:pool[UnityEngine.Random.Range(0,pool.Count)];
        }
        void TickCampaign(float dt,bool verification=false)
        {
            if(!campaignActive||levelResults||(!verification&&(testing||verificationRunning||replay)))return;
            NotifyShiftGoals();
            if(gameOver){FinishCampaignLevel(false,"Bo beat the countdown.");return;}
            if(bossActive&&resolved)
            {
                UnlockedBossMask|=1<<((int)encounter-1);bossActive=false;if(CurrentLevel%5==0)milestoneCleared=true;chaos=Mathf.Max(0,chaos-35);
                SaveCampaignProgress(CurrentLevel);
                Toast(scenarioTitles[(int)encounter]+" defeated · Can now return at random");
                encounter=Encounter.Store;encounterRollClock=UnityEngine.Random.Range(24f,42f);
            }
            if(shiftTime>=ShiftBudget){FinishCampaignLevel(false,"The shop closed before all targets were met.");return;}
            if(chaos>=100){FinishCampaignLevel(false,"The shop became too chaotic. Keep spills and queues under control.");return;}
            if(served>=ServeGoal&&stockActions>=StockGoal&&hazardsCleared>=CleanGoal&&!bossActive&&!mandatoryBossPending&&skillType<0)
            {FinishCampaignLevel(true,"");return;}
            if(bossActive)return;
            encounterRollClock-=dt;
            if(encounterRollClock>0||skillType>=0)return;
            if(mandatoryBossPending)
            {mandatoryBossPending=false;StartCampaignEncounter(MilestoneBoss(CurrentLevel));return;}
            encounterRollClock=UnityEngine.Random.Range(18f,32f);
            // Only defeated bosses are eligible; two surprise encounters at most in one shift.
            if(UnlockedBossMask!=0&&randomFights<2&&UnityEngine.Random.value<Mathf.Min(.65f,.25f+CurrentLevel*.01f))
            {var next=PickUnlockedBoss();if(next!=Encounter.Store){randomFights++;StartCampaignEncounter(next);}}
        }
        void FinishCampaignLevel(bool won,string reason)
        {
            if(levelResults)return;
            levelResults=true;levelWon=won;gameOver=!won;levelFailure=reason;skillType=-1;
            playerVelocity=Vector2.zero;ReleaseMobileControls();heldKeys.Clear();shiftBriefOpen=false;
            invoiceStars=Rating(won).stars;
            if(won&&campaignPersistence)SaveLevelBest();
            if(won){SaveCampaignProgress(CurrentLevel+1);PlaySound("Success");}else PlaySound("Miss");
        }
        string CampaignObjective()
        {
            if(bossActive)return scenarioDescriptions[(int)encounter];
            return "Level "+CurrentLevel+"   Serve "+served+"/"+ServeGoal+"   ·   Stock "+stockActions+"/"+StockGoal+"   ·   Clean "+hazardsCleared+"/"+CleanGoal;
        }
        int InvoicePenalty=>missedCustomers*100+priceMistakes*25;
        int InvoiceTotal=>salesCents+stockActions*75+hazardsCleared*50-InvoicePenalty;
        void DrawCampaignStatus()
        {
            if(!campaignActive||!selected||levelResults)return;
            Panel(new Rect(24,92,390,92),surface, uiKey:"LevelProgression_7543");
            Text(new Rect(40,102,355,28),"LEVEL "+CurrentLevel+(mandatoryBossPending||bossActive&&CurrentLevel%5==0?"  /  BOSS SHIFT":""),19,paper,FontStyle.Bold, uiKey:"LevelProgression_7594");
            float left=Mathf.Max(0,ShiftBudget-shiftTime);
            Text(new Rect(40,136,355,28),"Closing in "+((int)left/60)+":"+((int)left%60).ToString("00")+"   ·   Chaos "+Mathf.RoundToInt(chaos)+"%",18,left<35?gold:mint, uiKey:"LevelProgression_7817");
            if(bossActive)
            {
                Panel(new Rect(24,196,480,49),surface, uiKey:"LevelProgression_8033");
                Text(new Rect(38,201,452,38),"Goals: "+served+"/"+ServeGoal+" served  ·  "+stockActions+"/"+StockGoal+" stocked  ·  "+hazardsCleared+"/"+CleanGoal+" cleaned",16,paper, uiKey:"LevelProgression_8089");
            }
        }
        Texture2D InvoiceStar()
        {
            var supplied=NovemberArt.UI("Star");if(supplied)return supplied;
            if(invoiceStar)return invoiceStar;
            invoiceStar=new Texture2D(96,96,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear};
            var vertices=new Vector2[10];for(int i=0;i<10;i++){float angle=(90+i*36)*Mathf.Deg2Rad;vertices[i]=new Vector2(48,48)+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*(i%2==0?45:20);}
            var pixels=new Color[96*96];
            for(int y=0;y<96;y++)for(int x=0;x<96;x++)
            {
                bool inside=false;for(int i=0,j=9;i<10;j=i++)if((vertices[i].y>y)!=(vertices[j].y>y)&&x<(vertices[j].x-vertices[i].x)*(y-vertices[i].y)/(vertices[j].y-vertices[i].y)+vertices[i].x)inside=!inside;
                pixels[y*96+x]=inside?Color.white:Color.clear;
            }
            invoiceStar.SetPixels(pixels);invoiceStar.Apply();return invoiceStar;
        }
        void DrawLevelInvoice()
        {
            Fill(new Rect(0,0,1600,900),new Color(.94f,.95f,.97f), uiKey:"LevelProgression_9215");var blue=new Color(.24f,.45f,.66f);var paleBlue=new Color(.34f,.56f,.76f);
            Fill(new Rect(370,132,860,602),blue, uiKey:"LevelProgression_9357");Panel(new Rect(346,104,908,82),paleBlue,true, uiKey:"LevelProgression_9394");
            Text(new Rect(410,202,780,65),levelWon?"Invoice":"Shift interrupted",49,paper,FontStyle.Bold,TextAnchor.MiddleCenter, uiKey:"LevelProgression_9453");
            Text(new Rect(410,275,780,35),"LEVEL "+CurrentLevel+(levelWon?"  COMPLETE":"  /  TRY AGAIN"),21,paper,FontStyle.Bold,TextAnchor.MiddleCenter, uiKey:"LevelProgression_9584");
            string[] labels={"Sales · "+served+" customers","Shelf restocking","Cleanup bonus","Mistakes & lost customers"};
            int[] amounts={salesCents,stockActions*75,hazardsCleared*50,-InvoicePenalty};
            for(int i=0;i<4;i++)
            {
                Text(new Rect(445,334+i*45,520,35),labels[i],23,paper, uiKey:"LevelProgression_10005");
                Text(new Rect(965,334+i*45,190,35),(amounts[i]<0?"− ":"")+Ringgit(Mathf.Abs(amounts[i])),23,paper,FontStyle.Bold,TextAnchor.UpperRight, uiKey:"LevelProgression_10077");
            }
            Fill(new Rect(445,521,710,2),paleBlue, uiKey:"LevelProgression_10240");
            Text(new Rect(445,538,440,40),"Shift total",25,paper,FontStyle.Bold, uiKey:"LevelProgression_10292");
            Text(new Rect(950,538,205,40),(InvoiceTotal<0?"− ":"")+Ringgit(Mathf.Abs(InvoiceTotal)),25,paper,FontStyle.Bold,TextAnchor.UpperRight, uiKey:"LevelProgression_10374");
            for(int i=0;i<4;i++){GUI.color=i<invoiceStars?gold:new Color(.16f,.31f,.47f);EditableTexture(new Rect(504+i*151,598,108,108),InvoiceStar());}GUI.color=Color.white;
            if(!levelWon)Text(new Rect(370,748,860,44),levelFailure,19,paper,FontStyle.Normal,TextAnchor.MiddleCenter, uiKey:"LevelProgression_10711");
            if(Button(new Rect(370,805,625,57),levelWon?"Continue  →  Level "+(CurrentLevel+1):"Retry level "+CurrentLevel,paleBlue,24, uiKey:"LevelProgression_10821"))BeginCampaignLevel(levelWon?CurrentLevel+1:CurrentLevel,campaignPersistence);
            if(Button(new Rect(1010,805,220,57),"Main menu",surface,21, uiKey:"LevelProgression_11035"))ReturnToMainMenu();
        }

        public void BeginLevelVerification(string directory){StartCoroutine(VerifyLevels(directory));}
        IEnumerator VerifyLevels(string directory)
        {
            Directory.CreateDirectory(directory);var checks=new List<string>();int failures=0;
            Action<bool,string> check=(ok,name)=>{checks.Add((ok?"PASS ":"FAIL ")+name);if(!ok)failures++;};
            var randomState=UnityEngine.Random.state;testing=true;verificationRunning=true;settingsOpen=false;autoPaused=false;
            BeginCampaignLevel(1,false);check(CurrentLevel==1&&ServeGoal==3&&StockGoal==1&&CleanGoal==1,"Level one has explicit completion targets");
            float firstArrival=ArrivalInterval,firstPatience=CustomerPatience,firstTask=TaskDifficulty;
            BeginCampaignLevel(12,false);check(ServeGoal>3&&StockGoal>1&&CleanGoal>1&&ArrivalInterval<firstArrival&&CustomerPatience<firstPatience&&TaskDifficulty>firstTask,"Higher levels increase traffic, targets and timing difficulty and reduce patience");
            check(MilestoneBoss(5)==Encounter.Goblins&&MilestoneBoss(10)==Encounter.KKung&&MilestoneBoss(15)==Encounter.Salmah&&MilestoneBoss(20)==Encounter.Bo&&MilestoneBoss(25)==Encounter.Goblins,"Every fifth level cycles through all four bosses");
            BeginCampaignLevel(5,false);UnlockedBossMask=0;served=ServeGoal;stockActions=StockGoal;hazardsCleared=CleanGoal;TickCampaign(6,true);
            check(bossActive&&encounter==Encounter.Goblins&&!levelResults,"A boss milestone cannot finish before the boss is defeated");
            int paidBefore=salesCents;player=phone;Interact();UpdateEncounter(tuning.rayWait+.1f);TickCampaign(0,true);check((UnlockedBossMask&1)!=0&&levelResults&&levelWon&&served==ServeGoal&&salesCents==paidBefore,"Calling Ray defeats the milestone boss, unlocks it, and preserves shift progress");
            BeginCampaignLevel(6,false);UnlockedBossMask=1;bool onlyGoblin=true;for(int i=0;i<50;i++)onlyGoblin&=PickUnlockedBoss()==Encounter.Goblins;
            check(onlyGoblin,"Random encounter pool excludes bosses not yet defeated");
            UnityEngine.Random.InitState(31415);bool randomAppeared=false;
            for(int i=0;i<40&&!randomAppeared;i++){encounterRollClock=0;TickCampaign(0,true);randomAppeared=bossActive;}
            check(randomAppeared&&encounter==Encounter.Goblins&&randomFights==1,"An unlocked boss can actually appear at random on a later non-boss level");
            int keptServed=served;resolved=true;TickCampaign(0,true);check(!bossActive&&encounter==Encounter.Store&&served==keptServed,"Random boss defeat resumes the same shift");
            BeginCampaignLevel(1,false);served=ServeGoal;stockActions=StockGoal;hazardsCleared=CleanGoal;salesCents=1050;TickCampaign(0,true);
            check(levelResults&&levelWon&&invoiceStars==3&&InvoiceTotal==1175,"Completing targets opens a three-star invoice with correct earnings");
            int previous=CurrentLevel;BeginCampaignLevel(CurrentLevel+1,false);check(CurrentLevel==previous+1&&served==0&&!levelResults,"Continue advances a level and resets shift counters");
            shiftTime=ShiftBudget;TickCampaign(0,true);check(levelResults&&!levelWon&&gameOver,"Missing the shift deadline opens retry results");
            BeginCampaignLevel(20,false);StartCampaignEncounter(Encounter.Bo);boRemaining=.01f;UpdateEncounter(.1f);TickCampaign(.1f,true);check(levelResults&&!levelWon,"A failed boss countdown prevents level advancement");
            BeginCampaignLevel(1,false);UnlockedBossMask=0;check(PickUnlockedBoss()==Encounter.Store,"No random boss appears before the first boss is defeated");
            randomFights=2;UnlockedBossMask=15;for(int i=0;i<40;i++){encounterRollClock=0;TickCampaign(0,true);}check(!bossActive,"Random encounters respect the two-per-shift limit");
            BeginCampaignLevel(3,false);var impatient=SpawnCustomer();impatient.basket=1;impatient.state=ShopperState.Queuing;impatient.position=QueuePoint(0);impatient.timer=CustomerPatience+1;customerQueue.Add(impatient);UpdateCustomers(.1f);
            check(missedCustomers==1&&!customerQueue.Contains(impatient)&&chaos==8,"Expired customer patience removes the customer and adds a penalty");
            BeginCampaignLevel(1,false);var sale=SpawnCustomer();sale.basket=2;sale.items.AddRange(new[]{0,1});sale.state=ShopperState.Queuing;sale.position=QueuePoint(0);customerQueue.Add(sale);player=ShopLayout.ToGame(shop.register.approachPoint.position);Interact();
            priceEntry="1";SubmitPrice();check(priceMistakes==1&&served==0,"A wrong price is counted without completing a sale");
            priceEntry=RinggitNumber(receiptTotal);SubmitPrice();check(served==1&&salesCents==600&&InvoicePenalty==25,"Payment records actual basket revenue and invoice deductions");
            bool hadLevel=PlayerPrefs.HasKey(LevelSave),hadBoss=PlayerPrefs.HasKey(BossSave);int savedLevel=PlayerPrefs.GetInt(LevelSave),savedBoss=PlayerPrefs.GetInt(BossSave);
            try
            {
                BeginCampaignLevel(7,true);UnlockedBossMask=5;SaveCampaignProgress(8);BeginCampaignLevel(SavedLevel,false);
                check(CurrentLevel==8&&UnlockedBossMask==5,"Saved campaign resumes its level and defeated-boss pool");
            }
            finally
            {
                if(hadLevel)PlayerPrefs.SetInt(LevelSave,savedLevel);else PlayerPrefs.DeleteKey(LevelSave);
                if(hadBoss)PlayerPrefs.SetInt(BossSave,savedBoss);else PlayerPrefs.DeleteKey(BossSave);PlayerPrefs.Save();
            }
            BeginCampaignLevel(1,false);check(sceneShelves.Length==8,"Reference layout has six green shelves plus two red shelves");
            check(shop.storageArea.bounds.center.y>shop.walkableArea.bounds.max.y&&shop.storageArea.bounds.center.x>shop.walkableArea.bounds.center.x,"Storage and breaker room is at the top right");
            var geometry=new List<Vector2>{supply,ShopLayout.ToGame(shop.customerDoor.position),ShopLayout.ToGame(shop.register.approachPoint.position),phone,nailGun};
            foreach(var shelf in sceneShelves)geometry.Add(ShopLayout.ToGame(shelf.shoppingPoint.position));
            foreach(var marker in shop.switchLocations)geometry.Add(ShopLayout.ToGame(marker.position));
            foreach(var marker in shop.canLocations)geometry.Add(ShopLayout.ToGame(marker.position));
            foreach(var goal in geometry)
            {
                var p=playerStart;for(int n=0;n<2200&&Vector2.Distance(p,goal)>7;n++)p=Navigate(p,goal,16);
                check(Vector2.Distance(p,goal)<7&&Walkable(p),"Reference layout route "+goal);yield return null;
            }
            verificationRunning=false;testing=true;
            BeginCampaignLevel(1,false);gameplayZoom=8.2f;player=ShopLayout.ToGame(shop.MapBounds.center);cameraInitialized=false;noticeUntil=0;
            yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"layout-overview.png"));
            gameplayZoom=2.35f;player=playerStart;cameraInitialized=false;
            yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"level-one.png"));
            BeginCampaignLevel(5,false);UnlockedBossMask=0;encounterRollClock=0;TickCampaign(0,true);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"boss-level-five.png"));
            BeginCampaignLevel(6,false);UnlockedBossMask=1;StartCampaignEncounter(Encounter.Goblins);randomFights=1;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"random-boss-level-six.png"));
            BeginCampaignLevel(1,false);served=ServeGoal;stockActions=StockGoal;hazardsCleared=CleanGoal;salesCents=1050;TickCampaign(0,true);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"level-invoice.png"));
            File.WriteAllText(Path.Combine(directory,"level-verification.txt"),string.Join("\n",checks)+"\n\n"+checks.Count+" checks; "+failures+" failures.\n");
            UnityEngine.Random.state=randomState;ReturnToMainMenu();testing=false;verificationRunning=false;
            Debug.Log("TokTokMart levels: "+checks.Count+" checks; "+failures+" failures.");
        }
    }
}
