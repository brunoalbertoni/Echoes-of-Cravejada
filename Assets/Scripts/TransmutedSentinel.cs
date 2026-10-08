using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Transmuted Sentinel: uma armadura de vidro animada. Muda de material
    /// quando e atingida:
    ///
    ///   vidro   azulada e translucida. Fragil: o cinzel fere.
    ///   prata   opaca e brilhante. Blindada: o golpe so tine.
    ///
    /// Cada golpe que acerta no vidro a transmuta em prata por alguns segundos.
    /// Nao adianta martelar: e bater, recuar, esperar o vidro voltar. O raio de
    /// luz alquimica nao quer saber de material — queima do mesmo jeito.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class TransmutedSentinel : MonoBehaviour
    {
        [SerializeField] private float speed = 1.5f;

        [Tooltip("Limites de onde ela anda, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;

        [Tooltip("Segundos que ela fica em prata depois de um golpe.")]
        [SerializeField] private float silverTime = 2.6f;

        private static readonly Color Glass = new Color(0.60f, 0.85f, 1f, 0.75f);
        private static readonly Color Silver = new Color(1f, 1f, 1f, 1f);

        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private Transform _player;
        private float _silverUntil;
        private float _seenHit = -100f;

        public bool IsSilver => Time.time < _silverUntil;

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

            // Em prata ela apara tudo, venha de onde vier.
            _damageable.Blocks = from => IsSilver;
            _damageable.Revived += () => _silverUntil = 0f;
        }

        private void Start()
        {
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null) _player = controller.transform;
        }

        private void Update()
        {
            // Um golpe novo entrou no vidro: vira prata.
            if (_damageable.LastHitTime > _seenHit)
            {
                _seenHit = _damageable.LastHitTime;
                _silverUntil = Time.time + silverTime;
            }

            // A cor e o aviso. Perto de voltar a vidro, ela pisca.
            if (_sprite != null && Time.time - _damageable.LastHitTime > 0.15f)
            {
                float left = _silverUntil - Time.time;
                bool showSilver = left > 0.6f || (left > 0f && Mathf.FloorToInt(left * 12f) % 2 == 0);
                _sprite.color = showSilver ? Silver : Glass;
            }

            if (_player == null) return;

            Vector2 offset = _player.position - transform.position;
            if (Mathf.Abs(offset.x) > 10f || Mathf.Abs(offset.y) > 3f || Mathf.Abs(offset.x) < 0.3f) return;

            // Em prata ela e pesada: anda pela metade.
            float pace = IsSilver ? speed * 0.5f : speed;
            float direction = Mathf.Sign(offset.x);

            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x + direction * pace * Time.deltaTime, minX, maxX);
            transform.position = position;

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = direction < 0f;
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
