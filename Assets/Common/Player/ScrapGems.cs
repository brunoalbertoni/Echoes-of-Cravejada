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
        [SaveProgress("HasRuby")]
        public static bool TemRubi { get; private set; }
        [SaveProgress("RubySocketed")]
        public static bool EncaixadoRubi { get; private set; }

        [SaveProgress("HasSapphire")]
        public static bool TemSafira { get; private set; }
        [SaveProgress("SapphireSocketed")]
        public static bool EncaixadoSafira { get; private set; }

        /// <summary>Quando cada gema foi entregue e quando foi cravada. O HUD usa pros avisos.</summary>
        public static float TempoDadoRubi { get; private set; } = -100f;
        public static float TempoEncaixadoRubi { get; private set; } = -100f;
        public static float TempoDadoSafira { get; private set; } = -100f;
        public static float TempoEncaixadoSafira { get; private set; } = -100f;

        /// <summary>O dano a mais que o cinzel causa por causa das gemas cravadas.</summary>
        public static int DanoBonus => (EncaixadoRubi ? 1 : 0) + Shop.DanoBonus;

        /// <summary>Por quanto se multiplica o ruido do corpo (passo, pulo, queda).</summary>
        public static float RuidoCorpo => EncaixadoSafira ? 0.5f : 1f;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            TemRubi = false;
            EncaixadoRubi = false;
            TemSafira = false;
            EncaixadoSafira = false;
            TempoDadoRubi = -100f;
            TempoEncaixadoRubi = -100f;
            TempoDadoSafira = -100f;
            TempoEncaixadoSafira = -100f;
        }

        public static void DarRubi()
        {
            if (TemRubi) return;

            TemRubi = true;
            TempoDadoRubi = Time.time;
            ItemToast.MostrarRubi();
            Sfx.Gema.Tocar();
        }

        public static void DarSafira()
        {
            if (TemSafira) return;

            TemSafira = true;
            TempoDadoSafira = Time.time;
            Sfx.Gema.Tocar();
        }

        /// <summary>Chamado ao descansar numa bancada: crava o que estiver solto.</summary>
        public static void EncaixarNaBancada()
        {
            bool encaixado = false;

            if (TemRubi && !EncaixadoRubi)
            {
                EncaixadoRubi = true;
                TempoEncaixadoRubi = Time.time;

                // Depois que a gravura da bancada sai do meio da tela.
                ItemToast.Encaixar(1.7f);
                encaixado = true;
            }

            if (TemSafira && !EncaixadoSafira)
            {
                EncaixadoSafira = true;
                TempoEncaixadoSafira = Time.time;
                encaixado = true;
            }

            // Depois do som da bancada, o clique da gema entrando no encaixe.
            if (encaixado) Sfx.EncaixeGema.Tocar(atraso: 0.35f);
        }
    }
}
