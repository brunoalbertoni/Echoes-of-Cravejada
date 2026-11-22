using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O Alquimista de Mercurio, o chefe do District 4: o Alquimista-Chefe dos
    /// Ateliers, que entrou no tanque grande pra SER mercurio. Educado, quer
    /// uma "amostra" do prototipo que ele mesmo descartou.
    ///
    /// LIQUIDO, ELE NAO APANHA: o cinzel atravessa e so espirra. Os tres
    /// circulos de transmutacao dos estrados sao a resposta — terminar de
    /// polir um com ele perto o transforma em PRATA MACICA por alguns
    /// segundos, e so ai ele apanha. Depois ele se solta e afoga aquele
    /// circulo em mercurio: e preciso ir a outro.
    ///
    ///   Campanula   ele aponta pro teto e uma redoma de vidro cai em cima do
    ///               Scrap (a sombra avisa). Preso, so o cinzel quebra.
    ///   Destilacao  ferve, vira uma nuvem prateada, passeia pelo teto
    ///               pingando gotas, e se condensa em outro lugar.
    ///   Pipeta      o braco vira um chicote de mercurio que varre a arena
    ///               numa altura: rente ao chao (pula-se ou sobe-se no
    ///               estrado) ou rente ao alto dos estrados (fica-se no chao,
    ///               sem pular). O fio fino aparece antes.
    ///   Geiser      curva-se e derrama mercurio no chao; colunas brotam
    ///               embaixo do Scrap, uma atras da outra, do chao ao teto —
    ///               pegam tambem quem esta em cima do estrado. O fio fino
    ///               vertical avisa onde. E o castigo de quem fica parado
    ///               num estrado.
    ///
    /// Ao se soltar da prata ele estoura: quem estiver colado leva o golpe, e
    /// gotas espirram pros dois lados. Bater ate o ultimo segundo custa caro.
    ///
    /// Na metade da vida ele ri, os tanques racham e o chao entre os estrados
    /// vira mercurio (o Scrap escorrega). Ele fica mais rapido, o chicote vem
    /// em dobro e o geiser em tres.
    ///
    /// Ao cair, escorre pelo ralo e a grade da saida abre. A purificacao vem
    /// depois, no palco do anfiteatro.
    ///
    /// Os avisos sao poses, nao piscadas: cada golpe tem a sua preparacao nos
    /// quadros do AlchemistArt, tocados por Play como nos outros chefes.
    /// </summary>
    public class MercuryAlchemist : MonoBehaviour, IDamageable
    {
        // Os quadros (os mesmos numeros do AlchemistArt).
        private const int Idle = 0, IdleFrames = 6;
        private const int Glide = 6, GlideFrames = 6;
        private const int BellCall = 12, BellCallFrames = 3;
        private const int Distill = 15, DistillFrames = 3;
        private const int Vapor = 18, VaporFrames = 3;
        private const int Condense = 21, CondenseFrames = 3;
        private const int WhipWindup = 24, WhipWindupFrames = 2;
        private const int Whip = 26, WhipFrames = 3;
        private const int SilverPose = 29, SilverFrames = 2;
        private const int BreakFree = 31, BreakFreeFrames = 3;
        private const int Smother = 34, SmotherFrames = 2;
        private const int Hurt = 36;
        private const int Laugh = 37, LaughFrames = 4;
        private const int Death = 41, DeathFrames = 8;

        [Header("Arena")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField] private float floorY;
        [SerializeField] private float ceilingY;
        [SerializeField] private Rect wakeArea;

        [Header("Luta")]
        [SerializeField] private int maxHealth = 40;
        [SerializeField] private float silverTime = 2.6f;
        [SerializeField] private float smotherTime = 9f;
        [SerializeField] private float transmuteRange = 4.2f;
        [SerializeField] private float glideSpeed = 4.2f;

        [Header("Pecas")]
        [SerializeField] private ArenaCircle[] circles;
        [SerializeField] private BellJar jar;
        [SerializeField] private GameObject[] flood;
        [SerializeField] private SpriteRenderer whip;
        [SerializeField] private Sprite beadSprite;

        [Tooltip("A carcaca que fica no chao depois da queda (ver BossWreck). Vazio: nao fica nada.")]
        [SerializeField] private GameObject wreck;
        [SerializeField] private Gate exitGate;

        [Header("Quadros")]
        [SerializeField] private Sprite[] frames;

        private SpriteRenderer _sprite;
        private Transform _player;
        private ScrapHealth _health;
        private Vector3 _home;

        private int _hp;
        private bool _awake;
        private bool _dead;
        private bool _phaseTwo;
        private bool _floodPending;
        private bool _silver;
        private bool _vapor;
        private int _facing = -1;
        private int _lastAttack = -1;
        private float _contactReady;

        private readonly List<SpriteRenderer> _geysers = new List<SpriteRenderer>();

        // As alturas do chicote, acima do chao. Os estrados tem 2 tiles: o
        // golpe alto passa rente ao alto deles e por cima da cabeca de quem
        // esta no chao.
        private const float WhipLow = 0.45f;
        private const float WhipHigh = 2.4f;

        // Quanto tempo cada coluna do geiser fica de pe.
        private const float GeyserBurst = 0.45f;

        private int _animStart;
        private int _animCount = 1;
        private float _animFps = 1f;
        private bool _animLoop = true;
        private float _animSince;
        private float _hurtUntil = -1f;

        private static bool _defeated;

        /// <summary>O chefe em luta, se houver. O HUD usa pra barra de vida.</summary>
        public static MercuryAlchemist Fighting { get; private set; }

        /// <summary>Se ele ja caiu de vez (a trilha sonora usa pra calar a musica da luta).</summary>
        public bool IsDown => _dead;

        public static bool Defeated => _defeated;

        public float HealthFraction => Mathf.Clamp01(_hp / (float)Mathf.Max(1, maxHealth));

        /// <summary>Em prata: a unica hora em que o cinzel fere.</summary>
        public bool IsSilver => _silver;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Fighting = null;
            _defeated = false;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(float newMinX, float newMaxX, float newFloorY, float newCeilingY, Rect newWakeArea,
            ArenaCircle[] newCircles, BellJar newJar, GameObject[] newFlood, SpriteRenderer newWhip, Sprite bead,
            Gate gate)
        {
            minX = newMinX;
            maxX = newMaxX;
            floorY = newFloorY;
            ceilingY = newCeilingY;
            wakeArea = newWakeArea;
            circles = newCircles;
            jar = newJar;
            flood = newFlood;
            whip = newWhip;
            beadSprite = bead;
            exitGate = gate;
        }

        /// <summary>Usado pelos geradores de fase: a folha do AlchemistArt.</summary>
        /// <summary>A carcaca que fica no lugar dele depois da queda. Usado pelo gerador.</summary>
        public void ConfigureWreck(GameObject newWreck)
        {
            wreck = newWreck;
        }

        public void ConfigureFrames(Sprite[] newFrames)
        {
            frames = newFrames;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _hp = maxHealth;

            // O corpo do tamanho do desenho: 2 de largura (os bracos abertos) e 3,6 de altura.
            var body = GetComponent<BoxCollider2D>();
            if (body != null)
            {
                body.size = new Vector2(2f, 3.6f);
                body.offset = new Vector2(0f, 1.8f);
            }
            _home = transform.position;
            Play(Idle, IdleFrames, 3f);
            SetFlood(false);
            HideWhip();

            ScrapHealth.Died += ResetFight;

            if (circles == null) return;
            foreach (ArenaCircle circle in circles)
            {
                if (circle != null) circle.Polished += OnPolished;
            }
        }

        private void Start()
        {
            // Purificado: nao sobra nem a poca.
            if (AreaPurifier.IsPurified)
            {
                if (exitGate != null) exitGate.Open();
                Destroy(gameObject);
                return;
            }

            // Ja caiu nesta sessao: fica a poca com a mascara, e a saida aberta.
            if (_defeated)
            {
                _dead = true;
                ShowFrame(Death + DeathFrames - 1);
                LeaveWreck();
                if (exitGate != null) exitGate.Open();
                SetFlood(true);
                return;
            }

            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null)
            {
                _player = controller.transform;
                _health = controller.GetComponent<ScrapHealth>();
            }
        }

        private void OnDestroy()
        {
            ScrapHealth.Died -= ResetFight;
            if (Fighting == this) Fighting = null;

            if (circles == null) return;
            foreach (ArenaCircle circle in circles)
            {
                if (circle != null) circle.Polished -= OnPolished;
            }
        }

        private void Update()
        {
            if (_dead || _player == null) return;

            if (!_awake)
            {
                FacePlayer();
                if (!wakeArea.Contains(_player.position)) return;

                _awake = true;
                Fighting = this;
                SetCircles(true);
                StartCoroutine(Brain());
                return;
            }

            // Encostar nele, liquido, machuca. Em prata ou em nuvem, nao.
            if (!_silver && !_vapor && Time.time >= _contactReady && _health != null)
            {
                Vector2 offset = _player.position - transform.position;
                // O corpo dele no desenho (1 tile pra cada lado), mais a metade do Scrap.
                if (Mathf.Abs(offset.x) < 1.3f && offset.y > -0.9f && offset.y < 3.5f)
                {
                    _health.TakeDamage(1, transform.position + Vector3.up);
                    _contactReady = Time.time + 0.8f;
                }
            }

            if (_sprite != null)
            {
                Color target = _silver ? new Color(0.92f, 0.96f, 1f) : Color.white;
                _sprite.color = Color.Lerp(_sprite.color, target, 10f * Time.deltaTime);
            }
        }

        // ------------------------------------------------------------------
        // O cerebro
        // ------------------------------------------------------------------

        private IEnumerator Brain()
        {
            // Acorda rindo: ja sabe quem e.
            FacePlayer();
            Play(Laugh, LaughFrames, 7f);
            Sfx.AlchemistLaugh.Play(transform.position);
            NoiseSystem.Emit(transform.position, 12f, 0.6f);
            yield return new WaitForSeconds(1.2f);

            yield return Loop();
        }

        private IEnumerator Loop()
        {
            while (!_dead)
            {
                if (_floodPending)
                {
                    _floodPending = false;
                    yield return FloodTheFloor();
                }

                int attack = NextAttack();
                _lastAttack = attack;

                if (attack == 0) yield return BellJarAttack();
                else if (attack == 1) yield return DistillAttack();
                else if (attack == 2) yield return WhipAttack();
                else yield return GeyserAttack();

                // Entre um golpe e outro, ele desliza atras do Scrap.
                yield return GlideToward(_phaseTwo ? 0.7f : 1.1f);
            }
        }

        private int NextAttack()
        {
            // Quem se instala em cima de um estrado chama o geiser.
            bool perched = _player != null && _player.position.y > floorY + 1.2f;
            if (perched && _lastAttack != 3 && Random.value < 0.5f) return 3;

            int attack;
            do attack = Random.Range(0, 4);
            while (attack == _lastAttack && Random.value < 0.75f);
            return attack;
        }

        private IEnumerator GlideToward(float seconds)
        {
            Play(Glide, GlideFrames, 8f);
            Sfx.AlchemistGlide.Play(transform.position);
            float speed = _phaseTwo ? glideSpeed * 1.3f : glideSpeed;

            for (float t = 0f; t < seconds && _player != null; t += Time.deltaTime)
            {
                float target = _player.position.x - _facing * 2.4f;
                float step = Mathf.Clamp(target - transform.position.x, -speed * Time.deltaTime, speed * Time.deltaTime);
                Move(step);
                FacePlayer();
                yield return null;
            }

            Play(Idle, IdleFrames, 6f);
            yield return new WaitForSeconds(_phaseTwo ? 0.12f : 0.28f);
        }

        // ------------------------------------------------------------------
        // Os golpes
        // ------------------------------------------------------------------

        private IEnumerator BellJarAttack()
        {
            FacePlayer();
            Play(BellCall, BellCallFrames, 5f, false);
            Sfx.AlchemistBellCall.Play(transform.position + Vector3.up * 3f);
            yield return new WaitForSeconds(0.6f);

            if (jar != null && _player != null) jar.Drop(Mathf.Clamp(_player.position.x, minX, maxX));

            Play(Idle, IdleFrames, 6f);
            yield return new WaitForSeconds(_phaseTwo ? 0.6f : 1.0f);
        }

        private IEnumerator DistillAttack()
        {
            FacePlayer();
            Play(Distill, DistillFrames, 5f, false);
            Sfx.AlchemistDistill.Play(transform.position);
            yield return new WaitForSeconds(0.65f);

            // Nuvem: sobe ao teto.
            _vapor = true;
            Play(Vapor, VaporFrames, 6f);
            float cloudY = ceilingY - 2.6f;

            for (float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                transform.position = new Vector3(transform.position.x, Mathf.Lerp(floorY, cloudY, t / 0.5f), 0f);
                yield return null;
            }

            // Passeia pingando.
            float rain = _phaseTwo ? 4.0f : 3.2f;
            float nextBead = 0f;
            for (float t = 0f; t < rain && _player != null; t += Time.deltaTime)
            {
                float target = _player.position.x;
                Move(Mathf.Clamp(target - transform.position.x, -4f * Time.deltaTime, 4f * Time.deltaTime), cloudY);

                if (Time.time >= nextBead)
                {
                    nextBead = Time.time + (_phaseTwo ? 0.16f : 0.23f);
                    float x = transform.position.x + Random.Range(-1.4f, 1.4f);
                    MercuryBead.Fall(new Vector3(x, cloudY + 0.3f, 0f), floorY, beadSprite);
                }

                yield return null;
            }

            // Escolhe onde se condensar: longe o bastante do Scrap pra ser justo.
            float land = transform.position.x;
            if (_player != null)
            {
                float side = _player.position.x > (minX + maxX) * 0.5f ? -1f : 1f;
                land = Mathf.Clamp(_player.position.x + side * 4f, minX, maxX);
            }

            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                Move(Mathf.Clamp(land - transform.position.x, -8f * Time.deltaTime, 8f * Time.deltaTime), cloudY);
                yield return null;
            }

            Play(Condense, CondenseFrames, 5f, false);
            Sfx.AlchemistCondense.Play(transform.position);
            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                transform.position = new Vector3(transform.position.x, Mathf.Lerp(cloudY, floorY, t / 0.6f), 0f);
                yield return null;
            }

            transform.position = new Vector3(transform.position.x, floorY, 0f);
            _vapor = false;
            FacePlayer();
            Play(Idle, IdleFrames, 6f);
            yield return new WaitForSeconds(0.4f);
        }

        private IEnumerator WhipAttack()
        {
            int lashes = _phaseTwo ? 3 : 2;
            bool high = Random.value < 0.5f;

            for (int lash = 0; lash < lashes; lash++)
            {
                FacePlayer();
                float height = high ? WhipHigh : WhipLow;

                // O aviso: ele recolhe o braco, a gota cresce na luva e o fio fino aparece.
                Play(WhipWindup, WhipWindupFrames, 4f, false);
                ShowWhip(height, true);
                Sfx.AlchemistWhipWindup.Play(transform.position, pitch: _phaseTwo && lash > 0 ? 1.4f : 1f);
                yield return new WaitForSeconds(lash > 0 ? 0.42f : 0.58f);

                // O golpe.
                Play(Whip, WhipFrames, 10f, false);
                ShowWhip(height, false);
                Sfx.AlchemistWhip.Play(transform.position + Vector3.right * (_facing * 3f), pitch: high ? 1.1f : 0.9f);
                for (float t = 0f; t < 0.35f; t += Time.deltaTime)
                {
                    WhipHit();
                    yield return null;
                }

                HideWhip();
                high = !high;
                yield return new WaitForSeconds(0.25f);
            }

            Play(Idle, IdleFrames, 6f);
            yield return new WaitForSeconds(0.5f);
        }

        private void ShowWhip(float height, bool preview)
        {
            if (whip == null) return;

            float start = transform.position.x + _facing * 0.8f;
            float end = _facing > 0 ? maxX + 1f : minX - 1f;
            float length = Mathf.Abs(end - start);

            whip.enabled = true;
            whip.transform.position = new Vector3((start + end) * 0.5f, floorY + height, 0f);
            whip.size = new Vector2(length, preview ? 0.12f : 0.4f);
            whip.color = preview ? new Color(1f, 1f, 1f, 0.35f) : Color.white;
        }

        private void HideWhip()
        {
            if (whip != null) whip.enabled = false;
        }

        private void WhipHit()
        {
            if (whip == null || _health == null || _player == null) return;

            Bounds bounds = whip.bounds;
            Vector2 scrap = _player.position;

            // O Scrap tem ~1,2 tile de altura, com os pes em position.
            bool across = scrap.x > bounds.min.x - 0.3f && scrap.x < bounds.max.x + 0.3f;
            bool level = bounds.center.y > scrap.y - 0.1f && bounds.center.y < scrap.y + 1.25f;

            if (across && level) _health.TakeDamage(1, new Vector2(transform.position.x, bounds.center.y));
        }

        private IEnumerator GeyserAttack()
        {
            // O aviso: curva-se e derrama mercurio no chao.
            FacePlayer();
            Play(Smother, SmotherFrames, 4f);
            Sfx.AlchemistPour.Play(transform.position);
            NoiseSystem.Emit(transform.position, 8f, 0.4f);
            yield return new WaitForSeconds(0.5f);

            int columns = _phaseTwo ? 3 : 2;
            float warn = _phaseTwo ? 0.6f : 0.75f;
            float gap = _phaseTwo ? 0.45f : 0.7f;
            EnsureGeysers(columns);

            // Cada coluna mira onde o Scrap esta agora: parar e apanhar.
            for (int i = 0; i < columns && i < _geysers.Count && _player != null; i++)
            {
                StartCoroutine(Geyser(_geysers[i], _player.position.x, warn));
                yield return new WaitForSeconds(gap);
            }

            // A ultima ainda sobe: ele aponta pro teto com ela.
            Play(BellCall, BellCallFrames, 5f, false);
            yield return new WaitForSeconds(Mathf.Max(0f, warn + GeyserBurst - gap) + 0.1f);

            HideGeysers();
            Play(Idle, IdleFrames, 6f);
            yield return new WaitForSeconds(_phaseTwo ? 0.3f : 0.5f);
        }

        private IEnumerator Geyser(SpriteRenderer column, float x, float warn)
        {
            float ground = GroundUnder(x);
            float top = ceilingY;

            // O aviso: o fio fino sobe do chao e treme.
            column.enabled = true;
            Sfx.AlchemistGeyserWarn.Play(new Vector2(x, ground), pitch: 0.75f / warn);
            for (float t = 0f; t < warn; t += Time.deltaTime)
            {
                float wobble = Mathf.Sin(t * 50f) * 0.05f;
                PlaceColumn(column, x + wobble, ground, top, false, new Color(1f, 1f, 1f, 0.3f + 0.3f * (t / warn)));
                yield return null;
            }

            // A erupcao: sobe de uma vez e fica um instante.
            NoiseSystem.Emit(new Vector3(x, ground, 0f), 9f, 0.5f);
            Sfx.AlchemistGeyserBurst.Play(new Vector2(x, ground));
            for (float t = 0f; t < GeyserBurst; t += Time.deltaTime)
            {
                float reach = Mathf.Lerp(ground + 0.5f, top, Mathf.Clamp01(t / 0.1f));
                PlaceColumn(column, x, ground, reach, true, Color.white);
                GeyserHit(x, ground, reach);
                yield return null;
            }

            column.enabled = false;
        }

        private static void PlaceColumn(SpriteRenderer column, float x, float ground, float top, bool full, Color color)
        {
            // O chicote deitado, de pe: o comprimento vira a altura.
            column.transform.SetPositionAndRotation(new Vector3(x, (ground + top) * 0.5f, 0f), Quaternion.Euler(0f, 0f, 90f));
            column.transform.localScale = full ? new Vector3(1f, 2.5f, 1f) : Vector3.one;
            column.size = new Vector2(Mathf.Max(0.1f, top - ground), full ? 0.4f : 0.12f);
            column.color = color;
        }

        private void GeyserHit(float x, float ground, float top)
        {
            if (_health == null || _player == null) return;

            Vector2 scrap = _player.position;
            if (Mathf.Abs(scrap.x - x) < 0.75f && scrap.y > ground - 0.3f && scrap.y < top)
                _health.TakeDamage(1, new Vector2(x, scrap.y));
        }

        /// <summary>As colunas sao copias do chicote, feitas na primeira vez.</summary>
        private void EnsureGeysers(int count)
        {
            if (whip == null) return;

            while (_geysers.Count < count)
            {
                GameObject copy = Instantiate(whip.gameObject, whip.transform.parent);
                copy.name = "MercuryGeyser";
                var column = copy.GetComponent<SpriteRenderer>();
                column.enabled = false;
                _geysers.Add(column);
            }
        }

        private void HideGeysers()
        {
            foreach (SpriteRenderer column in _geysers)
            {
                if (column != null) column.enabled = false;
            }
        }

        /// <summary>O chao embaixo de x: o piso da arena ou o alto de um estrado.</summary>
        private float GroundUnder(float x)
        {
            int mask = LayerMask.GetMask("Ground");
            RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(x, ceilingY - 3.2f), Vector2.down, ceilingY - floorY + 2f, mask);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider.isTrigger) continue;
                if (jar != null && hit.collider.transform.IsChildOf(jar.transform)) continue;
                return Mathf.Max(floorY, hit.point.y);
            }

            return floorY;
        }

        // ------------------------------------------------------------------
        // A prata
        // ------------------------------------------------------------------

        private void OnPolished(ArenaCircle circle)
        {
            if (!_awake || _dead || _silver || _vapor) return;
            if (Mathf.Abs(circle.transform.position.x - transform.position.x) > transmuteRange) return;

            // A runa estoura e o raio dela desce ate ele.
            circle.Strike(transform.position + Vector3.up * 1.4f);

            StopAllCoroutines();
            HideWhip();
            HideGeysers();
            transform.position = new Vector3(transform.position.x, floorY, 0f);
            StartCoroutine(Transmuted(circle));
        }

        private IEnumerator Transmuted(ArenaCircle circle)
        {
            _silver = true;
            Sfx.AlchemistTransmute.Play(transform.position);
            NoiseSystem.Emit(transform.position, 10f, 0.7f);
            Play(SilverPose, SilverFrames, 3f);

            yield return new WaitForSeconds(_phaseTwo ? silverTime * 0.85f : silverTime);

            // Se solta: racha, o mercurio vaza pelas fendas, e ele se sacode...
            Play(BreakFree, BreakFreeFrames, 5f, false);
            Sfx.AlchemistCracking.Play(transform.position);
            NoiseSystem.Emit(transform.position, 8f, 0.4f);
            yield return new WaitForSeconds(0.6f);
            _silver = false;

            // ...e estoura: quem ficou colado batendo leva o golpe e os respingos.
            BurstFree();

            // E afoga o circulo que o prendeu.
            Play(Smother, SmotherFrames, 4f);
            float x = circle.transform.position.x;
            for (float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                Move(Mathf.Clamp(x - transform.position.x, -5f * Time.deltaTime, 5f * Time.deltaTime));
                yield return null;
            }

            circle.Smother(smotherTime);
            Sfx.AlchemistPour.Play(circle.transform.position, pitch: 0.85f);
            yield return new WaitForSeconds(0.5f);

            yield return Loop();
        }

        private void BurstFree()
        {
            NoiseSystem.Emit(transform.position, 12f, 0.7f);
            Sfx.AlchemistBurst.Play(transform.position);

            if (_health != null && _player != null)
            {
                Vector2 offset = _player.position - transform.position;
                if (Mathf.Abs(offset.x) < 2.3f && offset.y > -0.5f && offset.y < 3.2f)
                    _health.TakeDamage(1, transform.position + Vector3.up);
            }

            if (beadSprite == null) return;

            // Gotas pros dois lados, das mais curtas as mais longas.
            int drops = _phaseTwo ? 8 : 6;
            Vector3 chest = transform.position + Vector3.up * 2f;
            for (int i = 0; i < drops; i++)
            {
                float side = i % 2 == 0 ? 1f : -1f;
                float speed = 3f + (i / 2) * 2.2f;
                MercuryBead.Splash(chest, new Vector2(side * speed, 9f + Random.Range(-1f, 1f)), floorY, beadSprite);
            }
        }

        public void Damage(int amount, Vector2 from)
        {
            if (_dead || !_awake) return;

            if (!_silver)
            {
                // Liquido: o cinzel atravessa e so espirra.
                if (_sprite != null) _sprite.color = new Color(0.8f, 0.9f, 1f, 0.75f);
                NoiseSystem.Emit(transform.position, 4f, 0.2f);
                Sfx.AlchemistSplash.Play(transform.position + Vector3.up);
                return;
            }

            Sfx.BossHurtSilver.Play(transform.position, pitch: 1.1f);

            _hp -= amount;
            _hurtUntil = Time.time + 0.15f;
            if (_sprite != null) _sprite.color = new Color(1f, 0.6f, 0.6f);

            if (_hp <= 0)
            {
                Die();
                return;
            }

            if (!_phaseTwo && _hp <= maxHealth / 2)
            {
                _phaseTwo = true;
                _floodPending = true;
            }
        }

        private IEnumerator FloodTheFloor()
        {
            // Ri, e os tanques racham: o chao entre os estrados vira mercurio.
            FacePlayer();
            Play(Laugh, LaughFrames, 7f);
            Sfx.AlchemistLaugh.Play(transform.position, pitch: 0.9f);
            NoiseSystem.Emit(transform.position, 14f, 0.8f);
            yield return new WaitForSeconds(0.7f);
            SetFlood(true);
            Sfx.AlchemistFlood.Play(transform.position);
            yield return new WaitForSeconds(0.7f);
        }

        // ------------------------------------------------------------------
        // Cair e recomecar
        // ------------------------------------------------------------------

        private void Die()
        {
            _dead = true;
            _defeated = true;
            _silver = false;
            StopAllCoroutines();
            HideWhip();
            HideGeysers();
            SetCircles(false);
            if (jar != null) jar.Hide();
            if (Fighting == this) Fighting = null;

            StartCoroutine(DrainAway());
        }

        private IEnumerator DrainAway()
        {
            Play(Death, DeathFrames, 5f, false);
            Sfx.AlchemistDrain.Play(transform.position);
            NoiseSystem.Emit(transform.position, 12f, 0.6f);
            yield return new WaitForSeconds(DeathFrames / 5f + 0.4f);

            LeaveWreck();
            if (exitGate != null) exitGate.Open();
        }

        /// <summary>A poca vira a carcaca desenhada: o jaleco, a mascara, o tanque vazando, o vapor.</summary>
        private void LeaveWreck()
        {
            if (wreck == null) return;
            if (_sprite != null) _sprite.enabled = false;
            BossWreck.Show(wreck, transform.position);
        }

        /// <summary>O Scrap morreu: a luta recomeca do zero.</summary>
        private void ResetFight()
        {
            if (_dead) return;

            StopAllCoroutines();

            _hp = maxHealth;
            _awake = false;
            _phaseTwo = false;
            _floodPending = false;
            _silver = false;
            _vapor = false;
            _lastAttack = -1;
            transform.position = _home;

            HideWhip();
            HideGeysers();
            SetFlood(false);
            SetCircles(false);
            if (jar != null) jar.Hide();
            if (circles != null) foreach (ArenaCircle circle in circles) if (circle != null) circle.ResetCircle();

            if (_sprite != null) _sprite.color = Color.white;
            Play(Idle, IdleFrames, 3f);

            if (Fighting == this) Fighting = null;
        }

        private void SetFlood(bool on)
        {
            if (flood == null) return;
            foreach (GameObject pool in flood)
            {
                if (pool != null) pool.SetActive(on);
            }
        }

        private void SetCircles(bool armed)
        {
            if (circles == null) return;
            foreach (ArenaCircle circle in circles)
            {
                if (circle != null) circle.Armed = armed;
            }
        }

        // ------------------------------------------------------------------
        // Movimento e animacao
        // ------------------------------------------------------------------

        private void Move(float deltaX, float y = float.NaN)
        {
            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x + deltaX, minX, maxX);
            if (!float.IsNaN(y)) position.y = y;
            transform.position = position;
        }

        private void FacePlayer()
        {
            if (_player == null) return;
            _facing = _player.position.x >= transform.position.x ? 1 : -1;

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = _facing < 0;
        }

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

        private void ShowFrame(int frame)
        {
            Play(frame, 1, 1f, false);
        }

        private void LateUpdate()
        {
            if (_sprite == null || frames == null || frames.Length == 0) return;

            int step = (int)((Time.time - _animSince) * _animFps);
            step = _animLoop ? step % _animCount : Mathf.Min(step, _animCount - 1);
            int frame = _animStart + step;

            if (_silver && Time.time < _hurtUntil) frame = Hurt;

            if (frame >= 0 && frame < frames.Length && frames[frame] != null) _sprite.sprite = frames[frame];
        }
    }
}
