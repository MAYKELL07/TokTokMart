using UnityEngine;
using UnityEngine.InputSystem;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        [Header("Mobile UI sizing")]
        [Tooltip("HUD size on phones and when touch controls are enabled in the Editor.")]
        [Range(1f,1.45f)] public float mobileHudScale=1.25f;
        [Tooltip("Default touch size. Players can override this in Settings > Controls.")]
        [Range(.7f,1.4f)] public float defaultTouchScale=1.25f;
        bool MobileInterface => UnityEngine.Device.Application.isMobilePlatform || showTouchControls;
        Matrix4x4 interfaceCanvasMatrix;
        Matrix4x4 HudMatrix(Matrix4x4 canvas, Vector2 anchor)
        {
            float size=MobileInterface?Mathf.Clamp(mobileHudScale,1f,1.45f):1f;
            return canvas*Matrix4x4.Translate(anchor)*Matrix4x4.Scale(new Vector3(size,size,1))*Matrix4x4.Translate(-anchor);
        }
        float leftControlScale=1,rightControlScale=1,leftControlPadding=42,rightControlPadding=42;
        float leftControlLift=38,rightControlLift=38;
        int stickFinger=-1,sprintFinger=-1;
        Vector2 stickVisual;
        const string ControlPref="TokMart.Controls.";
        void LoadControlSettings()
        {
            leftControlScale=Mathf.Clamp(PlayerPrefs.GetFloat(ControlPref+"LeftScale",defaultTouchScale),.7f,1.4f);
            rightControlScale=Mathf.Clamp(PlayerPrefs.GetFloat(ControlPref+"RightScale",defaultTouchScale),.7f,1.4f);
            leftControlPadding=Mathf.Clamp(PlayerPrefs.GetFloat(ControlPref+"LeftPadding",42),0,260);
            rightControlPadding=Mathf.Clamp(PlayerPrefs.GetFloat(ControlPref+"RightPadding",42),0,260);
            leftControlLift=Mathf.Clamp(PlayerPrefs.GetFloat(ControlPref+"LeftLift",38),0,220);
            rightControlLift=Mathf.Clamp(PlayerPrefs.GetFloat(ControlPref+"RightLift",38),0,220);
        }
        void SaveControlSettings()
        {
            PlayerPrefs.SetFloat(ControlPref+"LeftScale",leftControlScale);PlayerPrefs.SetFloat(ControlPref+"RightScale",rightControlScale);
            PlayerPrefs.SetFloat(ControlPref+"LeftPadding",leftControlPadding);PlayerPrefs.SetFloat(ControlPref+"RightPadding",rightControlPadding);
            PlayerPrefs.SetFloat(ControlPref+"LeftLift",leftControlLift);PlayerPrefs.SetFloat(ControlPref+"RightLift",rightControlLift);
        }
        void ReleaseMobileControls()
        {stickFinger=sprintFinger=-1;touchHeld=false;touchSprint=false;touchMove=stickVisual=Vector2.zero;}
        Vector2 ScreenToCanvas(Vector2 point)
        {
            float scale=Mathf.Min(Screen.width/1600f,Screen.height/900f);
            return new Vector2((point.x-(Screen.width-1600*scale)*.5f)/scale,
                (Screen.height-point.y-(Screen.height-900*scale)*.5f)/scale);
        }
        Rect ControlSafeArea()
        {
            var safe=Screen.safeArea;var a=ScreenToCanvas(new Vector2(safe.xMin,safe.yMax));var b=ScreenToCanvas(new Vector2(safe.xMax,safe.yMin));
            return Rect.MinMaxRect(Mathf.Clamp(a.x,0,1600),Mathf.Clamp(a.y,0,900),Mathf.Clamp(b.x,0,1600),Mathf.Clamp(b.y,0,900));
        }
        void ControlRects(out Rect stick,out Rect action,out Rect sprint)
        {
            var safe=ControlSafeArea();float size=190*leftControlScale,button=128*rightControlScale;
            stick=new Rect(safe.xMin+leftControlPadding,safe.yMax-leftControlLift-size,size,size);
            action=new Rect(safe.xMax-rightControlPadding-button,safe.yMax-rightControlLift-button,button,button);
            sprint=new Rect(action.x+button*.18f,action.y-button*.97f,button*.72f,button*.72f);
            stick.x=Mathf.Clamp(stick.x,safe.xMin,Mathf.Max(safe.xMin,safe.xMax-stick.width));
            stick.y=Mathf.Clamp(stick.y,safe.yMin,Mathf.Max(safe.yMin,safe.yMax-stick.height));
            action.y=Mathf.Clamp(action.y,safe.yMin,Mathf.Max(safe.yMin,safe.yMax-action.height));
        }
        Rect EditableControlRect(string texture,Rect fallback)
        {
            var image=editableUI.ControlImage(texture);
            if(image&&image.gameObject.activeInHierarchy)
            {
                var corners=new Vector3[4];image.rectTransform.GetWorldCorners(corners);
                var a=ScreenToCanvas(corners[1]);var b=ScreenToCanvas(corners[3]);return Rect.MinMaxRect(a.x,a.y,b.x,b.y);
            }
            return fallback;
        }
        void MobilePointer(int id,Vector2 point,bool began,bool held)
        {
            ControlRects(out var stick,out var action,out var sprint);
            if(editableUI&&!useOriginalImgui){stick=EditableControlRect("JoystickBase",stick);action=EditableControlRect("InteractButton01",action);sprint=EditableControlRect("SprintButton01",sprint);}
            if(!held){if(stickFinger==id){stickFinger=-1;touchMove=stickVisual=Vector2.zero;}if(sprintFinger==id){sprintFinger=-1;touchSprint=false;}return;}
            if(began)
            {
                if(stick.Contains(point)&&stickFinger<0)stickFinger=id;
                else if(action.Contains(point))ActionInput();
                else if(sprint.Contains(point)&&sprintFinger<0){sprintFinger=id;touchSprint=true;}
            }
            if(stickFinger==id)
            {
                stickVisual=Vector2.ClampMagnitude((point-stick.center)/(stick.width*.34f),1);
                float magnitude=stickVisual.magnitude;
                touchMove=magnitude<.12f?Vector2.zero:stickVisual.normalized*Mathf.InverseLerp(.12f,1,magnitude);
            }
        }
        void UpdateMobileControls()
        {
            if(!showTouchControls||!selected||mainMenu||settingsOpen||autoPaused||gameOver||levelResults||skillType>=0||replay||shiftBriefOpen||scoreGuideOpen)
            {ReleaseMobileControls();return;}
            bool anyTouch=false;
            if(Touchscreen.current!=null)foreach(var touch in Touchscreen.current.touches)
            {
                if(!touch.press.isPressed&&!touch.press.wasReleasedThisFrame)continue;
                anyTouch=true;MobilePointer(touch.touchId.ReadValue(),ScreenToCanvas(touch.position.ReadValue()),touch.press.wasPressedThisFrame,touch.press.isPressed);
            }
            // Mouse support lets the same controls be previewed and checked in the editor.
            if(!anyTouch&&Mouse.current!=null)
                MobilePointer(-2,ScreenToCanvas(Mouse.current.position.ReadValue()),Mouse.current.leftButton.wasPressedThisFrame,Mouse.current.leftButton.isPressed);
        }
        void DrawMobileControls()
        {using(NativeUiScope("DrawMobileControls","DrawMobileControls")){
            ControlRects(out var stick,out var action,out var sprint);
            using(NativeUiScope("Joystick","Joystick (move this group)"))
            {
            ArtImage(stick,"JoystickBase", uiKey:"MobileControls_5937");
            var knob=stick.center+stickVisual*stick.width*.27f;float k=stick.width*.38f;
            ArtImage(new Rect(knob.x-k/2,knob.y-k/2,k,k),"JoystickKnob", uiKey:"MobileControls_6070");
            }
            using(NativeUiScope("Sprint","Sprint (move this group)"))
            {
            GUI.color=touchSprint?arcadeGold:Color.white;ArtImage(sprint,"SprintButton01", uiKey:"MobileControls_6189");GUI.color=Color.white;
            ArcadeText(new Rect(sprint.x-10,sprint.yMax+4,sprint.width+20,24),"HOLD SPRINT",14,arcadeCream,TextAnchor.MiddleCenter,true, uiKey:"MobileControls_6307");
            }
            using(NativeUiScope("Interact","Interact (move this group)"))
            {
            ArtImage(action,"InteractButton01", uiKey:"MobileControls_6258");
            ArcadeText(new Rect(action.x-10,action.yMax+4,action.width+20,24),"INTERACT",15,arcadeCream,TextAnchor.MiddleCenter,true, uiKey:"MobileControls_6445");
            }
        }}
        void ControlSlider(float y,string label,ref float value,float min,float max,bool percent=false,string uiKey="")
        {using(NativeUiScope(uiKey,"ControlSlider")){
            Text(new Rect(641,y,250,32),label,21,ink,FontStyle.Bold, uiKey:"MobileControls_6704");
            value=Mathf.Lerp(min,max,Slider(new Rect(902,y,300,36),Mathf.InverseLerp(min,max,value), uiKey:"MobileControls_6799"));
            Text(new Rect(1220,y,110,32),percent?Mathf.RoundToInt(value*100)+"%":Mathf.RoundToInt(value).ToString(),20,ink, uiKey:"MobileControls_6877");
        }}
        void DrawControlSettings()
        {
            Text(new Rect(641,234,650,35),"Mobile controls",29,ink,FontStyle.Bold, uiKey:"MobileControls_7057");
            ControlSlider(282,"Joystick size",ref leftControlScale,.7f,1.4f,true, uiKey:"MobileControls_7141");
            ControlSlider(322,"Action buttons size",ref rightControlScale,.7f,1.4f,true, uiKey:"MobileControls_7224");
            ControlSlider(362,"Left padding",ref leftControlPadding,0,260, uiKey:"MobileControls_7314");
            ControlSlider(402,"Right padding",ref rightControlPadding,0,260, uiKey:"MobileControls_7390");
            ControlSlider(442,"Joystick height",ref leftControlLift,0,220, uiKey:"MobileControls_7468");
            ControlSlider(482,"Action buttons height",ref rightControlLift,0,220, uiKey:"MobileControls_7544");
            var preview=new Rect(641,535,690,169);Panel(preview,arcadeTeal, uiKey:"MobileControls_7665");
            var matrix=GUI.matrix;GUI.BeginGroup(preview);nativeGroupOffset=preview.position;
            ControlRects(out var j,out var a,out var s);
            float previewScale=preview.height/900;float previewX=(preview.width-1600*previewScale)/2;
            Rect Fit(Rect r)=>new Rect(previewX+r.x*previewScale,r.y*previewScale,r.width*previewScale,r.height*previewScale);
            ArtImage(Fit(j),"JoystickBase", uiKey:"MobileControls_8049");var k=Fit(j);k=new Rect(k.center.x-k.width*.19f,k.center.y-k.height*.19f,k.width*.38f,k.height*.38f);ArtImage(k,"JoystickKnob", uiKey:"MobileControls_8182");
            ArtImage(Fit(a),"InteractButton01", uiKey:"MobileControls_8222");ArtImage(Fit(s),"SprintButton01", uiKey:"MobileControls_8258");
            GUI.EndGroup();nativeGroupOffset=Vector2.zero;GUI.matrix=matrix;
            Text(new Rect(652,542,175,48),"LIVE POSITION PREVIEW",14,arcadeCream, uiKey:"MobileControls_8351");
            if(Button(new Rect(641,717,282,44),"Reset control layout",gold,19, uiKey:"MobileControls_8437"))
            {leftControlScale=rightControlScale=defaultTouchScale;leftControlPadding=rightControlPadding=42;leftControlLift=rightControlLift=38;}
            Text(new Rect(942,716,390,46),"Saved when you tap Back.",18,ink, uiKey:"MobileControls_8660");
        }
    }
}
