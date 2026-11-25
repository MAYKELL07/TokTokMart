using UnityEngine;

namespace TokTokMart
{
    public static class DeviceVibration
    {
        const string SaveKey = "TokMart.Vibration";
        static float nextPulse;
        static bool unavailable;
        public static bool Enabled { get => PlayerPrefs.GetInt(SaveKey, 1) == 1; set { PlayerPrefs.SetInt(SaveKey, value ? 1 : 0); PlayerPrefs.Save(); } }
        public static void Pulse(int milliseconds = 15)
        {
            if (!Enabled || unavailable || !Application.isFocused || Time.unscaledTime < nextPulse) return;
            nextPulse = Time.unscaledTime + .08f;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))
                using(var vibrator=activity.Call<AndroidJavaObject>("getSystemService","vibrator"))
                {
                    if(vibrator==null||!vibrator.Call<bool>("hasVibrator")){unavailable=true;return;}
                    using(var effectClass=new AndroidJavaClass("android.os.VibrationEffect"))
                    using(var effect=effectClass.CallStatic<AndroidJavaObject>("createOneShot",(long)Mathf.Clamp(milliseconds,10,100),-1))
                        vibrator.Call("vibrate",effect);
                }
            }
            catch(AndroidJavaException){unavailable=true;}
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
    }
}
