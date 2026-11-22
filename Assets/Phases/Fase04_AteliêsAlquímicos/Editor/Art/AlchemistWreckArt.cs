using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O que sobra do Alquimista do Mercurio depois que ele escorre: uma poca
    /// larga de mercurio, o jaleco amarrotado no meio dela, a mascara de bico
    /// caida de lado (uma lente estilhacada, a outra apagada), a mangueira
    /// ainda presa no tanque de latao tombado, que vaza por uma rachadura, e
    /// um frasco quebrado. O vapor de mercurio sobe fino e a poca brilha.
    ///
    /// Mesmo traco e escala do AlchemistArt (32 px por tile, as mesmas pecas:
    /// capuz, bico rebitado, lentes de latao, tanque com cintas). Folha de
    /// FrameCount quadros em loop, mesmo chao do Alquimista.
    /// </summary>
    public static class AlchemistWreckArt
    {
        public const int Width = 160;
        public const int Height = 64;
        public const int PixelsPerUnit = 32;
        public const int FrameCount = 8;
        public const float PivotX = 0.5f;
        public const float PivotY = 4f / Height;

        private const float Ground = Height - 4f;

        private static readonly Rgba[] Mercury =
            { new Rgba(46, 52, 72), new Rgba(98, 108, 134), new Rgba(170, 182, 206), new Rgba(246, 250, 255) };

        private static readonly Rgba[] Coat =
            { new Rgba(118, 122, 120), new Rgba(170, 174, 168), new Rgba(210, 212, 202), new Rgba(240, 240, 230) };

        private static readonly Rgba[] DeadGlass =
            { new Rgba(30, 34, 40), new Rgba(44, 50, 58), new Rgba(70, 76, 86), new Rgba(120, 126, 136) };

        private static readonly Rgba Glass = new Rgba(190, 226, 240, 170);
        private static readonly Rgba GlassEdge = new Rgba(236, 250, 255);
        private static readonly Rgba Ink = ArtKit.Outline;

        public static PixelCanvas[] Frames()
        {
            var c = new PixelCanvas(Width, Height);

            // A poca: larga, chata, com a borda irregular.
            for (int y = (int)Ground - 7; y <= (int)Ground + 2; y++)
            {
                for (int x = 8; x < Width - 8; x++)
                {
                    float dx = (x + 0.5f - 80f) / 62f;
                    float dy = (y + 0.5f - (Ground - 1.5f)) / 4.5f;
                    float wobble = (ArtKit.Noise(x * 0.12f, 3f, 911) - 0.5f) * 0.5f;
                    if (dx * dx + dy * dy > 1f + wobble) continue;

                    float light = -0.7f * dy + 0.1f;
                    if (ArtKit.Noise(x * 0.2f, y * 0.6f, 912) > 0.78f) light += 0.6f;
                    c.Set(x, y, ArtKit.Tone(Mercury, light));
                }
            }

            // As contas de mercurio soltas em volta.
            foreach ((float bx, float r) in new[] { (12f, 2f), (20f, 1.4f), (142f, 2.2f), (150f, 1.5f), (134f, 1.2f), (30f, 1.2f) })
            {
                ArtKit.Ball(c, bx, Ground - 1.5f, r, r * 0.8f, ArtKit.Plain(Mercury), 0.3f);
            }

            DrawCoat(c);
            DrawTank(c, out (float X, float Y) leak);
            DrawHose(c);
            DrawMask(c);
            DrawFlask(c);

            ArtKit.AddOutline(c);

            // O filete que escorre da rachadura do tanque ate a poca.
            for (int y = (int)leak.Y; y < Ground - 2; y++) c.Set((int)leak.X + (y % 3 == 0 ? 1 : 0), y, y % 2 == 0 ? Mercury[3] : Mercury[2]);

            var vapor = new Rgba(214, 220, 238);
            var vaporDark = new Rgba(150, 156, 184);
            var vents = new[]
            {
                new WreckKit.Vent(leak.X, leak.Y - 2f, 34f, 3.5f, vapor, vaporDark),
                new WreckKit.Vent(132f, Ground - 12f, 30f, 3f, vapor, vaporDark),
                new WreckKit.Vent(80f, Ground - 6f, 40f, 4.5f, vapor, vaporDark),
            };

            PixelCanvas[] frames = WreckKit.Animate(c, vents, null, FrameCount, 61, 4);

            // O brilho correndo pela poca, e o tremor das contas.
            for (int f = 0; f < frames.Length; f++)
            {
                for (int g = 0; g < 3; g++)
                {
                    int gx = 22 + (g * 41 + f * 7) % 116;
                    int gy = (int)Ground - 2 - (g % 2);
                    if (frames[f].Get(gx, gy).A > 200 && frames[f].Get(gx, gy).B > 120)
                    {
                        frames[f].Set(gx, gy, new Rgba(255, 255, 255));
                        frames[f].Set(gx + 1, gy, Mercury[3]);
                        frames[f].Set(gx - 1, gy, Mercury[3]);
                    }
                }
            }

            return frames;
        }

        /// <summary>O jaleco largado, amarrotado em dobras no meio da poca.</summary>
        private static void DrawCoat(PixelCanvas c)
        {
            ArtKit.Polygon(c, new[] { (46f, Ground - 3f), (58f, Ground - 12f), (74f, Ground - 15f), (92f, Ground - 11f), (104f, Ground - 4f), (100f, Ground - 1f), (52f, Ground - 1f) },
                (x, y) => ArtKit.Tone(Coat, 0.5f - (x - 46f) / 70f - (y - (Ground - 15f)) / 30f));

            // As dobras: linhas de sombra em leque e a barra rasgada.
            foreach ((int x0, int x1) in new[] { (60, 66), (70, 74), (82, 86), (92, 94) })
            {
                ArtKit.Line(c, x0, (int)Ground - 11, x1, (int)Ground - 2, Coat[0]);
                ArtKit.Line(c, x0 + 1, (int)Ground - 11, x1 + 1, (int)Ground - 2, Coat[3]);
            }

            for (int x = 54; x < 100; x += 4) c.Set(x + 2, (int)Ground - 1, Coat[0]);

            // Os botoes e as manchas de mercurio no pano.
            ArtKit.Rivet(c, 66, (int)Ground - 8, ArtKit.Brass);
            ArtKit.Rivet(c, 78, (int)Ground - 10, ArtKit.Brass);
            c.Fill(84, (int)Ground - 7, 3, 2, Mercury[1]);
            c.Fill(56, (int)Ground - 5, 2, 2, Mercury[1]);
        }

        /// <summary>O tanque respirador, tombado: latao com cintas, a tampa de cobre e a rachadura que vaza.</summary>
        private static void DrawTank(PixelCanvas c, out (float X, float Y) leak)
        {
            float x0 = 22f, x1 = 50f, y = Ground - 8f;
            ArtKit.Limb(c, x0, y, x1, y - 2f, 6.5f, ArtKit.Corroded(ArtKit.Brass, 0.14f, 6101, 0.3f), 0.1f);
            for (int x = 27; x < 49; x += 6) ArtKit.Line(c, x, (int)y - 7, x + 1, (int)y + 4, ArtKit.Brass[0]);

            ArtKit.Ball(c, x1 + 4f, y - 2.5f, 3.2f, 3.2f, ArtKit.Plain(ArtKit.Copper), 0.2f);
            ArtKit.Ball(c, 36f, y - 3f, 2.6f, 2.6f, ArtKit.Plain(Mercury), 0.3f);
            c.Set(36, (int)y - 3, Ink);

            // A rachadura, com as bordas viradas pra fora.
            leak = (42f, y + 2f);
            ArtKit.Line(c, 39, (int)y - 4, 43, (int)y + 2, Ink);
            ArtKit.Line(c, 40, (int)y - 4, 44, (int)y + 2, ArtKit.Brass[3]);
            c.Set(42, (int)y + 1, Mercury[3]);
        }

        /// <summary>A mangueira de borracha, do tanque ate o queixo da mascara, caida em curva.</summary>
        private static void DrawHose(PixelCanvas c)
        {
            for (int i = 0; i <= 18; i++)
            {
                float t = i / 18f;
                float px = 56f + 30f * t;
                float py = Ground - 9f - (float)Math.Sin(t * Math.PI) * 9f + t * 2f;
                ArtKit.Ball(c, px, py, 1.8f, 1.8f, ArtKit.Plain(ArtKit.Rubber), 0.2f);
                if (i % 3 == 0) c.Set((int)px, (int)py - 1, ArtKit.Rubber[3]);
            }
        }

        /// <summary>A mascara de bico, de lado: o capuz de couro, o bico rebitado pro chao, uma lente quebrada.</summary>
        private static void DrawMask(PixelCanvas c)
        {
            float hx = 94f, hy = Ground - 13f;

            ArtKit.Ball(c, hx - 2f, hy, 11f, 10f, ArtKit.Plain(ArtKit.Leather), -0.1f);
            ArtKit.Ball(c, hx - 1f, hy - 1f, 9f, 8.5f, ArtKit.Plain(ArtKit.Leather), 0.1f);

            // Um rasgo no capuz, com o forro aparecendo.
            ArtKit.Line(c, (int)hx - 9, (int)hy - 3, (int)hx - 5, (int)hy + 2, ArtKit.Leather[0]);
            c.Set((int)hx - 7, (int)hy - 1, ArtKit.Cloth[2]);

            // O bico, apontando pro chao a direita, com a costura e os rebites.
            const float angle = 22f;
            (float tipX, float tipY) = ArtKit.Along(hx + 5f, hy + 3f, angle, 26f);
            ArtKit.Taper(c, hx + 5f, hy + 3f, tipX, tipY, 6f, 1.2f, ArtKit.Plain(ArtKit.Leather), 0.2f);
            ArtKit.Line(c, (int)hx + 6, (int)hy + 4, (int)tipX, (int)tipY, ArtKit.Leather[0]);
            for (int i = 1; i <= 3; i++)
            {
                (float rx, float ry) = ArtKit.Along(hx + 5f, hy + 3f, angle, i * 6.5f);
                ArtKit.Rivet(c, (int)rx, (int)ry - 2, ArtKit.Brass);
            }

            // A ponta do bico lascada.
            c.Set((int)tipX, (int)tipY, Rgba.Clear);
            c.Set((int)tipX - 1, (int)tipY + 1, ArtKit.Leather[0]);

            // As lentes: a de tras apagada, a da frente estilhacada (aro vazio, cacos).
            for (int eye = 0; eye < 2; eye++)
            {
                float ex = hx - 2f + eye * 8f;
                float ey = hy - 2f;
                ArtKit.Ball(c, ex, ey, 4f, 4f, ArtKit.Plain(ArtKit.Brass), 0.2f);

                if (eye == 0)
                {
                    ArtKit.Ball(c, ex, ey, 2.7f, 2.7f, ArtKit.Plain(DeadGlass), 0.2f);
                    c.Set((int)ex - 1, (int)ey - 1, DeadGlass[3]);
                }
                else
                {
                    ArtKit.Ball(c, ex, ey, 2.7f, 2.7f, ArtKit.Plain(new[] { Ink, Ink, new Rgba(30, 24, 30), new Rgba(30, 24, 30) }));
                    c.Set((int)ex - 2, (int)ey - 1, GlassEdge);
                    c.Set((int)ex + 1, (int)ey + 1, Glass);
                }
            }

            // Os cacos da lente no chao, embaixo.
            foreach ((int x, int y) in new[] { (100, 0), (104, 1), (97, 1), (108, 0) })
            {
                c.Set(x, (int)Ground - 2 - y, GlassEdge);
                c.Set(x + 1, (int)Ground - 2 - y, Glass);
            }
        }

        /// <summary>Um balao de vidro do cinto, quebrado: o fundo redondo e a boca serrilhada, com resto de mercurio.</summary>
        private static void DrawFlask(PixelCanvas c)
        {
            float fx = 132f, fy = Ground - 6f;

            for (int y = (int)(fy - 7f); y <= (int)(fy + 5f); y++)
            {
                for (int x = (int)(fx - 7f); x <= (int)(fx + 7f); x++)
                {
                    float dx = (x + 0.5f - fx) / 6.5f;
                    float dy = (y + 0.5f - fy) / 5.5f;
                    float d = dx * dx + dy * dy;
                    if (d > 1f) continue;

                    // A boca quebrada: o alto some num serrilhado.
                    int jag = (int)(fy - 2f) + ((x * 7) % 4) - 1;
                    if (y < jag) continue;

                    bool rim = d > 0.72f || y == jag;
                    c.Blend(x, y, rim ? GlassEdge : (y > fy + 1f ? Mercury[2] : Glass));
                }
            }

            c.Set((int)fx - 3, (int)fy - 1, new Rgba(255, 255, 255));

            // Cacos em volta: triangulinhos de vidro.
            foreach ((float x, float lean) in new[] { (120f, -30f), (144f, 20f), (148f, -10f) })
            {
                (float tx, float ty) = ArtKit.Along(x, Ground - 1f, -90f + lean, 4f);
                ArtKit.Polygon(c, new[] { (x - 1.5f, Ground - 1f), (tx, ty), (x + 1.5f, Ground - 1f) }, (px, py) => px < x ? GlassEdge : Glass);
            }
        }
    }
}
