using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O que se le no District 2, o de latao: aqui ficam a lista e os desenhos
    /// do mapa. Os desenhos de leitura estao em Docs/LoreDocs.Phase02.cs.
    /// </summary>
    public static partial class LoreArt
    {
        private static Dictionary<string, Piece> _phase02;

        private static readonly Rgba BlueInk = new Rgba(44, 50, 86);

        /// <summary>As do District 2, pelo nome do LoreSpec.</summary>
        public static IReadOnlyDictionary<string, Piece> Phase02 => _phase02 ?? (_phase02 = BuildPhase02());

        private static readonly Rgba OldPaint = new Rgba(196, 150, 60);
        private static readonly Rgba FreshPaint = new Rgba(176, 40, 34);
        private static readonly Rgba TinScratch = new Rgba(232, 232, 226);

        private static Dictionary<string, Piece> BuildPhase02()
        {
            return Index(new[]
            {
                // --- O livro de turnos: quatro folhas do mesmo caderno ---
                Make("Lore_Logbook_1", "logbook_page", LogbookMap, 0f, false, LogbookDoc(1, "noite", new[]
                {
                    "A pressão subiu de novo. Abri a válvula de alívio duas vezes, e ela voltou a subir.",
                    "O vapor das caldeiras de baixo tá saindo esverdeado. Avisei lá em cima. Disseram que é impressão minha.",
                }, 0f, 1201)),
                Make("Lore_Logbook_2", "logbook_page", LogbookMap, 0f, false, LogbookDoc(2, "manhã", new[]
                {
                    "A água tá com gosto de metal podre. Troquei o cano e não adiantou: o problema vem de baixo, do fundo da rocha.",
                    "Quem pega turno perto das caldeiras sai tossindo verde.",
                }, 0.25f, 1202)),
                Make("Lore_Logbook_3", "logbook_page", LogbookMap, 0f, false, LogbookDoc(3, "noite", new[]
                {
                    "Três colegas não voltaram do turno. Fui atrás deles no corredor de baixo, onde o vapor é mais grosso.",
                    "Achei só as ferramentas. Estavam cobertas de Óxido.",
                }, 0.6f, 1203)),
                Make("Lore_Logbook_4", "logbook_page", LogbookMap, 0f, false, LogbookDoc(4, "", new[]
                {
                    "Quem trabalha de máscara de solda tá bem. Nenhuma mancha. O vapor verde não passa pelo filtro.",
                    "As máscaras protegem do Óxido. Vou contar pro chefe.",
                }, 0.9f, 1204)),

                // --- Os papeis da Coroa ---
                Make("Lore_InspectionReport", "noble_paper", () => NoblePaperMap(false), 0f, false, InspectionReportDoc()),
                Make("Lore_DepotSheet", "noble_paper_pinned", () => NoblePaperMap(true), 1.3f, true, DepotSheetDoc()),

                // --- Placas ---
                Make("Lore_SewerPlaque", "plaque_corroded", () => PlaqueMap(true), 0f, false, SewerPlaqueDoc()),
                Make("Lore_TowerLine", "plaque_corroded", () => PlaqueMap(true), 1.9f, true, TowerLineDoc()),
                Make("Lore_MasterPipe", "plaque_corroded", () => PlaqueMap(true), 1.9f, true, MasterPipeDoc()),
                Make("Lore_GalleryView", "letter", LetterMap, 1.2f, true, GalleryViewDoc()),
                Make("Lore_ElevatorSign", "plaque_corroded", () => PlaqueMap(true), 1.9f, true, ElevatorSignDoc()),

                // --- Riscado na chapa ---
                Make("Lore_Graffiti_1", "scratches_metal", ScratchesMetalMap, 1.3f, true, HastyScratchDoc()),
                Make("Lore_Graffiti_3", "scratches_metal", ScratchesMetalMap, 1.3f, true,
                    VentWallDoc(false), VentWallDoc(true)),
                Make("Lore_BoilerDoor", "scratches_metal", ScratchesMetalMap, 1.3f, true, BoilerDoorDoc()),

                // --- Giz e tinta nas paredes de ferro ---
                Make("Lore_Graffiti_2", "chalk_mask", ChalkMaskMap, 1.3f, true, ChalkMaskDoc()),
                Make("Lore_SewerTally", "chalk_tally", ChalkTallyMap, 1.3f, true, ChalkTallyDoc()),
                Make("Lore_ClimbArrow", "chalk_arrow", ChalkArrowMap, 1.3f, true, ChalkArrowDoc()),
                Make("Lore_TunnelArrow", "paint_arrow", PaintArrowMap, 1.3f, true, PaintArrowDoc()),

                // --- O que so existe uma vez ---
                Make("Lore_CrownPoster", "crown_poster", () => DecorArt.All()[DecorArt.PosterCrown], 1.3f, true, CrownPosterDoc()),
                Make("Lore_Letter", "letter", LetterMap, 1.2f, true, LetterDoc()),
                Make("Lore_ChiefDiary", "diary", DiaryMap, 0f, false, ChiefDiaryDoc()),
                Make("Lore_LakeMask", "welding_mask", WeldingMaskMap, 1.0f, true, WeldingMaskDoc()),
            });
        }

        // ------------------------------------------------------------------
        // O livro de turnos
        // ------------------------------------------------------------------


        private static PixelCanvas LogbookMap()
        {
            var c = new PixelCanvas(12, 7);
            c.Fill(1, 2, 10, 5, new Rgba(224, 218, 196));
            for (int x = 2; x < 10; x++)
            {
                c.Set(x, 4, new Rgba(120, 140, 190));
                if (x % 3 != 0) c.Set(x, 3, BlueInk);
            }

            c.Set(3, 2, new Rgba(190, 70, 64));
            c.Set(3, 5, new Rgba(190, 70, 64));
            c.Set(1, 3, Rgba.Clear);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // A placa do elevador (a placa de sempre, com um aviso pregado)
        // ------------------------------------------------------------------


        // ------------------------------------------------------------------
        // Riscado na chapa
        // ------------------------------------------------------------------





        /// <summary>No mapa: riscos claros de prego, sem a chapa (a parede do cenario e a chapa).</summary>
        private static PixelCanvas ScratchesMetalMap()
        {
            var c = new PixelCanvas(32, 16);
            Rgba mark = new Rgba(236, 232, 214, 220);
            int[] starts = { 2, 4, 1 };
            int[] ends = { 28, 24, 30 };
            for (int row = 0; row < starts.Length; row++)
            {
                int y = 3 + row * 4;
                for (int x = starts[row]; x < ends[row]; x++)
                {
                    if (ArtKit.Hash(x, y, 1331) % 4 == 0) continue;
                    c.Set(x, y + (ArtKit.Hash(x, y, 1332) % 4 == 0 ? 1 : 0), mark);
                }
            }

            return c;
        }

        // ------------------------------------------------------------------
        // Giz e tinta nas paredes de ferro
        // ------------------------------------------------------------------



        /// <summary>Uma mascara de soldador desenhada a giz: o casco, o visor hachurado, as presilhas.</summary>
        private static void ChalkWeldingMask(PixelCanvas c, int cx, int cy, float scale)
        {
            int w = (int)(28 * scale);
            int h = (int)(34 * scale);

            // O casco: um retangulo de cantos redondos, num traco continuo.
            int steps = Math.Max(24, (int)(400 * scale));
            for (int i = 0; i < steps; i++)
            {
                double a = i * Math.PI * 2.0 / steps;
                double sx = Math.Cos(a);
                double sy = Math.Sin(a);
                double k = Math.Pow(Math.Abs(sx), 0.35) * Math.Sign(sx);
                double l = Math.Pow(Math.Abs(sy), 0.35) * Math.Sign(sy);
                int px = cx + (int)Math.Round(k * w);
                int py = cy + (int)Math.Round(l * h);
                c.Set(px, py, Chalk);
                if (scale >= 0.5f) c.Set(px + 1, py, Chalk.WithAlpha(200));
            }

            // O queixo e a testa, marcados.
            if (scale >= 0.5f)
            {
                ArtKit.Line(c, cx - w / 2, cy + h - 6, cx + w / 2, cy + h - 6, Chalk.WithAlpha(170));
                ArtKit.Line(c, cx - w / 2, cy - h + 6, cx + w / 2, cy - h + 6, Chalk.WithAlpha(170));
            }

            // O visor, hachurado.
            int vx = cx - (int)(16 * scale);
            int vy = cy - (int)(8 * scale);
            int vw = (int)(32 * scale);
            int vh = (int)(10 * scale);
            Box(c, vx, vy, vw, vh, Chalk);
            for (int x = vx + 2; x < vx + vw - 1; x += 3) ArtKit.Line(c, x, vy + vh - 2, x + 2, vy + 1, Chalk.WithAlpha(180));

            // As presilhas dos lados.
            foreach (int side in new[] { -1, 1 })
            {
                int px = cx + side * (w + 3);
                for (int i = 0; i < 12; i++)
                {
                    double a = i * Math.PI / 6.0;
                    c.Set(px + (int)Math.Round(Math.Cos(a) * 3), cy + (int)Math.Round(Math.Sin(a) * 3), Chalk);
                }
            }
        }

        private static PixelCanvas ChalkMaskMap()
        {
            var c = new PixelCanvas(14, 14);
            ChalkWeldingMask(c, 7, 7, 0.17f);
            c.Fill(3, 5, 8, 3, Chalk.WithAlpha(200));
            return c;
        }


        private static PixelCanvas ChalkTallyMap()
        {
            var c = new PixelCanvas(26, 14);
            for (int g = 0; g < 4; g++)
            {
                for (int row = 0; row < 2; row++)
                {
                    int x = 1 + g * 6;
                    int y = 2 + row * 6;
                    for (int i = 0; i < 4; i++) ArtKit.Line(c, x + i, y, x + i, y + 4, Chalk.WithAlpha(210));
                    ArtKit.Line(c, x - 1, y + 3, x + 4, y + 1, Chalk.WithAlpha(210));
                }
            }

            return c;
        }


        private static PixelCanvas ChalkArrowMap()
        {
            var c = new PixelCanvas(10, 22);
            ArtKit.Line(c, 5, 21, 5, 3, Chalk);
            ArtKit.Line(c, 5, 1, 1, 6, Chalk);
            ArtKit.Line(c, 5, 1, 9, 6, Chalk);
            return c;
        }


        /// <summary>Tinta escorrendo de uma faixa pra baixo: fios de comprimento aleatorio.</summary>
        private static void Drips(PixelCanvas c, int x0, int x1, int y, Rgba paint, int seed, int maxLength)
        {
            for (int x = x0; x < x1; x++)
            {
                if (ArtKit.Hash(x, 0, seed) % 5 != 0) continue;
                int length = 2 + ArtKit.Hash(x, 1, seed) % maxLength;
                for (int i = 0; i < length; i++) c.Set(x, y + i, paint);
                c.Set(x, y + length, paint.Scaled(0.8f));
                c.Set(x + 1, y + length, paint.Scaled(0.8f));
            }
        }

        private static PixelCanvas PaintArrowMap()
        {
            var c = new PixelCanvas(26, 12);
            c.Fill(1, 6, 18, 2, OldPaint);
            for (int i = 0; i < 4; i++) ArtKit.Line(c, 19 + i, 7 - (3 - i), 19 + i, 7 + (3 - i), OldPaint);
            for (int x = 2; x < 14; x++) if (x % 3 != 0) c.Set(x, 2, OldPaint);
            ArtKit.Line(c, 2, 3, 14, 1, FreshPaint);
            c.Set(5, 8, OldPaint);
            c.Set(5, 9, OldPaint);
            c.Set(12, 8, OldPaint);
            return c;
        }

        // ------------------------------------------------------------------
        // O que so existe uma vez
        // ------------------------------------------------------------------



        private static PixelCanvas LetterMap()
        {
            var c = new PixelCanvas(10, 10);
            c.Fill(1, 1, 8, 8, new Rgba(222, 210, 178));
            for (int i = 1; i < 9; i++)
            {
                c.Set(5, i, new Rgba(178, 160, 118));
                c.Set(i, 5, new Rgba(178, 160, 118));
            }

            c.Set(4, 2, ArtKit.Copper[3]);
            c.Set(2, 7, new Rgba(70, 50, 40));
            c.Set(3, 7, new Rgba(70, 50, 40));
            ArtKit.AddOutline(c, Ink);
            return c;
        }


        private static PixelCanvas DiaryMap()
        {
            var c = new PixelCanvas(14, 6);
            c.Fill(1, 2, 12, 4, ArtKit.Leather[1]);
            c.Fill(2, 2, 4, 3, new Rgba(226, 216, 188));
            c.Fill(8, 2, 4, 3, new Rgba(226, 216, 188));
            c.Set(9, 3, new Rgba(60, 36, 26));
            c.Set(10, 3, new Rgba(60, 36, 26));
            c.Set(3, 3, new Rgba(52, 40, 36));
            c.Set(4, 3, new Rgba(52, 40, 36));
            ArtKit.AddOutline(c, Ink);
            return c;
        }


        private static PixelCanvas WeldingMaskMap()
        {
            var c = new PixelCanvas(12, 12);
            c.Fill(2, 1, 8, 10, ArtKit.Rubber[1]);
            c.Fill(3, 4, 6, 2, new Rgba(40, 80, 64));
            c.Fill(3, 8, 6, 1, ArtKit.Brass[2]);
            for (int y = 1; y < 11; y++) c.Set(6 + (y % 3 == 0 ? 1 : 0), y, Rgba.Clear);
            ArtKit.AddOutline(c, Ink);
            return c;
        }
    }
}
