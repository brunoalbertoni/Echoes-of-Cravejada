using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Pipe Leech: uma criatura de borracha e cobre grudada num cano do teto.
    /// Fica parada ate o Scrap passar embaixo, e entao despenca. No chao, ela
    /// se arrasta devagar atras dele.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PipeLeech : MonoBehaviour
    {
        [Tooltip("A que distancia, de lado, ela percebe o Scrap embaixo.")]
        [SerializeField] private float triggerRange = 1.3f;

        [Tooltip("Altura do chao embaixo dela, em y de mundo.")]
        [SerializeField] private float floorY;

        [SerializeField] private float crawlSpeed = 1.1f;

        [Tooltip("Ate onde ela se arrasta, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;

        private Transform _player;
        private SpriteRenderer _sprite;
        private Vector3 _home;
        private float _fallSpeed;
        private int _state;   // 0 no teto, 1 caindo, 2 no chao

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newFloorY, float newMinX, float newMaxX)
        {
            floorY = newFloorY;
            minX = newMinX;
            maxX = newMaxX;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _home = transform.position;

            var damageable = GetComponent<Damageable>();
            if (damageable != null) damageable.Revived += BackToCeiling;
        }

        private void Start()
        {
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null) _player = controller.transform;
        }

        private void BackToCeiling()
        {
            _state = 0;
            _fallSpeed = 0f;
            transform.position = _home;
            if (_sprite != null) _sprite.flipY = true;
        }

        private void OnEnable()
        {
            // No teto ela fica de cabeca pra baixo.
            if (_sprite != null) _sprite.flipY = _state == 0;
        }

        private void Update()
        {
            if (_player == null) return;

            Vector3 position = transform.position;

            if (_state == 0)
            {
                bool under = Mathf.Abs(_player.position.x - position.x) < triggerRange
                    && _player.position.y < position.y;

                if (under) _state = 1;
                return;
            }

            if (_state == 1)
            {
                _fallSpeed += 40f * Time.deltaTime;
                position.y -= _fallSpeed * Time.deltaTime;

                if (position.y <= floorY)
                {
                    position.y = floorY;
                    _state = 2;
                    if (_sprite != null) _sprite.flipY = false;
                }

                transform.position = position;
                return;
            }

            float direction = Mathf.Sign(_player.position.x - position.x);
            position.x = Mathf.Clamp(position.x + direction * crawlSpeed * Time.deltaTime, minX, maxX);
            transform.position = position;

            if (_sprite != null) _sprite.flipX = direction > 0f;
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
