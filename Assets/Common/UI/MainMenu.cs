using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// O menu principal, na cena MainMenu (gerada pelo menu Echoes): o
    /// santuario da Bancada de Ourives aceso a esquerda, o titulo de latao e
    /// tres placas a direita.
    ///
    ///   Continuar  carrega o save e poe o Scrap na bancada em que salvou
    ///              (apagada quando nao ha save).
    ///   Novo jogo  zera o progresso e comeca do Ferro-Velho. O save antigo so
    ///              e substituido quando se salvar numa bancada.
    ///   Sair       fecha o jogo.
    ///
    /// Tudo em pixel art (Resources/MainMenu), numa grade de 320x180 ampliada
    /// inteira, como a tela de morte.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        /// <summary>O nome da cena do menu (quem quer voltar pra ele usa este).</summary>
        public const string NomeCena = "MainMenu";

        /// <summary>Onde um jogo novo comeca.</summary>
        public const string PrimeiroCena = "Phase01_ScrapYard";

        private const int LarguraGrade = 320;
        private const int AlturaGrade = 180;
        private const int ColunaX = 212;
        private const int BotaoY = 98;
        private const int EspacoBotao = 24;

        private static readonly string[] Nomes = { "continue", "new", "quit" };

        private Texture2D _pequeno, _grande, _divisor, _brilho;
        private Texture2D[,] _botoes;
        private Texture2D[] _santuario;
        private readonly Vector3[] _particulas = new Vector3[40];
        private readonly Color[] _coresParticula = new Color[40];

        private bool _temSalvar;
        private int _selecionado;
        private float _abertosEm;
        private bool _saindo;

        private void Start()
        {
            // Quem chega aqui de dentro do jogo (pela tela de morte) traz o tempo parado e as cenas no meio.
            Time.timeScale = 1f;
            if (Cinematic.Ativo) Cinematic.Encerrar();

            _temSalvar = SaveSystem.TemSalvar;
            _selecionado = _temSalvar ? 0 : 1;
            _abertosEm = Time.unscaledTime;

            _pequeno = CarregarArte("title_small");
            _grande = CarregarArte("title_big");
            _divisor = CarregarArte("divider");
            _botoes = new Texture2D[3, 2];
            for (int i = 0; i < 3; i++)
            {
                _botoes[i, 0] = CarregarArte($"button_{Nomes[i]}_off");
                _botoes[i, 1] = CarregarArte($"button_{Nomes[i]}_on");
            }

            // O santuario aceso: os quadros da bancada (Resources/Checkpoint).
            _santuario = new Texture2D[6];
            for (int i = 0; i < _santuario.Length; i++) _santuario[i] = Resources.Load<Texture2D>($"Checkpoint/checkpoint_{4 + i:00}");

            _brilho = TexturaDeBrilho();

            for (int i = 0; i < _particulas.Length; i++)
            {
                _particulas[i] = new Vector3(Random.Range(0f, LarguraGrade), Random.Range(0f, AlturaGrade), Random.Range(3f, 9f));
                _coresParticula[i] = Random.value < 0.4f ? new Color(0.62f, 0.78f, 0.95f) : new Color(0.70f, 0.36f, 0.18f);
            }
        }

        /// <summary>Um brilho redondo e suave (o halo da gema do santuario no fundo).</summary>
        private static Texture2D TexturaDeBrilho()
        {
            const int tamanho = 64;
            var textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var pixels = new Color32[tamanho * tamanho];
            for (int y = 0; y < tamanho; y++)
            {
                for (int x = 0; x < tamanho; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(tamanho * 0.5f, tamanho * 0.5f)) / (tamanho * 0.5f);
                    float a = Mathf.Pow(Mathf.Clamp01(1f - d), 2.2f);
                    pixels[x + y * tamanho] = new Color(1f, 1f, 1f, a);
                }
            }

            textura.SetPixels32(pixels);
            textura.Apply();
            return textura;
        }

        private static Texture2D CarregarArte(string nome)
        {
            var textura = Resources.Load<Texture2D>("MainMenu/" + nome);
            if (textura == null) Debug.LogWarning("[Menu] Falta Resources/MainMenu/" + nome);
            return textura;
        }

        // ------------------------------------------------------------------
        // Escolha
        // ------------------------------------------------------------------

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            // A poeira sobe devagar (prata) e a ferrugem cai.
            for (int i = 0; i < _particulas.Length; i++)
            {
                Vector3 m = _particulas[i];
                bool prata = _coresParticula[i].b > 0.5f;
                m.y += (prata ? -m.z : m.z) * dt;
                m.x += Mathf.Sin((Time.unscaledTime + i) * 0.9f) * 2f * dt;
                if (m.y > AlturaGrade + 2f) m.y = -2f;
                if (m.y < -2f) m.y = AlturaGrade + 2f;
                _particulas[i] = m;
            }

            if (_saindo || Time.unscaledTime - _abertosEm < 0.8f) return;

            int mover = 0;
            Keyboard teclado = Keyboard.current;
            Gamepad controle = Gamepad.current;

            if (teclado != null)
            {
                if (teclado.upArrowKey.wasPressedThisFrame || teclado.wKey.wasPressedThisFrame) mover = -1;
                if (teclado.downArrowKey.wasPressedThisFrame || teclado.sKey.wasPressedThisFrame) mover = 1;
            }

            if (controle != null)
            {
                if (controle.dpad.up.wasPressedThisFrame || controle.leftStick.up.wasPressedThisFrame) mover = -1;
                if (controle.dpad.down.wasPressedThisFrame || controle.leftStick.down.wasPressedThisFrame) mover = 1;
            }

            if (mover != 0) Selecionar(Avancar(_selecionado, mover));

            bool confirmar =
                (teclado != null && (teclado.enterKey.wasPressedThisFrame || teclado.numpadEnterKey.wasPressedThisFrame
                                      || teclado.spaceKey.wasPressedThisFrame || teclado.eKey.wasPressedThisFrame))
                || (controle != null && (controle.buttonSouth.wasPressedThisFrame || controle.startButton.wasPressedThisFrame));

            if (confirmar) Escolher(_selecionado);
        }

        /// <summary>A proxima placa nessa direcao, pulando o Continuar quando nao ha save.</summary>
        private int Avancar(int de, int direcao)
        {
            int proximo = de;
            for (int tentativas = 0; tentativas < 3; tentativas++)
            {
                proximo = (proximo + direcao + 3) % 3;
                if (proximo != 0 || _temSalvar) return proximo;
            }

            return de;
        }

        private void Selecionar(int opcao)
        {
            if (opcao == _selecionado || (opcao == 0 && !_temSalvar)) return;
            _selecionado = opcao;
            Sfx.MoverLoja.Tocar();
        }

        private void Escolher(int opcao)
        {
            if (_saindo) return;

            switch (opcao)
            {
                case 0:
                    if (!_temSalvar) return;
                    if (!GameManager.Instancia.ContinuarJogo())
                    {
                        Sfx.NegarLoja.Tocar();
                        return;
                    }

                    Sfx.ComprarLoja.Tocar();
                    _saindo = true;
                    break;

                case 1:
                    Sfx.ComprarLoja.Tocar();
                    _saindo = true;
                    GameManager.Instancia.NovoJogo();
                    break;

                default:
                    Sfx.FecharDialogo.Tocar();
GameManager.Instancia.SairDoJogo();
                    break;
            }
        }

        // ------------------------------------------------------------------
        // Desenho
        // ------------------------------------------------------------------

        private void OnGUI()
        {
            if (_grande == null) return;

            float w = Screen.width;
            float h = Screen.height;
            float idade = Time.unscaledTime - _abertosEm;

            int escalaAtual = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(w / LarguraGrade, h / AlturaGrade)));
            float ox = Mathf.Round((w - LarguraGrade * escalaAtual) * 0.5f);
            float oy = Mathf.Round((h - AlturaGrade * escalaAtual) * 0.5f);
            Color valorAnterior = GUI.color;

            // O fundo: azul de caverna, um brilho onde o santuario esta, e o chao escuro.
            Preencher(new Rect(0f, 0f, w, h), new Color(0.035f, 0.035f, 0.06f));
            float brilho = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 1.2f);
            GUI.color = new Color(0.32f, 0.66f, 0.92f, (0.28f + 0.08f * brilho) * Mathf.Clamp01(idade / 1f));
            TelaCanvas.Textura(new Rect(ox + (70f - 110f) * escalaAtual, oy + (92f - 90f) * escalaAtual, 220f * escalaAtual, 180f * escalaAtual), _brilho);

            Preencher(new Rect(0f, oy + 156f * escalaAtual, w, h - (oy + 156f * escalaAtual)), new Color(0.07f, 0.065f, 0.09f));
            Preencher(new Rect(0f, oy + 156f * escalaAtual, w, escalaAtual), new Color(0.16f, 0.15f, 0.20f));

            // A poeira e a ferrugem no ar.
            for (int i = 0; i < _particulas.Length; i++)
            {
                Color c = _coresParticula[i];
                GUI.color = new Color(c.r, c.g, c.b, 0.7f);
                TelaCanvas.Textura(new Rect(ox + Mathf.Floor(_particulas[i].x) * escalaAtual, oy + Mathf.Floor(_particulas[i].y) * escalaAtual, escalaAtual, escalaAtual), Texture2D.whiteTexture);
            }

            // O santuario aceso, a esquerda, girando a engrenagem.
            Texture2D santuario = _santuario[(int)(Time.unscaledTime * 8f) % _santuario.Length];
            Desenhar(santuario, 22, 60, escalaAtual, ox, oy, Mathf.Clamp01(idade / 0.8f));

            // O titulo, as placas.
            float titulo = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((idade - 0.2f) / 0.6f));
            Desenhar(_pequeno, ColunaX - _pequeno.width / 2, 16 + Mathf.RoundToInt((1f - titulo) * 4f), escalaAtual, ox, oy, titulo);
            Desenhar(_grande, ColunaX - _grande.width / 2, 36 + Mathf.RoundToInt((1f - titulo) * 4f), escalaAtual, ox, oy, titulo);

            float divisor = Mathf.Clamp01((idade - 0.5f) / 0.4f);
            Desenhar(_divisor, ColunaX - _divisor.width / 2, 80, escalaAtual, ox, oy, divisor);

            Event e = Event.current;
            for (int i = 0; i < 3; i++)
            {
                float t = Mathf.Clamp01((idade - 0.6f - i * 0.1f) / 0.35f);
                if (t <= 0f) continue;

                bool on = _selecionado == i;
                bool desativado = i == 0 && !_temSalvar;
                Texture2D placa = _botoes[i, on ? 1 : 0];
                int x = ColunaX - placa.width / 2 + Mathf.RoundToInt((1f - t) * 30f);
                int y = BotaoY + i * EspacoBotao;
                if (on && t >= 1f && Mathf.Sin(Time.unscaledTime * 4f) > 0.2f) y -= 1;

                Desenhar(placa, x, y, escalaAtual, ox, oy, t * (desativado ? 0.35f : 1f));

                // O mouse: passar por cima escolhe, clicar confirma.
                var retangulo = new Rect(ox + x * escalaAtual, oy + y * escalaAtual, placa.width * escalaAtual, placa.height * escalaAtual);
                if (_saindo || desativado || e == null || !retangulo.Contains(e.mousePosition)) continue;

                if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag) Selecionar(i);
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    Selecionar(i);
                    Escolher(i);
                    e.Use();
                }
            }

            GUI.color = valorAnterior;
        }

        private static void Desenhar(Texture2D arte, int x, int y, int escalaAtual, float ox, float oy, float alfa)
        {
            if (arte == null || alfa <= 0f) return;

            GUI.color = new Color(1f, 1f, 1f, alfa);
            TelaCanvas.Textura(new Rect(ox + x * escalaAtual, oy + y * escalaAtual, arte.width * escalaAtual, arte.height * escalaAtual), arte);
        }

        private static void Preencher(Rect retangulo, Color cor)
        {
            GUI.color = cor;
            TelaCanvas.Textura(retangulo, Texture2D.whiteTexture);
        }
    }
}
