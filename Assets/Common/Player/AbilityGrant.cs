using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Garante que o Scrap ja tenha, nesta cena, as habilidades que o mapa dela
    /// exige — sem aviso na tela, porque ele nao esta ganhando nada agora.
    ///
    /// Serve pra dar Play direto numa fase do meio: o District 3 conta com o
    /// Steam Dash e com o Pressure Jump, que no jogo de verdade vem dos bosses
    /// anteriores.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public class AbilityGrant : MonoBehaviour
    {
        [SerializeField] private bool steamDash;
        [SerializeField] private bool pressureJump;
        [SerializeField] private bool resonanceStrike;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(bool dash, bool jump)
        {
            steamDash = dash;
            pressureJump = jump;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(bool dash, bool jump, bool resonance)
        {
            steamDash = dash;
            pressureJump = jump;
            resonanceStrike = resonance;
        }

        private void Awake()
        {
            if (resonanceStrike) ScrapChisel.UnlockResonanceStrikeSilently();

            if (steamDash)
            {
                var dash = GetComponent<ScrapDash>();
                if (dash != null) dash.SetUnlockedFromStart(true);
                ScrapDash.UnlockSilently();
            }

            if (pressureJump) ScrapController.UnlockPressureJumpSilently();
        }
    }
}
