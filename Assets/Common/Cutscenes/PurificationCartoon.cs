using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoesOfCravejada
{
    /// <summary>Uma fonte de particulas de um quadro da cutscene, em pixels do quadro (y pra baixo).</summary>
    [Serializable]
    public class CartoonEmitter
    {
        public enum Kind { Flakes, Sparks, Steam, Glint }

        public Kind kind;
        public RectInt area;

        [Tooltip("Em que ponto da transformacao (0 a 1) ela comeca.")]
        public float startAt;
    }

    /// <summary>
    /// Um quadro da cutscene: o "antes", o "depois" e o mapa de ordem (canal
    /// vermelho, 0 a 254: quem e mais escuro muda primeiro; 255 nunca muda).
    /// Todos 320x180, legiveis (Read/Write) e com filtro de pixel.
    /// </summary>
    [Serializable]
    public class CartoonShot
    {
        public Texture2D before;
        public Texture2D after;
        public Texture2D order;

        [Tooltip("Quadros repetidos depois da transformacao (a capa na pose final). Vazio: nenhum.")]
        public Texture2D[] loop;

        public CartoonEmitter[] emitters;
        public float duration = 3f;

        [Tooltip("Quanto a camera desliza ao longo do quadro, em pixels do quadro (y pra baixo).")]
        public Vector2 pan;

        public float zoomFrom = 1f;
        public float zoomTo = 1f;
    }

    /// <summary>
    /// A cutscene desenhada da purificacao do Scrap (fim da Phase 1): sai da
    /// camera do jogo e passa quadros ilustrados em tela cheia, como um
    /// desenho animado. Em cada quadro a sucata vira o polido parte por parte,
    /// na ordem do mapa, com uma faixa de luz na borda e lascas e faiscas
    /// saindo exatamente de onde a luz esta passando.
    ///
    /// No fim, um clarao branco: no pico dele o Scrap troca de visual de
    /// verdade (onPolish), a cutscene some e o branco desbota sobre o jogo.
    ///
    /// Da pra pular com pular ou atacar depois do primeiro segundo: vai direto
    /// pro fim da pose final, e a troca acontece do mesmo jeito.
    ///
    /// A arte vem do editor (PurificationCartoonArt + CartoonAssets).
    /// </summary>
    public class PurificationCartoon : MonoBehaviour
    {
        public const int Width = 320;
        public const int Height = 180;

        /// <summary>Altura de cada faixa preta, em fracao da altura do quadro.</summary>
        private const float BarFraction = 0.068f;

        /// <summary>Parte do quadro em que a transformacao acontece; o resto segura o resultado.</summary>
        private const float RevealShare = 0.75f;

        private const float EdgeBand = 14f;
        private const float SkipAfter = 1f;
        private const float FinalFlashStart = 0.6f;
        private const float FinalFlashRise = 0.35f;
        private const float FadeOutTime = 0.45f;
        private const int CutFrames = 3;
        private const int MaxParticles = 500;

        private static readonly Color32 EdgeColor = new Color32(255, 240, 190, 255);
        private static readonly Color32 CutColor = new Color32(255, 247, 224, 255);

        [SerializeField] private CartoonShot[] shots;

        /// <summary>Se alguma cutscene desenhada esta na tela.</summary>
        public static bool IsPlaying { get; private set; }

        private struct Particle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Age;
            public float Life;
            public Color32 Color;
            public float Size;
            public float Grow;
            public float Gravity;
            public bool Fade;
        }

        private struct ShotPixels
        {
            public Color32[] Before;
            public Color32[] After;
            public byte[] Order;
            public Color32[][] Loop;
        }

        private ShotPixels[] _pixels;
        private Color32[] _buffer;
        private Texture2D _screen;
        private readonly Particle[] _particles = new Particle[MaxParticles];
        private int _particleCount;
        private System.Random _random = new System.Random(7);

        private bool _showPanel;
        private float _white;
        private int _cut;
        private Rect _uv = new Rect(0f, 0f, 1f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            IsPlaying = false;
        }

        /// <summary>Usado pelo gerador da Phase 1.</summary>
        public void Configure(CartoonShot[] newShots)
        {
            shots = newShots;
        }

        /// <summary>Se todos os quadros estao presentes, legiveis e no tamanho certo.</summary>
        public bool IsReady
        {
            get
            {
                if (shots == null || shots.Length == 0) return false;

                foreach (CartoonShot shot in shots)
                {
                    if (shot == null || shot.duration <= 0f) return false;
                    if (!Usable(shot.before) || !Usable(shot.after) || !Usable(shot.order)) return false;

                    if (shot.loop == null) continue;
                    foreach (Texture2D frame in shot.loop)
                    {
                        if (!Usable(frame)) return false;
                    }
                }

                return true;
            }
        }

        private static bool Usable(Texture2D texture)
        {
            return texture != null && texture.isReadable && texture.width == Width && texture.height == Height;
        }

        /// <summary>
        /// Passa a cutscene inteira. onPolish e chamado uma vez, no pico do
        /// clarao final, com a tela toda branca.
        /// </summary>
        public IEnumerator Play(Action onPolish)
        {
            bool polished = false;

            void PolishOnce()
            {
                if (polished) return;
                polished = true;
                onPolish?.Invoke();
            }

            if (!IsReady)
            {
                PolishOnce();
                yield break;
            }

            Prepare();
            IsPlaying = true;
            _showPanel = true;
            _white = 1f;
            _cut = 0;

            int index = 0;
            float t = 0f;
            float elapsed = 0f;
            bool skipped = false;
            int last = shots.Length - 1;

            while (index < shots.Length)
            {
                CartoonShot shot = shots[index];

                if (!skipped && elapsed > SkipAfter && SkipPressed())
                {
                    skipped = true;
                    index = last;
                    shot = shots[last];
                    t = Mathf.Max(0f, shot.duration - 0.8f);
                    _particleCount = 0;
                    _cut = CutFrames;
                }

                // O clarao do inicio desbota rapido; o do fim sobe ate o branco.
                _white = Mathf.MoveTowards(_white, 0f, Time.deltaTime * 5f);

                if (index == last)
                {
                    float rise = (t - (shot.duration - FinalFlashStart)) / FinalFlashRise;
                    if (rise > 0f) _white = Mathf.Max(_white, Mathf.Clamp01(rise));
                    if (rise >= 1f) PolishOnce();
                }

                Compose(index, shot, t);

                yield return null;

                if (_cut > 0) _cut--;

                t += Time.deltaTime;
                elapsed += Time.deltaTime;

                if (t >= shot.duration)
                {
                    index++;
                    t = 0f;
                    _particleCount = 0;
                    if (index < shots.Length) _cut = CutFrames;
                }
            }

            // A tela esta branca: troca o Scrap, tira o quadro e desbota sobre o jogo.
            PolishOnce();
            _white = 1f;
            _showPanel = false;

            for (float f = 0f; f < FadeOutTime; f += Time.deltaTime)
            {
                _white = 1f - f / FadeOutTime;
                yield return null;
            }

            Stop();
        }

        private void Stop()
        {
            _white = 0f;
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

        /// <summary>Le os pixels de todos os quadros uma vez.</summary>
        private void Prepare()
        {
            if (_pixels != null) return;

            _pixels = new ShotPixels[shots.Length];
            for (int i = 0; i < shots.Length; i++)
            {
                CartoonShot shot = shots[i];
                Color32[] order = shot.order.GetPixels32();
                var orderBytes = new byte[order.Length];
                for (int k = 0; k < order.Length; k++) orderBytes[k] = order[k].r;

                Color32[][] loop = null;
                if (shot.loop != null && shot.loop.Length > 0)
                {
                    loop = new Color32[shot.loop.Length][];
                    for (int k = 0; k < shot.loop.Length; k++) loop[k] = shot.loop[k].GetPixels32();
                }

                _pixels[i] = new ShotPixels
                {
                    Before = shot.before.GetPixels32(),
                    After = shot.after.GetPixels32(),
                    Order = orderBytes,
                    Loop = loop,
                };
            }

            _buffer = new Color32[Width * Height];
            _screen = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
        }

        // ------------------------------------------------------------------
        // Montagem de cada quadro
        // ------------------------------------------------------------------

        private void Compose(int index, CartoonShot shot, float t)
        {
            ShotPixels pixels = _pixels[index];
            float progress = Mathf.Clamp01(t / (shot.duration * RevealShare));
            float threshold = progress * 270f;

            if (progress >= 1f && pixels.Loop != null)
            {
                Color32[] frame = pixels.Loop[(int)(t * 6f) % pixels.Loop.Length];
                Array.Copy(frame, _buffer, _buffer.Length);
            }
            else if (progress >= 1f)
            {
                Array.Copy(pixels.After, _buffer, _buffer.Length);
            }
            else
            {
                Reveal(pixels, threshold);
            }

            Emit(pixels, shot, progress, threshold, t);
            StepParticles(Time.deltaTime);
            DrawParticles();

            _screen.SetPixels32(_buffer);
            _screen.Apply(false);

            // Camera dentro do quadro: aproxima/afasta e desliza.
            float k = Mathf.Clamp01(t / shot.duration);
            float zoom = Mathf.Max(1f, Mathf.Lerp(shot.zoomFrom, shot.zoomTo, Smooth(k)));
            float w = 1f / zoom, h = 1f / zoom;
            float cx = 0.5f + shot.pan.x * k / Width;
            float cy = 0.5f - shot.pan.y * k / Height;
            _uv = new Rect(Mathf.Clamp(cx - w / 2f, 0f, 1f - w), Mathf.Clamp(cy - h / 2f, 0f, 1f - h), w, h);
        }

        private void Reveal(ShotPixels pixels, float threshold)
        {
            Color32[] before = pixels.Before, after = pixels.After;
            byte[] order = pixels.Order;
            float edgeStart = threshold - EdgeBand;

            for (int i = 0; i < _buffer.Length; i++)
            {
                byte o = order[i];
                if (o == 255 || o >= threshold)
                {
                    _buffer[i] = before[i];
                }
                else if (o < edgeStart)
                {
                    _buffer[i] = after[i];
                }
                else
                {
                    Color32 a = after[i];
                    _buffer[i] = new Color32(
                        (byte)(a.r * 0.4f + EdgeColor.r * 0.6f),
                        (byte)(a.g * 0.4f + EdgeColor.g * 0.6f),
                        (byte)(a.b * 0.4f + EdgeColor.b * 0.6f),
                        255);
                }
            }
        }

        private static float Smooth(float k) => k * k * (3f - 2f * k);

        // ------------------------------------------------------------------
        // Particulas
        // ------------------------------------------------------------------

        private void Emit(ShotPixels pixels, CartoonShot shot, float progress, float threshold, float t)
        {
            if (shot.emitters == null) return;

            float dt = Time.deltaTime;

            foreach (CartoonEmitter emitter in shot.emitters)
            {
                if (emitter == null || progress < emitter.startAt) continue;
                RectInt area = emitter.area;

                switch (emitter.kind)
                {
                    case CartoonEmitter.Kind.Flakes:
                    case CartoonEmitter.Kind.Sparks:
                    {
                        // So enquanto a transformacao corre: as particulas nascem na borda dela.
                        if (progress >= 1f) break;

                        bool flakes = emitter.kind == CartoonEmitter.Kind.Flakes;
                        int tries = Mathf.CeilToInt((flakes ? 1600f : 900f) * dt);

                        for (int n = 0; n < tries; n++)
                        {
                            int x = area.x + _random.Next(0, Mathf.Max(1, area.width));
                            int y = area.y + _random.Next(0, Mathf.Max(1, area.height));
                            int i = Index(x, y);
                            if (i < 0) continue;

                            byte o = pixels.Order[i];
                            if (o == 255 || o >= threshold || o < threshold - EdgeBand * 1.6f) continue;

                            if (flakes) SpawnFlake(x, y, pixels.Before[i]);
                            else if (_random.NextDouble() < 0.35) SpawnSpark(x, y);
                        }

                        break;
                    }

                    case CartoonEmitter.Kind.Steam:
                    {
                        int count = Mathf.CeilToInt(70f * dt);
                        for (int n = 0; n < count; n++) SpawnSteam(area);
                        break;
                    }

                    case CartoonEmitter.Kind.Glint:
                    {
                        float since = t - emitter.startAt * shot.duration * RevealShare;
                        DrawGlint(area, since / 0.4f);
                        break;
                    }
                }
            }
        }

        private void SpawnFlake(int x, int y, Color32 color)
        {
            Add(new Particle
            {
                Position = new Vector2(x, y),
                Velocity = new Vector2(Range(-22f, 22f), Range(-34f, -6f)),
                Life = Range(0.6f, 1.2f),
                Color = color,
                Size = _random.NextDouble() < 0.3 ? 2f : 1f,
                Gravity = 110f,
            });
        }

        private void SpawnSpark(int x, int y)
        {
            bool cyan = _random.NextDouble() < 0.5;
            Add(new Particle
            {
                Position = new Vector2(x, y),
                Velocity = new Vector2(Range(-10f, 10f), Range(-42f, -18f)),
                Life = Range(0.35f, 0.8f),
                Color = cyan ? new Color32(170, 240, 255, 255) : new Color32(255, 250, 220, 255),
                Size = 1f,
                Gravity = -10f,
                Fade = true,
            });
        }

        private void SpawnSteam(RectInt area)
        {
            Add(new Particle
            {
                Position = new Vector2(area.x + Range(0f, area.width), area.y + Range(0f, area.height)),
                Velocity = new Vector2(Range(-10f, 10f), Range(-95f, -60f)),
                Life = Range(0.5f, 0.9f),
                Color = new Color32(220, 234, 238, 255),
                Size = Range(2f, 4f),
                Grow = 7f,
                Gravity = 30f,
                Fade = true,
            });
        }

        private void Add(Particle particle)
        {
            if (_particleCount >= MaxParticles) return;
            _particles[_particleCount++] = particle;
        }

        private void StepParticles(float dt)
        {
            for (int i = _particleCount - 1; i >= 0; i--)
            {
                Particle p = _particles[i];
                p.Age += dt;

                if (p.Age >= p.Life)
                {
                    _particles[i] = _particles[--_particleCount];
                    continue;
                }

                p.Velocity.y += p.Gravity * dt;
                p.Position += p.Velocity * dt;
                p.Size += p.Grow * dt;
                _particles[i] = p;
            }
        }

        private void DrawParticles()
        {
            for (int i = 0; i < _particleCount; i++)
            {
                Particle p = _particles[i];
                float alpha = p.Fade ? 1f - p.Age / p.Life : 1f;
                int size = Mathf.Max(1, Mathf.RoundToInt(p.Size));
                int x0 = Mathf.RoundToInt(p.Position.x - size / 2f), y0 = Mathf.RoundToInt(p.Position.y - size / 2f);

                for (int y = y0; y < y0 + size; y++)
                {
                    for (int x = x0; x < x0 + size; x++)
                    {
                        // O vapor e redondo; o resto, quadradinho.
                        if (size > 3)
                        {
                            float dx = x + 0.5f - p.Position.x, dy = y + 0.5f - p.Position.y;
                            if (dx * dx + dy * dy > size * size * 0.25f) continue;
                        }

                        Blend(x, y, p.Color, alpha);
                    }
                }
            }
        }

        /// <summary>O brilho que corre pelo fio: um risco claro com uma estrelinha na ponta, da base ate a ponta.</summary>
        private void DrawGlint(RectInt area, float k)
        {
            if (k < 0f || k > 1.3f) return;
            k = Mathf.Clamp01(k);

            var from = new Vector2(area.x, area.y + area.height);
            var to = new Vector2(area.x + area.width, area.y);
            Vector2 head = Vector2.Lerp(from, to, k);
            Vector2 dir = (to - from).normalized;
            var white = new Color32(255, 255, 255, 255);

            for (int s = 0; s < 18; s++)
            {
                Vector2 q = head - dir * s;
                float a = 1f - s / 18f;
                Blend(Mathf.RoundToInt(q.x), Mathf.RoundToInt(q.y), white, a);
                Blend(Mathf.RoundToInt(q.x), Mathf.RoundToInt(q.y) + 1, white, a * 0.6f);
            }

            int hx = Mathf.RoundToInt(head.x), hy = Mathf.RoundToInt(head.y);
            for (int r = -4; r <= 4; r++)
            {
                float a = 1f - Mathf.Abs(r) / 5f;
                Blend(hx + r, hy, white, a);
                Blend(hx, hy + r, white, a);
            }
        }

        /// <summary>Indice no buffer (que e de baixo pra cima, como as texturas) de um pixel do quadro (y pra baixo).</summary>
        private static int Index(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return -1;
            return x + (Height - 1 - y) * Width;
        }

        private void Blend(int x, int y, Color32 color, float alpha)
        {
            int i = Index(x, y);
            if (i < 0 || alpha <= 0f) return;

            Color32 under = _buffer[i];
            _buffer[i] = new Color32(
                (byte)(under.r + (color.r - under.r) * alpha),
                (byte)(under.g + (color.g - under.g) * alpha),
                (byte)(under.b + (color.b - under.b) * alpha),
                255);
        }

        private float Range(float min, float max) => min + (float)_random.NextDouble() * (max - min);

        // ------------------------------------------------------------------
        // Tela e controle
        // ------------------------------------------------------------------

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

            // Por cima do HUD e de tudo.
            GUI.depth = -1000;
            Color previous = GUI.color;
            var full = new Rect(0f, 0f, Screen.width, Screen.height);

            if (_showPanel)
            {
                Fill(full, Color.black);

                // O maior 16:9 que cabe, centralizado; o resto fica preto.
                float scale = Mathf.Min(Screen.width / (float)Width, Screen.height / (float)Height);
                float w = Width * scale, h = Height * scale;
                var panel = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

                if (_cut > 0)
                {
                    Fill(panel, CutColor);
                }
                else if (_screen != null)
                {
                    GUI.color = Color.white;
                    GUI.DrawTextureWithTexCoords(panel, _screen, _uv, false);
                }

                float bar = h * BarFraction;
                Fill(new Rect(panel.x, panel.y, w, bar), Color.black);
                Fill(new Rect(panel.x, panel.yMax - bar, w, bar), Color.black);
            }

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
