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
        private const int GridWidth = 320;
        private const int GridHeight = 180;

        private const float DarkTime = 1.4f;
        private const float EmblemIn = 1.0f;
        private const float TitleIn = 2.0f;
        private const float DividerIn = 2.8f;
        private const float LinesIn = 3.6f;
        private const float LineGap = 1.9f;
        private const float ButtonAfter = 3.5f;

        private const int EmblemY = 4;
        private const int TitleY = 52;
        private const int DividerY = 84;
        private const int LineY = 92;
        private const int LineStep = 10;
        private const int ButtonY = 146;
        private const int CreditY = 168;

        private static EndScreen _instance;

        private string _id;
        private int _lines;
        private bool _spores;
        private float _openedAt;
        private bool _leaving;
        private bool _hover;

        private Texture2D _emblem, _title, _divider, _credit, _vignette;
        private Texture2D[] _story;
        private Texture2D[] _button;

        private readonly Vector3[] _motes = new Vector3[60];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _instance = null;
        }

        /// <summary>Se a tela de fim esta aberta.</summary>
        public static bool Showing => _instance != null;

        /// <summary>Abre a tela do final <paramref name="id"/> (as linhas sao {id}_line0..).</summary>
        public static void Show(string id, int lines, bool spores)
        {
            if (_instance != null) return;
            var host = new GameObject("EndScreen");
            _instance = host.AddComponent<EndScreen>();
            _instance.Open(id, lines, spores);
        }

        private void Open(string id, int lines, bool spores)
        {
            _id = id;
            _lines = lines;
            _spores = spores;
            _openedAt = Time.unscaledTime;

            _emblem = Art(id + "_emblem");
            _title = Art(id + "_title");
            _divider = Art("divider");
            _credit = Art("credit");
            _story = new Texture2D[lines];
            for (int i = 0; i < lines; i++) _story[i] = Art($"{id}_line{i}");
            _button = new[] { Art("button_menu_off"), Art("button_menu_on") };
            _vignette = Vignette(spores);

            for (int i = 0; i < _motes.Length; i++) _motes[i] = NewMote(Random.Range(0f, GridHeight));
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private static Texture2D Art(string name)
        {
            var texture = Resources.Load<Texture2D>("EndScreen/" + name);
            if (texture == null) Debug.LogWarning("[Fim] Falta Resources/EndScreen/" + name);
            return texture;
        }

        /// <summary>O fundo: escuro, com o meio levemente tingido (verde doente ou azul frio).</summary>
        private static Texture2D Vignette(bool sick)
        {
            const int w = 64, h = 36;
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var pixels = new Color32[w * h];
            Color center = sick ? new Color(0.04f, 0.11f, 0.08f) : new Color(0.06f, 0.07f, 0.12f);
            var rim = new Color(0.01f, 0.01f, 0.015f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float u = (x + 0.5f) / w - 0.5f;
                    float v = (y + 0.5f) / h - 0.5f;
                    float edge = Mathf.Clamp01((u * u * 1.6f + v * v * 2.2f) * 1.9f);
                    Color c = Color.Lerp(center, rim, edge);
                    pixels[x + y * w] = new Color(c.r, c.g, c.b, 1f);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>Uma particula nova: x ao acaso, a altura dada, a velocidade de subida no z.</summary>
        private static Vector3 NewMote(float y) => new Vector3(Random.Range(0f, GridWidth), y, Random.Range(2f, 7f));

        private float ButtonAt => LinesIn + LineGap * Mathf.Max(0, _lines - 1) + ButtonAfter;

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            for (int i = 0; i < _motes.Length; i++)
            {
                Vector3 m = _motes[i];
                m.y -= m.z * dt;
                m.x += Mathf.Sin((Time.unscaledTime * 0.7f + i) * 1.1f) * 2f * dt;
                if (m.y < -2f) m = NewMote(GridHeight + 2f);
                _motes[i] = m;
            }

            float age = Time.unscaledTime - _openedAt;
            if (_leaving || age < ButtonAt + 0.4f) return;

            Keyboard keyboard = Keyboard.current;
            Gamepad pad = Gamepad.current;
            bool confirm =
                (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame
                                      || keyboard.spaceKey.wasPressedThisFrame || keyboard.eKey.wasPressedThisFrame))
                || (pad != null && (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame));

            if (confirm) Leave();
        }

        private void Leave()
        {
            if (_leaving) return;
            _leaving = true;
            Sfx.ShopBuy.Play();
            Time.timeScale = 1f;
            SceneLoader.Go(MainMenu.SceneName, null);
        }

        private void OnGUI()
        {
            GUI.depth = -160;

            float w = Screen.width, h = Screen.height;
            float age = Time.unscaledTime - _openedAt;
            int scale = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(w / GridWidth, h / GridHeight)));
            float ox = Mathf.Round((w - GridWidth * scale) * 0.5f);
            float oy = Mathf.Round((h - GridHeight * scale) * 0.5f);
            Color previous = GUI.color;

            // 1. O escuro.
            float dark = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(age / DarkTime));
            GUI.color = new Color(1f, 1f, 1f, dark);
            GUI.DrawTexture(new Rect(0f, 0f, w, h), _vignette);

            // 2. As particulas subindo.
            Color mote = _spores ? new Color(0.47f, 1f, 0.67f) : new Color(0.8f, 0.86f, 1f);
            for (int i = 0; i < _motes.Length; i++)
            {
                float flicker = 0.35f + 0.35f * Mathf.Sin(Time.unscaledTime * 2f + i * 1.7f);
                GUI.color = new Color(mote.r, mote.g, mote.b, flicker * dark);
                GUI.DrawTexture(new Rect(ox + Mathf.Floor(_motes[i].x) * scale, oy + Mathf.Floor(_motes[i].y) * scale, scale, scale), Texture2D.whiteTexture);
            }

            // 3. O desenho, o titulo e o friso.
            Rise(_emblem, EmblemY, EmblemIn, 0.9f, 6, age, scale, ox, oy);
            Rise(_title, TitleY, TitleIn, 0.6f, 4, age, scale, ox, oy);

            if (_divider != null && age >= DividerIn)
            {
                float open = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((age - DividerIn) / 0.6f));
                int full = _divider.width;
                int shown = Mathf.Max(2, Mathf.RoundToInt(full * open));
                int x0 = (GridWidth - shown) / 2;
                GUI.color = Color.white;
                var uv = new Rect((full - shown) * 0.5f / full, 0f, shown / (float)full, 1f);
                GUI.DrawTextureWithTexCoords(new Rect(ox + x0 * scale, oy + DividerY * scale, shown * scale, _divider.height * scale), _divider, uv);
            }

            // 4. A historia, uma linha de cada vez.
            for (int i = 0; i < _story.Length; i++)
            {
                Rise(_story[i], LineY + i * LineStep, LinesIn + i * LineGap, 0.9f, 2, age, scale, ox, oy);
            }

            // 5. O credito e, depois de uns segundos, a placa.
            float lastLine = LinesIn + LineGap * Mathf.Max(0, _lines - 1);
            Rise(_credit, CreditY, lastLine + 1.2f, 1f, 0, age, scale, ox, oy, 0.7f);
            DrawButton(age, scale, ox, oy);

            // 6. Saindo: o preto por cima.
            if (_leaving)
            {
                GUI.color = Color.black;
                GUI.DrawTexture(new Rect(0f, 0f, w, h), Texture2D.whiteTexture);
            }

            GUI.color = previous;
        }

        private void DrawButton(float age, int scale, float ox, float oy)
        {
            float t = Mathf.Clamp01((age - ButtonAt) / 0.5f);
            if (t <= 0f || _button[0] == null) return;

            Event e = Event.current;
            bool lit = _hover || Mathf.Sin(Time.unscaledTime * 3f) > 0.6f;
            Texture2D plate = _button[lit ? 1 : 0];
            int x = (GridWidth - plate.width) / 2;
            int y = ButtonY + Mathf.RoundToInt((1f - t) * 6f);
            var rect = new Rect(ox + x * scale, oy + y * scale, plate.width * scale, plate.height * scale);
            Draw(plate, x, y, scale, ox, oy, t);

            if (_leaving || t < 1f || e == null) return;
            bool inside = rect.Contains(e.mousePosition);
            if (inside != _hover && (e.type == EventType.MouseMove || e.type == EventType.Repaint))
            {
                if (inside) Sfx.ShopMove.Play();
                _hover = inside;
            }

            if (inside && e.type == EventType.MouseDown && e.button == 0)
            {
                e.Use();
                Leave();
            }
        }

        private static void Rise(Texture2D art, int y, float start, float time, int distance,
            float age, int scale, float ox, float oy, float alpha = 1f)
        {
            if (art == null || age < start) return;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((age - start) / time));
            Draw(art, (GridWidth - art.width) / 2, y + Mathf.RoundToInt((1f - t) * distance), scale, ox, oy, t * alpha);
        }

        private static void Draw(Texture2D art, int x, int y, int scale, float ox, float oy, float alpha)
        {
            if (art == null || alpha <= 0f) return;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.DrawTexture(new Rect(ox + x * scale, oy + y * scale, art.width * scale, art.height * scale), art);
        }
    }
}
