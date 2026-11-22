using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace EchoesOfCravejada
{
    /// <summary>
    /// A pausa (Esc ou Start), em pixel art no mesmo latao da tela de morte
    /// (Resources/PauseScreen, desenhada pelo HudIconAssets). O mundo congela,
    /// o escuro desce, "PAUSA" sobe e as placas Continuar / Sair entram.
    ///
    /// Sair abre um aviso: o jogo foi salvo na ultima bancada. Dali, ir pro
    /// menu principal ou voltar ao jogo. Esc volta um passo.
    /// Nasce sozinha em qualquer cena que nao seja o menu.
    /// </summary>
    public class PauseScreen : MonoBehaviour
    {
        private const int LarguraGrade = 320;
        private const int AlturaGrade = 180;

        private const int TituloY = 46;
        private const int DivisorY = 82;
        private const int BotaoY = 98;
        private const int EspacoBotao = 24;
        private const int VolumeY = 148;
        private const int EspacoVolume = 12;
        private const int VolumeX = 126;
        private const int PassoSegmento = 8;

        private static PauseScreen _instancia;

        /// <summary>Se a pausa esta aberta.</summary>
        public static bool Mostrando => _instancia != null && _instancia._aberto;

        private bool _aberto;
        private bool _aviso;
        private float _abertosEm;
        private float _avisoEm;
        private int _selecionado;
        private float _escalaTempoAnterior = 1f;

        private Texture2D _titulo, _divisor, _tituloAviso, _linhaAviso, _sombra;
        private Texture2D[,] _principal, _confirmar;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReiniciarAoJogar() => _instancia = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Criar()
        {
            if (_instancia != null) return;
            var hospedeiro = new GameObject("PauseScreen");
            DontDestroyOnLoad(hospedeiro);
            _instancia = hospedeiro.AddComponent<PauseScreen>();
        }

        private void OnDestroy()
        {
            if (_aberto) Time.timeScale = _escalaTempoAnterior;
            if (_instancia == this) _instancia = null;
        }

        private void Carregar()
        {
            if (_titulo != null) return;

            _titulo = CarregarArte("title");
            _divisor = CarregarArte("divider");
            _tituloAviso = CarregarArte("notice_title");
            _linhaAviso = CarregarArte("notice_line");
            _principal = new Texture2D[2, 2]
            {
                { CarregarArte("button_continue_off"), CarregarArte("button_continue_on") },
                { CarregarArte("button_quit_off"), CarregarArte("button_quit_on") },
            };
            _confirmar = new Texture2D[2, 2]
            {
                { CarregarArte("button_menu_off"), CarregarArte("button_menu_on") },
                { CarregarArte("button_stay_off"), CarregarArte("button_stay_on") },
            };

            _sombra = new Texture2D(1, 1);
            _sombra.SetPixel(0, 0, Color.white);
            _sombra.Apply();
        }

        private static Texture2D CarregarArte(string nome)
        {
            var textura = Resources.Load<Texture2D>("PauseScreen/" + nome);
            if (textura == null) Debug.LogWarning("[Pausa] Falta Resources/PauseScreen/" + nome + " (Echoes/Redesenhar a tela de pausa)");
            return textura;
        }

        /// <summary>Se da pra pausar agora: nada de outra tela por cima, nem cena, nem loading.</summary>
        private static bool PodePausar()
        {
            if (SceneManager.GetActiveScene().name == MainMenu.NomeCena) return false;
            if (DeathScreen.Mostrando || AbilityCard.Mostrando || SceneLoader.Ocupado || SaveEngraving.HudEsconder) return false;
            if (Cinematic.Ativo || ScriptedDialogue.Ativo || Npc.Conversando != null) return false;
            return Time.timeScale > 0f;
        }

        private void Update()
        {
            Keyboard teclado = Keyboard.current;
            Gamepad controle = Gamepad.current;

            bool costas = (teclado != null && teclado.escapeKey.wasPressedThisFrame)
                        || (controle != null && (controle.startButton.wasPressedThisFrame || (_aberto && controle.buttonEast.wasPressedThisFrame)));

            if (!_aberto)
            {
                if (costas && PodePausar()) Abrir();
                return;
            }

            if (costas)
            {
                if (_aviso) FecharAviso();
                else Retomar();
                return;
            }

            int mover = 0;
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
                int linhas = _aviso ? 2 : 4;
                _selecionado = (_selecionado + mover + linhas) % linhas;
                Sfx.MoverLoja.Tocar();
            }

            bool confirmar =
                (teclado != null && (teclado.enterKey.wasPressedThisFrame || teclado.numpadEnterKey.wasPressedThisFrame
                                      || teclado.spaceKey.wasPressedThisFrame || teclado.eKey.wasPressedThisFrame))
                || (controle != null && controle.buttonSouth.wasPressedThisFrame);

            if (!_aviso && _selecionado >= 2)
            {
                int lado = 0;
                if (teclado != null)
                {
                    if (teclado.leftArrowKey.wasPressedThisFrame || teclado.aKey.wasPressedThisFrame) lado = -1;
                    if (teclado.rightArrowKey.wasPressedThisFrame || teclado.dKey.wasPressedThisFrame) lado = 1;
                }

                if (controle != null)
                {
                    if (controle.dpad.left.wasPressedThisFrame || controle.leftStick.left.wasPressedThisFrame) lado = -1;
                    if (controle.dpad.right.wasPressedThisFrame || controle.leftStick.right.wasPressedThisFrame) lado = 1;
                }

                if (lado != 0) DefinirVolume(_selecionado - 2, Passos(_selecionado - 2) + lado);
            }

            if (confirmar && _selecionado < 2) Escolher(_selecionado);
        }

        private void Abrir()
        {
            Carregar();
            _aberto = true;
            _aviso = false;
            _selecionado = 0;
            _abertosEm = Time.unscaledTime;
            _escalaTempoAnterior = GameManager.Instancia.CongelarTempo();
            Sfx.AbertoDialogo.Tocar();
        }

        private void Retomar()
        {
            _aberto = false;
            _aviso = false;
            GameManager.Instancia.RestaurarTempo(_escalaTempoAnterior);
            Sfx.FecharDialogo.Tocar();
        }

        private void FecharAviso()
        {
            _aviso = false;
            _selecionado = 1;
            Sfx.MoverLoja.Tocar();
        }

        private void Escolher(int opcao)
        {
            if (!_aviso)
            {
                if (opcao == 0)
                {
                    Retomar();
                    return;
                }

                _aviso = true;
                _avisoEm = Time.unscaledTime;
                _selecionado = 0;
                Sfx.Aviso.Tocar();
                return;
            }

            if (opcao == 1)
            {
                FecharAviso();
                return;
            }

            // Pro menu: o progresso que vale e o da ultima bancada.
            _aberto = false;
            _aviso = false;
            GameManager.Instancia.RestaurarTempo(_escalaTempoAnterior);
            Sfx.FecharDialogo.Tocar();
            GameManager.Instancia.VoltarAoMenu();
        }

        // ------------------------------------------------------------------
        // Desenho
        // ------------------------------------------------------------------

        private void OnGUI()
        {
            if (!_aberto || _titulo == null) return;

            GUI.depth = -140;

            float w = Screen.width;
            float h = Screen.height;
            float idade = Time.unscaledTime - _abertosEm;

            int escalaAtual = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(w / LarguraGrade, h / AlturaGrade)));
            float ox = Mathf.Round((w - LarguraGrade * escalaAtual) * 0.5f);
            float oy = Mathf.Round((h - AlturaGrade * escalaAtual) * 0.5f);

            Color valorAnterior = GUI.color;

            // O escuro, com um rubor de latao no meio.
            float escuro = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(idade / 0.25f));
            GUI.color = new Color(0.03f, 0.02f, 0.03f, 0.78f * escuro);
            TelaCanvas.Textura(new Rect(0f, 0f, w, h), _sombra);

            float alfaMenu = _aviso ? 0.25f : 1f;

            // "PAUSA" sobe um pouco ao aparecer.
            float subida = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(idade / 0.3f));
            Desenhar(_titulo, (LarguraGrade - _titulo.width) / 2, TituloY + Mathf.RoundToInt((1f - subida) * 5f), escalaAtual, ox, oy, subida * alfaMenu);

            // O friso abre do meio.
            if (_divisor != null)
            {
                float aberto = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((idade - 0.1f) / 0.35f));
                int cheio = _divisor.width;
                int mostrado = Mathf.Max(2, Mathf.RoundToInt(cheio * aberto));
                GUI.color = new Color(1f, 1f, 1f, alfaMenu);
                var uv = new Rect((cheio - mostrado) * 0.5f / cheio, 0f, mostrado / (float)cheio, 1f);
                TelaCanvas.TexturaUV(new Rect(ox + (LarguraGrade - mostrado) / 2 * escalaAtual, oy + DivisorY * escalaAtual, mostrado * escalaAtual, _divisor.height * escalaAtual), _divisor, uv);
            }

            if (!_aviso) DesenharBotoes(_principal, BotaoY, idade - 0.15f, escalaAtual, ox, oy, 1f, true);
            else DesenharBotoes(_principal, BotaoY, 1f, escalaAtual, ox, oy, alfaMenu, false);

            DesenharVolumes(escalaAtual, ox, oy, _aviso ? alfaMenu : Mathf.Clamp01((idade - 0.35f) / 0.25f));

            if (_aviso) DesenharAviso(escalaAtual, ox, oy);

            GUI.color = valorAnterior;
        }

        private static int Passos(int canal) => canal == 0 ? GameAudio.PassosMusica : GameAudio.PassosEfeitos;

        private static void DefinirVolume(int canal, int passos)
        {
            passos = Mathf.Clamp(passos, 0, GameAudio.PassosMax);
            if (passos == Passos(canal)) return;

            if (canal == 0) GameAudio.PassosMusica = passos;
            else GameAudio.PassosEfeitos = passos;
            Sfx.MoverLoja.Tocar();
        }

        /// <summary>Os dois volumes: nota (musica) e alto-falante (efeitos), cada um com dez rebites.</summary>
        private void DesenharVolumes(int escalaAtual, float ox, float oy, float alfa)
        {
            if (alfa <= 0f) return;

            var latao = new Color(0.93f, 0.72f, 0.36f);
            var apagado = new Color(0.55f, 0.36f, 0.16f);
            var vazia = new Color(0.20f, 0.14f, 0.11f);
            Event e = Event.current;

            for (int canal = 0; canal < 2; canal++)
            {
                int y = VolumeY + canal * EspacoVolume;
                bool on = !_aviso && _selecionado == canal + 2;
                Color tinta = on ? latao : apagado;

                if (canal == 0)
                {
                    Caixa(VolumeX - 12, y + 4, 3, 3, tinta, escalaAtual, ox, oy, alfa);
                    Caixa(VolumeX - 10, y, 1, 5, tinta, escalaAtual, ox, oy, alfa);
                    Caixa(VolumeX - 9, y, 3, 1, tinta, escalaAtual, ox, oy, alfa);
                }
                else
                {
                    Caixa(VolumeX - 13, y + 2, 2, 3, tinta, escalaAtual, ox, oy, alfa);
                    Caixa(VolumeX - 11, y + 1, 1, 5, tinta, escalaAtual, ox, oy, alfa);
                    Caixa(VolumeX - 10, y, 1, 7, tinta, escalaAtual, ox, oy, alfa);
                    Caixa(VolumeX - 8, y + 2, 1, 3, tinta, escalaAtual, ox, oy, alfa);
                }

                int preenchido = Passos(canal);
                for (int i = 0; i < GameAudio.PassosMax; i++)
                {
                    int x = VolumeX + i * PassoSegmento;
                    Caixa(x, y, 6, 6, new Color(0.08f, 0.06f, 0.07f), escalaAtual, ox, oy, alfa);
                    Caixa(x + 1, y + 1, 4, 4, i < preenchido ? tinta : vazia, escalaAtual, ox, oy, alfa);
                }

                if (_aviso || e == null || e.type != EventType.MouseDown || e.button != 0) continue;

                var linha = new Rect(ox + (VolumeX - 14) * escalaAtual, oy + y * escalaAtual, (14 + GameAudio.PassosMax * PassoSegmento) * escalaAtual, 7 * escalaAtual);
                if (!linha.Contains(e.mousePosition)) continue;

                _selecionado = canal + 2;
                int celula = Mathf.FloorToInt((e.mousePosition.x - (ox + VolumeX * escalaAtual)) / (PassoSegmento * escalaAtual));
                DefinirVolume(canal, celula < 0 ? 0 : celula + 1);
                e.Use();
            }
        }

        /// <summary>O aviso: uma placa escura com moldura de latao, o recado e as duas escolhas.</summary>
        private void DesenharAviso(int escalaAtual, float ox, float oy)
        {
            float t = Mathf.Clamp01((Time.unscaledTime - _avisoEm) / 0.25f);
            float estalo = SuavizarComRecuo(t);

            const int pw = 200, ph = 110;
            int px = (LarguraGrade - pw) / 2;
            int py = (AlturaGrade - ph) / 2 + Mathf.RoundToInt((1f - estalo) * 12f);

            Caixa(px, py, pw, ph, new Color(0.36f, 0.22f, 0.10f), escalaAtual, ox, oy, t);
            Caixa(px + 1, py + 1, pw - 2, ph - 2, new Color(0.93f, 0.72f, 0.36f), escalaAtual, ox, oy, t);
            Caixa(px + 2, py + 2, pw - 4, ph - 4, new Color(0.55f, 0.36f, 0.16f), escalaAtual, ox, oy, t);
            Caixa(px + 3, py + 3, pw - 6, ph - 6, new Color(0.08f, 0.06f, 0.07f, 0.97f), escalaAtual, ox, oy, t);

            // Rebites nos cantos.
            foreach ((int rx, int ry) in new[] { (px + 5, py + 5), (px + pw - 7, py + 5), (px + 5, py + ph - 7), (px + pw - 7, py + ph - 7) })
                Caixa(rx, ry, 2, 2, new Color(0.93f, 0.72f, 0.36f), escalaAtual, ox, oy, t);

            if (_tituloAviso != null) Desenhar(_tituloAviso, (LarguraGrade - _tituloAviso.width) / 2, py + 9, escalaAtual, ox, oy, t);
            if (_linhaAviso != null) Desenhar(_linhaAviso, (LarguraGrade - _linhaAviso.width) / 2, py + 36, escalaAtual, ox, oy, t);

            DesenharBotoes(_confirmar, py + 58, t >= 1f ? 1f : 0f, escalaAtual, ox, oy, t, true);
        }

        private void DesenharBotoes(Texture2D[,] placas, int topo, float idade, int escalaAtual, float ox, float oy, float alfa, bool ativo)
        {
            Event e = Event.current;

            for (int i = 0; i < 2; i++)
            {
                float t = Mathf.Clamp01((idade - i * 0.08f) / 0.3f);
                if (t <= 0f) continue;

                bool on = ativo && _selecionado == i;
                Texture2D placa = placas[i, on ? 1 : 0];
                if (placa == null) continue;

                int de = i == 0 ? -1 : 1;
                int x = (LarguraGrade - placa.width) / 2 + Mathf.RoundToInt((1f - SuavizarComRecuo(t)) * de * 160f);
                int y = topo + i * EspacoBotao;
                if (on && t >= 1f) y -= Mathf.Sin(Time.unscaledTime * 4f) > 0.2f ? 1 : 0;

                Desenhar(placa, x, y, escalaAtual, ox, oy, Mathf.Clamp01(t * 2f) * alfa);

                if (!ativo || t < 1f || e == null) continue;
                var retangulo = new Rect(ox + x * escalaAtual, oy + y * escalaAtual, placa.width * escalaAtual, placa.height * escalaAtual);
                if (!retangulo.Contains(e.mousePosition)) continue;

                if ((e.type == EventType.MouseMove || e.type == EventType.MouseDrag) && _selecionado != i)
                {
                    _selecionado = i;
                    Sfx.MoverLoja.Tocar();
                }

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    _selecionado = i;
                    e.Use();
                    Escolher(i);
                    return;
                }
            }
        }

        private void Caixa(int x, int y, int bw, int bh, Color cor, int escalaAtual, float ox, float oy, float alfa)
        {
            GUI.color = new Color(cor.r, cor.g, cor.b, cor.a * alfa);
            TelaCanvas.Textura(new Rect(ox + x * escalaAtual, oy + y * escalaAtual, bw * escalaAtual, bh * escalaAtual), _sombra);
        }

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
