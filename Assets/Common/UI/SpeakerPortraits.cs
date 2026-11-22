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
        public const string Generico = "generic";

        private static readonly Dictionary<string, string> PorNome = new Dictionary<string, string>
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

        private static readonly Dictionary<string, Color> Destaques = new Dictionary<string, Color>
        {
            { "refugo", CorHexadecimal(0xffcf80) },
            { "rebite", CorHexadecimal(0xffb27a) },
            { "ferrolho", CorHexadecimal(0xffc870) },
            { "pequena", CorHexadecimal(0x7ae6ff) },
            { "plumber", CorHexadecimal(0xe8c070) },
            { "solda", CorHexadecimal(0xff8a6a) },
            { "crisol", CorHexadecimal(0xbfeaff) },
            { "miner", CorHexadecimal(0xffe0a0) },
            { "miner_f", CorHexadecimal(0xc8ffe0) },
            { "assistant", CorHexadecimal(0x9ff0d8) },
            { "king", CorHexadecimal(0xe8f2ff) },
            { "king_corrupt", CorHexadecimal(0x78ffaa) },
        };

        private static readonly Color DestaquePadrao = new Color(0.95f, 0.80f, 0.45f);

        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();

        /// <summary>O id do retrato pelo nome de quem fala (sem retrato proprio: o generico).</summary>
        public static string IdDe(string falante)
        {
            if (string.IsNullOrEmpty(falante)) return Generico;
            return PorNome.TryGetValue(falante.Trim().ToLowerInvariant(), out string id) ? id : Generico;
        }

        /// <summary>O quadro do retrato (aberto ou piscando), ou null se o PNG nao existe.</summary>
        public static Texture2D Obter(string id, bool piscar)
        {
            if (string.IsNullOrEmpty(id)) return null;

            string chave = piscar ? id + "_blink" : id;
            if (Cache.TryGetValue(chave, out Texture2D textura) && textura != null) return textura;

            textura = Resources.Load<Texture2D>("Portraits/" + chave);
            if (textura == null && piscar) textura = Obter(id, false);
            if (textura == null && id != Generico) textura = Obter(Generico, piscar);

            Cache[chave] = textura;
            return textura;
        }

        /// <summary>A cor do nome e das cantoneiras da caixa pra quem fala.</summary>
        public static Color CorDeDestaque(string id)
        {
            return id != null && Destaques.TryGetValue(id, out Color cor) ? cor : DestaquePadrao;
        }

        private static Color CorHexadecimal(int rgb)
        {
            return new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f);
        }
    }
}
