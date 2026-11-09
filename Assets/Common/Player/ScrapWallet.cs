using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Os Opaque Shards que o Scrap carrega. E estatico pra atravessar a troca
    /// de cena; ainda nao ha save, entao zera quando o jogo fecha.
    /// </summary>
    public static class ScrapWallet
    {
        public static int Shards { get; private set; }

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Shards = 0;
        }

        public static void Add(int amount)
        {
            Shards = Mathf.Max(0, Shards + amount);
        }

        /// <summary>Esvazia a carteira e devolve o que havia. E o que acontece na morte.</summary>
        public static int TakeAll()
        {
            int all = Shards;
            Shards = 0;
            return all;
        }
    }
}
