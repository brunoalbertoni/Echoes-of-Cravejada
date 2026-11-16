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
        public const string SceneName = "MainMenu";

        /// <summary>Onde um jogo novo comeca.</summary>
        public const string FirstScene = "Phase01_ScrapYard";

        private const int GridWidth = 320;
        private const int GridHeight = 180;
        private const int ColumnX = 212;
        private const int ButtonY = 98;
        private const int ButtonGap = 24;

        private static readonly string[] Names = { "continue", "new", "quit" };

        private Texture2D _small, _big, _divider, _glow;
        private Texture2D[,] _buttons;
        private Texture2D[] _shrine;
        private readonly Vector3[] _motes = new Vector3[40];
        private readonly Color[] _moteColors = new Color[40];

        private bool _hasSave;
        private int _selected;
        private float _openedAt;
        private bool _leaving;

        private void Start()
        {
            // Quem chega aqui de dentro do jogo (pela tela de morte) traz o tempo parado e as cenas no meio.
            Time.timeScale = 1f;
            if (Cinematic.Active) Cinematic.End();

            _hasSave = SaveSystem.HasSave;
            _selected = _hasSave ? 0 : 1;
            _openedAt = Time.unscaledTime;

            _small = Art("title_small");
            _big = Art("title_big");
            _divider = Art("divider");
            _buttons = new Texture2D[3, 2];
            for (int i = 0; i < 3; i++)
            {
                _buttons[i, 0] = Art($"button_{Names[i]}_off");
                _buttons[i, 1] = Art($"button_{Names[i]}_on");
            }

            // O santuario aceso: os quadros da bancada (Resources/Checkpoint).
            _shrine = new Texture2D[6];
            for (int i = 0; i < _shrine.Length; i++) _shrine[i] = Resources.Load<Texture2D>($"Checkpoint/checkpoint_{4 + i:00}");

            _glow = Glow();

            for (int i = 0; i < _motes.Length; i++)
            {
                _motes[i] = new Vector3(Random.Range(0f, GridWidth), Random.Range(0f, GridHeight), Random.Range(3f, 9f));
                _moteColors[i] = Random.value < 0.4f ? new Color(0.62f, 0.78f, 0.95f) : new Color(0.70f, 0.36f, 0.18f);
            }
        }

        /// <summary>Um brilho redondo e suave (o halo da gema do santuario no fundo).</summary>
        private static Texture2D Glow()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(size * 0.5f, size * 0.5f)) / (size * 0.5f);
                    float a = Mathf.Pow(Mathf.Clamp01(1f - d), 2.2f);
                    pixels[x + y * size] = new Color(1f, 1f, 1f, a);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        private static Texture2D Art(string name)
        {
            var texture = Resources.Load<Texture2D>("MainMenu/" + name);
            if (texture == null) Debug.LogWarning("[Menu] Falta Resources/MainMenu/" + name);
            return texture;
        }

        // ------------------------------------------------------------------
        // Escolha
        // ------------------------------------------------------------------

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            // A poeira sobe devagar (prata) e a ferrugem cai.
            for (int i = 0; i < _motes.Length; i++)
            {
                Vector3 m = _motes[i];
                bool silver = _moteColors[i].b > 0.5f;
                m.y += (silver ? -m.z : m.z) * dt;
                m.x += Mathf.Sin((Time.unscaledTime + i) * 0.9f) * 2f * dt;
                if (m.y > GridHeight + 2f) m.y = -2f;
                if (m.y < -2f) m.y = GridHeight + 2f;
                _motes[i] = m;
            }

            if (_leaving || Time.unscaledTime - _openedAt < 0.8f) return;

            int move = 0;
            Keyboard keyboard = Keyboard.current;
            Gamepad pad = Gamepad.current;

            if (keyboard != null)
            {
                if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame) move = -1;
                if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame) move = 1;
            }

            if (pad != null)
            {
                if (pad.dpad.up.wasPressedThisFrame || pad.leftStick.up.wasPressedThisFrame) move = -1;
                if (pad.dpad.down.wasPressedThisFrame || pad.leftStick.down.wasPressedThisFrame) move = 1;
            }

            if (move != 0) Select(Step(_selected, move));

            bool confirm =
                (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame
                                      || keyboard.spaceKey.wasPressedThisFrame || keyboard.eKey.wasPressedThisFrame))
                || (pad != null && (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame));

            if (confirm) Choose(_selected);
        }

        /// <summary>A proxima placa nessa direcao, pulando o Continuar quando nao ha save.</summary>
        private int Step(int from, int direction)
        {
            int next = from;
            for (int tries = 0; tries < 3; tries++)
            {
                next = (next + direction + 3) % 3;
                if (next != 0 || _hasSave) return next;
            }

            return from;
        }

        private void Select(int option)
        {
            if (option == _selected || (option == 0 && !_hasSave)) return;
            _selected = option;
            Sfx.ShopMove.Play();
        }

        private void Choose(int option)
        {
            if (_leaving) return;

            switch (option)
            {
                case 0:
                    if (!_hasSave) return;
                    string scene = SaveSystem.Load();
                    if (string.IsNullOrEmpty(scene) || !Application.CanStreamedLevelBeLoaded(scene))
                    {
                        Sfx.ShopDeny.Play();
                        return;
                    }

                    Sfx.ShopBuy.Play();
                    _leaving = true;
                    SceneLoader.Go(scene, null);
                    break;

                case 1:
                    Sfx.ShopBuy.Play();
                    _leaving = true;
                    SaveSystem.ResetProgress();
                    SceneLoader.Go(FirstScene, null);
                    break;

                default:
                    Sfx.DialogueClose.Play();
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    break;
            }
        }

        // ------------------------------------------------------------------
        // Desenho
        // ------------------------------------------------------------------

        private void OnGUI()
        {
            if (_big == null) return;

            float w = Screen.width;
            float h = Screen.height;
            float age = Time.unscaledTime - _openedAt;

            int scale = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(w / GridWidth, h / GridHeight)));
            float ox = Mathf.Round((w - GridWidth * scale) * 0.5f);
            float oy = Mathf.Round((h - GridHeight * scale) * 0.5f);
            Color previous = GUI.color;

            // O fundo: azul de caverna, um brilho onde o santuario esta, e o chao escuro.
            Fill(new Rect(0f, 0f, w, h), new Color(0.035f, 0.035f, 0.06f));
            float glow = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 1.2f);
            GUI.color = new Color(0.32f, 0.66f, 0.92f, (0.28f + 0.08f * glow) * Mathf.Clamp01(age / 1f));
            GUI.DrawTexture(new Rect(ox + (70f - 110f) * scale, oy + (92f - 90f) * scale, 220f * scale, 180f * scale), _glow);

            Fill(new Rect(0f, oy + 156f * scale, w, h - (oy + 156f * scale)), new Color(0.07f, 0.065f, 0.09f));
            Fill(new Rect(0f, oy + 156f * scale, w, scale), new Color(0.16f, 0.15f, 0.20f));

            // A poeira e a ferrugem no ar.
            for (int i = 0; i < _motes.Length; i++)
            {
                Color c = _moteColors[i];
                GUI.color = new Color(c.r, c.g, c.b, 0.7f);
                GUI.DrawTexture(new Rect(ox + Mathf.Floor(_motes[i].x) * scale, oy + Mathf.Floor(_motes[i].y) * scale, scale, scale), Texture2D.whiteTexture);
            }

            // O santuario aceso, a esquerda, girando a engrenagem.
            Texture2D shrine = _shrine[(int)(Time.unscaledTime * 8f) % _shrine.Length];
            Draw(shrine, 22, 60, scale, ox, oy, Mathf.Clamp01(age / 0.8f));

            // O titulo, as placas.
            float title = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((age - 0.2f) / 0.6f));
            Draw(_small, ColumnX - _small.width / 2, 16 + Mathf.RoundToInt((1f - title) * 4f), scale, ox, oy, title);
            Draw(_big, ColumnX - _big.width / 2, 36 + Mathf.RoundToInt((1f - title) * 4f), scale, ox, oy, title);

            float divider = Mathf.Clamp01((age - 0.5f) / 0.4f);
            Draw(_divider, ColumnX - _divider.width / 2, 80, scale, ox, oy, divider);

            Event e = Event.current;
            for (int i = 0; i < 3; i++)
            {
                float t = Mathf.Clamp01((age - 0.6f - i * 0.1f) / 0.35f);
                if (t <= 0f) continue;

                bool on = _selected == i;
                bool disabled = i == 0 && !_hasSave;
                Texture2D plate = _buttons[i, on ? 1 : 0];
                int x = ColumnX - plate.width / 2 + Mathf.RoundToInt((1f - t) * 30f);
                int y = ButtonY + i * ButtonGap;
                if (on && t >= 1f && Mathf.Sin(Time.unscaledTime * 4f) > 0.2f) y -= 1;

                Draw(plate, x, y, scale, ox, oy, t * (disabled ? 0.35f : 1f));

                // O mouse: passar por cima escolhe, clicar confirma.
                var rect = new Rect(ox + x * scale, oy + y * scale, plate.width * scale, plate.height * scale);
                if (_leaving || disabled || e == null || !rect.Contains(e.mousePosition)) continue;

                if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag) Select(i);
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    Select(i);
                    Choose(i);
                    e.Use();
                }
            }

            GUI.color = previous;
        }

        private static void Draw(Texture2D art, int x, int y, int scale, float ox, float oy, float alpha)
        {
            if (art == null || alpha <= 0f) return;

            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.DrawTexture(new Rect(ox + x * scale, oy + y * scale, art.width * scale, art.height * scale), art);
        }

        private static void Fill(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
        }
    }
}
