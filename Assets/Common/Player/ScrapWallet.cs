using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Os Opaque Shards que o Scrap carrega. E estatico pra atravessar a troca
    /// de cena; ainda nao ha save, entao zera quando o jogo fecha.
    /// </summary>
    public static class ScrapWallet
    {
        [SaveProgress("Shards")]
        public static int QuantidadeFragmentos { get; private set; }

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            QuantidadeFragmentos = 0;
        }

        public static void Adicionar(int quantia)
        {
            QuantidadeFragmentos = Mathf.Max(0, QuantidadeFragmentos + quantia);
        }

        /// <summary>Esvazia a carteira e devolve o que havia. E o que acontece na morte.</summary>
        public static int PegarTudo()
        {
            int todos = QuantidadeFragmentos;
            QuantidadeFragmentos = 0;
            return todos;
        }
    }
}
