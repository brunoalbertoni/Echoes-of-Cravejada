using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class LabArt
    {
        // ------------------------------------------------------------------
        // Mobilia
        // ------------------------------------------------------------------

        /// <summary>Cabide de jalecos, 26x36: dois jalecos e uma mascara de bico pendurada.</summary>
        public static PixelCanvas CoatRack()
        {
            var c = new PixelCanvas(26, 36);

            c.Fill(12, 3, 2, 31, Walnut[2]);
            c.Fill(7, 34, 12, 2, Walnut[1]);
            c.Fill(6, 4, 14, 1, Walnut[3]);

            var coat = new[] { new Rgba(150, 150, 150), new Rgba(196, 196, 192), new Rgba(226, 226, 220), new Rgba(250, 250, 246) };

            ArtKit.Polygon(c, new[] { (6f, 5f), (11f, 5f), (12f, 26f), (3f, 27f) }, (x, y) => ArtKit.Tone(coat, 0.4f - (x - 3f) / 9f));
            ArtKit.Polygon(c, new[] { (15f, 5f), (20f, 5f), (23f, 25f), (14f, 26f) }, (x, y) => ArtKit.Tone(coat, 0.2f - (x - 14f) / 9f));
            c.Set(17, 20, OxideGreen);
            c.Set(18, 21, OxideDark);

            // A mascara de bico, de couro, pendurada no alto.
            ArtKit.Ball(c, 13f, 8f, 3f, 3f, ArtKit.Plain(ArtKit.Leather), 0.2f);
            ArtKit.Taper(c, 13f, 9f, 13f, 16f, 1.6f, 0.4f, ArtKit.Leather, 0.1f);
            c.Set(12, 7, Brass[3]);
            c.Set(14, 7, Brass[3]);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Pulpito de nogueira, 20x28, com o tampo inclinado e um papel.</summary>
        public static PixelCanvas Lectern()
        {
            var c = new PixelCanvas(20, 28);

            ArtKit.Polygon(c, new[] { (6f, 10f), (14f, 10f), (15f, 26f), (5f, 26f) }, (x, y) => ArtKit.Tone(Walnut, 0.3f - (x - 5f) / 8f));
            c.Fill(3, 26, 14, 2, Walnut[1]);
            ArtKit.Polygon(c, new[] { (1f, 9f), (19f, 5f), (19f, 8f), (1f, 12f) }, (x, y) => ArtKit.Tone(Walnut, 0.5f));
            ArtKit.Polygon(c, new[] { (4f, 8f), (15f, 6f), (15f, 7f), (4f, 9f) }, (x, y) => new Rgba(230, 222, 200));
            c.Set(9, 7, new Rgba(168, 42, 38));

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Busto de prata num pedestal de marmore, 16x30: um Nobre, com o nariz lascado.</summary>
        public static PixelCanvas Bust()
        {
            var c = new PixelCanvas(16, 30);

            ArtKit.Drum(c, 8f, 24f, 5f, 6f, 6f, ArtKit.Plain(Marble), 0f);
            c.Fill(2, 17, 12, 2, Marble[3]);
            ArtKit.Ball(c, 8f, 14f, 6f, 3f, ArtKit.Plain(Silver), 0.1f);
            ArtKit.Ball(c, 8f, 8f, 3.6f, 4.4f, ArtKit.Plain(Silver), 0.2f);
            c.Set(6, 7, Silver[0]);
            c.Set(9, 7, Silver[0]);
            c.Set(10, 9, Silver[1]);
            c.Set(5, 11, OxideGreen);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Bancada de lentes, 36x24: lentes em suportes, uma lampada de arco e cadernos.</summary>
        public static PixelCanvas LensBench()
        {
            var c = new PixelCanvas(36, 24);

            c.Fill(1, 12, 34, 3, Walnut[2]);
            c.Fill(1, 12, 34, 1, Walnut[3]);
            c.Fill(3, 15, 3, 9, Walnut[1]);
            c.Fill(30, 15, 3, 9, Walnut[0]);

            // A lampada de arco.
            c.Fill(3, 6, 6, 6, Brass[1]);
            c.Fill(3, 6, 6, 1, Brass[3]);
            c.Fill(8, 8, 2, 2, new Rgba(255, 244, 190));

            // As lentes.
            for (int i = 0; i < 3; i++)
            {
                int x = 14 + i * 7;
                c.Fill(x, 8, 1, 4, Iron[2]);
                ArtKit.Ball(c, x + 0.5f, 6f, 1.4f, 3f, ArtKit.Plain(new[] { GlassRim.WithAlpha(255), GlassRim.WithAlpha(255), GlassShine, GlassShine }), 0.2f);
                c.Fill(x - 1, 3, 3, 1, Brass[2]);
            }

            // O feixe fino atravessando as lentes.
            for (int x = 10; x < 33; x++) c.Blend(x, 6, new Rgba(255, 240, 170, 110));

            // Cadernos.
            c.Fill(29, 9, 5, 3, new Rgba(120, 40, 40));
            c.Fill(29, 9, 5, 1, new Rgba(214, 200, 160));

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Prisma num tripe, 16x24, decompondo um fiapo de luz.</summary>
        public static PixelCanvas Prism()
        {
            var c = new PixelCanvas(16, 24);

            Tripod(c, 8, 13, 24, 3);
            ArtKit.Polygon(c, new[] { (8f, 2f), (14f, 12f), (2f, 12f) }, (x, y) => (x < 8f ? GlassShine : GlassRim).WithAlpha(230));
            ArtKit.Polygon(c, new[] { (8f, 5f), (11f, 10f), (5f, 10f) }, (x, y) => GlassFill.WithAlpha(160));

            Rgba[] spectrum = { new Rgba(255, 80, 80), Amber, new Rgba(240, 240, 90), OxideGreen, Azure, new Rgba(170, 100, 255) };
            for (int i = 0; i < spectrum.Length; i++) c.Set(13 + i / 3, 9 + i, spectrum[i]);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Espelho de prata num eixo, 16x32: o vidro inclinado e o pe de tres garras.</summary>
        public static PixelCanvas StandingMirror()
        {
            var c = new PixelCanvas(16, 32);

            c.Fill(7, 16, 2, 14, Silver[1]);
            c.Fill(3, 30, 10, 2, Silver[2]);
            ArtKit.Polygon(c, new[] { (3f, 4f), (12f, 1f), (13f, 16f), (4f, 19f) }, (x, y) => Silver[3]);
            ArtKit.Polygon(c, new[] { (5f, 5f), (11f, 3f), (11f, 14f), (6f, 16f) }, (x, y) => new Rgba(170, 196, 220));
            ArtKit.Line(c, 6, 6, 9, 5, GlassShine);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Uma fileira de poltronas do anfiteatro, 64x18: veludo vermelho, encosto de nogueira.</summary>
        public static PixelCanvas Seats()
        {
            var c = new PixelCanvas(64, 18);

            for (int i = 0; i < 6; i++)
            {
                int x = 2 + i * 10;
                ArtKit.Drum(c, x + 4f, 7f, 4f, 5f, 4f, ArtKit.Plain(Velvet), 0.1f);
                ArtKit.Drum(c, x + 4f, 13f, 4.5f, 2.5f, 4f, ArtKit.Plain(Velvet), 0.2f);
                c.Fill(x, 15, 1, 3, Walnut[1]);
                c.Fill(x + 8, 15, 1, 3, Walnut[0]);
                for (int b = 0; b < 3; b++) c.Set(x + 2 + b * 2, 7, Brass[3]);
            }

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>A borda do palco, 52x8: o tablado de nogueira com o friso de prata, rente ao chao.</summary>
        public static PixelCanvas Stage()
        {
            var c = new PixelCanvas(52, 8);

            c.Fill(0, 0, 52, 8, Walnut[2]);
            for (int x = 0; x < 52; x += 8) c.Fill(x, 0, 1, 8, Walnut[1]);
            c.Fill(0, 0, 52, 1, Walnut[3]);
            c.Fill(0, 6, 52, 2, Silver[1]);
            c.Fill(0, 6, 52, 1, Silver[3]);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>O ralo de prata no chao da arena, 26x6.</summary>
        public static PixelCanvas Drain()
        {
            var c = new PixelCanvas(26, 6);

            ArtKit.Ball(c, 13f, 3f, 12f, 3f, ArtKit.Plain(Silver), 0.1f);
            for (int x = 4; x < 23; x += 3) c.Fill(x, 2, 1, 3, Ink);
            c.Fill(10, 5, 6, 1, Mercury[2]);

            return c;
        }

        /// <summary>
        /// Uma mesa de prototipo, 34x26, com o que sobrou da tentativa:
        /// 0 o de ferro fundido, 1 o de latao comido, 2 o de cobre (coberto
        /// por um pano), 3 o de vidro estilhacado, 4 o de prata podre, 5 o de
        /// quartzo lapidado em pedacos, 6 a setima mesa, vazia, com as cintas
        /// abertas e a etiqueta "7".
        /// </summary>
        public static PixelCanvas PrototypeTable(int n)
        {
            var c = new PixelCanvas(34, 26);

            c.Fill(3, 17, 3, 9, Iron[1]);
            c.Fill(28, 17, 3, 9, Iron[0]);
            c.Fill(1, 14, 32, 3, Walnut[2]);
            c.Fill(1, 14, 32, 1, Walnut[3]);
            c.Fill(1, 16, 32, 1, Walnut[0]);

            switch (n)
            {
                case 0:
                    ArtKit.Ball(c, 17f, 9f, 9f, 5f, ArtKit.Plain(Iron), 0.1f);
                    ArtKit.Ball(c, 8f, 8f, 3.5f, 3.5f, ArtKit.Plain(Iron), 0.2f);
                    ArtKit.Rivet(c, 14, 7, Iron);
                    ArtKit.Rivet(c, 20, 7, Iron);
                    break;
                case 1:
                    ArtKit.Ball(c, 16f, 10f, 8f, 4f, ArtKit.Corroded(Brass, 0.55f, 4171), 0.1f);
                    ArtKit.Ball(c, 7f, 10f, 3f, 3f, ArtKit.Corroded(Brass, 0.55f, 4172), 0.2f);
                    break;
                case 2:
                    ArtKit.Polygon(c, new[] { (4f, 13f), (8f, 5f), (26f, 4f), (30f, 13f) }, (x, y) =>
                        ArtKit.Tone(new[] { new Rgba(150, 146, 136), new Rgba(190, 186, 174), new Rgba(222, 218, 206), new Rgba(244, 242, 234) },
                            0.4f - (y - 4f) / 9f + (((int)x) % 6 == 0 ? -0.4f : 0f)));
                    c.Fill(27, 10, 3, 3, Copper[2]);
                    break;
                case 3:
                    for (int i = 0; i < 14; i++)
                    {
                        int x = 4 + ArtKit.Hash(i, 3, 4173) % 26;
                        int y = 6 + ArtKit.Hash(i, 3, 4174) % 7;
                        ArtKit.Polygon(c, new[] { (x, y + 0f), (x + 2.5f, y + 2f), (x + 0.5f, y + 3f) }, (px, py) => GlassRim);
                    }
                    break;
                case 4:
                    ArtKit.Ball(c, 17f, 10f, 9f, 4f, ArtKit.Corroded(Silver, 0.65f, 4175, 0.3f), 0.1f);
                    ArtKit.Ball(c, 7f, 10f, 3f, 3f, ArtKit.Corroded(Silver, 0.65f, 4176, 0.3f), 0.2f);
                    c.Fill(11, 13, 12, 1, OxideGreen);
                    break;
                case 5:
                    for (int i = 0; i < 5; i++)
                    {
                        float x = 6f + i * 5.2f;
                        ArtKit.Polygon(c, new[] { (x, 13f), (x + 2f, 7f + i % 2 * 2), (x + 4f, 13f) }, (px, py) =>
                            px < x + 2f ? new Rgba(250, 236, 246) : new Rgba(214, 170, 214));
                    }
                    break;
                default:
                    // A setima: vazia. As cintas abertas e a etiqueta.
                    for (int x = 7; x <= 25; x += 9)
                    {
                        c.Fill(x, 13, 3, 1, ArtKit.Leather[2]);
                        c.Fill(x + 3, 15, 1, 6, ArtKit.Leather[1]);
                    }

                    c.Fill(14, 9, 6, 4, new Rgba(214, 200, 160));
                    c.Fill(16, 10, 2, 1, Ink);
                    c.Set(17, 11, Ink);
                    ArtKit.Line(c, 17, 8, 19, 5, Iron[2]);
                    break;
            }

            ArtKit.AddOutline(c);
            return c;
        }

        // ------------------------------------------------------------------
        // O Grande Alambique e os reagentes
        // ------------------------------------------------------------------

        /// <summary>
        /// O Grande Alambique do atelie, 72x116: uma caldeira de prata maior
        /// que o Scrap, com tres encaixes na base. Cada reagente entregue
        /// acende um; o pescoco sobe ate o mezanino e a grade.
        /// </summary>
        public static PixelCanvas GreatAlembic(int lit)
        {
            var c = new PixelCanvas(72, 116);

            // O pescoco e a cupula de vidro, subindo ate o mezanino.
            ArtKit.Limb(c, 36f, 4f, 36f, 40f, 5f, Silver, 0.15f);
            for (int y = 8; y < 40; y += 6) c.Fill(31, y, 11, 1, Brass[2]);
            Bulb(c, 36f, 48f, 16f, 12f, lit >= 3 ? new Rgba(250, 236, 180) : Mercury[1], 0.25f + lit * 0.2f);

            // A caldeira.
            ArtKit.Ball(c, 36f, 78f, 30f, 20f, ArtKit.Plain(Silver), 0.1f);
            for (int x = 10; x <= 62; x += 6) ArtKit.Rivet(c, x, 82, Silver);
            c.Fill(6, 62, 60, 2, Silver[3]);

            // A base, com os tres encaixes.
            c.Fill(2, 96, 68, 20, Marble[1]);
            c.Fill(2, 96, 68, 2, Marble[3]);
            c.Fill(2, 114, 68, 2, Marble[0]);

            Rgba[] colors = { Mercury[3], new Rgba(255, 236, 140), new Rgba(250, 200, 240) };
            for (int i = 0; i < 3; i++)
            {
                int x = 14 + i * 22;
                ArtKit.Ball(c, x, 104f, 6f, 6f, ArtKit.Plain(Brass), 0.2f);
                ArtKit.Ball(c, x, 104f, 4f, 4f, ArtKit.Plain(Iron), -0.3f);

                if (i < lit)
                {
                    ArtKit.Ball(c, x, 104f, 3.4f, 3.4f, ArtKit.Plain(new[] { colors[i].Scaled(0.6f), colors[i].Scaled(0.8f), colors[i], new Rgba(255, 255, 255) }), 0.3f);
                    ArtKit.Glow(c, x, 104f, 8f, colors[i], 0.6f, true);
                }
            }

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Um reagente, 12x16: 0 o mercurio destilado, 1 a luz engarrafada, 2 o sal de quartzo.</summary>
        public static PixelCanvas Reagent(int index)
        {
            var c = new PixelCanvas(12, 16);

            Rgba liquid = index == 0 ? Mercury[2] : index == 1 ? new Rgba(255, 236, 140) : new Rgba(250, 210, 240);
            Bulb(c, 6f, 10.5f, 4.5f, 4.5f, liquid, index == 2 ? 0.5f : 0.75f);
            Tube(c, 4, 2, 7, 6);
            c.Fill(3, 1, 6, 2, Brass[2]);

            if (index == 1) ArtKit.Glow(c, 6f, 10f, 6f, new Rgba(255, 244, 190), 0.8f, true);
            if (index == 2)
            {
                ArtKit.Polygon(c, new[] { (3f, 13f), (5f, 8f), (7f, 13f) }, (x, y) => new Rgba(255, 240, 250));
                ArtKit.Polygon(c, new[] { (6f, 13f), (8f, 10f), (9f, 13f) }, (x, y) => new Rgba(220, 170, 220));
            }

            ArtKit.AddOutline(c);
            return c;
        }

        // ------------------------------------------------------------------
        // A redoma (a do chefe)
        // ------------------------------------------------------------------

        /// <summary>
        /// A campanula de vidro que o Alquimista derruba do teto, 40x44 (2,5 x
        /// 2,75 tiles). Cracks de 0 a 3: cada golpe racha mais.
        /// </summary>
        public static PixelCanvas BellJar(int cracks)
        {
            var c = new PixelCanvas(40, 44);

            for (int y = 6; y < 42; y++)
            {
                float half = y < 18 ? (float)Math.Sqrt(Math.Max(0f, 144f - (18 - y) * (18 - y))) + 6f : 18f;
                for (int x = (int)(20 - half); x < (int)(20 + half); x++)
                {
                    bool edge = x == (int)(20 - half) || x == (int)(20 + half) - 1 || y == 41;
                    c.Blend(x, y, edge ? GlassRim : GlassFill.WithAlpha(90));
                }
            }

            // O puxador de latao e o aro de baixo.
            ArtKit.Ball(c, 20f, 4f, 3f, 3f, ArtKit.Plain(Brass), 0.3f);
            c.Fill(1, 41, 38, 3, Brass[1]);
            c.Fill(1, 41, 38, 1, Brass[3]);
            for (int y = 10; y < 36; y++) c.Set(9 + (y < 18 ? (18 - y) / 3 : 0), y, GlassShine);

            for (int i = 0; i < cracks; i++)
            {
                int sx = 12 + i * 8;
                int sy = 16 + i * 6;
                for (int k = 0; k < 10 + i * 4; k++)
                {
                    int x = sx + (ArtKit.Hash(k, i, 4181) % 3) - 1 + k / 2;
                    int y = sy + k;
                    c.Set(x, y, new Rgba(255, 255, 255));
                }
            }

            return c;
        }

        /// <summary>A sombra que avisa onde a redoma vai cair, 40x6.</summary>
        public static PixelCanvas BellShadow()
        {
            var c = new PixelCanvas(40, 6);
            for (int y = 0; y < 6; y++)
            {
                for (int x = 0; x < 40; x++)
                {
                    float dx = (x + 0.5f - 20f) / 20f;
                    float dy = (y + 0.5f - 3f) / 3f;
                    if (dx * dx + dy * dy <= 1f) c.Set(x, y, new Rgba(10, 8, 16, 150));
                }
            }

            return c;
        }

        /// <summary>Um pedaco do chicote de mercurio, 16x8, emendavel deitado (modo Tiled).</summary>
        public static PixelCanvas WhipSegment()
        {
            var c = new PixelCanvas(16, 8);
            for (int x = 0; x < 16; x++)
            {
                int wave = (int)Math.Round(Math.Sin(x * Math.PI / 8.0));
                for (int y = 2 + wave; y < 6 + wave; y++)
                {
                    int row = y - (2 + wave);
                    c.Set(x, y, row == 0 ? Mercury[3] : row == 3 ? Mercury[0] : Mercury[x % 5 == 0 ? 3 : 2]);
                }
            }

            return c;
        }

        /// <summary>Uma gota de mercurio que pinga da nuvem do Alquimista, 6x8.</summary>
        public static PixelCanvas MercuryBead()
        {
            var c = new PixelCanvas(6, 8);
            ArtKit.Ball(c, 3f, 5f, 2.6f, 2.6f, ArtKit.Plain(Mercury), 0.3f);
            c.Set(3, 1, Mercury[2]);
            c.Set(3, 2, Mercury[2]);
            ArtKit.AddOutline(c);
            return c;
        }
    }
}
