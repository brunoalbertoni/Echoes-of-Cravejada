using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>Uma fonte de particulas de um quadro da cutscene, em pixels do quadro (y pra baixo).</summary>
    [Serializable]
    public class CartoonEmitter
    {
        public enum Kind { Flocos, Faiscas, Vapor, Brilho }

        [UnityEngine.Serialization.FormerlySerializedAs("kind")] public Kind tipo;
        [UnityEngine.Serialization.FormerlySerializedAs("area")] public RectInt area;

        [UnityEngine.Serialization.FormerlySerializedAs("startAt")] [Tooltip("Em que ponto da transformacao (0 a 1) ela comeca.")]
        public float inicioEm;
    }

    /// <summary>
    /// Um quadro da cutscene: o "antes", o "depois" e o mapa de ordem (canal
    /// vermelho, 0 a 254: quem e mais escuro muda primeiro; 255 nunca muda).
    /// Todos 320x180, legiveis (Read/Write) e com filtro de pixel.
    /// </summary>
    [Serializable]
    public class CartoonShot
    {
        [UnityEngine.Serialization.FormerlySerializedAs("before")] public Texture2D antes;
        [UnityEngine.Serialization.FormerlySerializedAs("after")] public Texture2D depois;
        [UnityEngine.Serialization.FormerlySerializedAs("order")] public Texture2D ordem;

        [UnityEngine.Serialization.FormerlySerializedAs("loop")] [Tooltip("Quadros repetidos depois da transformacao (a capa na pose final). Vazio: nenhum.")]
        public Texture2D[] repeticao;

        [UnityEngine.Serialization.FormerlySerializedAs("emitters")] public CartoonEmitter[] emissores;
        [UnityEngine.Serialization.FormerlySerializedAs("duration")] public float duracao = 3f;

        [UnityEngine.Serialization.FormerlySerializedAs("pan")] [Tooltip("Quanto a camera desliza ao longo do quadro, em pixels do quadro (y pra baixo).")]
        public Vector2 panorama;

        [UnityEngine.Serialization.FormerlySerializedAs("zoomFrom")] public float zoomDe = 1f;
        [UnityEngine.Serialization.FormerlySerializedAs("zoomTo")] public float zoomPara = 1f;
    }

    /// <summary>
    /// A cutscene desenhada da purificacao do Scrap (fim da Phase 1): sai da
    /// camera do jogo e passa quadros ilustrados em tela cheia, como um
    /// desenho animado. Em cada quadro a sucata vira o polido parte por parte,
    /// na ordem do mapa, com uma faixa de luz na borda e lascas e faiscas
    /// saindo exatamente de onde a luz esta passando.
    ///
    /// No fim, um clarao branco: no pico dele o Scrap troca de visual de
    /// verdade (onPolish), a cutscene some e o branco desbota sobre o jogo.
    ///
    /// Da pra pular com pular ou atacar depois do primeiro segundo: vai direto
    /// pro fim da pose final, e a troca acontece do mesmo jeito.
    ///
    /// A arte vem do editor (PurificationCartoonArt + CartoonAssets).
    /// </summary>
    public class PurificationCartoon : MonoBehaviour
    {
        public const int Largura = 320;
        public const int Altura = 180;

        /// <summary>Altura de cada faixa preta, em fracao da altura do quadro.</summary>
        private const float FracaoBarra = 0.068f;

        /// <summary>Parte do quadro em que a transformacao acontece; o resto segura o resultado.</summary>
        private const float ParcelaRevelar = 0.75f;

        private const float FaixaBeirada = 14f;
        private const float PularDepois = 1f;
        private const float InicioClaraoFinal = 0.6f;
        private const float SubidaClaraoFinal = 0.35f;
        private const float TempoForaEsmaecer = 0.45f;
        private const int QuadrosCorte = 3;
        private const int ParticulasMax = 500;

        private static readonly Color32 CorBeirada = new Color32(255, 240, 190, 255);
        private static readonly Color32 CorCorte = new Color32(255, 247, 224, 255);

        [UnityEngine.Serialization.FormerlySerializedAs("shots")] [SerializeField] private CartoonShot[] planos;

        /// <summary>Se alguma cutscene desenhada esta na tela.</summary>
        public static bool EstaTocando { get; private set; }

        private struct Particle
        {
            public Vector2 Posicao;
            public Vector2 VelocidadeVetorial;
            public float Idade;
            public float DuracaoVida;
            public Color32 Cor;
            public float Tamanho;
            public float Crescer;
            public float Gravidade;
            public bool Esmaecimento;
        }

        private struct ShotPixels
        {
            public Color32[] Antes;
            public Color32[] Depois;
            public byte[] Ordem;
            public Color32[][] Repeticao;
        }

        private ShotPixels[] _pixels;
        private Color32[] _reserva;
        private Texture2D _tela;
        private readonly Particle[] _particulas2 = new Particle[ParticulasMax];
        private int _quantidadeParticula;
        private System.Random _aleatorio = new System.Random(7);

        private bool _painelMostrar;
        private float _branco;
        private int _corte;
        private Rect _uv = new Rect(0f, 0f, 1f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarEstaticos()
        {
            EstaTocando = false;
        }

        /// <summary>Usado pelo gerador da Phase 1.</summary>
        public void Configurar(CartoonShot[] novoPlanos)
        {
            planos = novoPlanos;
        }

        /// <summary>Se todos os quadros estao presentes, legiveis e no tamanho certo.</summary>
        public bool EstaProntoUso
        {
            get
            {
                if (planos == null || planos.Length == 0) return false;

                foreach (CartoonShot plano in planos)
                {
                    if (plano == null || plano.duracao <= 0f) return false;
                    if (!EUsavel(plano.antes) || !EUsavel(plano.depois) || !EUsavel(plano.ordem)) return false;

                    if (plano.repeticao == null) continue;
                    foreach (Texture2D quadro in plano.repeticao)
                    {
                        if (!EUsavel(quadro)) return false;
                    }
                }

                return true;
            }
        }

        private static bool EUsavel(Texture2D textura)
        {
            return textura != null && textura.isReadable && textura.width == Largura && textura.height == Altura;
        }

        /// <summary>
        /// Passa a cutscene inteira. onPolish e chamado uma vez, no pico do
        /// clarao final, com a tela toda branca.
        /// </summary>
        public IEnumerator Tocar(Action ligadoPolimento)
        {
            bool polido = false;

            void PolishOnce()
            {
                if (polido) return;
                polido = true;
                ligadoPolimento?.Invoke();
            }

            if (!EstaProntoUso)
            {
                PolishOnce();
                yield break;
            }

            Preparar();
            EstaTocando = true;
            _painelMostrar = true;
            _branco = 1f;
            _corte = 0;

            int indice = 0;
            float t = 0f;
            float decorrido = 0f;
            bool pulado = false;
            bool clareado = false;
            int ultimo = planos.Length - 1;
            SomDoPlano(planos[0], true);

            while (indice < planos.Length)
            {
                CartoonShot plano = planos[indice];

                if (!pulado && decorrido > PularDepois && PularPressionado())
                {
                    pulado = true;
                    indice = ultimo;
                    plano = planos[ultimo];
                    t = Mathf.Max(0f, plano.duracao - 0.8f);
                    _quantidadeParticula = 0;
                    _corte = QuadrosCorte;
                    Sfx.PlanoDesenho.Tocar();
                }

                // O clarao do inicio desbota rapido; o do fim sobe ate o branco.
                _branco = Mathf.MoveTowards(_branco, 0f, Time.deltaTime * 5f);

                if (indice == ultimo)
                {
                    float subida = (t - (plano.duracao - InicioClaraoFinal)) / SubidaClaraoFinal;
                    if (subida > 0f) _branco = Mathf.Max(_branco, Mathf.Clamp01(subida));

                    // O clarao: o som sobe junto com o branco.
                    if (subida > 0f && !clareado)
                    {
                        clareado = true;
                        Sfx.ClaraoDesenho.Tocar();
                    }
                    if (subida >= 1f) PolishOnce();
                }

                Compor(indice, plano, t);

                yield return null;

                if (_corte > 0) _corte--;

                t += Time.deltaTime;
                decorrido += Time.deltaTime;

                if (t >= plano.duracao)
                {
                    indice++;
                    t = 0f;
                    _quantidadeParticula = 0;
                    if (indice < planos.Length)
                    {
                        _corte = QuadrosCorte;
                        if (!pulado) SomDoPlano(planos[indice], false);
                    }
                }
            }

            // A tela esta branca: troca o Scrap, tira o quadro e desbota sobre o jogo.
            PolishOnce();
            _branco = 1f;
            _painelMostrar = false;

            for (float f = 0f; f < TempoForaEsmaecer; f += Time.deltaTime)
            {
                _branco = 1f - f / TempoForaEsmaecer;
                yield return null;
            }

            Parar();
        }

        /// <summary>
        /// O som de um quadro: a troca (menos no primeiro) e a luz passando,
        /// esticada pra durar o tanto que a transformacao dura.
        /// </summary>
        private static void SomDoPlano(CartoonShot plano, bool primeiro)
        {
            if (plano == null) return;

            if (!primeiro) Sfx.PlanoDesenho.Tocar();

            const float clipe = 2.2f;
            float revelar = Mathf.Max(0.5f, plano.duracao * ParcelaRevelar);
            Sfx.RevelarDesenho.Tocar(tom2: Mathf.Clamp(clipe / revelar, 0.7f, 1.3f), atraso: primeiro ? 0.15f : 0.05f);
        }

        private void Parar()
        {
            _branco = 0f;
            _painelMostrar = false;
            EstaTocando = false;
        }

        private void OnDisable()
        {
            Parar();
        }

        private void OnDestroy()
        {
            EstaTocando = false;
            if (_tela != null) Destroy(_tela);
        }

        /// <summary>Le os pixels de todos os quadros uma vez.</summary>
        private void Preparar()
        {
            if (_pixels != null) return;

            _pixels = new ShotPixels[planos.Length];
            for (int i = 0; i < planos.Length; i++)
            {
                CartoonShot plano = planos[i];
                Color32[] ordem = plano.ordem.GetPixels32();
                var bytesOrdem = new byte[ordem.Length];
                for (int k = 0; k < ordem.Length; k++) bytesOrdem[k] = ordem[k].r;

                Color32[][] repeticao = null;
                if (plano.repeticao != null && plano.repeticao.Length > 0)
                {
                    repeticao = new Color32[plano.repeticao.Length][];
                    for (int k = 0; k < plano.repeticao.Length; k++) repeticao[k] = plano.repeticao[k].GetPixels32();
                }

                _pixels[i] = new ShotPixels
                {
                    Antes = plano.antes.GetPixels32(),
                    Depois = plano.depois.GetPixels32(),
                    Ordem = bytesOrdem,
                    Repeticao = repeticao,
                };
            }

            _reserva = new Color32[Largura * Altura];
            _tela = new Texture2D(Largura, Altura, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
        }

        // ------------------------------------------------------------------
        // Montagem de cada quadro
        // ------------------------------------------------------------------

        private void Compor(int indice, CartoonShot plano, float t)
        {
            ShotPixels pixels = _pixels[indice];
            float progresso = Mathf.Clamp01(t / (plano.duracao * ParcelaRevelar));
            float limiar = progresso * 270f;

            if (progresso >= 1f && pixels.Repeticao != null)
            {
                Color32[] quadro = pixels.Repeticao[(int)(t * 6f) % pixels.Repeticao.Length];
                Array.Copy(quadro, _reserva, _reserva.Length);
            }
            else if (progresso >= 1f)
            {
                Array.Copy(pixels.Depois, _reserva, _reserva.Length);
            }
            else
            {
                Revelar(pixels, limiar);
            }

            Emitir(pixels, plano, progresso, limiar, t);
            AvancarParticulas(Time.deltaTime);
            DesenharParticulas();

            _tela.SetPixels32(_reserva);
            _tela.Apply(false);

            // Camera dentro do quadro: aproxima/afasta e desliza.
            float k = Mathf.Clamp01(t / plano.duracao);
            float zoom = Mathf.Max(1f, Mathf.Lerp(plano.zoomDe, plano.zoomPara, SuavizarValor(k)));
            float w = 1f / zoom, h = 1f / zoom;
            float cx = 0.5f + plano.panorama.x * k / Largura;
            float cy = 0.5f - plano.panorama.y * k / Altura;
            _uv = new Rect(Mathf.Clamp(cx - w / 2f, 0f, 1f - w), Mathf.Clamp(cy - h / 2f, 0f, 1f - h), w, h);
        }

        private void Revelar(ShotPixels pixels, float limiar)
        {
            Color32[] antes = pixels.Antes, depois = pixels.Depois;
            byte[] ordem = pixels.Ordem;
            float inicioBeirada = limiar - FaixaBeirada;

            for (int i = 0; i < _reserva.Length; i++)
            {
                byte o = ordem[i];
                if (o == 255 || o >= limiar)
                {
                    _reserva[i] = antes[i];
                }
                else if (o < inicioBeirada)
                {
                    _reserva[i] = depois[i];
                }
                else
                {
                    Color32 a = depois[i];
                    _reserva[i] = new Color32(
                        (byte)(a.r * 0.4f + CorBeirada.r * 0.6f),
                        (byte)(a.g * 0.4f + CorBeirada.g * 0.6f),
                        (byte)(a.b * 0.4f + CorBeirada.b * 0.6f),
                        255);
                }
            }
        }

        private static float SuavizarValor(float k) => k * k * (3f - 2f * k);

        // ------------------------------------------------------------------
        // Particulas
        // ------------------------------------------------------------------

        private void Emitir(ShotPixels pixels, CartoonShot plano, float progresso, float limiar, float t)
        {
            if (plano.emissores == null) return;

            float dt = Time.deltaTime;

            foreach (CartoonEmitter emissor in plano.emissores)
            {
                if (emissor == null || progresso < emissor.inicioEm) continue;
                RectInt area = emissor.area;

                switch (emissor.tipo)
                {
                    case CartoonEmitter.Kind.Flocos:
                    case CartoonEmitter.Kind.Faiscas:
                    {
                        // So enquanto a transformacao corre: as particulas nascem na borda dela.
                        if (progresso >= 1f) break;

                        bool flocos = emissor.tipo == CartoonEmitter.Kind.Flocos;
                        int tentativas = Mathf.CeilToInt((flocos ? 1600f : 900f) * dt);

                        for (int n = 0; n < tentativas; n++)
                        {
                            int x = area.x + _aleatorio.Next(0, Mathf.Max(1, area.width));
                            int y = area.y + _aleatorio.Next(0, Mathf.Max(1, area.height));
                            int i = Indice(x, y);
                            if (i < 0) continue;

                            byte o = pixels.Ordem[i];
                            if (o == 255 || o >= limiar || o < limiar - FaixaBeirada * 1.6f) continue;

                            if (flocos) CriarLasca(x, y, pixels.Antes[i]);
                            else if (_aleatorio.NextDouble() < 0.35) CriarFaisca(x, y);
                        }

                        break;
                    }

                    case CartoonEmitter.Kind.Vapor:
                    {
                        int quantidade = Mathf.CeilToInt(70f * dt);
                        for (int n = 0; n < quantidade; n++) CriarVapor(area);
                        break;
                    }

                    case CartoonEmitter.Kind.Brilho:
                    {
                        float desde = t - emissor.inicioEm * plano.duracao * ParcelaRevelar;
                        DesenharBrilho(area, desde / 0.4f);
                        break;
                    }
                }
            }
        }

        private void CriarLasca(int x, int y, Color32 cor)
        {
            Adicionar(new Particle
            {
                Posicao = new Vector2(x, y),
                VelocidadeVetorial = new Vector2(SortearIntervalo(-22f, 22f), SortearIntervalo(-34f, -6f)),
                DuracaoVida = SortearIntervalo(0.6f, 1.2f),
                Cor = cor,
                Tamanho = _aleatorio.NextDouble() < 0.3 ? 2f : 1f,
                Gravidade = 110f,
            });
        }

        private void CriarFaisca(int x, int y)
        {
            bool ciano = _aleatorio.NextDouble() < 0.5;
            Adicionar(new Particle
            {
                Posicao = new Vector2(x, y),
                VelocidadeVetorial = new Vector2(SortearIntervalo(-10f, 10f), SortearIntervalo(-42f, -18f)),
                DuracaoVida = SortearIntervalo(0.35f, 0.8f),
                Cor = ciano ? new Color32(170, 240, 255, 255) : new Color32(255, 250, 220, 255),
                Tamanho = 1f,
                Gravidade = -10f,
                Esmaecimento = true,
            });
        }

        private void CriarVapor(RectInt area)
        {
            Adicionar(new Particle
            {
                Posicao = new Vector2(area.x + SortearIntervalo(0f, area.width), area.y + SortearIntervalo(0f, area.height)),
                VelocidadeVetorial = new Vector2(SortearIntervalo(-10f, 10f), SortearIntervalo(-95f, -60f)),
                DuracaoVida = SortearIntervalo(0.5f, 0.9f),
                Cor = new Color32(220, 234, 238, 255),
                Tamanho = SortearIntervalo(2f, 4f),
                Crescer = 7f,
                Gravidade = 30f,
                Esmaecimento = true,
            });
        }

        private void Adicionar(Particle particula2)
        {
            if (_quantidadeParticula >= ParticulasMax) return;
            _particulas2[_quantidadeParticula++] = particula2;
        }

        private void AvancarParticulas(float dt)
        {
            for (int i = _quantidadeParticula - 1; i >= 0; i--)
            {
                Particle p = _particulas2[i];
                p.Idade += dt;

                if (p.Idade >= p.DuracaoVida)
                {
                    _particulas2[i] = _particulas2[--_quantidadeParticula];
                    continue;
                }

                p.VelocidadeVetorial.y += p.Gravidade * dt;
                p.Posicao += p.VelocidadeVetorial * dt;
                p.Tamanho += p.Crescer * dt;
                _particulas2[i] = p;
            }
        }

        private void DesenharParticulas()
        {
            for (int i = 0; i < _quantidadeParticula; i++)
            {
                Particle p = _particulas2[i];
                float alfa = p.Esmaecimento ? 1f - p.Idade / p.DuracaoVida : 1f;
                int tamanho = Mathf.Max(1, Mathf.RoundToInt(p.Tamanho));
                int x0 = Mathf.RoundToInt(p.Posicao.x - tamanho / 2f), y0 = Mathf.RoundToInt(p.Posicao.y - tamanho / 2f);

                for (int y = y0; y < y0 + tamanho; y++)
                {
                    for (int x = x0; x < x0 + tamanho; x++)
                    {
                        // O vapor e redondo; o resto, quadradinho.
                        if (tamanho > 3)
                        {
                            float dx = x + 0.5f - p.Posicao.x, dy = y + 0.5f - p.Posicao.y;
                            if (dx * dx + dy * dy > tamanho * tamanho * 0.25f) continue;
                        }

                        Misturar(x, y, p.Cor, alfa);
                    }
                }
            }
        }

        /// <summary>O brilho que corre pelo fio: um risco claro com uma estrelinha na ponta, da base ate a ponta.</summary>
        private void DesenharBrilho(RectInt area, float k)
        {
            if (k < 0f || k > 1.3f) return;
            k = Mathf.Clamp01(k);

            var de = new Vector2(area.x, area.y + area.height);
            var to = new Vector2(area.x + area.width, area.y);
            Vector2 cabeca = Vector2.Lerp(de, to, k);
            Vector2 dir = (to - de).normalized;
            var branco = new Color32(255, 255, 255, 255);

            for (int s = 0; s < 18; s++)
            {
                Vector2 q = cabeca - dir * s;
                float a = 1f - s / 18f;
                Misturar(Mathf.RoundToInt(q.x), Mathf.RoundToInt(q.y), branco, a);
                Misturar(Mathf.RoundToInt(q.x), Mathf.RoundToInt(q.y) + 1, branco, a * 0.6f);
            }

            int hx = Mathf.RoundToInt(cabeca.x), hy = Mathf.RoundToInt(cabeca.y);
            for (int r = -4; r <= 4; r++)
            {
                float a = 1f - Mathf.Abs(r) / 5f;
                Misturar(hx + r, hy, branco, a);
                Misturar(hx, hy + r, branco, a);
            }
        }

        /// <summary>Indice no buffer (que e de baixo pra cima, como as texturas) de um pixel do quadro (y pra baixo).</summary>
        private static int Indice(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Largura || y >= Altura) return -1;
            return x + (Altura - 1 - y) * Largura;
        }

        private void Misturar(int x, int y, Color32 cor, float alfa)
        {
            int i = Indice(x, y);
            if (i < 0 || alfa <= 0f) return;

            Color32 abaixo = _reserva[i];
            _reserva[i] = new Color32(
                (byte)(abaixo.r + (cor.r - abaixo.r) * alfa),
                (byte)(abaixo.g + (cor.g - abaixo.g) * alfa),
                (byte)(abaixo.b + (cor.b - abaixo.b) * alfa),
                255);
        }

        private float SortearIntervalo(float min, float max) => min + (float)_aleatorio.NextDouble() * (max - min);

        // ------------------------------------------------------------------
        // Tela e controle
        // ------------------------------------------------------------------

        private static bool PularPressionado()
        {
            Keyboard teclado = Keyboard.current;
            if (teclado != null && (teclado.spaceKey.wasPressedThisFrame || teclado.jKey.wasPressedThisFrame
                                     || teclado.xKey.wasPressedThisFrame || teclado.enterKey.wasPressedThisFrame))
            {
                return true;
            }

            Gamepad controle = Gamepad.current;
            return controle != null && (controle.buttonSouth.wasPressedThisFrame || controle.buttonWest.wasPressedThisFrame);
        }

        private void OnGUI()
        {
            if (!EstaTocando) return;
            if (Event.current.type != EventType.Repaint) return;

            // Por cima do HUD e de tudo.
            GUI.depth = -1000;
            Color valorAnterior = GUI.color;
            var cheio = new Rect(0f, 0f, Screen.width, Screen.height);

            if (_painelMostrar)
            {
                Preencher(cheio, Color.black);

                // O maior 16:9 que cabe, centralizado; o resto fica preto.
                float escalaAtual = Mathf.Min(Screen.width / (float)Largura, Screen.height / (float)Altura);
                float w = Largura * escalaAtual, h = Altura * escalaAtual;
                var painel = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

                if (_corte > 0)
                {
                    Preencher(painel, CorCorte);
                }
                else if (_tela != null)
                {
                    GUI.color = Color.white;
                    TelaCanvas.TexturaUV(painel, _tela, _uv, false);
                }

                float barra = h * FracaoBarra;
                Preencher(new Rect(painel.x, painel.y, w, barra), Color.black);
                Preencher(new Rect(painel.x, painel.yMax - barra, w, barra), Color.black);
            }

            if (_branco > 0f) Preencher(cheio, new Color(1f, 0.98f, 0.92f, _branco));

            GUI.color = valorAnterior;
        }

        private static void Preencher(Rect retangulo, Color cor)
        {
            GUI.color = cor;
            TelaCanvas.Textura(retangulo, Texture2D.whiteTexture);
        }
    }
}
