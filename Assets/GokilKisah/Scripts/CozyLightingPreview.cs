using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        public void BeginCozyLightingPreview(string directory){StartCoroutine(CozyLightingPreview(directory));}
        IEnumerator CozyLightingPreview(string directory)
        {
            Directory.CreateDirectory(directory);testing=true;verificationRunning=true;settingsOpen=false;autoPaused=false;
            ResetRun(Encounter.Store);selected=true;mainMenu=false;player=playerStart;cameraInitialized=false;
            var lighting=FindFirstObjectByType<CozyShopLighting>();var checks=new List<string>();
            yield return new WaitForSecondsRealtime(1);
            checks.Add((shop.worldCamera.GetUniversalAdditionalCameraData().renderPostProcessing?"PASS":"FAIL")+" World camera post processing enabled");
            var volume=lighting.GetComponent<Volume>();
            checks.Add((volume&&volume.sharedProfile.TryGet<Vignette>(out var vignette)&&vignette.intensity.value>0?"PASS":"FAIL")+" Subtle vignette configured");
            checks.Add((lighting.shopLights.Length==7&&lighting.ambient.intensity>.75f?"PASS":"FAIL")+" Warm task lighting and ambient fill active");
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"cozy-gameplay.png"));
            player=ShopLayout.ToGame(new Vector3(17.25f,-6.1f,0));cameraInitialized=false;
            yield return new WaitForSecondsRealtime(.6f);
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"checkout-lighting.png"));
            player=ShopLayout.ToGame(shop.storageArea.bounds.center);cameraInitialized=false;
            yield return new WaitForSecondsRealtime(1);
            checks.Add((roomPresentation.InStorage&&roomPresentation.MainOpacity>.9f?"PASS":"FAIL")+" Room concealment remains intact");
            yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(directory,"storage-lighting.png"));
            ResetRun(Encounter.KKung);selected=true;mainMenu=false;player=playerStart;cameraInitialized=false;
            yield return new WaitForSecondsRealtime(1);
            checks.Add((lighting.ambient.intensity<.55f&&lighting.shopLights[0].intensity<.04f?"PASS":"FAIL")+" Lights-out encounter dims the shop");
            lightsOn=true;
            yield return new WaitForSecondsRealtime(1);
            checks.Add((lighting.ambient.intensity>.75f&&lighting.shopLights[0].intensity>.25f?"PASS":"FAIL")+" Breaker restores warm shop lighting");
            var samples=new List<float>();
            ResetRun(Encounter.Store);selected=true;mainMenu=false;
            for(int i=0;i<100;i++){yield return null;if(i>=30)samples.Add(Time.unscaledDeltaTime*1000);}
            samples.Sort();float sum=0;foreach(float ms in samples)sum+=ms;
            checks.Add("Mean frame: "+(sum/samples.Count).ToString("F2")+" ms; p95: "+samples[(int)(samples.Count*.95f)].ToString("F2")+" ms");
            File.WriteAllLines(Path.Combine(directory,"lighting-checks.txt"),checks);
            testing=false;verificationRunning=false;ReturnToMainMenu();
        }
    }
}
