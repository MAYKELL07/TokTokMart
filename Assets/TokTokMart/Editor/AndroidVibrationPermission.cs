using System.IO;
using System.Xml;
using UnityEditor.Android;

namespace TokTokMart.Editor
{
    public sealed class AndroidVibrationPermission : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 20;
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string file = Path.Combine(path, "src/main/AndroidManifest.xml");
            var doc = new XmlDocument(); doc.Load(file); var manifest = doc.DocumentElement;
            const string ns = "http://schemas.android.com/apk/res/android";
            foreach (XmlNode node in manifest.ChildNodes)
                if (node.Name == "uses-permission" && node.Attributes?["name", ns]?.Value == "android.permission.VIBRATE") return;
            var permission = doc.CreateElement("uses-permission"); permission.SetAttribute("name", ns, "android.permission.VIBRATE"); manifest.AppendChild(permission); doc.Save(file);
        }
    }
}
