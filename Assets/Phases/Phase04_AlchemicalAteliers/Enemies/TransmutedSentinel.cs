using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Transmuted Sentinel: uma armadura oca de vidro, movida por uma chama
    /// verde de Oxide que aparece pelo vidro. Muda de material quando e
    /// atingida:
    ///
    ///   vidro   translucida, a chama a vista. Fragil: o cinzel fere.
    ///   prata   opaca e polida. Blindada: o golpe so tine.
    ///
    /// Cada golpe que acerta no vidro a transmuta em prata por alguns segundos.
    /// Nao adianta martelar: e bater, recuar, esperar o vidro voltar. O raio de
    /// luz alquimica nao quer saber de material — queima do mesmo jeito.
    ///
    /// Os dois materiais sao duas folhas com os mesmos quadros (GlassSentinelArt).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Damageable))]
    public class TransmutedSentinel : MonoBehaviour
    {
        // A ordem dos quadros. E a mesma do GlassSentinelArt, no editor.
        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Walk = 4;
        public const int WalkFrames = 8;
        public const int Hurt = 12;
        public const int Death = 13;
        public const int DeathFrames = 6;

        [SerializeField] private float speed = 1.5f;

        [Tooltip("Limites de onde ela anda, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;

        [Tooltip("Segundos que ela fica em prata depois de um golpe.")]
        [SerializeField] private float silverTime = 2.6f;

        [Tooltip("Os quadros de vidro e os de prata, na ordem das constantes.")]
        [SerializeField] private Sprite[] glassFrames;
        [SerializeField] private Sprite[] silverFrames;

        private SpriteRenderer _sprite;
        private Damageable _damageable;
        private Transform _player;
        private float _silverUntil;
        private float _seenHit = -100f;
        private float _clock;

        public bool IsSilver => Time.time < _silverUntil;

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newMinX, float newMaxX, Sprite[] newGlassFrames = null, Sprite[] newSilverFrames = null)
        {
            minX = newMinX;
            maxX = newMaxX;
            if (newGlassFrames != null) glassFrames = newGlassFrames;
            if (newSilverFrames != null) silverFrames = newSilverFrames;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _damageable = GetComponent<Damageable>();
            _clock = Mathf.Abs(transform.position.x * 0.27f);

            // Em prata ela apara tudo, venha de onde vier.
            _damageable.Blocks = from => IsSilver;
            _damageable.Revived += () => _silverUntil = 0f;
            _damageable.DeathAnimation = Shatter;
            _damageable.DeathSound = Sfx.DeathGlass;
        }

        private void Start()
        {
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null) _player = controller.transform;
        }

        private void Update()
        {
            if (_damageable.IsDead) return;

            // Um golpe novo entrou no vidro: vira prata.
            if (_damageable.LastHitTime > _seenHit)
            {
                _seenHit = _damageable.LastHitTime;
                _silverUntil = Time.time + silverTime;
                Sfx.Transmute.Play(transform.position);
            }

            // O material e o aviso. Perto de voltar a vidro, ela pisca.
            float left = _silverUntil - Time.time;
            bool showSilver = left > 0.6f || (left > 0f && Mathf.FloorToInt(left * 12f) % 2 == 0);

            bool walking = false;
            if (_player != null)
            {
                Vector2 offset = _player.position - transform.position;
                if (Mathf.Abs(offset.x) <= 10f && Mathf.Abs(offset.y) <= 3f && Mathf.Abs(offset.x) >= 0.3f)
                {
                    // Em prata ela e pesada: anda pela metade.
                    float pace = IsSilver ? speed * 0.5f : speed;
                    float direction = Mathf.Sign(offset.x);

                    Vector3 position = transform.position;
                    float before = position.x;
                    position.x = Mathf.Clamp(position.x + direction * pace * Time.deltaTime, minX, maxX);
                    transform.position = position;
                    walking = !Mathf.Approximately(before, position.x);

                    // A arte olha pra direita.
                    if (_sprite != null) _sprite.flipX = direction < 0f;
                }
            }

            int frame;
            if (Time.time - _damageable.LastHitTime < 0.15f)
            {
                frame = Hurt;
            }
            else if (walking)
            {
                _clock += Time.deltaTime * (IsSilver ? 5f : 9f);
                frame = Walk + (int)_clock % WalkFrames;
            }
            else
            {
                _clock += Time.deltaTime * 4f;
                frame = Idle + (int)_clock % IdleFrames;
            }

            Show(showSilver ? silverFrames : glassFrames, frame);
        }

        private void Show(Sprite[] frames, int frame)
        {
            if (_sprite == null || frames == null || frame < 0 || frame >= frames.Length || frames[frame] == null) return;
            _sprite.sprite = frames[frame];
        }

        /// <summary>A morte (so morre de vidro): racha inteira, vira cacos e a chama escapa.</summary>
        private IEnumerator Shatter()
        {
            if (_sprite != null) _sprite.color = Color.white;

            for (int i = 0; i < DeathFrames; i++)
            {
                Show(glassFrames, Death + i);
                yield return new WaitForSeconds(i == 0 ? 0.15f : 0.1f);
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
            if (health != null) health.TakeDamage(1, transform.position);
        }
    }
}
