using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A tela de morte, toda em pixel art (Resources/DeathScreen, desenhada pelo
    /// DeathScreenArt). O mundo congela e a cena entra em tempos:
    ///
    ///   o escuro desce, com uma vinheta de ferrugem e lascas caindo devagar;
    ///   o coracao de sucata cai do alto, racha e se parte em dois, soltando
    ///   fagulhas; a brasa da vida fica tremendo no vao;
    ///   o titulo de latao ("A GLORIA DE PRATA") sobe, o friso se abre do
    ///   meio e as duas placas entram, uma de cada lado.
    ///
    /// A placa escolhida fica polida, com rubis acesos, e balanca de leve.
    /// Teclado (W/S, setas, Enter/E/Espaco), controle e mouse. Voltar pra
    /// bancada desbota tudo e o Scrap renasce; Sair volta pro menu principal.
    /// </summary>
    public class DeathScreen : MonoBehaviour
    {
        // A tela e montada numa grade de 320x180 e ampliada inteira.
        private const int LarguraGrade = 320;
        private const int AlturaGrade = 180;

        // Os tempos da entrada, em segundos desde a morte.
        private const float TempoEscuro = 0.8f;
        private const float CoracaoDentro = 0.15f;
        private const float PousadoCoracao = 0.6f;
        private const float RachadurasCoracao = 0.85f;
        private const float DivideCoracao = 1.15f;
        private const float TituloDentro = 1.2f;
        private const float DivisorDentro = 1.45f;
        private const float BotoesDentro = 1.7f;
        private const float EntradaDe = 1.95f;
        private const float TempoFechar = 0.45f;

        // Onde cada peca fica na grade (o y e o topo dela).
        private const int CoracaoY = 10;
        private const int TituloY = 66;
        private const int DivisorY = 110;
        private const int BotaoY = 126;
        private const int EspacoBotao = 24;

        private static DeathScreen _instancia;

        /// <summary>Se a tela de morte esta aberta.</summary>
        public static bool Mostrando => _instancia != null && _instancia._aberto;

        private bool _aberto;
        private float _abertosEm;
        private int _selecionado;
        private bool _fechando;
        private float _fechadoEm;
        private Action _ligadoRetorno;
        private float _escalaTempoAnterior = 1f;
        private bool _rachado;
        private bool _divisao;

        private Texture2D[] _coracoes;
        private Texture2D _titulo, _divisor, _vinheta;
        private Texture2D[,] _botoes;

        // As lascas de ferrugem e cinza caindo, em pixels da grade.
        private readonly Vector3[] _particulas = new Vector3[48];
        private readonly Color[] _coresParticula = new Color[48];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar()
        {
            _instancia = null;
        }

        /// <summary>
        /// Abre a tela. <paramref name="ligadoRetorno"/> e chamado quando o jogador
        /// escolhe voltar pra bancada (ja com o tempo correndo de novo).
        /// </summary>
        public static void Mostrar(Action ligadoRetorno)
        {
            if (_instancia == null)
            {
                var hospedeiro = new GameObject("DeathScreen");
                _instancia = hospedeiro.AddComponent<DeathScreen>();
            }

            _instancia.Abrir(ligadoRetorno);
        }

        private void Abrir(Action ligadoRetorno)
        {
            Carregar();

            _ligadoRetorno = ligadoRetorno;
            _aberto = true;
            _fechando = false;
            _rachado = false;
            _divisao = false;
            _selecionado = 0;
            _abertosEm = Time.unscaledTime;

            for (int i = 0; i < _particulas.Length; i++)
            {
                _particulas[i] = NovaPoeira(UnityEngine.Random.Range(0f, AlturaGrade));
                _coresParticula[i] = CorDaPoeira();
            }

            // O mundo para atras da tela.
            _escalaTempoAnterior = GameManager.Instancia.CongelarTempo();
        }

        private void Carregar()
        {
            if (_titulo != null) return;

            _coracoes = new Texture2D[7];
            for (int i = 0; i < _coracoes.Length; i++) _coracoes[i] = CarregarArte("heart_" + i);

            _titulo = CarregarArte("title");
            _divisor = CarregarArte("divider");
            _botoes = new Texture2D[2, 2]
            {
                { CarregarArte("button_back_off"), CarregarArte("button_back_on") },
                { CarregarArte("button_quit_off"), CarregarArte("button_quit_on") },
            };

            _vinheta = Vinheta();
        }

        private static Texture2D CarregarArte(string nome)
        {
            var textura = Resources.Load<Texture2D>("DeathScreen/" + nome);
            if (textura == null) Debug.LogWarning("[Morte] Falta Resources/DeathScreen/" + nome);
            return textura;
        }

        /// <summary>A vinheta: escuro nas bordas, um rubor de ferrugem no meio de baixo.</summary>
        private static Texture2D Vinheta()
        {
            const int w = 64, h = 36;
            var textura = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var pixels = new Color32[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float u = (x + 0.5f) / w - 0.5f;
                    float v = (y + 0.5f) / h - 0.42f;
                    float beirada = Mathf.Clamp01((u * u * 1.6f + v * v * 2.2f) * 1.9f);
                    var centro = new Color(0.20f, 0.06f, 0.05f);
                    var aro = new Color(0.02f, 0.01f, 0.015f);
                    Color c = Color.Lerp(centro, aro, beirada);
                    pixels[x + y * w] = new Color(c.r, c.g, c.b, Mathf.Lerp(0.82f, 0.97f, beirada));
                }
            }

            textura.SetPixels32(pixels);
            textura.Apply();
            return textura;
        }

        /// <summary>Uma lasca nova: x ao acaso, a altura dada, e a velocidade de queda (grade por segundo) no z.</summary>
        private static Vector3 NovaPoeira(float y)
        {
            return new Vector3(UnityEngine.Random.Range(0f, LarguraGrade), y, UnityEngine.Random.Range(4f, 14f));
        }

        private void OnDestroy()
        {
            // Trocou de cena com a tela aberta: o tempo nao pode ficar parado.
            if (_aberto) Time.timeScale = _escalaTempoAnterior;
            if (_instancia == this) _instancia = null;
        }

        // ------------------------------------------------------------------
        // Escolha
        // ------------------------------------------------------------------

        private void Update()
        {
            if (!_aberto) return;

            float dt = Time.unscaledDeltaTime;
            float idade = Time.unscaledTime - _abertosEm;

            // As lascas caem e balancam; as que saem por baixo voltam por cima.
            for (int i = 0; i < _particulas.Length; i++)
            {
                Vector3 m = _particulas[i];
                m.y += m.z * dt;
                m.x += Mathf.Sin((Time.unscaledTime + i) * 1.3f) * 3f * dt;
                if (m.y > AlturaGrade + 2f)
                {
                    m = NovaPoeira(-2f);
                    _coresParticula[i] = CorDaPoeira();
                }

                _particulas[i] = m;
            }

            // A queda do coracao: racha e parte, com som.
            if (!_rachado && idade >= RachadurasCoracao)
            {
                _rachado = true;
                Sfx.RachaduraCristal.Tocar(tom2: 0.6f);
            }

            if (!_divisao && idade >= DivideCoracao)
            {
                _divisao = true;
                Sfx.MetalFeridoChefe.Tocar(tom2: 0.7f);
            }

            if (_fechando)
            {
                if (Time.unscaledTime - _fechadoEm >= TempoFechar) _aberto = false;
                return;
            }

            if (idade < EntradaDe) return;

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

            if (mover != 0)
            {
                _selecionado = (_selecionado + mover + 2) % 2;
                Sfx.MoverLoja.Tocar();
            }

            bool confirmar =
                (teclado != null && (teclado.enterKey.wasPressedThisFrame || teclado.numpadEnterKey.wasPressedThisFrame
                                      || teclado.spaceKey.wasPressedThisFrame || teclado.eKey.wasPressedThisFrame))
                || (controle != null && (controle.buttonSouth.wasPressedThisFrame || controle.startButton.wasPressedThisFrame));

            if (confirmar) Escolher(_selecionado);
        }

        private static Color CorDaPoeira()
        {
            switch (UnityEngine.Random.Range(0, 5))
            {
                case 0: return new Color(1f, 0.55f, 0.22f);
                case 1:
                case 2: return new Color(0.70f, 0.34f, 0.16f);
                default: return new Color(0.42f, 0.32f, 0.30f);
            }
        }

        private void Escolher(int opcao)
        {
            if (_fechando) return;

            if (opcao == 1)
            {
                // Sair: pro menu principal. O progresso fica o da ultima bancada em que salvou.
                Sfx.FecharDialogo.Tocar();
                _fechando = true;
                _fechadoEm = Time.unscaledTime + 10f;
                GameManager.Instancia.RestaurarTempo(_escalaTempoAnterior);
                GameManager.Instancia.VoltarAoMenu();
                return;
            }

            // Volta: o tempo corre de novo e o Scrap renasce; a tela desbota por cima.
            Sfx.ComprarLoja.Tocar();
            _fechando = true;
            _fechadoEm = Time.unscaledTime;
            GameManager.Instancia.RestaurarTempo(_escalaTempoAnterior);
            _ligadoRetorno?.Invoke();
        }

        // ------------------------------------------------------------------
        // Desenho
        // ------------------------------------------------------------------

        private void OnGUI()
        {
            if (!_aberto || _titulo == null) return;

            // Por cima do HUD e da gravura; o loading de cena fica por cima de tudo.
            GUI.depth = -150;

            float w = Screen.width;
            float h = Screen.height;
            float idade = Time.unscaledTime - _abertosEm;

            // O quanto a tela inteira esta la (desbota ao voltar).
            float esmaecimento = _fechando ? 1f - Mathf.Clamp01((Time.unscaledTime - _fechadoEm) / TempoFechar) : 1f;

            // A grade: o maior multiplo inteiro de 320x180 que cabe, centralizado.
            int escalaAtual = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(w / LarguraGrade, h / AlturaGrade)));
            float ox = Mathf.Round((w - LarguraGrade * escalaAtual) * 0.5f);
            float oy = Mathf.Round((h - AlturaGrade * escalaAtual) * 0.5f);

            Color valorAnterior = GUI.color;

            // 1. O escuro e a vinheta, na tela inteira.
            float escuro = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(idade / TempoEscuro)) * esmaecimento;
            GUI.color = new Color(1f, 1f, 1f, escuro);
            TelaCanvas.Textura(new Rect(0f, 0f, w, h), _vinheta);

            // 2. As lascas caindo (um pixel da grade cada).
            for (int i = 0; i < _particulas.Length; i++)
            {
                Color c = _coresParticula[i];
                GUI.color = new Color(c.r, c.g, c.b, 0.75f * escuro);
                TelaCanvas.Textura(new Rect(ox + Mathf.Floor(_particulas[i].x) * escalaAtual, oy + Mathf.Floor(_particulas[i].y) * escalaAtual, escalaAtual, escalaAtual), Texture2D.whiteTexture);
            }

            // 3. O coracao: cai, racha, parte, e a brasa treme.
            if (idade >= CoracaoDentro)
            {
                float queda = Mathf.Clamp01((idade - CoracaoDentro) / (PousadoCoracao - CoracaoDentro));
                float quique = queda < 1f ? 1f - SuavizarComRecuo(queda) : 0f;
                int y = CoracaoY - Mathf.RoundToInt(quique * 70f);

                int quadro = idade < RachadurasCoracao ? 0 : idade < DivideCoracao ? 1 : idade < DivideCoracao + 0.25f ? 2
                    : 3 + (int)((idade - DivideCoracao) * 5f) % 4;

                // Um tranco quando racha e quando parte.
                int tremor = (idade >= RachadurasCoracao && idade < RachadurasCoracao + 0.12f) || (idade >= DivideCoracao && idade < DivideCoracao + 0.18f)
                    ? (Mathf.FloorToInt(idade * 60f) % 2 == 0 ? 1 : -1) : 0;

                Desenhar(_coracoes[quadro], (LarguraGrade - _coracoes[quadro].width) / 2 + tremor, y, escalaAtual, ox, oy, Mathf.Clamp01((idade - CoracaoDentro) / 0.15f) * esmaecimento);
            }

            // 4. O titulo sobe e aparece.
            Subir(_titulo, TituloY, TituloDentro, 0.35f, 5, idade, escalaAtual, ox, oy, esmaecimento);

            // 5. O friso se abre do meio pras pontas.
            if (idade >= DivisorDentro)
            {
                float aberto = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((idade - DivisorDentro) / 0.45f));
                int cheio = _divisor.width;
                int mostrado = Mathf.Max(2, Mathf.RoundToInt(cheio * aberto));
                int x0 = (LarguraGrade - mostrado) / 2;
                GUI.color = new Color(1f, 1f, 1f, esmaecimento);
                var uv = new Rect((cheio - mostrado) * 0.5f / cheio, 0f, mostrado / (float)cheio, 1f);
                TelaCanvas.TexturaUV(new Rect(ox + x0 * escalaAtual, oy + DivisorY * escalaAtual, mostrado * escalaAtual, _divisor.height * escalaAtual), _divisor, uv);
            }

            // 6. As placas entram, uma de cada lado, e a escolhida balanca.
            if (idade >= BotoesDentro) DesenharBotoes(idade, escalaAtual, ox, oy, esmaecimento);

            GUI.color = valorAnterior;
        }

        private void DesenharBotoes(float idade, int escalaAtual, float ox, float oy, float esmaecimento)
        {
            Event e = Event.current;

            for (int i = 0; i < 2; i++)
            {
                float t = Mathf.Clamp01((idade - BotoesDentro - i * 0.12f) / 0.4f);
                if (t <= 0f) continue;

                // Desliza de fora da tela: a de cima pela esquerda, a de baixo pela direita.
                float deslize = 1f - SuavizarComRecuo(t);
                int de = i == 0 ? -1 : 1;
                Texture2D placa = _botoes[i, _selecionado == i ? 1 : 0];

                int x = (LarguraGrade - placa.width) / 2 + Mathf.RoundToInt(deslize * de * 200f);
                int y = BotaoY + i * EspacoBotao;

                // A escolhida sobe e desce um pixel, devagar.
                if (_selecionado == i && t >= 1f) y -= Mathf.Sin(Time.unscaledTime * 4f) > 0.2f ? 1 : 0;

                var retangulo = new Rect(ox + x * escalaAtual, oy + y * escalaAtual, placa.width * escalaAtual, placa.height * escalaAtual);
                Desenhar(placa, x, y, escalaAtual, ox, oy, Mathf.Clamp01(t * 2f) * esmaecimento);

                // O mouse: passar por cima escolhe, clicar confirma.
                if (_fechando || idade < EntradaDe || e == null || !retangulo.Contains(e.mousePosition)) continue;

                if ((e.type == EventType.MouseMove || e.type == EventType.MouseDrag) && _selecionado != i)
                {
                    _selecionado = i;
                    Sfx.MoverLoja.Tocar();
                }

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    _selecionado = i;
                    Escolher(i);
                    e.Use();
                }
            }
        }

        /// <summary>Uma peca que sobe uns pixels da grade enquanto aparece.</summary>
        private static void Subir(Texture2D arte, int y, float inicio, float tempo, int distancia,
            float idade, int escalaAtual, float ox, float oy, float esmaecimento)
        {
            if (arte == null || idade < inicio) return;

            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((idade - inicio) / tempo));
            Desenhar(arte, (LarguraGrade - arte.width) / 2, y + Mathf.RoundToInt((1f - t) * distancia), escalaAtual, ox, oy, t * esmaecimento);
        }

        /// <summary>Desenha uma peca na grade, em pixels inteiros.</summary>
        private static void Desenhar(Texture2D arte, int x, int y, int escalaAtual, float ox, float oy, float alfa)
        {
            if (arte == null || alfa <= 0f) return;

            GUI.color = new Color(1f, 1f, 1f, alfa);
            TelaCanvas.Textura(new Rect(ox + x * escalaAtual, oy + y * escalaAtual, arte.width * escalaAtual, arte.height * escalaAtual), arte);
        }

        private static float SuavizarComRecuo(float t)
        {
            const float c1 = 1.4f;
            const float c3 = c1 + 1f;
            float u = t - 1f;
            return 1f + c3 * u * u * u + c1 * u * u;
        }
    }
}
