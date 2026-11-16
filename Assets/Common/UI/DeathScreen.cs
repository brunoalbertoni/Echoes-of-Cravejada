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
        private const int GridWidth = 320;
        private const int GridHeight = 180;

        // Os tempos da entrada, em segundos desde a morte.
        private const float DarkTime = 0.8f;
        private const float HeartIn = 0.15f;
        private const float HeartLanded = 0.6f;
        private const float HeartCracks = 0.85f;
        private const float HeartSplits = 1.15f;
        private const float TitleIn = 1.2f;
        private const float DividerIn = 1.45f;
        private const float ButtonsIn = 1.7f;
        private const float InputFrom = 1.95f;
        private const float CloseTime = 0.45f;

        // Onde cada peca fica na grade (o y e o topo dela).
        private const int HeartY = 10;
        private const int TitleY = 66;
        private const int DividerY = 110;
        private const int ButtonY = 126;
        private const int ButtonGap = 24;

        private static DeathScreen _instance;

        /// <summary>Se a tela de morte esta aberta.</summary>
        public static bool Showing => _instance != null && _instance._open;

        private bool _open;
        private float _openedAt;
        private int _selected;
        private bool _closing;
        private float _closedAt;
        private Action _onReturn;
        private float _previousTimeScale = 1f;
        private bool _cracked;
        private bool _split;

        private Texture2D[] _hearts;
        private Texture2D _title, _divider, _vignette;
        private Texture2D[,] _buttons;

        // As lascas de ferrugem e cinza caindo, em pixels da grade.
        private readonly Vector3[] _motes = new Vector3[48];
        private readonly Color[] _moteColors = new Color[48];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _instance = null;
        }

        /// <summary>
        /// Abre a tela. <paramref name="onReturn"/> e chamado quando o jogador
        /// escolhe voltar pra bancada (ja com o tempo correndo de novo).
        /// </summary>
        public static void Show(Action onReturn)
        {
            if (_instance == null)
            {
                var host = new GameObject("DeathScreen");
                _instance = host.AddComponent<DeathScreen>();
            }

            _instance.Open(onReturn);
        }

        private void Open(Action onReturn)
        {
            Load();

            _onReturn = onReturn;
            _open = true;
            _closing = false;
            _cracked = false;
            _split = false;
            _selected = 0;
            _openedAt = Time.unscaledTime;

            for (int i = 0; i < _motes.Length; i++)
            {
                _motes[i] = NewMote(UnityEngine.Random.Range(0f, GridHeight));
                _moteColors[i] = MoteColor();
            }

            // O mundo para atras da tela.
            _previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;
        }

        private void Load()
        {
            if (_title != null) return;

            _hearts = new Texture2D[7];
            for (int i = 0; i < _hearts.Length; i++) _hearts[i] = Art("heart_" + i);

            _title = Art("title");
            _divider = Art("divider");
            _buttons = new Texture2D[2, 2]
            {
                { Art("button_back_off"), Art("button_back_on") },
                { Art("button_quit_off"), Art("button_quit_on") },
            };

            _vignette = Vignette();
        }

        private static Texture2D Art(string name)
        {
            var texture = Resources.Load<Texture2D>("DeathScreen/" + name);
            if (texture == null) Debug.LogWarning("[Morte] Falta Resources/DeathScreen/" + name);
            return texture;
        }

        /// <summary>A vinheta: escuro nas bordas, um rubor de ferrugem no meio de baixo.</summary>
        private static Texture2D Vignette()
        {
            const int w = 64, h = 36;
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var pixels = new Color32[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float u = (x + 0.5f) / w - 0.5f;
                    float v = (y + 0.5f) / h - 0.42f;
                    float edge = Mathf.Clamp01((u * u * 1.6f + v * v * 2.2f) * 1.9f);
                    var center = new Color(0.20f, 0.06f, 0.05f);
                    var rim = new Color(0.02f, 0.01f, 0.015f);
                    Color c = Color.Lerp(center, rim, edge);
                    pixels[x + y * w] = new Color(c.r, c.g, c.b, Mathf.Lerp(0.82f, 0.97f, edge));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>Uma lasca nova: x ao acaso, a altura dada, e a velocidade de queda (grade por segundo) no z.</summary>
        private static Vector3 NewMote(float y)
        {
            return new Vector3(UnityEngine.Random.Range(0f, GridWidth), y, UnityEngine.Random.Range(4f, 14f));
        }

        private void OnDestroy()
        {
            // Trocou de cena com a tela aberta: o tempo nao pode ficar parado.
            if (_open) Time.timeScale = _previousTimeScale;
            if (_instance == this) _instance = null;
        }

        // ------------------------------------------------------------------
        // Escolha
        // ------------------------------------------------------------------

        private void Update()
        {
            if (!_open) return;

            float dt = Time.unscaledDeltaTime;
            float age = Time.unscaledTime - _openedAt;

            // As lascas caem e balancam; as que saem por baixo voltam por cima.
            for (int i = 0; i < _motes.Length; i++)
            {
                Vector3 m = _motes[i];
                m.y += m.z * dt;
                m.x += Mathf.Sin((Time.unscaledTime + i) * 1.3f) * 3f * dt;
                if (m.y > GridHeight + 2f)
                {
                    m = NewMote(-2f);
                    _moteColors[i] = MoteColor();
                }

                _motes[i] = m;
            }

            // A queda do coracao: racha e parte, com som.
            if (!_cracked && age >= HeartCracks)
            {
                _cracked = true;
                Sfx.CrystalCrack.Play(pitch: 0.6f);
            }

            if (!_split && age >= HeartSplits)
            {
                _split = true;
                Sfx.BossHurtMetal.Play(pitch: 0.7f);
            }

            if (_closing)
            {
                if (Time.unscaledTime - _closedAt >= CloseTime) _open = false;
                return;
            }

            if (age < InputFrom) return;

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

            if (move != 0)
            {
                _selected = (_selected + move + 2) % 2;
                Sfx.ShopMove.Play();
            }

            bool confirm =
                (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame
                                      || keyboard.spaceKey.wasPressedThisFrame || keyboard.eKey.wasPressedThisFrame))
                || (pad != null && (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame));

            if (confirm) Choose(_selected);
        }

        private static Color MoteColor()
        {
            switch (UnityEngine.Random.Range(0, 5))
            {
                case 0: return new Color(1f, 0.55f, 0.22f);
                case 1:
                case 2: return new Color(0.70f, 0.34f, 0.16f);
                default: return new Color(0.42f, 0.32f, 0.30f);
            }
        }

        private void Choose(int option)
        {
            if (_closing) return;

            if (option == 1)
            {
                // Sair: pro menu principal. O progresso fica o da ultima bancada em que salvou.
                Sfx.DialogueClose.Play();
                _closing = true;
                _closedAt = Time.unscaledTime + 10f;
                Time.timeScale = _previousTimeScale;
                SceneLoader.Go(MainMenu.SceneName, null);
                return;
            }

            // Volta: o tempo corre de novo e o Scrap renasce; a tela desbota por cima.
            Sfx.ShopBuy.Play();
            _closing = true;
            _closedAt = Time.unscaledTime;
            Time.timeScale = _previousTimeScale;
            _onReturn?.Invoke();
        }

        // ------------------------------------------------------------------
        // Desenho
        // ------------------------------------------------------------------

        private void OnGUI()
        {
            if (!_open || _title == null) return;

            // Por cima do HUD e da gravura; o loading de cena fica por cima de tudo.
            GUI.depth = -150;

            float w = Screen.width;
            float h = Screen.height;
            float age = Time.unscaledTime - _openedAt;

            // O quanto a tela inteira esta la (desbota ao voltar).
            float fade = _closing ? 1f - Mathf.Clamp01((Time.unscaledTime - _closedAt) / CloseTime) : 1f;

            // A grade: o maior multiplo inteiro de 320x180 que cabe, centralizado.
            int scale = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(w / GridWidth, h / GridHeight)));
            float ox = Mathf.Round((w - GridWidth * scale) * 0.5f);
            float oy = Mathf.Round((h - GridHeight * scale) * 0.5f);

            Color previous = GUI.color;

            // 1. O escuro e a vinheta, na tela inteira.
            float dark = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(age / DarkTime)) * fade;
            GUI.color = new Color(1f, 1f, 1f, dark);
            GUI.DrawTexture(new Rect(0f, 0f, w, h), _vignette);

            // 2. As lascas caindo (um pixel da grade cada).
            for (int i = 0; i < _motes.Length; i++)
            {
                Color c = _moteColors[i];
                GUI.color = new Color(c.r, c.g, c.b, 0.75f * dark);
                GUI.DrawTexture(new Rect(ox + Mathf.Floor(_motes[i].x) * scale, oy + Mathf.Floor(_motes[i].y) * scale, scale, scale), Texture2D.whiteTexture);
            }

            // 3. O coracao: cai, racha, parte, e a brasa treme.
            if (age >= HeartIn)
            {
                float fall = Mathf.Clamp01((age - HeartIn) / (HeartLanded - HeartIn));
                float bounce = fall < 1f ? 1f - EaseOutBack(fall) : 0f;
                int y = HeartY - Mathf.RoundToInt(bounce * 70f);

                int frame = age < HeartCracks ? 0 : age < HeartSplits ? 1 : age < HeartSplits + 0.25f ? 2
                    : 3 + (int)((age - HeartSplits) * 5f) % 4;

                // Um tranco quando racha e quando parte.
                int shake = (age >= HeartCracks && age < HeartCracks + 0.12f) || (age >= HeartSplits && age < HeartSplits + 0.18f)
                    ? (Mathf.FloorToInt(age * 60f) % 2 == 0 ? 1 : -1) : 0;

                Draw(_hearts[frame], (GridWidth - _hearts[frame].width) / 2 + shake, y, scale, ox, oy, Mathf.Clamp01((age - HeartIn) / 0.15f) * fade);
            }

            // 4. O titulo sobe e aparece.
            Rise(_title, TitleY, TitleIn, 0.35f, 5, age, scale, ox, oy, fade);

            // 5. O friso se abre do meio pras pontas.
            if (age >= DividerIn)
            {
                float open = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((age - DividerIn) / 0.45f));
                int full = _divider.width;
                int shown = Mathf.Max(2, Mathf.RoundToInt(full * open));
                int x0 = (GridWidth - shown) / 2;
                GUI.color = new Color(1f, 1f, 1f, fade);
                var uv = new Rect((full - shown) * 0.5f / full, 0f, shown / (float)full, 1f);
                GUI.DrawTextureWithTexCoords(new Rect(ox + x0 * scale, oy + DividerY * scale, shown * scale, _divider.height * scale), _divider, uv);
            }

            // 6. As placas entram, uma de cada lado, e a escolhida balanca.
            if (age >= ButtonsIn) DrawButtons(age, scale, ox, oy, fade);

            GUI.color = previous;
        }

        private void DrawButtons(float age, int scale, float ox, float oy, float fade)
        {
            Event e = Event.current;

            for (int i = 0; i < 2; i++)
            {
                float t = Mathf.Clamp01((age - ButtonsIn - i * 0.12f) / 0.4f);
                if (t <= 0f) continue;

                // Desliza de fora da tela: a de cima pela esquerda, a de baixo pela direita.
                float slide = 1f - EaseOutBack(t);
                int from = i == 0 ? -1 : 1;
                Texture2D plate = _buttons[i, _selected == i ? 1 : 0];

                int x = (GridWidth - plate.width) / 2 + Mathf.RoundToInt(slide * from * 200f);
                int y = ButtonY + i * ButtonGap;

                // A escolhida sobe e desce um pixel, devagar.
                if (_selected == i && t >= 1f) y -= Mathf.Sin(Time.unscaledTime * 4f) > 0.2f ? 1 : 0;

                var rect = new Rect(ox + x * scale, oy + y * scale, plate.width * scale, plate.height * scale);
                Draw(plate, x, y, scale, ox, oy, Mathf.Clamp01(t * 2f) * fade);

                // O mouse: passar por cima escolhe, clicar confirma.
                if (_closing || age < InputFrom || e == null || !rect.Contains(e.mousePosition)) continue;

                if ((e.type == EventType.MouseMove || e.type == EventType.MouseDrag) && _selected != i)
                {
                    _selected = i;
                    Sfx.ShopMove.Play();
                }

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    _selected = i;
                    Choose(i);
                    e.Use();
                }
            }
        }

        /// <summary>Uma peca que sobe uns pixels da grade enquanto aparece.</summary>
        private static void Rise(Texture2D art, int y, float start, float time, int distance,
            float age, int scale, float ox, float oy, float fade)
        {
            if (art == null || age < start) return;

            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((age - start) / time));
            Draw(art, (GridWidth - art.width) / 2, y + Mathf.RoundToInt((1f - t) * distance), scale, ox, oy, t * fade);
        }

        /// <summary>Desenha uma peca na grade, em pixels inteiros.</summary>
        private static void Draw(Texture2D art, int x, int y, int scale, float ox, float oy, float alpha)
        {
            if (art == null || alpha <= 0f) return;

            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.DrawTexture(new Rect(ox + x * scale, oy + y * scale, art.width * scale, art.height * scale), art);
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.4f;
            const float c3 = c1 + 1f;
            float u = t - 1f;
            return 1f + c3 * u * u * u + c1 * u * u;
        }
    }
}
