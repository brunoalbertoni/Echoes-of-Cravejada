using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>
    /// Toca um filme de camadas (Film.cs) em tela cheia, por cima do jogo:
    /// planos com camera, animacao, particulas, claroes e som, com faixas
    /// pretas de cinema. Os quadros vem do editor (FilmAssets), legiveis.
    ///
    /// Termina de um de dois jeitos:
    ///   endWhite   sobe ao branco; no pico chama onPeak e desbota sobre o
    ///              jogo (a transformacao do Rei: ele ja e o outro).
    ///   no preto   chama onPeak no preto e segura o preto um pouco,
    ///              desbotando devagar (o cartao de FIM entra por baixo).
    ///
    /// Pular (espaco, J, X, Enter ou A/X no controle) depois do primeiro
    /// segundo vai pro fim do ultimo plano.
    /// </summary>
    public class FilmCutscene : MonoBehaviour
    {
        private const int Width = FilmCompositor.Width;
        private const int Height = FilmCompositor.Height;
        private const float BarFraction = 0.068f;
        private const float SkipAfter = 1f;
        private const float WhiteRise = 0.5f;
        private const float WhiteFade = 0.5f;
        private const float BlackHold = 2.2f;

        [SerializeField] private FilmShot[] shots;
        [SerializeField] private Texture2D[] frames;
        [SerializeField] private bool endWhite = true;

        private List<FilmFrame> _frames;
        private uint[] _buffer;
        private Color32[] _colors;
        private Texture2D _screen;
        private bool _showPanel;
        private float _white;
        private float _black;

        /// <summary>Se algum filme esta na tela.</summary>
        public static bool IsPlaying { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            IsPlaying = false;
        }

        /// <summary>Usado pelos geradores de fase.</summary>
        public void Configure(FilmShot[] newShots, Texture2D[] newFrames, bool newEndWhite)
        {
            shots = newShots;
            frames = newFrames;
            endWhite = newEndWhite;
        }

        public bool IsReady
        {
            get
            {
                if (shots == null || shots.Length == 0 || frames == null || frames.Length == 0) return false;
                foreach (Texture2D frame in frames)
                {
                    if (frame == null || !frame.isReadable) return false;
                }

                return true;
            }
        }

        public IEnumerator Play(Action onPeak)
        {
            bool peaked = false;

            void PeakOnce()
            {
                if (peaked) return;
                peaked = true;
                onPeak?.Invoke();
            }

            if (!IsReady)
            {
                PeakOnce();
                yield break;
            }

            Prepare();
            IsPlaying = true;
            _showPanel = true;
            _white = 0f;
            _black = 0f;

            int last = shots.Length - 1;
            int index = 0;
            float t = 0f;
            float elapsed = 0f;
            float lastT = -1f;
            bool skipped = false;

            while (index < shots.Length)
            {
                FilmShot shot = shots[index];

                if (!skipped && elapsed > SkipAfter && SkipPressed())
                {
                    skipped = true;
                    index = last;
                    shot = shots[last];
                    t = Mathf.Max(0f, shot.duration - (endWhite ? WhiteRise + 0.2f : 1f));
                    lastT = t;
                }

                if (!skipped) Cues(shot, lastT, t);
                lastT = t;

                if (endWhite && index == last)
                {
                    float rise = (t - (shot.duration - WhiteRise)) / WhiteRise;
                    _white = Mathf.Clamp01(rise);
                    if (rise > 0f && rise - Time.deltaTime / WhiteRise <= 0f) Sfx.CartoonFlash.Play();
                }

                FilmCompositor.Render(shot, _frames, t, _buffer);
                Upload();

                yield return null;

                t += Time.deltaTime;
                elapsed += Time.deltaTime;

                if (t >= shot.duration)
                {
                    index++;
                    t = 0f;
                    lastT = -1f;
                }
            }

            if (endWhite)
            {
                _white = 1f;
                PeakOnce();
                _showPanel = false;
                for (float f = 0f; f < WhiteFade; f += Time.deltaTime)
                {
                    _white = 1f - f / WhiteFade;
                    yield return null;
                }
            }
            else
            {
                _black = 1f;
                _showPanel = false;
                PeakOnce();
                StartCoroutine(ReleaseBlack());
                yield break;
            }

            Stop();
        }

        private IEnumerator ReleaseBlack()
        {
            for (float f = 0f; f < BlackHold; f += Time.deltaTime)
            {
                _black = 1f - f / BlackHold;
                yield return null;
            }

            Stop();
        }

        private static readonly Dictionary<string, SfxCue> Sounds = new Dictionary<string, SfxCue>();

        private static void Cues(FilmShot shot, float from, float to)
        {
            if (shot.cues == null) return;

            foreach (FilmCue cue in shot.cues)
            {
                if (cue == null || string.IsNullOrEmpty(cue.sound)) continue;
                if (cue.t <= from || cue.t > to) continue;
                if (from < 0f && cue.t < 0f) continue;

                SfxCue sound = Find(cue.sound);
                sound?.Play(null, cue.volume, cue.pitch);
            }
        }

        private static SfxCue Find(string name)
        {
            if (Sounds.TryGetValue(name, out SfxCue cached)) return cached;
            FieldInfo field = typeof(Sfx).GetField(name, BindingFlags.Public | BindingFlags.Static);
            SfxCue cue = field != null ? field.GetValue(null) as SfxCue : null;
            if (cue == null) Debug.LogWarning($"[Film] Som '{name}' nao existe em Sfx.");
            Sounds[name] = cue;
            return cue;
        }

        private void Prepare()
        {
            if (_frames != null) return;

            _frames = new List<FilmFrame>(frames.Length);
            foreach (Texture2D texture in frames)
            {
                Color32[] colors = texture.GetPixels32();
                int w = texture.width, h = texture.height;
                var pixels = new uint[w * h];
                for (int y = 0; y < h; y++)
                {
                    int src = (h - 1 - y) * w;
                    int dst = y * w;
                    for (int x = 0; x < w; x++)
                    {
                        Color32 c = colors[src + x];
                        pixels[dst + x] = (uint)c.a << 24 | (uint)c.r << 16 | (uint)c.g << 8 | c.b;
                    }
                }

                _frames.Add(new FilmFrame(w, h, pixels));
            }

            _buffer = new uint[Width * Height];
            _colors = new Color32[Width * Height];
            _screen = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
        }

        private void Upload()
        {
            for (int y = 0; y < Height; y++)
            {
                int src = y * Width;
                int dst = (Height - 1 - y) * Width;
                for (int x = 0; x < Width; x++)
                {
                    uint p = _buffer[src + x];
                    _colors[dst + x] = new Color32((byte)(p >> 16), (byte)(p >> 8), (byte)p, 255);
                }
            }

            _screen.SetPixels32(_colors);
            _screen.Apply(false);
        }

        private void Stop()
        {
            _white = 0f;
            _black = 0f;
            _showPanel = false;
            IsPlaying = false;
        }

        private void OnDisable()
        {
            Stop();
        }

        private void OnDestroy()
        {
            IsPlaying = false;
            if (_screen != null) Destroy(_screen);
        }

        private static bool SkipPressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.jKey.wasPressedThisFrame
                                     || keyboard.xKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame))
            {
                return true;
            }

            Gamepad pad = Gamepad.current;
            return pad != null && (pad.buttonSouth.wasPressedThisFrame || pad.buttonWest.wasPressedThisFrame);
        }

        private void OnGUI()
        {
            if (!IsPlaying) return;
            if (Event.current.type != EventType.Repaint) return;

            GUI.depth = -1000;
            Color previous = GUI.color;
            var full = new Rect(0f, 0f, Screen.width, Screen.height);

            if (_showPanel)
            {
                Fill(full, Color.black);

                float scale = Mathf.Min(Screen.width / (float)Width, Screen.height / (float)Height);
                float w = Width * scale, h = Height * scale;
                var panel = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

                if (_screen != null)
                {
                    GUI.color = Color.white;
                    GUI.DrawTexture(panel, _screen);
                }

                float bar = h * BarFraction;
                Fill(new Rect(panel.x, panel.y, w, bar), Color.black);
                Fill(new Rect(panel.x, panel.yMax - bar, w, bar), Color.black);
            }

            if (_black > 0f) Fill(full, new Color(0f, 0f, 0f, _black));
            if (_white > 0f) Fill(full, new Color(1f, 0.98f, 0.92f, _white));

            GUI.color = previous;
        }

        private static void Fill(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
        }
    }
}
