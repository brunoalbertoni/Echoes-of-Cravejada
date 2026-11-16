using System;
using System.Collections.Generic;
using static EchoesOfCravejada.EditorTools.DocKit;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>Os documentos do District 3 (Geodos Silenciosos), desenhados com o DocKit.</summary>
    public static partial class LoreArt
    {
        private static readonly C3 Amethyst = C3.Hex(0x8a5ac8);
        private static readonly C3 AmethystLight = C3.Hex(0xd0b0f4);

        // ------------------------------------------------------------------
        // O capacete, visto por dentro
        // ------------------------------------------------------------------

        private static PixelCanvas HelmetDoc()
        {
            var s = new DocSurface(440, 330);
            s.Shape((x, y) =>
            {
                float dx = (x - 220f) / 205f, dy = (y - 200f) / 180f;
                return dx * dx + dy * dy <= 1f && y < 318;
            });

            // O fundo do casco: concavo, a luz bate no lado de baixo a direita.
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                {
                    int i = x + y * s.W;
                    if (s.A[i] <= 0f) continue;
                    float dx = (x - 220f) / 205f, dy = (y - 200f) / 180f;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    s.Col[i] = Brass * 0.8f * (0.85f + DocSurface.Fbm(x * 0.03f, y * 0.03f, 3201, 3) * 0.3f);
                    s.Ht[i] = r * r * 40f;
                    s.Gloss[i] = 0.55f;
                }

            // Amassados de pedra que caiu.
            s.Dent(150, 120, 30f, -2.2f);
            s.Dent(310, 210, 24f, -1.8f);
            s.Tint((x, y) => (DocSurface.Fbm(x * 0.04f, y * 0.04f, 3202, 3) - 0.62f) * 2f, C3.Hex(0x4a3418));

            // A carneira de couro, costurada, atravessando o casco.
            for (int y = 248; y < 290; y++)
                for (int x = 30; x < 410; x++)
                {
                    int i = x + y * s.W;
                    if (s.A[i] <= 0f) continue;
                    s.Col[i] = C3.Hex(0x5a3420) * (0.85f + DocSurface.Fbm(x * 0.1f, y * 0.1f, 3203, 3) * 0.3f);
                    s.Ht[i] += 6f + Math.Min(Math.Min(y - 248, 290 - y), 4) * 0.6f;
                    s.Gloss[i] = 0.2f;
                }

            var stitches = new List<List<P2>>();
            for (int x = 40; x < 400; x += 10)
            {
                stitches.AddRange(DocInk.Line(x, 254, x + 5, 254));
                stitches.AddRange(DocInk.Line(x, 284, x + 5, 284));
            }

            DocInk.Ink(s, stitches, C3.Hex(0xc8b490), Ballpoint(1.4f, 3204));
            for (int x = 60; x < 400; x += 80) s.Rivet(x, 269, 4.5f, Brass * 1.1f, 0.8f);

            // Riscado a prego, por dentro, onde so o dono via.
            NailLine(s, Scratcher(48f, 3205, 0.05f), "SHH!", 220, 170, BrassBright * 1.1f, 3206, 3.4f, 0.5f, 1.9f);
            return s.Bake(relief: 0.9f);
        }

        // ------------------------------------------------------------------
        // A placa da Coroa no rasgo da mina
        // ------------------------------------------------------------------

        private static PixelCanvas CrownSignDoc()
        {
            var s = new DocSurface(460, 300);
            s.Shape(DocSurface.Sheet(10, 10, 450, 290, 3301, corner: 10f));
            s.Metal(Brass, 3302, 0f, bevel: 6f, gloss: 0.8f, wear: 0.5f);

            // O esmalte vermelho dentro da moldura, lascado ate o ferro.
            for (int y = 34; y < 266; y++)
                for (int x = 34; x < 426; x++)
                {
                    int i = x + y * s.W;
                    float n = DocSurface.Fbm(x * 0.05f, y * 0.05f, 3303, 4);
                    bool chip = n > 0.72f;
                    s.Col[i] = chip ? Iron * (0.8f + n * 0.3f) : C3.Lerp(C3.Hex(0xb02a30), C3.Hex(0x6a141c), (y - 34) / 232f);
                    s.Ht[i] = chip ? 1f : 3f;
                    s.Gloss[i] = chip ? 0.2f : 0.95f;
                }

            s.Tint((x, y) => x > 34 && x < 426 && y > 34 && y < 266 ? (DocSurface.Fbm(x * 0.05f, y * 0.05f, 3303, 4) - 0.76f) * 6f : 0f, Rust);

            // A coroa e as letras, em esmalte prateado.
            var silver = CrownCrest(230, 82, 54f);
            Hand h = Cast(16f, 3304);
            h.Tracking = 2f;
            silver.AddRange(h.Line("PARA TRÁS.", 230, 160, 0.5f));
            Hand h2 = Cast(11f, 3305);
            h2.Tracking = 1.5f;
            silver.AddRange(h2.Line("ESPAÇO RESERVADO PARA", 230, 192, 0.5f));
            silver.AddRange(h2.Line("MINERAÇÃO PRIVADA", 230, 214, 0.5f));
            silver.AddRange(Rule(110, 350, 236, 3f));
            DocInk.Emboss(s, silver, 1.5f, 1f, 0.8f, C3.Hex(0xdfe4ee), 1f, 1f);
            s.Tint((x, y) => x > 34 && x < 426 && y > 34 && y < 266 ? (DocSurface.Fbm(x * 0.05f, y * 0.05f, 3303, 4) - 0.72f) * 8f : 0f, Iron);

            // Po de pedra assentado em cima de tudo.
            s.Tint((x, y) => (DocSurface.Fbm(x * 0.02f, y * 0.1f, 3306, 3) - 0.55f) * 0.8f, C3.Hex(0xb0a898));
            s.Rivet(22, 22, 5f, BrassBright * 0.85f, 0.8f);
            s.Rivet(438, 22, 5f, BrassBright * 0.85f, 0.8f);
            s.Rivet(22, 278, 5f, BrassBright * 0.85f, 0.8f);
            s.Rivet(438, 278, 5f, BrassBright * 0.85f, 0.8f);
            return s.Bake(relief: 1.1f);
        }

        // ------------------------------------------------------------------
        // A tabua do turno
        // ------------------------------------------------------------------

        /// <summary>
        /// A folha sobre o Capataz: arrancada de um caderno de turno e presa na
        /// rocha por um colega. Letra apertada, de quem escreveu com medo.
        /// </summary>
        private static PixelCanvas RosterDoc()
        {
            var s = new DocSurface(420, 400);
            s.Shape(DocSurface.Sheet(16, 16, 404, 384, 3401, 6f, 1.5f, 1.5f, 1.5f));
            s.Paper(C3.Hex(0xd8ccb0), C3.Hex(0x8a7a5a), 3402, age: 0.7f, fibres: 1f, foxing: 0.6f);
            s.Fold(16, 200, 404, 196, 1f, 2.5f);

            Hand h = Hurried(15f, 3403);
            var t = new List<List<P2>>();
            float y = 62;
            foreach (string p in new[]
                     {
                         "O capataz foi o primeiro a descer. Ouviu a pedra antes de todo mundo. Dizia que ela cantava pra ele.",
                         "Primeiro ele perdeu os olhos. Depois a voz. Depois parou de caber no capacete.",
                         "Desceu sozinho pra toca do fim do corredor e nunca mais subiu.",
                         "Às vezes ainda dá pra ouvir ele chamando o turno.",
                     })
            {
                y = h.Block(p, 40, y, 340, t) + 14;
            }

            DocInk.Ink(s, t, IronGall, Nib(1.5f, 3404));

            // Furado pela lasca de cristal que a prendia, com o roxo do po.
            for (int yy = 0; yy < s.H; yy++)
                for (int xx = 0; xx < s.W; xx++)
                {
                    float d = (float)Math.Sqrt((xx - 210) * (xx - 210) + (yy - 34) * (yy - 34));
                    if (d < 4f) s.A[xx + yy * s.W] = 0f;
                    else if (d < 22f && s.In(xx, yy)) s.Col[xx + yy * s.W] = C3.Lerp(s.Col[xx + yy * s.W], Amethyst, (1f - d / 22f) * 0.35f);
                }

            s.ComputeEdge();
            return s.Bake(relief: 1.4f);
        }

        // ------------------------------------------------------------------
        // A placa de prata das palavras e o bloco de notas
        // ------------------------------------------------------------------

        private static PixelCanvas WordPlaqueDoc()
        {
            var s = new DocSurface(460, 400);
            s.Shape(DocSurface.Sheet(14, 12, 446, 110, 3501, corner: 8f));
            s.Metal(DocKit.Silver * 0.95f, 3502, 0f, bevel: 6f, gloss: 1f, wear: 0.3f);
            Hand h = Cast(11.5f, 3503);
            h.Tracking = 1.5f;
            var words = h.Line("GELO · RUBI · MEL · SANGUE · CÉU", 230, 70, 0.5f);
            DocInk.Engrave(s, words, 1.1f, 1.8f, C3.Hex(0x2a2e3a), 0.85f);
            DocInk.Engrave(s, DocInk.Rect(30, 28, 430, 94), 0.8f, 1f, C3.Hex(0x2a2e3a), 0.6f);
            s.Rivet(24, 61, 4.5f, DocKit.Silver, 1f);
            s.Rivet(436, 61, 4.5f, DocKit.Silver, 1f);
            s.Tint((x, y) => (DocSurface.Fbm(x * 0.04f, y * 0.04f, 3504, 3) - 0.68f) * 2.5f, C3.Hex(0x2a6a4a));

            // O bloco de notas grudado embaixo, torto, com espiral.
            var note = new DocSurface(s.W, s.H);
            Func<int, int, bool> page = DocSurface.Sheet(110, 140, 380, 388, 3505, 1f, 0f, 1f, 1f);
            note.Shape((x, y) =>
            {
                float a = 0.06f;
                float u = 245 + (x - 245) * (float)Math.Cos(a) + (y - 264) * (float)Math.Sin(a);
                float v = 264 - (x - 245) * (float)Math.Sin(a) + (y - 264) * (float)Math.Cos(a);
                return page((int)u, (int)v);
            });
            note.Paper(C3.Hex(0xe8e0c4), C3.Hex(0xa89a70), 3506, age: 0.35f);
            var lines = new List<List<P2>>();
            for (int k = 0; k < 9; k++) lines.AddRange(DocInk.Line(110, 190 + k * 22, 384, 190 + k * 22));
            DocInk.Ink(note, DocKit.Rotate(lines, 245, 264, 3.4f), C3.Hex(0x7a9ac8), new Pen { Width = 0.8f, Contrast = 0f, Opacity = 0.6f, Seed = 3507 });

            Hand pencil = Hurried(16f, 3508);
            var text = new List<List<P2>>();
            pencil.Block("muito chato ter que ficar andando de um lado pro outro pra abrir uma porta", 130, 206, 230, text);
            text.AddRange(Hurried(14f, 3509).Line(":(", 300, 340));
            DocInk.Ink(note, DocKit.Rotate(text, 245, 264, 3.4f), Graphite, new Pen { Width = 1.4f, Contrast = 0.1f, Pressure = 0.4f, Opacity = 0.8f, Dry = 0.25f, Seed = 3510 });

            for (int i = 0; i < s.A.Length; i++)
            {
                if (note.A[i] <= 0f) continue;
                s.A[i] = 1f;
                s.Col[i] = note.Col[i];
                s.Ht[i] = 3f + note.Ht[i];
                s.Gloss[i] = 0.02f;
            }

            // A espiral de arame em cima do bloco.
            for (int k = 0; k < 16; k++)
            {
                float cx = 128 + k * 16 + 2.4f, cy = 146 + k * 1f;
                s.Rivet(cx, cy, 4f, IronLight, 0.9f);
                s.Spot(cx, cy + 6, 2f, Soot, 0.8f);
            }

            s.ComputeEdge();
            return s.Bake(relief: 1.1f);
        }

        // ------------------------------------------------------------------
        // Os bilhetes de mineiro: cada um num papel e numa mao
        // ------------------------------------------------------------------

        private static PixelCanvas VaultPageDoc()
        {
            var s = new DocSurface(400, 220);
            s.Shape(DocSurface.Sheet(14, 14, 386, 206, 3601, 3f, 3f, 3f, 3f));
            s.Paper(C3.Hex(0xcac2a4), C3.Hex(0x7a7058), 3602, age: 0.8f, fibres: 1.4f);
            s.Fold(200, 14, 200, 206, 1.2f, 3f);
            s.Tint((x, y) => (DocSurface.Fbm(x * 0.05f, y * 0.05f, 3603, 3) - 0.5f) * 0.7f, C3.Hex(0x8a8478));

            // A lapis grosso de mineiro, letra redonda e devagar, como quem reza.
            Hand h = Careful(19f, 3604);
            var t = h.Line("A abóbada canta quando", 40, 70);
            t.AddRange(h.Line("a gente pisa errado.", 40, 100));
            t.AddRange(h.Line("Aprendemos a pisar", 40, 146));
            t.AddRange(h.Line("como quem reza.", 40, 176));
            DocInk.Ink(s, t, Graphite, new Pen { Width = 2f, Contrast = 0.15f, Pressure = 0.35f, Opacity = 0.8f, Dry = 0.3f, Seed = 3605 });
            Thumbprint(s, 352, 186, 12f, C3.Hex(0x3a3a40), 0.55f, 3606);
            return s.Bake(relief: 1.3f);
        }

        private static PixelCanvas LoopNoteDoc()
        {
            var s = new DocSurface(400, 260);

            // Amassado e reaberto: dobras pra todo lado.
            s.Shape(DocSurface.Sheet(16, 16, 384, 244, 3701, 5f, 5f, 5f, 5f));
            s.Paper(C3.Hex(0xd8ccb0), C3.Hex(0x8a7a5a), 3702, age: 0.7f);
            s.Fold(16, 90, 384, 140, 1f, 2f);
            s.Fold(120, 16, 170, 244, 1f, 2f);
            s.Fold(280, 16, 240, 244, 0.8f, 2f);
            s.Fold(16, 200, 384, 170, 0.8f, 2f);

            Hand h = Shaky(16f, 3703);
            var t = new List<List<P2>>();
            float y = h.Block("Dei a volta pela direita, como o capataz mandou.", 40, 70, 320, t) + 10;
            h.Block("Que portão é aquele?", 40, y, 320, t);
            DocInk.Ink(s, t, IronGall, Nib(1.5f, 3704));

            // Furado pela lasca de cristal que o prendia, com o roxo do po.
            for (int yy = 0; yy < s.H; yy++)
                for (int xx = 0; xx < s.W; xx++)
                {
                    float d = (float)Math.Sqrt((xx - 340) * (xx - 340) + (yy - 52) * (yy - 52));
                    if (d < 4f) s.A[xx + yy * s.W] = 0f;
                    else if (d < 22f && s.In(xx, yy)) s.Col[xx + yy * s.W] = C3.Lerp(s.Col[xx + yy * s.W], Amethyst, (1f - d / 22f) * 0.35f);
                }

            s.ComputeEdge();
            return s.Bake(relief: 1.5f);
        }

        private static PixelCanvas MinerNoteDoc()
        {
            var s = new DocSurface(440, 600);
            s.Shape(DocSurface.Sheet(14, 12, 426, 588, 3801, 2f, 2f, 2f, 7f));
            s.Paper(C3.Hex(0xd4c8a6), C3.Hex(0x7e6e50), 3802, age: 0.75f, fibres: 1.2f, foxing: 0.8f);
            s.Fold(14, 200, 426, 200, 1.1f, 3f);
            s.Fold(14, 400, 426, 400, 1.1f, 3f);

            Hand h = Hurried(14.5f, 3803);
            h.Jitter = 0.45f;
            h.Wave = 1.4f;
            var t = new List<List<P2>>();
            float y = 52;
            foreach (string p in new[]
                     {
                         "Ela não para mais. Desligamos a chave, e ela não parou. Não há mais ninguém lá dentro, e ela não parou.",
                         "Se você está lendo isto: não faça barulho perto dela. Ela não enxerga. Ela escuta.",
                         "E tem a luz. Os pilares de cristal lá dentro acendem quando cantam, e ela corre atrás da luz como bicho atrás de lampião.",
                         "Mas a luz apaga rápido. Se o cristal ainda estiver brilhando forte quando ela chega, ela perde o controle e se crava nele. Foi a única vez que vi aquele motor parado.",
                     })
            {
                y = h.Block(p, 36, y, 368, t) + 10;
            }

            // Sublinhado com forca.
            t.AddRange(DocInk.Line(170, 230, 360, 228));
            DocInk.Ink(s, t, C3.Hex(0x22202a), Nib(1.4f, 3804));

            // O esquema no pe da pagina: o pilar aceso, a broca, a seta.
            var d = new List<List<P2>>();
            float py = Math.Max(y + 20, 470);
            d.Add(new List<P2> { new P2(320, py + 80), new P2(310, py + 30), new P2(330, py), new P2(350, py + 30), new P2(340, py + 80) });
            for (int k = 0; k < 6; k++)
            {
                double a = -Math.PI / 2 + (k - 2.5) * 0.35;
                d.AddRange(DocInk.Line(330 + (float)Math.Cos(a) * 46f, py + 30 + (float)Math.Sin(a) * 46f, 330 + (float)Math.Cos(a) * 60f, py + 30 + (float)Math.Sin(a) * 60f));
            }

            d.Add(new List<P2> { new P2(60, py + 60), new P2(150, py + 46), new P2(170, py + 60), new P2(150, py + 74), new P2(60, py + 60) });
            for (int k = 0; k < 4; k++) d.AddRange(DocInk.Line(80 + k * 18, py + 52, 88 + k * 18, py + 68));
            d.Add(new List<P2> { new P2(186, py + 60), new P2(290, py + 56) });
            d.Add(new List<P2> { new P2(278, py + 46), new P2(292, py + 56), new P2(278, py + 66) });
            DocInk.Ink(s, d, C3.Hex(0x22202a), Ballpoint(1.4f, 3805));
            DocInk.Ink(s, h.Line("luz!", 360, py + 20), C3.Hex(0x22202a), Nib(1.3f, 3806));

            // Fuligem de dedo e po de cristal.
            Thumbprint(s, 60, 560, 13f, Soot, 0.6f, 3807);
            Thumbprint(s, 390, 40, 11f, Soot, 0.5f, 3808);
            s.Tint((x, yy) => (DocSurface.Fbm(x * 0.03f, yy * 0.03f, 3809, 3) - 0.6f) * 1.2f, C3.Hex(0x6a5a8a));
            return s.Bake(relief: 1.3f);
        }

        // ------------------------------------------------------------------
        // A placa do chassi da broca
        // ------------------------------------------------------------------

        private static PixelCanvas DrillPlaqueDoc()
        {
            var s = new DocSurface(480, 280);
            s.Shape(DocSurface.Sheet(12, 12, 468, 268, 3901, corner: 4f));
            s.Metal(DocKit.Silver * 0.9f, 3902, 0f, bevel: 5f, gloss: 0.9f, wear: 0.6f);

            C3 groove = C3.Hex(0x30343e);
            DocInk.Engrave(s, CrownCrest(240, 50, 40f), 1f, 1.5f, groove, 0.8f);
            Hand h = Cast(13f, 3903);
            h.Tracking = 2.5f;
            var t = h.Line("POR ORDEM DE SUA MAJESTADE,", 240, 112, 0.5f);
            t.AddRange(h.Line("O REI DE PRATA.", 240, 136, 0.5f));
            t.AddRange(Rule(120, 360, 150, 3f));
            Hand small = Cast(9.5f, 3904);
            small.Tracking = 2f;
            t.AddRange(small.Line("PRIMEIRA PERFURAÇÃO: ANO 925", 240, 176, 0.5f));
            t.AddRange(small.Line("OPERADORES:", 60, 210));
            for (int k = 0; k < 3; k++) t.AddRange(DocInk.Line(160 + k * 100, 212, 240 + k * 100, 212));
            t.AddRange(small.Line("PROFUNDIDADE:", 60, 240));
            t.AddRange(DocInk.Line(176, 242, 300, 242));
            DocInk.Engrave(s, t, 0.9f, 1.4f, groove, 0.85f);

            // Os campos dos operadores, vazios: ninguem gravou nome nenhum.
            // E o Oxido, raspado da metade de cima, ainda crostado embaixo.
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                {
                    int i = x + y * s.W;
                    if (s.A[i] <= 0f) continue;
                    float scrape = y < 150 + (DocSurface.Fbm(x * 0.03f, 0.5f, 3906, 3) - 0.5f) * 60f ? 1f : 0f;
                    if (scrape > 0f) continue;
                    float n = DocSurface.Fbm(x * 0.06f, y * 0.06f, 3907, 3);
                    s.Col[i] = C3.Lerp(s.Col[i], C3.Hex(0x2c5a44) * (0.7f + n * 0.5f), 0.45f + n * 0.35f);
                    s.Ht[i] += n * 2f;
                    s.Gloss[i] *= 0.3f;
                }

            for (int x = 30; x < 470; x += 140)
            {
                s.Rivet(x, 26, 5f, DocKit.Silver * 0.8f, 0.9f);
                s.Rivet(x, 254, 5f, DocKit.Silver * 0.8f, 0.9f);
            }

            return s.Bake(relief: 1.1f);
        }

        // ------------------------------------------------------------------
        // As lajes gravadas: a historia da escavacao, em relevo na pedra
        // ------------------------------------------------------------------

        /// <summary>Um mineiro em entalhe de laje: capacete com lampiao, picareta no ombro, olhando pra baixo.</summary>
        private static List<List<P2>> MinerFigure(float x, float y, float k)
        {
            var f = new List<List<P2>>();
            f.AddRange(DocInk.Circle(x, y - 46 * k, 6f * k, 10));
            f.Add(new List<P2> { new P2(x - 8 * k, y - 50 * k), new P2(x + 8 * k, y - 50 * k) });
            f.Add(new List<P2> { new P2(x + 7 * k, y - 50 * k), new P2(x + 11 * k, y - 52 * k) });
            f.Add(new List<P2> { new P2(x, y - 40 * k), new P2(x + 2 * k, y - 18 * k) });
            f.Add(new List<P2> { new P2(x + 2 * k, y - 18 * k), new P2(x - 6 * k, y) });
            f.Add(new List<P2> { new P2(x + 2 * k, y - 18 * k), new P2(x + 10 * k, y - 2 * k) });
            f.Add(new List<P2> { new P2(x, y - 34 * k), new P2(x - 8 * k, y - 40 * k), new P2(x - 4 * k, y - 48 * k) });
            f.Add(new List<P2> { new P2(x - 14 * k, y - 62 * k), new P2(x + 2 * k, y - 36 * k) });
            f.Add(new List<P2> { new P2(x - 22 * k, y - 58 * k), new P2(x - 14 * k, y - 62 * k), new P2(x - 4 * k, y - 66 * k) });
            return f;
        }

        private static PixelCanvas EngravingDoc(int scene)
        {
            var s = new DocSurface(440, 300);
            int seed = 4001 + scene * 17;
            s.Shape(DocSurface.Sheet(10, 10, 430, 290, seed, 6f, 6f, 6f, 6f));
            s.Stone(C3.Hex(0x3a3640), C3.Hex(0x8a8490), seed + 1, strata: 0.3f, roughness: 0.6f);

            // A moldura cavada da laje.
            DocInk.Carve(s, DocInk.Rect(30, 30, 410, 270), 3f, 3f, C3.Hex(0x2a2830), 0.4f, seed + 2, chips: 0.4f, burr: 0f);

            var cuts = new List<List<P2>>();
            C3 inner = C3.Hex(0x2c2a32);
            switch (scene)
            {
                case 0:
                {
                    // A fila de mineiros descendo a escada, todos olhando pra baixo.
                    for (int k = 0; k < 6; k++)
                    {
                        float x = 70 + k * 58, y = 120 + k * 24;
                        cuts.AddRange(MinerFigure(x, y, 0.9f));
                        cuts.AddRange(DocInk.Line(x - 24, y + 2, x + 36, y + 2));
                    }

                    break;
                }
                case 1:
                {
                    // O Rei, maior que todos, de coroa, sem ferramenta: so aponta pra baixo.
                    float x = 220, y = 250;
                    cuts.AddRange(DocInk.Crown(x, y - 186, 46f));
                    cuts.AddRange(DocInk.Circle(x, y - 150, 18f, 18));
                    cuts.Add(new List<P2> { new P2(x - 34, y - 126), new P2(x + 34, y - 126), new P2(x + 46, y), new P2(x - 46, y), new P2(x - 34, y - 126) });
                    cuts.Add(new List<P2> { new P2(x - 30, y - 118), new P2(x - 70, y - 70), new P2(x - 74, y - 30) });
                    cuts.Add(new List<P2> { new P2(x - 74, y - 30), new P2(x - 74, y - 12) });
                    cuts.Add(new List<P2> { new P2(x - 80, y - 20), new P2(x - 74, y - 10), new P2(x - 68, y - 20) });
                    cuts.Add(new List<P2> { new P2(x + 30, y - 118), new P2(x + 40, y - 70), new P2(x + 34, y - 60) });
                    for (int k = 0; k < 4; k++) cuts.AddRange(DocInk.Line(x - 30 + k * 20, y - 100, x - 36 + k * 24, y - 4));
                    for (int k = 0; k < 3; k++) cuts.AddRange(MinerFigure(70 + k * 26, 262, 0.5f));
                    break;
                }
                case 2:
                {
                    // A broca em espiral furando a rocha; os mineiros, pequenos, atras dela.
                    var spiral = new List<P2>();
                    for (int k = 0; k <= 60; k++)
                    {
                        float t = k / 60f;
                        float r = 40f * (1f - t);
                        spiral.Add(new P2(260 + (float)Math.Cos(t * 18f) * r, 50 + t * 210f));
                    }

                    cuts.Add(spiral);
                    cuts.Add(new List<P2> { new P2(220, 50), new P2(260, 262), new P2(300, 50) });
                    for (int k = 0; k < 10; k++)
                    {
                        double a = Math.PI * (0.15 + k * 0.08);
                        cuts.AddRange(DocInk.Line(260 + (float)Math.Cos(a) * 24f, 262 - (float)Math.Sin(a) * 12f, 260 + (float)Math.Cos(a) * 44f, 262 - (float)Math.Sin(a) * 26f));
                    }

                    for (int k = 0; k < 4; k++) cuts.AddRange(MinerFigure(60 + k * 30, 130 + k * 4, 0.45f));
                    break;
                }
                default:
                {
                    // O Coracao de Quartzo no fundo da terra, com a fenda pintada de verde.
                    cuts.Add(new List<P2> { new P2(220, 50), new P2(330, 160), new P2(220, 262), new P2(110, 160), new P2(220, 50) });
                    cuts.Add(new List<P2> { new P2(220, 80), new P2(290, 160), new P2(220, 236), new P2(150, 160), new P2(220, 80) });
                    cuts.AddRange(DocInk.Line(110, 160, 330, 160));
                    for (int k = 0; k < 8; k++)
                    {
                        double a = k * Math.PI / 4;
                        cuts.AddRange(DocInk.Line(220 + (float)Math.Cos(a) * 130f, 160 + (float)Math.Sin(a) * 110f, 220 + (float)Math.Cos(a) * 150f, 160 + (float)Math.Sin(a) * 124f));
                    }

                    break;
                }
            }

            DocInk.Carve(s, DocInk.Scratchify(cuts, seed + 3, 0.8f, 0.8f, 0.2f), 2.2f, 3.4f, inner, 0.6f, seed + 4, chips: 0.5f, burr: 0.15f);

            if (scene == 1)
            {
                // Partida no meio: a fenda atravessa a laje.
                var crack = new List<P2>();
                float cx = 10;
                for (float yy = 140; cx < 430; cx += 6) crack.Add(new P2(cx, yy + (float)Math.Sin(cx * 0.07f) * 10f + DocSurface.Rand((int)cx, 1, seed) * 6f));
                DocInk.Carve(s, new List<List<P2>> { crack }, 3f, 7f, C3.Hex(0x0e0c10), 0.95f, seed + 5, chips: 1f, burr: 0f);
            }

            if (scene == 3)
            {
                // A fenda de cima a baixo, pintada de verde por quem gravou.
                var rift = new List<List<P2>> { new List<P2> { new P2(220, 52), new P2(214, 100), new P2(226, 140), new P2(212, 190), new P2(222, 260) } };
                DocInk.Carve(s, rift, 3f, 4f, C3.Hex(0x2a9a5a), 1f, seed + 6, chips: 0.3f);
                Paint(s, rift, C3.Hex(0x48d080), 4f, seed + 7, 0.4f, 0.8f);
            }

            // O po de pedra que assenta nos sulcos.
            s.Tint((x, y) => (DocSurface.Fbm(x * 0.03f, y * 0.03f, seed + 8, 3) - 0.62f) * 1.5f, C3.Hex(0x5a5068));
            return s.Bake(relief: 1.3f);
        }
    }
}
