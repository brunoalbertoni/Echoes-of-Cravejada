using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Echo Hunter: um mineiro corrompido, cego, com cristais crescendo pela
    /// cabeca. Ele nao ve o Scrap — so ouve.
    ///
    /// Fica parado ate um ruido chegar nele. Entao corre ate a ORIGEM do som,
    /// fareja um instante e, sem ouvir mais nada, volta devagar pro lugar. Da
    /// pra passar por ele so andando e pulando baixo — sem dash, sem golpe, sem
    /// queda alta — ou manda-lo pra longe golpeando um cristal ressonante. Se
    /// ele alcancar, machuca muito.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class EchoHunter : MonoBehaviour
    {
        [SerializeField] private float chargeSpeed = 7f;
        [SerializeField] private float returnSpeed = 1.6f;

        [Tooltip("Ruido mais fraco que isso ele nao percebe. Passo faz 0.15 e pulo 0.2; golpe 0.8, dash 1.")]
        [SerializeField, Range(0f, 1f)] private float hearingThreshold = 0.3f;

        [Tooltip("Limites de onde ele consegue ir, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;

        [SerializeField] private int contactDamage = 2;

        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private Vector3 _home;
        private float _targetX;
        private float _sniffUntil;
        private int _state;   // 0 parado, 1 correndo pro som, 2 farejando, 3 voltando

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newMinX, float newMaxX)
        {
            minX = newMinX;
            maxX = newMaxX;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _damageable = GetComponent<Damageable>();
            _home = transform.position;

            _damageable.Revived += GoHome;
        }

        private void OnEnable()
        {
            NoiseSystem.Heard += OnNoise;
        }

        private void OnDisable()
        {
            NoiseSystem.Heard -= OnNoise;
        }

        private void GoHome()
        {
            _state = 0;
            transform.position = _home;
        }

        private void OnNoise(Vector2 origin, float radius, float loudness)
        {
            if (loudness < hearingThreshold) return;

            // Ele escuta na mesma altura, mais ou menos: um andar acima ja abafa.
            Vector2 offset = origin - (Vector2)transform.position;
            if (Mathf.Abs(offset.y) > 5f || offset.magnitude > radius + 2f) return;

            _targetX = Mathf.Clamp(origin.x, minX, maxX);
            _state = 1;
        }

        private void Update()
        {
            Vector3 position = transform.position;

            // Apanhou: cambaleia, e passa a saber onde o Scrap esta.
            if (Time.time - _damageable.LastHitTime < 0.25f)
            {
                position.x = Mathf.Clamp(position.x - _damageable.LastHitSide * 3f * Time.deltaTime, minX, maxX);
                transform.position = position;
                return;
            }

            if (_state == 0) return;

            if (_state == 2)
            {
                if (Time.time >= _sniffUntil) _state = 3;
                return;
            }

            float goal = _state == 1 ? _targetX : _home.x;
            float speed = _state == 1 ? chargeSpeed : returnSpeed;

            position.x = Mathf.MoveTowards(position.x, goal, speed * Time.deltaTime);
            transform.position = position;

            // A arte olha pra direita.
            if (_sprite != null && !Mathf.Approximately(goal, position.x)) _sprite.flipX = goal < position.x;

            if (!Mathf.Approximately(position.x, goal)) return;

            if (_state == 1)
            {
                _state = 2;
                _sniffUntil = Time.time + 1.6f;
            }
            else
            {
                _state = 0;
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
            var health = other.GetComponentInParent<ScrapHealth>();
            if (health != null) health.TakeDamage(contactDamage, transform.position);
        }
    }
}
