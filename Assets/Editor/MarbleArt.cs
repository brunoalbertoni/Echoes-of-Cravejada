using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A arte do District 4 — The Alchemical Ateliers: marmore, vidro, prata e
    /// mercurio, mais o que mora e o que se le la.
    ///
    /// Como as outras, e desenhada pixel a pixel aqui e nao depende de
    /// UnityEngine: da pra gerar as imagens fora do editor e olhar antes.
    /// </summary>
    public static class MarbleArt
    {
        private static readonly Rgba Outline = new Rgba(26, 28, 40);

        private static readonly Rgba MarbleDark = new Rgba(92, 98, 120);
        private static readonly Rgba MarbleMid = new Rgba(176, 182, 200);
        private static readonly Rgba MarbleLight = new Rgba(236, 238, 246);

        private static readonly Rgba SilverDark = new Rgba(96, 104, 124);
        private static readonly Rgba SilverMid = new Rgba(168, 178, 198);
        private static readonly Rgba SilverLight = new Rgba(232, 238, 250);

        private static readonly Rgba Oxide = new Rgba(92, 242, 153);
        private static readonly Rgba OxideDark = new Rgba(40, 150, 96);

        private static readonly Rgba Copper = new Rgba(184, 110, 70);
        private static readonly Rgba CopperDark = new Rgba(116, 64, 44);

        private static int Hash(int x, int y, int seed)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + seed * 1442695041;
                h = (h ^ (h >> 13)) * 1274126177;
                return (h ^ (h >> 16)) & 0x7fffffff;
            }
        }

        private static void AddOutline(PixelCanvas canvas)
        {
            int width = canvas.Width;
            int height = canvas.Height;
            var filled = new bool[width * height];
            for (int i = 0; i < filled.Length; i++) filled[i] = canvas.Pixels[i].A != 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (filled[x + y * width]) continue;

                    bool touches = (x > 0 && filled[x - 1 + y * width])
                        || (x < width - 1 && filled[x + 1 + y * width])
                        || (y > 0 && filled[x + (y - 1) * width])
                        || (y < height - 1 && filled[x + (y + 1) * width]);

                    if (touches) canvas.Set(x, y, Outline);
                }
            }
        }

        // ------------------------------------------------------------------
        // Terreno
        // ------------------------------------------------------------------

        /// <summary>
        /// O tileset de marmore. Tem exatamente a mesma grade do de latao
        /// (BrassArt.BuildTiles) — bordas, quinas, miolos, plataformas e parede
        /// do fundo nas mesmas celulas — so que relido em pedra branca: cada
        /// pixel vira um tom de marmore pelo quanto era claro, e por cima
        /// entram veios cinzentos e, aqui e ali, uma mancha verde de Oxide.
        /// </summary>
        public static PixelCanvas Tiles()
        {
            PixelCanvas brass = BrassArt.BuildTiles();
            var canvas = new PixelCanvas(brass.Width, brass.Height);

            for (int y = 0; y < brass.Height; y++)
            {
                for (int x = 0; x < brass.Width; x++)
                {
                    Rgba source = brass.Get(x, y);
                    if (source.A == 0) continue;

                    float light = (source.R * 0.45f + source.G * 0.40f + source.B * 0.15f) / 255f;

                    // O latao e escuro; estica pra cobrir do cinza ao branco.
                    light = Math.Min(1f, Math.Max(0f, (light - 0.08f) * 2.3f));

                    Rgba color = light < 0.5f
                        ? Rgba.Lerp(MarbleDark, MarbleMid, light * 2f)
                        : Rgba.Lerp(MarbleMid, MarbleLight, (light - 0.5f) * 2f);

                    // Veios: riscos diagonais finos, quebrados.
                    int vein = (x * 3 + y * 5 + Hash(x / 7, y / 5, 3) % 9) % 23;
                    if (vein == 0 && Hash(x, y, 5) % 3 != 0) color = Rgba.Lerp(color, MarbleDark, 0.45f);

                    // Manchas de Oxide, poucas e pequenas.
                    if (Hash(x / 3, y / 3, 11) % 61 == 0 && Hash(x, y, 13) % 2 == 0) color = Rgba.Lerp(color, Oxide, 0.35f);

                    canvas.Set(x, y, color.WithAlpha(source.A));
                }
            }

            return canvas;
        }

        /// <summary>Mercurio, 16x16, pra repetir: um espelho cinzento com reflexos deitados.</summary>
        public static PixelCanvas Mercury()
        {
            var canvas = new PixelCanvas(16, 16);

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    Rgba color = Rgba.Lerp(SilverMid, SilverDark, y / 15f);

                    // Reflexos compridos, que se desencontram de linha em linha.
                    int streak = (x + y * 5) % 16;
                    if (y % 4 == 1 && streak < 6) color = SilverLight;
                    if (y % 4 == 3 && streak > 9 && streak < 13) color = Rgba.Lerp(color, SilverLight, 0.5f);

                    canvas.Set(x, y, color);
                }
            }

            // A linha de cima e o brilho da superficie.
            canvas.Fill(0, 0, 16, 1, SilverLight);
            canvas.Fill(0, 1, 16, 1, Rgba.Lerp(SilverLight, SilverMid, 0.5f));

            return canvas;
        }

        /// <summary>Vidro, 16x16, pra repetir: quase transparente, com caixilho e um reflexo.</summary>
        public static PixelCanvas Glass()
        {
            var canvas = new PixelCanvas(16, 16);
            var pane = new Rgba(150, 210, 240, 70);
            var shine = new Rgba(230, 248, 255, 150);
            var frame = new Rgba(120, 150, 180, 220);

            canvas.Fill(0, 0, 16, 16, pane);

            for (int i = 3; i < 11; i++) canvas.Set(i, 13 - i, shine);
            for (int i = 6; i < 10; i++) canvas.Set(i + 2, 14 - i, shine);

            canvas.Fill(0, 0, 1, 16, frame);
            canvas.Fill(15, 0, 1, 16, frame);
            canvas.Fill(0, 15, 16, 1, frame);

            return canvas;
        }

        /// <summary>Prata transmutada, 16x16, pra repetir: chapa lisa, clara, com filete.</summary>
        public static PixelCanvas SilverPlate()
        {
            var canvas = new PixelCanvas(16, 16);

            canvas.Fill(0, 0, 16, 16, SilverMid);
            canvas.Fill(0, 0, 16, 2, SilverLight);
            canvas.Fill(0, 14, 16, 2, SilverDark);
            canvas.Fill(0, 0, 1, 16, SilverLight);
            canvas.Fill(15, 0, 1, 16, SilverDark);

            // Um losango gravado no meio.
            for (int i = 0; i < 4; i++)
            {
                canvas.Set(8 - i, 4 + i, SilverDark);
                canvas.Set(8 + i, 4 + i, SilverLight);
                canvas.Set(8 - i, 11 - i, SilverLight);
                canvas.Set(8 + i, 11 - i, SilverDark);
            }

            return canvas;
        }

        /// <summary>Um bloco de marmore solto, 16x16, pra repetir: o que vira areia.</summary>
        public static PixelCanvas MarbleBlock()
        {
            var canvas = new PixelCanvas(16, 16);

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    Rgba color = Rgba.Lerp(MarbleLight, MarbleMid, (Hash(x / 2, y / 2, 21) % 5) / 8f);
                    if ((x * 2 + y * 3) % 19 == 0) color = Rgba.Lerp(color, MarbleDark, 0.5f);
                    canvas.Set(x, y, color);
                }
            }

            // Juntas: e alvenaria, nao rocha.
            canvas.Fill(0, 0, 16, 1, MarbleDark);
            canvas.Fill(0, 8, 16, 1, MarbleDark);
            canvas.Fill(0, 0, 1, 8, MarbleDark);
            canvas.Fill(8, 8, 1, 8, MarbleDark);

            // Rachaduras: da pra ver que cede.
            for (int i = 0; i < 5; i++) canvas.Set(4 + i, 2 + i, MarbleDark);
            for (int i = 0; i < 4; i++) canvas.Set(12 - i, 10 + i, MarbleDark);

            return canvas;
        }

        // ------------------------------------------------------------------
        // Mecanicas
        // ------------------------------------------------------------------

        /// <summary>
        /// Um circulo de transmutacao gravado no chao, 48x8, visto de lado: uma
        /// faixa de prata com a runa no centro. Em cinza claro — a cor entra no jogo.
        /// </summary>
        public static PixelCanvas Circle()
        {
            var canvas = new PixelCanvas(48, 8);
            var line = new Rgba(210, 216, 230);
            var dim = new Rgba(140, 148, 168);

            canvas.Fill(2, 6, 44, 1, line);
            canvas.Fill(0, 7, 48, 1, dim);

            // Marcas a intervalos, como um compasso.
            for (int x = 4; x < 46; x += 6) canvas.Fill(x, 4, 1, 2, dim);

            // A runa: um triangulo com um olho.
            for (int i = 0; i < 5; i++)
            {
                canvas.Set(24 - i, 1 + i, line);
                canvas.Set(24 + i, 1 + i, line);
            }

            canvas.Fill(20, 5, 9, 1, line);
            canvas.Set(24, 3, new Rgba(255, 255, 255));

            return canvas;
        }

        /// <summary>Um espelho de prata num eixo, 16x16, inclinado em "/".</summary>
        public static PixelCanvas Mirror()
        {
            var canvas = new PixelCanvas(16, 16);

            // A moldura e a face, em diagonal.
            for (int i = 1; i < 15; i++)
            {
                canvas.Set(i, 15 - i, SilverLight);
                canvas.Set(i + 1, 15 - i, SilverMid);
                canvas.Set(i, 16 - i, SilverDark);
            }

            // O eixo no centro.
            canvas.Fill(7, 7, 3, 3, CopperDark);
            canvas.Set(8, 8, Copper);

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>O emissor do raio, 16x16, apontando pra direita: uma lanterna de latao com lente.</summary>
        public static PixelCanvas Emitter()
        {
            var canvas = new PixelCanvas(16, 16);

            canvas.Fill(1, 4, 10, 8, CopperDark);
            canvas.Fill(2, 5, 8, 2, Copper);
            canvas.Fill(0, 6, 2, 4, SilverDark);
            canvas.Fill(11, 3, 2, 10, SilverMid);
            canvas.Fill(13, 5, 2, 6, new Rgba(255, 244, 190));
            canvas.Set(14, 7, new Rgba(255, 255, 255));
            canvas.Set(14, 8, new Rgba(255, 255, 255));

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>Uma fechadura de cristal, 16x16: um losango facetado numa moldura de prata.</summary>
        public static PixelCanvas Lock()
        {
            var canvas = new PixelCanvas(16, 16);

            canvas.Fill(1, 1, 14, 14, SilverDark);
            canvas.Fill(2, 2, 12, 12, new Rgba(48, 52, 70));

            for (int y = 3; y < 13; y++)
            {
                int half = y < 8 ? y - 3 : 12 - y;

                for (int dx = -half; dx <= half; dx++)
                {
                    Rgba color = dx < 0 ? new Rgba(236, 240, 255) : new Rgba(170, 190, 240);
                    if (dx == 0) color = new Rgba(255, 255, 255);
                    canvas.Set(8 + dx, y, color);
                }
            }

            return canvas;
        }

        /// <summary>Uma gota de mercurio, 6x8.</summary>
        public static PixelCanvas Drop()
        {
            var canvas = new PixelCanvas(6, 8);

            canvas.Fill(2, 0, 2, 2, SilverMid);
            canvas.Fill(1, 2, 4, 5, SilverMid);
            canvas.Fill(2, 7, 2, 1, SilverDark);
            canvas.Fill(4, 3, 1, 3, SilverDark);
            canvas.Set(2, 3, SilverLight);
            canvas.Set(2, 4, SilverLight);

            return canvas;
        }

        /// <summary>Um frasco arremessado, 8x10: vidro redondo com Oxide dentro.</summary>
        public static PixelCanvas Flask()
        {
            var canvas = new PixelCanvas(8, 10);
            var glass = new Rgba(200, 230, 245);

            canvas.Fill(3, 0, 2, 1, CopperDark);
            canvas.Fill(3, 1, 2, 3, glass);
            canvas.Fill(1, 4, 6, 5, glass);
            canvas.Fill(2, 5, 4, 4, Oxide);
            canvas.Fill(2, 8, 4, 1, OxideDark);
            canvas.Set(2, 5, new Rgba(220, 255, 230));

            AddOutline(canvas);
            return canvas;
        }

        // ------------------------------------------------------------------
        // Cenario
        // ------------------------------------------------------------------

        /// <summary>
        /// Um tanque de vidro, 32x64, com uma silhueta suspensa no liquido:
        /// um Tarnished usado nos experimentos.
        /// </summary>
        public static PixelCanvas Tank()
        {
            var canvas = new PixelCanvas(32, 64);
            var liquid = new Rgba(70, 150, 120, 150);
            var liquidLight = new Rgba(120, 210, 160, 150);
            var body = new Rgba(24, 44, 40, 230);

            // Tampa e base de prata.
            canvas.Fill(2, 0, 28, 5, SilverDark);
            canvas.Fill(3, 1, 26, 2, SilverMid);
            canvas.Fill(1, 57, 30, 7, SilverDark);
            canvas.Fill(2, 58, 28, 2, SilverMid);

            // O liquido.
            canvas.Fill(4, 5, 24, 52, liquid);
            canvas.Fill(5, 5, 3, 52, liquidLight);

            // A silhueta: cabeca caida, bracos soltos, pernas juntas.
            canvas.Fill(13, 16, 6, 6, body);
            canvas.Fill(11, 22, 10, 14, body);
            canvas.Fill(9, 23, 2, 14, body);
            canvas.Fill(21, 23, 2, 14, body);
            canvas.Fill(12, 36, 3, 13, body);
            canvas.Fill(17, 36, 3, 13, body);

            // Um tubo saindo da nuca pra tampa.
            for (int y = 5; y < 17; y++) canvas.Set(16 + (y % 4 < 2 ? 0 : 1), y, body);

            // Bolhas.
            canvas.Set(8, 44, new Rgba(220, 255, 235));
            canvas.Set(24, 30, new Rgba(220, 255, 235));
            canvas.Set(23, 12, new Rgba(220, 255, 235));

            // O vidro: bordas e um reflexo.
            canvas.Fill(3, 5, 1, 52, new Rgba(200, 235, 245));
            canvas.Fill(28, 5, 1, 52, new Rgba(120, 160, 180));
            for (int y = 8; y < 30; y++) canvas.Set(25, y, new Rgba(220, 245, 250, 120));

            return canvas;
        }

        /// <summary>
        /// Um retrato da nobreza, 24x32. Variante 0 e 1: Nobles de prata, com o
        /// rosto riscado de verde. Variante 2: um jovem de monoculo, sem risco
        /// nenhum — Crisol, antes de ser descartado.
        /// </summary>
        public static PixelCanvas Portrait(int variant)
        {
            var canvas = new PixelCanvas(24, 32);
            var gold = new Rgba(190, 160, 90);
            var goldDark = new Rgba(120, 96, 52);
            var back = variant == 1 ? new Rgba(56, 40, 60) : new Rgba(40, 48, 66);

            // Moldura.
            canvas.Fill(0, 0, 24, 32, goldDark);
            canvas.Fill(1, 1, 22, 30, gold);
            canvas.Fill(3, 3, 18, 26, back);

            // Ombros e gola.
            canvas.Fill(5, 22, 14, 7, variant == 2 ? new Rgba(70, 76, 110) : new Rgba(90, 60, 80));
            canvas.Fill(9, 20, 6, 3, SilverLight);

            // O rosto de prata.
            canvas.Fill(8, 9, 8, 11, SilverMid);
            canvas.Fill(8, 9, 3, 10, SilverLight);
            canvas.Fill(8, 7, 8, 3, variant == 2 ? new Rgba(60, 50, 44) : SilverDark);

            // Olhos e boca.
            canvas.Set(10, 13, Outline);
            canvas.Set(14, 13, Outline);
            canvas.Fill(11, 17, 3, 1, SilverDark);

            if (variant == 2)
            {
                // O monoculo, e a correntinha.
                canvas.Fill(13, 12, 3, 3, gold);
                canvas.Set(14, 13, new Rgba(200, 230, 255));
                canvas.Set(16, 15, gold);
                canvas.Set(16, 17, gold);
            }
            else
            {
                // Riscado de Oxide, com raiva.
                for (int i = 0; i < 10; i++)
                {
                    canvas.Set(7 + i, 9 + i, Oxide);
                    canvas.Set(16 - i, 9 + i, Oxide);
                    if (i % 2 == 0) canvas.Set(8 + i, 9 + i, OxideDark);
                }
            }

            return canvas;
        }

        /// <summary>Um vitral gotico, 32x48, rachado. Variante 1 e mais esverdeada.</summary>
        public static PixelCanvas Window(int variant)
        {
            var canvas = new PixelCanvas(32, 48);
            var lead = new Rgba(40, 44, 60);

            Rgba[] panes =
            {
                new Rgba(90, 130, 200), new Rgba(150, 110, 190), new Rgba(200, 200, 230),
                variant == 1 ? new Rgba(90, 200, 140) : new Rgba(120, 170, 220),
            };

            for (int y = 0; y < 48; y++)
            {
                // Arco ogival em cima.
                int inset = y < 14 ? (14 - y) * (14 - y) / 14 : 0;

                for (int x = 2 + inset; x < 30 - inset; x++)
                {
                    Rgba color = panes[(x / 7 + y / 9 + variant) % panes.Length];
                    if (x % 7 == 0 || y % 9 == 0) color = lead;
                    canvas.Set(x, y, color);
                }
            }

            // A rachadura.
            int cx = 20;
            for (int y = 10; y < 40; y++)
            {
                cx += Hash(y, variant, 31) % 3 - 1;
                canvas.Set(cx, y, lead);
            }

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>Uma estante de laboratorio, 32x32, com frascos e alambiques borbulhando.</summary>
        public static PixelCanvas Shelf()
        {
            var canvas = new PixelCanvas(32, 32);
            var wood = new Rgba(70, 56, 60);
            var woodLight = new Rgba(100, 84, 86);
            var glass = new Rgba(190, 225, 240, 200);

            canvas.Fill(0, 0, 32, 32, wood);
            canvas.Fill(1, 1, 30, 30, new Rgba(34, 34, 48));
            canvas.Fill(0, 10, 32, 2, woodLight);
            canvas.Fill(0, 21, 32, 2, woodLight);
            canvas.Fill(0, 30, 32, 2, woodLight);

            for (int shelf = 0; shelf < 3; shelf++)
            {
                int floor = shelf == 0 ? 9 : (shelf == 1 ? 20 : 29);

                for (int i = 0; i < 4; i++)
                {
                    int x = 3 + i * 7 + Hash(i, shelf, 41) % 2;
                    int tall = 4 + Hash(i, shelf, 43) % 4;
                    Rgba fill = Hash(i, shelf, 47) % 3 == 0 ? SilverMid : Oxide;

                    canvas.Fill(x, floor - tall + 1, 4, tall, glass);
                    canvas.Fill(x + 1, floor - tall + 3, 2, tall - 3, fill);
                    canvas.Fill(x + 1, floor - tall, 2, 1, CopperDark);
                }
            }

            return canvas;
        }

        /// <summary>O armario de reagentes onde a Assistente se esconde, 24x40, de porta entreaberta.</summary>
        public static PixelCanvas Cabinet()
        {
            var canvas = new PixelCanvas(24, 40);
            var wood = new Rgba(78, 62, 64);
            var woodLight = new Rgba(112, 94, 94);

            canvas.Fill(0, 0, 24, 40, wood);
            canvas.Fill(1, 1, 22, 38, woodLight);
            canvas.Fill(2, 2, 9, 36, wood);

            // A fresta: escuro, e dois olhos de cobre la dentro.
            canvas.Fill(12, 2, 10, 36, new Rgba(16, 14, 22));
            canvas.Set(15, 16, Copper);
            canvas.Set(18, 16, Copper);

            canvas.Set(10, 20, SilverLight);
            canvas.Fill(0, 38, 24, 2, wood);

            return canvas;
        }

        /// <summary>
        /// A ficha do prototipo, 32x32: uma prancheta com o desenho tecnico do
        /// proprio Scrap — a cabeca de pedra, os dois olhos, a mola — e um
        /// carimbo vermelho atravessado.
        /// </summary>
        public static PixelCanvas PrototypeSheet()
        {
            var canvas = new PixelCanvas(32, 32);
            var paper = new Rgba(222, 214, 190);
            var ink = new Rgba(60, 70, 110);
            var stamp = new Rgba(190, 50, 50);

            canvas.Fill(1, 0, 30, 32, new Rgba(86, 66, 56));
            canvas.Fill(3, 3, 26, 27, paper);
            canvas.Fill(12, 0, 8, 3, SilverMid);

            // O desenho: cabeca redonda, olhos, corpo, mola no topo.
            for (int x = 10; x < 20; x++)
            {
                canvas.Set(x, 8, ink);
                canvas.Set(x, 17, ink);
            }

            for (int y = 9; y < 17; y++)
            {
                canvas.Set(9, y, ink);
                canvas.Set(20, y, ink);
            }

            canvas.Fill(12, 12, 2, 2, ink);
            canvas.Fill(16, 12, 2, 2, ink);
            canvas.Fill(12, 18, 6, 6, ink);
            canvas.Fill(13, 19, 4, 4, paper);
            canvas.Set(14, 7, ink);
            canvas.Set(15, 6, ink);
            canvas.Set(14, 5, ink);

            // Linhas de cota e de texto.
            canvas.Fill(5, 8, 1, 10, ink);
            canvas.Fill(22, 10, 5, 1, ink);
            canvas.Fill(22, 13, 4, 1, ink);
            canvas.Fill(5, 26, 14, 1, ink);

            // O carimbo, na diagonal.
            for (int i = 0; i < 20; i++)
            {
                canvas.Set(6 + i, 24 - i / 2, stamp);
                canvas.Set(6 + i, 25 - i / 2, stamp);
                if (i % 3 != 0) canvas.Set(6 + i, 21 - i / 2, stamp);
            }

            return canvas;
        }

        // ------------------------------------------------------------------
        // Inimigos e gente
        // ------------------------------------------------------------------

        /// <summary>Quicksilver Slime, 20x14: uma gota gorda de mercurio, com um nucleo de metal verde.</summary>
        public static PixelCanvas Slime()
        {
            var canvas = new PixelCanvas(20, 14);

            for (int y = 2; y < 14; y++)
            {
                float t = (y - 2) / 11f;
                int half = (int)Math.Round(3 + 6 * Math.Sqrt(t));

                for (int dx = -half; dx <= half; dx++)
                {
                    Rgba color = dx < -half / 2 ? SilverLight : (dx > half / 2 ? SilverDark : SilverMid);
                    canvas.Set(10 + dx, y, color);
                }
            }

            // O nucleo.
            canvas.Fill(9, 8, 3, 3, OxideDark);
            canvas.Set(10, 9, Oxide);

            // Um reflexo.
            canvas.Fill(6, 5, 2, 1, new Rgba(255, 255, 255));

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>Flask Thrower, 16x26, olhando pra direita: jaleco manchado, um frasco na mao erguida.</summary>
        public static PixelCanvas FlaskThrower()
        {
            var canvas = new PixelCanvas(16, 26);
            var coat = new Rgba(200, 204, 214);
            var coatDark = new Rgba(140, 146, 164);

            // Pernas.
            canvas.Fill(5, 20, 2, 6, CopperDark);
            canvas.Fill(9, 20, 2, 6, CopperDark);

            // Jaleco comprido.
            canvas.Fill(4, 9, 8, 13, coat);
            canvas.Fill(9, 9, 3, 13, coatDark);
            canvas.Fill(5, 15, 2, 3, Oxide);
            canvas.Set(8, 19, OxideDark);

            // Cabeca de cobre, com oculos de protecao.
            canvas.Fill(5, 3, 6, 6, Copper);
            canvas.Fill(5, 5, 6, 2, Outline);
            canvas.Set(7, 5, Oxide);
            canvas.Set(10, 5, Oxide);

            // O braco erguido e o frasco.
            canvas.Fill(12, 6, 2, 5, coat);
            canvas.Fill(12, 2, 3, 4, new Rgba(200, 230, 245));
            canvas.Fill(13, 3, 1, 3, Oxide);

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>
        /// Failed Experiment, 24x28, olhando pra direita: metade prata, fina e
        /// comprida; metade cobre, grossa e curta; costura no meio.
        /// </summary>
        public static PixelCanvas FailedExperiment()
        {
            var canvas = new PixelCanvas(24, 28);

            // Metade de cobre (atras): perna curta e grossa, braco pesado.
            canvas.Fill(4, 19, 6, 9, CopperDark);
            canvas.Fill(5, 19, 2, 8, Copper);
            canvas.Fill(4, 8, 8, 12, Copper);
            canvas.Fill(4, 8, 3, 11, new Rgba(214, 140, 96));
            canvas.Fill(1, 10, 4, 12, CopperDark);

            // Metade de prata (na frente): perna fina e longa, braco em lamina.
            canvas.Fill(14, 16, 3, 12, SilverDark);
            canvas.Fill(14, 16, 1, 11, SilverLight);
            canvas.Fill(12, 6, 7, 11, SilverMid);
            canvas.Fill(16, 6, 3, 11, SilverDark);
            canvas.Fill(19, 9, 5, 2, SilverLight);
            canvas.Fill(22, 11, 2, 2, SilverMid);

            // A costura: grampos verdes no meio do peito.
            for (int y = 7; y < 19; y += 3)
            {
                canvas.Fill(11, y, 3, 1, Oxide);
                canvas.Set(12, y + 1, OxideDark);
            }

            // A cabeca, torta: um olho de cada material.
            canvas.Fill(7, 1, 10, 6, SilverMid);
            canvas.Fill(7, 1, 5, 6, Copper);
            canvas.Set(9, 3, Oxide);
            canvas.Fill(13, 3, 2, 2, Oxide);

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>
        /// Transmuted Sentinel, 16x30, olhando pra direita: uma armadura vazia,
        /// em branco e cinza — a cor (vidro ou prata) entra no jogo.
        /// </summary>
        public static PixelCanvas Sentinel()
        {
            var canvas = new PixelCanvas(16, 30);
            var light = new Rgba(245, 247, 252);
            var mid = new Rgba(196, 202, 216);
            var dark = new Rgba(132, 140, 160);

            // Pernas.
            canvas.Fill(4, 19, 3, 11, mid);
            canvas.Fill(9, 19, 3, 11, dark);
            canvas.Fill(3, 28, 5, 2, light);
            canvas.Fill(8, 28, 5, 2, mid);

            // Peitoral.
            canvas.Fill(3, 8, 10, 12, mid);
            canvas.Fill(4, 9, 3, 10, light);
            canvas.Fill(10, 9, 3, 10, dark);
            canvas.Fill(3, 14, 10, 1, dark);

            // Ombreiras.
            canvas.Fill(1, 7, 4, 4, light);
            canvas.Fill(11, 7, 4, 4, dark);

            // Elmo, com a fenda vazia.
            canvas.Fill(5, 1, 6, 7, mid);
            canvas.Fill(5, 1, 2, 6, light);
            canvas.Fill(7, 4, 4, 1, Outline);

            // A lanca curta, na frente.
            canvas.Fill(14, 5, 1, 18, dark);
            canvas.Fill(13, 3, 3, 3, light);

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>A Assistente de Laboratorio, 12x20, olhando pra direita: pequena, de cobre, de avental.</summary>
        public static PixelCanvas Assistant()
        {
            var canvas = new PixelCanvas(12, 20);
            var apron = new Rgba(210, 214, 224);

            canvas.Fill(3, 15, 2, 5, CopperDark);
            canvas.Fill(7, 15, 2, 5, CopperDark);
            canvas.Fill(2, 8, 8, 8, Copper);
            canvas.Fill(3, 9, 6, 7, apron);
            canvas.Set(5, 11, SilverDark);
            canvas.Set(5, 13, SilverDark);

            canvas.Fill(3, 2, 6, 6, Copper);
            canvas.Fill(3, 2, 2, 5, new Rgba(220, 150, 104));
            canvas.Set(6, 4, new Rgba(255, 236, 190));
            canvas.Set(8, 4, new Rgba(255, 236, 190));

            // Um lenco na cabeca.
            canvas.Fill(3, 1, 6, 2, apron);

            AddOutline(canvas);
            return canvas;
        }
    }
}
