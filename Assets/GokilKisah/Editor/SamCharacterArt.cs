using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GokilKisah.Editor
{
    public static class SamCharacterArt
    {
        const string TexturePath="Assets/GokilKisah/Resources/CharacterArt/Art_TTM_Char_Sam01.png";
        const string SpritePath="Assets/GokilKisah/Resources/PresentationSprites/SamStatic.asset";
        [MenuItem("Gokil Kisah/Apply Supplied Sam Character")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            AssetDatabase.ImportAsset(TexturePath,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(TexturePath);
            importer.textureType=TextureImporterType.Default;importer.isReadable=true;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
            importer.npotScale=TextureImporterNPOTScale.None;importer.filterMode=FilterMode.Bilinear;
            importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);var pixels=texture.GetPixels32();
            int left=texture.width,right=-1,bottom=texture.height,top=-1;
            for(int y=0;y<texture.height;y++)for(int x=0;x<texture.width;x++)if(pixels[y*texture.width+x].a>8)
            {left=Math.Min(left,x);right=Math.Max(right,x);bottom=Math.Min(bottom,y);top=Math.Max(top,y);}
            if(right<left)throw new InvalidOperationException("Sam image contains no visible pixels.");
            // Match Sam's existing intrinsic height so scene scale, collisions and held items stay aligned.
            var original=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GokilKisah/Resources/PresentationSprites/Sam_3_0.asset");
            var rect=new Rect(left,bottom,right-left+1,top-bottom+1);
            var created=Sprite.Create(texture,rect,new Vector2(.5f,0),rect.height/original.bounds.size.y,0,SpriteMeshType.FullRect);
            created.name="SamStatic";
            var saved=AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            if(saved){EditorUtility.CopySerialized(created,saved);UnityEngine.Object.DestroyImmediate(created);}
            else {AssetDatabase.CreateAsset(created,SpritePath);saved=created;}
            var shop=UnityEngine.Object.FindFirstObjectByType<ShopLayout>();
            Undo.RecordObject(shop.playerCharacter.artwork,"Apply Sam character artwork");
            shop.playerCharacter.artwork.sprite=saved;EditorUtility.SetDirty(shop.playerCharacter.artwork);
            EditorSceneManager.MarkSceneDirty(shop.gameObject.scene);EditorSceneManager.SaveScene(shop.gameObject.scene);AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Verification/SamStill");
            File.WriteAllText("Verification/SamStill/import.txt","Supplied image: "+TexturePath+"\nVisible crop: "+rect+"\nFoot pivot: "+saved.pivot+"\nIntrinsic height: "+saved.bounds.size.y+"\nZack assets unchanged.\n");
        }
    }
}
