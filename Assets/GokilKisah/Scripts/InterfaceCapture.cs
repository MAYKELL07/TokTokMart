using System.Collections;
using System.IO;
using UnityEngine;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        public void BeginPresentationCapture(string folder){StartCoroutine(PresentationCapture(folder));}
        IEnumerator Shot(string folder,string name)
        {yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));yield return null;}
        IEnumerator PresentationCapture(string folder)
        {
            Directory.CreateDirectory(folder);testing=true;showPitchControls=false;showTouchControls=false;settingsOpen=false;Time.timeScale=1;
            ResetRun(Encounter.Store);selected=false;yield return Shot(folder,"01_Character_Select");
            Choose(false);player=new Vector2(637,295);yield return Shot(folder,"02_Behind_Shelf");
            player=new Vector2(637,352);yield return Shot(folder,"03_In_Front_Of_Shelf");
            player=new Vector2(760,710);settingsTab=0;SetSettingsOpen(true);yield return Shot(folder,"04_Audio_Settings");
            settingsTab=1;yield return Shot(folder,"05_Display_Settings");
            settingsTab=3;yield return Shot(folder,"06_Pitch_Settings");
            SetSettingsOpen(false);showPitchControls=true;yield return Shot(folder,"07_Pitch_Toggle_On");showPitchControls=false;
            carryingCrate=true;player=ShelfPoint(4);BeginSkill(0,4);skillClock=.7f;yield return Shot(folder,"08_Timing_Task");
            skillType=-1;ResetRun(Encounter.Store);float savedTextScale=textScale;textScale=1.15f;
            settingsTab=1;SetSettingsOpen(true);yield return Shot(folder,"09_Larger_Text_Settings");SetSettingsOpen(false);
            selected=false;yield return Shot(folder,"10_Larger_Text_Selection");textScale=savedTextScale;SaveSettings();testing=false;
            File.WriteAllText(Path.Combine(folder,"ui-complete.txt"),"Presentation states captured from the real game.");
        }
    }
}
