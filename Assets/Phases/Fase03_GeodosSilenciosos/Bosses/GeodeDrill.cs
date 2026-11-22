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
    ///   Por baixo   sem pilar pra seguir, as vezes ela nao sai da parede: fura
    ///               pra cima, do chao, onde ouviu o Scrap. Lascas pulam do
    ///               chao antes (o aviso). Na furia sao duas, a segunda em
    ///               cima de onde ele estiver.
    ///   Presa       se o Scrap golpeou um pilar ressonante enquanto ela
    ///               escutava, ela vai pra ele (o pilar pulsa) — mas so crava
    ///               se ele estiver ACESO quando ela chegar, e o pilar so fica
    ///               aceso um instante. Tocar uma vez chama; e preciso tocar
    ///               de novo na hora certa, com ela vindo. Apagado, ela passa
    ///               direto e atropela quem estiver atras.
    ///               Cravada, o pilar estilhaca pra frente (quem se escondeu
    ///               atras dele leva), o chao solta tres traças e o motor fica
    ///               exposto por pouco tempo: so assim o cinzel fere. Ao se
    ///               soltar, ela ronca e gira — quem ficou colado apanha.
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
    public class GeodeDrill : MonoBehaviour, IDamageable, IBossFight
    {
        [UnityEngine.Serialization.FormerlySerializedAs("maxHealth")] [SerializeField] private int vidaMax = 24;

        [UnityEngine.Serialization.FormerlySerializedAs("exposedTime")] [Tooltip("Quanto tempo o motor fica exposto depois de cravar (na furia, um pouco menos).")]
        [SerializeField] private float tempoExposto = 2.6f;

        // Deitada, metade da altura dela: de pe (furando pra cima), e o
        // quanto o corpo fica pro lado do pivo.
        private const float AlturaMetade = 1.1f;

        [UnityEngine.Serialization.FormerlySerializedAs("minX")] [Header("Arena")]
        [SerializeField] private float minX;
        [UnityEngine.Serialization.FormerlySerializedAs("maxX")] [SerializeField] private float maxX;
        [UnityEngine.Serialization.FormerlySerializedAs("wakeArea")] [SerializeField] private Rect areaDespertar;

        [UnityEngine.Serialization.FormerlySerializedAs("floors")] [Tooltip("A altura do chao de cada camada, de cima pra baixo.")]
        [SerializeField] private float[] chaos;

        [UnityEngine.Serialization.FormerlySerializedAs("slabs")] [Tooltip("O chao que desaba em cada camada (a ultima nao desaba).")]
        [SerializeField] private GameObject[] lajes;

        [UnityEngine.Serialization.FormerlySerializedAs("pillars")] [Tooltip("Os pilares ressonantes, de todas as camadas.")]
        [SerializeField] private ResonantCrystal[] pilares;

        [UnityEngine.Serialization.FormerlySerializedAs("debrisSprite")] [Header("O que ela usa e deixa")]
        [SerializeField] private Sprite spriteEntulho;
        [UnityEngine.Serialization.FormerlySerializedAs("shardSprite")] [SerializeField] private Sprite spriteFragmento;
        [UnityEngine.Serialization.FormerlySerializedAs("rewardSprite")] [SerializeField] private Sprite spriteRecompensa;

        [UnityEngine.Serialization.FormerlySerializedAs("wreck")] [Tooltip("A carcaca que fica no chao depois da queda (ver BossWreck). Vazio: nao fica nada.")]
        [SerializeField] private GameObject destrocos;
        [UnityEngine.Serialization.FormerlySerializedAs("purifier")] [SerializeField] private AreaPurifier purificador;

        [UnityEngine.Serialization.FormerlySerializedAs("plaque")] [Tooltip("A placa do chassi: um objeto desligado, que aparece quando ela para.")]
        [SerializeField] private GameObject placaTexto;

        [UnityEngine.Serialization.FormerlySerializedAs("brood")] [Header("As traças")]
        [Tooltip("Os mites que a pancada solta, desligados na cena.")]
        [SerializeField] private GameObject[] crias;
        [UnityEngine.Serialization.FormerlySerializedAs("broodPerCrash")] [SerializeField] private int criasPorImpacto = 3;
        [UnityEngine.Serialization.FormerlySerializedAs("crackFrames")] [Tooltip("A rachadura da parede de onde elas saem (os mesmos 3 quadros do Capataz).")]
        [SerializeField] private Sprite[] quadrosRachadura;

        private readonly System.Collections.Generic.List<GameObject> _eclodindo = new System.Collections.Generic.List<GameObject>();
        private SpriteRenderer[] _rachaduras;

        private SpriteRenderer _sprite;
        private Collider2D _colisor;
        private Transform _jogador;
        private ScrapHealth _vidaSucata;
        private int _vida;
        private int _camadaVisual;
        private bool _acordado;
        private bool _morto;
        private bool _carregando;
        private bool _escutando;
        private bool _ruidoProprio;
        private float _expostoAte;
        private float _ouvidoX;
        private ResonantCrystal _isca;
        private Transform _tremendo;
        private Vector3 _origemTremendo;
        private SfxLoop _motor;

        [SaveProgress("_defeated")]
        private static bool _derrotado;

        /// <summary>O boss em luta, se houver. O HUD usa pra mostrar a barra de vida.</summary>
        public static GeodeDrill Lutando { get; private set; }

        /// <summary>Se ele ja caiu de vez (a trilha sonora usa pra calar a musica da luta).</summary>
        public bool EstaDerrubado => _morto;

        public MusicTrack MusicaBatalha => SoundtrackCatalog.BrocaGeodo;

        public bool CinematicasIgnoraMusica => false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegistrarLuta() => BossRoster.Adicionar(4, () => Lutando);

        /// <summary>Se ela ja parou nesta sessao.</summary>
        public static bool Derrotado => _derrotado;

        public float FracaoVida => Mathf.Clamp01(_vida / (float)Mathf.Max(1, vidaMax));

        /// <summary>Verdadeiro enquanto ela esta presa num pilar, com o motor a mostra.</summary>
        public bool EstaExposto => Time.time < _expostoAte;

        private float ChaoY => chaos[Mathf.Clamp(_camadaVisual, 0, chaos.Length - 1)];
        private bool Frenetico => _camadaVisual > 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Lutando = null;
            _derrotado = false;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(float novoMinX, float novoMaxX, Rect novoAreaDespertar, float[] novoChaos,
            GameObject[] novoLajes, ResonantCrystal[] novoPilares, Sprite entulho, Sprite fragmento, Sprite recompensa,
            AreaPurifier novoPurificador, GameObject novoPlaca)
        {
            minX = novoMinX;
            maxX = novoMaxX;
            areaDespertar = novoAreaDespertar;
            chaos = novoChaos;
            lajes = novoLajes;
            pilares = novoPilares;
            spriteEntulho = entulho;
            spriteFragmento = fragmento;
            spriteRecompensa = recompensa;
            purificador = novoPurificador;
            placaTexto = novoPlaca;
        }

        /// <summary>Usado pelos geradores de fase: as traças que saem do chao a cada pilar cravado.</summary>
        /// <summary>A carcaca que fica no lugar dele depois da queda. Usado pelo gerador.</summary>
        public void ConfigurarDestrocos(GameObject novoDestrocos)
        {
            destrocos = novoDestrocos;
        }

        public void ConfigurarCrias(GameObject[] novoCrias, Sprite[] novoQuadrosRachadura = null)
        {
            crias = novoCrias;
            quadrosRachadura = novoQuadrosRachadura;
        }

        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _colisor = GetComponent<Collider2D>();
            _vida = vidaMax;
            _motor = SfxLoop.Prender(gameObject, Sfx.MotorBroca);

            ScrapHealth.Morreu += ReiniciarLuta;
        }

        private void OnEnable()
        {
            NoiseSystem.Ouvido += AoOuvirRuido;

            if (pilares == null) return;

            foreach (ResonantCrystal pilar in pilares)
            {
                if (pilar != null) pilar.Soou += AoPilarSoar;
            }
        }

        private void OnDisable()
        {
            NoiseSystem.Ouvido -= AoOuvirRuido;

            if (pilares == null) return;

            foreach (ResonantCrystal pilar in pilares)
            {
                if (pilar != null) pilar.Soou -= AoPilarSoar;
            }
        }

        private void Start()
        {
            if (placaTexto != null) placaTexto.SetActive(false);

            if (_derrotado)
            {
                // Ja parou numa visita anterior: a arena fica como ela deixou.
                _morto = true;
                _camadaVisual = chaos.Length - 1;
                DefinirLajes(false);
                VirarDestrocos(new Vector3((minX + maxX) * 0.5f, ChaoY, 0f));

                if (!ScrapChisel.TemGolpeRessonancia) SoltarRecompensa();
                return;
            }

            Esconder();

            var controlador = ScrapRef.Controlador;
            if (controlador != null)
            {
                _jogador = controlador.transform;
                _vidaSucata = controlador.GetComponent<ScrapHealth>();
            }
        }

        private void OnDestroy()
        {
            ScrapHealth.Morreu -= ReiniciarLuta;
            if (Lutando == this) Lutando = null;
        }

        private void Update()
        {
            AtualizarMotor();

            if (_sprite != null && !_morto && _sprite.enabled)
            {
                // Presa no pilar ela fica em brasa: e a deixa pra bater.
                Color alvo = EstaExposto ? new Color(1f, 0.62f, 0.35f) : Color.white;
                _sprite.color = Color.Lerp(_sprite.color, alvo, 12f * Time.deltaTime);
            }

            if (_acordado || _morto || _jogador == null) return;
            if (!areaDespertar.Contains(_jogador.position)) return;

            _acordado = true;
            Lutando = this;
            _ouvidoX = _jogador.position.x;
            StartCoroutine(Cerebro());
        }

        /// <summary>
        /// O motor roda o tempo todo: abafado enterrada, cheio fora da rocha,
        /// mais agudo correndo e engasgando presa no pilar.
        /// </summary>
        private void AtualizarMotor()
        {
            if (_motor == null) return;

            bool correndo = _acordado && !_morto;
            bool visivel = _sprite != null && _sprite.enabled;

            _motor.Nivel = !correndo ? 0f : visivel ? (EstaExposto ? 0.7f : 1f) : 0.4f;
            _motor.Tom = (_carregando ? 1.25f : EstaExposto ? 0.75f : 1f) * (Frenetico ? 1.08f : 1f);
        }

        /// <summary>O Scrap morreu: a luta recomeca do zero, com o chao de volta.</summary>
        private void ReiniciarLuta()
        {
            if (_morto) return;

            StopAllCoroutines();
            PararTremor();

            _vida = vidaMax;
            _camadaVisual = 0;
            _acordado = false;
            _carregando = false;
            _escutando = false;
            _expostoAte = 0f;
            _isca = null;
            LimparAlvos();

            DefinirLajes(true);
            Esconder();

            if (Lutando == this) Lutando = null;

            // O Damageable delas as religa nesse mesmo aviso: desliga no quadro seguinte.
            StartCoroutine(RecolocarCriasNoProximoQuadro());
        }

        private IEnumerator RecolocarCriasNoProximoQuadro()
        {
            yield return null;
            RecolocarCrias();
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

            if (_rachaduras != null)
            {
                foreach (SpriteRenderer rachadura in _rachaduras)
                {
                    if (rachadura != null) rachadura.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// A pancada no pilar acorda as paredes: as tracas racham a parede
        /// (como no turno do Capataz) e saem dela, alternando os lados.
        /// </summary>
        private void LiberarCrias(float x)
        {
            if (crias == null) return;

            int solto = 0;
            int lado = Random.value < 0.5f ? 0 : 1;

            foreach (GameObject acaro in crias)
            {
                if (solto >= criasPorImpacto) break;
                if (acaro == null || acaro.activeSelf || _eclodindo.Contains(acaro)) continue;

                StartCoroutine(Chocar(acaro, (lado + solto) % 2, solto * 0.25f));
                solto++;
            }
        }

        /// <summary>A rachadura de cada parede, criada na primeira vez (a arte do Capataz).</summary>
        private SpriteRenderer Rachar(int lado)
        {
            if (quadrosRachadura == null || quadrosRachadura.Length < 3) return null;

            if (_rachaduras == null) _rachaduras = new SpriteRenderer[2];
            if (_rachaduras[lado] == null)
            {
                var hospedeiro = new GameObject(lado == 0 ? "WallCrack_Left" : "WallCrack_Right");
                hospedeiro.transform.SetParent(transform.parent, false);
                var arte = new GameObject("Art");
                arte.transform.SetParent(hospedeiro.transform, false);
                var renderizador = arte.AddComponent<SpriteRenderer>();
                if (_sprite != null) renderizador.sortingLayerID = _sprite.sortingLayerID;
                renderizador.sortingOrder = (_sprite != null ? _sprite.sortingOrder : 0) - 1;
                renderizador.flipX = lado == 1;
                _rachaduras[lado] = renderizador;
            }

            return _rachaduras[lado];
        }

        private IEnumerator Chocar(GameObject acaro, int lado, float atraso)
        {
            _eclodindo.Add(acaro);
            if (atraso > 0f) yield return new WaitForSeconds(atraso);

            var local = new Vector3(lado == 0 ? minX + 1f : maxX - 1f, ChaoY, 0f);
            SpriteRenderer rachadura = Rachar(lado);
            if (rachadura != null)
            {
                rachadura.transform.parent.position = local;
                rachadura.gameObject.SetActive(true);
                rachadura.sprite = quadrosRachadura[0];
            }

            Sfx.EclosaoCapataz.Tocar(local);

            // A parede treme, rachando.
            for (float t = 0f; t < 0.7f; t += Time.deltaTime)
            {
                if (rachadura != null)
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
                rachadura.sprite = quadrosRachadura[2];
            }

            if (_morto) yield break;

            acaro.SetActive(true);
            var corpo = acaro.GetComponent<Damageable>();
            if (corpo != null) corpo.Restaurar();

            var rastejante = acaro.GetComponent<CrystalMite>();
            if (rastejante != null) rastejante.MudarDeCasa(local);
            else acaro.transform.position = local;

            // A parede estourando acorda a traca (a broca nao se engana com o proprio estrondo).
            Estrondo(new Vector2(local.x, ChaoY + 0.5f), 6f, 0.6f);

            yield return new WaitForSeconds(1.2f);
            if (rachadura != null && !_eclodindo.Exists(m => m != null)) rachadura.gameObject.SetActive(false);
        }

        private void DefinirLajes(bool presente)
        {
            if (lajes == null) return;

            foreach (GameObject laje in lajes)
            {
                if (laje != null) laje.SetActive(presente);
            }
        }

        private void Esconder()
        {
            if (_sprite != null) _sprite.enabled = false;
            if (_colisor != null) _colisor.enabled = false;
            transform.rotation = Quaternion.identity;
            _carregando = false;
        }

        private void LimparAlvos()
        {
            if (pilares == null) return;
            foreach (ResonantCrystal pilar in pilares)
            {
                if (pilar != null) pilar.Mirado = false;
            }
        }

        private void Mostrar(float x, int virado)
        {
            transform.rotation = Quaternion.identity;
            transform.position = new Vector3(x, ChaoY, 0f);

            if (_sprite != null)
            {
                _sprite.enabled = true;
                _sprite.color = Color.white;

                // A arte olha pra direita.
                _sprite.flipX = virado < 0;
            }

            if (_colisor != null) _colisor.enabled = true;
        }

        /// <summary>O ruido que ela mesma faz nao pode engana-la.</summary>
        private void Estrondo(Vector2 pontoOrigem, float raioArea, float volumeRuido2)
        {
            _ruidoProprio = true;
            NoiseSystem.Emitir(pontoOrigem, raioArea, volumeRuido2);
            _ruidoProprio = false;
        }

        // ------------------------------------------------------------------
        // Ouvir
        // ------------------------------------------------------------------

        private void AoOuvirRuido(Vector2 pontoOrigem, float raioArea, float volumeRuido2)
        {
            if (_ruidoProprio || !_escutando) return;

            // So o que acontece na camada dela, dentro da arena.
            if (Mathf.Abs(pontoOrigem.y - ChaoY) > 9f) return;
            if (pontoOrigem.x < minX - 3f || pontoOrigem.x > maxX + 3f) return;

            _ouvidoX = pontoOrigem.x;
        }

        private void AoPilarSoar(ResonantCrystal pilar)
        {
            if (!_escutando) return;
            if (!pilar.isActiveAndEnabled || Mathf.Abs(pilar.transform.position.y - ChaoY) > 1f) return;

            // Uma nota dessas apaga qualquer passo: e pra ela que a broca vai.
            _isca = pilar;
        }

        // ------------------------------------------------------------------
        // Cerebro
        // ------------------------------------------------------------------

        private IEnumerator Cerebro()
        {
            while (!_morto)
            {
                // Enterrada: escuta, e o cristal acende por onde ela passa.
                Esconder();
                _isca = null;
                _escutando = true;

                float enterrado = Frenetico ? 1.5f : 2.3f;
                float tremorX2 = transform.position.x;

                for (float t = 0f; t < enterrado; t += Time.deltaTime)
                {
                    tremorX2 = Mathf.MoveTowards(tremorX2, _ouvidoX, 9f * Time.deltaTime);

                    // Escondida, ela anda junto com o ronco: o motor soa de onde ela esta.
                    transform.position = new Vector3(tremorX2, ChaoY, 0f);
                    if (Time.frameCount % 12 == 0) Estrondo(new Vector2(tremorX2, ChaoY + 0.5f), 4f, 0.1f);

                    yield return null;
                }

                _escutando = false;

                ResonantCrystal isca = _isca;

                // Sem pilar pra seguir, as vezes ela fura pra cima, por baixo do Scrap.
                if (isca == null && Random.value < (Frenetico ? 0.6f : 0.4f))
                {
                    yield return EntrarEmErupcao();
                    continue;
                }

                if (isca != null) isca.Mirado = true;
                float alvo = Mathf.Clamp(isca != null ? isca.transform.position.x : _ouvidoX, minX + 3f, maxX - 3f);

                // Sai da parede mais distante, pra atravessar a arena inteira.
                bool deEsquerda = alvo - minX > maxX - alvo;
                float inicioX = deEsquerda ? minX : maxX;
                int direcao = deEsquerda ? 1 : -1;

                Mostrar(inicioX, direcao);

                // O aviso: ela treme na boca do tunel, e a sala acende.
                Estrondo(new Vector2(inicioX, ChaoY + 1f), 12f, 0.2f);
                Sfx.EmergirBroca.Tocar(new Vector2(inicioX, ChaoY + 1f));

                for (float t = 0f; t < 0.7f; t += Time.deltaTime)
                {
                    transform.position = new Vector3(inicioX + Mathf.Sin(t * 70f) * 0.08f, ChaoY, 0f);
                    if (_sprite != null && Mathf.FloorToInt(t * 12f) % 2 == 0) _sprite.color = new Color(1f, 0.92f, 0.45f);
                    yield return null;
                }


                // Com isca ela vai ate o pilar; sem, passa do alvo e some na outra parede.
                float paredeX = deEsquerda ? maxX : minX;
                float paradaX = isca != null ? isca.transform.position.x - direcao * 2.2f : paredeX;

                float velocidade = Frenetico ? 17f : 13f;
                float x = inicioX;

                _carregando = true;

                while (!Mathf.Approximately(x, paradaX))
                {
                    x = Mathf.MoveTowards(x, paradaX, velocidade * Time.deltaTime);
                    transform.position = new Vector3(x, ChaoY, 0f);

                    if (Time.frameCount % 6 == 0) Estrondo(new Vector2(x, ChaoY + 1f), 7f, 0.2f);
                    yield return null;
                }

                // Chegou no pilar: so crava se ele ainda estiver aceso. Apagado,
                // ela nem percebe — passa direto e some na parede.
                if (isca != null && !isca.EstaAceso)
                {
                    isca.Mirado = false;
                    isca = null;

                    while (!Mathf.Approximately(x, paredeX))
                    {
                        x = Mathf.MoveTowards(x, paredeX, velocidade * Time.deltaTime);
                        transform.position = new Vector3(x, ChaoY, 0f);

                        if (Time.frameCount % 6 == 0) Estrondo(new Vector2(x, ChaoY + 1f), 7f, 0.2f);
                        yield return null;
                    }
                }

                _carregando = false;

                if (isca != null)
                {
                    // Cravou no pilar: ele estilhaca pra frente, o chao solta
                    // traças e ela fica presa com o motor a mostra.
                    isca.Mirado = false;
                    Estrondo(new Vector2(x, ChaoY + 1f), 18f, 0.2f);
                    Sfx.EmpalarBroca.Tocar(new Vector2(x, ChaoY + 1f));
                    LiberarCrias(x);

                    _expostoAte = Time.time + (Frenetico ? tempoExposto * 0.8f : tempoExposto);
                    yield return RachadurasDoPilar(isca, direcao);
                    while (EstaExposto) yield return null;

                    yield return TremerAteSoltar(x);
                }
            }
        }

        /// <summary>
        /// Fura pra cima, do chao, onde ouviu o Scrap. Antes, as lascas pulam
        /// e o cristal da sala acende com a vibracao. Na furia sao duas
        /// seguidas, a segunda mirando onde ele estiver.
        /// </summary>
        private IEnumerator EntrarEmErupcao()
        {
            int rajadas = Frenetico ? 2 : 1;

            for (int i = 0; i < rajadas; i++)
            {
                float at = i == 0 || _jogador == null ? _ouvidoX : _jogador.position.x;
                // Ate as paredes: nao ha canto da camada onde ela nao fure.
                at = Mathf.Clamp(at, minX, maxX);

                // O aviso: o chao estala e cospe lascas onde ela vai sair.
                float alerta = Frenetico ? 0.65f : 0.85f;
                float proximoPedrinha = 0f;
                Sfx.TremorBroca.Tocar(new Vector2(at, ChaoY), tom2: 0.85f / alerta);
                for (float t = 0f; t < alerta; t += Time.deltaTime)
                {
                    if (t >= proximoPedrinha)
                    {
                        proximoPedrinha = t + 0.08f;
                        DrillPebble.Saltitar(new Vector2(at + Random.Range(-1f, 1f), ChaoY + 0.1f), spriteEntulho);
                    }

                    if (Time.frameCount % 6 == 0) Estrondo(new Vector2(at, ChaoY + 0.5f), 5f, 0.2f);
                    yield return null;
                }

                // De pe: a arte olha pra direita, entao girada 90 graus ela
                // aponta pra cima com o corpo pro lado do pivo.
                Mostrar(at + AlturaMetade, 1);
                transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                _carregando = true;
                Estrondo(new Vector2(at, ChaoY + 1f), 14f, 0.3f);
                Sfx.ErupcaoBroca.Tocar(new Vector2(at, ChaoY + 1f));

                // Comeca com a metade de baixo dentro da laje (2 tiles de
                // grossura): mais fundo, apareceria no vao da camada de baixo.
                float enterradoY = ChaoY;
                float cimaY = ChaoY + 2.1f;

                for (float t = 0f; t < 0.12f; t += Time.deltaTime)
                {
                    transform.position = new Vector3(at + AlturaMetade, Mathf.Lerp(enterradoY, cimaY, t / 0.12f), 0f);
                    yield return null;
                }

                // Um instante no alto, tremendo, e volta pro buraco.
                for (float t = 0f; t < 0.4f; t += Time.deltaTime)
                {
                    transform.position = new Vector3(at + AlturaMetade + Mathf.Sin(t * 70f) * 0.06f, cimaY, 0f);
                    yield return null;
                }

                for (float t = 0f; t < 0.2f; t += Time.deltaTime)
                {
                    transform.position = new Vector3(at + AlturaMetade, Mathf.Lerp(cimaY, enterradoY, t / 0.2f), 0f);
                    yield return null;
                }

                Esconder();
                _ouvidoX = at;
                yield return new WaitForSeconds(0.2f);
            }
        }

        /// <summary>O pilar cravado treme um instante (o aviso) e estilhaca pra frente.</summary>
        private IEnumerator RachadurasDoPilar(ResonantCrystal pilar, int direcao)
        {
            _tremendo = pilar.transform;
            _origemTremendo = _tremendo.position;
            Sfx.RachaduraPilarBroca.Tocar(_origemTremendo);

            for (float t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                _tremendo.position = _origemTremendo + Vector3.right * (Mathf.Sin(t * 80f) * 0.07f);
                yield return null;
            }

            PararTremor();
            Estilhacos(_origemTremendo.x, direcao);
        }

        private void PararTremor()
        {
            if (_tremendo != null) _tremendo.position = _origemTremendo;
            _tremendo = null;
        }

        /// <summary>O pilar cravado estilhaca pra frente: uma lasca rente ao chao (pula-se) e uma alta.</summary>
        private void Estilhacos(float pilarX, int direcao)
        {
            if (spriteEntulho == null) return;

            var de = new Vector2(pilarX + direcao * 0.6f, ChaoY + 0.5f);
            Sfx.EstilhacoPilar.Tocar(de);
            ShardShot.Disparar(de, new Vector2(direcao * 12f, 0f), 10f, spriteEntulho);
            ShardShot.Disparar(de, new Vector2(direcao, 1f).normalized * 12f, 8f, spriteEntulho);
        }

        /// <summary>Se solta do pilar: ronca em brasa, e gira — quem ficou colado apanha.</summary>
        private IEnumerator TremerAteSoltar(float x)
        {
            Sfx.LivreTremorBroca.Tocar(new Vector2(x, ChaoY + 1f));

            for (float t = 0f; t < 0.45f; t += Time.deltaTime)
            {
                transform.position = new Vector3(x + Mathf.Sin(t * 90f) * 0.1f, ChaoY, 0f);
                if (_sprite != null) _sprite.color = Mathf.FloorToInt(t * 16f) % 2 == 0 ? new Color(1f, 0.5f, 0.25f) : Color.white;
                if (Time.frameCount % 5 == 0) Estrondo(new Vector2(x, ChaoY + 1f), 6f, 0.2f);
                yield return null;
            }

            transform.position = new Vector3(x, ChaoY, 0f);
            Estrondo(new Vector2(x, ChaoY + 1f), 14f, 0.3f);

            if (_vidaSucata != null && _jogador != null)
            {
                Vector2 deslocamento = _jogador.position - transform.position;
                if (Mathf.Abs(deslocamento.x) < 3f && deslocamento.y > -0.5f && deslocamento.y < 2.6f)
                    _vidaSucata.LevarDano(1, transform.position + Vector3.up);
            }

            yield return new WaitForSeconds(0.3f);
        }

        /// <summary>Perdeu o controle: fura o chao, e a arena desce uma camada.</summary>
        private IEnumerator Desabar()
        {
            _carregando = false;
            _escutando = false;
            _expostoAte = 0f;

            float x = transform.position.x;
            Mostrar(x, 1);
            Sfx.ColapsoBroca.Tocar(new Vector2(x, ChaoY));

            // Ela gira no lugar, apontada pra baixo. E o tempo de entender o que vem.
            for (float t = 0f; t < 1.3f; t += Time.deltaTime)
            {
                transform.position = new Vector3(x + Mathf.Sin(t * 80f) * 0.12f, ChaoY, 0f);
                if (_sprite != null) _sprite.color = new Color(0.55f, 1f, 0.65f);
                if (Time.frameCount % 8 == 0) Estrondo(new Vector2(x, ChaoY + 1f), 30f, 0.2f);
                yield return null;
            }

            if (lajes != null && _camadaVisual < lajes.Length && lajes[_camadaVisual] != null) lajes[_camadaVisual].SetActive(false);

            // O chao foi embora com as traças em cima.
            RecolocarCrias();

            _camadaVisual++;
            Esconder();
            transform.position = new Vector3(x, ChaoY, 0f);
            _ouvidoX = x;

            // O tempo de cair e se achar la embaixo.
            yield return new WaitForSeconds(1.6f);

            if (_jogador != null) _ouvidoX = _jogador.position.x;
            StartCoroutine(Cerebro());
        }

        // ------------------------------------------------------------------
        // Apanhar e parar
        // ------------------------------------------------------------------

        public void ReceberDano(int quantia, Vector2 de)
        {
            if (_morto || !_acordado) return;

            // Blindada: fora do pilar o golpe so faz barulho.
            if (!EstaExposto)
            {
                if (_sprite != null) _sprite.color = new Color(0.7f, 0.8f, 1f);
                Sfx.QuadroBloqueio.Tocar(transform.position, tom2: 0.8f);
                return;
            }

            Sfx.FeridoBroca.Tocar(transform.position);

            // Cada terco de vida e uma camada: um golpe forte nao pula nenhuma.
            int camadas = chaos.Length;
            int limiar = vidaMax * (camadas - 1 - _camadaVisual) / camadas;

            _vida = Mathf.Max(_vida - quantia, limiar);
            if (_sprite != null) _sprite.color = new Color(1f, 0.35f, 0.3f);

            if (_vida > limiar) return;

            StopAllCoroutines();
            PararTremor();
            LimparAlvos();

            if (_vida <= 0) Morrer();
            else StartCoroutine(Desabar());
        }

        private void Morrer()
        {
            _morto = true;
            _derrotado = true;
            _expostoAte = 0f;

            if (Lutando == this) Lutando = null;

            Vector3 lugar = new Vector3(transform.position.x, ChaoY, 0f);
            Sfx.MorrerBroca.Tocar(lugar);

            for (int i = 0; i < 20; i++)
            {
                ShardPickup.Criar(lugar + Vector3.up * 0.2f, spriteFragmento, 1, i);
            }

            RecolocarCrias();
            VirarDestrocos(lugar);
            SoltarRecompensa();

            // O ultimo giro acende a arena inteira, e o distrito acorda.
            NoiseSystem.Emitir(lugar + Vector3.up, 40f, 0.2f);
            if (purificador != null) purificador.Purificar(lugar);
        }

        /// <summary>Parada: vira carcaca, nao machuca mais, e a placa do chassi aparece.</summary>
        private void VirarDestrocos(Vector3 lugar)
        {
            transform.position = lugar;

            if (destrocos != null)
            {
                // A carcaca desenhada toma o lugar dela: a broca arrancada, a esteira partida, fumaca.
                if (_sprite != null) _sprite.enabled = false;
                var movimento = GetComponent<MotionFrames>();
                if (movimento != null) movimento.enabled = false;
                BossWreck.Mostrar(destrocos, lugar);
            }
            else if (_sprite != null)
            {
                _sprite.enabled = true;
                _sprite.color = new Color(0.55f, 0.55f, 0.62f);
            }

            if (_colisor != null) _colisor.enabled = false;
            if (placaTexto != null) placaTexto.SetActive(true);
        }

        private void SoltarRecompensa()
        {
            // Atras dela, pra nao ficar em cima da placa.
            RewardPickup.Soltar(transform.position + Vector3.left * 3.5f, spriteRecompensa,
                RewardPickup.Kind.GolpeRessonancia, Color.white);
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
            // Presa ela nao machuca: e a hora de chegar perto.
            if (_morto || !_acordado || EstaExposto) return;

            var vida = outro.GetComponentInParent<ScrapHealth>();
            if (vida != null) vida.LevarDano(_carregando ? 2 : 1, transform.position);
        }
    }
}
