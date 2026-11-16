using System;
using System.Collections.Generic;
using static EchoesOfCravejada.EditorTools.DocKit;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>Os documentos do District 2 (Encanamentos de Latao), desenhados com o DocKit.</summary>
    public static partial class LoreArt
    {
        // ------------------------------------------------------------------
        // O livro de turnos: quatro folhas do mesmo caderno, cada uma com o seu estrago
        // ------------------------------------------------------------------

        /// <summary>A mao do encanador: a mesma nas quatro paginas, piorando.</summary>
        private static Hand Plumber(int seed, float tremor)
        {
            Hand h = Careful(15f, 1200);
            h.Seed = seed;
            h.Jitter = 0.15f + tremor * 0.6f;
            h.Wave = 0.6f + tremor * 1.6f;
            h.SizeJitter = 0.04f + tremor * 0.1f;
            h.Slant = 0.06f + tremor * 0.06f;
            return h;
        }

        private static PixelCanvas LogbookDoc(int page, string shift, string[] entries, float tremor, int seed)
        {
            var s = new DocSurface(420, 440);

            // Arrancada do caderno: a beirada da esquerda rasgada, com os furos da costura.
            s.Shape(DocSurface.Sheet(18, 16, 404, 426, seed, left: 9f, top: 1.2f, right: 1.2f, bottom: 1.2f));
            s.Paper(Ledger, LedgerDark, seed + 1, age: 0.6f, fibres: 1f, foxing: 0.4f);
            for (int hy = 50; hy < 410; hy += 46)
                for (int yy = -3; yy <= 3; yy++)
                    for (int xx = -3; xx <= 3; xx++)
                        if (xx * xx + yy * yy <= 9 && s.In(30 + xx, hy + yy)) s.A[30 + xx + (hy + yy) * s.W] = 0f;
            s.ComputeEdge();

            // O cabecalho impresso, com as caixas de turno, data e pressao.
            var head = new List<List<P2>>();
            Hand title = Cast(11f, 1101);
            title.Tracking = 3f;
            head.AddRange(title.Line("LIVRO DE TURNOS DA CALDEIRA", 216, 46, 0.5f));
            Hand label = Print(7f, 1102);
            head.AddRange(DocInk.Rect(52, 60, 390, 92));
            head.AddRange(DocInk.Line(170, 60, 170, 92));
            head.AddRange(DocInk.Line(280, 60, 280, 92));
            head.AddRange(label.Line("TURNO", 58, 72));
            head.AddRange(label.Line("FOLHA", 176, 72));
            head.AddRange(label.Line("PRESSÃO (LB)", 286, 72));
            head.AddRange(label.Line("N.º " + (1200 + page * 37), 390, 108, 1f));
            DocInk.Ink(s, head, C3.Hex(0x2a3a5a), Type(1.1f, seed + 2));
            Ruled(s, 44, 396, 124, 410, 22f, C3.Hex(0x5a7aa8), 76, C3.Hex(0xb84a4a), seed + 3);

            Hand hand = Plumber(seed + 4, tremor);
            var ink = new List<List<P2>>();
            if (shift.Length > 0) ink.AddRange(hand.Line(shift, 60, 88));
            ink.AddRange(hand.Line(page.ToString(), 190, 88));
            if (page < 4) ink.AddRange(hand.Line(page == 1 ? "160!" : page == 2 ? "175" : "—", 300, 88));

            float y = 146;
            foreach (string entry in entries)
            {
                foreach (string line in hand.Wrap(entry, 300f))
                {
                    ink.AddRange(hand.Line(line, 86, y - 2));
                    y += 22f;
                }

                y += 22f;
            }

            DocInk.Ink(s, ink, page >= 3 ? FadedBrown : BlueBlack, Nib(1.5f, seed + 5));

            // O estrago de cada folha.
            if (page == 1)
            {
                s.Ring(330, 360, 30f, C3.Hex(0x7a5530), 0.45f, seed + 6);
                s.Ring(345, 372, 30f, C3.Hex(0x7a5530), 0.25f, seed + 7, false);
            }
            else if (page == 2)
            {
                // Na margem, as leituras do manometro anotadas a lapis, e um mostrador rabiscado.
                Hand pencil = Hurried(10f, seed + 8);
                var p = pencil.Line("1ª 140", 290, 300);
                p.AddRange(pencil.Line("2ª 158", 290, 318));
                p.AddRange(pencil.Line("3ª 175", 290, 336));
                p.AddRange(DocInk.Circle(330, 380, 20f, 30));
                p.AddRange(DocInk.Line(330, 380, 346, 366));
                for (int t = 0; t < 7; t++)
                {
                    double a = Math.PI * (0.8 + t * 0.233);
                    p.AddRange(DocInk.Line(330 + (float)Math.Cos(a) * 16f, 380 + (float)Math.Sin(a) * 16f, 330 + (float)Math.Cos(a) * 20f, 380 + (float)Math.Sin(a) * 20f));
                }

                DocInk.Ink(s, p, Graphite, Ballpoint(1.1f, seed + 9));
                Thumbprint(s, 370, 240, 11f, C3.Hex(0x2a2a30), 0.45f, seed + 10);
            }
            else if (page == 3)
            {
                // Agua: a tinta escorreu, e o canto ficou com fuligem.
                s.Tint((x, yy) => yy > 200 && yy < 320 ? (DocSurface.Fbm(x * 0.02f, yy * 0.08f, seed + 11, 3) - 0.5f) * 0.9f : 0f, C3.Hex(0x6a6a7a));
                s.Ring(250, 260, 70f, C3.Hex(0x8a7a60), 0.35f, seed + 12, false);
                s.Tint((x, yy) => (x + yy > 700 ? (x + yy - 700) / 120f : 0f) * DocSurface.Fbm(x * 0.05f, yy * 0.05f, seed + 13, 3), Soot);
            }
            else
            {
                // A ultima: quase em branco, e o canto queimado.
                s.Burn(34f, seed + 14, (x, yy) => DocSurface.Clamp01((x - 250f) / 90f) * DocSurface.Clamp01((yy - 280f) / 90f) * 1.6f);
            }

            s.Fold(18, 210, 404, 230, 0.7f, 2.5f);
            return s.Bake(relief: 1.3f);
        }

        // ------------------------------------------------------------------
        // Os papeis da Coroa
        // ------------------------------------------------------------------

        /// <summary>Datilografado: cada letra bate num lugar levemente diferente.</summary>
        private static Hand Typewriter(float size, int seed)
        {
            Hand h = Print(size, seed);
            h.Font = DocFont.RomanThin;
            h.Jitter = 0.2f;
            h.SizeJitter = 0.03f;
            h.Tracking = 2.2f;
            h.LineGap = 1.85f;
            return h;
        }

        private static PixelCanvas InspectionReportDoc()
        {
            var s = new DocSurface(420, 500);
            s.Shape(DocSurface.Sheet(16, 12, 404, 488, 1301, 1f, 1f, 1f, 1f));
            s.Paper(Cream, CreamDark, 1302, age: 0.45f, fibres: 0.7f, foxing: 0.3f);
            s.Fold(16, 172, 404, 172, 1f, 2.5f);
            s.Fold(16, 330, 404, 330, 1f, 2.5f);

            // Timbre da inspetoria.
            var head = new List<List<P2>>();
            head.AddRange(CrownCrest(64, 54, 40f));
            Hand caps = Cast(10f, 1303);
            caps.Tracking = 3f;
            head.AddRange(caps.Line("INSPETORIA DA COROA", 100, 48));
            Hand small = Italic(9f, 1304);
            head.AddRange(small.Line("Serviço de Caldeiras e Vapor", 100, 66));
            head.AddRange(DocInk.Line(40, 84, 380, 84));
            head.AddRange(DocInk.Line(40, 87, 380, 87));
            DocInk.Ink(s, head, IronGall, Type(1.3f, 1305));

            Hand type = Typewriter(9f, 1306);
            var t = new List<List<P2>>();
            t.AddRange(type.Line("RELATÓRIO DE INSPEÇÃO N.º 77", 40, 118));
            t.AddRange(type.Line("ASSUNTO: CALDEIRAS DO DISTRITO DE LATÃO", 40, 136));
            float y = type.Block("Pressão acima do limite em todas as linhas. Válvulas de alívio no fim da vida útil. Corrosão verde nas juntas.", 40, 168, 340, t);
            y = type.Block("RECOMENDO DESLIGAR METADE DAS CALDEIRAS.", 40, y + 8, 340, t);
            t.AddRange(type.Line("Insp. B. Gaxeta", 250, y + 18));
            DocInk.Ink(s, t, C3.Hex(0x262428), new Pen { Width = 1.25f, Contrast = 0.1f, Pressure = 0.4f, Dry = 0.18f, Seed = 1307 });

            // NEGADO: o carimbo grande, atravessado.
            Hand stamp = Cast(30f, 1308);
            stamp.Tracking = 6f;
            var st = stamp.Line("NEGADO", 210, y + 76, 0.5f);
            st.AddRange(DocInk.Rect(86, y + 38, 334, y + 88));
            DocInk.Stamp(s, Rotate(st, 210, y + 63, -12f), DocKit.StampRed, Type(2.6f, 1309), 0.35f);

            // Por baixo, a mao do Rei, em tinta prateada.
            Hand king = Clerk(16f, 1310);
            var k = king.Line("A cidade não pode escurecer.", 60, y + 128);
            k.AddRange(Signature("Argentum Rex", 210, y + 160, 18f, 1311));
            DocInk.Ink(s, k, C3.Hex(0x707688), Nib(1.8f, 1312));

            s.Ring(330, 120, 28f, C3.Hex(0x7a5530), 0.3f, 1315, false);
            return s.Bake(relief: 1.3f);
        }

        private static PixelCanvas DepotSheetDoc()
        {
            var s = new DocSurface(400, 470);
            s.Shape(DocSurface.Sheet(16, 16, 384, 456, 1401, 1.5f, 1f, 1.5f, 6f));
            s.Paper(Manila * 1.04f, ManilaDark, 1402, age: 0.65f, fibres: 1f, foxing: 0.5f);

            var form = new List<List<P2>>();
            Hand caps = Cast(12f, 1403);
            caps.Tracking = 3f;
            form.AddRange(caps.Line("FICHA DE INVENTÁRIO", 200, 64, 0.5f));
            Hand small = Print(7.5f, 1404);
            form.AddRange(small.Line("ORDEM DA COROA — DEPÓSITO DE CALDEIRAS N.º 2", 200, 82, 0.5f));
            form.AddRange(DocInk.Rect(36, 96, 364, 300));
            form.AddRange(DocInk.Line(36, 116, 364, 116));
            form.AddRange(DocInk.Line(250, 96, 250, 300));
            form.AddRange(DocInk.Line(300, 96, 300, 300));
            form.AddRange(small.Line("ITEM", 44, 110));
            form.AddRange(small.Line("QTD.", 256, 110));
            form.AddRange(small.Line("DEST.", 306, 110));
            for (float y = 140; y < 300; y += 24) form.AddRange(DocInk.Line(36, y, 364, y));
            DocInk.Ink(s, form, C3.Hex(0x3a2a20), Type(1.1f, 1405));

            Hand type = Typewriter(8.5f, 1406);
            var t = type.Line("Máscaras de solda (todas)", 44, 134);
            t.AddRange(type.Line("—", 266, 134));
            t.AddRange(type.Line("PRATA", 304, 134));
            t.AddRange(type.Line("Filtros de respiro", 44, 158));
            t.AddRange(type.Line("—", 266, 158));
            t.AddRange(type.Line("PRATA", 304, 158));
            t.AddRange(type.Line("PRIORIDADE: NOBRES.", 44, 182));
            DocInk.Ink(s, t, C3.Hex(0x262428), new Pen { Width = 1.2f, Contrast = 0.1f, Pressure = 0.4f, Dry = 0.15f, Seed = 1407 });

            // A resposta do chefe de manutencao: letra firme, tinta preta, sublinhada.
            Hand chief = Clerk(15f, 1408);
            chief.Slant = 0.1f;
            var c = chief.Line("recolhidas: nenhuma.", 50, 340);
            c.AddRange(chief.Line("não há máscaras neste depósito.", 50, 368));
            c.AddRange(chief.Line("— o chefe de manutenção.", 90, 404));
            c.AddRange(DocInk.Line(56, 346, 196, 344));
            c.AddRange(DocInk.Line(266, 130, 290, 130));
            c.AddRange(chief.Line("0", 272, 156));
            DocInk.Ink(s, c, IronGall, Nib(1.8f, 1409));

            DocInk.NailHole(s, 42, 34, Rust, 1410);
            DocInk.NailHole(s, 358, 32, Rust, 1411);
            s.Tint((x, y) => y > 420 ? (DocSurface.Fbm(x * 0.05f, y * 0.05f, 1412, 3) - 0.45f) * 1.5f : 0f, C3.Hex(0x4a3a28));
            return s.Bake(relief: 1.3f);
        }

        // ------------------------------------------------------------------
        // Placas
        // ------------------------------------------------------------------

        /// <summary>A placa do esgoto: doze nomes riscados, afundada na lama.</summary>
        private static PixelCanvas SewerPlaqueDoc()
        {
            DocSurface s = CastPlaque(470, 300, new[] { "COLETOR DE CONDENSADO N.º 4", "NÃO ENTRAR SEM MÁSCARA." }, 12f, 1501, 0.4f, 0,
                out float bottom);
            string[] names = { "TRAVA   MOLA   GAXETA", "BUCHA   PISTÃO   CALDO", "FUMO   REBITE-9   ARRUELA", "NÍVEL   SOLDINHA   FERR" };
            for (int i = 0; i < names.Length; i++)
            {
                Hand h = Scratcher(10f, 1510 + i * 7, (i % 2 == 0 ? 0.06f : -0.04f));
                NailLine(s, h, names[i], 46 + i * 6, bottom + 34 + i * 24, BrassBright, 1520 + i);
            }

            // A lama do esgoto subindo pela placa.
            s.Tint((x, y) =>
            {
                float top = 240f + (DocSurface.Fbm(x * 0.02f, 0.5f, 1530, 3) - 0.5f) * 40f;
                return y > top ? 0.95f : y > top - 6 ? 0.5f : 0f;
            }, C3.Hex(0x3e3424));
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                {
                    float top = 240f + (DocSurface.Fbm(x * 0.02f, 0.5f, 1530, 3) - 0.5f) * 40f;
                    int i = x + y * s.W;
                    if (y > top && s.A[i] > 0f)
                    {
                        s.Ht[i] = 6f + DocSurface.Fbm(x * 0.1f, y * 0.1f, 1531, 3) * 3f;
                        s.Gloss[i] = 0.5f;
                    }
                }

            return s.Bake(relief: 1.1f);
        }

        /// <summary>A placa da torre: esmalte azul de rua, lascado, e a mesma pergunta a giz em tres letras.</summary>
        private static PixelCanvas TowerLineDoc()
        {
            var s = new DocSurface(470, 260);
            s.Shape(DocSurface.Sheet(10, 10, 460, 200, 1601, corner: 14f));
            s.Metal(C3.Hex(0x1e3a78), 1602, 0f, bevel: 7f, gloss: 0.9f, wear: 0.2f);

            // Esmalte lascado: o ferro aparece por baixo, enferrujando.
            s.Tint((x, y) => (DocSurface.Fbm(x * 0.05f, y * 0.05f, 1603, 4) - 0.7f) * 6f, Iron);
            s.Tint((x, y) => (DocSurface.Fbm(x * 0.05f, y * 0.05f, 1603, 4) - 0.74f) * 6f, Rust);
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                {
                    int i = x + y * s.W;
                    if (s.A[i] > 0f && DocSurface.Fbm(x * 0.05f, y * 0.05f, 1603, 4) > 0.7f)
                    {
                        s.Ht[i] -= 1.2f;
                        s.Gloss[i] = 0.15f;
                    }
                }

            // Filete e letras brancas de esmalte, levemente em relevo.
            var white = DocInk.Rect(24, 24, 446, 186);
            Hand h = Cast(13f, 1604);
            h.Font = DocFont.RomanPrint;
            h.Tracking = 1.5f;
            white.AddRange(h.Line("LINHA 3 — AVENIDA DAS FACETAS", 235, 92, 0.5f));
            Hand h2 = h.Clone();
            h2.Size = 11f;
            white.AddRange(h2.Line("PRIORIDADE: NOBRES.", 235, 132, 0.5f));
            DocInk.Emboss(s, white, 1.6f, 0.8f, 0.8f, C3.Hex(0xece8dc), 1f, 0.9f);
            s.Tint((x, y) => (DocSurface.Fbm(x * 0.05f, y * 0.05f, 1603, 4) - 0.7f) * 6f, Iron * 1.1f);

            s.Rivet(30, 105, 5f, C3.Hex(0x9a9ca4), 0.7f, true, 0.4f);
            s.Rivet(440, 105, 5f, C3.Hex(0x9a9ca4), 0.7f, true, 1.4f);

            // A parede de baixo, onde escreveram a giz.
            s.AddShape(DocSurface.Sheet(0, 196, 470, 260, 1605, 0f, 0f, 0f, 3f));
            for (int y = 196; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                {
                    int i = x + y * s.W;
                    if (s.A[i] <= 0f || y < 201) continue;
                    s.Col[i] = C3.Hex(0x3a3a42) * (0.9f + DocSurface.Fbm(x * 0.05f, y * 0.05f, 1606, 3) * 0.25f);
                    s.Ht[i] = -2f;
                    s.Gloss[i] = 0.05f;
                }

            var q = Hurried(18f, 1607).Line("sempre eles.", 235, 240, 0.5f);
            DocInk.Chalk(s, q, ChalkWhite, Stick(2.1f, 1610));
            return s.Bake(relief: 1.1f);
        }

        /// <summary>O cano mestre: a flange curva de um cano do tamanho de uma casa, com a letra fundida.</summary>
        private static PixelCanvas MasterPipeDoc()
        {
            var s = new DocSurface(460, 320);
            s.Shape(DocSurface.Sheet(10, 10, 450, 310, 1701, 0f, 4f, 0f, 4f));
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                {
                    int i = x + y * s.W;
                    if (s.A[i] <= 0f) continue;
                    float u = (x - 230f) / 220f;
                    float bulge = (float)Math.Sqrt(Math.Max(0f, 1f - u * u));
                    s.Col[i] = Brass * 0.75f * (0.85f + DocSurface.Fbm(x * 0.6f, y * 0.02f, 1702, 2) * 0.2f);
                    s.Ht[i] = bulge * 40f;
                    s.Gloss[i] = 0.45f;
                }

            // Os aneis de flange, em cima e embaixo, com parafusos grossos.
            foreach (int fy in new[] { 34, 286 })
            {
                for (int y = fy - 14; y <= fy + 14; y++)
                    for (int x = 10; x < 450; x++)
                    {
                        int i = x + y * s.W;
                        if (!s.In(x, y)) continue;
                        s.Ht[i] += 6f - Math.Abs(y - fy) * 0.2f;
                        s.Col[i] = BrassDark * 1.4f;
                    }

                for (int x = 40; x < 440; x += 52) s.Rivet(x, fy, 7f, BrassDark * 1.6f, 0.5f);
            }

            Hand cast = Cast(26f, 1703);
            cast.Tracking = 4f;
            var t = cast.Line("PARA A CIDADE", 230, 132, 0.5f);
            t.AddRange(cast.Line("DE PRATA.", 230, 180, 0.5f));
            DocInk.Emboss(s, t, 2.2f, 3f, 1.4f, BrassBright, 0.5f);

            // Seco e corroido: o azinhavre come o cano por dentro e por fora.
            s.Patina(0.5f, 1704, Verdigris * 0.85f, VerdigrisLight, 1.6f);

            return s.Bake(relief: 0.9f);
        }

        /// <summary>
        /// O bilhete na grade do mirante: um pedaco de papel amassado, na mesma
        /// letra redonda da carta. Quem escreveu olhava pra cima, pra cidade.
        /// </summary>
        private static PixelCanvas GalleryViewDoc()
        {
            var s = new DocSurface(360, 220);
            s.Shape(DocSurface.Sheet(16, 16, 344, 204, 1801, 4f, 3f, 5f, 3f, corner: 2f));
            s.Paper(C3.Hex(0xe2d6bc), C3.Hex(0xa08a64), 1802, age: 0.7f, fibres: 1f, foxing: 0.8f);

            // Amassado e desamassado: dobras tortas pra todo lado.
            s.Fold(16, 70, 344, 110, 1.2f, 2.5f);
            s.Fold(120, 16, 150, 204, 1.1f, 2.5f);
            s.Fold(250, 16, 220, 204, 0.9f, 2.5f);
            s.Fold(16, 170, 344, 140, 0.8f, 2f);

            // A mesma mao da carta, mais apertada: escrita com pressa.
            Hand pai = Careful(20f, 2803);
            pai.Jitter = 0.35f;
            pai.Wave = 1f;
            var ink = pai.Line("Me desculpa,", 46, 92);
            ink.AddRange(pai.Line("minha Pequena.", 92, 136));
            DocInk.Ink(s, ink, FadedBrown, Nib(1.8f, 1803));

            // Um pingo que borrou a tinta, e o furo do arame da grade.
            s.Ring(270, 150, 16f, C3.Hex(0x806848), 0.35f, 1804, false);
            DocInk.NailHole(s, 36, 34, Rust, 1805, false);
            return s.Bake(relief: 1.4f);
        }

        private static PixelCanvas ElevatorSignDoc()
        {
            DocSurface s = CastPlaque(440, 220, new[] { "ELEVADOR DE SERVIÇO", "DO DISTRITO DE LATÃO", "AO LABORATÓRIO" }, 15f, 1901,
                0.2f, 1, out float bottom, 70f);
            return s.Bake(relief: 1.1f);
        }

        // ------------------------------------------------------------------
        // Riscado na chapa
        // ------------------------------------------------------------------

        private static PixelCanvas HastyScratchDoc()
        {
            var s = new DocSurface(440, 260);
            s.Shape(DocSurface.Sheet(8, 8, 432, 252, 2001, 2f, 2f, 2f, 2f));
            s.Metal(C3.Hex(0x8a8c94), 2002, 0f, bevel: 3f, gloss: 0.5f, wear: 1.2f);
            s.Tint((x, y) => (DocSurface.Fbm(x * 0.03f, y * 0.03f, 2003, 4) - 0.55f) * 1.6f, C3.Hex(0x4a4c54));

            // Riscado as pressas: cortes longos, fundos, que passam da conta.
            Hand h = Scratcher(30f, 2004, 0.18f);
            h.Jitter = 1.2f;
            h.Wave = 3f;
            h.Tracking = 0.5f;
            var a = DocInk.Scratchify(h.Line("A PRATA BEBE,", 34, 104), 2005, 1.6f, 4f, 0.7f);
            var b = DocInk.Scratchify(h.Line("O LATÃO SUA.", 60, 184), 2006, 1.6f, 4f, 0.7f);
            DocInk.Carve(s, DocKit.Join(a, b), 1.7f, 3.2f, C3.Hex(0xeef0f2), 0.95f, 2007, chips: 0.5f);

            s.Rivet(24, 24, 5f, Iron * 1.3f, 0.4f);
            s.Rivet(416, 236, 5f, Iron * 1.3f, 0.4f);
            return s.Bake(relief: 1.2f);
        }

        private static PixelCanvas VentWallDoc(bool purified)
        {
            var s = new DocSurface(480, 380);
            s.Shape(DocSurface.Sheet(8, 8, 472, 372, 2101, 1.5f, 1.5f, 1.5f, 1.5f));
            s.Metal(C3.Hex(0x7a7c86), 2102, 0f, bevel: 3f, gloss: 0.35f, wear: 1f);
            s.Tint((x, y) => (DocSurface.Fbm(x * 0.03f, y * 0.03f, 2103, 4) - 0.55f) * 1.4f, C3.Hex(0x3e4048));

            // O respiro do teto: aletas, e a luz amarela vazando por elas.
            for (int y = 14; y < 74; y++)
                for (int x = 150; x < 330; x++)
                {
                    int i = x + y * s.W;
                    bool slot = (y - 18) % 10 < 5 && y > 18 && y < 70 && x > 156 && x < 324;
                    s.Ht[i] = slot ? -4f : 2f;
                    s.Col[i] = slot ? C3.Hex(0xe8c878) * (1f - Math.Abs(x - 240) / 200f) : C3.Hex(0x3a3c44);
                    s.Gloss[i] = slot ? 0f : 0.5f;
                }

            for (int y = 74; y < 200; y++)
                for (int x = 120; x < 360; x++)
                {
                    float k = Math.Max(0f, 1f - Math.Abs(x - 240) / (60f + (y - 74) * 0.8f)) * Math.Max(0f, 1f - (y - 74) / 126f);
                    if (k > 0f) s.Col[x + y * s.W] = C3.Lerp(s.Col[x + y * s.W], C3.Hex(0xf0dca0), k * 0.25f);
                }

            NailLine(s, Scratcher(13f, 2104, 0.02f), "ELES NUNCA DESCERAM ATÉ AQUI.", 240, 110, C3.Hex(0xf2f2ee), 2105, 2.4f, 0.5f);
            NailLine(s, Scratcher(13f, 2106, 0.02f), "NEM PARA VER.", 240, 136, C3.Hex(0xf2f2ee), 2107, 2.4f, 0.5f);

            // A outra mao, de outro dia: cursiva miuda, funda e paciente.
            Hand other = Careful(15f, 2108);
            float y2 = 186;
            foreach (string line in other.Wrap("daqui eu via as luzes da Avenida das Facetas pelo respiro do teto. apagaram-se uma a uma.", 400f))
            {
                List<List<P2>> cut = DocInk.Scratchify(other.Line(line, 40, y2), 2109 + (int)y2, 0.7f, 0.5f, 0.2f);
                DocInk.Carve(s, cut, 1.05f, 1.8f, C3.Hex(0xdcdcd8), 0.8f, 2110, chips: 0.15f);
                y2 += 26f;
            }

            if (purified)
            {
                var c = Hurried(17f, 2111).Line("um desceu. era de sucata.", 70, y2 + 30);
                DocInk.Chalk(s, c, ChalkWhite, Stick(2f, 2112));
            }

            return s.Bake(relief: 1.2f);
        }

        private static PixelCanvas BoilerDoorDoc()
        {
            var s = new DocSurface(400, 500);
            s.Shape(DocSurface.Sheet(14, 10, 386, 490, 2201, corner: 6f));
            s.Metal(C3.Hex(0x585a62), 2202, 1.5708f, bevel: 6f, gloss: 0.35f, wear: 1f);
            s.Tint((x, y) => (DocSurface.Fbm(x * 0.03f, y * 0.03f, 2203, 4) - 0.55f) * 1.6f, Rust);

            // Os quatro paineis prensados e a faixa de ferro com a tranca.
            foreach ((int x0, int y0, int x1, int y1) in new[] { (40, 40, 190, 200), (210, 40, 360, 200), (40, 290, 190, 460), (210, 290, 360, 460) })
                DocInk.Emboss(s, DocInk.Rect(x0, y0, x1, y1), 3f, 2f, 2.5f, C3.Hex(0x6a6c74), 0.3f, 0.4f);
            for (int y = 222; y < 268; y++)
                for (int x = 14; x < 386; x++)
                {
                    int i = x + y * s.W;
                    if (s.A[i] <= 0f) continue;
                    s.Ht[i] += 5f;
                    s.Col[i] = C3.Hex(0x3a3a40) * (0.9f + DocSurface.Fbm(x * 0.05f, y * 0.3f, 2204, 2) * 0.2f);
                }

            for (int x = 34; x < 380; x += 40)
            {
                s.Rivet(x, 230, 4f, C3.Hex(0x6a6a72), 0.5f);
                s.Rivet(x, 260, 4f, C3.Hex(0x6a6a72), 0.5f);
            }

            // A tranca soldada pelo lado de fora: a lingueta e um cordao de solda grosso.
            for (int y = 232; y < 258; y++)
                for (int x = 300; x < 390; x++)
                {
                    int i = x + y * s.W;
                    s.A[i] = 1f;
                    s.Col[i] = C3.Hex(0x4e4e56);
                    s.Ht[i] = 10f;
                    s.Gloss[i] = 0.6f;
                }

            Weld(s, new List<P2> { new P2(300, 228), new P2(300, 262) }, 3.5f, C3.Hex(0x9a9aa4), 2205);
            Weld(s, new List<P2> { new P2(380, 228), new P2(380, 262) }, 3.5f, C3.Hex(0x9a9aa4), 2206);

            C3 bright = C3.Hex(0xe8e8e4);
            NailLine(s, Scratcher(13f, 2207, 0.05f), "NÃO ABRAM.", 115, 90, bright, 2208, 2.3f, 0.5f);
            NailLine(s, Scratcher(12f, 2209, -0.05f), "OUÇAM O QUE", 285, 86, bright, 2210, 2.3f, 0.5f);
            NailLine(s, Scratcher(12f, 2211, -0.05f), "OUVIREM.", 285, 110, bright, 2212, 2.3f, 0.5f);
            NailLine(s, Hurried(18f, 2213), "ele pediu.", 115, 150, C3.Hex(0xffffff), 2214, 2.6f, 0.5f);

            // "desculpe, chefe.": riscado tantas vezes no mesmo lugar que virou uma vala.
            for (int pass = 0; pass < 5; pass++)
            {
                Hand h = Hurried(22f, 2220);
                h.Seed = 2220 + pass;
                h.Jitter = 0.6f;
                List<List<P2>> cut = DocInk.Scratchify(h.Line("desculpe, chefe.", 60 + pass * 0.7f, 380 + pass * 0.5f), 2230 + pass, 0.9f, 1.5f, 0.3f);
                DocInk.Carve(s, cut, 1.3f + pass * 0.15f, 1.4f, C3.Hex(0xf6f0e0), 0.8f, 2240 + pass, chips: 0.3f);
            }

            return s.Bake(relief: 1.2f);
        }

        // ------------------------------------------------------------------
        // Giz e tinta nas paredes de ferro
        // ------------------------------------------------------------------

        private static PixelCanvas ChalkMaskDoc()
        {
            DocSurface s = IronSheetWall(440, 380, 2301, 0.5f);

            // O cabide, com o gancho vazio.
            s.Rivet(390, 70, 6f, C3.Hex(0x8a8a92), 0.6f);
            var hook = new List<List<P2>> { new List<P2> { new P2(390, 70), new P2(390, 104), new P2(396, 116), new P2(406, 110) } };
            float[] hd = DocInk.Distance(s, hook, 3f);
            for (int i = 0; i < hd.Length; i++)
                if (hd[i] < 2.4f)
                {
                    s.Col[i] = IronLight;
                    s.Ht[i] = 8f + (2.4f - hd[i]) * 2f;
                    s.Gloss[i] = 0.7f;
                }

            // A mascara de soldador, desenhada a giz: casco, visor, tira.
            var mask = new List<List<P2>>();
            var shell = new List<P2>();
            for (int i = 0; i <= 40; i++)
            {
                double a = Math.PI * (0.9 + i * 1.2 / 40.0);
                shell.Add(new P2(190 + (float)Math.Cos(a) * 110f, 150 + (float)Math.Sin(a) * 96f));
            }

            shell.Add(new P2(260, 230));
            shell.Add(new P2(240, 248));
            shell.Add(new P2(140, 248));
            shell.Add(new P2(120, 230));
            shell.Add(shell[0]);
            mask.Add(shell);
            mask.AddRange(DocInk.Rect(130, 120, 250, 170));
            mask.AddRange(DocInk.Line(136, 160, 160, 128));
            mask.AddRange(DocInk.Line(150, 166, 180, 126));
            mask.AddRange(DocInk.Line(80, 128, 30, 112));
            mask.AddRange(DocInk.Line(300, 128, 350, 112));
            mask.AddRange(DocInk.Circle(190, 214, 8f, 14));
            mask = DocKit.Rotate(mask, 190, 160, -4f);
            for (int i = 0; i < mask.Count; i++)
                for (int k = 0; k < mask[i].Count; k++)
                    mask[i][k] = new P2(mask[i][k].X + (DocSurface.Rand(i, k, 2302) - 0.5f) * 3f, mask[i][k].Y + (DocSurface.Rand(k, i, 2302) - 0.5f) * 3f);
            DocInk.Chalk(s, mask, ChalkWhite, Stick(3f, 2303), 0.15f);

            DocInk.Chalk(s, Scratcher(14f, 2304, 0.04f).Line("QUEM TEM MÁSCARA RESPIRA.", 220, 300, 0.5f), ChalkWhite, Stick(2.2f, 2305));
            return s.Bake(relief: 1.1f);
        }

        private static PixelCanvas ChalkTallyDoc()
        {
            DocSurface s = IronSheetWall(500, 340, 2401, 0.35f, false);
            DocInk.Chalk(s, Scratcher(22f, 2402, 0f).Line("DIAS SEM SUBIR.", 250, 58, 0.5f), ChalkWhite, Stick(3f, 2403));

            var tally = new List<List<P2>>();
            int groups = 0;
            for (int row = 0; row < 4; row++)
                for (int col = 0; col < 9; col++)
                {
                    if (row == 3 && col > 5) break;
                    float gx = 40 + col * 50 + (DocSurface.Rand(row, col, 2404) - 0.5f) * 6f;
                    float gy = 96 + row * 56 + (DocSurface.Rand(col, row, 2404) - 0.5f) * 6f;
                    bool last = row == 3 && col == 5;
                    int sticks = last ? 3 : 4;
                    for (int i = 0; i < sticks; i++)
                        tally.Add(new List<P2> { new P2(gx + i * 8, gy), new P2(gx + i * 8 + 1.5f, gy + 34) });
                    if (!last) tally.Add(new List<P2> { new P2(gx - 6, gy + 28), new P2(gx + 32, gy + 6) });
                    groups++;
                }

            // A mao vai ficando cansada: os ultimos grupos mais fracos.
            DocInk.Chalk(s, tally, ChalkWhite, Stick(2.6f, 2405), 0.12f);
            s.Tint((x, y) => y > 250 ? 0.12f : 0f, C3.Hex(0x45454e));
            return s.Bake(relief: 1.1f);
        }

        private static PixelCanvas ChalkArrowDoc()
        {
            DocSurface s = IronSheetWall(360, 460, 2501, 0.45f, false);
            var arrow = new List<List<P2>>
            {
                new List<P2> { new P2(70, 440), new P2(68, 300), new P2(72, 160), new P2(70, 40) },
                new List<P2> { new P2(30, 92), new P2(70, 36), new P2(112, 90) },
                new List<P2> { new P2(84, 440), new P2(82, 160), new P2(84, 60) },
            };
            DocInk.Chalk(s, arrow, ChalkWhite, Stick(4f, 2502), 0.2f);
            Hand big = Scratcher(24f, 2503, 0.02f);
            DocInk.Chalk(s, big.Line("ELEVADOR", 116, 92), ChalkWhite, Stick(3.2f, 2504));
            Hand note = Careful(18f, 2505);
            var n = note.Line("a escada caiu.", 120, 230);
            n.AddRange(note.Line("subam por aqui.", 120, 262));
            DocInk.Chalk(s, n, C3.Hex(0xe8e0c4), Stick(2.2f, 2506));
            return s.Bake(relief: 1.1f);
        }

        private static PixelCanvas PaintArrowDoc()
        {
            DocSurface s = IronSheetWall(500, 300, 2601, 0.6f);

            // Tinta amarela velha, em estencil: letras com as pontes do molde.
            Hand stencil = Cast(26f, 2602);
            stencil.Font = DocFont.RomanPrint;
            stencil.Tracking = 4f;
            var old = stencil.Line("DEPÓSITO", 40, 86);
            old.AddRange(new List<List<P2>> { new List<P2> { new P2(300, 74), new P2(360, 74) } });
            old.AddRange(new List<List<P2>>
            {
                new List<P2> { new P2(380, 76), new P2(470, 76) },
                new List<P2> { new P2(446, 56), new P2(472, 76), new P2(446, 96) },
            });
            var masks = stencil.Line("MÁSCARAS", 70, 160);
            Paint(s, old, C3.Hex(0xc8962c), 5f, 2603, 0.25f, 0.85f);
            Paint(s, masks, C3.Hex(0xc8962c), 5f, 2604, 0.25f, 0.85f);
            s.Tint((x, y) => (DocSurface.Fbm(x * 0.06f, y * 0.06f, 2605, 3) - 0.6f) * 2f, Rust);

            // Riscado com tinta vermelha fresca, e NADA por cima, escorrendo antes de secar.
            var cross = new List<List<P2>>
            {
                new List<P2> { new P2(60, 150), new P2(200, 140), new P2(340, 148) },
                new List<P2> { new P2(64, 160), new P2(210, 152), new P2(336, 158) },
            };
            Paint(s, cross, C3.Hex(0xb02820), 7f, 2606, 0.6f);
            Hand rough = Scratcher(40f, 2607, 0.1f);
            rough.Jitter = 2f;
            Paint(s, rough.Line("NADA", 200, 252), C3.Hex(0xb02820), 8f, 2608, 1f);
            return s.Bake(relief: 1.1f);
        }

        // ------------------------------------------------------------------
        // O que so existe uma vez
        // ------------------------------------------------------------------

        private static PixelCanvas CrownPosterDoc()
        {
            var s = new DocSurface(380, 520);
            s.Shape(DocSurface.Sheet(14, 12, 366, 508, 2701, 4f, 3f, 5f, 10f));
            s.Paper(C3.Hex(0xd8d2c0), C3.Hex(0x988e74), 2702, age: 0.55f, fibres: 0.6f);

            // Litografia em duas cores: um azul-noite de fundo em retícula e a prata.
            Halftone(s, (x, y) => y > 30 && y < 400 ? 0.65f + (float)Math.Sin(y * 0.02f) * 0.1f : 0f, C3.Hex(0x1e2a4a), 4.2f, 0.26f);
            Halftone(s, (x, y) =>
            {
                float d = (float)Math.Sqrt((x - 190) * (x - 190) + (y - 150) * (y - 150));
                return y > 30 && y < 400 ? Math.Max(0f, 1f - d / 150f) * 0.8f : 0f;
            }, C3.Hex(0xa8b4cc), 4.2f, 1.1f);

            // Raios de luz saindo da coroa, impressos em prata.
            var rays = new List<List<P2>>();
            for (int r = 0; r < 18; r++)
            {
                double a = r * Math.PI * 2 / 18;
                rays.AddRange(DocInk.Line(190 + (float)Math.Cos(a) * 70f, 130 + (float)Math.Sin(a) * 70f,
                    190 + (float)Math.Cos(a) * 170f, 130 + (float)Math.Sin(a) * 170f));
            }

            DocInk.Ink(s, DocKit.Clip(s, rays), C3.Hex(0xc8d0e0), new Pen { Width = 3f, Contrast = 0f, Opacity = 0.5f, Seed = 2703 });
            DocInk.Ink(s, DocInk.Crown(190, 120, 110f), C3.Hex(0xe8ecf4), new Pen { Width = 5f, Contrast = 0.2f, Opacity = 1f, Seed = 2704 });
            Hand num = Cast(42f, 2705);
            num.Tracking = 6f;
            DocInk.Ink(s, num.Line("925", 190, 248, 0.5f), C3.Hex(0xf0f2f8), new Pen { Width = 4.5f, Contrast = 0.4f, Opacity = 1f, Seed = 2706 });

            Hand slogan = Cast(14f, 2707);
            slogan.Tracking = 2f;
            var t = slogan.Line("O VAPOR É O SANGUE", 190, 300, 0.5f);
            t.AddRange(slogan.Line("DA CIDADE.", 190, 324, 0.5f));
            Hand slogan2 = Cast(16f, 2708);
            slogan2.Tracking = 2f;
            t.AddRange(slogan2.Line("CADA OPERÁRIO,", 190, 358, 0.5f));
            t.AddRange(slogan2.Line("UMA GOTA.", 190, 384, 0.5f));
            DocInk.Ink(s, t, C3.Hex(0xe4e8f0), new Pen { Width = 2f, Contrast = 0.3f, Opacity = 1f, Seed = 2709 });

            // A faixa de baixo, sem tinta: o rodape da grafica e o carvao de alguem.
            DocInk.Ink(s, Print(6f, 2710).Line("TIPOGRAFIA DA COROA · CRAVEJADA", 190, 420, 0.5f), C3.Hex(0x4a4a5a), Type(0.9f, 2711));

            // Cola de parede: o cartaz enrugou, e um canto descolou.
            s.Fold(14, 200, 366, 220, 0.6f, 3f);
            s.Fold(120, 12, 150, 508, 0.5f, 3f);
            s.Tint((x, y) => (DocSurface.Fbm(x * 0.03f, y * 0.03f, 2714, 3) - 0.55f) * 0.6f, C3.Hex(0x8a7a50));
            return s.Bake(relief: 1.3f);
        }

        private static PixelCanvas LetterDoc()
        {
            var s = new DocSurface(420, 420);
            s.Shape(DocSurface.Sheet(16, 16, 404, 404, 2801, 1.2f, 1.2f, 1.2f, 1.2f, corner: 3f));
            s.Paper(C3.Hex(0xe2d6bc), C3.Hex(0xa08a64), 2802, age: 0.6f, fibres: 1f, foxing: 0.7f);
            s.Fold(210, 16, 210, 404, 1.4f, 3f);
            s.Fold(16, 210, 404, 210, 1.4f, 3f);

            // Letra de quem estudou pouco e capricha: redonda, cuidadosa, a tinta acabando.
            Hand pai = Careful(17f, 2803);
            pai.Jitter = 0.25f;
            pai.Wave = 0.9f;
            var ink = pai.Line("Pequena,", 40, 64);
            float y = 106;
            string[] paras =
            {
                "O vapor está mais pesado esta semana. Não venha me visitar no turno. Fique no distrito de cobre, perto das janelas.",
                "Se mandarem todo mundo descer pela calha, não tenha medo. Eu desço os canos atrás de você. Prometo.",
            };
            foreach (string p in paras)
            {
                bool first = true;
                foreach (string line in pai.Wrap(p, 330f))
                {
                    ink.AddRange(pai.Line(line, first ? 70 : 40, y));
                    y += 27f;
                    first = false;
                }

                y += 12f;
            }

            DocInk.Ink(s, ink, FadedBrown, Nib(1.6f, 2804));

            // Um desenho de canto: uma janelinha com sol, feito pra ela.
            var doodle = DocInk.Rect(300, 330, 360, 380);
            doodle.AddRange(DocInk.Line(330, 330, 330, 380));
            doodle.AddRange(DocInk.Line(300, 355, 360, 355));
            doodle.AddRange(DocInk.Circle(372, 322, 8f, 14));
            DocInk.Ink(s, doodle, FadedBrown, Ballpoint(1.2f, 2805));

            // O furo do rebite que a prendia atras do cano, com ferrugem.
            DocInk.NailHole(s, 60, 360, Rust, 2806, false);
            s.Spot(60, 360, 4f, Soot, 0.9f);
            s.Ring(250, 330, 40f, C3.Hex(0x806848), 0.3f, 2807, false);
            return s.Bake(relief: 1.4f);
        }

        private static PixelCanvas ChiefDiaryDoc()
        {
            var s = new DocSurface(520, 360);

            // A capa de couro, aberta, e as duas paginas.
            s.Shape(DocSurface.Sheet(10, 10, 510, 350, 2901, corner: 10f));
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                {
                    int i = x + y * s.W;
                    if (s.A[i] <= 0f) continue;
                    float n = DocSurface.Fbm(x * 0.08f, y * 0.08f, 2902, 4);
                    s.Col[i] = C3.Hex(0x4a2a1a) * (0.8f + n * 0.4f);
                    s.Ht[i] = n * 1.6f + Math.Min(s.Edge[i], 6f) * 0.4f;
                    s.Gloss[i] = 0.25f;
                }

            var pages = new DocSurface(s.W, s.H);
            pages.Shape((x, y) => x >= 26 && x <= 494 && y >= 22 && y <= 338);
            pages.Paper(C3.Hex(0xe0d4b4), C3.Hex(0xa08a60), 2903, age: 0.5f, fibres: 0.8f);
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                {
                    int i = x + y * s.W;
                    if (pages.A[i] <= 0f) continue;
                    float u = Math.Abs(x - 260f);
                    pages.Ht[i] = 4f + (float)Math.Sqrt(u) * 1.6f;
                }

            Hand chief = Clerk(15f, 2904);
            chief.Slant = 0.08f;
            var ink = new List<List<P2>>();
            float y0 = 64;
            foreach (string p in new[]
                     {
                         "Se as caldeiras pararem, o vapor para. Sem vapor, o pessoal aqui de baixo congela antes mesmo de o Óxido chegar.",
                         "Vou ficar com elas e manter a pressão. Tranquem a porta por fora e não abram, escutem o que escutarem.",
                     })
            {
                y0 = chief.Block(p, 44, y0, 196, ink) + 12;
            }

            DocInk.Ink(pages, ink, IronGall, Nib(1.5f, 2905));

            Hand sign = Clerk(14f, 2908);
            sign.Slant = 0.08f;
            var last = sign.Line("Esse é o meu", 300, 286);
            last.AddRange(sign.Line("sacrifício.", 320, 308));
            last.AddRange(Clerk(10f, 2909).Line("— o chefe de manutenção", 290, 330));
            DocInk.Ink(pages, last, IronGall, Nib(1.6f, 2907));

            // Na pagina da direita, so a marca de uma mao, queimada no papel.
            var palm = new List<List<P2>>();
            palm.AddRange(DocInk.Circle(378, 214, 34f, 30));
            float[] fingersX = { 340, 362, 386, 408 };
            float[] fingersL = { 60, 76, 74, 58 };
            for (int f = 0; f < 4; f++) palm.Add(new List<P2> { new P2(fingersX[f], 190), new P2(fingersX[f] + (f - 1.5f) * 6f, 190 - fingersL[f]) });
            palm.Add(new List<P2> { new P2(344, 228), new P2(316, 200), new P2(304, 176) });
            float[] pd = DocInk.Distance(pages, palm, 20f);
            for (int i = 0; i < pd.Length; i++)
            {
                if (pages.A[i] <= 0f) continue;
                int x = i % s.W, y = i / s.W;
                float n = DocSurface.Fbm(x * 0.12f, y * 0.12f, 2906, 3);
                float pdx = x - 378f, pdy = y - 214f;
                float inPalm = pdx * pdx + pdy * pdy < 34f * 34f ? 1f : 0f;
                float core = Math.Max(inPalm, pd[i] < 9f + n * 4f ? 1f : 0f);
                float scorch = inPalm > 0f ? 0f : DocSurface.Clamp01(1f - (pd[i] - 9f) / 10f);
                if (core > 0f)
                {
                    pages.Col[i] = C3.Lerp(C3.Hex(0x1a100a), C3.Hex(0x3a1e0e), n);
                    pages.Ht[i] -= 1.2f;
                }
                else if (scorch > 0f) pages.Col[i] = C3.Lerp(pages.Col[i], pages.Col[i] * C3.Hex(0xa86a30), scorch * 0.9f);
            }

            for (int i = 0; i < s.A.Length; i++)
            {
                if (pages.A[i] <= 0f) continue;
                s.Col[i] = pages.Col[i];
                s.Ht[i] = pages.Ht[i];
                s.Gloss[i] = 0.02f;
            }

            // A fita marcadora e a costura no meio.
            for (int y = 22; y < 360; y++)
                for (int x = 256; x < 264; x++)
                {
                    int i = x + y * s.W;
                    if (y > 338 && (x - 256) > (y - 338) * 0.4f + 3) continue;
                    s.A[i] = 1f;
                    s.Col[i] = C3.Hex(0x7a1a1a) * (0.9f + (x - 256) * 0.03f);
                    s.Ht[i] = 6f + (float)Math.Sin((x - 256) * 0.4f) * 0.6f;
                }

            s.ComputeEdge();
            return s.Bake(relief: 1.2f);
        }

        private static PixelCanvas WeldingMaskDoc()
        {
            var s = new DocSurface(440, 380);

            // O casco de fibra, rachado ao meio, com o visor verde.
            s.Shape((x, y) =>
            {
                float dx = (x - 220f) / (150f + (y - 40f) * 0.12f), dy = (y - 190f) / 175f;
                if (dx * dx + dy * dy > 1f || y > 352) return false;
                // A rachadura: um vao torto descendo pelo meio.
                float crack = 300f + (float)Math.Sin(y * 0.09f) * 8f + (y - 180f) * 0.12f;
                return Math.Abs(x - crack) > 2.5f + DocSurface.Rand(0, y, 3001) * 1.5f || y < 40;
            });
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                {
                    int i = x + y * s.W;
                    if (s.A[i] <= 0f) continue;
                    float dx = (x - 220f) / (150f + (y - 40f) * 0.12f), dy = (y - 190f) / 175f;
                    float dome = (float)Math.Sqrt(Math.Max(0f, 1f - dx * dx - dy * dy));
                    float n = DocSurface.Fbm(x * 0.1f, y * 0.1f, 3002, 3);
                    s.Col[i] = C3.Hex(0x2e2a28) * (0.85f + n * 0.3f);
                    s.Ht[i] = dome * 30f + n * 1.2f;
                    s.Gloss[i] = 0.3f;
                }

            // Visor.
            for (int y = 120; y < 190; y++)
                for (int x = 120; x < 320; x++)
                {
                    int i = x + y * s.W;
                    if (s.A[i] <= 0f) continue;
                    bool frame = x < 128 || x > 312 || y < 128 || y > 182;
                    s.Col[i] = frame ? C3.Hex(0x4a4a50) : C3.Lerp(C3.Hex(0x0e2a1c), C3.Hex(0x2a6a4a), Math.Max(0f, 1f - Math.Abs((x - y * 1.4f) - 20f) / 40f));
                    s.Ht[i] += frame ? 3f : -1f;
                    s.Gloss[i] = frame ? 0.6f : 1f;
                }

            // A tira de latao por dentro, dobrada pra fora, com o que foi gravado a prego.
            for (int y = 236; y < 300; y++)
                for (int x = 80; x < 360; x++)
                {
                    int i = x + y * s.W;
                    if (!s.In(x, y)) continue;
                    s.Col[i] = Brass * (0.9f + DocSurface.Fbm(x * 0.05f, y * 0.5f, 3003, 2) * 0.15f);
                    s.Ht[i] += 4f + Math.Min(Math.Min(y - 236, 300 - y), 3);
                    s.Gloss[i] = 0.8f;
                }

            s.Rivet(90, 268, 4f, BrassDark * 1.4f, 0.6f);
            s.Rivet(350, 268, 4f, BrassDark * 1.4f, 0.6f);
            Hand small = Careful(17f, 3004);
            NailLine(s, small, "para a Pequena,", 220, 264, C3.Hex(0x3a2408), 3005, 2f, 0.5f, 1.1f);
            NailLine(s, small, "quando crescer.", 220, 290, C3.Hex(0x3a2408), 3006, 2f, 0.5f, 1.1f);
            return s.Bake(relief: 1.1f);
        }
    }
}
