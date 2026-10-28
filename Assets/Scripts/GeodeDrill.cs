using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// The Geode Drill, o mini boss do District 3: a perfuratriz que furou o
    /// Heart of Quartz por ordem do Rei de Prata. Hoje esta fundida ao Oxide e
    /// perfura sozinha, sem parar.
    ///
    /// Ela e cega: caca pelo som.
    ///
    ///   Enterrada   some dentro da rocha e escuta. A vibracao acende o cristal
    ///               por onde ela passa. E blindada: nada a fere aqui.
    ///   Investida   sai da parede e atravessa a arena em linha reta, ate onde
    ///               ouviu o ultimo som.
    ///   Presa       se o ultimo som foi um pilar ressonante, ela se crava nele
    ///               e fica presa, com o motor exposto. So assim o cinzel fere.
    ///
    /// A cada terco de vida perdido ela perde o controle e fura o chao: a arena
    /// desaba uma camada e a luta desce junto, com menos pilares a cada vez e
    /// estilhacos caindo do teto. Sao tres camadas, cada uma mais perto do
    /// brilho verde la embaixo.
    ///
    /// Ao parar, purifica o distrito, deixa o Resonance Strike e mostra a
    /// placa gravada no chassi.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GeodeDrill : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 18;

        [Header("Arena")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField] private Rect wakeArea;

        [Tooltip("A altura do chao de cada camada, de cima pra baixo.")]
        [SerializeField] private float[] floors;

        [Tooltip("O chao que desaba em cada camada (a ultima nao desaba).")]
        [SerializeField] private GameObject[] slabs;

        [Tooltip("Os pilares ressonantes, de todas as camadas.")]
        [SerializeField] private ResonantCrystal[] pillars;

        [Header("O que ela usa e deixa")]
        [SerializeField] private Sprite debrisSprite;
        [SerializeField] private Sprite shardSprite;
        [SerializeField] private Sprite rewardSprite;
        [SerializeField] private AreaPurifier purifier;

        [Tooltip("A placa do chassi: um objeto desligado, que aparece quando ela para.")]
        [SerializeField] private GameObject plaque;

        private SpriteRenderer _sprite;
        private Collider2D _collider;
        private Transform _player;
        private int _health;
        private int _layer;
        private bool _awake;
        private bool _dead;
        private bool _charging;
        private bool _listening;
        private bool _ownNoise;
        private float _exposedUntil;
        private float _heardX;
        private ResonantCrystal _lure;

        private static bool _defeated;

        /// <summary>O boss em luta, se houver. O HUD usa pra mostrar a barra de vida.</summary>
        public static GeodeDrill Fighting { get; private set; }

        /// <summary>Se ela ja parou nesta sessao.</summary>
        public static bool Defeated => _defeated;

        public float HealthFraction => Mathf.Clamp01(_health / (float)Mathf.Max(1, maxHealth));

        /// <summary>Verdadeiro enquanto ela esta presa num pilar, com o motor a mostra.</summary>
        public bool IsExposed => Time.time < _exposedUntil;

        private float FloorY => floors[Mathf.Clamp(_layer, 0, floors.Length - 1)];
        private bool Frenzied => _layer > 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Fighting = null;
            _defeated = false;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newMinX, float newMaxX, Rect newWakeArea, float[] newFloors,
            GameObject[] newSlabs, ResonantCrystal[] newPillars, Sprite debris, Sprite shard, Sprite reward,
            AreaPurifier newPurifier, GameObject newPlaque)
        {
            minX = newMinX;
            maxX = newMaxX;
            wakeArea = newWakeArea;
            floors = newFloors;
            slabs = newSlabs;
            pillars = newPillars;
            debrisSprite = debris;
            shardSprite = shard;
            rewardSprite = reward;
            purifier = newPurifier;
            plaque = newPlaque;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
            _health = maxHealth;

            ScrapHealth.Died += ResetFight;
        }

        private void OnEnable()
        {
            NoiseSystem.Heard += OnNoise;

            if (pillars == null) return;

            foreach (ResonantCrystal pillar in pillars)
            {
                if (pillar != null) pillar.Rang += OnPillarRang;
            }
        }

        private void OnDisable()
        {
            NoiseSystem.Heard -= OnNoise;

            if (pillars == null) return;

            foreach (ResonantCrystal pillar in pillars)
            {
                if (pillar != null) pillar.Rang -= OnPillarRang;
            }
        }

        private void Start()
        {
            if (plaque != null) plaque.SetActive(false);

            if (_defeated)
            {
                // Ja parou numa visita anterior: a arena fica como ela deixou.
                _dead = true;
                _layer = floors.Length - 1;
                SetSlabs(false);
                BecomeWreck(new Vector3((minX + maxX) * 0.5f, FloorY, 0f));

                if (!ScrapChisel.HasResonanceStrike) DropReward();
                return;
            }

            Hide();

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
            if (_sprite != null && !_dead && _sprite.enabled)
            {
                // Presa no pilar ela fica em brasa: e a deixa pra bater.
                Color target = IsExposed ? new Color(1f, 0.62f, 0.35f) : Color.white;
                _sprite.color = Color.Lerp(_sprite.color, target, 12f * Time.deltaTime);
            }

            if (_awake || _dead || _player == null) return;
            if (!wakeArea.Contains(_player.position)) return;

            _awake = true;
            Fighting = this;
            _heardX = _player.position.x;
            StartCoroutine(Brain());
        }

        /// <summary>O Scrap morreu: a luta recomeca do zero, com o chao de volta.</summary>
        private void ResetFight()
        {
            if (_dead) return;

            StopAllCoroutines();

            _health = maxHealth;
            _layer = 0;
            _awake = false;
            _charging = false;
            _listening = false;
            _exposedUntil = 0f;
            _lure = null;

            SetSlabs(true);
            Hide();

            if (Fighting == this) Fighting = null;
        }

        private void SetSlabs(bool present)
        {
            if (slabs == null) return;

            foreach (GameObject slab in slabs)
            {
                if (slab != null) slab.SetActive(present);
            }
        }

        private void Hide()
        {
            if (_sprite != null) _sprite.enabled = false;
            if (_collider != null) _collider.enabled = false;
        }

        private void Show(float x, int facing)
        {
            transform.position = new Vector3(x, FloorY, 0f);

            if (_sprite != null)
            {
                _sprite.enabled = true;
                _sprite.color = Color.white;

                // A arte olha pra direita.
                _sprite.flipX = facing < 0;
            }

            if (_collider != null) _collider.enabled = true;
        }

        /// <summary>O ruido que ela mesma faz nao pode engana-la.</summary>
        private void Rumble(Vector2 origin, float radius, float loudness)
        {
            _ownNoise = true;
            NoiseSystem.Emit(origin, radius, loudness);
            _ownNoise = false;
        }

        // ------------------------------------------------------------------
        // Ouvir
        // ------------------------------------------------------------------

        private void OnNoise(Vector2 origin, float radius, float loudness)
        {
            if (_ownNoise || !_listening) return;

            // So o que acontece na camada dela, dentro da arena.
            if (Mathf.Abs(origin.y - FloorY) > 9f) return;
            if (origin.x < minX - 3f || origin.x > maxX + 3f) return;

            _heardX = origin.x;
        }

        private void OnPillarRang(ResonantCrystal pillar)
        {
            if (!_listening) return;
            if (Mathf.Abs(pillar.transform.position.y - FloorY) > 1f) return;

            // Uma nota dessas apaga qualquer passo: e pra ela que a broca vai.
            _lure = pillar;
        }

        // ------------------------------------------------------------------
        // Cerebro
        // ------------------------------------------------------------------

        private IEnumerator Brain()
        {
            while (!_dead)
            {
                // Enterrada: escuta, e o cristal acende por onde ela passa.
                Hide();
                _lure = null;
                _listening = true;

                float buried = Frenzied ? 1.5f : 2.3f;
                float rumbleX = transform.position.x;

                for (float t = 0f; t < buried; t += Time.deltaTime)
                {
                    rumbleX = Mathf.MoveTowards(rumbleX, _heardX, 9f * Time.deltaTime);
                    if (Time.frameCount % 12 == 0) Rumble(new Vector2(rumbleX, FloorY + 0.5f), 4f, 0.1f);

                    yield return null;
                }

                _listening = false;

                ResonantCrystal lure = _lure;
                float target = Mathf.Clamp(lure != null ? lure.transform.position.x : _heardX, minX + 3f, maxX - 3f);

                // Sai da parede mais distante, pra atravessar a arena inteira.
                bool fromLeft = target - minX > maxX - target;
                float startX = fromLeft ? minX : maxX;
                int direction = fromLeft ? 1 : -1;

                Show(startX, direction);

                // O aviso: ela treme na boca do tunel, e a sala acende.
                Rumble(new Vector2(startX, FloorY + 1f), 12f, 0.2f);

                for (float t = 0f; t < 0.7f; t += Time.deltaTime)
                {
                    transform.position = new Vector3(startX + Mathf.Sin(t * 70f) * 0.08f, FloorY, 0f);
                    if (_sprite != null && Mathf.FloorToInt(t * 12f) % 2 == 0) _sprite.color = new Color(1f, 0.92f, 0.45f);
                    yield return null;
                }

                if (Frenzied) Debris();

                // Com isca ela para no pilar; sem, passa do alvo e some na outra parede.
                float stopX = lure != null
                    ? lure.transform.position.x - direction * 2.2f
                    : (fromLeft ? maxX : minX);

                float speed = Frenzied ? 17f : 13f;
                float x = startX;

                _charging = true;

                while (!Mathf.Approximately(x, stopX))
                {
                    x = Mathf.MoveTowards(x, stopX, speed * Time.deltaTime);
                    transform.position = new Vector3(x, FloorY, 0f);

                    if (Time.frameCount % 6 == 0) Rumble(new Vector2(x, FloorY + 1f), 7f, 0.2f);
                    yield return null;
                }

                _charging = false;

                if (lure != null)
                {
                    // Cravou no pilar: presa, com o motor a mostra.
                    Rumble(new Vector2(x, FloorY + 1f), 18f, 0.2f);

                    _exposedUntil = Time.time + (Frenzied ? 3.2f : 4.2f);
                    while (IsExposed) yield return null;

                    yield return new WaitForSeconds(0.3f);
                }
            }
        }

        /// <summary>Estilhacos soltos do teto, caindo em volta de onde ela ouviu o Scrap.</summary>
        private void Debris()
        {
            for (int i = -1; i <= 1; i++)
            {
                float x = Mathf.Clamp(_heardX + i * 2.6f + Random.Range(-0.6f, 0.6f), minX, maxX);
                ShardShot.Fire(new Vector2(x, FloorY + 9f), Vector2.down * 8f, 9f, debrisSprite);
            }
        }

        /// <summary>Perdeu o controle: fura o chao, e a arena desce uma camada.</summary>
        private IEnumerator Collapse()
        {
            _charging = false;
            _listening = false;
            _exposedUntil = 0f;

            float x = transform.position.x;
            Show(x, 1);

            // Ela gira no lugar, apontada pra baixo. E o tempo de entender o que vem.
            for (float t = 0f; t < 1.3f; t += Time.deltaTime)
            {
                transform.position = new Vector3(x + Mathf.Sin(t * 80f) * 0.12f, FloorY, 0f);
                if (_sprite != null) _sprite.color = new Color(0.55f, 1f, 0.65f);
                if (Time.frameCount % 8 == 0) Rumble(new Vector2(x, FloorY + 1f), 30f, 0.2f);
                yield return null;
            }

            if (slabs != null && _layer < slabs.Length && slabs[_layer] != null) slabs[_layer].SetActive(false);

            _layer++;
            Hide();
            transform.position = new Vector3(x, FloorY, 0f);
            _heardX = x;

            // O tempo de cair e se achar la embaixo.
            yield return new WaitForSeconds(1.6f);

            if (_player != null) _heardX = _player.position.x;
            StartCoroutine(Brain());
        }

        // ------------------------------------------------------------------
        // Apanhar e parar
        // ------------------------------------------------------------------

        public void Damage(int amount, Vector2 from)
        {
            if (_dead || !_awake) return;

            // Blindada: fora do pilar o golpe so faz barulho.
            if (!IsExposed)
            {
                if (_sprite != null) _sprite.color = new Color(0.7f, 0.8f, 1f);
                return;
            }

            // Cada terco de vida e uma camada: um golpe forte nao pula nenhuma.
            int layers = floors.Length;
            int threshold = maxHealth * (layers - 1 - _layer) / layers;

            _health = Mathf.Max(_health - amount, threshold);
            if (_sprite != null) _sprite.color = new Color(1f, 0.35f, 0.3f);

            if (_health > threshold) return;

            StopAllCoroutines();

            if (_health <= 0) Die();
            else StartCoroutine(Collapse());
        }

        private void Die()
        {
            _dead = true;
            _defeated = true;
            _exposedUntil = 0f;

            if (Fighting == this) Fighting = null;

            Vector3 place = new Vector3(transform.position.x, FloorY, 0f);

            for (int i = 0; i < 20; i++)
            {
                ShardPickup.Spawn(place + Vector3.up * 0.2f, shardSprite, 1, i);
            }

            BecomeWreck(place);
            DropReward();

            // O ultimo giro acende a arena inteira, e o distrito acorda.
            NoiseSystem.Emit(place + Vector3.up, 40f, 0.2f);
            if (purifier != null) purifier.Purify(place);
        }

        /// <summary>Parada: vira carcaca, nao machuca mais, e a placa do chassi aparece.</summary>
        private void BecomeWreck(Vector3 place)
        {
            transform.position = place;

            if (_sprite != null)
            {
                _sprite.enabled = true;
                _sprite.color = new Color(0.55f, 0.55f, 0.62f);
            }

            if (_collider != null) _collider.enabled = false;
            if (plaque != null) plaque.SetActive(true);
        }

        private void DropReward()
        {
            // Atras dela, pra nao ficar em cima da placa.
            RewardPickup.Drop(transform.position + Vector3.left * 3.5f, rewardSprite,
                RewardPickup.Kind.ResonanceStrike, new Color(0.80f, 0.90f, 1f));
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
            // Presa ela nao machuca: e a hora de chegar perto.
            if (_dead || !_awake || IsExposed) return;

            var health = other.GetComponentInParent<ScrapHealth>();
            if (health != null) health.TakeDamage(_charging ? 2 : 1, transform.position);
        }
    }
}
