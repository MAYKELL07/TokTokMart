using System.Collections;
using System.IO;
using UnityEngine;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        public void BeginSamArtPreview(string directory){StartCoroutine(SamArtPreview(directory));}
        IEnumerator SamArtPreview(string directory)
        {
            Directory.CreateDirectory(directory);testing=true;verificationRunning=true;
            ResetRun(Encounter.Store);selected=true;mainMenu=false;settingsOpen=false;autoPaused=false;zack=false;
            player=playerStart;cameraInitialized=false;
            yield return new WaitForSecondsRealtime(.5f);
            var still=SmoothArt.Load("Sam_3_0");bool staticSam=true;
            for(int d=0;d<4;d++)for(int f=0;f<3;f++)staticSam&=SmoothArt.Load("Sam_"+d+"_"+f)==still;
            bool zackAnimated=SmoothArt.Load("Zack_3_0")!=SmoothArt.Load("Zack_3_1");
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"sam-gameplay.png"));
            zack=true;
            yield return new WaitForSecondsRealtime(.3f);
            bool swapped=shop.playerCharacter.artwork.sprite==SmoothArt.Load("Zack_3_0")&&shop.partnerCharacter.artwork.sprite==still;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"zack-unchanged.png"));
            File.WriteAllText(Path.Combine(directory,"preview.txt"),(staticSam?"PASS":"FAIL")+" Sam uses the supplied still for all directions and frames\n"+(zackAnimated?"PASS":"FAIL")+" Zack keeps separate animation frames\n"+(swapped?"PASS":"FAIL")+" Selecting Zack gives the partner Sam's supplied still\n");
            zack=false;testing=false;verificationRunning=false;ReturnToMainMenu();
        }
    }
}
