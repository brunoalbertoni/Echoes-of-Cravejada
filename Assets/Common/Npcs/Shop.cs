using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>Com o que se paga um item da loja.</summary>
    public enum Currency
    {
        /// <summary>Os fragmentos opacos (Opaque Shards): caem dos corrompidos, se perdem na morte.</summary>
        Shards,

        /// <summary>As gemas lapidadas: raras, ganhas explorando e ajudando, nunca se perdem.</summary>
        Gems,
    }

    /// <summary>Um item da loja do Crisol.</summary>
    public sealed class ShopItem
    {
        public string Id;
        public string Name;
        public string Description;
        public Currency Currency;

        /// <summary>O preco da primeira compra, e quanto ele sobe a cada uma.</summary>
        public int BasePrice;
        public int PriceStep;

        /// <summary>Quantas vezes se compra. Zero: sem limite.</summary>
        public int MaxCount = 1;

        /// <summary>Se e uma historia que o Crisol conta (so aparece se ele tiver uma pra contar).</summary>
        public bool IsMemory;
    }

    /// <summary>
    /// A loja do Crisol: o que ele vende, o que ja foi comprado e o efeito de
    /// cada compra. O catalogo e um so, o mesmo em todo distrito onde ele
    /// abre a bancada; quem desenha e quem le as teclas e o Crisol e o GameHud.
    ///
    /// Os efeitos sao lidos pelos outros sistemas (dash, cinzel, morte,
    /// inimigos) pelas propriedades no fim. Estatico pra atravessar a troca de
    /// cena; sem save, zera quando o jogo fecha.
    /// </summary>
    public static class Shop
    {
        public const string Chassis = "chassis";
        public const string Pistons = "pistons";
        public const string LinedBag = "lined_bag";
        public const string GoldSetting = "gold_setting";
        public const string Harvest = "harvest_chisel";
        public const string Memory = "memory_crown";

        public static readonly ShopItem[] Catalog =
        {
            new ShopItem
            {
                Id = Chassis, Name = "Reforço de chassi", Currency = Currency.Shards,
                Description = "+1 de vida máxima. Cada reforço seguinte custa mais.",
                BasePrice = 25, PriceStep = 25, MaxCount = 4,
            },
            new ShopItem
            {
                Id = Pistons, Name = "Pistões regulados", Currency = Currency.Shards,
                Description = "A Sobrecarga de Velocidade esfria quase pela metade do tempo.",
                BasePrice = 60,
            },
            new ShopItem
            {
                Id = LinedBag, Name = "Bolsa forrada", Currency = Currency.Shards,
                Description = "Ao quebrar, metade dos fragmentos fica com você.",
                BasePrice = 80,
            },
            new ShopItem
            {
                Id = GoldSetting, Name = "Engaste de ouro", Currency = Currency.Gems,
                Description = "Um engaste no cabo do cinzel: +1 de dano em cada golpe.",
                BasePrice = 2,
            },
            new ShopItem
            {
                Id = Harvest, Name = "Cinzel de colheita", Currency = Currency.Gems,
                Description = "Os corrompidos soltam metade a mais de fragmentos.",
                BasePrice = 1,
            },
            new ShopItem
            {
                Id = Memory, Name = "Memória: a Coroa de Facetas", Currency = Currency.Gems,
                Description = "Crisol conta como lapidou a coroa do Rei de Prata.",
                BasePrice = 1, IsMemory = true,
            },
        };

        private static readonly Dictionary<string, int> Bought = new Dictionary<string, int>();

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Bought.Clear();
        }

        public static int Count(string id) => Bought.TryGetValue(id, out int count) ? count : 0;

        public static bool Has(string id) => Count(id) > 0;

        public static int Price(ShopItem item) => item.BasePrice + item.PriceStep * Count(item.Id);

        public static bool SoldOut(ShopItem item) => item.MaxCount > 0 && Count(item.Id) >= item.MaxCount;

        public static int Funds(Currency currency) => currency == Currency.Shards ? ScrapWallet.Shards : GemPurse.Gems;

        public static string CurrencyName(Currency currency, int amount)
        {
            if (currency == Currency.Gems) return amount == 1 ? "gema" : "gemas";
            return amount == 1 ? "fragmento" : "fragmentos";
        }

        /// <summary>
        /// Tenta comprar. Devolve o que o Crisol responde; comprou e verdadeiro
        /// so se a compra aconteceu. O efeito imediato (o reforco de chassi) ja
        /// vem aplicado; os outros valem a partir de agora pelas propriedades.
        /// </summary>
        public static string TryBuy(ShopItem item, ScrapHealth customer, out bool bought)
        {
            bought = false;

            if (SoldOut(item)) return "Isso você já comprou. Não tenho outro.";

            int price = Price(item);
            if (Funds(item.Currency) < price)
            {
                return item.Currency == Currency.Gems
                    ? "Gemas de menos. Não lapido fiado."
                    : "Fragmentos de menos. Volte quando tiver com que pagar.";
            }

            if (item.Currency == Currency.Shards) ScrapWallet.Add(-price);
            else GemPurse.Add(-price);

            Bought[item.Id] = Count(item.Id) + 1;
            bought = true;

            if (item.Id == Chassis && customer != null) customer.UpgradeChassis();

            return item.IsMemory ? "Sente-se. Isto vai demorar." : "Pronto. Que eu não me arrependa disso.";
        }

        // ------------------------------------------------------------------
        // Os efeitos, lidos por quem os aplica
        // ------------------------------------------------------------------

        /// <summary>Por quanto se multiplica a recarga do Steam Dash.</summary>
        public static float DashCooldownScale => Has(Pistons) ? 0.55f : 1f;

        /// <summary>O dano a mais do cinzel por causa da loja.</summary>
        public static int BonusDamage => Has(GoldSetting) ? 1 : 0;

        /// <summary>Por quanto se multiplica o que os corrompidos soltam.</summary>
        public static float ShardDropScale => Has(Harvest) ? 1.5f : 1f;

        /// <summary>Se, ao quebrar, metade dos fragmentos fica com o Scrap.</summary>
        public static bool KeepsHalfOnDeath => Has(LinedBag);
    }
}
