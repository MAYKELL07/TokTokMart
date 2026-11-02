from pathlib import Path
p=Path('Assets/GokilKisah/Scripts')
f=p/'StoreGameController.cs';s=f.read_text(encoding='utf-8-sig')
s=s.replace('public void Choose(bool chooseZack) { selected=true;', 'public void Choose(bool chooseZack) { mainMenu=false; selected=true;')
s=s.replace('skillType=-1; playerVelocity=Vector2.zero; touchHeld=false;', 'cameraInitialized=false; priceEntry=""; taskCombo=0; skillType=-1; playerVelocity=Vector2.zero; touchHeld=false;')
s=s.replace('if(!selected || gameOver || settingsOpen || autoPaused)', 'if(mainMenu || !selected || gameOver || settingsOpen || autoPaused)')
s=s.replace('void BeginSkill(int type,int target) {PlaySound("Click");', 'void BeginSkill(int type,int target) {PrepareTask(type);PlaySound("Click");')
s=s.replace('float SkillMarker() {return Mathf.PingPong(skillClock*(skillType==1?.85f:.65f),1);}', 'float SkillMarker() {return Mathf.PingPong(skillClock*(skillType==2?.75f+.08f*taskCombo:.65f+.09f*taskCombo),1);}\n        float SkillZoneCenter()=>skillType==0?.5f:taskCombo%2==0?.35f:.65f;\n        float SkillZoneHalf()=>Mathf.Max(.11f,.18f-taskCombo*.025f);')
s=s.replace('if(skillType<0)return false; float marker=SkillMarker();','if(skillType<0)return false; if(skillType==1)return SubmitPrice(); float marker=SkillMarker();')
s=s.replace('if(marker<.36f||marker>.64f)', 'if(Mathf.Abs(marker-SkillZoneCenter())>SkillZoneHalf())')
s=s.replace('skillFlash=Time.unscaledTime+.25f;Toast("Just missed! Try the green zone.");','skillFlash=Time.unscaledTime+.25f;taskFeedback="Missed — aim for the highlighted zone.";Toast("Just missed! Try the green zone.");')
s=s.replace('if(skillType==0){shelfStock', 'taskCombo++;skillScans=taskCombo;taskFeedback="Nice! "+taskCombo+" / "+(skillType==0?3:4);if(taskCombo<(skillType==0?3:4)){skillClock=0;return true;}\n            if(skillType==0){shelfStock')
s=s.replace('SetStep("03 / CHECKOUT / THREE SCANS","Move to the checkout. Time three successful scans to serve a customer.")','SetStep("03 / CHECKOUT / ENTER THE TOTAL","Move to checkout. Add up the receipt and enter the total on the keypad.")')
s=s.replace('else if(skillType==1){skillScans++;if(skillScans<3){skillClock=0;return true;}ServeCustomer();SetStep("04 / CLEAR THE HAZARD","Move to the spill and pass the mop skill check. Store tasks continue with AI help.");}\n            ', '')
s=s.replace('while(skillType>=0){while(SkillMarker()<.44f||SkillMarker()>.55f)yield return null;AttemptSkill();yield return new WaitForSeconds(.12f);}', 'while(skillType>=0){if(skillType==1){priceEntry=receiptTotal.ToString();SubmitPrice();}else {while(Mathf.Abs(SkillMarker()-SkillZoneCenter())>.03f)yield return null;AttemptSkill();}yield return new WaitForSeconds(.12f);}')
f.write_text(s,encoding='utf-8')
f=p/'SceneBindings.cs';s=f.read_text(encoding='utf-8-sig');s=s.replace('{camera.transform.position=new Vector3(9.364815f,-4.627407f,-10);camera.orthographic=true;camera.orthographicSize=2.953704f;}','{FollowPlayerCamera(camera);}')
f.write_text(s,encoding='utf-8')
f=p/'CharacterSpriteAnimator.cs';s=f.read_text(encoding='utf-8-sig');s=s.replace('public bool animate=true;', 'public bool animate=true;\n        public string smoothSkin="";')
s=s.replace('if(!animate||!atlas||!artwork)return;', '''if(artwork&&!string.IsNullOrEmpty(smoothSkin))
            {
                int direction=facing==23?0:facing==26?1:facing==25?2:3;
                int frame=motion.sqrMagnitude>.001f?(int)(clock*framesPerSecond)%3:0;
                string skin=smoothSkin=="Keeper"?(row<6?"Sam":"Zack"):smoothSkin;
                artwork.sprite=SmoothArt.Load(skin+"_"+direction+"_"+frame);
                return;
            }
            if(!animate||!atlas||!artwork)return;''')
f.write_text(s,encoding='utf-8')
f=p/'StoreInterface.cs';s=f.read_text(encoding='utf-8-sig');s=s.replace('GUIKeyboard();','CashierKeyboard();GUIKeyboard();')
s=s.replace('if(cleanCaptureMode)\n            {','if(mainMenu&&!cleanCaptureMode){DrawMainMenu();GUI.matrix=Matrix4x4.identity;return;}\n            if(cleanCaptureMode)\n            {')
s=s.replace('skillType>=0?"Time it!  [Space]":', 'skillType==1?"Enter receipt total":skillType>=0?"Time it!  [Space]":')
s=s.replace('Tile(urbanArt,24,zack?6:0,new Rect(300,31,44,53));','GUI.DrawTexture(new Rect(300,31,44,53),Resources.Load<Texture2D>("SmoothArt/"+(zack?"Zack":"Sam")+"_3_0"),ScaleMode.ScaleToFit);')
s=s.replace('if(stockActions>0)return "Head to checkout and complete three scans.";', 'if(stockActions>0)return "Head to checkout and enter the receipt total.";')
s=s.replace('Panel(r,new Color(.87f,.90f,.81f));Tile(urbanArt,24,row,new Rect(r.center.x-48,r.y+17,96,112));','Panel(r,new Color(.87f,.90f,.81f));GUI.DrawTexture(new Rect(r.center.x-48,r.y+17,96,112),Resources.Load<Texture2D>("SmoothArt/"+name+"_3_0"),ScaleMode.ScaleToFit);')
s=s.replace('void DrawSkill()\n        {','void DrawSkill()\n        {\n            if(skillType==1){DrawCashier();return;}')
s=s.replace('skillType==0?"Fill the shelf":skillType==1?"Scan "+(skillScans+1)+" of 3":"Mop the spill"','skillType==0?"Stock item "+(taskCombo+1)+" of 3":"Mop stroke "+(taskCombo+1)+" of 4"')
s=s.replace('Panel(new Rect(454+690*.36f,467,690*.28f,32),teal);','Panel(new Rect(454+690*(SkillZoneCenter()-SkillZoneHalf()),467,690*SkillZoneHalf()*2,32),teal);')
s=s.replace('GUI.enabled=true;\n        }\n        void DrawGameOver()', 'GUI.enabled=true;Text(new Rect(454,512,690,25),taskFeedback,17,teal,FontStyle.Bold);\n        }\n        void DrawGameOver()')
s=s.replace('if(Event.current.isMouse)Event.current.Use();','if(Button(new Rect(267,730,290,44),"Return to main menu",surface,19))ReturnToMainMenu();\n            if(Event.current.isMouse)Event.current.Use();')
s=s.replace('"Made with artist-made assets"', '"Art & audio credits"').replace('"Game art & sound effects\\nKenney Vleugels · Kenney.nl · CC0', '"Smooth store & character art\\nOriginal vector artwork · Tok Mart\\n\\nSound effects\\nKenney Vleugels · Kenney.nl · CC0')
f.write_text(s,encoding='utf-8')
