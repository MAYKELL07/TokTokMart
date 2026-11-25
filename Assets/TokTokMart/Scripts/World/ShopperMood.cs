using UnityEngine;

namespace TokTokMart
{
    [DefaultExecutionOrder(110)]
    public sealed class ShopperMood : MonoBehaviour
    {
        [SerializeField] CharacterAnimation actor;
        [SerializeField] SpriteRenderer angerMark;
        [SerializeField] SpriteRenderer patienceTrack;
        [SerializeField] SpriteRenderer patienceFill;
        [SerializeField] Color healthyColor = new(.3f, .85f, .45f);
        [SerializeField] Color cautionColor = new(1, .75f, .2f);
        [SerializeField] Color criticalColor = new(1, .25f, .2f);

        Color idleColor;
        Vector3 markScale;
        Vector3 fillScale;
        Vector3 markOffset;
        Vector3 trackOffset;
        Vector3 fillOffset;
        float frustration;
        bool queuing;

        void Awake()
        {
            idleColor = actor.artwork.color;
            markScale = angerMark.transform.localScale;
            fillScale = patienceFill.transform.localScale;
            markOffset = angerMark.transform.localPosition;
            trackOffset = patienceTrack.transform.localPosition;
            fillOffset = patienceFill.transform.localPosition;
        }

        public void SetFrustration(float amount, bool waiting)
        {
            frustration = Mathf.Clamp01(amount);
            queuing = waiting;
        }

        void LateUpdate()
        {
            var art = actor.artwork;
            bool showPatience = queuing && art.enabled;
            patienceTrack.enabled = patienceFill.enabled = showPatience;
            var bounds = art.bounds;
            if (showPatience)
            {
                float remaining = 1 - frustration;
                var anchor = new Vector3(bounds.center.x, bounds.max.y, art.transform.position.z);
                patienceTrack.transform.position = anchor + trackOffset;
                patienceFill.transform.position = anchor + fillOffset - Vector3.right * (fillScale.x * frustration / 2);
                patienceFill.transform.localScale = new Vector3(fillScale.x * remaining, fillScale.y, fillScale.z);
                patienceFill.color = remaining < .25f ? criticalColor : remaining < .5f ? cautionColor : healthyColor;
            }

            float annoyed = Mathf.InverseLerp(.45f, .85f, frustration);
            art.color = Color.Lerp(idleColor, new Color(1, .68f, .57f, idleColor.a), annoyed * .45f);
            angerMark.enabled = frustration >= .75f && art.enabled;
            if (angerMark.enabled)
            {
                angerMark.transform.position = new Vector3(bounds.max.x, bounds.max.y, art.transform.position.z) + markOffset;
                angerMark.transform.localScale = markScale * (1 + Mathf.Sin(Time.time * 8) * .08f);
                angerMark.sortingOrder = art.sortingOrder + 3;
            }
        }
    }
}
