using UnityEngine;

namespace GokilKisah
{
    // Each shift adds a customer; stocking and cleanup targets rise every three/four shifts.
    // The fixed opening allowance means the required workload per available second increases
    // every shift, while added work still receives enough time to remain a playable shop loop.
    public readonly struct ShiftTargets
    {
        public readonly int serve,stock,clean;
        public readonly float workloadSeconds,duration;
        public ShiftTargets(int shift)
        {
            int n=Mathf.Clamp(shift,1,10000)-1;
            serve=3+n;stock=1+n/3;clean=1+n/4;
            workloadSeconds=serve*8+stock*16+clean*12;
            duration=150+workloadSeconds;
        }
    }
}
