using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um aviso curto no meio da tela, pra quem nao tem um aviso proprio no
    /// HUD: pegar uma ferramenta, o motor do Coletor, a gema, esbarrar na
    /// nevoa. Um por vez; o mais novo substitui o anterior.
    /// </summary>
    public static class HudNotice
    {
        public static string Text { get; private set; }

        private static float _until = -100f;

        /// <summary>Se ha um aviso na tela agora.</summary>
        public static bool Showing => Text != null && Time.time < _until;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Text = null;
            _until = -100f;
        }

        public static void Show(string text, float seconds = 5f)
        {
            Text = text;
            _until = Time.time + seconds;
        }
    }
}
