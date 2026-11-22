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
        [UnityEngine.Serialization.FormerlySerializedAs("steamDash")] [SerializeField] private bool dashVapor;
        [UnityEngine.Serialization.FormerlySerializedAs("pressureJump")] [SerializeField] private bool puloPressao;
        [UnityEngine.Serialization.FormerlySerializedAs("resonanceStrike")] [SerializeField] private bool golpeRessonancia;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(bool dash, bool pulo)
        {
            dashVapor = dash;
            puloPressao = pulo;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(bool dash, bool pulo, bool ressonancia)
        {
            dashVapor = dash;
            puloPressao = pulo;
            golpeRessonancia = ressonancia;
        }

        private void Awake()
        {
            if (golpeRessonancia) ScrapChisel.DesbloquearGolpeRessonanteEmSilencio();

            if (dashVapor)
            {
                var dash = GetComponent<ScrapDash>();
                if (dash != null) dash.DefinirLiberadoDesdeInicio(true);
                ScrapDash.DesbloquearEmSilencio();
            }

            if (puloPressao) ScrapController.DesbloquearSaltoDePressaoEmSilencio();
        }
    }
}
