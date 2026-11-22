using System.Collections;
using UnityEngine;

namespace EchoesOfCravejada
{
    /// <summary>
    /// The Rusty Scrapper, o mini boss do District 1: o coletor do poco.
    ///
    /// E o primeiro boss do jogo, entao e lento e avisa tudo o que vai fazer —
    /// ele pisca em amarelo antes de cada golpe, e cada golpe tem a sua pose
    /// de preparo (ver os quadros do ColetorArt, no editor).
    ///
    ///   Dorme arriado ate o Scrap entrar na arena; acorda tossindo fumaca.
    ///   Garra:      avanca reto e golpeia.
    ///   Arremesso:  joga tres pedacos de sucata em arco.
    ///   Fase 2 (metade da vida): salta e bate no chao, soltando duas ondas de
    ///               choque que so se evita pulando.
    ///
    /// Ao chegar na metade ele racha (troca pros quadros rachados), para,
    /// urra apitando e solta uma nuvem densa de Oxide — que nao faz nada ao Scrap. Ao cair, roda a cena da queda (ver Fall): ele explode em
    /// Oxide, a area e purificada, o motor de vapor das costas dele aparece no
    /// fosso e a ferrugem sai do corpo do Scrap. E o Ferrolho quem monta o motor
    /// no Scrap: e o Steam Dash.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RustyScrapper : MonoBehaviour, IDamageable, IBossFight
    {
        [UnityEngine.Serialization.FormerlySerializedAs("maxHealth")] [SerializeField] private int vidaMax = 30;

        [UnityEngine.Serialization.FormerlySerializedAs("minX")] [Header("Arena")]
        [Tooltip("Ate onde ele anda, em x de mundo, e a altura do chao em que pisa.")]
        [SerializeField] private float minX;
        [UnityEngine.Serialization.FormerlySerializedAs("maxX")] [SerializeField] private float maxX;
        [UnityEngine.Serialization.FormerlySerializedAs("floorY")] [SerializeField] private float chaoY;

        [UnityEngine.Serialization.FormerlySerializedAs("wakeArea")] [Tooltip("A area que acorda o boss quando o Scrap entra.")]
        [SerializeField] private Rect areaDespertar;

        [UnityEngine.Serialization.FormerlySerializedAs("frames")] [Header("Os quadros")]
        [Tooltip("Inteiro, na ordem das constantes.")]
        [SerializeField] private Sprite[] quadros;

        [UnityEngine.Serialization.FormerlySerializedAs("crackedFrames")] [Tooltip("Rachado (da metade da vida pra baixo), na mesma ordem.")]
        [SerializeField] private Sprite[] quadrosRachado;

        [UnityEngine.Serialization.FormerlySerializedAs("chunkSprite")] [Header("O que ele usa")]
        [SerializeField] private Sprite spritePedaco;
        [UnityEngine.Serialization.FormerlySerializedAs("waveSprite")] [SerializeField] private Sprite spriteOnda;
        [UnityEngine.Serialization.FormerlySerializedAs("shardSprite")] [SerializeField] private Sprite spriteFragmento;

        [UnityEngine.Serialization.FormerlySerializedAs("wreck")] [Tooltip("A carcaca que fica no chao depois da queda (ver BossWreck). Vazio: nao fica nada.")]
        [SerializeField] private GameObject destrocos;
        [UnityEngine.Serialization.FormerlySerializedAs("oxideCloud")] [SerializeField] private ParticleSystem nuvemOxido;

        [UnityEngine.Serialization.FormerlySerializedAs("exitGate")] [Header("O que acontece quando ele cai")]
        [SerializeField] private Gate portaoSaida;
        [UnityEngine.Serialization.FormerlySerializedAs("purifier")] [SerializeField] private AreaPurifier purificador;

        [UnityEngine.Serialization.FormerlySerializedAs("cartoon")] [Tooltip("A cutscene desenhada da troca de visual. Vazio: so o brilho no sprite.")]
        [SerializeField] private PurificationCartoon desenho2;

        // A ordem dos quadros. E a mesma do ColetorArt, no editor.
        public const int QuadroAdormecido = 0;
        public const int QuadrosAdormecido = 4;
        public const int QuadroDespertar = 4;
        public const int QuadrosDespertar = 4;
        public const int QuadroOcioso = 8;
        public const int QuadrosOcioso = 6;
        public const int QuadroAndar = 14;
        public const int QuadrosAndar = 8;
        public const int QuadroPreparoGarra = 22;
        public const int QuadrosPreparoGarra = 2;
        public const int QuadroGolpeGarra = 24;
        public const int QuadrosGolpeGarra = 3;
        public const int QuadroAlcanceArremesso = 27;
        public const int QuadrosAlcanceArremesso = 2;
        public const int QuadroSolturaArremesso = 29;
        public const int QuadrosSolturaArremesso = 2;
        public const int QuadroAgacharPancada = 31;
        public const int QuadrosAgacharPancada = 2;
        public const int SubidaPancada = 33;
        public const int ArPancada = 34;
        public const int QuedaPancada = 35;
        public const int QuadroImpactoPancada = 36;
        public const int QuadrosImpactoPancada = 2;
        public const int Ferido = 38;
        public const int QuadroRugido = 39;
        public const int QuadrosRugido = 4;
        public const int QuadroMorte = 43;
        public const int QuadrosMorte = 8;

        private SpriteRenderer _sprite;
        private Transform _jogador;
        private int _vida;
        private bool _acordado;
        private bool _doisFase;
        private bool _morto;
        private bool _pendenteRugido;
        private int _direcaoOlhar = -1;
        private Vector3 _origem;

        // A animacao tocando agora: de qual quadro, quantos, em que ritmo.
        private readonly FramePlayback _reproducao = new FramePlayback();
        private float _feridoAte = -1f;
        private int _quadroMorte = -1;
        private float _proximoPasso;

        // O corpo (encostar machuca, e e nele que o cinzel bate), medido no
        // desenho: de 1,2 atras a 1,5 na frente, e quase 3 de altura.
        private BoxCollider2D _corpo;
        private const float CostasCorpo = 1.2f;
        private const float FrenteCorpo = 1.5f;
        private const float AlturaCorpo = 2.8f;

        /// <summary>O boss em luta, se houver. O HUD usa pra mostrar a barra de vida.</summary>
        public static RustyScrapper Lutando { get; private set; }

        /// <summary>Se ele ja caiu de vez (a trilha sonora usa pra calar a musica da luta).</summary>
        public bool EstaDerrubado => _morto;

        public MusicTrack MusicaBatalha => SoundtrackCatalog.SucateiroEnferrujado;

        public bool CinematicasIgnoraMusica => false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegistrarLuta() => BossRoster.Adicionar(1, () => Lutando);

        public float FracaoVida => Mathf.Clamp01(_vida / (float)Mathf.Max(1, vidaMax));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Lutando = null;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoMinX, float novoMaxX, float novoChaoY, Rect novoAreaDespertar,
            Sprite pedaco, Sprite onda, Sprite fragmento, ParticleSystem nuvem,
            Gate portao, AreaPurifier novoPurificador, PurificationCartoon novoDesenho = null,
            Sprite[] novoQuadros = null, Sprite[] novoQuadrosRachado = null)
        {
            quadros = novoQuadros;
            quadrosRachado = novoQuadrosRachado;
            minX = novoMinX;
            maxX = novoMaxX;
            chaoY = novoChaoY;
            areaDespertar = novoAreaDespertar;
            spritePedaco = pedaco;
            spriteOnda = onda;
            spriteFragmento = fragmento;
            nuvemOxido = nuvem;
            portaoSaida = portao;
            purificador = novoPurificador;
            desenho2 = novoDesenho;
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

        /// <summary>
        /// O Scrap morreu: a luta recomeca do zero. Ele volta pro lugar, enche
        /// a vida, esquece a fase 2 e dorme ate o Scrap entrar na arena de novo.
        /// </summary>
        private void ReiniciarLuta()
        {
            if (_morto) return;

            StopAllCoroutines();

            _vida = vidaMax;
            _doisFase = false;
            _pendenteRugido = false;
            _acordado = false;
            _feridoAte = -1f;
            transform.position = _origem;

            if (_sprite != null) _sprite.color = Color.white;
            Virar(-1);
            Tocar(QuadroAdormecido, QuadrosAdormecido, 2.5f);

            if (Lutando == this) Lutando = null;
        }

        private void Start()
        {
            // Ja foi derrotado nesta sessao: so a carcaca dele na arena.
            if (AreaPurifier.EstaPurificado)
            {
                BossWreck.Mostrar(destrocos);
                Destroy(gameObject);
                return;
            }

            var controlador = ScrapRef.Controlador;
            if (controlador != null) _jogador = controlador.transform;
        }

        private void Update()
        {
            if (_acordado || _morto || _jogador == null) return;
            if (!areaDespertar.Contains(_jogador.position)) return;

            _acordado = true;
            Lutando = this;
            StartCoroutine(Cerebro());
        }

        private void OnDestroy()
        {
            ScrapHealth.Morreu -= ReiniciarLuta;
            if (Lutando == this) Lutando = null;
        }

        // ------------------------------------------------------------------
        // Cerebro
        // ------------------------------------------------------------------

        private IEnumerator Cerebro()
        {
            // Acorda: o olho tremula, ele se levanta, a chamine tosse.
            VirarParaJogador();
            Tocar(QuadroDespertar, QuadrosDespertar, QuadrosDespertar / 0.9f, false);
            Sfx.DespertarColetor.Tocar(transform.position);
            yield return new WaitForSeconds(1.0f);

            while (!_morto)
            {
                // Chegou na metade da vida: para tudo e urra.
                if (_pendenteRugido)
                {
                    _pendenteRugido = false;
                    yield return RugirESoltarVapor();
                }

                // Anda um pouco na direcao do Scrap antes de decidir.
                float tempoAndar = Random.Range(0.7f, 1.3f);
                float velocidadeAndar = _doisFase ? 2.4f : 1.6f;
                Tocar(QuadroAndar, QuadrosAndar, velocidadeAndar * 5f);

                for (float t = 0f; t < tempoAndar; t += Time.deltaTime)
                {
                    VirarParaJogador();
                    Mover(_direcaoOlhar * velocidadeAndar * Time.deltaTime);

                    // Dois passos por volta da animacao de andar (8 quadros).
                    Passo(4f / (velocidadeAndar * 5f));
                    yield return null;
                }

                float distancia = Mathf.Abs(_jogador.position.x - transform.position.x);
                float rolar = Random.value;

                if (_doisFase && rolar < 0.35f) yield return Pancada();
                else if (distancia > 6f || rolar < 0.45f) yield return Arremessar();
                else yield return Garra();
            }
        }

        /// <summary>O urro da metade: ergue a cabeca, apita, e a nuvem de Oxide sai.</summary>
        private IEnumerator RugirESoltarVapor()
        {
            VirarParaJogador();
            Tocar(QuadroRugido, QuadrosRugido, QuadrosRugido / 1.1f, false);

            // A nuvem de Oxide. Nao tem gatilho de dano nenhum de proposito:
            // e aqui que o jogador ve que o Scrap e imune.
            if (nuvemOxido != null) nuvemOxido.Play();
            Sfx.RugidoColetor.Tocar(transform.position);
            ScrapCameraFollow.Tremer(0.12f, 0.8f);
            NoiseSystem.Emitir(transform.position, 10f, 0.6f);

            yield return new WaitForSeconds(1.2f);
        }

        private IEnumerator Garra()
        {
            VirarParaJogador();
            float preparo = _doisFase ? 0.4f : 0.6f;
            Tocar(QuadroPreparoGarra, QuadrosPreparoGarra, QuadrosPreparoGarra / preparo, false);
            Sfx.PreparoGarraColetor.Tocar(transform.position, tom2: 0.6f / preparo);
            yield return Avisar(preparo);

            Tocar(QuadroGolpeGarra, QuadrosGolpeGarra, QuadrosGolpeGarra / 0.35f, false);
            Sfx.GolpeGarraColetor.Tocar(transform.position);

            int direcao = _direcaoOlhar;
            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                Mover(direcao * 9f * Time.deltaTime);

                // A garra: na frente do corpo, onde ela aparece no desenho (ate 2,2 tiles).
                AreaDeAcerto(new Vector2(1.45f, 1.4f), new Vector2(1.5f, 2.4f));
                yield return null;
            }

            yield return new WaitForSeconds(0.25f);
            Tocar(QuadroOcioso, QuadrosOcioso, 6f);
            yield return new WaitForSeconds(0.45f);
        }

        private IEnumerator Arremessar()
        {
            VirarParaJogador();
            Tocar(QuadroAlcanceArremesso, QuadrosAlcanceArremesso, QuadrosAlcanceArremesso / 0.5f, false);
            Sfx.PreparoArremessoColetor.Tocar(transform.position);
            yield return Avisar(0.5f);

            Tocar(QuadroSolturaArremesso, QuadrosSolturaArremesso, 7f, false);
            Sfx.ArremessoColetor.Tocar(transform.position);

            const float gravidade = 18f;

            // Sai da mao de tras, erguida por cima da cacamba.
            Vector3 mao = transform.position + new Vector3(_direcaoOlhar * 0.6f, 2.6f, 0f);

            // Um no Scrap (no corpo dele, ate em cima de uma plataforma), um
            // antes e um depois, no chao: ficar parado nao salva, subir tambem nao.
            for (int i = -1; i <= 1; i++)
            {
                var alvo = new Vector2(_jogador.position.x + i * 2.5f, i == 0 ? _jogador.position.y + 0.5f : chaoY);
                ScrapChunk.Arremessar(mao, ArcoDeLancamento(mao, alvo, gravidade), gravidade, chaoY, spritePedaco);
            }

            yield return new WaitForSeconds(0.35f);
            Tocar(QuadroOcioso, QuadrosOcioso, 6f);
            yield return new WaitForSeconds(0.55f);
        }

        private IEnumerator Pancada()
        {
            VirarParaJogador();
            Tocar(QuadroAgacharPancada, QuadrosAgacharPancada, QuadrosAgacharPancada / 0.6f, false);
            Sfx.PreparoMarteloMestreCaldeira.Tocar(transform.position, tom2: 0.8f);
            yield return Avisar(0.6f);

            Sfx.PuloChefe.Tocar(transform.position);
            Vector3 de = transform.position;
            float alvoX = Mathf.Clamp(_jogador.position.x, minX, maxX);

            // Sobe num arco ate em cima do Scrap...
            const float tempoPulo = 0.55f;
            for (float t = 0f; t < tempoPulo; t += Time.deltaTime)
            {
                float k = t / tempoPulo;
                Mostrar(k < 0.6f ? SubidaPancada : ArPancada);
                float altura = Mathf.Sin(k * Mathf.PI * 0.5f) * 5f;
                transform.position = new Vector3(Mathf.Lerp(de.x, alvoX, k), chaoY + altura, 0f);
                yield return null;
            }

            // ...paira um instante, que e a hora de sair de baixo...
            Mostrar(ArPancada);
            yield return new WaitForSeconds(0.25f);
            Mostrar(QuedaPancada);

            // ...e despenca.
            const float tempoQueda = 0.16f;
            float topo = transform.position.y;
            for (float t = 0f; t < tempoQueda; t += Time.deltaTime)
            {
                transform.position = new Vector3(alvoX, Mathf.Lerp(topo, chaoY, t / tempoQueda), 0f);
                yield return null;
            }

            transform.position = new Vector3(alvoX, chaoY, 0f);

            Vector3 solo = transform.position;
            Sfx.PancadaChefe.Tocar(solo);
            ShockWave.Lancar(solo + Vector3.right * 1.2f, 7f, minX - 1f, maxX + 1f, spriteOnda);
            ShockWave.Lancar(solo + Vector3.left * 1.2f, -7f, minX - 1f, maxX + 1f, spriteOnda);

            Tocar(QuadroImpactoPancada, QuadrosImpactoPancada, 5f, false);
            yield return new WaitForSeconds(0.5f);
            Tocar(QuadroOcioso, QuadrosOcioso, 6f);
            yield return new WaitForSeconds(0.5f);
        }

        /// <summary>O aviso antes de cada golpe: ele pisca em amarelo.</summary>
        private IEnumerator Avisar(float tempo)
        {
            for (float t = 0f; t < tempo; t += Time.deltaTime)
            {
                if (_sprite != null)
                {
                    bool aceso = Mathf.FloorToInt(t * 12f) % 2 == 0;
                    _sprite.color = aceso ? new Color(1f, 0.9f, 0.4f) : Color.white;
                }

                yield return null;
            }

            if (_sprite != null) _sprite.color = Color.white;
        }

        /// <summary>
        /// A velocidade de um arco que sai de <paramref name="de"/> e passa por
        /// <paramref name="alvo"/> descendo. Sobe no minimo 9,5 (o arco de
        /// sempre) e mais, se o alvo estiver alto.
        /// </summary>
        private static Vector2 ArcoDeLancamento(Vector2 de, Vector2 alvo, float gravidade)
        {
            float subida = alvo.y - de.y;
            float lancamento = Mathf.Max(9.5f, Mathf.Sqrt(2f * gravidade * Mathf.Max(0f, subida + 1.5f)));
            float voo = (lancamento + Mathf.Sqrt(Mathf.Max(0f, lancamento * lancamento - 2f * gravidade * subida))) / gravidade;
            return new Vector2((alvo.x - de.x) / Mathf.Max(0.3f, voo), lancamento);
        }

        /// <summary>Machuca o Scrap numa caixa medida a partir dos pes dele, olhando pra frente.</summary>
        private void AreaDeAcerto(Vector2 local2, Vector2 tamanho)
        {
            Vector2 centro = (Vector2)transform.position + new Vector2(local2.x * _direcaoOlhar, local2.y);
            foreach (Collider2D golpe in Physics2D.OverlapBoxAll(centro, tamanho, 0f))
            {
                var vida = golpe.GetComponentInParent<ScrapHealth>();
                if (vida != null) vida.LevarDano(1, transform.position);
            }
        }

        /// <summary>Um passo pesado, no maximo um a cada <paramref name="intervalo"/> segundos.</summary>
        private void Passo(float intervalo)
        {
            if (Time.time < _proximoPasso) return;

            _proximoPasso = Time.time + intervalo;
            Sfx.PassoChefe.Tocar(transform.position, tom2: 0.9f);
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

        /// <summary>Fica parado num quadro so.</summary>
        private void Mostrar(int quadro)
        {
            Tocar(quadro, 1, 1f, false);
        }

        // No LateUpdate, depois de o cerebro decidir o que ele esta fazendo.
        private void LateUpdate()
        {
            if (_sprite == null) return;

            // Da metade pra baixo, os quadros rachados.
            Sprite[] conjunto = _doisFase || _morto ? quadrosRachado : quadros;
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

                // Apanhar so troca o quadro quando ele esta andando ou parado:
                // no meio de um golpe, o golpe continua legivel.
                bool calmo = _reproducao.Inicio == QuadroAndar || _reproducao.Inicio == QuadroOcioso;
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

            _vida -= quantia;

            if (_vida <= 0)
            {
                Morrer();
                return;
            }

            if (!_doisFase && _vida <= vidaMax / 2)
            {
                // Racha na hora; o urro e a nuvem vem assim que o golpe que
                // ele estiver dando acabar.
                _doisFase = true;
                _pendenteRugido = true;
            }

            _feridoAte = Time.time + 0.15f;
            Sfx.MetalFeridoChefe.Tocar(transform.position, tom2: _doisFase ? 0.9f : 1f);
            StartCoroutine(Piscada());
        }

        private IEnumerator Piscada()
        {
            if (_sprite == null) yield break;

            _sprite.color = new Color(1f, 0.4f, 0.35f);
            yield return new WaitForSeconds(0.1f);
            _sprite.color = Color.white;
        }

        private void Morrer()
        {
            _morto = true;
            StopAllCoroutines();

            // A barra de vida some e a grade da entrada abre ja.
            if (Lutando == this) Lutando = null;

            StartCoroutine(Cair());
        }

        /// <summary>
        /// A cena da queda. Ele trava, treme e pisca em verde, solta jatos de
        /// Oxide cada vez mais fortes e estoura numa nuvem. Quando a nuvem
        /// assenta: a purificacao, o motor dele no fosso (aparece com ela) e,
        /// por fim, a crosta saindo do corpo do Scrap.
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

            // O Scrap para onde esta (mas cai, se estava no ar).
            Hold();
            if (corpo != null) corpo.linearVelocity = new Vector2(0f, Mathf.Min(0f, corpo.linearVelocity.y));

            Vector3 origem2 = new Vector3(transform.position.x, chaoY, 0f);

            // 1 e 2. Travado, tremendo e piscando, os jatos de Oxide e o estouro.
            //    Enquanto treme, ele cambaleia, ajoelha e o verde toma o corpo.
            _quadroMorte = QuadroMorte;
            yield return BossFall.Explodir(transform, _sprite, nuvemOxido, origem2, Hold, 2.2f,
                k => _quadroMorte = QuadroMorte + Mathf.Min(QuadrosMorte - 1, (int)(k * QuadrosMorte)));

            // Quando a nuvem abre, sobra a carcaca no chao.
            BossWreck.Mostrar(destrocos, origem2);

            // Os fragmentos se espalham de onde ele estava.
            for (int i = 0; i < 12; i++)
            {
                ShardPickup.Criar(origem2 + Vector3.up * 0.2f, spriteFragmento, 1, i);
            }

            if (portaoSaida != null) portaoSaida.Abrir();

            // Deixa a nuvem assentar um pouco antes de a luz mudar.
            for (float t = 0f; t < 1.2f; t += Time.deltaTime)
            {
                Hold();
                yield return null;
            }

            // 3. A purificacao. No fim dela aparece o que estava esperando por
            //    ela: o motor dele no fosso, o vapor da volta, a Pequena...
            if (purificador != null)
            {
                purificador.Purificar(origem2);

                for (float t = 0f; t < purificador.Duracao + 0.2f; t += Time.deltaTime)
                {
                    Hold();
                    yield return null;
                }
            }

            // 4. E a crosta sai do corpo do Scrap: ele volta a brilhar. Com a
            //    cutscene desenhada, a troca acontece no clarao do fim dela;
            //    sem ela (cena antiga), so o brilho no sprite.
            if (desenho2 != null && desenho2.EstaProntoUso)
            {
                yield return TocarDesenho(controlador);
            }
            else
            {
                yield return Polir(controlador);
            }

            if (controlador != null) controlador.ControleTravado = false;
            Cinematic.Encerrar();

            Destroy(gameObject);
        }

        /// <summary>
        /// A cutscene desenhada: o jogo some, os quadros passam e, no clarao
        /// final, o Scrap vira o polido. O controle segue travado o tempo todo.
        /// </summary>
        private IEnumerator TocarDesenho(ScrapController controlador)
        {
            var aparencia = _jogador != null ? _jogador.GetComponent<ScrapAppearance>() : null;

            IEnumerator tocar = desenho2.Tocar(() =>
            {
                if (aparencia != null) aparencia.Polir();
            });

            while (true)
            {
                if (controlador != null) controlador.ControleTravado = true;
                if (!tocar.MoveNext()) break;
                yield return tocar.Current;
            }

        }

        /// <summary>
        /// A troca de aparencia: um facho de luz desce sobre o Scrap, a
        /// ferrugem vai descascando do corpo dele em lascas que caem; no auge
        /// do brilho ele troca pro polido, num estalo de faiscas, e volta ao
        /// tamanho ja limpo.
        /// </summary>
        private IEnumerator Polir(ScrapController controlador)
        {
            var aparencia = _jogador != null ? _jogador.GetComponent<ScrapAppearance>() : null;
            var visual = _jogador != null ? _jogador.GetComponent<SpriteRenderer>() : null;
            if (aparencia == null) yield break;

            Vector3 tamanho = _jogador.localScale;
            var ofuscar = new Color(1f, 0.95f, 0.75f);
            Sfx.LustroPolimento.Tocar();

            const float lustro = 2.0f;
            bool mudou = false;
            var luzFerrugem = new Color(0.78f, 0.46f, 0.24f);
            var escuroFerrugem = new Color(0.40f, 0.21f, 0.13f);
            Vector2 peito = (Vector2)_jogador.position + Vector2.up * 0.6f;

            // A camera chega perto dele.
            ScrapCameraFollow.Direcionar(peito + Vector2.up * 0.4f, 0.55f, 0.4f);

            // O facho: uma luz forte em cima, descendo sobre ele.
            var objetoRaio = new GameObject("PolishBeam");
            objetoRaio.transform.position = new Vector3(peito.x, peito.y + 3f, 0f);
            var raio = objetoRaio.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
            raio.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
            raio.color = ofuscar;
            raio.intensity = 0f;
            raio.pointLightOuterRadius = 4f;

            float levar = 0f;

            for (float t = 0f; t < lustro; t += Time.deltaTime)
            {
                if (controlador != null) controlador.ControleTravado = true;

                float k = t / lustro;
                raio.intensity = Mathf.Sin(k * Mathf.PI) * 2.4f;
                raio.transform.position = new Vector3(peito.x, peito.y + Mathf.Lerp(3f, 0.4f, Mathf.Min(1f, k * 1.6f)), 0f);

                // A ferrugem descascando, cada vez mais, ate a troca; dali pra
                // frente so uns pontinhos de brilho subindo.
                if (!mudou)
                {
                    levar += Mathf.Lerp(10f, 60f, k * 2f) * Time.deltaTime;
                    int flocos = (int)levar;
                    levar -= flocos;
                    for (int i = 0; i < flocos; i++)
                    {
                        Vector2 at = peito + new Vector2(Random.Range(-0.45f, 0.45f), Random.Range(-0.5f, 0.5f));
                        Motes.Estourar(at, 1, luzFerrugem, escuroFerrugem, 0.6f, 2.2f, -9f, 1.2f, 2f, 0.6f);
                    }
                }
                else if (Random.value < 0.4f)
                {
                    Motes.Subir(peito + Random.insideUnitCircle * 0.4f, 1, Color.white, ofuscar, 0.2f, 1.2f);
                }

                // Troca no meio do brilho, quando ele esta mais claro.
                if (!mudou && k >= 0.5f)
                {
                    aparencia.Polir();
                    ScrapCameraFollow.Tremer(0.12f, 0.35f);
                    Cinematic.Clarao2 = 0.6f;
                    Motes.Estourar(peito, 60, Color.white, ofuscar, 3f, 8f, -3f, 1f, 1f, 1.2f);
                    Motes.Estourar(peito, 30, luzFerrugem, escuroFerrugem, 2f, 5f, -12f, 1.4f, 2f, 0.6f);
                    mudou = true;
                }

                if (mudou) Cinematic.Clarao2 = Mathf.Max(0f, Cinematic.Clarao2 - Time.deltaTime * 2f);

                float inchar = 1f + 0.18f * Mathf.Sin(k * Mathf.PI);
                _jogador.localScale = new Vector3(tamanho.x * inchar, tamanho.y * inchar, tamanho.z);
                if (visual != null) visual.color = Color.Lerp(Color.white, ofuscar, Mathf.Sin(k * Mathf.PI));

                yield return null;
            }

            _jogador.localScale = tamanho;
            if (visual != null) visual.color = Color.white;
            Cinematic.Clarao2 = 0f;
            Destroy(objetoRaio);
            ScrapCameraFollow.Liberar();

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
