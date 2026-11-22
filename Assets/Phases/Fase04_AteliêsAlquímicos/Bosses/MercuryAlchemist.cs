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
    public class MercuryAlchemist : MonoBehaviour, IDamageable, IBossFight
    {
        // Os quadros (os mesmos numeros do AlchemistArt).
        private const int QuadroOcioso = 0, QuadrosOcioso = 6;
        private const int QuadroPlanar = 6, QuadrosPlanar = 6;
        private const int QuadroChamadaSino = 12, QuadrosChamadaSino = 3;
        private const int QuadroDestilar = 15, QuadrosDestilar = 3;
        private const int QuadroVapor = 18, QuadrosVapor = 3;
        private const int QuadroCondensar = 21, QuadrosCondensar = 3;
        private const int QuadroPreparoChicote = 24, QuadrosPreparoChicote = 2;
        private const int QuadroChicote = 26, QuadrosChicote = 3;
        private const int PosePrata = 29, QuadrosPrata = 2;
        private const int QuadroLivreQuebra = 31, QuadrosLivreQuebra = 3;
        private const int QuadroAbafar = 34, QuadrosAbafar = 2;
        private const int Ferido = 36;
        private const int QuadroRisada = 37, QuadrosRisada = 4;
        private const int QuadroMorte = 41, QuadrosMorte = 8;

        [UnityEngine.Serialization.FormerlySerializedAs("minX")] [Header("Arena")]
        [SerializeField] private float minX;
        [UnityEngine.Serialization.FormerlySerializedAs("maxX")] [SerializeField] private float maxX;
        [UnityEngine.Serialization.FormerlySerializedAs("floorY")] [SerializeField] private float chaoY;
        [UnityEngine.Serialization.FormerlySerializedAs("ceilingY")] [SerializeField] private float tetoY;
        [UnityEngine.Serialization.FormerlySerializedAs("wakeArea")] [SerializeField] private Rect areaDespertar;

        [UnityEngine.Serialization.FormerlySerializedAs("maxHealth")] [Header("Luta")]
        [SerializeField] private int vidaMax = 40;
        [UnityEngine.Serialization.FormerlySerializedAs("silverTime")] [SerializeField] private float tempoPrata = 2.6f;
        [UnityEngine.Serialization.FormerlySerializedAs("smotherTime")] [SerializeField] private float tempoAbafar = 9f;
        [UnityEngine.Serialization.FormerlySerializedAs("transmuteRange")] [SerializeField] private float alcanceTransmutar = 4.2f;
        [UnityEngine.Serialization.FormerlySerializedAs("glideSpeed")] [SerializeField] private float velocidadePlanar = 4.2f;

        [UnityEngine.Serialization.FormerlySerializedAs("circles")] [Header("Pecas")]
        [SerializeField] private ArenaCircle[] circulos;
        [UnityEngine.Serialization.FormerlySerializedAs("jar")] [SerializeField] private BellJar redoma;
        [UnityEngine.Serialization.FormerlySerializedAs("flood")] [SerializeField] private GameObject[] inundacao;
        [UnityEngine.Serialization.FormerlySerializedAs("whip")] [SerializeField] private SpriteRenderer chicote;
        [UnityEngine.Serialization.FormerlySerializedAs("beadSprite")] [SerializeField] private Sprite spriteGota;

        [UnityEngine.Serialization.FormerlySerializedAs("wreck")] [Tooltip("A carcaca que fica no chao depois da queda (ver BossWreck). Vazio: nao fica nada.")]
        [SerializeField] private GameObject destrocos;
        [UnityEngine.Serialization.FormerlySerializedAs("exitGate")] [SerializeField] private Gate portaoSaida;

        [UnityEngine.Serialization.FormerlySerializedAs("frames")] [Header("Quadros")]
        [SerializeField] private Sprite[] quadros;

        private SpriteRenderer _sprite;
        private Transform _jogador;
        private ScrapHealth _vida;
        private Vector3 _origem;

        private int _hp;
        private bool _acordado;
        private bool _morto;
        private bool _doisFase;
        private bool _pendenteInundacao;
        private bool _prata;
        private bool _vapor;
        private int _direcaoOlhar = -1;
        private int _ultimoAtaque = -1;
        private float _prontoContato;

        private readonly List<SpriteRenderer> _geiseres = new List<SpriteRenderer>();

        // As alturas do chicote, acima do chao. Os estrados tem 2 tiles: o
        // golpe alto passa rente ao alto deles e por cima da cabeca de quem
        // esta no chao.
        private const float BaixoChicote = 0.45f;
        private const float AltoChicote = 2.4f;

        // Quanto tempo cada coluna do geiser fica de pe.
        private const float RajadaGeiser = 0.45f;

        private readonly FramePlayback _reproducao = new FramePlayback();
        private float _feridoAte = -1f;

        [SaveProgress("_defeated")]
        private static bool _derrotado;

        /// <summary>O chefe em luta, se houver. O HUD usa pra barra de vida.</summary>
        public static MercuryAlchemist Lutando { get; private set; }

        /// <summary>Se ele ja caiu de vez (a trilha sonora usa pra calar a musica da luta).</summary>
        public bool EstaDerrubado => _morto;

        public MusicTrack MusicaBatalha => SoundtrackCatalog.AlquimistaMercurio;

        public bool CinematicasIgnoraMusica => false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegistrarLuta() => BossRoster.Adicionar(5, () => Lutando);

        public static bool Derrotado => _derrotado;

        public float FracaoVida => Mathf.Clamp01(_hp / (float)Mathf.Max(1, vidaMax));

        /// <summary>Em prata: a unica hora em que o cinzel fere.</summary>
        public bool EstaPrata => _prata;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Lutando = null;
            _derrotado = false;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoMinX, float novoMaxX, float novoChaoY, float novoTetoY, Rect novoAreaDespertar,
            ArenaCircle[] novoCirculos, BellJar novoRedoma, GameObject[] novoInundacao, SpriteRenderer novoChicote, Sprite gota,
            Gate portao)
        {
            minX = novoMinX;
            maxX = novoMaxX;
            chaoY = novoChaoY;
            tetoY = novoTetoY;
            areaDespertar = novoAreaDespertar;
            circulos = novoCirculos;
            redoma = novoRedoma;
            inundacao = novoInundacao;
            chicote = novoChicote;
            spriteGota = gota;
            portaoSaida = portao;
        }

        /// <summary>Usado pelos geradores de fase: a folha do AlchemistArt.</summary>
        /// <summary>A carcaca que fica no lugar dele depois da queda. Usado pelo gerador.</summary>
        public void ConfigurarDestrocos(GameObject novoDestrocos)
        {
            destrocos = novoDestrocos;
        }

        public void ConfigurarQuadros(Sprite[] novoQuadros)
        {
            quadros = novoQuadros;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _hp = vidaMax;

            // O corpo do tamanho do desenho: 2 de largura (os bracos abertos) e 3,6 de altura.
            var corpo = GetComponent<BoxCollider2D>();
            if (corpo != null)
            {
                corpo.size = new Vector2(2f, 3.6f);
                corpo.offset = new Vector2(0f, 1.8f);
            }
            _origem = transform.position;
            Tocar(QuadroOcioso, QuadrosOcioso, 3f);
            DefinirInundacao(false);
            EsconderChicote();

            ScrapHealth.Morreu += ReiniciarLuta;

            if (circulos == null) return;
            foreach (ArenaCircle circulo in circulos)
            {
                if (circulo != null) circulo.Polido += AoPolir;
            }
        }

        private void Start()
        {
            // Purificado: nao sobra nem a poca.
            if (AreaPurifier.EstaPurificado)
            {
                if (portaoSaida != null) portaoSaida.Abrir();
                Destroy(gameObject);
                return;
            }

            // Ja caiu nesta sessao: fica a poca com a mascara, e a saida aberta.
            if (_derrotado)
            {
                _morto = true;
                MostrarQuadro(QuadroMorte + QuadrosMorte - 1);
                DeixarDestrocos();
                if (portaoSaida != null) portaoSaida.Abrir();
                DefinirInundacao(true);
                return;
            }

            var controlador = ScrapRef.Controlador;
            if (controlador != null)
            {
                _jogador = controlador.transform;
                _vida = controlador.GetComponent<ScrapHealth>();
            }
        }

        private void OnDestroy()
        {
            ScrapHealth.Morreu -= ReiniciarLuta;
            if (Lutando == this) Lutando = null;

            if (circulos == null) return;
            foreach (ArenaCircle circulo in circulos)
            {
                if (circulo != null) circulo.Polido -= AoPolir;
            }
        }

        private void Update()
        {
            if (_morto || _jogador == null) return;

            if (!_acordado)
            {
                VirarParaJogador();
                if (!areaDespertar.Contains(_jogador.position)) return;

                _acordado = true;
                Lutando = this;
                DefinirCirculos(true);
                StartCoroutine(Cerebro());
                return;
            }

            // Encostar nele, liquido, machuca. Em prata ou em nuvem, nao.
            if (!_prata && !_vapor && Time.time >= _prontoContato && _vida != null)
            {
                Vector2 deslocamento = _jogador.position - transform.position;
                // O corpo dele no desenho (1 tile pra cada lado), mais a metade do Scrap.
                if (Mathf.Abs(deslocamento.x) < 1.3f && deslocamento.y > -0.9f && deslocamento.y < 3.5f)
                {
                    _vida.LevarDano(1, transform.position + Vector3.up);
                    _prontoContato = Time.time + 0.8f;
                }
            }

            if (_sprite != null)
            {
                Color alvo = _prata ? new Color(0.92f, 0.96f, 1f) : Color.white;
                _sprite.color = Color.Lerp(_sprite.color, alvo, 10f * Time.deltaTime);
            }
        }

        // ------------------------------------------------------------------
        // O cerebro
        // ------------------------------------------------------------------

        private IEnumerator Cerebro()
        {
            // Acorda rindo: ja sabe quem e.
            VirarParaJogador();
            Tocar(QuadroRisada, QuadrosRisada, 7f);
            Sfx.RisadaAlquimista.Tocar(transform.position);
            NoiseSystem.Emitir(transform.position, 12f, 0.6f);
            yield return new WaitForSeconds(1.2f);

            yield return Repetir();
        }

        private IEnumerator Repetir()
        {
            while (!_morto)
            {
                if (_pendenteInundacao)
                {
                    _pendenteInundacao = false;
                    yield return InundarOChao();
                }

                int ataque = ProximoAtaque();
                _ultimoAtaque = ataque;

                if (ataque == 0) yield return AtaqueDaRedoma();
                else if (ataque == 1) yield return AtaqueDeDestilacao();
                else if (ataque == 2) yield return AtaqueDeChicote();
                else yield return AtaqueDeGeiser();

                // Entre um golpe e outro, ele desliza atras do Scrap.
                yield return DeslizarAte(_doisFase ? 0.7f : 1.1f);
            }
        }

        private int ProximoAtaque()
        {
            // Quem se instala em cima de um estrado chama o geiser.
            bool empoleirado = _jogador != null && _jogador.position.y > chaoY + 1.2f;
            if (empoleirado && _ultimoAtaque != 3 && Random.value < 0.5f) return 3;

            int ataque;
            do ataque = Random.Range(0, 4);
            while (ataque == _ultimoAtaque && Random.value < 0.75f);
            return ataque;
        }

        private IEnumerator DeslizarAte(float segundos)
        {
            Tocar(QuadroPlanar, QuadrosPlanar, 8f);
            Sfx.PlanarAlquimista.Tocar(transform.position);
            float velocidade = _doisFase ? velocidadePlanar * 1.3f : velocidadePlanar;

            for (float t = 0f; t < segundos && _jogador != null; t += Time.deltaTime)
            {
                float alvo = _jogador.position.x - _direcaoOlhar * 2.4f;
                float passo = Mathf.Clamp(alvo - transform.position.x, -velocidade * Time.deltaTime, velocidade * Time.deltaTime);
                Mover(passo);
                VirarParaJogador();
                yield return null;
            }

            Tocar(QuadroOcioso, QuadrosOcioso, 6f);
            yield return new WaitForSeconds(_doisFase ? 0.12f : 0.28f);
        }

        // ------------------------------------------------------------------
        // Os golpes
        // ------------------------------------------------------------------

        private IEnumerator AtaqueDaRedoma()
        {
            VirarParaJogador();
            Tocar(QuadroChamadaSino, QuadrosChamadaSino, 5f, false);
            Sfx.ChamadaSinoAlquimista.Tocar(transform.position + Vector3.up * 3f);
            yield return new WaitForSeconds(0.6f);

            if (redoma != null && _jogador != null) redoma.Soltar(Mathf.Clamp(_jogador.position.x, minX, maxX));

            Tocar(QuadroOcioso, QuadrosOcioso, 6f);
            yield return new WaitForSeconds(_doisFase ? 0.6f : 1.0f);
        }

        private IEnumerator AtaqueDeDestilacao()
        {
            VirarParaJogador();
            Tocar(QuadroDestilar, QuadrosDestilar, 5f, false);
            Sfx.DestilarAlquimista.Tocar(transform.position);
            yield return new WaitForSeconds(0.65f);

            // Nuvem: sobe ao teto.
            _vapor = true;
            Tocar(QuadroVapor, QuadrosVapor, 6f);
            float nuvemY = tetoY - 2.6f;

            for (float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                transform.position = new Vector3(transform.position.x, Mathf.Lerp(chaoY, nuvemY, t / 0.5f), 0f);
                yield return null;
            }

            // Passeia pingando.
            float chuva = _doisFase ? 4.0f : 3.2f;
            float proximoGota = 0f;
            for (float t = 0f; t < chuva && _jogador != null; t += Time.deltaTime)
            {
                float alvo = _jogador.position.x;
                Mover(Mathf.Clamp(alvo - transform.position.x, -4f * Time.deltaTime, 4f * Time.deltaTime), nuvemY);

                if (Time.time >= proximoGota)
                {
                    proximoGota = Time.time + (_doisFase ? 0.16f : 0.23f);
                    float x = transform.position.x + Random.Range(-1.4f, 1.4f);
                    MercuryBead.Cair(new Vector3(x, nuvemY + 0.3f, 0f), chaoY, spriteGota);
                }

                yield return null;
            }

            // Escolhe onde se condensar: longe o bastante do Scrap pra ser justo.
            float pousar = transform.position.x;
            if (_jogador != null)
            {
                float lado = _jogador.position.x > (minX + maxX) * 0.5f ? -1f : 1f;
                pousar = Mathf.Clamp(_jogador.position.x + lado * 4f, minX, maxX);
            }

            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                Mover(Mathf.Clamp(pousar - transform.position.x, -8f * Time.deltaTime, 8f * Time.deltaTime), nuvemY);
                yield return null;
            }

            Tocar(QuadroCondensar, QuadrosCondensar, 5f, false);
            Sfx.CondensarAlquimista.Tocar(transform.position);
            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                transform.position = new Vector3(transform.position.x, Mathf.Lerp(nuvemY, chaoY, t / 0.6f), 0f);
                yield return null;
            }

            transform.position = new Vector3(transform.position.x, chaoY, 0f);
            _vapor = false;
            VirarParaJogador();
            Tocar(QuadroOcioso, QuadrosOcioso, 6f);
            yield return new WaitForSeconds(0.4f);
        }

        private IEnumerator AtaqueDeChicote()
        {
            int chicotadas = _doisFase ? 3 : 2;
            bool alto = Random.value < 0.5f;

            for (int chicotada = 0; chicotada < chicotadas; chicotada++)
            {
                VirarParaJogador();
                float altura = alto ? AltoChicote : BaixoChicote;

                // O aviso: ele recolhe o braco, a gota cresce na luva e o fio fino aparece.
                Tocar(QuadroPreparoChicote, QuadrosPreparoChicote, 4f, false);
                MostrarChicote(altura, true);
                Sfx.PreparoChicoteAlquimista.Tocar(transform.position, tom2: _doisFase && chicotada > 0 ? 1.4f : 1f);
                yield return new WaitForSeconds(chicotada > 0 ? 0.42f : 0.58f);

                // O golpe.
                Tocar(QuadroChicote, QuadrosChicote, 10f, false);
                MostrarChicote(altura, false);
                Sfx.ChicoteAlquimista.Tocar(transform.position + Vector3.right * (_direcaoOlhar * 3f), tom2: alto ? 1.1f : 0.9f);
                for (float t = 0f; t < 0.35f; t += Time.deltaTime)
                {
                    AcertoDoChicote();
                    yield return null;
                }

                EsconderChicote();
                alto = !alto;
                yield return new WaitForSeconds(0.25f);
            }

            Tocar(QuadroOcioso, QuadrosOcioso, 6f);
            yield return new WaitForSeconds(0.5f);
        }

        private void MostrarChicote(float altura, bool previa)
        {
            if (chicote == null) return;

            float inicio = transform.position.x + _direcaoOlhar * 0.8f;
            float fim = _direcaoOlhar > 0 ? maxX + 1f : minX - 1f;
            float comprimento = Mathf.Abs(fim - inicio);

            chicote.enabled = true;
            chicote.transform.position = new Vector3((inicio + fim) * 0.5f, chaoY + altura, 0f);
            chicote.size = new Vector2(comprimento, previa ? 0.12f : 0.4f);
            chicote.color = previa ? new Color(1f, 1f, 1f, 0.35f) : Color.white;
        }

        private void EsconderChicote()
        {
            if (chicote != null) chicote.enabled = false;
        }

        private void AcertoDoChicote()
        {
            if (chicote == null || _vida == null || _jogador == null) return;

            Bounds limites = chicote.bounds;
            Vector2 sucata = _jogador.position;

            // O Scrap tem ~1,2 tile de altura, com os pes em position.
            bool atraves = sucata.x > limites.min.x - 0.3f && sucata.x < limites.max.x + 0.3f;
            bool nivel = limites.center.y > sucata.y - 0.1f && limites.center.y < sucata.y + 1.25f;

            if (atraves && nivel) _vida.LevarDano(1, new Vector2(transform.position.x, limites.center.y));
        }

        private IEnumerator AtaqueDeGeiser()
        {
            // O aviso: curva-se e derrama mercurio no chao.
            VirarParaJogador();
            Tocar(QuadroAbafar, QuadrosAbafar, 4f);
            Sfx.DespejarAlquimista.Tocar(transform.position);
            NoiseSystem.Emitir(transform.position, 8f, 0.4f);
            yield return new WaitForSeconds(0.5f);

            int colunasTotal = _doisFase ? 3 : 2;
            float alerta = _doisFase ? 0.6f : 0.75f;
            float espaco = _doisFase ? 0.45f : 0.7f;
            GarantirGeiseres(colunasTotal);

            // Cada coluna mira onde o Scrap esta agora: parar e apanhar.
            for (int i = 0; i < colunasTotal && i < _geiseres.Count && _jogador != null; i++)
            {
                StartCoroutine(Geiser(_geiseres[i], _jogador.position.x, alerta));
                yield return new WaitForSeconds(espaco);
            }

            // A ultima ainda sobe: ele aponta pro teto com ela.
            Tocar(QuadroChamadaSino, QuadrosChamadaSino, 5f, false);
            yield return new WaitForSeconds(Mathf.Max(0f, alerta + RajadaGeiser - espaco) + 0.1f);

            EsconderGeiseres();
            Tocar(QuadroOcioso, QuadrosOcioso, 6f);
            yield return new WaitForSeconds(_doisFase ? 0.3f : 0.5f);
        }

        private IEnumerator Geiser(SpriteRenderer coluna, float x, float alerta)
        {
            float solo = ChaoAbaixo(x);
            float topo = tetoY;

            // O aviso: o fio fino sobe do chao e treme.
            coluna.enabled = true;
            Sfx.AlertaGeiserAlquimista.Tocar(new Vector2(x, solo), tom2: 0.75f / alerta);
            for (float t = 0f; t < alerta; t += Time.deltaTime)
            {
                float oscilacao = Mathf.Sin(t * 50f) * 0.05f;
                PosicionarColuna(coluna, x + oscilacao, solo, topo, false, new Color(1f, 1f, 1f, 0.3f + 0.3f * (t / alerta)));
                yield return null;
            }

            // A erupcao: sobe de uma vez e fica um instante.
            NoiseSystem.Emitir(new Vector3(x, solo, 0f), 9f, 0.5f);
            Sfx.RajadaGeiserAlquimista.Tocar(new Vector2(x, solo));
            for (float t = 0f; t < RajadaGeiser; t += Time.deltaTime)
            {
                float alcanceGolpe = Mathf.Lerp(solo + 0.5f, topo, Mathf.Clamp01(t / 0.1f));
                PosicionarColuna(coluna, x, solo, alcanceGolpe, true, Color.white);
                AcertoDoGeiser(x, solo, alcanceGolpe);
                yield return null;
            }

            coluna.enabled = false;
        }

        private static void PosicionarColuna(SpriteRenderer coluna, float x, float solo, float topo, bool cheio, Color cor)
        {
            // O chicote deitado, de pe: o comprimento vira a altura.
            coluna.transform.SetPositionAndRotation(new Vector3(x, (solo + topo) * 0.5f, 0f), Quaternion.Euler(0f, 0f, 90f));
            coluna.transform.localScale = cheio ? new Vector3(1f, 2.5f, 1f) : Vector3.one;
            coluna.size = new Vector2(Mathf.Max(0.1f, topo - solo), cheio ? 0.4f : 0.12f);
            coluna.color = cor;
        }

        private void AcertoDoGeiser(float x, float solo, float topo)
        {
            if (_vida == null || _jogador == null) return;

            Vector2 sucata = _jogador.position;
            if (Mathf.Abs(sucata.x - x) < 0.75f && sucata.y > solo - 0.3f && sucata.y < topo)
                _vida.LevarDano(1, new Vector2(x, sucata.y));
        }

        /// <summary>As colunas sao copias do chicote, feitas na primeira vez.</summary>
        private void GarantirGeiseres(int quantidade)
        {
            if (chicote == null) return;

            while (_geiseres.Count < quantidade)
            {
                GameObject copia = Instantiate(chicote.gameObject, chicote.transform.parent);
                copia.name = "MercuryGeyser";
                var coluna = copia.GetComponent<SpriteRenderer>();
                coluna.enabled = false;
                _geiseres.Add(coluna);
            }
        }

        private void EsconderGeiseres()
        {
            foreach (SpriteRenderer coluna in _geiseres)
            {
                if (coluna != null) coluna.enabled = false;
            }
        }

        /// <summary>O chao embaixo de x: o piso da arena ou o alto de um estrado.</summary>
        private float ChaoAbaixo(float x)
        {
            int mascara = LayerMask.GetMask("Ground");
            RaycastHit2D[] golpes = Physics2D.RaycastAll(new Vector2(x, tetoY - 3.2f), Vector2.down, tetoY - chaoY + 2f, mascara);
            foreach (RaycastHit2D golpe in golpes)
            {
                if (golpe.collider == null || golpe.collider.isTrigger) continue;
                if (redoma != null && golpe.collider.transform.IsChildOf(redoma.transform)) continue;
                return Mathf.Max(chaoY, golpe.point.y);
            }

            return chaoY;
        }

        // ------------------------------------------------------------------
        // A prata
        // ------------------------------------------------------------------

        private void AoPolir(ArenaCircle circulo)
        {
            if (!_acordado || _morto || _prata || _vapor) return;
            if (Mathf.Abs(circulo.transform.position.x - transform.position.x) > alcanceTransmutar) return;

            // A runa estoura e o raio dela desce ate ele.
            circulo.Golpear(transform.position + Vector3.up * 1.4f);

            StopAllCoroutines();
            EsconderChicote();
            EsconderGeiseres();
            transform.position = new Vector3(transform.position.x, chaoY, 0f);
            StartCoroutine(Transmutado(circulo));
        }

        private IEnumerator Transmutado(ArenaCircle circulo)
        {
            _prata = true;
            Sfx.TransmutarAlquimista.Tocar(transform.position);
            NoiseSystem.Emitir(transform.position, 10f, 0.7f);
            Tocar(PosePrata, QuadrosPrata, 3f);

            yield return new WaitForSeconds(_doisFase ? tempoPrata * 0.85f : tempoPrata);

            // Se solta: racha, o mercurio vaza pelas fendas, e ele se sacode...
            Tocar(QuadroLivreQuebra, QuadrosLivreQuebra, 5f, false);
            Sfx.RachandoAlquimista.Tocar(transform.position);
            NoiseSystem.Emitir(transform.position, 8f, 0.4f);
            yield return new WaitForSeconds(0.6f);
            _prata = false;

            // ...e estoura: quem ficou colado batendo leva o golpe e os respingos.
            EstourarLivre();

            // E afoga o circulo que o prendeu.
            Tocar(QuadroAbafar, QuadrosAbafar, 4f);
            float x = circulo.transform.position.x;
            for (float t = 0f; t < 0.5f; t += Time.deltaTime)
            {
                Mover(Mathf.Clamp(x - transform.position.x, -5f * Time.deltaTime, 5f * Time.deltaTime));
                yield return null;
            }

            circulo.Abafar(tempoAbafar);
            Sfx.DespejarAlquimista.Tocar(circulo.transform.position, tom2: 0.85f);
            yield return new WaitForSeconds(0.5f);

            yield return Repetir();
        }

        private void EstourarLivre()
        {
            NoiseSystem.Emitir(transform.position, 12f, 0.7f);
            Sfx.RajadaAlquimista.Tocar(transform.position);

            if (_vida != null && _jogador != null)
            {
                Vector2 deslocamento = _jogador.position - transform.position;
                if (Mathf.Abs(deslocamento.x) < 2.3f && deslocamento.y > -0.5f && deslocamento.y < 3.2f)
                    _vida.LevarDano(1, transform.position + Vector3.up);
            }

            if (spriteGota == null) return;

            // Gotas pros dois lados, das mais curtas as mais longas.
            int gotas = _doisFase ? 8 : 6;
            Vector3 peito = transform.position + Vector3.up * 2f;
            for (int i = 0; i < gotas; i++)
            {
                float lado = i % 2 == 0 ? 1f : -1f;
                float velocidade = 3f + (i / 2) * 2.2f;
                MercuryBead.Respingar(peito, new Vector2(lado * velocidade, 9f + Random.Range(-1f, 1f)), chaoY, spriteGota);
            }
        }

        public void ReceberDano(int quantia, Vector2 de)
        {
            if (_morto || !_acordado) return;

            if (!_prata)
            {
                // Liquido: o cinzel atravessa e so espirra.
                if (_sprite != null) _sprite.color = new Color(0.8f, 0.9f, 1f, 0.75f);
                NoiseSystem.Emitir(transform.position, 4f, 0.2f);
                Sfx.RespingoAlquimista.Tocar(transform.position + Vector3.up);
                return;
            }

            Sfx.PrataFeridoChefe.Tocar(transform.position, tom2: 1.1f);

            _hp -= quantia;
            _feridoAte = Time.time + 0.15f;
            if (_sprite != null) _sprite.color = new Color(1f, 0.6f, 0.6f);

            if (_hp <= 0)
            {
                Morrer();
                return;
            }

            if (!_doisFase && _hp <= vidaMax / 2)
            {
                _doisFase = true;
                _pendenteInundacao = true;
            }
        }

        private IEnumerator InundarOChao()
        {
            // Ri, e os tanques racham: o chao entre os estrados vira mercurio.
            VirarParaJogador();
            Tocar(QuadroRisada, QuadrosRisada, 7f);
            Sfx.RisadaAlquimista.Tocar(transform.position, tom2: 0.9f);
            NoiseSystem.Emitir(transform.position, 14f, 0.8f);
            yield return new WaitForSeconds(0.7f);
            DefinirInundacao(true);
            Sfx.InundacaoAlquimista.Tocar(transform.position);
            yield return new WaitForSeconds(0.7f);
        }

        // ------------------------------------------------------------------
        // Cair e recomecar
        // ------------------------------------------------------------------

        private void Morrer()
        {
            _morto = true;
            _derrotado = true;
            _prata = false;
            StopAllCoroutines();
            EsconderChicote();
            EsconderGeiseres();
            DefinirCirculos(false);
            if (redoma != null) redoma.Esconder();
            if (Lutando == this) Lutando = null;

            StartCoroutine(EscoarTudo());
        }

        private IEnumerator EscoarTudo()
        {
            Tocar(QuadroMorte, QuadrosMorte, 5f, false);
            Sfx.DrenoAlquimista.Tocar(transform.position);
            NoiseSystem.Emitir(transform.position, 12f, 0.6f);
            yield return new WaitForSeconds(QuadrosMorte / 5f + 0.4f);

            DeixarDestrocos();
            if (portaoSaida != null) portaoSaida.Abrir();
        }

        /// <summary>A poca vira a carcaca desenhada: o jaleco, a mascara, o tanque vazando, o vapor.</summary>
        private void DeixarDestrocos()
        {
            if (destrocos == null) return;
            if (_sprite != null) _sprite.enabled = false;
            BossWreck.Mostrar(destrocos, transform.position);
        }

        /// <summary>O Scrap morreu: a luta recomeca do zero.</summary>
        private void ReiniciarLuta()
        {
            if (_morto) return;

            StopAllCoroutines();

            _hp = vidaMax;
            _acordado = false;
            _doisFase = false;
            _pendenteInundacao = false;
            _prata = false;
            _vapor = false;
            _ultimoAtaque = -1;
            transform.position = _origem;

            EsconderChicote();
            EsconderGeiseres();
            DefinirInundacao(false);
            DefinirCirculos(false);
            if (redoma != null) redoma.Esconder();
            if (circulos != null) foreach (ArenaCircle circulo in circulos) if (circulo != null) circulo.ReiniciarCirculo();

            if (_sprite != null) _sprite.color = Color.white;
            Tocar(QuadroOcioso, QuadrosOcioso, 3f);

            if (Lutando == this) Lutando = null;
        }

        private void DefinirInundacao(bool on)
        {
            if (inundacao == null) return;
            foreach (GameObject poca in inundacao)
            {
                if (poca != null) poca.SetActive(on);
            }
        }

        private void DefinirCirculos(bool armado)
        {
            if (circulos == null) return;
            foreach (ArenaCircle circulo in circulos)
            {
                if (circulo != null) circulo.Armado = armado;
            }
        }

        // ------------------------------------------------------------------
        // Movimento e animacao
        // ------------------------------------------------------------------

        private void Mover(float deltaX, float y = float.NaN)
        {
            Vector3 posicao = transform.position;
            posicao.x = Mathf.Clamp(posicao.x + deltaX, minX, maxX);
            if (!float.IsNaN(y)) posicao.y = y;
            transform.position = posicao;
        }

        private void VirarParaJogador()
        {
            if (_jogador == null) return;
            _direcaoOlhar = _jogador.position.x >= transform.position.x ? 1 : -1;

            // A arte olha pra direita.
            if (_sprite != null) _sprite.flipX = _direcaoOlhar < 0;
        }

        private void Tocar(int inicio, int quantidade, float quadrosPorSegundo, bool repeticao = true) =>
            _reproducao.Tocar(inicio, quantidade, quadrosPorSegundo, repeticao);

        private void MostrarQuadro(int quadro)
        {
            Tocar(quadro, 1, 1f, false);
        }

        private void LateUpdate()
        {
            if (_sprite == null || quadros == null || quadros.Length == 0) return;

            int quadro = _reproducao.Quadro;

            if (_prata && Time.time < _feridoAte) quadro = Ferido;

            if (quadro >= 0 && quadro < quadros.Length && quadros[quadro] != null) FrameAnimator.Exibir(_sprite, quadros[quadro]);
        }
    }
}
