from pathlib import Path
p=Path('Assets/GokilKisah/Scripts/UpgradeVerification.cs');s=p.read_text(encoding='utf-8-sig').replace('var bounds=shop.walkableArea.bounds;', 'var bounds=shop.MapBounds;').replace('priceEntry=receiptTotal.ToString();', 'priceEntry=RinggitNumber(receiptTotal);').replace('priceEntry.Length==7','priceEntry.Length==8')
s=s.replace('            check(sceneShelves.Length==9', '''            check(shop.storageArea&&shop.storageDoor&&shop.schemaVersion>=6,"Separate storage room and doorway are saved");
            check(shop.storageArea.OverlapPoint(ShopLayout.ToWorld(supply)),"Stock pickup is inside the storage room");
            var roomRoute=playerStart;for(int n=0;n<2000&&Vector2.Distance(roomRoute,supply)>7;n++)roomRoute=Navigate(roomRoute,supply,12);
            check(Vector2.Distance(roomRoute,supply)<7,"Player and helper can reach stock through the doorway");
            var roomWall=ShopLayout.ToGame(shop.storageDoor.position+Vector3.up*1.1f);check(!Walkable(roomWall)&&Walkable(ShopLayout.ToGame(shop.storageDoor.position)),"Storage partition blocks feet while doorway remains open");
            check(sceneShelves.Length==9''')
s=s.replace('            var cam=shop.worldCamera;', '''            check(UnityEngine.Resources.Load<Texture2D>("OnlineArt/Sam_3_0")&&UnityEngine.Resources.Load<Texture2D>("OnlineArt/ShelfStock"),"Downloaded Kenney character and market resources load");
            check(UnityEngine.Resources.Load<Texture2D>("OnlineArt/ShelfStock").width==512,"Market models are baked into high-resolution non-pixel sprites");
            check(Ringgit(350)=="RM 3.50"&&TryRinggit("3.50",out int cents)&&cents==350,"MYR amounts format and parse exact ringgit and sen");
            check(!TryRinggit("3.501",out cents)&&!TryRinggit("-1",out cents),"Price entry rejects fractional sen and negative prices");
            priceEntry="3";EnterPricePoint();EnterPriceDigit('5');EnterPriceDigit('0');EnterPriceDigit('1');check(priceEntry=="3.50","Cashier accepts at most two decimal places");EnterPricePoint();check(priceEntry=="3.50","Cashier rejects a second decimal point");
            ResetStamina();float walk=MovementSpeed(false,true),run=MovementSpeed(true,true);check(run>walk&&Mathf.Approximately(run,walk*sprintMultiplier),"Sprint increases movement speed");
            UpdateStamina(1,true);check(Stamina<maxStamina&&IsSprinting,"Moving while sprinting drains stamina");float savedStamina=Stamina;MovementSpeed(true,false);UpdateStamina(.2f,false);check(Stamina==savedStamina&&!IsSprinting,"Standing still consumes no stamina");
            MovementSpeed(true,true);UpdateStamina(20,true);check(Stamina==0&&sprintExhausted&&MovementSpeed(true,true)==walk,"Exhaustion prevents sprinting below recovery threshold");
            UpdateStamina(staminaRecoveryDelay+.1f,false);check(Stamina>0&&sprintExhausted,"Stamina recovers after delay while exhausted");UpdateStamina(5,false);check(Stamina==maxStamina&&MovementSpeed(true,true)>walk,"Rest restores stamina and allows sprinting again");
            ResetStamina();MovementSpeed(true,true);UpdateStamina(1,false);check(Stamina==maxStamina,"Running against a wall does not drain stamina");
            skillType=0;check(MovementSpeed(true,true)==walk,"Tasks block sprinting");skillType=-1;ResetStamina();
            var cam=shop.worldCamera;''')
p.write_text(s,encoding='utf-8')
