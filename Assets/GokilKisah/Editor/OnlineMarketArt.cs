using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
namespace GokilKisah.Editor
{
    public static class OnlineMarketArt
    {
        const string Output="Assets/GokilKisah/Resources/OnlineArt/";
        public static void Bake()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode before baking art.");
            var scene=EditorSceneManager.NewPreviewScene();var root=new GameObject("Asset bake camera");SceneManager.MoveGameObjectToScene(root,scene);
            var camera=root.AddComponent<Camera>();camera.scene=scene;camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.nearClipPlane=.01f;camera.farClipPlane=100;camera.enabled=false;
            var rt=new RenderTexture(512,512,24,RenderTextureFormat.ARGB32){antiAliasing=4};camera.targetTexture=rt;
            var material=new Material(Shader.Find("TokMart/AssetBake"));material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/GokilKisah/ThirdParty/KenneyMiniMarket/Textures/colormap.png");
            var pipeline=GraphicsSettings.defaultRenderPipeline;var quality=QualitySettings.renderPipeline;
            try
            {
                GraphicsSettings.defaultRenderPipeline=null;QualitySettings.renderPipeline=null;
                var items=new[]{new[]{"ShelfStock","shelf-boxes"},new[]{"ShelfBags","shelf-bags"},new[]{"ShelfEmpty","shelf-end"},new[]{"Checkout","cash-register"},new[]{"Crate","shopping-basket"},new[]{"Wall","wall"},new[]{"Door","wall-door-rotate"},new[]{"Fruit","display-fruit"},new[]{"Bread","display-bread"},new[]{"Fridge","freezers-standing"},new[]{"Gun","@blaster-a"}};
                foreach(var item in items)
                {
                    bool blaster=item[1].StartsWith("@");
                    string source=blaster?"Assets/GokilKisah/ThirdParty/KenneyBlasterKit/"+item[1].Substring(1)+".fbx":"Assets/GokilKisah/ThirdParty/KenneyMiniMarket/"+item[1]+".fbx";
                    material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(blaster?"Assets/GokilKisah/ThirdParty/KenneyBlasterKit/Textures/colormap.png":"Assets/GokilKisah/ThirdParty/KenneyMiniMarket/Textures/colormap.png");
                    var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(source);if(!prefab)throw new InvalidOperationException("Missing model "+item[1]);
                    var model=UnityEngine.Object.Instantiate(prefab);SceneManager.MoveGameObjectToScene(model,scene);model.transform.position=Vector3.zero;if(blaster)model.transform.rotation=Quaternion.Euler(0,90,0);
                    var renderers=model.GetComponentsInChildren<Renderer>();Bounds bounds=renderers[0].bounds;
                    foreach(var renderer in renderers){var mats=renderer.sharedMaterials;for(int i=0;i<mats.Length;i++)mats[i]=material;renderer.sharedMaterials=mats;bounds.Encapsulate(renderer.bounds);}
                    camera.transform.position=bounds.center+new Vector3(0,8,-12);camera.transform.LookAt(bounds.center);
                    float extent=0;
                    for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2){var point=bounds.center+Vector3.Scale(bounds.extents,new Vector3(x,y,z));var local=camera.transform.InverseTransformPoint(point);extent=Mathf.Max(extent,Mathf.Abs(local.x),Mathf.Abs(local.y));}
                    camera.orthographicSize=Mathf.Max(.1f,extent*1.1f);camera.Render();
                    var previous=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(512,512,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,512,512),0,0);image.Apply();RenderTexture.active=previous;
                    File.WriteAllBytes(Output+item[0]+".png",image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(model);
                }
            }
            finally{GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=quality;UnityEngine.Object.DestroyImmediate(material);UnityEngine.Object.DestroyImmediate(rt);EditorSceneManager.ClosePreviewScene(scene);}
            AssetDatabase.Refresh();
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{Output.TrimEnd('/')}))
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);importer.spritePixelsPerUnit=256;importer.filterMode=FilterMode.Bilinear;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.alphaIsTransparency=true;importer.SaveAndReimport();
            }
            Debug.Log("Kenney downloaded non-pixel artwork baked and imported.");
        }
        public static void Apply()
        {
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();if(!shop)throw new InvalidOperationException("Open the store scene.");
            Undo.RegisterFullObjectHierarchyUndo(shop.gameObject,"Use downloaded Kenney artwork");
            foreach(var renderer in shop.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if(renderer.name=="Car bottom"){renderer.gameObject.SetActive(false);continue;}
                string path=AssetDatabase.GetAssetPath(renderer.sprite);string name=Path.GetFileNameWithoutExtension(path);if(!path.Contains("/SmoothArt/"))continue;
                var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Output+name+".png");if(!sprite)continue;
                var size=renderer.bounds.size;if(renderer.name=="Car top"){size=new Vector3(.75f,1.28f,0);renderer.transform.position+=Vector3.down*.32f;}renderer.sprite=sprite;if(name=="Spill")renderer.color=new Color(.3f,.68f,.85f,.5f);var parent=renderer.transform.parent?renderer.transform.parent.lossyScale:Vector3.one;
                renderer.transform.localScale=new Vector3(size.x/sprite.bounds.size.x/parent.x,size.y/sprite.bounds.size.y/parent.y,1);
            }
            foreach(var shelf in shop.Shelves)
            {
                shelf.stockedSprite=AssetDatabase.LoadAssetAtPath<Sprite>(Output+(shelf.name.Contains("Middle")?"ShelfBags":"ShelfStock")+".png");shelf.emptySprite=AssetDatabase.LoadAssetAtPath<Sprite>(Output+"ShelfEmpty.png");shelf.ShowStock(shelf.initialStock==0?1:0);shelf.ShowStock(shelf.initialStock);EditorUtility.SetDirty(shelf);
            }
            foreach(var actor in shop.GetComponentsInChildren<CharacterSpriteAnimator>(true))EditorUtility.SetDirty(actor);
            var audit=new System.Collections.Generic.List<string>();foreach(var renderer in shop.GetComponentsInChildren<SpriteRenderer>(true))if(renderer.gameObject.activeInHierarchy||renderer.GetComponentInParent<CharacterSpriteAnimator>(true))audit.Add(renderer.name+" | "+AssetDatabase.GetAssetPath(renderer.sprite));
            Directory.CreateDirectory("Verification");File.WriteAllLines("Verification/art-source-audit.txt",audit);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
        }
    }
}

