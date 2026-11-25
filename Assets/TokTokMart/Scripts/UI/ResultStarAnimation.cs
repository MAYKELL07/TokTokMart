using UnityEngine;
using UnityEngine.UI;

namespace TokTokMart
{
    public sealed class ResultStarAnimation : MonoBehaviour
    {
        public float delay;
        public RawImage star;
        public RectTransform[] sparks;
        float started;
        bool sounded;
        void OnEnable() { started = Time.unscaledTime; sounded = false; transform.localScale = Vector3.one * .01f; if (sparks != null) foreach (var spark in sparks) if (spark) spark.localScale = Vector3.zero; }
        void OnDisable() { transform.localScale = Vector3.one; transform.localRotation = Quaternion.identity; }
        void Update()
        {
            float age = Time.unscaledTime - started - delay;
            bool earned = star && star.color.a > .8f;
            if (!earned) { transform.localScale = Vector3.one; transform.localRotation = Quaternion.identity; if (sparks != null) foreach (var spark in sparks) if (spark) spark.localScale = Vector3.zero; return; }
            float t = Mathf.Clamp01(age / .48f);
            float overshoot = t - 1;
            float scale = age < 0 ? .01f : 1 + 3.70158f * overshoot * overshoot * overshoot + 2.70158f * overshoot * overshoot;
            transform.localScale = Vector3.one * Mathf.Max(.01f, scale);
            transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI) * -18);
            if (sparks != null) for (int i = 0; i < sparks.Length; i++) if (sparks[i])
                    {
                        float burst = Mathf.Clamp01((age - .16f) / .45f), angle = i * Mathf.PI * 2 / sparks.Length;
                        sparks[i].anchoredPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Mathf.Lerp(18, 88, burst);
                        sparks[i].localScale = Vector3.one * (age < .16f ? 0 : Mathf.Sin(burst * Mathf.PI));
                    }
            if (age >= .18f && !sounded) { sounded = true; DeviceVibration.Pulse(26); var source = GetComponentInParent<AuthoredCanvas>().starAudio; if (source && source.clip) source.PlayOneShot(source.clip, .65f); }
        }
    }
}
