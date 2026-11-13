using UnityEngine;
namespace GokilKisah
{
    // Objective points are capped: basket RNG and repeat-task farming never affect the rating.
    public readonly struct ShiftScore
    {
        public readonly int objectives, accuracy, speed, control, total, maximum, stars;
        public readonly int oneStar, twoStars, threeStars;
        public static int Base(ShiftTargets t, bool boss) => t.serve*100+t.stock*150+t.clean*100+(boss?200:0);
        public ShiftScore(ShiftTargets t, int served, int stocked, int cleaned, bool bossRequired, bool bossCleared,
            int priceErrors, int timingErrors, int lostCustomers, float elapsed, float chaos, bool complete)
        {
            objectives=Mathf.Clamp(served,0,t.serve)*100+Mathf.Clamp(stocked,0,t.stock)*150+Mathf.Clamp(cleaned,0,t.clean)*100+(bossRequired&&bossCleared?200:0);
            oneStar=Base(t,bossRequired);twoStars=oneStar+300;threeStars=oneStar+450;maximum=oneStar+500;
            accuracy=Mathf.Max(0,200-25*(Mathf.Max(0,priceErrors)+Mathf.Max(0,timingErrors))-100*Mathf.Max(0,lostCustomers));
            speed=elapsed<=t.duration*.70f+.0001f?200:elapsed<=t.duration*.85f+.0001f?100:0;
            control=chaos<=15?100:chaos<=40?50:0;
            total=objectives+(complete?accuracy+speed+control:0);
            stars=!complete?0:total>=threeStars?3:total>=twoStars?2:1;
        }
    }
}
