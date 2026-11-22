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
    public class TheResonant : MonoBehaviour, IDamageable, IBossFight
    {
        // A ordem dos quadros. E a mesma do CaveMinerArt, no editor.
        public const int QuadroAdormecido = 0;
        public const int QuadrosAdormecido = 4;
        public const int QuadroDespertar = 4;
        public const int QuadrosDespertar = 4;
        public const int QuadroOcioso = 8;
        public const int QuadrosOcioso = 6;
        public const int QuadroAndar = 14;
        public const int QuadrosAndar = 8;
        public const int QuadroPreparoPancada = 22;
        public const int QuadrosPreparoPancada = 2;
        public const int QuadroPancada = 24;
        public const int QuadrosPancada = 3;
        public const int QuadroPreso = 27;
        public const int QuadrosPreso = 4;
        public const int QuadroPreparoCarga = 31;
        public const int QuadrosPreparoCarga = 2;
        public const int QuadroCarga = 33;
        public const int QuadrosCarga = 4;
        public const int QuadroAtordoado = 37;
        public const int QuadrosAtordoado = 4;
        public const int QuadroApito = 41;
        public const int QuadrosApito = 4;
        public const int QuadroRugido = 45;
        public const int QuadrosRugido = 4;
        public const int Ferido = 49;
        public const int QuadroMorte = 50;
        public const int QuadrosMorte = 8;

        [UnityEngine.Serialization.FormerlySerializedAs("maxHealth")] [SerializeField] private int vidaMax = 24;

        [UnityEngine.Serialization.FormerlySerializedAs("wakeArea")] [Header("Toca")]
        [Tooltip("A area que o acorda. So ouve o que acontece dentro dela.")]
        [SerializeField] private Rect areaDespertar;

        [UnityEngine.Serialization.FormerlySerializedAs("minX")] [Tooltip("Ate onde ele anda, em x de mundo.")]
        [SerializeField] private float minX;
        [UnityEngine.Serialization.FormerlySerializedAs("maxX")] [SerializeField] private float maxX;
        [UnityEngine.Serialization.FormerlySerializedAs("floorY")] [SerializeField] private float chaoY;
        [UnityEngine.Serialization.FormerlySerializedAs("ceilingY")] [SerializeField] private float tetoY;

        [UnityEngine.Serialization.FormerlySerializedAs("closeHearing")] [Header("Ouvido")]
        [Tooltip("Ate onde ele ouve passo e pulo (o resto, golpe, dash, queda, ele ouve na toca inteira).")]
        [SerializeField] private float audicaoFechar = 6f;

        [UnityEngine.Serialization.FormerlySerializedAs("smell")] [Tooltip("Perto assim, mesmo sem barulho, ele sente o cheiro.")]
        [SerializeField] private float olfato = 1.6f;

        [UnityEngine.Serialization.FormerlySerializedAs("slamReach")] [Header("Golpes")]
        [Tooltip("Onde a picareta crava, em tiles a frente dele (CaveMinerArt.SlamReach).")]
        [SerializeField] private float alcancePancada = 2.1f;
        [UnityEngine.Serialization.FormerlySerializedAs("slamDamage")] [SerializeField] private int danoPancada = 2;
        [UnityEngine.Serialization.FormerlySerializedAs("chargeDamage")] [SerializeField] private int danoCarga = 2;
        [UnityEngine.Serialization.FormerlySerializedAs("contactDamage")] [SerializeField] private int danoContato = 1;

        [UnityEngine.Serialization.FormerlySerializedAs("frames")] [Header("Quadros")]
        [Tooltip("A folha inteira, na ordem do CaveMinerArt.")]
        [SerializeField] private Sprite[] quadros;

        [UnityEngine.Serialization.FormerlySerializedAs("fallingShardSprite")] [Header("O que ele usa e deixa")]
        [SerializeField] private Sprite spriteFragmentoCaindo;
        [UnityEngine.Serialization.FormerlySerializedAs("shardSprite")] [SerializeField] private Sprite spriteFragmento;
        [UnityEngine.Serialization.FormerlySerializedAs("rewardSprite")] [SerializeField] private Sprite spriteRecompensa;

        [UnityEngine.Serialization.FormerlySerializedAs("keyId")] [Header("A chave")]
        [SerializeField] private string idChave;
        [UnityEngine.Serialization.FormerlySerializedAs("keyMessage")] [SerializeField] private string mensagemChave;
        [UnityEngine.Serialization.FormerlySerializedAs("keySprite")] [SerializeField] private Sprite spriteChave;

        [UnityEngine.Serialization.FormerlySerializedAs("brood")] [Header("O turno")]
        [Tooltip("Os bichos que brotam quando ele apita, desligados na cena.")]
        [SerializeField] private GameObject[] crias;
        [UnityEngine.Serialization.FormerlySerializedAs("broodSpots")] [SerializeField] private Transform[] locaisCrias;
        [UnityEngine.Serialization.FormerlySerializedAs("broodCracks")] [SerializeField] private SpriteRenderer[] rachadurasCrias;
        [UnityEngine.Serialization.FormerlySerializedAs("crackFrames")] [SerializeField] private Sprite[] quadrosRachadura;
        [UnityEngine.Serialization.FormerlySerializedAs("broodMaxAlive")] [SerializeField] private int vivoMaxCrias = 3;

        private SpriteRenderer _sprite;
        private Collider2D _colisor;

        // O corpo, medido no desenho: a barriga e a picareta na frente. Na
        // investida ele abaixa a cabeca e avanca o tronco: o corpo encurta e
        // vai mais pra frente.
        private BoxCollider2D _corpo;
        private Transform _jogador;
        private Vector3 _origem;
        private int _vida;
        private bool _acordado;
        private bool _morto;
        private bool _doisFase;
        private bool _pendenteRugido;
        private bool _ruidoProprioSom;
        private bool _carregando;
        private bool _recuavel;
        private int _direcaoOlhar = -1;
        private float _expostoAte;
        private float _feridoAte;
        private float _proximoApito;
        private float _proximoPasso;
        private bool _ouvido;
        private float _ouvidoX;

        // A animacao tocando agora.
        private readonly FramePlayback _reproducao = new FramePlayback(QuadrosAdormecido, 2.5f);

        [SaveProgress("_defeated")]
        private static bool _derrotado;

        /// <summary>O Capataz em luta, se houver. O HUD usa pra mostrar a barra de vida.</summary>
        public static TheResonant Lutando { get; private set; }

        /// <summary>Se ele ja caiu de vez (a trilha sonora usa pra calar a musica da luta).</summary>
        public bool EstaDerrubado => _morto;

        public MusicTrack MusicaBatalha => SoundtrackCatalog.RessonanteO;

        public bool CinematicasIgnoraMusica => false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegistrarLuta() => BossRoster.Adicionar(3, () => Lutando);

        public float FracaoVida => Mathf.Clamp01(_vida / (float)Mathf.Max(1, vidaMax));

        /// <summary>Verdadeiro enquanto ele esta preso (picareta cravada) ou tonto: o cinzel fere em dobro.</summary>
        public bool EstaExposto => Time.time < _expostoAte;

        private float Velocidade => _doisFase ? 2.6f : 1.8f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Lutando = null;
            _derrotado = false;
        }

        /// <summary>Usado pelos geradores de fase: a toca.</summary>
        public void Configurar(Rect novoAreaDespertar, float novoMinX, float novoMaxX, float novoChaoY, float novoTetoY,
            Sprite novoFragmentoCaindo, Sprite novoFragmento, Sprite novoRecompensa)
        {
            areaDespertar = novoAreaDespertar;
            minX = novoMinX;
            maxX = novoMaxX;
            chaoY = novoChaoY;
            tetoY = novoTetoY;
            spriteFragmentoCaindo = novoFragmentoCaindo;
            spriteFragmento = novoFragmento;
            spriteRecompensa = novoRecompensa;
        }

        /// <summary>Usado pelos geradores de fase: a folha de quadros e onde a picareta crava.</summary>
        public void ConfigurarQuadros(Sprite[] novoQuadros, float novoAlcancePancada)
        {
            quadros = novoQuadros;
            alcancePancada = novoAlcancePancada;
        }

        /// <summary>Usado pelos geradores de fase: a chave que ele solta ao morrer.</summary>
        public void ConfigurarChave(string novoIdChave, string novoMensagemChave, Sprite novoSpriteChave)
        {
            idChave = novoIdChave;
            mensagemChave = novoMensagemChave;
            spriteChave = novoSpriteChave;
        }

        /// <summary>Usado pelos geradores de fase: os bichos que brotam do chao quando ele apita.</summary>
        public void ConfigurarCrias(GameObject[] novoCrias, Transform[] novoLocais, SpriteRenderer[] novoRachaduras, Sprite[] novoQuadrosRachadura)
        {
            crias = novoCrias;
            locaisCrias = novoLocais;
            rachadurasCrias = novoRachaduras;
            quadrosRachadura = novoQuadrosRachadura;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _colisor = GetComponent<Collider2D>();
            _corpo = _colisor as BoxCollider2D;
            _vida = vidaMax;
            _origem = transform.position;
            Virar(-1);
            TocarAdormecido();

            ScrapHealth.Morreu += ReiniciarLuta;
        }

        private void OnEnable()
        {
            NoiseSystem.Ouvido += AoOuvirRuido;
        }

        private void OnDisable()
        {
            NoiseSystem.Ouvido -= AoOuvirRuido;
        }

        private void Start()
        {
            if (_derrotado)
            {
                // Caiu numa visita anterior e a gema (ou a chave) ficou pra
                // tras: ela volta.
                Vector3 lugar = transform.position + Vector3.up * 0.6f;
                SoltarGema(lugar);
                if (!string.IsNullOrEmpty(idChave) && !KeyItems.FoiEncontrado(idChave)) SoltarChave(lugar + Vector3.left * 1.2f);

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
            if (_sprite != null && !_morto && Time.time >= _feridoAte)
            {
                // Preso ou tonto: um pouco em brasa — e a deixa pra bater.
                Color alvo = EstaExposto ? new Color(1f, 0.82f, 0.68f) : Color.white;
                _sprite.color = Color.Lerp(_sprite.color, alvo, 10f * Time.deltaTime);
            }

            if (_acordado || _morto || _jogador == null) return;
            if (!areaDespertar.Contains(_jogador.position)) return;

            Acordar();
        }

        private void Acordar()
        {
            _acordado = true;
            Lutando = this;
            StartCoroutine(Cerebro());
        }

        // ------------------------------------------------------------------
        // Ouvir
        // ------------------------------------------------------------------

        private void AoOuvirRuido(Vector2 pontoOrigem, float raioArea, float volumeRuido2)
        {
            if (!_acordado || _morto || _ruidoProprioSom) return;
            if (!areaDespertar.Contains(pontoOrigem)) return;

            // Passo e pulo, so de perto; o resto ele ouve na toca inteira.
            float distancia = Mathf.Abs(pontoOrigem.x - transform.position.x);
            if (volumeRuido2 < 0.3f && distancia > audicaoFechar) return;

            Ouvir(pontoOrigem.x);
        }

        private void Ouvir(float x)
        {
            _ouvido = true;
            _ouvidoX = Mathf.Clamp(x, minX, maxX);
        }

        /// <summary>Colado nele, mesmo calado, o Scrap e farejado.</summary>
        private void Farejar()
        {
            if (_jogador == null || _ouvido) return;

            Vector2 deslocamento = _jogador.position - transform.position;
            if (Mathf.Abs(deslocamento.x) <= olfato && deslocamento.y > -1f && deslocamento.y < 3f) Ouvir(_jogador.position.x);
        }

        private void Emitir(Vector2 pontoOrigem, float raioArea, float volumeRuido2)
        {
            _ruidoProprioSom = true;
            NoiseSystem.Emitir(pontoOrigem, raioArea, volumeRuido2);
            _ruidoProprioSom = false;
        }

        // ------------------------------------------------------------------
        // Cerebro
        // ------------------------------------------------------------------

        private IEnumerator Cerebro()
        {
            // Acorda: ergue a cabeca, fareja e se levanta.
            if (_jogador != null) Virar(_jogador.position.x >= transform.position.x ? 1 : -1);
            Tocar(QuadroDespertar, QuadrosDespertar, QuadrosDespertar / 1.1f, false);
            Sfx.DespertarCapataz.Tocar(transform.position);
            yield return new WaitForSeconds(1.2f);

            // E a primeira coisa que faz e chamar o turno.
            yield return ChamarTurno();

            while (!_morto)
            {
                if (_pendenteRugido)
                {
                    _pendenteRugido = false;
                    yield return Gritar();
                }

                if (Time.time >= _proximoApito && CriasVivas() < vivoMaxCrias)
                {
                    yield return ChamarTurno();
                    continue;
                }

                Farejar();

                if (!_ouvido)
                {
                    yield return Escutar();
                    continue;
                }

                float distancia = Mathf.Abs(_ouvidoX - transform.position.x);
                Virar(_ouvidoX >= transform.position.x ? 1 : -1);

                if (distancia <= alcancePancada + 0.5f)
                {
                    _ouvido = false;
                    yield return EscolherPancada();
                }
                else if (distancia > 7f || (distancia > 4f && Random.value < (_doisFase ? 0.5f : 0.3f)))
                {
                    _ouvido = false;
                    yield return Atropelar();
                }
                else
                {
                    yield return Aproximar();
                }
            }
        }

        /// <summary>Nada ouvido: escuta parado, e anda devagar pra um lado e pro outro.</summary>
        private IEnumerator Escutar()
        {
            _recuavel = true;
            Tocar(QuadroOcioso, QuadrosOcioso, 5f);

            // Escutando, ele fareja: e o jeito de saber onde ele esta no escuro.
            Sfx.EscutarCapataz.Tocar(transform.position);

            for (float t = 0f; t < 0.9f && !_ouvido; t += Time.deltaTime)
            {
                Farejar();
                yield return null;
            }

            if (_ouvido) yield break;

            // Uns passos pra onde a toca e mais larga.
            float centroMeio = (minX + maxX) * 0.5f;
            int direcao = transform.position.x < centroMeio ? 1 : -1;
            if (Random.value < 0.35f) direcao = -direcao;
            Virar(direcao);

            Tocar(QuadroAndar, QuadrosAndar, 7f);
            for (float t = 0f; t < 1.1f && !_ouvido; t += Time.deltaTime)
            {
                Mover(direcao * Velocidade * 0.55f * Time.deltaTime);
                Passo(4f / 7f, 0.7f);
                Farejar();
                yield return null;
            }

            _recuavel = false;
        }

        /// <summary>Anda na direcao do que ouviu, ate ficar ao alcance da picareta.</summary>
        private IEnumerator Aproximar()
        {
            _recuavel = true;
            Tocar(QuadroAndar, QuadrosAndar, Velocidade * 4.5f);

            for (float t = 0f; t < 0.7f; t += Time.deltaTime)
            {
                float distancia = _ouvidoX - transform.position.x;
                if (Mathf.Abs(distancia) <= alcancePancada) break;

                Virar(distancia >= 0f ? 1 : -1);
                if (!Mover(_direcaoOlhar * Velocidade * Time.deltaTime)) break;
                Passo(4f / (Velocidade * 4.5f), 1f);
                yield return null;
            }

            _recuavel = false;
        }

        /// <summary>A picaretada: ergue, crava na frente, fica preso puxando o cabo.</summary>
        private IEnumerator EscolherPancada()
        {
            float preparo = _doisFase ? 0.45f : 0.6f;
            Tocar(QuadroPreparoPancada, QuadrosPreparoPancada, QuadrosPreparoPancada / preparo, false);
            Sfx.PreparoPicaretaCapataz.Tocar(transform.position, tom2: 0.6f / preparo);
            yield return Alertar(preparo);

            Tocar(QuadroPancada, QuadrosPancada, QuadrosPancada / 0.3f, false);
            yield return new WaitForSeconds(0.1f);

            // A pancada: pega do peito ate a ponta cravada (como no desenho), e
            // acorda o que estiver por perto.
            Vector2 ponta = new Vector2(transform.position.x + _direcaoOlhar * alcancePancada, chaoY + 0.6f);
            float perto = 0.7f;
            float longe = alcancePancada + 0.7f;
            Acertar(new Vector2(transform.position.x + _direcaoOlhar * (perto + longe) * 0.5f, chaoY + 1.1f), new Vector2(longe - perto, 2.2f), danoPancada);
            Emitir(ponta, 8f, 0.6f);
            Sfx.PancadaPicaretaCapataz.Tocar(ponta);
            yield return new WaitForSeconds(0.2f);

            // A ponta presa no chao: ele puxa o cabo e treme.
            _expostoAte = Time.time + (_doisFase ? 1.1f : 1.5f);
            Tocar(QuadroPreso, QuadrosPreso, 8f);
            Sfx.PresoPicaretaCapataz.Tocar(transform.position);
            while (EstaExposto && !_morto) yield return null;

            Tocar(QuadroOcioso, QuadrosOcioso, 5f);
            yield return new WaitForSeconds(_doisFase ? 0.2f : 0.35f);
        }

        /// <summary>A investida: abaixa a cabeca e corre ate a parede; bateu, fica tonto e o teto cai.</summary>
        private IEnumerator Atropelar()
        {
            float preparo = _doisFase ? 0.45f : 0.65f;
            Tocar(QuadroPreparoCarga, QuadrosPreparoCarga, QuadrosPreparoCarga / preparo, false);
            Sfx.PreparoInvestidaCapataz.Tocar(transform.position);
            yield return Alertar(preparo);

            float parede = _direcaoOlhar > 0 ? maxX : minX;
            float velocidade = _doisFase ? 11f : 9f;
            _carregando = true;
            AjustarCorpo();
            Tocar(QuadroCarga, QuadrosCarga, 14f);

            while (!Mathf.Approximately(transform.position.x, parede) && !_morto)
            {
                float x = Mathf.MoveTowards(transform.position.x, parede, velocidade * Time.deltaTime);
                transform.position = new Vector3(x, chaoY, 0f);
                if (Time.frameCount % 8 == 0) Emitir(new Vector2(x, chaoY + 0.5f), 5f, 0.2f);
                Passo(0.16f, 1.1f);
                yield return null;
            }

            _carregando = false;
            AjustarCorpo();

            // A pancada na parede: a toca inteira treme e o teto solta estilhacos.
            Tocar(QuadroAtordoado, 1, 1f, false);
            Sfx.ParedeInvestidaCapataz.Tocar(transform.position);
            Emitir(new Vector2(transform.position.x + _direcaoOlhar, chaoY + 2f), 20f, 0.6f);
            SoltarFragmentos(_doisFase ? 6 : 4, transform.position.x);

            _expostoAte = Time.time + (_doisFase ? 1.4f : 1.9f);
            yield return new WaitForSeconds(0.2f);
            Tocar(QuadroAtordoado + 1, QuadrosAtordoado - 1, 6f);
            while (EstaExposto && !_morto) yield return null;

            Virar(-_direcaoOlhar);
            Tocar(QuadroOcioso, QuadrosOcioso, 5f);
            yield return new WaitForSeconds(0.3f);
        }

        /// <summary>O apito: chama o turno, e as traças brotam das rachaduras.</summary>
        private IEnumerator ChamarTurno()
        {
            _proximoApito = Time.time + (_doisFase ? 9f : 13f);

            Tocar(QuadroApito, QuadrosApito, QuadrosApito / 0.7f, false);
            yield return new WaitForSeconds(0.3f);
            Sfx.ApitoCapataz.Tocar(transform.position + Vector3.up * 2.5f, tom2: _doisFase ? 1.06f : 1f);

            Emitir(transform.position + Vector3.up * 2.5f, 14f, 0.5f);

            int chamadas = _doisFase ? 3 : 2;
            int local = Random.Range(0, Mathf.Max(1, locaisCrias != null ? locaisCrias.Length : 1));
            for (int i = 0; i < chamadas && CriasVivas() < vivoMaxCrias + (_doisFase ? 1 : 0); i++)
            {
                GameObject livre = AcaroLivre();
                if (livre == null || locaisCrias == null || locaisCrias.Length == 0) break;

                StartCoroutine(Chocar(livre, (local + i) % locaisCrias.Length));
            }

            yield return new WaitForSeconds(0.6f);
            Tocar(QuadroOcioso, QuadrosOcioso, 5f);
            yield return new WaitForSeconds(0.2f);
        }

        /// <summary>Na metade da vida: o grito, e o teto despenca.</summary>
        private IEnumerator Gritar()
        {
            Tocar(QuadroRugido, QuadrosRugido, 8f);
            Sfx.GritoCapataz.Tocar(transform.position);
            Emitir(transform.position + Vector3.up * 2.5f, 24f, 1f);
            SoltarFragmentos(7, _jogador != null ? _jogador.position.x : transform.position.x);

            yield return new WaitForSeconds(1.4f);
            _proximoApito = Time.time;
            Tocar(QuadroOcioso, QuadrosOcioso, 5f);
        }

        /// <summary>O aviso antes do golpe: pisca em amarelo.</summary>
        private IEnumerator Alertar(float segundos)
        {
            // Os olhos de quartzo tilintam: ele vai golpear.
            Sfx.AlertaCapataz.Tocar(transform.position + Vector3.up * 2.6f);

            for (float t = 0f; t < segundos; t += Time.deltaTime)
            {
                if (_sprite != null && Time.time >= _feridoAte)
                {
                    _sprite.color = Mathf.FloorToInt(t * 14f) % 2 == 0 ? new Color(1f, 0.92f, 0.5f) : Color.white;
                }

                yield return null;
            }

            if (_sprite != null) _sprite.color = Color.white;
        }

        /// <summary>Estilhacos soltos do teto: metade perto de um ponto, o resto pela toca.</summary>
        private void SoltarFragmentos(int quantidade, float perto)
        {
            for (int i = 0; i < quantidade; i++)
            {
                float x = i % 2 == 0
                    ? Mathf.Clamp(perto + Random.Range(-2.5f, 2.5f), minX - 1f, maxX + 1f)
                    : Random.Range(minX - 1f, maxX + 1f);

                FallingShard.Soltar(new Vector2(x, tetoY), chaoY, spriteFragmentoCaindo, 0.5f + i * 0.12f);
            }
        }

        /// <summary>Uma botada, no maximo uma a cada <paramref name="intervalo"/> segundos.</summary>
        private void Passo(float intervalo, float volume)
        {
            if (Time.time < _proximoPasso) return;

            _proximoPasso = Time.time + intervalo;
            Sfx.PassoCapataz.Tocar(transform.position, volume: volume);
        }

        private void Acertar(Vector2 centro, Vector2 tamanho, int dano)
        {
            foreach (Collider2D golpe in Physics2D.OverlapBoxAll(centro, tamanho, 0f))
            {
                var vida = golpe.GetComponentInParent<ScrapHealth>();
                if (vida != null) vida.LevarDano(dano, transform.position);
            }
        }

        /// <summary>Anda, sem sair da toca. Devolve falso quando ja esta na beirada.</summary>
        private bool Mover(float dx)
        {
            Vector3 posicao = transform.position;
            float x = Mathf.Clamp(posicao.x + dx, minX, maxX);
            bool movido = !Mathf.Approximately(x, posicao.x);
            transform.position = new Vector3(x, chaoY, 0f);
            return movido;
        }

        private void Virar(int direcao)
        {
            _direcaoOlhar = direcao >= 0 ? 1 : -1;

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = _direcaoOlhar < 0;
            AjustarCorpo();
        }

        /// <summary>O corpo do lado em que ele olha: em pe, ou abaixado na investida.</summary>
        private void AjustarCorpo()
        {
            if (_corpo == null) return;

            float costas = _carregando ? 0.8f : 0.6f;
            float frente = _carregando ? 1.6f : 1.2f;
            float altura = _carregando ? 2.7f : 3.3f;

            _corpo.size = new Vector2(costas + frente, altura);
            _corpo.offset = new Vector2((frente - costas) * 0.5f * _direcaoOlhar, altura * 0.5f);
        }

        // ------------------------------------------------------------------
        // O turno
        // ------------------------------------------------------------------

        private readonly HashSet<GameObject> _eclodindo = new HashSet<GameObject>();

        private int CriasVivas()
        {
            if (crias == null) return 0;

            int vivo = 0;
            foreach (GameObject acaro in crias)
            {
                if (acaro != null && (acaro.activeSelf || _eclodindo.Contains(acaro))) vivo++;
            }

            return vivo;
        }

        private GameObject AcaroLivre()
        {
            if (crias == null) return null;

            foreach (GameObject acaro in crias)
            {
                if (acaro != null && !acaro.activeSelf && !_eclodindo.Contains(acaro)) return acaro;
            }

            return null;
        }

        private IEnumerator Chocar(GameObject acaro, int local)
        {
            _eclodindo.Add(acaro);

            SpriteRenderer rachadura = rachadurasCrias != null && local < rachadurasCrias.Length ? rachadurasCrias[local] : null;
            bool rachando = quadrosRachadura != null && quadrosRachadura.Length >= 3;
            if (locaisCrias != null && local < locaisCrias.Length) Sfx.EclosaoCapataz.Tocar(locaisCrias[local].position);

            for (float t = 0f; t < 0.7f; t += Time.deltaTime)
            {
                if (rachadura != null && rachando)
                {
                    rachadura.sprite = quadrosRachadura[1];
                    rachadura.transform.localPosition = new Vector3(Mathf.Sin(t * 70f) * 0.04f, 0f, 0f);
                }

                yield return null;
            }

            _eclodindo.Remove(acaro);
            if (rachadura != null)
            {
                rachadura.transform.localPosition = Vector3.zero;
                if (rachando) rachadura.sprite = quadrosRachadura[2];
            }

            if (_morto || !_acordado) yield break;

            // Religa, devolve a vida (quem ja morreu uma vez voltava imortal) e
            // so entao poe no lugar: o Restore manda ele de volta pra casa.
            acaro.SetActive(true);
            var corpo = acaro.GetComponent<Damageable>();
            if (corpo != null) corpo.Restaurar();
            acaro.transform.position = locaisCrias[local].position;

            // O chao estourando acorda o bicho que saiu dele.
            Emitir(locaisCrias[local].position, 4f, 0.5f);

            yield return new WaitForSeconds(1.2f);
            if (rachadura != null && rachando) rachadura.sprite = quadrosRachadura[0];
        }

        private void RecolocarCrias()
        {
            _eclodindo.Clear();
            if (crias != null)
            {
                foreach (GameObject acaro in crias)
                {
                    if (acaro != null) acaro.SetActive(false);
                }
            }

            if (rachadurasCrias != null && quadrosRachadura != null && quadrosRachadura.Length >= 3)
            {
                foreach (SpriteRenderer rachadura in rachadurasCrias)
                {
                    if (rachadura == null) continue;
                    rachadura.sprite = quadrosRachadura[0];
                    rachadura.transform.localPosition = Vector3.zero;
                }
            }
        }

        // ------------------------------------------------------------------
        // Apanhar e cair
        // ------------------------------------------------------------------

        public void ReceberDano(int quantia, Vector2 de)
        {
            if (_morto) return;

            // Dormindo, a pancada so o acorda.
            if (!_acordado)
            {
                Acordar();
                return;
            }

            _vida -= EstaExposto ? quantia * 2 : quantia;
            _feridoAte = Time.time + 0.14f;
            if (_sprite != null) _sprite.color = new Color(1f, 0.35f, 0.3f);
            if (_vida > 0) Sfx.FeridoCapataz.Tocar(transform.position);

            if (_vida <= 0)
            {
                Morrer();
                return;
            }

            if (!_doisFase && _vida <= vidaMax / 2)
            {
                _doisFase = true;
                _pendenteRugido = true;
            }
        }

        private void Morrer()
        {
            _morto = true;
            _derrotado = true;
            _carregando = false;
            _expostoAte = 0f;
            StopAllCoroutines();
            RecolocarCrias();
            if (Lutando == this) Lutando = null;
            if (_colisor != null) _colisor.enabled = false;

            StartCoroutine(Cair());
        }

        /// <summary>
        /// Cambaleia, larga a picareta, cai de joelhos; o capacete cai (a
        /// safira e a chave rolam), os cristais se desfazem e ele deita.
        /// </summary>
        private IEnumerator Cair()
        {
            if (_sprite != null) _sprite.color = Color.white;

            // Um homem desse tamanho caindo: a caverna inteira escuta.
            NoiseSystem.Emitir(transform.position + Vector3.up * 2f, 24f, 1f);

            // O som da queda inteira, casado com os quadros abaixo.
            Sfx.MorteCapataz.Tocar(transform.position);

            float[] segurar = { 0.2f, 0.3f, 0.4f, 0.45f, 0.6f, 0.45f, 0.6f, 0.5f };
            for (int i = 0; i < QuadrosMorte; i++)
            {
                Tocar(QuadroMorte + i, 1, 1f, false);

                if (i == 3)
                {
                    // O capacete no chao: a gema do soquete se solta, e a chave cai da argola.
                    Vector3 lugar = transform.position + new Vector3(_direcaoOlhar * 1.1f, 0.6f, 0f);
                    for (int s = 0; s < 12; s++) ShardPickup.Criar(lugar + Vector3.up * 0.2f, spriteFragmento, 1, s);
                    SoltarGema(lugar);
                    SoltarChave(lugar + new Vector3(-_direcaoOlhar * 1.6f, 0f, 0f));
                }

                yield return new WaitForSeconds(segurar[i]);
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

        /// <summary>O id da gema lapidada que ele guarda: pega uma vez, nao volta.</summary>
        public const string IdGema = "D3_Foreman_Gem";

        /// <summary>A gema lapidada que estava no soquete do capacete (moeda da loja do Crisol).</summary>
        private void SoltarGema(Vector3 lugar)
        {
            RewardPickup.Soltar(lugar, spriteRecompensa, RewardPickup.Kind.Gema, Color.white, IdGema);
        }

        /// <summary>A CHAVE ANTIGA: um item-chave flutuando onde ele caiu.</summary>
        private void SoltarChave(Vector3 lugar)
        {
            if (string.IsNullOrEmpty(idChave) || KeyItems.FoiEncontrado(idChave)) return;

            var chave = Prefabs.Criar("AncientKey");
            chave.transform.position = DropSpot.PontoLivre(lugar + Vector3.up * 0.8f);

            var renderizador = Prefabs.Componente<SpriteRenderer>(chave);
            renderizador.sprite = spriteChave;
            if (_sprite != null)
            {
                renderizador.sortingLayerID = _sprite.sortingLayerID;
                renderizador.sortingOrder = _sprite.sortingOrder + 1;
            }

            var gatilho = Prefabs.Componente<CircleCollider2D>(chave);
            gatilho.isTrigger = true;
            gatilho.radius = 0.7f;

            chave.AddComponent<KeyItemPickup>().Configurar(idChave, mensagemChave);
        }

        /// <summary>O Scrap morreu: a luta recomeca do zero, e o turno volta pra pedra.</summary>
        private void ReiniciarLuta()
        {
            if (_morto) return;

            StopAllCoroutines();

            _vida = vidaMax;
            _acordado = false;
            _doisFase = false;
            _pendenteRugido = false;
            _carregando = false;
            _recuavel = false;
            _ouvido = false;
            _expostoAte = 0f;
            _feridoAte = 0f;
            transform.position = _origem;

            if (_sprite != null) _sprite.color = Color.white;
            if (_colisor != null) _colisor.enabled = true;
            AjustarCorpo();
            Virar(-1);
            TocarAdormecido();

            if (Lutando == this) Lutando = null;

            // Os bichos sao religados pelo Damageable nesse mesmo aviso: desliga depois.
            StartCoroutine(RecolocarNoProximoQuadro());
        }

        private IEnumerator RecolocarNoProximoQuadro()
        {
            yield return null;
            RecolocarCrias();
        }

        // ------------------------------------------------------------------
        // Encostar
        // ------------------------------------------------------------------

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
            // Preso ou tonto ele nao machuca: e a hora de chegar perto.
            if (_morto || !_acordado || EstaExposto) return;

            var vida = outro.GetComponentInParent<ScrapHealth>();
            if (vida != null) vida.LevarDano(_carregando ? danoCarga : danoContato, transform.position);
        }

        // ------------------------------------------------------------------
        // Animacao
        // ------------------------------------------------------------------

        private void TocarAdormecido() => Tocar(QuadroAdormecido, QuadrosAdormecido, 2.5f);

        /// <summary>Toca uma sequencia da folha. Em loop, pedir a mesma de novo nao reinicia.</summary>
        private void Tocar(int inicio, int quantidade, float quadrosPorSegundo, bool repeticao = true) =>
            _reproducao.Tocar(inicio, quantidade, quadrosPorSegundo, repeticao);

        private void LateUpdate()
        {
            if (_sprite == null || quadros == null || quadros.Length == 0) return;

            int quadro;
            if (_recuavel && !_morto && Time.time < _feridoAte)
            {
                quadro = Ferido;
            }
            else
            {
                quadro = _reproducao.Quadro;
            }

            if (quadro >= 0 && quadro < quadros.Length && quadros[quadro] != null) FrameAnimator.Exibir(_sprite, quadros[quadro]);
        }
    }
}
