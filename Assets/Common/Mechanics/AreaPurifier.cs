using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A purificacao da area: o que acontece com o distrito quando o mini boss
    /// cai. A nevoa do Oxide para, as luzes verdes se apagam, a luz geral muda,
    /// os lampioes acendem, o acido seca e os corrompidos somem.
    ///
    /// Ao vivo, e uma cena com camera dirigida:
    ///
    ///   1. O coracao. A camera fecha no ponto de onde a purificacao parte, o
    ///      mundo escurece e o Oxide da area inteira e sugado pra la — fios de
    ///      particulas verdes vindo de todo lado, girando — enquanto um nucleo
    ///      de luz cresce, pulsando, com a tela tremendo cada vez mais.
    ///   2. O estouro. Um clarao branco, um tremor forte, a ferrugem estourando
    ///      em lascas que caem e faiscas douradas, e uma onda de luz que se abre
    ///      em circulo a partir dali.
    ///   3. A onda. A camera abre devagar enquanto a onda corre o mapa, e cada
    ///      coisa muda quando a onda passa por ela — nao tudo de uma vez: os
    ///      verdes evaporam virando ouro, os lampioes acendem soltando faisca,
    ///      as correntes balancam, as engrenagens disparam, a nevoa para, o
    ///      acido seca, os corrompidos se desfazem, do centro pra fora.
    ///      Depois dela, uma poeira de luz cai devagar sobre tudo.
    ///   4. O passeio. A camera vai ate quem aparece com a purificacao (a
    ///      Solda, os mineiros) e mostra a chegada.
    ///   5. A volta. A camera volta pro Scrap e devolve o controle.
    ///
    /// O estado persiste: voltar a cena depois de purificada ja a encontra
    /// assim, sem cena. Ainda nao ha save, entao vale ate o jogo fechar.
    /// </summary>
    public class AreaPurifier : MonoBehaviour
    {
        [UnityEngine.Serialization.FormerlySerializedAs("globalLight")] [Header("Luz geral")]
        [SerializeField] private Light2D luzGlobal;
        [UnityEngine.Serialization.FormerlySerializedAs("purifiedColor")] [SerializeField] private Color corPurificado = new Color(1f, 0.90f, 0.70f);
        [UnityEngine.Serialization.FormerlySerializedAs("purifiedIntensity")] [SerializeField] private float intensidadePurificado = 0.95f;

        [UnityEngine.Serialization.FormerlySerializedAs("corruptedLights")] [Header("O que apaga e o que acende")]
        [SerializeField] private Light2D[] luzesCorrompida;
        [UnityEngine.Serialization.FormerlySerializedAs("purifiedLights")] [SerializeField] private Light2D[] luzesPurificado;
        [UnityEngine.Serialization.FormerlySerializedAs("oxideFog")] [SerializeField] private ParticleSystem[] nevoaOxido;

        [UnityEngine.Serialization.FormerlySerializedAs("tintedTilemaps")] [Header("Cenario")]
        [SerializeField] private Tilemap[] tilemapsTingido;
        [UnityEngine.Serialization.FormerlySerializedAs("purifiedTint")] [SerializeField] private Color tomPurificado = new Color(0.85f, 0.80f, 0.68f);
        [UnityEngine.Serialization.FormerlySerializedAs("viewCamera")] [SerializeField] private Camera cameraVista;
        [UnityEngine.Serialization.FormerlySerializedAs("purifiedBackground")] [SerializeField] private Color fundoPurificado = new Color(0.30f, 0.24f, 0.14f);

        [UnityEngine.Serialization.FormerlySerializedAs("appearWhenPurified")] [Header("O que aparece e o que some")]
        [SerializeField] private GameObject[] aparecerQuandoPurificado;
        [UnityEngine.Serialization.FormerlySerializedAs("vanishWhenPurified")] [SerializeField] private GameObject[] sumirQuandoPurificado;
        [UnityEngine.Serialization.FormerlySerializedAs("gatesToOpen")] [SerializeField] private Gate[] portoesParaAberto;

        [UnityEngine.Serialization.FormerlySerializedAs("poolsToDrain")] [Tooltip("Pocas de acido que secam com a purificacao.")]
        [SerializeField] private AcidPool[] pocasParaDreno;

        // As marcas da cerimonia, em segundos.
        private const float TempoCoracao = 2.2f;
        private const float TempoOnda = 4.4f;
        private const float ParadaPercurso = 1.6f;
        private const float TempoRetorno = 1.0f;
        private const int ParadasPercursoMax = 2;

        /// <summary>Quantos tiles a beirada da onda leva pra apagar ou acender uma luz depois de passar por ela.</summary>
        private const float MaciezOnda = 7f;

        private float[] _alvosPurificado;

        // Cada cena tem a sua purificacao; guarda-se o nome das que ja foram.
        [SaveProgress("Purified")]
        private static readonly HashSet<string> Purificado = new HashSet<string>();

        private bool _purificado;

        /// <summary>Se uma cena (pelo nome) ja foi purificada nesta sessao. O palacio (e o final) vem depois dos quatro distritos.</summary>
        public static bool FoiPurificado(string nomeCena) => Purificado.Contains(nomeCena);

        /// <summary>Marca uma cena como purificada sem cerimonia (pra testar o palacio dando Play direto nele).</summary>
        public static void MarcarPurificado(string nomeCena) => Purificado.Add(nomeCena);

        /// <summary>Se a cena aberta agora ja foi purificada nesta sessao.</summary>
        public static bool EstaPurificado =>
            Purificado.Contains(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        /// <summary>
        /// Quantos segundos a cerimonia leva, do coracao a volta da camera. Quem
        /// espera ela acabar (a cena do boss) usa.
        /// </summary>
        public float Duracao => TempoCoracao + TempoOnda + ParadasDoPercurso().Count * ParadaPercurso + TempoRetorno;

        /// <summary>Quando a onda terminou de correr o mapa. O HUD usa pra mostrar o aviso.</summary>
        public static float TempoPurificado { get; private set; } = -100f;

        /// <summary>
        /// Avisa que a cena aberta acabou de ser purificada. Serve pra quem tem
        /// um jeito proprio de reagir, como os cristais do District 3.
        /// </summary>
        public static event System.Action Purificando;

        // O projeto entra em Play sem recarregar o dominio, entao estatico nao
        // zera sozinho entre uma sessao e outra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            Purificado.Clear();
            TempoPurificado = -100f;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configurar(Light2D global, Light2D[] corrompida, Light2D[] purificado, ParticleSystem[] nevoa,
            Tilemap[] tilemaps, Camera camera, GameObject[] aparecer, GameObject[] sumir, Gate[] portoes)
        {
            luzGlobal = global;
            luzesCorrompida = corrompida;
            luzesPurificado = purificado;
            nevoaOxido = nevoa;
            tilemapsTingido = tilemaps;
            cameraVista = camera;
            aparecerQuandoPurificado = aparecer;
            sumirQuandoPurificado = sumir;
            portoesParaAberto = portoes;
        }

        /// <summary>Usado pelos geradores de fase, pra distritos que nao ficam dourados.</summary>
        public void ConfigurarVisual(Color corLuz, float intensidadeLuz, Color tomTilemap, Color fundo)
        {
            corPurificado = corLuz;
            intensidadePurificado = intensidadeLuz;
            tomPurificado = tomTilemap;
            fundoPurificado = fundo;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void ConfigurarPocas(AcidPool[] pocas)
        {
            pocasParaDreno = pocas;
        }

        private void Awake()
        {
            // Os lampioes ficam apagados ate a purificacao; a intensidade que o
            // gerador deu a cada um e ate onde eles acendem.
            _alvosPurificado = new float[luzesPurificado != null ? luzesPurificado.Length : 0];

            for (int i = 0; i < _alvosPurificado.Length; i++)
            {
                if (luzesPurificado[i] == null) continue;

                _alvosPurificado[i] = luzesPurificado[i].intensity;
                luzesPurificado[i].intensity = 0f;
            }

            DefinirAtivos(aparecerQuandoPurificado, false);
        }

        private void Start()
        {
            // Ja purificada numa visita anterior: aplica tudo de uma vez, sem cena.
            if (EstaPurificado)
            {
                _purificado = true;
                AplicarTudo();
            }
        }

        /// <summary>Dispara a purificacao partindo de onde o Scrap esta. Chamar de novo nao faz nada.</summary>
        public void Purificar()
        {
            var sucata = ScrapRef.Controlador;
            Purificar(sucata != null ? sucata.transform.position : transform.position);
        }

        /// <summary>Dispara a purificacao partindo de source (onde o boss caiu). Chamar de novo nao faz nada.</summary>
        public void Purificar(Vector3 fonte)
        {
            if (_purificado) return;

            _purificado = true;
            Purificado.Add(gameObject.scene.name);
            Purificando?.Invoke();
            Sfx.OndaPurificar.Tocar();
            StartCoroutine(Cerimonia(fonte));
        }

        // ------------------------------------------------------------------
        // A cerimonia
        // ------------------------------------------------------------------

        /// <summary>O que a onda vai mudando, cada coisa com a distancia dela ate o centro.</summary>
        private sealed class Wave
        {
            public Vector2 Centro;
            public float Raio;
            public float RaioMax;

            public readonly List<(Light2D Light, float From, float Distance)> Esmaecendo = new List<(Light2D, float, float)>();
            public readonly List<(Light2D Light, float Target, float Distance)> Subindo = new List<(Light2D, float, float)>();
            public readonly List<(ParticleSystem Fog, float Distance)> Nevoa = new List<(ParticleSystem, float)>();
            public readonly List<(GameObject Target, float Distance)> Sumir2 = new List<(GameObject, float)>();
            public readonly List<(GameObject Target, float Distance)> Aparecer = new List<(GameObject, float)>();
            public readonly List<(AcidPool Pool, float Distance)> Pocas = new List<(AcidPool, float)>();
            public readonly List<(Sway Swing, float Distance)> Balancos = new List<(Sway, float)>();
            public readonly List<(SlowSpin Gear, float Distance)> Engrenagens = new List<(SlowSpin, float)>();
        }

        private static readonly Color ParticulaOxido = new Color(0.45f, 1f, 0.62f);
        private static readonly Color LuzFerrugem = new Color(0.78f, 0.46f, 0.24f);
        private static readonly Color EscuroFerrugem = new Color(0.40f, 0.21f, 0.13f);
        private Color Brilho => Color.Lerp(corPurificado, Color.white, 0.55f);

        private IEnumerator Cerimonia(Vector3 fonte)
        {
            var controlador = ScrapRef.Controlador;
            bool comecou = !Cinematic.Ativo;
            if (comecou) Cinematic.Iniciar();

            void Hold()
            {
                if (controlador != null) controlador.ControleTravado = true;
            }

            Vector2 centro = (Vector2)fonte + Vector2.up * 1.2f;
            Wave onda = ConstruirOnda(centro);

            Color deGlobal = luzGlobal != null ? luzGlobal.color : Color.white;
            float deIntensidade = luzGlobal != null ? luzGlobal.intensity : 1f;
            Color deFundo = cameraVista != null ? cameraVista.backgroundColor : Color.black;
            Color[] deTons = TonsAtuais();

            // 1. O coracao: a camera fecha, o mundo escurece e o nucleo cresce pulsando.
            ScrapCameraFollow.Direcionar(centro, 0.6f, 0.45f);
            Light2D nucleo = CriarLuz("PurificationCore", centro, Color.Lerp(corPurificado, Color.white, 0.5f));

            for (float t = 0f; t < TempoCoracao; t += Time.deltaTime)
            {
                Hold();
                float k = t / TempoCoracao;

                if (luzGlobal != null) luzGlobal.intensity = Mathf.Lerp(deIntensidade, deIntensidade * 0.45f, k);

                float batida = 1f + 0.35f * Mathf.Sin(t * Mathf.Lerp(8f, 22f, k));
                nucleo.intensity = Mathf.Lerp(0.2f, 3.2f, k * k) * batida;
                nucleo.pointLightOuterRadius = Mathf.Lerp(1f, 6f, k);

                // O tremor cresce junto, em batidas.
                if (Mathf.Repeat(t, 0.35f) < Time.deltaTime) ScrapCameraFollow.Tremer(0.03f + 0.08f * k, 0.25f);

                // O Oxide da area sendo sugado pro coracao: cada vez mais, e
                // chegando antes do estouro.
                float viagem = Mathf.Lerp(1.1f, 0.45f, k);
                if (t + viagem < TempoCoracao) Inspirar(onda, centro, Mathf.Lerp(40f, 170f, k) * Time.deltaTime, viagem);

                yield return null;
            }

            // 2. O estouro: a ferrugem arrancada em lascas que caem, faiscas
            //    douradas e o resto do verde se desfazendo.
            Cinematic.Clarao2 = 1f;
            ScrapCameraFollow.Tremer(0.45f, 1.1f);
            Motes.Estourar(centro, 150, LuzFerrugem, EscuroFerrugem, 3f, 10f, -12f, 1.9f, 2f, 0.8f);
            Motes.Estourar(centro, 90, Color.white, corPurificado, 6f, 14f, -2f, 1.1f, 1f, 1.4f);
            Motes.Estourar(centro, 40, ParticulaOxido, corPurificado, 2f, 6f, 1f, 1.4f, 1f, 1f);
            SpriteRenderer anel = CriarAnel(centro);
            Light2D frente = CriarLuz("PurificationFront", centro, corPurificado);

            // 3. A onda: a camera abre devagar enquanto ela corre o mapa.
            ScrapCameraFollow.Direcionar(centro, 1.75f, 1.1f);

            for (float t = 0f; t < TempoOnda; t += Time.deltaTime)
            {
                Hold();
                float k = t / TempoOnda;

                // A onda corre rapido no comeco e desacelera.
                onda.Raio = onda.RaioMax * (1f - (1f - k) * (1f - k));
                Espalhar(onda);

                float visual = Mathf.SmoothStep(0f, 1f, k);
                AplicarGlobal(visual, deGlobal, deIntensidade * 0.45f, deFundo, deTons);

                Cinematic.Clarao2 = Mathf.Max(0f, 1f - t * 2.2f);
                nucleo.intensity = Mathf.Lerp(3.2f, 0f, Mathf.Clamp01(k * 2.5f));

                if (anel != null)
                {
                    anel.transform.localScale = Vector3.one * Mathf.Max(0.1f, onda.Raio);
                    Color cor = anel.color;
                    cor.a = Mathf.Lerp(0.85f, 0f, k);
                    anel.color = cor;
                }

                // Uma luz acompanha a beirada da onda, pra o mundo acender por onde ela passa.
                frente.pointLightOuterRadius = Mathf.Max(2f, onda.Raio + 3f);
                frente.pointLightInnerRadius = Mathf.Max(0f, onda.Raio - 4f);
                frente.intensity = Mathf.Lerp(1.4f, 0f, k);

                yield return null;
            }

            Cinematic.Clarao2 = 0f;
            onda.Raio = float.MaxValue;
            Espalhar(onda);

            // Os corrompidos sumiram um por um; agora some o grupo inteiro, pra
            // ninguem voltar quando o Scrap quebrar (eles revivem sozinhos).
            DefinirAtivos(sumirQuandoPurificado, false);
            AplicarGlobal(1f, deGlobal, deIntensidade * 0.45f, deFundo, deTons);
            AbrirPortoes();

            // O aviso do HUD ("Area purificada") entra quando a onda termina.
            TempoPurificado = Time.time;

            // A poeira de luz que fica caindo, mesmo depois de a camera voltar.
            StartCoroutine(BrilhoFinal(7f));

            Destroy(nucleo.gameObject);
            Destroy(frente.gameObject);
            if (anel != null) Destroy(anel.gameObject);

            // 4. O passeio: quem apareceu com a purificacao.
            foreach (Vector2 parada in ParadasDoPercurso())
            {
                ScrapCameraFollow.Direcionar(parada + Vector2.up * 1.5f, 0.8f, 0.55f);
                StartCoroutine(Brilhar(parada));

                for (float t = 0f; t < ParadaPercurso; t += Time.deltaTime)
                {
                    Hold();
                    yield return null;
                }
            }

            // 5. A volta.
            if (controlador != null)
            {
                ScrapCameraFollow.Direcionar((Vector2)controlador.transform.position + Vector2.up * 1.5f, 1f, 0.45f);
            }

            for (float t = 0f; t < TempoRetorno; t += Time.deltaTime)
            {
                Hold();
                yield return null;
            }

            ScrapCameraFollow.Liberar();

            if (comecou)
            {
                if (controlador != null) controlador.ControleTravado = false;
                Cinematic.Encerrar();
            }
        }

        /// <summary>Mede a distancia de cada coisa ate o centro, e o raio que a onda precisa cobrir.</summary>
        private Wave ConstruirOnda(Vector2 centro)
        {
            var onda = new Wave { Centro = centro };
            float maisLonge = 20f;

            float Measure(Vector3 posicao)
            {
                float distancia = Vector2.Distance(centro, posicao);
                maisLonge = Mathf.Max(maisLonge, distancia);
                return distancia;
            }

            if (luzesCorrompida != null)
            {
                foreach (Light2D luz in luzesCorrompida)
                {
                    if (luz != null) onda.Esmaecendo.Add((luz, luz.intensity, Measure(luz.transform.position)));
                }
            }

            for (int i = 0; i < _alvosPurificado.Length; i++)
            {
                if (luzesPurificado[i] != null) onda.Subindo.Add((luzesPurificado[i], _alvosPurificado[i], Measure(luzesPurificado[i].transform.position)));
            }

            if (nevoaOxido != null)
            {
                foreach (ParticleSystem nevoa in nevoaOxido)
                {
                    if (nevoa != null) onda.Nevoa.Add((nevoa, Measure(nevoa.transform.position)));
                }
            }

            // O que some: os filhos um por um (os corrompidos vem todos num pai so).
            if (sumirQuandoPurificado != null)
            {
                foreach (GameObject alvo in sumirQuandoPurificado)
                {
                    if (alvo == null) continue;

                    if (alvo.transform.childCount == 0)
                    {
                        onda.Sumir2.Add((alvo, Measure(alvo.transform.position)));
                        continue;
                    }

                    foreach (Transform filho in alvo.transform)
                    {
                        if (filho.gameObject.activeSelf) onda.Sumir2.Add((filho.gameObject, Measure(filho.position)));
                    }
                }
            }

            if (aparecerQuandoPurificado != null)
            {
                foreach (GameObject alvo in aparecerQuandoPurificado)
                {
                    if (alvo != null) onda.Aparecer.Add((alvo, Measure(FocoDe(alvo))));
                }
            }

            if (pocasParaDreno != null)
            {
                foreach (AcidPool poca in pocasParaDreno)
                {
                    if (poca != null) onda.Pocas.Add((poca, Measure(poca.transform.position)));
                }
            }

            // O que balanca e o que gira no cenario reage quando a onda passa.
            foreach (Sway balancoGolpe in FindObjectsByType<Sway>(FindObjectsSortMode.None))
            {
                onda.Balancos.Add((balancoGolpe, Vector2.Distance(centro, balancoGolpe.transform.position)));
            }

            foreach (SlowSpin engrenagem in FindObjectsByType<SlowSpin>(FindObjectsSortMode.None))
            {
                onda.Engrenagens.Add((engrenagem, Vector2.Distance(centro, engrenagem.transform.position)));
            }

            onda.RaioMax = Mathf.Clamp(maisLonge + 6f, 24f, 220f);
            return onda;
        }

        /// <summary>Aplica a onda ate o raio atual: cada coisa muda conforme a beirada passa por ela.</summary>
        private void Espalhar(Wave onda)
        {
            float r = onda.Raio;

            foreach ((Light2D luz, float de, float distancia) in onda.Esmaecendo)
            {
                if (luz == null) continue;
                bool estavaAceso = luz.intensity > de * 0.98f;
                luz.intensity = de * (1f - Mathf.Clamp01((r - distancia) / MaciezOnda));

                // O verde evapora virando ouro, na hora que a beirada passa.
                if (estavaAceso && r >= distancia) Motes.Subir(luz.transform.position, 18, ParticulaOxido, corPurificado, 1f, 2f);
            }

            foreach ((Light2D luz, float alvo, float distancia) in onda.Subindo)
            {
                if (luz == null) continue;
                bool estavaDesligado = luz.intensity <= 0.001f;
                luz.intensity = alvo * Mathf.Clamp01((r - distancia) / MaciezOnda);

                // O lampiao acende soltando faisca.
                if (estavaDesligado && r >= distancia) Motes.Estourar(luz.transform.position, 12, Color.white, Brilho, 1f, 3.5f, -4f, 0.9f, 1f, 1f);
            }

            for (int i = onda.Balancos.Count - 1; i >= 0; i--)
            {
                (Sway balancoGolpe, float distancia) = onda.Balancos[i];
                if (r < distancia) continue;
                if (balancoGolpe != null) balancoGolpe.Impulsionar(14f);
                onda.Balancos.RemoveAt(i);
            }

            for (int i = onda.Engrenagens.Count - 1; i >= 0; i--)
            {
                (SlowSpin engrenagem, float distancia) = onda.Engrenagens[i];
                if (r < distancia) continue;
                if (engrenagem != null)
                {
                    engrenagem.Impulsionar(9f);
                    Motes.Estourar(engrenagem.transform.position, 8, Brilho, corPurificado, 1f, 2.5f, -6f, 0.8f, 1f, 1f);
                }

                onda.Engrenagens.RemoveAt(i);
            }

            for (int i = onda.Nevoa.Count - 1; i >= 0; i--)
            {
                (ParticleSystem nevoa, float distancia) = onda.Nevoa[i];
                if (r < distancia) continue;

                // Para de nascer nevoa; a que ja existe se desfaz sozinha.
                if (nevoa != null) nevoa.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                onda.Nevoa.RemoveAt(i);
            }

            for (int i = onda.Sumir2.Count - 1; i >= 0; i--)
            {
                (GameObject alvo, float distancia) = onda.Sumir2[i];
                if (r < distancia) continue;

                if (alvo != null && alvo.activeInHierarchy)
                {
                    StartCoroutine(Baforada(alvo.transform.position + Vector3.up * 0.6f, new Color(0.55f, 1f, 0.7f)));
                    Motes.Estourar((Vector2)alvo.transform.position + Vector2.up * 0.6f, 24, LuzFerrugem, EscuroFerrugem, 1.5f, 4.5f, -10f, 1.2f, 2f, 0.8f);
                    Motes.Subir(alvo.transform.position, 10, ParticulaOxido, corPurificado, 0.5f, 1.4f);
                    alvo.SetActive(false);
                }

                onda.Sumir2.RemoveAt(i);
            }

            for (int i = onda.Aparecer.Count - 1; i >= 0; i--)
            {
                (GameObject alvo, float distancia) = onda.Aparecer[i];
                if (r < distancia) continue;

                if (alvo != null)
                {
                    alvo.SetActive(true);
                    StartCoroutine(Baforada((Vector3)FocoDe(alvo) + Vector3.up, corPurificado));
                }

                onda.Aparecer.RemoveAt(i);
            }

            for (int i = onda.Pocas.Count - 1; i >= 0; i--)
            {
                (AcidPool poca, float distancia) = onda.Pocas[i];
                if (r < distancia) continue;

                if (poca != null)
                {
                    poca.Drenar();
                    Motes.Subir(poca.transform.position, 20, ParticulaOxido, corPurificado, 1.5f, 2f);
                }

                onda.Pocas.RemoveAt(i);
            }
        }

        /// <summary>Os pontos que a camera visita no passeio: quem aparece com a purificacao.</summary>
        private List<Vector2> ParadasDoPercurso()
        {
            var paradas = new List<Vector2>();
            if (aparecerQuandoPurificado == null) return paradas;

            foreach (GameObject alvo in aparecerQuandoPurificado)
            {
                if (alvo == null || paradas.Count >= ParadasPercursoMax) continue;
                paradas.Add(FocoDe(alvo));
            }

            return paradas;
        }

        /// <summary>Onde uma coisa "esta": ela mesma, ou o primeiro filho, se ela e so um agrupador.</summary>
        private static Vector2 FocoDe(GameObject alvo)
        {
            if (alvo.transform.childCount > 0 && alvo.transform.position == Vector3.zero)
            {
                Vector3 soma = Vector3.zero;
                foreach (Transform filho in alvo.transform) soma += filho.position;
                return soma / alvo.transform.childCount;
            }

            return alvo.transform.position;
        }

        // ------------------------------------------------------------------
        // Efeitos da cerimonia
        // ------------------------------------------------------------------

        /// <summary>
        /// O Oxide sendo sugado pro coracao: particulas verdes saindo das
        /// luzes verdes por perto (de onde o Oxide esta) e de qualquer canto em
        /// volta, todas chegando ao centro em travel segundos. amount e
        /// quantas soltar neste quadro (a fracao vira chance).
        /// </summary>
        private void Inspirar(Wave onda, Vector2 centro, float quantia, float viagem)
        {
            int quantidade = (int)quantia + (Random.value < quantia - (int)quantia ? 1 : 0);
            for (int n = 0; n < quantidade; n++)
            {
                Vector2 de;
                if (onda.Esmaecendo.Count > 0 && Random.value < 0.45f)
                {
                    (Light2D luz, float _, float distancia) = onda.Esmaecendo[Random.Range(0, onda.Esmaecendo.Count)];
                    de = luz != null && distancia < 22f
                        ? (Vector2)luz.transform.position + Random.insideUnitCircle * 1.2f
                        : centro + Random.insideUnitCircle.normalized * Random.Range(7f, 14f);
                }
                else
                {
                    de = centro + Random.insideUnitCircle.normalized * Random.Range(6f, 14f);
                }

                Motes.Atrair(de, centro, ParticulaOxido, Color.Lerp(ParticulaOxido, Color.white, 0.6f), viagem, Random.value < 0.3f ? 2f : 1f);
            }
        }

        /// <summary>A poeira de luz caindo devagar em volta da camera, rareando ate parar.</summary>
        private IEnumerator BrilhoFinal(float segundos)
        {
            Camera camera = cameraVista != null ? cameraVista : Camera.main;
            float levar = 0f;

            for (float t = 0f; t < segundos; t += Time.deltaTime)
            {
                if (camera == null) yield break;

                float altura = camera.orthographicSize * 2f;
                float largura = altura * camera.aspect;
                Vector3 c = camera.transform.position;
                var area = new Rect(c.x - largura * 0.5f, c.y + altura * 0.1f, largura, altura * 0.45f);

                levar += Mathf.Lerp(30f, 0f, t / segundos) * Time.deltaTime;
                int quantidade = (int)levar;
                levar -= quantidade;
                if (quantidade > 0) Motes.Flutuar(area, quantidade, Brilho, corPurificado, 4f);

                yield return null;
            }
        }

        private Light2D CriarLuz(string nome, Vector2 posicao, Color cor)
        {
            var objetoLuz = new GameObject(nome);
            objetoLuz.transform.SetParent(transform, false);
            objetoLuz.transform.position = new Vector3(posicao.x, posicao.y, 0f);

            var luz = objetoLuz.AddComponent<Light2D>();
            luz.lightType = Light2D.LightType.Point;
            luz.color = cor;
            luz.intensity = 0f;
            luz.pointLightInnerRadius = 0f;
            luz.pointLightOuterRadius = 1f;
            return luz;
        }

        private static Sprite _spriteAnel;
        private static Material _sem;

        /// <summary>A onda: um anel claro, desenhado aqui mesmo, que cresce a partir do centro.</summary>
        private SpriteRenderer CriarAnel(Vector2 centro)
        {
            if (_spriteAnel == null)
            {
                const int tamanho = 256;
                var textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
                var pixels = new Color32[tamanho * tamanho];

                for (int y = 0; y < tamanho; y++)
                {
                    for (int x = 0; x < tamanho; x++)
                    {
                        float dx = (x + 0.5f) / tamanho * 2f - 1f;
                        float dy = (y + 0.5f) / tamanho * 2f - 1f;
                        float d = Mathf.Sqrt(dx * dx + dy * dy);

                        // Uma beirada forte por fora e um veu fraco por dentro.
                        float beirada = Mathf.Clamp01(1f - Mathf.Abs(d - 0.95f) / 0.05f);
                        float veu = d < 0.95f ? Mathf.Pow(d, 6f) * 0.35f : 0f;
                        byte alfa = (byte)(Mathf.Clamp01(beirada + veu) * 255f);
                        pixels[x + y * tamanho] = new Color32(255, 255, 255, alfa);
                    }
                }

                textura.SetPixels32(pixels);
                textura.Apply();

                // Diametro de 2 unidades: a escala vira o raio.
                _spriteAnel = Sprite.Create(textura, new Rect(0, 0, tamanho, tamanho), new Vector2(0.5f, 0.5f), tamanho / 2f);
            }

            if (_sem == null)
            {
                Shader sombreador = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (sombreador != null) _sem = new Material(sombreador);
            }

            var objetoAnel = new GameObject("PurificationWave");
            objetoAnel.transform.SetParent(transform, false);
            objetoAnel.transform.position = new Vector3(centro.x, centro.y, 0f);

            var anel = objetoAnel.AddComponent<SpriteRenderer>();
            anel.sprite = _spriteAnel;
            anel.sortingLayerName = "Actors";
            anel.sortingOrder = 40;
            anel.color = new Color(corPurificado.r, corPurificado.g, corPurificado.b, 0.85f);
            if (_sem != null) anel.sharedMaterial = _sem;
            return anel;
        }

        /// <summary>Um brilho rapido num ponto: um corrompido se desfazendo, ou alguem chegando.</summary>
        private IEnumerator Baforada(Vector3 posicao, Color cor)
        {
            Light2D luz = CriarLuz("PurificationPuff", posicao, cor);

            for (float t = 0f; t < 0.6f; t += Time.deltaTime)
            {
                float k = t / 0.6f;
                luz.intensity = Mathf.Sin(k * Mathf.PI) * 1.8f;
                luz.pointLightOuterRadius = Mathf.Lerp(1f, 3.5f, k);
                yield return null;
            }

            Destroy(luz.gameObject);
        }

        /// <summary>O brilho em quem a camera visita no passeio.</summary>
        private IEnumerator Brilhar(Vector2 posicao)
        {
            Light2D luz = CriarLuz("PurificationSparkle", posicao + Vector2.up, Color.Lerp(corPurificado, Color.white, 0.4f));

            for (float t = 0f; t < ParadaPercurso; t += Time.deltaTime)
            {
                float k = t / ParadaPercurso;
                luz.intensity = Mathf.Sin(k * Mathf.PI) * 1.4f;
                luz.pointLightOuterRadius = 4f + Mathf.Sin(t * 6f) * 0.5f;
                yield return null;
            }

            Destroy(luz.gameObject);
        }

        // ------------------------------------------------------------------
        // O estado final
        // ------------------------------------------------------------------

        /// <summary>Tudo de uma vez: pra cena que ja chega purificada.</summary>
        private void AplicarTudo()
        {
            if (nevoaOxido != null)
            {
                foreach (ParticleSystem nevoa in nevoaOxido)
                {
                    if (nevoa != null) nevoa.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }

            DefinirAtivos(sumirQuandoPurificado, false);

            if (pocasParaDreno != null)
            {
                foreach (AcidPool poca in pocasParaDreno)
                {
                    if (poca != null) poca.Drenar();
                }
            }

            if (luzesCorrompida != null)
            {
                foreach (Light2D luz in luzesCorrompida)
                {
                    if (luz != null) luz.intensity = 0f;
                }
            }

            for (int i = 0; i < _alvosPurificado.Length; i++)
            {
                if (luzesPurificado[i] != null) luzesPurificado[i].intensity = _alvosPurificado[i];
            }

            AplicarGlobal(1f, luzGlobal != null ? luzGlobal.color : Color.white,
                luzGlobal != null ? luzGlobal.intensity : 1f,
                cameraVista != null ? cameraVista.backgroundColor : Color.black, TonsAtuais());

            DefinirAtivos(aparecerQuandoPurificado, true);
            AbrirPortoes();
        }

        private Color[] TonsAtuais()
        {
            var tons = new Color[tilemapsTingido != null ? tilemapsTingido.Length : 0];
            for (int i = 0; i < tons.Length; i++)
            {
                tons[i] = tilemapsTingido[i] != null ? tilemapsTingido[i].color : Color.white;
            }

            return tons;
        }

        private void AplicarGlobal(float k, Color deGlobal, float deIntensidade, Color deFundo, Color[] deTons)
        {
            if (luzGlobal != null)
            {
                luzGlobal.color = Color.Lerp(deGlobal, corPurificado, k);
                luzGlobal.intensity = Mathf.Lerp(deIntensidade, intensidadePurificado, k);
            }

            if (cameraVista != null) cameraVista.backgroundColor = Color.Lerp(deFundo, fundoPurificado, k);

            for (int i = 0; i < deTons.Length; i++)
            {
                if (tilemapsTingido[i] != null) tilemapsTingido[i].color = Color.Lerp(deTons[i], tomPurificado, k);
            }
        }

        private void AbrirPortoes()
        {
            if (portoesParaAberto == null) return;

            foreach (Gate portao in portoesParaAberto)
            {
                if (portao != null) portao.Abrir();
            }
        }

        private static void DefinirAtivos(GameObject[] objetos, bool ativo)
        {
            if (objetos == null) return;

            foreach (GameObject alvo in objetos)
            {
                if (alvo != null) alvo.SetActive(ativo);
            }
        }
    }
}
