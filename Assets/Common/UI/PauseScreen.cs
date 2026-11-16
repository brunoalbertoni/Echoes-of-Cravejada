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
        private const int GridWidth = 320;
        private const int GridHeight = 180;

        private const int TitleY = 46;
        private const int DividerY = 82;
        private const int ButtonY = 98;
        private const int ButtonGap = 24;

        private static PauseScreen _instance;

        /// <summary>Se a pausa esta aberta.</summary>
        public static bool Showing => _instance != null && _instance._open;

        private bool _open;
        private bool _notice;
        private float _openedAt;
        private float _noticeAt;
        private int _selected;
        private float _previousTimeScale = 1f;

        private Texture2D _title, _divider, _noticeTitle, _noticeLine, _shade;
        private Texture2D[,] _main, _confirm;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay() => _instance = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Spawn()
        {
            if (_instance != null) return;
            var host = new GameObject("PauseScreen");
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<PauseScreen>();
        }

        private void OnDestroy()
        {
            if (_open) Time.timeScale = _previousTimeScale;
            if (_instance == this) _instance = null;
        }

        private void Load()
        {
            if (_title != null) return;

            _title = Art("title");
            _divider = Art("divider");
            _noticeTitle = Art("notice_title");
            _noticeLine = Art("notice_line");
            _main = new Texture2D[2, 2]
            {
                { Art("button_continue_off"), Art("button_continue_on") },
                { Art("button_quit_off"), Art("button_quit_on") },
            };
            _confirm = new Texture2D[2, 2]
            {
                { Art("button_menu_off"), Art("button_menu_on") },
                { Art("button_stay_off"), Art("button_stay_on") },
            };

            _shade = new Texture2D(1, 1);
            _shade.SetPixel(0, 0, Color.white);
            _shade.Apply();
        }

        private static Texture2D Art(string name)
        {
            var texture = Resources.Load<Texture2D>("PauseScreen/" + name);
            if (texture == null) Debug.LogWarning("[Pausa] Falta Resources/PauseScreen/" + name + " (Echoes/Redesenhar a tela de pausa)");
            return texture;
        }

        /// <summary>Se da pra pausar agora: nada de outra tela por cima, nem cena, nem loading.</summary>
        private static bool CanPause()
        {
            if (SceneManager.GetActiveScene().name == MainMenu.SceneName) return false;
            if (DeathScreen.Showing || AbilityCard.Showing || SceneLoader.Busy || SaveEngraving.HideHud) return false;
            if (Cinematic.Active || ScriptedDialogue.Active || Npc.Talking != null) return false;
            return Time.timeScale > 0f;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Gamepad pad = Gamepad.current;

            bool back = (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                        || (pad != null && (pad.startButton.wasPressedThisFrame || (_open && pad.buttonEast.wasPressedThisFrame)));

            if (!_open)
            {
                if (back && CanPause()) Open();
                return;
            }

            if (back)
            {
                if (_notice) CloseNotice();
                else Resume();
                return;
            }

            int move = 0;
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
                || (pad != null && pad.buttonSouth.wasPressedThisFrame);

            if (confirm) Choose(_selected);
        }

        private void Open()
        {
            Load();
            _open = true;
            _notice = false;
            _selected = 0;
            _openedAt = Time.unscaledTime;
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            Sfx.DialogueOpen.Play();
        }

        private void Resume()
        {
            _open = false;
            _notice = false;
            Time.timeScale = _previousTimeScale;
            Sfx.DialogueClose.Play();
        }

        private void CloseNotice()
        {
            _notice = false;
            _selected = 1;
            Sfx.ShopMove.Play();
        }

        private void Choose(int option)
        {
            if (!_notice)
            {
                if (option == 0)
                {
                    Resume();
                    return;
                }

                _notice = true;
                _noticeAt = Time.unscaledTime;
                _selected = 0;
                Sfx.Notice.Play();
                return;
            }

            if (option == 1)
            {
                CloseNotice();
                return;
            }

            // Pro menu: o progresso que vale e o da ultima bancada.
            _open = false;
            _notice = false;
            Time.timeScale = _previousTimeScale;
            Sfx.DialogueClose.Play();
            SceneLoader.Go(MainMenu.SceneName, null);
        }

        // ------------------------------------------------------------------
        // Desenho
        // ------------------------------------------------------------------

        private void OnGUI()
        {
            if (!_open || _title == null) return;

            GUI.depth = -140;

            float w = Screen.width;
            float h = Screen.height;
            float age = Time.unscaledTime - _openedAt;

            int scale = Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(w / GridWidth, h / GridHeight)));
            float ox = Mathf.Round((w - GridWidth * scale) * 0.5f);
            float oy = Mathf.Round((h - GridHeight * scale) * 0.5f);

            Color previous = GUI.color;

            // O escuro, com um rubor de latao no meio.
            float dark = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(age / 0.25f));
            GUI.color = new Color(0.03f, 0.02f, 0.03f, 0.78f * dark);
            GUI.DrawTexture(new Rect(0f, 0f, w, h), _shade);

            float menuAlpha = _notice ? 0.25f : 1f;

            // "PAUSA" sobe um pouco ao aparecer.
            float rise = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(age / 0.3f));
            Draw(_title, (GridWidth - _title.width) / 2, TitleY + Mathf.RoundToInt((1f - rise) * 5f), scale, ox, oy, rise * menuAlpha);

            // O friso abre do meio.
            if (_divider != null)
            {
                float open = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((age - 0.1f) / 0.35f));
                int full = _divider.width;
                int shown = Mathf.Max(2, Mathf.RoundToInt(full * open));
                GUI.color = new Color(1f, 1f, 1f, menuAlpha);
                var uv = new Rect((full - shown) * 0.5f / full, 0f, shown / (float)full, 1f);
                GUI.DrawTextureWithTexCoords(new Rect(ox + (GridWidth - shown) / 2 * scale, oy + DividerY * scale, shown * scale, _divider.height * scale), _divider, uv);
            }

            if (!_notice) DrawButtons(_main, ButtonY, age - 0.15f, scale, ox, oy, 1f, true);
            else DrawButtons(_main, ButtonY, 1f, scale, ox, oy, menuAlpha, false);

            if (_notice) DrawNotice(scale, ox, oy);

            GUI.color = previous;
        }

        /// <summary>O aviso: uma placa escura com moldura de latao, o recado e as duas escolhas.</summary>
        private void DrawNotice(int scale, float ox, float oy)
        {
            float t = Mathf.Clamp01((Time.unscaledTime - _noticeAt) / 0.25f);
            float pop = EaseOutBack(t);

            const int pw = 200, ph = 110;
            int px = (GridWidth - pw) / 2;
            int py = (GridHeight - ph) / 2 + Mathf.RoundToInt((1f - pop) * 12f);

            Box(px, py, pw, ph, new Color(0.36f, 0.22f, 0.10f), scale, ox, oy, t);
            Box(px + 1, py + 1, pw - 2, ph - 2, new Color(0.93f, 0.72f, 0.36f), scale, ox, oy, t);
            Box(px + 2, py + 2, pw - 4, ph - 4, new Color(0.55f, 0.36f, 0.16f), scale, ox, oy, t);
            Box(px + 3, py + 3, pw - 6, ph - 6, new Color(0.08f, 0.06f, 0.07f, 0.97f), scale, ox, oy, t);

            // Rebites nos cantos.
            foreach ((int rx, int ry) in new[] { (px + 5, py + 5), (px + pw - 7, py + 5), (px + 5, py + ph - 7), (px + pw - 7, py + ph - 7) })
                Box(rx, ry, 2, 2, new Color(0.93f, 0.72f, 0.36f), scale, ox, oy, t);

            if (_noticeTitle != null) Draw(_noticeTitle, (GridWidth - _noticeTitle.width) / 2, py + 9, scale, ox, oy, t);
            if (_noticeLine != null) Draw(_noticeLine, (GridWidth - _noticeLine.width) / 2, py + 36, scale, ox, oy, t);

            DrawButtons(_confirm, py + 58, t >= 1f ? 1f : 0f, scale, ox, oy, t, true);
        }

        private void DrawButtons(Texture2D[,] plates, int top, float age, int scale, float ox, float oy, float alpha, bool active)
        {
            Event e = Event.current;

            for (int i = 0; i < 2; i++)
            {
                float t = Mathf.Clamp01((age - i * 0.08f) / 0.3f);
                if (t <= 0f) continue;

                bool on = active && _selected == i;
                Texture2D plate = plates[i, on ? 1 : 0];
                if (plate == null) continue;

                int from = i == 0 ? -1 : 1;
                int x = (GridWidth - plate.width) / 2 + Mathf.RoundToInt((1f - EaseOutBack(t)) * from * 160f);
                int y = top + i * ButtonGap;
                if (on && t >= 1f) y -= Mathf.Sin(Time.unscaledTime * 4f) > 0.2f ? 1 : 0;

                Draw(plate, x, y, scale, ox, oy, Mathf.Clamp01(t * 2f) * alpha);

                if (!active || t < 1f || e == null) continue;
                var rect = new Rect(ox + x * scale, oy + y * scale, plate.width * scale, plate.height * scale);
                if (!rect.Contains(e.mousePosition)) continue;

                if ((e.type == EventType.MouseMove || e.type == EventType.MouseDrag) && _selected != i)
                {
                    _selected = i;
                    Sfx.ShopMove.Play();
                }

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    _selected = i;
                    e.Use();
                    Choose(i);
                    return;
                }
            }
        }

        private void Box(int x, int y, int bw, int bh, Color color, int scale, float ox, float oy, float alpha)
        {
            GUI.color = new Color(color.r, color.g, color.b, color.a * alpha);
            GUI.DrawTexture(new Rect(ox + x * scale, oy + y * scale, bw * scale, bh * scale), _shade);
        }

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
