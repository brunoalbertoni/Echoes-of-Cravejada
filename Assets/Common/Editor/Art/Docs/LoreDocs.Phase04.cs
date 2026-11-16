using System;
using System.Collections.Generic;
using static EchoesOfCravejada.EditorTools.DocKit;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>Os documentos do District 4 (Ateliês Alquímicos), desenhados com o DocKit.</summary>
    public static partial class LoreArt
    {
        private static readonly C3 Mercury = C3.Hex(0xb8c0cc);

        /// <summary>Uma gota de mercurio: bolinha espelhada, quase sem molhar o papel.</summary>
        private static void MercuryBead(DocSurface s, float cx, float cy, float r)
        {
            for (int y = (int)(cy - r - 2); y <= cy + r + 2; y++)
                for (int x = (int)(cx - r - 2); x <= cx + r + 2; x++)
                {
                    if (!s.In(x, y)) continue;
                    float dx = x + 0.5f - cx, dy = (y + 0.5f - cy) * 1.15f;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    int i = x + y * s.W;
                    if (d <= r)
                    {
                        s.Col[i] = C3.Lerp(C3.Hex(0x50586a), Mercury, (1f - d / r) * 0.6f + 0.2f);
                        s.Ht[i] += 2f + (float)Math.Sqrt(Math.Max(0f, r * r - d * d)) * 1.2f;
                        s.Gloss[i] = 1.6f;
                    }
                    else if (d <= r + 2f) s.Col[i] = s.Col[i] * 0.8f;
                }
        }

        // ------------------------------------------------------------------
        // O diario de laboratorio
        // ------------------------------------------------------------------

        private static PixelCanvas LabPageDoc(int day, string[] paragraphs, int seed, int decay, string overwrite = null)
        {
            var s = new DocSurface(420, 520);
            s.Shape(DocSurface.Sheet(16, 14, 404, 506, seed, 1f, 1f, 1f, decay >= 2 ? 6f : 1f, corner: 4f));
            s.Paper(C3.Hex(0xe6e2d0), C3.Hex(0xa8a084), seed + 1, age: 0.3f + decay * 0.15f, fibres: 0.6f, foxing: 0.2f + decay * 0.2f);

            // Papel milimetrado de laboratorio: a grade fina e a grossa a cada cinco.
            var fine = new List<List<P2>>();
            var bold = new List<List<P2>>();
            for (int k = 0; k <= 76; k++)
            {
                float x = 24 + k * 5f, y = 24 + k * 6.3f;
                (k % 5 == 0 ? bold : fine).AddRange(DocInk.Line(x, 20, x, 500));
                if (y < 500) (k % 5 == 0 ? bold : fine).AddRange(DocInk.Line(20, y, 400, y));
            }

            DocInk.Ink(s, fine, C3.Hex(0x7ab0a0), new Pen { Width = 0.6f, Contrast = 0f, Opacity = 0.3f, Seed = seed + 2 });
            DocInk.Ink(s, bold, C3.Hex(0x5a9a88), new Pen { Width = 0.8f, Contrast = 0f, Opacity = 0.45f, Seed = seed + 3 });

            // O cabecalho do laboratorio, impresso.
            Hand head = Cast(6.5f, seed + 4);
            head.Tracking = 1.5f;
            var h = head.Line("CADERNO DE PROTOCOLO · ATELIÊS DE SUA MAJESTADE", 210, 40, 0.5f);
            DocInk.Ink(s, h, C3.Hex(0x2a4a40), Type(1f, seed + 5));

            // A letra, de capricho no comeco a desespero no fim.
            Hand hand = decay == 0 ? Clerk(16f, seed + 6) : decay == 1 ? Careful(15f, seed + 6) : decay == 2 ? Hurried(15f, seed + 6) : Shaky(15f, seed + 6);
            var ink = new List<List<P2>>();
            Hand dayHand = hand.Clone();
            dayHand.Size = 26f;
            ink.AddRange(dayHand.Line("Dia " + day + ".", 44, 86));
            float y0 = 128;
            foreach (string p in paragraphs)
            {
                y0 = hand.Block(p, 44, y0, 330, ink) + 10;
            }

            C3 inkColor = decay < 2 ? BlueBlack : IronGall;
            DocInk.Ink(s, ink, inkColor, Nib(1.6f, seed + 7));

            if (overwrite != null)
            {
                // A palavra raspada a canivete, e a nova escrita por cima, mais escura.
                float wx = 0f, wy = 0f;
                bool found = false;
                float yy = 128;
                foreach (string p in paragraphs)
                {
                    foreach (string line in hand.Wrap(p, 330f))
                    {
                        int idx = line.IndexOf(overwrite, StringComparison.Ordinal);
                        if (idx >= 0 && !found)
                        {
                            wx = 44 + hand.Measure(line.Substring(0, idx));
                            wy = yy;
                            found = true;
                        }

                        yy += hand.LineHeight;
                    }

                    yy += 10;
                }

                if (found)
                {
                    float ww = hand.Measure(overwrite);
                    for (int py = (int)wy - 18; py < wy + 6; py++)
                        for (int px = (int)wx - 4; px < wx + ww + 6; px++)
                        {
                            if (!s.In(px, py)) continue;
                            int i = px + py * s.W;
                            s.Col[i] = C3.Lerp(s.Col[i], C3.Hex(0xf2eee0), 0.7f) * (0.95f + DocSurface.Rand(px, py, seed) * 0.08f);
                            s.Ht[i] += DocSurface.Rand(px, py, seed + 1) * 0.8f - 0.3f;
                        }

                    var ghost = hand.Line("cobre", wx + 4, wy - 2);
                    DocInk.Ink(s, ghost, inkColor, new Pen { Width = 1.2f, Contrast = 0.4f, Opacity = 0.25f, Seed = seed + 8 });
                    DocInk.Ink(s, hand.Line(overwrite, wx, wy), C3.Hex(0x0e0c14), Nib(2.1f, seed + 9));
                }
            }

            // O estrago que cresce: gotas de mercurio, manchas, dedos prateados.
            if (decay >= 1) Thumbprint(s, 360, 470, 13f, C3.Hex(0x7a8090), 0.6f, seed + 10);
            for (int k = 0; k < decay * 4; k++)
            {
                float bx = 40 + DocSurface.Rand(k, 1, seed + 11) * 340f, by = 300 + DocSurface.Rand(k, 2, seed + 11) * 190f;
                MercuryBead(s, bx, by, 2.5f + DocSurface.Rand(k, 3, seed + 11) * 4f);
            }

            if (decay >= 2) s.Ring(300, 440, 46f, C3.Hex(0x707888), 0.35f, seed + 12);
            if (decay >= 3)
            {
                s.Tint((x, yy2) => (DocSurface.Fbm(x * 0.02f, yy2 * 0.02f, seed + 13, 3) - 0.6f) * 1.3f, C3.Hex(0x8a90a0));
                s.Burn(14f, seed + 14, (x, yy2) => yy2 > 420 ? 1f : 0f);
            }

            return s.Bake(relief: 1.2f);
        }

        // ------------------------------------------------------------------
        // As lousas
        // ------------------------------------------------------------------

        private static DocSurface Chalkboard(int w, int h, int seed)
        {
            var s = new DocSurface(w, h);
            s.Shape(DocSurface.Sheet(8, 8, w - 8, h - 8, seed, corner: 4f));
            s.Wood(C3.Hex(0x3a2414), C3.Hex(0x7a4e2a), seed + 1);
            var slate = new DocSurface(w, h);
            slate.Shape((x, y) => x > 30 && x < w - 30 && y > 30 && y < h - 46);
            slate.Slate(C3.Hex(0x22302a), seed + 2, 0.8f);
            for (int i = 0; i < s.A.Length; i++)
            {
                if (slate.A[i] <= 0f) continue;
                s.Col[i] = slate.Col[i];
                s.Ht[i] = -3f + slate.Ht[i];
                s.Gloss[i] = 0.04f;
            }

            // A canaleta do giz, com tocos e o apagador.
            for (int y = h - 44; y < h - 30; y++)
                for (int x = 30; x < w - 30; x++)
                {
                    int i = x + y * w;
                    s.Ht[i] = 4f - (y - (h - 44)) * 0.3f;
                    s.Col[i] = C3.Hex(0x5a3a20);
                }

            s.Rivet(w * 0.3f, h - 38, 4f, ChalkWhite, 0.1f);
            s.Rivet(w * 0.3f + 9, h - 38, 4f, ChalkWhite, 0.1f);
            for (int y = h - 48; y < h - 32; y++)
                for (int x = (int)(w * 0.7f); x < w * 0.7f + 50; x++)
                {
                    int i = x + y * w;
                    s.Col[i] = y < h - 42 ? C3.Hex(0x6a4a2a) : C3.Hex(0x9a9a92);
                    s.Ht[i] = 9f;
                }

            return s;
        }

        private static PixelCanvas ChalkMercuryDoc()
        {
            DocSurface s = Chalkboard(520, 380, 4501);

            // Contas pela lousa inteira: formulas, simbolos, uma curva.
            Hand math = Hurried(11f, 4502);
            var bg = new List<List<P2>>();
            string[] eq = { "Hg + Ag = ?", "3 dias (placa 4)", "densidade 13,5", "7 dest. = puro", "Óx. menor ??", "vol. > placa" };
            for (int k = 0; k < eq.Length; k++) bg.AddRange(math.Line(eq[k], 50 + (k % 2) * 160, 66 + (k / 2) * 30));
            var curve = new List<P2>();
            for (int k = 0; k <= 30; k++) curve.Add(new P2(380 + k * 3.5f, 170 - (float)Math.Log(1 + k) * 22f));
            bg.Add(curve);
            bg.AddRange(DocInk.Line(380, 176, 490, 176));
            bg.AddRange(DocInk.Line(380, 176, 380, 90));
            DocInk.Chalk(s, bg, ChalkWhite * 0.75f, Stick(1.5f, 4503), 0.1f);

            // No meio, sublinhado tres vezes.
            Hand main = Scratcher(17f, 4504, 0.02f);
            main.Font = DocFont.RomanPrint;
            var t = main.Line("PRATA + MERCÚRIO =", 70, 190);
            t.AddRange(main.Line("PRATA QUE NÃO APODRECE?", 70, 220));
            for (int k = 0; k < 3; k++) t.Add(new List<P2> { new P2(66, 232 + k * 6), new P2(200 + k * 20, 233 + k * 5), new P2(360, 231 + k * 6) });
            DocInk.Chalk(s, t, ChalkWhite, Stick(2.6f, 4505), 0.15f);

            // Embaixo, em outra letra.
            return s.Bake(relief: 1.1f);
        }

        private static PixelCanvas ChalkLightDoc()
        {
            DocSurface s = Chalkboard(520, 380, 4601);

            // O desenho: a lampada, o raio, o espelho inclinado, a curva da esquina.
            var d = new List<List<P2>>();
            d.AddRange(DocInk.Circle(90, 120, 22f, 20));
            d.AddRange(DocInk.Line(80, 142, 80, 156));
            d.AddRange(DocInk.Line(100, 142, 100, 156));
            d.AddRange(DocInk.Line(78, 156, 102, 156));
            for (int k = 0; k < 8; k++)
            {
                double a = k * Math.PI / 4;
                d.AddRange(DocInk.Line(90 + (float)Math.Cos(a) * 28f, 120 + (float)Math.Sin(a) * 28f, 90 + (float)Math.Cos(a) * 36f, 120 + (float)Math.Sin(a) * 36f));
            }

            d.Add(new List<P2> { new P2(126, 120), new P2(300, 120) });
            d.Add(new List<P2> { new P2(290, 112), new P2(302, 120), new P2(290, 128) });
            d.Add(new List<P2> { new P2(296, 92), new P2(346, 146) });
            d.Add(new List<P2> { new P2(300, 88), new P2(350, 142) });
            d.Add(new List<P2> { new P2(322, 124), new P2(322, 230) });
            d.Add(new List<P2> { new P2(314, 220), new P2(322, 232), new P2(330, 220) });
            d.AddRange(Careful(13f, 4603).Line("45°", 352, 112));

            // A seta de girar, em volta do espelho: meia volta com a ponta.
            var turn = new List<P2>();
            for (int k = 0; k <= 10; k++)
            {
                double a = Math.PI * (0.15 + 0.07 * k);
                turn.Add(new P2(321 + (float)Math.Cos(a) * 44f, 117 - (float)Math.Sin(a) * 44f));
            }

            d.Add(turn);
            P2 tip = turn[turn.Count - 1];
            d.Add(new List<P2> { new P2(tip.X - 2, tip.Y - 12), tip, new P2(tip.X + 11, tip.Y - 5) });

            // No fim do raio, o cristal: um losango com as facetas.
            d.Add(new List<P2> { new P2(322, 236), new P2(334, 248), new P2(322, 260), new P2(310, 248), new P2(322, 236) });
            d.Add(new List<P2> { new P2(310, 248), new P2(334, 248) });
            d.Add(new List<P2> { new P2(322, 236), new P2(322, 260) });
            d.AddRange(Careful(13f, 4602).Line("cristal", 344, 254));
            DocInk.Chalk(s, d, ChalkWhite * 0.9f, Stick(2f, 4604), 0.12f);

            // Em letra de aula, redonda e paciente.
            Hand aula = Careful(19f, 4605);
            var t = aula.Line("O espelho não cria a luz.", 40, 282);
            t.AddRange(aula.Line("Só escolhe para onde ela vai.", 40, 306));
            DocInk.Chalk(s, t, ChalkWhite, Stick(2.3f, 4606), 0.1f);

            // E, menor, o lembrete do assistente.
            var n = Careful(14f, 4607).Line("Gire. Siga o raio. O cristal abre.", 44, 326);
            DocInk.Chalk(s, n, ChalkWhite * 0.8f, Stick(1.8f, 4608), 0.14f);
            return s.Bake(relief: 1.1f);
        }

        // ------------------------------------------------------------------
        // As placas de prata
        // ------------------------------------------------------------------

        private static PixelCanvas SilverPlaqueDoc(string[] lines, string nail, int seed)
        {
            int h = 120 + lines.Length * 26 + (nail != null ? 40 : 0);
            var s = new DocSurface(460, h);
            s.Shape((x, y) =>
            {
                if (x < 12 || x > 448 || y < 12 || y > h - 12) return false;
                // Cantos recortados em arco, de ourives.
                float cx = Math.Min(x - 12, 448 - x), cy = Math.Min(y - 12, h - 12 - y);
                if (cx < 18 && cy < 18) return (18 - cx) * (18 - cx) + (18 - cy) * (18 - cy) > 18 * 18 - 1 ? false : true;
                return true;
            });
            s.Metal(DocKit.Silver, seed, 0f, bevel: 7f, gloss: 1.1f, wear: 0.25f);

            // Moldura de folhagem gravada: filete duplo e voltas nas pontas.
            C3 groove = C3.Hex(0x3a3e4c);
            var frame = DocInk.Rect(34, 34, 426, h - 34);
            frame.AddRange(DocInk.Rect(40, 40, 420, h - 40));
            for (int k = 0; k < 4; k++)
            {
                float cx = k % 2 == 0 ? 52 : 408, cy = k < 2 ? 52 : h - 52;
                frame.AddRange(DocInk.Circle(cx, cy, 7f, 14));
                frame.AddRange(DocInk.Circle(cx, cy, 3f, 8));
            }

            DocInk.Engrave(s, frame, 0.8f, 1.2f, groove, 0.8f);

            Hand title = Cast(13f, seed + 1);
            title.Tracking = 3f;
            Hand body = Italic(13f, seed + 2);
            var t = new List<List<P2>>();
            float y = 82;
            bool second = false;
            foreach (string line in lines)
            {
                if (line.Length == 0)
                {
                    t.AddRange(Rule(150, 310, y - 10, 3f));
                    y += 12;
                    second = true;
                    continue;
                }

                t.AddRange((second ? body : title).Line(line, 230, y, 0.5f));
                y += 26;
            }

            DocInk.Engrave(s, t, 0.95f, 1.6f, groove, 0.9f);

            if (nail != null) NailLine(s, Hurried(15f, seed + 3), nail, 130, h - 52, C3.Hex(0xffffff), seed + 4, 2.4f);
            s.Tint((x, yy) => (DocSurface.Fbm(x * 0.04f, yy * 0.04f, seed + 5, 3) - 0.66f) * 1.6f, C3.Hex(0x4a3a48));
            return s.Bake(relief: 1.1f);
        }

        // ------------------------------------------------------------------
        // Os retratos da galeria
        // ------------------------------------------------------------------

        private static PixelCanvas PortraitDoc(int kind, int scratch, string plate, int seed)
        {
            const int w = 360, h = 480;
            var s = new DocSurface(w, h);
            s.Shape((x, y) => x >= 6 && x < w - 6 && y >= 6 && y < h - 6);

            // A tela: fundo escuro de veludo, pincelado.
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int i = x + y * w;
                    float brush = DocSurface.Fbm(x * 0.02f + y * 0.05f, y * 0.02f - x * 0.03f, seed, 4);
                    float glow = Math.Max(0f, 1f - ((x - 180f) * (x - 180f) + (y - 170f) * (y - 170f)) / (160f * 160f));
                    s.Col[i] = C3.Lerp(C3.Hex(0x14121c), C3.Hex(0x3e3654), glow * 0.8f + (brush - 0.5f) * 0.4f);
                    s.Ht[i] = brush * 1.5f;
                    s.Gloss[i] = 0.25f;
                }

            // Pinta um volume: cor e altura de pincelada, sombreado como esfera.
            void Blob(Func<float, float, float> inside, C3 dark, C3 light, float height, float gloss)
            {
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        float k = inside(x, y);
                        if (k <= 0f) continue;
                        int i = x + y * w;
                        float stroke = DocSurface.Fbm(x * 0.05f, y * 0.12f, seed + 7, 3);
                        s.Col[i] = C3.Lerp(dark, light, DocSurface.Clamp01(k * 0.8f + (stroke - 0.5f) * 0.4f));
                        s.Ht[i] = 3f + k * height + stroke * 1.2f;
                        s.Gloss[i] = gloss;
                    }
            }

            float Ell(float x, float y, float cx, float cy, float rx, float ry)
            {
                float dx = (x - cx) / rx, dy = (y - cy) / ry;
                float d = dx * dx + dy * dy;
                return d < 1f ? (float)Math.Sqrt(1f - d) : 0f;
            }

            C3 clothDark = kind == 1 ? C3.Hex(0x3a0c1c) : kind == 3 ? C3.Hex(0xb8b4a8) : C3.Hex(0x161a34);
            C3 clothLight = kind == 1 ? C3.Hex(0x9a2a44) : kind == 3 ? C3.Hex(0xf0ece0) : C3.Hex(0x4a5490);
            if (kind == 4)
            {
                clothDark = C3.Hex(0x2a0c3a);
                clothLight = C3.Hex(0x7a3aa0);
            }

            if (kind == 2)
            {
                clothDark = C3.Hex(0x1a2a24);
                clothLight = C3.Hex(0x4a7a64);
            }

            // Ombros largos de casaca, com lapelas e botoes.
            Blob((x, y) =>
            {
                if (y < 330) return 0f;
                float half = 70f + (y - 330) * 1.1f;
                float t = 1f - Math.Abs(x - 180) / Math.Max(1f, half);
                return t > 0f ? (float)Math.Sqrt(t) * Math.Min(1f, (y - 326) / 30f) : 0f;
            }, clothDark, clothLight, 18f, 0.2f);
            DocInk.Ink(s, new List<List<P2>>
            {
                new List<P2> { new P2(150, 334), new P2(170, 400), new P2(180, 480) },
                new List<P2> { new P2(210, 334), new P2(190, 400), new P2(180, 480) },
            }, clothDark * 0.6f, new Pen { Width = 3f, Contrast = 0f, Opacity = 0.8f, Seed = seed + 20 });
            for (int b = 0; b < 3; b++) s.Rivet(180, 420 + b * 20, 4f, Brass, 1f);

            // Pescoco de aneis e cabeca de prata polida, de queixo marcado.
            C3 sd = C3.Hex(0x4a4e5c), sl = C3.Hex(0xf4f6fa);
            Blob((x, y) => y > 270 && y < 340 && Math.Abs(x - 180) < 24 ? 1f - Math.Abs(x - 180) / 30f : 0f, sd, sl, 10f, 1f);
            DocInk.Engrave(s, new List<List<P2>>
            {
                new List<P2> { new P2(158, 292), new P2(202, 292) },
                new List<P2> { new P2(158, 310), new P2(202, 310) },
            }, 1f, 2f, sd * 0.6f, 0.6f);
            Blob((x, y) => Math.Max(Ell(x, y, 180, 190, 60, 64), Ell(x, y, 180, 236, 44, 40)), sd, sl, 40f, 1.1f);
            s.Rivet(120, 200, 7f, sd * 1.3f, 1f, true, 0.8f);
            s.Rivet(240, 200, 7f, sd * 1.3f, 1f, true, 2.1f);
            var face = new List<List<P2>>
            {
                new List<P2> { new P2(140, 172), new P2(160, 166), new P2(176, 172) },
                new List<P2> { new P2(184, 172), new P2(200, 166), new P2(220, 172) },
                new List<P2> { new P2(180, 130), new P2(180, 160) },
                new List<P2> { new P2(146, 236), new P2(160, 262), new P2(200, 262), new P2(214, 236) },
            };
            if (kind != 3) for (int g = 0; g < 3; g++) face.Add(new List<P2> { new P2(166, 236 + g * 6), new P2(194, 236 + g * 6) });
            DocInk.Engrave(s, face, 1.1f, 1.8f, sd * 0.5f, 0.6f);

            switch (kind)
            {
                case 0:
                    // Gola alta engomada e bigode de prata.
                    Blob((x, y) => y > 270 && y < 360 ? Math.Max(Ell(x, y, 132, 330, 44, 70), Ell(x, y, 228, 330, 44, 70)) : 0f, C3.Hex(0xc8c4b8), C3.Hex(0xffffff), 10f, 0.3f);
                    Blob((x, y) => Ell(x, y, 180, 232, 30, 7), sd * 0.6f, sd, 6f, 0.9f);
                    break;
                case 1:
                    // Coque alto e colar de contas.
                    Blob((x, y) => Ell(x, y, 180, 112, 40, 30), sd, sl, 18f, 1f);
                    for (int k = 0; k < 13; k++)
                    {
                        double a = Math.PI * (0.15 + k * 0.7 / 12);
                        s.Rivet(180 + (float)Math.Cos(a) * 60f, 300 + (float)Math.Sin(a) * 44f, 5f, C3.Hex(0xe8e4f0), 1f);
                    }

                    break;
                case 2:
                    // Crisol jovem: prata lisa e o monoculo de lente de quartzo.
                    for (int y = 0; y < h; y++)
                        for (int x = 0; x < w; x++)
                        {
                            float dx = x - 205, dy = y - 190;
                            float d = (float)Math.Sqrt(dx * dx + dy * dy);
                            int i = x + y * w;
                            if (d < 18 && d > 14) { s.Col[i] = Brass; s.Ht[i] += 6f; s.Gloss[i] = 1f; }
                            else if (d <= 14) { s.Col[i] = C3.Lerp(s.Col[i], C3.Hex(0xd8e8ff), 0.45f); s.Gloss[i] = 1.4f; }
                        }

                    DocInk.Ink(s, new List<List<P2>> { new List<P2> { new P2(222, 196), new P2(240, 260), new P2(236, 330) } }, Brass, Ballpoint(1.4f, seed + 8));
                    break;
                case 4:
                    // O Rei jovem: gola de arminho e a coroa de quartzo acesa.
                    Blob((x, y) => y > 320 && y < 360 && Math.Abs(x - 180) < 120 ? 1f - Math.Abs(y - 340) / 24f : 0f, C3.Hex(0xc8c4b8), C3.Hex(0xffffff), 8f, 0.2f);
                    for (int k = 0; k < 14; k++) s.Rivet(70 + k * 16, 340 + (k % 2) * 6, 2.2f, C3.Hex(0x1a1a1e), 0.2f);
                    for (int k = -4; k <= 4; k++) QuartzSpike(s, 180 + k * 13, 146, 7f, 46f - Math.Abs(k) * 6f, seed + 30 + k);
                    for (int y = 138; y < 152; y++)
                        for (int x = 122; x < 238; x++)
                        {
                            int i = x + y * w;
                            s.Col[i] = Brass;
                            s.Ht[i] = 44f;
                            s.Gloss[i] = 1f;
                        }

                    for (int y = 0; y < 130; y++)
                        for (int x = 80; x < 280; x++)
                        {
                            float d = (float)Math.Sqrt((x - 180) * (x - 180) + (y - 100) * (y - 100));
                            if (d < 90f && s.Ht[x + y * w] < 10f) s.Col[x + y * w] = C3.Lerp(s.Col[x + y * w], C3.Hex(0xfff0c0), (1f - d / 90f) * 0.45f);
                        }

                    // A manchinha verde no canto, como uma assinatura.
                    Paint(s, DocInk.Circle(320, 440, 4f, 10), C3.Hex(0x3ab070), 4f, seed + 40, 0.2f);
                    break;
                default:
                    // Jaleco e mascara de bico de alquimista.
                    Blob((x, y) =>
                    {
                        if (x < 180 || x > 300 || y < 196 || y > 250) return 0f;
                        float t = (x - 180) / 120f;
                        float half = 26f * (1f - t);
                        return Math.Abs(y - 220 - t * 14f) < half ? 1f - t * 0.6f : 0f;
                    }, C3.Hex(0x3a2a1a), C3.Hex(0x8a6a44), 14f, 0.4f);
                    foreach (int ex in new[] { 156, 204 }) s.Rivet(ex, 186, 13f, C3.Hex(0x2a3a3a), 1.2f);
                    break;
            }

            // Os olhos: fendas escuras com uma brasa fria.
            if (kind != 3)
                foreach (int ex in new[] { 156, 204 })
                    for (int y = 186; y < 196; y++)
                        for (int x = ex - 12; x < ex + 12; x++)
                        {
                            int i = x + y * w;
                            s.Col[i] = Math.Abs(x - ex) < 3 && y > 188 && y < 194 ? C3.Hex(0xa8d0ff) : C3.Hex(0x101018);
                            s.Ht[i] -= 4f;
                        }

            // O risco em X verde. Forte demais: rasga a tela. De leve: quase carinho.
            if (scratch > 0)
            {
                var x1 = new List<List<P2>>
                {
                    new List<P2> { new P2(110, 110), new P2(180, 200), new P2(250, 300) },
                    new List<P2> { new P2(256, 112), new P2(180, 205), new P2(104, 296) },
                };
                if (scratch == 2)
                {
                    DocInk.Carve(s, DocInk.Scratchify(x1, seed + 9, 2f, 6f, 0.8f), 3.5f, 9f, C3.Hex(0x0a080c), 1f, seed + 10, chips: 1f, burr: 0.4f);
                    Paint(s, x1, C3.Hex(0x3aa86a), 5f, seed + 11, 0.4f);
                }
                else if (scratch == 1)
                {
                    Paint(s, x1, C3.Hex(0x3aa86a), 6f, seed + 12, 0.6f);
                }
                else
                {
                    var soft = new List<List<P2>>
                    {
                        new List<P2> { new P2(150, 160), new P2(180, 200), new P2(210, 240) },
                        new List<P2> { new P2(212, 162), new P2(180, 202), new P2(148, 238) },
                    };
                    DocInk.Ink(s, soft, C3.Hex(0x3aa86a), new Pen { Width = 2f, Contrast = 0f, Opacity = 0.55f, Dry = 0.3f, Seed = seed + 13 });
                }
            }

            // Craquele do verniz velho por cima da tela.
            for (int k = 0; k < 220; k++)
            {
                float x = DocSurface.Rand(k, 1, seed + 14) * w, y = DocSurface.Rand(k, 2, seed + 14) * (h - 90);
                float a = DocSurface.Rand(k, 3, seed + 14) * 6.28f;
                for (int t = 0; t < 7; t++)
                {
                    int px = (int)(x + Math.Cos(a) * t), py = (int)(y + Math.Sin(a) * t);
                    if (s.In(px, py)) s.Col[px + py * w] = s.Col[px + py * w] * 0.85f;
                }
            }

            PixelCanvas painting = s.Bake(relief: 0.9f, shadow: false, outline: false);

            // A moldura de prata, larga, com contas e rosetas, e a plaqueta de nome.
            int fw = w + 80, fh = h + 120;
            var f = new DocSurface(fw, fh);
            f.Shape((x, y) => x >= 8 && x < fw - 8 && y >= 8 && y < fh - 8);
            f.Metal(kind == 4 ? Brass : DocKit.Silver * 0.85f, seed + 15, 0f, bevel: 4f, gloss: 1f, wear: 0.4f);
            for (int y = 0; y < fh; y++)
                for (int x = 0; x < fw; x++)
                {
                    int i = x + y * fw;
                    if (f.A[i] <= 0f) continue;
                    float e = Math.Min(Math.Min(x - 8, fw - 9 - x), Math.Min(y - 8, fh - 9 - y));
                    f.Ht[i] = e < 12 ? e * 0.8f : e < 18 ? 10f - (e - 12) * 0.6f : e < 26 ? 6f + (float)Math.Sin((e - 18) * 0.4f) * 3f : 4f;
                    f.Col[i] = f.Col[i] * (e > 26 ? 0.7f : 1f);
                }

            for (int x = 30; x < fw - 30; x += 14)
            {
                f.Rivet(x, 22, 3.2f, DocKit.Silver, 1f);
                f.Rivet(x, fh - 22, 3.2f, DocKit.Silver, 1f);
            }

            for (int y = 36; y < fh - 30; y += 14)
            {
                f.Rivet(22, y, 3.2f, DocKit.Silver, 1f);
                f.Rivet(fw - 22, y, 3.2f, DocKit.Silver, 1f);
            }

            foreach ((int rx, int ry) in new[] { (24, 24), (fw - 24, 24), (24, fh - 24), (fw - 24, fh - 24) })
                f.Rivet(rx, ry, 10f, DocKit.Silver * 1.05f, 1.2f);

            // A plaqueta de latao embaixo; a do Crisol foi arrancada pela metade.
            int py0 = 40 + h + 8;
            for (int y = py0; y < py0 + 40; y++)
                for (int x = 90; x < fw - 90; x++)
                {
                    if (kind == 2 && x > fw / 2 + 110 + DocSurface.Rand(0, y, seed) * 8) continue;
                    int i = x + y * fw;
                    f.Col[i] = Brass;
                    f.Ht[i] = 12f + Math.Min(Math.Min(y - py0, py0 + 40 - y), 3);
                    f.Gloss[i] = 0.9f;
                }

            Hand ph = Cast(kind == 2 ? 8.5f : plate.Contains("|") ? 8f : 11f, seed + 16);
            ph.Tracking = kind == 2 ? 1f : 2f;
            string[] plateLines = plate.Split('|');
            for (int pl = 0; pl < plateLines.Length; pl++)
            {
                float ly = plateLines.Length == 1 ? py0 + 26 : py0 + 17 + pl * 15;
                DocInk.Engrave(f, ph.Line(plateLines[pl], kind == 2 ? 100f : fw / 2f, ly, kind == 2 ? 0f : 0.5f), 0.9f, 1.4f, C3.Hex(0x2a1c08), 0.9f);
            }
            if (kind == 2)
            {
                for (int y = py0; y < py0 + 40; y++)
                    for (int x = fw / 2 + 110; x < fw - 90; x++)
                    {
                        int i = x + y * fw;
                        if (f.Col[i].R > 0.6f && x > fw / 2 + 118) continue;
                        f.Ht[i] = Math.Min(f.Ht[i], 8f);
                    }

                f.Dent(fw - 100, py0 + 8, 4f, 2f);
                f.Dent(fw - 100, py0 + 32, 4f, 2f);
            }

            PixelCanvas frame = f.Bake(relief: 1f);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    Rgba p = painting.Pixels[x + y * w];
                    if (p.A == 0) continue;
                    frame.Pixels[(x + 40) + (y + 40) * fw] = p;
                }

            return frame;
        }

        // ------------------------------------------------------------------
        // O quadro de cortica e a prancha do prototipo
        // ------------------------------------------------------------------

        private static PixelCanvas PrototypeBoardDoc()
        {
            var s = new DocSurface(560, 420);
            s.Shape(DocSurface.Sheet(8, 8, 552, 412, 4701, corner: 4f));
            s.Wood(C3.Hex(0x3a2414), C3.Hex(0x7a4e2a), 4702);
            for (int y = 26; y < 394; y++)
                for (int x = 26; x < 534; x++)
                {
                    int i = x + y * s.W;
                    float n = DocSurface.Rand(x / 2, y / 2, 4703);
                    s.Col[i] = C3.Lerp(C3.Hex(0x8a5a32), C3.Hex(0xc89a62), n * 0.7f + DocSurface.Fbm(x * 0.05f, y * 0.05f, 4704, 2) * 0.3f);
                    s.Ht[i] = -3f + n * 1.2f;
                    s.Gloss[i] = 0.02f;
                }

            string[][] cards =
            {
                new[] { "N.º 1", "Ferro fundido.", "Pesado demais", "para andar." },
                new[] { "N.º 2", "Latão.", "O Óxido pegou", "em três dias." },
                new[] { "N.º 3", "Cobre." },
                new[] { "N.º 4", "Vidro. Quebrou", "ao se ver no", "espelho." },
                new[] { "N.º 5", "Prata nobre.", "Apodreceu antes", "de acordar." },
                new[] { "N.º 6", "Quartzo lapidado.", "Perfeito. Não", "aguentou o", "primeiro passo." },
            };
            C3[] pins = { C3.Hex(0xc83a3a), C3.Hex(0x3a6ac8), C3.Hex(0xc8a03a), C3.Hex(0x3aa86a), C3.Hex(0x9a3ac8), C3.Hex(0xc8c8c8) };

            for (int k = 0; k < 6; k++)
            {
                float cx = 50 + (k % 4) * 124 + (k >= 4 ? 62 : 0), cy = 44 + (k / 4) * 170;
                float tilt = (DocSurface.Rand(k, 1, 4705) - 0.5f) * 0.12f;
                var card = new DocSurface(s.W, s.H);
                Func<int, int, bool> rect = DocSurface.Sheet((int)cx, (int)cy, (int)cx + 112, (int)cy + 150, 4706 + k, 0.6f, 0.6f, 0.6f, 0.6f);
                card.Shape((x, y) =>
                {
                    float u = cx + 56 + (x - cx - 56) * (float)Math.Cos(tilt) + (y - cy - 75) * (float)Math.Sin(tilt);
                    float v = cy + 75 - (x - cx - 56) * (float)Math.Sin(tilt) + (y - cy - 75) * (float)Math.Cos(tilt);
                    return rect((int)u, (int)v);
                });
                card.Paper(C3.Hex(0xf0ead6), C3.Hex(0xb0a280), 4710 + k, age: 0.3f, fibres: 0.4f);
                var lines = new List<List<P2>>();
                for (int r = 0; r < 6; r++) lines.AddRange(DocInk.Line(cx + 6, cy + 44 + r * 18, cx + 106, cy + 44 + r * 18));
                lines.AddRange(DocInk.Line(cx + 6, cy + 30, cx + 106, cy + 30));
                DocInk.Ink(card, DocKit.Rotate(lines, cx + 56, cy + 75, -tilt * 57.3f), C3.Hex(0x7a9ac8), new Pen { Width = 0.7f, Contrast = 0f, Opacity = 0.55f, Seed = 4720 + k });

                Hand hand = Careful(9.5f, 4730 + k);
                var t = new List<List<P2>>();
                Hand num = Cast(10f, 4740 + k);
                t.AddRange(num.Line(cards[k][0], cx + 10, cy + 26));
                for (int r = 1; r < cards[k].Length; r++) t.AddRange(hand.Line(cards[k][r], cx + 8, cy + 26 + r * 18));
                DocInk.Ink(card, DocKit.Rotate(t, cx + 56, cy + 75, -tilt * 57.3f), BlueBlack, Nib(1.1f, 4750 + k));

                if (k == 2)
                {
                    // Cobre: a ficha diz o que ele era, na mesma letra das outras.
                    var v = hand.Line("Cobaia", cx + 8, cy + 62);
                    v.AddRange(hand.Line("forçada.", cx + 8, cy + 80));
                    DocInk.Ink(card, DocKit.Rotate(v, cx + 56, cy + 75, -tilt * 57.3f), BlueBlack, Nib(1.1f, 4762));
                }

                for (int i = 0; i < s.A.Length; i++)
                {
                    if (card.A[i] <= 0f) continue;
                    s.Col[i] = card.Col[i];
                    s.Ht[i] = 2f + card.Ht[i];
                    s.Gloss[i] = 0.02f;
                }

                s.Rivet(cx + 56, cy + 8, 5.5f, pins[k], 1.1f);
            }

            // O setimo alfinete, sem ficha: so um cantinho de papel rasgado preso nele.
            float sx = 50 + 2 * 124 + 62 + 124 - 40, sy = 44 + 170 + 60;
            var scrap = new DocSurface(s.W, s.H);
            scrap.Shape(DocSurface.Sheet((int)sx + 44, (int)sy - 6, (int)sx + 70, (int)sy + 10, 4770, 3f, 0f, 3f, 4f));
            scrap.Paper(C3.Hex(0xf0ead6), C3.Hex(0xb0a280), 4771);
            for (int i = 0; i < s.A.Length; i++)
            {
                if (scrap.A[i] <= 0f) continue;
                s.Col[i] = scrap.Col[i];
                s.Ht[i] = 2f;
            }

            s.Rivet(sx + 56, sy, 5.5f, C3.Hex(0x2a2a2a), 1.1f);
            return s.Bake(relief: 1.1f);
        }

        private static PixelCanvas PrototypeSheetDoc()
        {
            var s = new DocSurface(500, 600);

            // A prancheta, e a folha de desenho presa no prendedor.
            s.Shape(DocSurface.Sheet(10, 10, 490, 590, 4801, corner: 12f));
            s.Wood(C3.Hex(0x6a4626), C3.Hex(0xa87a48), 4802, vertical: true);
            var paper = new DocSurface(s.W, s.H);
            paper.Shape(DocSurface.Sheet(36, 40, 464, 566, 4803, 0.5f, 0.5f, 0.5f, 0.5f));
            paper.Paper(C3.Hex(0xeee8d6), C3.Hex(0xb0a688), 4804, age: 0.35f, fibres: 0.5f);

            // Carimbo da folha: moldura de desenho tecnico e o bloco do titulo.
            C3 pen = C3.Hex(0x1c2a4a);
            var frame = DocInk.Rect(52, 70, 448, 550);
            frame.AddRange(DocInk.Rect(260, 470, 448, 550));
            frame.AddRange(DocInk.Line(260, 496, 448, 496));
            frame.AddRange(DocInk.Line(260, 522, 448, 522));
            Hand label = Cast(6.5f, 4805);
            frame.AddRange(label.Line("ATELIÊ DE PROTÓTIPOS · FL. 7", 268, 488));
            frame.AddRange(label.Line("ESCALA 1:2 · COTAS EM MM", 268, 514));
            Hand title = Cast(8f, 4806);
            title.Tracking = 1f;
            frame.AddRange(title.Line("PROTÓTIPO BRUTO N.º 7", 268, 541));
            DocInk.Ink(paper, frame, pen, Type(1.1f, 4807));

            // O desenho: cabeca de quartzo bruto (facetas irregulares), dois olhos, a mola.
            var d = new List<List<P2>>();
            d.Add(new List<P2> { new P2(170, 130), new P2(240, 104), new P2(318, 122), new P2(346, 190), new P2(330, 262), new P2(260, 290), new P2(186, 276), new P2(150, 210), new P2(170, 130) });
            d.Add(new List<P2> { new P2(240, 104), new P2(228, 170), new P2(150, 210) });
            d.Add(new List<P2> { new P2(228, 170), new P2(346, 190) });
            d.Add(new List<P2> { new P2(228, 170), new P2(260, 290) });
            d.AddRange(DocInk.Circle(212, 214, 14f, 20));
            d.AddRange(DocInk.Circle(282, 214, 14f, 20));
            d.AddRange(DocInk.Circle(212, 214, 5f, 10));
            d.AddRange(DocInk.Circle(282, 214, 5f, 10));
            var spring = new List<P2>();
            for (int k = 0; k <= 80; k++) spring.Add(new P2(250 + (float)Math.Sin(k * 0.6f) * 22f, 296 + k * 1.6f));
            d.Add(spring);
            d.AddRange(DocInk.Line(206, 426, 294, 426));
            DocInk.Ink(paper, d, pen, new Pen { Width = 1.5f, Contrast = 0.1f, Opacity = 0.95f, Seed = 4808 });

            // Cotas: linhas de chamada, setas, numeros.
            var dims = new List<List<P2>>();
            void Dim(float x0, float y0, float x1, float y1, string text, float tx, float ty)
            {
                dims.AddRange(DocInk.Line(x0, y0, x1, y1));
                float dx = x1 - x0, dy = y1 - y0, len = (float)Math.Sqrt(dx * dx + dy * dy);
                float ux = dx / len * 6f, uy = dy / len * 6f;
                dims.Add(new List<P2> { new P2(x0 + ux - uy * 0.5f, y0 + uy + ux * 0.5f), new P2(x0, y0), new P2(x0 + ux + uy * 0.5f, y0 + uy - ux * 0.5f) });
                dims.Add(new List<P2> { new P2(x1 - ux - uy * 0.5f, y1 - uy + ux * 0.5f), new P2(x1, y1), new P2(x1 - ux + uy * 0.5f, y1 - uy - ux * 0.5f) });
                dims.AddRange(Careful(10f, (int)(x0 + y0)).Line(text, tx, ty));
            }

            Dim(150, 316, 346, 316, "196", 236, 312);
            Dim(380, 104, 380, 290, "186", 386, 204);
            Dim(108, 296, 108, 426, "mola 128", 60, 370);
            dims.AddRange(DocInk.Line(346, 190, 380, 190));
            dims.AddRange(DocInk.Line(150, 300, 150, 320));
            DocInk.Ink(paper, dims, pen, new Pen { Width = 0.9f, Contrast = 0f, Opacity = 0.8f, Seed = 4809 });

            Hand notes = Careful(12.5f, 4810);
            var n = notes.Line("não lapidar!", 352, 140);
            n.AddRange(notes.Line("imune ao Óxido", 60, 470));
            n.AddRange(notes.Line("funcional!", 60, 492));
            DocInk.Ink(paper, n, pen, Nib(1.2f, 4811));

            // Por cima de tudo, em vermelho; e embaixo, a mao do Rei.
            Hand stamp = Cast(20f, 4812);
            stamp.Tracking = 3f;
            var st = stamp.Line("REJEITADO", 250, 380, 0.5f);
            Hand stamp2 = Cast(13f, 4813);
            stamp2.Tracking = 4f;
            st.AddRange(stamp2.Line("— IMPERFEITO —", 250, 406, 0.5f));
            st.AddRange(DocInk.Rect(120, 350, 380, 420));
            st.AddRange(DocInk.Rect(124, 354, 376, 416));
            DocInk.Stamp(paper, Rotate(st, 250, 385, -14f), DocKit.StampRed, Type(2.4f, 4814), 0.3f);
            Hand field = Print(9f, 4815);
            DocInk.Ink(paper, field.Line("DESTINO: POÇO DE SUCATA", 60, 530), C3.Hex(0x2a3a5a), Type(1.1f, 4816));

            for (int i = 0; i < s.A.Length; i++)
            {
                if (paper.A[i] <= 0f) continue;
                s.Col[i] = paper.Col[i];
                s.Ht[i] = 3f + paper.Ht[i];
                s.Gloss[i] = 0.02f;
            }

            // O prendedor de metal no topo.
            for (int y = 18; y < 62; y++)
                for (int x = 170; x < 330; x++)
                {
                    int i = x + y * s.W;
                    float e = Math.Min(Math.Min(x - 170, 329 - x), Math.Min(y - 18, 61 - y));
                    s.Col[i] = IronLight * 1.1f;
                    s.Ht[i] = 8f + Math.Min(e, 4f) + (y < 40 ? 4f : 0f);
                    s.Gloss[i] = 1f;
                }

            s.Rivet(196, 30, 4f, IronLight, 1f);
            s.Rivet(304, 30, 4f, IronLight, 1f);
            return s.Bake(relief: 1.1f);
        }

        // ------------------------------------------------------------------
        // A ordem de descarte
        // ------------------------------------------------------------------

        private static PixelCanvas DiscardOrderDoc()
        {
            var s = new DocSurface(440, 520);
            s.Shape(DocSurface.Sheet(16, 14, 424, 506, 4901, 1f, 1f, 1f, 1f));
            s.Paper(Vellum * 1.02f, VellumDark, 4902, age: 0.3f, fibres: 0.6f, foxing: 0.2f);

            // Borda de filigrana prateada impressa.
            var border = DocInk.Rect(34, 32, 406, 488);
            border.AddRange(DocInk.Rect(40, 38, 400, 482));
            for (float x = 50; x < 396; x += 12) border.AddRange(DocInk.Circle(x, 35, 1.6f, 6));
            for (float x = 50; x < 396; x += 12) border.AddRange(DocInk.Circle(x, 485, 1.6f, 6));
            DocInk.Ink(s, border, C3.Hex(0x8a90a4), new Pen { Width = 1f, Contrast = 0f, Opacity = 0.9f, Seed = 4903 });

            var head = new List<List<P2>>();
            head.AddRange(CrownCrest(220, 82, 50f));
            head.AddRange(Blackletter(20f, 4904).Line("Decreto de Sua Majestade", 220, 148, 0.5f));
            Hand caps = Cast(10f, 4905);
            caps.Tracking = 3f;
            head.AddRange(caps.Line("SOBRE O PROTÓTIPO BRUTO", 220, 172, 0.5f));
            head.AddRange(Rule(90, 350, 186, 3f));
            DocInk.Ink(s, head, IronGall, Type(1.4f, 4906));

            Hand body = Print(10.5f, 4907);
            body.Font = DocFont.RomanCast;
            var t = new List<List<P2>>();
            float y = body.Block("O protótipo bruto foi apresentado a este Tribunal. É imune ao Óxido. Isso não muda nada.", 60, 224, 320, t) + 10;
            y = body.Block("Cravejada está apodrecendo, distrito por distrito. Que apodreça. Prefiro uma cidade podre a uma cidade salva por sucata.", 60, y, 320, t) + 10;
            y = body.Block("Destino de todo o lote: Poço de Sucata. Que não se fale mais nisso.", 60, y, 320, t);
            DocInk.Ink(s, t, IronGall, Type(1.2f, 4908));

            // A assinatura firme, numa letra que nao treme, e o lacre.
            Hand king = Clerk(24f, 4909);
            king.Jitter = 0f;
            king.Wave = 0f;
            DocInk.Ink(s, Signature("Argentum Rex", 140, y + 40, 22f, 4910), C3.Hex(0x5a6070), Nib(2.2f, 4911));
            DocInk.Seal(s, 362, y + 26, 24f, SilverWax, CrownCrest(362, y + 26, 28f), 4912);

            Hand stamp = Cast(26f, 4913);
            stamp.Tracking = 6f;
            var st = stamp.Line("DESCARTE", 220, y + 92, 0.5f);
            st.AddRange(DocInk.Rect(90, y + 58, 350, y + 104));
            DocInk.Stamp(s, Rotate(st, 220, y + 82, 6f), DocKit.StampRed, Type(2.4f, 4914), 0.3f);

            DocInk.NailHole(s, 220, 26, Rust, 4915);
            return s.Bake(relief: 1.2f);
        }
    }
}
