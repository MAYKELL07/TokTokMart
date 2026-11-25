using UnityEngine;

namespace TokTokMart
{
    public sealed class RoomShading : MonoBehaviour
    {
        [SerializeField] ShopLayout shop;
        [SerializeField] SpriteRenderer mainRoomShade;
        [SerializeField] SpriteRenderer storageRoomShade;
        [SerializeField, Range(0, 1)] float shadeOpacity = .94f;

        public bool InStorage { get; private set; }

        public void Tick(Vector2 feet, bool playing, float deltaTime)
        {
            float boundary = shop.walkableArea.bounds.max.y;
            if (feet.y > boundary + .1f)
                InStorage = true;
            else if (feet.y < boundary - .1f)
                InStorage = false;

            float blend = 1 - Mathf.Exp(-Mathf.Min(deltaTime, .05f) / .16f);
            SetOpacity(mainRoomShade, playing && InStorage ? shadeOpacity : 0, blend);
            SetOpacity(storageRoomShade, playing && !InStorage ? shadeOpacity : 0, blend);
        }

        static void SetOpacity(SpriteRenderer renderer, float opacity, float blend)
        {
            var color = renderer.color;
            color.a = Mathf.Lerp(color.a, opacity, blend);
            renderer.color = color;
        }
    }
}
