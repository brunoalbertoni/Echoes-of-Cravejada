using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// The Boiler Master, o mini boss do District 2: o chefe de manutencao,
    /// fundido a uma caldeira que hoje queima Oxide.
    ///
    /// Ele e blindado. O cinzel so fere quando a fornalha das costas esta
    /// exposta — logo depois de cada martelada, ou enquanto ele esta atordoado.
    ///
    ///   Martelo:  golpe pesado no chao, com aviso longo. Depois dele a
    ///             fornalha fica exposta por um instante.
    ///   Rajada:   tres jatos de vapor em alturas diferentes.
    ///   Fase 2 (metade da vida): mais rapido, e geiseres de vapor brotam da
    ///             grade em pontos sorteados.
    ///
    /// As tres valvulas da arena sao a saida inteligente: raspar e girar uma,
    /// no meio da luta, devolve a pressao pra dentro dele e o atordoa.
    ///
    /// Ao cair, deixa o Pressure Jump e abre a saida.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BoilerMaster : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 20;

        [Header("Arena")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField] private float floorY;
        [SerializeField] private Rect wakeArea;

        [Header("O que ele usa")]
        [SerializeField] private Sprite boltSprite;
        [SerializeField] private Sprite waveSprite;
        [SerializeField] private Sprite geyserSprite;
        [SerializeField] private Sprite shardSprite;
        [SerializeField] private Sprite rewardSprite;

        [Header("A arena")]
        [SerializeField] private Valve[] valves;
        [SerializeField] private Gate exitGate;

        private SpriteRenderer _sprite;
        private Transform _player;
        private Vector3 _home;
        private int _health;
        private bool _awake;
        private bool _phaseTwo;
        private bool _dead;
        private int _facing = -1;
        private float _exposedUntil;
        private float _stunnedUntil;

        private static bool _defeated;

        /// <summary>O boss em luta, se houver. O HUD usa pra mostrar a barra de vida.</summary>
        public static BoilerMaster Fighting { get; private set; }

        /// <summary>Se ele ja caiu nesta sessao. E o que solta o acido ascendente.</summary>
        public static bool Defeated => _defeated;

        public float HealthFraction => Mathf.Clamp01(_health / (float)Mathf.Max(1, maxHealth));

        /// <summary>Verdadeiro enquanto a fornalha esta aberta e o cinzel fere.</summary>
        public bool IsExposed => Time.time < _exposedUntil || Time.time < _stunnedUntil;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Fighting = null;
            _defeated = false;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newMinX, float newMaxX, float newFloorY, Rect newWakeArea,
            Sprite bolt, Sprite wave, Sprite geyser, Sprite shard, Sprite reward,
            Valve[] newValves, Gate gate)
        {
            minX = newMinX;
            maxX = newMaxX;
            floorY = newFloorY;
            wakeArea = newWakeArea;
            boltSprite = bolt;
            waveSprite = wave;
            geyserSprite = geyser;
            shardSprite = shard;
            rewardSprite = reward;
            valves = newValves;
            exitGate = gate;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _health = maxHealth;
            _home = transform.position;

            ScrapHealth.Died += ResetFight;

            if (valves != null)
            {
                foreach (Valve valve in valves)
                {
                    if (valve != null) valve.Turned += OnValveTurned;
                }
            }
        }

        private void Start()
        {
            // Ja foi derrotado nesta sessao: a arena fica vazia e aberta.
            if (_defeated)
            {
                if (exitGate != null) exitGate.Open();
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
            if (_sprite != null && !_dead)
            {
                // Fornalha exposta: ele fica em brasa. E a deixa pra bater.
                Color target = IsExposed ? new Color(1f, 0.62f, 0.35f) : Color.white;
                _sprite.color = Color.Lerp(_sprite.color, target, 12f * Time.deltaTime);
            }

            if (_awake || _dead || _player == null) return;
            if (!wakeArea.Contains(_player.position)) return;

            _awake = true;
            Fighting = this;
            StartCoroutine(Brain());
        }

        /// <summary>O Scrap morreu: a luta recomeca do zero, e as valvulas enferrujam de novo.</summary>
        private void ResetFight()
        {
            if (_dead) return;

            StopAllCoroutines();

            _health = maxHealth;
            _phaseTwo = false;
            _awake = false;
            _exposedUntil = 0f;
            _stunnedUntil = 0f;
            transform.position = _home;

            if (_sprite != null) _sprite.color = Color.white;
            if (Fighting == this) Fighting = null;

            RustValves();
        }

        private void RustValves()
        {
            if (valves == null) return;

            foreach (Valve valve in valves)
            {
                if (valve != null) valve.Rust();
            }
        }

        private void OnValveTurned(Valve valve)
        {
            if (_dead || !_awake) return;

            // A pressao volta pra dentro dele: quatro segundos parado e aberto.
            _stunnedUntil = Time.time + 4f;
        }

        // ------------------------------------------------------------------
        // Cerebro
        // ------------------------------------------------------------------

        private IEnumerator Brain()
        {
            yield return new WaitForSeconds(1f);

            while (!_dead)
            {
                while (Time.time < _stunnedUntil) yield return null;

                float walkTime = Random.Range(0.8f, 1.4f);
                float walkSpeed = _phaseTwo ? 2.6f : 1.7f;

                for (float t = 0f; t < walkTime && Time.time >= _stunnedUntil; t += Time.deltaTime)
                {
                    FacePlayer();
                    Move(_facing * walkSpeed * Time.deltaTime);
                    yield return null;
                }

                if (Time.time < _stunnedUntil) continue;

                float distance = Mathf.Abs(_player.position.x - transform.position.x);
                float roll = Random.value;

                if (_phaseTwo && roll < 0.3f) yield return Geysers();
                else if (distance < 5f || roll < 0.45f) yield return Hammer();
                else yield return SteamBurst();
            }
        }

        private IEnumerator Hammer()
        {
            FacePlayer();
            yield return Telegraph(_phaseTwo ? 0.65f : 0.9f);

            // A martelada pega quem estiver na frente, perto e baixo.
            Vector2 center = (Vector2)transform.position + new Vector2(_facing * 2.4f, 1f);

            foreach (Collider2D hit in Physics2D.OverlapBoxAll(center, new Vector2(3.2f, 2.4f), 0f))
            {
                var health = hit.GetComponentInParent<ScrapHealth>();
                if (health != null) health.TakeDamage(1, transform.position);
            }

            // E solta uma onda pra frente, que so se evita pulando.
            ShockWave.Launch(new Vector3(transform.position.x + _facing * 2.5f, floorY, 0f),
                _facing * 7.5f, minX - 2f, maxX + 2f, waveSprite);

            // O contragolpe: a fornalha fica aberta enquanto ele recolhe o martelo.
            _exposedUntil = Time.time + (_phaseTwo ? 1.8f : 2.4f);
            yield return new WaitForSeconds(_phaseTwo ? 1.5f : 2.1f);
        }

        private IEnumerator SteamBurst()
        {
            FacePlayer();
            yield return Telegraph(0.6f);

            // Tres alturas, uma de cada vez: rente ao chao, no meio e alto.
            float[] heights = { 0.5f, 1.9f, 3.3f };
            int skip = Random.Range(0, heights.Length);

            for (int i = 0; i < heights.Length; i++)
            {
                // Sempre sobra uma altura livre, pra haver por onde passar.
                if (i == skip) continue;

                Vector3 muzzle = transform.position + new Vector3(_facing * 1.8f, heights[i], 0f);
                SteamBolt.Fire(muzzle, _facing * 9f, 40f, boltSprite);

                yield return new WaitForSeconds(0.25f);
            }

            yield return new WaitForSeconds(0.9f);
        }

        private IEnumerator Geysers()
        {
            yield return Telegraph(0.5f);

            // Um embaixo do Scrap e dois sorteados.
            SteamGeyser.Erupt(new Vector3(Mathf.Clamp(_player.position.x, minX, maxX), floorY, 0f), geyserSprite);

            for (int i = 0; i < 2; i++)
            {
                SteamGeyser.Erupt(new Vector3(Random.Range(minX, maxX), floorY, 0f), geyserSprite);
            }

            yield return new WaitForSeconds(1.6f);
        }

        /// <summary>O aviso antes de cada golpe: ele pisca em amarelo.</summary>
        private IEnumerator Telegraph(float time)
        {
            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                if (_sprite != null && Mathf.FloorToInt(t * 12f) % 2 == 0) _sprite.color = new Color(1f, 0.92f, 0.45f);
                yield return null;
            }
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

            // Blindado: fora da janela o golpe so faz barulho.
            if (!IsExposed)
            {
                if (_sprite != null) _sprite.color = new Color(0.7f, 0.8f, 1f);
                return;
            }

            _health -= amount;
            if (_sprite != null) _sprite.color = new Color(1f, 0.35f, 0.3f);

            if (_health <= 0)
            {
                Die();
                return;
            }

            if (!_phaseTwo && _health <= maxHealth / 2) _phaseTwo = true;
        }

        private void Die()
        {
            _dead = true;
            _defeated = true;
            StopAllCoroutines();

            Vector3 place = new Vector3(transform.position.x, floorY, 0f);

            for (int i = 0; i < 16; i++)
            {
                ShardPickup.Spawn(place + Vector3.up * 0.2f, shardSprite, 1, i);
            }

            PressureJumpPickup.Drop(place, rewardSprite);

            if (exitGate != null) exitGate.Open();

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
            // Atordoado ele nao machuca: e a hora de chegar perto.
            if (_dead || !_awake || Time.time < _stunnedUntil) return;

            var health = other.GetComponentInParent<ScrapHealth>();
            if (health != null) health.TakeDamage(1, transform.position);
        }
    }
}
