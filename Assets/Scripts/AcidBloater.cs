using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Acid Bloater: um tanque ambulante de acido. Lento e resistente. Ao
    /// morrer ele estoura e deixa uma poca de acido no chao por alguns
    /// segundos — entao e melhor nao mata-lo onde voce vai pisar.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class AcidBloater : MonoBehaviour
    {
        [SerializeField] private float speed = 0.8f;

        [Tooltip("Limites da patrulha, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;

        [Tooltip("Segundos que a poca fica no chao.")]
        [SerializeField] private float puddleTime = 4f;

        [SerializeField] private Sprite puddleSprite;

        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private int _direction = 1;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newMinX, float newMaxX, Sprite newPuddleSprite)
        {
            minX = newMinX;
            maxX = newMaxX;
            puddleSprite = newPuddleSprite;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _damageable = GetComponent<Damageable>();
            _damageable.Died += Splash;
        }

        private void Splash()
        {
            // Os pes dele ficam meio tile abaixo do centro.
            Vector3 ground = transform.position + Vector3.down * 0.75f;
            AcidPuddle.Spill(ground, 3f, puddleTime, puddleSprite);
        }

        private void Update()
        {
            Vector3 position = transform.position;

            if (Time.time - _damageable.LastHitTime < 0.2f)
            {
                position.x = Mathf.Clamp(position.x - _damageable.LastHitSide * 2f * Time.deltaTime, minX, maxX);
                transform.position = position;
                return;
            }

            if (position.x >= maxX) _direction = -1;
            else if (position.x <= minX) _direction = 1;

            position.x = Mathf.Clamp(position.x + _direction * speed * Time.deltaTime, minX, maxX);
            transform.position = position;

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = _direction < 0;
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
