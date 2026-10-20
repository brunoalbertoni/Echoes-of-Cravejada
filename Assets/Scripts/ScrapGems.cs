using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// As gemas do Scrap. Carregar nao faz nada; e preciso crava-las numa
    /// Workbench.
    ///
    ///   Rubi bruto     quem entrega e Crisol. O cinzel bate mais forte, em brasa.
    ///   Safira bruta   cai do Resonant. Passos, pulos e quedas fazem metade do
    ///                  ruido — o corpo de ferro aprende a pisar leve.
    ///
    /// Estatico pra valer em qualquer cena; sem save, zera quando o jogo fecha.
    /// </summary>
    public static class ScrapGems
    {
        public static bool HasRuby { get; private set; }
        public static bool RubySocketed { get; private set; }

        public static bool HasSapphire { get; private set; }
        public static bool SapphireSocketed { get; private set; }

        /// <summary>Quando cada gema foi entregue e quando foi cravada. O HUD usa pros avisos.</summary>
        public static float RubyGivenTime { get; private set; } = -100f;
        public static float RubySocketedTime { get; private set; } = -100f;
        public static float SapphireGivenTime { get; private set; } = -100f;
        public static float SapphireSocketedTime { get; private set; } = -100f;

        /// <summary>O dano a mais que o cinzel causa por causa das gemas cravadas.</summary>
        public static int BonusDamage => (RubySocketed ? 1 : 0) + Shop.BonusDamage;

        /// <summary>Por quanto se multiplica o ruido do corpo (passo, pulo, queda).</summary>
        public static float BodyNoise => SapphireSocketed ? 0.5f : 1f;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            HasRuby = false;
            RubySocketed = false;
            HasSapphire = false;
            SapphireSocketed = false;
            RubyGivenTime = -100f;
            RubySocketedTime = -100f;
            SapphireGivenTime = -100f;
            SapphireSocketedTime = -100f;
        }

        public static void GiveRuby()
        {
            if (HasRuby) return;

            HasRuby = true;
            RubyGivenTime = Time.time;
        }

        public static void GiveSapphire()
        {
            if (HasSapphire) return;

            HasSapphire = true;
            SapphireGivenTime = Time.time;
        }

        /// <summary>Chamado ao descansar numa bancada: crava o que estiver solto.</summary>
        public static void SocketAtWorkbench()
        {
            if (HasRuby && !RubySocketed)
            {
                RubySocketed = true;
                RubySocketedTime = Time.time;
            }

            if (HasSapphire && !SapphireSocketed)
            {
                SapphireSocketed = true;
                SapphireSocketedTime = Time.time;
            }
        }
    }
}
