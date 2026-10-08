using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Quicksilver Slime: uma massa de mercurio corrompido, com alguma coisa de
    /// metal dentro. Rasteja de um lado pro outro. Golpeada, nao morre: se
    /// parte em duas menores, mais rapidas. So as pequenas se desfazem de vez.
    ///
    /// As pequenas nascem em jogo; quando o Scrap morre elas somem e a grande
    /// volta inteira.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class QuicksilverSlime : MonoBehaviour
    {
        [SerializeField] private float speed = 1.4f;

        [Tooltip("Limites de onde ela rasteja, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;

        [Tooltip("As pequenas nao se partem de novo.")]
        [SerializeField] private bool small;

        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private int _direction = 1;
        private float _baseY;

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
            _baseY = transform.position.y;

            if (small) ScrapHealth.Died += Vanish;
            else _damageable.Died += Split;
        }

        private void OnDestroy()
        {
            if (small) ScrapHealth.Died -= Vanish;
        }

        private void Vanish()
        {
            Destroy(gameObject);
        }

        /// <summary>A grande "morreu": nascem duas pequenas, uma pra cada lado.</summary>
        private void Split()
        {
            for (int side = -1; side <= 1; side += 2)
            {
                var half = new GameObject(name + "_Half");
                half.transform.position = new Vector3(
                    Mathf.Clamp(transform.position.x + side * 0.6f, minX, maxX), _baseY, 0f);
                half.transform.localScale = Vector3.one * 0.65f;

                var renderer = half.AddComponent<SpriteRenderer>();
                renderer.sprite = _sprite != null ? _sprite.sprite : null;
                renderer.sortingLayerName = "Actors";
                renderer.sortingOrder = -3;

                var body = half.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic;

                var trigger = half.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;
                trigger.size = new Vector2(0.9f, 0.6f);
                trigger.offset = new Vector2(0f, 0.35f);

                half.AddComponent<Damageable>().Configure(1, 0, null);

                var slime = half.AddComponent<QuicksilverSlime>();
                slime.minX = minX;
                slime.maxX = maxX;
                slime.speed = speed * 1.9f;
                slime.small = true;
                slime._direction = side;

                // O Awake rodou antes de "small" valer: acerta as inscricoes.
                slime._damageable.Died -= slime.Split;
                ScrapHealth.Died += slime.Vanish;
            }
        }

        private void Update()
        {
            // Apanhou: encolhe um instante, parada.
            if (Time.time - _damageable.LastHitTime < 0.2f) return;

            Vector3 position = transform.position;
            position.x += _direction * speed * Time.deltaTime;

            if (position.x <= minX) { position.x = minX; _direction = 1; }
            else if (position.x >= maxX) { position.x = maxX; _direction = -1; }

            transform.position = position;

            // Ela pulsa enquanto anda, como uma gota gorda.
            if (_sprite != null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.08f;
                float size = small ? 0.65f : 1f;
                transform.localScale = new Vector3(size * pulse, size / pulse, 1f);
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
            var controller = other.GetComponentInParent<ScrapController>();
            if (controller == null) return;

            // Encostar machuca e ainda gruda mercurio no corpo.
            var health = controller.GetComponent<ScrapHealth>();
            if (health != null) health.TakeDamage(1, transform.position);

            controller.Slow(1.5f);
        }
    }
}
