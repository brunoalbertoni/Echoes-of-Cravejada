using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O Rei de Prata, o chefe final. Espera sentado no trono, de cabeca
    /// baixa. Quando o Scrap entra, a cena: os olhos acendem, ele encara o
    /// Scrap e fala (caixa de dialogo); se levanta, desce os degraus e aponta
    /// o cetro. Ai a luta. Ele apanha a qualquer hora, mas cada golpe tem
    /// aviso (ele pisca em branco). Durante a luta, nada de aviso na tela: as
    /// falas so na entrada, na troca de fase e no fim.
    ///
    /// FASE 1 — O REI DE PRATA
    ///   Cetrada      perto: recolhe o cetro por cima do ombro e varre rente
    ///                ao chao. Depois fica apoiado, arfando — exposto, o cinzel
    ///                fere em dobro.
    ///   Raio         meio longe: aponta o cetro; o fio fino mira; o raio varre
    ///                do alto ate o chao na frente dele.
    ///   Facetas      longe: a coroa solta lascas de luz que quicam.
    ///   Coluna       quem fica em cima dele (na sacada, ou pulando pra bater
    ///                na cabeca) leva a Coluna de Luz: ele agacha (o aviso) e
    ///                enfia o cetro pro alto — uma coluna de luz ate o teto e
    ///                dois raios em leque. Ficar la em cima nao e seguro.
    ///
    /// A TRANSFORMACAO: a vida zera, ele cai de joelhos e a coroa racha; ele
    /// fala; a cutscene desenhada; ele se ergue inteiro em Oxide, flutuando.
    /// A vida volta cheia, a sala fica verde, os espelhos quebram e viram
    /// escudo.
    ///
    /// FASE 2 — O REI CORROMPIDO (flutua, mais rapido, mais vida)
    ///   Cetrada, Coluna (mais rapidas), Tesoura (o raio pros dois lados),
    ///   Chuva de facetas, Sopro de Oxide (so os cacos de espelho ou o alto
    ///   protegem; depois arfa, exposto) e Investida (desliza a sala toda).
    ///
    /// Ao cair: larga o cetro, ajoelha, a coroa rola, as ultimas palavras e a
    /// passagem pro Altar do Coracao abre. Se o Scrap morre no meio da luta,
    /// tudo volta ao comeco: o Rei sentado no trono, na fase 1.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SilverKing : MonoBehaviour, IDamageable
    {
        // A ordem dos quadros. E a mesma do KingArt, no editor (as duas folhas).
        public const int Seated = 0;
        public const int SeatedFrames = 4;
        public const int Rise = 4;
        public const int RiseFrames = 10;
        public const int Idle = 14;
        public const int IdleFrames = 6;
        public const int Walk = 20;
        public const int WalkFrames = 8;
        public const int SweepWindup = 28;
        public const int SweepWindupFrames = 2;
        public const int Sweep = 30;
        public const int SweepFrames = 3;
        public const int Exposed = 33;
        public const int ExposedFrames = 2;
        public const int BeamRaise = 35;
        public const int BeamRaiseFrames = 2;
        public const int BeamFire = 37;
        public const int BeamFireFrames = 3;
        public const int Cast = 40;
        public const int CastFrames = 3;
        public const int Skyward = 43;
        public const int SkywardFrames = 3;
        public const int Inhale = 46;
        public const int InhaleFrames = 3;
        public const int Breath = 49;
        public const int BreathFrames = 3;
        public const int Hurt = 52;
        public const int Fall = 53;
        public const int FallFrames = 4;
        public const int Death = 57;
        public const int DeathFrames = 8;

        private const string Name = "O Rei de Prata";

        private static readonly string[] IntroLines =
        {
            "...Uma mancha. No meu palácio.",
            "Eu conheço você. O protótipo de sucata. Fui eu que assinei: \"que não se fale mais nisso\".",
            "E agora a sucata sobe as minhas escadas e quer polir o meu reino?",
            "A prata não se curva à ferrugem. Venha.",
        };

        private static readonly string[] TransitionLines =
        {
            "Não... A cidade não pode... escurecer.",
            "Se a minha luz não basta... que seja a do Coração inteiro.",
        };

        private static readonly string[] RisenLines =
        {
            "Agora a Coluna inteira me ilumina, sucata. E nada que brilha pode me tocar.",
        };

        private static readonly string[] LastLines =
        {
            "...Eu só queria... que a cidade não escurecesse.",
            "Cada ordem... um pouco pior que a outra. Até que eu era só a ordem.",
            "Diga ao lapidário... que eram novecentas e vinte e cinco. Eu contei.",
        };

        [Header("Sala")]
        [SerializeField] private float minX;
        [SerializeField] private float maxX;
        [SerializeField] private float floorY;
        [SerializeField] private float ceilingY;
        [SerializeField] private Rect wakeArea;

        [Header("Vida")]
        [SerializeField] private int phaseOneHealth = 36;
        [SerializeField] private int phaseTwoHealth = 48;

        [Header("Pontos (em tiles a partir do pivo, olhando pra direita)")]
        [SerializeField] private Vector2 scepterTip = new Vector2(3.4f, 3.1f);
        [SerializeField] private Vector2 skywardTip = new Vector2(1f, 7f);
        [SerializeField] private Vector2 crownPoint = new Vector2(-0.3f, 5.5f);
        [SerializeField] private Vector2 mouthPoint = new Vector2(1.5f, 4f);

        [Tooltip("Onde o cetro varre, em tiles a frente dele.")]
        [SerializeField] private float sweepReach = 3.1f;

        [Header("Pecas")]
        [SerializeField] private KingBeam beamA;
        [SerializeField] private KingBeam beamB;
        [SerializeField] private OxideBreath breath;
        [SerializeField] private Sprite facetSprite;
        [SerializeField] private Sprite facetCorruptSprite;
        [SerializeField] private Sprite glintSprite;

        [Header("A sala na fase 2")]
        [SerializeField] private GameObject[] phaseOneObjects;
        [SerializeField] private GameObject[] phaseTwoObjects;
        [SerializeField] private Light2D globalLight;
        [SerializeField] private Color phaseTwoLight = new Color(0.55f, 0.85f, 0.65f);

        [Header("A cutscene e o fim")]
        [SerializeField] private PurificationCartoon cartoon;
        [SerializeField] private Gate altarGate;
        [SerializeField] private GameObject fallenCrown;

        [Header("Quadros")]
        [SerializeField] private Sprite[] regalFrames;
        [SerializeField] private Sprite[] corruptFrames;

        private SpriteRenderer _sprite;
        private Collider2D _collider;
        private Transform _player;
        private Vector3 _home;
        private Color _baseLight = Color.white;

        private int _hp;
        private bool _awake;
        private bool _dead;
        private bool _phaseTwo;
        private bool _scene;
        private bool _charging;
        private bool _flinchable;
        private int _facing = -1;
        private int _lastAttack = -1;
        private float _exposedUntil;
        private float _hurtUntil = -1f;
        private float _skywardReady;

        private int _animStart;
        private int _animCount = SeatedFrames;
        private float _animFps = 2f;
        private bool _animLoop = true;
        private float _animSince;

        private static bool _defeated;
        private static bool _introHeard;
        private static bool _transformationSeen;

        /// <summary>O Rei em luta, se houver. O HUD usa pra barra.</summary>
        public static SilverKing Fighting { get; private set; }

        /// <summary>Se ele ja caiu de vez (a trilha sonora usa pra calar a musica da luta).</summary>
        public bool IsDown => _dead;

        public static bool Defeated => _defeated;

        public float HealthFraction => Mathf.Clamp01(_hp / (float)Mathf.Max(1, _phaseTwo ? phaseTwoHealth : phaseOneHealth));

        public bool IsPhaseTwo => _phaseTwo;

        /// <summary>Verdadeiro durante as cenas (a entrada, a transformacao): o HUD esconde a barra.</summary>
        public bool IsTransforming => _scene;

        /// <summary>Verdadeiro enquanto ele esta exposto (apoiado no cetro ou arfando): o cinzel fere em dobro.</summary>
        public bool IsExposed => Time.time < _exposedUntil;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            Fighting = null;
            _defeated = false;
            _introHeard = false;
            _transformationSeen = false;
        }

        /// <summary>Usado pelos geradores de fase: a sala e os pontos do desenho.</summary>
        public void Configure(float newMinX, float newMaxX, float newFloorY, float newCeilingY, Rect newWakeArea,
            Vector2 tip, Vector2 skyward, Vector2 crown, Vector2 mouth, float reach)
        {
            minX = newMinX;
            maxX = newMaxX;
            floorY = newFloorY;
            ceilingY = newCeilingY;
            wakeArea = newWakeArea;
            scepterTip = tip;
            skywardTip = skyward;
            crownPoint = crown;
            mouthPoint = mouth;
            sweepReach = reach;
        }

        /// <summary>Usado pelos geradores de fase: os golpes.</summary>
        public void ConfigureAttacks(KingBeam a, KingBeam b, OxideBreath newBreath, Sprite facet, Sprite facetCorrupt, Sprite glint)
        {
            beamA = a;
            beamB = b;
            breath = newBreath;
            facetSprite = facet;
            facetCorruptSprite = facetCorrupt;
            glintSprite = glint;
        }

        /// <summary>Usado pelos geradores de fase: o que muda na sala, a cutscene e o fim.</summary>
        public void ConfigureStage(GameObject[] objectsOne, GameObject[] objectsTwo, Light2D global,
            PurificationCartoon newCartoon, Gate gate, GameObject crown)
        {
            phaseOneObjects = objectsOne;
            phaseTwoObjects = objectsTwo;
            globalLight = global;
            cartoon = newCartoon;
            altarGate = gate;
            fallenCrown = crown;
        }

        /// <summary>Usado pelos geradores de fase: as duas folhas do KingArt.</summary>
        public void ConfigureFrames(Sprite[] regal, Sprite[] corrupt)
        {
            regalFrames = regal;
            corruptFrames = corrupt;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
            _home = transform.position;
            if (globalLight != null) _baseLight = globalLight.color;

            ScrapHealth.Died += ResetFight;
        }

        private void Start()
        {
            var controller = FindAnyObjectByType<ScrapController>();
            if (controller != null) _player = controller.transform;

            if (_defeated)
            {
                // Ja caiu: fica deitado, a coroa no chao, o Altar aberto.
                _dead = true;
                _phaseTwo = true;
                SetActive(phaseOneObjects, false);
                SetActive(phaseTwoObjects, true);
                if (globalLight != null) globalLight.color = phaseTwoLight;
                Play(Death + DeathFrames - 1, 1, 1f, false);
                if (_collider != null) _collider.enabled = false;
                if (fallenCrown != null) fallenCrown.SetActive(true);
                if (altarGate != null) altarGate.Open();
                return;
            }

            if (fallenCrown != null) fallenCrown.SetActive(false);
            EnterPhaseOne();
        }

        private void OnDestroy()
        {
            ScrapHealth.Died -= ResetFight;
            if (Fighting == this) Fighting = null;
        }

        /// <summary>O comeco de tudo: sentado no trono, a sala de prata, a vida cheia.</summary>
        private void EnterPhaseOne()
        {
            _phaseTwo = false;
            _hp = phaseOneHealth;
            transform.position = _home;
            Face(-1);
            Play(Seated, SeatedFrames, 2f);
            SetActive(phaseOneObjects, true);
            SetActive(phaseTwoObjects, false);
            if (globalLight != null) globalLight.color = _baseLight;
        }

        private void ApplyPhaseTwoLook()
        {
            _phaseTwo = true;
            SetActive(phaseOneObjects, false);
            SetActive(phaseTwoObjects, true);
            if (globalLight != null) globalLight.color = phaseTwoLight;
        }

        private void Update()
        {
            if (_sprite != null && !_dead && Time.time >= _hurtUntil)
            {
                // Exposto: um pouco em brasa (a deixa pra bater).
                Color target = IsExposed ? new Color(1f, 0.82f, 0.72f) : Color.white;
                _sprite.color = Color.Lerp(_sprite.color, target, 10f * Time.deltaTime);
            }

            if (_dead || _player == null || _awake) return;
            if (!wakeArea.Contains(_player.position)) return;

            _awake = true;
            Fighting = this;
            StartCoroutine(Intro());
        }

        // ------------------------------------------------------------------
        // A entrada
        // ------------------------------------------------------------------

        /// <summary>A cena da entrada: acorda, encara, fala, se levanta, desce do trono e aponta o cetro.</summary>
        private IEnumerator Intro()
        {
            _scene = true;
            var controller = FindAnyObjectByType<ScrapController>();
            bool began = !Cinematic.Active;
            if (began) Cinematic.Begin();
            if (controller != null) controller.ControlLocked = true;

            ScrapCameraFollow.Direct((Vector2)transform.position + Vector2.up * 3.5f, 0.9f, 0.7f);
            Face(_player != null && _player.position.x >= transform.position.x ? 1 : -1);

            // Os olhos acendem, a cabeca sobe, ele encara.
            float[] wake = { 0.55f, 0.55f, 0.5f };
            for (int i = 0; i < wake.Length; i++)
            {
                Play(Rise + i, 1, 1f, false);
                yield return new WaitForSeconds(wake[i]);
            }

            if (!_introHeard)
            {
                _introHeard = true;
                yield return ScriptedDialogue.Play(Name, _sprite, IntroLines);
            }

            // Firma o cetro, inclina, se levanta, desce os degraus, aponta.
            float[] rise = { 0.3f, 0.3f, 0.3f, 0.35f, 0.25f, 0.25f, 0.55f };
            for (int i = 0; i < rise.Length; i++)
            {
                Play(Rise + 3 + i, 1, 1f, false);
                if (i == 5) ScrapCameraFollow.Shake(0.15f, 0.3f);
                yield return new WaitForSeconds(rise[i]);
            }

            ScrapCameraFollow.Release();
            if (controller != null) controller.ControlLocked = false;
            if (began) Cinematic.End();
            _scene = false;

            _skywardReady = Time.time + 1f;
            yield return Fight();
        }

        // ------------------------------------------------------------------
        // A luta
        // ------------------------------------------------------------------

        private IEnumerator Fight()
        {
            while (!_dead)
            {
                if (PlayerAbove() && Time.time >= _skywardReady)
                {
                    yield return SkyLance();
                }
                else
                {
                    yield return Approach();
                    if (_dead) yield break;

                    if (PlayerAbove() && Time.time >= _skywardReady) yield return SkyLance();
                    else yield return _phaseTwo ? PhaseTwoAttack() : PhaseOneAttack();
                }

                // O respiro entre um golpe e outro — curto, e cortado se o Scrap sobe nele.
                Play(Idle, IdleFrames, 6f);
                _flinchable = true;
                float rest = _phaseTwo ? 0.3f : 0.5f;
                for (float t = 0f; t < rest; t += Time.deltaTime)
                {
                    if (PlayerAbove() && Time.time >= _skywardReady) break;
                    yield return null;
                }

                _flinchable = false;
            }
        }

        /// <summary>
        /// O Scrap esta em cima dele: numa sacada logo acima, ou pulando pra
        /// bater na cabeca dele.
        /// </summary>
        private bool PlayerAbove()
        {
            if (_player == null) return false;

            Vector2 offset = _player.position - transform.position;
            return offset.y > 2.2f && Mathf.Abs(offset.x) < 4.5f;
        }

        /// <summary>Uns passos na direcao do Scrap.</summary>
        private IEnumerator Approach()
        {
            _flinchable = true;
            float time = Random.Range(0.4f, _phaseTwo ? 0.6f : 0.9f);
            float speed = _phaseTwo ? 3.2f : 2.3f;
            Play(Walk, WalkFrames, speed * 3.5f);

            for (float t = 0f; t < time; t += Time.deltaTime)
            {
                FacePlayer();
                if (Distance() < 3f) break;
                if (PlayerAbove() && Time.time >= _skywardReady) break;
                if (!Move(_facing * speed * Time.deltaTime)) break;
                yield return null;
            }

            _flinchable = false;
        }

        private int Pick(int[] options)
        {
            for (int tries = 0; tries < 6; tries++)
            {
                int pick = options[Random.Range(0, options.Length)];
                if (pick != _lastAttack || options.Length == 1) return _lastAttack = pick;
            }

            return _lastAttack = options[0];
        }

        private IEnumerator PhaseOneAttack()
        {
            float distance = Distance();
            int attack = distance < 4f ? Pick(new[] { 0, 0, 1 }) : (distance < 10f ? Pick(new[] { 1, 1, 2 }) : Pick(new[] { 2, 1 }));

            switch (attack)
            {
                case 0: yield return ScepterSweep(); break;
                case 1: yield return ScepterBeam(); break;
                default: yield return CrownFacets(5, false); break;
            }
        }

        private IEnumerator PhaseTwoAttack()
        {
            float distance = Distance();
            int attack = distance < 4f ? Pick(new[] { 0, 0, 3, 4 }) : Pick(new[] { 1, 2, 3, 4 });

            switch (attack)
            {
                case 0: yield return ScepterSweep(); break;
                case 1: yield return LightScissors(); break;
                case 2: yield return FacetRain(); break;
                case 3: yield return OxideBreathAttack(); break;
                default: yield return Glide(); break;
            }
        }

        // ------------------------------------------------------------------
        // Os golpes
        // ------------------------------------------------------------------

        /// <summary>A cetrada: recolhe por cima do ombro e varre rente ao chao; depois fica exposto, apoiado.</summary>
        private IEnumerator ScepterSweep()
        {
            FacePlayer();
            float windup = _phaseTwo ? 0.34f : 0.48f;
            Play(SweepWindup, SweepWindupFrames, SweepWindupFrames / windup, false);
            yield return Warn(windup);

            Play(Sweep, SweepFrames, SweepFrames / 0.24f, false);
            yield return new WaitForSeconds(0.08f);

            Vector2 tip = new Vector2(transform.position.x + _facing * sweepReach * 0.75f, floorY + 0.9f);
            Hit(tip, new Vector2(sweepReach * 1.4f, 1.8f), 2);
            ScrapCameraFollow.Shake(0.12f, 0.3f);
            yield return new WaitForSeconds(0.2f);

            // Apoiado no cetro, arfando: a hora de bater.
            _exposedUntil = Time.time + (_phaseTwo ? 0.8f : 1.1f);
            Play(Exposed, ExposedFrames, 4f);
            while (IsExposed && !_dead) yield return null;
        }

        /// <summary>
        /// A Coluna de Luz, contra quem fica em cima dele: agacha com o cetro
        /// baixo (o aviso), enfia o cetro pro alto — a coluna queima tudo
        /// acima dele ate o teto — e dois raios abrem em leque, com as
        /// facetas subindo.
        /// </summary>
        private IEnumerator SkyLance()
        {
            _skywardReady = Time.time + (_phaseTwo ? 1.6f : 2.2f);
            FacePlayer();

            float windup = _phaseTwo ? 0.26f : 0.36f;
            Play(Skyward, 1, 1f, false);
            yield return Warn(windup);

            Play(Skyward + 1, SkywardFrames - 1, 6f, false);
            Vector2 origin = Point(skywardTip);

            // A coluna: tudo em cima dele, do peito ao teto.
            float bottom = floorY + 2f;
            Hit(new Vector2(transform.position.x, (bottom + ceilingY) * 0.5f), new Vector2(3.6f, ceilingY - bottom), 2);
            ScrapCameraFollow.Shake(0.2f, 0.4f);

            for (int i = 0; i < 5; i++)
            {
                float angle = Mathf.Lerp(55f, 125f, i / 4f);
                Vector2 velocity = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * 11f;
                FacetShard.Launch(origin, velocity, _phaseTwo ? 2 : 1, _phaseTwo ? facetCorruptSprite : facetSprite);
            }

            // Os dois raios abrindo em leque, por cima dele.
            for (float t = 0f; t < 0.55f; t += Time.deltaTime)
            {
                float k = t / 0.55f;
                if (beamA != null) beamA.Fire(origin, Mathf.Lerp(92f, 48f, k));
                if (beamB != null) beamB.Fire(origin, Mathf.Lerp(88f, 132f, k));
                yield return null;
            }

            if (beamA != null) beamA.Hide();
            if (beamB != null) beamB.Hide();

            Play(Idle, IdleFrames, 6f);
            yield return new WaitForSeconds(_phaseTwo ? 0.15f : 0.25f);
        }

        /// <summary>O raio: aponta, o fio fino mira, e o raio varre do alto ate o chao na frente.</summary>
        private IEnumerator ScepterBeam()
        {
            FacePlayer();
            Play(BeamRaise, BeamRaiseFrames, BeamRaiseFrames / 0.4f, false);
            yield return new WaitForSeconds(0.4f);

            Play(BeamFire, 1, 1f, false);
            Vector2 origin = Point(scepterTip);

            // Do alto, na frente, ate o chao na frente (y pra cima: angulo positivo e pra cima).
            float from = Angle(35f);
            float to = Angle(-32f);

            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                if (beamA != null) beamA.Aim(origin, from);
                yield return null;
            }

            Play(BeamFire, BeamFireFrames, 8f);
            ScrapCameraFollow.Shake(0.1f, 1.1f);

            for (float t = 0f; t < 1.1f; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / 1.1f);
                if (beamA != null) beamA.Fire(origin, Mathf.LerpAngle(from, to, k));
                yield return null;
            }

            if (beamA != null) beamA.Hide();
        }

        /// <summary>As facetas: a coroa solta lascas de luz pra frente, que quicam nas paredes.</summary>
        private IEnumerator CrownFacets(int count, bool corrupt)
        {
            FacePlayer();
            Play(Cast, CastFrames, CastFrames / 0.45f, false);
            yield return Warn(0.45f);

            Vector2 origin = Point(crownPoint);
            for (int i = 0; i < count; i++)
            {
                float angle = Angle(Mathf.Lerp(15f, 75f, count == 1 ? 0.5f : i / (float)(count - 1)));
                Vector2 velocity = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * 10f;
                FacetShard.Launch(origin, velocity, 3, corrupt ? facetCorruptSprite : facetSprite);
            }

            ScrapCameraFollow.Shake(0.08f, 0.3f);
            yield return new WaitForSeconds(0.9f);
        }

        /// <summary>A tesoura: o raio sai pros dois lados ao mesmo tempo e cruza a sala.</summary>
        private IEnumerator LightScissors()
        {
            Play(BeamRaise, BeamRaiseFrames, BeamRaiseFrames / 0.35f, false);
            yield return new WaitForSeconds(0.35f);

            Play(BeamFire, 1, 1f, false);
            Vector2 origin = Point(scepterTip);
            Vector2 crown = Point(crownPoint);

            for (float t = 0f; t < 0.65f; t += Time.deltaTime)
            {
                if (beamA != null) beamA.Aim(origin, 50f);
                if (beamB != null) beamB.Aim(crown, 130f);
                yield return null;
            }

            Play(BeamFire, BeamFireFrames, 8f);
            ScrapCameraFollow.Shake(0.15f, 1.1f);

            for (float t = 0f; t < 1.05f; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / 1.05f);
                if (beamA != null) beamA.Fire(origin, Mathf.Lerp(50f, -28f, k));
                if (beamB != null) beamB.Fire(crown, Mathf.Lerp(130f, 208f, k));
                yield return null;
            }

            if (beamA != null) beamA.Hide();
            if (beamB != null) beamB.Hide();
        }

        /// <summary>A chuva de facetas: brilhos no teto avisam, as lascas caem com vaos.</summary>
        private IEnumerator FacetRain()
        {
            Play(Cast, CastFrames, CastFrames / 0.4f, false);
            yield return new WaitForSeconds(0.35f);

            for (int wave = 0; wave < 3; wave++)
            {
                var glints = new List<GameObject>();
                var spots = new List<float>();
                float offset = wave % 2 == 0 ? 0f : 2f;

                for (float x = minX + offset; x <= maxX; x += 4f)
                {
                    spots.Add(x);
                    spots.Add(x + 1f);
                }

                foreach (float x in spots) glints.Add(Glint(new Vector2(x, ceilingY - 1.5f)));
                yield return new WaitForSeconds(0.65f);

                foreach (GameObject glint in glints) if (glint != null) Destroy(glint);
                foreach (float x in spots) FacetShard.Launch(new Vector2(x, ceilingY - 1.5f), Vector2.down * 14f, 0, facetCorruptSprite);

                ScrapCameraFollow.Shake(0.1f, 0.3f);
                yield return new WaitForSeconds(0.75f);
            }
        }

        private GameObject Glint(Vector2 at)
        {
            var glint = new GameObject("FacetGlint");
            glint.transform.position = at;
            var renderer = glint.AddComponent<SpriteRenderer>();
            renderer.sprite = glintSprite;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = 9;
            return glint;
        }

        /// <summary>O sopro: inspira (o aviso), sopra uma onda rente ao chao, e fica arfando, exposto.</summary>
        private IEnumerator OxideBreathAttack()
        {
            FacePlayer();
            Play(Inhale, InhaleFrames, InhaleFrames / 1f, false);
            yield return Warn(1f);

            Play(Breath, BreathFrames, 7f);
            ScrapCameraFollow.Shake(0.2f, 1.1f);
            if (breath != null) yield return breath.Blow(transform.position.x);

            _exposedUntil = Time.time + 1f;
            Play(Exposed, ExposedFrames, 4f);
            while (IsExposed && !_dead) yield return null;
        }

        /// <summary>A investida: flutua rapido pela sala inteira, ate o outro lado.</summary>
        private IEnumerator Glide()
        {
            FacePlayer();
            Play(SweepWindup, 1, 1f, false);
            yield return Warn(0.4f);

            float wall = _facing > 0 ? maxX : minX;
            _charging = true;
            Play(Walk, WalkFrames, 16f);

            while (!Mathf.Approximately(transform.position.x, wall) && !_dead)
            {
                float x = Mathf.MoveTowards(transform.position.x, wall, 11f * Time.deltaTime);
                transform.position = new Vector3(x, floorY, 0f);
                yield return null;
            }

            _charging = false;
            Face(-_facing);
        }

        /// <summary>O aviso antes do golpe: pisca em branco.</summary>
        private IEnumerator Warn(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                if (_sprite != null && Time.time >= _hurtUntil)
                {
                    _sprite.color = Mathf.FloorToInt(t * 14f) % 2 == 0 ? new Color(1f, 1f, 0.75f) : Color.white;
                }

                yield return null;
            }

            if (_sprite != null) _sprite.color = Color.white;
        }

        private void Hit(Vector2 center, Vector2 size, int damage)
        {
            foreach (Collider2D hit in Physics2D.OverlapBoxAll(center, size, 0f))
            {
                var health = hit.GetComponentInParent<ScrapHealth>();
                if (health != null) health.TakeDamage(damage, transform.position);
            }
        }

        // ------------------------------------------------------------------
        // Contas
        // ------------------------------------------------------------------

        private float Distance() => _player == null ? 0f : Mathf.Abs(_player.position.x - transform.position.x);

        /// <summary>Um ponto do desenho (olhando pra direita), virado pro lado em que ele olha.</summary>
        private Vector2 Point(Vector2 local) => (Vector2)transform.position + new Vector2(local.x * _facing, local.y);

        /// <summary>Um angulo de quem olha pra direita, virado pro lado em que ele olha.</summary>
        private float Angle(float right) => _facing > 0 ? right : 180f - right;

        private void FacePlayer()
        {
            if (_player != null) Face(_player.position.x >= transform.position.x ? 1 : -1);
        }

        private void Face(int direction)
        {
            _facing = direction >= 0 ? 1 : -1;

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = _facing < 0;
        }

        private bool Move(float dx)
        {
            Vector3 position = transform.position;
            float x = Mathf.Clamp(position.x + dx, minX, maxX);
            bool moved = !Mathf.Approximately(x, position.x);
            transform.position = new Vector3(x, floorY, 0f);
            return moved;
        }

        // ------------------------------------------------------------------
        // Apanhar
        // ------------------------------------------------------------------

        public void Damage(int amount, Vector2 from)
        {
            if (_dead || _scene) return;

            if (!_awake)
            {
                // Sentado, de cabeca baixa, ele nem sente: o cinzel tine na prata.
                NoiseSystem.Emit(from, 6f, 0.3f);
                return;
            }

            _hp -= IsExposed ? amount * 2 : amount;
            _hurtUntil = Time.time + 0.14f;
            if (_sprite != null) _sprite.color = new Color(1f, 0.55f, 0.55f);

            if (_hp > 0) return;

            StopAllCoroutines();
            StopAttacks();
            _charging = false;
            _flinchable = false;
            _exposedUntil = 0f;

            if (!_phaseTwo) StartCoroutine(Transformation());
            else Die();
        }

        // ------------------------------------------------------------------
        // A transformacao
        // ------------------------------------------------------------------

        private IEnumerator Transformation()
        {
            _scene = true;

            var controller = FindAnyObjectByType<ScrapController>();
            bool began = !Cinematic.Active;
            if (began) Cinematic.Begin();
            if (controller != null) controller.ControlLocked = true;

            // Ele cai de joelhos; a coroa racha. A sala treme.
            ScrapCameraFollow.Direct((Vector2)transform.position + Vector2.up * 3f, 0.9f, 0.6f);
            Play(Fall, FallFrames, FallFrames / 1f, false);
            ScrapCameraFollow.Shake(0.3f, 1.4f);
            yield return new WaitForSeconds(1.5f);

            yield return ScriptedDialogue.Play(Name, _sprite, TransitionLines);

            // A cutscene desenhada (inteira so na primeira vez): no pico do clarao, ele ja e o outro.
            if (!_transformationSeen && cartoon != null && cartoon.IsReady)
            {
                _transformationSeen = true;
                yield return cartoon.Play(BecomeCorrupt);
            }
            else
            {
                Cinematic.Flash = 1f;
                BecomeCorrupt();
                for (float t = 0f; t < 0.8f; t += Time.deltaTime)
                {
                    Cinematic.Flash = 1f - t / 0.8f;
                    yield return null;
                }

                Cinematic.Flash = 0f;
            }

            // Ele se ergue, inteiro em Oxide, e a coroa acende verde.
            ScrapCameraFollow.Direct((Vector2)transform.position + Vector2.up * 3f, 1f, 0.5f);
            for (int i = FallFrames - 1; i >= 0; i--)
            {
                Play(Fall + i, 1, 1f, false);
                yield return new WaitForSeconds(0.22f);
            }

            Play(Cast, CastFrames, CastFrames / 0.8f, false);
            ScrapCameraFollow.Shake(0.4f, 1.2f);
            yield return new WaitForSeconds(0.9f);

            yield return ScriptedDialogue.Play(Name, _sprite, RisenLines);

            ScrapCameraFollow.Release();
            if (controller != null) controller.ControlLocked = false;
            if (began) Cinematic.End();

            _scene = false;
            _skywardReady = Time.time + 0.8f;
            yield return Fight();
        }

        private void BecomeCorrupt()
        {
            _hp = phaseTwoHealth;
            ApplyPhaseTwoLook();
            Play(Fall + FallFrames - 1, 1, 1f, false);
        }

        // ------------------------------------------------------------------
        // Cair e recomecar
        // ------------------------------------------------------------------

        private void Die()
        {
            _dead = true;
            _defeated = true;
            if (_collider != null) _collider.enabled = false;
            StartCoroutine(Fallen());
        }

        private IEnumerator Fallen()
        {
            _scene = true;
            var controller = FindAnyObjectByType<ScrapController>();
            bool began = !Cinematic.Active;
            if (began) Cinematic.Begin();
            if (controller != null) controller.ControlLocked = true;

            if (_sprite != null) _sprite.color = Color.white;
            ScrapCameraFollow.Direct((Vector2)transform.position + Vector2.up * 2.5f, 0.85f, 0.7f);
            ScrapCameraFollow.Shake(0.45f, 2f);

            // Cambaleia, larga o cetro, ajoelha, a coroa cai.
            float[] hold = { 0.3f, 0.45f, 0.6f, 0.7f, 0.9f, 0.7f };
            for (int i = 0; i < hold.Length; i++)
            {
                Play(Death + i, 1, 1f, false);
                yield return new WaitForSeconds(hold[i]);
            }

            yield return ScriptedDialogue.Play(Name, _sprite, LastLines);

            // E deita.
            Play(Death + 6, 1, 1f, false);
            yield return new WaitForSeconds(0.8f);
            Play(Death + 7, 1, 1f, false);

            if (Fighting == this) Fighting = null;
            if (fallenCrown != null) fallenCrown.SetActive(true);
            if (altarGate != null) altarGate.Open();

            ScrapCameraFollow.Release();
            if (controller != null) controller.ControlLocked = false;
            if (began) Cinematic.End();
            _scene = false;
        }

        private void StopAttacks()
        {
            if (beamA != null) beamA.Hide();
            if (beamB != null) beamB.Hide();
            if (breath != null) breath.Clear();
        }

        /// <summary>O Scrap morreu no meio da luta: tudo volta ao comeco — o Rei sentado no trono, na fase 1.</summary>
        private void ResetFight()
        {
            if (_dead || _scene) return;

            StopAllCoroutines();
            StopAttacks();
            _awake = false;
            _charging = false;
            _flinchable = false;
            _exposedUntil = 0f;
            _lastAttack = -1;
            if (_sprite != null) _sprite.color = Color.white;
            if (Fighting == this) Fighting = null;

            EnterPhaseOne();
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
            // Exposto ele nao machuca: e a hora de chegar perto.
            if (_dead || !_awake || _scene || IsExposed) return;

            var health = other.GetComponentInParent<ScrapHealth>();
            if (health != null) health.TakeDamage(_charging ? 2 : 1, transform.position);
        }

        // ------------------------------------------------------------------
        // Animacao
        // ------------------------------------------------------------------

        private static void SetActive(GameObject[] objects, bool active)
        {
            if (objects == null) return;
            foreach (GameObject target in objects) if (target != null) target.SetActive(active);
        }

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
            Sprite[] set = _phaseTwo ? corruptFrames : regalFrames;
            if (_sprite == null || set == null || set.Length == 0) return;

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

            if (frame >= 0 && frame < set.Length && set[frame] != null) _sprite.sprite = set[frame];
        }
    }
}
