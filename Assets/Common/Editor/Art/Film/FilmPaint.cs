using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O pincel dos filmes: degrades com pontilhado ordenado (o jeito pixel
    /// art de fazer luz suave), luzes que somam, raios, neblina, contraluz
    /// (luz de borda) em personagens, silhuetas e ampliacao.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class FilmPaint
    {
        private static readonly float[] Bayer4 =
        {
            0f, 8f, 2f, 10f,
            12f, 4f, 14f, 6f,
            3f, 11f, 1f, 9f,
            15f, 7f, 13f, 5f,
        };

        /// <summary>O limiar do pontilhado ordenado em (x, y), entre 0 e 1.</summary>
        public static float Bayer(int x, int y) => (Bayer4[(x & 3) + (y & 3) * 4] + 0.5f) / 16f;

        public static Rgba C(int r, int g, int b, int a = 255) => new Rgba(r, g, b, a);

        /// <summary>Uma rampa de cores com pontilhado: t de 0 a 1 escolhe entre as paradas.</summary>
        public static Rgba Ramp(Rgba[] stops, float t, int x, int y)
        {
            if (stops.Length == 1) return stops[0];
            t = Math.Max(0f, Math.Min(0.9999f, t));
            float f = t * (stops.Length - 1);
            int i = (int)f;
            float k = f - i;
            return k > Bayer(x, y) ? stops[Math.Min(stops.Length - 1, i + 1)] : stops[i];
        }

        /// <summary>Uma rampa suave (sem pontilhado), pra misturar cores.</summary>
        public static Rgba Smooth(Rgba[] stops, float t)
        {
            if (stops.Length == 1) return stops[0];
            t = Math.Max(0f, Math.Min(1f, t));
            float f = t * (stops.Length - 1);
            int i = Math.Min(stops.Length - 2, (int)f);
            return Rgba.Lerp(stops[i], stops[i + 1], f - i);
        }

        /// <summary>Degrade vertical pontilhado num retangulo.</summary>
        public static void VGradient(PixelCanvas c, int x0, int y0, int w, int h, params Rgba[] stops)
        {
            for (int y = y0; y < y0 + h; y++)
            {
                float t = h <= 1 ? 0f : (y - y0) / (float)(h - 1);
                for (int x = x0; x < x0 + w; x++) c.Set(x, y, Ramp(stops, t, x, y));
            }
        }

        /// <summary>Degrade radial pontilhado (do centro pra fora) num retangulo.</summary>
        public static void RGradient(PixelCanvas c, float cx, float cy, float radius, int x0, int y0, int w, int h, params Rgba[] stops)
        {
            for (int y = y0; y < y0 + h; y++)
            {
                for (int x = x0; x < x0 + w; x++)
                {
                    float d = (float)Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) / radius;
                    c.Set(x, y, Ramp(stops, d, x, y));
                }
            }
        }

        /// <summary>Soma luz num pixel (como a camada aditiva).</summary>
        public static void AddLight(PixelCanvas c, int x, int y, Rgba color, float amount)
        {
            if (!c.Inside(x, y) || amount <= 0f) return;
            Rgba p = c.Pixels[x + y * c.Width];
            c.Pixels[x + y * c.Width] = new Rgba(
                p.R + (int)(color.R * amount), p.G + (int)(color.G * amount), p.B + (int)(color.B * amount), p.A);
        }

        /// <summary>
        /// Uma luz redonda que soma, em degraus pontilhados (pixel art, nao
        /// borrao): forte no meio, sumindo ate o raio. Com onlyOpaque, so
        /// acende o que ja esta pintado.
        /// </summary>
        public static void Light(PixelCanvas c, float cx, float cy, float radius, Rgba color, float strength,
            bool onlyOpaque = false, float steps = 6f)
        {
            int x0 = (int)(cx - radius) - 1, x1 = (int)(cx + radius) + 1;
            int y0 = (int)(cy - radius) - 1, y1 = (int)(cy + radius) + 1;

            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    if (!c.Inside(x, y)) continue;
                    if (onlyOpaque && c.Pixels[x + y * c.Width].A == 0) continue;

                    float d = (float)Math.Sqrt((x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy)) / radius;
                    if (d >= 1f) continue;
                    float k = (1f - d) * (1f - d);
                    float q = k * steps;
                    int band = (int)q;
                    float level = (band + ((q - band) > Bayer(x, y) ? 1 : 0)) / steps;
                    AddLight(c, x, y, color, level * strength);
                }
            }
        }

        /// <summary>Uma luz numa camada transparente (pra camada aditiva): o alfa e a forca.</summary>
        public static void Halo(PixelCanvas c, float cx, float cy, float radius, Rgba color, float strength, float steps = 6f)
        {
            int x0 = (int)(cx - radius) - 1, x1 = (int)(cx + radius) + 1;
            int y0 = (int)(cy - radius) - 1, y1 = (int)(cy + radius) + 1;

            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    if (!c.Inside(x, y)) continue;
                    float d = (float)Math.Sqrt((x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy)) / radius;
                    if (d >= 1f) continue;
                    float k = (1f - d) * (1f - d);
                    float q = k * steps;
                    int band = (int)q;
                    float level = (band + ((q - band) > Bayer(x, y) ? 1 : 0)) / steps * strength;
                    if (level <= 0f) continue;

                    Rgba p = c.Pixels[x + y * c.Width];
                    int a = Math.Min(255, p.A + (int)(level * 255f));
                    Rgba mixed = p.A == 0 ? color : Rgba.Lerp(p, color, level);
                    c.Pixels[x + y * c.Width] = mixed.WithAlpha(a);
                }
            }
        }

        /// <summary>Raios de luz saindo de um ponto (fatias pontilhadas que somam).</summary>
        public static void Rays(PixelCanvas c, float cx, float cy, int count, float length, float width, Rgba color,
            float strength, int seed, float startAngle = 0f, float spread = 360f, bool onLayer = false)
        {
            for (int y = 0; y < c.Height; y++)
            {
                for (int x = 0; x < c.Width; x++)
                {
                    float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (d > length || d < 2f) continue;

                    float angle = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);
                    float rel = angle - startAngle;
                    while (rel < 0f) rel += 360f;
                    while (rel >= 360f) rel -= 360f;
                    if (rel > spread) continue;

                    float slot = rel / spread * count;
                    int index = (int)slot;
                    float jitter = ArtKit.Hash(index, 0, seed) % 100 / 100f;
                    float center = 0.5f + (jitter - 0.5f) * 0.4f;
                    float w = width * (0.5f + (ArtKit.Hash(index, 1, seed) % 100) / 100f);
                    float off = Math.Abs(slot - index - center) * (spread / count) * (float)(Math.PI / 180.0) * d;
                    if (off > w) continue;

                    float fall = 1f - d / length;
                    float k = fall * fall * (1f - off / w) * strength;
                    if (k * 1.4f < Bayer(x, y) * 0.9f) continue;

                    if (onLayer)
                    {
                        Rgba p = c.Pixels[x + y * c.Width];
                        int a = Math.Min(255, p.A + (int)(k * 255f));
                        c.Pixels[x + y * c.Width] = (p.A == 0 ? color : Rgba.Lerp(p, color, k)).WithAlpha(a);
                    }
                    else
                    {
                        AddLight(c, x, y, color, k);
                    }
                }
            }
        }

        /// <summary>Neblina: manchas de ruido translucidas numa faixa.</summary>
        public static void Fog(PixelCanvas c, int y0, int y1, Rgba color, float density, int seed, float scale = 0.03f)
        {
            for (int y = Math.Max(0, y0); y < Math.Min(c.Height, y1); y++)
            {
                float band = (y - y0) / (float)Math.Max(1, y1 - y0);
                float edge = Math.Min(band, 1f - band) * 2f;
                for (int x = 0; x < c.Width; x++)
                {
                    float n = ArtKit.Noise(x * scale, y * scale * 2f, seed) * 0.65f + ArtKit.Noise(x * scale * 3f, y * scale * 5f, seed + 1) * 0.35f;
                    float k = (n - 0.35f) * 1.6f * density * edge;
                    if (k <= Bayer(x, y) * 0.8f) continue;
                    Rgba p = c.Get(x, y);
                    c.Set(x, y, Rgba.Lerp(p, color.WithAlpha(p.A == 0 ? 255 : p.A), Math.Min(0.75f, k)));
                }
            }
        }

        /// <summary>Pontinhos de luz desfocados ao fundo (velas longe, poeira acesa).</summary>
        public static void Bokeh(PixelCanvas c, int count, int x0, int y0, int w, int h, Rgba color, float minR, float maxR, int seed, float strength = 0.35f)
        {
            for (int i = 0; i < count; i++)
            {
                float x = x0 + ArtKit.Hash(i, 0, seed) % Math.Max(1, w);
                float y = y0 + ArtKit.Hash(i, 1, seed) % Math.Max(1, h);
                float r = minR + (ArtKit.Hash(i, 2, seed) % 100) / 100f * (maxR - minR);
                Light(c, x, y, r, color, strength * (0.5f + (ArtKit.Hash(i, 3, seed) % 100) / 200f), false, 3f);
            }
        }

        /// <summary>Multiplica as cores (escurece e tinge).</summary>
        public static void Multiply(PixelCanvas c, Rgba tint, float amount = 1f)
        {
            for (int i = 0; i < c.Pixels.Length; i++)
            {
                Rgba p = c.Pixels[i];
                if (p.A == 0) continue;
                var m = new Rgba(p.R * tint.R / 255, p.G * tint.G / 255, p.B * tint.B / 255, p.A);
                c.Pixels[i] = Rgba.Lerp(p, m, amount);
            }
        }

        /// <summary>
        /// Contraluz: escurece o personagem (ambiente) e acende a borda virada
        /// pra luz (dirX, dirY e de onde a luz vem). Funciona em qualquer
        /// sprite com transparencia.
        /// </summary>
        public static PixelCanvas Relight(PixelCanvas sprite, Rgba ambient, Rgba rim, float dirX, float dirY,
            int rimWidth = 2, float rimStrength = 1f, float ambientAmount = 1f)
        {
            var c = Copy(sprite);
            Multiply(c, ambient, ambientAmount);

            float len = (float)Math.Sqrt(dirX * dirX + dirY * dirY);
            if (len < 0.001f) return c;
            float ux = dirX / len, uy = dirY / len;

            for (int y = 0; y < c.Height; y++)
            {
                for (int x = 0; x < c.Width; x++)
                {
                    if (sprite.Get(x, y).A < 128) continue;

                    for (int k = 1; k <= rimWidth; k++)
                    {
                        int sx = (int)Math.Round(x + ux * k), sy = (int)Math.Round(y + uy * k);
                        if (sprite.Get(sx, sy).A >= 128) continue;

                        float power = rimStrength * (1f - (k - 1) / (float)(rimWidth + 1));
                        Rgba p = c.Get(x, y);
                        c.Set(x, y, Rgba.Lerp(p, rim.WithAlpha(p.A), Math.Min(1f, power)));
                        break;
                    }
                }
            }

            return c;
        }

        /// <summary>Uma silhueta chapada (mantendo o alfa).</summary>
        public static PixelCanvas Silhouette(PixelCanvas sprite, Rgba color)
        {
            var c = new PixelCanvas(sprite.Width, sprite.Height);
            for (int i = 0; i < c.Pixels.Length; i++)
            {
                int a = sprite.Pixels[i].A;
                if (a > 0) c.Pixels[i] = color.WithAlpha(a);
            }

            return c;
        }

        public static PixelCanvas Copy(PixelCanvas source)
        {
            var c = new PixelCanvas(source.Width, source.Height);
            Array.Copy(source.Pixels, c.Pixels, source.Pixels.Length);
            return c;
        }

        /// <summary>Amplia por um fator inteiro (vizinho mais proximo).</summary>
        public static PixelCanvas Scaled(PixelCanvas source, int factor)
        {
            var c = new PixelCanvas(source.Width * factor, source.Height * factor);
            for (int y = 0; y < c.Height; y++)
                for (int x = 0; x < c.Width; x++)
                    c.Pixels[x + y * c.Width] = source.Pixels[x / factor + y / factor * source.Width];
            return c;
        }

        /// <summary>Espelha na horizontal.</summary>
        public static PixelCanvas Flipped(PixelCanvas source)
        {
            var c = new PixelCanvas(source.Width, source.Height);
            for (int y = 0; y < c.Height; y++)
                for (int x = 0; x < c.Width; x++)
                    c.Pixels[x + y * c.Width] = source.Pixels[source.Width - 1 - x + y * source.Width];
            return c;
        }

        /// <summary>Recorta um pedaco.</summary>
        public static PixelCanvas Crop(PixelCanvas source, int x0, int y0, int w, int h)
        {
            var c = new PixelCanvas(w, h);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    c.Pixels[x + y * w] = source.Get(x0 + x, y0 + y);
            return c;
        }

        /// <summary>Cola um desenho (com alfa) em (x, y).</summary>
        public static void Stamp(PixelCanvas c, PixelCanvas art, int x, int y, float alpha = 1f)
        {
            for (int py = 0; py < art.Height; py++)
            {
                for (int px = 0; px < art.Width; px++)
                {
                    Rgba p = art.Pixels[px + py * art.Width];
                    if (p.A == 0) continue;
                    c.Blend(x + px, y + py, alpha >= 1f ? p : p.WithAlpha((int)(p.A * alpha)));
                }
            }
        }

        /// <summary>Um poligono com sombreado por funcao (pontilhado ja vem da funcao).</summary>
        public static void Poly(PixelCanvas c, Func<int, int, Rgba> shade, params (float X, float Y)[] points)
        {
            ArtKit.Polygon(c, points, (x, y) => shade((int)x, (int)y));
        }

        /// <summary>Uma linha grossa.</summary>
        public static void Thick(PixelCanvas c, float x0, float y0, float x1, float y1, float half, Rgba color)
        {
            float dx = x1 - x0, dy = y1 - y0;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            if (len < 0.01f) { Disc(c, x0, y0, half, color); return; }
            float nx = -dy / len * half, ny = dx / len * half;
            ArtKit.Polygon(c, new[] { (x0 + nx, y0 + ny), (x1 + nx, y1 + ny), (x1 - nx, y1 - ny), (x0 - nx, y0 - ny) }, (x, y) => color);
        }

        public static void Disc(PixelCanvas c, float cx, float cy, float r, Rgba color)
        {
            for (int y = (int)(cy - r) - 1; y <= (int)(cy + r) + 1; y++)
                for (int x = (int)(cx - r) - 1; x <= (int)(cx + r) + 1; x++)
                {
                    float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                    if (dx * dx + dy * dy <= r * r) c.Blend(x, y, color);
                }
        }

        /// <summary>Uma elipse sombreada por funcao (nx, ny de -1 a 1 dentro dela).</summary>
        public static void Oval(PixelCanvas c, float cx, float cy, float rx, float ry, Func<float, float, int, int, Rgba> shade)
        {
            for (int y = (int)(cy - ry) - 1; y <= (int)(cy + ry) + 1; y++)
                for (int x = (int)(cx - rx) - 1; x <= (int)(cx + rx) + 1; x++)
                {
                    float nx = (x + 0.5f - cx) / rx, ny = (y + 0.5f - cy) / ry;
                    if (nx * nx + ny * ny > 1f) continue;
                    Rgba color = shade(nx, ny, x, y);
                    if (color.A > 0) c.Blend(x, y, color);
                }
        }

        /// <summary>Um caminho torto de rachadura, como lista de pontos (do comeco pro fim).</summary>
        public static List<(float X, float Y)> CrackPath(float x, float y, float angle, int length, int seed, float wobble = 0.55f)
        {
            var points = new List<(float, float)>();
            double a = angle * Math.PI / 180.0;
            float px = x, py = y;
            for (int i = 0; i < length; i++)
            {
                points.Add((px, py));
                if (ArtKit.Hash(i, seed, 41) % 4 == 0) a += ArtKit.Hash(i, seed, 42) % 2 == 0 ? wobble : -wobble;
                px += (float)Math.Cos(a);
                py += (float)Math.Sin(a);
            }

            return points;
        }

        /// <summary>Desenha uma parte (0 a 1) de um caminho de rachadura acesa por dentro.</summary>
        public static void DrawCrack(PixelCanvas c, List<(float X, float Y)> path, float amount, Rgba dark, Rgba core, Rgba glow, bool onlyOpaque = true)
        {
            int count = (int)(path.Count * Math.Max(0f, Math.Min(1f, amount)));
            for (int i = 0; i < count; i++)
            {
                int x = (int)path[i].X, y = (int)path[i].Y;
                if (onlyOpaque && c.Get(x, y).A == 0) continue;
                c.Set(x, y, i % 3 == 0 ? glow : core);
                if (!onlyOpaque || c.Get(x + 1, y).A > 0) c.Set(x + 1, y, dark);
                if (!onlyOpaque || c.Get(x, y + 1).A > 0) c.Set(x, y + 1, glow.Scaled(0.7f).WithAlpha(255));
            }
        }

        /// <summary>A luminancia de uma cor, de 0 a 1.</summary>
        public static float Luma(Rgba p) => (p.R * 0.3f + p.G * 0.59f + p.B * 0.11f) / 255f;

        /// <summary>Repinta pela luminancia numa rampa (troca de material mantendo a luz).</summary>
        public static Rgba Remap(Rgba p, Rgba[] ramp, int x, int y)
        {
            return Ramp(ramp, Luma(p) * 1.1f, x, y).WithAlpha(p.A);
        }

        /// <summary>Uma folha de quadros com os mesmos parametros de 0 a 1.</summary>
        public static PixelCanvas[] Frames(int count, Func<float, int, PixelCanvas> draw)
        {
            var frames = new PixelCanvas[count];
            for (int i = 0; i < count; i++) frames[i] = draw(count == 1 ? 1f : i / (float)(count - 1), i);
            return frames;
        }
    }
}
