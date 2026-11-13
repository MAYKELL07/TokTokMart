using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace GokilKisah
{
    // Authored RectTransforms are never replaced by the binding pass. Only changing game data is applied.
    public sealed class TokMartCanvas : MonoBehaviour
    {
        public RectTransform designRoot;
        readonly Dictionary<string,RawImage> controlImages=new Dictionary<string,RawImage>();
        readonly Dictionary<string,TokMartCanvasElement> elements=new Dictionary<string,TokMartCanvasElement>();
        readonly HashSet<TokMartCanvasElement> seen=new HashSet<TokMartCanvasElement>();
        readonly Dictionary<string,int> counts=new Dictionary<string,int>();
        readonly Stack<Transform> parents=new Stack<Transform>();
        readonly HashSet<string> clicks=new HashSet<string>();
        readonly Dictionary<string,float> sliders=new Dictionary<string,float>();
        public Matrix4x4 sourceMatrix;
        public bool building;
        [System.NonSerialized] public bool authoringBake;
        readonly Dictionary<Texture2D,Sprite> panelSprites=new Dictionary<Texture2D,Sprite>();
        public static Color GuiColor(Color c){c.a=1-Mathf.Pow(1-Mathf.Clamp01(c.a),2.2f);return c;}
        void Awake(){GetComponent<Canvas>().vertexColorAlwaysGammaSpace=true;Index();EnsureEvents();}
        void Index(){elements.Clear();foreach(var e in GetComponentsInChildren<TokMartCanvasElement>(true)){elements[e.key]=e;e.owner=this;e.Wire();var raw=e.GetComponent<RawImage>();if(raw&&raw.texture&&e.key.StartsWith("DrawMobileControls_"))controlImages[raw.texture.name]=raw;}}
        public static TokMartCanvas Create()
        {
            var go=new GameObject("TokTokMart Editable UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),typeof(TokMartCanvas));
            var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;canvas.vertexColorAlwaysGammaSpace=true;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            var root=new GameObject("Design (1600 x 900)",typeof(RectTransform)).GetComponent<RectTransform>();root.SetParent(go.transform,false);root.anchorMin=root.anchorMax=new Vector2(.5f,.5f);root.sizeDelta=new Vector2(1600,900);
            var result=go.GetComponent<TokMartCanvas>();result.designRoot=root;result.EnsureEvents();return result;
        }
        void EnsureEvents(){if(!EventSystem.current){var go=new GameObject("UI EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();}}
        public void Begin(Matrix4x4 matrix){if(elements.Count==0)Index();sourceMatrix=matrix;seen.Clear();counts.Clear();parents.Clear();parents.Push(designRoot);building=true;}
        public void End(){foreach(var e in elements.Values)if(e)e.gameObject.SetActive(seen.Contains(e));building=false;}
        TokMartCanvasElement Element(string id,string title,string kind)
        {
            string parent=parents.Peek()==designRoot?"":parents.Peek().GetComponent<TokMartCanvasElement>().key+"/";
            string stem=parent+id;counts.TryGetValue(stem,out int number);counts[stem]=number+1;string key=stem+"_"+number;
            if(!elements.TryGetValue(key,out var e)||!e)
            {
                var go=new GameObject(title,typeof(RectTransform),typeof(TokMartCanvasElement));go.transform.SetParent(parents.Peek(),false);e=go.GetComponent<TokMartCanvasElement>();e.key=key;e.owner=this;e.kind=kind;
                var r=e.GetComponent<RectTransform>();r.anchorMin=new Vector2(0,1);r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.sizeDelta=new Vector2(1600,900);
                elements[key]=e;
            }
            e.gameObject.SetActive(true);e.transform.SetAsLastSibling();seen.Add(e);return e;
        }
        public IDisposable Scope(string id,string title)
        {
            bool root=id.StartsWith("DrawSettings");if(root)parents.Push(designRoot);
            var e=Element(id,title,"Group");parents.Push(e.transform);return new ScopeLease(this,root);
        }
        sealed class ScopeLease:IDisposable{TokMartCanvas owner;bool root;public ScopeLease(TokMartCanvas o,bool r){owner=o;root=r;}public void Dispose(){if(owner!=null){owner.parents.Pop();if(root)owner.parents.Pop();owner=null;}}}
        public Rect CanvasRect(Rect rect,Matrix4x4 matrix)
        {
            var m=sourceMatrix.inverse*matrix;var a=m.MultiplyPoint3x4(rect.min);var b=m.MultiplyPoint3x4(rect.max);return Rect.MinMaxRect(a.x,a.y,b.x,b.y);
        }
        TokMartCanvasElement Visual(string id,string title,string kind,Rect rect,Matrix4x4 matrix)
        {
            var e=Element(id,title,kind);e.Layout(CanvasRect(rect,matrix));return e;
        }
        public void Label(Rect rect,string value,GUIStyle style,Matrix4x4 matrix)
        {
            var e=Visual("Label","Text: "+(value??"").Replace('\n',' '),"Text",rect,matrix);
            var t=e.GetComponent<Text>();if(!t){t=e.gameObject.AddComponent<Text>();t.raycastTarget=false;t.font=style.font;t.fontSize=style.fontSize;t.alignment=style.alignment;t.horizontalOverflow=style.wordWrap?HorizontalWrapMode.Wrap:HorizontalWrapMode.Overflow;t.verticalOverflow=VerticalWrapMode.Overflow;t.supportRichText=false;}
            if(t.font!=style.font)t.font=style.font;
            int scaledSize=Mathf.RoundToInt(style.fontSize*(sourceMatrix.inverse*matrix).GetColumn(0).magnitude);
            e.SetText(value,style.normal.textColor,scaledSize);
        }
        public void Texture(Rect rect,Texture texture,Rect uv,Color tint,Matrix4x4 matrix,ScaleMode mode=ScaleMode.StretchToFill)
        {
            if(!texture)return;
            if(mode==ScaleMode.ScaleToFit){float ratio=(float)texture.width/texture.height;if(rect.width/rect.height>ratio){float w=rect.height*ratio;rect.x+=(rect.width-w)/2;rect.width=w;}else{float h=rect.width/ratio;rect.y+=(rect.height-h)/2;rect.height=h;}}
            else if(mode==ScaleMode.ScaleAndCrop){float ratio=(float)texture.width/texture.height;float area=rect.width/rect.height;if(area>ratio){float h=ratio/area;uv=new Rect(0,(1-h)/2,1,h);}else{float w=area/ratio;uv=new Rect((1-w)/2,0,w,1);}}
            var e=Visual("Image","Image: "+texture.name,"Image",rect,matrix);var image=e.GetComponent<RawImage>();if(!image){image=e.gameObject.AddComponent<RawImage>();image.raycastTarget=false;image.texture=texture;image.uvRect=uv;image.color=GuiColor(tint);}
            if(authoringBake){image.texture=texture;image.uvRect=uv;e.originalTexture=texture;e.sourceTextureName=texture.name;}
            e.SetImage(texture,uv,tint);
            if(e.key.StartsWith("DrawMobileControls_"))controlImages[texture.name]=image;
        }
        public void Panel(Rect rect,Color color,Texture2D texture,Matrix4x4 matrix)
        {
            var e=Visual("Panel","Rounded panel","Panel",rect,matrix);var image=e.GetComponent<Image>();
            if(!image){image=e.gameObject.AddComponent<Image>();image.raycastTarget=false;image.sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(10,10,10,10));image.type=Image.Type.Sliced;image.color=GuiColor(color);}
            if(authoringBake)
            {
                if(!panelSprites.TryGetValue(texture,out var sprite))panelSprites[texture]=sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(10,10,10,10));
                image.sprite=sprite;
            }
            e.SetPanel(color);
        }
        public bool Button(Rect rect,bool enabled,Matrix4x4 matrix)
        {
            var e=Visual("Button","Button hit area","Button",rect,matrix);var b=e.GetComponent<Button>();if(!b){var image=e.gameObject.AddComponent<Image>();image.color=Color.clear;b=e.gameObject.AddComponent<Button>();b.transition=Selectable.Transition.None;e.Wire();}b.interactable=enabled;
            return enabled&&clicks.Remove(e.key);
        }
        public float Slider(Rect rect,float value,bool enabled,Matrix4x4 matrix)
        {
            var e=Visual("Slider","Slider hit area","Slider",rect,matrix);var s=e.GetComponent<Slider>();if(!s){var image=e.gameObject.AddComponent<Image>();image.color=Color.clear;s=e.gameObject.AddComponent<Slider>();s.transition=Selectable.Transition.None;s.minValue=0;s.maxValue=1;var handle=new GameObject("Handle area",typeof(RectTransform));handle.transform.SetParent(e.transform,false);var h=handle.GetComponent<RectTransform>();h.anchorMin=Vector2.zero;h.anchorMax=Vector2.one;h.offsetMin=h.offsetMax=Vector2.zero;s.handleRect=h;e.Wire();}s.interactable=enabled;
            if(sliders.TryGetValue(e.key,out var changed)){value=changed;sliders.Remove(e.key);}s.SetValueWithoutNotify(value);return value;
        }
        public RawImage ControlImage(string name)=>controlImages.TryGetValue(name,out var value)?value:null;
        public void Click(string key){clicks.Add(key);}
        public void Slide(string key,float value){sliders[key]=value;}
    }
}
