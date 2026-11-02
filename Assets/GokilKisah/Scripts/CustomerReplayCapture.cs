using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        public void StartCustomerReplay()
        {if(demoRoutine!=null)StopCoroutine(demoRoutine);demoRoutine=StartCoroutine(CustomerPitchDemo());}
        IEnumerator CustomerPitchDemo()
        {
            replay=true;Choose(false);ResetRun(Encounter.Store);
            SetStep("01 / SHOPPERS ENTER","Customers arrive through the door and choose a stocked shelf.");
            yield return new WaitForSeconds(1.5f);
            yield return Go(ShopLayout.ToGame(shop.register.approachPoint.position));
            SetStep("02 / SHOP AND DROP ITEMS","Customers collect real shelf stock. A clumsy shopper can create a spill.");
            float guard=40;
            while(CustomerAtRegister()==null&&guard>0){guard-=Time.deltaTime;yield return null;}
            if(CustomerAtRegister()!=null)
            {
                SetStep("03 / THE CHECKOUT QUEUE","The first shopper waits at the register. Add the receipt and enter the total to take payment.");
                var paidShopper=CustomerAtRegister();Interact();yield return CompleteSkill();
                SetStep("04 / PAID SHOPPER LEAVES","Payment releases the shopper. They return to the door while the queue advances.");
                guard=25;while(customers.Contains(paidShopper)&&guard>0){guard-=Time.deltaTime;yield return null;}
            }
            yield return new WaitForSeconds(2);replay=false;demoRoutine=null;
        }
        public void BeginCustomerCapture(string directory){StartCoroutine(CaptureCustomerJourney(directory));}
        IEnumerator CaptureCustomerJourney(string directory)
        {
            Directory.CreateDirectory(directory);showPitchControls=false;settingsOpen=false;autoPaused=false;Time.timeScale=1;
            recordingSounds=true;recordingFrame=0;captureSounds.Clear();Time.captureFramerate=30;
            StartCustomerReplay();int frame=0;var timeline=new List<string>();
            do
            {
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(Path.Combine(directory,frame.ToString("D5")+".png"));
                timeline.Add(JsonUtility.ToJson(new CustomerFrame{frame=frame,title=stepTitle,shoppers=customers.Count,
                    spills=customerSpills,queue=customerQueue.Count,paid=served,departed=completedShoppingTrips,skill=skillType}));
                frame++;recordingFrame=frame;
            }while(replay||frame<2);
            File.WriteAllLines(Path.Combine(directory,"timeline.jsonl"),timeline);
            File.WriteAllLines(Path.Combine(directory,"audio.jsonl"),captureSounds.ConvertAll(e=>JsonUtility.ToJson(e)));
            recordingSounds=false;Time.captureFramerate=0;
            File.WriteAllText(Path.Combine(directory,"capture-complete.txt"),"Real Unity customer journey captured at 30 fps.");
            ResetRun(Encounter.Store);selected=false;
        }
        [System.Serializable] class CustomerFrame
        {public int frame,shoppers,spills,queue,paid,departed,skill;public string title;}
    }
}

