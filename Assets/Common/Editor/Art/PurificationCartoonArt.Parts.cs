using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class PurificationCartoonArt
    {
        // ------------------------------------------------------------------
        // Pecas do Scrap
        // ------------------------------------------------------------------

        /// <summary>
        /// A cabeca: uma gema lapidada (mesa no meio, facetas em volta). Na
        /// sucata e pedra bruta com rachas; no polido, cristal com quinas claras.
        /// (tableX, tableY) desloca a mesa: pra onde a cabeca "olha".
        /// </summary>
        private static void Gem(Painter p, float cx, float cy, float rx, float ry, float tableX, float tableY,
            OrderFn order, int group, int seed)
        {
            var rng = new Random(seed);
            const int sides = 9;

            var outer = new (float x, float y)[sides];
            var inner = new (float x, float y)[sides];
            for (int i = 0; i < sides; i++)
            {
                double a = -Math.PI / 2 + i * Math.PI * 2 / sides + (rng.NextDouble() - 0.5) * 0.25;
                float k = 0.9f + (float)rng.NextDouble() * 0.16f;
                outer[i] = (cx + (float)Math.Cos(a) * rx * k, cy + (float)Math.Sin(a) * ry * k);
                float ki = 0.5f + (float)rng.NextDouble() * 0.08f;
                inner[i] = (cx + tableX + (float)Math.Cos(a) * rx * ki, cy + tableY + (float)Math.Sin(a) * ry * ki);
            }

            float rise = (rx + ry) * 0.35f;

            // As facetas: a mesa e um quadrilatero entre cada par de lados.
            var facets = new List<((float x, float y)[] pts, float nx, float ny, float nz)>();
            facets.Add((inner, 0.05f, -0.1f, 1f));
            for (int i = 0; i < sides; i++)
            {
                int j = (i + 1) % sides;
                var pts = new[] { outer[i], outer[j], inner[j], inner[i] };

                // Normal pelo produto vetorial (borda em z=0, mesa em z=rise).
                float ax = outer[j].x - outer[i].x, ay = outer[j].y - outer[i].y, az = 0f;
                float bx = inner[i].x - outer[i].x, by = inner[i].y - outer[i].y, bz = rise;
                float nx = ay * bz - az * by, ny = az * bx - ax * bz, nz = ax * by - ay * bx;
                if (nz < 0) { nx = -nx; ny = -ny; nz = -nz; }
                Normalize(ref nx, ref ny, ref nz);
                facets.Add((pts, nx, ny, nz));
            }

            int FacetAt(float x, float y)
            {
                for (int f = 0; f < facets.Count; f++)
                {
                    if (InsidePoly(facets[f].pts, x, y)) return f;
                }

                return -1;
            }

            Probe probe = (int x, int y, out float nx, out float ny, out float nz) =>
            {
                nx = ny = 0; nz = 1;
                int f = FacetAt(x + 0.5f, y + 0.5f);
                if (f < 0) return false;
                nx = facets[f].nx; ny = facets[f].ny; nz = facets[f].nz;
                return true;
            };

            int x0 = (int)(cx - rx * 1.1f) - 1, x1 = (int)(cx + rx * 1.1f) + 1;
            int y0 = (int)(cy - ry * 1.1f) - 1, y1 = (int)(cy + ry * 1.1f) + 1;
            p.Shape(probe, x0, y0, x1, y1, Rock(), Quartz(), order, group);

            // As quinas: claras do lado da luz no cristal, rachas escuras na pedra.
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    int f = FacetAt(x + 0.5f, y + 0.5f);
                    if (f < 0) continue;
                    int right = FacetAt(x + 1.5f, y + 0.5f), down = FacetAt(x + 0.5f, y + 1.5f);
                    if ((right == f || right < 0) && (down == f || down < 0)) continue;

                    bool lit = (x - cx) * 0.6f + (y - cy) < 4f;
                    Rgba after = lit ? new Rgba(250, 254, 255) : GemTones[0];
                    p.Overlay(x, y, S.Stone[0], after, order);
                }
            }

            // Rachas extras e lascas na pedra.
            var crack = new Random(seed + 1);
            for (int i = 0; i < 4; i++)
            {
                float sx = cx + crack.Next(-(int)(rx * 0.6f), (int)(rx * 0.6f));
                float sy = cy + crack.Next(-(int)(ry * 0.6f), (int)(ry * 0.6f));
                double a = crack.NextDouble() * Math.PI * 2;
                for (int k = 0; k < 10; k++)
                {
                    a += (crack.NextDouble() - 0.5) * 0.8;
                    sx += (float)Math.Cos(a); sy += (float)Math.Sin(a);
                    if (FacetAt(sx, sy) < 0) break;
                    p.Overlay((int)sx, (int)sy, S.Stone[0].Scaled(0.8f), null, order);
                }
            }
        }

        private static void Eyes(Painter p, float x0, float x1, float y, OrderFn order)
        {
            foreach (float ex in new[] { x0, x1 })
            {
                const float r = 7f;

                for (int py = (int)(y - r - 5); py <= y + r + 5; py++)
                {
                    for (int px = (int)(ex - r - 5); px <= ex + r + 5; px++)
                    {
                        float d = Dist(px + 0.5f, py + 0.5f, ex, y);
                        Rgba color;

                        if (d <= r * 0.45f) color = P.EyeCore;
                        else if (d <= r * 0.8f) color = Rgba.Lerp(P.EyeCore, P.EyeGlow, (d - r * 0.45f) / (r * 0.35f));
                        else if (d <= r) color = P.EyeGlow.Scaled(0.8f);
                        else continue;

                        p.Overlay(px, py, null, color, order);
                    }
                }

                // O brilhinho do olho.
                p.Overlay((int)(ex - 3), (int)(y - 3), null, new Rgba(255, 255, 255), order);
                p.Overlay((int)(ex - 2), (int)(y - 3), null, new Rgba(255, 255, 255), order);
                p.Overlay((int)(ex - 3), (int)(y - 2), null, new Rgba(255, 255, 255), order);
            }

            // O halo azul em volta dos olhos, pontilhado, so por cima da cabeca.
            for (int py = (int)(y - 14); py <= y + 14; py++)
            {
                for (int px = (int)(x0 - 14); px <= x1 + 14; px++)
                {
                    float d = Math.Min(Dist(px, py, x0, y), Dist(px, py, x1, y));
                    if (d <= 7.5f || d > 12f) continue;
                    if (Bayer(px, py) > 0.55f - (d - 7.5f) * 0.12f) continue;
                    Rgba under = p.After.Get(px, py);
                    p.Overlay(px, py, null, Rgba.Lerp(under, P.EyeGlow, 0.45f), order);
                }
            }
        }

        /// <summary>A mola de cima da cabeca: aneis empilhados e um botao no alto.</summary>
        private static void Spring(Painter p, float cx, float baseY, float radius, int coils, OrderFn order, int group)
        {
            for (int i = 0; i < coils; i++)
            {
                float y = baseY - i * radius * 0.7f;
                float x = cx + (float)Math.Sin(i * 1.3f) * 0.8f;
                p.Ellipse(x, y, radius, radius * 0.45f, RustSpring(), BrightSpring(), order, group + i);
            }

            float top = baseY - coils * radius * 0.7f - 1;
            p.Ellipse(cx, top, radius * 0.7f, radius * 0.62f, RustSpring(), BrightSpring(), order, group + coils);
        }

        /// <summary>
        /// A lamina larga do cinzel, de (ax, ay) ate (bx, by): duas faces com
        /// uma quina no meio e o chanfro na ponta.
        /// </summary>
        private static void ChiselBlade(Painter p, float ax, float ay, float bx, float by, float width, OrderFn order, int group)
        {
            float dx = bx - ax, dy = by - ay;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            float ux = dx / len, uy = dy / len;
            float px = -uy, py = ux;
            float half = width / 2f;

            (float, float) At(float along, float side) => (ax + ux * along + px * side, ay + uy * along + py * side);

            float bevel = len - Math.Max(6f, width * 1.3f);

            // A face "de cima" (normal pro lado -perp) e a "de baixo".
            float tlx = -px * 0.45f, tly = -py * 0.45f;
            p.Poly(new[] { At(0, 0), At(bevel, 0), At(bevel, -half + 1), At(0, -half) },
                Flat(tlx, tly, 0.89f), RustBlade(), Silver(), order, group);
            p.Poly(new[] { At(0, 0), At(bevel, 0), At(bevel, half - 1), At(0, half) },
                Flat(-tlx * 0.6f, -tly * 0.6f, 0.95f), RustBlade(), Silver(), order, group);

            // O chanfro, inclinado pra frente.
            p.Poly(new[] { At(bevel, -half + 1), At(len, -half + 2), At(len, half - 2), At(bevel, half - 1) },
                Flat(ux * 0.6f - 0.2f, uy * 0.6f - 0.3f, 0.7f), RustBlade(), Silver(), order, group);

            // O fio: uma linha clara no polido.
            for (float s = -half + 2; s <= half - 2; s += 0.5f)
            {
                var (x, y) = At(len - 0.5f, s);
                p.Overlay((int)x, (int)y, null, new Rgba(255, 255, 255), order);
            }

            // Dois reflexos em faixa atravessando a lamina polida.
            foreach (float at in new[] { 0.35f, 0.45f })
            {
                for (float s = -half; s <= half; s += 0.5f)
                {
                    for (float w = 0; w < (at < 0.4f ? 3f : 1.5f); w += 0.5f)
                    {
                        var (x, y) = At(bevel * at + w + s * 0.5f, s);
                        if (!p.IsAfterGroup((int)x, (int)y, group)) continue;
                        p.Overlay((int)x, (int)y, null, Rgba.Lerp(SilverTones[3], new Rgba(255, 255, 255), 0.6f), order);
                    }
                }
            }
        }

        /// <summary>
        /// A capa de costas: sai dos ombros arredondados e abre ate a barra.
        /// Na sucata a barra e rasgada (com buracos); no polido e inteira e tem
        /// barra dourada.
        /// </summary>
        private static void BackCape(Painter p, float cx, float top, float bottom, OrderFn order, int group)
        {
            float Half(float y)
            {
                float t = (y - top) / (bottom - top);
                float shoulder = (float)Math.Sqrt(Math.Min(1f, Math.Max(0f, (y - top) / 10f)));
                return (14f + 22f * shoulder) + t * 14f;
            }

            float HemRagged(float x) => bottom + (((int)(x * 7) % 13) < 6 ? -3 : 4) + (float)Math.Sin(x * 0.7f) * 2f;
            float HemClean(float x) => bottom + 2 + (float)Math.Sin(x * 0.2f) * 2f;

            Probe Make(Func<float, float> hem) => (int x, int y, out float nx, out float ny, out float nz) =>
            {
                float fold = (float)Math.Sin((x - cx) * 0.24f + (y - top) * 0.015f);
                float edge = (x - cx) / Math.Max(1f, Half(y));
                nx = fold * 0.45f + edge * 0.5f;
                ny = -0.2f + Math.Max(0f, 1f - (y - top) / 12f) * -0.6f;
                nz = (float)Math.Sqrt(Math.Max(0.05f, 1 - nx * nx - ny * ny));
                if (y < top) return false;
                if (y > hem(x)) return false;
                return Math.Abs(x - cx) <= Half(y);
            };

            int x0 = (int)(cx - 54), x1 = (int)(cx + 54);
            int y0 = (int)top - 2, y1 = (int)bottom + 8;

            p.Shape(Make(HemRagged), x0, y0, x1, y1, RawCloth(), null, order, group);
            p.Shape(Make(HemClean), x0, y0, x1, y1, null, Leather(), order, group);

            // Buracos na sucata.
            var rng = new Random(3);
            for (int i = 0; i < 6; i++)
            {
                float hx = cx + rng.Next(-34, 35), hy = bottom - rng.Next(5, 30);
                int r = 1 + rng.Next(0, 2);
                for (int y = (int)hy - r; y <= hy + r; y++)
                {
                    for (int x = (int)hx - r - 1; x <= hx + r + 1; x++)
                    {
                        if (Dist(x, y, hx, hy) > r + 0.6f) continue;
                        if (!Make(HemRagged)(x, y, out _, out _, out _)) continue;
                        p.CarveBefore(x, y, 0, order);
                    }
                }
            }

            // A barra dourada no polido.
            for (int x = x0; x <= x1; x++)
            {
                float hem = HemClean(x);
                for (int k = 0; k < 3; k++)
                {
                    int y = (int)hem - k;
                    if (!Make(HemClean)(x, y, out _, out _, out _)) continue;
                    p.Overlay(x, y, null, k == 1 ? P.Trim : P.Trim.Scaled(0.75f), order);
                }
            }
        }

        /// <summary>A capa da pose final: sai dos ombros e voa pra direita, ondulando com a fase.</summary>
        private static void HeroCape(Painter p, float phase, OrderFn order, int group)
        {
            double w = phase * Math.PI * 2;

            float Top(float x) => 88 + Math.Max(0, x - 192) * 0.16f + (float)Math.Sin(x * 0.07f - w) * 2.5f * Math.Max(0, (x - 180) / 100f);

            float Bottom(float x)
            {
                float t = Math.Max(0f, (x - 150) / 140f);
                float baseLine = 158 - t * t * 62;
                float wave = (float)Math.Sin(x * 0.09f - w) * 4f * t;
                float tatter = ((int)(x / 7) % 2 == 0) ? 0 : -3 * t;
                return baseLine + wave + tatter;
            }

            Probe probe = (int x, int y, out float nx, out float ny, out float nz) =>
            {
                nx = (float)Math.Sin(x * 0.15f - w) * 0.55f;
                ny = 0.15f;
                nz = (float)Math.Sqrt(1 - nx * nx - ny * ny);
                if (x < 118) return false;
                float top = Top(x), bottom = Bottom(x);
                return bottom - top > 1 && y >= top && y <= bottom;
            };

            p.Shape(probe, 114, 70, 316, 170, null, Leather(), order, group);

            // O forro vermelho escuro aparecendo nas dobras.
            var lining = new[] { new Rgba(52, 18, 24), new Rgba(84, 28, 34), new Rgba(120, 40, 44), new Rgba(152, 60, 56) };
            for (int x = 190; x <= 316; x++)
            {
                float fold = (float)Math.Sin(x * 0.15f - w);
                if (fold < 0.45f) continue;
                int depth = 3 + (int)((fold - 0.45f) * 14);
                for (int y = (int)Bottom(x) - depth; y <= Bottom(x); y++)
                {
                    if (!probe(x, y, out _, out _, out _)) continue;
                    p.Overlay(x, y, null, lining[Math.Min(3, (int)((fold - 0.45f) * 7f))], order);
                }
            }

            // Barra dourada embaixo.
            for (int x = 118; x <= 316; x++)
            {
                int y = (int)Bottom(x);
                for (int k = 0; k < 2; k++)
                {
                    if (!probe(x, y - k, out _, out _, out _)) continue;
                    p.Overlay(x, y - k, null, k == 0 ? P.Trim.Scaled(0.75f) : P.Trim, order);
                }
            }
        }

        /// <summary>O rochedo da pose final: pedra escura com rachas que acendem.</summary>
        private static void Ledge(Painter p, OrderFn order, int group)
        {
            float Top(float x) => 156 + (float)Math.Abs(x - 160) * 0.05f + Noise(x / 9f, 0, 41) * 5f;

            Probe probe = (int x, int y, out float nx, out float ny, out float nz) =>
            {
                float top = Top(x);
                nx = (Noise(x / 6f, y / 6f, 42) - 0.5f) * 0.8f;
                ny = y < top + 3 ? -0.8f : 0.1f;
                nz = (float)Math.Sqrt(Math.Max(0.05f, 1 - nx * nx - ny * ny));
                return x > 56 && x < 268 && y >= top;
            };

            p.Shape(probe, 50, 150, 270, 180, Shade(CaveRock), Shade(CaveRock), order, group);

            // Rachas com luz azul.
            var rng = new Random(9);
            for (int i = 0; i < 7; i++)
            {
                float x = rng.Next(70, 250), y = 162 + rng.Next(0, 6);
                double a = Math.PI * (0.2 + rng.NextDouble() * 0.6);
                for (int k = 0; k < 14; k++)
                {
                    a += (rng.NextDouble() - 0.5) * 0.7;
                    x += (float)Math.Cos(a) * (k % 2 == 0 ? 1 : 0.5f);
                    y += (float)Math.Sin(a) * 0.6f;
                    if (!probe((int)x, (int)y, out _, out _, out _)) break;
                    p.Overlay((int)x, (int)y, CaveRock[0], Rgba.Lerp(Teal, PurifyCore, 0.4f), order);
                }
            }
        }

        /// <summary>O sulco entre duas placas do braco, com rebites no polido.</summary>
        private static void Groove(Painter p, float cx, float cy, float dx, float dy, float radius, OrderFn order, int group)
        {
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            float ux = dx / len, uy = dy / len;
            float px = -uy, py = ux;

            for (float s = -radius; s <= radius; s += 0.5f)
            {
                for (int w = -1; w <= 1; w++)
                {
                    int x = (int)Math.Round(cx + px * s + ux * w);
                    int y = (int)Math.Round(cy + py * s + uy * w);
                    if (!p.IsAfterGroup(x, y, 2)) continue;
                    p.Overlay(x, y, DeepRust.Scaled(0.7f), w == 1 ? BronzeTones[3] : BronzeTones[0], order);
                }
            }

            for (int i = -2; i <= 2; i++)
            {
                float s = i * radius * 0.38f;
                float rx = cx + px * s + ux * 6, ry = cy + py * s + uy * 6;
                p.Ellipse(rx, ry, 2.4f, 2.4f, RustMetal(), Gold(), order, group);
            }
        }

        /// <summary>Um reflexo comprido ao longo do braco polido, do lado da luz.</summary>
        private static void Shine(Painter p, float ax, float ay, float bx, float by, float offset, OrderFn order)
        {
            float dx = bx - ax, dy = by - ay;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            float ux = dx / len, uy = dy / len;
            float px = uy, py = -ux;

            for (float s = 20; s < len - 10; s += 0.5f)
            {
                for (float w = 0; w < 2.5f; w += 0.5f)
                {
                    int x = (int)(ax + ux * s + px * (offset + w)), y = (int)(ay + uy * s + py * (offset + w));
                    if (!p.IsAfterGroup(x, y, 2)) continue;
                    p.Overlay(x, y, null, Rgba.Lerp(BronzeTones[3], new Rgba(255, 250, 230), w < 1 ? 0.7f : 0.3f), order);
                }
            }
        }

        /// <summary>Um reflexo vertical no tubo polido.</summary>
        private static void ShineVertical(Painter p, float x, float y0, float y1, OrderFn order)
        {
            for (int y = (int)y0; y <= y1; y++)
            {
                for (int k = 0; k < 3; k++)
                {
                    int px = (int)x + k;
                    if (!p.IsAfterGroup(px, y, 1) && !p.IsAfterGroup(px, y, 4)) continue;
                    p.Overlay(px, y, null, Rgba.Lerp(BronzeTones[3], new Rgba(255, 250, 230), k == 1 ? 0.7f : 0.25f), order);
                }
            }
        }

        /// <summary>
        /// Uma rachadura que anda e se ramifica. No antes e uma linha escura; a
        /// ordem dela vem antes da placa em volta (bias negativo), entao a luz
        /// vaza por ela primeiro.
        /// </summary>
        private static void Crack(Painter p, float x, float y, Random rng, int steps, OrderFn order, int bias, int depth)
        {
            double angle = rng.NextDouble() * Math.PI * 2;
            for (int i = 0; i < steps; i++)
            {
                angle += (rng.NextDouble() - 0.5) * 0.9;
                x += (float)Math.Cos(angle);
                y += (float)Math.Sin(angle);

                int px = (int)Math.Round(x), py = (int)Math.Round(y);
                int o = order(px, py) + bias + i;
                p.OverlayOrder(px, py, DeepRust.Scaled(0.55f), null, o);

                if (depth > 0 && rng.NextDouble() < 0.08)
                {
                    int start = o;
                    Crack(p, x, y, rng, steps / 2, (cx, cy) => start, 0, depth - 1);
                }
            }
        }

        /// <summary>Um tufo de musgo com cristais: seco e cinza antes, verde e aceso depois.</summary>
        private static void Moss(Painter p, float cx, float cy, Random rng, OrderFn order, int group)
        {
            var dead = new[] { new Rgba(34, 34, 32), new Rgba(54, 52, 44), new Rgba(74, 70, 58), new Rgba(94, 88, 70) };
            var green = new[] { new Rgba(22, 56, 42), new Rgba(36, 90, 58), new Rgba(64, 132, 72), new Rgba(120, 188, 104) };

            int blobs = 4 + rng.Next(0, 4);
            for (int i = 0; i < blobs; i++)
            {
                float bx = cx + rng.Next(-12, 13), by = cy + rng.Next(-4, 5);
                p.Ellipse(bx, by, 4 + rng.Next(0, 5), 2 + rng.Next(0, 3), new Mat(dead, Surface.Moss, dead[3], 0f),
                    new Mat(green, Surface.Moss, new Rgba(180, 236, 140), 0f), order, group);
            }

            // Os cristais so existem depois.
            var crystal = new Mat(new[] { new Rgba(30, 96, 130), new Rgba(56, 160, 196), new Rgba(120, 220, 240), new Rgba(220, 255, 255) }, Surface.Crystal, PurifyCore, 0.5f);
            int count = 1 + rng.Next(0, 3);
            for (int i = 0; i < count; i++)
            {
                float bx = cx + rng.Next(-10, 11), by = cy - 1;
                float h = 6 + rng.Next(0, 8);
                float lean = (rng.Next(0, 2) == 0 ? -1 : 1) * rng.Next(0, 4);
                p.Poly(new[] { (bx - 3, by), (bx, by + 1), (bx + lean * 0.5f, by - h - 2), (bx - 1 + lean, by - h) },
                    Flat(-0.7f, -0.3f, 0.64f), null, crystal, order, group + 100);
                p.Poly(new[] { (bx, by + 1), (bx + 3, by), (bx + 1 + lean, by - h), (bx + lean * 0.5f, by - h - 2) },
                    Flat(0.5f, -0.2f, 0.84f), null, crystal, order, group + 100);
            }
        }
    }
}
