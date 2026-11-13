from pathlib import Path
p=Path('Assets/GokilKisah/Scripts/EncounterPresentation.cs');s=p.read_text(encoding='utf-8-sig');start=s.index('            if(encounter==Encounter.Bo)');end=s.rindex('        }');s=s[:start]+s[end:];s=s.replace('if(encounter==Encounter.KKung&&!lightsOn)Fill(new Rect(320,187,1035,548),new Color(.13f,.08f,.22f,.10f));','if(encounter==Encounter.KKung&&!lightsOn){var b=shop.walkableArea.bounds;Fill(new Rect(b.min.x*100,-b.max.y*100,b.size.x*100,b.size.y*100),new Color(.13f,.08f,.22f,.10f));}');p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/ShiftUpgrade.cs');s=p.read_text(encoding='utf-8-sig').replace('        void DrawCashier()', '''        void DrawEncounterStatus()
        {
            if(!selected||encounter==Encounter.Store)return;
            Panel(new Rect(1210,200,340,100),resolved?mint:surface,true);
            Color text=resolved?ink:paper;
            string title=resolved?"All clear":scenarioTitles[(int)encounter];
            Text(new Rect(1226,212,308,25),title,20,text,FontStyle.Bold);
            string status=resolved?"The store is safe.":encounter==Encounter.Bo?boRemaining.ToString("0.0")+"s remaining":encounter==Encounter.Goblins?(rayCalled?"Uncle Ray: "+rayRemaining.ToString("0.0")+"s":"Call Uncle Ray at the wall phone"):encounter==Encounter.KKung?"Find the fluorescent light switch":nailsHit+" / "+tuning.nailHits+" rear hits";
            Text(new Rect(1226,252,308,40),status,21,!resolved&&encounter==Encounter.Bo&&boRemaining<5?red:text,FontStyle.Bold);
        }
        void DrawCashier()''');p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/StoreInterface.cs');s=p.read_text(encoding='utf-8-sig').replace('            DrawContextHint();','            DrawEncounterStatus();DrawContextHint();');p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/GameplayVerification.cs');p.write_text('''namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        bool testing, verificationRunning;
        public void BeginVerification(string resultPath){BeginUpgradeVerification(resultPath);}
    }
}
''',encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/UpgradeVerification.cs');s=p.read_text(encoding='utf-8-sig');s=s.replace('            foreach(var actor in shop.GetComponentsInChildren<CharacterSpriteAnimator>(true))Debug.Log("Smooth actor "+actor.name+" size="+actor.artwork.bounds.size+" scale="+actor.artwork.transform.lossyScale);\n','');p.write_text(s,encoding='utf-8')
