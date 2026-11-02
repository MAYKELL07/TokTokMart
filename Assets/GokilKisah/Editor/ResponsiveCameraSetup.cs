using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
namespace GokilKisah.Editor
{
    public static class ResponsiveCameraSetup
    {
        [MenuItem("Gokil Kisah/Camera Test/Set Up Follow Camera and Soft Fades")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();
            var game=UnityEngine.Object.FindFirstObjectByType<StoreGameController>();
            if(!shop||!shop.worldCamera)throw new InvalidOperationException("Shop camera missing.");
            if(!shop.originalWorldCamera)shop.originalWorldCamera=shop.worldCamera;
            if(!shop.testFollowCamera)
            {
                var copy=UnityEngine.Object.Instantiate(shop.originalWorldCamera.gameObject);
                copy.name="Test Follow Camera";
                shop.testFollowCamera=copy.AddComponent<ResponsiveFollowCamera>();
                shop.testFollowCamera.originalGameplayZoom=game.gameplayZoom;
            }
            var root=shop.transform.Find("Reference shop layout");
            var full=root.Find("North wall frieze").GetComponent<SpriteRenderer>();
            var part=root.Find("Storage south wall fade");
            if(!part)
            {
                float west=full.bounds.min.x,east=full.bounds.max.x,split=shop.storageArea.bounds.min.x;
                var copied=UnityEngine.Object.Instantiate(full.gameObject,root);copied.name="Storage south wall fade";part=copied.transform;
                var south=copied.GetComponent<SpriteRenderer>();south.size=new Vector2((east-split)/south.transform.lossyScale.x,south.size.y);
                part.position=new Vector3((east+split)/2,full.transform.position.y,full.transform.position.z);
                full.size=new Vector2((split-west)/full.transform.lossyScale.x,full.size.y);
                full.transform.position=new Vector3((west+split)/2,full.transform.position.y,full.transform.position.z);
            }
            foreach(var shelf in shop.Shelves)
            {
                var fixture=shelf.transform.Find("Artist shelf fixture").GetComponent<SpriteRenderer>();
                var fade=shelf.GetComponent<SoftOccluder>();if(!fade)fade=shelf.gameObject.AddComponent<SoftOccluder>();
                fade.kind=SoftOccluder.OccluderKind.Shelf;fade.silhouette=fixture;
                fade.artwork=new[]{fixture}.Concat(shelf.stockSlots).ToArray();
                fade.minimumOpacity=.22f;EditorUtility.SetDirty(fade);
            }
            foreach(var t in new[]{part,root.Find("Storage door beside opening"),root.Find("North wall end")})
            {
                var r=t.GetComponent<SpriteRenderer>();r.sortingOrder=55;
                var fade=t.GetComponent<SoftOccluder>();if(!fade)fade=t.gameObject.AddComponent<SoftOccluder>();
                fade.kind=SoftOccluder.OccluderKind.SouthWall;fade.silhouette=r;fade.artwork=new[]{r};fade.minimumOpacity=.18f;fade.wallReach=1.05f;
                EditorUtility.SetDirty(fade);EditorUtility.SetDirty(r);
            }
            game.moveAcceleration=5200;game.turnAcceleration=8500;
            UseTest();
        }
        [MenuItem("Gokil Kisah/Camera Test/Use Test Follow Camera")]
        public static void UseTest(){Switch(true);}
        [MenuItem("Gokil Kisah/Camera Test/Use Original Camera")]
        public static void UseOriginal(){Switch(false);}
        static void Switch(bool test)
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode before switching cameras.");
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();var game=UnityEngine.Object.FindFirstObjectByType<StoreGameController>();
            if(!shop||!shop.originalWorldCamera||!shop.testFollowCamera)throw new InvalidOperationException("Set up the camera test first.");
            shop.originalWorldCamera.gameObject.SetActive(!test);shop.testFollowCamera.gameObject.SetActive(test);
            shop.worldCamera=test?shop.testFollowCamera.GetComponent<Camera>():shop.originalWorldCamera;
            game.gameplayZoom=test?shop.testFollowCamera.testGameplayZoom:shop.testFollowCamera.originalGameplayZoom;
            EditorUtility.SetDirty(shop);EditorUtility.SetDirty(game);EditorUtility.SetDirty(shop.testFollowCamera);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
    }
}
