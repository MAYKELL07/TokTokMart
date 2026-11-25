using UnityEngine;

namespace TokTokMart
{
    public sealed class CombatReaction : MonoBehaviour
    {
        SpriteRenderer art;
        Vector3 baseScale, basePosition;
        Quaternion baseRotation;
        Color baseColor;
        public void Apply(CharacterAnimation actor, float impact, float stun, float strike, float clock, int facing, bool defeated = false)
        {
            if (!art) { art = actor.artwork; if (!art) return; baseScale = art.transform.localScale; basePosition = art.transform.localPosition; baseRotation = art.transform.localRotation; baseColor = art.color; }
            float punch = Mathf.Sin(Mathf.Clamp01(strike / .28f) * Mathf.PI);
            float hurt = Mathf.Clamp01(impact / .25f), direction = facing == 23 ? -1 : 1;
            art.transform.localPosition = basePosition + new Vector3(direction * punch * .09f, stun > 0 ? Mathf.Sin(clock * 18) * .015f : 0, 0);
            art.transform.localRotation = baseRotation * Quaternion.Euler(0, 0, direction * punch * -13 + (stun > 0 ? Mathf.Sin(clock * 15) * 6 : 0));
            art.transform.localScale = Vector3.Scale(baseScale, new Vector3(1 + punch * .12f - hurt * .08f, 1 - punch * .1f + hurt * .08f, 1)) * (defeated ? Mathf.Max(.1f, Mathf.Clamp01(impact / .4f)) : 1);
            art.color = Color.Lerp(baseColor, Color.white, hurt * .8f);
        }
    }
}
