using Unity.Cinemachine;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A camera do jogo, feita com Cinemachine. Este script calcula o
    /// enquadramento (seguir o Scrap, travar nas bordas, cenas e tremor) e o
    /// entrega a uma CinemachineCamera; o CinemachineBrain leva o resultado
    /// para a camera real no mesmo quadro.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class ScrapCameraFollow : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("target")] [SerializeField] private Transform alvo;

        [UnityEngine.Serialization.FormerlySerializedAs("offset")] [Tooltip("Deslocamento em relacao ao alvo. Um Y positivo mostra mais do que vem pela frente em cima.")]
        [SerializeField] private Vector2 deslocamento = new Vector2(0f, 1.5f);

        [UnityEngine.Serialization.FormerlySerializedAs("smoothTime")] [Tooltip("Quanto menor, mais colada a camera fica. Zero remove o amortecimento.")]
        [SerializeField] private float tempoSuave = 0.18f;

        [UnityEngine.Serialization.FormerlySerializedAs("clampToBounds")] [Header("Limites do mundo")]
        [SerializeField] private bool limitarParaLimites = true;

        [UnityEngine.Serialization.FormerlySerializedAs("worldBounds")] [Tooltip("Retangulo do mapa em unidades de mundo. O gerador preenche com o tamanho da Phase 1.")]
        [SerializeField] private Rect limitesMundo = new Rect(0f, 0f, 80f, 24f);

        private Camera _camera;
        private CinemachineBrain _cerebro;
        private CinemachineCamera _cameraVirtual;
        private Vector3 _posicao;
        private Vector3 _velocidadeVetor;
        private float _tamanhoBase;
        private float _tamanhoAtual;

        // A camera dirigida: numa cena, quem dirige diz pra onde ela olha e o
        // quanto aproxima (1 = o normal; menor fecha, maior abre). Estatico
        // pra a cena nao precisar achar a camera.
        private static bool _dirigido;
        private static Vector2 _foco;
        private static float _zoom = 1f;
        private static float _suaveDireto = 0.4f;

        // O tremor: forca e ate quando.
        private static float _forcaTremor;
        private static float _tremorAte;
        private static float _comprimentoTremor = 1f;
        private Vector3 _deslocamentoTremor;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarDirecao()
        {
            _dirigido = false;
            _zoom = 1f;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarTremor()
        {
            _forcaTremor = 0f;
            _tremorAte = 0f;
        }

        /// <summary>
        /// Toma a camera numa cena: ela desliza ate focus (em unidades de mundo)
        /// e aproxima ou afasta ate zoom vezes o tamanho normal. smooth e o
        /// tempo do deslize: maior, mais lento e mais cinematografico.
        /// </summary>
        public static void Direcionar(Vector2 foco, float zoom, float suave = 0.4f)
        {
            _dirigido = true;
            _foco = foco;
            _zoom = Mathf.Max(0.3f, zoom);
            _suaveDireto = Mathf.Max(0.01f, suave);
        }

        /// <summary>Devolve a camera ao Scrap, voltando ao tamanho normal devagar.</summary>
        public static void Liberar()
        {
            _dirigido = false;
            _zoom = 1f;
        }

        /// <summary>
        /// Treme a tela por um tempo, em tiles de deslocamento, diminuindo ate
        /// parar. Um tremor mais fraco nao corta um mais forte em andamento.
        /// </summary>
        public static void Tremer(float forca2, float duracao)
        {
            float restante = Mathf.Max(0f, _tremorAte - Time.time);
            float atual = _comprimentoTremor > 0f ? _forcaTremor * restante / _comprimentoTremor : 0f;
            if (forca2 < atual) return;

            _forcaTremor = forca2;
            _comprimentoTremor = Mathf.Max(0.01f, duracao);
            _tremorAte = Time.time + duracao;
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _tamanhoBase = _camera.orthographicSize;
            _tamanhoAtual = _tamanhoBase;
            _posicao = transform.position;
            PrepararCinemachine();
        }

        private void OnEnable()
        {
            // Comeca ja enquadrado, senao a camera faz um deslize feio no primeiro frame.
            if (alvo != null)
            {
                _posicao = Resolver(PosicaoDesejada());
                EntregarAoCinemachine();
            }
        }

        private void OnDestroy()
        {
            if (_cameraVirtual != null) Destroy(_cameraVirtual.gameObject);
        }

        private void PrepararCinemachine()
        {
            _cerebro = GetComponent<CinemachineBrain>();
            if (_cerebro == null) _cerebro = gameObject.AddComponent<CinemachineBrain>();

            // Atualizado por este script: quem le a camera no LateUpdate (o parallax) ve a posicao certa.
            _cerebro.UpdateMethod = CinemachineBrain.UpdateMethods.ManualUpdate;
            _cerebro.BlendUpdateMethod = CinemachineBrain.BrainUpdateMethods.LateUpdate;
            _cerebro.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);

            var objeto = new GameObject("CameraVirtual");
            objeto.transform.SetPositionAndRotation(transform.position, transform.rotation);
            _cameraVirtual = objeto.AddComponent<CinemachineCamera>();
            _cameraVirtual.Lens = LensSettings.FromCamera(_camera);
        }

        private void LateUpdate()
        {
            if (alvo == null) return;

            // O tremor do quadro anterior sai antes de seguir o alvo, senao ele
            // entra no amortecimento e a camera escorrega.
            _posicao -= _deslocamentoTremor;
            _deslocamentoTremor = Vector3.zero;

            // O tamanho vai devagar ate o pedido (ou volta ao normal).
            float tamanho = _tamanhoBase * (_dirigido ? _zoom : 1f);
            _tamanhoAtual = Mathf.Lerp(_tamanhoAtual, tamanho, 1f - Mathf.Exp(-Time.deltaTime * 2.6f));

            Vector3 objetivo = _dirigido ? new Vector3(_foco.x, _foco.y, _posicao.z) : PosicaoDesejada();
            Vector3 desejado = Vector3.SmoothDamp(_posicao, objetivo, ref _velocidadeVetor, _dirigido ? _suaveDireto : tempoSuave);
            _posicao = Resolver(desejado);

            float esquerda = _tremorAte - Time.time;
            if (esquerda > 0f)
            {
                float forca2 = _forcaTremor * esquerda / _comprimentoTremor;
                _deslocamentoTremor = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f) * forca2;
                _posicao += _deslocamentoTremor;
            }

            EntregarAoCinemachine();
        }

        private void EntregarAoCinemachine()
        {
            if (_cameraVirtual == null || _cerebro == null) return;

            _cameraVirtual.transform.position = _posicao;
            LensSettings lente = _cameraVirtual.Lens;
            lente.OrthographicSize = _tamanhoAtual;
            _cameraVirtual.Lens = lente;
            _cerebro.ManualUpdate();
        }

        private Vector3 PosicaoDesejada()
        {
            return new Vector3(alvo.position.x + deslocamento.x, alvo.position.y + deslocamento.y, _posicao.z);
        }

        /// <summary>Aplica os limites do mundo, mantendo o Z da camera.</summary>
        private Vector3 Resolver(Vector3 posicao)
        {
            posicao.z = _posicao.z;
            if (!limitarParaLimites || _camera == null) return posicao;

            float alturaMetade = _tamanhoAtual;
            float larguraMetade = alturaMetade * _camera.aspect;

            // Se o mapa e menor que a tela naquele eixo, centraliza em vez de travar.
            posicao.x = limitesMundo.width < larguraMetade * 2f
                ? limitesMundo.center.x
                : Mathf.Clamp(posicao.x, limitesMundo.xMin + larguraMetade, limitesMundo.xMax - larguraMetade);

            posicao.y = limitesMundo.height < alturaMetade * 2f
                ? limitesMundo.center.y
                : Mathf.Clamp(posicao.y, limitesMundo.yMin + alturaMetade, limitesMundo.yMax - alturaMetade);

            return posicao;
        }

        /// <summary>Usado pelo gerador da Phase 1 pra configurar tudo de uma vez.</summary>
        public void Configurar(Transform novoAlvo, Rect limites)
        {
            alvo = novoAlvo;
            limitesMundo = limites;
            limitarParaLimites = true;
        }

        /// <summary>
        /// Pula direto pro enquadramento do alvo, sem amortecer. Pra quando o
        /// alvo e teleportado — ao chegar de outra cena, por exemplo.
        /// </summary>
        public void IrDiretoAoAlvo()
        {
            if (alvo == null) return;

            _velocidadeVetor = Vector3.zero;
            _posicao = Resolver(PosicaoDesejada());
            EntregarAoCinemachine();
        }

        private void OnDrawGizmosSelected()
        {
            if (!limitarParaLimites) return;

            Gizmos.color = new Color(0.3f, 1f, 0.6f, 0.75f);
            Gizmos.DrawWireCube(
                new Vector3(limitesMundo.center.x, limitesMundo.center.y, 0f),
                new Vector3(limitesMundo.width, limitesMundo.height, 0f));
        }
    }
}
