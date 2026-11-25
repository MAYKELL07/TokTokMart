using UnityEngine;

namespace TokTokMart
{
    public enum ShiftFocus { Warmup, Rush, Restock, Cleanup, Chaos }
    public readonly struct ShiftGoals
    {
        public readonly int serve, stock, clean;
        public readonly float workloadSeconds, duration;
        public readonly ShiftFocus focus;
        public int taskCount => serve + stock + clean;
        public string Title => focus == ShiftFocus.Rush ? "LUNCH RUSH" : focus == ShiftFocus.Restock ? "STOCK UP" : focus == ShiftFocus.Cleanup ? "MOP UP" : focus == ShiftFocus.Chaos ? "CHAOS SHIFT" : "WELCOME SHIFT";
        public string Hint => focus == ShiftFocus.Rush ? "Beat the clock. Keep the checkout moving!" : focus == ShiftFocus.Restock ? "Grab crates and refill shelves. Keep an eye on the till." : focus == ShiftFocus.Cleanup ? "Clear the puddles and open up the aisles." : focus == ShiftFocus.Chaos ? "Handle the surprise guest and keep chaos below 100%." : "Stock, serve and clean. Find your rhythm!";
        public float ArrivalScale => focus == ShiftFocus.Rush ? .72f : focus == ShiftFocus.Cleanup ? 1.25f : focus == ShiftFocus.Restock ? 1.1f : .95f;
        public int SpillEvery => focus == ShiftFocus.Cleanup ? 2 : focus == ShiftFocus.Chaos ? 3 : focus == ShiftFocus.Rush ? 8 : 6;
        public ShiftGoals(int shift)
        {
            int level = Mathf.Clamp(shift, 1, 10000), tasks = level + 4;
            focus = level == 1 ? ShiftFocus.Warmup : level % 5 == 0 ? ShiftFocus.Chaos : (ShiftFocus)(1 + (level - 2) % 4);
            float service = focus == ShiftFocus.Rush ? .65f : focus == ShiftFocus.Restock || focus == ShiftFocus.Cleanup ? .35f : .45f;
            float stocking = focus == ShiftFocus.Restock ? .50f : focus == ShiftFocus.Cleanup ? .15f : focus == ShiftFocus.Rush ? .20f : .25f;
            serve = Mathf.Max(3, Mathf.RoundToInt(tasks * service)); stock = Mathf.Max(1, Mathf.RoundToInt(tasks * stocking));
            clean = Mathf.Max(1, tasks - serve - stock); stock = Mathf.Max(1, tasks - serve - clean);
            workloadSeconds = serve * 8 + stock * 16 + clean * 12;
            duration = 172 + (level - 1) * 18 + (focus == ShiftFocus.Rush ? -4 : focus == ShiftFocus.Restock ? 2 : focus == ShiftFocus.Cleanup ? 3 : focus == ShiftFocus.Chaos ? 8 : 0);
        }
    }
}
