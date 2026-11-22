using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A troca de fase: a tela fecha em blocos pretos que crescem na diagonal,
    /// o "Carregando" aparece no canto de baixo a direita (com uma engrenagem
    /// de latao girando) enquanto a cena nova carrega por tras, e a tela abre
    /// com a mesma transicao ao contrario.
    ///
    /// Quem troca de cena chama <see cref="Ir"/> em vez de SceneManager.LoadScene.
    /// Nasce sozinho na primeira vez e atravessa as cenas.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        private const float TempoFechar = 0.55f;
        private const float TempoAberto = 0.55f;

        /// <summary>Mesmo carregando rapido, o loading fica pelo menos isto na tela (sem piscar).</summary>
        private const float MostradoMin = 0.7f;

        private static readonly Color Tinta = new Color(0.03f, 0.025f, 0.035f, 1f);
        private static readonly Color CorTexto = new Color(0.93f, 0.82f, 0.56f, 1f);

        private static SceneLoader _instancia;
        private static Texture2D[] _engrenagem;

        /// <summary>Se ha uma troca de cena acontecendo agora.</summary>
        public static bool Ocupado => _instancia != null && _instancia._ocupado;

        private bool _ocupado;

        /// <summary>Quanto a tela esta coberta: 0 aberta, 1 toda preta.</summary>
        private float _cobertura;

        /// <summary>Se a cobertura esta fechando (true) ou abrindo (false): muda o sentido da varredura.</summary>
        private bool _fechando;

        /// <summary>O "Carregando": 0 escondido, 1 inteiro.</summary>
        private float _rotulo;
        private GUIStyle _estilo;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            _instancia = null;
        }

        /// <summary>
        /// Leva o Scrap pra <paramref name="nomeCena"/>, aparecendo no
        /// SpawnPoint <paramref name="idNascimento"/>. Com <paramref name="pretoJa"/>
        /// (o elevador, que ja escureceu a tela) a tela nao fecha de novo.
        /// </summary>
        public static void Ir(string nomeCena, string idNascimento, bool pretoJa = false)
        {
            if (_instancia == null)
            {
                var hospedeiro = new GameObject("SceneLoader");
                DontDestroyOnLoad(hospedeiro);
                _instancia = hospedeiro.AddComponent<SceneLoader>();
            }

            if (_instancia._ocupado) return;
            _instancia.StartCoroutine(_instancia.Viagem(nomeCena, idNascimento, pretoJa));
        }

        private IEnumerator Viagem(string nomeCena, string idNascimento, bool pretoJa)
        {
            _ocupado = true;
            _fechando = true;

            // O Scrap para onde esta enquanto a tela fecha.
            var controlador = ScrapRef.Controlador;
            if (controlador != null)
            {
                controlador.ControleTravado = true;
                var corpo = controlador.GetComponent<Rigidbody2D>();
                if (corpo != null) corpo.linearVelocity = new Vector2(0f, Mathf.Min(0f, corpo.linearVelocity.y));
            }

            // 1. Fecha.
            if (pretoJa)
            {
                _cobertura = 1f;
            }
            else
            {
                Sfx.PlanoDesenho.Tocar(tom2: 0.8f);
                yield return AnimarCobertura(0f, 1f, TempoFechar);
            }

            // 2. Carrega por tras, com o loading no canto.
            float mostradoEm = Time.unscaledTime;
            SceneTravel.IdNascimentoPendente = idNascimento;
            AsyncOperation carregamento = SceneManager.LoadSceneAsync(nomeCena);

            if (carregamento == null)
            {
                Debug.LogError("[SceneLoader] Nao deu pra carregar a cena " + nomeCena + ".");
                _ocupado = false;
                _cobertura = 0f;
                yield break;
            }

            carregamento.allowSceneActivation = false;

            while (carregamento.progress < 0.9f || Time.unscaledTime - mostradoEm < MostradoMin)
            {
                _rotulo = Mathf.MoveTowards(_rotulo, 1f, Time.unscaledDeltaTime * 4f);
                yield return null;
            }

            carregamento.allowSceneActivation = true;
            while (!carregamento.isDone)
            {
                yield return null;
            }

            // Uns quadros pra cena nova se montar (o chao, a camera no Scrap).
            for (float t = 0f; t < 0.3f; t += Time.unscaledDeltaTime)
            {
                yield return null;
            }

            // 3. O loading some e a tela abre, a mesma varredura ao contrario.
            while (_rotulo > 0f)
            {
                _rotulo = Mathf.MoveTowards(_rotulo, 0f, Time.unscaledDeltaTime * 5f);
                yield return null;
            }

            _fechando = false;
            Sfx.PlanoDesenho.Tocar(tom2: 1.1f);
            yield return AnimarCobertura(1f, 0f, TempoAberto);
            _ocupado = false;
        }

        /// <summary>Anima a cobertura da tela com LeanTween, mesmo com o tempo do jogo parado.</summary>
        private IEnumerator AnimarCobertura(float de, float para, float duracao)
        {
            bool terminou = false;
            LeanTween.value(gameObject, de, para, duracao)
                .setIgnoreTimeScale(true)
                .setOnUpdate((float valor) => _cobertura = valor)
                .setOnComplete(() => terminou = true);

            float limite = Time.unscaledTime + duracao + 0.5f;
            while (!terminou && Time.unscaledTime < limite) yield return null;

            _cobertura = para;
        }

        // ------------------------------------------------------------------
        // Desenho
        // ------------------------------------------------------------------

        private void OnGUI()
        {
            if (_cobertura <= 0f && _rotulo <= 0f) return;

            // Por cima de tudo, ate da gravura do salvamento.
            GUI.depth = -200;

            DesenharCobertura();
            if (_rotulo > 0f) DesenharCarregamento();
        }

        /// <summary>
        /// A cobertura: uma grade de quadrados pretos que crescem do centro de
        /// cada celula, um atras do outro na diagonal (da esquerda pra direita
        /// ao fechar; ao abrir, somem na mesma ordem).
        /// </summary>
        private void DesenharCobertura()
        {
            float w = Screen.width;
            float h = Screen.height;

            if (_cobertura >= 1f)
            {
                Preencher(new Rect(0f, 0f, w, h), Tinta);
                return;
            }

            float celula = Mathf.Ceil(h / 10f);
            int colunas = Mathf.CeilToInt(w / celula);
            int linhas = Mathf.CeilToInt(h / celula);
            float extensao = colunas + linhas * 0.5f;

            // Cada quadrado leva este tanto da transicao pra crescer.
            const float crescer = 0.35f;

            for (int linha = 0; linha < linhas; linha++)
            {
                for (int col = 0; col < colunas; col++)
                {
                    float ordem = (col + linha * 0.5f) / extensao;

                    // Ao fechar, os da esquerda vem primeiro; ao abrir, os da esquerda saem primeiro.
                    float k = _fechando
                        ? Mathf.Clamp01((_cobertura - ordem * (1f - crescer)) / crescer)
                        : 1f - Mathf.Clamp01(((1f - _cobertura) - ordem * (1f - crescer)) / crescer);

                    if (k <= 0f) continue;

                    // Em degraus de pixel, como o resto do jogo.
                    float tamanho = Mathf.Ceil(celula * k / 4f) * 4f;
                    float x = col * celula + (celula - tamanho) * 0.5f;
                    float y = linha * celula + (celula - tamanho) * 0.5f;
                    Preencher(new Rect(x, y, tamanho + 1f, tamanho + 1f), Tinta);
                }
            }
        }

        /// <summary>O "Carregando" no canto de baixo a direita, com a engrenagem girando aos trancos.</summary>
        private void DesenharCarregamento()
        {
            if (_estilo == null)
            {
                _estilo = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleRight,
                    fontStyle = FontStyle.Bold,
                };
            }

            float escalaAtual = Mathf.Max(1f, Screen.height / 360f);
            _estilo.fontSize = Mathf.RoundToInt(9f * escalaAtual);

            float engrenagem = 16f * escalaAtual;
            float margem = 14f * escalaAtual;
            var retanguloEngrenagem = new Rect(Screen.width - margem - engrenagem, Screen.height - margem - engrenagem, engrenagem, engrenagem);

            // Os pontinhos andando.
            int pontos = (int)(Time.unscaledTime * 3f) % 4;
            string texto = "Carregando" + new string('.', pontos) + new string(' ', 3 - pontos);
            var retanguloTexto = new Rect(retanguloEngrenagem.x - 220f * escalaAtual, retanguloEngrenagem.y, 220f * escalaAtual - 6f * escalaAtual, engrenagem);

            Color valorAnterior = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, _rotulo);

            // A engrenagem: 8 quadros de um dente, trocados aos trancos.
            Texture2D[] quadros = QuadrosDaEngrenagem();
            TelaCanvas.Textura(retanguloEngrenagem, quadros[(int)(Time.unscaledTime * 12f) % quadros.Length]);

            _estilo.normal.textColor = new Color(CorTexto.r, CorTexto.g, CorTexto.b, _rotulo);
            TelaCanvas.Texto(retanguloTexto, texto, _estilo);

            GUI.color = valorAnterior;
        }

        /// <summary>
        /// Uma engrenagem de latao de 16 px, em 8 quadros que giram um dente
        /// inteiro (45 graus): em loop, ela gira sem parar.
        /// </summary>
        private static Texture2D[] QuadrosDaEngrenagem()
        {
            if (_engrenagem != null) return _engrenagem;

            var escuro = new Color32(90, 60, 30, 255);
            var meio = new Color32(198, 152, 70, 255);
            var luz = new Color32(240, 206, 124, 255);
            var gema = new Color32(120, 230, 255, 255);
            var vazio = new Color32(0, 0, 0, 0);

            const int tamanho = 16;
            const int dentes = 8;
            const int quantidade = 8;
            _engrenagem = new Texture2D[quantidade];

            for (int f = 0; f < quantidade; f++)
            {
                var pixels = new Color32[tamanho * tamanho];
                float giroValvula = f * (360f / dentes) / quantidade;

                for (int y = 0; y < tamanho; y++)
                {
                    for (int x = 0; x < tamanho; x++)
                    {
                        float dx = x + 0.5f - tamanho * 0.5f;
                        float dy = y + 0.5f - tamanho * 0.5f;
                        float r = Mathf.Sqrt(dx * dx + dy * dy);
                        float angulo = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg - giroValvula;
                        float fase = Mathf.Repeat(angulo, 360f / dentes) / (360f / dentes);

                        Color32 c = vazio;
                        if (r < 2.1f) c = gema;
                        else if (r < 3.2f) c = vazio;
                        else if (r < 5.6f) c = (dx - dy) < 0f ? luz : meio;
                        else if (r < 6.6f) c = escuro;
                        else if (r < 7.9f && fase < 0.5f) c = fase < 0.25f ? meio : escuro;

                        pixels[x + y * tamanho] = c;
                    }
                }

                var textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                };
                textura.SetPixels32(pixels);
                textura.Apply();
                _engrenagem[f] = textura;
            }

            return _engrenagem;
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
