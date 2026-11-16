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
            public Texture2D Icon;
            public int Count;
            public string Name;
            public string Use;
        }

        // O id (ou comeco de id) no KeyItems e o icone de cada um.
        // Nome e uso aparecem ao passar o mouse na casinha.
        private static readonly (string Id, string Icon, bool Prefix, string Name, string Use)[] Items =
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
        private static readonly List<Entry> Held = new List<Entry>();

        private static Texture2D Load(string name)
        {
            if (!Cache.TryGetValue(name, out Texture2D icon))
            {
                icon = Resources.Load<Texture2D>("HudIcons/item_" + name);
                Cache[name] = icon;
            }

            return icon;
        }

        /// <summary>O que ele tem na mao agora, na ordem do jogo.</summary>
        public static List<Entry> Current()
        {
            Held.Clear();
            foreach ((string id, string icon, bool prefix, string name, string use) in Items)
            {
                int count = prefix ? KeyItems.CountHeld(id) : KeyItems.Has(id) ? 1 : 0;
                if (count > 0) Add(icon, prefix ? count : 0, name, use);
            }

            // As gemas brutas ainda nao cravadas na bancada.
            if (ScrapGems.HasRuby && !ScrapGems.RubySocketed) Add("ruby", 0, "Rubi bruto", "Cravar na Bancada");
            return Held;
        }

        private static void Add(string icon, int count, string name, string use)
        {
            Texture2D texture = Load(icon);
            if (texture != null) Held.Add(new Entry { Icon = texture, Count = count, Name = name, Use = use });
        }
    }
}
