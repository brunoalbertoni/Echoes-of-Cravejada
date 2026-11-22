using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Escolhe o quadro do Scrap a cada instante, olhando o que ele esta
    /// fazendo: parado (respirando e piscando de vez em quando), andando, no
    /// ar (subindo, no alto, caindo), aterrissando, na parede, preparando e
    /// dando o golpe (de lado, pra cima ou pra baixo), no dash, apanhando.
    ///
    /// Nao usa Animator nem AnimationClip: sao so duas listas de sprites na
    /// mesma ordem, uma da sucata e uma do polido. Pra trocar a arte, basta
    /// trocar os sprites das listas mantendo a ordem (ver as constantes).
    /// </summary>
    [RequireComponent(typeof(ScrapController))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class ScrapAnimator : MonoBehaviour
    {
        // A ordem dos quadros nas duas listas. E a mesma do HeroArt, no editor.
        public const int QuadroOcioso = 0;
        public const int QuadrosOcioso = 8;
        public const int QuadroAndar = 8;
        public const int QuadrosAndar = 8;
        public const int Pulo = 16;
        public const int Apice = 17;
        public const int QuadroQueda = 18;
        public const int QuadrosQueda = 2;
        public const int Pousar = 20;
        public const int Parede2 = 21;
        public const int QuadroPreparo = 22;
        public const int QuadrosPreparo = 2;
        public const int QuadroGolpe = 24;
        public const int QuadrosGolpe = 3;
        public const int QuadroCimaGolpe = 27;
        public const int QuadrosCimaGolpe = 2;
        public const int QuadroBaixoGolpe = 29;
        public const int QuadrosBaixoGolpe = 2;
        public const int QuadroDash = 31;
        public const int QuadrosDash = 2;
        public const int Ferido = 33;

        /// <summary>O quadro de olhos fechados, dentro do ciclo de parado.</summary>
        private const int QuadroPiscar = QuadroOcioso + 5;

        [UnityEngine.Serialization.FormerlySerializedAs("scrapFrames")] [Tooltip("Os quadros da sucata, na ordem das constantes.")]
        [SerializeField] private Sprite[] quadrosSucata;

        [UnityEngine.Serialization.FormerlySerializedAs("polishedFrames")] [Tooltip("Os quadros do polido, na mesma ordem.")]
        [SerializeField] private Sprite[] quadrosPolido;

        [UnityEngine.Serialization.FormerlySerializedAs("idleFramesPerSecond")] [SerializeField] private float paradoQuadrosPorSegundo = 6f;
        [UnityEngine.Serialization.FormerlySerializedAs("walkFramesPerSecond")] [SerializeField] private float andarQuadrosPorSegundo = 13f;

        [UnityEngine.Serialization.FormerlySerializedAs("landTime")] [Tooltip("Quanto dura o quadro de aterrissagem, em segundos.")]
        [SerializeField] private float tempoPousar = 0.1f;

        [UnityEngine.Serialization.FormerlySerializedAs("landSpeed")] [Tooltip("A queda mais lenta que ainda mostra a aterrissagem, em tiles por segundo.")]
        [SerializeField] private float velocidadePousar = 6f;

        private ScrapController _controlador;
        private ScrapChisel _cinzel;
        private ScrapDash _dash;
        private ScrapHealth _vida;
        private Rigidbody2D _corpo;
        private SpriteRenderer _renderizador;

        private bool _estavaNoChao = true;
        private float _velocidadeQueda;
        private float _pousarAte = -100f;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(Sprite[] sucata, Sprite[] polido)
        {
            quadrosSucata = sucata;
            quadrosPolido = polido;
        }

        /// <summary>
        /// Os quadros da aparencia atual (sucata ou polido). Os cards de
        /// habilidade e as placas de tutorial usam pra animar o proprio Scrap.
        /// </summary>
        public Sprite[] QuadrosAtual => ScrapAppearance.EstaPolido ? quadrosPolido : quadrosSucata;

        /// <summary>O quadro mostrado agora (o indice das constantes). O ScrapSounds usa pra pisar no tempo do passo.</summary>
        public int Quadro { get; private set; } = -1;

        private void Awake()
        {
            _controlador = GetComponent<ScrapController>();
            _cinzel = GetComponent<ScrapChisel>();
            _dash = GetComponent<ScrapDash>();
            _vida = GetComponent<ScrapHealth>();
            _corpo = GetComponent<Rigidbody2D>();
            _renderizador = GetComponent<SpriteRenderer>();
        }

        // No LateUpdate, depois de todo mundo decidir o que aconteceu no quadro.
        private void LateUpdate()
        {
            AcompanharPouso();

            Sprite[] quadros = ScrapAppearance.EstaPolido ? quadrosPolido : quadrosSucata;
            if (quadros == null || quadros.Length == 0) return;

            int quadro = Mathf.Clamp(EscolherQuadro(), 0, quadros.Length - 1);
            Quadro = quadro;
            if (quadros[quadro] != null) FrameAnimator.Exibir(_renderizador, quadros[quadro]);
        }

        /// <summary>Guarda a velocidade da queda e, ao tocar o chao depois de uma queda de verdade, marca a aterrissagem.</summary>
        private void AcompanharPouso()
        {
            bool noChao = _controlador.EstaNoChao;

            if (!noChao) _velocidadeQueda = Mathf.Min(_velocidadeQueda, _corpo.linearVelocity.y);

            if (noChao && !_estavaNoChao && -_velocidadeQueda >= velocidadePousar) _pousarAte = Time.time + tempoPousar;
            if (noChao) _velocidadeQueda = 0f;

            _estavaNoChao = noChao;
        }

        private int EscolherQuadro()
        {
            // O que interrompe tudo vem primeiro.
            if (_vida != null && Time.time - _vida.UltimoTempoGolpe < 0.3f) return Ferido;
            if (_dash != null && _dash.EstaDashando) return QuadroDash + (int)(Time.time * 16f) % QuadrosDash;

            if (_cinzel != null)
            {
                int ataque = QuadroDeAtaque();
                if (ataque >= 0) return ataque;
            }

            if (_controlador.EstaDeslizandoParede) return Parede2;

            if (!_controlador.EstaNoChao)
            {
                float vy = _corpo.linearVelocity.y;
                if (vy > 2.5f) return Pulo;
                if (vy > -2.5f) return Apice;
                return QuadroQueda + (int)(Time.time * 8f) % QuadrosQueda;
            }

            if (Time.time < _pousarAte) return Pousar;

            if (_controlador.VelocidadeNormalizada > 0.12f)
            {
                float taxa = andarQuadrosPorSegundo * Mathf.Max(0.6f, _controlador.VelocidadeNormalizada);
                return QuadroAndar + (int)(Time.time * taxa) % QuadrosAndar;
            }

            // Parado: respira, e so pisca de vez em quando (um ciclo em tres).
            int parado = QuadroOcioso + (int)(Time.time * paradoQuadrosPorSegundo) % QuadrosOcioso;
            float ciclo = QuadrosOcioso / paradoQuadrosPorSegundo;
            if (parado == QuadroPiscar && (int)(Time.time / ciclo) % 3 != 0) parado = QuadroPiscar - 1;
            return parado;
        }

        /// <summary>O quadro do golpe, pelo relogio do cinzel, ou -1 se ele nao esta golpeando.</summary>
        private int QuadroDeAtaque()
        {
            Vector2 direcao = _cinzel.DirecaoGolpe;

            if (_cinzel.EstaCimaPreparando)
            {
                // Pra cima e pra baixo o preparo e o primeiro quadro do proprio golpe.
                if (direcao.y > 0f) return QuadroCimaGolpe;
                if (direcao.y < 0f) return QuadroBaixoGolpe;

                float t = (Time.time - _cinzel.TempoInicioPreparo) / Mathf.Max(0.01f, _cinzel.DuracaoPreparo);
                return QuadroPreparo + Mathf.Clamp((int)(t * QuadrosPreparo), 0, QuadrosPreparo - 1);
            }

            if (_cinzel.EstaGolpeando)
            {
                float t = (Time.time - _cinzel.TempoInicioGolpe) / Mathf.Max(0.01f, _cinzel.DuracaoGolpe);

                if (direcao.y > 0f) return QuadroCimaGolpe + Mathf.Clamp(1 + (int)(t * 2f), 1, QuadrosCimaGolpe - 1);
                if (direcao.y < 0f) return QuadroBaixoGolpe + Mathf.Clamp(1 + (int)(t * 2f), 1, QuadrosBaixoGolpe - 1);

                return QuadroGolpe + Mathf.Clamp((int)(t * QuadrosGolpe), 0, QuadrosGolpe - 1);
            }

            // O acompanhamento: o golpe de lado termina com a lamina embaixo, um instante.
            if (direcao.y == 0f && Time.time - _cinzel.TempoFimGolpe < 0.1f) return QuadroGolpe + QuadrosGolpe - 1;

            return -1;
        }
    }
}
