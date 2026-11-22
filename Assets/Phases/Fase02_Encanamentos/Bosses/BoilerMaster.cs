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
    public class BoilerMaster : MonoBehaviour, IDamageable, IBossFight
    {
        [UnityEngine.Serialization.FormerlySerializedAs("maxHealth")] [SerializeField] private int vidaMax = 46;

        [UnityEngine.Serialization.FormerlySerializedAs("oxideCloud")] [Tooltip("A nuvem de Oxide da queda: um filho dele.")]
        [SerializeField] private ParticleSystem nuvemOxido;

        [UnityEngine.Serialization.FormerlySerializedAs("minX")] [Header("Arena")]
        [SerializeField] private float minX;
        [UnityEngine.Serialization.FormerlySerializedAs("maxX")] [SerializeField] private float maxX;
        [UnityEngine.Serialization.FormerlySerializedAs("floorY")] [SerializeField] private float chaoY;
        [UnityEngine.Serialization.FormerlySerializedAs("wakeArea")] [SerializeField] private Rect areaDespertar;

        [UnityEngine.Serialization.FormerlySerializedAs("boltSprite")] [Header("O que ele usa")]
        [SerializeField] private Sprite spriteProjetil;
        [UnityEngine.Serialization.FormerlySerializedAs("waveSprite")] [SerializeField] private Sprite spriteOnda;
        [UnityEngine.Serialization.FormerlySerializedAs("geyserSprite")] [SerializeField] private Sprite spriteGeiser;
        [UnityEngine.Serialization.FormerlySerializedAs("shardSprite")] [SerializeField] private Sprite spriteFragmento;
        [UnityEngine.Serialization.FormerlySerializedAs("rewardSprite")] [SerializeField] private Sprite spriteRecompensa;

        [UnityEngine.Serialization.FormerlySerializedAs("wreck")] [Tooltip("A carcaca que fica no chao depois da queda (ver BossWreck). Vazio: nao fica nada.")]
        [SerializeField] private GameObject destrocos;

        [UnityEngine.Serialization.FormerlySerializedAs("frames")] [Header("Os quadros")]
        [Tooltip("Normal, na ordem das constantes.")]
        [SerializeField] private Sprite[] quadros;

        [UnityEngine.Serialization.FormerlySerializedAs("hotFrames")] [Tooltip("Em sobrepressao (da metade da vida pra baixo), na mesma ordem.")]
        [SerializeField] private Sprite[] quadrosQuente;

        [UnityEngine.Serialization.FormerlySerializedAs("exitGate")] [Header("O que acontece quando ele cai")]
        [SerializeField] private Gate portaoSaida;
        [UnityEngine.Serialization.FormerlySerializedAs("purifier")] [SerializeField] private AreaPurifier purificador;

        // A ordem dos quadros. E a mesma do BoilerMasterArt, no editor.
        public const int QuadroAdormecido = 0;
        public const int QuadrosAdormecido = 4;
        public const int QuadroDespertar = 4;
        public const int QuadrosDespertar = 4;
        public const int QuadroOcioso = 8;
        public const int QuadrosOcioso = 6;
        public const int QuadroAndar = 14;
        public const int QuadrosAndar = 8;
        public const int QuadroPreparoMartelo = 22;
        public const int QuadrosPreparoMartelo = 2;
        public const int QuadroGolpeMartelo = 24;
        public const int QuadrosGolpeMartelo = 3;
        public const int QuadroPreparoCarga = 27;
        public const int QuadrosPreparoCarga = 2;
        public const int QuadroCorridaCarga = 29;
        public const int QuadrosCorridaCarga = 4;
        public const int QuadroPreparoRajada = 33;
        public const int QuadrosPreparoRajada = 2;
        public const int QuadroDisparoRajada = 35;
        public const int QuadrosDisparoRajada = 3;
        public const int QuadroLancamentoGeiser = 38;
        public const int QuadrosLancamentoGeiser = 3;
        public const int QuadroAgacharSalto = 41;
        public const int QuadrosAgacharSalto = 2;
        public const int SubidaSalto = 43;
        public const int ArSalto = 44;
        public const int QuedaSalto = 45;
        public const int QuadroImpactoSalto = 46;
        public const int QuadrosImpactoSalto = 2;
        public const int PoseExposto = 48;
        public const int QuadrosExposto = 2;
        public const int Ferido = 50;
        public const int QuadroRugido = 51;
        public const int QuadrosRugido = 4;
        public const int QuadroMorte = 55;
        public const int QuadrosMorte = 8;

        private SpriteRenderer _sprite;
        private Transform _jogador;
        private Vector3 _origem;
        private int _vida;
        private bool _acordado;
        private bool _doisFase;
        private bool _morto;
        private int _direcaoOlhar = -1;
        private float _expostoAte;
        private int _ultimoAtaque = -1;
        private bool _pendenteRugido;

        // A animacao tocando agora.
        private readonly FramePlayback _reproducao = new FramePlayback();
        private float _feridoAte = -1f;
        private int _quadroMorte = -1;
        private float _proximoPasso;

        // O corpo, medido no desenho: os bracos e o martelo ficam na frente
        // (ate 2,3 tiles), as costas quase rentes ao pivo.
        private BoxCollider2D _corpo;
        private const float CostasCorpo = 0.8f;
        private const float FrenteCorpo = 2.2f;
        private const float AlturaCorpo = 3.5f;

        [SaveProgress("_defeated")]
        private static bool _derrotado;

        /// <summary>O boss em luta, se houver. O HUD usa pra mostrar a barra de vida.</summary>
        public static BoilerMaster Lutando { get; private set; }

        /// <summary>Se ele ja caiu de vez (a trilha sonora usa pra calar a musica da luta).</summary>
        public bool EstaDerrubado => _morto;

        public MusicTrack MusicaBatalha => SoundtrackCatalog.MestreCaldeira;

        public bool CinematicasIgnoraMusica => false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegistrarLuta() => BossRoster.Adicionar(2, () => Lutando);

        /// <summary>Se ele ja caiu nesta sessao.</summary>
        public static bool Derrotado => _derrotado;

        public float FracaoVida => Mathf.Clamp01(_vida / (float)Mathf.Max(1, vidaMax));

        /// <summary>Verdadeiro enquanto a fornalha esta aberta e o cinzel fere em dobro.</summary>
        public bool EstaExposto => Time.time < _expostoAte;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Lutando = null;
            _derrotado = false;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoMinX, float novoMaxX, float novoChaoY, Rect novoAreaDespertar,
            Sprite projetil, Sprite onda, Sprite geiser, Sprite fragmento, Sprite recompensa,
            Gate portao, AreaPurifier novoPurificador)
        {
            minX = novoMinX;
            maxX = novoMaxX;
            chaoY = novoChaoY;
            areaDespertar = novoAreaDespertar;
            spriteProjetil = projetil;
            spriteOnda = onda;
            spriteGeiser = geiser;
            spriteFragmento = fragmento;
            spriteRecompensa = recompensa;
            portaoSaida = portao;
            purificador = novoPurificador;
        }

        /// <summary>Usado pelos geradores de fase: a nuvem de Oxide em que ele explode ao cair.</summary>
        public void ConfigurarNuvem(ParticleSystem nuvem)
        {
            nuvemOxido = nuvem;
        }

        /// <summary>Usado pelos geradores de fase: os quadros, normal e em sobrepressao.</summary>
        public void ConfigurarQuadros(Sprite[] novoQuadros, Sprite[] novoQuadrosQuente)
        {
            quadros = novoQuadros;
            quadrosQuente = novoQuadrosQuente;
        }

        /// <summary>A carcaca que ele deixa ao cair. Usado pelo gerador.</summary>
        public void ConfigurarDestrocos(GameObject novoDestrocos)
        {
            destrocos = novoDestrocos;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _vida = vidaMax;
            _origem = transform.position;

            _corpo = GetComponent<BoxCollider2D>();
            if (_corpo != null) _corpo.size = new Vector2(CostasCorpo + FrenteCorpo, AlturaCorpo);
            Virar(-1);
            Tocar(QuadroAdormecido, QuadrosAdormecido, 2.5f);

            ScrapHealth.Morreu += ReiniciarLuta;
        }

        private void Start()
        {
            // Ja foi derrotado nesta sessao: a arena fica vazia e aberta.
            if (_derrotado || AreaPurifier.EstaPurificado)
            {
                if (portaoSaida != null) portaoSaida.Abrir();
                BossWreck.Mostrar(destrocos);
                Destroy(gameObject);
                return;
            }

            var controlador = ScrapRef.Controlador;
            if (controlador != null) _jogador = controlador.transform;
        }

        private void OnDestroy()
        {
            ScrapHealth.Morreu -= ReiniciarLuta;
            if (Lutando == this) Lutando = null;
        }

        private void Update()
        {
            if (_sprite != null && !_morto)
            {
                // Fornalha exposta: um pouco em brasa (a porta aberta, no desenho, e a deixa).
                Color alvo = EstaExposto ? new Color(1f, 0.82f, 0.66f) : Color.white;
                _sprite.color = Color.Lerp(_sprite.color, alvo, 12f * Time.deltaTime);
            }

            if (_acordado || _morto || _jogador == null) return;
            if (!areaDespertar.Contains(_jogador.position)) return;

            _acordado = true;
            Lutando = this;
            StartCoroutine(Cerebro());
        }

        /// <summary>O Scrap morreu: a luta recomeca do zero.</summary>
        private void ReiniciarLuta()
        {
            if (_morto) return;

            StopAllCoroutines();

            _vida = vidaMax;
            _doisFase = false;
            _pendenteRugido = false;
            _acordado = false;
            _expostoAte = 0f;
            _ultimoAtaque = -1;
            _feridoAte = -1f;
            transform.position = _origem;

            if (_sprite != null) _sprite.color = Color.white;
            Virar(-1);
            Tocar(QuadroAdormecido, QuadrosAdormecido, 2.5f);

            if (Lutando == this) Lutando = null;
        }

        // ------------------------------------------------------------------
        // Cerebro
        // ------------------------------------------------------------------

        private IEnumerator Cerebro()
        {
            // Acorda: o visor acende, ele se ergue, as chamines tossem.
            VirarParaJogador();
            Tocar(QuadroDespertar, QuadrosDespertar, QuadrosDespertar / 0.9f, false);
            Sfx.DespertarMestreCaldeira.Tocar(transform.position);
            yield return new WaitForSeconds(1.0f);

            while (!_morto)
            {
                // Chegou a sobrepressao: para tudo e urra.
                if (_pendenteRugido)
                {
                    _pendenteRugido = false;
                    yield return Sobrepressao();
                }

                // Ainda exposto do golpe anterior: arfa com a fornalha aberta.
                while (EstaExposto && !_morto)
                {
                    Tocar(PoseExposto, QuadrosExposto, 6f);
                    yield return null;
                }

                float tempoAndar = Random.Range(0.5f, _doisFase ? 0.8f : 1.1f);
                float velocidadeAndar = _doisFase ? 3.2f : 2.2f;
                Tocar(QuadroAndar, QuadrosAndar, velocidadeAndar * 4f);

                for (float t = 0f; t < tempoAndar; t += Time.deltaTime)
                {
                    VirarParaJogador();
                    Mover(_direcaoOlhar * velocidadeAndar * Time.deltaTime);

                    // Dois passos por volta da animacao de andar.
                    Passo(QuadrosAndar / 2f / (velocidadeAndar * 4f));
                    yield return null;
                }

                int ataque = Escolher();
                _ultimoAtaque = ataque;

                switch (ataque)
                {
                    case 0: yield return Martelar(); break;
                    case 1: yield return Investir(); break;
                    case 2: yield return JatoDeVapor(); break;
                    case 3: yield return Geiseres(); break;
                    default: yield return Saltar(); break;
                }

                // Na fase 2 ele as vezes emenda um segundo golpe sem respirar.
                if (_doisFase && !_morto && Random.value < 0.35f)
                {
                    yield return Distancia() < 5f ? Martelar() : JatoDeVapor();
                }
            }
        }

        /// <summary>Escolhe o golpe pela distancia, sem repetir o anterior.</summary>
        private int Escolher()
        {
            float distancia = Distancia();

            for (int tentativas = 0; tentativas < 6; tentativas++)
            {
                int picareta;
                float rolar = Random.value;

                if (_doisFase && rolar < 0.22f) picareta = 4;
                else if (distancia < 4.5f) picareta = rolar < 0.6f ? 0 : 3;
                else if (distancia > 9f) picareta = rolar < 0.5f ? 1 : 2;
                else picareta = Random.Range(0, 4);

                if (picareta != _ultimoAtaque) return picareta;
            }

            return (_ultimoAtaque + 1) % 4;
        }

        private float Distancia() => _jogador == null ? 0f : Mathf.Abs(_jogador.position.x - transform.position.x);

        /// <summary>A sobrepressao: a nuvem sai das juntas, ele urra e o ponteiro vai pro fim.</summary>
        private IEnumerator Sobrepressao()
        {
            VirarParaJogador();
            Tocar(QuadroRugido, QuadrosRugido, QuadrosRugido / 1.1f, false);
            if (nuvemOxido != null) nuvemOxido.Play();
            Sfx.SobrepressaoMestreCaldeira.Tocar(transform.position);
            ScrapCameraFollow.Tremer(0.15f, 0.8f);
            yield return new WaitForSeconds(1.2f);
        }

        private IEnumerator Martelar()
        {
            VirarParaJogador();
            float preparo = _doisFase ? 0.45f : 0.65f;
            Tocar(QuadroPreparoMartelo, QuadrosPreparoMartelo, QuadrosPreparoMartelo / preparo, false);
            Sfx.PreparoMarteloMestreCaldeira.Tocar(transform.position, tom2: 0.65f / preparo);
            yield return Avisar(preparo);
            Tocar(QuadroGolpeMartelo, QuadrosGolpeMartelo, 14f, false);
            Sfx.PancadaChefe.Tocar(transform.position + Vector3.right * (_direcaoOlhar * 2.4f), tom2: 1.1f);

            // A martelada: do peito ate onde o martelo bate no chao (3,1 tiles
            // na frente), e alta, porque ele desce de cima.
            Vector2 centro = (Vector2)transform.position + new Vector2(_direcaoOlhar * 1.9f, 1.5f);

            foreach (Collider2D golpe in Physics2D.OverlapBoxAll(centro, new Vector2(2.4f, 3f), 0f))
            {
                var vida = golpe.GetComponentInParent<ScrapHealth>();
                if (vida != null) vida.LevarDano(1, transform.position);
            }

            ScrapCameraFollow.Tremer(0.08f, 0.2f);

            // A onda pelo chao, que so se evita pulando. Na fase 2, pros dois lados.
            ShockWave.Lancar(new Vector3(transform.position.x + _direcaoOlhar * 2.5f, chaoY, 0f),
                _direcaoOlhar * 8.5f, minX - 2f, maxX + 2f, spriteOnda);

            if (_doisFase)
            {
                ShockWave.Lancar(new Vector3(transform.position.x - _direcaoOlhar * 2.5f, chaoY, 0f),
                    -_direcaoOlhar * 8.5f, minX - 2f, maxX + 2f, spriteOnda);
            }

            _expostoAte = Time.time + (_doisFase ? 0.9f : 1.2f);
            yield return new WaitForSeconds(0.25f);
            Tocar(PoseExposto, QuadrosExposto, 6f);
            yield return new WaitForSeconds(_doisFase ? 0.55f : 0.85f);
        }

        private IEnumerator Investir()
        {
            VirarParaJogador();
            float preparo = _doisFase ? 0.4f : 0.55f;
            Tocar(QuadroPreparoCarga, QuadrosPreparoCarga, QuadrosPreparoCarga / preparo, false);
            Sfx.PreparoCargaMestreCaldeira.Tocar(transform.position);
            yield return Avisar(preparo);

            // Atravessa ate a parede (ou ate cansar), soltando vapor pra tras.
            Tocar(QuadroCorridaCarga, QuadrosCorridaCarga, 12f);
            Sfx.CorridaCargaMestreCaldeira.Tocar(transform.position, tom2: _doisFase ? 1.15f : 1f);
            int direcao = _direcaoOlhar;
            float velocidade = _doisFase ? 15f : 12f;

            for (float t = 0f; t < 1.4f; t += Time.deltaTime)
            {
                float antes = transform.position.x;
                Mover(direcao * velocidade * Time.deltaTime);
                if (Mathf.Approximately(antes, transform.position.x)) break;
                yield return null;
            }

            ScrapCameraFollow.Tremer(0.1f, 0.25f);
            Sfx.GolpeParedeChefe.Tocar(transform.position, tom2: 1.1f);

            // Bater na parede o deixa aberto um instante.
            _expostoAte = Time.time + (_doisFase ? 1.0f : 1.4f);
            Tocar(PoseExposto, QuadrosExposto, 6f);
            yield return new WaitForSeconds(_doisFase ? 0.7f : 1.0f);
        }

        private IEnumerator JatoDeVapor()
        {
            VirarParaJogador();
            float preparo = _doisFase ? 0.4f : 0.55f;
            Tocar(QuadroPreparoRajada, QuadrosPreparoRajada, QuadrosPreparoRajada / preparo, false);
            Sfx.PreparoRajadaMestreCaldeira.Tocar(transform.position, tom2: 0.55f / preparo);
            yield return Avisar(preparo);

            int saraivadas = _doisFase ? 2 : 1;

            for (int saraivada = 0; saraivada < saraivadas; saraivada++)
            {
                VirarParaJogador();

                // Tres alturas: rente ao chao, no meio e alto. Sempre sobra uma.
                float[] alturas = { 0.5f, 1.9f, 3.3f };
                int pular = Random.Range(0, alturas.Length);

                for (int i = 0; i < alturas.Length; i++)
                {
                    if (i == pular) continue;

                    Vector3 bocal = transform.position + new Vector3(_direcaoOlhar * 1.8f, alturas[i], 0f);
                    SteamBolt.Disparar(bocal, _direcaoOlhar * 10f, 40f, spriteProjetil);
                    Sfx.DisparoRajadaMestreCaldeira.Tocar(bocal, tom2: 1.1f - 0.1f * i);
                    Tocar(QuadroDisparoRajada, QuadrosDisparoRajada, 15f, false);
                    yield return new WaitForSeconds(0.2f);
                }

                yield return new WaitForSeconds(0.45f);
            }

            Tocar(QuadroOcioso, QuadrosOcioso, 6f);
            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator Geiseres()
        {
            float preparo = _doisFase ? 0.35f : 0.5f;
            Tocar(QuadroLancamentoGeiser, 2, 2f / preparo, false);
            Sfx.PreparoRajadaMestreCaldeira.Tocar(transform.position, tom2: 0.75f);
            yield return Avisar(preparo);
            Mostrar(QuadroLancamentoGeiser + 2);

            // Fase 1: um embaixo do Scrap e um sorteado. Fase 2: quatro, um
            // atras do outro, perseguindo o Scrap.
            int quantidade = _doisFase ? 4 : 1;

            for (int i = 0; i < quantidade; i++)
            {
                // O geiser sai embaixo do Scrap ate rente as grades: nao ha canto onde ele nao chegue.
                SteamGeyser.EntrarEmErupcao(new Vector3(Mathf.Clamp(_jogador.position.x, minX - 1.5f, maxX + 1.5f), chaoY, 0f), spriteGeiser);
                yield return new WaitForSeconds(0.38f);
            }

            if (!_doisFase)
            {
                SteamGeyser.EntrarEmErupcao(new Vector3(Random.Range(minX, maxX), chaoY, 0f), spriteGeiser);
            }

            Tocar(QuadroOcioso, QuadrosOcioso, 6f);
            yield return new WaitForSeconds(_doisFase ? 0.6f : 1.0f);
        }

        /// <summary>So na fase 2: sobe num arco, para em cima do Scrap e despenca.</summary>
        private IEnumerator Saltar()
        {
            VirarParaJogador();
            Tocar(QuadroAgacharSalto, QuadrosAgacharSalto, QuadrosAgacharSalto / 0.45f, false);
            Sfx.PreparoMarteloMestreCaldeira.Tocar(transform.position, tom2: 0.9f);
            yield return Avisar(0.45f);

            Sfx.PuloChefe.Tocar(transform.position, tom2: 1.05f);
            Vector3 de = transform.position;
            float alvoX = Mathf.Clamp(_jogador.position.x, minX, maxX);

            const float tempoPulo = 0.5f;
            for (float t = 0f; t < tempoPulo; t += Time.deltaTime)
            {
                float k = t / tempoPulo;
                Mostrar(k < 0.55f ? SubidaSalto : ArSalto);
                float altura = Mathf.Sin(k * Mathf.PI * 0.5f) * 6f;
                transform.position = new Vector3(Mathf.Lerp(de.x, alvoX, k), chaoY + altura, 0f);
                yield return null;
            }

            // Paira um instante: e a hora de sair de baixo.
            Mostrar(ArSalto);
            yield return new WaitForSeconds(0.22f);
            Mostrar(QuedaSalto);

            const float tempoQueda = 0.14f;
            float topo = transform.position.y;
            for (float t = 0f; t < tempoQueda; t += Time.deltaTime)
            {
                transform.position = new Vector3(alvoX, Mathf.Lerp(topo, chaoY, t / tempoQueda), 0f);
                yield return null;
            }

            transform.position = new Vector3(alvoX, chaoY, 0f);
            ScrapCameraFollow.Tremer(0.14f, 0.3f);
            Sfx.PancadaChefe.Tocar(transform.position);

            ShockWave.Lancar(transform.position + Vector3.right * 1.4f, 8f, minX - 1f, maxX + 1f, spriteOnda);
            ShockWave.Lancar(transform.position + Vector3.left * 1.4f, -8f, minX - 1f, maxX + 1f, spriteOnda);

            Tocar(QuadroImpactoSalto, QuadrosImpactoSalto, 6f, false);
            _expostoAte = Time.time + 0.9f;
            yield return new WaitForSeconds(0.35f);
            Tocar(PoseExposto, QuadrosExposto, 6f);
            yield return new WaitForSeconds(0.55f);
        }

        /// <summary>O aviso antes de cada golpe: ele pisca em amarelo.</summary>
        private IEnumerator Avisar(float tempo)
        {
            for (float t = 0f; t < tempo; t += Time.deltaTime)
            {
                if (_sprite != null)
                {
                    bool aceso = Mathf.FloorToInt(t * 12f) % 2 == 0;
                    _sprite.color = aceso ? new Color(1f, 0.92f, 0.45f) : Color.white;
                }

                yield return null;
            }
        }

        /// <summary>Um passo pesado, no maximo um a cada <paramref name="intervalo"/> segundos.</summary>
        private void Passo(float intervalo)
        {
            if (Time.time < _proximoPasso) return;

            _proximoPasso = Time.time + intervalo;
            Sfx.PassoChefe.Tocar(transform.position, tom2: 1.05f);
        }

        private void VirarParaJogador()
        {
            if (_jogador == null) return;

            Virar(_jogador.position.x >= transform.position.x ? 1 : -1);
        }

        private void Virar(int direcao)
        {
            _direcaoOlhar = direcao;

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = _direcaoOlhar < 0;

            // O corpo vai junto: mais comprido na frente que atras.
            if (_corpo != null) _corpo.offset = new Vector2((FrenteCorpo - CostasCorpo) * 0.5f * _direcaoOlhar, AlturaCorpo * 0.5f + 0.05f);
        }

        // ------------------------------------------------------------------
        // Animacao
        // ------------------------------------------------------------------

        /// <summary>Toca uma sequencia de quadros. Em loop, pedir a mesma de novo nao reinicia.</summary>
        private void Tocar(int inicio, int quantidade, float quadrosPorSegundo, bool repeticao = true) =>
            _reproducao.Tocar(inicio, quantidade, quadrosPorSegundo, repeticao);

        private void Mostrar(int quadro)
        {
            Tocar(quadro, 1, 1f, false);
        }

        private void LateUpdate()
        {
            if (_sprite == null) return;

            Sprite[] conjunto = _doisFase || _morto ? quadrosQuente : quadros;
            if (conjunto == null || conjunto.Length == 0) conjunto = quadros;
            if (conjunto == null || conjunto.Length == 0) return;

            int quadro;
            if (_quadroMorte >= 0)
            {
                quadro = _quadroMorte;
            }
            else
            {
                quadro = _reproducao.Quadro;

                bool calmo = _reproducao.Inicio == QuadroAndar || _reproducao.Inicio == QuadroOcioso || _reproducao.Inicio == PoseExposto;
                if (calmo && Time.time < _feridoAte) quadro = Ferido;
            }

            if (quadro >= 0 && quadro < conjunto.Length && conjunto[quadro] != null) FrameAnimator.Exibir(_sprite, conjunto[quadro]);
        }

        private void Mover(float deltaX)
        {
            Vector3 posicao = transform.position;
            posicao.x = Mathf.Clamp(posicao.x + deltaX, minX, maxX);
            posicao.y = chaoY;
            transform.position = posicao;
        }

        // ------------------------------------------------------------------
        // Apanhar e cair
        // ------------------------------------------------------------------

        public void ReceberDano(int quantia, Vector2 de)
        {
            if (_morto || !_acordado) return;

            // Fornalha exposta: o golpe entra em dobro.
            _vida -= EstaExposto ? quantia * 2 : quantia;
            if (_sprite != null) _sprite.color = new Color(1f, 0.35f, 0.3f);

            if (_vida <= 0)
            {
                Morrer();
                return;
            }

            _feridoAte = Time.time + 0.15f;
            Sfx.MetalFeridoChefe.Tocar(transform.position, tom2: 1.2f, volume: EstaExposto ? 1f : 0.8f);

            if (!_doisFase && _vida <= vidaMax / 2)
            {
                // Entra em brasa na hora; o urro e a nuvem vem quando o golpe
                // que ele estiver dando acabar.
                _doisFase = true;
                _pendenteRugido = true;
            }
        }

        private void Morrer()
        {
            _morto = true;
            _derrotado = true;
            StopAllCoroutines();

            if (Lutando == this) Lutando = null;

            StartCoroutine(Cair());
        }

        /// <summary>
        /// A cena da queda, como a do Coletor: a explosao em Oxide e, quando a
        /// nuvem assenta, os fragmentos, o Salto de Pressao, a grade da saida
        /// se abrindo e a purificacao do distrito.
        /// </summary>
        private IEnumerator Cair()
        {
            Cinematic.Iniciar();

            var controlador = _jogador != null ? _jogador.GetComponent<ScrapController>() : null;
            var corpo = _jogador != null ? _jogador.GetComponent<Rigidbody2D>() : null;

            void Hold()
            {
                if (controlador != null) controlador.ControleTravado = true;
            }

            Hold();
            if (corpo != null) corpo.linearVelocity = new Vector2(0f, Mathf.Min(0f, corpo.linearVelocity.y));

            Vector3 lugar = new Vector3(transform.position.x, chaoY, 0f);

            _quadroMorte = QuadroMorte;
            yield return BossFall.Explodir(transform, _sprite, nuvemOxido, lugar, Hold, 2.2f,
                k => _quadroMorte = QuadroMorte + Mathf.Min(QuadrosMorte - 1, (int)(k * QuadrosMorte)));

            // Quando a nuvem abre, sobra a carcaca no chao.
            BossWreck.Mostrar(destrocos, lugar);

            for (int i = 0; i < 16; i++)
            {
                ShardPickup.Criar(lugar + Vector3.up * 0.2f, spriteFragmento, 1, i);
            }

            PressureJumpPickup.Soltar(lugar, spriteRecompensa);

            yield return BossFall.Segurar(1.0f, Hold);

            if (portaoSaida != null) portaoSaida.Abrir();
            ScrapCameraFollow.Tremer(0.12f, 0.5f);

            // A purificacao: o vapor do distrito volta a ser so vapor.
            if (purificador != null)
            {
                purificador.Purificar(lugar);
                yield return BossFall.Segurar(purificador.Duracao + 0.2f, Hold);
            }

            if (controlador != null) controlador.ControleTravado = false;
            Cinematic.Encerrar();

            // A nuvem ainda esta no ar: so some com ele quando ela assentar.
            yield return new WaitForSeconds(3f);
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            Encostar(outro);
        }

        private void OnTriggerStay2D(Collider2D outro)
        {
            Encostar(outro);
        }

        private void Encostar(Collider2D outro)
        {
            if (_morto || !_acordado) return;

            var vida = outro.GetComponentInParent<ScrapHealth>();
            if (vida != null) vida.LevarDano(1, transform.position);
        }
    }
}
