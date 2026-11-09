using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Echo Hunter: um mineiro do turno do Capataz, corrompido, de olhos
    /// vendados e uma trompa de cristal no lugar da lanterna. Ele nao ve o
    /// Scrap — so ouve.
    ///
    /// Fica parado ate um ruido chegar nele. Entao corre ate a ORIGEM do som,
    /// fareja um instante e, sem ouvir mais nada, volta devagar pro lugar. Da
    /// pra passar por ele so andando e pulando baixo — sem dash, sem golpe, sem
    /// queda alta — ou manda-lo pra longe golpeando um cristal ressonante. Se
    /// ele alcancar, machuca muito.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class EchoHunter : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do HunterArt, no editor.
        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Alert = 4;
        public const int AlertFrames = 2;
        public const int Run = 6;
        public const int RunFrames = 6;
        public const int Sniff = 12;
        public const int SniffFrames = 4;
        public const int Walk = 16;
        public const int WalkFrames = 6;
        public const int Hurt = 22;
        public const int Death = 23;
        public const int DeathFrames = 6;

        [SerializeField] private float chargeSpeed = 7f;
        [SerializeField] private float returnSpeed = 1.6f;

        [Tooltip("Ruido mais fraco que isso ele nao percebe. Passo faz 0.15 e pulo 0.2; golpe 0.8, dash 1.")]
        [SerializeField, Range(0f, 1f)] private float hearingThreshold = 0.3f;

        [Tooltip("Limites de onde ele consegue ir, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;

        [SerializeField] private int contactDamage = 2;

        [Tooltip("Todos os quadros, na ordem das constantes.")]
        [SerializeField] private Sprite[] frames;

        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private Vector3 _home;
        private float _targetX;
        private float _sniffUntil;
        private int _state;   // 0 parado, 1 correndo pro som, 2 farejando, 3 voltando
        private float _stateSince;
        private float _clock;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newMinX, float newMaxX, Sprite[] newFrames = null)
        {
            minX = newMinX;
            maxX = newMaxX;
            if (newFrames != null) frames = newFrames;
        }

        /// <summary>O mesmo Configure, devolvendo ele: o gerador encadeia.</summary>
        public EchoHunter ConfigureAndReturn(float newMinX, float newMaxX, Sprite[] newFrames = null)
        {
            Configure(newMinX, newMaxX, newFrames);
            return this;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _damageable = GetComponent<Damageable>();
            _home = transform.position;
            _clock = Mathf.Abs(transform.position.x * 0.29f);

            _damageable.Revived += GoHome;
            _damageable.DeathAnimation = Collapse;
            _damageable.DeathSound = Sfx.DeathHunter;
        }

        private void OnDestroy()
        {
            if (_damageable != null) _damageable.Revived -= GoHome;
        }

        private void OnEnable()
        {
            NoiseSystem.Heard += OnNoise;
        }

        private void OnDisable()
        {
            NoiseSystem.Heard -= OnNoise;
        }

        private void GoHome()
        {
            SetState(0);
            transform.position = _home;
        }

        private void SetState(int state)
        {
            if (_state == state) return;
            _state = state;
            _stateSince = Time.time;

            // Ouviu: o estalo da trompa antes de sair correndo.
            if (state == 1) Sfx.HunterAlert.Play(transform.position);
        }

        private void OnNoise(Vector2 origin, float radius, float loudness)
        {
            if (_damageable.IsDead || loudness < hearingThreshold) return;

            // Ele escuta na mesma altura, mais ou menos: um andar acima ja abafa.
            Vector2 offset = origin - (Vector2)transform.position;
            if (Mathf.Abs(offset.y) > 5f || offset.magnitude > radius + 2f) return;

            _targetX = Mathf.Clamp(origin.x, minX, maxX);
            SetState(1);
        }

        private void Update()
        {
            if (_damageable.IsDead) return;

            Vector3 position = transform.position;

            // Apanhou: cambaleia, e passa a saber onde o Scrap esta.
            if (Time.time - _damageable.LastHitTime < 0.25f)
            {
                position.x = Mathf.Clamp(position.x - _damageable.LastHitSide * 3f * Time.deltaTime, minX, maxX);
                transform.position = position;
                Show(Hurt);
                return;
            }

            Animate();

            if (_state == 0) return;

            if (_state == 2)
            {
                if (Time.time >= _sniffUntil) SetState(3);
                return;
            }

            float goal = _state == 1 ? _targetX : _home.x;
            float speed = _state == 1 ? chargeSpeed : returnSpeed;

            position.x = Mathf.MoveTowards(position.x, goal, speed * Time.deltaTime);
            transform.position = position;

            // A arte olha pra direita.
            if (_sprite != null && !Mathf.Approximately(goal, position.x)) _sprite.flipX = goal < position.x;

            if (!Mathf.Approximately(position.x, goal)) return;

            if (_state == 1)
            {
                SetState(2);
                _sniffUntil = Time.time + 1.6f;
            }
            else
            {
                SetState(0);
            }
        }

        /// <summary>Escutando, de estalo, correndo, farejando ou voltando.</summary>
        private void Animate()
        {
            float since = Time.time - _stateSince;
            _clock += Time.deltaTime;

            switch (_state)
            {
                case 1:
                    // O estalo de quem ouviu, e a corrida.
                    if (since < 0.16f) Show(Alert + Mathf.Min(AlertFrames - 1, (int)(since / 0.08f)));
                    else Show(Run + (int)(Time.time * 13f) % RunFrames);
                    break;

                case 2:
                    Show(Sniff + (int)(since * 6f) % SniffFrames);
                    break;

                case 3:
                    Show(Walk + (int)(_clock * 6f) % WalkFrames);
                    break;

                default:
                    Show(Idle + (int)(_clock * 4f) % IdleFrames);
                    break;
            }
        }

        private void Show(int frame)
        {
            if (_sprite == null || frames == null || frame < 0 || frame >= frames.Length || frames[frame] == null) return;
            _sprite.sprite = frames[frame];
        }

        /// <summary>A morte: leva as maos a trompa, cai de joelhos, tomba e a trompa se desfaz.</summary>
        private IEnumerator Collapse()
        {
            if (_sprite != null) _sprite.color = Color.white;

            float[] hold = { 0.12f, 0.15f, 0.18f, 0.2f, 0.2f, 0.25f };
            for (int i = 0; i < DeathFrames; i++)
            {
                Show(Death + i);
                yield return new WaitForSeconds(hold[i]);
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

            var health = other.GetComponentInParent<ScrapHealth>();
            if (health != null) health.TakeDamage(contactDamage, transform.position);
        }
    }
}
