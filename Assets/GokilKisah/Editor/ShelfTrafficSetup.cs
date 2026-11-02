using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace GokilKisah.Editor
{
    public static class ShelfTrafficSetup
    {
        static SpriteRenderer[] Slots(ShelfInventory s)=>s.GetComponentsInChildren<SpriteRenderer>(true).Where(r=>r.name.StartsWith("Stock slot",StringComparison.Ordinal)).ToArray();
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();var all=shop.GetComponentsInChildren<ShelfInventory>(true);
            var regular=all.Single(s=>s.name=="Shelf 5");var middle=all.Single(s=>s.name=="Middle aisle Shelf 1");
            Directory.CreateDirectory("Backups/ShelfTraffic-20261021");
            EditorSceneManager.SaveScene(shop.gameObject.scene,"Backups/ShelfTraffic-20261021/BeforeShelfTemplates-live.unity",true);
            Undo.RegisterFullObjectHierarchyUndo(shop.gameObject,"Use authored Shelf 5 and middle aisle Shelf 1 templates");
            foreach(var target in all.Where(s=>s.gameObject.activeInHierarchy))
            {
                var source=target.name.StartsWith("Middle aisle",StringComparison.Ordinal)?middle:regular;
                if(source!=target)
                {
                    foreach(var slot in Slots(target))Undo.DestroyObjectImmediate(slot.gameObject);
                    var sourceSlots=Slots(source);
                    foreach(var slot in sourceSlots)
                    {
                        var copy=UnityEngine.Object.Instantiate(slot.gameObject,target.transform);
                        copy.transform.position=target.transform.position+(slot.transform.position-source.transform.position);
                        copy.transform.rotation=slot.transform.rotation;
                        var parent=target.transform.lossyScale;var scale=slot.transform.lossyScale;
                        copy.transform.localScale=new Vector3(scale.x/parent.x,scale.y/parent.y,1);Undo.RegisterCreatedObjectUndo(copy,"Copy authored stock slots");
                    }
                    var sourceArt=source.transform.Find("Artist shelf fixture").GetComponent<SpriteRenderer>();
                    var art=target.transform.Find("Artist shelf fixture").GetComponent<SpriteRenderer>();
                    art.sprite=sourceArt.sprite;art.drawMode=sourceArt.drawMode;art.size=sourceArt.size;art.color=sourceArt.color;
                    art.transform.position=target.transform.position+(sourceArt.transform.position-source.transform.position);
                    art.transform.rotation=sourceArt.transform.rotation;var p=target.transform.lossyScale;var a=sourceArt.transform.lossyScale;
                    art.transform.localScale=new Vector3(a.x/p.x,a.y/p.y,1);
                    target.stockedSprite=source.stockedSprite;target.emptySprite=source.emptySprite;
                }
                // Read all authored slot children, including newly duplicated rows omitted from the old array.
                var ordered=Slots(target).OrderByDescending(r=>Mathf.Round(r.transform.position.y*100)).ThenBy(r=>r.transform.position.x).ToArray();
                for(int i=0;i<ordered.Length;i++){ordered[i].name="Stock slot "+(i+1).ToString("00");ordered[i].transform.SetSiblingIndex(i);}
                target.stockSlots=ordered;target.capacity=ordered.Length;target.initialStock=Mathf.Clamp(target.initialStock,0,target.capacity);
                target.ShowStock(target.initialStock==0?1:0);target.ShowStock(target.initialStock);
                var fixture=target.transform.Find("Artist shelf fixture").GetComponent<SpriteRenderer>();
                var fade=target.GetComponent<SoftOccluder>();if(fade){fade.silhouette=fixture;fade.artwork=new[]{fixture}.Concat(ordered).ToArray();}
                var sorter=target.GetComponent<FootDepthSorter>();if(sorter){sorter.artwork=new[]{fixture}.Concat(ordered).ToArray();sorter.artworkOffsets=new[]{0}.Concat(ordered.Select(r=>2)).ToArray();}
                foreach(var c in target.GetComponentsInChildren<Component>(true))if(c)EditorUtility.SetDirty(c);
            }
            EditorSceneManager.MarkSceneDirty(shop.gameObject.scene);EditorSceneManager.SaveScene(shop.gameObject.scene);AssetDatabase.SaveAssets();Inspect();
        }
        public static void OrderSlots()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();
            foreach(var shelf in shop.Shelves)for(int i=0;i<shelf.stockSlots.Length;i++)
            {shelf.stockSlots[i].transform.SetSiblingIndex(i);EditorUtility.SetDirty(shelf.stockSlots[i].transform);}
            EditorSceneManager.MarkSceneDirty(shop.gameObject.scene);EditorSceneManager.SaveScene(shop.gameObject.scene);
        }
        public static void Inspect()
        {
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();var lines=new List<string>();
            foreach(var s in shop.GetComponentsInChildren<ShelfInventory>(true))
            {
                lines.Add(s.name+" active="+s.gameObject.activeInHierarchy+" pos="+s.transform.position+" scale="+s.transform.lossyScale+" capacity="+s.capacity);
                foreach(var r in s.GetComponentsInChildren<SpriteRenderer>(true))lines.Add("  "+r.name+" sprite="+(r.sprite?r.sprite.name:"none")+" local="+r.transform.localPosition+" scale="+r.transform.localScale+" world="+r.transform.position);
            }
            Directory.CreateDirectory("Verification/ShelfTraffic");File.WriteAllLines("Verification/ShelfTraffic/before.txt",lines);
        }
    }
}
