using UnityEngine;
using UnityEngine.UI;
namespace GokilKisah
{
    [DisallowMultipleComponent]
    public sealed class TokMartCanvasElement:MonoBehaviour
    {
        [HideInInspector] public string key,kind;
        [HideInInspector] public Rect originalRect;
        [HideInInspector] public bool hasLayout,hasData;
        [HideInInspector] public string originalText;
        [HideInInspector] public Color originalColor;
        [HideInInspector] public int originalFontSize;
        [HideInInspector] public Texture originalTexture;
        [HideInInspector] public string sourceTextureName;
        [HideInInspector] public Rect originalUV;
        [Tooltip("Keep enabled for counters and other changing game values. Disable to use only your authored text/color/image.")]
        public bool bindGameData=true;
        [Tooltip("Keep enabled for progress bars and moving task markers. Static layouts remain where you place them.")]
        public bool animateLayout=true;
        [System.NonSerialized] public TokMartCanvas owner;
        RectTransform rt;Vector2 authoredPosition,authoredSize;bool captured;
        public void Layout(Rect source)
        {
            if(!rt)rt=GetComponent<RectTransform>();
            if(!hasLayout){originalRect=source;hasLayout=true;rt.anchoredPosition=new Vector2(source.x,-source.y);rt.sizeDelta=source.size;}
            if(!captured){authoredPosition=rt.anchoredPosition;authoredSize=rt.sizeDelta;captured=true;}
            if(animateLayout){rt.anchoredPosition=authoredPosition+new Vector2(source.x-originalRect.x,-source.y+originalRect.y);rt.sizeDelta=authoredSize+source.size-originalRect.size;}
        }
        bool liveData;
        void BeginData(){if(liveData)return;liveData=true;lastText=originalText;lastColor=originalColor;lastSize=originalFontSize;lastTextureName=sourceTextureName;lastUV=originalUV;}
        public void SetText(string value,Color color,int size)
        {
            var t=GetComponent<Text>();if(!hasData){originalText=value;originalColor=color;originalFontSize=size;hasData=true;t.text=value;t.color=TokMartCanvas.GuiColor(color);t.fontSize=size;}
            BeginData();
            if(bindGameData){if(value!=originalText)t.text=value;else if(lastText!=null&&lastText!=originalText)t.text=originalText;if(color!=originalColor||lastColor!=originalColor)t.color=TokMartCanvas.GuiColor(color);if(size!=originalFontSize||lastSize!=originalFontSize)t.fontSize=size;}
            lastText=value;lastColor=color;lastSize=size;
        }
        string lastText,lastTextureName;Rect lastUV;Color lastColor;int lastSize;
        public void SetImage(Texture texture,Rect uv,Color color)
        {
            var image=GetComponent<RawImage>();if(!hasData){originalTexture=texture;sourceTextureName=texture?texture.name:"";originalUV=uv;originalColor=color;hasData=true;}
            BeginData();
            if(bindGameData){if(texture&&(texture.name!=sourceTextureName||lastTextureName!=sourceTextureName))image.texture=texture.name==sourceTextureName?originalTexture:texture;if(uv!=originalUV||lastUV!=originalUV)image.uvRect=uv;if(color!=originalColor||lastColor!=originalColor)image.color=TokMartCanvas.GuiColor(color);}lastColor=color;lastTextureName=texture?texture.name:"";lastUV=uv;
        }
        public void SetPanel(Color color)
        {
            var image=GetComponent<Image>();if(!hasData){originalColor=color;hasData=true;image.color=TokMartCanvas.GuiColor(color);}
            BeginData();
            if(bindGameData&&(color!=originalColor||lastColor!=originalColor))image.color=TokMartCanvas.GuiColor(color);lastColor=color;
        }
        public void ResetBindingPreview()
        {
            if(hasLayout){var r=GetComponent<RectTransform>();r.anchoredPosition=new Vector2(originalRect.x,-originalRect.y);r.sizeDelta=originalRect.size;}
            if(!hasData)return;
            var text=GetComponent<Text>();if(text){text.text=originalText;text.color=TokMartCanvas.GuiColor(originalColor);text.fontSize=originalFontSize;}
            var raw=GetComponent<RawImage>();if(raw){raw.texture=originalTexture;raw.uvRect=originalUV;raw.color=TokMartCanvas.GuiColor(originalColor);}
            var image=GetComponent<Image>();if(image&&kind=="Panel")image.color=TokMartCanvas.GuiColor(originalColor);
        }
        public void Wire()
        {
            var b=GetComponent<Button>();if(b){b.onClick.RemoveListener(Clicked);b.onClick.AddListener(Clicked);}
            var s=GetComponent<Slider>();if(s){s.onValueChanged.RemoveListener(Changed);s.onValueChanged.AddListener(Changed);}
        }
        void Clicked(){if(owner)owner.Click(key);}
        void Changed(float v){if(owner)owner.Slide(key,v);}
    }
}
