using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O Capataz, o mini chefe do District 3: o primeiro mineiro a descer, o
    /// que "ouviu a pedra cantar". Um homem grande, cego — os olhos viraram
    /// quartzo —, de picareta na mao e a safira cravada no capacete. Ele anda
    /// pela toca e caca pelo SOM: passo so de perto, golpe e dash de longe.
    /// Encostado nele, sem fazer barulho, ele ainda sente o cheiro.
    ///
    ///   Picaretada  ouviu perto: ergue a picareta com as duas maos (pisca
    ///               amarelo) e crava no chao a frente. A ponta fica presa e
    ///               ele puxa o cabo — preso, o cinzel fere em dobro.
    ///   Investida   ouviu longe: abaixa a cabeca como carneiro e dispara ate
    ///               a parede. Nao para no meio. Bateu: o teto solta
    ///               estilhacos e ele fica tonto — tonto, fere em dobro.
    ///   Apito       de tempos em tempos chama o turno: o apito de capataz
    ///               acorda o chao e traças brotam das rachaduras.
    ///   Grito       na metade da vida: o teto despenca, e dali pra frente
    ///               ele fica mais rapido e chama o turno mais vezes.
    ///
    /// Morto, larga a picareta, ajoelha, o capacete cai e o rosto volta a
    /// ser de gente. Solta a safira e a CHAVE ANTIGA, que abre a grade do
    /// Salao das Estatuas. (A classe ainda se chama TheResonant pra nao
    /// quebrar o HUD e a vedacao da arena.)
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TheResonant : MonoBehaviour, IDamageable
    {
        // A ordem dos quadros. E a mesma do CaveMinerArt, no editor.
        public const int Dormant = 0;
        public const int DormantFrames = 4;
        public const int Wake = 4;
        public const int WakeFrames = 4;
        public const int Idle = 8;
        public const int IdleFrames = 6;
        public const int Walk = 14;
        public const int WalkFrames = 8;
        public const int SlamWindup = 22;
        public const int SlamWindupFrames = 2;
        public const int Slam = 24;
        public const int SlamFrames = 3;
        public const int Stuck = 27;
        public const int StuckFrames = 4;
        public const int ChargeWindup = 31;
        public const int ChargeWindupFrames = 2;
        public const int Charge = 33;
        public const int ChargeFrames = 4;
        public const int Dazed = 37;
        public const int DazedFrames = 4;
        public const int Whistle = 41;
        public const int WhistleFrames = 4;
        public const int Roar = 45;
        public const int RoarFrames = 4;
        public const int Hurt = 49;
        public const int Death = 50;
        public const int DeathFrames = 8;

        [SerializeField] private int maxHealth = 24;

        [Header("Toca")]
        [Tooltip("A area que o acorda. So ouve o que acontece dentro dela.")]
        [SerializeField] private Rect wakeArea;

        [Tooltip("Ate onde ele anda, em x de mundo.")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField] private float floorY;
        [SerializeField] private float ceilingY;

        [Header("Ouvido")]
        [Tooltip("Ate onde ele ouve passo e pulo (o resto, golpe, dash, queda, ele ouve na toca inteira).")]
        [SerializeField] private float closeHearing = 6f;

        [Tooltip("Perto assim, mesmo sem barulho, ele sente o cheiro.")]
        [SerializeField] private float smell = 1.6f;

        [Header("Golpes")]
        [Tooltip("Onde a picareta crava, em tiles a frente dele (CaveMinerArt.SlamReach).")]
        [SerializeField] private float slamReach = 2.1f;
        [SerializeField] private int slamDamage = 2;
        [SerializeField] private int chargeDamage = 2;
        [SerializeField] private int contactDamage = 1;

        [Header("Quadros")]
        [Tooltip("A folha inteira, na ordem do CaveMinerArt.")]
        [SerializeField] private Sprite[] frames;

        [Header("O que ele usa e deixa")]
        [SerializeField] private Sprite fallingShardSprite;
        [SerializeField] private Sprite shardSprite;
        [SerializeField] private Sprite rewardSprite;

        [Header("A chave")]
        [SerializeField] private string keyId;
        [SerializeField] private string keyMessage;
        [SerializeField] private Sprite keySprite;

        [Header("O turno")]
        [Tooltip("Os bichos que brotam quando ele apita, desligados na cena.")]
        [SerializeField] private GameObject[] brood;
        [SerializeField] private Transform[] broodSpots;
        [SerializeField] private SpriteRenderer[] broodCracks;
        [SerializeField] private Sprite[] crackFrames;
        [SerializeField] private int broodMaxAlive = 3;

        private SpriteRenderer _sprite;
        private Collider2D _collider;
        private Transform _player;
        private Vector3 _home;
        private int _health;
        private bool _awake;
        private bool _dead;
        private bool _phaseTwo;
        private bool _roarPending;
        private bool _selfNoise;
        private bool _charging;
        private bool _flinchable;
        private int _facing = -1;
        private float _exposedUntil;
        private float _hurtUntil;
        private float _nextWhistle;
        private bool _heard;
        private float _heardX;

        // A animacao tocando agora.
        private int _animStart;
        private int _animCount = DormantFrames;
        private float _animFps = 2.5f;
        private bool _animLoop = true;
        private float _animSince;

        private static bool _defeated;

        /// <summary>O Capataz em luta, se houver. O HUD usa pra mostrar a barra de vida.</summary>
        public static TheResonant Fighting { get; private set; }

        /// <summary>Se ele ja caiu de vez (a trilha sonora usa pra calar a musica da luta).</summary>
        public bool IsDown => _dead;

        public float HealthFraction => Mathf.Clamp01(_health / (float)Mathf.Max(1, maxHealth));

        /// <summary>Verdadeiro enquanto ele esta preso (picareta cravada) ou tonto: o cinzel fere em dobro.</summary>
        public bool IsExposed => Time.time < _exposedUntil;

        private float Speed => _phaseTwo ? 2.6f : 1.8f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Fighting = null;
            _defeated = false;
        }

        /// <summary>Usado pelos geradores de fase: a toca.</summary>
        public void Configure(Rect newWakeArea, float newMinX, float newMaxX, float newFloorY, float newCeilingY,
            Sprite newFallingShard, Sprite newShard, Sprite newReward)
        {
            wakeArea = newWakeArea;
            minX = newMinX;
            maxX = newMaxX;
            floorY = newFloorY;
            ceilingY = newCeilingY;
            fallingShardSprite = newFallingShard;
            shardSprite = newShard;
            rewardSprite = newReward;
        }

        /// <summary>Usado pelos geradores de fase: a folha de quadros e onde a picareta crava.</summary>
        public void ConfigureFrames(Sprite[] newFrames, float newSlamReach)
        {
            frames = newFrames;
            slamReach = newSlamReach;
        }

        /// <summary>Usado pelos geradores de fase: a chave que ele solta ao morrer.</summary>
        public void ConfigureKey(string newKeyId, string newKeyMessage, Sprite newKeySprite)
        {
            keyId = newKeyId;
            keyMessage = newKeyMessage;
            keySprite = newKeySprite;
        }

        /// <summary>Usado pelos geradores de fase: os bichos que brotam do chao quando ele apita.</summary>
        public void ConfigureBrood(GameObject[] newBrood, Transform[] newSpots, SpriteRenderer[] newCracks, Sprite[] newCrackFrames)
        {
            brood = newBrood;
            broodSpots = newSpots;
            broodCracks = newCracks;
            crackFrames = newCrackFrames;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
            _health = maxHealth;
            _home = transform.position;
            Face(-1);
            PlayDormant();

            ScrapHealth.Died += ResetFight;
        }

        private void OnEnable()
        {
            NoiseSystem.Heard += OnNoise;
        }

        private void OnDisable()
        {
            NoiseSystem.Heard -= OnNoise;
        }

        private void Start()
        {
            if (_defeated)
            {
                // Caiu numa visita anterior e a safira (ou a chave) ficou pra
                // tras: ela volta.
                Vector3 place = transform.position + Vector3.up * 0.6f;
                if (!ScrapGems.HasSapphire) DropSapphire(place);
                if (!string.IsNullOrEmpty(keyId) && !KeyItems.WasFound(keyId)) DropKey(place + Vector3.left * 1.2f);

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
                // Preso ou tonto: um pouco em brasa — e a deixa pra bater.
                Color target = IsExposed ? new Color(1f, 0.82f, 0.68f) : Color.white;
                _sprite.color = Color.Lerp(_sprite.color, target, 10f * Time.deltaTime);
            }

            if (_awake || _dead || _player == null) return;
            if (!wakeArea.Contains(_player.position)) return;

            WakeUp();
        }

        private void WakeUp()
        {
            _awake = true;
            Fighting = this;
            StartCoroutine(Brain());
        }

        // ------------------------------------------------------------------
        // Ouvir
        // ------------------------------------------------------------------

        private void OnNoise(Vector2 origin, float radius, float loudness)
        {
            if (!_awake || _dead || _selfNoise) return;
            if (!wakeArea.Contains(origin)) return;

            // Passo e pulo, so de perto; o resto ele ouve na toca inteira.
            float distance = Mathf.Abs(origin.x - transform.position.x);
            if (loudness < 0.3f && distance > closeHearing) return;

            Hear(origin.x);
        }

        private void Hear(float x)
        {
            _heard = true;
            _heardX = Mathf.Clamp(x, minX, maxX);
        }

        /// <summary>Colado nele, mesmo calado, o Scrap e farejado.</summary>
        private void Smell()
        {
            if (_player == null || _heard) return;

            Vector2 offset = _player.position - transform.position;
            if (Mathf.Abs(offset.x) <= smell && offset.y > -1f && offset.y < 3f) Hear(_player.position.x);
        }

        private void Emit(Vector2 origin, float radius, float loudness)
        {
            _selfNoise = true;
            NoiseSystem.Emit(origin, radius, loudness);
            _selfNoise = false;
        }

        // ------------------------------------------------------------------
        // Cerebro
        // ------------------------------------------------------------------

        private IEnumerator Brain()
        {
            // Acorda: ergue a cabeca, fareja e se levanta.
            if (_player != null) Face(_player.position.x >= transform.position.x ? 1 : -1);
            Play(Wake, WakeFrames, WakeFrames / 1.1f, false);
            yield return new WaitForSeconds(1.2f);

            // E a primeira coisa que faz e chamar o turno.
            yield return CallShift();

            while (!_dead)
            {
                if (_roarPending)
                {
                    _roarPending = false;
                    yield return Scream();
                }

                if (Time.time >= _nextWhistle && AliveBrood() < broodMaxAlive)
                {
                    yield return CallShift();
                    continue;
                }

                Smell();

                if (!_heard)
                {
                    yield return Listen();
                    continue;
                }

                float distance = Mathf.Abs(_heardX - transform.position.x);
                Face(_heardX >= transform.position.x ? 1 : -1);

                if (distance <= slamReach + 0.5f)
                {
                    _heard = false;
                    yield return PickSlam();
                }
                else if (distance > 7f || (distance > 4f && Random.value < (_phaseTwo ? 0.5f : 0.3f)))
                {
                    _heard = false;
                    yield return Ram();
                }
                else
                {
                    yield return Approach();
                }
            }
        }

        /// <summary>Nada ouvido: escuta parado, e anda devagar pra um lado e pro outro.</summary>
        private IEnumerator Listen()
        {
            _flinchable = true;
            Play(Idle, IdleFrames, 5f);

            for (float t = 0f; t < 0.9f && !_heard; t += Time.deltaTime)
            {
                Smell();
                yield return null;
            }

            if (_heard) yield break;

            // Uns passos pra onde a toca e mais larga.
            float middle = (minX + maxX) * 0.5f;
            int direction = transform.position.x < middle ? 1 : -1;
            if (Random.value < 0.35f) direction = -direction;
            Face(direction);

            Play(Walk, WalkFrames, 7f);
            for (float t = 0f; t < 1.1f && !_heard; t += Time.deltaTime)
            {
                Move(direction * Speed * 0.55f * Time.deltaTime);
                Smell();
                yield return null;
            }

            _flinchable = false;
        }

        /// <summary>Anda na direcao do que ouviu, ate ficar ao alcance da picareta.</summary>
        private IEnumerator Approach()
        {
            _flinchable = true;
            Play(Walk, WalkFrames, Speed * 4.5f);

            for (float t = 0f; t < 0.7f; t += Time.deltaTime)
            {
                float distance = _heardX - transform.position.x;
                if (Mathf.Abs(distance) <= slamReach) break;

                Face(distance >= 0f ? 1 : -1);
                if (!Move(_facing * Speed * Time.deltaTime)) break;
                yield return null;
            }

            _flinchable = false;
        }

        /// <summary>A picaretada: ergue, crava na frente, fica preso puxando o cabo.</summary>
        private IEnumerator PickSlam()
        {
            float windup = _phaseTwo ? 0.45f : 0.6f;
            Play(SlamWindup, SlamWindupFrames, SlamWindupFrames / windup, false);
            yield return Warn(windup);

            Play(Slam, SlamFrames, SlamFrames / 0.3f, false);
            yield return new WaitForSeconds(0.1f);

            // A pancada: machuca na ponta e acorda o que estiver por perto.
            Vector2 tip = new Vector2(transform.position.x + _facing * slamReach, floorY + 0.6f);
            Hit(tip, new Vector2(1.8f, 1.4f), slamDamage);
            Emit(tip, 8f, 0.6f);
            yield return new WaitForSeconds(0.2f);

            // A ponta presa no chao: ele puxa o cabo e treme.
            _exposedUntil = Time.time + (_phaseTwo ? 1.1f : 1.5f);
            Play(Stuck, StuckFrames, 8f);
            while (IsExposed && !_dead) yield return null;

            Play(Idle, IdleFrames, 5f);
            yield return new WaitForSeconds(_phaseTwo ? 0.2f : 0.35f);
        }

        /// <summary>A investida: abaixa a cabeca e corre ate a parede; bateu, fica tonto e o teto cai.</summary>
        private IEnumerator Ram()
        {
            float windup = _phaseTwo ? 0.45f : 0.65f;
            Play(ChargeWindup, ChargeWindupFrames, ChargeWindupFrames / windup, false);
            yield return Warn(windup);

            float wall = _facing > 0 ? maxX : minX;
            float speed = _phaseTwo ? 11f : 9f;
            _charging = true;
            Play(Charge, ChargeFrames, 14f);

            while (!Mathf.Approximately(transform.position.x, wall) && !_dead)
            {
                float x = Mathf.MoveTowards(transform.position.x, wall, speed * Time.deltaTime);
                transform.position = new Vector3(x, floorY, 0f);
                if (Time.frameCount % 8 == 0) Emit(new Vector2(x, floorY + 0.5f), 5f, 0.2f);
                yield return null;
            }

            _charging = false;

            // A pancada na parede: a toca inteira treme e o teto solta estilhacos.
            Play(Dazed, 1, 1f, false);
            Emit(new Vector2(transform.position.x + _facing, floorY + 2f), 20f, 0.6f);
            DropShards(_phaseTwo ? 6 : 4, transform.position.x);

            _exposedUntil = Time.time + (_phaseTwo ? 1.4f : 1.9f);
            yield return new WaitForSeconds(0.2f);
            Play(Dazed + 1, DazedFrames - 1, 6f);
            while (IsExposed && !_dead) yield return null;

            Face(-_facing);
            Play(Idle, IdleFrames, 5f);
            yield return new WaitForSeconds(0.3f);
        }

        /// <summary>O apito: chama o turno, e as traças brotam das rachaduras.</summary>
        private IEnumerator CallShift()
        {
            _nextWhistle = Time.time + (_phaseTwo ? 9f : 13f);

            Play(Whistle, WhistleFrames, WhistleFrames / 0.7f, false);
            yield return new WaitForSeconds(0.3f);

            Emit(transform.position + Vector3.up * 2.5f, 14f, 0.5f);

            int calls = _phaseTwo ? 3 : 2;
            int spot = Random.Range(0, Mathf.Max(1, broodSpots != null ? broodSpots.Length : 1));
            for (int i = 0; i < calls && AliveBrood() < broodMaxAlive + (_phaseTwo ? 1 : 0); i++)
            {
                GameObject free = FreeMite();
                if (free == null || broodSpots == null || broodSpots.Length == 0) break;

                StartCoroutine(Hatch(free, (spot + i) % broodSpots.Length));
            }

            yield return new WaitForSeconds(0.6f);
            Play(Idle, IdleFrames, 5f);
            yield return new WaitForSeconds(0.2f);
        }

        /// <summary>Na metade da vida: o grito, e o teto despenca.</summary>
        private IEnumerator Scream()
        {
            Play(Roar, RoarFrames, 8f);
            Emit(transform.position + Vector3.up * 2.5f, 24f, 1f);
            DropShards(7, _player != null ? _player.position.x : transform.position.x);

            yield return new WaitForSeconds(1.4f);
            _nextWhistle = Time.time;
            Play(Idle, IdleFrames, 5f);
        }

        /// <summary>O aviso antes do golpe: pisca em amarelo.</summary>
        private IEnumerator Warn(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                if (_sprite != null && Time.time >= _hurtUntil)
                {
                    _sprite.color = Mathf.FloorToInt(t * 14f) % 2 == 0 ? new Color(1f, 0.92f, 0.5f) : Color.white;
                }

                yield return null;
            }

            if (_sprite != null) _sprite.color = Color.white;
        }

        /// <summary>Estilhacos soltos do teto: metade perto de um ponto, o resto pela toca.</summary>
        private void DropShards(int count, float near)
        {
            for (int i = 0; i < count; i++)
            {
                float x = i % 2 == 0
                    ? Mathf.Clamp(near + Random.Range(-2.5f, 2.5f), minX - 1f, maxX + 1f)
                    : Random.Range(minX - 1f, maxX + 1f);

                FallingShard.Drop(new Vector2(x, ceilingY), floorY, fallingShardSprite, 0.5f + i * 0.12f);
            }
        }

        private void Hit(Vector2 center, Vector2 size, int damage)
        {
            foreach (Collider2D hit in Physics2D.OverlapBoxAll(center, size, 0f))
            {
                var health = hit.GetComponentInParent<ScrapHealth>();
                if (health != null) health.TakeDamage(damage, transform.position);
            }
        }

        /// <summary>Anda, sem sair da toca. Devolve falso quando ja esta na beirada.</summary>
        private bool Move(float dx)
        {
            Vector3 position = transform.position;
            float x = Mathf.Clamp(position.x + dx, minX, maxX);
            bool moved = !Mathf.Approximately(x, position.x);
            transform.position = new Vector3(x, floorY, 0f);
            return moved;
        }

        private void Face(int direction)
        {
            _facing = direction >= 0 ? 1 : -1;

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = _facing < 0;
        }

        // ------------------------------------------------------------------
        // O turno
        // ------------------------------------------------------------------

        private readonly HashSet<GameObject> _hatching = new HashSet<GameObject>();

        private int AliveBrood()
        {
            if (brood == null) return 0;

            int alive = 0;
            foreach (GameObject mite in brood)
            {
                if (mite != null && (mite.activeSelf || _hatching.Contains(mite))) alive++;
            }

            return alive;
        }

        private GameObject FreeMite()
        {
            if (brood == null) return null;

            foreach (GameObject mite in brood)
            {
                if (mite != null && !mite.activeSelf && !_hatching.Contains(mite)) return mite;
            }

            return null;
        }

        private IEnumerator Hatch(GameObject mite, int spot)
        {
            _hatching.Add(mite);

            SpriteRenderer crack = broodCracks != null && spot < broodCracks.Length ? broodCracks[spot] : null;
            bool cracking = crackFrames != null && crackFrames.Length >= 3;

            for (float t = 0f; t < 0.7f; t += Time.deltaTime)
            {
                if (crack != null && cracking)
                {
                    crack.sprite = crackFrames[1];
                    crack.transform.localPosition = new Vector3(Mathf.Sin(t * 70f) * 0.04f, 0f, 0f);
                }

                yield return null;
            }

            _hatching.Remove(mite);
            if (crack != null)
            {
                crack.transform.localPosition = Vector3.zero;
                if (cracking) crack.sprite = crackFrames[2];
            }

            if (_dead || !_awake) yield break;

            // Religa, devolve a vida (quem ja morreu uma vez voltava imortal) e
            // so entao poe no lugar: o Restore manda ele de volta pra casa.
            mite.SetActive(true);
            var body = mite.GetComponent<Damageable>();
            if (body != null) body.Restore();
            mite.transform.position = broodSpots[spot].position;

            // O chao estourando acorda o bicho que saiu dele.
            Emit(broodSpots[spot].position, 4f, 0.5f);

            yield return new WaitForSeconds(1.2f);
            if (crack != null && cracking) crack.sprite = crackFrames[0];
        }

        private void PutBroodBack()
        {
            _hatching.Clear();
            if (brood != null)
            {
                foreach (GameObject mite in brood)
                {
                    if (mite != null) mite.SetActive(false);
                }
            }

            if (broodCracks != null && crackFrames != null && crackFrames.Length >= 3)
            {
                foreach (SpriteRenderer crack in broodCracks)
                {
                    if (crack == null) continue;
                    crack.sprite = crackFrames[0];
                    crack.transform.localPosition = Vector3.zero;
                }
            }
        }

        // ------------------------------------------------------------------
        // Apanhar e cair
        // ------------------------------------------------------------------

        public void Damage(int amount, Vector2 from)
        {
            if (_dead) return;

            // Dormindo, a pancada so o acorda.
            if (!_awake)
            {
                WakeUp();
                return;
            }

            _health -= IsExposed ? amount * 2 : amount;
            _hurtUntil = Time.time + 0.14f;
            if (_sprite != null) _sprite.color = new Color(1f, 0.35f, 0.3f);

            if (_health <= 0)
            {
                Die();
                return;
            }

            if (!_phaseTwo && _health <= maxHealth / 2)
            {
                _phaseTwo = true;
                _roarPending = true;
            }
        }

        private void Die()
        {
            _dead = true;
            _defeated = true;
            _charging = false;
            _exposedUntil = 0f;
            StopAllCoroutines();
            PutBroodBack();
            if (Fighting == this) Fighting = null;
            if (_collider != null) _collider.enabled = false;

            StartCoroutine(Fall());
        }

        /// <summary>
        /// Cambaleia, larga a picareta, cai de joelhos; o capacete cai (a
        /// safira e a chave rolam), os cristais se desfazem e ele deita.
        /// </summary>
        private IEnumerator Fall()
        {
            if (_sprite != null) _sprite.color = Color.white;

            // Um homem desse tamanho caindo: a caverna inteira escuta.
            NoiseSystem.Emit(transform.position + Vector3.up * 2f, 24f, 1f);

            float[] hold = { 0.2f, 0.3f, 0.4f, 0.45f, 0.6f, 0.45f, 0.6f, 0.5f };
            for (int i = 0; i < DeathFrames; i++)
            {
                Play(Death + i, 1, 1f, false);

                if (i == 3)
                {
                    // O capacete no chao: a safira se solta, e a chave cai da argola.
                    Vector3 place = transform.position + new Vector3(_facing * 1.1f, 0.6f, 0f);
                    for (int s = 0; s < 12; s++) ShardPickup.Spawn(place + Vector3.up * 0.2f, shardSprite, 1, s);
                    DropSapphire(place);
                    DropKey(place + new Vector3(-_facing * 1.6f, 0f, 0f));
                }

                yield return new WaitForSeconds(hold[i]);
            }

            yield return new WaitForSeconds(1.5f);

            // Deitado, em paz — e some.
            if (_sprite != null)
            {
                for (float t = 0f; t < 1f; t += Time.deltaTime)
                {
                    _sprite.color = new Color(1f, 1f, 1f, 1f - t);
                    yield return null;
                }
            }

            Destroy(gameObject);
        }

        private void DropSapphire(Vector3 place)
        {
            RewardPickup.Drop(place, rewardSprite, RewardPickup.Kind.Sapphire, Color.white);
        }

        /// <summary>A CHAVE ANTIGA: um item-chave flutuando onde ele caiu.</summary>
        private void DropKey(Vector3 place)
        {
            if (string.IsNullOrEmpty(keyId) || KeyItems.WasFound(keyId)) return;

            var key = new GameObject("AncientKey");
            key.transform.position = place + Vector3.up * 0.8f;

            var renderer = key.AddComponent<SpriteRenderer>();
            renderer.sprite = keySprite;
            if (_sprite != null)
            {
                renderer.sortingLayerID = _sprite.sortingLayerID;
                renderer.sortingOrder = _sprite.sortingOrder + 1;
            }

            var trigger = key.AddComponent<CircleCollider2D>();
            trigger.isTrigger = true;
            trigger.radius = 0.7f;

            key.AddComponent<KeyItemPickup>().Configure(keyId, keyMessage);
        }

        /// <summary>O Scrap morreu: a luta recomeca do zero, e o turno volta pra pedra.</summary>
        private void ResetFight()
        {
            if (_dead) return;

            StopAllCoroutines();

            _health = maxHealth;
            _awake = false;
            _phaseTwo = false;
            _roarPending = false;
            _charging = false;
            _flinchable = false;
            _heard = false;
            _exposedUntil = 0f;
            _hurtUntil = 0f;
            transform.position = _home;

            if (_sprite != null) _sprite.color = Color.white;
            if (_collider != null) _collider.enabled = true;
            Face(-1);
            PlayDormant();

            if (Fighting == this) Fighting = null;

            // Os bichos sao religados pelo Damageable nesse mesmo aviso: desliga depois.
            StartCoroutine(PutBackNextFrame());
        }

        private IEnumerator PutBackNextFrame()
        {
            yield return null;
            PutBroodBack();
        }

        // ------------------------------------------------------------------
        // Encostar
        // ------------------------------------------------------------------

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
            // Preso ou tonto ele nao machuca: e a hora de chegar perto.
            if (_dead || !_awake || IsExposed) return;

            var health = other.GetComponentInParent<ScrapHealth>();
            if (health != null) health.TakeDamage(_charging ? chargeDamage : contactDamage, transform.position);
        }

        // ------------------------------------------------------------------
        // Animacao
        // ------------------------------------------------------------------

        private void PlayDormant() => Play(Dormant, DormantFrames, 2.5f);

        /// <summary>Toca uma sequencia da folha. Em loop, pedir a mesma de novo nao reinicia.</summary>
        private void Play(int start, int count, float framesPerSecond, bool loop = true)
        {
            if (loop && _animLoop && _animStart == start && _animCount == count)
            {
                _animFps = framesPerSecond;
                return;
            }

            _animStart = start;
            _animCount = Mathf.Max(1, count);
            _animFps = Mathf.Max(0.01f, framesPerSecond);
            _animLoop = loop;
            _animSince = Time.time;
        }

        private void LateUpdate()
        {
            if (_sprite == null || frames == null || frames.Length == 0) return;

            int frame;
            if (_flinchable && !_dead && Time.time < _hurtUntil)
            {
                frame = Hurt;
            }
            else
            {
                int step = (int)((Time.time - _animSince) * _animFps);
                step = _animLoop ? step % _animCount : Mathf.Min(step, _animCount - 1);
                frame = _animStart + step;
            }

            if (frame >= 0 && frame < frames.Length && frames[frame] != null) _sprite.sprite = frames[frame];
        }
    }
}
