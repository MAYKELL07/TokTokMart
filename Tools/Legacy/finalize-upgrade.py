from pathlib import Path
for name in ['StoreGameController.cs','StoreInterface.cs']:
 p=Path('Assets/GokilKisah/Scripts')/name;s=p.read_text(encoding="utf-8-sig");s=s.replace('Vector2.Distance(player,checkout)<82','Vector2.Distance(player,checkout)<135');p.write_text(s,encoding="utf-8")
p=Path('Assets/GokilKisah/Scripts/GameplayVerification.cs');s=p.read_text(encoding='utf-8-sig').replace('StartCoroutine(Verify(resultPath));','BeginUpgradeVerification(resultPath);');p.write_text(s,encoding="utf-8")
p=Path('Assets/GokilKisah/Scripts/CustomerReplayCapture.cs');s=p.read_text(encoding='utf-8-sig').replace('Time three scans to take payment.','Add the receipt and enter the total to take payment.');p.write_text(s,encoding="utf-8")

