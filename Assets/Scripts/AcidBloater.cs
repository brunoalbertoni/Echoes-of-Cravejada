using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Acid Bloater: um tanque ambulante de acido. Lento e resistente. Quando
    /// o Scrap fica na frente dele, abaixa, gira a valvula (o aviso) e borrifa
    /// acido pelo bico. Ao morrer ele incha, o vidro racha e ele estoura,
    /// deixando uma poca de acido no chao por alguns segundos — entao e melhor
    /// nao mata-lo onde voce vai pisar.
    ///
    /// O pivo do desenho e a base: a posicao e onde os pes pisam.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class AcidBloater : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do BloaterArt, no editor.
        public const int Walk = 0;
        public const int WalkFrames = 6;
        public const int Windup = 6;
        public const int WindupFrames = 2;
        public const int Spray = 8;
        public const int SprayFrames = 3;
        public const int Hurt = 11;
        public const int Swell = 12;
        public const int SwellFrames = 4;

        [SerializeField] private float speed = 0.8f;

        [Tooltip("Limites da patrulha, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;

        [Tooltip("Segundos que a poca fica no chao.")]
        [SerializeField] private float puddleTime = 4f;

        [Tooltip("Largura da poca, em tiles.")]
        [SerializeField] private float puddleWidth = 2.4f;

        [SerializeField] private Sprite puddleSprite;

        [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] frames;

        [Header("O borrifo")]
        [SerializeField] private float sprayRange = 2.4f;
        [SerializeField] private float windupTime = 0.5f;
        [SerializeField] private float sprayTime = 0.6f;
        [SerializeField] private float recoverTime = 0.3f;
        [SerializeField] private float cooldown = 2.5f;

        private enum State { Walk, Windup, Spray, Recover }

        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private Transform _player;
        private int _direction = 1;
        private State _state = State.Walk;
        private float _stateTime;
        private float _nextSpray;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newMinX, float newMaxX, Sprite newPuddleSprite, Sprite[] newFrames = null)
        {
            minX = newMinX;
            maxX = newMaxX;
            puddleSprite = newPuddleSprite;
            frames = newFrames;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _damageable = GetComponent<Damageable>();
            _damageable.Died += Splash;
            _damageable.Revived += ResetState;
        }

        private void OnDestroy()
        {
            if (_damageable != null)
            {
                _damageable.Died -= Splash;
                _damageable.Revived -= ResetState;
            }
        }

        private void Start()
        {
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null) _player = controller.transform;
        }

        private void ResetState()
        {
            _state = State.Walk;
            _stateTime = 0f;
            _nextSpray = 0f;
        }

        private void Splash()
        {
            // Ele mesmo some na hora (o Damageable o desliga), entao quem incha
            // e estoura e uma copia: da tempo de o jogador ver e se afastar.
            Sprite[] swell = null;
            if (frames != null && frames.Length >= Swell + SwellFrames)
            {
                swell = new Sprite[SwellFrames];
                for (int i = 0; i < SwellFrames; i++) swell[i] = frames[Swell + i];
            }

            BloaterBurst.Begin(_sprite, transform.position, transform.position, puddleWidth, puddleTime, puddleSprite, swell);
        }

        private void Update()
        {
            Vector3 position = transform.position;
            _stateTime += Time.deltaTime;

            if (Time.time - _damageable.LastHitTime < 0.2f)
            {
                _state = State.Walk;
                position.x = Mathf.Clamp(position.x - _damageable.LastHitSide * 2f * Time.deltaTime, minX, maxX);
                transform.position = position;
                Show(Hurt);
                return;
            }

            switch (_state)
            {
                case State.Walk:
                    if (position.x >= maxX) _direction = -1;
                    else if (position.x <= minX) _direction = 1;

                    position.x = Mathf.Clamp(position.x + _direction * speed * Time.deltaTime, minX, maxX);
                    Show(Walk + (int)(Time.time * 6f) % WalkFrames);

                    if (_player != null && Time.time >= _nextSpray && InFront(position))
                    {
                        _direction = _player.position.x >= position.x ? 1 : -1;
                        Enter(State.Windup);
                    }

                    break;

                case State.Windup:
                    Show(Windup + Mathf.Min(WindupFrames - 1, (int)(_stateTime / windupTime * WindupFrames)));
                    if (_stateTime >= windupTime) Enter(State.Spray);
                    break;

                case State.Spray:
                    Show(Spray + Mathf.Min(SprayFrames - 1, (int)(_stateTime / sprayTime * SprayFrames)));
                    Splatter();
                    if (_stateTime >= sprayTime) Enter(State.Recover);
                    break;

                case State.Recover:
                    Show(Walk);
                    if (_stateTime >= recoverTime)
                    {
                        _nextSpray = Time.time + cooldown;
                        Enter(State.Walk);
                    }

                    break;
            }

            transform.position = position;
        }

        private bool InFront(Vector3 position)
        {
            float dx = _player.position.x - position.x;
            return Mathf.Abs(dx) < sprayRange && Mathf.Abs(_player.position.y - position.y) < 1.2f;
        }

        /// <summary>O borrifo: queima quem estiver na frente do bico.</summary>
        private void Splatter()
        {
            Vector2 center = (Vector2)transform.position + new Vector2(_direction * 1.75f, 0.6f);
            foreach (Collider2D hit in Physics2D.OverlapBoxAll(center, new Vector2(1.6f, 0.7f), 0f))
            {
                var health = hit.GetComponentInParent<ScrapHealth>();
                if (health != null) health.TakeDamage(1, transform.position);
            }
        }

        private void Enter(State state)
        {
            _state = state;
            _stateTime = 0f;
        }

        private void Show(int frame)
        {
            if (_sprite == null) return;

            // A arte olha pra direita.
            _sprite.flipX = _direction < 0;
            if (frames != null && frame >= 0 && frame < frames.Length && frames[frame] != null) _sprite.sprite = frames[frame];
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Touch(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Touch(other);
        }

        private void Touch(Collider2D other)
        {
            var health = other.GetComponentInParent<ScrapHealth>();
            if (health != null) health.TakeDamage(1, transform.position);
        }
    }
}
