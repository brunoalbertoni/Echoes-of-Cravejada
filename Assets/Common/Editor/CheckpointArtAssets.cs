using System.IO;
using UnityEditor;
using UnityEngine;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Os quadros da Bancada de Ourives (o checkpoint). Ficam em Resources,
    /// porque a <see cref="Workbench"/> os carrega sozinha ao abrir a cena: a
    /// bancada muda de cara em todas as fases sem regerar nenhuma.
    ///
    /// - O import acerta cada PNG: 32 px por tile, sem filtro, pivo no canto
    ///   de baixo a esquerda (como a bancada antiga).
    /// - O menu redesenha os PNGs a partir do <see cref="CheckpointArt"/>.
    /// </summary>
    public class CheckpointArtAssets : AssetPostprocessor
    {
        public const string Folder = "Assets/Common/Mechanics/Resources/Checkpoint";

        public override uint GetVersion() => 1;

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Folder + "/")) return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = CheckpointArt.PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = Vector2.zero;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
        }

        public static void Redraw()
        {
            Directory.CreateDirectory(Folder);

            PixelCanvas[] frames = CheckpointArt.Frames();
            for (int i = 0; i < frames.Length; i++)
            {
                PixelCanvas canvas = frames[i];
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
                File.WriteAllBytes($"{Folder}/checkpoint_{i:00}.png", texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
            }

            AssetDatabase.Refresh();
            Debug.Log($"[Bancada] {frames.Length} quadros redesenhados em {Folder}.");
        }
    }
}
