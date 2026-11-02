using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        Font boldFont;
        Texture2D roundedPanel;
        GUIStyle panelStyle;
        Vector2 playerVelocity;
        bool settingsOpen, autoPaused, focused=true, showPitchControls, showTouchControls, showNames, pauseOnFocusLoss=true;
        int settingsTab, frameCap=60, resolutionIndex=1;
        float masterVolume=.8f, musicVolume=.35f, effectsVolume=.7f, skillFlash, textScale=1;
        AudioSource musicSource, effectsSource;
        readonly Dictionary<string,AudioClip> sounds=new Dictionary<string,AudioClip>();
        readonly List<AudioEvent> captureSounds=new List<AudioEvent>();
        bool recordingSounds;
        int recordingFrame;
        readonly HashSet<KeyCode> heldKeys=new HashSet<KeyCode>();
        int actionInputFrame=-1,escapeInputFrame=-1;
        [Serializable] class AudioEvent { public int frame;public string sound;public float gain; }
        readonly Color paper=new Color(.97f,.95f,.89f), surface=new Color(.15f,.23f,.23f), teal=new Color(.13f,.67f,.53f);
        readonly string[] scenarioTitles={"Store shift","Goblins","K-Kung","Miss Salmah","Bo & the gang"};
        readonly string[] scenarioDescriptions={
            "Keep the shelves stocked, serve your customers and clean up spills.",
            "Call Uncle Ray and keep the shop running until he collects the goblins.",
            "Find the switch. Fluorescent lights will drive the possessed idols away.",
            "Pick up the nail gun, then move behind Miss Salmah. It fires automatically.",
            "Find the poo-filled Ikan Iblis and kick it at Bo before he throws his can."};
        void InitializePolish()
        {
            LoadControlSettings();
            masterVolume=PlayerPrefs.GetFloat("TokMart.Master",.8f);musicVolume=PlayerPrefs.GetFloat("TokMart.Music",.35f);effectsVolume=PlayerPrefs.GetFloat("TokMart.Effects",.7f);
            showPitchControls=PlayerPrefs.GetInt("TokMart.Pitch",0)==1;
            showTouchControls=PlayerPrefs.GetInt("TokMart.Touch",Application.isMobilePlatform?1:0)==1;
            showNames=PlayerPrefs.GetInt("TokMart.Names",0)==1;
            textScale=PlayerPrefs.GetFloat("TokMart.Text",1);frameCap=PlayerPrefs.GetInt("TokMart.FPS",60);
            pauseOnFocusLoss=PlayerPrefs.GetInt("TokMart.Focus",1)==1;
            foreach(var source in GetComponents<AudioSource>()){if(source.loop&&!musicSource)musicSource=source;else if(!source.loop&&!effectsSource)effectsSource=source;}
            if(!musicSource)musicSource=gameObject.AddComponent<AudioSource>();
            musicSource.loop=true;musicSource.playOnAwake=false;musicSource.clip=Resources.Load<AudioClip>("Audio/ShopMusic");
            if(!effectsSource)effectsSource=gameObject.AddComponent<AudioSource>();
            effectsSource.playOnAwake=false;effectsSource.spatialBlend=0;
            sounds.Clear();
            foreach(string key in new[]{"Click","Confirm","Pickup","Stock","Scan","Mop","Miss","Call","Switch","Shot","Kick","Impact","Success","Footstep"})
            {var clip=Resources.Load<AudioClip>("Audio/"+key);if(clip)sounds.Add(key,clip);else Debug.LogError("Missing sound: "+key);}
            QualitySettings.vSyncCount=0;Application.targetFrameRate=frameCap;
            ApplyAudio();
            if(musicSource.clip&&!musicSource.isPlaying)musicSource.Play();
        }
        void PlaySound(string key,float gain=1)
        {
            if(!effectsSource||!sounds.TryGetValue(key,out var clip))return;
            effectsSource.PlayOneShot(clip,Mathf.Clamp01(gain));
            if(recordingSounds)captureSounds.Add(new AudioEvent{frame=recordingFrame,sound=key,gain=gain});
        }
        void ApplyAudio()
        {
            if(musicSource)musicSource.volume=masterVolume*musicVolume;
            if(effectsSource)effectsSource.volume=masterVolume*effectsVolume;
        }
        void SaveSettings()
        {
            SaveControlSettings();
            PlayerPrefs.SetFloat("TokMart.Master",masterVolume);PlayerPrefs.SetFloat("TokMart.Music",musicVolume);PlayerPrefs.SetFloat("TokMart.Effects",effectsVolume);
            PlayerPrefs.SetInt("TokMart.Pitch",showPitchControls?1:0);PlayerPrefs.SetInt("TokMart.Touch",showTouchControls?1:0);PlayerPrefs.SetInt("TokMart.Names",showNames?1:0);
            PlayerPrefs.SetInt("TokMart.Focus",pauseOnFocusLoss?1:0);PlayerPrefs.SetFloat("TokMart.Text",textScale);PlayerPrefs.SetInt("TokMart.FPS",frameCap);PlayerPrefs.Save();ApplyAudio();
        }
        void SetSettingsOpen(bool open)
        {settingsOpen=open;bufferedActionUntil=-1;ReleaseMobileControls();playerVelocity=Vector2.zero;Time.timeScale=open||autoPaused||shiftBriefOpen?0:1;if(!open)SaveSettings();}
        void UpdatePolish()
        {
            autoPaused=!focused&&pauseOnFocusLoss&&!testing&&!verificationRunning&&!recordingSounds;
            Time.timeScale=settingsOpen||autoPaused||shiftBriefOpen?0:1;
            if(Keyboard.current!=null&&Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                EscapeInput();
            }
            ApplyAudio();
        }
        void OnApplicationFocus(bool focus) {focused=focus;if(!focus){bufferedActionUntil=-1;heldKeys.Clear();ReleaseMobileControls();playerVelocity=Vector2.zero;}}
        void OnApplicationPause(bool paused) {if(paused){ReleaseMobileControls();bufferedActionUntil=-1;heldKeys.Clear();playerVelocity=Vector2.zero;SaveSettings();}focused=!paused;}
        void EscapeInput()
        {
            if(escapeInputFrame==Time.frameCount)return;escapeInputFrame=Time.frameCount;
            if(skillType>=0&&!settingsOpen&&!replay){skillType=-1;Toast("Task cancelled");}
            else SetSettingsOpen(!settingsOpen);
        }
        void ActionInput()
        {
            if(actionInputFrame==Time.frameCount||settingsOpen||autoPaused||replay||!selected||mainMenu||gameOver||levelResults||shiftBriefOpen)return;
            actionInputFrame=Time.frameCount;
            if(skillType>=0){bufferedActionUntil=-1;Interact();}
            else {bufferedActionUntil=Time.unscaledTime+.14f;if(InteractionInReach()){bufferedActionUntil=-1;Interact();}}
        }
        void GUIKeyboard()
        {
            // IMGUI events also support keyboard access through desktop automation and accessibility tools.
            var e=Event.current;
            if(e.type==EventType.KeyUp){heldKeys.Remove(e.keyCode);return;}
            if(e.type!=EventType.KeyDown||!heldKeys.Add(e.keyCode))return;
            if(e.keyCode==KeyCode.Escape){EscapeInput();e.Use();}
            else if((e.keyCode==KeyCode.E||e.keyCode==KeyCode.Space)&&selected&&!settingsOpen){ActionInput();e.Use();}
        }
        void StopReplay()
        {if(demoRoutine!=null)StopCoroutine(demoRoutine);demoRoutine=null;replay=false;ResetRun(encounter);}
        void Panel(Rect r,Color c,bool shadow=false)
        {
            if(drawingWorldOverlay&&!WorldOverlayVisible(r))return;
            if(!roundedPanel)
            {
                roundedPanel=new Texture2D(32,32,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear};
                for(int y=0;y<32;y++)for(int x=0;x<32;x++)
                {
                    float dx=Mathf.Max(8-x,Mathf.Max(0,x-23)),dy=Mathf.Max(8-y,Mathf.Max(0,y-23));
                    roundedPanel.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(8.5f-Mathf.Sqrt(dx*dx+dy*dy))));
                }
                roundedPanel.Apply();panelStyle=new GUIStyle{normal={background=roundedPanel},border=new RectOffset(10,10,10,10)};
            }
            if(Event.current.type!=EventType.Repaint)return;
            if(shadow){GUI.color=new Color(0,0,0,.18f);panelStyle.Draw(new Rect(r.x,r.y+5,r.width,r.height),false,false,false,false);}
            GUI.color=c;panelStyle.Draw(r,false,false,false,false);GUI.color=Color.white;
        }
        bool UiButton(Rect r,string label,Color c,int size=18)
        {
            bool hover=r.Contains(Event.current.mousePosition)&&GUI.enabled;
            Sticker(r,!GUI.enabled?Color.Lerp(c,paper,.45f):hover?Color.Lerp(c,Color.white,.17f):c);
            ArcadeText(r,label,size,c==surface||c==arcadeCoral?arcadeCream:arcadeNavy,TextAnchor.MiddleCenter);
            bool clicked=InterfaceButton(r);if(clicked)PlaySound("Click",.55f);return clicked;
        }
        void Eyebrow(Rect r,string label,Color c) {Text(r,label,14,c,FontStyle.Bold);}
        void Shade() {Fill(new Rect(0,0,1600,900),new Color(.04f,.10f,.10f,.75f));}
        void OnGUI()
        {
            CashierKeyboard();GUIKeyboard();
            float scale=Mathf.Min(Screen.width/1600f,Screen.height/900f);
            GUI.matrix=Matrix4x4.identity;
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1600*scale)/2,(Screen.height-900*scale)/2,0),Quaternion.identity,new Vector3(scale,scale,1));
            if(mainMenu&&!cleanCaptureMode){DrawNovemberMenu();GUI.matrix=Matrix4x4.identity;return;}
            if(levelResults&&!cleanCaptureMode){DrawArcadeResults();if(settingsOpen)DrawSettings();GUI.matrix=Matrix4x4.identity;return;}
            if(cleanCaptureMode){if(skillType>=0)DrawSkill();GUI.matrix=Matrix4x4.identity;return;}
            var canvas=GUI.matrix;GUI.matrix=SceneOverlayMatrix();drawingWorldOverlay=true;
            DrawStore();drawingWorldOverlay=false;GUI.matrix=canvas;
            GUI.enabled=!settingsOpen;
            if(selected)DrawNovemberHud();
            GUI.enabled=!settingsOpen&&!replay;
            if(showTouchControls&&selected&&!settingsOpen&&!gameOver&&!shiftBriefOpen&&skillType<0)DrawMobileControls();
            GUI.enabled=true;
            if(!selected&&!settingsOpen)DrawArcadeSelection();
            else if(gameOver&&!settingsOpen)DrawGameOver();
            else if(skillType>=0&&!settingsOpen)DrawSkill();
            if(shiftBriefOpen&&!settingsOpen)DrawShiftBriefing();
            if(Time.unscaledTime<noticeUntil&&!settingsOpen&&selected&&!shiftBriefOpen)
            {Sticker(new Rect(480,700,640,44),arcadeNavy);ArcadeText(new Rect(493,704,614,34),notice,18,arcadeCream,TextAnchor.MiddleCenter);}
            if(autoPaused&&!settingsOpen){Shade();ArcadeText(new Rect(450,380,700,80),"ON A BREAK",56,arcadeCream,TextAnchor.MiddleCenter,true);ArcadeText(new Rect(450,470,700,40),"Return to the shop to carry on.",23,arcadeCream,TextAnchor.MiddleCenter);}
            if(settingsOpen)DrawSettings();
            GUI.matrix=Matrix4x4.identity;
        }
        void Stat(Rect r,string label,int value)
        {Eyebrow(new Rect(r.x,r.y,r.width,20),label,new Color(.35f,.47f,.43f));Text(new Rect(r.x,r.y+20,r.width,38),value.ToString("00"),30,ink,FontStyle.Bold);}
        string ObjectiveText()
        {
            if(campaignActive)return CampaignObjective();
            if(replay)return stepBody;
            if(gameOver)return "Bo threw Ikan Iblis. Try again and beat the countdown.";
            if(resolved&&encounter!=Encounter.Store)return "Keep stocking, serving and cleaning.";
            if(encounter==Encounter.Goblins)return rayCalled?"Uncle Ray arrives in "+rayRemaining.ToString("0.0")+"s.":"Call Uncle Ray from the wall phone.";
            if(encounter==Encounter.KKung)return "Turn on the fluorescent lights.";
            if(encounter==Encounter.Salmah)return hasGun?"Move behind Miss Salmah to fire.":"Pick up the nail gun near the back wall.";
            if(encounter==Encounter.Bo)return canKicked?"Ikan Iblis is heading to Bo.":"Find Ikan Iblis and kick it before the timer runs out.";
            if(hazardsCleared>0&&stockActions>0)return "Keep stocking, serving and cleaning.";
            if(stockActions>0&&served>0)return "Mop the spill.";
            if(stockActions>0)return "Serve a customer at checkout.";
            return carryingCrate?"Restock a shelf.":"Collect a crate from the stockroom.";
        }
        string InteractionLabel()
        {
            if(skillType>=0)return "Time the task";
            if(encounter==Encounter.Goblins&&!resolved&&Vector2.Distance(player,phone)<76)return rayCalled?"Ray is on his way":"Call Uncle Ray";
            if(encounter==Encounter.KKung&&!resolved&&Vector2.Distance(player,lightSwitch)<70)return "Turn on the lights";
            if(encounter==Encounter.Salmah&&!hasGun&&Vector2.Distance(player,nailGun)<70)return "Pick up nail gun";
            if(encounter==Encounter.Bo&&!canKicked&&Vector2.Distance(player,can)<70)return "Kick Ikan Iblis";
            if(NearestSpill()>=0)return "Mop the spill";
            if(Vector2.Distance(player,supply)<75)return "Collect stock";
            int index=NearestShelf();if(index>=0)return shelfStock[index]>=ShelfCapacity(index)?"Shelf is full":carryingCrate?"Restock shelf":"Needs a stock crate";
            if(Vector2.Distance(player,checkout)<135)return CustomerAtRegister()!=null?"Serve customer":"Waiting for a shopper";
            return "Move near a station";
        }
        void DrawContextHint()
        {
            string hint=InteractionLabel();
            if(!selected||replay||skillType>=0||hint=="Move near a station")return;
            Panel(new Rect(505,691,590,44),surface);
            Text(new Rect(515,699,570,28),"E  ·  "+hint,19,paper,FontStyle.Bold,TextAnchor.MiddleCenter);
        }
        void DrawSelection()
        {
            Shade();Panel(new Rect(385,185,830,530),paper);
            Text(new Rect(432,220,736,58),"Choose a character",38,ink,FontStyle.Bold,TextAnchor.MiddleCenter);
            CharacterCard(new Rect(430,315,347,246),"Sam",0,false);
            CharacterCard(new Rect(822,315,347,246),"Zack",6,true);
            Text(new Rect(432,620,736,40),"WASD  Move     Shift  Sprint     E  Interact",21,ink,FontStyle.Normal,TextAnchor.MiddleCenter);
        }
        void CharacterCard(Rect r,string name,int row,bool pickZack)
        {
            Panel(r,new Color(.87f,.90f,.81f));
            string portrait=pickZack?"OnlineArt/"+name+"_3_0":"CharacterArt/Art_TTM_Char_Sam01";
            GUI.DrawTexture(new Rect(r.center.x-48,r.y+17,96,112),Resources.Load<Texture2D>(portrait),ScaleMode.ScaleToFit);
            Text(new Rect(r.x,r.y+138,r.width,35),name,27,ink,FontStyle.Bold,TextAnchor.MiddleCenter);
            if(Button(new Rect(r.x+24,r.y+187,r.width-48,43),"Select",pickZack?new Color(.67f,.74f,.98f):mint,20))Choose(pickZack);
        }
        void DrawLegacySkill()
        {
            if(skillType==1){DrawCashier();return;}
            Shade();Panel(new Rect(420,282,760,348),paper,true);
            Eyebrow(new Rect(454,311,690,25),skillType==0?"RESTOCKING":skillType==1?"CHECKOUT":"CLEANUP",teal);
            Text(new Rect(454,350,690,42),skillType==0?"Stock item "+(taskCombo+1)+" of 3":"Mop stroke "+(taskCombo+1)+" of 4",34,ink,FontStyle.Bold);
            Text(new Rect(454,404,690,35),"Press Space when the marker is in the green zone.",21,new Color(.30f,.41f,.39f));
            Panel(new Rect(454,467,690,32),Time.unscaledTime<skillFlash?red:new Color(.75f,.80f,.73f));
            Panel(new Rect(454+690*(SkillZoneCenter()-SkillZoneHalf()),467,690*SkillZoneHalf()*2,32),teal);
            Panel(new Rect(454+SkillMarker()*690-5,457,10,52),ink);
            GUI.enabled=!replay;
            if(Button(new Rect(454,546,460,51),"Time it!  [Space]",gold,21))AttemptSkill();
            if(Button(new Rect(936,546,208,51),"Cancel  [Esc]",new Color(.83f,.86f,.79f),18)){skillType=-1;playerVelocity=Vector2.zero;}
            GUI.enabled=true;Text(new Rect(454,512,690,25),taskFeedback,17,teal,FontStyle.Bold);
        }
        void DrawGameOver()
        {
            Shade();Panel(new Rect(450,285,700,336),paper,true);
            Eyebrow(new Rect(490,316,620,22),"SHIFT INTERRUPTED",red);
            Text(new Rect(490,361,620,56),"Bo beat the countdown.",34,ink,FontStyle.Bold);
            Text(new Rect(490,430,620,66),"Find the scattered Ikan Iblis and kick it at his car before he throws his can.",23,new Color(.30f,.41f,.39f));
            if(Button(new Rect(490,533,620,51),"Try again",gold,22))ResetRun(Encounter.Bo);
        }
        float Slider(Rect r,float value)
        {
            value=InterfaceSlider(r,value);
            int id=GUIUtility.GetControlID(FocusType.Passive);
            Event e=Event.current;
            if(e.type==EventType.MouseDown&&r.Contains(e.mousePosition)){GUIUtility.hotControl=id;value=Mathf.Clamp01((e.mousePosition.x-r.x)/r.width);e.Use();}
            if(GUIUtility.hotControl==id&&(e.type==EventType.MouseDrag||e.type==EventType.MouseDown)){value=Mathf.Clamp01((e.mousePosition.x-r.x)/r.width);e.Use();}
            if(GUIUtility.hotControl==id&&e.rawType==EventType.MouseUp){GUIUtility.hotControl=0;e.Use();}
            Panel(new Rect(r.x,r.y+12,r.width,8),new Color(.70f,.76f,.68f));
            Panel(new Rect(r.x,r.y+12,r.width*value,8),teal);
            Panel(new Rect(r.x+r.width*value-10,r.y+6,20,20),ink);return value;
        }
        void VolumeRow(float y,string label,ref float value)
        {Text(new Rect(641,y,265,35),label,23,ink,FontStyle.Bold);value=Slider(new Rect(934,y+1,320,36),value);Text(new Rect(1273,y,80,36),Mathf.RoundToInt(value*100)+"%",21,ink);}
        void ToggleRow(float y,string title,string detail,ref bool value)
        {
            Text(new Rect(641,y,560,32),title,23,ink,FontStyle.Bold);Text(new Rect(641,y+36,570,54),detail,18,new Color(.30f,.41f,.39f));
            if(Button(new Rect(1245,y+5,90,42),value?"On":"Off",value?mint:new Color(.79f,.83f,.76f),18))value=!value;
        }
        void DrawSettings()
        {
            Shade();Sticker(new Rect(240,102,1120,694),paper,true,6);
            ArcadeText(new Rect(278,133,590,55),"SHOP SETTINGS",38,arcadeNavy);
            if(Button(new Rect(1187,133,133,46),"Back",surface,17))SetSettingsOpen(false);
            Fill(new Rect(278,206,1042,1),new Color(.77f,.82f,.75f));
            string[] tabs={"Audio","Display","Gameplay","Pitch tools","Controls","Credits"};
            for(int i=0;i<tabs.Length;i++)if(Button(new Rect(278,235+i*62,270,52),tabs[i],settingsTab==i?mint:new Color(.86f,.89f,.82f),21))settingsTab=i;
            Fill(new Rect(582,235,1,475),new Color(.77f,.82f,.75f));
            if(settingsTab==0)
            {
                Text(new Rect(641,235,650,45),"Audio",30,ink,FontStyle.Bold);
                VolumeRow(322,"Master volume",ref masterVolume);VolumeRow(403,"Music",ref musicVolume);VolumeRow(484,"Sound effects",ref effectsVolume);
                if(Button(new Rect(641,592,325,47),"Preview sound",mint,20))PlaySound("Success");
                Text(new Rect(641,659,650,48),"Shop music by Cleyton Kauffman · Sound effects by Kenney",18,new Color(.30f,.41f,.39f));
            }
            else if(settingsTab==1)
            {
                Text(new Rect(641,235,650,45),"Display & readability",30,ink,FontStyle.Bold);
                GUI.enabled=!Application.isMobilePlatform;
                Text(new Rect(641,306,650,32),"Window resolution",23,ink,FontStyle.Bold);
                string[] presets={"1280 × 720","1920 × 1080","2560 × 1440"};
                for(int i=0;i<3;i++)if(Button(new Rect(641+i*229,350,215,47),presets[i],resolutionIndex==i?mint:new Color(.84f,.88f,.81f),18)){resolutionIndex=i;if(!Application.isEditor)Screen.SetResolution(i==0?1280:i==1?1920:2560,i==0?720:i==1?1080:1440,Screen.fullScreenMode);else Toast("Resolution applies in the standalone game.");}
                if(Button(new Rect(641,423,690,46),Screen.fullScreen?"Switch to windowed":"Switch to fullscreen",new Color(.84f,.88f,.81f),20)){if(!Application.isEditor)Screen.fullScreen=!Screen.fullScreen;else Toast("Fullscreen applies in the standalone game.");}
                GUI.enabled=true;
                Text(new Rect(641,496,650,32),"Frame rate",23,ink,FontStyle.Bold);
                int[] rates={30,60,120};for(int i=0;i<3;i++)if(Button(new Rect(641+i*229,537,215,45),rates[i]+" fps",frameCap==rates[i]?mint:new Color(.84f,.88f,.81f),19)){frameCap=rates[i];Application.targetFrameRate=frameCap;}
                Text(new Rect(641,617,270,35),"Text size",23,ink,FontStyle.Bold);
                if(Button(new Rect(940,612,183,45),"Standard",textScale<1.1f?mint:new Color(.84f,.88f,.81f),18))textScale=1;
                if(Button(new Rect(1139,612,192,45),"Larger",textScale>1.1f?mint:new Color(.84f,.88f,.81f),18))textScale=1.15f;
            }
            else if(settingsTab==2)
            {
                ToggleRow(236,"Touch controls","Joystick + hold Sprint + tap Interact. Customize in Controls.",ref showTouchControls);
                ToggleRow(337,"Character names","Show name tags over characters.",ref showNames);
                ToggleRow(438,"Pause when unfocused","Freeze the shift when you switch windows.",ref pauseOnFocusLoss);
                if(campaignActive)
                {
                    Text(new Rect(641,542,650,42),"Level "+CurrentLevel+" · "+ServeGoal+" customers / "+StockGoal+" shelves / "+CleanGoal+" spills",21,ink,FontStyle.Bold);
                    Text(new Rect(641,596,650,65),"A boss arrives every five levels. Defeated bosses can return at random.",21,new Color(.30f,.41f,.39f));
                    if(Button(new Rect(641,717,690,44),"Restart level "+CurrentLevel,gold,19)){SetSettingsOpen(false);BeginCampaignLevel(CurrentLevel,campaignPersistence);}
                }
                else
                {
                    Text(new Rect(641,542,650,42),"Choose an encounter",23,ink,FontStyle.Bold);
                    GUI.enabled=!replay;
                    for(int i=0;i<5;i++)if(Button(new Rect(641+(i%3)*229,587+(i/3)*57,215,46),scenarioTitles[i],encounter==(Encounter)i?mint:new Color(.84f,.88f,.81f),18)){ResetRun((Encounter)i);SetSettingsOpen(false);}
                    GUI.enabled=true;
                    if(Button(new Rect(641,717,690,44),"Restart this encounter",gold,19)){StopReplay();SetSettingsOpen(false);}
                }
            }
            else if(settingsTab==3)
            {
                ToggleRow(236,"Show pitch controls","Keep guided replay controls hidden during normal play.",ref showPitchControls);
                Text(new Rect(641,360,650,60),"Guided replays use the same movement, collisions and interactions as the game.",22,new Color(.30f,.41f,.39f));
                for(int i=0;i<5;i++)if(Button(new Rect(641+(i%3)*229,466+(i/3)*65,215,51),scenarioTitles[i],new Color(.84f,.88f,.81f),19)){SetSettingsOpen(false);StartReplay((Encounter)i);}
                if(Button(new Rect(1099,531,215,51),"Customer journey",new Color(.84f,.88f,.81f),19)){SetSettingsOpen(false);StartCustomerReplay();}
                if(replay&&Button(new Rect(641,650,690,52),"Stop replay",gold,21)){StopReplay();SetSettingsOpen(false);}
            }
            else if(settingsTab==4)DrawControlSettings();
            else
            {
                Text(new Rect(641,235,650,45),"Art & audio credits",30,ink,FontStyle.Bold);
                Text(new Rect(641,306,650,300),"TokTokMart art & walk animations\nSupplied project artist exports · November 2026\nAdditional props: Kenney · CC0\n\nSound effects\nKenney Vleugels · Kenney.nl · CC0\n\nShop Theme / Buy something!\nCleyton Kauffman · OpenGameArt.org · CC0\n\nLato / Lilita One\nŁukasz Dziedzic / Juan Montoreano · SIL OFL",23,new Color(.25f,.38f,.34f));
                Text(new Rect(641,650,650,60),"Gokil Kisah mechanics follow the supplied design document. Sam, Zack and Toyol use supplied project artwork.",19,new Color(.30f,.41f,.39f));
            }
            if(Button(new Rect(267,730,290,44),"Main menu",surface,19))ReturnToMainMenu();
            if(Event.current.isMouse)Event.current.Use();
        }
    }
}



