from pathlib import Path
p=Path('Assets/GokilKisah/Editor/OnlineMarketArt.cs');s=p.read_text(encoding='utf-8-sig').replace('new[]{"Fridge","freezers-standing"}', 'new[]{"Fridge","freezers-standing"},new[]{"Gun","@blaster-a"}')
s=s.replace('var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GokilKisah/ThirdParty/KenneyMiniMarket/"+item[1]+".fbx");', '''bool blaster=item[1].StartsWith("@");
                    string source=blaster?"Assets/GokilKisah/ThirdParty/KenneyBlasterKit/"+item[1].Substring(1)+".fbx":"Assets/GokilKisah/ThirdParty/KenneyMiniMarket/"+item[1]+".fbx";
                    material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(blaster?"Assets/GokilKisah/ThirdParty/KenneyBlasterKit/Textures/colormap.png":"Assets/GokilKisah/ThirdParty/KenneyMiniMarket/Textures/colormap.png");
                    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(source);''')
s=s.replace('model.transform.position=Vector3.zero;','model.transform.position=Vector3.zero;if(blaster)model.transform.rotation=Quaternion.Euler(0,90,0);')
s=s.replace('var size=renderer.bounds.size;renderer.sprite=sprite;', 'var size=renderer.bounds.size;renderer.sprite=sprite;if(name=="Spill")renderer.color=new Color(.3f,.68f,.85f,.5f);')
p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/StoreInterface.cs');s=p.read_text(encoding='utf-8-sig').replace('int id=GUIUtility.GetControlID(FocusType.Passive);if(Button(new Rect(42,545,166,44),touchSprint?"Sprint: ON":"Sprint: OFF",touchSprint?gold:mint,18))touchSprint=!touchSprint;','int id=GUIUtility.GetControlID(FocusType.Passive);');p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/ShiftUpgrade.cs');s=p.read_text(encoding='utf-8-sig').replace('new Rect(945,350+i*110,420,100)', 'new Rect(945+i*140,375,130,220)').replace('"OnlineArt/ShelfStock"),ScaleMode.StretchToFill','"OnlineArt/ShelfStock"),ScaleMode.ScaleToFit');p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/UpgradeVerification.cs');s=p.read_text(encoding='utf-8-sig').replace('            var shopper=SpawnCustomer();shopper.basket=3;', '''            player=supply+new Vector2(40,65);shop.playerCharacter.Apply(player,24,0,Vector2.zero,pulse);cameraInitialized=false;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"storage-room.png"));
            var shopper=SpawnCustomer();shopper.basket=3;''',1);p.write_text(s,encoding='utf-8')
