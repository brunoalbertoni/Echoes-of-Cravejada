using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O laboratorio do District 4, peca por peca: alambiques, retortas, bicos
    /// de gas, o grande destilador, vidros de especime, estantes de reagente,
    /// tanques de cultura com as cobaias de cobre, gaiolas, a mesa de
    /// dissecacao, as mesas dos prototipos, o Grande Alambique e o que vai no
    /// anfiteatro. Tudo a 16 px por tile, desenhado aqui (y cresce pra baixo),
    /// sem UnityEngine.
    ///
    /// <see cref="Sprites"/> lista os nomes e o desenho de cada um, pro gerador
    /// gravar tudo num laco.
    /// </summary>
    public static class LabArt
    {
        // ------------------------------------------------------------------
        // Cores
        // ------------------------------------------------------------------

        public static readonly Rgba[] Silver =
            { new Rgba(70, 76, 96), new Rgba(120, 128, 150), new Rgba(178, 186, 206), new Rgba(236, 240, 250) };

        public static readonly Rgba[] Marble =
            { new Rgba(92, 98, 120), new Rgba(150, 156, 176), new Rgba(198, 204, 220), new Rgba(238, 240, 248) };

        public static readonly Rgba[] Walnut =
            { new Rgba(40, 26, 24), new Rgba(70, 44, 34), new Rgba(104, 68, 46), new Rgba(142, 98, 66) };

        public static readonly Rgba[] Velvet =
            { new Rgba(56, 14, 26), new Rgba(98, 26, 42), new Rgba(146, 44, 62), new Rgba(194, 82, 96) };

        public static readonly Rgba[] Copper = ArtKit.Copper;
        public static readonly Rgba[] Brass = ArtKit.Brass;
        public static readonly Rgba[] Iron = ArtKit.Iron;

        public static readonly Rgba[] Mercury =
            { new Rgba(96, 104, 124), new Rgba(160, 170, 192), new Rgba(214, 222, 236), new Rgba(255, 255, 255) };

        public static readonly Rgba OxideGreen = new Rgba(92, 242, 153);
        public static readonly Rgba OxideDark = new Rgba(40, 150, 96);
        public static readonly Rgba Amber = new Rgba(240, 168, 60);
        public static readonly Rgba Azure = new Rgba(96, 156, 255);
        public static readonly Rgba Rose = new Rgba(224, 96, 196);
        public static readonly Rgba Flame = new Rgba(120, 180, 255);
        public static readonly Rgba FlameCore = new Rgba(230, 244, 255);

        private static readonly Rgba GlassRim = new Rgba(200, 228, 240, 230);
        private static readonly Rgba GlassFill = new Rgba(150, 196, 214, 70);
        private static readonly Rgba GlassShine = new Rgba(250, 254, 255, 235);
        private static readonly Rgba Ink = ArtKit.Outline;

        // ------------------------------------------------------------------
        // A lista
        // ------------------------------------------------------------------

        /// <summary>Os sprites de cenario: nome, desenho e se o pivo e no alto (pendurado).</summary>
        public static IEnumerable<(string Name, Func<PixelCanvas> Draw, bool Hanging)> Sprites()
        {
            yield return ("lab_alembic", Alembic, false);
            yield return ("lab_retort", Retort, false);
            yield return ("lab_burner", Burner, false);
            yield return ("lab_still", Still, false);
            for (int i = 0; i < 3; i++) { int k = i; yield return ("lab_jar_" + i, () => SpecimenJar(k), false); }
            yield return ("lab_rack", TubeRack, false);
            yield return ("lab_table", DissectionTable, false);
            yield return ("lab_shelf", ReagentShelf, false);
            yield return ("lab_cage", () => Cage(false), false);
            yield return ("lab_cage_broken", () => Cage(true), false);
            for (int i = 0; i < 3; i++) { int k = i; yield return ("lab_tank_" + i, () => CultureTank(k), false); }
            yield return ("lab_great_tank", GreatTank, false);
            yield return ("lab_chandelier", Chandelier, true);
            yield return ("lab_pipes", Pipes, true);
            yield return ("lab_coats", CoatRack, false);
            yield return ("lab_lectern", Lectern, false);
            yield return ("lab_bust", Bust, false);
            yield return ("lab_lens_bench", LensBench, false);
            yield return ("lab_prism", Prism, false);
            yield return ("lab_mirror", StandingMirror, false);
            yield return ("lab_seats", Seats, false);
            yield return ("lab_stage", Stage, false);
            yield return ("lab_drain", Drain, false);
            for (int i = 0; i < 7; i++) { int k = i; yield return ("lab_proto_table_" + i, () => PrototypeTable(k), false); }
        }

        /// <summary>O Grande Alambique com tantos encaixes acesos (0 a 3).</summary>
        public static string GreatAlembicName(int lit) => "lab_great_alembic_" + lit;

        public static string ReagentName(int index) => "lab_reagent_" + index;

        // ------------------------------------------------------------------
        // Pecas de vidro
        // ------------------------------------------------------------------

        /// <summary>Um bulbo de vidro (elipse) com liquido ate level (0 a 1) e o brilho no alto a esquerda.</summary>
        private static void Bulb(PixelCanvas c, float cx, float cy, float rx, float ry, Rgba liquid, float level)
        {
            float surface = cy + ry - level * 2f * ry;

            for (int py = (int)Math.Floor(cy - ry) - 1; py <= (int)(cy + ry) + 1; py++)
            {
                for (int px = (int)Math.Floor(cx - rx) - 1; px <= (int)(cx + rx) + 1; px++)
                {
                    float dx = (px + 0.5f - cx) / rx;
                    float dy = (py + 0.5f - cy) / ry;
                    float d = dx * dx + dy * dy;
                    if (d > 1f) continue;

                    bool rim = d > 0.78f;
                    if (py + 0.5f >= surface && level > 0f)
                    {
                        Rgba l = py + 0.5f < surface + 1.2f ? Rgba.Lerp(liquid, new Rgba(255, 255, 255), 0.45f) : liquid;
                        if (dx > 0.35f) l = l.Scaled(0.75f);
                        c.Set(px, py, rim ? Rgba.Lerp(l, GlassRim, 0.4f).WithAlpha(255) : l);
                    }
                    else
                    {
                        c.Blend(px, py, rim ? GlassRim : GlassFill);
                    }
                }
            }

            // O reflexo.
            for (int i = 0; i < 3; i++)
            {
                c.Set((int)(cx - rx * 0.5f) + i / 2, (int)(cy - ry * 0.55f) + i, GlassShine);
            }
        }

        /// <summary>Um tubo de vidro reto (vertical ou deitado), com liquido opcional.</summary>
        private static void Tube(PixelCanvas c, int x0, int y0, int x1, int y1, Rgba? liquid = null)
        {
            for (int y = Math.Min(y0, y1); y <= Math.Max(y0, y1); y++)
            {
                for (int x = Math.Min(x0, x1); x <= Math.Max(x0, x1); x++)
                {
                    bool edge = x == Math.Min(x0, x1) || x == Math.Max(x0, x1) || y == Math.Min(y0, y1) || y == Math.Max(y0, y1);
                    if (liquid.HasValue && !edge) c.Set(x, y, liquid.Value);
                    else c.Blend(x, y, edge ? GlassRim : GlassFill);
                }
            }
        }

        /// <summary>Um bico de gas com a chama azul, de pe em (cx, base).</summary>
        private static void SmallBurner(PixelCanvas c, int cx, int baseY, bool lit = true)
        {
            c.Fill(cx - 3, baseY - 2, 7, 2, Brass[1]);
            c.Fill(cx - 3, baseY - 2, 7, 1, Brass[2]);
            c.Fill(cx - 1, baseY - 8, 3, 6, Brass[2]);
            c.Fill(cx - 1, baseY - 8, 1, 6, Brass[3]);
            c.Fill(cx + 1, baseY - 8, 1, 6, Brass[0]);
            c.Set(cx + 2, baseY - 4, Brass[1]);
            c.Set(cx + 3, baseY - 4, Brass[0]);

            if (!lit) return;

            c.Blend(cx - 1, baseY - 10, Flame.WithAlpha(200));
            c.Blend(cx + 1, baseY - 10, Flame.WithAlpha(200));
            c.Set(cx, baseY - 9, FlameCore);
            c.Set(cx, baseY - 10, Flame);
            c.Set(cx, baseY - 11, Flame);
            c.Blend(cx, baseY - 12, Flame.WithAlpha(140));
        }

        /// <summary>Um tripe de ferro com o aro em cima, em y top.</summary>
        private static void Tripod(PixelCanvas c, int cx, int top, int baseY, int half)
        {
            ArtKit.Line(c, cx - half, top, cx - half - 2, baseY - 1, Iron[2]);
            ArtKit.Line(c, cx + half, top, cx + half + 2, baseY - 1, Iron[1]);
            ArtKit.Line(c, cx, top, cx, baseY - 1, Iron[1]);
            c.Fill(cx - half - 1, top, half * 2 + 3, 1, Iron[3]);
            c.Fill(cx - half - 1, top + 1, half * 2 + 3, 1, Iron[1]);
        }

        // ------------------------------------------------------------------
        // Bancada e vidraria
        // ------------------------------------------------------------------

        /// <summary>Alambique de cobre, 30x36: a panela no fogo, o capitel e o bico descendo ate o frasco.</summary>
        public static PixelCanvas Alembic()
        {
            var c = new PixelCanvas(30, 36);

            // O fogareiro de tijolo.
            ArtKit.Drum(c, 10f, 31f, 8f, 4.5f, 6f, ArtKit.Plain(Marble), -0.1f);
            c.Fill(6, 30, 8, 2, new Rgba(40, 20, 16));
            c.Fill(7, 30, 6, 1, Amber);
            c.Set(9, 29, new Rgba(255, 230, 150));

            // A panela.
            ArtKit.Ball(c, 10f, 21f, 8f, 6.5f, ArtKit.Corroded(Copper, 0.08f, 4101, 0.3f), 0.1f);
            for (int x = 4; x <= 16; x += 4) ArtKit.Rivet(c, x, 23, Copper);
            c.Fill(3, 16, 15, 1, Copper[3]);

            // O capitel e o pescoco.
            ArtKit.Ball(c, 10f, 11f, 4.5f, 4f, ArtKit.Plain(Copper), 0.2f);
            ArtKit.Limb(c, 10f, 7f, 10f, 4f, 1.4f, Copper, 0.2f);

            // O bico, descendo ate o frasco.
            ArtKit.Taper(c, 13f, 9f, 23f, 17f, 1.5f, 0.9f, Copper, 0.1f);

            // O frasco que recolhe.
            Bulb(c, 24f, 26f, 4.5f, 5f, Azure, 0.45f);
            Tube(c, 23, 18, 25, 21);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Retorta de vidro, 22x22: o bulbo no tripe, sobre o bico de gas, com o pescoco curvo.</summary>
        public static PixelCanvas Retort()
        {
            var c = new PixelCanvas(22, 22);

            SmallBurner(c, 7, 22);
            Tripod(c, 7, 12, 22, 5);
            Bulb(c, 7f, 7f, 5f, 5f, Rose, 0.5f);

            // O pescoco, descendo pra direita.
            for (int i = 0; i < 10; i++)
            {
                int x = 11 + i;
                int y = 3 + i / 2 + (i > 6 ? i - 6 : 0);
                c.Blend(x, y, GlassRim);
                c.Blend(x, y + 1, GlassFill);
            }

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Bico de gas sozinho, com uma garra e um tubo de ensaio fervendo, 12x22.</summary>
        public static PixelCanvas Burner()
        {
            var c = new PixelCanvas(12, 22);
            SmallBurner(c, 5, 22);

            // A garra e o tubo.
            c.Fill(9, 2, 1, 20, Iron[2]);
            c.Fill(4, 4, 6, 1, Iron[3]);
            Tube(c, 4, 1, 6, 10, OxideGreen);
            c.Blend(5, 0, new Rgba(160, 255, 200, 120));

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>
        /// O grande destilador, 52x52: a caldeira de cobre, a coluna de pratos,
        /// a serpentina no barril de agua e o garrafao de mercurio no fim.
        /// </summary>
        public static PixelCanvas Still()
        {
            var c = new PixelCanvas(52, 52);

            // A caldeira e a fornalha.
            c.Fill(2, 44, 22, 8, Iron[1]);
            c.Fill(2, 44, 22, 1, Iron[3]);
            c.Fill(7, 46, 12, 4, new Rgba(36, 18, 14));
            c.Fill(8, 47, 10, 2, Amber);
            for (int x = 9; x < 18; x += 3) c.Set(x, 47, new Rgba(255, 236, 170));
            ArtKit.Ball(c, 13f, 36f, 11f, 9f, ArtKit.Corroded(Copper, 0.1f, 4111, 0.25f), 0.1f);
            for (int x = 5; x <= 21; x += 4) ArtKit.Rivet(c, x, 38, Copper);
            c.Fill(2, 28, 23, 1, Copper[3]);

            // A coluna de pratos.
            ArtKit.Limb(c, 13f, 28f, 13f, 6f, 3.4f, Copper, 0.15f);
            for (int y = 10; y <= 26; y += 4)
            {
                c.Fill(9, y, 9, 1, Brass[2]);
                c.Fill(9, y + 1, 9, 1, Brass[0]);
            }

            // O manometro.
            ArtKit.Ball(c, 19f, 18f, 2.5f, 2.5f, ArtKit.Plain(Silver), 0.3f);
            c.Set(19, 18, Ink);
            c.Set(20, 17, Ink);

            // O braco ate a serpentina.
            ArtKit.Taper(c, 14f, 6f, 36f, 14f, 1.6f, 1.2f, Copper, 0.1f);

            // O barril de agua com a serpentina dentro (as voltas aparecem pelo vidro).
            ArtKit.Drum(c, 38f, 27f, 7f, 12f, 5f, ArtKit.Plain(Walnut), 0f);
            for (int y = 17; y <= 37; y += 6)
            {
                c.Fill(31, y, 15, 1, Brass[1]);
            }

            c.Fill(34, 18, 8, 18, new Rgba(70, 120, 150));
            for (int y = 20; y <= 34; y += 3)
            {
                ArtKit.Line(c, 35, y, 41, y + 1, Copper[2]);
            }

            c.Fill(34, 18, 1, 18, GlassShine);

            // A torneira e o garrafao.
            ArtKit.Limb(c, 44f, 38f, 47f, 38f, 1f, Brass, 0.1f);
            Bulb(c, 46f, 46f, 5f, 5f, Mercury[2], 0.6f);
            c.Fill(45, 40, 3, 1, Walnut[2]);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Vidro de especime, 16x22: 0 o olho no verde, 1 mercurio puro, 2 uma mao de cobre.</summary>
        public static PixelCanvas SpecimenJar(int kind)
        {
            var c = new PixelCanvas(16, 22);

            Rgba liquid = kind == 0 ? new Rgba(70, 190, 120) : kind == 1 ? Mercury[1] : new Rgba(200, 180, 110);
            for (int y = 5; y < 21; y++)
            {
                for (int x = 2; x < 14; x++)
                {
                    bool edge = x == 2 || x == 13 || y == 20;
                    if (edge) c.Set(x, y, GlassRim.WithAlpha(255));
                    else c.Set(x, y, y < 7 ? GlassFill.WithAlpha(150) : liquid);
                }
            }

            if (kind == 0)
            {
                ArtKit.Ball(c, 8f, 13f, 3.5f, 3.5f, ArtKit.Plain(new[] { new Rgba(180, 170, 160), new Rgba(220, 214, 200), new Rgba(244, 240, 230), new Rgba(255, 255, 255) }), 0.2f);
                ArtKit.Ball(c, 9f, 13f, 1.6f, 1.6f, ArtKit.Plain(new[] { new Rgba(20, 60, 40), new Rgba(40, 120, 80), new Rgba(60, 180, 110), new Rgba(150, 255, 190) }), 0.2f);
                c.Set(9, 13, Ink);
            }
            else if (kind == 1)
            {
                for (int y = 8; y < 20; y += 3) c.Set(4 + y % 5, y, Mercury[3]);
            }
            else
            {
                ArtKit.Limb(c, 8f, 18f, 8f, 13f, 1.6f, Copper, 0.2f);
                for (int i = 0; i < 4; i++) ArtKit.Limb(c, 6.5f + i, 12f, 5.5f + i * 1.5f, 8f, 0.6f, Copper, 0.2f);
            }

            // A tampa e o rotulo.
            c.Fill(1, 3, 14, 3, Brass[1]);
            c.Fill(1, 3, 14, 1, Brass[3]);
            c.Fill(4, 15, 8, 3, new Rgba(214, 200, 160));
            c.Fill(5, 16, 5, 1, new Rgba(90, 70, 60));
            c.Set(3, 8, GlassShine);
            c.Set(3, 9, GlassShine);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Estante de tubos de ensaio, 22x16: cinco tubos de cores, um quebrado.</summary>
        public static PixelCanvas TubeRack()
        {
            var c = new PixelCanvas(22, 16);

            Rgba[] colors = { OxideGreen, Azure, Amber, Rose, Mercury[2] };
            for (int i = 0; i < 5; i++)
            {
                int x = 2 + i * 4;
                int top = i == 3 ? 7 : 1;
                Tube(c, x, top, x + 2, 13, colors[i]);
                if (i == 3) { c.Set(x, 6, GlassRim); c.Set(x + 2, 5, GlassRim); }
            }

            c.Fill(0, 9, 22, 2, Walnut[2]);
            c.Fill(0, 9, 22, 1, Walnut[3]);
            c.Fill(0, 14, 22, 2, Walnut[1]);
            c.Fill(1, 11, 1, 3, Walnut[1]);
            c.Fill(20, 11, 1, 3, Walnut[0]);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>
        /// Mesa de dissecacao, 40x22: tampo de marmore com canaleta, as cintas
        /// abertas, um braco de cobre desmontado e as ferramentas numa bandeja.
        /// </summary>
        public static PixelCanvas DissectionTable()
        {
            var c = new PixelCanvas(40, 22);

            // As pernas de ferro.
            c.Fill(4, 12, 3, 10, Iron[1]);
            c.Fill(33, 12, 3, 10, Iron[0]);
            c.Fill(4, 18, 32, 1, Iron[2]);

            // O tampo.
            c.Fill(1, 8, 38, 4, Marble[2]);
            c.Fill(1, 8, 38, 1, Marble[3]);
            c.Fill(1, 11, 38, 1, Marble[0]);
            c.Fill(3, 9, 34, 1, Marble[1]);

            // As cintas abertas, penduradas.
            for (int x = 10; x <= 28; x += 9)
            {
                c.Fill(x, 9, 3, 6, ArtKit.Leather[2]);
                c.Fill(x, 14, 3, 1, Brass[2]);
            }

            // O braco de cobre e o que escorreu.
            ArtKit.Limb(c, 9f, 6f, 20f, 5f, 1.8f, ArtKit.Corroded(Copper, 0.2f, 4121), 0.2f);
            ArtKit.Limb(c, 20f, 5f, 26f, 7f, 1.4f, Copper, 0.2f);
            ArtKit.Ball(c, 27f, 7f, 1.8f, 1.6f, ArtKit.Plain(Copper), 0.2f);
            c.Set(18, 12, OxideGreen);
            c.Set(18, 13, OxideDark);

            // A bandeja.
            c.Fill(30, 6, 7, 2, Silver[2]);
            c.Fill(31, 5, 1, 1, Silver[3]);
            c.Fill(33, 4, 1, 2, Silver[3]);
            c.Fill(35, 5, 1, 1, Silver[1]);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Estante de reagentes, 32x42: tres prateleiras de frascos, um caido e quebrado.</summary>
        public static PixelCanvas ReagentShelf()
        {
            var c = new PixelCanvas(32, 42);

            c.Fill(1, 1, 30, 41, Walnut[1]);
            c.Fill(1, 1, 30, 2, Walnut[3]);
            c.Fill(1, 1, 2, 41, Walnut[2]);
            c.Fill(29, 1, 2, 41, Walnut[0]);
            c.Fill(3, 3, 26, 37, new Rgba(30, 22, 22));

            Rgba[] colors = { OxideGreen, Azure, Amber, Rose, Mercury[2], new Rgba(180, 120, 240) };

            for (int shelf = 0; shelf < 3; shelf++)
            {
                int floor = 14 + shelf * 12;
                c.Fill(3, floor, 26, 2, Walnut[2]);
                c.Fill(3, floor, 26, 1, Walnut[3]);

                for (int i = 0; i < 5; i++)
                {
                    if (shelf == 1 && i == 2) continue;

                    int x = 5 + i * 5;
                    int h = 4 + ArtKit.Hash(i, shelf, 4131) % 4;
                    Rgba col = colors[(i + shelf * 2) % colors.Length];

                    if (ArtKit.Hash(i, shelf, 4132) % 2 == 0) Bulb(c, x + 1.5f, floor - 2.5f, 2f, 2.5f, col, 0.6f);
                    else Tube(c, x, floor - h, x + 2, floor - 1, col);

                    c.Set(x + 1, floor - h - 1, Walnut[3]);
                }
            }

            // O frasco que caiu da prateleira do meio: a poca no chao.
            c.Fill(15, 40, 6, 1, OxideGreen);
            c.Set(14, 40, OxideDark);
            c.Fill(1, 40, 30, 2, Walnut[0]);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Gaiola de bioterio, 24x30, pendurada num pe. Quebrada: a porta arrancada e as barras tortas.</summary>
        public static PixelCanvas Cage(bool broken)
        {
            var c = new PixelCanvas(24, 30);

            // O pe.
            c.Fill(10, 22, 4, 8, Iron[1]);
            c.Fill(6, 28, 12, 2, Iron[2]);

            // A cupula.
            for (int x = 3; x <= 20; x++)
            {
                int top = 4 + (int)(Math.Pow((x - 11.5f) / 8.5f, 2) * 4f);
                for (int y = top; y <= 20; y++)
                {
                    bool bar = (x - 3) % 3 == 0;
                    bool bent = broken && x >= 12 && x <= 18 && y >= 9 && y <= 17;

                    if (bent)
                    {
                        if ((x + y / 3) % 4 == 0) c.Set(x, y, Iron[3]);
                        continue;
                    }

                    if (bar || y == top || y == 12) c.Set(x, y, Brass[bar ? 2 : 1]);
                }
            }

            c.Fill(2, 20, 20, 2, Brass[1]);
            c.Fill(2, 20, 20, 1, Brass[3]);
            c.Fill(10, 1, 4, 3, Brass[2]);

            if (!broken)
            {
                // Uma coisinha de cobre la dentro, encolhida.
                ArtKit.Ball(c, 9f, 17f, 3f, 2.5f, ArtKit.Corroded(Copper, 0.3f, 4141), 0.2f);
                c.Set(8, 16, OxideGreen);
            }
            else
            {
                c.Fill(20, 24, 4, 1, Brass[1]);
                c.Fill(21, 25, 3, 1, Brass[0]);
            }

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>
        /// Tanque de cultura, 30x54: cilindro de vidro com tampa e base de
        /// latao, liquido verde, as bolhas e a cobaia de cobre dentro. 0
        /// encolhida, 1 de pe (com o tubo na nuca), 2 o tanque rachado e vazio.
        /// </summary>
        public static PixelCanvas CultureTank(int kind)
        {
            var c = new PixelCanvas(30, 54);

            bool empty = kind == 2;
            var fluid = new Rgba(54, 150, 106, 220);

            // O vidro e o liquido.
            for (int y = 9; y < 46; y++)
            {
                for (int x = 3; x < 27; x++)
                {
                    bool edge = x == 3 || x == 26;
                    float level = empty ? 40f : 13f;

                    if (edge) c.Set(x, y, GlassRim.WithAlpha(255));
                    else if (y >= level) c.Set(x, y, x > 20 ? fluid.Scaled(0.8f).WithAlpha(230) : fluid);
                    else c.Blend(x, y, GlassFill);
                }
            }

            if (!empty)
            {
                // A cobaia.
                if (kind == 0)
                {
                    ArtKit.Ball(c, 15f, 34f, 6f, 6f, ArtKit.Corroded(Copper, 0.2f, 4151), 0.1f);
                    ArtKit.Ball(c, 12f, 27f, 3.6f, 3.4f, ArtKit.Plain(Copper), 0.2f);
                    ArtKit.Limb(c, 18f, 38f, 13f, 41f, 1.4f, Copper, 0.1f);
                    c.Set(11, 27, OxideGreen);
                }
                else
                {
                    ArtKit.Limb(c, 15f, 22f, 15f, 36f, 3.2f, ArtKit.Corroded(Copper, 0.2f, 4152), 0.1f);
                    ArtKit.Ball(c, 15f, 19f, 3.4f, 3.6f, ArtKit.Plain(Copper), 0.2f);
                    ArtKit.Limb(c, 12f, 24f, 10f, 33f, 1.2f, Copper, 0.1f);
                    ArtKit.Limb(c, 18f, 24f, 20f, 33f, 1.2f, Copper, 0.1f);
                    ArtKit.Limb(c, 14f, 36f, 13f, 44f, 1.3f, Copper, 0f);
                    ArtKit.Limb(c, 16f, 36f, 17f, 44f, 1.3f, Copper, 0f);
                    c.Set(14, 19, OxideGreen);
                    c.Set(16, 19, OxideGreen);
                    for (int y = 9; y < 17; y++) c.Set(15, y, Iron[2]);
                }

                // As bolhas.
                for (int i = 0; i < 6; i++)
                {
                    int bx = 6 + ArtKit.Hash(i, kind, 4153) % 17;
                    int by = 15 + ArtKit.Hash(i, kind, 4154) % 26;
                    c.Set(bx, by, new Rgba(180, 255, 210, 220));
                }
            }
            else
            {
                // O rombo, os cacos e a poca verde no pe.
                for (int y = 18; y < 36; y++)
                {
                    int w = 6 - Math.Abs(y - 27) / 2;
                    for (int x = 15 - w; x < 15 + w; x++) c.Set(x, y, Rgba.Clear);
                }

                ArtKit.Line(c, 9, 14, 13, 19, GlassRim);
                ArtKit.Line(c, 21, 15, 17, 20, GlassRim);
                c.Fill(4, 44, 22, 1, OxideGreen);
            }

            c.Fill(4, 10, 1, 30, GlassShine);

            // Tampa, base e os canos.
            c.Fill(1, 4, 28, 5, Brass[1]);
            c.Fill(1, 4, 28, 1, Brass[3]);
            c.Fill(13, 0, 4, 4, Brass[2]);
            c.Fill(1, 46, 28, 8, Brass[1]);
            c.Fill(1, 46, 28, 1, Brass[3]);
            for (int x = 4; x < 28; x += 6) ArtKit.Rivet(c, x, 49, Brass);

            // A plaqueta com o numero.
            c.Fill(11, 50, 8, 3, Silver[2]);
            c.Fill(13, 51, 4, 1, Ink);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>O tanque grande do Grande Laboratorio, 48x80: onde o Mestre entrou. Vazio e rachado em cima.</summary>
        public static PixelCanvas GreatTank()
        {
            var c = new PixelCanvas(48, 80);

            for (int y = 12; y < 66; y++)
            {
                for (int x = 4; x < 44; x++)
                {
                    bool edge = x == 4 || x == 43;
                    if (edge) c.Set(x, y, GlassRim.WithAlpha(255));
                    else if (y > 58) c.Set(x, y, Mercury[x > 34 ? 0 : 1]);
                    else c.Blend(x, y, GlassFill);
                }
            }

            // As marcas de dedo prateadas por dentro, escorrendo.
            for (int i = 0; i < 5; i++)
            {
                int x = 10 + i * 6;
                int top = 26 + ArtKit.Hash(i, 0, 4161) % 14;
                for (int y = top; y < top + 8 + i; y++) c.Set(x + (y % 3 == 0 ? 1 : 0), y, Mercury[2]);
            }

            ArtKit.Crack(c, 20f, 14f, 60f, 18, 4162, 0f);
            c.Fill(6, 14, 1, 40, GlassShine);

            c.Fill(0, 4, 48, 8, Silver[1]);
            c.Fill(0, 4, 48, 2, Silver[3]);
            c.Fill(18, 0, 12, 4, Silver[2]);
            c.Fill(0, 66, 48, 14, Silver[1]);
            c.Fill(0, 66, 48, 2, Silver[3]);
            for (int x = 4; x < 46; x += 6) ArtKit.Rivet(c, x, 71, Silver);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Lustre de cristal, 36x26, pendurado do teto pelo alto (velas apagadas, gotas de vidro).</summary>
        public static PixelCanvas Chandelier()
        {
            var c = new PixelCanvas(36, 26);

            c.Fill(17, 0, 2, 8, Silver[1]);
            ArtKit.Ball(c, 18f, 10f, 4f, 3f, ArtKit.Plain(Silver), 0.3f);

            // Os bracos e as velas.
            for (int i = -2; i <= 2; i++)
            {
                float x = 18f + i * 7f;
                ArtKit.Limb(c, 18f, 12f, x, 15f - Math.Abs(i), 0.8f, Silver, 0.2f);
                c.Fill((int)x - 1, 14 - Math.Abs(i) - 3, 3, 3, new Rgba(236, 226, 200));
                c.Set((int)x, 10 - Math.Abs(i), new Rgba(255, 220, 140));
            }

            // As gotas de cristal.
            for (int i = -3; i <= 3; i++)
            {
                int x = 18 + i * 5;
                int len = 4 + (3 - Math.Abs(i));
                for (int y = 16; y < 16 + len; y++) c.Blend(x, y, GlassRim);
                c.Set(x, 16 + len, GlassShine);
            }

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Canos de laboratorio no teto, 48x18 (pivo no alto): latao, vidro com liquido e um registro.</summary>
        public static PixelCanvas Pipes()
        {
            var c = new PixelCanvas(48, 18);

            ArtKit.Limb(c, 0f, 4f, 48f, 4f, 2.2f, Brass, 0.1f);
            Tube(c, 0, 9, 47, 12, Azure);
            for (int x = 6; x < 48; x += 12)
            {
                c.Fill(x, 0, 2, 13, Iron[1]);
                c.Fill(x - 1, 2, 4, 2, Iron[2]);
            }

            // O registro e o cotovelo descendo.
            ArtKit.Ball(c, 30f, 4f, 3f, 3f, ArtKit.Plain(Copper), 0.2f);
            c.Fill(29, 0, 3, 1, Copper[3]);
            ArtKit.Limb(c, 40f, 12f, 40f, 17f, 1.5f, Brass, 0.1f);

            ArtKit.AddOutline(c);
            return c;
        }

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
