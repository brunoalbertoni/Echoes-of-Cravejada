using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Os icones 12x12 dos itens da loja do Crisol, desenhados em codigo (uma
    /// letra por pixel) e guardados depois do primeiro uso.
    /// </summary>
    public static class ShopIcons
    {
        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();

        private static readonly Dictionary<char, Color32> Paleta = new Dictionary<char, Color32>
        {
            { 'k', new Color32(28, 20, 22, 255) },
            { 'S', new Color32(222, 226, 232, 255) },
            { 's', new Color32(150, 156, 170, 255) },
            { 'G', new Color32(255, 222, 130, 255) },
            { 'g', new Color32(206, 150, 64, 255) },
            { 'B', new Color32(176, 120, 76, 255) },
            { 'b', new Color32(120, 76, 48, 255) },
            { 'r', new Color32(214, 64, 60, 255) },
            { 'c', new Color32(86, 190, 220, 255) },
            { 'C', new Color32(190, 240, 255, 255) },
        };

        private static readonly Dictionary<string, string[]> Mapas = new Dictionary<string, string[]>
        {
            {
                Shop.Chassi, new[]
                {
                    "kkkkkkkkkkkk",
                    "kSSSSSSSSSSk",
                    "kSkssssssksk",
                    "kSssssrrsssk",
                    "kSssssrrsssk",
                    "kSssrrrrrrsk",
                    "kSssrrrrrrsk",
                    "kSssssrrsssk",
                    "kSssssrrsssk",
                    "kSkssssssksk",
                    "kssssssssssk",
                    "kkkkkkkkkkkk",
                }
            },
            {
                Shop.Pistoes, new[]
                {
                    "..kk....kk..",
                    "..kk....kk..",
                    ".kSSk..kSSk.",
                    ".kSsk..kSsk.",
                    "kkkkkkkkkkkk",
                    "kSSSskkSSSsk",
                    "kSssskkSsssk",
                    "kSssskkSsssk",
                    "kSssskkSsssk",
                    "kbbbbkkbbbbk",
                    "kkkkkkkkkkkk",
                    "............",
                }
            },
            {
                Shop.BolsaForrado, new[]
                {
                    "....kkkk....",
                    "...kbkkbk...",
                    "...kk..kk...",
                    "..kkkkkkkk..",
                    ".kBBBBBBBBk.",
                    "kBBbBBBBbBBk",
                    "kBbbbbbbbbBk",
                    "kBbbcbbcbbBk",
                    "kBbbbbbbbbBk",
                    "kBbbbbbbbbbk",
                    ".kbbbbbbbbk.",
                    "..kkkkkkkk..",
                }
            },
            {
                Shop.EngasteOuro, new[]
                {
                    "....kkkk....",
                    "...kcCCck...",
                    "...kcccck...",
                    "....kkkk....",
                    "...kGGGGk...",
                    "..kGgkkgGk..",
                    ".kGgk..kgGk.",
                    ".kGk....kGk.",
                    ".kGk....kgk.",
                    "..kgk..kgk..",
                    "...kggggk...",
                    "....kkkk....",
                }
            },
            {
                Shop.Colheita, new[]
                {
                    "..........kk",
                    ".........kSk",
                    "........kSsk",
                    ".......kSsk.",
                    "......kSsk..",
                    ".....kSsk...",
                    "....kkkk....",
                    "...kBbk.....",
                    "..kBbk......",
                    ".kBbk.......",
                    "kBbk........",
                    "kkk.........",
                }
            },
            {
                Shop.Memoria, new[]
                {
                    "............",
                    "............",
                    "k...k..k...k",
                    "kk.kGkkGk.kk",
                    "kGkGgGGgGkGk",
                    "kGGgGrGGgGGk",
                    "kgGGGGGGGGgk",
                    "kgggcggrgggk",
                    "kGGGGGGGGGGk",
                    "kkkkkkkkkkkk",
                    "............",
                    "............",
                }
            },
        };

        /// <summary>O icone do item, ou null se ele nao tiver um.</summary>
        public static Texture2D Obter(string id)
        {
            if (id == null) return null;
            if (Cache.TryGetValue(id, out Texture2D guardado)) return guardado;
            if (!Mapas.TryGetValue(id, out string[] linhas)) return Cache[id] = null;

            var textura = new Texture2D(12, 12, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };

            for (int y = 0; y < 12; y++)
                for (int x = 0; x < 12; x++)
                {
                    char c = x < linhas[y].Length ? linhas[y][x] : '.';
                    // As linhas vem de cima pra baixo; a textura, de baixo pra cima.
                    textura.SetPixel(x, 11 - y, Paleta.TryGetValue(c, out Color32 cor) ? (Color)cor : Color.clear);
                }

            textura.Apply();
            return Cache[id] = textura;
        }
    }
}
