using System;
using System.Collections.Generic;
using static EchoesOfCravejada.EditorTools.DocKit;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>Os documentos do District 1 (Poco de Sucata), desenhados com o DocKit.</summary>
    public static partial class LoreArt
    {
        // ------------------------------------------------------------------
        // A etiqueta rasgada: a ficha de remessa do Scrap
        // ------------------------------------------------------------------

        private static PixelCanvas TornTagDoc()
        {
            var s = new DocSurface(400, 236);

            // Cartao de remessa: ponta chanfrada a esquerda, a direita arrancada.
            Func<int, int, bool> card = DocSurface.Sheet(14, 14, 380, 216, 11, right: 26f, top: 1.5f, bottom: 1.5f);
            s.Shape((x, y) =>
            {
                if (!card(x, y)) return false;
                float cut = 34f - Math.Min(y - 14, 216 - y);
                return x - 14 >= cut;
            });
            s.Paper(Manila, ManilaDark, 12, age: 0.75f, fibres: 1.4f, foxing: 0.6f);

            // O reforco do furo: um anel de papelao mais escuro e o ilhos de latao.
            s.Tint((x, y) =>
            {
                float d = (float)Math.Sqrt((x - 54) * (x - 54) + (y - 115) * (y - 115));
                return d < 22f ? 0.35f : 0f;
            }, ManilaDark);
            s.Rivet(54, 115, 10f, Brass, 0.85f);
            for (int y = 104; y < 127; y++)
                for (int x = 43; x < 66; x++)
                    if ((x - 54) * (x - 54) + (y - 115) * (y - 115) < 30) s.A[x + y * s.W] = 0f;

            // Formulario impresso.
            var form = new List<List<P2>>();
            Hand small = Print(7f, 3);
            Hand big = Print(15f, 4);
            big.Tracking = 2f;
            form.AddRange(big.Line("REMESSA DA COROA", 92, 46));
            form.AddRange(DocInk.Rect(88, 56, 372, 57));
            form.AddRange(small.Line("SÉRIE", 92, 84));
            form.AddRange(small.Line("DESTINO", 92, 122));
            form.AddRange(small.Line("MOTIVO", 92, 160));
            for (int row = 0; row < 3; row++) form.AddRange(DocInk.Line(150, 86 + row * 38, 360, 86 + row * 38));
            DocInk.Ink(s, Clip(s, form), C3.Hex(0x40281c), Type(1.2f, 5));

            // Preenchido a mao pelo despachante.
            Hand clerk = Hurried(17f, 21);
            var hand = new List<List<P2>>();
            hand.AddRange(clerk.Line("Protótipo n.º 7", 156, 82));
            hand.AddRange(clerk.Line("Poço de Sucata", 156, 120));
            hand.AddRange(clerk.Line("bruto. sem polimento.", 156, 158));
            DocInk.Ink(s, Clip(s, hand), BlueBlack, Nib(1.7f, 22));

            // A tinta borrou o comeco: um dedo molhado passou por cima.
            s.Tint((x, y) =>
            {
                if (x < 150 || x > 250 || y < 60 || y > 92) return 0f;
                return (DocSurface.Fbm(x * 0.06f, y * 0.12f, 31, 3) - 0.35f) * 1.4f;
            }, C3.Hex(0x6a6070));
            Thumbprint(s, 214, 78, 10f, C3.Hex(0x3a3448), 0.35f, 32);

            // Carimbo vermelho, torto, pela metade: a outra metade foi com o rasgo.
            Hand stamp = Print(24f, 5);
            stamp.Font = DocFont.RomanCast;
            stamp.Tracking = 4f;
            var st = DocInk.Rect(196, 168, 406, 208);
            st.AddRange(DocInk.Rect(200, 172, 402, 204));
            st.AddRange(stamp.Line("REJEITADO", 210, 200));
            DocInk.Stamp(s, Clip(s, Rotate(st, 300, 188, -5f)), DocKit.StampRed, Type(2.2f, 6), 0.45f);

            // Graxa no canto e a dobra de quem amassou.
            s.Ring(330, 60, 22f, C3.Hex(0x5a4020), 0.4f, 41, false);
            s.Fold(240, 14, 210, 216, 0.9f, 2.5f);

            // O arame partido, enroscado no ilhos.
            var wire = new List<List<P2>>
            {
                new List<P2> { new P2(54, 108), new P2(40, 92), new P2(28, 70), new P2(22, 48), new P2(26, 30), new P2(20, 12) },
                new List<P2> { new P2(50, 112), new P2(34, 116), new P2(22, 126), new P2(14, 146), new P2(18, 160) },
            };
            WireOver(s, wire);

            return s.Bake(relief: 1.2f);
        }

        /// <summary>Um arame de ferro por cima do desenho (fora do papel tambem).</summary>
        private static void WireOver(DocSurface s, List<List<P2>> wire)
        {
            float[] d = DocInk.Distance(s, wire, 3f);
            for (int i = 0; i < d.Length; i++)
            {
                if (d[i] > 1.6f) continue;
                s.A[i] = 1f;
                s.Col[i] = Iron * (1.1f - d[i] * 0.3f);
                s.Ht[i] = 6f + (1.6f - d[i]) * 1.5f;
                s.Gloss[i] = 0.6f;
            }
        }

        // ------------------------------------------------------------------
        // As placas de latao do poco
        // ------------------------------------------------------------------

        /// <summary>A placa da calha: bronze fundido, tomado de azinhavre.</summary>
        private static PixelCanvas ChutePlaqueDoc()
        {
            var s = new DocSurface(452, 236);
            s.Shape(DocSurface.Sheet(10, 10, 440, 222, 51, corner: 7f));
            s.Metal(Brass * 0.92f, 52, 0f, bevel: 6f, gloss: 0.55f, wear: 0.9f);

            // Moldura fundida: um friso em relevo a 12 px da borda.
            DocInk.Emboss(s, DocInk.Rect(24, 24, 426, 208), 1.6f, 1.5f, 1f, BrassBright, 0.25f);

            Hand cast = Cast(13f, 53);
            var letters = new List<List<P2>>();
            letters.AddRange(cast.Line("CALHA DE DESCARTE N.º 1", 225, 86, 0.5f));
            Hand castSmall = Cast(10f, 54);
            letters.AddRange(castSmall.Line("SERVIÇO DE LIMPEZA DA COROA", 225, 114, 0.5f));
            Hand motto = Italic(12f, 55);
            motto.Tracking = 1f;
            letters.AddRange(motto.Line("\"O que não brilha, desce.\"", 225, 156, 0.5f));
            DocInk.Emboss(s, letters, 1.4f, 2.2f, 1f, BrassBright, 0.55f);

            // O azinhavre tomou a placa: crosta e escorrido dos parafusos.
            s.Patina(0.36f, 56, Verdigris, VerdigrisLight, 1.2f);

            // Parafusos de fenda: um caiu, so o furo e a ferrugem.
            s.Rivet(30, 30, 6f, BrassDark * 1.3f, 0.5f, true, 0.4f);
            s.Rivet(420, 30, 6f, BrassDark * 1.3f, 0.5f, true, 1.9f);
            s.Rivet(30, 202, 6f, BrassDark * 1.3f, 0.5f, true, 2.6f);
            s.Dent(420, 202, 6f, 3f);
            s.Spot(420, 202, 5f, Soot, 0.9f);

            // Mossas de pancada.
            s.Dent(120, 190, 9f, 1.8f);
            s.Dent(380, 70, 6f, 1.4f);

            return s.Bake(relief: 1.1f);
        }

        /// <summary>A placa da obra abandonada: latao novo, letra gravada, e o carimbo de suspensa por cima.</summary>
        private static PixelCanvas TrialPlaqueDoc()
        {
            var s = new DocSurface(452, 268);

            // Recorte em escudo: topo em arco, cantos chanfrados.
            s.Shape((x, y) =>
            {
                if (x < 10 || x > 442 || y > 258) return false;
                float arc = 30f - (float)Math.Sqrt(Math.Max(0f, 1f - Math.Pow((x - 226) / 216f, 2))) * 22f;
                if (y < arc) return false;
                float cx = Math.Min(x - 10, 442 - x), cy = 258 - y;
                return cx + cy >= 10f;
            });
            s.Metal(Brass * 1.05f, 71, 0.15f, bevel: 7f, gloss: 1f, wear: 0.25f);

            // Filete duplo gravado e cantoneiras.
            C3 enamel = C3.Hex(0x1c140e);
            var frame = DocInk.Rect(30, 48, 422, 238);
            frame.AddRange(DocInk.Rect(36, 54, 416, 232));
            DocInk.Engrave(s, frame, 0.9f, 1.4f, enamel, 0.85f);

            // A coroa no topo, gravada.
            DocInk.Engrave(s, CrownCrest(226, 30, 34f), 0.9f, 1.4f, enamel, 0.9f);

            Hand title = Cast(14f, 72);
            Hand body = Cast(9.5f, 73);
            body.Tracking = 1.5f;
            var text = new List<List<P2>>();
            text.AddRange(title.Line("OBRA DA COROA", 226, 84, 0.5f));
            text.AddRange(Rule(110, 342, 98, 3.5f));
            text.AddRange(body.Line("ELEVADOR DE CARGA N.º 2", 226, 124, 0.5f));
            text.AddRange(body.Line("LEVARÁ A SUCATA BOA DE VOLTA À CIDADE.", 226, 142, 0.5f));
            Hand motto = Italic(13f, 74);
            text.AddRange(motto.Line("\"Cravejada não para de crescer.\"", 226, 168, 0.5f));
            DocInk.Engrave(s, text, 0.85f, 1.3f, enamel, 0.92f);

            // Carimbado as pressas por cima de tudo: a obra parou antes de comecar.
            Hand halt = Print(17f, 75);
            halt.Font = DocFont.RomanCast;
            halt.Tracking = 3f;
            var stop = DocInk.Rect(100, 186, 352, 222);
            stop.AddRange(DocInk.Rect(104, 190, 348, 218));
            stop.AddRange(halt.Line("OBRA SUSPENSA", 226, 213, 0.5f));
            DocInk.Stamp(s, Clip(s, Rotate(stop, 226, 204, -5f)), DocKit.StampRed, Type(2.2f, 77), 0.22f);

            s.Rivet(22, 44, 5f, BrassBright * 0.9f, 1f);
            s.Rivet(430, 44, 5f, BrassBright * 0.9f, 1f);
            s.Rivet(22, 244, 5f, BrassBright * 0.9f, 1f);
            s.Rivet(430, 244, 5f, BrassBright * 0.9f, 1f);

            // So uma pintinha de azinhavre num canto: alguem ainda polia.
            s.Patina(0.12f, 76, Verdigris, VerdigrisLight, 0.2f);
            return s.Bake(relief: 1f);
        }

        // ------------------------------------------------------------------
        // O muro dos nomes
        // ------------------------------------------------------------------

        private static PixelCanvas WallOfNamesDoc(bool purified)
        {
            var s = new DocSurface(520, 360);
            s.Shape(DocSurface.Sheet(8, 8, 510, 350, 81, 9f, 9f, 9f, 9f));
            s.Stone(C3.Hex(0x26242c), C3.Hex(0x77727a), 82, strata: 0.8f);

            C3 freshStone = C3.Hex(0xc8c2b4);
            C3 oldStone = C3.Hex(0x8a8680);

            // Cada nome na mao de quem riscou: alturas, inclinacoes e fundos diferentes.
            void Name(string text, float x, float y, float size, float slant, int seed, float depth, C3 inner, float amount,
                bool cursive = false)
            {
                Hand h = cursive ? Hurried(size, seed) : Scratcher(size, seed, slant);
                if (cursive) h.Slant = slant;
                List<List<P2>> cut = DocInk.Scratchify(h.Line(text, x, y), seed + 1, cursive ? 0.9f : 1.2f, cursive ? 0.6f : 1.4f);
                DocInk.Carve(s, cut, 1.1f + depth * 0.15f, depth, inner, amount, seed + 2, chips: 0.35f);
            }

            Name("LAMPIÃO-12. AINDA ACENDO.", 30, 52, 13f, 0.05f, 101, 2f, oldStone, 0.6f);
            Name("Bigorna. trabalhei quarenta anos.", 52, 94, 18f, 0.15f, 111, 1.8f, oldStone, 0.55f, cursive: true);
            Name("PEQUENA. NUNCA TIVE NÚMERO.", 26, 128, 12f, -0.06f, 121, 2.3f, oldStone, 0.65f);

            // Riscos miudos: series, contas de dias, engrenagens desenhadas.
            Name("A-77", 400, 56, 10f, 0f, 131, 1.6f, oldStone, 0.5f);
            Name("K9", 456, 150, 11f, 0.2f, 141, 1.6f, oldStone, 0.5f);
            Name("R.I.P. ESTOPA", 360, 132, 9f, -0.1f, 151, 1.4f, oldStone, 0.45f);
            var marks = new List<List<P2>>();
            for (int g = 0; g < 4; g++)
            {
                float gx = 330 + g * 26, gy = 176;
                for (int i = 0; i < 4; i++) marks.AddRange(DocInk.Line(gx + i * 4, gy, gx + i * 4 + 1, gy + 16));
                marks.AddRange(DocInk.Line(gx - 3, gy + 13, gx + 17, gy + 3));
            }

            marks.AddRange(GearScratch(470, 30, 12f));
            marks.AddRange(GearScratch(40, 250, 9f));
            marks.AddRange(GearScratch(480, 300, 14f));
            DocInk.Carve(s, DocInk.Scratchify(marks, 161, 1f, 1f), 1.1f, 1.7f, oldStone, 0.55f, 162, chips: 0.2f);

            if (purified)
            {
                Name("AINDA AQUI.", 120, 168, 15f, 0.02f, 171, 2.6f, freshStone, 0.95f);
                Name("COLETOR N.º 1.", 70, 208, 13f, 0f, 181, 2.4f, freshStone, 0.9f);
                Name("TAMBÉM ERA UM DE NÓS.", 70, 232, 13f, 0f, 191, 2.4f, freshStone, 0.9f);
            }

            // Embaixo de todos, em letra grande, a cinzel: o unico entalhe feito com ferramenta de verdade.
            Hand chisel = Cast(21f, 201);
            chisel.Tracking = 2f;
            var big = new List<List<P2>>();
            big.AddRange(chisel.Line("QUEM LER ISTO,", 260, 286, 0.5f));
            big.AddRange(chisel.Line("LEMBRE-SE DE NÓS.", 260, 326, 0.5f));
            DocInk.Carve(s, DocInk.Smooth(big), 2.6f, 5f, C3.Hex(0x3a3640), 0.55f, 202, chips: 0.6f, burr: 0.1f);

            // Musgo e oxido nas frestas de baixo.
            s.Tint((x, y) =>
            {
                float n = DocSurface.Fbm(x * 0.05f, y * 0.05f, 211, 3);
                float low = Math.Max(0f, (y - 240f) / 120f);
                return (n - 0.66f + low * 0.2f) * 1.6f;
            }, C3.Hex(0x34483a));

            PixelCanvas canvas = s.Bake(relief: 1.3f);

            if (!purified)
            {
                // O prego pendurado num barbante, esperando o proximo nome.
                var n = new DocSurface(s.W, s.H);
                var str = new List<List<P2>> { new List<P2> { new P2(286, 150), new P2(288, 168), new P2(291, 186) } };
                n.AddShape((x, y) => false);
                float[] d = DocInk.Distance(n, str, 2f);
                for (int i = 0; i < d.Length; i++)
                    if (d[i] < 0.9f)
                    {
                        n.A[i] = 1f;
                        n.Col[i] = C3.Hex(0xb8a070);
                        n.Ht[i] = 2f;
                    }

                n.Rivet(286, 148, 3.5f, Iron * 1.2f, 0.6f);
                var shaft = new List<List<P2>> { new List<P2> { new P2(291, 188), new P2(293, 222) } };
                float[] sd = DocInk.Distance(n, shaft, 3f);
                for (int i = 0; i < sd.Length; i++)
                    if (sd[i] < 1.8f)
                    {
                        n.A[i] = 1f;
                        n.Col[i] = IronLight * (1.1f - sd[i] * 0.25f);
                        n.Ht[i] = 3f + (1.8f - sd[i]) * 1.5f;
                        n.Gloss[i] = 0.8f;
                    }

                for (int x = 285; x <= 298; x++)
                    for (int y = 186; y <= 189; y++)
                    {
                        int i = x + y * n.W;
                        n.A[i] = 1f;
                        n.Col[i] = IronLight;
                        n.Ht[i] = 4f + (189 - y);
                        n.Gloss[i] = 0.8f;
                    }

                n.ComputeEdge();
                Overlay(canvas, n.Bake(shadow: true, outline: true));
            }

            return canvas;
        }

        /// <summary>Uma engrenagem desenhada a ponta: o aro e os dentes, em cortes.</summary>
        private static List<List<P2>> GearScratch(float cx, float cy, float r)
        {
            List<List<P2>> g = DocInk.Circle(cx, cy, r, 10);
            for (int t = 0; t < 8; t++)
            {
                double a = t * Math.PI / 4.0;
                g.AddRange(DocInk.Line(cx + (float)Math.Cos(a) * r, cy + (float)Math.Sin(a) * r,
                    cx + (float)Math.Cos(a) * (r + 4f), cy + (float)Math.Sin(a) * (r + 4f)));
            }

            g.AddRange(DocInk.Circle(cx, cy, r * 0.3f, 6));
            return g;
        }

        /// <summary>Cola um desenho por cima de outro (alfa por cima).</summary>
        private static void Overlay(PixelCanvas under, PixelCanvas over)
        {
            for (int i = 0; i < under.Pixels.Length; i++)
            {
                Rgba o = over.Pixels[i];
                if (o.A == 0) continue;
                if (o.A == 255 || under.Pixels[i].A == 0)
                {
                    under.Pixels[i] = o.A == 255 ? o : (under.Pixels[i].A == 0 ? o : under.Pixels[i]);
                    continue;
                }

                Rgba u = under.Pixels[i];
                float k = o.A / 255f;
                under.Pixels[i] = new Rgba((int)(u.R + (o.R - u.R) * k), (int)(u.G + (o.G - u.G) * k), (int)(u.B + (o.B - u.B) * k), u.A);
            }
        }

        // ------------------------------------------------------------------
        // O decreto dos Nobres
        // ------------------------------------------------------------------

        private static PixelCanvas DecreeDoc()
        {
            var s = new DocSurface(420, 520);
            s.Shape(DocSurface.Sheet(16, 12, 404, 504, 301, 2.2f, 2.2f, 2.2f, 2.2f));
            s.Paper(Vellum, VellumDark, 302, age: 0.55f, fibres: 0.8f, foxing: 0.4f);

            // Dobrado em quatro pra caber no bolso de quem desceu pela calha.
            s.Fold(210, 0, 210, 520, 1.3f, 3f);
            s.Fold(0, 258, 420, 258, 1f, 3f);

            // Timbre: a coroa entre filetes, e o titulo em gotica.
            var head = new List<List<P2>>();
            head.AddRange(Rule(46, 374, 96, 4f));
            Hand goth = Blackletter(22f, 303);
            head.AddRange(goth.Line("Decreto do Conselho", 210, 134, 0.5f));
            Hand caps = Cast(11f, 304);
            caps.Tracking = 4f;
            head.AddRange(caps.Line("DOS 925 NOBRES DE CRAVEJADA", 210, 158, 0.5f));
            head.AddRange(Rule(70, 350, 172, 3f));
            DocInk.Ink(s, head, IronGall, Type(1.5f, 305));

            Hand body = Print(10f, 306);
            body.Font = DocFont.RomanCast;
            body.LineGap = 1.75f;
            var text = new List<List<P2>>();
            float y = 206;
            y = body.Block("A corrosão verde não atinge a prata pura.", 46, y, 328, text) + 6;
            y = body.Block("Todo autômato de cobre ou de latão que apresentar manchas, rangidos ou desobediência será recolhido e descartado.", 46, y, 328, text) + 6;
            Hand bold = Cast(11.5f, 307);
            bold.Tracking = 2f;
            text.AddRange(bold.Line("NÃO HÁ CURA. HÁ DESCARTE.", 210, y + 8, 0.5f));
            y += 34;
            y = body.Block("Por ordem de Sua Majestade, o Rei de Prata.", 46, y, 328, text);
            DocInk.Ink(s, text, IronGall, Type(1.25f, 308));

            // A assinatura do Rei, em tinta prateada, e o lacre de cera de prata com o 925.
            DocInk.Ink(s, Signature("Argentum Rex", 150, y + 44, 22f, 309), C3.Hex(0x6a7080), Nib(2f, 310));
            DocInk.Seal(s, 94, 52, 30f, SilverWax, CrownCrest(94, 52, 34f), 311);
            s.Ring(94, 52, 42f, C3.Hex(0x8a8070), 0.25f, 312);

            // Pisado: meia sola de bota e lama.
            BootPrint(s, 340, 420, 40f, 0.5f, C3.Hex(0x4a3a28), 0.45f, 315);
            return s.Bake(relief: 1.4f);
        }

        // ------------------------------------------------------------------
        // A lista da triagem: giz numa folha de flandres pregada
        // ------------------------------------------------------------------

        private static PixelCanvas SortingListDoc()
        {
            var s = new DocSurface(440, 360);
            s.Shape((x, y) =>
            {
                if (x < 12 || x > 428 || y < 22 || y > 350) return false;
                return (x - 392) + (y - 316) <= 36;
            });
            s.Metal(C3.Hex(0x3e4048), 401, 1.5708f, bevel: 3f, gloss: 0.25f, wear: 1f);

            // Ferrugem subindo das bordas e uma ondulacao na chapa.
            s.Tint((x, y) =>
            {
                float e = s.Edge[x + y * s.W];
                float n = DocSurface.Fbm(x * 0.04f, y * 0.04f, 402, 3);
                return (n - 0.5f) * 1.5f + Math.Max(0f, 1f - e / 14f) * 0.8f - 0.2f;
            }, Rust);
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                    s.Ht[x + y * s.W] += (float)Math.Sin(x * 0.045f) * 1.2f;

            // A ponta dobrada mostra o avesso, mais claro.
            s.Tint((x, y) => (x - 392) + (y - 316) > 22 ? 0.7f : 0f, C3.Hex(0x9a9ca4));

            // Giz em letra de maquina, toda igual: o capataz da triagem.
            Hand machine = Scratcher(13f, 403);
            machine.Font = DocFont.RomanPrint;
            machine.Jitter = 0.2f;
            machine.Wave = 0.3f;
            machine.SizeJitter = 0.02f;
            machine.Tracking = 1f;
            var chalk = new List<List<P2>>();
            chalk.AddRange(machine.Line("PEÇAS BOAS:", 34, 66));
            chalk.AddRange(machine.Line("PEÇAS RUINS:", 236, 66));
            chalk.AddRange(DocInk.Line(28, 80, 412, 82));
            chalk.AddRange(DocInk.Line(222, 48, 220, 204));
            string[] good = { "ENGRENAGENS", "MOLAS", "REBITES", "LENTES" };
            for (int i = 0; i < good.Length; i++) chalk.AddRange(machine.Line(good[i], 40, 108 + i * 26));
            chalk.AddRange(machine.Line("SOBEM PELA CALHA.", 34, 228));
            chalk.AddRange(machine.Line("TODO O RESTO.", 240, 108));
            chalk.AddRange(machine.Line("FICAM NO POÇO.", 240, 140));
            chalk.AddRange(DocInk.Line(28, 248, 412, 246));
            // Os tiques ao lado de cada peca boa.
            for (int i = 0; i < 4; i++) chalk.Add(new List<P2> { new P2(26, 102 + i * 26), new P2(30, 107 + i * 26), new P2(36, 96 + i * 26) });
            DocInk.Chalk(s, chalk, ChalkWhite, Stick(2.2f, 404), 0.12f);

            // A terceira coluna, comecada depois, em cursiva que treme; ninguem a preencheu.
            Hand shaky = Shaky(19f, 405);
            List<List<P2>> third = shaky.Line("peças que se mexem:", 40, 278);
            DocInk.Chalk(s, third, ChalkWhite * 0.95f, Stick(2f, 406), 0.1f);
            var dots = new List<List<P2>>();
            for (int x = 48; x < 330; x += 9) dots.Add(new List<P2> { new P2(x, 302), new P2(x + 1.5f, 302) });
            DocInk.Chalk(s, dots, ChalkWhite * 0.8f, Stick(1.8f, 407), 0f);

            // A assinatura, na mesma letra de maquina: a lista e do proprio Coletor.
            Hand sign = Scratcher(10f, 410);
            sign.Font = DocFont.RomanPrint;
            sign.Jitter = 0.2f;
            sign.Tracking = 1f;
            DocInk.Chalk(s, sign.Line("ASS.: COLETOR DE REFUGO N.º 1", 40, 334), ChalkWhite, Stick(1.9f, 411), 0.1f);

            // O giz apagado com a manga, deixando a nevoa.
            s.Tint((x, y) => (DocSurface.Fbm(x * 0.012f, y * 0.05f, 408, 3) - 0.6f) * 0.5f, ChalkWhite);

            // O prego que segura a chapa, com a ferrugem escorrida.
            DocInk.NailHole(s, 220, 30, Rust, 409);
            return s.Bake(relief: 1.2f);
        }

        // ------------------------------------------------------------------
        // A placa do Coletor: o pedaco de chassi que sobrou
        // ------------------------------------------------------------------

        private static PixelCanvas CollectorPlateDoc()
        {
            var s = new DocSurface(480, 300);

            // O pedaco de chassi, de beirada rasgada pela queda.
            (float X, float Y)[] outline =
            {
                (16, 40), (90, 12), (210, 26), (330, 8), (466, 22), (458, 140), (470, 284),
                (330, 272), (220, 290), (100, 274), (8, 286), (22, 160),
            };
            s.Shape((x, y) =>
            {
                float jag = (DocSurface.Fbm(x * 0.15f, y * 0.15f, 501, 2) - 0.5f) * 10f;
                return ArtKit.InsidePolygon(outline, x + jag, y + jag * 0.6f);
            });
            s.Metal(Iron, 502, 0f, bevel: 4f, gloss: 0.3f, wear: 1f);
            s.Tint((x, y) => (DocSurface.Fbm(x * 0.03f, y * 0.03f, 503, 4) - 0.42f) * 2f, Rust);
            s.Tint((x, y) => (DocSurface.Fbm(x * 0.09f, y * 0.09f, 504, 3) - 0.6f) * 2.5f, RustLight);

            // As fileiras de rebites do chassi.
            for (int x = 46; x < 440; x += 30)
            {
                s.Rivet(x, 44, 5.5f, Iron * 1.3f, 0.4f);
                s.Rivet(x + 8, 252, 5.5f, Iron * 1.3f, 0.4f);
            }

            // A placa de latao, rebitada no meio. Letra puncionada a martelo.
            Func<int, int, bool> rect = DocSurface.Sheet(60, 70, 420, 224, 505, corner: 4f);
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                {
                    if (!rect(x, y)) continue;
                    int i = x + y * s.W;
                    s.Col[i] = Brass * (0.95f + DocSurface.Fbm(x * 0.04f, y * 0.6f, 506, 2) * 0.1f);
                    float e = Math.Min(Math.Min(x - 60, 420 - x), Math.Min(y - 70, 224 - y));
                    s.Ht[i] = 3f + Math.Min(e, 3f);
                    s.Gloss[i] = 0.75f;
                }

            Hand punch = Cast(13f, 507);
            punch.Font = DocFont.RomanThin;
            punch.Tracking = 3f;
            punch.Jitter = 0.25f;
            punch.SizeJitter = 0.04f;
            var text = new List<List<P2>>();
            text.AddRange(punch.Line("COLETOR DE REFUGO N.º 1", 240, 108, 0.5f));
            Hand punch2 = punch.Clone();
            punch2.Size = 10f;
            text.AddRange(punch2.Line("SERVIÇO DE LIMPEZA DA COROA", 240, 136, 0.5f));
            text.AddRange(Rule(110, 370, 152, 3f));
            text.AddRange(punch2.Line("MANTER O POÇO LIMPO.", 240, 178, 0.5f));
            punch2.Size = 12f;
            text.AddRange(punch2.Line("NÃO PARAR.", 240, 204, 0.5f));
            DocInk.Engrave(s, text, 1.1f, 1.8f, BrassDark, 0.6f, 0.6f);

            s.Rivet(72, 82, 5f, BrassBright * 0.85f, 0.9f);
            s.Rivet(408, 82, 5f, BrassBright * 0.85f, 0.9f);
            s.Rivet(72, 212, 5f, BrassBright * 0.85f, 0.9f);
            s.Rivet(408, 212, 5f, BrassBright * 0.85f, 0.9f);

            // A ferrugem do chassi invadindo a placa por baixo.
            s.Tint((x, y) => y > 190 ? (DocSurface.Fbm(x * 0.06f, y * 0.06f, 508, 3) - 0.55f) * 2f : 0f, Rust);

            // O aro de prata preso entre as chapas, no canto, brilhando.
            for (int y = 230; y < 290; y++)
                for (int x = 380; x < 450; x++)
                {
                    float dx = x - 414, dy = (y - 262) * 1.25f;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (d > 20f || d < 12f) continue;
                    int i = x + y * s.W;
                    s.A[i] = 1f;
                    s.Col[i] = DocKit.Silver * 0.62f;
                    float t = (d - 16f) / 4f;
                    s.Ht[i] = 9f + (1f - t * t) * 4f;
                    s.Gloss[i] = 1f;
                }

            return s.Bake(relief: 1.2f);
        }
    }
}
