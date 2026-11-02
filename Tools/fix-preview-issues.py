from pathlib import Path
p=Path('Assets/GokilKisah/Editor/ShiftSceneUpgrade.cs');s=p.read_text(encoding='utf-8-sig').replace('GetComponentInParent<CharacterSpriteAnimator>()','GetComponentInParent<CharacterSpriteAnimator>(true)').replace('GetComponentInParent<ShopStation>()','GetComponentInParent<ShopStation>(true)').replace('GetComponentInParent<ShelfInventory>()','GetComponentInParent<ShelfInventory>(true)');p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/StoreInterface.cs');s=p.read_text(encoding='utf-8-sig');s=s.replace('Matrix4x4 canvas=GUI.matrix;float ws=1.08f;\n            GUI.matrix=SceneOverlayMatrix();\n            DrawStore();\n            GUI.matrix=canvas;', '''Matrix4x4 canvas=GUI.matrix;
            GUI.matrix=Matrix4x4.identity;
            var pixels=shop.worldCamera.pixelRect;
            var viewport=new Rect(pixels.x,Screen.height-pixels.yMax,pixels.width,pixels.height);
            GUI.BeginGroup(viewport);
            var overlay=SceneOverlayMatrix();overlay.m03-=viewport.x;overlay.m13-=viewport.y;
            GUI.matrix=overlay;DrawStore();GUI.matrix=Matrix4x4.identity;
            GUI.EndGroup();GUI.matrix=canvas;''');p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/UpgradeVerification.cs');s=p.read_text(encoding='utf-8-sig').replace('check(shop.playerCharacter.artwork.sprite.texture.width>=256', 'check(shop.customerTemplate.smoothSkin=="Customer"&&shop.goblinTemplate.smoothSkin=="Goblin"&&shop.idolTemplate.smoothSkin=="Idol","Inactive spawn templates use smooth artwork");\n            check(shop.playerCharacter.artwork.sprite.texture.width>=256');s=s.replace('check(TotalStock()<initialStock&&CustomerAtRegister()!=null,"Customers shop and physically arrive at register");', '''check(TotalStock()<initialStock&&CustomerAtRegister()!=null,"Customers shop and physically arrive at register");
            var actual=CustomerAtRegister();if(actual!=null){player=ShopLayout.ToGame(shop.register.approachPoint.position);Interact();int expected=0;foreach(int item in actual.items)expected+=groceryPrices[item];check(receiptTotal==expected&&actual.items.Count==actual.basket,"Receipt matches the products actually collected");}
            yield return new WaitForEndOfFrame();bool sizeOK=true;foreach(var view in shopperViews.Values)if(view.artwork.bounds.size.x<.4f||view.artwork.bounds.size.y<.6f)sizeOK=false;check(sizeOK,"Spawned customers retain full character size");''');p.write_text(s,encoding='utf-8')
