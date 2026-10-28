using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// The Boiler Master, o mini boss do District 2: o chefe de manutencao,
    /// fundido a uma caldeira que hoje queima Oxide.
    ///
    /// E o segundo boss do jogo e tem que cobrar o que o Scrap aprendeu e
    /// comprou desde o primeiro. Apanha a qualquer hora, mas e resistente e
    /// nao para: cada golpe tem aviso (ele pisca em amarelo), e quase todos
    /// pedem pular ou dar dash por cima.
    ///
    ///   Martelo:   golpe pesado na frente e uma onda de choque pelo chao.
    ///   Investida: atravessa a arena soltando vapor, ate a parede.
    ///   Rajada:    jatos de vapor em tres alturas, sempre com uma livre.
    ///   Geiser:    um geiser brota embaixo do Scrap.
    ///
    /// Na metade da vida a caldeira entra em sobrepressao (fase 2): ele fica
    /// mais rapido, os avisos encurtam, a onda do martelo sai pros dois lados,
    /// as rajadas vem em dobro, os geiseres perseguem o Scrap e aparece o
    /// Salto: ele cai em cima do Scrap e solta duas ondas.
    ///
    /// Depois do martelo e da investida a fornalha das costas fica exposta um
    /// instante: ali o cinzel fere em dobro. E a recompensa de quem fica perto.
    ///
    /// Ao cair, explode em Oxide (ver BossFall), o distrito e purificado, ele
    /// deixa o Salto de Pressao e a grade da saida abre.
    ///
    /// Cada golpe tem a sua animacao (ver os quadros do BoilerMasterArt, no
    /// editor): dorme arriado ate o Scrap entrar, acorda, ergue o martelo,
    /// agacha soltando vapor antes da investida, aponta o bico pra rajada,
    /// pisa forte pros geiseres; exposto, a porta da fornalha fica escancarada.
    /// Na sobrepressao ele para, urra, e troca pros quadros em brasa.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BoilerMaster : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 46;

        [Tooltip("A nuvem de Oxide da queda: um filho dele.")]
        [SerializeField] private ParticleSystem oxideCloud;

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

        [Header("Os quadros")]
        [Tooltip("Normal, na ordem das constantes.")]
        [SerializeField] private Sprite[] frames;

        [Tooltip("Em sobrepressao (da metade da vida pra baixo), na mesma ordem.")]
        [SerializeField] private Sprite[] hotFrames;

        [Header("O que acontece quando ele cai")]
        [SerializeField] private Gate exitGate;
        [SerializeField] private AreaPurifier purifier;

        // A ordem dos quadros. E a mesma do BoilerMasterArt, no editor.
        public const int Dormant = 0;
        public const int DormantFrames = 4;
        public const int Wake = 4;
        public const int WakeFrames = 4;
        public const int Idle = 8;
        public const int IdleFrames = 6;
        public const int Walk = 14;
        public const int WalkFrames = 8;
        public const int HammerWindup = 22;
        public const int HammerWindupFrames = 2;
        public const int HammerStrike = 24;
        public const int HammerStrikeFrames = 3;
        public const int ChargeWindup = 27;
        public const int ChargeWindupFrames = 2;
        public const int ChargeRun = 29;
        public const int ChargeRunFrames = 4;
        public const int BurstWindup = 33;
        public const int BurstWindupFrames = 2;
        public const int BurstFire = 35;
        public const int BurstFireFrames = 3;
        public const int GeyserCast = 38;
        public const int GeyserCastFrames = 3;
        public const int LeapCrouch = 41;
        public const int LeapCrouchFrames = 2;
        public const int LeapRise = 43;
        public const int LeapAir = 44;
        public const int LeapFall = 45;
        public const int LeapImpact = 46;
        public const int LeapImpactFrames = 2;
        public const int ExposedPose = 48;
        public const int ExposedFrames = 2;
        public const int Hurt = 50;
        public const int Roar = 51;
        public const int RoarFrames = 4;
        public const int Death = 55;
        public const int DeathFrames = 8;

        private SpriteRenderer _sprite;
        private Transform _player;
        private Vector3 _home;
        private int _health;
        private bool _awake;
        private bool _phaseTwo;
        private bool _dead;
        private int _facing = -1;
        private float _exposedUntil;
        private int _lastAttack = -1;
        private bool _roarPending;

        // A animacao tocando agora.
        private int _animStart;
        private int _animCount = 1;
        private float _animFps = 1f;
        private bool _animLoop = true;
        private float _animSince;
        private float _hurtUntil = -1f;
        private int _deathFrame = -1;

        private static bool _defeated;

        /// <summary>O boss em luta, se houver. O HUD usa pra mostrar a barra de vida.</summary>
        public static BoilerMaster Fighting { get; private set; }

        /// <summary>Se ele ja caiu nesta sessao.</summary>
        public static bool Defeated => _defeated;

        public float HealthFraction => Mathf.Clamp01(_health / (float)Mathf.Max(1, maxHealth));

        /// <summary>Verdadeiro enquanto a fornalha esta aberta e o cinzel fere em dobro.</summary>
        public bool IsExposed => Time.time < _exposedUntil;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Fighting = null;
            _defeated = false;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newMinX, float newMaxX, float newFloorY, Rect newWakeArea,
            Sprite bolt, Sprite wave, Sprite geyser, Sprite shard, Sprite reward,
            Gate gate, AreaPurifier newPurifier)
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
            exitGate = gate;
            purifier = newPurifier;
        }

        /// <summary>Usado pelos geradores de fase: a nuvem de Oxide em que ele explode ao cair.</summary>
        public void ConfigureCloud(ParticleSystem cloud)
        {
            oxideCloud = cloud;
        }

        /// <summary>Usado pelos geradores de fase: os quadros, normal e em sobrepressao.</summary>
        public void ConfigureFrames(Sprite[] newFrames, Sprite[] newHotFrames)
        {
            frames = newFrames;
            hotFrames = newHotFrames;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _health = maxHealth;
            _home = transform.position;
            Face(-1);
            Play(Dormant, DormantFrames, 2.5f);

            ScrapHealth.Died += ResetFight;
        }

        private void Start()
        {
            // Ja foi derrotado nesta sessao: a arena fica vazia e aberta.
            if (_defeated || AreaPurifier.IsPurified)
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
                // Fornalha exposta: um pouco em brasa (a porta aberta, no desenho, e a deixa).
                Color target = IsExposed ? new Color(1f, 0.82f, 0.66f) : Color.white;
                _sprite.color = Color.Lerp(_sprite.color, target, 12f * Time.deltaTime);
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
            _phaseTwo = false;
            _roarPending = false;
            _awake = false;
            _exposedUntil = 0f;
            _lastAttack = -1;
            _hurtUntil = -1f;
            transform.position = _home;

            if (_sprite != null) _sprite.color = Color.white;
            Face(-1);
            Play(Dormant, DormantFrames, 2.5f);

            if (Fighting == this) Fighting = null;
        }

        // ------------------------------------------------------------------
        // Cerebro
        // ------------------------------------------------------------------

        private IEnumerator Brain()
        {
            // Acorda: o visor acende, ele se ergue, as chamines tossem.
            FacePlayer();
            Play(Wake, WakeFrames, WakeFrames / 0.9f, false);
            yield return new WaitForSeconds(1.0f);

            while (!_dead)
            {
                // Chegou a sobrepressao: para tudo e urra.
                if (_roarPending)
                {
                    _roarPending = false;
                    yield return Overpressure();
                }

                // Ainda exposto do golpe anterior: arfa com a fornalha aberta.
                while (IsExposed && !_dead)
                {
                    Play(ExposedPose, ExposedFrames, 6f);
                    yield return null;
                }

                float walkTime = Random.Range(0.5f, _phaseTwo ? 0.8f : 1.1f);
                float walkSpeed = _phaseTwo ? 3.2f : 2.2f;
                Play(Walk, WalkFrames, walkSpeed * 4f);

                for (float t = 0f; t < walkTime; t += Time.deltaTime)
                {
                    FacePlayer();
                    Move(_facing * walkSpeed * Time.deltaTime);
                    yield return null;
                }

                int attack = Choose();
                _lastAttack = attack;

                switch (attack)
                {
                    case 0: yield return Hammer(); break;
                    case 1: yield return Charge(); break;
                    case 2: yield return SteamBurst(); break;
                    case 3: yield return Geysers(); break;
                    default: yield return Leap(); break;
                }

                // Na fase 2 ele as vezes emenda um segundo golpe sem respirar.
                if (_phaseTwo && !_dead && Random.value < 0.35f)
                {
                    yield return Distance() < 5f ? Hammer() : SteamBurst();
                }
            }
        }

        /// <summary>Escolhe o golpe pela distancia, sem repetir o anterior.</summary>
        private int Choose()
        {
            float distance = Distance();

            for (int tries = 0; tries < 6; tries++)
            {
                int pick;
                float roll = Random.value;

                if (_phaseTwo && roll < 0.22f) pick = 4;
                else if (distance < 4.5f) pick = roll < 0.6f ? 0 : 3;
                else if (distance > 9f) pick = roll < 0.5f ? 1 : 2;
                else pick = Random.Range(0, 4);

                if (pick != _lastAttack) return pick;
            }

            return (_lastAttack + 1) % 4;
        }

        private float Distance() => _player == null ? 0f : Mathf.Abs(_player.position.x - transform.position.x);

        /// <summary>A sobrepressao: a nuvem sai das juntas, ele urra e o ponteiro vai pro fim.</summary>
        private IEnumerator Overpressure()
        {
            FacePlayer();
            Play(Roar, RoarFrames, RoarFrames / 1.1f, false);
            if (oxideCloud != null) oxideCloud.Play();
            ScrapCameraFollow.Shake(0.15f, 0.8f);
            yield return new WaitForSeconds(1.2f);
        }

        private IEnumerator Hammer()
        {
            FacePlayer();
            float windup = _phaseTwo ? 0.45f : 0.65f;
            Play(HammerWindup, HammerWindupFrames, HammerWindupFrames / windup, false);
            yield return Telegraph(windup);
            Play(HammerStrike, HammerStrikeFrames, 14f, false);

            // A martelada pega quem estiver na frente, perto e baixo.
            Vector2 center = (Vector2)transform.position + new Vector2(_facing * 2.4f, 1f);

            foreach (Collider2D hit in Physics2D.OverlapBoxAll(center, new Vector2(3.4f, 2.4f), 0f))
            {
                var health = hit.GetComponentInParent<ScrapHealth>();
                if (health != null) health.TakeDamage(1, transform.position);
            }

            ScrapCameraFollow.Shake(0.08f, 0.2f);

            // A onda pelo chao, que so se evita pulando. Na fase 2, pros dois lados.
            ShockWave.Launch(new Vector3(transform.position.x + _facing * 2.5f, floorY, 0f),
                _facing * 8.5f, minX - 2f, maxX + 2f, waveSprite);

            if (_phaseTwo)
            {
                ShockWave.Launch(new Vector3(transform.position.x - _facing * 2.5f, floorY, 0f),
                    -_facing * 8.5f, minX - 2f, maxX + 2f, waveSprite);
            }

            _exposedUntil = Time.time + (_phaseTwo ? 0.9f : 1.2f);
            yield return new WaitForSeconds(0.25f);
            Play(ExposedPose, ExposedFrames, 6f);
            yield return new WaitForSeconds(_phaseTwo ? 0.55f : 0.85f);
        }

        private IEnumerator Charge()
        {
            FacePlayer();
            float windup = _phaseTwo ? 0.4f : 0.55f;
            Play(ChargeWindup, ChargeWindupFrames, ChargeWindupFrames / windup, false);
            yield return Telegraph(windup);

            // Atravessa ate a parede (ou ate cansar), soltando vapor pra tras.
            Play(ChargeRun, ChargeRunFrames, 12f);
            int direction = _facing;
            float speed = _phaseTwo ? 15f : 12f;

            for (float t = 0f; t < 1.4f; t += Time.deltaTime)
            {
                float before = transform.position.x;
                Move(direction * speed * Time.deltaTime);
                if (Mathf.Approximately(before, transform.position.x)) break;
                yield return null;
            }

            ScrapCameraFollow.Shake(0.1f, 0.25f);

            // Bater na parede o deixa aberto um instante.
            _exposedUntil = Time.time + (_phaseTwo ? 1.0f : 1.4f);
            Play(ExposedPose, ExposedFrames, 6f);
            yield return new WaitForSeconds(_phaseTwo ? 0.7f : 1.0f);
        }

        private IEnumerator SteamBurst()
        {
            FacePlayer();
            float windup = _phaseTwo ? 0.4f : 0.55f;
            Play(BurstWindup, BurstWindupFrames, BurstWindupFrames / windup, false);
            yield return Telegraph(windup);

            int volleys = _phaseTwo ? 2 : 1;

            for (int volley = 0; volley < volleys; volley++)
            {
                FacePlayer();

                // Tres alturas: rente ao chao, no meio e alto. Sempre sobra uma.
                float[] heights = { 0.5f, 1.9f, 3.3f };
                int skip = Random.Range(0, heights.Length);

                for (int i = 0; i < heights.Length; i++)
                {
                    if (i == skip) continue;

                    Vector3 muzzle = transform.position + new Vector3(_facing * 1.8f, heights[i], 0f);
                    SteamBolt.Fire(muzzle, _facing * 10f, 40f, boltSprite);
                    Play(BurstFire, BurstFireFrames, 15f, false);
                    yield return new WaitForSeconds(0.2f);
                }

                yield return new WaitForSeconds(0.45f);
            }

            Play(Idle, IdleFrames, 6f);
            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator Geysers()
        {
            float windup = _phaseTwo ? 0.35f : 0.5f;
            Play(GeyserCast, 2, 2f / windup, false);
            yield return Telegraph(windup);
            Show(GeyserCast + 2);

            // Fase 1: um embaixo do Scrap e um sorteado. Fase 2: quatro, um
            // atras do outro, perseguindo o Scrap.
            int count = _phaseTwo ? 4 : 1;

            for (int i = 0; i < count; i++)
            {
                SteamGeyser.Erupt(new Vector3(Mathf.Clamp(_player.position.x, minX, maxX), floorY, 0f), geyserSprite);
                yield return new WaitForSeconds(0.38f);
            }

            if (!_phaseTwo)
            {
                SteamGeyser.Erupt(new Vector3(Random.Range(minX, maxX), floorY, 0f), geyserSprite);
            }

            Play(Idle, IdleFrames, 6f);
            yield return new WaitForSeconds(_phaseTwo ? 0.6f : 1.0f);
        }

        /// <summary>So na fase 2: sobe num arco, para em cima do Scrap e despenca.</summary>
        private IEnumerator Leap()
        {
            FacePlayer();
            Play(LeapCrouch, LeapCrouchFrames, LeapCrouchFrames / 0.45f, false);
            yield return Telegraph(0.45f);

            Vector3 from = transform.position;
            float targetX = Mathf.Clamp(_player.position.x, minX, maxX);

            const float jumpTime = 0.5f;
            for (float t = 0f; t < jumpTime; t += Time.deltaTime)
            {
                float k = t / jumpTime;
                Show(k < 0.55f ? LeapRise : LeapAir);
                float height = Mathf.Sin(k * Mathf.PI * 0.5f) * 6f;
                transform.position = new Vector3(Mathf.Lerp(from.x, targetX, k), floorY + height, 0f);
                yield return null;
            }

            // Paira um instante: e a hora de sair de baixo.
            Show(LeapAir);
            yield return new WaitForSeconds(0.22f);
            Show(LeapFall);

            const float fallTime = 0.14f;
            float top = transform.position.y;
            for (float t = 0f; t < fallTime; t += Time.deltaTime)
            {
                transform.position = new Vector3(targetX, Mathf.Lerp(top, floorY, t / fallTime), 0f);
                yield return null;
            }

            transform.position = new Vector3(targetX, floorY, 0f);
            ScrapCameraFollow.Shake(0.14f, 0.3f);

            ShockWave.Launch(transform.position + Vector3.right * 1.4f, 8f, minX - 1f, maxX + 1f, waveSprite);
            ShockWave.Launch(transform.position + Vector3.left * 1.4f, -8f, minX - 1f, maxX + 1f, waveSprite);

            Play(LeapImpact, LeapImpactFrames, 6f, false);
            _exposedUntil = Time.time + 0.9f;
            yield return new WaitForSeconds(0.35f);
            Play(ExposedPose, ExposedFrames, 6f);
            yield return new WaitForSeconds(0.55f);
        }

        /// <summary>O aviso antes de cada golpe: ele pisca em amarelo.</summary>
        private IEnumerator Telegraph(float time)
        {
            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                if (_sprite != null)
                {
                    bool lit = Mathf.FloorToInt(t * 12f) % 2 == 0;
                    _sprite.color = lit ? new Color(1f, 0.92f, 0.45f) : Color.white;
                }

                yield return null;
            }
        }

        private void FacePlayer()
        {
            if (_player == null) return;

            Face(_player.position.x >= transform.position.x ? 1 : -1);
        }

        private void Face(int direction)
        {
            _facing = direction;

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = _facing < 0;
        }

        // ------------------------------------------------------------------
        // Animacao
        // ------------------------------------------------------------------

        /// <summary>Toca uma sequencia de quadros. Em loop, pedir a mesma de novo nao reinicia.</summary>
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

        private void Show(int frame)
        {
            Play(frame, 1, 1f, false);
        }

        private void LateUpdate()
        {
            if (_sprite == null) return;

            Sprite[] set = _phaseTwo || _dead ? hotFrames : frames;
            if (set == null || set.Length == 0) set = frames;
            if (set == null || set.Length == 0) return;

            int frame;
            if (_deathFrame >= 0)
            {
                frame = _deathFrame;
            }
            else
            {
                int step = (int)((Time.time - _animSince) * _animFps);
                step = _animLoop ? step % _animCount : Mathf.Min(step, _animCount - 1);
                frame = _animStart + step;

                bool calm = _animStart == Walk || _animStart == Idle || _animStart == ExposedPose;
                if (calm && Time.time < _hurtUntil) frame = Hurt;
            }

            if (frame >= 0 && frame < set.Length && set[frame] != null) _sprite.sprite = set[frame];
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

            // Fornalha exposta: o golpe entra em dobro.
            _health -= IsExposed ? amount * 2 : amount;
            if (_sprite != null) _sprite.color = new Color(1f, 0.35f, 0.3f);

            if (_health <= 0)
            {
                Die();
                return;
            }

            _hurtUntil = Time.time + 0.15f;

            if (!_phaseTwo && _health <= maxHealth / 2)
            {
                // Entra em brasa na hora; o urro e a nuvem vem quando o golpe
                // que ele estiver dando acabar.
                _phaseTwo = true;
                _roarPending = true;
            }
        }

        private void Die()
        {
            _dead = true;
            _defeated = true;
            StopAllCoroutines();

            if (Fighting == this) Fighting = null;

            StartCoroutine(Fall());
        }

        /// <summary>
        /// A cena da queda, como a do Coletor: a explosao em Oxide e, quando a
        /// nuvem assenta, os fragmentos, o Salto de Pressao, a grade da saida
        /// se abrindo e a purificacao do distrito.
        /// </summary>
        private IEnumerator Fall()
        {
            Cinematic.Begin();

            var controller = _player != null ? _player.GetComponent<ScrapController>() : null;
            var body = _player != null ? _player.GetComponent<Rigidbody2D>() : null;

            void Hold()
            {
                if (controller != null) controller.ControlLocked = true;
            }

            Hold();
            if (body != null) body.linearVelocity = new Vector2(0f, Mathf.Min(0f, body.linearVelocity.y));

            Vector3 place = new Vector3(transform.position.x, floorY, 0f);

            _deathFrame = Death;
            yield return BossFall.Explode(transform, _sprite, oxideCloud, place, Hold, 2.2f,
                k => _deathFrame = Death + Mathf.Min(DeathFrames - 1, (int)(k * DeathFrames)));

            for (int i = 0; i < 16; i++)
            {
                ShardPickup.Spawn(place + Vector3.up * 0.2f, shardSprite, 1, i);
            }

            PressureJumpPickup.Drop(place, rewardSprite);

            yield return BossFall.Hold(1.0f, Hold);

            if (exitGate != null) exitGate.Open();
            ScrapCameraFollow.Shake(0.12f, 0.5f);

            // A purificacao: o vapor do distrito volta a ser so vapor.
            if (purifier != null)
            {
                purifier.Purify(place);
                yield return BossFall.Hold(purifier.Duration + 0.2f, Hold);
            }

            if (controller != null) controller.ControlLocked = false;
            Cinematic.End();

            // A nuvem ainda esta no ar: so some com ele quando ela assentar.
            yield return new WaitForSeconds(3f);
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
