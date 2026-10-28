using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Pipe Leech: uma sanguessuga de mangueira de borracha e cobre, grudada
    /// num cano do teto pela ventosa. Fica pulsando ate o Scrap passar
    /// embaixo, e entao despenca. No chao ela se arrasta como lagarta atras
    /// dele e, perto, empina e da o bote. Ao morrer, fura e murcha.
    ///
    /// O pivo do desenho e a base: no teto, o objeto fica 1,5 tile abaixo do
    /// cano, e o corpo pende do alto do desenho.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PipeLeech : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do LeechArt, no editor.
        public const int Crawl = 0;
        public const int CrawlFrames = 6;
        public const int Cling = 6;
        public const int ClingFrames = 4;
        public const int Drop = 10;
        public const int DropFrames = 2;
        public const int Land = 12;
        public const int BiteWindup = 13;
        public const int BiteWindupFrames = 2;
        public const int Bite = 15;
        public const int BiteFrames = 3;
        public const int Hurt = 18;
        public const int Death = 19;
        public const int DeathFrames = 6;

        [Tooltip("A que distancia, de lado, ela percebe o Scrap embaixo.")]
        [SerializeField] private float triggerRange = 1.3f;

        [Tooltip("Altura do chao embaixo dela, em y de mundo (onde ficam os pes).")]
        [SerializeField] private float floorY;

        [SerializeField] private float crawlSpeed = 1.1f;

        [Tooltip("Ate onde ela se arrasta, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;

        [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] frames;

        [Header("O bote")]
        [SerializeField] private float biteRange = 1.6f;
        [SerializeField] private float biteWindup = 0.35f;
        [SerializeField] private float biteTime = 0.2f;
        [SerializeField] private float biteDistance = 1f;
        [SerializeField] private float biteRecover = 0.4f;
        [SerializeField] private float biteCooldown = 1.2f;

        private enum State { Cling, Falling, Landed, Crawl, Windup, Lunge, Recover }

        private Transform _player;
        private SpriteRenderer _sprite;
        private BoxCollider2D _box;
        private Damageable _damageable;
        private Vector3 _home;
        private float _fallSpeed;
        private State _state = State.Cling;
        private float _stateTime;
        private float _nextBite;
        private int _facing = 1;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newFloorY, float newMinX, float newMaxX, Sprite[] newFrames)
        {
            floorY = newFloorY;
            minX = newMinX;
            maxX = newMaxX;
            frames = newFrames;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _box = GetComponent<BoxCollider2D>();
            _home = transform.position;

            _damageable = GetComponent<Damageable>();
            if (_damageable != null)
            {
                _damageable.Revived += BackToCeiling;
                _damageable.DeathAnimation = Burst;
            }

            Enter(State.Cling);
        }

        private void OnDestroy()
        {
            if (_damageable != null) _damageable.Revived -= BackToCeiling;
        }

        private void Start()
        {
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null) _player = controller.transform;
        }

        private void BackToCeiling()
        {
            _fallSpeed = 0f;
            transform.position = _home;
            if (_sprite != null) _sprite.color = Color.white;
            Enter(State.Cling);
        }

        private void Enter(State state)
        {
            _state = state;
            _stateTime = 0f;

            if (_box == null) return;

            // No teto, o corpo pende do alto do desenho; no chao, fica embaixo.
            bool hanging = state == State.Cling || state == State.Falling;
            _box.size = hanging ? new Vector2(0.6f, 1f) : new Vector2(0.9f, 0.6f);
            _box.offset = hanging ? new Vector2(0f, 0.95f) : new Vector2(0f, 0.32f);
        }

        private void Update()
        {
            if (_damageable != null && _damageable.IsDead) return;

            _stateTime += Time.deltaTime;
            Vector3 position = transform.position;

            switch (_state)
            {
                case State.Cling:
                    Show(Cling + (int)(Time.time * 4f + _home.x) % ClingFrames);
                    if (_player != null && Mathf.Abs(_player.position.x - position.x) < triggerRange && _player.position.y < position.y + 0.5f)
                    {
                        Enter(State.Falling);
                    }

                    return;

                case State.Falling:
                    _fallSpeed += 40f * Time.deltaTime;
                    position.y -= _fallSpeed * Time.deltaTime;
                    Show(Drop + (_stateTime < 0.12f ? 0 : 1));

                    if (position.y <= floorY)
                    {
                        position.y = floorY;
                        Enter(State.Landed);
                    }

                    transform.position = position;
                    return;

                case State.Landed:
                    Show(Land);
                    if (_stateTime > 0.25f) Enter(State.Crawl);
                    return;
            }

            // Apanhando: e jogada pra tras e perde o bote.
            bool stunned = _damageable != null && Time.time - _damageable.LastHitTime < 0.2f;
            if (stunned)
            {
                if (_state != State.Crawl) Enter(State.Crawl);
                position.x = Mathf.Clamp(position.x - _damageable.LastHitSide * 3f * Time.deltaTime, minX, maxX);
                transform.position = position;
                Show(Hurt);
                return;
            }

            switch (_state)
            {
                case State.Crawl:
                    if (_player != null) _facing = _player.position.x >= position.x ? 1 : -1;
                    position.x = Mathf.Clamp(position.x + _facing * crawlSpeed * Time.deltaTime, minX, maxX);
                    Show(Crawl + (int)(Time.time * 8f) % CrawlFrames);

                    if (_player != null && Time.time >= _nextBite
                        && Mathf.Abs(_player.position.x - position.x) < biteRange
                        && Mathf.Abs(_player.position.y - position.y) < 1f)
                    {
                        Enter(State.Windup);
                    }

                    break;

                case State.Windup:
                    Show(BiteWindup + Mathf.Min(BiteWindupFrames - 1, (int)(_stateTime / biteWindup * BiteWindupFrames)));
                    if (_stateTime >= biteWindup) Enter(State.Lunge);
                    break;

                case State.Lunge:
                    position.x = Mathf.Clamp(position.x + _facing * biteDistance / biteTime * Time.deltaTime, minX, maxX);
                    Show(_stateTime < biteTime * 0.5f ? Bite : Bite + 1);
                    if (_stateTime >= biteTime) Enter(State.Recover);
                    break;

                case State.Recover:
                    Show(Bite + 2);
                    if (_stateTime >= biteRecover)
                    {
                        _nextBite = Time.time + biteCooldown;
                        Enter(State.Crawl);
                    }

                    break;
            }

            transform.position = position;
        }

        private void Show(int frame)
        {
            if (_sprite == null) return;

            // A arte olha pra direita; no teto ela nao vira.
            _sprite.flipX = _state != State.Cling && _facing < 0;

            if (frames != null && frame >= 0 && frame < frames.Length && frames[frame] != null) _sprite.sprite = frames[frame];
        }

        /// <summary>A morte: se estava no teto, despenca antes; no chao, fura, vaza e murcha.</summary>
        private IEnumerator Burst()
        {
            Vector3 position = transform.position;
            float speed = 0f;

            while (position.y > floorY)
            {
                speed += 40f * Time.deltaTime;
                position.y = Mathf.Max(floorY, position.y - speed * Time.deltaTime);
                transform.position = position;
                _state = State.Falling;
                Show(Drop + 1);
                yield return null;
            }

            _state = State.Crawl;
            float[] hold = { 0.12f, 0.12f, 0.12f, 0.14f, 0.2f, 0.4f };
            for (int i = 0; i < DeathFrames; i++)
            {
                Show(Death + i);
                yield return new WaitForSeconds(hold[i]);
            }

            if (_sprite == null) yield break;
            Color start = _sprite.color;
            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                _sprite.color = new Color(start.r, start.g, start.b, start.a * (1f - t / 0.35f));
                yield return null;
            }
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
            if (_damageable != null && _damageable.IsDead) return;

            var health = other.GetComponentInParent<ScrapHealth>();
            if (health != null) health.TakeDamage(1, transform.position);
        }
    }
}
