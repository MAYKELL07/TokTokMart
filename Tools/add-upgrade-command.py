from pathlib import Path
p=Path('Assets/GokilKisah/Editor/PitchEditorTools.cs')
s=p.read_text(encoding='utf-8-sig').replace('case "setup":','case "improve":ShiftSceneUpgrade.Apply();ReplyTo(command.action,"Larger store and smooth art saved");break;\n                    case "upgradeverify":RequireGame().BeginUpgradeVerification(command.path);ReplyTo(command.action,"Upgrade verification started");break;\n                    case "setup":')
p.write_text(s,encoding='utf-8')
