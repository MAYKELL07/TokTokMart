from pathlib import Path
p=Path('Assets/GokilKisah/Editor/ShiftSceneUpgrade.cs');s=p.read_text(encoding='utf-8-sig').replace('new Vector2(.53f,.75f)','new Vector2(.685f,.80f)').replace('shelf.ShowStock(shelf.initialStock);','shelf.ShowStock(shelf.initialStock==0?1:0);shelf.ShowStock(shelf.initialStock);');p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/CustomerSimulation.cs');s=p.read_text(encoding='utf-8-sig').replace('public int id, shelf=-1, basket, wanted, facing=25, visits;','public int id, shelf=-1, basket, wanted, facing=25, visits;\n            public readonly List<int> items=new List<int>();').replace('shelfStock[c.shelf]--;c.basket++;','shelfStock[c.shelf]--;c.items.Add(c.shelf%groceries.Length);c.basket++;');p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/ShiftUpgrade.cs');s=p.read_text(encoding='utf-8-sig').replace('int item=((customer==null?0:customer.id)+i)%groceries.Length;', 'int item=customer!=null&&i<customer.items.Count?customer.items[i]:((customer==null?0:customer.id)+i)%groceries.Length;');p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/UpgradeVerification.cs');s=p.read_text(encoding='utf-8-sig');s=s.replace('public void BeginUpgradeVerification', '''public void BeginUpgradePreview(string directory){StartCoroutine(CaptureUpgradePreview(directory));}
        IEnumerator CaptureUpgradePreview(string directory)
        {
            Directory.CreateDirectory(directory);ReturnToMainMenu();showPitchControls=false;showTouchControls=false;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"main-menu.png"));
            mainMenu=false;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"choose-character.png"));
            Choose(false);testing=true;player=playerStart;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"store.png"));
            var shopper=SpawnCustomer();shopper.basket=3;shopper.items.AddRange(new[]{0,1,2});shopper.state=ShopperState.Queuing;shopper.position=QueuePoint(0);customerQueue.Add(shopper);
            player=ShopLayout.ToGame(shop.register.approachPoint.position);shop.playerCharacter.Apply(player,24,0,Vector2.zero,pulse);cameraInitialized=false;Interact();
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"cashier.png"));
            foreach(var actor in shop.GetComponentsInChildren<CharacterSpriteAnimator>(true))Debug.Log("Smooth actor "+actor.name+" size="+actor.artwork.bounds.size+" scale="+actor.artwork.transform.lossyScale);
            testing=false;
        }
        public void BeginUpgradeVerification''')
p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Editor/PitchEditorTools.cs');s=p.read_text(encoding='utf-8-sig').replace('case "upgradeverify":','case "upgradepreview":RequireGame().BeginUpgradePreview(command.path);ReplyTo(command.action,"Preview screens captured and cashier opened");break;\n                    case "upgradeverify":');p.write_text(s,encoding='utf-8')
