using UnityEngine;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        bool mainMenu=true;
        bool cameraInitialized;
        Vector2 cameraCenter;
        [Header("Follow camera")]
        [Range(1.3f,4)] public float gameplayZoom=2.35f;
        [Range(.1f,.8f)] public float deadZoneWidth=.48f, deadZoneHeight=.44f;
        [Range(1,20)] public float followSpeed=8;
        string priceEntry="", taskFeedback="";
        int receiptTotal, taskCombo;
        string[] receiptNames;
        int[] receiptPrices;
        readonly string[] groceries={"Milk", "Bread", "Rice", "Tea", "Eggs", "Noodles"};
        readonly int[] groceryPrices={350,250,500,150,450,200};
        void FollowPlayerCamera(Camera camera)
        {
            camera.orthographic=true;camera.orthographicSize=gameplayZoom;
            var point=(Vector2)ShopLayout.ToWorld(player)+Vector2.up*.22f;
            if(!cameraInitialized){cameraCenter=point;cameraInitialized=true;}
            float halfY=camera.orthographicSize,halfX=halfY*camera.aspect;
            var delta=point-cameraCenter;
            Vector2 excess=new Vector2(Mathf.Sign(delta.x)*Mathf.Max(0,Mathf.Abs(delta.x)-halfX*deadZoneWidth),Mathf.Sign(delta.y)*Mathf.Max(0,Mathf.Abs(delta.y)-halfY*deadZoneHeight));
            cameraCenter+=excess*(1-Mathf.Exp(-followSpeed*Time.unscaledDeltaTime));
            if(shop.walkableArea)
            {
                var bounds=shop.CameraBounds;
                cameraCenter.x=bounds.size.x<halfX*2?bounds.center.x:Mathf.Clamp(cameraCenter.x,bounds.min.x+halfX,bounds.max.x-halfX);
                cameraCenter.y=bounds.size.y<halfY*2?bounds.center.y:Mathf.Clamp(cameraCenter.y,bounds.min.y+halfY,bounds.max.y-halfY);
            }
            camera.transform.position=new Vector3(cameraCenter.x,cameraCenter.y,-10);
        }
        void PrepareTask(int type)
        {
            taskCombo=0;taskFeedback="";priceEntry="";receiptTotal=0;
            if(type!=1)return;
            var customer=CustomerAtRegister();int count=Mathf.Clamp(customer==null?1:customer.basket,1,6);
            receiptNames=new string[count];receiptPrices=new int[count];
            for(int i=0;i<count;i++)
            {
                int item=customer!=null&&i<customer.items.Count?customer.items[i]:((customer==null?0:customer.id)+i)%groceries.Length;
                receiptNames[i]=groceries[item];receiptPrices[i]=groceryPrices[item];receiptTotal+=groceryPrices[item];
            }
        }
        void EnterPriceDigit(char digit)
        {int dot=priceEntry.IndexOf('.');if(priceEntry.Length<8&&(dot<0||priceEntry.Length-dot<=2)){priceEntry+=digit;PlaySound("Click",.4f);taskFeedback="";}}
        bool SubmitPrice()
        {
            var customer=CustomerAtRegister();
            if(customer==null||customer.id!=skillTarget){skillType=-1;Toast("The customer has left the register.");return false;}
            if(!TryRinggit(priceEntry,out int total)||total!=receiptTotal)
            {if(campaignActive)priceMistakes++;PlaySound("Miss");taskFeedback="That total does not match. Check the receipt and try again.";skillFlash=Time.unscaledTime+.3f;return false;}
            ServeCustomer();skillType=-1;PlaySound("Success");Toast("Correct total · "+Ringgit(receiptTotal));SetStep("04 / KEEP YOUR SHOP RUNNING","Keep serving customers, stocking shelves and clearing spills.");return true;
        }
        void CashierKeyboard()
        {
            if(skillType!=1||settingsOpen||mainMenu||replay)return;
            var e=Event.current;if(e.type!=EventType.KeyDown)return;
            int key=(int)e.keyCode;
            if(key>=(int)KeyCode.Alpha0&&key<=(int)KeyCode.Alpha9){EnterPriceDigit((char)('0'+key-(int)KeyCode.Alpha0));e.Use();}
            else if(key>=(int)KeyCode.Keypad0&&key<=(int)KeyCode.Keypad9){EnterPriceDigit((char)('0'+key-(int)KeyCode.Keypad0));e.Use();}
            else if(e.keyCode==KeyCode.Period||e.keyCode==KeyCode.KeypadPeriod){EnterPricePoint();e.Use();}
            else if(e.keyCode==KeyCode.Backspace){if(priceEntry.Length>0)priceEntry=priceEntry.Substring(0,priceEntry.Length-1);e.Use();}
            else if(e.keyCode==KeyCode.Delete){priceEntry="";e.Use();}
            else if(e.keyCode==KeyCode.Return||e.keyCode==KeyCode.KeypadEnter){SubmitPrice();e.Use();}
        }
        void ReturnToMainMenu()
        {
            if(demoRoutine!=null)StopCoroutine(demoRoutine);demoRoutine=null;replay=false;
            SetSettingsOpen(false);ResetRun(Encounter.Store);selected=false;mainMenu=true;heldKeys.Clear();
        }
        void DrawMainMenu()
        {
            Fill(new Rect(0,0,1600,900),paper);
            Text(new Rect(500,220,600,100),"Tok Mart",72,ink,FontStyle.Bold,TextAnchor.MiddleCenter);
            GUI.enabled=!settingsOpen;
            if(UiButton(new Rect(610,350,380,64),SavedLevel>1?"Continue · Level "+SavedLevel:"Start · Level 1",surface,26)){mainMenu=false;selected=false;}
            if(SavedLevel>1&&UiButton(new Rect(610,426,380,54),"New game",new Color(.87f,.88f,.83f),23)){PlayerPrefs.SetInt(LevelSave,1);PlayerPrefs.SetInt(BossSave,0);PlayerPrefs.Save();mainMenu=false;selected=false;}
            if(UiButton(new Rect(610,496,380,64),"Settings",new Color(.87f,.88f,.83f),26))SetSettingsOpen(true);
            if(UiButton(new Rect(610,582,380,64),"Quit",new Color(.87f,.88f,.83f),26)){if(!Application.isEditor)Application.Quit();else Toast("Quit is available in the standalone game.");}
            GUI.enabled=true;
            if(settingsOpen)DrawSettings();
        }
        void DrawEncounterStatus()
        {
            if(!selected||encounter==Encounter.Store)return;
            Panel(new Rect(1210,200,340,100),resolved?mint:surface,true);
            Color text=resolved?ink:paper;
            string title=resolved?"All clear":scenarioTitles[(int)encounter];
            Text(new Rect(1226,212,308,25),title,20,text,FontStyle.Bold);
            string status=resolved?"The store is safe.":encounter==Encounter.Bo?boRemaining.ToString("0.0")+"s remaining":encounter==Encounter.Goblins?(rayCalled?"Uncle Ray: "+rayRemaining.ToString("0.0")+"s":"Call Uncle Ray at the wall phone"):encounter==Encounter.KKung?"Find the fluorescent light switch":nailsHit+" / "+tuning.nailHits+" rear hits";
            Text(new Rect(1226,252,308,40),status,21,!resolved&&encounter==Encounter.Bo&&boRemaining<5?red:text,FontStyle.Bold);
        }
        void DrawLegacyCashier()
        {
            Shade();Panel(new Rect(290,165,1020,570),paper,true);
            Eyebrow(new Rect(330,200,490,26),"CHECKOUT / CUSTOMER RECEIPT",teal);
            Text(new Rect(330,243,490,48),"Enter the total price",34,ink,FontStyle.Bold);
            Text(new Rect(330,298,470,46),"Add the prices shown below.",20,new Color(.3f,.42f,.4f));
            for(int i=0;receiptNames!=null&&i<receiptNames.Length;i++)
            {
                Text(new Rect(330,365+i*40,260,35),receiptNames[i],22,ink);
                Text(new Rect(575,365+i*40,215,35),Ringgit(receiptPrices[i]),22,ink,FontStyle.Bold,TextAnchor.UpperRight);
            }
            Panel(new Rect(835,200,430,76),Time.unscaledTime<skillFlash?new Color(1,.78f,.73f):new Color(.83f,.89f,.83f));
            Text(new Rect(853,216,395,42),"RM "+(priceEntry.Length==0?"0.00":priceEntry),32,ink,FontStyle.Bold,TextAnchor.MiddleRight);
            string[] keys={"7","8","9","4","5","6","1","2","3",".","0","Back"};
            GUI.enabled=!replay;
            for(int i=0;i<12;i++)if(UiButton(new Rect(835+i%3*146,302+i/3*71,137,60),keys[i],i<9||i==10?new Color(.86f,.9f,.83f):gold,24))
            {if(i==9)EnterPricePoint();else if(i==11){if(priceEntry.Length>0)priceEntry=priceEntry.Substring(0,priceEntry.Length-1);}else EnterPriceDigit(keys[i][0]);}
            if(UiButton(new Rect(835,594,430,61),"Charge customer  [Enter]",mint,23))SubmitPrice();
            if(UiButton(new Rect(330,650,220,45),"Cancel  [Esc]",new Color(.86f,.9f,.83f),18))skillType=-1;
            if(UiButton(new Rect(565,650,215,45),"Clear",gold,18))priceEntry="";
            if(taskFeedback.Length>0)Text(new Rect(330,602,475,44),taskFeedback,18,red,FontStyle.Bold);
            GUI.enabled=true;
        }
    }
}
