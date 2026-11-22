using System;
using static EchoesOfCravejada.EditorTools.FilmPaint;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>Os andares do corte da cidade, o Coracao e a luz correndo na Coluna.</summary>
    public static partial class FinaleFilms
    {
        private static void Ateliers(PixelCanvas c, int y0, int y1, float lit)
        {
            Carve(c, y0, y1, new[] { C(28, 22, 40), C(52, 40, 70), C(84, 64, 108), C(126, 100, 150) }, 0.4f, 841);
            int floor = y1 - 6;

            // Alambiques e retortas em silhueta contra a luz lilas.
            for (int i = 0; i < 9; i++)
            {
                int x = 12 + i * 36 + ArtKit.Hash(i, 0, 842) % 8;
                if (Math.Abs(x - ColumnX) < 26) continue;
                float r = 7 + ArtKit.Hash(i, 2, 842) % 6;
                Disc(c, x, floor - r - 2, r, C(26, 20, 38));
                Thick(c, x, floor - r * 2 + 1, x, floor - r * 2 - 12, 2.2f, C(26, 20, 38));
                Thick(c, x, floor - r * 2 - 12, x + 10, floor - r * 2 - 4, 1.2f, C(26, 20, 38));
                Thick(c, x - r, floor - 1, x + r, floor - 1, 1f, C(26, 20, 38));
                bool on = (ArtKit.Hash(i, 3, 842) % 100) / 100f < lit;
                Rgba liquid = i % 3 == 0 ? C(220, 120, 255) : i % 3 == 1 ? C(120, 230, 255) : C(255, 150, 200);
                if (on)
                {
                    Disc(c, x, floor - r - 2, r - 2, liquid.Scaled(0.7f).WithAlpha(255));
                    Light(c, x, floor - r, r * 4f, liquid, 0.22f, false, 4f);
                }
            }

            // Um mezanino com janelas redondas.
            for (int x = 0; x < c.Width; x++) c.Set(x, y0 + 52, Ink);
            for (int i = 0; i < 8; i++)
            {
                int wx = 18 + i * 40;
                if (Math.Abs(wx - ColumnX) < 26) continue;
                bool on = (ArtKit.Hash(i, 4, 843) % 100) / 100f < lit;
                Oval(c, wx, y0 + 28, 7, 7, (nx, ny, px, py) => on ? Ramp(new[] { C(150, 110, 200), C(255, 220, 255) }, 0.8f - ny * 0.5f, px, py) : C(20, 16, 30));
                if (on) Light(c, wx, y0 + 28, 20f, C(220, 170, 255), 0.18f, false, 3f);
            }

            for (int x = 0; x < c.Width; x++) for (int k = 0; k < 6; k++) c.Set(x, floor + k, Ramp(Marble, 0.4f - k * 0.05f, x, floor + k));
        }

        private static void Pipes(PixelCanvas c, int y0, int y1, float lit)
        {
            Carve(c, y0, y1, new[] { C(30, 18, 12), C(58, 36, 20), C(94, 60, 32), C(140, 96, 54) }, 0.38f, 851);

            // Canos grossos de latao de ponta a ponta, flanges e manometros.
            int[] rows = { y0 + 24, y0 + 58, y0 + 96 };
            for (int r = 0; r < rows.Length; r++)
            {
                int py = rows[r];
                int half = 7 - r;
                for (int x = 0; x < c.Width; x++)
                    for (int k = -half; k <= half; k++)
                    {
                        float u = k / (float)half;
                        c.Set(x, py + k, Ramp(Brass, 0.62f - u * 0.45f + (Math.Abs(u + 0.5f) < 0.15f ? 0.3f : 0f), x, py + k));
                    }

                for (int x = 20 + r * 17; x < c.Width; x += 46)
                {
                    for (int k = -half - 2; k <= half + 2; k++) for (int d = 0; d < 4; d++) c.Set(x + d, py + k, Ramp(Brass, 0.5f - d * 0.12f, x + d, py + k));
                    ArtKit.Rivet(c, x + 1, py - half - 1, Brass);
                }
            }

            for (int i = 0; i < 6; i++)
            {
                int gx = 26 + i * 52;
                if (Math.Abs(gx - ColumnX) < 30) continue;
                int gy = y0 + 76;
                Oval(c, gx, gy, 8, 8, (nx, ny, px, py) => Ramp(Brass, 0.7f - nx * 0.3f - ny * 0.3f, px, py));
                bool on = (ArtKit.Hash(i, 0, 852) % 100) / 100f < lit;
                Oval(c, gx, gy, 5.5f, 5.5f, (nx, ny, px, py) => on ? C(255, 236, 190) : C(70, 62, 50));
                ArtKit.Line(c, gx, gy, gx + 3, gy - 3, Ink);
                if (on) Light(c, gx, gy, 22f, Candle, 0.22f, false, 3f);
            }

            for (int i = 0; i < 10; i++)
            {
                float px = ArtKit.Hash(i, 0, 853) % c.Width;
                float py = y0 + 30 + ArtKit.Hash(i, 1, 853) % 90;
                ArtKit.Puff(c, px, py, 9f, C(200, 190, 170), C(140, 120, 100), 70);
            }

            for (int x = 0; x < c.Width; x++)
            {
                for (int k = 0; k < 6; k++) c.Set(x, y1 - 6 + k, Ramp(Brass, 0.3f - k * 0.04f, x, y1));
                if (x % 12 == 0) ArtKit.Rivet(c, x, y1 - 5, Brass);
            }

            for (int i = 0; i < 14; i++)
            {
                int wx = 6 + ArtKit.Hash(i, 2, 854) % (c.Width - 12);
                if (Math.Abs(wx - ColumnX) < 22) continue;
                Window(c, wx, y0 + 108 + ArtKit.Hash(i, 3, 854) % 10, 4, 6, lit, Candle, 855);
            }
        }

        private static void Geodes(PixelCanvas c, int y0, int y1, float lit)
        {
            Carve(c, y0, y1, new[] { C(16, 10, 28), C(30, 20, 50), C(48, 32, 76), C(70, 50, 104) }, 0.3f, 861);

            for (int i = 0; i < 26; i++)
            {
                bool ceiling = i % 2 == 0;
                float x = 6 + ArtKit.Hash(i, 0, 862) % (c.Width - 12);
                if (Math.Abs(x - ColumnX) < 22) continue;
                float baseY = ceiling ? y0 + 1 : y1 - 1;
                float len = 12 + ArtKit.Hash(i, 1, 862) % 26;
                float lean = (ArtKit.Hash(i, 2, 862) % 20 - 10) * 0.6f;
                float tipY = ceiling ? baseY + len : baseY - len;
                float w = 3 + len * 0.18f;
                Poly(c, (px, py) => Ramp(Amethyst, px < x + lean * 0.5f ? 0.75f : 0.4f, px, py), (x - w, baseY), (x + w, baseY), (x + lean + 0.5f, tipY));
                if ((ArtKit.Hash(i, 3, 862) % 100) / 100f < 0.4f + lit * 0.4f) Light(c, x + lean * 0.5f, (baseY + tipY) * 0.5f, len, C(190, 140, 255), 0.15f, false, 3f);
            }

            for (int i = 0; i < 5; i++)
            {
                int lx = 30 + i * 64;
                if (Math.Abs(lx - ColumnX) < 30) continue;
                ArtKit.Line(c, lx, y0 + 1, lx, y0 + 30, C(40, 34, 30));
                bool on = (ArtKit.Hash(i, 0, 863) % 100) / 100f < lit;
                c.Fill(lx - 2, y0 + 30, 5, 6, on ? Warm : C(50, 44, 40));
                if (on) Light(c, lx, y0 + 33, 26f, Candle, 0.28f, false, 4f);
            }
        }

        private static void HeartCave(PixelCanvas c, int y0, int y1, float heart)
        {
            for (int y = y0; y < y1; y++)
                for (int x = 0; x < c.Width; x++)
                {
                    float d = (float)Math.Sqrt((x - ColumnX) * (x - ColumnX) * 0.6f + (y - HeartY) * (y - HeartY)) / 150f;
                    if (d > 1f) continue;
                    Rgba[] wall = Mix(new[] { C(6, 16, 12), C(14, 34, 26), C(24, 56, 42), C(44, 90, 66) },
                        new[] { C(10, 12, 26), C(24, 28, 54), C(44, 52, 90), C(80, 96, 150) }, heart);
                    c.Set(x, y, Ramp(wall, 0.85f - d * 0.8f + ArtKit.Noise(x * 0.08f, y * 0.08f, 871) * 0.2f, x, y));
                }

            HeartGem(c, ColumnX, HeartY, 30f, heart, 1f, 872);
        }

        /// <summary>
        /// O Coracao de Quartzo: um octaedro grande, de facetas largas. heart 0
        /// doente (verde, com crosta de Oxide), 1 limpo (azul-branco). glow e a
        /// forca da luz em volta.
        /// </summary>
        private static void HeartGem(PixelCanvas c, float cx, float cy, float r, float heart, float glow, int seed)
        {
            Rgba[] ramp = Mix(SickQuartz, new[] { C(70, 90, 150), C(130, 160, 220), C(200, 222, 255), C(236, 246, 255), C(255, 255, 255) }, heart);
            Rgba light = Rgba.Lerp(Oxide, Pure, heart);
            Light(c, cx, cy, r * 3.4f, light, 0.45f * glow, false, 7f);

            float top = cy - r * 1.45f, bottom = cy + r * 1.45f;
            var left = (cx - r, cy);
            var right = (cx + r, cy);
            var mid = (cx + r * 0.18f, cy + r * 0.12f);

            Poly(c, (x, y) => Ramp(ramp, 0.95f - (cy - y) / (r * 4f), x, y), (cx, top), left, mid);
            Poly(c, (x, y) => Ramp(ramp, 0.68f, x, y), (cx, top), mid, right);
            Poly(c, (x, y) => Ramp(ramp, 0.48f, x, y), left, mid, (cx, bottom));
            Poly(c, (x, y) => Ramp(ramp, 0.28f, x, y), mid, right, (cx, bottom));
            ArtKit.Line(c, (int)cx, (int)top, (int)mid.Item1, (int)mid.Item2, ramp[4]);
            ArtKit.Line(c, (int)left.Item1, (int)left.Item2, (int)mid.Item1, (int)mid.Item2, ramp[3]);

            if (heart < 1f)
            {
                for (int y = (int)top; y <= (int)bottom; y++)
                    for (int x = (int)(cx - r); x <= (int)(cx + r); x++)
                    {
                        if (c.Get(x, y).A == 0) continue;
                        float d = Math.Abs(x - cx) / r + Math.Abs(y - cy) / (r * 1.45f);
                        if (d > 1f) continue;
                        float n = ArtKit.Noise(x * 0.18f, y * 0.18f, seed);
                        if (n > 0.62f - (1f - heart) * 0.18f && n < 0.62f + (1f - heart) * 0.25f)
                            c.Set(x, y, n > 0.7f ? C(30, 60, 44) : Rgba.Lerp(OxideDeep, C(20, 40, 30), 0.4f));
                    }
            }

            ArtKit.Line(c, (int)cx, (int)top, (int)left.Item1, (int)left.Item2, Ink);
            ArtKit.Line(c, (int)cx, (int)top, (int)right.Item1, (int)right.Item2, Ink);
            ArtKit.Line(c, (int)left.Item1, (int)left.Item2, (int)cx, (int)bottom, Ink);
            ArtKit.Line(c, (int)right.Item1, (int)right.Item2, (int)cx, (int)bottom, Ink);
            c.Set((int)(cx - r * 0.35f), (int)(cy - r * 0.6f), C(255, 255, 255));
        }

        /// <summary>
        /// A luz correndo na Coluna (camada aditiva, 96 de largura e a altura
        /// do corte): enche ate fill (0 a 1), de baixo pra cima (up) ou de cima
        /// pra baixo. A ponta e mais forte.
        /// </summary>
        private static PixelCanvas ColumnEnergy(float fill, bool up, Rgba color, Rgba core)
        {
            const int w = 96;
            var c = new PixelCanvas(w, SectionH);
            float from = ColumnTop, to = HeartY - 30f;
            float span = to - from;
            float head = up ? to - span * fill : from + span * fill;

            for (int y = (int)from; y < (int)to; y++)
            {
                bool inside = up ? y >= head : y <= head;
                if (!inside) continue;
                float near = 1f - Math.Min(1f, Math.Abs(y - head) / 60f);
                float wave = (float)Math.Sin(y * 0.15f) * 2f;

                for (int x = 0; x < w; x++)
                {
                    float d = Math.Abs(x - w * 0.5f - wave * 0.4f);
                    float k;
                    if (d < 2f) k = 1f;
                    else if (d < 9f) k = 0.55f + near * 0.3f;
                    else k = Math.Max(0f, 0.35f * (1f - (d - 9f) / 36f)) * (0.5f + near);
                    if (k <= 0f || k < Bayer(x, y) * 0.35f) continue;
                    Rgba col = d < 2f || near > 0.7f && d < 6f ? core : color;
                    c.Set(x, y, col.WithAlpha((int)(Math.Min(1f, k) * 255f)));
                }
            }

            Halo(c, w * 0.5f, head, 40f, core, 0.9f, 6f);
            return c;
        }
    }
}
