using UnityEditor;

namespace GokilKisah.Editor
{
    public sealed class ArtistAssetImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.Contains("GokilKisah/Resources/ArtistArt/"))return;
            var importer=(TextureImporter)assetImporter;
            importer.textureType=TextureImporterType.Default;
            importer.filterMode=UnityEngine.FilterMode.Point;
            importer.wrapMode=UnityEngine.TextureWrapMode.Clamp;
            importer.mipmapEnabled=false;
            importer.npotScale=TextureImporterNPOTScale.None;
            importer.alphaIsTransparency=true;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.isReadable=true;
            importer.maxTextureSize=2048;
        }
    }
}
