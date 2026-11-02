namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        bool testing, verificationRunning;
        public void BeginVerification(string resultPath){BeginUpgradeVerification(resultPath);}
    }
}
