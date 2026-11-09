using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O que se le no District 3, o das geodes. Mesmo esquema das outras
    /// fases: o desenho pequeno do mapa (o mesmo do cenario, do GeodeArt e do
    /// StatueArt) e o desenho grande da leitura, com o texto escrito nele.
    ///
    /// Os bilhetes de mineiro sao todos o mesmo papel (MinerNoteView), preso
    /// por uma lasca de cristal; as gravuras das Ruinas aparecem ampliadas;
    /// a tabua do turno tem os doze nomes; a placa da Coroa e a de prata tem
    /// so o que esta escrito nelas.
    /// </summary>
    public static partial class LoreArt
    {
        private static Dictionary<string, Piece> _phase03;

        /// <summary>As do District 3, pelo nome do LoreSpot (e a placa do chassi da broca).</summary>
        public static IReadOnlyDictionary<string, Piece> Phase03 => _phase03 ?? (_phase03 = BuildPhase03());

        /// <summary>A placa no chassi da broca, que nao e um LoreSpot: o gerador liga ela na mao.</summary>
        public const string DrillPlaque = "Lore_DrillPlaque";

        private static readonly Rgba MinerInk = new Rgba(54, 48, 72);

        private static Dictionary<string, Piece> BuildPhase03()
        {
            var pieces = new List<Piece>
            {
                Make("Lore_Helmet", "helmet", GeodeArt.Helmet, 0f, false, HelmetView()),
                Make("Lore_RiftSign", "crown_sign", GeodeArt.CrownSign, 0f, false, CrownSignView()),
                Make("Lore_HunterRoster", "roster_board", GeodeArt.RosterBoard, 0f, false, RosterView()),
                Make("Lore_WordPlaque", "silver_words", StatueArt.SilverPlaque, 1.45f, true, WordPlaqueView()),

                Make("Lore_VaultPage", "miner_note", GeodeArt.MinerNote, 0.8f, true, MinerNoteView(new[]
                {
                    "A abóbada canta quando a gente pisa errado.",
                    "Aprendemos a pisar como quem reza.",
                }, 3101, false)),
                Make("Lore_LoopNote", "miner_note", GeodeArt.MinerNote, 0.8f, true, MinerNoteView(new[]
                {
                    "Dei a volta pela direita, como o capataz mandou.",
                    "Ninguém disse que lá em cima tinha um deles.",
                }, 3102, true)),
                Make("Lore_MinerNote", "miner_note", GeodeArt.MinerNote, 0.8f, true, MinerNoteView(new[]
                {
                    "Ela não para mais. Desligamos a chave, e ela não parou. Não há mais ninguém lá dentro, e ela não parou.",
                    "Se você está lendo isto: não faça barulho perto dela. Ela não enxerga. Ela escuta.",
                    "E tem a luz. Os pilares de cristal lá dentro acendem quando cantam, e ela corre atrás da luz como bicho atrás de lampião.",
                    "Mas a luz apaga rápido. Se o cristal ainda estiver brilhando forte quando ela chega, ela perde o controle e se crava nele. Foi a única vez que vi aquele motor parado.",
                }, 3103, true)),

                Make(DrillPlaque, "drill_plaque", DrillPlaqueMap, 0f, false, DrillPlaqueView()),
            };

            for (int scene = 0; scene < CrystalArt.EngravingCount; scene++)
            {
                int s = scene;
                pieces.Add(Make("Lore_Engraving_" + (scene + 1), "engraving_" + scene, () => CrystalArt.Engraving(s), 1.75f, true,
                    EngravingView(scene)));
            }

            return Index(pieces);
        }

        // ------------------------------------------------------------------
        // O capacete
        // ------------------------------------------------------------------

        /// <summary>O capacete visto por dentro, grande: o latao amassado e o que foi riscado a prego.</summary>
        private static PixelCanvas HelmetView()
        {
            var c = new PixelCanvas(196, 132);

            // A cupula, vista de baixo: a borda da aba e o forro escuro por dentro.
            ArtKit.Ball(c, 98f, 70f, 92f, 58f, ArtKit.Corroded(ArtKit.Brass, 0.25f, 3111, 0.08f), 0.1f);
            ArtKit.Ball(c, 98f, 74f, 78f, 46f, new[] { new Rgba(36, 28, 24), new Rgba(52, 40, 32), new Rgba(70, 54, 40), new Rgba(88, 68, 50) }, -0.2f);

            // O amassado, de um lado.
            ArtKit.Line(c, 150, 30, 170, 60, ArtKit.Brass[0]);
            ArtKit.Line(c, 151, 30, 171, 60, ArtKit.Brass[3]);

            // A tira de couro do forro, rebitada.
            for (int x = 30; x < 166; x++) c.Set(x, 98, ArtKit.Leather[1]);
            ArtKit.Rivet(c, 32, 97, ArtKit.Brass);
            ArtKit.Rivet(c, 162, 97, ArtKit.Brass);

            // O risco a prego, no forro.
            PixelFont.DrawCentered(c, "NÃO CORRA.", 98, 56, NailScratch, ArtKit.Brass[0], PixelFont.Style.Scratched, 2, 3112);
            PixelFont.DrawCentered(c, "ELES ESCUTAM.", 98, 80, NailScratch, ArtKit.Brass[0], PixelFont.Style.Scratched, 1, 3113);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // A placa da Coroa, na beira da Fenda
        // ------------------------------------------------------------------

        /// <summary>A chapa esmaltada da Coroa, grande: a coroa e o 925 em cima, e o aviso — so ele.</summary>
        private static PixelCanvas CrownSignView()
        {
            const int width = 220;
            var enamel = new[] { new Rgba(90, 20, 30), new Rgba(140, 34, 44), new Rgba(178, 52, 58), new Rgba(214, 90, 84) };
            List<string> lines = PixelFont.Wrap("PARA TRÁS, ESPAÇO RESERVADO PARA MINERAÇÃO PRIVADA", 176, 1);
            int height = 76 + lines.Count * 14 + 22;

            var c = new PixelCanvas(width, height);
            Plate(c, 2, 2, width - 4, height - 4, ArtKit.Brass, 0.15f, 3121);

            // O esmalte vermelho dentro da moldura, lascado aqui e ali.
            for (int y = 10; y < height - 10; y++)
            {
                for (int x = 10; x < width - 10; x++)
                {
                    bool chip = ArtKit.Noise(x * 0.12f, y * 0.12f, 3122) > 0.8f;
                    c.Set(x, y, chip ? ArtKit.Iron[1] : ArtKit.Tone(enamel, 0.2f - y / (float)height * 0.5f));
                }
            }

            // A coroa e o 925, de latao.
            foreach (int tip in new[] { 92, 110, 128 })
            {
                int top = tip == 110 ? 16 : 22;
                for (int y = top; y < 38; y++)
                {
                    int half = 1 + (y - top) * 6 / (38 - top);
                    for (int x = tip - half; x <= tip + half; x++) c.Set(x, y, x < tip ? ArtKit.Brass[3] : ArtKit.Brass[2]);
                }
            }

            c.Fill(84, 36, 53, 8, ArtKit.Brass[2]);
            c.Fill(84, 36, 53, 2, ArtKit.Brass[3]);
            PixelFont.DrawCentered(c, "925", width / 2, 50, ArtKit.Brass[3], ArtKit.Brass[0], PixelFont.Style.Embossed);

            int lineY = 70;
            foreach (string line in lines)
            {
                PixelFont.DrawCentered(c, line, width / 2, lineY, ArtKit.Brass[3], ArtKit.Brass[0], PixelFont.Style.Embossed);
                lineY += 14;
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // A tabua do turno
        // ------------------------------------------------------------------

        private static readonly string[] Shift =
        {
            "BENTO", "ISAURA", "JOAQUIM", "TEREZA", "OSVALDO", "MIRANDA",
            "LÚCIO", "DAMIÃO", "ZÉ CORDA", "NEVES", "ARLINDO",
        };

        /// <summary>
        /// A tabua do turno, grande: o CAPATAZ entalhado em cima, os onze do
        /// turno embaixo (tres riscados com forca, os outros com um cristal
        /// desenhado do lado) e o que alguem escreveu depois, a lapis.
        /// </summary>
        private static PixelCanvas RosterView()
        {
            const int width = 224;
            const int height = 212;
            var c = new PixelCanvas(width, height);
            Rgba[] wood = { new Rgba(46, 30, 24), new Rgba(76, 50, 34), new Rgba(108, 74, 48), new Rgba(140, 100, 66) };

            // As tabuas, com os vaos e os pregos.
            for (int y = 4; y < height - 4; y++)
            {
                bool seam = (y - 4) % 34 == 33;
                for (int x = 4; x < width - 4; x++)
                {
                    float grain = (ArtKit.Noise(x * 0.04f, y * 0.5f, 3131) - 0.5f) * 0.9f + ((y - 4) % 34 < 2 ? 0.5f : 0f);
                    c.Set(x, y, seam ? wood[0] : ArtKit.Tone(wood, grain));
                }
            }

            for (int y = 20; y < height; y += 34)
            {
                ArtKit.Rivet(c, 10, y, ArtKit.Iron);
                ArtKit.Rivet(c, width - 12, y, ArtKit.Iron);
            }

            // A rachadura de quem riscou com forca demais.
            ArtKit.Line(c, 160, 4, 176, height - 6, wood[0]);
            ArtKit.Line(c, 161, 4, 177, height - 6, ArtKit.Outline);

            // O capataz, entalhado fundo.
            PixelFont.DrawCentered(c, "CAPATAZ", width / 2, 12, wood[3], wood[0], PixelFont.Style.Engraved, 2);
            PixelFont.DrawCentered(c, "O PRIMEIRO A DESCER", width / 2, 34, wood[3], wood[0], PixelFont.Style.Engraved);

            // Os onze, em duas colunas.
            for (int i = 0; i < Shift.Length; i++)
            {
                int column = i < 6 ? 0 : 1;
                int x = 26 + column * 104;
                int y = 60 + (i % 6) * 15;
                PixelFont.Draw(c, Shift[i], x, y, wood[3], wood[0], PixelFont.Style.Engraved);

                int end = x + PixelFont.Measure(Shift[i]);
                if (i == 3 || i == 7 || i == 10)
                {
                    // Riscado com tanta forca que lascou a madeira.
                    ArtKit.Line(c, x - 3, y + 4, end + 3, y + 2, ArtKit.Outline);
                    ArtKit.Line(c, x - 3, y + 5, end + 3, y + 3, wood[0]);
                }
                else
                {
                    // O cristalzinho desenhado do lado.
                    ArtKit.Taper(c, end + 6f, y + 7f, end + 7f, y, 1.6f, 0.3f,
                        new[] { new Rgba(70, 40, 120), new Rgba(128, 82, 190), new Rgba(186, 142, 236), new Rgba(236, 216, 255) }, 0.2f);
                }
            }

            // O que escreveram depois, a giz, tremido.
            int noteY = 156;
            foreach (string line in new[] { "ELE OUVIU A PEDRA ANTES DE TODO MUNDO.", "DESCEU PRA TOCA E NÃO SUBIU MAIS.", "LEVOU A CHAVE DO SALÃO." })
            {
                PixelFont.Draw(c, line, 18, noteY, Chalk, default, PixelFont.Style.Scratched, 1, 3132 + noteY);
                noteY += 13;
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // A placa de prata e o bloco de notas
        // ------------------------------------------------------------------

        /// <summary>A placa de prata so com as cinco palavras, e embaixo o bloco de notas pequeno.</summary>
        private static PixelCanvas WordPlaqueView()
        {
            const int width = 232;
            const int plateHeight = 58;
            var c = new PixelCanvas(width, 168);

            Plate(c, 2, 2, width - 4, plateHeight, Silver, 0f, 3141);
            PixelFont.DrawCentered(c, "GELO  RUBI  MEL  SANGUE  CÉU", width / 2, 26, Silver[0], Silver[3], PixelFont.Style.Engraved);

            // O bloco de notas: papel pautado, torto, com a espiral em cima.
            var paper = new Rgba(222, 214, 182);
            var paperDark = new Rgba(178, 164, 122);
            const int nx = 62;
            const int ny = 72;
            const int nw = 140;
            const int nh = 88;
            for (int y = ny; y < ny + nh; y++)
            {
                int shift = (y - ny) / 22;
                for (int x = nx + shift; x < nx + nw + shift; x++)
                {
                    bool rule = (y - ny) % 12 == 11 && y > ny + 14;
                    c.Set(x, y, rule ? new Rgba(150, 170, 200) : PaperTone(paper, paperDark, x, y, 3142));
                }
            }

            for (int x = nx + 6; x < nx + nw - 4; x += 8) ArtKit.Ball(c, x + 0.5f, ny + 1.5f, 2.2f, 2.6f, ArtKit.Iron, 0.4f);

            int lineY = ny + 16;
            foreach (string line in PixelFont.Wrap("muito chato ter que ficar andando de um lado pro outro pra abrir uma porta", nw - 20))
            {
                PixelFont.Draw(c, line, nx + 10 + (lineY - ny) / 22, lineY, MinerInk, default, PixelFont.Style.Scratched, 1, 3143 + lineY);
                lineY += 12;
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // Os bilhetes de mineiro
        // ------------------------------------------------------------------

        /// <summary>
        /// O bilhete de mineiro, grande: papel sujo de po de pedra, dobrado, com
        /// a lasca de cristal que o prendia na rocha. Todo bilhete do distrito e
        /// este papel; muda a letra (tremida ou firme) e o texto.
        /// </summary>
        private static PixelCanvas MinerNoteView(string[] paragraphs, int seed, bool shaky)
        {
            const int width = 188;
            const int wrap = 156;

            var wrapped = new List<List<string>>();
            int lines = 0;
            foreach (string paragraph in paragraphs)
            {
                List<string> l = PixelFont.Wrap(paragraph, wrap);
                wrapped.Add(l);
                lines += l.Count;
            }

            int height = 38 + lines * 13 + wrapped.Count * 8 + 18;
            var c = new PixelCanvas(width, height);
            var paper = new Rgba(206, 198, 172);
            var dust = new Rgba(150, 144, 150);

            for (int y = 3; y < height - 3; y++)
            {
                for (int x = 3; x < width - 3; x++)
                {
                    // A borda de baixo rasgada.
                    if (y > height - 10 && y > height - 10 + ArtKit.Hash(x / 3, 0, seed) % 6) continue;
                    c.Set(x, y, PaperTone(paper, dust, x, y, seed));
                }
            }

            // A dobra do meio.
            for (int y = 3; y < height - 3; y++)
            {
                if (c.Get(width / 2, y).A == 0) continue;
                c.Set(width / 2, y, Rgba.Lerp(c.Get(width / 2, y), dust, 0.6f));
            }

            // A lasca de cristal que prendia o papel na rocha.
            ArtKit.Taper(c, width / 2f - 4f, 18f, width / 2f + 4f, 2f, 4f, 0.5f,
                new[] { new Rgba(70, 40, 120), new Rgba(128, 82, 190), new Rgba(186, 142, 236), new Rgba(236, 216, 255) }, 0.2f);

            int lineY = 30;
            foreach (List<string> paragraph in wrapped)
            {
                foreach (string line in paragraph)
                {
                    int wobble = shaky ? ArtKit.Hash(lineY, 0, seed) % 3 - 1 : 0;
                    PixelFont.Draw(c, line, 16 + wobble, lineY, MinerInk, default, PixelFont.Style.Scratched, 1, seed + lineY);
                    lineY += 13;
                }

                lineY += 8;
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // As gravuras das Ruinas
        // ------------------------------------------------------------------

        /// <summary>A gravura da laje, ampliada tres vezes: e o mesmo desenho, de perto.</summary>
        private static PixelCanvas EngravingView(int scene)
        {
            PixelCanvas small = CrystalArt.Engraving(scene);
            const int k = 3;
            var c = new PixelCanvas(small.Width * k + 4, small.Height * k + 4);

            for (int y = 0; y < small.Height; y++)
            {
                for (int x = 0; x < small.Width; x++)
                {
                    Rgba color = small.Get(x, y);
                    if (color.A == 0) continue;
                    c.Fill(2 + x * k, 2 + y * k, k, k, color);
                }
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        // ------------------------------------------------------------------
        // A placa do chassi da broca
        // ------------------------------------------------------------------

        /// <summary>A placa de prata rebitada no chassi, com a ordem do Rei e a data — e nenhum nome.</summary>
        private static PixelCanvas DrillPlaqueView()
        {
            const int width = 220;
            const int height = 92;
            var c = new PixelCanvas(width, height);
            Plate(c, 2, 2, width - 4, height - 4, Silver, 0.25f, 3151);

            PixelFont.DrawCentered(c, "POR ORDEM DE SUA MAJESTADE,", width / 2, 18, Silver[0], Silver[3], PixelFont.Style.Engraved);
            PixelFont.DrawCentered(c, "O REI DE PRATA.", width / 2, 34, Silver[0], Silver[3], PixelFont.Style.Engraved);
            PixelFont.DrawCentered(c, "PRIMEIRA PERFURAÇÃO: ANO 925", width / 2, 58, Silver[0], Silver[3], PixelFont.Style.Engraved);

            // Oxido escorrido por cima: estava coberta.
            for (int x = 6; x < width - 6; x++)
            {
                int drip = (int)(ArtKit.Noise(x * 0.1f, 0f, 3152) * 18f);
                for (int y = height - 6 - drip; y < height - 4; y++) c.Blend(x, y, ArtKit.OxideDeep.WithAlpha(110));
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        private static PixelCanvas DrillPlaqueMap()
        {
            var c = new PixelCanvas(12, 7);
            c.Fill(1, 1, 10, 5, Silver[2]);
            c.Fill(1, 1, 10, 1, Silver[3]);
            for (int x = 3; x < 9; x++) if (x % 2 == 0) c.Set(x, 3, Silver[0]);
            ArtKit.AddOutline(c, Ink);
            return c;
        }
    }
}
