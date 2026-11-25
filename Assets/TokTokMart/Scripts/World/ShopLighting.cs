using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TokTokMart
{
    // Keep the encounter's lights-out mechanic while adding gentle everyday light pools.
    public sealed class ShopLighting : MonoBehaviour
    {
        public Light2D ambient;
        public Light2D[] shopLights;
        public float ambientIntensity = .8f, poolIntensity = .28f;
        ShiftController game;
        void Start() { game = FindFirstObjectByType<ShiftController>(); }
        void LateUpdate()
        {
            bool dark = game && game.encounter == ShiftController.Encounter.KKung && !game.lightsOn;
            float blend = 1 - Mathf.Exp(-Time.unscaledDeltaTime * 6);
            if (ambient) ambient.intensity = Mathf.Lerp(ambient.intensity, dark ? .5f : ambientIntensity, blend);
            if (shopLights != null) foreach (var light in shopLights) if (light)
                        light.intensity = Mathf.Lerp(light.intensity, dark ? .025f : poolIntensity, blend);
        }
    }
}
