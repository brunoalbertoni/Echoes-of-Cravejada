using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Um autômato corrompido que rasteja de um lado pro outro num trecho de
    /// chao e machuca quem encosta. Nao persegue: e o primeiro inimigo do jogo,
    /// e existe pra ensinar a bater.
    ///
    /// O trecho e dado pelo gerador da fase, entao ele nao precisa sondar o
    /// chao: so vai e volta entre dois pontos.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CrawlerEnemy : MonoBehaviour
    {
        [SerializeField] private float speed = 1.4f;
        [SerializeField] private int contactDamage = 1;

        [Tooltip("Limites da patrulha, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;

        [Tooltip("Quadros da caminhada, em ordem.")]
        [SerializeField] private Sprite[] walkFrames;
        [SerializeField] private float framesPerSecond = 7f;

        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private int _direction = 1;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newMinX, float newMaxX, Sprite[] newWalkFrames)
        {
            minX = newMinX;
            maxX = newMaxX;
            walkFrames = newWalkFrames;
        }

        private void Awake()
        {
            _sprite = GetComponentInChildren<SpriteRenderer>();
            _damageable = GetComponent<Damageable>();

            // Cada um comeca indo pra um lado.
            _direction = Mathf.RoundToInt(transform.position.x) % 2 == 0 ? 1 : -1;
        }

        private void Update()
        {
            Vector3 position = transform.position;

            // Logo depois de apanhar ele e empurrado pra longe do golpe, parado.
            bool stunned = _damageable != null && Time.time - _damageable.LastHitTime < 0.25f;
            float step = stunned
                ? -_damageable.LastHitSide * 3.5f * Time.deltaTime
                : _direction * speed * Time.deltaTime;

            position.x = Mathf.Clamp(position.x + step, minX, maxX);

            if (!stunned)
            {
                if (position.x >= maxX) _direction = -1;
                else if (position.x <= minX) _direction = 1;
            }

            transform.position = position;

            if (_sprite == null) return;

            // A arte olha pra esquerda.
            _sprite.flipX = _direction > 0;

            if (walkFrames != null && walkFrames.Length > 0)
            {
                int frame = (int)(Time.time * framesPerSecond) % walkFrames.Length;
                _sprite.sprite = walkFrames[frame];
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
