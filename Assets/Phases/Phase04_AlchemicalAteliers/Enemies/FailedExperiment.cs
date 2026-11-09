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
    /// e treme antes de sair, e o olho branco acende.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class FailedExperiment : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do ExperimentArt, no editor.
        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Drag = 4;
        public const int DragFrames = 8;
        public const int Charge = 12;
        public const int ChargeFrames = 2;
        public const int Lunge = 14;
        public const int LungeFrames = 4;
        public const int Hurt = 18;
        public const int Death = 19;
        public const int DeathFrames = 7;

        [SerializeField] private float dragSpeed = 1.1f;
        [SerializeField] private float lungeSpeed = 9f;

        [Tooltip("Limites de onde ele anda, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;

        [Tooltip("Ate onde ele percebe o Scrap, em tiles.")]
        [SerializeField] private float sight = 9f;

        [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] frames;

        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private Transform _player;
        private int _direction = 1;
        private bool _lunging;

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
            _damageable.DeathAnimation = FallApart;
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

        private bool Hurting => Time.time - _damageable.LastHitTime < 0.25f;

        private IEnumerator Brain()
        {
            while (!_damageable.IsDead)
            {
                // A metade de cobre: arrasta.
                float dragTime = Random.Range(0.8f, 2.2f);

                for (float t = 0f; t < dragTime; t += Time.deltaTime)
                {
                    if (_damageable.IsDead) yield break;

                    if (Hurting)
                    {
                        Show(Hurt);
                    }
                    else
                    {
                        Step(dragSpeed);
                        Show(Drag + (int)(Time.time * 8f) % DragFrames);
                    }

                    yield return null;
                }

                if (!Sees()) continue;

                // A metade de prata: trava, treme e dispara.
                _direction = _player.position.x >= transform.position.x ? 1 : -1;
                Face();

                Vector3 anchor = transform.position;
                Sfx.ExperimentCharge.Play(anchor);
                for (float t = 0f; t < 0.35f; t += Time.deltaTime)
                {
                    if (_damageable.IsDead) yield break;
                    transform.position = anchor + Vector3.right * (Mathf.Sin(t * 90f) * 0.06f);
                    Show(Charge + (int)(t * 20f) % ChargeFrames);
                    yield return null;
                }

                transform.position = anchor;
                _lunging = true;
                Sfx.ExperimentLunge.Play(anchor);

                for (float t = 0f; t < 0.45f; t += Time.deltaTime)
                {
                    if (_damageable.IsDead) yield break;
                    Step(lungeSpeed);
                    Show(Lunge + (int)(t * 14f) % LungeFrames);
                    yield return null;
                }

                _lunging = false;

                for (float t = 0f; t < 0.5f; t += Time.deltaTime)
                {
                    if (_damageable.IsDead) yield break;
                    Show(Hurting ? Hurt : Idle + (int)(t * 6f) % IdleFrames);
                    yield return null;
                }
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

        private void Show(int frame)
        {
            if (_sprite == null || frames == null || frame < 0 || frame >= frames.Length || frames[frame] == null) return;
            _sprite.sprite = frames[frame];
        }

        /// <summary>A morte: a costura arrebenta e as metades caem cada uma pra um lado.</summary>
        private IEnumerator FallApart()
        {
            _lunging = false;
            if (_sprite != null) _sprite.color = Color.white;

            for (int i = 0; i < DeathFrames; i++)
            {
                Show(Death + i);
                yield return new WaitForSeconds(i == 0 ? 0.15f : 0.11f);
            }

            yield return new WaitForSeconds(0.2f);
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

            var health = other.GetComponentInParent<ScrapHealth>();
            if (health != null) health.TakeDamage(_lunging ? 2 : 1, transform.position);
        }
    }
}
