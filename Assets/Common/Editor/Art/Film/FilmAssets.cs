using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Grava os quadros de um filme (FilmDraft) como PNG em
    /// Assets/Generated/Film/{nome}_{i}.png — legiveis, sem compressao, com
    /// filtro de pixel — e entrega o filme pronto pro FilmCutscene.
    /// </summary>
    public static class FilmAssets
    {
        public const string Folder = Phase01Assets.GeneratedRoot + "/Film";

        private static string PathOf(string set, int index) => $"{Folder}/{set}_{index:D3}.png";

        /// <summary>Grava, importa e configura o tocador com o filme.</summary>
        public static void Build(FilmCutscene player, string set, FilmDraft film, bool endWhite)
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder(Phase01Assets.GeneratedRoot, "Film");

            // Os quadros velhos deste filme saem (o filme pode ter encolhido).
            foreach (string old in Directory.GetFiles(Folder, set + "_*.png"))
            {
                AssetDatabase.DeleteAsset(old.Replace('\\', '/'));
            }

            for (int i = 0; i < film.Frames.Count; i++) WritePng(PathOf(set, i), film.Frames[i]);
            AssetDatabase.Refresh();

            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < film.Frames.Count; i++)
                {
                    var importer = AssetImporter.GetAtPath(PathOf(set, i)) as TextureImporter;
                    if (importer == null) continue;
                    importer.textureType = TextureImporterType.Default;
                    importer.isReadable = true;
                    importer.filterMode = FilterMode.Point;
                    importer.mipmapEnabled = false;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.npotScale = TextureImporterNPOTScale.None;
                    importer.wrapMode = TextureWrapMode.Clamp;
                    importer.alphaIsTransparency = true;
                    importer.sRGBTexture = true;
                    importer.maxTextureSize = 2048;
                    importer.SaveAndReimport();
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            var textures = new List<Texture2D>(film.Frames.Count);
            for (int i = 0; i < film.Frames.Count; i++)
            {
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(PathOf(set, i));
                if (texture == null) Debug.LogWarning($"[Film] Quadro {PathOf(set, i)} nao foi importado.");
                textures.Add(texture);
            }

            player.Configure(film.Shots.ToArray(), textures.ToArray(), endWhite);
        }

        private static void WritePng(string path, PixelCanvas canvas)
        {
            var texture = new Texture2D(canvas.Width, canvas.Height, TextureFormat.RGBA32, false);
            var pixels = new Color32[canvas.Width * canvas.Height];
            for (int y = 0; y < canvas.Height; y++)
            {
                for (int x = 0; x < canvas.Width; x++)
                {
                    Rgba p = canvas.Pixels[x + y * canvas.Width];
                    pixels[x + (canvas.Height - 1 - y) * canvas.Width] = new Color32(p.R, p.G, p.B, p.A);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false);
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }
    }
}
