using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O que sobrou largado pelos Ateliês quando o Mestre virou mercurio e os
    /// assistentes fugiram: cacos de vidraria no chao, pilhas de livros de
    /// formula, caixotes de reagente com palha, banquinhos de bancada e os
    /// garrafoes de vime. Nada de papel solto (papel e o que se le) nem de
    /// mercurio (mercurio machuca): o que nao se toca nao pode parecer com o
    /// que se usa.
    ///
    /// Tudo assenta na linha de baixo do desenho (o pivo e o meio da base,
    /// como o resto do LabArt). Pixel a pixel, y cresce pra baixo, sem
    /// UnityEngine.
    /// </summary>
    public static class LabClutterArt
    {
        public const string Shards = "lab_shards";
        public const string Books = "lab_books";
        public const string ReagentCrate = "lab_reagent_crate";
        public const string Stool = "lab_stool";
        public const string Carboy = "lab_carboy";

        public static readonly string[] Names = { Shards, Books, ReagentCrate, Stool, Carboy };

        public static (string Name, PixelCanvas Canvas)[] All() => new[]
        {
            (Shards, DrawShards()),
            (Books, DrawBooks()),
            (ReagentCrate, DrawReagentCrate()),
            (Stool, DrawStool()),
            (Carboy, DrawCarboy()),
        };

        private static readonly Rgba Ink = new Rgba(20, 18, 28);
        private static readonly Rgba[] Wood =
            { new Rgba(58, 38, 28), new Rgba(96, 64, 40), new Rgba(138, 96, 58), new Rgba(176, 132, 84) };
        private static readonly Rgba[] GlassRamp =
            { new Rgba(70, 96, 110), new Rgba(110, 146, 160), new Rgba(160, 200, 210), new Rgba(222, 242, 246) };
        private static readonly Rgba[] Straw =
            { new Rgba(120, 96, 40), new Rgba(168, 140, 62), new Rgba(206, 182, 96), new Rgba(236, 218, 140) };
        private static readonly Rgba[][] Covers =
        {
            new[] { new Rgba(60, 28, 34), new Rgba(110, 44, 50), new Rgba(150, 66, 70), new Rgba(190, 100, 96) },
            new[] { new Rgba(28, 40, 66), new Rgba(44, 64, 108), new Rgba(70, 94, 146), new Rgba(110, 134, 184) },
            new[] { new Rgba(34, 56, 40), new Rgba(52, 90, 62), new Rgba(80, 124, 88), new Rgba(120, 164, 124) },
        };

        /// <summary>Cacos de vidraria, 32x8: o gargalo de um frasco, lascas e o brilho delas.</summary>
        private static PixelCanvas DrawShards()
        {
            var c = new PixelCanvas(32, 8);
            var rng = new Random(971);
            for (int i = 0; i < 9; i++)
            {
                int x = 2 + rng.Next(27);
                int w = 2 + rng.Next(3);
                int h = 1 + rng.Next(3);
                for (int k = 0; k < w; k++)
                {
                    for (int j = 0; j < h - (k == 0 || k == w - 1 ? 1 : 0); j++) c.Set(x + k, 7 - j, GlassRamp[1 + (k + j) % 2]);
                }

                c.Set(x, 7 - h + 1, GlassRamp[3]);
            }

            // O gargalo inteiro, deitado.
            c.Fill(12, 4, 6, 3, GlassRamp[1]);
            c.Fill(18, 5, 3, 1, GlassRamp[2]);
            c.Set(13, 4, GlassRamp[3]);
            for (int px = 1; px < 31; px++)
            {
                if (c.Get(px, 6).A > 0 && c.Get(px, 7).A == 0) c.Set(px, 7, GlassRamp[0]);
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Pilha de livros de formula, 16x16: tres lombadas de cor, um aberto por cima, uma fita marcando.</summary>
        private static PixelCanvas DrawBooks()
        {
            var c = new PixelCanvas(16, 16);
            int y = 15;
            for (int b = 0; b < 3; b++)
            {
                int h = 3;
                int x0 = 1 + (b % 2);
                int x1 = 14 - (b == 1 ? 1 : 0);
                Rgba[] cover = Covers[b];
                for (int py = y - h + 1; py <= y; py++)
                {
                    for (int px = x0; px <= x1; px++)
                    {
                        bool pages = px > x0 + 1 && px < x1 && py == y - 1;
                        c.Set(px, py, pages ? new Rgba(222, 210, 180) : ArtKit.Tone(cover, 0.5f - (py - (y - h)) * 0.3f));
                    }
                }

                y -= h;
            }

            // O livro aberto em cima.
            for (int px = 3; px < 14; px++)
            {
                c.Set(px, y, px == 8 ? new Rgba(160, 146, 120) : new Rgba(232, 222, 196));
                c.Set(px, y - 1, px == 8 ? new Rgba(160, 146, 120) : new Rgba(214, 202, 174));
            }

            c.Set(5, y - 1, new Rgba(120, 110, 100));
            c.Set(10, y - 1, new Rgba(120, 110, 100));
            ArtKit.Line(c, 11, y + 1, 12, y + 4, new Rgba(170, 40, 40));
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Caixote de reagente, 16x16: tabuas, a palha saindo por cima e o gargalo de dois frascos.</summary>
        private static PixelCanvas DrawReagentCrate()
        {
            var c = new PixelCanvas(16, 16);

            // Os gargalos e a palha, por tras da borda.
            c.Fill(4, 2, 2, 4, GlassRamp[1]);
            c.Set(4, 2, GlassRamp[3]);
            c.Fill(10, 3, 2, 3, new Rgba(120, 60, 140));
            c.Fill(10, 2, 2, 1, Wood[2]);
            for (int px = 1; px < 15; px++)
            {
                int top = 5 - (int)(ArtKit.Noise(px * 0.9f, 1f, 981) * 2f);
                for (int py = top; py < 7; py++) c.Set(px, py, ArtKit.Tone(Straw, 0.6f - (py - top) * 0.4f));
            }

            for (int py = 6; py < 16; py++)
            {
                for (int px = 0; px < 16; px++)
                {
                    bool seam = (py - 6) % 5 == 4;
                    float grain = ArtKit.Noise(px * 0.5f, py * 2f, 982) - 0.5f;
                    c.Set(px, py, seam ? Wood[0] : ArtKit.Tone(Wood, 0.35f + grain * 0.5f - (px > 12 ? 0.6f : 0f)));
                }
            }

            // A etiqueta: um frasco desenhado a tinta.
            c.Fill(5, 9, 6, 4, new Rgba(214, 202, 170));
            c.Set(7, 10, Ink);
            c.Set(8, 10, Ink);
            c.Fill(6, 11, 4, 1, Ink);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Banquinho de bancada, 16x16: assento redondo e tres pernas abertas.</summary>
        private static PixelCanvas DrawStool()
        {
            var c = new PixelCanvas(16, 16);
            ArtKit.Line(c, 4, 6, 2, 15, Wood[1]);
            ArtKit.Line(c, 11, 6, 13, 15, Wood[1]);
            ArtKit.Line(c, 8, 6, 8, 15, Wood[0]);
            for (int px = 3; px < 13; px++) c.Set(px, 11, Wood[0]);
            ArtKit.Drum(c, 8f, 5f, 6f, 1.8f, 3f, ArtKit.Plain(Wood), 0.3f);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Garrafao de vime, 16x24: o vidro verde-escuro com resto de liquido, o vime trancado embaixo.</summary>
        private static PixelCanvas DrawCarboy()
        {
            var c = new PixelCanvas(16, 24);
            ArtKit.Ball(c, 8f, 15f, 7f, 8f, new[] { new Rgba(30, 60, 50), new Rgba(46, 90, 74), new Rgba(70, 130, 104), new Rgba(150, 200, 180) }, 0.2f);
            c.Fill(6, 2, 4, 6, new Rgba(46, 90, 74));
            c.Fill(5, 1, 6, 2, Wood[2]);
            c.Set(6, 4, new Rgba(150, 200, 180));

            // O vime, da metade pra baixo.
            for (int py = 15; py < 24; py++)
            {
                for (int px = 1; px < 15; px++)
                {
                    if (c.Get(px, py).A == 0 && py < 23) continue;
                    bool weave = (px + py) % 3 == 0;
                    c.Set(px, py, weave ? Straw[0] : ArtKit.Tone(Straw, 0.4f - (py - 15) * 0.1f));
                }
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }
    }
}
