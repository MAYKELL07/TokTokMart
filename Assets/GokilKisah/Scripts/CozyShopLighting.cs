using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GokilKisah
{
    // Keep the encounter's lights-out mechanic while adding gentle everyday light pools.
    public sealed class CozyShopLighting : MonoBehaviour
    {
        public Light2D ambient;
        public Light2D[] shopLights;
        public float ambientIntensity=.8f, poolIntensity=.28f;
        StoreGameController game;
        void Start(){game=FindFirstObjectByType<StoreGameController>();}
        void LateUpdate()
        {
            bool dark=game&&game.encounter==StoreGameController.Encounter.KKung&&!game.lightsOn;
            float blend=1-Mathf.Exp(-Time.unscaledDeltaTime*6);
            if(ambient)ambient.intensity=Mathf.Lerp(ambient.intensity,dark?.5f:ambientIntensity,blend);
            if(shopLights!=null)foreach(var light in shopLights)if(light)
                light.intensity=Mathf.Lerp(light.intensity,dark?.025f:poolIntensity,blend);
        }
    }
}
