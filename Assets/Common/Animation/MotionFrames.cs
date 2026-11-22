using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Anima quem anda sem que o cerebro dele precise saber de animacao:
    /// olha se o objeto se mexeu neste quadro e toca os quadros de andar (mais
    /// rapido quanto mais rapido ele vai) ou os de parado, respirando.
    ///
    /// Quem quiser uma pose especial (o monstro erguendo os bracos, batendo
    /// no chao) chama Hold, e ela fica na tela pelo tempo pedido.
    ///
    /// Nao mexe no flipX nem na cor: isso e do cerebro e do Damageable.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class MotionFrames : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("idle")] [SerializeField] private Sprite[] parado;
        [UnityEngine.Serialization.FormerlySerializedAs("move")] [SerializeField] private Sprite[] mover;

        [UnityEngine.Serialization.FormerlySerializedAs("idleFps")] [SerializeField] private float qpsParado = 3f;

        [UnityEngine.Serialization.FormerlySerializedAs("framesPerTile")] [Tooltip("Quadros por segundo a cada tile por segundo de velocidade.")]
        [SerializeField] private float quadrosPorTile = 1.6f;

        [UnityEngine.Serialization.FormerlySerializedAs("maxMoveFps")] [SerializeField] private float qpsMoverMax = 14f;

        private SpriteRenderer _renderizador;
        private Vector3 _ultimo;
        private float _relogio;
        private float _velocidade;
        private Sprite _segurado;
        private float _seguradoAte;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(Sprite[] novoParado, Sprite[] novoMover, float novoQpsParado = 3f, float novoQuadrosPorTile = 1.6f)
        {
            parado = novoParado;
            mover = novoMover;
            qpsParado = novoQpsParado;
            quadrosPorTile = novoQuadrosPorTile;
        }

        /// <summary>Segura uma pose por um tempo, por cima do andar e do parado.</summary>
        public void Segurar(Sprite pose, float segundos)
        {
            _segurado = pose;
            _seguradoAte = Time.time + segundos;
        }

        private void Awake()
        {
            _renderizador = GetComponent<SpriteRenderer>();
            _ultimo = transform.position;

            // Cada um num ponto do ciclo, pra nao respirarem todos juntos.
            _relogio = Mathf.Abs(transform.position.x * 0.37f + transform.position.y * 0.11f);
        }

        private void LateUpdate()
        {
            Vector3 posicao = transform.position;
            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            float velocidade = Mathf.Abs(posicao.x - _ultimo.x) / dt;
            _ultimo = posicao;

            // Suaviza: um quadro parado no meio da corrida nao vira "parado".
            _velocidade = Mathf.Lerp(_velocidade, velocidade, 12f * dt);

            if (_segurado != null && Time.time < _seguradoAte)
            {
                FrameAnimator.Exibir(_renderizador, _segurado);
                return;
            }

            bool movendo = _velocidade > 0.3f && mover != null && mover.Length > 0;
            Sprite[] quadros = movendo ? mover : parado;
            if (quadros == null || quadros.Length == 0) return;

            float qps = movendo ? Mathf.Min(qpsMoverMax, 4f + _velocidade * quadrosPorTile) : qpsParado;
            _relogio += dt * qps;

            Sprite quadro = quadros[(int)_relogio % quadros.Length];
            if (quadro != null) FrameAnimator.Exibir(_renderizador, quadro);
        }
    }
}
