using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O inventario do HUD: so os icones do que o Scrap tem na mao pra usar
    /// ou entregar (ferramentas, motor, chave, reagentes, rubi bruto). Arte em Resources/HudIcons/item_* (ItemIconArt).
    /// </summary>
    public static class InventoryIcons
    {
        public struct Entry
        {
            public Texture2D Icone2;
            public int Quantidade;
            public string Nome;
            public string Uso;
        }

        // O id (ou comeco de id) no KeyItems e o icone de cada um.
        // Nome e uso aparecem ao passar o mouse na casinha.
        private static readonly (string Id, string Icon, bool Prefix, string Name, string Use)[] Itens =
        {
            ("Tool_Graver", "graver", false, "Buril", "Entregar ao Rebite"),
            ("Tool_Loupe", "loupe", false, "Lupa", "Entregar ao Rebite"),
            ("Tool_Tweezers", "tweezers", false, "Pinça", "Entregar ao Rebite"),
            ("Motor_Collector", "motor", false, "Motor a vapor", "Entregar ao Ferrolho"),
            ("D3_AncientKey", "key", false, "Chave antiga", "Abre a porta trancada"),
            ("D4_Reagent_Mercury", "mercury", false, "Mercúrio destilado", "Levar ao Grande Alambique"),
            ("D4_Reagent_Light", "light", false, "Luz engarrafada", "Levar ao Grande Alambique"),
            ("D4_Reagent_Salt", "salt", false, "Sal de quartzo", "Levar ao Grande Alambique"),
        };

        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();
        private static readonly List<Entry> Segurado = new List<Entry>();

        private static Texture2D Carregar(string nome)
        {
            if (!Cache.TryGetValue(nome, out Texture2D icone))
            {
                icone = Resources.Load<Texture2D>("HudIcons/item_" + nome);
                Cache[nome] = icone;
            }

            return icone;
        }

        /// <summary>O que ele tem na mao agora, na ordem do jogo.</summary>
        public static List<Entry> ItensAtuais()
        {
            Segurado.Clear();
            foreach ((string id, string icone, bool prefixo, string nome, string uso) in Itens)
            {
                int quantidade = prefixo ? KeyItems.ContarGuardados(id) : KeyItems.Tem(id) ? 1 : 0;
                if (quantidade > 0) Adicionar(icone, prefixo ? quantidade : 0, nome, uso);
            }

            // As gemas brutas ainda nao cravadas na bancada.
            if (ScrapGems.TemRubi && !ScrapGems.EncaixadoRubi) Adicionar("ruby", 0, "Rubi bruto", "Cravar na Bancada");
            return Segurado;
        }

        private static void Adicionar(string icone, int quantidade, string nome, string uso)
        {
            Texture2D textura = Carregar(icone);
            if (textura != null) Segurado.Add(new Entry { Icone2 = textura, Quantidade = quantidade, Nome = nome, Uso = uso });
        }
    }
}
