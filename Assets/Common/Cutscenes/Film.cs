using System;
using System.Collections.Generic;

namespace EchoesOfCravejada
{
    // ----------------------------------------------------------------------
    // O filme: cutscenes de camadas, com camera, animacao e particulas.
    //
    // Cada plano (FilmShot) e um palco em pixels (y pra baixo) com camadas
    // desenhadas: cada uma tem os seus quadros (animacao), as suas chaves
    // (posicao, escala, transparencia ao longo do tempo) e a sua profundidade
    // (parallax: 0 colado na tela, 1 no palco, acima de 1 na frente). A
    // camera anda, aproxima e treme; claroes, fades e particulas por cima.
    //
    // Este arquivo nao depende de UnityEngine de proposito: o mesmo
    // compositor toca o filme no jogo e gera as previas fora do Unity.
    // ----------------------------------------------------------------------

    /// <summary>Uma chave de animacao de camada: onde o pivo esta, escala e transparencia, num instante.</summary>
    [Serializable]
    public class FilmKey
    {
        public float t;
        public float x;
        public float y;
        public float scale = 1f;
        public float alpha = 1f;

        /// <summary>Como chega nesta chave: 0 linear, 1 suave, 2 freando, 3 acelerando.</summary>
        public int ease = 1;

        public FilmKey() { }

        public FilmKey(float t, float x, float y, float scale = 1f, float alpha = 1f, int ease = 1)
        {
            this.t = t;
            this.x = x;
            this.y = y;
            this.scale = scale;
            this.alpha = alpha;
            this.ease = ease;
        }
    }

    /// <summary>Uma camada de um plano.</summary>
    [Serializable]
    public class FilmLayer
    {
        public string name;

        /// <summary>Indice do primeiro quadro na lista do filme, e quantos sao.</summary>
        public int firstFrame;
        public int frameCount = 1;

        /// <summary>A animacao: quadros por segundo, quando comeca e se repete.</summary>
        public float fps = 8f;
        public float animStart;
        public bool loop;

        /// <summary>Profundidade: 0 colado na tela, 1 no palco, maior que 1 passa na frente.</summary>
        public float parallax = 1f;

        /// <summary>Soma a luz em vez de cobrir (brilhos, raios).</summary>
        public bool additive;

        /// <summary>Onde fica o pivo, em fracao do quadro (0,0 canto de cima a esquerda).</summary>
        public float pivotX = 0.5f;
        public float pivotY = 0.5f;

        public FilmKey[] keys;
    }

    /// <summary>A camera: o ponto do palco no meio da tela e o zoom.</summary>
    [Serializable]
    public class FilmCam
    {
        public float t;
        public float x;
        public float y;
        public float zoom = 1f;
        public int ease = 1;

        public FilmCam() { }

        public FilmCam(float t, float x, float y, float zoom = 1f, int ease = 1)
        {
            this.t = t;
            this.x = x;
            this.y = y;
            this.zoom = zoom;
            this.ease = ease;
        }
    }

    /// <summary>Um tremor de camera, que comeca forte e some.</summary>
    [Serializable]
    public class FilmShake
    {
        public float t;
        public float duration;
        public float amount;

        public FilmShake() { }
        public FilmShake(float t, float duration, float amount) { this.t = t; this.duration = duration; this.amount = amount; }
    }

    /// <summary>Um clarao de cor por cima de tudo: sobe rapido e desbota.</summary>
    [Serializable]
    public class FilmFlash
    {
        public float t;
        public float duration;
        public uint color = 0xFFFFFFFF;
        public float strength = 1f;

        public FilmFlash() { }

        public FilmFlash(float t, float duration, uint color = 0xFFFFFFFF, float strength = 1f)
        {
            this.t = t;
            this.duration = duration;
            this.color = color;
            this.strength = strength;
        }
    }

    /// <summary>
    /// Uma fonte de particulas. As particulas nao guardam estado: cada uma
    /// nasce num instante fixo, com sorteio fixo, e a posicao sai da conta —
    /// pular e voltar no tempo da sempre o mesmo quadro.
    /// </summary>
    [Serializable]
    public class FilmSpray
    {
        /// <summary>0 quadradinho, 1 bolinha macia, 2 risco no sentido do voo.</summary>
        public int shape;

        /// <summary>Onde nascem, no palco.</summary>
        public float x, y, w, h;

        /// <summary>Quando nascem e quantas por segundo.</summary>
        public float from, to, rate;

        /// <summary>Velocidade (pixels por segundo) e o quanto ela varia.</summary>
        public float vx, vy, spreadX, spreadY;

        /// <summary>Velocidade pra fora de um centro (estouros): soma com a de cima.</summary>
        public float burst;

        public float gravity;
        public float life = 1f;
        public uint color = 0xFFFFFFFF;
        public uint color2 = 0x00FFFFFF;
        public int size = 1;
        public float grow;
        public bool additive;
        public float parallax = 1f;
        public int seed = 1;
    }

    /// <summary>Um som num instante do plano (nome do campo em Sfx).</summary>
    [Serializable]
    public class FilmCue
    {
        public float t;
        public string sound;
        public float pitch = 1f;
        public float volume = 1f;

        public FilmCue() { }

        public FilmCue(float t, string sound, float pitch = 1f, float volume = 1f)
        {
            this.t = t;
            this.sound = sound;
            this.pitch = pitch;
            this.volume = volume;
        }
    }

    /// <summary>Um plano do filme.</summary>
    [Serializable]
    public class FilmShot
    {
        public string name;
        public float duration = 4f;
        public uint background = 0xFF000000;
        public FilmLayer[] layers;
        public FilmCam[] camera;
        public FilmShake[] shakes;
        public FilmFlash[] flashes;
        public FilmSpray[] sprays;
        public FilmCue[] cues;

        /// <summary>Entrada e saida pelo preto (0: corte seco).</summary>
        public float fadeIn = 0.3f;
        public float fadeOut = 0.3f;

        /// <summary>Quanto as bordas escurecem.</summary>
        public float vignette = 0.35f;
    }

    /// <summary>Os pixels de um quadro: 0xAARRGGBB, linha a linha de cima pra baixo.</summary>
    public sealed class FilmFrame
    {
        public readonly int Width;
        public readonly int Height;
        public readonly uint[] Pixels;

        public FilmFrame(int width, int height, uint[] pixels)
        {
            Width = width;
            Height = height;
            Pixels = pixels;
        }
    }

    /// <summary>Monta um instante de um plano num quadro de 320x180.</summary>
    public static class FilmCompositor
    {
        public const int Width = 320;
        public const int Height = 180;

        private static float[] _vignette;
        private static float _vignetteFor = -1f;

        public static float Ease(float k, int ease)
        {
            k = k < 0f ? 0f : k > 1f ? 1f : k;
            switch (ease)
            {
                case 0: return k;
                case 2: return 1f - (1f - k) * (1f - k);
                case 3: return k * k;
                default: return k * k * (3f - 2f * k);
            }
        }

        private static float Lerp(float a, float b, float k) => a + (b - a) * k;

        public static FilmKey Sample(FilmKey[] keys, float t)
        {
            if (keys == null || keys.Length == 0) return new FilmKey(0f, 160f, 90f);
            if (t <= keys[0].t || keys.Length == 1) return keys[0];

            for (int i = 1; i < keys.Length; i++)
            {
                if (t > keys[i].t) continue;
                FilmKey a = keys[i - 1], b = keys[i];
                float k = Ease((t - a.t) / Math.Max(0.0001f, b.t - a.t), b.ease);
                return new FilmKey(t, Lerp(a.x, b.x, k), Lerp(a.y, b.y, k), Lerp(a.scale, b.scale, k), Lerp(a.alpha, b.alpha, k));
            }

            return keys[keys.Length - 1];
        }

        public static FilmCam SampleCam(FilmCam[] keys, float t)
        {
            if (keys == null || keys.Length == 0) return new FilmCam(0f, 160f, 90f);
            if (t <= keys[0].t || keys.Length == 1) return keys[0];

            for (int i = 1; i < keys.Length; i++)
            {
                if (t > keys[i].t) continue;
                FilmCam a = keys[i - 1], b = keys[i];
                float k = Ease((t - a.t) / Math.Max(0.0001f, b.t - a.t), b.ease);
                return new FilmCam(t, Lerp(a.x, b.x, k), Lerp(a.y, b.y, k), Lerp(a.zoom, b.zoom, k));
            }

            return keys[keys.Length - 1];
        }

        public static int FrameAt(FilmLayer layer, float t)
        {
            if (layer.frameCount <= 1 || t < layer.animStart) return layer.firstFrame;
            int index = (int)((t - layer.animStart) * layer.fps);
            index = layer.loop ? index % layer.frameCount : Math.Min(index, layer.frameCount - 1);
            return layer.firstFrame + index;
        }

        /// <summary>Monta o plano no instante t. O alvo tem 320x180, y pra baixo.</summary>
        public static void Render(FilmShot shot, IList<FilmFrame> frames, float t, uint[] target)
        {
            for (int i = 0; i < target.Length; i++) target[i] = shot.background | 0xFF000000;

            FilmCam cam = SampleCam(shot.camera, t);
            (float shakeX, float shakeY) = Shake(shot, t);

            if (shot.layers != null)
            {
                foreach (FilmLayer layer in shot.layers)
                {
                    if (layer == null) continue;
                    FilmKey key = Sample(layer.keys, t);
                    if (key.alpha <= 0.003f) continue;

                    int index = FrameAt(layer, t);
                    if (index < 0 || index >= frames.Count || frames[index] == null) continue;
                    DrawLayer(target, frames[index], layer, key, cam, shakeX, shakeY);
                }
            }

            if (shot.sprays != null)
            {
                foreach (FilmSpray spray in shot.sprays) DrawSpray(target, spray, t, cam, shakeX, shakeY);
            }

            Vignette(target, shot.vignette);

            if (shot.flashes != null)
            {
                foreach (FilmFlash flash in shot.flashes)
                {
                    float since = t - flash.t;
                    if (since < 0f || since > flash.duration) continue;
                    float rise = 0.08f;
                    float a = since < rise ? since / rise : 1f - (since - rise) / Math.Max(0.001f, flash.duration - rise);
                    Wash(target, flash.color, a * flash.strength);
                }
            }

            float fade = 1f;
            if (shot.fadeIn > 0f && t < shot.fadeIn) fade = t / shot.fadeIn;
            if (shot.fadeOut > 0f && t > shot.duration - shot.fadeOut) fade = Math.Min(fade, (shot.duration - t) / shot.fadeOut);
            if (fade < 1f) Wash(target, 0xFF000000, 1f - Math.Max(0f, fade));
        }

        private static (float, float) Shake(FilmShot shot, float t)
        {
            float x = 0f, y = 0f;
            if (shot.shakes == null) return (0f, 0f);

            foreach (FilmShake shake in shot.shakes)
            {
                float since = t - shake.t;
                if (since < 0f || since > shake.duration) continue;
                float power = shake.amount * (1f - since / shake.duration);
                x += (float)Math.Sin(since * 71f + shake.t * 13f) * power;
                y += (float)Math.Cos(since * 59f + shake.t * 7f) * power;
            }

            return (x, y);
        }

        /// <summary>Do palco pra tela, com a profundidade da camada.</summary>
        private static void Project(float wx, float wy, float parallax, FilmCam cam, float shakeX, float shakeY,
            out float sx, out float sy, out float zoom)
        {
            zoom = 1f + (cam.zoom - 1f) * parallax;
            sx = Width * 0.5f + (wx - cam.x * parallax - Width * 0.5f * (1f - parallax)) * zoom + shakeX * parallax;
            sy = Height * 0.5f + (wy - cam.y * parallax - Height * 0.5f * (1f - parallax)) * zoom + shakeY * parallax;
        }

        private static void DrawLayer(uint[] target, FilmFrame frame, FilmLayer layer, FilmKey key, FilmCam cam, float shakeX, float shakeY)
        {
            Project(key.x, key.y, layer.parallax, cam, shakeX, shakeY, out float px, out float py, out float zoom);
            float scale = key.scale * zoom;
            if (scale <= 0.001f) return;

            float left = px - frame.Width * layer.pivotX * scale;
            float top = py - frame.Height * layer.pivotY * scale;
            int x0 = Math.Max(0, (int)Math.Floor(left));
            int y0 = Math.Max(0, (int)Math.Floor(top));
            int x1 = Math.Min(Width, (int)Math.Ceiling(left + frame.Width * scale));
            int y1 = Math.Min(Height, (int)Math.Ceiling(top + frame.Height * scale));
            float inv = 1f / scale;
            int alpha = (int)(Math.Min(1f, key.alpha) * 256f);
            uint[] src = frame.Pixels;

            for (int y = y0; y < y1; y++)
            {
                int v = (int)((y + 0.5f - top) * inv);
                if (v < 0 || v >= frame.Height) continue;
                int row = v * frame.Width;
                int dstRow = y * Width;

                for (int x = x0; x < x1; x++)
                {
                    int u = (int)((x + 0.5f - left) * inv);
                    if (u < 0 || u >= frame.Width) continue;

                    uint p = src[row + u];
                    int a = (int)(p >> 24);
                    if (a == 0) continue;
                    a = a * alpha >> 8;
                    if (a <= 0) continue;

                    int i = dstRow + x;
                    target[i] = layer.additive ? Add(target[i], p, a) : Over(target[i], p, a);
                }
            }
        }

        private static uint Over(uint under, uint over, int a)
        {
            if (a >= 255) return over | 0xFF000000;
            int r0 = (int)(under >> 16 & 255), g0 = (int)(under >> 8 & 255), b0 = (int)(under & 255);
            int r1 = (int)(over >> 16 & 255), g1 = (int)(over >> 8 & 255), b1 = (int)(over & 255);
            int r = r0 + ((r1 - r0) * a >> 8);
            int g = g0 + ((g1 - g0) * a >> 8);
            int b = b0 + ((b1 - b0) * a >> 8);
            return 0xFF000000 | (uint)r << 16 | (uint)g << 8 | (uint)b;
        }

        private static uint Add(uint under, uint over, int a)
        {
            int r = (int)(under >> 16 & 255) + ((int)(over >> 16 & 255) * a >> 8);
            int g = (int)(under >> 8 & 255) + ((int)(over >> 8 & 255) * a >> 8);
            int b = (int)(under & 255) + ((int)(over & 255) * a >> 8);
            return 0xFF000000 | (uint)Math.Min(255, r) << 16 | (uint)Math.Min(255, g) << 8 | (uint)Math.Min(255, b);
        }

        private static void Wash(uint[] target, uint color, float amount)
        {
            if (amount <= 0f) return;
            int a = (int)(Math.Min(1f, amount) * 256f);
            if (a > 255) a = 255;
            for (int i = 0; i < target.Length; i++) target[i] = Over(target[i], color, a);
        }

        private static void Vignette(uint[] target, float strength)
        {
            if (strength <= 0f) return;

            if (_vignette == null || Math.Abs(_vignetteFor - strength) > 0.0001f)
            {
                _vignette = new float[Width * Height];
                _vignetteFor = strength;
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        float dx = (x - Width * 0.5f) / (Width * 0.5f);
                        float dy = (y - Height * 0.5f) / (Height * 0.5f);
                        float d = (float)Math.Sqrt(dx * dx * 0.8f + dy * dy * 0.6f);
                        float k = Math.Max(0f, d - 0.55f) / 0.6f;
                        _vignette[x + y * Width] = 1f - Math.Min(1f, k * k) * strength;
                    }
                }
            }

            for (int i = 0; i < target.Length; i++)
            {
                float k = _vignette[i];
                if (k >= 0.999f) continue;
                uint p = target[i];
                target[i] = 0xFF000000 | (uint)((p >> 16 & 255) * k) << 16 | (uint)((p >> 8 & 255) * k) << 8 | (uint)((p & 255) * k);
            }
        }

        // ------------------------------------------------------------------
        // Particulas
        // ------------------------------------------------------------------

        private static float Rand(int seed, int i, int salt)
        {
            unchecked
            {
                uint h = (uint)(seed * 73856093) ^ (uint)(i * 19349663) ^ (uint)(salt * 83492791);
                h ^= h >> 13;
                h *= 0x5bd1e995;
                h ^= h >> 15;
                return (h & 0xFFFFFF) / (float)0x1000000;
            }
        }

        private static uint Mix(uint a, uint b, float k)
        {
            int r = (int)Lerp(a >> 16 & 255, b >> 16 & 255, k);
            int g = (int)Lerp(a >> 8 & 255, b >> 8 & 255, k);
            int bl = (int)Lerp(a & 255, b & 255, k);
            int al = (int)Lerp(a >> 24 & 255, b >> 24 & 255, k);
            return (uint)al << 24 | (uint)r << 16 | (uint)g << 8 | (uint)bl;
        }

        private static void DrawSpray(uint[] target, FilmSpray spray, float t, FilmCam cam, float shakeX, float shakeY)
        {
            if (spray == null || spray.rate <= 0f || t < spray.from) return;

            int total = (int)((spray.to - spray.from) * spray.rate);
            int last = Math.Min(total - 1, (int)((t - spray.from) * spray.rate));
            int first = Math.Max(0, (int)((t - spray.from - spray.life * 1.3f) * spray.rate));
            float cx = spray.x + spray.w * 0.5f, cy = spray.y + spray.h * 0.5f;

            for (int i = first; i <= last; i++)
            {
                float birth = spray.from + i / spray.rate;
                float life = spray.life * (0.7f + 0.6f * Rand(spray.seed, i, 5));
                float age = t - birth;
                if (age < 0f || age > life) continue;

                float x0 = spray.x + Rand(spray.seed, i, 1) * spray.w;
                float y0 = spray.y + Rand(spray.seed, i, 2) * spray.h;
                float vx = spray.vx + (Rand(spray.seed, i, 3) * 2f - 1f) * spray.spreadX;
                float vy = spray.vy + (Rand(spray.seed, i, 4) * 2f - 1f) * spray.spreadY;

                if (spray.burst != 0f)
                {
                    float dx = x0 - cx, dy = y0 - cy;
                    float len = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (len < 0.01f)
                    {
                        float ang = Rand(spray.seed, i, 6) * 6.2832f;
                        dx = (float)Math.Cos(ang);
                        dy = (float)Math.Sin(ang);
                        len = 1f;
                    }

                    float power = spray.burst * (0.5f + Rand(spray.seed, i, 7));
                    vx += dx / len * power;
                    vy += dy / len * power;
                }

                float wx = x0 + vx * age;
                float wy = y0 + vy * age + 0.5f * spray.gravity * age * age;
                float k = age / life;
                uint color = Mix(spray.color, spray.color2, k);
                float fade = (color >> 24 & 255) / 255f * (1f - k * k);
                float size = spray.size + spray.grow * age;

                Project(wx, wy, spray.parallax, cam, shakeX, shakeY, out float sx, out float sy, out float zoom);
                size *= zoom;

                if (spray.shape == 2)
                {
                    float svx = vx, svy = vy + spray.gravity * age;
                    float len = (float)Math.Sqrt(svx * svx + svy * svy);
                    if (len < 0.01f) continue;
                    int steps = Math.Max(2, (int)(len * 0.05f * zoom));
                    for (int s = 0; s < steps; s++)
                    {
                        float f = s / (float)steps;
                        Dot(target, sx - svx / len * s, sy - svy / len * s, Math.Max(1f, size), color, fade * (1f - f), spray.additive, false);
                    }
                }
                else
                {
                    Dot(target, sx, sy, Math.Max(1f, size), color, fade, spray.additive, spray.shape == 1);
                }
            }
        }

        private static void Dot(uint[] target, float cx, float cy, float size, uint color, float alpha, bool additive, bool soft)
        {
            int s = Math.Max(1, (int)Math.Round(size));
            int x0 = (int)Math.Round(cx - s * 0.5f), y0 = (int)Math.Round(cy - s * 0.5f);
            float r = s * 0.5f;

            for (int y = y0; y < y0 + s; y++)
            {
                if (y < 0 || y >= Height) continue;
                for (int x = x0; x < x0 + s; x++)
                {
                    if (x < 0 || x >= Width) continue;
                    float a = alpha;
                    if (soft && s > 2)
                    {
                        float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                        float d = (float)Math.Sqrt(dx * dx + dy * dy) / r;
                        if (d > 1f) continue;
                        a *= d < 0.5f ? 1f : 1f - (d - 0.5f) * 1.6f;
                    }

                    int ai = (int)(Math.Min(1f, a) * 255f);
                    if (ai <= 0) continue;
                    int i = x + y * Width;
                    target[i] = additive ? Add(target[i], color, ai) : Over(target[i], color, ai);
                }
            }
        }
    }
}
