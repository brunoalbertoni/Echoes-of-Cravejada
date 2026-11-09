using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// As ferramentas de desenho que a arte em alta (a do Scrap, a dos
    /// inimigos e a dos descartados do District 1) divide: rampas de quatro
    /// tons, esferas e membros sombreados, manchas de azinhavre presas na
    /// peca, contorno escuro e brilho.
    ///
    /// A luz vem sempre de cima e da esquerda. Nao depende de UnityEngine.
    /// </summary>
    public static class ArtKit
    {
        public static readonly Rgba Outline = new Rgba(22, 18, 22);

        // ------------------------------------------------------------------
        // As rampas de quatro tons, do escuro pro brilho
        // ------------------------------------------------------------------

        public static readonly Rgba[] Rust =
            { new Rgba(62, 34, 28), new Rgba(108, 60, 40), new Rgba(156, 94, 56), new Rgba(198, 134, 82) };

        public static readonly Rgba[] Tin =
            { new Rgba(50, 52, 62), new Rgba(86, 90, 100), new Rgba(128, 132, 140), new Rgba(178, 182, 186) };

        public static readonly Rgba[] Iron =
            { new Rgba(30, 28, 36), new Rgba(52, 50, 60), new Rgba(80, 78, 90), new Rgba(116, 114, 126) };

        public static readonly Rgba[] Brass =
            { new Rgba(90, 60, 30), new Rgba(146, 104, 46), new Rgba(198, 152, 70), new Rgba(240, 206, 124) };

        public static readonly Rgba[] Copper =
            { new Rgba(80, 40, 32), new Rgba(130, 70, 46), new Rgba(178, 106, 68), new Rgba(222, 150, 98) };

        public static readonly Rgba[] Verdigris =
            { new Rgba(34, 74, 68), new Rgba(52, 112, 96), new Rgba(80, 152, 124), new Rgba(128, 194, 158) };

        public static readonly Rgba[] Leather =
            { new Rgba(52, 32, 26), new Rgba(88, 54, 36), new Rgba(124, 80, 50), new Rgba(160, 110, 72) };

        public static readonly Rgba[] Cloth =
            { new Rgba(56, 52, 50), new Rgba(90, 84, 74), new Rgba(130, 120, 100), new Rgba(168, 158, 132) };

        public static readonly Rgba[] Rubber =
            { new Rgba(26, 24, 28), new Rgba(44, 40, 44), new Rgba(66, 60, 62), new Rgba(94, 86, 84) };

        public static readonly Rgba OxideGlow = new Rgba(120, 255, 170);
        public static readonly Rgba OxideCore = new Rgba(232, 255, 238);
        public static readonly Rgba OxideDeep = new Rgba(40, 150, 96);

        /// <summary>Um tom de uma rampa pela luz (de -1 a 1).</summary>
        public static Rgba Tone(Rgba[] ramp, float light)
        {
            if (light > 0.62f) return ramp[3];
            if (light > 0.18f) return ramp[2];
            if (light > -0.32f) return ramp[1];
            return ramp[0];
        }

        /// <summary>
        /// Como pintar um pixel de uma peca: recebe a posicao dele em relacao
        /// a peca (pra mancha nao escorregar quando a peca anda) e a luz.
        /// </summary>
        public delegate Rgba Skin(float localX, float localY, float light);

        public static Skin Plain(Rgba[] ramp) => (x, y, light) => Tone(ramp, light);

        /// <summary>
        /// Metal tomado pelo Oxide: manchas de azinhavre (pela quantidade) e
        /// pintas de ferrugem, presas na peca.
        /// </summary>
        public static Skin Corroded(Rgba[] ramp, float amount, int seed, float scale = 0.2f, Rgba[] patina = null)
        {
            Rgba[] green = patina ?? Verdigris;

            return (x, y, light) =>
            {
                float n = Noise(x * scale, y * scale, seed);
                if (n > 1f - amount) return Tone(green, light - 0.15f);

                // Picadas: um tom abaixo, espalhadas.
                if (Hash((int)Math.Floor(x), (int)Math.Floor(y), seed + 7) % 23 == 0)
                {
                    return ramp[Math.Max(0, Index(ramp, Tone(ramp, light)) - 1)];
                }

                return Tone(ramp, light);
            };
        }

        private static int Index(Rgba[] ramp, Rgba color)
        {
            for (int i = 0; i < ramp.Length; i++)
            {
                if (ramp[i].R == color.R && ramp[i].G == color.G && ramp[i].B == color.B) return i;
            }

            return 0;
        }

        /// <summary>O tom vizinho na rampa: delta -1 escurece um, +1 clareia um.</summary>
        public static Rgba Shift(Rgba[] ramp, Rgba color, int delta)
        {
            int i = Index(ramp, color) + delta;
            return ramp[Math.Max(0, Math.Min(ramp.Length - 1, i))];
        }

        // ------------------------------------------------------------------
        // Formas
        // ------------------------------------------------------------------

        /// <summary>Uma elipse sombreada como uma esfera.</summary>
        public static void Ball(PixelCanvas canvas, float cx, float cy, float rx, float ry, Skin skin, float bias = 0f)
        {
            for (int py = (int)Math.Floor(cy - ry) - 1; py <= (int)(cy + ry) + 1; py++)
            {
                for (int px = (int)Math.Floor(cx - rx) - 1; px <= (int)(cx + rx) + 1; px++)
                {
                    float dx = (px + 0.5f - cx) / rx;
                    float dy = (py + 0.5f - cy) / ry;
                    float d = dx * dx + dy * dy;
                    if (d > 1f) continue;

                    float nz = (float)Math.Sqrt(1f - d);
                    float light = -0.55f * dx - 0.65f * dy + 0.5f * nz - 0.25f + bias;
                    canvas.Set(px, py, skin(px + 0.5f - cx, py + 0.5f - cy, light));
                }
            }
        }

        public static void Ball(PixelCanvas canvas, float cx, float cy, float rx, float ry, Rgba[] ramp, float bias = 0f)
        {
            Ball(canvas, cx, cy, rx, ry, Plain(ramp), bias);
        }

        /// <summary>
        /// Um tambor em pe: largura rx, altura ry, cantos arredondados pelo
        /// expoente (2 e elipse; quanto maior, mais quadrado). Sombreado como
        /// cilindro, com o tampo um pouco mais claro.
        /// </summary>
        public static void Drum(PixelCanvas canvas, float cx, float cy, float rx, float ry, float power, Skin skin, float bias = 0f)
        {
            for (int py = (int)Math.Floor(cy - ry) - 1; py <= (int)(cy + ry) + 1; py++)
            {
                for (int px = (int)Math.Floor(cx - rx) - 1; px <= (int)(cx + rx) + 1; px++)
                {
                    float dx = (px + 0.5f - cx) / rx;
                    float dy = (py + 0.5f - cy) / ry;
                    float d = (float)(Math.Pow(Math.Abs(dx), power) + Math.Pow(Math.Abs(dy), power));
                    if (d > 1f) continue;

                    float nz = (float)Math.Sqrt(Math.Max(0f, 1f - dx * dx));
                    float light = -0.75f * dx + 0.45f * nz - 0.35f * dy - 0.2f + bias;
                    canvas.Set(px, py, skin(px + 0.5f - cx, py + 0.5f - cy, light));
                }
            }
        }

        /// <summary>Uma faixa grossa de um ponto a outro, sombreada como cilindro.</summary>
        public static void Limb(PixelCanvas canvas, float x0, float y0, float x1, float y1, float half, Skin skin, float bias = 0f)
        {
            Taper(canvas, x0, y0, x1, y1, half, half, skin, bias);
        }

        public static void Limb(PixelCanvas canvas, float x0, float y0, float x1, float y1, float half, Rgba[] ramp, float bias = 0f)
        {
            Taper(canvas, x0, y0, x1, y1, half, half, Plain(ramp), bias);
        }

        /// <summary>Uma faixa que afina (ou engrossa) de um ponto a outro.</summary>
        public static void Taper(PixelCanvas canvas, float x0, float y0, float x1, float y1, float half0, float half1,
            Skin skin, float bias = 0f)
        {
            float dx = x1 - x0;
            float dy = y1 - y0;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            if (length < 0.01f)
            {
                Ball(canvas, x0, y0, Math.Max(0.6f, half0), Math.Max(0.6f, half0), skin, bias);
                return;
            }

            float nx = -dy / length;
            float ny = dx / length;
            float most = Math.Max(half0, half1);

            int minX = (int)Math.Floor(Math.Min(x0, x1) - most - 1);
            int maxX = (int)Math.Ceiling(Math.Max(x0, x1) + most + 1);
            int minY = (int)Math.Floor(Math.Min(y0, y1) - most - 1);
            int maxY = (int)Math.Ceiling(Math.Max(y0, y1) + most + 1);

            // O lado virado pra cima/esquerda pega luz.
            float facing = -(nx * 0.55f + ny * 0.65f);

            for (int py = minY; py <= maxY; py++)
            {
                for (int px = minX; px <= maxX; px++)
                {
                    float rx = px + 0.5f - x0;
                    float ry = py + 0.5f - y0;
                    float along = (rx * dx + ry * dy) / (length * length);
                    if (along < -0.02f || along > 1.02f) continue;

                    float half = half0 + (half1 - half0) * Math.Max(0f, Math.Min(1f, along));
                    if (half < 0.35f) half = 0.35f;
                    float across = (rx * nx + ry * ny) / half;
                    if (Math.Abs(across) > 1f) continue;

                    float light = across * facing * 1.2f + 0.1f * (1f - Math.Abs(across)) + bias;
                    canvas.Set(px, py, skin(along * length, across * half, light));
                }
            }
        }

        public static void Taper(PixelCanvas canvas, float x0, float y0, float x1, float y1, float half0, float half1,
            Rgba[] ramp, float bias = 0f)
        {
            Taper(canvas, x0, y0, x1, y1, half0, half1, Plain(ramp), bias);
        }

        /// <summary>
        /// Um poligono cheio. O shade recebe a posicao do pixel e devolve a
        /// cor (Clear pula o pixel).
        /// </summary>
        public static void Polygon(PixelCanvas canvas, (float X, float Y)[] points, Func<float, float, Rgba> shade)
        {
            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
            foreach ((float x, float y) in points)
            {
                minX = Math.Min(minX, x);
                maxX = Math.Max(maxX, x);
                minY = Math.Min(minY, y);
                maxY = Math.Max(maxY, y);
            }

            for (int py = (int)Math.Floor(minY); py <= (int)Math.Ceiling(maxY); py++)
            {
                for (int px = (int)Math.Floor(minX); px <= (int)Math.Ceiling(maxX); px++)
                {
                    if (!InsidePolygon(points, px + 0.5f, py + 0.5f)) continue;

                    Rgba color = shade(px + 0.5f, py + 0.5f);
                    if (color.A == 0) continue;
                    canvas.Blend(px, py, color);
                }
            }
        }

        public static bool InsidePolygon((float X, float Y)[] points, float x, float y)
        {
            bool inside = false;

            for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
            {
                (float xi, float yi) = points[i];
                (float xj, float yj) = points[j];

                if ((yi > y) != (yj > y) && x < (xj - xi) * (y - yi) / (yj - yi) + xi) inside = !inside;
            }

            return inside;
        }

        /// <summary>Um rebite: o ponto claro em cima, a sombra embaixo e a direita.</summary>
        public static void Rivet(PixelCanvas canvas, int x, int y, Rgba[] ramp)
        {
            canvas.Set(x, y, ramp[3]);
            canvas.Set(x + 1, y, ramp[1]);
            canvas.Set(x, y + 1, ramp[1]);
            canvas.Set(x + 1, y + 1, ramp[0]);
        }

        /// <summary>Uma linha de pixels entre dois pontos (Bresenham).</summary>
        public static void Line(PixelCanvas canvas, int x0, int y0, int x1, int y1, Rgba color)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = -Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int error = dx + dy;

            while (true)
            {
                canvas.Blend(x0, y0, color);
                if (x0 == x1 && y0 == y1) return;

                int doubled = 2 * error;
                if (doubled >= dy) { error += dy; x0 += sx; }
                if (doubled <= dx) { error += dx; y0 += sy; }
            }
        }

        /// <summary>
        /// Uma rachadura que vaza Oxide: um caminho torto de pixels escuros
        /// com o verde aceso no meio. So pinta em cima do que ja existe.
        /// </summary>
        public static void Crack(PixelCanvas canvas, float x, float y, float angle, int length, int seed, float glow)
        {
            double a = angle * Math.PI / 180.0;
            float px = x;
            float py = y;

            for (int i = 0; i < length; i++)
            {
                int ix = (int)Math.Floor(px);
                int iy = (int)Math.Floor(py);

                if (canvas.Get(ix, iy).A > 0)
                {
                    bool lit = glow > 0f && (i % 3 != 0 || glow > 0.7f);
                    canvas.Set(ix, iy, lit ? (glow > 0.85f && i % 2 == 0 ? OxideCore : OxideGlow) : Outline);

                    // A borda escura da fresta.
                    if (canvas.Get(ix + 1, iy).A > 0 && lit) canvas.Set(ix + 1, iy, OxideDeep);
                }

                // Quebra de direcao, como vidro.
                if (Hash(i, seed, 41) % 3 == 0) a += (Hash(i, seed, 42) % 2 == 0 ? 0.6 : -0.6);
                px += (float)Math.Cos(a);
                py += (float)Math.Sin(a);
            }
        }

        /// <summary>
        /// Um brilho em volta de um ponto: so clareia o que ja esta pintado
        /// (halo na lataria), e com outside tambem pinta um halo translucido no vazio.
        /// </summary>
        public static void Glow(PixelCanvas canvas, float cx, float cy, float radius, Rgba color, float strength, bool outside = false)
        {
            for (int py = (int)(cy - radius) - 1; py <= (int)(cy + radius) + 1; py++)
            {
                for (int px = (int)(cx - radius) - 1; px <= (int)(cx + radius) + 1; px++)
                {
                    float dx = px + 0.5f - cx;
                    float dy = py + 0.5f - cy;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy) / radius;
                    if (d > 1f) continue;

                    float k = strength * (1f - d) * (1f - d);
                    Rgba under = canvas.Get(px, py);

                    if (under.A > 0)
                    {
                        canvas.Set(px, py, Rgba.Lerp(under, color.WithAlpha(under.A), k));
                    }
                    else if (outside)
                    {
                        int alpha = (int)(k * 140f);
                        if (alpha >= 12) canvas.Blend(px, py, color.WithAlpha(alpha));
                    }
                }
            }
        }

        /// <summary>Uma nuvenzinha de fumaca ou vapor, translucida, mais clara em cima.</summary>
        public static void Puff(PixelCanvas canvas, float cx, float cy, float r, Rgba light, Rgba dark, int alpha)
        {
            for (int py = (int)(cy - r) - 1; py <= (int)(cy + r) + 1; py++)
            {
                for (int px = (int)(cx - r) - 1; px <= (int)(cx + r) + 1; px++)
                {
                    float dx = px + 0.5f - cx;
                    float dy = py + 0.5f - cy;
                    float d = (dx * dx + dy * dy) / (r * r);
                    if (d > 1f) continue;

                    Rgba color = dy + dx * 0.5f < -r * 0.25f ? light : dark;
                    canvas.Blend(px, py, color.WithAlpha(d > 0.7f ? alpha * 2 / 3 : alpha));
                }
            }
        }

        /// <summary>Uma faisca: uma cruzinha clara, com o miolo branco.</summary>
        public static void Spark(PixelCanvas canvas, int x, int y, int size)
        {
            var hot = new Rgba(255, 236, 160);
            canvas.Set(x, y, new Rgba(255, 255, 240));

            for (int i = 1; i <= size; i++)
            {
                Rgba c = i == size ? new Rgba(250, 170, 70) : hot;
                canvas.Set(x + i, y, c);
                canvas.Set(x - i, y, c);
                canvas.Set(x, y + i, c);
                canvas.Set(x, y - i, c);
            }
        }

        // ------------------------------------------------------------------
        // Contorno
        // ------------------------------------------------------------------

        /// <summary>
        /// Contorna de escuro tudo o que e opaco, por fora. Os translucidos
        /// (fumaca, rastro) ficam sem contorno.
        /// </summary>
        public static void AddOutline(PixelCanvas canvas)
        {
            AddOutline(canvas, Outline);
        }

        public static void AddOutline(PixelCanvas canvas, Rgba color)
        {
            int w = canvas.Width;
            int h = canvas.Height;
            var filled = new bool[w * h];
            for (int i = 0; i < filled.Length; i++) filled[i] = canvas.Pixels[i].A > 200;

            var edges = new System.Collections.Generic.List<int>();

            for (int py = 0; py < h; py++)
            {
                for (int px = 0; px < w; px++)
                {
                    if (filled[px + py * w]) continue;

                    bool touches = (px > 0 && filled[px - 1 + py * w])
                        || (px < w - 1 && filled[px + 1 + py * w])
                        || (py > 0 && filled[px + (py - 1) * w])
                        || (py < h - 1 && filled[px + (py + 1) * w]);

                    if (touches) edges.Add(px + py * w);
                }
            }

            foreach (int i in edges) canvas.Pixels[i] = color;
        }

        // ------------------------------------------------------------------
        // Contas
        // ------------------------------------------------------------------

        public static (float X, float Y) Along(float x, float y, float angle, float distance)
        {
            double a = angle * Math.PI / 180.0;
            return (x + (float)Math.Cos(a) * distance, y + (float)Math.Sin(a) * distance);
        }

        /// <summary>
        /// O joelho (ou cotovelo) de um membro de dois ossos que vai de a ate
        /// b: bend +1 dobra pra direita da linha a-b, -1 pra esquerda.
        /// </summary>
        public static (float X, float Y) Joint(float ax, float ay, float bx, float by, float upper, float lower, int bend)
        {
            float dx = bx - ax;
            float dy = by - ay;
            float d = (float)Math.Sqrt(dx * dx + dy * dy);
            d = Math.Max(0.01f, Math.Min(d, upper + lower - 0.01f));

            float a = (upper * upper - lower * lower + d * d) / (2f * d);
            float h = (float)Math.Sqrt(Math.Max(0f, upper * upper - a * a));
            float ux = dx / d;
            float uy = dy / d;

            float mx = ax + ux * a;
            float my = ay + uy * a;
            return (mx - uy * h * bend, my + ux * h * bend);
        }

        public static int Hash(int x, int y, int seed)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + seed * 1442695041;
                h = (h ^ (h >> 13)) * 1274126177;
                return (h ^ (h >> 16)) & 0x7fffffff;
            }
        }

        /// <summary>Ruido suave entre 0 e 1 (interpolado entre pontos inteiros).</summary>
        public static float Noise(float x, float y, int seed)
        {
            int x0 = (int)Math.Floor(x);
            int y0 = (int)Math.Floor(y);
            float fx = x - x0;
            float fy = y - y0;

            float sx = fx * fx * (3f - 2f * fx);
            float sy = fy * fy * (3f - 2f * fy);

            float a = (Hash(x0, y0, seed) & 1023) / 1023f;
            float b = (Hash(x0 + 1, y0, seed) & 1023) / 1023f;
            float c = (Hash(x0, y0 + 1, seed) & 1023) / 1023f;
            float d = (Hash(x0 + 1, y0 + 1, seed) & 1023) / 1023f;

            float top = a + (b - a) * sx;
            float bottom = c + (d - c) * sx;
            return top + (bottom - top) * sy;
        }
    }
}
