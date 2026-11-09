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
        [SerializeField] private Sprite[] idle;
        [SerializeField] private Sprite[] move;

        [SerializeField] private float idleFps = 3f;

        [Tooltip("Quadros por segundo a cada tile por segundo de velocidade.")]
        [SerializeField] private float framesPerTile = 1.6f;

        [SerializeField] private float maxMoveFps = 14f;

        private SpriteRenderer _renderer;
        private Vector3 _last;
        private float _clock;
        private float _speed;
        private Sprite _held;
        private float _heldUntil;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(Sprite[] newIdle, Sprite[] newMove, float newIdleFps = 3f, float newFramesPerTile = 1.6f)
        {
            idle = newIdle;
            move = newMove;
            idleFps = newIdleFps;
            framesPerTile = newFramesPerTile;
        }

        /// <summary>Segura uma pose por um tempo, por cima do andar e do parado.</summary>
        public void Hold(Sprite pose, float seconds)
        {
            _held = pose;
            _heldUntil = Time.time + seconds;
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _last = transform.position;

            // Cada um num ponto do ciclo, pra nao respirarem todos juntos.
            _clock = Mathf.Abs(transform.position.x * 0.37f + transform.position.y * 0.11f);
        }

        private void LateUpdate()
        {
            Vector3 position = transform.position;
            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            float speed = Mathf.Abs(position.x - _last.x) / dt;
            _last = position;

            // Suaviza: um quadro parado no meio da corrida nao vira "parado".
            _speed = Mathf.Lerp(_speed, speed, 12f * dt);

            if (_held != null && Time.time < _heldUntil)
            {
                _renderer.sprite = _held;
                return;
            }

            bool moving = _speed > 0.3f && move != null && move.Length > 0;
            Sprite[] frames = moving ? move : idle;
            if (frames == null || frames.Length == 0) return;

            float fps = moving ? Mathf.Min(maxMoveFps, 4f + _speed * framesPerTile) : idleFps;
            _clock += dt * fps;

            Sprite frame = frames[(int)_clock % frames.Length];
            if (frame != null) _renderer.sprite = frame;
        }
    }
}
