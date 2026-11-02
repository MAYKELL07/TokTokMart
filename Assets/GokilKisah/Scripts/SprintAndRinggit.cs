using UnityEngine;
using UnityEngine.InputSystem;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        [Header("Sprint & stamina")]
        [Min(1)] public float sprintMultiplier=1.65f;
        [Min(1)] public float maxStamina=100, staminaDrain=20, staminaRecovery=25;
        [Min(0)] public float staminaRecoveryDelay=.8f;
        public float Stamina {get;private set;}=100;
        public bool IsSprinting {get;private set;}
        bool sprintExhausted, touchSprint;
        float staminaRest;
        void ResetStamina(){Stamina=maxStamina;IsSprinting=false;sprintExhausted=false;touchSprint=false;staminaRest=0;}
        float MovementSpeed(bool requested,bool moving)
        {
            IsSprinting=requested&&moving&&skillType<0&&!sprintExhausted&&Stamina>0;
            return tuning.moveSpeed*(IsSprinting?sprintMultiplier:1);
        }
        void UpdateStamina(float dt,bool moved)
        {
            if(IsSprinting&&moved)
            {
                Stamina=Mathf.Max(0,Stamina-staminaDrain*dt);staminaRest=staminaRecoveryDelay;
                if(Stamina<=0){sprintExhausted=true;IsSprinting=false;}
                return;
            }
            IsSprinting=false;
            float available=Mathf.Max(0,dt-staminaRest);staminaRest=Mathf.Max(0,staminaRest-dt);
            Stamina=Mathf.Min(maxStamina,Stamina+staminaRecovery*available);
            if(Stamina>=maxStamina*.25f)sprintExhausted=false;
        }
        void DrawStamina()
        {
            if(!selected)return;
            Text(new Rect(200,16,110,40),"Stamina",20,ink,FontStyle.Normal,TextAnchor.MiddleLeft);
            Bar(new Rect(316,32,150,8),Stamina/maxStamina,sprintExhausted?red:teal);
        }
        static string Ringgit(int sen)=>"RM "+RinggitNumber(sen);
        static string RinggitNumber(int sen)=>(sen/100m).ToString("0.00",System.Globalization.CultureInfo.InvariantCulture);
        static bool TryRinggit(string entry,out int sen)
        {
            sen=0;
            if(string.IsNullOrEmpty(entry)||!decimal.TryParse(entry,System.Globalization.NumberStyles.AllowDecimalPoint,System.Globalization.CultureInfo.InvariantCulture,out decimal value)||value<0||value>99999.99m)return false;
            decimal cents=value*100;if(cents!=decimal.Truncate(cents))return false;sen=(int)cents;return true;
        }
        void EnterPricePoint()
        {if(priceEntry.Contains("."))return;if(priceEntry.Length==0)priceEntry="0";if(priceEntry.Length<7)priceEntry+=".";}
    }
}
