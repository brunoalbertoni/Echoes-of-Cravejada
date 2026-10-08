using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Failed Experiment: um automato de prata e cobre costurados, resultado de
    /// um experimento. Metade do corpo e rapida, metade e lenta — entao ele
    /// alterna, sem aviso de ritmo, entre arrastar a perna de cobre e disparar
    /// com a de prata.
    ///
    /// O arrasto e lento e da pra ler. O disparo tem um aviso curto: ele trava
    /// e treme antes de sair.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class FailedExperiment : MonoBehaviour
    {
        [SerializeField] private float dragSpeed = 1.1f;
        [SerializeField] private float lungeSpeed = 9f;

        [Tooltip("Limites de onde ele anda, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;

        [Tooltip("Ate onde ele percebe o Scrap, em tiles.")]
        [SerializeField] private float sight = 9f;

        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private Transform _player;
        private int _direction = 1;
        private bool _lunging;

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
        }

        private void Start()
        {
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null) _player = controller.transform;
        }

        private void OnEnable()
        {
            _lunging = false;
            StartCoroutine(Brain());
        }

        private IEnumerator Brain()
        {
            while (true)
            {
                // A metade de cobre: arrasta.
                float dragTime = Random.Range(0.8f, 2.2f);

                for (float t = 0f; t < dragTime; t += Time.deltaTime)
                {
                    if (Time.time - _damageable.LastHitTime > 0.25f) Step(dragSpeed);
                    yield return null;
                }

                if (!Sees()) continue;

                // A metade de prata: trava, treme e dispara.
                _direction = _player.position.x >= transform.position.x ? 1 : -1;
                Face();

                Vector3 anchor = transform.position;

                for (float t = 0f; t < 0.35f; t += Time.deltaTime)
                {
                    transform.position = anchor + Vector3.right * (Mathf.Sin(t * 90f) * 0.06f);
                    yield return null;
                }

                transform.position = anchor;
                _lunging = true;

                for (float t = 0f; t < 0.45f; t += Time.deltaTime)
                {
                    Step(lungeSpeed);
                    yield return null;
                }

                _lunging = false;
                yield return new WaitForSeconds(0.5f);
            }
        }

        private bool Sees()
        {
            if (_player == null) return false;

            Vector2 offset = _player.position - transform.position;
            return Mathf.Abs(offset.x) < sight && Mathf.Abs(offset.y) < 3f;
        }

        private void Step(float speed)
        {
            Vector3 position = transform.position;
            position.x += _direction * speed * Time.deltaTime;

            if (position.x <= minX) { position.x = minX; _direction = 1; }
            else if (position.x >= maxX) { position.x = maxX; _direction = -1; }

            transform.position = position;
            Face();
        }

        private void Face()
        {
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
            if (health != null) health.TakeDamage(_lunging ? 2 : 1, transform.position);
        }
    }
}
