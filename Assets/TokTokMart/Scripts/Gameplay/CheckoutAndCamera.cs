using UnityEngine;
namespace TokTokMart
{
    public sealed partial class ShiftController
    {
        bool mainMenu = true;
        bool cameraInitialized;
        Vector2 cameraCenter;
        [Header("Follow camera")]
        [Range(1.3f, 4)] public float gameplayZoom = 2.35f;
        [Range(.1f, .8f)] public float deadZoneWidth = .48f, deadZoneHeight = .44f;
        [Range(1, 20)] public float followSpeed = 8;
        string priceEntry = "", taskFeedback = "";
        int receiptTotal, taskCombo;
        string[] receiptNames;
        int[] receiptPrices;
        string receiptItemsText = "", receiptPricesText = "";
        readonly int[] groceryPrices = { 400, 200, 500, 300, 400, 200, 100, 200, 300, 200 };
        readonly int[] smallBasketPrices = { 450, 250, 500, 350, 400, 250, 150, 250, 300, 200 };
        bool WholeRinggitCheckout { get; set; } = true;
        bool UsesWholeRinggit(int basketCount) => !campaignActive || CurrentLevel <= 3 || basketCount >= 4;
        int MaxBasketItems => !campaignActive ? 3 : Mathf.Clamp(3 + (CurrentLevel - 2) / 2, 3, 6);
        int UnitPrice(int item, int basketCount) => (UsesWholeRinggit(basketCount) ? groceryPrices : smallBasketPrices)[item];
        string ReceiptMoney(int sen) => WholeRinggitCheckout ? "RM " + (sen / 100) : Ringgit(sen);
        void FollowPlayerCamera(Camera camera)
        {
            camera.orthographic = true; camera.orthographicSize = gameplayZoom;
            var point = (Vector2)ShopLayout.ToWorld(player) + Vector2.up * .22f;
            if (!cameraInitialized) { cameraCenter = point; cameraInitialized = true; }
            float halfY = camera.orthographicSize, halfX = halfY * camera.aspect;
            var delta = point - cameraCenter;
            Vector2 excess = new Vector2(Mathf.Sign(delta.x) * Mathf.Max(0, Mathf.Abs(delta.x) - halfX * deadZoneWidth), Mathf.Sign(delta.y) * Mathf.Max(0, Mathf.Abs(delta.y) - halfY * deadZoneHeight));
            cameraCenter += excess * (1 - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime));
            if (shop.walkableArea)
            {
                var bounds = shop.CameraBounds;
                cameraCenter.x = bounds.size.x < halfX * 2 ? bounds.center.x : Mathf.Clamp(cameraCenter.x, bounds.min.x + halfX, bounds.max.x - halfX);
                cameraCenter.y = bounds.size.y < halfY * 2 ? bounds.center.y : Mathf.Clamp(cameraCenter.y, bounds.min.y + halfY, bounds.max.y - halfY);
            }
            camera.transform.position = new Vector3(cameraCenter.x, cameraCenter.y, -10);
        }
        void PrepareTask(int type)
        {
            taskCombo = 0; taskFeedback = ""; priceEntry = ""; receiptTotal = 0;
            if (type != 1) return;
            var customer = CustomerAtRegister(); int count = Mathf.Clamp(customer == null ? 6 : customer.basket, 1, 6);
            WholeRinggitCheckout = UsesWholeRinggit(count);
            receiptNames = new string[count]; receiptPrices = new int[count];
            for (int i = 0; i < count; i++)
            {
                int item = customer != null && i < customer.items.Count ? customer.items[i] : ((customer == null ? 0 : customer.id) + i) % ShopProducts.ProductCount;
                receiptNames[i] = ShopProducts.ProductName(item); receiptPrices[i] = UnitPrice(item, count); receiptTotal += receiptPrices[i];
            }
            receiptItemsText = string.Join("\n", receiptNames);
            receiptPricesText = string.Join("\n", System.Array.ConvertAll(receiptPrices, ReceiptMoney));
        }
        void EnterPriceDigit(char digit)
        { int dot = priceEntry.IndexOf('.'); if (priceEntry.Length < 8 && (dot < 0 || priceEntry.Length - dot <= 2)) { priceEntry += digit; PlaySound("Click", .4f); taskFeedback = ""; } }
        bool SubmitPrice()
        {
            var customer = CustomerAtRegister();
            if (customer == null || customer.id != skillTarget) { skillType = -1; Toast("Customer left"); return false; }
            if (!TryRinggit(priceEntry, out int total) || total != receiptTotal)
            { if (Time.unscaledTime < taskLastAttempt + .25f) return false; taskLastAttempt = Time.unscaledTime; if (campaignActive) priceMistakes++; taskFeedbackFailed = true; PlaySound("Miss"); taskFeedback = "Wrong total: -25 accuracy points. Add the receipt prices and retry."; skillFlash = Time.unscaledTime + .4f; return false; }
            ServeCustomer(); skillType = -1; PlaySound("Success"); Toast("Paid · " + ReceiptMoney(receiptTotal)); SetStep("04 / KEEP YOUR SHOP RUNNING", "Keep serving customers, stocking shelves and clearing spills."); return true;
        }
        void CashierKeyboard()
        {
            if (skillType != 1 || settingsOpen || mainMenu) return;
            var e = Event.current; if (e.type != EventType.KeyDown) return;
            int key = (int)e.keyCode;
            if (key >= (int)KeyCode.Alpha0 && key <= (int)KeyCode.Alpha9) { EnterPriceDigit((char)('0' + key - (int)KeyCode.Alpha0)); e.Use(); }
            else if (key >= (int)KeyCode.Keypad0 && key <= (int)KeyCode.Keypad9) { EnterPriceDigit((char)('0' + key - (int)KeyCode.Keypad0)); e.Use(); }
            else if (e.keyCode == KeyCode.Period || e.keyCode == KeyCode.KeypadPeriod) { EnterPricePoint(); e.Use(); }
            else if (e.keyCode == KeyCode.Backspace) { if (priceEntry.Length > 0) priceEntry = priceEntry.Substring(0, priceEntry.Length - 1); e.Use(); }
            else if (e.keyCode == KeyCode.Delete) { priceEntry = ""; e.Use(); }
            else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) { SubmitPrice(); e.Use(); }
        }
        void ReturnToMainMenu()
        {
            scoreGuideOpen = false; levelMapOpen = false; chosenLevel = 0; SetSettingsOpen(false); ResetRun(Encounter.Store); selected = false; mainMenu = true; heldKeys.Clear();
        }
    }
}
