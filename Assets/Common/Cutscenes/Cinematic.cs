using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O estado de "cena": enquanto uma mini cutscene roda, o Scrap nao apanha
    /// (ScrapHealth olha aqui) e o HUD poe as faixas pretas em cima e embaixo.
    /// Quem roda a cena trava o controle do Scrap por conta propria.
    ///
    /// Tambem guarda o escuro da tela inteira (Fade), pra cena que comeca no
    /// preto. O GameHud desenha os dois.
    /// </summary>
    public static class Cinematic
    {
        public static bool Ativo { get; private set; }

        /// <summary>Quanto a tela esta tampada de preto: 0 limpa, 1 toda preta.</summary>
        public static float Esmaecimento { get; set; }

        /// <summary>Um clarao branco por cima de tudo: 0 nada, 1 tela toda branca. Quem acende apaga.</summary>
        public static float Clarao2 { get; set; }

        /// <summary>Quando a cena comecou e quando acabou. O HUD usa pra deslizar as faixas.</summary>
        public static float TempoInicio { get; private set; } = -100f;
        public static float TempoFim { get; private set; } = -100f;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Ativo = false;
            Esmaecimento = 0f;
            Clarao2 = 0f;
            TempoInicio = -100f;
            TempoFim = -100f;
        }

        public static void Iniciar()
        {
            Ativo = true;
            TempoInicio = Time.time;
        }

        public static void Encerrar()
        {
            Ativo = false;
            Esmaecimento = 0f;
            Clarao2 = 0f;
            TempoFim = Time.time;
        }

        /// <summary>Quanto as faixas pretas estao pra dentro: 0 fora da tela, 1 inteiras.</summary>
        public static float FaixasCinema
        {
            get
            {
                const float deslize = 0.4f;
                return Ativo
                    ? Mathf.Clamp01((Time.time - TempoInicio) / deslize)
                    : 1f - Mathf.Clamp01((Time.time - TempoFim) / deslize);
            }
        }
    }
}
