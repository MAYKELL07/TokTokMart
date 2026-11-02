from pathlib import Path
p=Path('Assets/GokilKisah/Editor/OnlineMarketArt.cs');s=p.read_text(encoding='utf-8-sig').replace('string path=AssetDatabase.GetAssetPath(renderer.sprite);', 'if(renderer.name=="Car bottom"){renderer.gameObject.SetActive(false);continue;}\n                string path=AssetDatabase.GetAssetPath(renderer.sprite);')
s=s.replace('var size=renderer.bounds.size;renderer.sprite=sprite;', 'var size=renderer.bounds.size;if(renderer.name=="Car top"){size=new Vector3(.75f,1.28f,0);renderer.transform.position+=Vector3.down*.32f;}renderer.sprite=sprite;')
s=s.replace('            AssetDatabase.SaveAssets();', '            AssetDatabase.SaveAssets();')
s=s.replace('            foreach(var actor in shop.GetComponentsInChildren<CharacterSpriteAnimator>(true))EditorUtility.SetDirty(actor);', '''            foreach(var actor in shop.GetComponentsInChildren<CharacterSpriteAnimator>(true))EditorUtility.SetDirty(actor);
            var audit=new System.Collections.Generic.List<string>();foreach(var renderer in shop.GetComponentsInChildren<SpriteRenderer>(true))if(renderer.gameObject.activeInHierarchy||renderer.GetComponentInParent<CharacterSpriteAnimator>(true))audit.Add(renderer.name+" | "+AssetDatabase.GetAssetPath(renderer.sprite));
            Directory.CreateDirectory("Verification");File.WriteAllLines("Verification/art-source-audit.txt",audit);''')
p.write_text(s,encoding='utf-8')
