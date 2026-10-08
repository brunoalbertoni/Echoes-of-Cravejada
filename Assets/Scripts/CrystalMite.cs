using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Crystal Mite: um bichinho de cristal grudado no chao, camuflado. No
    /// escuro e no silencio ele e so mais uma pedra. Quando um som o ilumina,
    /// ele brilha, acorda e corre pra cima do Scrap por alguns segundos.
    /// Fraco — um golpe — mas aparece em grupo.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class CrystalMite : MonoBehaviour
    {
        [SerializeField] private float speed = 3.2f;

        [Tooltip("Segundos acordado depois do ultimo som.")]
        [SerializeField] private float awakeTime = 3.5f;

        [Tooltip("Limites de onde ele anda, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;

        private Transform _player;
        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private Vector3 _home;
        private float _awakeUntil;

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

            _damageable.Revived += Sleep;
        }

        private void Start()
        {
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null) _player = controller.transform;
        }

        private void OnEnable()
        {
            NoiseSystem.Heard += OnNoise;
        }

        private void OnDisable()
        {
            NoiseSystem.Heard -= OnNoise;
        }

        private void Sleep()
        {
            _awakeUntil = 0f;
            transform.position = _home;
        }

        private void OnNoise(Vector2 origin, float radius, float loudness)
        {
            if (loudness < 0.3f) return;
            if (Vector2.Distance(origin, transform.position) > radius) return;

            _awakeUntil = Time.time + awakeTime;
        }

        private void Update()
        {
            bool awake = Time.time < _awakeUntil;

            // Dormindo ele quase some; acordado, brilha.
            if (_sprite != null)
            {
                Color color = _sprite.color;
                color.a = Mathf.MoveTowards(color.a, awake ? 1f : 0.25f, 4f * Time.deltaTime);
                _sprite.color = color;
            }

            if (!awake || _player == null) return;
            if (Time.time - _damageable.LastHitTime < 0.2f) return;

            Vector3 position = transform.position;
            float direction = Mathf.Sign(_player.position.x - position.x);

            position.x = Mathf.Clamp(position.x + direction * speed * Time.deltaTime, minX, maxX);

            // Um pulinho a cada passo: ele corre saltitando.
            position.y = _home.y + Mathf.Abs(Mathf.Sin(Time.time * 14f)) * 0.25f;
            transform.position = position;

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
            // Dormindo ele e so pedra: nao machuca.
            if (Time.time >= _awakeUntil) return;

            var health = other.GetComponentInParent<ScrapHealth>();
            if (health != null) health.TakeDamage(1, transform.position);
        }
    }
}
