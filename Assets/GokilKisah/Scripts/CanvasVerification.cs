using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        public void BeginCanvasVerification(string folder){StartCoroutine(VerifyCanvas(folder));}
        IEnumerator VerifyCanvas(string folder)
        {
            Directory.CreateDirectory(folder);var report=new StringBuilder();testing=true;settingsOpen=false;mainMenu=true;selected=false;noticeUntil=0;bool savedTouch=showTouchControls;float savedVolume=masterVolume;
            System.Action<bool,string> check=(ok,label)=>report.AppendLine((ok?"PASS ":"FAIL ")+label);
            yield return null;yield return new WaitForEndOfFrame();
            check(editableUI&&editableUI.GetComponentsInChildren<TokMartCanvasElement>(true).Length>100,"Authored Canvas prefab is bound");
            int authoredCount=editableUI.GetComponentsInChildren<Transform>(true).Length;
            var authoredText=editableUI.GetComponentInChildren<Text>();
            var savedFont=authoredText.font;authoredText.font=null;
            yield return null;yield return new WaitForEndOfFrame();
            check(authoredText.font==null,"Binding pass preserves a manually assigned font");authoredText.font=savedFont;
            yield return CanvasPair(folder,"01-Menu");
            Button FindButton(string label)
            {
                foreach(var e in editableUI.GetComponentsInChildren<TokMartCanvasElement>())
                {
                    if(e.kind!="Group")continue;
                    var buttons=e.GetComponentsInChildren<Button>();if(buttons.Length!=1)continue;var button=buttons[0];
                    foreach(var text in e.GetComponentsInChildren<Text>())if(text.text==label)return button;
                }
                return null;
            }
            var settings=FindButton("SETTINGS");check(settings,"Native Settings button exists");if(settings)settings.onClick.Invoke();yield return null;yield return new WaitForEndOfFrame();yield return null;
            check(settingsOpen,"Native Settings click reaches existing game action");
            var slider=editableUI.GetComponentInChildren<Slider>();check(slider,"Native volume slider exists");if(slider)slider.onValueChanged.Invoke(.37f);yield return null;yield return new WaitForEndOfFrame();yield return null;
            check(Mathf.Abs(masterVolume-.37f)<.01f,"Native slider updates volume data");
            yield return CanvasPair(folder,"02-Settings");masterVolume=savedVolume;settingsOpen=false;
            yield return null;yield return new WaitForEndOfFrame();
            var open=FindButton(SavedLevel>1?"BACK TO SHIFT "+SavedLevel:"OPEN THE SHOP");check(open,"Native Open Shop button exists");if(open)open.onClick.Invoke();yield return null;yield return new WaitForEndOfFrame();yield return null;
            check(!mainMenu&&!selected,"Native Open Shop opens character selection");
            yield return CanvasPair(folder,"03-Selection");
            var sam=FindButton("PICK SAM");if(sam)sam.onClick.Invoke();yield return null;yield return new WaitForEndOfFrame();yield return null;check(selected&&!zack,"Native character selection keeps existing Choose action");
            BeginCampaignLevel(1,false);shiftBriefOpen=true;yield return CanvasPair(folder,"04-Briefing");
            shiftBriefOpen=false;showTouchControls=true;yield return CanvasPair(folder,"05-Mobile-HUD");
            var control=editableUI.ControlImage("InteractButton01");check(control,"Mobile control image is bound");
            if(control)
            {
                var group=control.transform.parent.parent.GetComponent<RectTransform>();var before=EditableControlRect("InteractButton01",new Rect());group.anchoredPosition+=new Vector2(-40,20);yield return null;
                var after=EditableControlRect("InteractButton01",new Rect());check(Mathf.Abs(after.x-before.x+40)<1&&Mathf.Abs(after.y-before.y+20)<1,"Moving native Interact group moves its touch hit area");group.anchoredPosition-=new Vector2(-40,20);
            }
            skillType=0;PrepareTask(0);yield return CanvasPair(folder,"06-Task");skillType=1;PrepareTask(1);yield return CanvasPair(folder,"07-Cashier");
            skillType=-1;levelResults=true;levelWon=true;yield return CanvasPair(folder,"08-Results");
            levelResults=false;mainMenu=true;selected=false;showTouchControls=savedTouch;useOriginalImgui=false;settingsOpen=false;masterVolume=savedVolume;ApplyAudio();testing=false;ResetRun(Encounter.Store);
            check(editableUI.GetComponentsInChildren<Transform>(true).Length==authoredCount,"Screen transitions do not generate UI objects");
            check(editableUI.MissingBindingCount==0,"All exercised controls have authored bindings");
            File.WriteAllText(Path.Combine(folder,"canvas-verification.txt"),report.ToString());
        }
        IEnumerator CanvasPair(string folder,string name)
        {
            useOriginalImgui=true;yield return null;yield return Shot(folder,name+"-Original");
            useOriginalImgui=false;yield return null;yield return Shot(folder,name+"-Canvas");
        }
    }
}
