using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O barulho que o Scrap faz. Cada coisa que ele faz vira um ruido no
    /// NoiseSystem, com um alcance e uma forca:
    ///
    ///   passo e pulo        fracos: acendem um pouco em volta e nao acordam ninguem
    ///   pouso de pulo       um baque que os bichos ouvem (a safira abafa: ai nao ouvem)
    ///   queda               depende da altura: um tombo acende a sala
    ///   golpe do cinzel     forte
    ///   Steam Dash          o mais forte de todos: vapor e pistoes
    ///
    /// Com a safira cravada, o que vem do corpo (passo, pulo e queda) soa pela
    /// metade. Golpe e dash continuam altos: sao ferramenta, nao peso.
    ///
    /// Ele anda numa velocidade so. A escolha do District 3 nao e correr ou
    /// andar: e usar ou nao o dash, o golpe e a queda — que sao o que mostra o
    /// caminho e tambem o que acorda o escuro.
    /// </summary>
    [RequireComponent(typeof(ScrapController))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class ScrapNoise : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("stepInterval")] [Header("Passos")]
        [SerializeField] private float intervaloPasso = 0.34f;
        [UnityEngine.Serialization.FormerlySerializedAs("stepRadius")] [SerializeField] private float raioPasso = 3.5f;
        [UnityEngine.Serialization.FormerlySerializedAs("stepLoudness")] [SerializeField, Range(0f, 1f)] private float volumePasso = 0.15f;

        [UnityEngine.Serialization.FormerlySerializedAs("jumpRadius")] [Header("Pulo e queda")]
        [SerializeField] private float raioPulo = 4f;
        [UnityEngine.Serialization.FormerlySerializedAs("jumpLoudness")] [SerializeField, Range(0f, 1f)] private float volumePulo = 0.2f;

        [UnityEngine.Serialization.FormerlySerializedAs("landRadius")] [Tooltip("Alcance de uma queda bem alta. Quedas menores ficam entre o pouso de pulo e isto.")]
        [SerializeField] private float raioPousar = 10f;

        [UnityEngine.Serialization.FormerlySerializedAs("jumpLandRadius")] [Tooltip("O pouso de um pulo comum: alto o bastante pros bichos ouvirem (eles escutam de 0.3 pra cima).")]
        [SerializeField] private float raioPousarPulo = 6f;
        [UnityEngine.Serialization.FormerlySerializedAs("jumpLandLoudness")] [SerializeField, Range(0f, 1f)] private float volumePousarPulo = 0.4f;

        [UnityEngine.Serialization.FormerlySerializedAs("strikeRadius")] [Header("Golpe e dash")]
        [SerializeField] private float raioGolpe = 10f;
        [UnityEngine.Serialization.FormerlySerializedAs("strikeLoudness")] [SerializeField, Range(0f, 1f)] private float volumeGolpe = 0.8f;
        [UnityEngine.Serialization.FormerlySerializedAs("dashRadius")] [SerializeField] private float raioDash = 16f;
        [UnityEngine.Serialization.FormerlySerializedAs("dashLoudness")] [SerializeField, Range(0f, 1f)] private float volumeDash = 1f;

        private ScrapController _controlador;
        private ScrapChisel _cinzel;
        private ScrapDash _dash;
        private Rigidbody2D _corpo;

        private float _cronometroPasso;
        private bool _estavaNoChao = true;
        private bool _estavaGolpeando;
        private bool _estavaDashando;
        private float _velocidadeQueda;

        private void Awake()
        {
            _controlador = GetComponent<ScrapController>();
            _cinzel = GetComponent<ScrapChisel>();
            _dash = GetComponent<ScrapDash>();
            _corpo = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            Vector2 pes = transform.position;
            bool noChao = _controlador.EstaNoChao;

            // Passos: so no chao, e so andando de verdade.
            if (noChao && _controlador.VelocidadeNormalizada > 0.15f)
            {
                _cronometroPasso -= Time.deltaTime;

                if (_cronometroPasso <= 0f)
                {
                    _cronometroPasso = intervaloPasso;
                    NoiseSystem.Emitir(pes, raioPasso, volumePasso * ScrapGems.RuidoCorpo);
                }
            }
            else
            {
                _cronometroPasso = 0f;
            }

            // Sair do chao subindo e pulo; chegar no chao e queda, e pesa pela velocidade.
            if (_estavaNoChao && !noChao && _corpo.linearVelocity.y > 1f)
            {
                NoiseSystem.Emitir(pes, raioPulo, volumePulo * ScrapGems.RuidoCorpo);
            }

            if (!noChao) _velocidadeQueda = Mathf.Max(_velocidadeQueda, -_corpo.linearVelocity.y);

            if (!_estavaNoChao && noChao)
            {
                // Voltar de um pulo comum chega a uns 17,5 de velocidade; so
                // acima disso a queda comeca a pesar de verdade.
                float peso = Mathf.InverseLerp(19f, 32f, _velocidadeQueda);

                // Qualquer pouso de pulo ja e um baque que os bichos ouvem; so
                // descer um degrau andando fica no nivel do passo.
                bool pulou = _velocidadeQueda > 6f;
                float raioArea = Mathf.Lerp(pulou ? raioPousarPulo : raioPasso, raioPousar, peso);
                float volumeRuido2 = Mathf.Lerp(pulou ? volumePousarPulo : volumePasso, 1f, peso);

                NoiseSystem.Emitir(pes, raioArea, volumeRuido2 * ScrapGems.RuidoCorpo);

                _velocidadeQueda = 0f;
            }

            _estavaNoChao = noChao;

            bool golpeando = _cinzel != null && _cinzel.EstaGolpeando;
            if (golpeando && !_estavaGolpeando) NoiseSystem.Emitir(pes + Vector2.up * 0.6f, raioGolpe, volumeGolpe);
            _estavaGolpeando = golpeando;

            bool dashando = _dash != null && _dash.EstaDashando;
            if (dashando && !_estavaDashando) NoiseSystem.Emitir(pes + Vector2.up * 0.6f, raioDash, volumeDash);
            _estavaDashando = dashando;
        }
    }
}
