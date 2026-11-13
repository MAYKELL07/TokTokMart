from pathlib import Path
p=Path('Assets/GokilKisah/Scripts/StoreGameController.cs');s=p.read_text(encoding='utf-8-sig').replace('cameraInitialized=false; priceEntry="";', 'ResetStamina();cameraInitialized=false; priceEntry="";')
s=s.replace('if(skillType<0) {playerVelocity=Vector2.MoveTowards(playerVelocity,dir.normalized*tuning.moveSpeed,(dir.sqrMagnitude>0?1900:2400)*dt);MovePlayer(playerVelocity*dt);} else playerVelocity=Vector2.zero;', '''bool sprint=touchSprint||kb?.leftShiftKey.isPressed==true||kb?.rightShiftKey.isPressed==true||heldKeys.Contains(KeyCode.LeftShift)||heldKeys.Contains(KeyCode.RightShift);
                float speed=MovementSpeed(sprint,dir.sqrMagnitude>0);
                var before=player;
                if(skillType<0) {playerVelocity=Vector2.MoveTowards(playerVelocity,dir.normalized*speed,(dir.sqrMagnitude>0?1900:2400)*dt);MovePlayer(playerVelocity*dt);} else playerVelocity=Vector2.zero;
                UpdateStamina(dt,(player-before).sqrMagnitude>.0001f);''')
s=s.replace('var bounds=shop.walkableArea.bounds;','var bounds=shop.MapBounds;').replace('priceEntry=receiptTotal.ToString();','priceEntry=RinggitNumber(receiptTotal);');p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/ShiftUpgrade.cs');s=p.read_text(encoding='utf-8-sig').replace('{12000,8500,18000,6500,14000,3500}','{350,250,500,150,450,200}').replace('var bounds=shop.walkableArea.bounds;','var bounds=shop.MapBounds;')
s=s.replace('{if(priceEntry.Length<7){priceEntry+=digit;PlaySound("Click",.4f);taskFeedback="";}}','{int dot=priceEntry.IndexOf(\'.\');if(priceEntry.Length<8&&(dot<0||priceEntry.Length-dot<=2)){priceEntry+=digit;PlaySound("Click",.4f);taskFeedback="";}}')
s=s.replace('!int.TryParse(priceEntry,out int total)','!TryRinggit(priceEntry,out int total)').replace('Toast("Correct total · Rp "+receiptTotal.ToString("N0"))','Toast("Correct total · "+Ringgit(receiptTotal))')
s=s.replace('else if(e.keyCode==KeyCode.Backspace)', 'else if(e.keyCode==KeyCode.Period||e.keyCode==KeyCode.KeypadPeriod){EnterPricePoint();e.Use();}\n            else if(e.keyCode==KeyCode.Backspace)')
s=s.replace('Resources.Load<Texture2D>("SmoothArt/', 'Resources.Load<Texture2D>("OnlineArt/')
s=s.replace('Smooth art · Room to explore','Free Kenney art · Malaysian ringgit').replace('WASD / Arrows — move around the store.\\nE', 'WASD / Arrows — move. Hold Shift to sprint.\\nE').replace('Cashier — add the receipt prices and enter the total.','Cashier — enter the total in RM, e.g. 3.50.')
s=s.replace('"Rp "+receiptPrices[i].ToString("N0")','Ringgit(receiptPrices[i])').replace('"Rp "+(priceEntry.Length==0?"0":priceEntry)','"RM "+(priceEntry.Length==0?"0.00":priceEntry)').replace('"Add the item prices, then use the keypad."','"Add the prices in ringgit (MYR). Use . for sen."')
s=s.replace('"Clear","0","Back"','".","0","Back"').replace('if(i==9)priceEntry="";', 'if(i==9)EnterPricePoint();')
s=s.replace('Text(new Rect(565,650,235,42),"Numpad or number keys",16,new Color(.3f,.42f,.4f));','if(UiButton(new Rect(565,650,215,45),"Clear",gold,18))priceEntry="";')
p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/SmoothArt.cs');s=p.read_text(encoding='utf-8-sig').replace('var texture=Resources.Load<Texture2D>("SmoothArt/"+name);','var texture=Resources.Load<Texture2D>("OnlineArt/"+name)??Resources.Load<Texture2D>("SmoothArt/"+name);');p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/StoreInterface.cs');s=p.read_text(encoding='utf-8-sig').replace('Resources.Load<Texture2D>("SmoothArt/', 'Resources.Load<Texture2D>("OnlineArt/').replace('            Stat(new Rect(890', '            DrawStamina();Stat(new Rect(890',1).replace('WASD / Arrows  ·  E to interact','WASD · Shift to sprint · E to interact')
s=s.replace('Event e=Event.current;\n            if(e.type==EventType.MouseDown', 'if(Button(new Rect(42,545,166,44),touchSprint?"Sprint: ON":"Sprint: OFF",touchSprint?gold:mint,18))touchSprint=!touchSprint;\n            Event e=Event.current;\n            if(e.type==EventType.MouseDown')
s=s.replace('touchHeld=false;touchMove=Vector2.zero;playerVelocity=Vector2.zero;}}','touchHeld=false;touchSprint=false;touchMove=Vector2.zero;playerVelocity=Vector2.zero;}}')
s=s.replace('Smooth store & character art\\nOriginal vector artwork · Tok Mart', 'Game artwork\\nKenney Toon Characters & Mini Market · CC0')
p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/ShopLayout.cs');s=p.read_text(encoding='utf-8-sig').replace('public BoxCollider2D walkableArea;', '''public BoxCollider2D walkableArea, storageArea;
        public Transform storageDoor;
        public Bounds MapBounds{get{var bounds=walkableArea.bounds;if(storageArea)bounds.Encapsulate(storageArea.bounds);return bounds;}}
        public bool IsFloor(Vector2 point)=>walkableArea&&walkableArea.OverlapPoint(point)||storageArea&&storageArea.OverlapPoint(point);''');p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/SceneBindings.cs');s=p.read_text(encoding='utf-8-sig');start=s.index('            if(!shop.walkableArea||!shop.walkableArea.OverlapPoint');end=s.index('            foreach(var shape',start);s=s[:start]+'''            if(!shop.IsFloor(wp+Vector2.left*radius)||!shop.IsFloor(wp+Vector2.right*radius)||!shop.IsFloor(wp+Vector2.up*radius)||!shop.IsFloor(wp+Vector2.down*radius))return false;
'''+s[end:];p.write_text(s,encoding='utf-8')
