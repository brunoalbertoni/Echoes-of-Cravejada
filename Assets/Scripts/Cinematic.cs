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
        public static bool Active { get; private set; }

        /// <summary>Quanto a tela esta tampada de preto: 0 limpa, 1 toda preta.</summary>
        public static float Fade { get; set; }

        /// <summary>Quando a cena comecou e quando acabou. O HUD usa pra deslizar as faixas.</summary>
        public static float StartTime { get; private set; } = -100f;
        public static float EndTime { get; private set; } = -100f;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Active = false;
            Fade = 0f;
            StartTime = -100f;
            EndTime = -100f;
        }

        public static void Begin()
        {
            Active = true;
            StartTime = Time.time;
        }

        public static void End()
        {
            Active = false;
            Fade = 0f;
            EndTime = Time.time;
        }

        /// <summary>Quanto as faixas pretas estao pra dentro: 0 fora da tela, 1 inteiras.</summary>
        public static float Letterbox
        {
            get
            {
                const float slide = 0.4f;
                return Active
                    ? Mathf.Clamp01((Time.time - StartTime) / slide)
                    : 1f - Mathf.Clamp01((Time.time - EndTime) / slide);
            }
        }
    }
}
