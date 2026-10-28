using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Brass Welder: um Tarnished corrompido, ainda de mascara de soldador.
    /// Patrulha um trecho de chao e, quando ve o Scrap, vira pra ele e avanca
    /// com o macarico. Perto, recolhe o macarico (a chama cresce: e o aviso) e
    /// solta um jato de fogo pra frente.
    ///
    /// Ele apara todo golpe que vem de frente — ergue a manopla e sai faisca.
    /// So da pra feri-lo por tras ou por cima, pulando e batendo pra baixo.
    /// Ao morrer, larga o macarico, ajoelha e a mascara cai.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class BrassWelder : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do WelderArt, no editor.
        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Walk = 4;
        public const int WalkFrames = 8;
        public const int Windup = 12;
        public const int WindupFrames = 2;
        public const int Flame = 14;
        public const int FlameFrames = 3;
        public const int Block = 17;
        public const int BlockFrames = 2;
        public const int Hurt = 19;
        public const int Death = 20;
        public const int DeathFrames = 6;

        [SerializeField] private float speed = 1.5f;

        [Tooltip("A que distancia ele percebe o Scrap e parte pra cima.")]
        [SerializeField] private float sightRange = 6f;

        [Tooltip("Limites da patrulha, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;

        [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] frames;

        [Header("O macarico")]
        [SerializeField] private float flameRange = 2.2f;
        [SerializeField] private float windupTime = 0.45f;
        [SerializeField] private float flameTime = 0.5f;
        [SerializeField] private float recoverTime = 0.3f;
        [SerializeField] private float cooldown = 1.4f;

        private enum State { Patrol, Windup, Flame, Recover }

        private Transform _player;
        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private int _facing = 1;
        private State _state = State.Patrol;
        private float _stateTime;
        private float _nextAttack;
        private float _blockUntil = -1f;
        private bool _chasing;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newMinX, float newMaxX, Sprite[] newFrames = null)
        {
            minX = newMinX;
            maxX = newMaxX;
            frames = newFrames;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _damageable = GetComponent<Damageable>();
            _damageable.Blocks = BlocksHitFrom;
            _damageable.DeathAnimation = FallDown;
            _damageable.Revived += ResetState;
        }

        private void OnDestroy()
        {
            if (_damageable != null) _damageable.Revived -= ResetState;
        }

        private void Start()
        {
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null) _player = controller.transform;
        }

        private void ResetState()
        {
            _state = State.Patrol;
            _stateTime = 0f;
            _nextAttack = 0f;
        }

        /// <summary>
        /// De frente ele apara. Vale golpe vindo do lado pra onde ele olha, a
        /// menos que venha bem de cima.
        /// </summary>
        private bool BlocksHitFrom(Vector2 from)
        {
            Vector2 offset = from - (Vector2)transform.position;

            bool fromAbove = offset.y > 0.9f && Mathf.Abs(offset.x) < 1.2f;
            if (fromAbove) return false;

            bool blocked = Mathf.Sign(offset.x) == _facing;
            if (blocked) _blockUntil = Time.time + 0.25f;
            return blocked;
        }

        private void Update()
        {
            if (_damageable.IsDead) return;

            Vector3 position = transform.position;
            _stateTime += Time.deltaTime;

            // Logo depois de apanhar ele cambaleia pra longe do golpe e perde o ataque.
            if (Time.time - _damageable.LastHitTime < 0.3f)
            {
                _state = State.Patrol;
                position.x = Mathf.Clamp(position.x - _damageable.LastHitSide * 3f * Time.deltaTime, minX, maxX);
                transform.position = position;
                Show(Hurt);
                return;
            }

            switch (_state)
            {
                case State.Patrol:
                    Patrol(ref position);
                    break;

                case State.Windup:
                    Show(Windup + Mathf.Min(WindupFrames - 1, (int)(_stateTime / windupTime * WindupFrames)));
                    if (_stateTime >= windupTime) Enter(State.Flame);
                    break;

                case State.Flame:
                    Show(Flame + Mathf.Min(FlameFrames - 1, (int)(_stateTime / flameTime * FlameFrames)));
                    Burn();
                    if (_stateTime >= flameTime) Enter(State.Recover);
                    break;

                case State.Recover:
                    Show(Idle);
                    if (_stateTime >= recoverTime)
                    {
                        _nextAttack = Time.time + cooldown;
                        Enter(State.Patrol);
                    }

                    break;
            }

            transform.position = position;
        }

        private void Patrol(ref Vector3 position)
        {
            _chasing = _player != null
                && Mathf.Abs(_player.position.x - position.x) < sightRange
                && Mathf.Abs(_player.position.y - position.y) < 2.5f;

            float pace = speed;

            if (_chasing)
            {
                _facing = _player.position.x >= position.x ? 1 : -1;
                pace = speed * 1.7f;

                bool close = Mathf.Abs(_player.position.x - position.x) < flameRange
                    && Mathf.Abs(_player.position.y - position.y) < 1.2f;
                if (close && Time.time >= _nextAttack)
                {
                    Enter(State.Windup);
                    return;
                }
            }
            else if (position.x >= maxX) _facing = -1;
            else if (position.x <= minX) _facing = 1;

            float before = position.x;
            position.x = Mathf.Clamp(position.x + _facing * pace * Time.deltaTime, minX, maxX);

            if (Time.time < _blockUntil) Show(Block + (Time.time + 0.12f > _blockUntil ? 1 : 0));
            else if (Mathf.Approximately(before, position.x)) Show(Idle + (int)(Time.time * 5f) % IdleFrames);
            else Show(Walk + (int)(Time.time * (_chasing ? 15f : 10f)) % WalkFrames);
        }

        /// <summary>O jato de fogo: queima quem estiver na frente, na altura do macarico.</summary>
        private void Burn()
        {
            Vector2 center = (Vector2)transform.position + new Vector2(_facing * 1.4f, 0.85f);
            foreach (Collider2D hit in Physics2D.OverlapBoxAll(center, new Vector2(1.8f, 0.7f), 0f))
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
            _sprite.flipX = _facing < 0;
            if (frames != null && frame >= 0 && frame < frames.Length && frames[frame] != null) _sprite.sprite = frames[frame];
        }

        /// <summary>A morte: cambaleia, larga o macarico, ajoelha, a mascara cai, tomba.</summary>
        private IEnumerator FallDown()
        {
            float[] hold = { 0.14f, 0.14f, 0.2f, 0.2f, 0.25f, 0.45f };
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
            if (_damageable.IsDead) return;

            var health = other.GetComponentInParent<ScrapHealth>();
            if (health != null) health.TakeDamage(1, transform.position);
        }
    }
}
