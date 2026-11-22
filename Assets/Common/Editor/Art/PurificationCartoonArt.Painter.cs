using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class PurificationCartoonArt
    {
        // ------------------------------------------------------------------
        // Fundos
        // ------------------------------------------------------------------

        /// <summary>
        /// A boca de uma galeria: paredes escuras dos lados, chao, e la no
        /// fundo a luz do coracao com raios largos.
        /// </summary>
        private static Rgba Gallery(int x, int y, float gx, float gy, float strength)
        {
            float n = Noise(x / 40f, y / 30f, 2) * 0.6f + Noise(x / 9f, y / 9f, 3) * 0.4f;
            int level = Clamp((int)(0.6f + n * 1.8f + Bayer(x, y) * 0.5f), 0, 3);
            Rgba rock = CaveRock[level];

            float d = Dist(x, y, gx, gy);
            float g = Math.Max(0f, 1f - d / (90f + 70f * strength));
            g = g * g * (0.6f + 0.5f * strength);

            double angle = Math.Atan2(y - gy, x - gx);
            float ray = (float)Math.Pow(Math.Max(0, Math.Cos(angle * 6 + 0.4)), 3);
            float rays = ray * Math.Max(0f, 1f - d / (200f * strength + 40f)) * 0.35f * strength;

            float light = Math.Min(1f, g + rays);
            Rgba glow = Rgba.Lerp(Purify, PurifyCore, Math.Min(1f, g * 1.8f));
            float k = (float)Math.Floor(light * 7f + Bayer(x, y) * 0.95f) / 7f;
            Rgba color = Rgba.Lerp(rock, glow, k);

            // As paredes da galeria nos lados, escuras e recortadas.
            float wallL = 40 + Noise(0, y / 12f, 5) * 26 - Math.Max(0, y - 120) * 0.25f;
            float wallR = 280 - Noise(1, y / 12f, 6) * 26 + Math.Max(0, y - 120) * 0.25f;
            if (x < wallL || x > wallR) color = Rgba.Lerp(CaveRock[0], color, 0.18f);

            // O chao, com o reflexo da luz vindo ate o Scrap.
            float floor = 152 + Noise(x / 12f, 0, 7) * 4;
            if (y > floor)
            {
                float lane = Math.Max(0f, 1f - Math.Abs(x - gx) / (34f + (y - floor) * 1.2f));
                Rgba ground = CaveRock[Clamp((int)(Noise(x / 6f, y / 3f, 8) * 2f + Bayer(x, y) * 0.5f), 0, 1)];
                float lk = (float)Math.Floor(lane * strength * 4f + Bayer(x, y) * 0.9f) / 4f;
                color = Rgba.Lerp(ground, Purify, lk * 0.45f);
            }

            return Vignette(color, x, y, 0.55f);
        }

        /// <summary>Fundo fora de foco dos closes: escuro, com bolhas grandes de luz.</summary>
        private static Rgba Bokeh(int x, int y, float strength, int seed)
        {
            float n = Noise(x / 60f, y / 50f, seed) * 0.8f;
            Rgba color = Rgba.Lerp(CaveRock[0], CaveRock[2], n);

            var rng = new Random(seed * 31);
            for (int i = 0; i < 14; i++)
            {
                float bx = rng.Next(0, Width), by = rng.Next(0, Height);
                float r = 8 + rng.Next(0, 18);
                float d = Dist(x, y, bx, by);
                if (d > r) continue;

                bool warm = rng.Next(0, 3) == 0;
                Rgba tint = warm ? new Rgba(150, 120, 80) : new Rgba(80, 150, 160);
                float edge = d > r - 1.5f ? 1.4f : 1f;
                color = Rgba.Lerp(color, tint, 0.12f * strength * edge + 0.05f * strength);
            }

            // Uma luz larga vindo do canto de cima.
            float glow = Math.Max(0f, 1f - Dist(x, y, 300, 0) / 260f);
            float k = (float)Math.Floor(glow * glow * strength * 5f + Bayer(x, y) * 0.9f) / 5f;
            color = Rgba.Lerp(color, Purify, k * 0.35f);
            return Vignette(color, x, y, 0.5f);
        }

        /// <summary>A parede de pedra do respiro: blocos (Voronoi) e, no polido, uma luz verde-agua.</summary>
        private static Rgba StoneWall(int x, int y, bool lit)
        {
            float cell = CellEdge(x / 30f, y / 24f, 7);
            float n = Noise(x / 7f, y / 7f, 8);

            int level = Clamp((int)(1.1f + n * 1.4f + Bayer(x, y) * 0.4f), 0, 3);
            if (cell < 0.06f) level = 0;
            else if (cell < 0.11f) level = Math.Max(0, level - 1);
            else if (cell < 0.15f) level = Math.Min(3, level + 1);

            Rgba rock = CaveRock[level];
            if (y > 170) rock = CaveRock[0];

            float d = Dist(x, y, 160, 60);
            float g = Math.Max(0f, 1f - d / (lit ? 210f : 130f));
            g = (float)Math.Floor(g * g * 5f + Bayer(x, y) * 0.9f) / 5f;
            Rgba glow = lit ? new Rgba(110, 210, 200) : new Rgba(110, 100, 84);
            return Vignette(Rgba.Lerp(rock, glow, g * (lit ? 0.5f : 0.22f)), x, y, 0.5f);
        }

        /// <summary>O fundo da pose final: escuro antes; depois, uma explosao de raios.</summary>
        private static Rgba Burst(int x, int y, float cx, float cy, float spin, bool lit)
        {
            if (!lit)
            {
                float n = Noise(x / 30f, y / 24f, 3);
                return Vignette(CaveRock[Clamp((int)(n * 2.2f + Bayer(x, y) * 0.5f), 0, 2)], x, y, 0.5f);
            }

            float d = Dist(x, y, cx, cy);
            double angle = Math.Atan2(y - cy, x - cx) + spin;
            bool stripe = ((int)Math.Floor((angle + Math.PI) / (Math.PI * 2) * 22)) % 2 == 0;

            var deep = new Rgba(14, 30, 58);
            var mid = new Rgba(34, 86, 128);
            var light = new Rgba(110, 200, 230);

            float k = Math.Max(0f, 1f - d / 240f);
            k = (float)Math.Floor(k * 6f + Bayer(x, y) * 0.9f) / 6f;
            Rgba color = Rgba.Lerp(deep, stripe ? light : mid, k);

            // O miolo branco atras da cabeca.
            if (d < 80f)
            {
                float c = (float)Math.Floor((1f - d / 80f) * 5f + Bayer(x, y) * 0.9f) / 5f;
                color = Rgba.Lerp(color, PurifyCore, c * 0.9f);
            }

            // Cristais flutuando: losangos pequenos.
            var rng = new Random(91);
            for (int i = 0; i < 18; i++)
            {
                float sx = rng.Next(10, 310), sy = rng.Next(16, 150);
                float s = 2 + rng.Next(0, 3);
                float m = Math.Abs(x - sx) / s + Math.Abs(y - sy) / (s * 1.8f);
                if (m <= 1f) color = m < 0.45f ? PurifyCore : new Rgba(130, 220, 240);
            }

            return Vignette(color, x, y, 0.45f);
        }

        private static Rgba Vignette(Rgba color, int x, int y, float amount)
        {
            float dx = (x - Width / 2f) / (Width / 2f), dy = (y - Height / 2f) / (Height / 2f);
            float v = Math.Max(0f, (dx * dx + dy * dy) * 0.5f - 0.1f);
            float k = (float)Math.Floor(Math.Min(1f, v * amount * 2f) * 4f + Bayer(x, y) * 0.9f) / 4f;
            return Rgba.Lerp(color, CaveRock[0], k);
        }

        // ------------------------------------------------------------------
        // O pintor
        // ------------------------------------------------------------------

        private delegate bool Probe(int x, int y, out float nx, out float ny, out float nz);

        private delegate int OrderFn(int x, int y);

        private static Probe Flat(float nx, float ny, float nz)
        {
            Normalize(ref nx, ref ny, ref nz);
            return (int x, int y, out float a, out float b, out float c) =>
            {
                a = nx; b = ny; c = nz;
                return true;
            };
        }

        private static OrderFn Radial(float cx, float cy, float perPixel, int offset, float jitter)
        {
            return (x, y) => offset + (int)(Dist(x, y, cx, cy) * perPixel + (Noise(x / 5f, y / 5f, 31) - 0.5f) * jitter);
        }

        private sealed class Painter
        {
            public readonly PixelCanvas Before = new PixelCanvas(Width, Height);
            public readonly PixelCanvas After = new PixelCanvas(Width, Height);
            public readonly PixelCanvas Order = new PixelCanvas(Width, Height);

            public float LX, LY, LZ = 1f;
            public float Ambient = 0.2f;
            public float Diffuse = 0.65f;

            private readonly int[] _groupB = new int[Width * Height];
            private readonly int[] _groupA = new int[Width * Height];
            private readonly int[] _layerB = new int[Width * Height];
            private readonly int[] _layerA = new int[Width * Height];
            private readonly Rgba[] _backB = new Rgba[Width * Height];
            private readonly Rgba[] _backA = new Rgba[Width * Height];
            private int _draw;

            public Painter()
            {
                for (int i = 0; i < Order.Pixels.Length; i++) Order.Pixels[i] = OrderColor(Static);
            }

            public bool IsAfterGroup(int x, int y, int group)
            {
                return x >= 0 && y >= 0 && x < Width && y < Height && _groupA[x + y * Width] == group;
            }

            /// <summary>O fundo, nas duas versoes. A ordem so vale onde elas diferem.</summary>
            public void Backdrop(Func<int, int, Rgba> before, Func<int, int, Rgba> after, OrderFn order)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int i = x + y * Width;
                        Rgba b = before(x, y), a = after(x, y);
                        Before.Pixels[i] = _backB[i] = b;
                        After.Pixels[i] = _backA[i] = a;
                        if (!Same(a, b)) SetOrder(i, order(x, y));
                    }
                }
            }

            /// <summary>
            /// Pinta uma forma. Material nulo: a forma nao existe naquela versao
            /// (ex.: crosta so na sucata, cristal so no polido).
            /// </summary>
            public void Shape(Probe probe, int x0, int y0, int x1, int y1, Mat before, Mat after, OrderFn order, int group)
            {
                _draw++;
                float lx = LX, ly = LY, lz = LZ;
                Normalize(ref lx, ref ly, ref lz);

                for (int y = Math.Max(0, y0); y <= Math.Min(Height - 1, y1); y++)
                {
                    for (int x = Math.Max(0, x0); x <= Math.Min(Width - 1, x1); x++)
                    {
                        if (!probe(x, y, out float nx, out float ny, out float nz)) continue;

                        int i = x + y * Width;
                        float d = nx * lx + ny * ly + nz * lz;
                        float lum = Ambient + Diffuse * Math.Max(0f, d);

                        // Especular: reflexo da luz em direcao a camera (0, 0, 1).
                        float rz = 2f * d * nz - lz;
                        float spec = rz > 0f ? (float)Math.Pow(rz, 18) : 0f;

                        if (before != null)
                        {
                            Before.Pixels[i] = ShadeSurface(before, lum + spec * before.Shine, x, y);
                            _groupB[i] = group;
                            _layerB[i] = _draw;
                        }

                        if (after != null)
                        {
                            After.Pixels[i] = ShadeSurface(after, lum + spec * after.Shine, x, y);
                            _groupA[i] = group;
                            _layerA[i] = _draw;
                        }

                        SetOrder(i, order(x, y));
                    }
                }
            }

            public void Ellipse(float cx, float cy, float rx, float ry, Mat before, Mat after, OrderFn order, int group)
            {
                Probe probe = (int x, int y, out float nx, out float ny, out float nz) =>
                {
                    nx = (x + 0.5f - cx) / rx;
                    ny = (y + 0.5f - cy) / ry;
                    float s = nx * nx + ny * ny;
                    nz = (float)Math.Sqrt(Math.Max(0f, 1f - s));
                    return s <= 1f;
                };

                Shape(probe, (int)(cx - rx - 1), (int)(cy - ry - 1), (int)(cx + rx + 1), (int)(cy + ry + 1), before, after, order, group);
            }

            /// <summary>Um tubo de A ate B, de raio ra ate rb, com pontas redondas.</summary>
            public void Capsule(float ax, float ay, float bx, float by, float ra, float rb, Mat before, Mat after, OrderFn order, int group)
            {
                float dx = bx - ax, dy = by - ay;
                float len2 = Math.Max(0.0001f, dx * dx + dy * dy);

                Probe probe = (int x, int y, out float nx, out float ny, out float nz) =>
                {
                    float px = x + 0.5f, py = y + 0.5f;
                    float t = ((px - ax) * dx + (py - ay) * dy) / len2;
                    t = t < 0 ? 0 : (t > 1 ? 1 : t);
                    float qx = ax + dx * t, qy = ay + dy * t;
                    float r = ra + (rb - ra) * t;
                    nx = (px - qx) / r;
                    ny = (py - qy) / r;
                    float s = nx * nx + ny * ny;
                    nz = (float)Math.Sqrt(Math.Max(0f, 1f - s));
                    return s <= 1f;
                };

                float m = Math.Max(ra, rb) + 1;
                Shape(probe, (int)(Math.Min(ax, bx) - m), (int)(Math.Min(ay, by) - m), (int)(Math.Max(ax, bx) + m),
                    (int)(Math.Max(ay, by) + m), before, after, order, group);
            }

            /// <summary>Um cilindro de pe, de y0 a y1, com a tampa de cima em elipse.</summary>
            public void Cylinder(float cx, float y0, float y1, float r, Mat before, Mat after, OrderFn order, int group)
            {
                float cap = r * 0.22f;
                Probe probe = (int x, int y, out float nx, out float ny, out float nz) =>
                {
                    nx = (x + 0.5f - cx) / r;
                    ny = 0f;
                    nz = (float)Math.Sqrt(Math.Max(0f, 1f - nx * nx));
                    if (Math.Abs(nx) > 1f) return false;
                    if (y > y1) return false;
                    float top = y0 - cap * nz;
                    if (y < top) return false;
                    if (y < y0) { ny = -0.8f; nz = 0.6f; }
                    return true;
                };

                Shape(probe, (int)(cx - r - 1), (int)(y0 - cap - 1), (int)(cx + r + 1), (int)y1, before, after, order, group);
            }

            /// <summary>Um cone de boca larga em cima (o bocal do respiro).</summary>
            public void Flare(float cx, float yTop, float yBottom, float rTop, float rBottom, Mat before, Mat after, OrderFn order, int group)
            {
                Probe probe = (int x, int y, out float nx, out float ny, out float nz) =>
                {
                    float t = (y - yTop) / (yBottom - yTop);
                    float r = rTop + (rBottom - rTop) * t;
                    nx = (x + 0.5f - cx) / r;
                    ny = -0.3f;
                    nz = (float)Math.Sqrt(Math.Max(0f, 1f - nx * nx - ny * ny));
                    return t >= 0f && t <= 1f && Math.Abs(nx) <= 1f;
                };

                Shape(probe, (int)(cx - rTop - 1), (int)yTop, (int)(cx + rTop + 1), (int)yBottom, before, after, order, group);
            }

            /// <summary>Uma faixa horizontal dentro de uma elipse (o cinto).</summary>
            public void Band(float cx, float cy, float rx, float ry, int yFrom, int yTo, Mat before, Mat after, OrderFn order, int group)
            {
                Probe probe = (int x, int y, out float nx, out float ny, out float nz) =>
                {
                    nx = (x + 0.5f - cx) / rx;
                    ny = (y + 0.5f - cy) / ry;
                    float s = nx * nx + ny * ny;
                    nz = (float)Math.Sqrt(Math.Max(0f, 1f - s));
                    return s <= 1f && y >= yFrom && y <= yTo;
                };

                Shape(probe, (int)(cx - rx - 1), yFrom, (int)(cx + rx + 1), yTo, before, after, order, group);
            }

            public void Poly((float x, float y)[] points, Probe normal, Mat before, Mat after, OrderFn order, int group)
            {
                float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
                foreach (var pt in points)
                {
                    minX = Math.Min(minX, pt.x); maxX = Math.Max(maxX, pt.x);
                    minY = Math.Min(minY, pt.y); maxY = Math.Max(maxY, pt.y);
                }

                Probe probe = (int x, int y, out float nx, out float ny, out float nz) =>
                {
                    normal(x, y, out nx, out ny, out nz);
                    return InsidePoly(points, x + 0.5f, y + 0.5f);
                };

                Shape(probe, (int)minX - 1, (int)minY - 1, (int)maxX + 1, (int)maxY + 1, before, after, order, group);
            }

            /// <summary>Pinta um pixel por cima, sem mexer no contorno. Cor nula: nao mexe naquela versao.</summary>
            public void Overlay(int x, int y, Rgba? before, Rgba? after, OrderFn order)
            {
                if (x < 0 || y < 0 || x >= Width || y >= Height) return;
                OverlayOrder(x, y, before, after, order(x, y));
            }

            public void OverlayOrder(int x, int y, Rgba? before, Rgba? after, int order)
            {
                if (x < 0 || y < 0 || x >= Width || y >= Height) return;
                int i = x + y * Width;
                if (before.HasValue) Before.Pixels[i] = before.Value;
                if (after.HasValue) After.Pixels[i] = after.Value;
                SetOrder(i, order);
            }

            /// <summary>Tira um pedaco so da versao "antes" (mossa, buraco), mostrando o fundo.</summary>
            public void CarveBefore(int cx, int cy, int radius, OrderFn order)
            {
                for (int y = cy - radius; y <= cy + radius; y++)
                {
                    for (int x = cx - radius; x <= cx + radius; x++)
                    {
                        if (x < 0 || y < 0 || x >= Width || y >= Height) continue;
                        if (Dist(x, y, cx, cy) > radius + 0.5f) continue;
                        int i = x + y * Width;
                        Before.Pixels[i] = _backB[i];
                        _groupB[i] = 0;
                        _layerB[i] = 0;
                        SetOrder(i, order(x, y));
                    }
                }
            }

            /// <summary>O "antes" vira a silhueta escura do "depois".</summary>
            public void SilhouetteBefore(Rgba fill)
            {
                for (int i = 0; i < _groupA.Length; i++)
                {
                    _groupB[i] = _groupA[i] == 0 ? 0 : 1;
                    _layerB[i] = _layerA[i];
                    Before.Pixels[i] = _groupA[i] != 0 ? fill : _backB[i];
                }
            }

            /// <summary>Contorno escuro onde uma forma encosta em algo atras dela, nas duas versoes.</summary>
            public void Finish()
            {
                Outline(Before, _groupB, _layerB);
                Outline(After, _groupA, _layerA);
            }

            private static void Outline(PixelCanvas canvas, int[] group, int[] layer)
            {
                var mark = new bool[group.Length];

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int i = x + y * Width;
                        if (group[i] == 0) continue;

                        for (int k = 0; k < 4; k++)
                        {
                            int nx = x + (k == 0 ? 1 : (k == 1 ? -1 : 0));
                            int ny = y + (k == 2 ? 1 : (k == 3 ? -1 : 0));
                            if (nx < 0 || ny < 0 || nx >= Width || ny >= Height) continue;

                            int j = nx + ny * Width;
                            if (group[j] == group[i]) continue;
                            if (layer[j] >= layer[i] && group[j] != 0) continue;

                            mark[i] = true;
                            break;
                        }
                    }
                }

                for (int i = 0; i < mark.Length; i++)
                {
                    if (mark[i]) canvas.Pixels[i] = Ink;
                }
            }

            /// <summary>
            /// Luz de borda: os pixels da figura que tem fundo logo na direcao da
            /// luz clareiam. Ponto: a luz vem de (lx, ly); senao, (lx, ly) e a direcao.
            /// </summary>
            public void Rim(Rgba color, int width, float lx, float ly, float amount, bool point = true)
            {
                RimOn(Before, _groupB, color, width, lx, ly, amount * 0.6f, point);
                RimOn(After, _groupA, color, width, lx, ly, amount, point);
            }

            private static void RimOn(PixelCanvas canvas, int[] group, Rgba color, int width, float lx, float ly, float amount, bool point)
            {
                var source = (Rgba[])canvas.Pixels.Clone();

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int i = x + y * Width;
                        if (group[i] == 0) continue;

                        float dx = point ? lx - x : lx, dy = point ? ly - y : ly;
                        float len = (float)Math.Sqrt(dx * dx + dy * dy);
                        if (len < 0.001f) continue;
                        dx /= len; dy /= len;

                        for (int k = 1; k <= width + 1; k++)
                        {
                            int sx = (int)Math.Round(x + dx * k), sy = (int)Math.Round(y + dy * k);
                            if (sx < 0 || sy < 0 || sx >= Width || sy >= Height) break;
                            if (group[sx + sy * Width] != 0) continue;

                            float strength = amount / k;
                            canvas.Pixels[i] = Rgba.Lerp(source[i], color, Math.Min(1f, strength));
                            break;
                        }
                    }
                }
            }

            private void SetOrder(int i, int order)
            {
                Order.Pixels[i] = OrderColor((byte)Clamp(order, 0, 254));
            }

            private static Rgba OrderColor(byte value) => new Rgba(value, value, value);

            private static bool Same(Rgba a, Rgba b) => a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;
        }

        private static Rgba ShadeSurface(Mat m, float lum, int x, int y)
        {
            if (m.Shine > 0f && lum > 1.02f) return m.Accent;

            float t = lum * 4f + (Bayer(x, y) - 0.5f) * 0.7f;
            int level = Clamp((int)Math.Floor(t), 0, 3);

            switch (m.Surface)
            {
                case Surface.Rust:
                {
                    if (Noise(x / 5f, y / 5f, 3) > 0.6f) level = Math.Max(0, level - 1);
                    Rgba c = m.Tones[level];
                    if (Noise(x / 3f, y / 3f, 11) > 0.68f) c = Rgba.Lerp(c, m.Accent, 0.5f);
                    if (Noise(x / 8f, y / 8f, 13) > 0.7f) c = Rgba.Lerp(c, DeepRust, 0.45f);
                    if (Hash(x, y, 7) < 0.07f) c = c.Scaled(0.72f);
                    return c;
                }

                case Surface.Rock:
                {
                    Rgba c = m.Tones[level];
                    float h = Hash(x, y, 5);
                    if (h < 0.03f && level >= 1) return m.Accent;
                    if (h > 0.92f) return c.Scaled(0.8f);
                    return c;
                }

                case Surface.Crystal:
                {
                    Rgba c = m.Tones[level];
                    if (Hash(x, y, 9) < 0.01f && level >= 2) return m.Accent;
                    return c;
                }

                case Surface.Cloth:
                {
                    if (Noise(x / 4f, y / 4f, 15) > 0.66f) level = Math.Max(0, level - 1);
                    Rgba c = m.Tones[level];
                    if (((x + y) & 3) == 0 && Hash(x, y, 17) < 0.35f) c = c.Scaled(0.9f);
                    return c;
                }

                case Surface.Leather:
                {
                    Rgba c = m.Tones[level];
                    if (Noise(x / 2.5f, y / 2.5f, 19) > 0.72f) c = c.Scaled(0.92f);
                    return c;
                }

                case Surface.Wood:
                {
                    if (Noise((x + y) / 2f, (x - y) / 9f, 23) > 0.62f) level = Math.Max(0, level - 1);
                    return m.Tones[level];
                }

                case Surface.Moss:
                {
                    Rgba c = m.Tones[level];
                    if (Hash(x, y, 25) < 0.08f) return m.Accent;
                    return c;
                }

                default:
                    return m.Tones[level];
            }
        }
    }
}
