using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Quicksilver Slime: uma massa de mercurio corrompido, com uma engrenagem
    /// dentro e dois olhos de vidro boiando. Rasteja de um lado pro outro, como
    /// lagarta. Golpeada, nao morre: se esparrama e se parte em duas menores,
    /// mais rapidas. So as pequenas se desfazem de vez.
    ///
    /// As pequenas nascem em jogo; quando o Scrap morre elas somem e a grande
    /// volta inteira.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class QuicksilverSlime : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do SlimeArt, no editor.
        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Crawl = 4;
        public const int CrawlFrames = 6;
        public const int Hurt = 10;
        public const int Death = 11;
        public const int DeathFrames = 6;

        private const float SmallScale = 0.65f;

        [SerializeField] private float speed = 1.4f;

        [Tooltip("Limites de onde ela rasteja, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;

        [Tooltip("As pequenas nao se partem de novo.")]
        [SerializeField] private bool small;

        [Tooltip("Todos os quadros, na ordem das constantes. As pequenas herdam.")]
        [SerializeField] private Sprite[] frames;

        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private int _direction = 1;
        private float _baseY;
        private float _clock;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newMinX, float newMaxX, Sprite[] newFrames = null)
        {
            minX = newMinX;
            maxX = newMaxX;
            if (newFrames != null) frames = newFrames;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _damageable = GetComponent<Damageable>();
            _baseY = transform.position.y;
            _clock = Mathf.Abs(transform.position.x * 0.31f);

            _damageable.DeathAnimation = Spread;

            // A grande "morre" se partindo; as pequenas (acertadas no Split) se desfazem.
            _damageable.DeathSound = small ? Sfx.DeathGoo : Sfx.SlimeSplit;

            if (small) ScrapHealth.Died += Vanish;
            else _damageable.Died += Split;
        }

        private void OnDestroy()
        {
            if (small) ScrapHealth.Died -= Vanish;
            else if (_damageable != null) _damageable.Died -= Split;
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
                half.transform.localScale = Vector3.one * SmallScale;

                var renderer = half.AddComponent<SpriteRenderer>();
                renderer.sprite = frames != null && frames.Length > Crawl ? frames[Crawl] : (_sprite != null ? _sprite.sprite : null);
                renderer.sortingLayerName = "Actors";
                renderer.sortingOrder = -3;
                renderer.flipX = side < 0;

                var body = half.AddComponent<Rigidbody2D>();
                body.bodyType = RigidbodyType2D.Kinematic;

                var trigger = half.AddComponent<BoxCollider2D>();
                trigger.isTrigger = true;
                trigger.size = new Vector2(1.3f, 0.9f);
                trigger.offset = new Vector2(0f, 0.45f);

                half.AddComponent<Damageable>().Configure(1, 0, null);

                var slime = half.AddComponent<QuicksilverSlime>();
                slime.minX = minX;
                slime.maxX = maxX;
                slime.speed = speed * 1.9f;
                slime.small = true;
                slime.frames = frames;
                slime._direction = side;

                // O Awake rodou antes de "small" valer: acerta as inscricoes.
                slime._damageable.Died -= slime.Split;
                slime._damageable.DeathSound = Sfx.DeathGoo;
                ScrapHealth.Died += slime.Vanish;
            }
        }

        private void Update()
        {
            if (_damageable.IsDead) return;

            // Apanhou: espirra, parada.
            if (Time.time - _damageable.LastHitTime < 0.2f)
            {
                Show(Hurt);
                return;
            }

            Vector3 position = transform.position;
            position.x += _direction * speed * Time.deltaTime;

            if (position.x <= minX) { position.x = minX; _direction = 1; }
            else if (position.x >= maxX) { position.x = maxX; _direction = -1; }

            transform.position = position;

            // Rasteja como lagarta: mais rapido, mais quadros.
            _clock += Time.deltaTime * (4f + speed * 2.5f);
            Show(Crawl + (int)_clock % CrawlFrames);

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = _direction < 0;
        }

        private void Show(int frame)
        {
            if (_sprite == null || frames == null || frame < 0 || frame >= frames.Length || frames[frame] == null) return;
            _sprite.sprite = frames[frame];
        }

        /// <summary>A morte: se esparrama numa poca e os olhos rolam pro chao.</summary>
        private IEnumerator Spread()
        {
            if (_sprite != null) _sprite.color = Color.white;

            for (int i = 0; i < DeathFrames; i++)
            {
                Show(Death + i);
                yield return new WaitForSeconds(1f / 12f);
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

            var controller = other.GetComponentInParent<ScrapController>();
            if (controller == null) return;

            // Encostar machuca e ainda gruda mercurio no corpo.
            var health = controller.GetComponent<ScrapHealth>();
            if (health != null) health.TakeDamage(1, transform.position);

            controller.Slow(1.5f);
        }
    }
}
