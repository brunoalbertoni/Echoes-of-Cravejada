using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O que se le no palacio. As placas sao a mesma placa de prata dos
    /// Ateliers; o decreto, o mesmo papel dos 925; o retrato e o do Rei jovem,
    /// inteiro, numa moldura maior; a almofada da coroa e a coroa caida tem
    /// desenho proprio.
    /// </summary>
    public static partial class LoreArt
    {
        private static Dictionary<string, Piece> _phase05;

        public static IReadOnlyDictionary<string, Piece> Phase05 => _phase05 ?? (_phase05 = BuildPhase05());

        /// <summary>A coroa caida, que so aparece com o Rei derrotado: o gerador liga ela na mao.</summary>
        public const string FallenCrown = "Lore_FallenCrown";

        private static Dictionary<string, Piece> BuildPhase05()
        {
            return Index(new[]
            {
                Make("Lore_GuardOath", "silver_sign", SilverSignMap, 2.2f, true, SilverPlaqueView(
                    new[] { "NENHUMA MANCHA PASSARÁ", "", "NENHUMA FERRUGEM", "NENHUM COBRE", "NENHUM VERDE", "", "NÓS SOMOS O ESPELHO", "DE SUA MAJESTADE" },
                    null, 5401)),
                Make("Lore_KingPortrait", "king_portrait", KingPortraitMap, 2.8f, true, KingPortraitView()),
                Make("Lore_CrownCushion", "crown_cushion", CushionMap, 0f, false, CushionView()),
                Make("Lore_LastDecree", "noble_paper_pinned", () => NoblePaperMap(true), 1.6f, true, NobleDocument(
                    new[] { "ÚLTIMO DECRETO DE", "SUA MAJESTADE" },
                    new[]
                    {
                        "A CIDADE NÃO PODE ESCURECER.",
                        "Que o trono beba do Coração até a última gota. Que ninguém desligue a Coluna. Nem eu.",
                    },
                    null, new[] { new Note("a assinatura escorre verde ate a borda", new Rgba(70, 170, 110), default, 30, 5402) }, 5403, true)),
                Make("Lore_ConduitPlaque", "silver_sign", SilverSignMap, 2.0f, true, SilverPlaqueView(
                    new[] { "COLUNA DO CORAÇÃO", "", "LUZ SEM FIM", "PARA SUA MAJESTADE" },
                    "e para mais ninguém", 5404)),
                Make(FallenCrown, "fallen_crown", PalaceArt.FallenCrown, 0f, false, FallenCrownView()),
            });
        }

        /// <summary>O retrato do Rei jovem, de perto: inteiro, de prata limpa, a coroa acesa. Uma mancha verde no canto.</summary>
        private static PixelCanvas KingPortraitView()
        {
            const int width = 170;
            const int height = 214;
            var c = new PixelCanvas(width, height);

            for (int y = 0; y < height - 28; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int edge = Math.Min(Math.Min(x, width - 1 - x), Math.Min(y, height - 29 - y));
                    if (edge < 12) c.Set(x, y, ArtKit.Tone(ArtKit.Brass, (x < y ? 0.6f : -0.1f) + (edge % 4 == 0 ? 0.3f : 0f)));
                    else c.Set(x, y, PaperTone(new Rgba(60, 46, 70), new Rgba(36, 28, 44), x, y, 5411));
                }
            }

            // O busto: manto, arminho, o rosto de prata limpa e a coroa.
            ArtKit.Ball(c, 85f, 186f, 62f, 40f, ArtKit.Plain(LabArt.Velvet), 0.1f);
            for (int x = 30; x < 140; x++) for (int y = 150; y < 160; y++) c.Set(x, y, (x * 7 + y * 3) % 19 == 0 ? new Rgba(30, 28, 34) : new Rgba(240, 238, 230));
            ArtKit.Drum(c, 85f, 104f, 30f, 36f, 2.4f, ArtKit.Plain(LabArt.Silver), 0.3f);
            c.Fill(64, 98, 16, 3, LabArt.Silver[0]);
            c.Fill(90, 98, 16, 3, LabArt.Silver[0]);
            ArtKit.Ball(c, 72f, 104f, 4f, 3f, ArtKit.Plain(new[] { new Rgba(180, 190, 210), new Rgba(220, 230, 245), new Rgba(250, 252, 255), new Rgba(255, 255, 255) }), 0.5f);
            ArtKit.Ball(c, 98f, 104f, 4f, 3f, ArtKit.Plain(new[] { new Rgba(180, 190, 210), new Rgba(220, 230, 245), new Rgba(250, 252, 255), new Rgba(255, 255, 255) }), 0.5f);
            c.Fill(76, 124, 18, 2, LabArt.Silver[0]);

            for (int i = -4; i <= 4; i++)
            {
                float px = 85f + i * 7f;
                float h = 26f - Math.Abs(i) * 3f;
                ArtKit.Polygon(c, new[] { (px - 4f, 70f), (px, 70f - h), (px + 4f, 70f) }, (x, y) => ((int)y % 4 == 0) ? new Rgba(255, 255, 255) : new Rgba(200, 220, 245));
            }

            c.Fill(52, 68, 66, 8, ArtKit.Brass[2]);
            ArtKit.Glow(c, 85f, 56f, 40f, new Rgba(255, 246, 210), 0.4f, false);

            // A manchinha verde, como uma assinatura.
            ArtKit.Ball(c, 140f, 168f, 4f, 3f, ArtKit.Plain(new[] { new Rgba(30, 110, 70), new Rgba(70, 170, 110), new Rgba(120, 255, 170), new Rgba(200, 255, 220) }), 0.2f);

            Plate(c, 14, height - 26, width - 28, 24, ArtKit.Brass, 0f, 5412);
            PixelFont.DrawCentered(c, "SUA MAJESTADE", width / 2, height - 22, ArtKit.Brass[0], ArtKit.Brass[3], PixelFont.Style.Engraved);
            PixelFont.DrawCentered(c, "NO DIA DA LUZ SEM FIM", width / 2, height - 12, ArtKit.Brass[0], ArtKit.Brass[3], PixelFont.Style.Engraved);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        private static PixelCanvas KingPortraitMap()
        {
            var c = new PixelCanvas(30, 38);
            c.Fill(0, 0, 30, 38, ArtKit.Brass[2]);
            c.Fill(3, 3, 24, 32, new Rgba(50, 38, 60));
            ArtKit.Ball(c, 15f, 32f, 10f, 6f, ArtKit.Plain(LabArt.Velvet), 0.1f);
            ArtKit.Ball(c, 15f, 18f, 6f, 7f, ArtKit.Plain(LabArt.Silver), 0.3f);
            for (int x = 10; x <= 20; x += 2) c.Set(x, 9, new Rgba(230, 240, 255));
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>A almofada de veludo vazia, de perto: a marca funda da coroa e a plaqueta com o nome raspado e reescrito.</summary>
        private static PixelCanvas CushionView()
        {
            const int width = 210;
            const int height = 150;
            var c = new PixelCanvas(width, height);

            // O pedestal de prata.
            c.Fill(60, 96, 90, 50, LabArt.Silver[1]);
            c.Fill(60, 96, 90, 3, LabArt.Silver[3]);

            // A almofada, com borlas de ouro nos cantos.
            ArtKit.Drum(c, 105f, 80f, 70f, 22f, 4f, ArtKit.Plain(LabArt.Velvet), 0.2f);
            foreach (float bx in new[] { 38f, 172f }) ArtKit.Ball(c, bx, 92f, 5f, 7f, ArtKit.Plain(ArtKit.Brass), 0.3f);

            // A marca funda, oval, onde a coroa pesou anos.
            ArtKit.Ball(c, 105f, 72f, 40f, 9f, ArtKit.Plain(new[] { new Rgba(40, 10, 18), new Rgba(70, 18, 30), new Rgba(100, 30, 44), new Rgba(130, 50, 64) }), -0.5f);

            Plate(c, 20, 112, 170, 30, LabArt.Silver, 0f, 5421);
            PixelFont.DrawCentered(c, "925 FACETAS. UMA PARA CADA NOBRE.", 105, 118, LabArt.Silver[0], LabArt.Silver[3], PixelFont.Style.Engraved);
            PixelFont.Draw(c, "lapidada por", 46, 130, LabArt.Silver[0], LabArt.Silver[3], PixelFont.Style.Engraved);
            PixelFont.Draw(c, "Crisol", 120, 130, PenInk, default, PixelFont.Style.Scratched, 1, 5422);
            for (int x = 116; x < 160; x++) c.Blend(x, 133, LabArt.Silver[0].WithAlpha(120));

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        private static PixelCanvas CushionMap()
        {
            var c = new PixelCanvas(24, 22);
            c.Fill(7, 12, 10, 10, LabArt.Silver[1]);
            c.Fill(7, 12, 10, 1, LabArt.Silver[3]);
            ArtKit.Drum(c, 12f, 9f, 10f, 4f, 4f, ArtKit.Plain(LabArt.Velvet), 0.2f);
            ArtKit.Ball(c, 12f, 8f, 5f, 1.6f, ArtKit.Plain(new[] { new Rgba(40, 10, 18), new Rgba(70, 18, 30), new Rgba(100, 30, 44), new Rgba(130, 50, 64) }), -0.5f);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>A coroa caida, de perto: rachada ao meio, novecentas e vinte e cinco facetas, um rosto riscado em cada uma.</summary>
        private static PixelCanvas FallenCrownView()
        {
            const int width = 220;
            const int height = 140;
            var c = new PixelCanvas(width, height);

            for (int i = -6; i <= 6; i++)
            {
                if (i == 1) continue;
                float px = 110f + i * 14f;
                float h = 70f - Math.Abs(i) * 7f;
                ArtKit.Polygon(c, new[] { (px - 7f, 104f), (px, 104f - h), (px + 7f, 104f) }, (x, y) =>
                {
                    bool facet = ((int)x + (int)y * 2) % 9 == 0;
                    return facet ? new Rgba(255, 255, 255) : ArtKit.Tone(new[] { new Rgba(130, 150, 186), new Rgba(186, 206, 232), new Rgba(230, 240, 252), new Rgba(255, 255, 255) }, 0.5f - (x - px + 7f) / 14f);
                });
            }

            c.Fill(16, 100, 188, 18, ArtKit.Brass[2]);
            c.Fill(16, 100, 188, 3, ArtKit.Brass[3]);
            for (int x = 24; x < 200; x += 16) ArtKit.Ball(c, x, 109f, 4f, 4f, ArtKit.Plain(new[] { new Rgba(130, 150, 186), new Rgba(186, 206, 232), new Rgba(230, 240, 252), new Rgba(255, 255, 255) }), 0.4f);

            // A rachadura, de cima a baixo.
            ArtKit.Line(c, 124, 30, 120, 118, Ink);
            ArtKit.Line(c, 125, 30, 121, 118, new Rgba(60, 64, 80));

            // Os rostos riscados nas facetas (um em cada uma que da pra ver).
            for (int i = 0; i < 12; i++)
            {
                int fx = 40 + ArtKit.Hash(i, 0, 5431) % 140;
                int fy = 60 + ArtKit.Hash(i, 1, 5431) % 34;
                c.Set(fx, fy, Ink);
                c.Set(fx + 2, fy, Ink);
                c.Set(fx + 1, fy + 2, Ink);
                ArtKit.Line(c, fx - 1, fy - 1, fx + 3, fy + 3, new Rgba(60, 170, 110));
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }
    }
}
