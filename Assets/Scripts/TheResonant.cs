using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// The Resonant, o elite do District 3: um automato grande com um diapasao
    /// de cristal no lugar do braco. A luta e opcional — ele guarda a safira.
    ///
    /// Anda devagar atras do Scrap e, de tempos em tempos, bate o diapasao no
    /// chao. A batida faz tres coisas de uma vez: solta uma onda pra cada lado
    /// (que so se evita pulando), acende a sala inteira e acorda tudo o que
    /// escuta por perto. Ele nao e blindado: o perigo e o que ele chama.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TheResonant : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 14;

        [Header("Sala")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField] private Rect wakeArea;

        [Tooltip("Ate onde a batida do diapasao se ouve, em tiles.")]
        [SerializeField] private float pulseRadius = 22f;

        [Header("O que ele usa")]
        [SerializeField] private Sprite waveSprite;
        [SerializeField] private Sprite shardSprite;
        [SerializeField] private Sprite rewardSprite;

        private SpriteRenderer _sprite;
        private Transform _player;
        private Vector3 _home;
        private int _health;
        private bool _awake;
        private bool _dead;
        private int _facing = -1;
        private float _hurtUntil;

        private static bool _defeated;

        /// <summary>O elite em luta, se houver. O HUD usa pra mostrar a barra de vida.</summary>
        public static TheResonant Fighting { get; private set; }

        public float HealthFraction => Mathf.Clamp01(_health / (float)Mathf.Max(1, maxHealth));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Fighting = null;
            _defeated = false;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newMinX, float newMaxX, Rect newWakeArea,
            Sprite wave, Sprite shard, Sprite reward)
        {
            minX = newMinX;
            maxX = newMaxX;
            wakeArea = newWakeArea;
            waveSprite = wave;
            shardSprite = shard;
            rewardSprite = reward;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _health = maxHealth;
            _home = transform.position;

            ScrapHealth.Died += ResetFight;
        }

        private void Start()
        {
            if (_defeated)
            {
                // Caiu numa visita anterior e a safira ficou pra tras: ela volta.
                if (!ScrapGems.HasSapphire) DropSapphire(transform.position);

                Destroy(gameObject);
                return;
            }

            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null) _player = controller.transform;
        }

        private void OnDestroy()
        {
            ScrapHealth.Died -= ResetFight;
            if (Fighting == this) Fighting = null;
        }

        private void Update()
        {
            if (_sprite != null && !_dead && Time.time >= _hurtUntil)
            {
                _sprite.color = Color.Lerp(_sprite.color, Color.white, 10f * Time.deltaTime);
            }

            if (_awake || _dead || _player == null) return;
            if (!wakeArea.Contains(_player.position)) return;

            _awake = true;
            Fighting = this;
            StartCoroutine(Brain());
        }

        /// <summary>O Scrap morreu: a luta recomeca do zero.</summary>
        private void ResetFight()
        {
            if (_dead) return;

            StopAllCoroutines();

            _health = maxHealth;
            _awake = false;
            transform.position = _home;

            if (_sprite != null) _sprite.color = Color.white;
            if (Fighting == this) Fighting = null;
        }

        private IEnumerator Brain()
        {
            // Ele abre a luta com uma batida: e assim que a sala aparece.
            yield return Slam();

            while (!_dead)
            {
                float walkTime = Random.Range(1.4f, 2.4f);

                for (float t = 0f; t < walkTime; t += Time.deltaTime)
                {
                    FacePlayer();

                    Vector3 position = transform.position;
                    position.x = Mathf.Clamp(position.x + _facing * 1.7f * Time.deltaTime, minX, maxX);
                    transform.position = position;

                    yield return null;
                }

                yield return Slam();
            }
        }

        private IEnumerator Slam()
        {
            FacePlayer();

            // O aviso: ele ergue o diapasao e pisca.
            for (float t = 0f; t < 0.75f; t += Time.deltaTime)
            {
                if (_sprite != null && Mathf.FloorToInt(t * 12f) % 2 == 0) _sprite.color = new Color(0.75f, 0.92f, 1f);
                yield return null;
            }

            Vector3 place = transform.position;

            NoiseSystem.Emit(place + Vector3.up, pulseRadius, 1f);

            ShockWave.Launch(place + Vector3.right * 1.4f, 7f, minX - 1f, maxX + 1f, waveSprite);
            ShockWave.Launch(place + Vector3.left * 1.4f, -7f, minX - 1f, maxX + 1f, waveSprite);

            // Enquanto o diapasao vibra ele fica parado: e a hora de bater.
            yield return new WaitForSeconds(1.3f);
        }

        private void FacePlayer()
        {
            if (_player == null) return;

            _facing = _player.position.x >= transform.position.x ? 1 : -1;

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = _facing < 0;
        }

        public void Damage(int amount, Vector2 from)
        {
            if (_dead || !_awake) return;

            _health -= amount;
            _hurtUntil = Time.time + 0.12f;
            if (_sprite != null) _sprite.color = new Color(1f, 0.35f, 0.3f);

            if (_health > 0) return;

            _dead = true;
            _defeated = true;
            StopAllCoroutines();

            for (int i = 0; i < 12; i++)
            {
                ShardPickup.Spawn(transform.position + Vector3.up * 0.2f, shardSprite, 1, i);
            }

            DropSapphire(transform.position);
            Destroy(gameObject);
        }

        private void DropSapphire(Vector3 place)
        {
            RewardPickup.Drop(place, rewardSprite, RewardPickup.Kind.Sapphire, Color.white);
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
            if (_dead || !_awake) return;

            var health = other.GetComponentInParent<ScrapHealth>();
            if (health != null) health.TakeDamage(1, transform.position);
        }
    }
}
