using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O luxo do corredor do palacio: colunas de marmore com capitel dourado,
    /// janeloes com a Cidade de Prata ao luar e cortinas de veludo, quadros em
    /// moldura de ouro, vasos, lustres de cristal, arandelas, o lambri de
    /// nogueira com filete de ouro, o friso dourado do teto, bustos dos
    /// Nobres e bancos de veludo.
    ///
    /// Tudo a 16 px por tile, como o resto do palacio (PalaceArt). Nao depende
    /// de UnityEngine.
    /// </summary>
    public static class PalaceDecorArt
    {
        public const int PortraitCount = 3;

        private static readonly Rgba[] Marble = LabArt.Marble;
        private static readonly Rgba[] Velvet = LabArt.Velvet;
        private static readonly Rgba[] Walnut = LabArt.Walnut;
        private static readonly Rgba[] Silver = LabArt.Silver;
        private static readonly Rgba[] Gold = ArtKit.Brass;

        private static readonly Rgba[] Crystal =
            { new Rgba(150, 170, 206), new Rgba(200, 220, 244), new Rgba(236, 244, 255), new Rgba(255, 255, 255) };

        private static readonly Rgba[] Night =
            { new Rgba(14, 16, 34), new Rgba(26, 30, 60), new Rgba(44, 52, 96), new Rgba(70, 84, 140) };

        private static readonly Rgba Flame = new Rgba(255, 220, 140);
        private static readonly Rgba FlameCore = new Rgba(255, 250, 220);

        /// <summary>O nome, o desenho, se e pendurado e se e emendavel (Tiled).</summary>
        public static IEnumerable<(string Name, Func<PixelCanvas> Draw, bool Hanging, bool Tiled)> Sprites()
        {
            yield return ("palace_column", Column, false, false);
            yield return ("palace_window", Window, false, false);
            for (int i = 0; i < PortraitCount; i++)
            {
                int kind = i;
                yield return ("palace_portrait_" + i, () => Portrait(kind), false, false);
            }

            yield return ("palace_vase", Vase, false, false);
            yield return ("palace_chandelier", Chandelier, true, false);
            yield return ("palace_sconce", Sconce, false, false);
            yield return ("palace_wainscot", Wainscot, false, true);
            yield return ("palace_frieze", Frieze, false, true);
            yield return ("palace_bust", Bust, false, false);
            yield return ("palace_bench", Bench, false, false);
        }

        /// <summary>Uma coluna de marmore canelada, base e capitel de ouro, 24x112 (7 tiles).</summary>
        public static PixelCanvas Column()
        {
            const int w = 24;
            const int h = 112;
            var c = new PixelCanvas(w, h);

            for (int y = 12; y < h - 10; y++)
            {
                for (int x = 4; x < w - 4; x++)
                {
                    float u = (x - 4) / (float)(w - 9);
                    bool flute = (x - 4) % 4 == 3;
                    float light = 0.75f - u * 1.4f - (flute ? 0.45f : 0f);
                    Rgba color = ArtKit.Tone(Marble, light);
                    if (ArtKit.Noise(x * 0.3f, y * 0.08f, 9401) > 0.82f) color = Marble[0];
                    c.Set(x, y, color);
                }
            }

            // O capitel: volutas douradas e o abaco.
            c.Fill(1, 0, w - 2, 4, Gold[2]);
            c.Fill(1, 3, w - 2, 1, Gold[0]);
            for (int y = 4; y < 12; y++)
            {
                int inset = (y - 4) / 3;
                for (int x = 2 + inset; x < w - 2 - inset; x++) c.Set(x, y, ArtKit.Tone(Gold, 0.6f - (x / (float)w) * 1.2f));
            }

            ArtKit.Ball(c, 4f, 7f, 3f, 3f, Gold, 0.3f);
            ArtKit.Ball(c, w - 5f, 7f, 3f, 3f, Gold, 0.1f);
            c.Set(4, 7, Gold[0]);
            c.Set(w - 5, 7, Gold[0]);

            // A base: dois aneis de ouro e o plinto de marmore.
            c.Fill(2, h - 10, w - 4, 3, Gold[2]);
            c.Fill(2, h - 8, w - 4, 1, Gold[0]);
            c.Fill(1, h - 7, w - 2, 7, Marble[1]);
            c.Fill(1, h - 7, w - 2, 1, Marble[3]);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>
        /// Um janelao em arco, 40x88: a Cidade de Prata la fora, ao luar (as
        /// torres, as janelinhas acesas, a lua), a cruz do caixilho de ouro e
        /// as cortinas de veludo presas com cordao dourado.
        /// </summary>
        public static PixelCanvas Window()
        {
            const int w = 40;
            const int h = 88;
            var c = new PixelCanvas(w, h);
            float cx = w / 2f;

            // O ceu e a cidade, dentro do arco.
            for (int y = 4; y < h - 6; y++)
            {
                for (int x = 6; x < w - 6; x++)
                {
                    float dx = (x + 0.5f - cx) / 14f;
                    if (y < 18 && dx * dx + (18 - y) * (18 - y) / 196f > 1f) continue;

                    float light = (y - 4) / 80f * 1.6f - 0.8f;
                    Rgba color = ArtKit.Tone(Night, light);
                    if (ArtKit.Hash(x, y, 9411) % 61 == 0 && y < 50) color = new Rgba(220, 226, 255);
                    c.Set(x, y, color);
                }
            }

            // A lua.
            ArtKit.Ball(c, 26f, 20f, 4f, 4f, new[] { new Rgba(180, 186, 210), new Rgba(220, 224, 240), new Rgba(245, 246, 252), new Rgba(255, 255, 255) }, 0.4f);

            // As torres da cidade, prata escura, com janelinhas acesas.
            int[] tops = { 54, 46, 58, 40, 52, 60, 48 };
            for (int i = 0; i < tops.Length; i++)
            {
                int x0 = 6 + i * 4;
                for (int y = tops[i]; y < h - 6; y++)
                {
                    for (int x = x0; x < x0 + 4 && x < w - 6; x++)
                    {
                        bool lit = (x + y * 3 + i) % 7 == 0 && y > tops[i] + 2;
                        c.Set(x, y, lit ? new Rgba(255, 236, 170) : ArtKit.Tone(Silver, -0.6f + (x - x0) * 0.15f));
                    }
                }

                c.Set(x0 + 1, tops[i] - 1, Silver[2]);
                c.Set(x0 + 1, tops[i] - 2, Silver[1]);
            }

            // O caixilho de ouro: o arco, a cruz.
            for (int y = 4; y < h - 6; y++)
            {
                c.Set((int)cx, y, Gold[2]);
                c.Set((int)cx - 1, y, Gold[1]);
            }

            for (int x = 6; x < w - 6; x++) c.Set(x, 44, Gold[2]);

            // A moldura de marmore e o peitoril.
            for (int a = 0; a <= 40; a++)
            {
                double t = Math.PI + a * Math.PI / 40.0;
                for (int r = 14; r <= 16; r++)
                {
                    c.Set((int)Math.Round(cx + Math.Cos(t) * r), (int)Math.Round(18 + Math.Sin(t) * r), r == 14 ? Gold[2] : Marble[2]);
                }
            }

            c.Fill(4, h - 6, w - 8, 3, Marble[3]);
            c.Fill(4, h - 3, w - 8, 3, Marble[1]);

            // As cortinas de veludo, caindo dos lados, presas no meio.
            for (int side = -1; side <= 1; side += 2)
            {
                for (int y = 2; y < h - 2; y++)
                {
                    float tie = Math.Abs(y - 50f) < 3f ? 0.5f : 1f;
                    float width = (y < 50 ? 4f + (50 - y) * 0.08f : 4f + (y - 50) * 0.12f) * tie;
                    for (int k = 0; k < (int)width; k++)
                    {
                        int x = side < 0 ? 2 + k : w - 3 - k;
                        float fold = (float)Math.Sin((k + y * 0.05f) * 1.6f);
                        c.Set(x, y, ArtKit.Tone(Velvet, fold * 0.5f + (side < 0 ? 0.1f : -0.1f)));
                    }
                }

                int tx = side < 0 ? 4 : w - 5;
                ArtKit.Ball(c, tx, 50f, 2.2f, 2.2f, Gold, 0.3f);
                c.Set(tx, 53, Gold[1]);
                c.Set(tx, 54, Gold[2]);
            }

            // O varao e as pontas douradas.
            c.Fill(0, 1, w, 2, Gold[2]);
            ArtKit.Ball(c, 1f, 2f, 2f, 2f, Gold, 0.3f);
            ArtKit.Ball(c, w - 2f, 2f, 2f, 2f, Gold, 0.3f);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>
        /// Um quadro em moldura de ouro trabalhada, 36x46: 0 uma Nobre de
        /// colar de prata, 1 a Cidade de Prata acesa, 2 a coroa na almofada.
        /// </summary>
        public static PixelCanvas Portrait(int kind)
        {
            const int w = 36;
            const int h = 46;
            var c = new PixelCanvas(w, h);

            // A moldura: duas camadas de ouro e os cantos em volutas.
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int edge = Math.Min(Math.Min(x, y), Math.Min(w - 1 - x, h - 1 - y));
                    if (edge > 4) continue;
                    float light = edge == 0 ? -0.4f : (edge == 1 ? 0.7f : (edge == 4 ? -0.6f : 0.2f));
                    if (edge == 2 && (x + y) % 3 == 0) light = 0.8f;
                    c.Set(x, y, ArtKit.Tone(Gold, light));
                }
            }

            foreach ((int x, int y) in new[] { (2, 2), (w - 3, 2), (2, h - 3), (w - 3, h - 3) }) ArtKit.Ball(c, x, y, 3f, 3f, Gold, 0.4f);

            // A pintura.
            for (int y = 5; y < h - 5; y++)
            {
                for (int x = 5; x < w - 5; x++)
                {
                    float light = (y - 5) / (float)(h - 10) * -1.2f + 0.3f;
                    c.Set(x, y, kind == 1 ? ArtKit.Tone(Night, light) : ArtKit.Tone(Velvet, light - 0.4f));
                }
            }

            if (kind == 0)
            {
                // A Nobre: o rosto de prata, o cabelo alto, o colar.
                ArtKit.Ball(c, 18f, 34f, 9f, 8f, Velvet, 0.3f);
                ArtKit.Ball(c, 18f, 20f, 5f, 6f, Silver, 0.4f);
                ArtKit.Ball(c, 18f, 13f, 6f, 4f, new[] { new Rgba(60, 50, 60), new Rgba(90, 76, 86), new Rgba(130, 112, 120), new Rgba(170, 150, 156) }, 0.2f);
                for (int i = 0; i < 7; i++) c.Set(14 + i, 28 + (i == 3 ? 1 : 0), Silver[3]);
                c.Set(16, 20, ArtKit.Outline);
                c.Set(20, 20, ArtKit.Outline);
            }
            else if (kind == 1)
            {
                // A cidade acesa, sob a lua.
                ArtKit.Ball(c, 25f, 12f, 3f, 3f, Crystal, 0.4f);
                for (int i = 0; i < 6; i++)
                {
                    int x0 = 6 + i * 4;
                    int top = 24 + (i * 7) % 11;
                    for (int y = top; y < h - 5; y++)
                    {
                        for (int x = x0; x < x0 + 3; x++) c.Set(x, y, (x + y) % 5 == 0 ? new Rgba(255, 230, 160) : Silver[1]);
                    }
                }
            }
            else
            {
                // A coroa sobre a almofada de veludo.
                ArtKit.Ball(c, 18f, 33f, 10f, 4f, Velvet, 0.4f);
                for (int i = 0; i < 5; i++)
                {
                    float px = 11f + i * 3.5f;
                    float hgt = i == 2 ? 10f : 7f;
                    ArtKit.Polygon(c, new[] { (px - 1.6f, 28f), (px, 28f - hgt), (px + 1.6f, 28f) }, (x, y) => ArtKit.Tone(Crystal, 0.4f));
                }

                c.Fill(10, 28, 17, 2, Gold[2]);
            }

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Um vaso alto de porcelana branca com filetes de ouro e lirios de prata, num pedestal, 18x40.</summary>
        public static PixelCanvas Vase()
        {
            var c = new PixelCanvas(18, 40);
            c.Fill(3, 30, 12, 10, Marble[2]);
            c.Fill(3, 30, 12, 1, Marble[3]);
            c.Fill(13, 31, 2, 9, Marble[0]);
            c.Fill(2, 29, 14, 1, Gold[2]);

            ArtKit.Ball(c, 9f, 22f, 6f, 7f, Marble, 0.4f);
            ArtKit.Limb(c, 9f, 15f, 9f, 11f, 2.6f, Marble, 0.3f);
            ArtKit.Ball(c, 9f, 10f, 3.6f, 1.4f, Gold, 0.3f);
            for (int x = 4; x <= 14; x++) if (c.Get(x, 22).A > 0) c.Set(x, 22, Gold[2]);
            for (int x = 5; x <= 13; x += 2) c.Set(x, 25, Velvet[2]);

            // Os lirios de prata.
            foreach ((float x, float y) in new[] { (5f, 3f), (9f, 1f), (13f, 4f) })
            {
                ArtKit.Line(c, 9, 10, (int)x, (int)y + 2, new Rgba(70, 110, 80));
                ArtKit.Ball(c, x, y + 1f, 2f, 1.6f, Silver, 0.5f);
            }

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Um lustre de cristal e ouro, 64x52, pendurado pela corrente: tres andares de pingentes e velas acesas.</summary>
        public static PixelCanvas Chandelier()
        {
            const int w = 64;
            const int h = 52;
            var c = new PixelCanvas(w, h);
            float cx = w / 2f;

            // A corrente.
            for (int y = 0; y < 12; y++) c.Set((int)cx, y, y % 2 == 0 ? Gold[2] : Gold[0]);

            // Os aros de ouro, de cima pra baixo, cada um maior.
            float[] rings = { 14f, 24f, 30f };
            float[] heights = { 14f, 24f, 34f };
            for (int r = 0; r < rings.Length; r++)
            {
                for (int x = (int)(cx - rings[r]); x <= (int)(cx + rings[r]); x++)
                {
                    float k = (x - cx) / rings[r];
                    int y = (int)(heights[r] + k * k * 3f);
                    c.Set(x, y, Gold[3]);
                    c.Set(x, y + 1, Gold[1]);
                }

                // Os pingentes de cristal pendurados no aro.
                for (int i = -3; i <= 3; i++)
                {
                    float px = cx + i * rings[r] / 3.2f;
                    float k = (px - cx) / rings[r];
                    float py = heights[r] + k * k * 3f + 2f;
                    int len = 4 + (i + 3) % 3 + r;
                    ArtKit.Polygon(c, new[] { (px - 1.2f, py), (px + 1.2f, py), (px, py + len) }, (x, y) => ArtKit.Tone(Crystal, 0.5f - (y - py) * 0.1f));
                }

                // As velas nas pontas.
                foreach (float side in new[] { -1f, 1f })
                {
                    float vx = cx + side * rings[r];
                    float vy = heights[r] + 2f;
                    c.Fill((int)vx, (int)vy - 5, 2, 5, Marble[3]);
                    c.Set((int)vx, (int)vy - 7, Flame);
                    c.Set((int)vx, (int)vy - 6, FlameCore);
                }
            }

            // O miolo: a bola de cristal.
            ArtKit.Ball(c, cx, 44f, 4f, 5f, Crystal, 0.5f);
            ArtKit.AddOutline(c);
            ArtKit.Glow(c, cx, 26f, 26f, Flame, 0.18f, true);
            return c;
        }

        /// <summary>Uma arandela de ouro na parede, 14x20: o braco curvo e duas velas acesas.</summary>
        public static PixelCanvas Sconce()
        {
            var c = new PixelCanvas(14, 20);
            ArtKit.Ball(c, 7f, 15f, 3f, 4f, Gold, 0.3f);
            ArtKit.Line(c, 7, 13, 3, 9, Gold[2]);
            ArtKit.Line(c, 7, 13, 11, 9, Gold[2]);
            foreach (int x in new[] { 3, 11 })
            {
                c.Fill(x - 1, 8, 3, 1, Gold[1]);
                c.Fill(x, 4, 1, 4, Marble[3]);
                c.Set(x, 2, Flame);
                c.Set(x, 3, FlameCore);
            }

            ArtKit.AddOutline(c);
            ArtKit.Glow(c, 7f, 4f, 7f, Flame, 0.35f, true);
            return c;
        }

        /// <summary>O lambri de nogueira com almofada e filete de ouro, 16x32 (emendavel).</summary>
        public static PixelCanvas Wainscot()
        {
            var c = new PixelCanvas(16, 32);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    bool frame = x < 2 || x > 13 || y < 4 || y > 27;
                    float light = frame ? 0.3f : -0.1f + (float)Math.Sin(x * 0.7f + y * 0.15f) * 0.15f;
                    if (!frame && (x == 2 || y == 4)) light = -0.6f;
                    if (!frame && (x == 13 || y == 27)) light = 0.6f;
                    c.Set(x, y, ArtKit.Tone(Walnut, light));
                }
            }

            for (int x = 0; x < 16; x++)
            {
                c.Set(x, 0, Gold[3]);
                c.Set(x, 1, Gold[1]);
                c.Set(x, 31, Gold[0]);
            }

            return c;
        }

        /// <summary>O friso dourado de ovulos e dardos, embaixo do teto, 16x10 (emendavel).</summary>
        public static PixelCanvas Frieze()
        {
            var c = new PixelCanvas(16, 10);
            for (int x = 0; x < 16; x++)
            {
                c.Set(x, 0, Gold[1]);
                c.Set(x, 1, Gold[3]);
                c.Set(x, 8, Gold[2]);
                c.Set(x, 9, Gold[0]);
            }

            for (int i = 0; i < 2; i++)
            {
                ArtKit.Ball(c, 4f + i * 8f, 5f, 2.6f, 2.6f, Gold, 0.3f);
                c.Set(8 * i, 4, Gold[0]);
                c.Set(8 * i, 5, Gold[0]);
                c.Set(8 * i, 6, Gold[0]);
            }

            return c;
        }

        /// <summary>O busto de prata de um dos 925 Nobres, num pedestal de marmore, 22x44.</summary>
        public static PixelCanvas Bust()
        {
            var c = new PixelCanvas(22, 44);
            c.Fill(4, 22, 14, 22, Marble[2]);
            c.Fill(4, 22, 14, 2, Marble[3]);
            c.Fill(15, 24, 3, 20, Marble[0]);
            c.Fill(3, 21, 16, 1, Gold[2]);
            c.Fill(6, 32, 10, 3, Gold[1]);

            ArtKit.Ball(c, 11f, 18f, 8f, 4f, Silver, 0.3f);
            ArtKit.Limb(c, 11f, 15f, 11f, 11f, 2.6f, Silver, 0.2f);
            ArtKit.Ball(c, 11f, 7f, 5f, 6f, Silver, 0.4f);
            ArtKit.Taper(c, 14f, 7f, 17f, 9f, 1.4f, 0.6f, Silver, 0.5f);
            c.Set(13, 6, ArtKit.Outline);
            ArtKit.Ball(c, 10f, 2f, 5f, 2f, Silver, 0.1f);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Um banco de veludo vermelho com pes de ouro torneados, 40x18.</summary>
        public static PixelCanvas Bench()
        {
            var c = new PixelCanvas(40, 18);
            ArtKit.Drum(c, 20f, 7f, 18f, 4f, 3f, ArtKit.Plain(Velvet), 0.3f);
            for (int x = 4; x < 36; x += 4) c.Set(x, 7, Gold[3]);
            c.Fill(2, 10, 36, 2, Gold[2]);
            c.Fill(2, 11, 36, 1, Gold[0]);
            foreach (int x in new[] { 4, 19, 34 })
            {
                for (int y = 12; y < 18; y++) c.Set(x, y, y % 2 == 0 ? Gold[3] : Gold[1]);
                c.Set(x + 1, 17, Gold[0]);
            }

            ArtKit.AddOutline(c);
            return c;
        }
    }
}
