using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GokilKisah
{
    // 1600x900 logical canvas. World sprites come from credited artist-made Kenney packs.
    public sealed partial class StoreGameController : MonoBehaviour
    {
        public enum Encounter { Store, Goblins, KKung, Salmah, Bo }
        [Serializable] public class Tuning
        {
            public float moveSpeed = 260, rayWait = 5, boDeadline = 18, nailRange = 165;
            public int nailHits = 3;
        }
        public Tuning tuning = new Tuning();
        public Encounter encounter;
        public bool selected, zack, replay, gameOver, resolved, hasGun, carryingCrate;
        public Vector2 player = new Vector2(760, 710), helper = new Vector2(1070, 400);
        public int served, stockActions, hazardsCleared, nailsHit, goblins;
        public string stepTitle = "OPEN THE STORE", stepBody = "Choose Sam or Zack. The other character helps with store tasks.";
        public string notice = "", aiStatus = "Waiting for store opening";
        public Vector2 supply = new Vector2(380, 660), checkout = new Vector2(1230, 430), phone = new Vector2(360, 290), nailGun = new Vector2(1210, 215);
        public Vector2 lightSwitch, can, salmah = new Vector2(1080, 575);
        public Vector2 salmahFacing = Vector2.left;
        public bool rayCalled, lightsOn, canFlying, canKicked;
        public float rayRemaining, boRemaining, chaos, shiftTime, nailCooldown, rayX = 1490;
        public Vector2 flyingCan;
        public int[] shelfStock = { 0, 4, 2, 4, 0, 3 };
        public readonly List<Rect> shelves = new List<Rect>();
        public readonly List<Vector2> spills = new List<Vector2>();
        public int switchIndex = -1, canIndex = -1;
        int skillType = -1, skillTarget, skillScans;
        float skillClock, aiWork, noticeUntil, pulse, nextMess, nextAttack, customerHit;
        Vector2 aiGoal, touchOrigin, touchMove;
        bool touchHeld;
        bool cleanCaptureMode;
        string captureDirectory;
        Coroutine demoRoutine;
        Font font;
        readonly Color ink = new Color(.08f,.13f,.19f), muted = new Color(.70f,.77f,.79f), mint = new Color(.35f,.91f,.70f), gold = new Color(1,.76f,.30f), red = new Color(1,.34f,.36f);
        readonly string[] names = { "STORE TASKS", "GOBLINS / UNCLE RAY", "K-KUNG / LIGHT SWITCH", "MISS SALMAH / REAR ATTACK", "BO / TIMED CAN KICK" };
        Vector2[] switches = { new Vector2(350,450), new Vector2(1080,190), new Vector2(1300,660), new Vector2(730,190) };
        Vector2[] cans = { new Vector2(425,495), new Vector2(810,670), new Vector2(1030,245) };

        void Awake()
        {
            Application.runInBackground = true;
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 60;
            cleanCaptureMode=Array.IndexOf(Environment.GetCommandLineArgs(), "--clean-capture")>=0;
            InitializeCanvas();
            InitializePolish();
            ResetRun(Encounter.Store);
            foreach(string arg in Environment.GetCommandLineArgs())
            {
                if(arg.StartsWith("--pitch-output=")) { captureDirectory=arg.Substring(15); StartCoroutine(CaptureAll()); }
                if(arg.StartsWith("--verify-output=")) BeginVerification(arg.Substring(16));
                if(arg.StartsWith("--customer-output=")) BeginCustomerCapture(arg.Substring(18));
                if(arg.StartsWith("--ui-output=")) BeginPresentationCapture(arg.Substring(12));
                if(arg.StartsWith("--release-verify="))BeginReleaseVerification(arg.Substring(17));
                if(arg.StartsWith("--gameplay-refresh-verify="))BeginGameplayRefreshVerification(arg.Substring(26));
            }
        }
        void OnEnable()
        {
            // Unity's script reload does not preserve readonly transient lists or rerun Awake.
            // Rebuild the canvas and reset a demo after a live edit in Play Mode.
            if(shelves.Count==0){InitializeCanvas();InitializePolish();ResetRun(encounter);selected=false;replay=false;}
        }
        void InitializeCanvas()
        {
            font = Resources.Load<Font>("Fonts/Bigbesty") ?? Resources.Load<Font>("Fonts/Lato-Regular") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            boldFont = Resources.Load<Font>("Fonts/Bigbesty") ?? Resources.Load<Font>("Fonts/Lato-Bold") ?? font;
            LoadArtistArt();
            shelves.Clear();
            InitializeSceneBindings();
        }
        void OnDestroy() { Time.captureFramerate=0; Time.timeScale=1; foreach(var icon in arcadeIcons.Values)if(icon)Destroy(icon);arcadeIcons.Clear(); if(invoiceStar)Destroy(invoiceStar);if(roundedPanel)Destroy(roundedPanel);if(shopFloor)Destroy(shopFloor);if(streetFloor)Destroy(streetFloor); }
        public void Choose(bool chooseZack) { mainMenu=false; selected=true; zack=chooseZack; if(!testing&&!verificationRunning&&!replay)BeginCampaignLevel(SavedLevel); PlaySound("Confirm"); aiStatus="On register standby"; }
        public void ResetRun(Encounter next)
        {
            if(campaignActive){tuning.rayWait=5;tuning.boDeadline=18;tuning.nailHits=3;}
            shiftBriefOpen=false;
            ResetSpillLifecycle();
            campaignActive=false;levelResults=false;bossActive=false;mandatoryBossPending=false;
            ResetStamina();cameraInitialized=false; priceEntry=""; taskCombo=0; skillType=-1; playerVelocity=Vector2.zero; touchHeld=false; touchMove=Vector2.zero;
            encounter=next; player=new Vector2(760,710); helper=new Vector2(1135,365); gameOver=false; resolved=false;
            bufferedActionUntil=-1;bufferedActionUntil=-1;lastPlayer=player;lastHelper=helper;playerMotion=Vector2.zero;helperMotion=Vector2.zero;playerFacing=24;helperFacing=24;
            carryingCrate=false; hasGun=false; rayCalled=false; lightsOn=false; canFlying=false; canKicked=false;
            rayRemaining=0; rayX=1490; boRemaining=tuning.boDeadline; chaos=0; shiftTime=0; nailsHit=0; nailCooldown=0;
            served=0; stockActions=0; hazardsCleared=0; skillType=-1; aiWork=0; nextMess=3; nextAttack=1; customerHit=0;
            shelfStock=new[]{0,4,2,4,0,3}; spills.Clear(); spills.Add(new Vector2(795,490));
            goblins=next==Encounter.Goblins?3:0;
            int newSwitch=UnityEngine.Random.Range(0,switches.Length-1); if(newSwitch>=switchIndex) newSwitch++; switchIndex=newSwitch; lightSwitch=switches[newSwitch];
            int newCan=UnityEngine.Random.Range(0,cans.Length-1); if(newCan>=canIndex) newCan++; canIndex=newCan; can=cans[newCan];
            ResetSceneState();
            ResetCustomers();ResetActiveEncounter(next);
            aiGoal=supply; aiStatus="On register standby";registerArrivals=0;ResetShopMishaps();
            switch(next)
            {
                case Encounter.Store: SetStep("01 / COLLECT A CRATE","Go to the stockroom, restock a shelf, scan a checkout and clear a spill."); break;
                case Encounter.Goblins: SetStep("01 / GOBLINS MAKE A MESS","Call Uncle Ray, then shoo goblins to stun them for his punches."); break;
                case Encounter.KKung: SetStep("01 / POSSESSED IDOLS ARRIVE","Restore three circuits and dodge the marked attacks."); break;
                case Encounter.Salmah: SetStep("01 / MISS SALMAH ATTACKS","Collect the nail gun. Dodge her rush, flank her and tap Interact to shoot."); break;
                case Encounter.Bo: SetStep("01 / BO AIMS IKAN IBLIS","Find and return three cans before each countdown expires."); break;
            }
        }
        public void SetStep(string title,string body) { stepTitle=title; stepBody=body; }
        void Toast(string value) { notice=value; noticeUntil=Time.unscaledTime+3; }
        void Update()
        {
            ReadManualSceneEdits();
            UpdateInterfacePointer();
            UpdatePolish();
            UpdateMobileControls();
            pulse+=Time.deltaTime;
            if(shiftTransition!=null || mainMenu || !selected || gameOver || levelResults || settingsOpen || autoPaused || shiftBriefOpen || scoreGuideOpen) return;
            float dt=Time.deltaTime; shiftTime+=dt;TickShiftFeedback(dt);
            if(!replay&&!verificationRunning)
            {
                Vector2 dir=touchMove; var kb=Keyboard.current;
                if(kb?.aKey.isPressed==true||kb?.leftArrowKey.isPressed==true||heldKeys.Contains(KeyCode.A)||heldKeys.Contains(KeyCode.LeftArrow))dir.x--;
                if(kb?.dKey.isPressed==true||kb?.rightArrowKey.isPressed==true||heldKeys.Contains(KeyCode.D)||heldKeys.Contains(KeyCode.RightArrow))dir.x++;
                if(kb?.wKey.isPressed==true||kb?.upArrowKey.isPressed==true||heldKeys.Contains(KeyCode.W)||heldKeys.Contains(KeyCode.UpArrow))dir.y--;
                if(kb?.sKey.isPressed==true||kb?.downArrowKey.isPressed==true||heldKeys.Contains(KeyCode.S)||heldKeys.Contains(KeyCode.DownArrow))dir.y++;
                if(kb!=null&&(kb.eKey.wasPressedThisFrame||kb.spaceKey.wasPressedThisFrame))ActionInput();
                bool sprint=touchSprint||kb?.leftShiftKey.isPressed==true||kb?.rightShiftKey.isPressed==true||heldKeys.Contains(KeyCode.LeftShift)||heldKeys.Contains(KeyCode.RightShift);
                float speed=MovementSpeed(sprint,dir.sqrMagnitude>0);
                var before=player;
                ApplyResponsiveMovement(dir,speed,dt);
                ProcessBufferedAction();
                UpdateStamina(dt,(player-before).sqrMagnitude>.0001f);
            }
            if(skillType>=0) skillClock+=dt;
            if(!testing) { UpdateCustomers(dt); UpdateAI(dt); }
            UpdateEncounter(dt);
            if(canFlying) { flyingCan=Vector2.MoveTowards(flyingCan,ShopLayout.ToGame(shop.boCarTarget.position),680*dt); if(Vector2.Distance(flyingCan,ShopLayout.ToGame(shop.boCarTarget.position))<8)FinishBoReturn(); }
            TrackArtistMotion();
            TickCampaign(dt,gameplayShiftPlaytest);
        }
        void UpdateEncounter(float dt)
        {
            customerHit=Mathf.Max(0,customerHit-dt);
            TickActiveEncounter(dt);
        }
        public bool CanHitSalmah() { Vector2 d=player-salmah; return d.magnitude<=tuning.nailRange && d.magnitude>2 && Vector2.Dot(d.normalized,salmahFacing)<-.45f; }
        const float FootRadius=12;
        Rect ShelfFootprint(int i) {var r=shelves[i];return new Rect(r.x,r.y+28,r.width,27);}
        bool CircleHitsRect(Vector2 p,Rect r)
        {var nearest=new Vector2(Mathf.Clamp(p.x,r.xMin,r.xMax),Mathf.Clamp(p.y,r.yMin,r.yMax));return (p-nearest).sqrMagnitude<FootRadius*FootRadius;}
        public bool Walkable(Vector2 p)
        {
            if(sceneInitialized&&!SceneWalkable(p))return false;
            if(routingShopper!=null)
            {
                foreach(var c in customers)
                    if(c!=routingShopper&&Vector2.Distance(p,c.position)<28)return false;
                foreach(var spill in spills)
                    if(Vector2.Distance(routingShopper.position,spill)>43&&Vector2.Distance(routingShopper.goal,spill)>43&&Vector2.Distance(p,spill)<30)return false;
            }
            return true;
        }
        void MovePlayer(Vector2 delta)
        {
            int steps=Mathf.Max(1,Mathf.CeilToInt(delta.magnitude/5));var step=delta/steps;
            for(int i=0;i<steps;i++)
            {
                var x=player+new Vector2(step.x,0);if(PlayerStepClear(x))player=x;
                var y=player+new Vector2(0,step.y);if(PlayerStepClear(y))player=y;
            }
        }
        int registerArrivals;
        void UpdateAI(float dt)
        {
            var shopper=CustomerAtRegister();
            if(shopper!=null&&shopper.registerTurn==0)
                shopper.registerTurn=++registerArrivals;

            aiGoal=ShopLayout.ToGame(shop.partnerCheckoutSpot.position);
            if(Vector2.Distance(helper,aiGoal)>8)
                helper=Navigate(helper,aiGoal,80*dt);

            if(shopper==null||shopper.registerTurn%5!=0||skillType==1)
            {
                aiWork=0;
                aiStatus="On register standby";
                return;
            }
            if(Vector2.Distance(helper,aiGoal)>8)return;

            aiStatus="Slowly ringing up a customer";
            aiWork+=dt;
            if(aiWork<5.5f)return;
            aiWork=0;
            Speak(shopper,"Wah, service finally respawned.");
            ServeCustomer();
        }
        public Vector2 ShelfPoint(int i) {return ShopLayout.ToGame(sceneShelves[i].shoppingPoint.position);}
        // Static routes and collision geometry are shared by all actors.
        public Vector2 Navigate(Vector2 from,Vector2 target,float distance)=>CachedNavigate(from,target,distance);
        bool LineClear(Vector2 a,Vector2 b)
        {
            if(!StaticSegmentClear(a,b))return false;
            if(routingShopper==null)return true;
            if(!Walkable(b))return false;
            int steps=Mathf.CeilToInt(Vector2.Distance(a,b)/10);
            for(int i=1;i<steps;i++)if(!Walkable(Vector2.Lerp(a,b,(float)i/steps)))return false;
            return true;
        }
        public void Interact()
        {
            if(!selected||gameOver||levelResults||settingsOpen||autoPaused||shiftBriefOpen||scoreGuideOpen||replay && demoRoutine==null)return;
            if(skillType>=0) { AttemptSkill();return; }
            if(TryEncounterInteraction())return;
            int droppedIndex=NearestDroppedItem();
            if(droppedIndex>=0){PickUpDroppedItem(droppedIndex);return;}
            int spillIndex=NearestSpill();
            if(spillIndex>=0){BeginSkill(2,spillIndex);return;}
            if(StationInReach(shop.stockroom)) { if(carryingCrate){Toast("Crate already held");return;} PlaySound("Pickup");carryingCrate=true;SetStep("02 / RESTOCK THE SHELF","Take the crate to a shelf. Drag the stock into its empty slots.");Toast("Crate collected");return; }
            int shelfIndex=NearestShelf();
            if(shelfIndex>=0) {if(!carryingCrate)Toast("Collect a crate first");else if(shelfStock[shelfIndex]>=ShelfCapacity(shelfIndex))Toast("This shelf is full");else BeginSkill(0,shelfIndex);return;}
            if(StationInReach(shop.register)) {if(CustomerAtRegister()!=null)BeginSkill(1,CustomerAtRegister().id);else Toast("Till is clear");return;}
            Toast("Move closer");
        }
        void BeginSkill(int type,int target) {if(type==2)CaptureMopTarget(target);else mopTargetView=null;PrepareTask(type);PlaySound("Click");playerVelocity=Vector2.zero;skillType=type;skillTarget=target;skillClock=0;skillScans=0;taskElapsed=0;taskLastAttempt=-1;taskFeedbackFailed=false;if(type==0)PrepareRestock();}
        float SkillMarker() {return Mathf.PingPong(skillClock*(.75f+.08f*taskCombo)*TaskDifficulty,1);}
        float SkillZoneCenter()=>taskCombo%2==0?.35f:.65f;
        float SkillZoneHalf()=>Mathf.Max(.07f,Mathf.Max(.11f,.18f-taskCombo*.025f)/TaskDifficulty);
        public bool AttemptSkill()
        {
            if(skillType<=0)return false; if(skillType==1)return SubmitPrice();
            if(Time.unscaledTime<taskLastAttempt+.18f)return false;taskLastAttempt=Time.unscaledTime;float marker=SkillMarker();
            if(Mathf.Abs(marker-SkillZoneCenter())>SkillZoneHalf()){RecordTaskMiss(marker<SkillZoneCenter()?"Left of the green zone":"Right of the green zone");return false;}
            PlaySound("Mop");
            taskFeedbackFailed=false;taskCombo++;skillScans=taskCombo;taskFeedback="Nice! "+taskCombo+" / 4";
            if(taskCombo<4){skillClock=0;return true;}
            if(FinishMopPatch())hazardsCleared++;
            SetStep("05 / STORE LOOP COMPLETE","Keep stocking, serving and cleaning.");
            FinishStoreTask("Spill cleaned");return true;
        }
        int TotalStock(){int total=0;foreach(int n in shelfStock)total+=n;return total;}
        void ServeCustomer()
        {
            var shopper=CustomerAtRegister();
            if(shopper==null)return;
            if(campaignActive)foreach(int item in shopper.items)salesCents+=UnitPrice(item,shopper.basket);
            LeaveRegister(shopper);served++;chaos=Mathf.Max(0,chaos-2);PlaySound("Confirm");
        }
        public void StartReplay(Encounter next) {if(demoRoutine!=null)StopCoroutine(demoRoutine);demoRoutine=StartCoroutine(Demo(next));}
        IEnumerator Go(Vector2 goal)
        {
            float remaining=14;
            while(Vector2.Distance(player,goal)>6 && remaining>0 && !gameOver) {player=Navigate(player,goal,tuning.moveSpeed*Time.deltaTime);remaining-=Time.deltaTime;yield return null;}
            if(remaining<=0)Debug.LogError("Replay navigation timed out: "+goal);
            yield return new WaitForSeconds(.5f);
        }
        IEnumerator CompleteSkill()
        {
            while(skillType>=0){if(skillType==0){if(!PlaceRestockPack())break;}else if(skillType==1){priceEntry=RinggitNumber(receiptTotal);SubmitPrice();}else {while(Mathf.Abs(SkillMarker()-SkillZoneCenter())>.03f)yield return null;AttemptSkill();}yield return new WaitForSeconds(.12f);}
            yield return new WaitForSeconds(.8f);
        }
        IEnumerator Demo(Encounter next)
        {
            replay=true;Choose(zack);ResetRun(next);yield return new WaitForSeconds(1.6f);
            if(next==Encounter.Store)
            {
                yield return Go(supply);Interact();yield return Go(ShelfPoint(4));Interact();yield return CompleteSkill();
                yield return Go(ShopLayout.ToGame(shop.register.approachPoint.position));
                float wait=35;while(CustomerAtRegister()==null&&wait>0){wait-=Time.deltaTime;yield return null;}
                Interact();yield return CompleteSkill();
                yield return Go(spills[0]);Interact();yield return CompleteSkill();
            }
            else yield return PlayActiveBoss(next);
            yield return new WaitForSeconds(2.4f);replay=false;demoRoutine=null;
        }
        public void BeginCapture(string directory) {captureDirectory=directory;StartCoroutine(CaptureAll());}
        [Serializable] class PitchFrame
        {
            public int frame,skill,scans,hits,stocked;public string title;public bool crate,gun,called,resolved,flying,kicked;public float canDistance;
        }
        IEnumerator CaptureAll()
        {
            Directory.CreateDirectory(captureDirectory);showPitchControls=true;settingsOpen=false;Time.timeScale=1;Choose(false);
            for(int i=0;i<5;i++)
            {
                string dir=Path.Combine(captureDirectory,"frames_"+i);Directory.CreateDirectory(dir);
                recordingSounds=true;recordingFrame=0;captureSounds.Clear();Time.captureFramerate=30;StartReplay((Encounter)i);int frame=0;var timeline=new List<string>();
                do {yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(dir,frame.ToString("D5")+".png"));timeline.Add(JsonUtility.ToJson(new PitchFrame{frame=frame,title=stepTitle,skill=skillType,scans=skillScans,hits=nailsHit,stocked=stockActions,crate=carryingCrate,gun=hasGun,called=rayCalled,resolved=resolved,flying=canFlying,kicked=canKicked,canDistance=Vector2.Distance(player,can)}));frame++;recordingFrame=frame;} while(replay||frame<2);
                File.WriteAllLines(Path.Combine(dir,"timeline.jsonl"),timeline);
                File.WriteAllLines(Path.Combine(dir,"audio.jsonl"),captureSounds.ConvertAll(e=>JsonUtility.ToJson(e)));recordingSounds=false;
                Time.captureFramerate=0;
                yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(captureDirectory,"0"+(i+1)+"_"+((Encounter)i)+".png"));
                yield return new WaitForSeconds(.4f);
            }
            File.WriteAllText(Path.Combine(captureDirectory,"capture-complete.txt"),"Five real Unity gameplay replays captured at 30 fps.");
            ResetRun(Encounter.Store);selected=false;
        }
        Color ActorColor(bool isZack) {return isZack?new Color(.53f,.64f,1):mint;}
        void Fill(Rect r,Color c,string uiKey=""){using(NativeUiScope(uiKey,"Fill")){if(drawingWorldOverlay){r=ClipWorldOverlay(r);if(r.width<=0||r.height<=0)return;}GUI.color=c;EditableTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;}}
        void Text(Rect r,string value,int size,Color c,FontStyle weight=FontStyle.Normal,TextAnchor align=TextAnchor.UpperLeft,string uiKey="")
        {using(NativeUiScope(uiKey,"Text: "+value)){if(drawingWorldOverlay&&!WorldOverlayVisible(r))return;int pixels=size<28?Mathf.RoundToInt(size*textScale):size;int key=pixels*100+(int)weight*10+(int)align;if(!labelStyles.TryGetValue(key,out var style)){style=new GUIStyle(GUI.skin.label){font=weight==FontStyle.Bold?boldFont:font,fontSize=pixels,fontStyle=FontStyle.Normal,alignment=align,wordWrap=true,padding=new RectOffset(0,0,0,0)};labelStyles[key]=style;}style.normal.textColor=c;EditableLabel(r,value,style);}}
        readonly Dictionary<int,GUIStyle> labelStyles=new Dictionary<int,GUIStyle>();
        bool Button(Rect r,string value,Color c,int size=16,string uiKey="")
        {using(NativeUiScope(uiKey,"Button")){return UiButton(r,value,c,size, uiKey:"StoreGameController_25424");}}
        void Bar(Rect r,float fraction,Color c){Fill(r,new Color(.14f,.23f,.29f), uiKey:"StoreGameController_25499");Fill(new Rect(r.x,r.y,r.width*Mathf.Clamp01(fraction),r.height),c, uiKey:"StoreGameController_25533");}
        void Actor(Vector2 p,string title,Color c,bool ai=false)
        {
            DrawArtistActor(p,title,ai);
        }
        void Station(Vector2 p,string label,Color c,string symbol)
        {
            DrawArtistStation(p,symbol);
            Text(new Rect(p.x-76,p.y+29,152,24),label,13,ink,FontStyle.Bold,TextAnchor.MiddleCenter, uiKey:"StoreGameController_25858");
            if(Vector2.Distance(player,p)<75) {Text(new Rect(p.x-30,p.y-48,60,22),"ACTION",11,new Color(.05f,.4f,.3f),FontStyle.Bold,TextAnchor.MiddleCenter, uiKey:"StoreGameController_25995");}
        }
    }
}


