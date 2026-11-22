using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>Com o que se paga um item da loja.</summary>
    public enum Currency
    {
        /// <summary>Os fragmentos opacos (Opaque Shards): caem dos corrompidos, se perdem na morte.</summary>
        QuantidadeFragmentos,

        /// <summary>As gemas lapidadas: raras, ganhas explorando e ajudando, nunca se perdem.</summary>
        Gemas,
    }

    /// <summary>Um item da loja do Crisol.</summary>
    public sealed class ShopItem
    {
        public string Id;
        public string Nome;
        public string DescricaoTexto;
        public Currency MoedaTipo;

        /// <summary>O preco da primeira compra, e quanto ele sobe a cada uma.</summary>
        public int PrecoBase;
        public int PassoPreco;

        /// <summary>Quantas vezes se compra. Zero: sem limite.</summary>
        public int QuantidadeMax = 1;

        /// <summary>Se e uma historia que o Crisol conta (so aparece se ele tiver uma pra contar).</summary>
        public bool EstaMemoria;
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
        public const string Chassi = "chassis";
        public const string Pistoes = "pistons";
        public const string BolsaForrado = "lined_bag";
        public const string EngasteOuro = "gold_setting";
        public const string Colheita = "harvest_chisel";
        public const string Memoria = "memory_crown";

        public static readonly ShopItem[] Catalogo =
        {
            new ShopItem
            {
                Id = Chassi, Nome = "Reforço de chassi", MoedaTipo = Currency.QuantidadeFragmentos,
                DescricaoTexto = "+1 de vida máxima. Cada reforço seguinte custa mais.",
                PrecoBase = 25, PassoPreco = 25, QuantidadeMax = 4,
            },
            new ShopItem
            {
                Id = Pistoes, Nome = "Pistões regulados", MoedaTipo = Currency.QuantidadeFragmentos,
                DescricaoTexto = "A Sobrecarga de Velocidade esfria quase pela metade do tempo.",
                PrecoBase = 60,
            },
            new ShopItem
            {
                Id = BolsaForrado, Nome = "Bolsa forrada", MoedaTipo = Currency.QuantidadeFragmentos,
                DescricaoTexto = "Ao quebrar, metade dos fragmentos fica com você.",
                PrecoBase = 80,
            },
            new ShopItem
            {
                Id = EngasteOuro, Nome = "Engaste de ouro", MoedaTipo = Currency.Gemas,
                DescricaoTexto = "Um engaste no cabo do cinzel: +1 de dano em cada golpe.",
                PrecoBase = 2,
            },
            new ShopItem
            {
                Id = Colheita, Nome = "Cinzel de colheita", MoedaTipo = Currency.Gemas,
                DescricaoTexto = "Os corrompidos soltam metade a mais de fragmentos.",
                PrecoBase = 1,
            },
            new ShopItem
            {
                Id = Memoria, Nome = "Memória: a Coroa de Facetas", MoedaTipo = Currency.Gemas,
                DescricaoTexto = "Crisol conta como lapidou a coroa do Rei de Prata.",
                PrecoBase = 1, EstaMemoria = true,
            },
        };

        [SaveProgress("Bought")]
        private static readonly Dictionary<string, int> Comprados = new Dictionary<string, int>();

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Comprados.Clear();
        }

        public static int Contar(string id) => Comprados.TryGetValue(id, out int quantidade) ? quantidade : 0;

        public static bool Tem(string id) => Contar(id) > 0;

        public static int Preco(ShopItem item) => item.PrecoBase + item.PassoPreco * Contar(item.Id);

        public static bool Esgotado(ShopItem item) => item.QuantidadeMax > 0 && Contar(item.Id) >= item.QuantidadeMax;

        public static int Saldo(Currency moeda) => moeda == Currency.QuantidadeFragmentos ? ScrapWallet.QuantidadeFragmentos : GemPurse.Gemas;

        public static string NomeDaMoeda(Currency moeda, int quantia)
        {
            if (moeda == Currency.Gemas) return quantia == 1 ? "gema" : "gemas";
            return quantia == 1 ? "fragmento" : "fragmentos";
        }

        /// <summary>
        /// Tenta comprar. Devolve o que o Crisol responde; comprou e verdadeiro
        /// so se a compra aconteceu. O efeito imediato (o reforco de chassi) ja
        /// vem aplicado; os outros valem a partir de agora pelas propriedades.
        /// </summary>
        public static string TentarComprar(ShopItem item, ScrapHealth cliente, out bool comprados)
        {
            comprados = false;

            if (Esgotado(item)) return "Isso você já comprou. Não tenho outro.";

            int preco = Preco(item);
            if (Saldo(item.MoedaTipo) < preco)
            {
                return item.MoedaTipo == Currency.Gemas
                    ? "Gemas de menos. Não lapido fiado."
                    : "Fragmentos de menos. Volte quando tiver com que pagar.";
            }

            if (item.MoedaTipo == Currency.QuantidadeFragmentos) ScrapWallet.Adicionar(-preco);
            else GemPurse.Adicionar(-preco);

            Comprados[item.Id] = Contar(item.Id) + 1;
            comprados = true;

            if (item.Id == Chassi && cliente != null) cliente.MelhorarChassi();

            return item.EstaMemoria ? "Sente-se. Isto vai demorar." : "Pronto. Que eu não me arrependa disso.";
        }

        // ------------------------------------------------------------------
        // Os efeitos, lidos por quem os aplica
        // ------------------------------------------------------------------

        /// <summary>Por quanto se multiplica a recarga do Steam Dash.</summary>
        public static float EscalaRecargaDash => Tem(Pistoes) ? 0.55f : 1f;

        /// <summary>O dano a mais do cinzel por causa da loja.</summary>
        public static int DanoBonus => Tem(EngasteOuro) ? 1 : 0;

        /// <summary>Por quanto se multiplica o que os corrompidos soltam.</summary>
        public static float EscalaQuedaFragmento => Tem(Colheita) ? 1.5f : 1f;

        /// <summary>Se, ao quebrar, metade dos fragmentos fica com o Scrap.</summary>
        public static bool MantemMetadeLigadoMorte => Tem(BolsaForrado);
    }
}
