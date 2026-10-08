using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// The Rusty Scrapper, o mini boss do District 1: o coletor do poco.
    ///
    /// E o primeiro boss do jogo, entao e lento e avisa tudo o que vai fazer —
    /// ele pisca em amarelo antes de cada golpe.
    ///
    ///   Garra:      avanca reto e golpeia.
    ///   Arremesso:  joga tres pedacos de sucata em arco.
    ///   Fase 2 (metade da vida): salta e bate no chao, soltando duas ondas de
    ///               choque que so se evita pulando.
    ///
    /// Ao chegar na metade ele solta uma nuvem densa de Oxide — que nao faz
    /// nada ao Scrap. Ao cair, deixa o Steam Dash, abre a saida e purifica a area.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RustyScrapper : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 14;

        [Header("Arena")]
        [Tooltip("Ate onde ele anda, em x de mundo, e a altura do chao em que pisa.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField] private float floorY;

        [Tooltip("A area que acorda o boss quando o Scrap entra.")]
        [SerializeField] private Rect wakeArea;

        [Header("O que ele usa")]
        [SerializeField] private Sprite chunkSprite;
        [SerializeField] private Sprite waveSprite;
        [SerializeField] private Sprite shardSprite;
        [SerializeField] private Sprite dashSprite;
        [SerializeField] private ParticleSystem oxideCloud;

        [Header("O que acontece quando ele cai")]
        [SerializeField] private Gate exitGate;
        [SerializeField] private AreaPurifier purifier;

        private SpriteRenderer _sprite;
        private Transform _player;
        private int _health;
        private bool _awake;
        private bool _phaseTwo;
        private bool _dead;
        private int _facing = -1;
        private Vector3 _home;

        /// <summary>O boss em luta, se houver. O HUD usa pra mostrar a barra de vida.</summary>
        public static RustyScrapper Fighting { get; private set; }

        public float HealthFraction => Mathf.Clamp01(_health / (float)Mathf.Max(1, maxHealth));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Fighting = null;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newMinX, float newMaxX, float newFloorY, Rect newWakeArea,
            Sprite chunk, Sprite wave, Sprite shard, Sprite dash, ParticleSystem cloud,
            Gate gate, AreaPurifier newPurifier)
        {
            minX = newMinX;
            maxX = newMaxX;
            floorY = newFloorY;
            wakeArea = newWakeArea;
            chunkSprite = chunk;
            waveSprite = wave;
            shardSprite = shard;
            dashSprite = dash;
            oxideCloud = cloud;
            exitGate = gate;
            purifier = newPurifier;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _health = maxHealth;
            _home = transform.position;

            ScrapHealth.Died += ResetFight;
        }

        /// <summary>
        /// O Scrap morreu: a luta recomeca do zero. Ele volta pro lugar, enche
        /// a vida, esquece a fase 2 e dorme ate o Scrap entrar na arena de novo.
        /// </summary>
        private void ResetFight()
        {
            if (_dead) return;

            StopAllCoroutines();

            _health = maxHealth;
            _phaseTwo = false;
            _awake = false;
            _facing = -1;
            transform.position = _home;

            if (_sprite != null)
            {
                _sprite.color = Color.white;
                _sprite.flipX = false;
            }

            if (Fighting == this) Fighting = null;
        }

        private void Start()
        {
            // Ja foi derrotado nesta sessao: a arena fica vazia.
            if (AreaPurifier.IsPurified)
            {
                Destroy(gameObject);
                return;
            }

            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null) _player = controller.transform;
        }

        private void Update()
        {
            if (_awake || _dead || _player == null) return;
            if (!wakeArea.Contains(_player.position)) return;

            _awake = true;
            Fighting = this;
            StartCoroutine(Brain());
        }

        private void OnDestroy()
        {
            ScrapHealth.Died -= ResetFight;
            if (Fighting == this) Fighting = null;
        }

        // ------------------------------------------------------------------
        // Cerebro
        // ------------------------------------------------------------------

        private IEnumerator Brain()
        {
            yield return new WaitForSeconds(0.8f);

            while (!_dead)
            {
                // Anda um pouco na direcao do Scrap antes de decidir.
                float walkTime = Random.Range(0.7f, 1.3f);
                float walkSpeed = _phaseTwo ? 2.4f : 1.6f;

                for (float t = 0f; t < walkTime; t += Time.deltaTime)
                {
                    FacePlayer();
                    Move(_facing * walkSpeed * Time.deltaTime);
                    yield return null;
                }

                float distance = Mathf.Abs(_player.position.x - transform.position.x);
                float roll = Random.value;

                if (_phaseTwo && roll < 0.35f) yield return Slam();
                else if (distance > 6f || roll < 0.45f) yield return Throw();
                else yield return Claw();
            }
        }

        private IEnumerator Claw()
        {
            FacePlayer();
            yield return Telegraph(_phaseTwo ? 0.4f : 0.6f);

            int direction = _facing;
            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                Move(direction * 9f * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(0.7f);
        }

        private IEnumerator Throw()
        {
            FacePlayer();
            yield return Telegraph(0.5f);

            const float gravity = 18f;
            const float launch = 9.5f;

            // Tempo de voo ate voltar a altura do chao, saindo de 2 tiles acima.
            float flight = (launch + Mathf.Sqrt(launch * launch + 2f * gravity * 2f)) / gravity;
            Vector3 hand = transform.position + new Vector3(_facing * 0.8f, 2f, 0f);

            // Um no Scrap, um antes e um depois: ficar parado nao salva.
            for (int i = -1; i <= 1; i++)
            {
                float targetX = _player.position.x + i * 2.5f;
                var velocity = new Vector2((targetX - hand.x) / flight, launch);
                ScrapChunk.Throw(hand, velocity, gravity, floorY, chunkSprite);
            }

            yield return new WaitForSeconds(0.9f);
        }

        private IEnumerator Slam()
        {
            FacePlayer();
            yield return Telegraph(0.6f);

            Vector3 from = transform.position;
            float targetX = Mathf.Clamp(_player.position.x, minX, maxX);

            // Sobe num arco ate em cima do Scrap...
            const float jumpTime = 0.55f;
            for (float t = 0f; t < jumpTime; t += Time.deltaTime)
            {
                float k = t / jumpTime;
                float height = Mathf.Sin(k * Mathf.PI * 0.5f) * 5f;
                transform.position = new Vector3(Mathf.Lerp(from.x, targetX, k), floorY + height, 0f);
                yield return null;
            }

            // ...paira um instante, que e a hora de sair de baixo...
            yield return new WaitForSeconds(0.25f);

            // ...e despenca.
            const float fallTime = 0.16f;
            float top = transform.position.y;
            for (float t = 0f; t < fallTime; t += Time.deltaTime)
            {
                transform.position = new Vector3(targetX, Mathf.Lerp(top, floorY, t / fallTime), 0f);
                yield return null;
            }

            transform.position = new Vector3(targetX, floorY, 0f);

            Vector3 ground = transform.position;
            ShockWave.Launch(ground + Vector3.right * 1.2f, 7f, minX - 1f, maxX + 1f, waveSprite);
            ShockWave.Launch(ground + Vector3.left * 1.2f, -7f, minX - 1f, maxX + 1f, waveSprite);

            yield return new WaitForSeconds(1.0f);
        }

        /// <summary>O aviso antes de cada golpe: ele pisca em amarelo.</summary>
        private IEnumerator Telegraph(float time)
        {
            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                if (_sprite != null)
                {
                    bool lit = Mathf.FloorToInt(t * 12f) % 2 == 0;
                    _sprite.color = lit ? new Color(1f, 0.9f, 0.4f) : Color.white;
                }

                yield return null;
            }

            if (_sprite != null) _sprite.color = Color.white;
        }

        private void FacePlayer()
        {
            if (_player == null) return;

            _facing = _player.position.x >= transform.position.x ? 1 : -1;

            // A arte olha pra esquerda.
            if (_sprite != null) _sprite.flipX = _facing > 0;
        }

        private void Move(float deltaX)
        {
            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x + deltaX, minX, maxX);
            position.y = floorY;
            transform.position = position;
        }

        // ------------------------------------------------------------------
        // Apanhar e cair
        // ------------------------------------------------------------------

        public void Damage(int amount, Vector2 from)
        {
            if (_dead || !_awake) return;

            _health -= amount;

            if (_health <= 0)
            {
                Die();
                return;
            }

            if (!_phaseTwo && _health <= maxHealth / 2)
            {
                _phaseTwo = true;

                // A nuvem de Oxide. Nao tem gatilho de dano nenhum de
                // proposito: e aqui que o jogador ve que o Scrap e imune.
                if (oxideCloud != null) oxideCloud.Play();
            }

            StartCoroutine(Flash());
        }

        private IEnumerator Flash()
        {
            if (_sprite == null) yield break;

            _sprite.color = new Color(1f, 0.4f, 0.35f);
            yield return new WaitForSeconds(0.1f);
            _sprite.color = Color.white;
        }

        private void Die()
        {
            _dead = true;
            StopAllCoroutines();

            Vector3 place = new Vector3(transform.position.x, floorY, 0f);

            for (int i = 0; i < 12; i++)
            {
                ShardPickup.Spawn(place + Vector3.up * 0.2f, shardSprite, 1, i);
            }

            DashPickup.Drop(place, dashSprite);

            if (exitGate != null) exitGate.Open();
            if (purifier != null) purifier.Purify();

            Destroy(gameObject);
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
