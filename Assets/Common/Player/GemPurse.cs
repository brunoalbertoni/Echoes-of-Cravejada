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
        [SaveProgress("Gems")]
        public static int Gemas { get; private set; }

        /// <summary>Quando a ultima foi recebida. O HUD usa pra destacar o contador.</summary>
        public static float UltimoTempoGanho { get; private set; } = -100f;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Gemas = 0;
            UltimoTempoGanho = -100f;
        }

        public static void Adicionar(int quantia)
        {
            Gemas = Mathf.Max(0, Gemas + quantia);
            if (quantia > 0) UltimoTempoGanho = Time.time;
        }
    }
}
