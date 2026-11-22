using System.IO;
using UnityEditor;
using UnityEngine;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>Importa os recursos essenciais do TextMeshPro (fonte padrao e shaders).</summary>
    public static class TmpInstaller
    {
        private const string Pacote = "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";

        public static void Instalar()
        {
            if (!File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"))
            {
                AssetDatabase.ImportPackage(Path.GetFullPath(Pacote), false);
                AssetDatabase.Refresh();
            }

            Debug.Log("[TMP] Essenciais: " + File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"));
        }
    }
}
