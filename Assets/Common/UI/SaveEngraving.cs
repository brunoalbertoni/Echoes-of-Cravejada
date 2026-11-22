using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O aviso de salvo, sem frase nenhuma: a Bancada de Ourives grava o
    /// momento numa placa de latao.
    ///
    /// Na hora do salvamento a tela e fotografada (sem o HUD). A foto congela
    /// grande no meio da tela, num clarao; vira gravura (tons de latao,
    /// pontilhado e riscos de buril) dentro de uma moldura rebitada com a
    /// legenda "GRAVURA N. X" e o nome da area; e voa pro canto, onde carimba
    /// e fica um instante antes de sumir.
    ///
    /// Nasce sozinha na primeira vez em que e pedida.
    /// </summary>
    public class SaveEngraving : MonoBehaviour
    {
        private const int LarguraPlaca = 240;

        private const float TempoClarao = 0.45f;
        private const float EntalheDe = 0.15f;
        private const float EntalhePara = 0.8f;
        private const float VoarDe = 1.55f;
        private const float VoarPara = 2.1f;
        private const float FicarAte = 4.4f;
        private const float SumidoEm = 5f;

        private static readonly Color32[] TonalidadesLatao =
        {
            new Color32(30, 19, 12, 255),
            new Color32(78, 52, 28, 255),
            new Color32(138, 98, 46, 255),
            new Color32(196, 150, 72, 255),
            new Color32(248, 220, 150, 255),
        };

        private static readonly Dictionary<string, string> Areas = new Dictionary<string, string>
        {
            { "ScrapYard", "Ferro-Velho" },
            { "BrassPipelines", "Tubulações de Latão" },
            { "SilentGeodes", "Geodos Silenciosos" },
            { "AlchemicalAteliers", "Ateliês Alquímicos" },
            { "SilverCity", "Cidade de Prata" },
        };

        private static readonly int[] Bayer = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };

        private static SaveEngraving _instancia;

        /// <summary>Enquanto a tela e fotografada, o HUD nao se desenha.</summary>
        public static bool HudEsconder { get; private set; }

        /// <summary>Quantas gravuras ja foram feitas na sessao (o numero da placa).</summary>
        [SaveProgress("Count")]
        public static int Quantidade { get; private set; }

        private Texture2D _foto;
        private Texture2D _gravado;
        private float _inicio = -100f;
        private string _area;
        private int _numero;
        private bool _carimbado;
        private GUIStyle _legenda;
        private Texture2D _etiquetaSalvo;
        private bool _carregadoEtiquetaSalvo;

        // O "jogo salvo" pequeno e meio transparente no canto de cima a direita.
        private const float EtiquetaSalvoPor = 3.5f;
        private const float EsmaecerEtiquetaSalvo = 0.8f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            _instancia = null;
            HudEsconder = false;
            Quantidade = 0;
        }

        /// <summary>Grava o momento: fotografa a tela no fim deste quadro e comeca a cena da placa.</summary>
        public static void Gravar()
        {
            if (_instancia == null)
            {
                var hospedeiro = new GameObject("SaveEngraving");
                DontDestroyOnLoad(hospedeiro);
                _instancia = hospedeiro.AddComponent<SaveEngraving>();
            }

            _instancia.StopAllCoroutines();
            _instancia.StartCoroutine(_instancia.Capturar());
        }

        private IEnumerator Capturar()
        {
            // Um quadro sem HUD (nem a placa anterior), e a foto no fim dele.
            HudEsconder = true;
            yield return null;
            yield return new WaitForEndOfFrame();

            Texture2D plano = ScreenCapture.CaptureScreenshotAsTexture();
            HudEsconder = false;

            if (plano == null) yield break;

            Liberar();
            Construir(plano);
            Destroy(plano);

            Quantidade++;
            _numero = Quantidade;
            string cenaAtual = SceneManager.GetActiveScene().name;
            _area = cenaAtual;
            foreach (KeyValuePair<string, string> area in Areas)
            {
                if (cenaAtual.EndsWith(area.Key)) _area = area.Value;
            }
            _inicio = Time.unscaledTime;
            _carimbado = false;

            Sfx.EntalheGravar.Tocar(atraso: EntalheDe);
        }

        private void OnDestroy()
        {
            Liberar();
            if (_instancia == this) _instancia = null;
        }

        private void Liberar()
        {
            if (_foto != null) Destroy(_foto);
            if (_gravado != null) Destroy(_gravado);
            _foto = null;
            _gravado = null;
        }

        // ------------------------------------------------------------------
        // A gravura
        // ------------------------------------------------------------------

        /// <summary>A foto reduzida (pixelada como o jogo) e a mesma foto gravada em latao.</summary>
        private void Construir(Texture2D plano)
        {
            int largura = LarguraPlaca;
            int altura = Mathf.Max(1, Mathf.RoundToInt(LarguraPlaca * plano.height / (float)plano.width));

            Color32[] fonte = plano.GetPixels32();
            var foto = new Color32[largura * altura];
            var gravado = new Color32[largura * altura];

            float passoX = plano.width / (float)largura;
            float passoY = plano.height / (float)altura;
            int caixa = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(passoX, passoY)));

            for (int y = 0; y < altura; y++)
            {
                for (int x = 0; x < largura; x++)
                {
                    // A media de um quadradinho da tela.
                    int sx0 = Mathf.FloorToInt(x * passoX);
                    int sy0 = Mathf.FloorToInt(y * passoY);
                    int r = 0, g = 0, b = 0, n = 0;
                    for (int oy = 0; oy < caixa; oy += 2)
                    {
                        int sy = Mathf.Min(plano.height - 1, sy0 + oy);
                        for (int ox = 0; ox < caixa; ox += 2)
                        {
                            Color32 c = fonte[Mathf.Min(plano.width - 1, sx0 + ox) + sy * plano.width];
                            r += c.r;
                            g += c.g;
                            b += c.b;
                            n++;
                        }
                    }

                    var media = new Color32((byte)(r / n), (byte)(g / n), (byte)(b / n), 255);
                    foto[x + y * largura] = media;

                    // Luz, com contraste e as bordas mais escuras.
                    float luz = (0.3f * media.r + 0.59f * media.g + 0.11f * media.b) / 255f;
                    luz = Mathf.Clamp01((luz - 0.06f) / 0.62f);
                    float u = x / (float)largura - 0.5f;
                    float v = y / (float)altura - 0.5f;
                    luz *= 1f - 1.1f * (u * u + v * v);

                    // Pontilhado ordenado: cinco tons de latao.
                    float limiar = (Bayer[(x & 3) + (y & 3) * 4] + 0.5f) / 16f - 0.5f;
                    int nivel = Mathf.Clamp(Mathf.FloorToInt(luz * 4f + 0.5f + limiar * 0.9f), 0, 4);

                    // Os riscos do buril nas sombras, de duas em duas linhas.
                    if (luz < 0.5f && y % 3 == 0 && nivel > 0) nivel--;

                    gravado[x + y * largura] = TonalidadesLatao[nivel];
                }
            }

            _foto = CriarTextura(largura, altura, foto);
            _gravado = CriarTextura(largura, altura, gravado);
        }

        private static Texture2D CriarTextura(int largura, int altura, Color32[] pixels)
        {
            var textura = new Texture2D(largura, altura, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            textura.SetPixels32(pixels);
            textura.Apply();
            return textura;
        }

        // ------------------------------------------------------------------
        // A cena da placa
        // ------------------------------------------------------------------

        private void OnGUI()
        {
            if (HudEsconder || _gravado == null) return;

            DesenharEtiquetaSalvo();

            float idade = Time.unscaledTime - _inicio;
            if (idade < 0f || idade > SumidoEm) return;

            // Por cima do HUD.
            GUI.depth = -100;

            float telaW = Screen.width;
            float telaH = Screen.height;
            float proporcao = _gravado.height / (float)_gravado.width;

            // Grande no meio; pequena no canto de baixo a direita.
            float larguraGrande = Mathf.Min(telaW * 0.6f, telaH * 0.62f / proporcao);
            float larguraPequeno = Mathf.Max(170f, telaW * 0.17f);

            var centroGrande = new Vector2(telaW * 0.5f, telaH * 0.46f);
            float alturaPequeno = AlturaDaPlaca(larguraPequeno, proporcao);
            var centroPequeno = new Vector2(telaW - 24f - larguraPequeno * 0.5f, telaH - 24f - alturaPequeno * 0.5f);

            float voar = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(VoarDe, VoarPara, idade));
            float largura = Mathf.Lerp(larguraGrande, larguraPequeno, voar);
            Vector2 centro = Vector2.Lerp(centroGrande, centroPequeno, voar);

            // Uma arqueada no caminho, como se fosse jogada.
            centro.y -= Mathf.Sin(voar * Mathf.PI) * telaH * 0.08f;

            // Ao chegar no canto: o carimbo.
            if (idade >= VoarPara && !_carimbado)
            {
                _carimbado = true;
                Sfx.CarimboGravar.Tocar();
            }

            float soco = idade >= VoarPara ? 1f + 0.14f * Mathf.Exp(-(idade - VoarPara) * 14f) * Mathf.Cos((idade - VoarPara) * 30f) : 1f;
            largura *= soco;

            // No meio ela pende pra um lado; no canto, pro outro.
            float inclinacao = Mathf.Lerp(-2.5f * Mathf.SmoothStep(0f, 1f, idade / 0.8f), 3f, voar);

            float alfa = 1f - Mathf.InverseLerp(FicarAte, SumidoEm, idade);
            float entalhe = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(EntalheDe, EntalhePara, idade));

            Matrix4x4 valorAnterior = GUI.matrix;
            GUIUtility.RotateAroundPivot(inclinacao, centro);
            DesenharPlaca(centro, largura, proporcao, entalhe, alfa);
            GUI.matrix = valorAnterior;

            // O clarao da gema, por cima de tudo, no comeco.
            float clarao = 1f - Mathf.Clamp01(idade / TempoClarao);
            if (clarao > 0f) Preencher(new Rect(0f, 0f, telaW, telaH), new Color(1f, 0.98f, 0.9f, clarao * clarao));
        }

        private void DesenharEtiquetaSalvo()
        {
            float idade = Time.unscaledTime - _inicio;
            if (idade < 0f || idade > EtiquetaSalvoPor) return;

            if (!_carregadoEtiquetaSalvo)
            {
                _carregadoEtiquetaSalvo = true;
                _etiquetaSalvo = Resources.Load<Texture2D>("PauseScreen/saved");
            }

            float alfa = 0.55f * Mathf.Clamp01(idade / 0.3f) * (1f - Mathf.InverseLerp(EtiquetaSalvoPor - EsmaecerEtiquetaSalvo, EtiquetaSalvoPor, idade));
            Color valorAnterior = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alfa);

            if (_etiquetaSalvo != null)
            {
                int escalaAtual = Mathf.Max(1, Mathf.FloorToInt(Screen.height / 360f));
                float tw = _etiquetaSalvo.width * escalaAtual, th = _etiquetaSalvo.height * escalaAtual;
                TelaCanvas.Textura(new Rect(Screen.width - 16f - tw, 14f, tw, th), _etiquetaSalvo);
            }
            else
            {
                TelaCanvas.Texto(new Rect(Screen.width - 160f, 12f, 148f, 24f), "jogo salvo");
            }

            GUI.color = valorAnterior;
        }

        private static float AlturaDaPlaca(float largura, float proporcao)
        {
            float borda = Mathf.Round(largura * 0.04f);
            return largura * proporcao + borda * 2f + AlturaDaLegenda(largura);
        }

        private static float AlturaDaLegenda(float largura) => Mathf.Max(16f, Mathf.Round(largura * 0.085f));

        /// <summary>A placa: moldura de latao rebitada, a imagem e a faixa com a legenda.</summary>
        private void DesenharPlaca(Vector2 centro, float largura, float proporcao, float entalhe, float alfa)
        {
            float borda = Mathf.Max(4f, Mathf.Round(largura * 0.04f));
            float imagemW = largura - borda * 2f;
            float imagemH = imagemW * proporcao;
            float legendaH = AlturaDaLegenda(largura);
            float altura = imagemH + borda * 2f + legendaH;

            var externo = new Rect(centro.x - largura * 0.5f, centro.y - altura * 0.5f, largura, altura);

            // A sombra no chao da tela, a moldura e o friso.
            Preencher(new Rect(externo.x + 6f, externo.y + 8f, externo.width, externo.height), new Color(0f, 0f, 0f, 0.45f * alfa));
            Preencher(externo, CorDeLatao(0, alfa));
            Preencher(new Rect(externo.x + 2f, externo.y + 2f, externo.width - 4f, externo.height - 4f), CorDeLatao(2, alfa));
            Preencher(new Rect(externo.x + 2f, externo.y + 2f, externo.width - 4f, 2f), CorDeLatao(4, alfa));
            Preencher(new Rect(externo.x + 2f, externo.yMax - 4f, externo.width - 4f, 2f), CorDeLatao(1, alfa));

            // A imagem: a foto virando gravura.
            var imagem = new Rect(externo.x + borda, externo.y + borda, imagemW, imagemH);
            Preencher(new Rect(imagem.x - 2f, imagem.y - 2f, imagem.width + 4f, imagem.height + 4f), CorDeLatao(0, alfa));

            Color tom = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alfa * (1f - entalhe));
            if (_foto != null && entalhe < 1f) TelaCanvas.Textura(imagem, _foto);
            GUI.color = new Color(1f, 1f, 1f, alfa * entalhe);
            TelaCanvas.Textura(imagem, _gravado);
            GUI.color = tom;

            // Os rebites nos quatro cantos.
            float rebite = Mathf.Max(3f, Mathf.Round(borda * 0.5f));
            float recuo = (borda - rebite) * 0.5f;
            Rebite(new Vector2(externo.x + recuo, externo.y + recuo), rebite, alfa);
            Rebite(new Vector2(externo.xMax - recuo - rebite, externo.y + recuo), rebite, alfa);
            Rebite(new Vector2(externo.x + recuo, externo.yMax - recuo - rebite), rebite, alfa);
            Rebite(new Vector2(externo.xMax - recuo - rebite, externo.yMax - recuo - rebite), rebite, alfa);

            // A legenda, gravada fundo na faixa de baixo.
            if (_legenda == null)
            {
                _legenda = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, clipping = TextClipping.Clip };
            }

            _legenda.fontSize = Mathf.Max(9, Mathf.RoundToInt(legendaH * 0.55f));
            var tira = new Rect(imagem.x, imagem.yMax + 2f, imagem.width, legendaH);
            string texto = $"GRAVURA Nº {_numero}  ·  {_area.ToUpperInvariant()}";

            _legenda.normal.textColor = CorDeLatao(4, alfa * 0.8f);
            TelaCanvas.Texto(new Rect(tira.x + 1f, tira.y + 1f, tira.width, tira.height), texto, _legenda);
            _legenda.normal.textColor = CorDeLatao(0, alfa);
            TelaCanvas.Texto(tira, texto, _legenda);
        }

        private static void Rebite(Vector2 at, float tamanho, float alfa)
        {
            Preencher(new Rect(at.x, at.y, tamanho, tamanho), CorDeLatao(0, alfa));
            Preencher(new Rect(at.x, at.y, tamanho - 1f, tamanho - 1f), CorDeLatao(4, alfa));
            Preencher(new Rect(at.x + 1f, at.y + 1f, tamanho - 2f, tamanho - 2f), CorDeLatao(3, alfa));
        }

        private static Color CorDeLatao(int tonalidade, float alfa)
        {
            Color32 c = TonalidadesLatao[tonalidade];
            return new Color(c.r / 255f, c.g / 255f, c.b / 255f, alfa);
        }

        private static void Preencher(Rect retangulo, Color cor)
        {
            Color valorAnterior = GUI.color;
            GUI.color = cor;
            TelaCanvas.Textura(retangulo, Texture2D.whiteTexture);
            GUI.color = valorAnterior;
        }
    }
}
