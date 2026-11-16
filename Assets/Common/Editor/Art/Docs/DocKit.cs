using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Pecas prontas que os documentos do jogo dividem: as maos de quem
    /// escreve, as tintas, os papeis e os enfeites (timbre da Coroa, lacre,
    /// assinatura, impressao digital). Cada documento escolhe o que usa e
    /// arruma do seu jeito; nenhum e igual ao outro.
    /// </summary>
    public static class DocKit
    {
        // ------------------------------------------------------------------
        // Tintas e materiais
        // ------------------------------------------------------------------

        public static readonly C3 IronGall = C3.Hex(0x2a1c16);
        public static readonly C3 BlueBlack = C3.Hex(0x1e2440);
        public static readonly C3 FadedBrown = C3.Hex(0x5a3e2c);
        public static readonly C3 Charcoal = C3.Hex(0x26262a);
        public static readonly C3 Graphite = C3.Hex(0x4a4a54);
        public static readonly C3 StampRed = C3.Hex(0xa8241e);
        public static readonly C3 StampViolet = C3.Hex(0x4a2c6a);
        public static readonly C3 ChalkWhite = C3.Hex(0xe6e4d8);
        public static readonly C3 Soot = C3.Hex(0x141012);
        public static readonly C3 RustStain = C3.Hex(0x8a4a22);

        public static readonly C3 Cream = C3.Hex(0xe8dcbc);
        public static readonly C3 CreamDark = C3.Hex(0xa88e62);
        public static readonly C3 Manila = C3.Hex(0xd2b47e);
        public static readonly C3 ManilaDark = C3.Hex(0x8e6c3c);
        public static readonly C3 Vellum = C3.Hex(0xf0e8d4);
        public static readonly C3 VellumDark = C3.Hex(0xb8a682);
        public static readonly C3 Ledger = C3.Hex(0xdcd8c2);
        public static readonly C3 LedgerDark = C3.Hex(0x9a9478);

        public static readonly C3 Brass = C3.Hex(0xc49a48);
        public static readonly C3 BrassBright = C3.Hex(0xf2d690);
        public static readonly C3 BrassDark = C3.Hex(0x6e4c1c);
        public static readonly C3 Verdigris = C3.Hex(0x3e8a72);
        public static readonly C3 VerdigrisLight = C3.Hex(0x7cc4a4);
        public static readonly C3 Iron = C3.Hex(0x4c4a52);
        public static readonly C3 IronLight = C3.Hex(0x8a8892);
        public static readonly C3 Rust = C3.Hex(0x7a3c1c);
        public static readonly C3 RustLight = C3.Hex(0xb8662c);
        public static readonly C3 Silver = C3.Hex(0xc4cad6);
        public static readonly C3 SilverWax = C3.Hex(0x9aa0ae);
        public static readonly C3 RedWax = C3.Hex(0x8c1e1c);

        // ------------------------------------------------------------------
        // Maos
        // ------------------------------------------------------------------

        /// <summary>Tipografia de maquina, a da Coroa: reta, igual, sem tremor.</summary>
        public static Hand Print(float size, int seed = 1) => new Hand { Font = DocFont.RomanPrint, Size = size, Seed = seed, LineGap = 1.6f };

        /// <summary>Caligrafia de escrivao: cursiva formal, inclinada, de bico chato.</summary>
        public static Hand Clerk(float size, int seed = 1) => new Hand
        {
            Font = DocFont.ScriptFormal, Size = size, Seed = seed, Jitter = 0.12f, Wave = 0.4f, SizeJitter = 0.03f, LineGap = 1.25f,
        };

        /// <summary>Cursiva corrida de quem tem pressa: letra deitada, linha que ondula.</summary>
        public static Hand Hurried(float size, int seed = 1) => new Hand
        {
            Font = DocFont.ScriptQuick, Size = size, Seed = seed, Slant = 0.12f, Jitter = 0.35f, Wave = 1.1f, SizeJitter = 0.08f, LineGap = 1.3f,
        };

        /// <summary>Cursiva redonda, de quem escreve com cuidado (carta, diario).</summary>
        public static Hand Careful(float size, int seed = 1) => new Hand
        {
            Font = DocFont.ScriptRound, Size = size, Seed = seed, Jitter = 0.15f, Wave = 0.5f, SizeJitter = 0.04f, LineGap = 1.35f,
        };

        /// <summary>Letra de quem treme (frio, medo, Oxido).</summary>
        public static Hand Shaky(float size, int seed = 1) => new Hand
        {
            Font = DocFont.ScriptQuick, Size = size, Seed = seed, Slant = 0.05f, Jitter = 0.7f, Wave = 1.8f, SizeJitter = 0.14f, LineGap = 1.35f,
        };

        /// <summary>Maiusculas de quem risca com prego: pra virar sulco (ver DocInk.Scratchify).</summary>
        public static Hand Scratcher(float size, int seed = 1, float slant = 0f) => new Hand
        {
            Font = DocFont.RomanThin, Size = size, Seed = seed, Slant = slant, Jitter = 0.5f, Wave = 1.2f, SizeJitter = 0.12f,
            Tracking = 1.5f, LineGap = 1.5f,
        };

        /// <summary>Letra de fundicao, serifada, pra placas.</summary>
        public static Hand Cast(float size, int seed = 1) => new Hand { Font = DocFont.RomanCast, Size = size, Seed = seed, Tracking = 2f, LineGap = 1.5f };

        /// <summary>Gotica dos titulos da Coroa.</summary>
        public static Hand Blackletter(float size, int seed = 1) => new Hand { Font = DocFont.Gothic, Size = size, Seed = seed, LineGap = 1.4f };

        public static Hand Italic(float size, int seed = 1) => new Hand { Font = DocFont.ItalicPrint, Size = size, Seed = seed, LineGap = 1.5f };

        // ------------------------------------------------------------------
        // Penas
        // ------------------------------------------------------------------

        public static Pen Nib(float width, int seed = 1) => new Pen { Width = width, Nib = 40f, Contrast = 0.75f, Pressure = 0.25f, Seed = seed };
        public static Pen Ballpoint(float width, int seed = 1) => new Pen { Width = width, Contrast = 0.05f, Pressure = 0.15f, Seed = seed };
        public static Pen Type(float width, int seed = 1) => new Pen { Width = width, Contrast = 0.35f, Nib = 0f, Pressure = 0.1f, Opacity = 0.95f, Seed = seed };
        public static Pen Stick(float width, int seed = 1) => new Pen { Width = width, Contrast = 0.2f, Pressure = 0.45f, Opacity = 0.85f, Seed = seed };

        // ------------------------------------------------------------------
        // Enfeites
        // ------------------------------------------------------------------

        /// <summary>O emblema da Coroa: a coroa de tres pontas sobre o 925.</summary>
        public static List<List<P2>> CrownCrest(float cx, float cy, float size)
        {
            List<List<P2>> crest = DocInk.Crown(cx, cy - size * 0.18f, size * 0.62f);
            Hand h = Print(size * 0.24f, 3);
            h.Font = DocFont.RomanCast;
            crest.AddRange(h.Line("925", cx, cy + size * 0.36f, 0.5f));
            return crest;
        }

        /// <summary>Um filete com losango no meio, o enfeite dos papeis da Coroa.</summary>
        public static List<List<P2>> Rule(float x0, float x1, float y, float gem = 3f)
        {
            float cx = (x0 + x1) * 0.5f;
            return new List<List<P2>>
            {
                new List<P2> { new P2(x0, y), new P2(cx - gem * 2f, y) },
                new List<P2> { new P2(cx + gem * 2f, y), new P2(x1, y) },
                new List<P2> { new P2(cx - gem, y), new P2(cx, y - gem), new P2(cx + gem, y), new P2(cx, y + gem), new P2(cx - gem, y) },
            };
        }

        /// <summary>Uma assinatura: o nome em cursiva formal e um floreio por baixo que volta sobre si.</summary>
        public static List<List<P2>> Signature(string name, float x, float y, float size, int seed)
        {
            Hand h = Clerk(size, seed);
            h.Slant = 0.25f;
            h.SizeJitter = 0.1f;
            List<List<P2>> s = h.Line(name, x, y);
            float w = h.Measure(name);
            var flourish = new List<P2>();
            for (int i = 0; i <= 40; i++)
            {
                float t = i / 40f;
                float fx = x - size * 0.3f + t * (w + size * 1.2f);
                float fy = y + size * 0.35f + (float)Math.Sin(t * Math.PI * 2.2f) * size * 0.22f - t * size * 0.1f;
                flourish.Add(new P2(fx, fy));
            }

            s.Add(flourish);
            return s;
        }

        /// <summary>Uma impressao digital suja: aneis concentricos interrompidos.</summary>
        public static void Thumbprint(DocSurface s, float cx, float cy, float r, C3 color, float strength, int seed)
        {
            for (int y = (int)(cy - r * 1.4f); y <= cy + r * 1.4f; y++)
                for (int x = (int)(cx - r); x <= cx + r; x++)
                {
                    if (!s.In(x, y)) continue;
                    float dx = (x - cx) / r, dy = (y - cy) / (r * 1.35f);
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (d > 1f) continue;
                    float ridge = (float)Math.Sin(d * r * 1.9f + DocSurface.Fbm(x * 0.2f, y * 0.2f, seed, 2) * 3f);
                    if (ridge < 0.2f) continue;
                    float k = strength * (1f - d) * (DocSurface.Fbm(x * 0.15f, y * 0.15f, seed + 3, 2) > 0.4f ? 1f : 0.3f);
                    int i = x + y * s.W;
                    s.Col[i] = C3.Lerp(s.Col[i], color * s.Col[i], k);
                }
        }

        /// <summary>Uma meia-sola de bota marcada de lama (papel pisado no chao).</summary>
        public static void BootPrint(DocSurface s, float cx, float cy, float w, float angle, C3 mud, float strength, int seed)
        {
            float ca = (float)Math.Cos(angle), sa = (float)Math.Sin(angle);
            for (int y = (int)(cy - w * 1.6f); y <= cy + w * 1.6f; y++)
                for (int x = (int)(cx - w * 1.6f); x <= cx + w * 1.6f; x++)
                {
                    if (!s.In(x, y)) continue;
                    float u = ((x - cx) * ca + (y - cy) * sa) / w, v = (-(x - cx) * sa + (y - cy) * ca) / w;
                    if (u * u / 2.2f + v * v > 0.36f) continue;
                    bool tread = ((int)Math.Floor((u + 2f) * 5f) % 2 == 0) && Math.Abs(v) < 0.5f;
                    float n = DocSurface.Fbm(x * 0.1f, y * 0.1f, seed, 2);
                    if (!tread || n < 0.38f) continue;
                    int i = x + y * s.W;
                    s.Col[i] = C3.Lerp(s.Col[i], mud, strength * n);
                }
        }

        /// <summary>Um pedaco de fita adesiva velha, amarelada e translucida.</summary>
        public static void Tape(DocSurface s, float cx, float cy, float w, float h, float angle, int seed)
        {
            float ca = (float)Math.Cos(angle), sa = (float)Math.Sin(angle);
            int r = (int)(Math.Max(w, h) * 0.8f);
            for (int y = (int)cy - r; y <= cy + r; y++)
                for (int x = (int)cx - r; x <= cx + r; x++)
                {
                    if (x < 0 || y < 0 || x >= s.W || y >= s.H) continue;
                    float u = (x - cx) * ca + (y - cy) * sa, v = -(x - cx) * sa + (y - cy) * ca;
                    float jag = (DocSurface.Rand((int)v + 50, 1, seed) - 0.5f) * 2f;
                    if (Math.Abs(u) > w / 2 + jag || Math.Abs(v) > h / 2) continue;
                    int i = x + y * s.W;
                    if (s.A[i] <= 0f) s.Col[i] = C3.Hex(0x6a5a40);
                    s.A[i] = 1f;
                    s.Col[i] = C3.Lerp(s.Col[i], C3.Hex(0xd8c070), 0.32f);
                    s.Gloss[i] = 0.55f;
                    s.Ht[i] += 0.4f;
                }
        }

        /// <summary>Deixa so as linhas que passam por dentro do papel (o resto foi com o rasgo).</summary>
        public static List<List<P2>> Clip(DocSurface s, List<List<P2>> strokes)
        {
            var result = new List<List<P2>>();
            foreach (List<P2> stroke in strokes)
            {
                var cur = new List<P2>();
                foreach (P2 p in stroke)
                {
                    if (s.In((int)p.X, (int)p.Y)) cur.Add(p);
                    else if (cur.Count > 0)
                    {
                        result.Add(cur);
                        cur = new List<P2>();
                    }
                }

                if (cur.Count > 0) result.Add(cur);
            }

            return result;
        }

        /// <summary>Gira e desloca tracos em volta de um ponto.</summary>
        public static List<List<P2>> Rotate(List<List<P2>> strokes, float cx, float cy, float degrees)
        {
            double a = degrees * Math.PI / 180.0;
            float c = (float)Math.Cos(a), sn = (float)Math.Sin(a);
            var r = new List<List<P2>>();
            foreach (List<P2> s in strokes)
            {
                var t = new List<P2>();
                foreach (P2 p in s) t.Add(new P2(cx + (p.X - cx) * c - (p.Y - cy) * sn, cy + (p.X - cx) * sn + (p.Y - cy) * c));
                r.Add(t);
            }

            return r;
        }

        /// <summary>Pauta de caderno: linhas finas e a margem vermelha.</summary>
        public static void Ruled(DocSurface s, float x0, float x1, float y0, float y1, float gap, C3 line, float marginX, C3 margin, int seed)
        {
            var lines = new List<List<P2>>();
            for (float y = y0; y <= y1; y += gap) lines.AddRange(DocInk.Line(x0, y, x1, y));
            DocInk.Ink(s, lines, line, new Pen { Width = 0.8f, Contrast = 0f, Opacity = 0.55f, Pressure = 0.05f, Seed = seed });
            if (marginX > 0f)
            {
                var m = DocInk.Line(marginX, y0 - gap, marginX, y1 + gap);
                m.AddRange(DocInk.Line(marginX + 3f, y0 - gap, marginX + 3f, y1 + gap));
                DocInk.Ink(s, m, margin, new Pen { Width = 0.9f, Contrast = 0f, Opacity = 0.6f, Seed = seed + 1 });
            }
        }

        /// <summary>Tinta grossa de pincel: cobre, ganha um pouco de espessura e escorre pra baixo.</summary>
        public static void Paint(DocSurface s, List<List<P2>> strokes, C3 color, float width, int seed, float drips = 0.5f,
            float opacity = 0.95f)
        {
            var pen = new Pen { Width = width, Contrast = 0.25f, Nib = 90f, Pressure = 0.35f, Opacity = opacity, Dry = 0.12f, Seed = seed };
            float[] cov = DocInk.Coverage(s, strokes, pen);

            // Escorridos: das pontas de baixo de cada traco, fios descendo.
            var runs = new List<List<P2>>();
            int n = 0;
            foreach (List<P2> st in strokes)
                foreach (P2 p in st)
                {
                    n++;
                    if (DocSurface.Rand(n, 1, seed) > drips * 0.35f) continue;
                    float len = 6f + DocSurface.Rand(n, 2, seed) * 30f * drips;
                    var run = new List<P2>();
                    for (float t = 0; t <= len; t += 1.5f)
                        run.Add(new P2(p.X + (float)Math.Sin(t * 0.2f + n) * 0.4f, p.Y + t));
                    runs.Add(run);
                }

            float[] dcov = DocInk.Coverage(s, runs, new Pen { Width = width * 0.38f, Contrast = 0f, Pressure = 0.5f, Seed = seed + 3 });
            for (int i = 0; i < cov.Length; i++)
            {
                float c = Math.Max(cov[i], dcov[i]);
                if (c <= 0f || s.A[i] <= 0f) continue;
                int x = i % s.W, y = i / s.W;
                if (DocSurface.Fbm(x * 0.4f, y * 0.4f, seed + 9, 2) < pen.Dry && cov[i] < 0.99f) continue;
                float k = DocSurface.Clamp01(c * 1.4f) * opacity;
                s.Col[i] = C3.Lerp(s.Col[i], color * (0.92f + DocSurface.Fbm(x * 0.1f, y * 0.1f, seed + 4, 2) * 0.16f), k);
                s.Ht[i] += 0.5f * k;
                s.Gloss[i] = Math.Max(s.Gloss[i], 0.35f * k);
            }
        }

        /// <summary>Retícula de impressao: pontos que crescem com a tinta (cartaz litografado).</summary>
        public static void Halftone(DocSurface s, Func<int, int, float> amount, C3 color, float cell, float angle = 0.4f)
        {
            float ca = (float)Math.Cos(angle), sa = (float)Math.Sin(angle);
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                {
                    int i = x + y * s.W;
                    if (s.A[i] <= 0f) continue;
                    float k = amount(x, y);
                    if (k <= 0f) continue;
                    float u = (x * ca + y * sa) / cell, v = (-x * sa + y * ca) / cell;
                    float fu = u - (float)Math.Floor(u) - 0.5f, fv = v - (float)Math.Floor(v) - 0.5f;
                    float d = (float)Math.Sqrt(fu * fu + fv * fv);
                    float r = (float)Math.Sqrt(DocSurface.Clamp01(k)) * 0.62f;
                    if (d < r) s.Col[i] = C3.Lerp(s.Col[i], color, 0.92f);
                }
        }

        /// <summary>Cordao de solda: gotas de metal derretido, uma encavalada na outra.</summary>
        public static void Weld(DocSurface s, List<P2> path, float r, C3 color, int seed)
        {
            for (int k = 0; k + 1 < path.Count; k++)
            {
                P2 a = path[k], b = path[k + 1];
                float len = (float)Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));
                for (float t = 0; t < len; t += r * 0.55f)
                {
                    float f = t / len;
                    float rr = r * (0.8f + DocSurface.Rand(k * 100 + (int)t, 1, seed) * 0.4f);
                    s.Rivet(a.X + (b.X - a.X) * f, a.Y + (b.Y - a.Y) * f, rr, color * (0.85f + DocSurface.Rand((int)t, k, seed) * 0.3f), 0.5f);
                }
            }

            // A queimadura azulada em volta.
            foreach (P2 p in path) s.Spot(p.X, p.Y, r * 3.5f, C3.Hex(0x3a3460), 0.35f);
        }

        /// <summary>Uma letra feita de gotas de solda ou de pontos de puncao, ponto a ponto ao longo do traco.</summary>
        public static List<P2> Dotted(List<List<P2>> strokes, float step)
        {
            var dots = new List<P2>();
            foreach (List<P2> st in strokes)
            {
                float carry = 0f;
                for (int k = 0; k + 1 < st.Count; k++)
                {
                    P2 a = st[k], b = st[k + 1];
                    float len = (float)Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));
                    for (float t = carry; t < len; t += step) dots.Add(new P2(a.X + (b.X - a.X) * t / len, a.Y + (b.Y - a.Y) * t / len));
                    carry = Math.Max(0f, step - (len - carry) % step);
                }
            }

            return dots;
        }

        public static List<List<P2>> Join(params List<List<P2>>[] parts)
        {
            var r = new List<List<P2>>();
            foreach (List<List<P2>> p in parts) r.AddRange(p);
            return r;
        }
    }
}
