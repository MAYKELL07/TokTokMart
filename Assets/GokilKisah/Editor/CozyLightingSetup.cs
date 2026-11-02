using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GokilKisah.Editor
{
    public static class CozyLightingSetup
    {
        const string ProfilePath="Assets/GokilKisah/CozyShopVolume.asset";
        static Transform Child(Transform parent,string name)
        {
            var t=parent.Find(name);if(t)return t;
            t=new GameObject(name).transform;t.SetParent(parent,false);return t;
        }
        static Light2D Light(Transform parent,string name,Vector2 position,Color color,float intensity,float radius)
        {
            var t=Child(parent,name);t.position=position;
            var light=t.GetComponent<Light2D>();if(!light)light=t.gameObject.AddComponent<Light2D>();
            light.lightType=radius==0?Light2D.LightType.Global:Light2D.LightType.Point;
            light.color=color;light.intensity=intensity;light.blendStyleIndex=0;
            if(radius>0){light.pointLightInnerRadius=.7f;light.pointLightOuterRadius=radius;light.falloffIntensity=.55f;}
            EditorUtility.SetDirty(light);return light;
        }
        static T Effect<T>(VolumeProfile profile) where T:VolumeComponent
        {if(profile.TryGet<T>(out var effect))return effect;effect=profile.Add<T>(true);AssetDatabase.AddObjectToAsset(effect,profile);return effect;}
        [MenuItem("Gokil Kisah/Apply Cozy 2D Lighting")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();
            if(!shop)throw new InvalidOperationException("Open the shop scene first.");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(shop.gameObject.scene.path,true)};
            Undo.RegisterFullObjectHierarchyUndo(shop.gameObject,"Cozy shop lighting");
            var root=Child(shop.transform,"Cozy shop lighting");
            var controller=root.GetComponent<CozyShopLighting>();if(!controller)controller=root.gameObject.AddComponent<CozyShopLighting>();
            controller.ambient=Light(root,"Soft sky ambient",Vector2.zero,new Color(.91f,.95f,1),.8f,0);
            var pools=new List<Light2D>();
            foreach(float y in new[]{-3f,-8f})foreach(float x in new[]{4.5f,10f,15.5f})
                pools.Add(Light(root,"Warm ceiling pool "+x+" "+y,new Vector2(x,y),new Color(1,.93f,.8f),.28f,4.7f));
            pools.Add(Light(root,"Checkout task light",new Vector2(18.2f,-5.8f),new Color(1,.94f,.85f),.28f,3.2f));
            controller.shopLights=pools.ToArray();
            Light(root,"Storage cool fill",new Vector2(16.8f,1.35f),new Color(.83f,.94f,1),.26f,3.4f);
            var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(ProfilePath);
            if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,ProfilePath);}
            var vignette=Effect<Vignette>(profile);vignette.intensity.Override(.18f);vignette.smoothness.Override(.46f);
            vignette.color.Override(new Color(.055f,.09f,.12f));vignette.rounded.Override(true);
            var grade=Effect<ColorAdjustments>(profile);grade.postExposure.Override(.05f);grade.contrast.Override(7);grade.saturation.Override(5);
            var balance=Effect<WhiteBalance>(profile);balance.temperature.Override(5);balance.tint.Override(0);
            var bloom=Effect<Bloom>(profile);bloom.threshold.Override(1.12f);bloom.intensity.Override(.12f);bloom.scatter.Override(.35f);bloom.highQualityFiltering.Override(false);
            var volume=root.GetComponent<Volume>();if(!volume)volume=root.gameObject.AddComponent<Volume>();
            volume.isGlobal=true;volume.priority=20;volume.sharedProfile=profile;volume.weight=1;
            foreach(var camera in new[]{shop.worldCamera,shop.originalWorldCamera})if(camera)
            {
                Undo.RecordObject(camera,"Enable shop post processing");camera.allowHDR=true;
                var data=camera.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;
                data.antialiasing=AntialiasingMode.FastApproximateAntialiasing;EditorUtility.SetDirty(data);EditorUtility.SetDirty(camera);
            }
            foreach(var c in profile.components)EditorUtility.SetDirty(c);
            EditorUtility.SetDirty(profile);EditorUtility.SetDirty(controller);EditorUtility.SetDirty(volume);
            EditorSceneManager.MarkSceneDirty(shop.gameObject.scene);EditorSceneManager.SaveScene(shop.gameObject.scene);AssetDatabase.SaveAssets();
            Debug.Log("Cozy 2D look saved: warm light pools, cool fill, soft vignette, light bloom and crisp HUD.");
        }
    }
}
