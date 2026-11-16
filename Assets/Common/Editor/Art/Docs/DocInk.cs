using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>Como a tinta sai: largura, bico (grosso e fino), pressao, falhas.</summary>
    public sealed class Pen
    {
        public float Width = 1.4f;

        /// <summary>Angulo do bico, em graus: o traco nessa direcao sai fino.</summary>
        public float Nib = 35f;

        /// <summary>0 caneta de ponta redonda; 1 pena de bico chato (contraste total).</summary>
        public float Contrast = 0.6f;

        /// <summary>Variacao de pressao ao longo do traco (0..1).</summary>
        public float Pressure = 0.3f;

        public float Opacity = 0.92f;

        /// <summary>Falhas de tinta (pena seca, carimbo gasto): 0 nenhuma.</summary>
        public float Dry;

        public int Seed = 1;

        public Pen With(float width, float contrast = -1f, float opacity = -1f)
        {
            var p = (Pen)MemberwiseClone();
            p.Width = width;
            if (contrast >= 0f) p.Contrast = contrast;
            if (opacity >= 0f) p.Opacity = opacity;
            return p;
        }
    }

    /// <summary>Escrever, riscar, entalhar e fundir texto numa <see cref="DocSurface"/>.</summary>
    public static class DocInk
    {
        private const int SS = 3;

        // ------------------------------------------------------------------
        // Tinta
        // ------------------------------------------------------------------

        /// <summary>Quanto de cada pixel o traco cobre (0..1), com a largura do bico.</summary>
        public static float[] Coverage(DocSurface s, List<List<P2>> strokes, Pen pen)
        {
            int hw = s.W * SS, hh = s.H * SS;
            var hi = new float[hw * hh];
            double nib = pen.Nib * Math.PI / 180.0;

            int si = 0;
            foreach (List<P2> stroke in strokes)
            {
                si++;
                if (stroke.Count == 0) continue;
                if (stroke.Count == 1)
                {
                    Capsule(hi, hw, hh, stroke[0], stroke[0], pen.Width * 0.6f, pen.Width * 0.6f);
                    continue;
                }

                float along = 0f;
                for (int k = 0; k + 1 < stroke.Count; k++)
                {
                    P2 a = stroke[k], b = stroke[k + 1];
                    float dx = b.X - a.X, dy = b.Y - a.Y;
                    float len = (float)Math.Sqrt(dx * dx + dy * dy);
                    double ang = Math.Atan2(dy, dx);
                    float shape = 1f - pen.Contrast + pen.Contrast * (float)Math.Abs(Math.Sin(ang - nib));
                    float p0 = 1f + (DocSurface.Fbm(along * 0.08f, si * 3.1f, pen.Seed, 2) - 0.5f) * 2f * pen.Pressure;
                    float p1 = 1f + (DocSurface.Fbm((along + len) * 0.08f, si * 3.1f, pen.Seed, 2) - 0.5f) * 2f * pen.Pressure;
                    float r0 = Math.Max(0.35f, pen.Width * 0.5f * shape * p0);
                    float r1 = Math.Max(0.35f, pen.Width * 0.5f * shape * p1);

                    // A tinta junta onde a pena pousa e onde levanta.
                    if (k == 0) r0 *= 1.15f;
                    if (k + 2 == stroke.Count) r1 *= 1.1f;

                    Capsule(hi, hw, hh, a, b, r0, r1);
                    along += len;
                }
            }

            var cov = new float[s.W * s.H];
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                {
                    float sum = 0f;
                    for (int j = 0; j < SS; j++)
                        for (int i = 0; i < SS; i++)
                            sum += hi[(x * SS + i) + (y * SS + j) * hw];
                    cov[x + y * s.W] = sum / (SS * SS);
                }

            return cov;
        }

        private static void Capsule(float[] hi, int hw, int hh, P2 a, P2 b, float r0, float r1)
        {
            float ax = a.X * SS, ay = a.Y * SS, bx = b.X * SS, by = b.Y * SS;
            float R0 = r0 * SS, R1 = r1 * SS, rm = Math.Max(R0, R1);
            int minX = Math.Max(0, (int)(Math.Min(ax, bx) - rm - 1)), maxX = Math.Min(hw - 1, (int)(Math.Max(ax, bx) + rm + 1));
            int minY = Math.Max(0, (int)(Math.Min(ay, by) - rm - 1)), maxY = Math.Min(hh - 1, (int)(Math.Max(ay, by) + rm + 1));
            float dx = bx - ax, dy = by - ay;
            float l2 = dx * dx + dy * dy;
            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                {
                    float px = x + 0.5f - ax, py = y + 0.5f - ay;
                    float t = l2 > 0f ? Math.Max(0f, Math.Min(1f, (px * dx + py * dy) / l2)) : 0f;
                    float qx = px - dx * t, qy = py - dy * t;
                    float d = (float)Math.Sqrt(qx * qx + qy * qy);
                    float r = R0 + (R1 - R0) * t;
                    if (d <= r) hi[x + y * hw] = 1f;
                }
        }

        /// <summary>Tinta no papel: escurece a cor de baixo, com variacao de carga e falhas.</summary>
        public static void Ink(DocSurface s, List<List<P2>> strokes, C3 color, Pen pen)
        {
            float[] cov = Coverage(s, strokes, pen);
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                {
                    int i = x + y * s.W;
                    float c = cov[i];
                    if (c <= 0f || s.A[i] <= 0f) continue;
                    float load = 0.82f + DocSurface.Fbm(x * 0.07f, y * 0.07f, pen.Seed + 17, 2) * 0.36f;
                    if (pen.Dry > 0f)
                    {
                        float grain = DocSurface.Fbm(x * 0.35f, y * 0.35f, pen.Seed + 23, 2) + DocSurface.Rand(x, y, pen.Seed) * 0.25f;
                        if (grain < pen.Dry) continue;
                    }

                    float k = DocSurface.Clamp01(c * 1.25f) * pen.Opacity * Math.Min(1f, load);
                    C3 under = s.Col[i];
                    C3 inked = C3.Lerp(color, color * (under * 1.2f), 0.25f);
                    s.Col[i] = C3.Lerp(under, inked, k);
                }
        }

        /// <summary>Giz: graos soltos, o traco falhado e um po em volta.</summary>
        public static void Chalk(DocSurface s, List<List<P2>> strokes, C3 color, Pen pen, float dust = 0.15f)
        {
            float[] cov = Coverage(s, strokes, pen);
            float[] halo = s.BoxBlur(cov, 2);
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                {
                    int i = x + y * s.W;
                    if (s.A[i] <= 0f) continue;
                    float grain = DocSurface.Rand(x, y, pen.Seed) * 0.6f + DocSurface.Fbm(x * 0.5f, y * 0.5f, pen.Seed + 3, 2) * 0.6f;
                    float k = cov[i] > 0.2f && grain > 0.42f ? Math.Min(1f, cov[i] * 1.3f) * pen.Opacity : 0f;
                    k = Math.Max(k, halo[i] * dust);
                    if (k > 0f) s.Col[i] = C3.Lerp(s.Col[i], color, k);
                }
        }

        /// <summary>Carimbo de borracha: tinta irregular, mais carregada nas bordas, com buracos.</summary>
        public static void Stamp(DocSurface s, List<List<P2>> strokes, C3 color, Pen pen, float wear = 0.4f)
        {
            float[] cov = Coverage(s, strokes, pen);
            float[] soft = s.BoxBlur(cov, 1);
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                {
                    int i = x + y * s.W;
                    if (cov[i] <= 0f || s.A[i] <= 0f) continue;
                    float n = DocSurface.Fbm(x * 0.09f, y * 0.09f, pen.Seed + 5, 3);
                    if (n < wear * 0.75f) continue;
                    float rim = DocSurface.Clamp01((cov[i] - soft[i]) * 4f + 0.75f);
                    float k = DocSurface.Clamp01(cov[i] * 1.3f) * pen.Opacity * rim * (0.6f + n * 0.5f);
                    s.Col[i] = C3.Lerp(s.Col[i], color * (s.Col[i] * 1.15f), k);
                }
        }

        // ------------------------------------------------------------------
        // Relevo
        // ------------------------------------------------------------------

        /// <summary>A distancia de cada pixel ao traco mais proximo (ate maxR).</summary>
        public static float[] Distance(DocSurface s, List<List<P2>> strokes, float maxR)
        {
            var d = new float[s.W * s.H];
            for (int i = 0; i < d.Length; i++) d[i] = 1e6f;
            foreach (List<P2> stroke in strokes)
            {
                for (int k = 0; k < stroke.Count; k++)
                {
                    P2 a = stroke[k], b = k + 1 < stroke.Count ? stroke[k + 1] : stroke[k];
                    int minX = Math.Max(0, (int)(Math.Min(a.X, b.X) - maxR - 1)), maxX = Math.Min(s.W - 1, (int)(Math.Max(a.X, b.X) + maxR + 1));
                    int minY = Math.Max(0, (int)(Math.Min(a.Y, b.Y) - maxR - 1)), maxY = Math.Min(s.H - 1, (int)(Math.Max(a.Y, b.Y) + maxR + 1));
                    float dx = b.X - a.X, dy = b.Y - a.Y, l2 = dx * dx + dy * dy;
                    for (int y = minY; y <= maxY; y++)
                        for (int x = minX; x <= maxX; x++)
                        {
                            float px = x + 0.5f - a.X, py = y + 0.5f - a.Y;
                            float t = l2 > 0f ? Math.Max(0f, Math.Min(1f, (px * dx + py * dy) / l2)) : 0f;
                            float qx = px - dx * t, qy = py - dy * t;
                            float dist = (float)Math.Sqrt(qx * qx + qy * qy);
                            int i = x + y * s.W;
                            if (dist < d[i]) d[i] = dist;
                        }
                }
            }

            return d;
        }

        /// <summary>
        /// Entalhe em V (cinzel ou ponta): o sulco desce ate o meio do traco,
        /// mostra o material fresco de dentro, levanta uma rebarba na borda e
        /// solta lascas aqui e ali.
        /// </summary>
        public static void Carve(DocSurface s, List<List<P2>> strokes, float width, float depth, C3 inner, float innerAmount,
            int seed, float chips = 0.5f, float burr = 0.25f)
        {
            float[] d = Distance(s, strokes, width + 3f);
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                {
                    int i = x + y * s.W;
                    if (s.A[i] <= 0f) continue;
                    float dist = d[i];
                    if (dist < width)
                    {
                        float t = 1f - dist / width;
                        s.Ht[i] -= depth * t;
                        float k = innerAmount * (0.55f + 0.45f * t) * (0.85f + DocSurface.Rand(x, y, seed) * 0.3f);
                        s.Col[i] = C3.Lerp(s.Col[i], inner, DocSurface.Clamp01(k));
                        s.Gloss[i] *= 0.4f;
                    }
                    else if (dist < width + 1.4f)
                    {
                        s.Ht[i] += depth * burr * (1f - (dist - width) / 1.4f);
                    }
                }

            if (chips <= 0f) return;

            // Lascas: o material salta na beira do sulco, nos cantos dos tracos.
            int count = 0;
            foreach (List<P2> stroke in strokes)
            {
                for (int k = 0; k < stroke.Count; k++)
                {
                    count++;
                    if (DocSurface.Rand(count, 1, seed + 7) > chips * 0.6f) continue;
                    P2 p = stroke[k];
                    float ox = (DocSurface.Rand(count, 2, seed + 7) - 0.5f) * width * 2.6f;
                    float oy = (DocSurface.Rand(count, 3, seed + 7) - 0.5f) * width * 2.6f;
                    float r = width * (0.5f + DocSurface.Rand(count, 4, seed + 7) * 0.7f);
                    Chip(s, p.X + ox, p.Y + oy, r, depth * 0.55f, inner, innerAmount * 0.7f);
                }
            }
        }

        private static void Chip(DocSurface s, float cx, float cy, float r, float depth, C3 inner, float k)
        {
            for (int y = (int)(cy - r - 1); y <= cy + r + 1; y++)
                for (int x = (int)(cx - r - 1); x <= cx + r + 1; x++)
                {
                    if (!s.In(x, y)) continue;
                    // Lasca angulosa: losango, nao circulo.
                    float d = (Math.Abs(x - cx) + Math.Abs(y - cy) * 1.3f) / r;
                    if (d > 1f) continue;
                    int i = x + y * s.W;
                    s.Ht[i] -= depth * (1f - d);
                    s.Col[i] = C3.Lerp(s.Col[i], inner, k * (1f - d * 0.5f));
                }
        }

        /// <summary>
        /// Letra fundida em relevo (placa de bronze): sobe com bisel, e o topo,
        /// polido pelo uso, fica mais claro que o fundo.
        /// </summary>
        public static void Emboss(DocSurface s, List<List<P2>> strokes, float width, float height, float bevel,
            C3 top, float topAmount, float gloss = 0.9f)
        {
            float[] d = Distance(s, strokes, width + bevel + 2f);
            for (int i = 0; i < d.Length; i++)
            {
                if (s.A[i] <= 0f || d[i] > width) continue;
                float t = DocSurface.Clamp01((width - d[i]) / bevel);
                s.Ht[i] += height * (float)Math.Sqrt(t);
                s.Col[i] = C3.Lerp(s.Col[i], top, topAmount * t);
                s.Gloss[i] = Math.Max(s.Gloss[i], gloss * t);
            }
        }

        /// <summary>Gravado fundo de fundo chato (buril, fresa), com esmalte opcional dentro.</summary>
        public static void Engrave(DocSurface s, List<List<P2>> strokes, float width, float depth, C3 fill, float fillAmount,
            float bevel = 0.8f)
        {
            float[] d = Distance(s, strokes, width + 2f);
            for (int i = 0; i < d.Length; i++)
            {
                if (s.A[i] <= 0f || d[i] > width) continue;
                float t = DocSurface.Clamp01((width - d[i]) / bevel);
                s.Ht[i] -= depth * t;
                s.Col[i] = C3.Lerp(s.Col[i], fill, fillAmount * t);
                s.Gloss[i] *= 1f - 0.6f * t;
            }
        }

        // ------------------------------------------------------------------
        // Transformar tracos
        // ------------------------------------------------------------------

        /// <summary>
        /// Risco de prego ou faca: a curva vira poucos cortes retos, cada um
        /// passa um pouco da conta nas pontas e as vezes leva uma segunda passada.
        /// </summary>
        public static List<List<P2>> Scratchify(List<List<P2>> strokes, int seed, float tolerance = 1.1f, float overshoot = 1.2f,
            float doublePass = 0.35f)
        {
            var result = new List<List<P2>>();
            int n = 0;
            foreach (List<P2> stroke in strokes)
            {
                List<P2> simple = Simplify(stroke, tolerance);
                for (int k = 0; k + 1 < simple.Count; k++)
                {
                    n++;
                    P2 a = simple[k], b = simple[k + 1];
                    float dx = b.X - a.X, dy = b.Y - a.Y;
                    float len = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (len < 0.01f) continue;
                    float ux = dx / len, uy = dy / len;
                    float o0 = overshoot * DocSurface.Rand(n, 1, seed), o1 = overshoot * DocSurface.Rand(n, 2, seed);
                    var cut = new List<P2>
                    {
                        new P2(a.X - ux * o0, a.Y - uy * o0),
                        new P2((a.X + b.X) * 0.5f + (DocSurface.Rand(n, 5, seed) - 0.5f) * 0.6f, (a.Y + b.Y) * 0.5f + (DocSurface.Rand(n, 6, seed) - 0.5f) * 0.6f),
                        new P2(b.X + ux * o1, b.Y + uy * o1),
                    };
                    result.Add(cut);
                    if (DocSurface.Rand(n, 3, seed) < doublePass)
                    {
                        float off = (DocSurface.Rand(n, 4, seed) - 0.5f) * 1.2f;
                        result.Add(new List<P2>
                        {
                            new P2(cut[0].X - uy * off + ux * 0.8f, cut[0].Y + ux * off + uy * 0.8f),
                            new P2(cut[2].X - uy * off - ux * 0.5f, cut[2].Y + ux * off - uy * 0.5f),
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>Ramer-Douglas-Peucker: tira os pontos que nao mudam a forma.</summary>
        public static List<P2> Simplify(List<P2> pts, float eps)
        {
            if (pts.Count < 3) return new List<P2>(pts);
            var keep = new bool[pts.Count];
            keep[0] = keep[pts.Count - 1] = true;
            var stack = new Stack<(int, int)>();
            stack.Push((0, pts.Count - 1));
            while (stack.Count > 0)
            {
                (int a, int b) = stack.Pop();
                float best = 0f;
                int idx = -1;
                for (int i = a + 1; i < b; i++)
                {
                    float d = SegDist(pts[i], pts[a], pts[b]);
                    if (d > best)
                    {
                        best = d;
                        idx = i;
                    }
                }

                if (best > eps && idx > 0)
                {
                    keep[idx] = true;
                    stack.Push((a, idx));
                    stack.Push((idx, b));
                }
            }

            var r = new List<P2>();
            for (int i = 0; i < pts.Count; i++)
                if (keep[i]) r.Add(pts[i]);
            return r;
        }

        private static float SegDist(P2 p, P2 a, P2 b)
        {
            float dx = b.X - a.X, dy = b.Y - a.Y, l2 = dx * dx + dy * dy;
            float t = l2 > 0f ? Math.Max(0f, Math.Min(1f, ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / l2)) : 0f;
            float qx = p.X - a.X - dx * t, qy = p.Y - a.Y - dy * t;
            return (float)Math.Sqrt(qx * qx + qy * qy);
        }

        /// <summary>Suaviza um traco (Chaikin), pra curvas de pena ficarem redondas.</summary>
        public static List<List<P2>> Smooth(List<List<P2>> strokes, int passes = 1)
        {
            var result = new List<List<P2>>();
            foreach (List<P2> s in strokes)
            {
                List<P2> cur = s;
                for (int p = 0; p < passes && cur.Count > 2; p++)
                {
                    var next = new List<P2> { cur[0] };
                    for (int i = 0; i + 1 < cur.Count; i++)
                    {
                        P2 a = cur[i], b = cur[i + 1];
                        next.Add(new P2(a.X * 0.75f + b.X * 0.25f, a.Y * 0.75f + b.Y * 0.25f));
                        next.Add(new P2(a.X * 0.25f + b.X * 0.75f, a.Y * 0.25f + b.Y * 0.75f));
                    }

                    next.Add(cur[cur.Count - 1]);
                    cur = next;
                }

                result.Add(cur);
            }

            return result;
        }

        // ------------------------------------------------------------------
        // Desenhos prontos
        // ------------------------------------------------------------------

        public static List<List<P2>> Circle(float cx, float cy, float r, int steps = 48)
        {
            var s = new List<P2>();
            for (int i = 0; i <= steps; i++)
            {
                double a = i * Math.PI * 2.0 / steps;
                s.Add(new P2(cx + (float)Math.Cos(a) * r, cy + (float)Math.Sin(a) * r));
            }

            return new List<List<P2>> { s };
        }

        public static List<List<P2>> Rect(float x0, float y0, float x1, float y1)
        {
            return new List<List<P2>>
            {
                new List<P2> { new P2(x0, y0), new P2(x1, y0), new P2(x1, y1), new P2(x0, y1), new P2(x0, y0) },
            };
        }

        public static List<List<P2>> Line(float x0, float y0, float x1, float y1)
        {
            return new List<List<P2>> { new List<P2> { new P2(x0, y0), new P2(x1, y1) } };
        }

        /// <summary>Uma coroa de tres pontas (o simbolo da Coroa), em tracos.</summary>
        public static List<List<P2>> Crown(float cx, float cy, float w)
        {
            float h = w * 0.62f, x0 = cx - w / 2, x1 = cx + w / 2, base0 = cy + h * 0.45f;
            var list = new List<List<P2>>
            {
                new List<P2>
                {
                    new P2(x0, base0), new P2(x0 - w * 0.04f, cy - h * 0.35f), new P2(cx - w * 0.25f, cy + h * 0.05f),
                    new P2(cx, cy - h * 0.55f), new P2(cx + w * 0.25f, cy + h * 0.05f), new P2(x1 + w * 0.04f, cy - h * 0.35f),
                    new P2(x1, base0), new P2(x0, base0),
                },
                new List<P2> { new P2(x0 + w * 0.02f, base0 - h * 0.18f), new P2(x1 - w * 0.02f, base0 - h * 0.18f) },
            };
            list.AddRange(Circle(cx, cy - h * 0.62f, w * 0.06f, 12));
            list.AddRange(Circle(x0 - w * 0.04f, cy - h * 0.43f, w * 0.05f, 12));
            list.AddRange(Circle(x1 + w * 0.04f, cy - h * 0.43f, w * 0.05f, 12));
            return list;
        }

        /// <summary>
        /// Um lacre de cera: a poca irregular com escorrido, o anel de borda e
        /// o emblema afundado pelo sinete.
        /// </summary>
        public static void Seal(DocSurface s, float cx, float cy, float r, C3 wax, List<List<P2>> emblem, int seed)
        {
            for (int y = (int)(cy - r * 1.5f); y <= cy + r * 1.5f; y++)
                for (int x = (int)(cx - r * 1.5f); x <= cx + r * 1.5f; x++)
                {
                    if (x < 0 || y < 0 || x >= s.W || y >= s.H) continue;
                    float ang = (float)Math.Atan2(y - cy, x - cx);
                    float blob = r * (1.08f + (DocSurface.Fbm(ang * 1.6f + 7f, 0.2f, seed, 3) - 0.5f) * 0.45f);
                    float d = (float)Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (d > blob) continue;
                    int i = x + y * s.W;
                    s.A[i] = 1f;
                    float t = d / blob;
                    float rim = Math.Abs(d - r * 0.78f) < r * 0.07f ? -0.8f : 0f;
                    s.Ht[i] = 3f + (1f - t * t) * r * 0.25f + rim + (t > 0.85f ? -(t - 0.85f) * 10f : 0f);
                    s.Col[i] = wax * (0.92f + DocSurface.Fbm(x * 0.15f, y * 0.15f, seed + 2, 2) * 0.16f);
                    s.Gloss[i] = 0.75f;
                }

            if (emblem != null) Engrave(s, emblem, 1.2f, 1.6f, wax * 0.7f, 0.5f, 0.9f);
        }

        /// <summary>Um furo de prego com ferrugem escorrida (papel pregado na parede).</summary>
        public static void NailHole(DocSurface s, float cx, float cy, C3 rust, int seed, bool withNail = true)
        {
            for (int k = 0; k < 18; k++)
            {
                float len = 4f + DocSurface.Rand(k, 1, seed) * 14f;
                float x = cx + (DocSurface.Rand(k, 2, seed) - 0.5f) * 3f;
                for (float t = 0; t < len; t += 0.6f)
                {
                    int px = (int)(x + Math.Sin(t * 0.3f + k) * 0.5f), py = (int)(cy + 2f + t);
                    if (!s.In(px, py)) continue;
                    int i = px + py * s.W;
                    s.Col[i] = C3.Lerp(s.Col[i], rust, 0.28f * (1f - t / len));
                }
            }

            if (withNail) s.Rivet(cx, cy, 3.2f, C3.Hex(0x5a5c66), 0.6f);
        }
    }
}
