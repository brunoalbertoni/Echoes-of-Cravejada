using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Os retratos de busto de quem fala (Resources/Portraits, desenhados pelo
    /// PortraitArt): o id sai do nome de quem fala, e cada id tem o quadro de
    /// olhos abertos, o da piscada e a cor do nome na caixa. Quem nao tem
    /// retrato proprio usa o "generic".
    /// </summary>
    public static class SpeakerPortraits
    {
        public const string Generic = "generic";

        private static readonly Dictionary<string, string> ByName = new Dictionary<string, string>
        {
            { "refugo", "refugo" },
            { "rebite", "rebite" },
            { "ferrolho", "ferrolho" },
            { "pequena", "pequena" },
            { "o velho encanador", "plumber" },
            { "velho encanador", "plumber" },
            { "solda", "solda" },
            { "crisol", "crisol" },
            { "mineiro", "miner" },
            { "mineira", "miner_f" },
            { "assistente", "assistant" },
            { "o rei de prata", "king" },
        };

        private static readonly Dictionary<string, Color> Accents = new Dictionary<string, Color>
        {
            { "refugo", Hex(0xffcf80) },
            { "rebite", Hex(0xffb27a) },
            { "ferrolho", Hex(0xffc870) },
            { "pequena", Hex(0x7ae6ff) },
            { "plumber", Hex(0xe8c070) },
            { "solda", Hex(0xff8a6a) },
            { "crisol", Hex(0xbfeaff) },
            { "miner", Hex(0xffe0a0) },
            { "miner_f", Hex(0xc8ffe0) },
            { "assistant", Hex(0x9ff0d8) },
            { "king", Hex(0xe8f2ff) },
            { "king_corrupt", Hex(0x78ffaa) },
        };

        private static readonly Color DefaultAccent = new Color(0.95f, 0.80f, 0.45f);

        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();

        /// <summary>O id do retrato pelo nome de quem fala (sem retrato proprio: o generico).</summary>
        public static string IdFor(string speaker)
        {
            if (string.IsNullOrEmpty(speaker)) return Generic;
            return ByName.TryGetValue(speaker.Trim().ToLowerInvariant(), out string id) ? id : Generic;
        }

        /// <summary>O quadro do retrato (aberto ou piscando), ou null se o PNG nao existe.</summary>
        public static Texture2D Get(string id, bool blink)
        {
            if (string.IsNullOrEmpty(id)) return null;

            string key = blink ? id + "_blink" : id;
            if (Cache.TryGetValue(key, out Texture2D texture) && texture != null) return texture;

            texture = Resources.Load<Texture2D>("Portraits/" + key);
            if (texture == null && blink) texture = Get(id, false);
            if (texture == null && id != Generic) texture = Get(Generic, blink);

            Cache[key] = texture;
            return texture;
        }

        /// <summary>A cor do nome e das cantoneiras da caixa pra quem fala.</summary>
        public static Color Accent(string id)
        {
            return id != null && Accents.TryGetValue(id, out Color color) ? color : DefaultAccent;
        }

        private static Color Hex(int rgb)
        {
            return new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f);
        }
    }
}
