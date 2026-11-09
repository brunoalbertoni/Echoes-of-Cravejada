using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O que se le no District 4, o dos laboratorios. Mesmo esquema das outras
    /// fases: o desenho pequeno do mapa e o desenho grande da leitura, com o
    /// texto escrito nele.
    ///
    /// Toda folha do diario de laboratorio e o mesmo papel pautado (LabPage),
    /// com o dia no alto; todo retrato e a mesma moldura de prata; os dois
    /// quadros-negros sao o mesmo quadro; as placas de prata sao a mesma placa.
    /// A ficha do n.º 7 tem o desenho tecnico do proprio Scrap.
    /// </summary>
    public static partial class LoreArt
    {
        private static Dictionary<string, Piece> _phase04;

        /// <summary>As do District 4, pelo nome do LoreSpot.</summary>
        public static IReadOnlyDictionary<string, Piece> Phase04 => _phase04 ?? (_phase04 = BuildPhase04());

        private static readonly Rgba LabInk = new Rgba(40, 46, 78);
        private static readonly Rgba LabPaper = new Rgba(226, 222, 204);
        private static readonly Rgba LabPaperDark = new Rgba(180, 172, 146);
        private static readonly Rgba RuleBlue = new Rgba(150, 174, 210);
        private static readonly Rgba Blackboard = new Rgba(34, 46, 40);

        private static Dictionary<string, Piece> BuildPhase04()
        {
            var pieces = new List<Piece>
            {
                Make("Lore_Portal", "silver_sign", SilverSignMap, 2.2f, true, SilverPlaqueView(
                    new[] { "ATELIÊS ALQUÍMICOS", "DE SUA MAJESTADE", "", "ENTRADA VEDADA", "A METAIS COMUNS" },
                    "e a quem mais?", 4401)),

                Make("Lore_Portrait_1", "portrait_0", () => MarbleArt.Portrait(0), 2.6f, true, PortraitView(0, 1, "MESTRE DE FUNDIÇÃO", 4411)),
                Make("Lore_Portrait_2", "portrait_1", () => MarbleArt.Portrait(1), 2.6f, true, PortraitView(1, 2, "TESOUREIRA DA COROA", 4412)),
                Make("Lore_Portrait_Crisol", "portrait_2", () => MarbleArt.Portrait(2), 2.6f, true, PortraitView(2, 0, "CRISOL. LAPIDÁRIO DA CORTE.", 4413)),
                Make("Lore_Portrait_3", "portrait_0", () => MarbleArt.Portrait(0), 2.6f, true, PortraitView(3, 3, "ALQUIMISTA-CHEFE", 4414)),

                Make("Lore_Diary_1", "lab_page", LabPageMap, 0f, false, LabPage(12, new[]
                {
                    "O mercúrio retarda a corrosão! Uma placa de prata mergulhada resistiu três dias a mais.",
                    "Sua Majestade foi informada. Há esperança. Vamos curar a cidade inteira.",
                }, 4421, 0)),
                Make("Lore_Diary_Mercury", "lab_page", LabPageMap, 0f, false, LabPage(40, new[]
                {
                    "Sete destilações. O mercúrio sai tão puro que reflete o rosto de quem olha, e o rosto não tem uma mancha de Óxido.",
                    "Os assistentes de cobre desmaiam com o vapor. O Mestre respira fundo e diz que é perfume.",
                }, 4422, 1)),
                Make("Lore_Diary_2", "lab_page", LabPageMap, 0f, false, LabPage(90, new[]
                {
                    "O mercúrio só retarda. Precisamos testar em corpo inteiro, não em placa.",
                    "Sua Majestade autorizou o uso de voluntários de cobre. São abundantes, e ninguém sente falta deles.",
                }, 4423, 1, "voluntários")),
                Make("Lore_Diary_Light", "lab_page", LabPageMap, 0f, false, LabPage(150, new[]
                {
                    "A luz alquímica atravessa o Óxido e mostra o que já apodreceu por dentro. Nas placas de prata nobre, mostra muito.",
                    "Sua Majestade proibiu que a luz fosse apontada para a Corte. Deixamos as lâmpadas acesas só aqui.",
                    "Queimam. Andem entre um clarão e outro.",
                }, 4424, 1)),
                Make("Lore_Diary_3", "lab_page", LabPageMap, 0f, false, LabPage(210, new[]
                {
                    "O tanque 4 não acordou. O tanque 7 acordou errado. Costuramos o que sobrou do 7 no que sobrou do 9.",
                    "O Mestre diz que estamos quase lá. Ele já não dorme. Ele já não é bem de prata.",
                }, 4425, 2)),
                Make("Lore_Diary_4", "lab_page", LabPageMap, 0f, false, LabPage(255, new[]
                {
                    "O Mestre entrou no tanque grande. Sozinho. Disse que, se o mercúrio retarda, então basta SER mercúrio.",
                    "Ele saiu. Fala como antes, educado. Mas escorre.",
                    "Os guardas de prata perfeita passaram a vigiar o laboratório. Não sei se guardam a nós ou a ele.",
                }, 4426, 3)),
                Make("Lore_Diary_5", "lab_page", LabPageMap, 0f, false, LabPage(300, new[]
                {
                    "Os protótipos brutos são imunes. Quartzo não lapidado, bronze sem polimento: o Óxido não pega neles.",
                    "Levei um até o trono. Andando. Imune. A cura da cidade inteira, de pé na frente dele.",
                    "Sua Majestade recusou. Ordem de descarte assinada.",
                }, 4427, 3)),

                Make("Lore_Chalk_Mercury", "chalkboard", ChalkboardMap, 1.2f, true, ChalkboardView(new[]
                {
                    "PRATA COM MERCÚRIO DÁ",
                    "PRATA QUE NÃO APODRECE?",
                }, "Não apodrece. Escorre.", 0, 4431)),
                Make("Lore_Chalk_Light", "chalkboard", ChalkboardMap, 1.2f, true, ChalkboardView(new[]
                {
                    "O ESPELHO NÃO CRIA A LUZ.",
                    "SÓ ESCOLHE PARA ONDE",
                    "ELA VAI.",
                }, null, 1, 4432)),

                Make("Lore_PrototypeBoard", "cork_board", CorkBoardMap, 1.4f, true, CorkBoardView()),
                Make("Lore_PrototypeSheet", "proto_sheet", () => MarbleArt.PrototypeSheet(), 1.6f, true, PrototypeSheetView()),

                Make("Lore_Amphitheater", "silver_sign", SilverSignMap, 1.8f, true, SilverPlaqueView(
                    new[] { "ANFITEATRO DE", "DEMONSTRAÇÕES", "", "AQUI A CIÊNCIA SE", "APRESENTA A", "SUA MAJESTADE" },
                    null, 4441)),
                Make("Lore_DiscardOrder", "noble_paper_pinned", () => NoblePaperMap(true), 1.6f, true, NobleDocument(
                    new[] { "DECRETO DE SUA MAJESTADE", "SOBRE O PROTÓTIPO BRUTO" },
                    new[]
                    {
                        "O protótipo bruto foi apresentado e recusado. A prata não será salva por sucata.",
                        "Destino de todo o lote: Poço de Sucata. Que não se fale mais nisso.",
                    },
                    new[] { "DESCARTE" }, new Note[0], 4451, true)),
            };

            return Index(pieces);
        }

        // ------------------------------------------------------------------
        // A folha do diario de laboratorio
        // ------------------------------------------------------------------

        /// <summary>
        /// Uma folha do caderno do laboratorio: papel pautado, margem vermelha,
        /// o dia no alto. Decay de 0 a 3: quanto mais adiante, pior a letra e
        /// mais manchas de mercurio. Overwrite e uma palavra escrita por cima
        /// de outra, raspada.
        /// </summary>
        private static PixelCanvas LabPage(int day, string[] paragraphs, int seed, int decay, string overwrite = null)
        {
            const int width = 196;
            const int wrap = 160;

            var wrapped = new List<List<string>>();
            int lines = 0;
            foreach (string paragraph in paragraphs)
            {
                List<string> l = PixelFont.Wrap(paragraph, wrap);
                wrapped.Add(l);
                lines += l.Count;
            }

            int height = 54 + lines * 13 + wrapped.Count * 7 + 22;
            var c = new PixelCanvas(width, height);

            for (int y = 3; y < height - 3; y++)
            {
                for (int x = 3; x < width - 3; x++)
                {
                    // Os furos da espiral, a esquerda.
                    if (x < 12 && y % 12 < 4 && x > 5) continue;
                    c.Set(x, y, PaperTone(LabPaper, LabPaperDark, x, y, seed));
                }
            }

            // A pauta e a margem.
            for (int y = 48; y < height - 8; y += 13)
            {
                for (int x = 14; x < width - 6; x++) c.Blend(x, y + 10, RuleBlue.WithAlpha(150));
            }

            for (int y = 4; y < height - 4; y++) c.Blend(24, y, new Rgba(200, 80, 80, 170));

            // O cabecalho.
            PixelFont.Draw(c, "DIA " + day, 30, 12, LabInk, default, PixelFont.Style.Ink, 2);
            PixelFont.Draw(c, "Ateliês de Sua Majestade", width - 10 - PixelFont.Measure("Ateliês de Sua Majestade"), 32, LabInk.WithAlpha(170));

            int lineY = 48;
            foreach (List<string> paragraph in wrapped)
            {
                foreach (string line in paragraph)
                {
                    int wobble = decay >= 2 ? ArtKit.Hash(lineY, day, seed) % (decay) - decay / 2 : 0;
                    PixelFont.Style style = decay >= 3 ? PixelFont.Style.Scratched : PixelFont.Style.Ink;
                    PixelFont.Draw(c, line, 30 + wobble, lineY, LabInk, default, style, 1, seed + lineY);

                    if (overwrite != null)
                    {
                        int at = line.IndexOf(overwrite, StringComparison.Ordinal);
                        if (at >= 0)
                        {
                            int x0 = 30 + wobble + PixelFont.Measure(line.Substring(0, at));
                            int w = PixelFont.Measure(overwrite);
                            for (int x = x0 - 1; x < x0 + w + 1; x++)
                            {
                                c.Blend(x, lineY + 3, LabInk.WithAlpha(90));
                                c.Blend(x, lineY + 5, LabPaperDark.WithAlpha(160));
                            }
                        }
                    }

                    lineY += 13;
                }

                lineY += 7;
            }

            // As manchas de mercurio: gotas prateadas, mais quanto pior.
            for (int i = 0; i < decay * 2; i++)
            {
                float mx = 40 + ArtKit.Hash(i, day, seed + 3) % (width - 60);
                float my = 30 + ArtKit.Hash(i, day, seed + 4) % (height - 44);
                ArtKit.Ball(c, mx, my, 2.5f + i % 2, 2f + i % 2, LabArt.Mercury, 0.3f);
            }

            // Uma digital prateada no canto.
            if (decay >= 1)
            {
                for (int r = 2; r < 7; r += 2)
                {
                    for (int a = 0; a < 16; a++)
                    {
                        double ang = a * Math.PI / 8.0;
                        c.Blend(width - 24 + (int)(Math.Cos(ang) * r), height - 20 + (int)(Math.Sin(ang) * r * 1.3), LabArt.Mercury[1].WithAlpha(150));
                    }
                }
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>A folha no chao: o caderno aberto, pautado.</summary>
        private static PixelCanvas LabPageMap()
        {
            var c = new PixelCanvas(16, 8);
            c.Fill(1, 2, 14, 5, LabPaper);
            c.Fill(8, 2, 1, 5, LabPaperDark);
            for (int x = 2; x < 14; x++)
            {
                if (x == 8) continue;
                if (x % 3 != 0) c.Set(x, 4, LabInk);
                if (x % 4 != 1) c.Set(x, 5, RuleBlue);
            }

            c.Set(11, 3, LabArt.Mercury[2]);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // Os retratos
        // ------------------------------------------------------------------

        /// <summary>
        /// Um retrato na moldura de prata, de perto. Kind: 0 o Nobre de gola
        /// alta, 1 a Nobre, 2 o jovem de monoculo (Crisol), 3 o de mascara de
        /// bico. Scratch: 0 intacto, 1 riscado em X, 2 rasgado, 3 de leve.
        /// </summary>
        private static PixelCanvas PortraitView(int kind, int scratch, string plate, int seed)
        {
            const int width = 150;
            const int height = 196;
            var c = new PixelCanvas(width, height);

            // A moldura.
            for (int y = 0; y < height - 26; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int edge = Math.Min(Math.Min(x, width - 1 - x), Math.Min(y, height - 27 - y));
                    if (edge < 10)
                    {
                        float light = (x < y ? 0.5f : -0.2f) + (edge % 3 == 0 ? 0.3f : 0f);
                        c.Set(x, y, ArtKit.Tone(LabArt.Silver, light));
                    }
                    else
                    {
                        c.Set(x, y, PaperTone(new Rgba(48, 44, 62), new Rgba(30, 28, 40), x, y, seed));
                    }
                }
            }

            // O busto: ombros, pescoco, cabeca de prata.
            Rgba[] cloth = kind == 1 ? LabArt.Velvet : kind == 3 ? ArtKit.Leather : new[] { new Rgba(30, 34, 60), new Rgba(48, 54, 92), new Rgba(70, 78, 126), new Rgba(100, 108, 160) };
            ArtKit.Ball(c, 75f, 168f, 52f, 34f, ArtKit.Plain(cloth), 0.1f);
            for (int y = 140; y < height - 26; y++) for (int x = 10; x < width - 10; x++) if (c.Get(x, y).A == 0) c.Set(x, y, cloth[1]);
            ArtKit.Limb(c, 75f, 128f, 75f, 112f, 10f, LabArt.Silver, 0.1f);
            ArtKit.Ball(c, 75f, 86f, 25f, 30f, ArtKit.Plain(LabArt.Silver), 0.15f);

            switch (kind)
            {
                case 0:
                    // A gola alta e o bigode.
                    ArtKit.Polygon(c, new[] { (46f, 140f), (54f, 108f), (75f, 122f), (96f, 108f), (104f, 140f) }, (x, y) => ArtKit.Tone(cloth, 0.3f));
                    c.Fill(64, 98, 22, 3, LabArt.Silver[0]);
                    break;
                case 1:
                    // O coque e o colar.
                    ArtKit.Ball(c, 75f, 54f, 18f, 12f, ArtKit.Plain(LabArt.Silver), 0.3f);
                    for (int i = 0; i < 9; i++) ArtKit.Ball(c, 55f + i * 5f, 132f + Math.Abs(i - 4), 2f, 2f, ArtKit.Plain(LabArt.Mercury), 0.4f);
                    break;
                case 2:
                    // O monoculo e o cabelo penteado.
                    ArtKit.Ball(c, 75f, 62f, 24f, 10f, ArtKit.Plain(LabArt.Silver), 0.4f);
                    for (int a = 0; a < 24; a++)
                    {
                        double ang = a * Math.PI / 12.0;
                        c.Set(86 + (int)(Math.Cos(ang) * 7), 84 + (int)(Math.Sin(ang) * 7), ArtKit.Brass[3]);
                    }

                    ArtKit.Line(c, 92, 90, 100, 128, ArtKit.Brass[2]);
                    break;
                default:
                    // A mascara de bico, de couro, e o capuz.
                    ArtKit.Ball(c, 75f, 80f, 27f, 33f, ArtKit.Plain(ArtKit.Leather), 0.1f);
                    ArtKit.Taper(c, 78f, 92f, 110f, 118f, 9f, 1.5f, ArtKit.Leather, 0.2f);
                    ArtKit.Ball(c, 66f, 80f, 6f, 6f, ArtKit.Plain(new[] { new Rgba(30, 60, 50), new Rgba(60, 120, 90), new Rgba(120, 200, 150), new Rgba(220, 255, 230) }), 0.3f);
                    ArtKit.Ball(c, 86f, 80f, 6f, 6f, ArtKit.Plain(new[] { new Rgba(30, 60, 50), new Rgba(60, 120, 90), new Rgba(120, 200, 150), new Rgba(220, 255, 230) }), 0.3f);
                    break;
            }

            if (kind != 3)
            {
                c.Fill(64, 84, 6, 2, LabArt.Silver[0]);
                c.Fill(80, 84, 6, 2, LabArt.Silver[0]);
                ArtKit.Line(c, 75, 88, 73, 98, LabArt.Silver[1]);
                c.Fill(69, 104, 12, 1, LabArt.Silver[0]);
            }

            // Os riscos.
            if (scratch == 1 || scratch == 2 || scratch == 3)
            {
                Rgba green = scratch == 3 ? LabArt.OxideGreen.WithAlpha(150) : LabArt.OxideGreen;
                int thick = scratch == 3 ? 1 : 3;
                for (int t = 0; t < thick; t++)
                {
                    ArtKit.Line(c, 30 + t, 40, 120 + t, 150, green);
                    ArtKit.Line(c, 120 + t, 40, 30 + t, 150, green);
                }
            }

            if (scratch == 2)
            {
                // A tela rasgada: o fundo escuro aparece pelo rombo.
                for (int y = 70; y < 130; y++)
                {
                    int half = 6 - Math.Abs(y - 100) / 6;
                    int cx = 72 + (y % 7) - 3;
                    for (int x = cx - half; x < cx + half; x++) c.Set(x, y, new Rgba(12, 10, 16));
                }
            }

            // A plaqueta de baixo.
            Plate(c, 14, height - 24, width - 28, 22, LabArt.Silver, 0f, seed);
            PixelFont.DrawCentered(c, plate, width / 2, height - 17, LabArt.Silver[0], LabArt.Silver[3], PixelFont.Style.Engraved);

            if (kind == 2)
            {
                // A plaqueta de baixo arrancada: so os furos.
                c.Set(20, height - 4, Ink);
                c.Set(width - 21, height - 4, Ink);
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // O quadro-negro
        // ------------------------------------------------------------------

        /// <summary>O quadro-negro no cavalete, de perto: o texto a giz, o desenho e, se houver, outra letra embaixo.</summary>
        private static PixelCanvas ChalkboardView(string[] lines, string footnote, int drawing, int seed)
        {
            const int width = 220;
            const int height = 150;
            var c = new PixelCanvas(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int edge = Math.Min(Math.Min(x, width - 1 - x), Math.Min(y, height - 1 - y));
                    if (edge < 7) c.Set(x, y, ArtKit.Tone(LabArt.Walnut, edge < 2 ? -0.3f : 0.3f));
                    else
                    {
                        // O giz apagado por cima de giz apagado.
                        float smear = ArtKit.Noise(x * 0.05f, y * 0.12f, seed);
                        c.Set(x, y, Rgba.Lerp(Blackboard, new Rgba(70, 84, 76), Math.Max(0f, smear - 0.45f)));
                    }
                }
            }

            int lineY = 16;
            foreach (string line in lines)
            {
                if (line.Length > 0) PixelFont.Draw(c, line, 16, lineY, Chalk, default, PixelFont.Style.Ink, 1, seed + lineY);
                lineY += 14;
            }

            if (drawing == 0)
            {
                // Uma placa de prata mergulhada num frasco, com setas.
                ArtKit.Line(c, 160, 40, 160, 90, Chalk);
                ArtKit.Line(c, 200, 40, 200, 90, Chalk);
                ArtKit.Line(c, 160, 90, 200, 90, Chalk);
                for (int x = 162; x < 199; x += 3) c.Set(x, 66, Chalk);
                c.Fill(176, 36, 8, 40, Chalk.WithAlpha(120));
                ArtKit.Line(c, 170, 96, 180, 80, Chalk);
                PixelFont.Draw(c, "3 DIAS A MAIS", 146, 98, Chalk);
            }
            else
            {
                // A lampada, o espelho inclinado e o raio dobrando a esquina.
                ArtKit.Ball(c, 140f, 100f, 6f, 6f, ArtKit.Plain(new[] { Chalk, Chalk, Chalk, Chalk }), 0f);
                for (int x = 147; x < 190; x += 2) c.Set(x, 100, Chalk);
                ArtKit.Line(c, 184, 108, 196, 92, Chalk);
                for (int y = 98; y > 56; y -= 2) c.Set(190, y, Chalk);
                ArtKit.Line(c, 186, 60, 190, 54, Chalk);
                ArtKit.Line(c, 194, 60, 190, 54, Chalk);
            }

            if (footnote != null)
            {
                PixelFont.Draw(c, footnote, 18, height - 26, new Rgba(196, 230, 200), default, PixelFont.Style.Scratched, 1, seed + 9);
            }

            // O apagador e o toco de giz.
            c.Fill(150, height - 12, 18, 5, ArtKit.Leather[2]);
            c.Fill(174, height - 10, 6, 2, Chalk);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>O quadro-negro no mapa: num cavalete, com riscos de giz.</summary>
        private static PixelCanvas ChalkboardMap()
        {
            var c = new PixelCanvas(26, 20);
            c.Fill(1, 1, 24, 15, LabArt.Walnut[2]);
            c.Fill(2, 2, 22, 13, Blackboard);
            for (int row = 0; row < 4; row++)
            {
                for (int x = 4; x < 21; x++) if ((x * 3 + row) % 5 != 0) c.Set(x, 4 + row * 3, Chalk.WithAlpha(200));
            }

            c.Fill(4, 16, 2, 4, LabArt.Walnut[1]);
            c.Fill(20, 16, 2, 4, LabArt.Walnut[1]);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // A placa de prata
        // ------------------------------------------------------------------

        /// <summary>Uma placa de prata, de perto: as letras em relevo e, se houver, o risco a prego embaixo.</summary>
        private static PixelCanvas SilverPlaqueView(string[] cast, string scratched, int seed)
        {
            const int width = 210;
            int height = 34 + cast.Length * 16 + (scratched != null ? 24 : 0);
            var c = new PixelCanvas(width, height);

            Plate(c, 2, 2, width - 4, height - 4, LabArt.Silver, 0.05f, seed);

            for (int i = 0; i < 4; i++)
            {
                int rx = i % 2 == 0 ? 10 : width - 13;
                int ry = i < 2 ? 9 : height - 12;
                ArtKit.Rivet(c, rx, ry, LabArt.Silver);
            }

            int lineY = 18;
            foreach (string line in cast)
            {
                if (line.Length > 0) PixelFont.DrawCentered(c, line, width / 2, lineY, LabArt.Silver[3], LabArt.Silver[0], PixelFont.Style.Embossed);
                lineY += 16;
            }

            if (scratched != null)
            {
                PixelFont.Draw(c, scratched, width - 26 - PixelFont.Measure(scratched), lineY + 2, ScratchFresh, ScratchShadow,
                    PixelFont.Style.Scratched, 1, seed + 1);
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>A placa de prata no mapa, na parede.</summary>
        private static PixelCanvas SilverSignMap()
        {
            var c = new PixelCanvas(24, 12);
            c.Fill(1, 1, 22, 10, LabArt.Silver[2]);
            c.Fill(1, 1, 22, 1, LabArt.Silver[3]);
            c.Fill(1, 10, 22, 1, LabArt.Silver[0]);
            for (int row = 0; row < 3; row++)
            {
                for (int x = 4; x < 20; x++) if ((x + row) % 4 != 0) c.Set(x, 3 + row * 2, LabArt.Silver[0]);
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // O quadro das fichas dos prototipos
        // ------------------------------------------------------------------

        private static PixelCanvas CorkBoardView()
        {
            const int width = 310;
            const int height = 196;
            var c = new PixelCanvas(width, height);

            var cork = new Rgba(176, 130, 86);
            var corkDark = new Rgba(134, 94, 58);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int edge = Math.Min(Math.Min(x, width - 1 - x), Math.Min(y, height - 1 - y));
                    if (edge < 6) c.Set(x, y, ArtKit.Tone(LabArt.Walnut, edge < 2 ? -0.3f : 0.3f));
                    else c.Set(x, y, ArtKit.Hash(x, y, 4461) % 5 == 0 ? corkDark : PaperTone(cork, corkDark, x, y, 4462));
                }
            }

            string[][] cards =
            {
                new[] { "N.º 1", "Ferro fundido.", "Pesado demais", "para andar." },
                new[] { "N.º 2", "Latão.", "O Óxido pegou", "em três dias." },
                new[] { "N.º 3", "Cobre.", "era um", "voluntário" },
                new[] { "N.º 4", "Vidro.", "Quebrou ao se", "ver no espelho." },
                new[] { "N.º 5", "Prata nobre.", "Apodreceu antes", "de acordar." },
                new[] { "N.º 6", "Quartzo lapidado.", "Perfeito. Caiu", "no 1º passo." },
            };

            for (int i = 0; i < 6; i++)
            {
                int cx = 12 + (i % 3) * 98;
                int cy = 14 + (i / 3) * 74;
                int tilt = ArtKit.Hash(i, 0, 4463) % 3 - 1;

                c.Fill(cx, cy + tilt, 92, 62, new Rgba(232, 226, 206));
                c.Fill(cx, cy + 60 + tilt, 92, 2, new Rgba(190, 180, 150));
                ArtKit.Ball(c, cx + 43f, cy + 3f + tilt, 2.5f, 2.5f, ArtKit.Plain(new[] { new Rgba(120, 20, 20), new Rgba(180, 40, 40), new Rgba(220, 80, 80), new Rgba(255, 180, 180) }), 0.3f);

                int ly = cy + 10 + tilt;
                for (int l = 0; l < cards[i].Length; l++)
                {
                    PixelFont.Draw(c, cards[i][l], cx + 4, ly, l == 0 ? StampRed.WithAlpha(255) : LabInk);
                    ly += 12;
                }

                if (i == 2)
                {
                    // A ficha do cobre: riscada.
                    ArtKit.Line(c, cx + 2, cy + 26, cx + 40, cy + 24, LabInk);
                    ArtKit.Line(c, cx + 2, cy + 27, cx + 40, cy + 25, LabInk);
                }
            }

            // O setimo alfinete, sem ficha nenhuma, e o furo de onde ela saiu.
            ArtKit.Ball(c, 150f, 170f, 3f, 3f, ArtKit.Plain(new[] { new Rgba(120, 20, 20), new Rgba(180, 40, 40), new Rgba(220, 80, 80), new Rgba(255, 180, 180) }), 0.3f);
            c.Fill(147, 178, 8, 1, corkDark);
            PixelFont.Draw(c, "N.º 7 ?", 161, 166, ScratchFresh, ScratchShadow, PixelFont.Style.Scratched, 1, 4464);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        private static PixelCanvas CorkBoardMap()
        {
            var c = new PixelCanvas(26, 20);
            c.Fill(1, 1, 24, 18, LabArt.Walnut[2]);
            c.Fill(2, 2, 22, 16, new Rgba(176, 130, 86));
            for (int i = 0; i < 6; i++) c.Fill(4 + (i % 3) * 7, 4 + (i / 3) * 7, 5, 5, new Rgba(232, 226, 206));
            c.Set(12, 16, new Rgba(200, 40, 40));
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // A ficha do n.º 7
        // ------------------------------------------------------------------

        /// <summary>
        /// A prancheta com o desenho tecnico do Scrap: a cabeca de quartzo
        /// bruto, os dois olhos, a mola, cotados em milimetros; o carimbo de
        /// REJEITADO por cima e o destino escrito a mao embaixo.
        /// </summary>
        private static PixelCanvas PrototypeSheetView()
        {
            const int width = 250;
            const int height = 230;
            var c = new PixelCanvas(width, height);

            // A prancheta e o prendedor.
            c.Fill(4, 10, width - 8, height - 14, LabArt.Walnut[2]);
            c.Fill(4, 10, width - 8, 2, LabArt.Walnut[3]);
            for (int y = 18; y < height - 10; y++)
            {
                for (int x = 12; x < width - 12; x++) c.Set(x, y, PaperTone(new Rgba(214, 222, 236), new Rgba(170, 180, 200), x, y, 4471));
            }

            c.Fill(100, 4, 50, 16, LabArt.Silver[2]);
            c.Fill(100, 4, 50, 2, LabArt.Silver[3]);
            ArtKit.Rivet(c, 124, 10, LabArt.Silver);

            // A grade de desenho tecnico.
            for (int y = 24; y < height - 14; y += 8) for (int x = 14; x < width - 14; x++) if (x % 2 == 0) c.Set(x, y, new Rgba(180, 196, 220));
            for (int x = 14; x < width - 14; x += 8) for (int y = 24; y < height - 14; y++) if (y % 2 == 0) c.Set(x, y, new Rgba(180, 196, 220));

            var pen = new Rgba(36, 50, 110);

            PixelFont.DrawCentered(c, "PROTÓTIPO BRUTO N.º 7", width / 2, 28, pen, default, PixelFont.Style.Ink, 1);

            // O desenho: a cabeca facetada, os olhos, o corpo redondo e a mola.
            var head = new[] { (78f, 58f), (104f, 46f), (132f, 58f), (136f, 86f), (104f, 100f), (74f, 86f) };
            for (int i = 0; i < head.Length; i++)
            {
                (float ax, float ay) = head[i];
                (float bx, float by) = head[(i + 1) % head.Length];
                ArtKit.Line(c, (int)ax, (int)ay, (int)bx, (int)by, pen);
            }

            ArtKit.Line(c, 104, 46, 104, 100, pen.WithAlpha(140));
            ArtKit.Line(c, 78, 58, 136, 86, pen.WithAlpha(100));
            for (int a = 0; a < 20; a++)
            {
                double ang = a * Math.PI / 10.0;
                c.Set(94 + (int)(Math.Cos(ang) * 5), 74 + (int)(Math.Sin(ang) * 5), pen);
                c.Set(116 + (int)(Math.Cos(ang) * 5), 74 + (int)(Math.Sin(ang) * 5), pen);
            }

            // A mola do pescoco.
            for (int i = 0; i < 6; i++) ArtKit.Line(c, 96, 102 + i * 4, 112, 104 + i * 4, pen);

            // O corpo.
            for (int a = 0; a < 60; a++)
            {
                double ang = a * Math.PI / 30.0;
                c.Set(104 + (int)(Math.Cos(ang) * 30), 152 + (int)(Math.Sin(ang) * 24), pen);
            }

            // As cotas.
            ArtKit.Line(c, 60, 46, 60, 100, pen.WithAlpha(180));
            PixelFont.Draw(c, "54mm", 26, 70, pen);
            ArtKit.Line(c, 74, 112, 136, 112, pen.WithAlpha(180));
            PixelFont.Draw(c, "QUARTZO BRUTO", 142, 52, pen);
            PixelFont.Draw(c, "IMUNE AO ÓXIDO", 142, 140, pen);
            PixelFont.Draw(c, "FUNCIONAL", 142, 152, pen);

            // O carimbo vermelho, torto, por cima de tudo.
            var ink = new PixelCanvas(width, height);
            Box(ink, 34, 110, 142, 28, StampRed);
            Box(ink, 35, 111, 140, 26, StampRed);
            PixelFont.DrawCentered(ink, "REJEITADO · IMPERFEITO", 105, 120, StampRed);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Rgba color = ink.Get(x, y);
                    if (color.A == 0 || ArtKit.Hash(x, y, 4472) % 6 == 0) continue;
                    c.Blend(x, y + (x - 34) / 18, color);
                }
            }

            PixelFont.Draw(c, "Destino: Poço de Sucata.", 22, height - 32, PenInk, default, PixelFont.Style.Scratched, 1, 4473);

            ArtKit.AddOutline(c, Ink);
            return c;
        }
    }
}
