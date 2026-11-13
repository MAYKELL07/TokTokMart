from pathlib import Path
p=Path('Assets/GokilKisah/Editor/ShiftSceneUpgrade.cs');s=p.read_text(encoding='utf-8-sig').replace('if(shop.schemaVersion<5)\n','bool expanding=shop.schemaVersion<5;\n            if(expanding)\n').replace('size=Vector2.Scale(size,new Vector2(1.65f,1.8f));','if(expanding)size=Vector2.Scale(size,new Vector2(1.65f,1.8f));');p.write_text(s,encoding='utf-8')
