using System.IO;
using UnityEditor;
using UnityEngine;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Os icones do HUD (vida, fragmento, gema). Ficam em Resources: o GameHud
    /// os carrega sozinho, sem cena nenhuma pra regerar.
    ///
    /// - O import deixa cada PNG nitido (sem filtro, sem compressao, sem mipmap):
    ///   desenhado em dobro na tela, cada pixel vira um quadrado limpo.
    /// - O menu redesenha os PNGs a partir do <see cref="HudIconArt"/>.
    /// </summary>
    public class HudIconAssets : AssetPostprocessor
    {
        public const string Folder = "Assets/Common/UI/Resources/HudIcons";

        /// <summary>A arte da tela de morte: mesma regra de importacao (pixel nitido).</summary>
        public const string DeathFolder = "Assets/Common/UI/Resources/DeathScreen";

        /// <summary>A arte do menu principal: mesma regra.</summary>
        public const string MenuFolder = "Assets/Common/UI/Resources/MainMenu";

        /// <summary>A arte da tela de pausa (e o "jogo salvo" do canto): mesma regra.</summary>
        public const string PauseFolder = "Assets/Common/UI/Resources/PauseScreen";

        /// <summary>Os bustos da caixa de dialogo (PortraitArt): mesma regra.</summary>
        public const string PortraitFolder = "Assets/Common/UI/Resources/Portraits";

        /// <summary>A tela de fim de cada final (EndScreen): mesma regra.</summary>
        public const string EndFolder = "Assets/Common/UI/Resources/EndScreen";

        public override uint GetVersion() => 4;

        /// <summary>
        /// Desenha a tela de fim de um final: {id}_emblem, {id}_title,
        /// {id}_line0.., e as pecas comuns (friso, credito, placa de voltar).
        /// </summary>
        public static void RedrawEnd(string id, PixelCanvas emblem, string title, string[] lines)
        {
            Directory.CreateDirectory(EndFolder);
            Write(id + "_emblem", emblem, EndFolder);
            Write(id + "_title", DeathScreenArt.Title(title, 3, false), EndFolder);
            for (int i = 0; i < lines.Length; i++) Write($"{id}_line{i}", EndScreenArt.Line(lines[i]), EndFolder);

            Write("divider", DeathScreenArt.Divider(), EndFolder);
            Write("credit", EndScreenArt.Line(EndScreenArt.Credit, true), EndFolder);
            Write("button_menu_off", DeathScreenArt.Button(EndScreenArt.MenuLabel, false), EndFolder);
            Write("button_menu_on", DeathScreenArt.Button(EndScreenArt.MenuLabel, true), EndFolder);
            AssetDatabase.Refresh();
        }

        // A pausa e nova: se a pasta ainda nao existe, desenha sozinha ao abrir o Editor.
        [InitializeOnLoadMethod]
        private static void DrawPauseIfMissing()
        {
            if (!Directory.Exists(PauseFolder))
                EditorApplication.delayCall += () => { if (!Directory.Exists(PauseFolder)) RedrawPause(); };

            // Os icones do inventario: se faltam, redesenha os icones do HUD.
            if (!File.Exists(Folder + "/item_key.png"))
                EditorApplication.delayCall += () => { if (!File.Exists(Folder + "/item_key.png")) Redraw(); };

            // Os bustos tambem: se a pasta nao existe, desenha sozinha.
            if (!Directory.Exists(PortraitFolder))
                EditorApplication.delayCall += () => { if (!Directory.Exists(PortraitFolder)) RedrawPortraits(); };
        }

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Folder + "/") && !assetPath.StartsWith(DeathFolder + "/") && !assetPath.StartsWith(MenuFolder + "/")
                && !assetPath.StartsWith(PauseFolder + "/") && !assetPath.StartsWith(PortraitFolder + "/")
                && !assetPath.StartsWith(EndFolder + "/")) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;

            // A loja centraliza os icones pela parte desenhada (GameHud.DrawCentered): precisa ler os pixels.
            importer.isReadable = true;
        }

        [MenuItem("Echoes/Redesenhar os ícones do HUD", false, 91)]
        public static void Redraw()
        {
            Directory.CreateDirectory(Folder);

            Write("heart_full", HudIconArt.Heart(true));
            Write("heart_empty", HudIconArt.Heart(false));
            Write("shard", HudIconArt.Nugget());
            Write("gem", HudIconArt.RedGem());
            foreach (string item in ItemIconArt.Names) Write("item_" + item, ItemIconArt.Draw(item));

            AssetDatabase.Refresh();
            Debug.Log($"[HUD] Ícones redesenhados em {Folder}.");
        }

        [MenuItem("Echoes/Redesenhar a tela de morte", false, 92)]
        public static void RedrawDeathScreen()
        {
            Directory.CreateDirectory(DeathFolder);

            PixelCanvas[] hearts = DeathScreenArt.HeartFrames();
            for (int i = 0; i < hearts.Length; i++) Write($"heart_{i}", hearts[i], DeathFolder);

            Write("title", DeathScreenArt.Title(), DeathFolder);
            Write("button_back_on", DeathScreenArt.Button(DeathScreenArt.ButtonLabels[0], true), DeathFolder);
            Write("button_back_off", DeathScreenArt.Button(DeathScreenArt.ButtonLabels[0], false), DeathFolder);
            Write("button_quit_on", DeathScreenArt.Button(DeathScreenArt.ButtonLabels[1], true), DeathFolder);
            Write("button_quit_off", DeathScreenArt.Button(DeathScreenArt.ButtonLabels[1], false), DeathFolder);
            Write("divider", DeathScreenArt.Divider(), DeathFolder);

            AssetDatabase.Refresh();
            Debug.Log($"[Morte] Arte redesenhada em {DeathFolder}.");
        }

        [MenuItem("Echoes/Redesenhar o menu principal", false, 93)]
        public static void RedrawMenu()
        {
            Directory.CreateDirectory(MenuFolder);

            Write("title_small", DeathScreenArt.MenuTitleSmall(), MenuFolder);
            Write("title_big", DeathScreenArt.MenuTitleBig(), MenuFolder);
            Write("divider", DeathScreenArt.Divider(), MenuFolder);

            string[] names = { "continue", "new", "quit" };
            for (int i = 0; i < names.Length; i++)
            {
                Write($"button_{names[i]}_off", DeathScreenArt.Button(DeathScreenArt.MenuLabels[i], false), MenuFolder);
                Write($"button_{names[i]}_on", DeathScreenArt.Button(DeathScreenArt.MenuLabels[i], true), MenuFolder);
            }

            AssetDatabase.Refresh();
            Debug.Log($"[Menu] Arte redesenhada em {MenuFolder}.");
        }

        [MenuItem("Echoes/Redesenhar a tela de pausa", false, 94)]
        public static void RedrawPause()
        {
            Directory.CreateDirectory(PauseFolder);

            Write("title", DeathScreenArt.Title("PAUSA", 3, false), PauseFolder);
            Write("divider", DeathScreenArt.Divider(), PauseFolder);
            Write("notice_title", DeathScreenArt.Title("JOGO SALVO", 2, false), PauseFolder);
            Write("notice_line", DeathScreenArt.Title("NA ÚLTIMA BANCADA", 1, false), PauseFolder);
            Write("saved", DeathScreenArt.Title("JOGO SALVO", 1, false), PauseFolder);

            (string file, string label)[] buttons =
            {
                ("continue", "CONTINUAR"), ("quit", "SAIR"), ("menu", "IR PARA O MENU"), ("stay", "VOLTAR AO JOGO"),
            };

            foreach ((string file, string label) in buttons)
            {
                Write($"button_{file}_off", DeathScreenArt.Button(label, false), PauseFolder);
                Write($"button_{file}_on", DeathScreenArt.Button(label, true), PauseFolder);
            }

            AssetDatabase.Refresh();
            Debug.Log($"[Pausa] Arte desenhada em {PauseFolder}.");
        }

        [MenuItem("Echoes/Redesenhar os retratos de diálogo", false, 95)]
        public static void RedrawPortraits()
        {
            Directory.CreateDirectory(PortraitFolder);

            foreach (string id in PortraitArt.Ids)
            {
                Write(id, PortraitArt.Draw(id, false), PortraitFolder);
                Write(id + "_blink", PortraitArt.Draw(id, true), PortraitFolder);
            }

            AssetDatabase.Refresh();
            Debug.Log($"[Retratos] Bustos desenhados em {PortraitFolder}.");
        }

        private static void Write(string name, PixelCanvas canvas, string folder = Folder)
        {
            var texture = new Texture2D(canvas.Width, canvas.Height, TextureFormat.RGBA32, false);

            // O canvas e de cima pra baixo; a textura, de baixo pra cima.
            var pixels = new Color32[canvas.Width * canvas.Height];
            for (int y = 0; y < canvas.Height; y++)
            {
                for (int x = 0; x < canvas.Width; x++)
                {
                    Rgba p = canvas.Get(x, y);
                    pixels[x + (canvas.Height - 1 - y) * canvas.Width] = new Color32(p.R, p.G, p.B, p.A);
                }
            }

            texture.SetPixels32(pixels);
            File.WriteAllBytes($"{folder}/{name}.png", texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }
    }
}
