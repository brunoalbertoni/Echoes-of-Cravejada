using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// As gemas lapidadas: a moeda rara do jogo, diferente dos Opaque Shards.
    /// Por enquanto so se ganham (o Rebite paga uma pelas ferramentas do
    /// mestre); o que elas compram vem depois.
    ///
    /// Nao se perdem na morte, ao contrario dos shards. Estatico pra
    /// atravessar a troca de cena; sem save, zera quando o jogo fecha.
    /// </summary>
    public static class GemPurse
    {
        public static int Gems { get; private set; }

        /// <summary>Quando a ultima foi recebida. O HUD usa pra destacar o contador.</summary>
        public static float LastGainTime { get; private set; } = -100f;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Gems = 0;
            LastGainTime = -100f;
        }

        public static void Add(int amount)
        {
            Gems = Mathf.Max(0, Gems + amount);
            if (amount > 0) LastGainTime = Time.time;
        }
    }
}
