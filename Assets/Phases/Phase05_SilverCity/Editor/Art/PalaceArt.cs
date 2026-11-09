using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O palacio do Rei de Prata, peca por peca, a 16 px por tile: estandartes
    /// de veludo com o 925, espelhos do chao ao teto, as estatuas dos Nobres,
    /// candelabros, o tapete vermelho, o trono (inteiro e partido), a Coluna do
    /// Coracao, o encaixe do nucleo e os cacos de espelho que viram escudo na
    /// segunda fase da luta.
    ///
    /// Desenhado aqui (y cresce pra baixo), sem UnityEngine.
    /// </summary>
    public static class PalaceArt
    {
        private static readonly Rgba[] Silver = LabArt.Silver;
        private static readonly Rgba[] Velvet = LabArt.Velvet;
        private static readonly Rgba[] Gold = ArtKit.Brass;
        private static readonly Rgba[] Marble = LabArt.Marble;
        private static readonly Rgba Ink = ArtKit.Outline;

        private static readonly Rgba[] MirrorGlass =
            { new Rgba(96, 118, 150), new Rgba(146, 170, 200), new Rgba(196, 214, 236), new Rgba(244, 250, 255) };

        private static readonly Rgba[] Quartz =
            { new Rgba(120, 140, 170), new Rgba(178, 196, 222), new Rgba(222, 234, 248), new Rgba(255, 255, 255) };

        /// <summary>Os sprites de cenario: nome, desenho, se e pendurado, e se se repete (Tiled).</summary>
        public static IEnumerable<(string Name, Func<PixelCanvas> Draw, bool Hanging, bool Tiled)> Sprites()
        {
            yield return ("palace_banner", Banner, false, false);
            yield return ("palace_candelabra", Candelabra, false, false);
            yield return ("palace_statue", Statue, false, false);
            yield return ("palace_mirror", () => Mirror(false), false, false);
            yield return ("palace_mirror_broken", () => Mirror(true), false, false);
            yield return ("palace_throne", () => Throne(false), false, false);
            yield return ("palace_throne_broken", () => Throne(true), false, false);
            yield return ("palace_conduit", () => Conduit(false), false, false);
            yield return ("palace_conduit_clean", () => Conduit(true), false, false);
            yield return ("palace_core_socket", CoreSocket, false, false);
            yield return ("palace_mirror_shard", MirrorShard, false, false);
            yield return ("palace_carpet", Carpet, false, true);
            yield return ("palace_crown", FallenCrown, false, false);
        }

        /// <summary>Estandarte de veludo da Coroa, 26x58: a coroa e o 925 bordados a ouro, franja embaixo.</summary>
        public static PixelCanvas Banner()
        {
            var c = new PixelCanvas(26, 58);

            c.Fill(0, 0, 26, 3, Gold[2]);
            c.Fill(0, 0, 26, 1, Gold[3]);
            ArtKit.Ball(c, 1f, 1.5f, 2f, 2f, ArtKit.Plain(Gold), 0.3f);
            ArtKit.Ball(c, 25f, 1.5f, 2f, 2f, ArtKit.Plain(Gold), 0.3f);

            for (int y = 3; y < 52; y++)
            {
                int tail = y > 44 ? (y - 44) : 0;
                for (int x = 3 + tail / 2; x < 23 - tail / 2; x++)
                {
                    bool fold = (x - 3) % 7 == 0;
                    c.Set(x, y, fold ? Velvet[0] : ArtKit.Tone(Velvet, 0.4f - (x - 3) / 22f));
                }
            }

            // A borda de ouro e o bordado.
            for (int y = 3; y < 46; y++) { c.Set(4, y, Gold[2]); c.Set(21, y, Gold[1]); }
            ArtKit.Polygon(c, new[] { (8f, 18f), (9f, 12f), (11f, 16f), (13f, 10f), (15f, 16f), (17f, 12f), (18f, 18f) }, (x, y) => Gold[3]);
            c.Fill(8, 18, 11, 2, Gold[2]);
            PixelFont.DrawCentered(c, "925", 13, 24, Gold[3]);
            for (int x = 6; x < 20; x += 3) c.Set(x, 34, Gold[2]);

            // A franja.
            for (int x = 8; x < 18; x += 2)
            {
                for (int y = 50; y < 56; y++) c.Set(x, y, Gold[y % 2 == 0 ? 2 : 1]);
            }

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Candelabro de prata de cinco velas, 22x36, acesas.</summary>
        public static PixelCanvas Candelabra()
        {
            var c = new PixelCanvas(22, 36);

            c.Fill(7, 32, 8, 4, Silver[1]);
            c.Fill(6, 34, 10, 2, Silver[2]);
            ArtKit.Limb(c, 11f, 32f, 11f, 14f, 1.4f, Silver, 0.2f);
            ArtKit.Ball(c, 11f, 22f, 2.4f, 2f, ArtKit.Plain(Silver), 0.3f);

            for (int i = -2; i <= 2; i++)
            {
                float x = 11f + i * 4f;
                float y = 14f - (2 - Math.Abs(i));
                ArtKit.Limb(c, 11f, 16f, x, y, 0.8f, Silver, 0.2f);
                c.Fill((int)x - 1, (int)y - 5, 3, 5, new Rgba(240, 232, 210));
                c.Set((int)x, (int)y - 6, new Rgba(255, 220, 130));
                c.Set((int)x, (int)y - 7, new Rgba(255, 250, 210));
                ArtKit.Glow(c, x, y - 7f, 3f, new Rgba(255, 230, 160), 0.6f, true);
            }

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Estatua de um dos 925 Nobres, 34x60: prata polida, mao no peito, num pedestal de marmore.</summary>
        public static PixelCanvas Statue()
        {
            var c = new PixelCanvas(34, 60);

            // O corpo: um manto longo.
            ArtKit.Polygon(c, new[] { (11f, 18f), (23f, 18f), (27f, 56f), (7f, 56f) }, (x, y) => ArtKit.Tone(Silver, 0.5f - (x - 7f) / 20f));
            for (int y = 24; y < 56; y += 5) ArtKit.Line(c, 13, y, 21, y + 2, Silver[1]);

            // A cabeca, a gola e a mao no peito.
            ArtKit.Ball(c, 17f, 12f, 5f, 6f, ArtKit.Plain(Silver), 0.3f);
            ArtKit.Polygon(c, new[] { (10f, 19f), (13f, 15f), (17f, 18f), (21f, 15f), (24f, 19f) }, (x, y) => Silver[3]);
            ArtKit.Ball(c, 19f, 26f, 2.4f, 2f, ArtKit.Plain(Silver), 0.4f);
            c.Set(15, 11, Silver[0]);
            c.Set(19, 11, Silver[0]);

            // A pequena coroa de nobre.
            for (int x = 13; x <= 21; x += 2) c.Fill(x, 5, 1, 2, Gold[3]);
            c.Fill(13, 7, 9, 1, Gold[2]);

            // O pedestal.
            c.Fill(4, 56, 26, 4, Marble[2]);
            c.Fill(4, 56, 26, 1, Marble[3]);

            ArtKit.AddOutline(c);
            ArtKit.Spark(c, 11, 22, 1);
            return c;
        }

        /// <summary>Espelho de chao ao teto, 46x92, de moldura de prata. Partido: cacos faltando e o fundo escuro.</summary>
        public static PixelCanvas Mirror(bool broken)
        {
            var c = new PixelCanvas(46, 92);

            for (int y = 0; y < 92; y++)
            {
                for (int x = 0; x < 46; x++)
                {
                    int edge = Math.Min(Math.Min(x, 45 - x), Math.Min(y, 91 - y));
                    float arch = y < 16 ? (float)Math.Sqrt(Math.Max(0f, 23f * 23f - (16 - y) * (16 - y) * 2.2f)) : 23f;
                    if (Math.Abs(x - 22.5f) > arch) continue;

                    if (edge < 4 || Math.Abs(x - 22.5f) > arch - 4) c.Set(x, y, ArtKit.Tone(Silver, (x < 23 ? 0.6f : -0.1f) + (y % 6 == 0 ? 0.3f : 0f)));
                    else
                    {
                        // O reflexo: o salao invertido, claro em cima, escuro embaixo, com faixas diagonais de brilho.
                        float light = 0.6f - y / 92f * 1.2f + ((x + y / 2) % 13 < 2 ? 0.7f : 0f);
                        c.Set(x, y, ArtKit.Tone(MirrorGlass, light));
                    }
                }
            }

            if (broken)
            {
                for (int i = 0; i < 6; i++)
                {
                    float cx = 10f + ArtKit.Hash(i, 0, 8101) % 26;
                    float cy = 20f + ArtKit.Hash(i, 0, 8102) % 60;
                    ArtKit.Polygon(c, new[] { (cx, cy - 7f), (cx + 6f, cy + 2f), (cx - 3f, cy + 6f) }, (x, y) => new Rgba(20, 22, 32));
                }

                ArtKit.Crack(c, 22f, 40f, 70f, 30, 8103, 0f);
                ArtKit.Crack(c, 22f, 40f, 200f, 24, 8104, 0f);
            }

            // O pe de garra.
            c.Fill(6, 88, 34, 4, Silver[1]);
            c.Fill(6, 88, 34, 1, Silver[3]);
            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>
        /// O trono, 168x184: prata 925, espaldar altissimo em leque de lancas,
        /// almofada de veludo, bracos com garras, e embaixo dele a boca da
        /// Coluna do Coracao, de onde sobe o brilho. Partido: o espaldar rachado
        /// ao meio, lancas caidas, o verde escorrendo.
        /// </summary>
        public static PixelCanvas Throne(bool broken)
        {
            var c = new PixelCanvas(168, 184);
            const int cx = 84;

            // O espaldar em leque.
            for (int i = -6; i <= 6; i++)
            {
                if (broken && (i == 2 || i == -4 || i == 5)) continue;

                float angle = -90f + i * 9f;
                float length = 150f - Math.Abs(i) * 9f;
                (float tx, float ty) = ArtKit.Along(cx, 150f, angle, length);
                ArtKit.Taper(c, cx, 150f, tx, ty, 7f, 1.5f, ArtKit.Plain(Silver), 0.1f + (i < 0 ? 0.2f : -0.1f));
                ArtKit.Ball(c, tx, ty, 2.6f, 2.6f, ArtKit.Plain(Gold), 0.3f);
            }

            // O encosto e o assento.
            ArtKit.Polygon(c, new[] { (cx - 40f, 150f), (cx - 30f, 40f), (cx + 30f, 40f), (cx + 40f, 150f) }, (x, y) => ArtKit.Tone(Silver, 0.4f - (x - cx + 40f) / 80f));
            ArtKit.Polygon(c, new[] { (cx - 26f, 140f), (cx - 20f, 56f), (cx + 20f, 56f), (cx + 26f, 140f) }, (x, y) => ArtKit.Tone(Velvet, 0.3f - (x - cx + 26f) / 52f));
            for (int y = 64; y < 136; y += 12) for (int x = cx - 18; x <= cx + 18; x += 12) ArtKit.Rivet(c, x, y, Gold);
            PixelFont.DrawCentered(c, "925", cx, 46, Gold[3], Gold[0], PixelFont.Style.Embossed, 2);

            c.Fill(cx - 48, 140, 96, 12, Silver[2]);
            c.Fill(cx - 48, 140, 96, 2, Silver[3]);

            // Os bracos, com a garra na ponta.
            foreach (int side in new[] { -1, 1 })
            {
                float ax = cx + side * 46f;
                ArtKit.Limb(c, ax, 104f, ax, 150f, 6f, Silver, side < 0 ? 0.3f : -0.2f);
                ArtKit.Ball(c, ax, 102f, 8f, 6f, ArtKit.Plain(Gold), 0.3f);
            }

            // A base em degraus e a boca da Coluna, com o brilho.
            for (int step = 0; step < 3; step++)
            {
                int w = 120 + step * 16;
                c.Fill(cx - w / 2, 152 + step * 10, w, 10, Marble[2 - step % 2]);
                c.Fill(cx - w / 2, 152 + step * 10, w, 1, Marble[3]);
            }

            Rgba heart = broken ? LabArt.OxideGreen : new Rgba(220, 240, 255);
            ArtKit.Ball(c, cx, 168f, 14f, 8f, ArtKit.Plain(new[] { heart.Scaled(0.4f), heart.Scaled(0.7f), heart, new Rgba(255, 255, 255) }), 0.4f);
            ArtKit.Glow(c, cx, 168f, 26f, heart, 0.7f, true);

            if (broken)
            {
                ArtKit.Crack(c, cx, 44f, 95f, 90, 8111, 1f);
                ArtKit.Crack(c, cx - 20f, 120f, 30f, 40, 8112, 1f);
                for (int y = 150; y < 180; y++) if (y % 3 == 0) c.Set(cx - 30 + (y * 7) % 60, y, LabArt.OxideGreen);
            }

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>
        /// A Coluna do Coracao no Altar, 44x224: um veio de quartzo do teto ao
        /// chao, com o verde do Oxide pulsando por dentro (limpo, so luz branca).
        /// </summary>
        public static PixelCanvas Conduit(bool clean)
        {
            var c = new PixelCanvas(44, 224);

            for (int y = 0; y < 224; y++)
            {
                float half = 12f + (float)Math.Sin(y * 0.07f) * 3f + (y > 200 ? (y - 200) * 0.5f : 0f);
                for (int x = (int)(22 - half); x < (int)(22 + half); x++)
                {
                    float dx = (x - 22f) / half;
                    float facet = (x + y / 3) % 9 < 1 ? 0.5f : 0f;
                    c.Set(x, y, ArtKit.Tone(Quartz, 0.3f - dx * 0.6f + facet));
                }
            }

            // O veio de dentro.
            Rgba vein = clean ? new Rgba(250, 252, 255) : LabArt.OxideGreen;
            for (int y = 0; y < 224; y++)
            {
                int x = 22 + (int)(Math.Sin(y * 0.15f) * 4f);
                c.Set(x, y, vein);
                c.Set(x + 1, y, vein.Scaled(0.8f));
            }

            ArtKit.Glow(c, 22f, 112f, 40f, vein, 0.35f, true);

            // O anel de prata no chao, de onde ela sai.
            c.Fill(4, 210, 36, 6, Silver[2]);
            c.Fill(4, 210, 36, 1, Silver[3]);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>O encaixe do nucleo, 34x34: um console de prata com uma concha redonda, vazia, esperando.</summary>
        public static PixelCanvas CoreSocket()
        {
            var c = new PixelCanvas(34, 34);

            ArtKit.Polygon(c, new[] { (6f, 33f), (9f, 14f), (25f, 14f), (28f, 33f) }, (x, y) => ArtKit.Tone(Silver, 0.4f - (x - 6f) / 22f));
            ArtKit.Ball(c, 17f, 12f, 11f, 8f, ArtKit.Plain(Silver), 0.2f);
            ArtKit.Ball(c, 17f, 12f, 7f, 5f, ArtKit.Plain(new[] { new Rgba(20, 22, 34), new Rgba(34, 38, 56), new Rgba(60, 66, 90), new Rgba(110, 120, 150) }), -0.4f);
            ArtKit.Glow(c, 17f, 12f, 8f, new Rgba(220, 240, 255), 0.5f, true);
            for (int x = 9; x < 26; x += 4) ArtKit.Rivet(c, x, 22, Gold);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Um caco de espelho grande cravado no chao, 30x46: o escudo contra o sopro do Rei.</summary>
        public static PixelCanvas MirrorShard()
        {
            var c = new PixelCanvas(30, 46);
            ArtKit.Polygon(c, new[] { (4f, 46f), (2f, 18f), (12f, 2f), (26f, 12f), (28f, 46f) }, (x, y) =>
                ArtKit.Tone(MirrorGlass, 0.5f - y / 46f + ((int)(x + y / 2) % 11 < 2 ? 0.7f : 0f)));
            for (int y = 10; y < 44; y++) c.Set(4 + (y % 5 == 0 ? 1 : 0), y, Silver[3]);
            ArtKit.Crack(c, 15f, 20f, 100f, 16, 8121, 0f);
            ArtKit.AddOutline(c);
            ArtKit.Spark(c, 12, 10, 2);
            return c;
        }

        /// <summary>Um pedaco do tapete vermelho, 16x5, emendavel deitado.</summary>
        public static PixelCanvas Carpet()
        {
            var c = new PixelCanvas(16, 5);
            for (int x = 0; x < 16; x++)
            {
                c.Set(x, 0, Gold[2]);
                for (int y = 1; y < 4; y++) c.Set(x, y, Velvet[(x + y) % 5 == 0 ? 1 : 2]);
                c.Set(x, 4, Gold[1]);
            }

            return c;
        }

        /// <summary>A coroa de quartzo caida, 28x18: rachada ao meio, as facetas ainda brilhando.</summary>
        public static PixelCanvas FallenCrown()
        {
            var c = new PixelCanvas(28, 18);
            ArtKit.Polygon(c, new[] { (2f, 16f), (3f, 7f), (8f, 11f), (12f, 3f), (16f, 11f), (21f, 5f), (26f, 12f), (26f, 16f) }, (x, y) =>
                ArtKit.Tone(Quartz, 0.5f - (x - 2f) / 26f + ((int)x % 4 == 0 ? 0.4f : 0f)));
            c.Fill(2, 14, 25, 3, Gold[2]);
            ArtKit.Line(c, 14, 3, 13, 16, Ink);
            ArtKit.AddOutline(c);
            ArtKit.Spark(c, 8, 8, 1);
            return c;
        }
    }
}
