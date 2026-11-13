using System;
using System.Collections;
using UnityEngine;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        [Header("Editable Canvas UI")]
        [Tooltip("Scene instance of the editable prefab. UI artwork, layout and typography live here.")]
        public TokMartCanvas editableUI;
        [Tooltip("Emergency visual fallback. Leave off to use the editable Canvas.")]
        public bool useOriginalImgui;
        int canvasPreviewLevel;
        Vector2 nativeGroupOffset;
        bool NativeUiReady=>!useOriginalImgui&&editableUI&&editableUI.building&&!drawingWorldOverlay&&Event.current.type==EventType.Repaint;
        Matrix4x4 NativeMatrix=>GUI.matrix*Matrix4x4.Translate(nativeGroupOffset);
        void EnsureEditableCanvas()
        {
            if(editableUI){editableUI.GetComponent<Canvas>().enabled=!useOriginalImgui;return;}
            editableUI=FindFirstObjectByType<TokMartCanvas>();
#if UNITY_EDITOR
            if(!editableUI&&testing)editableUI=TokMartCanvas.Create();
#endif
            if(!editableUI)throw new InvalidOperationException("Assign the authored Canvas to Store Game Controller > Editable UI. Runtime UI generation is disabled.");
        }
        sealed class EmptyUiScope:IDisposable{public void Dispose(){}}
        static readonly IDisposable emptyUiScope=new EmptyUiScope();
        IDisposable NativeUiScope(string id,string title)=>NativeUiReady?editableUI.Scope(id,title):emptyUiScope;
        void EditableLabel(Rect r,string value,GUIStyle style)
        {
            if(NativeUiReady)editableUI.Label(r,value,style,NativeMatrix);
            else if(drawingWorldOverlay||useOriginalImgui)GUI.Label(r,value,style);
        }
        void EditableTexture(Rect r,Texture texture,ScaleMode mode=ScaleMode.StretchToFill,bool alpha=true)
        {
            if(NativeUiReady)editableUI.Texture(r,texture,new Rect(0,0,1,1),GUI.color,NativeMatrix,mode);
            else if(drawingWorldOverlay||useOriginalImgui)GUI.DrawTexture(r,texture,mode,alpha);
        }
        void EditableTextureUV(Rect r,Texture texture,Rect uv,bool alpha=true)
        {
            if(NativeUiReady)editableUI.Texture(r,texture,uv,GUI.color,NativeMatrix);
            else if(drawingWorldOverlay||useOriginalImgui)GUI.DrawTextureWithTexCoords(r,texture,uv,alpha);
        }
        public bool CanvasBakeComplete {get;private set;}
        public void BakeEditableScreens(){StartCoroutine(BakeScreens());}
        IEnumerator BakeScreens()
        {
            // This temporary preview session never writes campaign progress or settings.
            EnsureEditableCanvas();editableUI.authoringBake=true;
            CanvasBakeComplete=false;testing=true;showPitchControls=false;settingsOpen=false;noticeUntil=0;
            mainMenu=true;yield return new WaitForEndOfFrame();yield return null;
            canvasPreviewLevel=2;yield return new WaitForEndOfFrame();yield return null;canvasPreviewLevel=0;
            levelMapOpen=true;mapSelection=1;mapPage=0;yield return new WaitForEndOfFrame();yield return null;scoreGuideOpen=true;yield return new WaitForEndOfFrame();yield return null;scoreGuideOpen=false;levelMapOpen=false;
            mainMenu=false;selected=false;yield return new WaitForEndOfFrame();yield return null;
            bool wasTouch=showTouchControls;
            foreach(bool touch in new[]{false,true})
            {
                showTouchControls=touch;selected=false;yield return new WaitForEndOfFrame();yield return null;
                BeginCampaignLevel(1,false);shiftBriefOpen=true;yield return new WaitForEndOfFrame();yield return null;
                shiftBriefOpen=false;yield return new WaitForEndOfFrame();yield return null;
                foreach(Encounter boss in new[]{Encounter.Goblins,Encounter.KKung,Encounter.Salmah,Encounter.Bo})
                {encounter=boss;yield return new WaitForEndOfFrame();yield return null;}
                encounter=Encounter.Store;
            }
            for(int tab=0;tab<6;tab++){settingsTab=tab;settingsOpen=true;yield return new WaitForEndOfFrame();yield return null;}
            settingsOpen=false;
            for(int task=0;task<3;task++)
            {skillType=task;PrepareTask(task);taskFeedback="Task feedback";yield return new WaitForEndOfFrame();yield return null;taskCombo=4;yield return new WaitForEndOfFrame();yield return null;taskCombo=0;}
            skillType=-1;levelResults=true;levelWon=true;yield return new WaitForEndOfFrame();yield return null;
            levelWon=false;yield return new WaitForEndOfFrame();yield return null;
            levelResults=false;gameOver=true;yield return new WaitForEndOfFrame();yield return null;
            gameOver=false;showTouchControls=wasTouch;mainMenu=true;selected=false;settingsOpen=false;taskFeedback="";
            yield return new WaitForEndOfFrame();yield return null;editableUI.authoringBake=false;CanvasBakeComplete=true;
        }
    }
}
