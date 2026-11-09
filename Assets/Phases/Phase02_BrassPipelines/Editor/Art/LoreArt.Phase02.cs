using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O que se le no District 2, o de latao. Os papeis da Coroa e as placas
    /// usam os mesmos modelos do District 1 (NobleDocument, BrassPlaque); as
    /// quatro paginas do livro de turnos sao a mesma pagina; o resto e riscado
    /// na chapa, escrito a giz ou pintado nas paredes de ferro.
    /// </summary>
    public static partial class LoreArt
    {
        private static Dictionary<string, Piece> _phase02;

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
                Make("Lore_Logbook_1", "logbook_page", LogbookMap, 0f, false, LogbookPage("NOITE", new[]
                {
                    "Pressão acima do normal de novo. Abri a válvula de alívio duas vezes, e a pressão voltou a subir.",
                    "Avisei lá em cima. Disseram que é o tempo. Aqui embaixo nem existe tempo.",
                }, false, 1201)),
                Make("Lore_Logbook_2", "logbook_page", LogbookMap, 0f, false, LogbookPage("MANHÃ", new[]
                {
                    "A água está com gosto de cobre. Não é do cano: troquei o cano.",
                    "O manômetro três não sai do vermelho. Parei de olhar para ele.",
                }, false, 1202)),
                Make("Lore_Logbook_3", "logbook_page", LogbookMap, 0f, false, LogbookPage("NOITE", new[]
                {
                    "Três colegas não voltaram do turno. Fui procurá-los no corredor de baixo.",
                    "Achei as ferramentas deles. Só as ferramentas. Estavam verdes.",
                }, true, 1203)),
                Make("Lore_Logbook_4", "logbook_page", LogbookMap, 0f, false, LogbookPage("", new[]
                {
                    "As máscaras funcionam.",
                }, true, 1204, lastPage: true)),

                // --- Os papeis da Coroa: o mesmo documento do decreto do District 1 ---
                Make("Lore_InspectionReport", "noble_paper", () => NoblePaperMap(false), 0f, false, NobleDocument(
                    new[] { "RELATÓRIO DE INSPEÇÃO", "CALDEIRAS DO DISTRITO DE LATÃO" },
                    new[]
                    {
                        "PRESSÃO ACIMA DO LIMITE EM TODAS AS LINHAS.",
                        "RECOMENDO DESLIGAR METADE DAS CALDEIRAS.",
                    },
                    new[] { "NEGADO.", "A CIDADE NÃO PODE ESCURECER.", "— O REI DE PRATA." },
                    new[] { Hand("então, que escureça", Pencil, 0, 1211), Hand("aqui embaixo.", Pencil, 10, 1212) },
                    1213, false)),
                Make("Lore_DepotSheet", "noble_paper_pinned", () => NoblePaperMap(true), 1.3f, true, NobleDocument(
                    new[] { "FICHA DE INVENTÁRIO", "ORDEM DA COROA" },
                    new[]
                    {
                        "RECOLHER TODAS AS MÁSCARAS DE SOLDA E ENVIÁ-LAS AO DISTRITO DE PRATA.",
                        "PRIORIDADE: NOBRES.",
                    },
                    null,
                    new[]
                    {
                        Hand("recolhidas: nenhuma.", PenInk, 0, 1221),
                        Hand("não há máscaras neste depósito.", PenInk, 0, 1222),
                        Hand("— o chefe de manutenção.", PenInk, 30, 1223),
                    },
                    1224, true)),

                // --- As placas de latao: a mesma placa do District 1 ---
                Make("Lore_SewerPlaque", "plaque_corroded", () => PlaqueMap(true), 0f, false, BrassPlaque(
                    new[] { "COLETOR DE CONDENSADO Nº 4", "NÃO ENTRAR SEM MÁSCARA." },
                    new[]
                    {
                        Nail("TRAVA   MOLA   GAXETA", 0, 1231),
                        Nail("BUCHA   PISTÃO   CALDO", 4, 1232),
                        Nail("FUMO   REBITE-9   ARRUELA", 2, 1233),
                        Nail("NÍVEL   SOLDINHA   FERR", 6, 1234),
                    },
                    0.36f, 1235, mud: true)),
                Make("Lore_TowerLine", "plaque_corroded", () => PlaqueMap(true), 1.9f, true, BrassPlaque(
                    new[] { "LINHA 3 — AVENIDA DAS FACETAS", "PRIORIDADE: NOBRES." },
                    new[]
                    {
                        Hand("e a nossa?", Chalk, 0, 1241),
                        Hand("E A NOSSA?", Chalk, 62, 1242),
                        Hand("e a nossa?", Chalk, 120, 1243),
                    },
                    0.3f, 1244)),
                Make("Lore_MasterPipe", "plaque_corroded", () => PlaqueMap(true), 1.9f, true, BrassPlaque(
                    new[] { "PARA A CIDADE", "DE PRATA." },
                    new[] { Nail("nada mais sobe.", 0, 1251) },
                    0.5f, 1252, castScale: 2)),
                Make("Lore_GalleryView", "plaque_corroded", () => PlaqueMap(true), 1.9f, true, BrassPlaque(
                    new[] { "GALERIA ALTA", "ACESSO RESTRITO AOS", "INSPETORES DA COROA." },
                    new[] { Nail("agora é de quem subir.", 0, 1261) },
                    0.14f, 1262)),
                Make("Lore_ElevatorSign", "plaque_corroded", () => PlaqueMap(true), 1.9f, true, ElevatorSignView()),

                // --- Riscado na chapa ---
                Make("Lore_Graffiti_1", "scratches_metal", ScratchesMetalMap, 1.3f, true, HastyScratchView()),
                Make("Lore_Graffiti_3", "scratches_metal", ScratchesMetalMap, 1.3f, true,
                    VentWallView(false), VentWallView(true)),
                Make("Lore_BoilerDoor", "scratches_metal", ScratchesMetalMap, 1.3f, true, BoilerDoorView()),

                // --- Giz e tinta nas paredes de ferro ---
                Make("Lore_Graffiti_2", "chalk_mask", ChalkMaskMap, 1.3f, true, ChalkMaskView()),
                Make("Lore_SewerTally", "chalk_tally", ChalkTallyMap, 1.3f, true, ChalkTallyView()),
                Make("Lore_ClimbArrow", "chalk_arrow", ChalkArrowMap, 1.3f, true, ChalkArrowView()),
                Make("Lore_TunnelArrow", "paint_arrow", PaintArrowMap, 1.3f, true, PaintArrowView()),

                // --- O que so existe uma vez ---
                Make("Lore_CrownPoster", "crown_poster", () => DecorArt.All()[DecorArt.PosterCrown], 1.3f, true, CrownPosterView()),
                Make("Lore_Letter", "letter", LetterMap, 1.2f, true, LetterView()),
                Make("Lore_ChiefDiary", "diary", DiaryMap, 0f, false, DiaryView()),
                Make("Lore_LakeMask", "welding_mask", WeldingMaskMap, 1.0f, true, WeldingMaskView()),
            });
        }

        // ------------------------------------------------------------------
        // O livro de turnos
        // ------------------------------------------------------------------

        /// <summary>
        /// Uma folha arrancada do livro de turnos da caldeira: pautada, com a
        /// margem vermelha, o cabecalho impresso e a letra do encanador. A
        /// beirada da esquerda ficou rasgada onde a folha saiu do caderno.
        /// </summary>
        private static PixelCanvas LogbookPage(string shift, string[] entries, bool shaky, int seed, bool lastPage = false)
        {
            const int width = 204;
            const int height = 200;
            var c = new PixelCanvas(width, height);
            var paper = new Rgba(224, 218, 196);
            var paperDark = new Rgba(184, 172, 140);

            for (int y = 4; y < height - 4; y++)
            {
                // A beirada arrancada do caderno, com os furos da costura.
                int tear = 4 + (int)(ArtKit.Noise(y * 0.5f, 0f, seed) * 6f);
                for (int x = tear; x < width - 4; x++) c.Set(x, y, PaperTone(paper, paperDark, x, y, seed + 1));
            }

            for (int y = 24; y < height; y += 40) ClearDisc(c, 14f, y, 2.5f);

            // As pautas azuis e a margem vermelha.
            for (int y = 46; y < height - 8; y += 12)
            {
                for (int x = 8; x < width - 6; x++) if (c.Get(x, y).A > 0) c.Blend(x, y, new Rgba(120, 140, 190, 120));
            }

            for (int y = 6; y < height - 6; y++) if (c.Get(26, y).A > 0) c.Blend(26, y, new Rgba(190, 70, 64, 150));

            // O cabecalho impresso, igual em toda folha.
            PixelFont.DrawCentered(c, "LIVRO DE TURNOS DA CALDEIRA", 112, 10, PenInk);
            for (int x = 34; x < 192; x++) c.Blend(x, 21, PenInk.WithAlpha(160));
            PixelFont.Draw(c, "TURNO:", 32, 27, PenInk);
            if (shift.Length > 0) PixelFont.Draw(c, shift, 72, 27, BlueInk, default, PixelFont.Style.Scratched, 1, seed + 2);

            // A letra do encanador, nas pautas. Tremida quando ele tinha medo.
            PixelFont.Style style = shaky ? PixelFont.Style.Scratched : PixelFont.Style.Ink;
            if (lastPage)
            {
                PixelFont.DrawCentered(c, entries[0], 112, 106, BlueInk, default, style, 1, seed + 3);
            }
            else
            {
                int lineY = 37;
                int index = 0;
                foreach (string entry in entries)
                {
                    foreach (string line in PixelFont.Wrap(entry, 166))
                    {
                        PixelFont.Draw(c, line, 32, lineY, BlueInk, default, style, 1, seed + 10 + index++);
                        lineY += 12;
                    }

                    lineY += 12;
                }
            }

            // Uma mancha de agua e uma marca de dedo sujo de graxa.
            ArtKit.Glow(c, 170f, 160f, 18f, new Rgba(170, 156, 120), 0.6f);
            for (int i = 0; i < 18; i++)
            {
                int x = 182 + ArtKit.Hash(i, 0, seed) % 8;
                int y = 176 + ArtKit.Hash(i, 1, seed) % 10;
                if (c.Get(x, y).A > 0) c.Blend(x, y, new Rgba(40, 36, 34, 120));
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

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

        private static PixelCanvas ElevatorSignView()
        {
            PixelCanvas plaque = BrassPlaque(
                new[] { "ELEVADOR DE SERVIÇO", "DO DISTRITO DE LATÃO", "AO DISTRITO DE CRISTAL" },
                new Note[0], 0.2f, 1271);

            var c = new PixelCanvas(Math.Max(plaque.Width, 200), plaque.Height + 74);
            c.Blit(plaque, (c.Width - plaque.Width) / 2, 0);

            // O aviso de papel, pregado as pressas embaixo, meio torto.
            int top = plaque.Height - 6;
            var paper = new Rgba(222, 214, 186);
            for (int y = top; y < top + 62; y++)
            {
                int shift = (y - top) / 20;
                for (int x = 14 + shift; x < c.Width - 14 + shift; x++)
                {
                    // A beirada de baixo rasgada.
                    if (y > top + 54 + (int)(ArtKit.Noise(x * 0.3f, 0f, 1272) * 8f)) continue;
                    c.Set(x, y, PaperTone(paper, new Rgba(180, 168, 132), x, y, 1273));
                }
            }

            ArtKit.Ball(c, c.Width / 2 + 0.5f, top + 4.5f, 2f, 2f, ArtKit.Iron, 0.3f);
            PixelFont.DrawCentered(c, "ESCADARIA INTERDITADA.", c.Width / 2 + 1, top + 12, PenInk);
            PixelFont.DrawCentered(c, "USE O ACESSO DAS CALDEIRAS.", c.Width / 2 + 1, top + 25, PenInk);
            PixelFont.Draw(c, "que acesso?", 34, top + 42, new Rgba(90, 86, 84), default, PixelFont.Style.Scratched, 1, 1274);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // Riscado na chapa
        // ------------------------------------------------------------------

        /// <summary>Uma chapa de ferro galvanizado, com rebites na borda e manchas de ferrugem.</summary>
        private static PixelCanvas TinSheet(int width, int height, int seed)
        {
            var c = new PixelCanvas(width, height);
            Plate(c, 2, 2, width - 4, height - 4, ArtKit.Tin, 0f, seed);

            for (int y = 4; y < height - 4; y++)
            {
                for (int x = 4; x < width - 4; x++)
                {
                    float n = ArtKit.Noise(x * 0.09f, y * 0.09f, seed + 3);
                    if (n > 0.74f) c.Blend(x, y, ArtKit.Rust[n > 0.84f ? 1 : 2].WithAlpha(150));
                }
            }

            for (int x = 14; x < width - 14; x += 18)
            {
                ArtKit.Rivet(c, x, 6, ArtKit.Tin);
                ArtKit.Rivet(c, x, height - 8, ArtKit.Tin);
            }

            return c;
        }

        /// <summary>"A prata bebe, o latao sua": riscado na chapa, as pressas.</summary>
        private static PixelCanvas HastyScratchView()
        {
            string text = "A PRATA BEBE,";
            string text2 = "O LATÃO SUA.";
            PixelCanvas c = TinSheet(220, 84, 1301);
            PixelFont.Draw(c, text, 22, 20, TinScratch, ArtKit.Tin[0], PixelFont.Style.Scratched, 2, 1302);
            PixelFont.Draw(c, text2, 46, 50, TinScratch, ArtKit.Tin[0], PixelFont.Style.Scratched, 2, 1303);

            // A pressa: uns riscos que escaparam da letra.
            ArtKit.Line(c, 180, 30, 196, 40, TinScratch);
            ArtKit.Line(c, 30, 70, 54, 68, TinScratch);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>
        /// La no alto, embaixo do respiro do teto: duas inscricoes de dias
        /// diferentes e, depois da purificacao, uma terceira, a giz.
        /// </summary>
        private static PixelCanvas VentWallView(bool purified)
        {
            PixelCanvas c = TinSheet(232, 150, 1311);

            // O respiro do teto, com a luz fraca passando pelas aletas.
            c.Fill(70, 4, 92, 22, ArtKit.Iron[0]);
            for (int y = 7; y < 24; y += 4) c.Fill(72, y, 88, 2, ArtKit.Iron[2]);
            ArtKit.Glow(c, 116f, 14f, 30f, new Rgba(240, 220, 160), 0.35f);

            PixelFont.DrawCentered(c, "ELES NUNCA DESCERAM ATÉ AQUI.", 116, 40, TinScratch, ArtKit.Tin[0], PixelFont.Style.Scratched, 1, 1312);
            PixelFont.DrawCentered(c, "NEM PARA VER.", 116, 54, TinScratch, ArtKit.Tin[0], PixelFont.Style.Scratched, 1, 1313);

            // A outra mao, de outro dia, menor e mais funda.
            Rgba other = new Rgba(204, 206, 204);
            int y2 = 74;
            foreach (string line in PixelFont.Wrap("DAQUI EU VIA AS LUZES DA AVENIDA DAS FACETAS PELO RESPIRO DO TETO. APAGARAM-SE UMA A UMA.", 196))
            {
                PixelFont.Draw(c, line, 16, y2, other, ArtKit.Tin[0], PixelFont.Style.Scratched, 1, 1314 + y2);
                y2 += 12;
            }

            if (purified)
            {
                PixelFont.Draw(c, "um desceu. era de sucata.", 30, y2 + 8, Chalk, default, PixelFont.Style.Scratched, 1, 1319);
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>A porta da sala das caldeiras: latao, a tranca soldada por fora e os recados.</summary>
        private static PixelCanvas BoilerDoorView()
        {
            const int width = 220;
            const int height = 212;
            var c = new PixelCanvas(width, height);
            Plate(c, 2, 2, width - 4, height - 4, ArtKit.Brass, 0.2f, 1321);

            // As almofadas da porta, rebaixadas.
            foreach ((int x, int y) in new[] { (20, 18), (116, 18), (20, 132), (116, 132) })
            {
                Box(c, x, y, 84, 62, ArtKit.Brass[0]);
                Box(c, x + 1, y + 1, 82, 60, ArtKit.Brass[3]);
            }

            // As dobradicas.
            foreach (int y in new[] { 30, 170 }) c.Fill(2, y, 8, 14, ArtKit.Iron[2]);

            // A tranca: uma barra de ferro atravessada e soldada nas duas pontas.
            c.Fill(4, 92, width - 8, 14, ArtKit.Iron[1]);
            c.Fill(4, 92, width - 8, 2, ArtKit.Iron[3]);
            c.Fill(4, 104, width - 8, 2, ArtKit.Iron[0]);
            foreach (int x in new[] { 10, width - 22 })
            {
                for (int i = 0; i < 4; i++) ArtKit.Ball(c, x + 3 + i * 3, 99f, 2.4f, 4f, ArtKit.Tin, 0.4f);
            }

            Rgba scratch = NailScratch;
            Rgba shade = ArtKit.Brass[0];
            PixelFont.Draw(c, "NÃO ABRAM.", 28, 32, scratch, shade, PixelFont.Style.Scratched, 1, 1322);
            PixelFont.Draw(c, "OUÇAM O QUE", 124, 30, scratch, shade, PixelFont.Style.Scratched, 1, 1323);
            PixelFont.Draw(c, "OUVIREM.", 130, 44, scratch, shade, PixelFont.Style.Scratched, 1, 1324);
            PixelFont.Draw(c, "ELE PEDIU.", 46, 60, NailFresh, shade, PixelFont.Style.Scratched, 1, 1325);

            // Riscado muitas vezes no mesmo lugar: mais fundo, mais largo.
            for (int pass = 0; pass < 3; pass++)
            {
                PixelFont.Draw(c, "desculpe, chefe.", 46 + pass % 2, 150 + pass / 2, ArtKit.Brass[3], ArtKit.Brass[0],
                    PixelFont.Style.Scratched, 1, 1326);
            }

            PixelFont.Draw(c, "desculpe, chefe.", 46, 150, NailFresh, ArtKit.Outline, PixelFont.Style.Scratched, 1, 1326);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

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

        /// <summary>Um pedaco de parede de chapas de ferro escuro, rebitadas, encardidas.</summary>
        private static PixelCanvas IronWall(int width, int height, int seed, bool damp = false)
        {
            var c = new PixelCanvas(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int border = Math.Min(Math.Min(x, width - 1 - x), Math.Min(y, height - 1 - y));
                    if (border + 1 < ArtKit.Noise(x * 0.15f, y * 0.15f, seed) * 5f) continue;

                    float light = (ArtKit.Noise(x * 0.04f, y * 0.04f, seed + 1) - 0.5f) * 1.2f - 0.25f;
                    Rgba color = ArtKit.Tone(ArtKit.Iron, light);

                    // As juntas das chapas.
                    if (x % 64 == 0 || y % 48 == 0) color = ArtKit.Iron[0];
                    else if (x % 64 == 1 || y % 48 == 1) color = ArtKit.Iron[2];

                    if (damp && ArtKit.Noise(x * 0.3f, y * 0.03f, seed + 2) > 0.72f)
                    {
                        color = Rgba.Lerp(color, ArtKit.Verdigris[0], 0.5f);
                    }

                    c.Set(x, y, color);
                }
            }

            for (int y = 6; y < height; y += 48)
            {
                for (int x = 6; x < width - 4; x += 10) ArtKit.Rivet(c, x, y, ArtKit.Iron);
            }

            return c;
        }

        /// <summary>O desenho a giz da mascara de soldador, do lado do cabide, e as duas frases embaixo.</summary>
        private static PixelCanvas ChalkMaskView()
        {
            PixelCanvas c = IronWall(220, 168, 1401);

            // O cabide: um gancho de ferro preso na parede.
            ArtKit.Limb(c, 186f, 20f, 186f, 40f, 1.4f, ArtKit.Iron, 0.4f);
            ArtKit.Limb(c, 186f, 40f, 194f, 36f, 1.4f, ArtKit.Iron, 0.4f);
            c.Fill(182, 16, 9, 5, ArtKit.Tin[2]);

            ChalkWeldingMask(c, 92, 50, 1f);

            PixelFont.DrawCentered(c, "QUEM TEM MÁSCARA TEM CARA.", 110, 112, Chalk, default, PixelFont.Style.Scratched, 1, 1402);
            PixelFont.Draw(c, "e quem não tem?", 70, 136, Chalk.Scaled(0.85f), default, PixelFont.Style.Scratched, 1, 1403);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

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

        /// <summary>Os dias sem subir: dezenas de grupos de cinco riscos, e o ultimo com tres.</summary>
        private static PixelCanvas ChalkTallyView()
        {
            PixelCanvas c = IronWall(236, 164, 1411, damp: true);

            PixelFont.DrawCentered(c, "DIAS SEM SUBIR.", 118, 14, Chalk, default, PixelFont.Style.Scratched, 2, 1412);

            int groups = 0;
            for (int row = 0; row < 5; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    int x = 18 + col * 26;
                    int y = 50 + row * 22;
                    bool last = row == 4 && col == 7;

                    int strokes = last ? 3 : 4;
                    for (int i = 0; i < strokes; i++)
                    {
                        int lean = ArtKit.Hash(groups, i, 1413) % 2;
                        ArtKit.Line(c, x + i * 3, y, x + i * 3 + lean, y + 12, Chalk.WithAlpha(220));
                    }

                    if (!last) ArtKit.Line(c, x - 2, y + 10, x + 11, y + 2, Chalk.WithAlpha(220));
                    groups++;
                }
            }

            ArtKit.AddOutline(c, Ink);
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

        /// <summary>A seta a giz subindo pela parede: "elevador", e o recado de outra mao.</summary>
        private static PixelCanvas ChalkArrowView()
        {
            PixelCanvas c = IronWall(200, 200, 1421);

            // A seta: a haste dupla subindo e a ponta.
            for (int x = 46; x < 50; x++) ArtKit.Line(c, x, 190, x + (x % 2), 30, Chalk);
            for (int i = 0; i < 4; i++)
            {
                ArtKit.Line(c, 48, 14 + i, 26 + i, 42, Chalk);
                ArtKit.Line(c, 48, 14 + i, 70 - i, 42, Chalk);
            }

            PixelFont.Draw(c, "ELEVADOR", 76, 40, Chalk, default, PixelFont.Style.Scratched, 2, 1422);
            PixelFont.Draw(c, "a escada caiu.", 76, 120, Chalk.Scaled(0.85f), default, PixelFont.Style.Scratched, 1, 1423);
            PixelFont.Draw(c, "subam por aqui.", 76, 136, Chalk.Scaled(0.85f), default, PixelFont.Style.Scratched, 1, 1424);

            ArtKit.AddOutline(c, Ink);
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

        /// <summary>
        /// A seta pintada as pressas: "deposito — mascaras", com "mascaras"
        /// riscado de tinta fresca e "nada" por cima, escorrendo.
        /// </summary>
        private static PixelCanvas PaintArrowView()
        {
            PixelCanvas c = IronWall(250, 150, 1431, damp: true);

            // A seta pra leste, de pincel grosso, escorrida: a ponta afina pra direita.
            c.Fill(20, 100, 170, 8, OldPaint);
            for (int i = 0; i < 22; i++)
            {
                int half = 15 - i * 15 / 22;
                ArtKit.Line(c, 190 + i, 104 - half, 190 + i, 104 + half, OldPaint);
            }

            Drips(c, 20, 190, 108, OldPaint, 1432, 9);

            PixelFont.Draw(c, "DEPÓSITO —", 20, 28, OldPaint, default, PixelFont.Style.Ink, 2, 1433);
            int start = 20;
            int words = PixelFont.Draw(c, "MÁSCARAS", start, 62, OldPaint, default, PixelFont.Style.Ink, 2, 1434);
            Drips(c, start, start + words, 76, OldPaint, 1435, 6);

            // Riscado com tinta fresca, e "NADA" por cima, escorrendo antes de secar.
            for (int t = 0; t < 3; t++) ArtKit.Line(c, start - 4, 71 + t, start + words + 6, 67 + t, FreshPaint);
            PixelFont.Draw(c, "NADA", 150, 52, FreshPaint, default, PixelFont.Style.Scratched, 2, 1436);
            Drips(c, 150, 196, 66, FreshPaint, 1437, 24);

            ArtKit.AddOutline(c, Ink);
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

        /// <summary>
        /// O cartaz da Coroa, o mesmo da decoracao (azul de tinta, coroa e 925
        /// de prata), grande: o lema e, na tira clara de baixo, o carvao.
        /// </summary>
        private static PixelCanvas CrownPosterView()
        {
            const int width = 168;
            const int height = 224;
            var c = new PixelCanvas(width, height);
            var paper = new Rgba(70, 70, 96);
            var paperDark = new Rgba(46, 46, 68);
            var strip = new Rgba(214, 204, 176);

            for (int y = 2; y < height - 2; y++)
            {
                for (int x = 2; x < width - 2; x++)
                {
                    // Os cantos rasgados e a borda comida.
                    if (x + y < 14 - ArtKit.Hash(x, y, 1501) % 4) continue;
                    if ((width - x) + (height - y) < 22 + ArtKit.Hash(x, y, 1502) % 4) continue;

                    bool bottom = y > height - 46;
                    Rgba color = bottom ? PaperTone(strip, new Rgba(170, 156, 120), x, y, 1503)
                        : ((x + y) % 11 == 0 ? paperDark : paper);
                    if (!bottom && ArtKit.Noise(x * 0.06f, y * 0.06f, 1504) > 0.78f) color = Rgba.Lerp(color, ArtKit.Verdigris[1], 0.5f);
                    c.Set(x, y, color);
                }
            }

            // A borda de prata.
            Box(c, 8, 8, width - 16, height - 58, Silver[1]);

            // A coroa: tres pontas afinando sobre a faixa, uma bolinha em cada
            // ponta e as pedras azuis na faixa.
            var gem = new Rgba(120, 180, 230);
            foreach (int tip in new[] { 54, 84, 114 })
            {
                int top = tip == 84 ? 20 : 26;
                for (int y = top; y < 48; y++)
                {
                    int half = 1 + (y - top) * 11 / (48 - top);
                    for (int x = tip - half; x <= tip + half; x++) c.Set(x, y, x < tip ? Silver[3] : Silver[2]);
                }

                ArtKit.Ball(c, tip + 0.5f, top - 2.5f, 3f, 3f, Silver, 0.3f);
            }

            c.Fill(42, 46, 85, 14, Silver[2]);
            c.Fill(42, 46, 85, 2, Silver[3]);
            c.Fill(42, 58, 85, 3, Silver[1]);
            foreach (int x in new[] { 52, 82, 112 }) c.Fill(x, 50, 5, 5, gem);

            PixelFont.DrawCentered(c, "925", width / 2, 72, Silver[3], Silver[0], PixelFont.Style.Embossed, 3);

            PixelFont.DrawCentered(c, "O VAPOR É O SANGUE", width / 2, 112, Silver[2]);
            PixelFont.DrawCentered(c, "DA CIDADE.", width / 2, 124, Silver[2]);
            PixelFont.DrawCentered(c, "CADA OPERÁRIO,", width / 2, 142, Silver[3]);
            PixelFont.DrawCentered(c, "UMA GOTA.", width / 2, 154, Silver[3]);

            PixelFont.Draw(c, "e eles bebem.", 30, height - 30, Charcoal, default, PixelFont.Style.Scratched, 1, 1505);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>A carta dobrada em quatro, presa com um rebite: as dobras e a letra de quem escreveu.</summary>
        private static PixelCanvas LetterView()
        {
            const int width = 200;
            string[] paragraphs =
            {
                "O vapor está mais pesado esta semana. Não venha me visitar no turno. Fique no distrito de cobre, perto das janelas.",
                "Se mandarem todo mundo descer pela calha, não tenha medo. Eu desço os canos atrás de você. Prometo.",
            };

            var wrapped = new List<List<string>>();
            int lines = 0;
            foreach (string paragraph in paragraphs)
            {
                List<string> l = PixelFont.Wrap(paragraph, 164);
                wrapped.Add(l);
                lines += l.Count;
            }

            int height = 46 + lines * 13 + wrapped.Count * 8 + 20;
            var c = new PixelCanvas(width, height);
            var paper = new Rgba(222, 210, 178);
            var paperDark = new Rgba(178, 160, 118);

            for (int y = 3; y < height - 3; y++)
            {
                for (int x = 3; x < width - 3; x++) c.Set(x, y, PaperTone(paper, paperDark, x, y, 1511));
            }

            // As dobras em quatro: o vinco escuro e o lado que pega luz.
            for (int y = 3; y < height - 3; y++)
            {
                c.Set(width / 2, y, Rgba.Lerp(c.Get(width / 2, y), paperDark, 0.6f));
                c.Set(width / 2 + 1, y, Rgba.Lerp(c.Get(width / 2 + 1, y), new Rgba(240, 232, 210), 0.5f));
            }

            for (int x = 3; x < width - 3; x++)
            {
                c.Set(x, height / 2, Rgba.Lerp(c.Get(x, height / 2), paperDark, 0.6f));
                c.Set(x, height / 2 + 1, Rgba.Lerp(c.Get(x, height / 2 + 1), new Rgba(240, 232, 210), 0.5f));
            }

            // O rebite que a prendia atras do cano, com a ferrugem em volta.
            ArtKit.Glow(c, width / 2f, 12f, 9f, ArtKit.Rust[2], 0.7f);
            ArtKit.Ball(c, width / 2f + 0.5f, 12.5f, 3f, 3f, ArtKit.Copper, 0.3f);

            var ink = new Rgba(70, 50, 40);
            PixelFont.Draw(c, "pequena,", 14, 26, ink, default, PixelFont.Style.Scratched, 1, 1512);

            int lineY = 44;
            foreach (List<string> paragraph in wrapped)
            {
                bool first = true;
                foreach (string line in paragraph)
                {
                    // A primeira linha de cada paragrafo entra um pouco, como na escola.
                    PixelFont.Draw(c, line, first ? 22 : 14, lineY, ink, default, PixelFont.Style.Scratched, 1, 1513 + lineY);
                    lineY += 13;
                    first = false;
                }

                lineY += 8;
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

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

        /// <summary>
        /// O diario do chefe, aberto: na pagina da esquerda o que ele escreveu;
        /// na da direita, so a marca de uma mao queimada no papel.
        /// </summary>
        private static PixelCanvas DiaryView()
        {
            const int width = 300;
            const int height = 210;
            var c = new PixelCanvas(width, height);

            // A capa de couro, aparecendo em volta das folhas.
            for (int y = 4; y < height - 4; y++)
            {
                for (int x = 4; x < width - 4; x++)
                {
                    float light = (ArtKit.Noise(x * 0.1f, y * 0.1f, 1601) - 0.5f) * 1.2f;
                    c.Set(x, y, ArtKit.Tone(ArtKit.Leather, light));
                }
            }

            var paper = new Rgba(226, 216, 188);
            var paperDark = new Rgba(186, 170, 130);
            for (int y = 12; y < height - 12; y++)
            {
                for (int x = 14; x < width - 14; x++)
                {
                    if (Math.Abs(x - width / 2) < 3) continue;
                    Rgba color = PaperTone(paper, paperDark, x, y, 1602);

                    // A sombra da lombada, no meio.
                    int d = Math.Abs(x - width / 2);
                    if (d < 14) color = Rgba.Lerp(color, paperDark, (14 - d) / 20f);
                    c.Set(x, y, color);
                }
            }

            for (int y = 10; y < height - 10; y++) c.Set(width / 2, y, ArtKit.Leather[0]);

            var ink = new Rgba(52, 40, 36);
            int lineY = 22;
            int seed = 1603;
            foreach (string paragraph in new[]
            {
                "Se as caldeiras pararem, o vapor para. Sem vapor, os de baixo congelam antes de o Óxido chegar.",
                "Vou ficar com elas. Alguém tem que manter a pressão. Tranquem a porta por fora e não a abram, ouçam o que ouvirem.",
            })
            {
                foreach (string line in PixelFont.Wrap(paragraph, 118))
                {
                    PixelFont.Draw(c, line, 22, lineY, ink, default, PixelFont.Style.Scratched, 1, seed++);
                    lineY += 12;
                }

                lineY += 6;
            }

            // A mao queimada: a palma e os dedos, chamuscados, com a borda torrada.
            var burn = new PixelCanvas(width, height);
            int hx = 222;
            int hy = 134;
            Rgba[] solid = { Ink, Ink, Ink, Ink };
            ArtKit.Ball(burn, hx, hy, 15f, 17f, solid);

            // Os quatro dedos abertos pra cima, e o polegar saindo de lado, mais baixo.
            float[] angles = { -116f, -98f, -80f, -62f };
            float[] lengths = { 34f, 40f, 38f, 30f };
            for (int i = 0; i < 4; i++)
            {
                float bx = hx - 10f + i * 6.5f;
                float by = hy - 12f;
                (float fx, float fy) = ArtKit.Along(bx, by, angles[i], lengths[i]);
                ArtKit.Limb(burn, bx, by, fx, fy, 3.2f, solid);
            }

            (float tx, float ty) = ArtKit.Along(hx - 12f, hy + 2f, -160f, 26f);
            ArtKit.Limb(burn, hx - 12f, hy + 2f, tx, ty, 3.6f, solid);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (burn.Get(x, y).A == 0 || c.Get(x, y).A == 0) continue;
                    bool edge = burn.Get(x - 2, y).A == 0 || burn.Get(x + 2, y).A == 0 || burn.Get(x, y - 2).A == 0 || burn.Get(x, y + 2).A == 0;
                    Rgba under = c.Get(x, y);
                    c.Set(x, y, edge ? Rgba.Lerp(under, new Rgba(120, 72, 36), 0.7f) : Rgba.Lerp(under, new Rgba(40, 26, 20), 0.75f));
                }
            }

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

        /// <summary>
        /// A mascara de solda pequena, rachada ao meio, e o que foi gravado na
        /// tira de latao de dentro dela.
        /// </summary>
        private static PixelCanvas WeldingMaskView()
        {
            const int width = 176;
            const int height = 176;
            var c = new PixelCanvas(width, height);

            // O casco de fibra escura, com as bordas arredondadas.
            for (int y = 8; y < height - 8; y++)
            {
                for (int x = 14; x < width - 14; x++)
                {
                    float nx = (x - width / 2f) / (width / 2f - 14f);
                    float ny = (y - height / 2f) / (height / 2f - 8f);
                    if (Math.Pow(Math.Abs(nx), 3) + Math.Pow(Math.Abs(ny), 3) > 1) continue;

                    float light = -nx * 0.5f - ny * 0.4f + (ArtKit.Noise(x * 0.12f, y * 0.12f, 1701) - 0.5f) * 0.4f;
                    c.Set(x, y, ArtKit.Tone(ArtKit.Rubber, light));
                }
            }

            // O visor: o vidro verde-escuro, com o reflexo.
            for (int y = 50; y < 82; y++)
            {
                for (int x = 40; x < 136; x++)
                {
                    Rgba glass = new Rgba(30, 58, 48);
                    if (y == 50 || y == 81 || x == 40 || x == 135) glass = ArtKit.Iron[3];
                    else if ((x - y) % 40 < 6 && x < 90) glass = new Rgba(70, 110, 92);
                    c.Set(x, y, glass);
                }
            }

            // A tira de latao, por dentro, com o que foi gravado a prego.
            c.Fill(30, 112, 116, 32, ArtKit.Brass[2]);
            c.Fill(30, 112, 116, 2, ArtKit.Brass[3]);
            c.Fill(30, 142, 116, 2, ArtKit.Brass[0]);
            PixelFont.DrawCentered(c, "PARA A PEQUENA,", 88, 117, ArtKit.Brass[0], ArtKit.Brass[3], PixelFont.Style.Engraved, 1, 1702);
            PixelFont.DrawCentered(c, "QUANDO CRESCER.", 88, 130, ArtKit.Brass[0], ArtKit.Brass[3], PixelFont.Style.Engraved, 1, 1703);

            // A rachadura: de cima a baixo, torta, separando as duas metades.
            int crackX = 92;
            for (int y = 6; y < height - 6; y++)
            {
                if (ArtKit.Hash(y, 0, 1704) % 3 == 0) crackX += ArtKit.Hash(y, 1, 1704) % 2 == 0 ? 1 : -1;
                if (c.Get(crackX, y).A == 0) continue;
                c.Set(crackX, y, Rgba.Clear);
                c.Set(crackX + 1, y, Ink);
                c.Set(crackX - 1, y, ArtKit.Rubber[3]);
            }

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
