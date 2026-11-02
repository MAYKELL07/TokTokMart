using System.Collections;
using System.IO;
using UnityEngine;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        public void BeginCloseCameraPreview(string directory){StartCoroutine(CloseCameraPreview(directory));}
        IEnumerator CloseCameraPreview(string directory)
        {
            Directory.CreateDirectory(directory);testing=true;verificationRunning=true;settingsOpen=false;autoPaused=false;
            BeginCampaignLevel(1,false);noticeUntil=0;player=playerStart;cameraInitialized=false;
            yield return new WaitForSecondsRealtime(.5f);
            var camera=shop.worldCamera;var initial=camera.transform.position;
            bool close=camera.orthographicSize<=3;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"close-follow.png"));
            player+=Vector2.right*250;
            yield return new WaitForSecondsRealtime(.6f);
            bool follows=camera.transform.position.x>initial.x+1.8f;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"moved-right.png"));
            player=supply;
            yield return new WaitForSecondsRealtime(.8f);
            bool storage=roomPresentation.InStorage&&camera.orthographicSize<=3&&camera.transform.position.y>0;
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"storage-follow.png"));
            File.WriteAllText(Path.Combine(directory,"results.txt"),(close?"PASS":"FAIL")+" Close camera zoom\n"+(follows?"PASS":"FAIL")+" Camera follows the character\n"+(storage?"PASS":"FAIL")+" Close follow continues into storage\n");
            testing=false;verificationRunning=false;ReturnToMainMenu();
        }
    }
}
