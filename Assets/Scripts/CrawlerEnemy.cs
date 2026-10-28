using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um autômato corrompido que rasteja de um lado pro outro num trecho de
    /// chao e machuca quem encosta. E o primeiro inimigo do jogo, e existe pra
    /// ensinar a bater: quando o Scrap chega perto, ele recua, abre as pincas
    /// (o aviso) e da um bote curto pra frente. Apanhar no meio do aviso
    /// cancela o bote.
    ///
    /// O trecho e dado pelo gerador da fase, entao ele nao precisa sondar o
    /// chao: so vai e volta entre dois pontos. Ao morrer ele se desmonta (ver
    /// Damageable.DeathAnimation) antes de sumir.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CrawlerEnemy : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do CrawlerArt, no editor.
        public const int Walk = 0;
        public const int WalkFrames = 8;
        public const int Windup = 8;
        public const int WindupFrames = 2;
        public const int Bite = 10;
        public const int BiteFrames = 3;
        public const int Hurt = 13;
        public const int Death = 14;
        public const int DeathFrames = 6;

        [SerializeField] private float speed = 1.4f;
        [SerializeField] private int contactDamage = 1;

        [Tooltip("Limites da patrulha, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;

        [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float walkFramesPerSecond = 11f;

        [Header("O bote")]
        [Tooltip("A que distancia do Scrap ele resolve dar o bote, em tiles.")]
        [SerializeField] private float attackRange = 2.2f;
        [SerializeField] private float windupTime = 0.5f;
        [SerializeField] private float lungeTime = 0.18f;
        [SerializeField] private float lungeDistance = 1.2f;
        [SerializeField] private float recoverTime = 0.45f;
        [SerializeField] private float attackCooldown = 1.4f;

        private enum State { Walk, Windup, Lunge, Recover }

        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private Transform _player;
        private int _direction = 1;
        private int _startDirection = 1;
        private State _state = State.Walk;
        private float _stateTime;
        private float _nextAttack;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newMinX, float newMaxX, Sprite[] newFrames)
        {
            minX = newMinX;
            maxX = newMaxX;
            frames = newFrames;
        }

        private void Awake()
        {
            _sprite = GetComponentInChildren<SpriteRenderer>();
            _damageable = GetComponent<Damageable>();

            // Cada um comeca indo pra um lado.
            _startDirection = Mathf.RoundToInt(transform.position.x) % 2 == 0 ? 1 : -1;
            _direction = _startDirection;

            if (_damageable != null)
            {
                _damageable.DeathAnimation = FallApart;
                _damageable.Revived += ResetState;
            }
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
            _state = State.Walk;
            _direction = _startDirection;
            _nextAttack = 0f;
        }

        private void Update()
        {
            if (_damageable != null && _damageable.IsDead) return;

            Vector3 position = transform.position;
            _stateTime += Time.deltaTime;

            // Logo depois de apanhar ele e empurrado pra longe do golpe, parado,
            // e perde o bote que estava armando.
            bool stunned = _damageable != null && Time.time - _damageable.LastHitTime < 0.25f;
            if (stunned)
            {
                if (_state != State.Walk) Enter(State.Walk);
                _nextAttack = Mathf.Max(_nextAttack, Time.time + 0.6f);

                position.x = Mathf.Clamp(position.x - _damageable.LastHitSide * 3.5f * Time.deltaTime, minX, maxX);
                transform.position = position;
                ShowFrame(Hurt);
                return;
            }

            switch (_state)
            {
                case State.Walk:
                    position.x = Mathf.Clamp(position.x + _direction * speed * Time.deltaTime, minX, maxX);
                    if (position.x >= maxX) _direction = -1;
                    else if (position.x <= minX) _direction = 1;

                    ShowFrame(Walk + (int)(Time.time * walkFramesPerSecond) % WalkFrames);

                    if (Time.time >= _nextAttack && PlayerInReach(position))
                    {
                        _direction = _player.position.x >= position.x ? 1 : -1;
                        Enter(State.Windup);
                    }

                    break;

                case State.Windup:
                    ShowFrame(Windup + Mathf.Min(WindupFrames - 1, (int)(_stateTime / windupTime * WindupFrames)));
                    if (_stateTime >= windupTime) Enter(State.Lunge);
                    break;

                case State.Lunge:
                    position.x = Mathf.Clamp(position.x + _direction * lungeDistance / lungeTime * Time.deltaTime, minX, maxX);
                    ShowFrame(_stateTime < lungeTime * 0.5f ? Bite : Bite + 1);
                    if (_stateTime >= lungeTime) Enter(State.Recover);
                    break;

                case State.Recover:
                    ShowFrame(Bite + 2);
                    if (_stateTime >= recoverTime)
                    {
                        _nextAttack = Time.time + attackCooldown;
                        Enter(State.Walk);
                    }

                    break;
            }

            transform.position = position;
        }

        private void Enter(State state)
        {
            _state = state;
            _stateTime = 0f;
        }

        /// <summary>O Scrap esta perto, na mesma altura, e dentro do trecho que ele alcanca.</summary>
        private bool PlayerInReach(Vector3 position)
        {
            if (_player == null) return false;

            Vector3 target = _player.position;
            if (Mathf.Abs(target.y - (position.y - 0.5f)) > 1.2f) return false;
            if (Mathf.Abs(target.x - position.x) > attackRange) return false;

            return target.x > minX - 1f && target.x < maxX + 1f;
        }

        private void ShowFrame(int frame)
        {
            if (_sprite == null) return;

            // A arte olha pra direita.
            _sprite.flipX = _direction < 0;

            if (frames != null && frame >= 0 && frame < frames.Length && frames[frame] != null)
            {
                _sprite.sprite = frames[frame];
            }
        }

        /// <summary>
        /// A morte: a chave salta, as patas cedem, o casco tomba e a cabeca
        /// rola. Os pedacos ficam um instante no chao e somem.
        /// </summary>
        private IEnumerator FallApart()
        {
            float[] hold = { 0.12f, 0.1f, 0.12f, 0.12f, 0.2f, 0.45f };

            for (int i = 0; i < DeathFrames; i++)
            {
                ShowFrame(Death + i);
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
            if (health != null) health.TakeDamage(contactDamage, transform.position);
        }
    }
}
