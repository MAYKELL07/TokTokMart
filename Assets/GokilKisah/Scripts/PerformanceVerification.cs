using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        public void BeginPerformanceVerification(string directory){StartCoroutine(PerformanceVerification(directory));}
        IEnumerator PerformanceVerification(string directory)
        {
            Directory.CreateDirectory(directory);testing=true;verificationRunning=true;settingsOpen=false;autoPaused=false;
            ResetRun(Encounter.Store);selected=true;mainMenu=false;RefreshSceneGeometry();
            var watch=new System.Diagnostics.Stopwatch();
            var destinations=new List<Vector2>{supply,ShopLayout.ToGame(shop.partnerCheckoutSpot.position),phone};
            foreach(var shelf in sceneShelves)destinations.Add(ShopLayout.ToGame(shelf.shoppingPoint.position));
            watch.Start();
            for(int i=0;i<60;i++)Navigate(playerStart,destinations[i%destinations.Count],4);
            watch.Stop();double navigationMs=watch.Elapsed.TotalMilliseconds;
            ResetCustomers();
            for(int i=0;i<10;i++)
            {
                var c=SpawnCustomer();c.position=i<8?ShelfPoint(i):ShopDoor+new Vector2((i-8)*50,-60);
                c.goal=ShelfPoint((i+4)%sceneShelves.Length);c.shelf=(i+4)%sceneShelves.Length;c.state=ShopperState.Browsing;
            }
            watch.Restart();for(int tick=0;tick<180;tick++)UpdateCustomers(1f/30);watch.Stop();
            File.WriteAllText(Path.Combine(directory,"performance.txt"),"60 route queries: "+navigationMs.ToString("F2")+" ms\n180 ten-customer simulation ticks: "+watch.Elapsed.TotalMilliseconds.ToString("F2")+" ms\nAverage simulation tick: "+(watch.Elapsed.TotalMilliseconds/180).ToString("F3")+" ms\n");
            ResetRun(Encounter.Store);selected=true;mainMenu=false;testing=false;
            var samples=new List<float>();
            for(int frame=0;frame<240;frame++){yield return null;if(frame>=60)samples.Add(Time.unscaledDeltaTime*1000);}
            samples.Sort();float sum=0;foreach(float ms in samples)sum+=ms;
            File.AppendAllText(Path.Combine(directory,"performance.txt"),"Live mean frame: "+(sum/samples.Count).ToString("F2")+" ms\nLive p95 frame: "+samples[(int)(samples.Count*.95f)].ToString("F2")+" ms\n");
            verificationRunning=false;testing=false;ReturnToMainMenu();
        }
    }
}
