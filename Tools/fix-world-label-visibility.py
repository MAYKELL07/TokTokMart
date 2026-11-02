from pathlib import Path
p=Path('Assets/GokilKisah/Scripts/StoreInterface.cs');s=p.read_text(encoding='utf-8-sig');start=s.index('            Matrix4x4 canvas=GUI.matrix;');end=s.index('            DrawEncounterStatus();',start);s=s[:start]+'''            Matrix4x4 canvas=GUI.matrix;
            GUI.matrix=SceneOverlayMatrix();drawingWorldOverlay=true;
            DrawStore();drawingWorldOverlay=false;GUI.matrix=canvas;
'''+s[end:]
s=s.replace('void Panel(Rect r,Color c,bool shadow=false)\n        {','void Panel(Rect r,Color c,bool shadow=false)\n        {\n            if(drawingWorldOverlay&&!WorldOverlayVisible(r))return;');p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/SceneBindings.cs');s=p.read_text(encoding='utf-8-sig').replace('        Matrix4x4 SceneOverlayMatrix()', '''        bool drawingWorldOverlay;
        Rect OverlayViewport(){var p=shop.worldCamera.pixelRect;return new Rect(p.x,Screen.height-p.yMax,p.width,p.height);}
        bool WorldOverlayVisible(Rect rect)
        {
            var viewport=OverlayViewport();var min=GUI.matrix.MultiplyPoint3x4(new Vector3(rect.xMin,rect.yMin));var max=GUI.matrix.MultiplyPoint3x4(new Vector3(rect.xMax,rect.yMax));
            return min.x>=viewport.xMin&&min.y>=viewport.yMin&&max.x<=viewport.xMax&&max.y<=viewport.yMax;
        }
        Rect ClipWorldOverlay(Rect rect)
        {
            var v=OverlayViewport();var inverse=GUI.matrix.inverse;var min=inverse.MultiplyPoint3x4(new Vector3(v.xMin,v.yMin));var max=inverse.MultiplyPoint3x4(new Vector3(v.xMax,v.yMax));
            return Rect.MinMaxRect(Mathf.Max(rect.xMin,min.x),Mathf.Max(rect.yMin,min.y),Mathf.Min(rect.xMax,max.x),Mathf.Min(rect.yMax,max.y));
        }
        Matrix4x4 SceneOverlayMatrix()''');p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/StoreGameController.cs');s=p.read_text(encoding='utf-8-sig').replace('void Fill(Rect r,Color c){GUI.color=c;', 'void Fill(Rect r,Color c){if(drawingWorldOverlay){r=ClipWorldOverlay(r);if(r.width<=0||r.height<=0)return;}GUI.color=c;').replace('{GUI.Label(r,value,new GUIStyle(GUI.skin.label)', '{if(drawingWorldOverlay&&!WorldOverlayVisible(r))return;GUI.Label(r,value,new GUIStyle(GUI.skin.label)');p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/EncounterPresentation.cs');s=p.read_text(encoding='utf-8-sig').replace('"Stockroom",15,new Color(.25f,.34f,.31f)', '"Take stock [E]",15,shop.storageArea?paper:new Color(.25f,.34f,.31f)');p.write_text(s,encoding='utf-8')
p=Path('Assets/GokilKisah/Scripts/UpgradeVerification.cs');s=p.read_text(encoding='utf-8-sig').replace('player=supply+new Vector2(40,65)', 'player=supply+new Vector2(30,50)');p.write_text(s,encoding='utf-8')
