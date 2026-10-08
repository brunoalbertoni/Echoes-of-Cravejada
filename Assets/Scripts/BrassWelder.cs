using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Brass Welder: um Tarnished corrompido, ainda de mascara de soldador.
    /// Patrulha um trecho de chao e, quando ve o Scrap, vira pra ele e avanca
    /// com o macarico.
    ///
    /// Ele apara todo golpe que vem de frente. So da pra feri-lo por tras ou
    /// por cima — pulando e batendo pra baixo.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class BrassWelder : MonoBehaviour
    {
        [SerializeField] private float speed = 1.5f;

        [Tooltip("A que distancia ele percebe o Scrap e parte pra cima.")]
        [SerializeField] private float sightRange = 6f;

        [Tooltip("Limites da patrulha, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;

        private Transform _player;
        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private int _facing = 1;

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
            _damageable.Blocks = BlocksHitFrom;
        }

        private void Start()
        {
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null) _player = controller.transform;
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

            return Mathf.Sign(offset.x) == _facing;
        }

        private void Update()
        {
            Vector3 position = transform.position;

            // Logo depois de apanhar ele cambaleia pra longe do golpe.
            if (Time.time - _damageable.LastHitTime < 0.3f)
            {
                position.x = Mathf.Clamp(position.x - _damageable.LastHitSide * 3f * Time.deltaTime, minX, maxX);
                transform.position = position;
                return;
            }

            bool sees = _player != null
                && Mathf.Abs(_player.position.x - position.x) < sightRange
                && Mathf.Abs(_player.position.y - position.y) < 2.5f;

            float pace = speed;

            if (sees)
            {
                _facing = _player.position.x >= position.x ? 1 : -1;
                pace = speed * 1.7f;
            }
            else if (position.x >= maxX) _facing = -1;
            else if (position.x <= minX) _facing = 1;

            position.x = Mathf.Clamp(position.x + _facing * pace * Time.deltaTime, minX, maxX);
            transform.position = position;

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = _facing < 0;
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
