using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A tela de fim de um final, toda em pixel (Resources/EndScreen, desenhada
    /// pelo EndScreenArt e pelo gerador da fase 5). Entra em tempos, devagar:
    ///
    ///   o escuro desce, com particulas subindo (esporos verdes no fim da
    ///   coroa; poeira palida no do nucleo);
    ///   o desenho do final sobe; o titulo de latao; o friso abre do meio;
    ///   a historia, uma linha de cada vez;
    ///   o credito da musica; e so depois de uns segundos, a placa de voltar
    ///   pro menu.
    ///
    /// O jogo nao volta: daqui, so o menu. Teclado (Enter/E/Espaco),
    /// controle (A/Start) e mouse.
    /// </summary>
    public class EndScreen : MonoBehaviour
    {
        private const int LarguraGrade = 320;
        private const int AlturaGrade = 180;

        private const float TempoEscuro = 1.4f;
        private const float EmblemaDentro = 1.0f;
        private const float TituloDentro = 2.0f;
        private const float DivisorDentro = 2.8f;
        private const float LinhasDentro = 3.6f;
        private const float EspacoLinha = 1.9f;
        private const float BotaoDepois = 3.5f;

        private const int EmblemaY = 4;
        private const int TituloY = 52;
        private const int DivisorY = 84;
        private const int LinhaY = 92;
        private const int PassoLinha = 10;
        private const int BotaoY = 146;
        private const int CreditoY = 168;

        private static EndScreen _instancia;

        private string _id;
        private int _linhas;
        private bool _esporos;
        private float _abertosEm;
        private bool _saindo;
        private bool _flutuar;

        private Texture2D _emblema, _titulo, _divisor, _credito, _vinheta;
        private Texture2D[] _historia;
        private Texture2D[] _botao;

        private readonly Vector3[] _particulas = new Vector3[60];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            _instancia = null;
        }

        /// <summary>Se a tela de fim esta aberta.</summary>
        public static bool Mostrando => _instancia != null;

        /// <summary>Abre a tela do final <paramref name="id"/> (as linhas sao {id}_line0..).</summary>
        public static void Mostrar(string id, int linhas2, bool esporos)
        {
            if (_instancia != null) return;
            var hospedeiro = new GameObject("EndScreen");
            _instancia = hospedeiro.AddComponent<EndScreen>();
            _instancia.Abrir(id, linhas2, esporos);
        }

        private void Abrir(string id, int linhas2, bool esporos)
        {
            _id = id;
            _linhas = linhas2;
            _esporos = esporos;
            _abertosEm = Time.unscaledTime;

            _emblema = CarregarArte(id + "_emblem");
            _titulo = CarregarArte(id + "_title");
            _divisor = CarregarArte("divider");
            _credito = CarregarArte("credit");
            _historia = new Texture2D[linhas2];
            for (int i = 0; i < linhas2; i++) _historia[i] = CarregarArte($"{id}_line{i}");
            _botao = new[] { CarregarArte("button_menu_off"), CarregarArte("button_menu_on") };
            _vinheta = Vinheta(esporos);

            for (int i = 0; i < _particulas.Length; i++) _particulas[i] = NovaPoeira(Random.Range(0f, AlturaGrade));
        }

        private void OnDestroy()
        {
            if (_instancia == this) _instancia = null;
        }

        private static Texture2D CarregarArte(string nome)
        {
            var textura = Resources.Load<Texture2D>("EndScreen/" + nome);
            if (textura == null) Debug.LogWarning("[Fim] Falta Resources/EndScreen/" + nome);
            return textura;
        }

        /// <summary>O fundo: escuro, com o meio levemente tingido (verde doente ou azul frio).</summary>
        private static Texture2D Vinheta(bool doente)
        {
            const int w = 64, h = 36;
            var textura = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var pixels = new Color32[w * h];
            Color centro = doente ? new Color(0.04f, 0.11f, 0.08f) : new Color(0.06f, 0.07f, 0.12f);
            var aro = new Color(0.01f, 0.01f, 0.015f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float u = (x + 0.5f) / w - 0.5f;
                    float v = (y + 0.5f) / h - 0.5f;
                    float beirada = Mathf.Clamp01((u * u * 1.6f + v * v * 2.2f) * 1.9f);
                    Color c = Color.Lerp(centro, aro, beirada);
                    pixels[x + y * w] = new Color(c.r, c.g, c.b, 1f);
                }
            }

            textura.SetPixels32(pixels);
            textura.Apply();
            return textura;
        }

        /// <summary>Uma particula nova: x ao acaso, a altura dada, a velocidade de subida no z.</summary>
        private static Vector3 NovaPoeira(float y) => new Vector3(Random.Range(0f, LarguraGrade), y, Random.Range(2f, 7f));

        private float BotaoEm => LinhasDentro + EspacoLinha * Mathf.Max(0, _linhas - 1) + BotaoDepois;

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            for (int i = 0; i < _particulas.Length; i++)
            {
                Vector3 m = _particulas[i];
                m.y -= m.z * dt;
                m.x += Mathf.Sin((Time.unscaledTime * 0.7f + i) * 1.1f) * 2f * dt;
                if (m.y < -2f) m = NovaPoeira(AlturaGrade + 2f);
                _particulas[i] = m;
            }

            float idade = Time.unscaledTime - _abertosEm;
            if (_saindo || idade < BotaoEm + 0.4f) return;

            Keyboard teclado = Keyboard.current;
            Gamepad controle = Gamepad.current;
            bool confirmar =
                (teclado != null && (teclado.enterKey.wasPressedThisFrame || teclado.numpadEnterKey.wasPressedThisFrame
                                      || teclado.spaceKey.wasPressedThisFrame || teclado.eKey.wasPressedThisFrame))
                || (controle != null && (controle.buttonSouth.wasPressedThisFrame || controle.startButton.wasPressedThisFrame));

            if (confirmar) Sair();
        }

        private void Sair()
        {
            if (_saindo) return;
            _saindo = true;
            Sfx.ComprarLoja.Tocar();
            GameManager.Instancia.RestaurarTempo(1f);
            GameManager.Instancia.VoltarAoMenu();
        }

        private void OnGUI()
        {
            GUI.depth = -160;

            float w = Screen.width, h = Screen.height;
            float idade = Time.unscaledTime - _abertosEm;
            int escalaAtual = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(w / LarguraGrade, h / AlturaGrade)));
            float ox = Mathf.Round((w - LarguraGrade * escalaAtual) * 0.5f);
            float oy = Mathf.Round((h - AlturaGrade * escalaAtual) * 0.5f);
            Color valorAnterior = GUI.color;

            // 1. O escuro.
            float escuro = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(idade / TempoEscuro));
            GUI.color = new Color(1f, 1f, 1f, escuro);
            TelaCanvas.Textura(new Rect(0f, 0f, w, h), _vinheta);

            // 2. As particulas subindo.
            Color particula = _esporos ? new Color(0.47f, 1f, 0.67f) : new Color(0.8f, 0.86f, 1f);
            for (int i = 0; i < _particulas.Length; i++)
            {
                float tremeluzir = 0.35f + 0.35f * Mathf.Sin(Time.unscaledTime * 2f + i * 1.7f);
                GUI.color = new Color(particula.r, particula.g, particula.b, tremeluzir * escuro);
                TelaCanvas.Textura(new Rect(ox + Mathf.Floor(_particulas[i].x) * escalaAtual, oy + Mathf.Floor(_particulas[i].y) * escalaAtual, escalaAtual, escalaAtual), Texture2D.whiteTexture);
            }

            // 3. O desenho, o titulo e o friso.
            Subir(_emblema, EmblemaY, EmblemaDentro, 0.9f, 6, idade, escalaAtual, ox, oy);
            Subir(_titulo, TituloY, TituloDentro, 0.6f, 4, idade, escalaAtual, ox, oy);

            if (_divisor != null && idade >= DivisorDentro)
            {
                float aberto = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((idade - DivisorDentro) / 0.6f));
                int cheio = _divisor.width;
                int mostrado = Mathf.Max(2, Mathf.RoundToInt(cheio * aberto));
                int x0 = (LarguraGrade - mostrado) / 2;
                GUI.color = Color.white;
                var uv = new Rect((cheio - mostrado) * 0.5f / cheio, 0f, mostrado / (float)cheio, 1f);
                TelaCanvas.TexturaUV(new Rect(ox + x0 * escalaAtual, oy + DivisorY * escalaAtual, mostrado * escalaAtual, _divisor.height * escalaAtual), _divisor, uv);
            }

            // 4. A historia, uma linha de cada vez.
            for (int i = 0; i < _historia.Length; i++)
            {
                Subir(_historia[i], LinhaY + i * PassoLinha, LinhasDentro + i * EspacoLinha, 0.9f, 2, idade, escalaAtual, ox, oy);
            }

            // 5. O credito e, depois de uns segundos, a placa.
            float ultimoLinha = LinhasDentro + EspacoLinha * Mathf.Max(0, _linhas - 1);
            Subir(_credito, CreditoY, ultimoLinha + 1.2f, 1f, 0, idade, escalaAtual, ox, oy, 0.7f);
            DesenharBotao(idade, escalaAtual, ox, oy);

            // 6. Saindo: o preto por cima.
            if (_saindo)
            {
                GUI.color = Color.black;
                TelaCanvas.Textura(new Rect(0f, 0f, w, h), Texture2D.whiteTexture);
            }

            GUI.color = valorAnterior;
        }

        private void DesenharBotao(float idade, int escalaAtual, float ox, float oy)
        {
            float t = Mathf.Clamp01((idade - BotaoEm) / 0.5f);
            if (t <= 0f || _botao[0] == null) return;

            Event e = Event.current;
            bool aceso = _flutuar || Mathf.Sin(Time.unscaledTime * 3f) > 0.6f;
            Texture2D placa = _botao[aceso ? 1 : 0];
            int x = (LarguraGrade - placa.width) / 2;
            int y = BotaoY + Mathf.RoundToInt((1f - t) * 6f);
            var retangulo = new Rect(ox + x * escalaAtual, oy + y * escalaAtual, placa.width * escalaAtual, placa.height * escalaAtual);
            Desenhar(placa, x, y, escalaAtual, ox, oy, t);

            if (_saindo || t < 1f || e == null) return;
            bool dentro = retangulo.Contains(e.mousePosition);
            if (dentro != _flutuar && (e.type == EventType.MouseMove || e.type == EventType.Repaint))
            {
                if (dentro) Sfx.MoverLoja.Tocar();
                _flutuar = dentro;
            }

            if (dentro && e.type == EventType.MouseDown && e.button == 0)
            {
                e.Use();
                Sair();
            }
        }

        private static void Subir(Texture2D arte, int y, float inicio, float tempo, int distancia,
            float idade, int escalaAtual, float ox, float oy, float alfa = 1f)
        {
            if (arte == null || idade < inicio) return;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((idade - inicio) / tempo));
            Desenhar(arte, (LarguraGrade - arte.width) / 2, y + Mathf.RoundToInt((1f - t) * distancia), escalaAtual, ox, oy, t * alfa);
        }

        private static void Desenhar(Texture2D arte, int x, int y, int escalaAtual, float ox, float oy, float alfa)
        {
            if (arte == null || alfa <= 0f) return;
            GUI.color = new Color(1f, 1f, 1f, alfa);
            TelaCanvas.Textura(new Rect(ox + x * escalaAtual, oy + y * escalaAtual, arte.width * escalaAtual, arte.height * escalaAtual), arte);
        }
    }
}
