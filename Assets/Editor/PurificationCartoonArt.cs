using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Os quadros da cutscene desenhada da purificacao do Scrap (fim da Phase 1):
    /// de costas, o braco, o cinzel, o tubo de ar e a pose final.
    ///
    /// Cada quadro tem 320x180 e sai em tres imagens: o "antes" (sucata), o
    /// "depois" (polido) e o mapa de ordem — no canal vermelho, de 0 a 254, quem
    /// e mais escuro se transforma primeiro; 255 nunca muda. No jogo, o
    /// PurificationCartoon varre esse mapa com um limite que sobe, e a borda da
    /// transformacao brilha.
    ///
    /// As faixas pretas de cinema cobrem ~12 pixels em cima e embaixo: o que
    /// importa fica entre y 14 e 166.
    ///
    /// Tudo e desenhado com um "pintor" simples: formas (elipse, capsula,
    /// poligono, gema lapidada) com normal, luz difusa e especular, contorno,
    /// luz de borda e textura do material (ferrugem, pedra, cristal, pano,
    /// couro, metal, madeira). As cores sao as do HeroArt.
    ///
    /// Nao depende de UnityEngine, como o resto da arte gerada.
    /// </summary>
    public static class PurificationCartoonArt
    {
        public const int Width = 320;
        public const int Height = 180;

        /// <summary>Valor do mapa de ordem pra pixel que nunca muda.</summary>
        public const byte Static = 255;

        public enum EmitterKind { Flakes, Sparks, Steam, Glint }

        /// <summary>Uma fonte de particulas do quadro, em pixels do quadro (y pra baixo).</summary>
        public readonly struct Emitter
        {
            public readonly EmitterKind Kind;
            public readonly int X;
            public readonly int Y;
            public readonly int W;
            public readonly int H;

            /// <summary>Em que ponto da transformacao (0 a 1) ela comeca.</summary>
            public readonly float StartAt;

            public Emitter(EmitterKind kind, int x, int y, int w, int h, float startAt)
            {
                Kind = kind;
                X = x;
                Y = y;
                W = w;
                H = h;
                StartAt = startAt;
            }
        }

        public sealed class Shot
        {
            public string Name;
            public PixelCanvas Before;
            public PixelCanvas After;
            public PixelCanvas Order;

            /// <summary>Quadros que se repetem depois da transformacao (a capa na pose final). Nulo: nenhum.</summary>
            public PixelCanvas[] Loop;

            public Emitter[] Emitters;
            public float Duration;

            /// <summary>Quanto a camera desliza ao longo do quadro, em pixels.</summary>
            public float PanX;
            public float PanY;

            public float ZoomFrom = 1f;
            public float ZoomTo = 1f;
        }

        public static Shot[] All()
        {
            return new[] { Back(), Arm(), Chisel(), Vent(), Hero() };
        }

        // ------------------------------------------------------------------
        // Cores e materiais
        // ------------------------------------------------------------------

        private static readonly Rgba Ink = new Rgba(18, 14, 22);

        private static readonly Rgba[] CaveRock =
        {
            new Rgba(8, 11, 17), new Rgba(15, 21, 28), new Rgba(24, 33, 40), new Rgba(36, 48, 54),
        };

        private static readonly Rgba RustOrange = new Rgba(176, 84, 38);
        private static readonly Rgba DeepRust = new Rgba(84, 36, 22);
        private static readonly Rgba Purify = new Rgba(150, 232, 246);
        private static readonly Rgba PurifyCore = new Rgba(255, 252, 236);
        private static readonly Rgba Teal = new Rgba(86, 220, 220);

        private enum Surface { Plain, Rust, Rock, Crystal, Cloth, Leather, Metal, Wood, Blade, Moss }

        private sealed class Mat
        {
            public readonly Rgba[] Tones;
            public readonly Surface Surface;
            public readonly Rgba Accent;

            /// <summary>Forca do brilho especular (0: fosco).</summary>
            public readonly float Shine;

            public Mat(Rgba[] tones, Surface surface, Rgba accent, float shine)
            {
                Tones = tones;
                Surface = surface;
                Accent = accent;
                Shine = shine;
            }
        }

        private static readonly HeroArt.Look S = HeroArt.Scrap();
        private static readonly HeroArt.Look P = HeroArt.Polished();

        private static readonly Rgba[] RustyTones =
        {
            new Rgba(52, 30, 24), new Rgba(96, 54, 36), new Rgba(140, 82, 50), new Rgba(178, 116, 70),
        };

        private static readonly Rgba[] BronzeTones =
        {
            new Rgba(70, 44, 30), new Rgba(128, 86, 50), new Rgba(184, 136, 80), new Rgba(226, 186, 120),
        };

        private static readonly Rgba[] RustBladeTones =
        {
            new Rgba(58, 42, 38), new Rgba(98, 70, 56), new Rgba(138, 100, 76), new Rgba(172, 136, 104),
        };

        private static readonly Rgba[] SilverTones =
        {
            new Rgba(62, 72, 94), new Rgba(108, 122, 148), new Rgba(160, 176, 198), new Rgba(208, 220, 236),
        };

        private static readonly Rgba[] GemTones =
        {
            new Rgba(76, 102, 140), new Rgba(130, 166, 204), new Rgba(186, 214, 238), new Rgba(232, 244, 255),
        };

        private static Mat RustMetal() => new Mat(RustyTones, Surface.Rust, RustOrange, 0.05f);
        private static Mat Bronze() => new Mat(BronzeTones, Surface.Metal, new Rgba(255, 244, 210), 0.65f);
        private static Mat Rock() => new Mat(S.Stone, Surface.Rock, S.Fleck, 0.05f);
        private static Mat Quartz() => new Mat(GemTones, Surface.Crystal, new Rgba(255, 255, 255), 0.55f);
        private static Mat RawCloth() => new Mat(S.Cloth, Surface.Cloth, S.Trim, 0f);
        private static Mat Leather() => new Mat(P.Cloth, Surface.Leather, P.Trim, 0.12f);
        private static Mat RustSpring() => new Mat(S.Spring, Surface.Rust, RustOrange, 0.05f);
        private static Mat BrightSpring() => new Mat(P.Spring, Surface.Metal, new Rgba(255, 240, 200), 0.6f);
        private static Mat Gold() => new Mat(new[] { new Rgba(110, 72, 28), new Rgba(176, 124, 46), new Rgba(226, 178, 80), new Rgba(255, 228, 150) }, Surface.Metal, new Rgba(255, 250, 220), 0.7f);
        private static Mat Wood() => new Mat(S.Wood, Surface.Wood, S.Wood[3], 0.05f);
        private static Mat RustBlade() => new Mat(RustBladeTones, Surface.Rust, RustOrange, 0.05f);
        private static Mat Silver() => new Mat(SilverTones, Surface.Blade, new Rgba(255, 255, 255), 0.3f);
        private static Mat Shade(Rgba[] tones) => new Mat(tones, Surface.Plain, tones[3], 0f);

        // ------------------------------------------------------------------
        // Quadro 1: de costas
        // ------------------------------------------------------------------

        /// <summary>
        /// O Scrap de costas, contra a luz do coracao da area, na boca de uma
        /// galeria. A transformacao comeca na cabeca e desce ate os pes; por
        /// ultimo a luz do fundo cresce.
        /// </summary>
        public static Shot Back()
        {
            var p = new Painter { LX = 0f, LY = -0.75f, LZ = 0.35f, Ambient = 0.12f, Diffuse = 0.5f };

            const float glowX = 160f, glowY = 58f;

            p.Backdrop(
                (x, y) => Gallery(x, y, glowX, glowY, 0.7f),
                (x, y) => Gallery(x, y, glowX, glowY, 1.0f),
                (x, y) => 150 + (int)(Dist(x, y, glowX, glowY) * 0.5f));

            OrderFn figure = Radial(160, 50, 1.1f, 0, 24f);

            // O cinzel, na mao direita dele (a direita da tela), de lamina pra cima.
            p.Capsule(204, 150, 216, 70, 3.5f, 3.5f, Wood(), Wood(), figure, 1);
            p.Capsule(215, 72, 217, 62, 5.5f, 5.5f, RustMetal(), Bronze(), figure, 2);
            ChiselBlade(p, 217, 62, 224, 18, 9f, figure, 3);

            // Pernas e pes.
            p.Capsule(149, 140, 147, 154, 7, 7, RustMetal(), Bronze(), figure, 4);
            p.Capsule(171, 140, 173, 154, 7, 7, RustMetal(), Bronze(), figure, 5);
            p.Ellipse(146, 157, 11, 5, RustMetal(), Bronze(), figure, 6);
            p.Ellipse(174, 157, 11, 5, RustMetal(), Bronze(), figure, 7);

            // Corpo.
            p.Ellipse(160, 120, 33, 27, RustMetal(), Bronze(), figure, 8);

            // Bracos e luvas.
            p.Capsule(130, 102, 120, 130, 7, 6, RustMetal(), Bronze(), figure, 9);
            p.Ellipse(119, 134, 9, 8, RustMetal(), Bronze(), figure, 10);
            p.Capsule(190, 102, 202, 124, 7, 6, RustMetal(), Bronze(), figure, 11);
            p.Ellipse(205, 128, 9, 8, RustMetal(), Bronze(), figure, 12);

            // A capa, por cima das costas.
            BackCape(p, 160, 88, 150, figure, 13);

            // A cabeca de pedra (cristal, depois), vista por tras.
            Gem(p, 160, 66, 34, 28, 0f, 6f, figure, 14, seed: 5);

            // A mola no alto.
            Spring(p, 160, 40, 6f, 4, figure, 20);

            p.Finish();
            p.Rim(PurifyCore, 2, glowX, glowY, 0.95f);

            return new Shot
            {
                Name = "back",
                Before = p.Before,
                After = p.After,
                Order = p.Order,
                Emitters = new[]
                {
                    new Emitter(EmitterKind.Flakes, 118, 50, 92, 100, 0.1f),
                    new Emitter(EmitterKind.Sparks, 120, 24, 80, 60, 0.3f),
                },
                Duration = 3.0f,
                ZoomFrom = 1.0f,
                ZoomTo = 1.1f,
                PanY = -8f,
            };
        }

        // ------------------------------------------------------------------
        // Quadro 2: o braco
        // ------------------------------------------------------------------

        /// <summary>
        /// Close do antebraco e da luva segurando o cabo. A crosta racha (a luz
        /// vaza pelas rachaduras primeiro), e as placas viram bronze do cotovelo
        /// pra mao.
        /// </summary>
        public static Shot Arm()
        {
            var p = new Painter { LX = 0.5f, LY = -0.7f, LZ = 0.5f, Ambient = 0.16f, Diffuse = 0.62f };

            p.Backdrop(
                (x, y) => Bokeh(x, y, 0.5f, 1),
                (x, y) => Bokeh(x, y, 1.0f, 1),
                (x, y) => 205 + (int)(Hash(x / 8, y / 8, 3) * 40f));

            const float ax = -30f, ay = 44f, bx = 172f, by = 102f;
            OrderFn plates = (x, y) =>
            {
                float t = Along(x, y, ax, ay, bx, by);
                int band = t < 0.36f ? 0 : (t < 0.68f ? 1 : 2);
                return 20 + band * 40 + (int)((Noise(x / 6f, y / 6f, 4) - 0.5f) * 12f);
            };

            // O cabo, atras da mao.
            OrderFn grip = (x, y) => 150 + (int)((Noise(x / 5f, y / 5f, 9) - 0.5f) * 14f);
            p.Capsule(266, -16, 186, 196, 12, 12, Wood(), Wood(), grip, 1);

            // O antebraco, mais grosso no cotovelo.
            p.Capsule(ax, ay, bx, by, 42, 33, RustMetal(), Bronze(), plates, 2);

            foreach (float t in new[] { 0.36f, 0.68f })
            {
                float cx = ax + (bx - ax) * t, cy = ay + (by - ay) * t;
                float r = 42f + (33f - 42f) * t;
                Groove(p, cx, cy, bx - ax, by - ay, r, plates, 3);
            }

            // A luva redonda.
            OrderFn glove = (x, y) => 140 + (int)((Noise(x / 6f, y / 6f, 6) - 0.5f) * 12f);
            p.Ellipse(210, 112, 48, 44, RustMetal(), Bronze(), glove, 4);

            // Os dedos fechados no cabo, e o polegar por cima.
            OrderFn fingers = (x, y) => 172 + (int)(Along(x, y, 196, 80, 240, 170) * 30f);
            for (int i = 0; i < 4; i++)
            {
                float fy = 94 + i * 18;
                p.Capsule(204, fy, 244, fy + 5, 9.5f, 8.5f, RustMetal(), Bronze(), fingers, 5 + i);
            }

            p.Capsule(178, 84, 228, 72, 10.5f, 8.5f, RustMetal(), Bronze(), fingers, 9);

            // Rachaduras na crosta: acendem antes da placa em volta.
            var cracks = new Random(21);
            for (int i = 0; i < 9; i++)
            {
                float t = 0.08f + 0.86f * i / 8f;
                float sx = ax + (bx - ax) * t + cracks.Next(-6, 7);
                float sy = ay + (by - ay) * t + cracks.Next(-24, 25);
                Crack(p, sx, sy, cracks, 34, plates, -34, 2);
            }

            Crack(p, 200, 110, cracks, 40, glove, -40, 4);
            Crack(p, 226, 132, cracks, 30, glove, -40, 4);

            p.Finish();
            p.Rim(new Rgba(255, 236, 190), 1, 0.7f, -0.7f, 0.7f, point: false);
            Shine(p, ax, ay, bx, by, 26f, plates);

            return new Shot
            {
                Name = "arm",
                Before = p.Before,
                After = p.After,
                Order = p.Order,
                Emitters = new[]
                {
                    new Emitter(EmitterKind.Flakes, 10, 30, 230, 110, 0.0f),
                    new Emitter(EmitterKind.Sparks, 150, 60, 120, 100, 0.55f),
                },
                Duration = 2.5f,
                PanX = -12f,
                ZoomFrom = 1.06f,
                ZoomTo = 1.0f,
            };
        }

        // ------------------------------------------------------------------
        // Quadro 3: o cinzel
        // ------------------------------------------------------------------

        /// <summary>
        /// O cinzel em diagonal, do cabo (embaixo, a esquerda) a ponta. A
        /// ferrugem descasca do cabo pra ponta e um brilho corre pelo fio.
        /// </summary>
        public static Shot Chisel()
        {
            var p = new Painter { LX = -0.45f, LY = -0.7f, LZ = 0.55f, Ambient = 0.18f, Diffuse = 0.62f };

            p.Backdrop(
                (x, y) => Bokeh(x, y, 0.45f, 2),
                (x, y) => Bokeh(x, y, 0.9f, 2),
                (x, y) => 215 + (int)(Hash(x / 8, y / 8, 4) * 35f));

            const float hx = -14f, hy = 196f; // ponta do cabo
            const float cx = 104f, cy = 126f; // gola
            const float tx = 300f, ty = 28f;  // ponta da lamina

            OrderFn axis = (x, y) => (int)(Along(x, y, cx, cy, tx, ty) * 200f) + 30
                                     + (int)((Noise(x / 7f, y / 7f, 2) - 0.5f) * 22f);

            // O cabo de madeira, com amarracao: pano velho antes, couro com aro dourado depois.
            OrderFn wrap = (x, y) => (int)(Along(x, y, hx, hy, cx, cy) * 26f);
            p.Capsule(hx, hy, cx, cy, 11, 11, Wood(), Wood(), wrap, 1);

            float hdx = cx - hx, hdy = cy - hy;
            float hlen = (float)Math.Sqrt(hdx * hdx + hdy * hdy);
            float hux = hdx / hlen, huy = hdy / hlen;
            for (int i = 0; i < 6; i++)
            {
                float t = 0.3f + i * 0.1f;
                float wx = hx + hdx * t, wy = hy + hdy * t;
                p.Capsule(wx - hux * 2, wy - huy * 2, wx + hux * 2, wy + huy * 2, 12.5f, 12.5f, RawCloth(), Leather(), wrap, 2 + i);
            }

            foreach (float t in new[] { 0.24f, 0.86f })
            {
                float wx = hx + hdx * t, wy = hy + hdy * t;
                p.Capsule(wx - hux * 1.5f, wy - huy * 1.5f, wx + hux * 1.5f, wy + huy * 1.5f, 13f, 13f, RustMetal(), Gold(), wrap, 9);
            }

            // A lamina larga, com chanfro na ponta.
            ChiselBlade(p, cx + hux * 10, cy + huy * 10, tx, ty, 40f, axis, 10);

            // A gola de metal entre o cabo e a lamina.
            p.Capsule(cx - hux * 6, cy - huy * 6, cx + hux * 12, cy + huy * 12, 16, 15, RustMetal(), Bronze(), axis, 12);

            // Mossas no fio: so na sucata a lamina tem dentes faltando.
            float dx = tx - cx, dy = ty - cy;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            float ux = dx / len, uy = dy / len;
            var nicks = new Random(8);
            for (int i = 0; i < 5; i++)
            {
                float along = 50 + i * (len - 100) / 4f + nicks.Next(-6, 7);
                float nx = cx + ux * along - uy * 20f, ny = cy + uy * along + ux * 20f;
                p.CarveBefore((int)nx, (int)ny, 2 + nicks.Next(0, 3), axis);
            }

            p.Finish();
            p.Rim(new Rgba(255, 244, 214), 1, -0.6f, -0.8f, 0.6f, point: false);

            return new Shot
            {
                Name = "chisel",
                Before = p.Before,
                After = p.After,
                Order = p.Order,
                Emitters = new[]
                {
                    new Emitter(EmitterKind.Flakes, 110, 24, 200, 110, 0.0f),
                    new Emitter(EmitterKind.Glint, 112, 28, 188, 98, 0.85f),
                },
                Duration = 2.5f,
                ZoomFrom = 1.0f,
                ZoomTo = 1.12f,
            };
        }

        // ------------------------------------------------------------------
        // Quadro 4: o tubo de ar
        // ------------------------------------------------------------------

        /// <summary>
        /// Um respiro da caverna saindo da rocha. A ferrugem recua de baixo pra
        /// cima, as juntas acendem e, em volta, o musgo e os cristais voltam.
        /// </summary>
        public static Shot Vent()
        {
            var p = new Painter { LX = -0.5f, LY = -0.6f, LZ = 0.6f, Ambient = 0.16f, Diffuse = 0.66f };

            p.Backdrop(
                (x, y) => StoneWall(x, y, false),
                (x, y) => StoneWall(x, y, true),
                (x, y) => 175 + (int)(Math.Abs(x - 160) * 0.45f) + (int)((Noise(x / 6f, y / 6f, 1) - 0.5f) * 30f));

            const float pipeX = 160f;
            OrderFn rise = (x, y) => (int)((186 - y) * 1.15f) + (int)((Noise(x / 6f, y / 6f, 12) - 0.5f) * 20f);

            // O tubo.
            p.Cylinder(pipeX, 74, 190, 25, RustMetal(), Bronze(), rise, 1);

            // As flanges com parafusos.
            foreach (int fy in new[] { 142, 102 })
            {
                p.Cylinder(pipeX, fy, fy + 10, 31, RustMetal(), Bronze(), rise, 2);
                for (int b = -2; b <= 2; b++)
                {
                    float bx = pipeX + b * 12.5f;
                    p.Ellipse(bx, fy + 5, 2.6f, 2.6f, RustMetal(), BrightSpring(), rise, 3);
                }

                // A junta acesa (so no polido).
                for (int x = (int)pipeX - 28; x <= pipeX + 28; x++)
                {
                    float k = 1f - Math.Abs(x - pipeX) / 29f;
                    p.Overlay(x, fy - 1, null, Rgba.Lerp(Teal, PurifyCore, k * 0.7f), rise);
                    p.Overlay(x, fy + 11, null, Rgba.Lerp(Teal, PurifyCore, k * 0.35f), rise);
                }
            }

            // O bocal alargado no alto, com a boca escura.
            p.Flare(pipeX, 58, 76, 33, 25, RustMetal(), Bronze(), rise, 4);
            p.Ellipse(pipeX, 58, 30, 5.5f, Shade(CaveRock), Shade(CaveRock), rise, 5);

            // Amassado e rachadura com escorrido de ferrugem, so na sucata.
            p.Ellipse(150, 124, 8, 6, RustMetal(), null, rise, 6);
            Crack(p, 172, 114, new Random(4), 28, rise, -10, 1);
            for (int y = 118; y < 176; y++)
            {
                int x = 171 + (int)(Math.Sin(y * 0.3f) * 1.2f);
                p.Overlay(x, y, Rgba.Lerp(RustOrange, DeepRust, (y - 118) / 58f), null, rise);
            }

            // Musgo e cristais em volta: secos e cinza antes, verdes e acesos depois.
            var rng = new Random(17);
            for (int i = 0; i < 10; i++)
            {
                float mx = i % 2 == 0 ? rng.Next(20, 110) : rng.Next(210, 302);
                float my = rng.Next(36, 160);
                OrderFn around = (x, y) => 150 + (int)(Math.Abs(x - pipeX) * 0.55f);
                Moss(p, mx, my, rng, around, 40 + i);
            }

            p.Finish();
            p.Rim(new Rgba(230, 250, 240), 1, -0.7f, -0.7f, 0.5f, point: false);
            ShineVertical(p, pipeX - 12, 60, 190, rise);

            return new Shot
            {
                Name = "vent",
                Before = p.Before,
                After = p.After,
                Order = p.Order,
                Emitters = new[]
                {
                    new Emitter(EmitterKind.Flakes, 130, 60, 60, 110, 0.05f),
                    new Emitter(EmitterKind.Steam, 134, 46, 52, 10, 0.75f),
                },
                Duration = 3.0f,
                PanY = -10f,
                ZoomFrom = 1.04f,
                ZoomTo = 1.0f,
            };
        }

        // ------------------------------------------------------------------
        // Quadro 5: a pose final
        // ------------------------------------------------------------------

        /// <summary>
        /// O Scrap polido de frente, num rochedo, com o cinzel cravado no chao e
        /// a mao no cabo, capa ao vento e uma explosao de raios atras. Comeca
        /// como silhueta: os olhos acendem primeiro, e a luz se espalha deles
        /// pro corpo e pro fundo.
        /// </summary>
        public static Shot Hero()
        {
            const int loopFrames = 4;
            var frames = new Painter[loopFrames];

            for (int f = 0; f < loopFrames; f++)
            {
                frames[f] = HeroFrame(f / (float)loopFrames);
            }

            Painter first = frames[0];

            var loop = new PixelCanvas[loopFrames];
            for (int f = 0; f < loopFrames; f++) loop[f] = frames[f].After;

            return new Shot
            {
                Name = "hero",
                Before = first.Before,
                After = first.After,
                Order = first.Order,
                Loop = loop,
                Emitters = new[]
                {
                    new Emitter(EmitterKind.Sparks, 50, 14, 230, 150, 0.4f),
                },
                Duration = 3.5f,
                ZoomFrom = 1.14f,
                ZoomTo = 1.0f,
            };
        }

        private static Painter HeroFrame(float phase)
        {
            var p = new Painter { LX = -0.55f, LY = -0.65f, LZ = 0.52f, Ambient = 0.2f, Diffuse = 0.66f };

            const float cx = 160f, headY = 62f, eyeY = 66f;
            float spin = phase * (float)(Math.PI * 2 / 22 * 2 / 4);

            p.Backdrop(
                (x, y) => Burst(x, y, cx, 56f, 0f, false),
                (x, y) => Burst(x, y, cx, 56f, spin, true),
                (x, y) => 196 + (int)(Dist(x, y, cx, 56f) * 0.28f));

            OrderFn fromEyes = Radial(cx, eyeY, 1.15f, 0, 16f);

            // O rochedo onde ele pisa.
            Ledge(p, fromEyes, 1);

            // A capa, atras de tudo, voando pra direita.
            HeroCape(p, phase, fromEyes, 2);

            // O cinzel cravado no chao, a esquerda (da tela), com a mao no cabo.
            p.Capsule(100, 112, 101, 70, 5, 5, Wood(), Wood(), fromEyes, 3);
            p.Capsule(100, 74, 101, 66, 5.6f, 5.6f, RustMetal(), Gold(), fromEyes, 3);
            p.Capsule(100, 120, 101, 110, 9, 9, RustMetal(), Bronze(), fromEyes, 4);
            ChiselBlade(p, 101, 122, 100, 160, 20f, fromEyes, 5);

            // Pernas e botas.
            p.Capsule(145, 138, 142, 152, 9, 8, RustMetal(), Bronze(), fromEyes, 6);
            p.Capsule(175, 138, 178, 152, 9, 8, RustMetal(), Bronze(), fromEyes, 7);
            p.Ellipse(140, 156, 13, 6, Leather(), Leather(), fromEyes, 8);
            p.Ellipse(180, 156, 13, 6, Leather(), Leather(), fromEyes, 9);

            // O corpo de barril, com placa no peito, cinto e rebites.
            p.Ellipse(160, 114, 35, 29, RustMetal(), Bronze(), fromEyes, 10);
            p.Band(160, 114, 35, 29, 124, 131, Leather(), Leather(), fromEyes, 11);
            p.Ellipse(160, 127, 5, 4, Gold(), Gold(), fromEyes, 12);
            for (int i = -3; i <= 3; i++)
            {
                if (i == 0) continue;
                float rx = 160 + i * 9.5f;
                float ry = 97 + Math.Abs(i) * 1.8f;
                p.Ellipse(rx, ry, 1.7f, 1.7f, Gold(), Gold(), fromEyes, 13);
            }

            // O nucleo no peito: uma janelinha redonda com a luz azul.
            p.Ellipse(160, 110, 8, 8, Gold(), Gold(), fromEyes, 14);
            p.Ellipse(160, 110, 5.5f, 5.5f, Shade(GemTones), Shade(new[] { new Rgba(60, 150, 230), new Rgba(110, 200, 255), new Rgba(190, 240, 255), new Rgba(250, 255, 255) }), fromEyes, 15);

            // O braco esquerdo (da tela) vai ate o cabo do cinzel; o direito, na cintura.
            p.Capsule(130, 100, 108, 92, 7.5f, 7f, RustMetal(), Bronze(), fromEyes, 16);
            p.Ellipse(102, 90, 10, 10, Leather(), Leather(), fromEyes, 17);
            p.Capsule(190, 100, 204, 118, 7.5f, 7f, RustMetal(), Bronze(), fromEyes, 18);
            p.Capsule(204, 118, 194, 128, 7f, 6.5f, RustMetal(), Bronze(), fromEyes, 19);
            p.Ellipse(191, 129, 9.5f, 9.5f, Leather(), Leather(), fromEyes, 20);

            // As ombreiras.
            p.Ellipse(129, 97, 13, 10, RustMetal(), Bronze(), fromEyes, 21);
            p.Ellipse(191, 97, 13, 10, RustMetal(), Bronze(), fromEyes, 22);

            // A gola da capa e o broche.
            p.Ellipse(160, 90, 25, 7, Leather(), Leather(), fromEyes, 23);
            p.Ellipse(160, 92, 5, 5, Gold(), Gold(), fromEyes, 24);
            p.Ellipse(160, 92, 2.6f, 2.6f, null, Shade(new[] { new Rgba(40, 90, 170), new Rgba(70, 150, 230), new Rgba(130, 206, 255), new Rgba(230, 250, 255) }), fromEyes, 25);

            // A cabeca de cristal lapidado.
            Gem(p, cx, headY, 40, 33, -3f, 3f, fromEyes, 26, seed: 11);

            // As orbitas.
            foreach (float ex in new[] { 146f, 174f })
            {
                p.Ellipse(ex, eyeY, 9.5f, 8.5f, Shade(new[] { Ink, Ink, Ink, Ink }), Shade(new[] { Ink, new Rgba(30, 40, 66), new Rgba(30, 40, 66), new Rgba(30, 40, 66) }), fromEyes, 27);
            }

            // A mola.
            Spring(p, cx + 1, 30, 7f, 3, fromEyes, 30);

            // A silhueta do "antes" vem do proprio desenho.
            p.SilhouetteBefore(new Rgba(22, 24, 36));
            p.Finish();
            p.Rim(new Rgba(190, 244, 255), 2, cx, 56f, 0.85f);

            // Os olhos acesos e o halo, por cima do contorno.
            Eyes(p, 146, 174, eyeY, fromEyes);

            return p;
        }

        // ------------------------------------------------------------------
        // Pecas do Scrap
        // ------------------------------------------------------------------

        /// <summary>
        /// A cabeca: uma gema lapidada (mesa no meio, facetas em volta). Na
        /// sucata e pedra bruta com rachas; no polido, cristal com quinas claras.
        /// (tableX, tableY) desloca a mesa: pra onde a cabeca "olha".
        /// </summary>
        private static void Gem(Painter p, float cx, float cy, float rx, float ry, float tableX, float tableY,
            OrderFn order, int group, int seed)
        {
            var rng = new Random(seed);
            const int sides = 9;

            var outer = new (float x, float y)[sides];
            var inner = new (float x, float y)[sides];
            for (int i = 0; i < sides; i++)
            {
                double a = -Math.PI / 2 + i * Math.PI * 2 / sides + (rng.NextDouble() - 0.5) * 0.25;
                float k = 0.9f + (float)rng.NextDouble() * 0.16f;
                outer[i] = (cx + (float)Math.Cos(a) * rx * k, cy + (float)Math.Sin(a) * ry * k);
                float ki = 0.5f + (float)rng.NextDouble() * 0.08f;
                inner[i] = (cx + tableX + (float)Math.Cos(a) * rx * ki, cy + tableY + (float)Math.Sin(a) * ry * ki);
            }

            float rise = (rx + ry) * 0.35f;

            // As facetas: a mesa e um quadrilatero entre cada par de lados.
            var facets = new List<((float x, float y)[] pts, float nx, float ny, float nz)>();
            facets.Add((inner, 0.05f, -0.1f, 1f));
            for (int i = 0; i < sides; i++)
            {
                int j = (i + 1) % sides;
                var pts = new[] { outer[i], outer[j], inner[j], inner[i] };

                // Normal pelo produto vetorial (borda em z=0, mesa em z=rise).
                float ax = outer[j].x - outer[i].x, ay = outer[j].y - outer[i].y, az = 0f;
                float bx = inner[i].x - outer[i].x, by = inner[i].y - outer[i].y, bz = rise;
                float nx = ay * bz - az * by, ny = az * bx - ax * bz, nz = ax * by - ay * bx;
                if (nz < 0) { nx = -nx; ny = -ny; nz = -nz; }
                Normalize(ref nx, ref ny, ref nz);
                facets.Add((pts, nx, ny, nz));
            }

            int FacetAt(float x, float y)
            {
                for (int f = 0; f < facets.Count; f++)
                {
                    if (InsidePoly(facets[f].pts, x, y)) return f;
                }

                return -1;
            }

            Probe probe = (int x, int y, out float nx, out float ny, out float nz) =>
            {
                nx = ny = 0; nz = 1;
                int f = FacetAt(x + 0.5f, y + 0.5f);
                if (f < 0) return false;
                nx = facets[f].nx; ny = facets[f].ny; nz = facets[f].nz;
                return true;
            };

            int x0 = (int)(cx - rx * 1.1f) - 1, x1 = (int)(cx + rx * 1.1f) + 1;
            int y0 = (int)(cy - ry * 1.1f) - 1, y1 = (int)(cy + ry * 1.1f) + 1;
            p.Shape(probe, x0, y0, x1, y1, Rock(), Quartz(), order, group);

            // As quinas: claras do lado da luz no cristal, rachas escuras na pedra.
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    int f = FacetAt(x + 0.5f, y + 0.5f);
                    if (f < 0) continue;
                    int right = FacetAt(x + 1.5f, y + 0.5f), down = FacetAt(x + 0.5f, y + 1.5f);
                    if ((right == f || right < 0) && (down == f || down < 0)) continue;

                    bool lit = (x - cx) * 0.6f + (y - cy) < 4f;
                    Rgba after = lit ? new Rgba(250, 254, 255) : GemTones[0];
                    p.Overlay(x, y, S.Stone[0], after, order);
                }
            }

            // Rachas extras e lascas na pedra.
            var crack = new Random(seed + 1);
            for (int i = 0; i < 4; i++)
            {
                float sx = cx + crack.Next(-(int)(rx * 0.6f), (int)(rx * 0.6f));
                float sy = cy + crack.Next(-(int)(ry * 0.6f), (int)(ry * 0.6f));
                double a = crack.NextDouble() * Math.PI * 2;
                for (int k = 0; k < 10; k++)
                {
                    a += (crack.NextDouble() - 0.5) * 0.8;
                    sx += (float)Math.Cos(a); sy += (float)Math.Sin(a);
                    if (FacetAt(sx, sy) < 0) break;
                    p.Overlay((int)sx, (int)sy, S.Stone[0].Scaled(0.8f), null, order);
                }
            }
        }

        private static void Eyes(Painter p, float x0, float x1, float y, OrderFn order)
        {
            foreach (float ex in new[] { x0, x1 })
            {
                const float r = 7f;

                for (int py = (int)(y - r - 5); py <= y + r + 5; py++)
                {
                    for (int px = (int)(ex - r - 5); px <= ex + r + 5; px++)
                    {
                        float d = Dist(px + 0.5f, py + 0.5f, ex, y);
                        Rgba color;

                        if (d <= r * 0.45f) color = P.EyeCore;
                        else if (d <= r * 0.8f) color = Rgba.Lerp(P.EyeCore, P.EyeGlow, (d - r * 0.45f) / (r * 0.35f));
                        else if (d <= r) color = P.EyeGlow.Scaled(0.8f);
                        else continue;

                        p.Overlay(px, py, null, color, order);
                    }
                }

                // O brilhinho do olho.
                p.Overlay((int)(ex - 3), (int)(y - 3), null, new Rgba(255, 255, 255), order);
                p.Overlay((int)(ex - 2), (int)(y - 3), null, new Rgba(255, 255, 255), order);
                p.Overlay((int)(ex - 3), (int)(y - 2), null, new Rgba(255, 255, 255), order);
            }

            // O halo azul em volta dos olhos, pontilhado, so por cima da cabeca.
            for (int py = (int)(y - 14); py <= y + 14; py++)
            {
                for (int px = (int)(x0 - 14); px <= x1 + 14; px++)
                {
                    float d = Math.Min(Dist(px, py, x0, y), Dist(px, py, x1, y));
                    if (d <= 7.5f || d > 12f) continue;
                    if (Bayer(px, py) > 0.55f - (d - 7.5f) * 0.12f) continue;
                    Rgba under = p.After.Get(px, py);
                    p.Overlay(px, py, null, Rgba.Lerp(under, P.EyeGlow, 0.45f), order);
                }
            }
        }

        /// <summary>A mola de cima da cabeca: aneis empilhados e um botao no alto.</summary>
        private static void Spring(Painter p, float cx, float baseY, float radius, int coils, OrderFn order, int group)
        {
            for (int i = 0; i < coils; i++)
            {
                float y = baseY - i * radius * 0.7f;
                float x = cx + (float)Math.Sin(i * 1.3f) * 0.8f;
                p.Ellipse(x, y, radius, radius * 0.45f, RustSpring(), BrightSpring(), order, group + i);
            }

            float top = baseY - coils * radius * 0.7f - 1;
            p.Ellipse(cx, top, radius * 0.7f, radius * 0.62f, RustSpring(), BrightSpring(), order, group + coils);
        }

        /// <summary>
        /// A lamina larga do cinzel, de (ax, ay) ate (bx, by): duas faces com
        /// uma quina no meio e o chanfro na ponta.
        /// </summary>
        private static void ChiselBlade(Painter p, float ax, float ay, float bx, float by, float width, OrderFn order, int group)
        {
            float dx = bx - ax, dy = by - ay;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            float ux = dx / len, uy = dy / len;
            float px = -uy, py = ux;
            float half = width / 2f;

            (float, float) At(float along, float side) => (ax + ux * along + px * side, ay + uy * along + py * side);

            float bevel = len - Math.Max(6f, width * 1.3f);

            // A face "de cima" (normal pro lado -perp) e a "de baixo".
            float tlx = -px * 0.45f, tly = -py * 0.45f;
            p.Poly(new[] { At(0, 0), At(bevel, 0), At(bevel, -half + 1), At(0, -half) },
                Flat(tlx, tly, 0.89f), RustBlade(), Silver(), order, group);
            p.Poly(new[] { At(0, 0), At(bevel, 0), At(bevel, half - 1), At(0, half) },
                Flat(-tlx * 0.6f, -tly * 0.6f, 0.95f), RustBlade(), Silver(), order, group);

            // O chanfro, inclinado pra frente.
            p.Poly(new[] { At(bevel, -half + 1), At(len, -half + 2), At(len, half - 2), At(bevel, half - 1) },
                Flat(ux * 0.6f - 0.2f, uy * 0.6f - 0.3f, 0.7f), RustBlade(), Silver(), order, group);

            // O fio: uma linha clara no polido.
            for (float s = -half + 2; s <= half - 2; s += 0.5f)
            {
                var (x, y) = At(len - 0.5f, s);
                p.Overlay((int)x, (int)y, null, new Rgba(255, 255, 255), order);
            }

            // Dois reflexos em faixa atravessando a lamina polida.
            foreach (float at in new[] { 0.35f, 0.45f })
            {
                for (float s = -half; s <= half; s += 0.5f)
                {
                    for (float w = 0; w < (at < 0.4f ? 3f : 1.5f); w += 0.5f)
                    {
                        var (x, y) = At(bevel * at + w + s * 0.5f, s);
                        if (!p.IsAfterGroup((int)x, (int)y, group)) continue;
                        p.Overlay((int)x, (int)y, null, Rgba.Lerp(SilverTones[3], new Rgba(255, 255, 255), 0.6f), order);
                    }
                }
            }
        }

        /// <summary>
        /// A capa de costas: sai dos ombros arredondados e abre ate a barra.
        /// Na sucata a barra e rasgada (com buracos); no polido e inteira e tem
        /// barra dourada.
        /// </summary>
        private static void BackCape(Painter p, float cx, float top, float bottom, OrderFn order, int group)
        {
            float Half(float y)
            {
                float t = (y - top) / (bottom - top);
                float shoulder = (float)Math.Sqrt(Math.Min(1f, Math.Max(0f, (y - top) / 10f)));
                return (14f + 22f * shoulder) + t * 14f;
            }

            float HemRagged(float x) => bottom + (((int)(x * 7) % 13) < 6 ? -3 : 4) + (float)Math.Sin(x * 0.7f) * 2f;
            float HemClean(float x) => bottom + 2 + (float)Math.Sin(x * 0.2f) * 2f;

            Probe Make(Func<float, float> hem) => (int x, int y, out float nx, out float ny, out float nz) =>
            {
                float fold = (float)Math.Sin((x - cx) * 0.24f + (y - top) * 0.015f);
                float edge = (x - cx) / Math.Max(1f, Half(y));
                nx = fold * 0.45f + edge * 0.5f;
                ny = -0.2f + Math.Max(0f, 1f - (y - top) / 12f) * -0.6f;
                nz = (float)Math.Sqrt(Math.Max(0.05f, 1 - nx * nx - ny * ny));
                if (y < top) return false;
                if (y > hem(x)) return false;
                return Math.Abs(x - cx) <= Half(y);
            };

            int x0 = (int)(cx - 54), x1 = (int)(cx + 54);
            int y0 = (int)top - 2, y1 = (int)bottom + 8;

            p.Shape(Make(HemRagged), x0, y0, x1, y1, RawCloth(), null, order, group);
            p.Shape(Make(HemClean), x0, y0, x1, y1, null, Leather(), order, group);

            // Buracos na sucata.
            var rng = new Random(3);
            for (int i = 0; i < 6; i++)
            {
                float hx = cx + rng.Next(-34, 35), hy = bottom - rng.Next(5, 30);
                int r = 1 + rng.Next(0, 2);
                for (int y = (int)hy - r; y <= hy + r; y++)
                {
                    for (int x = (int)hx - r - 1; x <= hx + r + 1; x++)
                    {
                        if (Dist(x, y, hx, hy) > r + 0.6f) continue;
                        if (!Make(HemRagged)(x, y, out _, out _, out _)) continue;
                        p.CarveBefore(x, y, 0, order);
                    }
                }
            }

            // A barra dourada no polido.
            for (int x = x0; x <= x1; x++)
            {
                float hem = HemClean(x);
                for (int k = 0; k < 3; k++)
                {
                    int y = (int)hem - k;
                    if (!Make(HemClean)(x, y, out _, out _, out _)) continue;
                    p.Overlay(x, y, null, k == 1 ? P.Trim : P.Trim.Scaled(0.75f), order);
                }
            }
        }

        /// <summary>A capa da pose final: sai dos ombros e voa pra direita, ondulando com a fase.</summary>
        private static void HeroCape(Painter p, float phase, OrderFn order, int group)
        {
            double w = phase * Math.PI * 2;

            float Top(float x) => 88 + Math.Max(0, x - 192) * 0.16f + (float)Math.Sin(x * 0.07f - w) * 2.5f * Math.Max(0, (x - 180) / 100f);

            float Bottom(float x)
            {
                float t = Math.Max(0f, (x - 150) / 140f);
                float baseLine = 158 - t * t * 62;
                float wave = (float)Math.Sin(x * 0.09f - w) * 4f * t;
                float tatter = ((int)(x / 7) % 2 == 0) ? 0 : -3 * t;
                return baseLine + wave + tatter;
            }

            Probe probe = (int x, int y, out float nx, out float ny, out float nz) =>
            {
                nx = (float)Math.Sin(x * 0.15f - w) * 0.55f;
                ny = 0.15f;
                nz = (float)Math.Sqrt(1 - nx * nx - ny * ny);
                if (x < 118) return false;
                float top = Top(x), bottom = Bottom(x);
                return bottom - top > 1 && y >= top && y <= bottom;
            };

            p.Shape(probe, 114, 70, 316, 170, null, Leather(), order, group);

            // O forro vermelho escuro aparecendo nas dobras.
            var lining = new[] { new Rgba(52, 18, 24), new Rgba(84, 28, 34), new Rgba(120, 40, 44), new Rgba(152, 60, 56) };
            for (int x = 190; x <= 316; x++)
            {
                float fold = (float)Math.Sin(x * 0.15f - w);
                if (fold < 0.45f) continue;
                int depth = 3 + (int)((fold - 0.45f) * 14);
                for (int y = (int)Bottom(x) - depth; y <= Bottom(x); y++)
                {
                    if (!probe(x, y, out _, out _, out _)) continue;
                    p.Overlay(x, y, null, lining[Math.Min(3, (int)((fold - 0.45f) * 7f))], order);
                }
            }

            // Barra dourada embaixo.
            for (int x = 118; x <= 316; x++)
            {
                int y = (int)Bottom(x);
                for (int k = 0; k < 2; k++)
                {
                    if (!probe(x, y - k, out _, out _, out _)) continue;
                    p.Overlay(x, y - k, null, k == 0 ? P.Trim.Scaled(0.75f) : P.Trim, order);
                }
            }
        }

        /// <summary>O rochedo da pose final: pedra escura com rachas que acendem.</summary>
        private static void Ledge(Painter p, OrderFn order, int group)
        {
            float Top(float x) => 156 + (float)Math.Abs(x - 160) * 0.05f + Noise(x / 9f, 0, 41) * 5f;

            Probe probe = (int x, int y, out float nx, out float ny, out float nz) =>
            {
                float top = Top(x);
                nx = (Noise(x / 6f, y / 6f, 42) - 0.5f) * 0.8f;
                ny = y < top + 3 ? -0.8f : 0.1f;
                nz = (float)Math.Sqrt(Math.Max(0.05f, 1 - nx * nx - ny * ny));
                return x > 56 && x < 268 && y >= top;
            };

            p.Shape(probe, 50, 150, 270, 180, Shade(CaveRock), Shade(CaveRock), order, group);

            // Rachas com luz azul.
            var rng = new Random(9);
            for (int i = 0; i < 7; i++)
            {
                float x = rng.Next(70, 250), y = 162 + rng.Next(0, 6);
                double a = Math.PI * (0.2 + rng.NextDouble() * 0.6);
                for (int k = 0; k < 14; k++)
                {
                    a += (rng.NextDouble() - 0.5) * 0.7;
                    x += (float)Math.Cos(a) * (k % 2 == 0 ? 1 : 0.5f);
                    y += (float)Math.Sin(a) * 0.6f;
                    if (!probe((int)x, (int)y, out _, out _, out _)) break;
                    p.Overlay((int)x, (int)y, CaveRock[0], Rgba.Lerp(Teal, PurifyCore, 0.4f), order);
                }
            }
        }

        /// <summary>O sulco entre duas placas do braco, com rebites no polido.</summary>
        private static void Groove(Painter p, float cx, float cy, float dx, float dy, float radius, OrderFn order, int group)
        {
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            float ux = dx / len, uy = dy / len;
            float px = -uy, py = ux;

            for (float s = -radius; s <= radius; s += 0.5f)
            {
                for (int w = -1; w <= 1; w++)
                {
                    int x = (int)Math.Round(cx + px * s + ux * w);
                    int y = (int)Math.Round(cy + py * s + uy * w);
                    if (!p.IsAfterGroup(x, y, 2)) continue;
                    p.Overlay(x, y, DeepRust.Scaled(0.7f), w == 1 ? BronzeTones[3] : BronzeTones[0], order);
                }
            }

            for (int i = -2; i <= 2; i++)
            {
                float s = i * radius * 0.38f;
                float rx = cx + px * s + ux * 6, ry = cy + py * s + uy * 6;
                p.Ellipse(rx, ry, 2.4f, 2.4f, RustMetal(), Gold(), order, group);
            }
        }

        /// <summary>Um reflexo comprido ao longo do braco polido, do lado da luz.</summary>
        private static void Shine(Painter p, float ax, float ay, float bx, float by, float offset, OrderFn order)
        {
            float dx = bx - ax, dy = by - ay;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            float ux = dx / len, uy = dy / len;
            float px = uy, py = -ux;

            for (float s = 20; s < len - 10; s += 0.5f)
            {
                for (float w = 0; w < 2.5f; w += 0.5f)
                {
                    int x = (int)(ax + ux * s + px * (offset + w)), y = (int)(ay + uy * s + py * (offset + w));
                    if (!p.IsAfterGroup(x, y, 2)) continue;
                    p.Overlay(x, y, null, Rgba.Lerp(BronzeTones[3], new Rgba(255, 250, 230), w < 1 ? 0.7f : 0.3f), order);
                }
            }
        }

        /// <summary>Um reflexo vertical no tubo polido.</summary>
        private static void ShineVertical(Painter p, float x, float y0, float y1, OrderFn order)
        {
            for (int y = (int)y0; y <= y1; y++)
            {
                for (int k = 0; k < 3; k++)
                {
                    int px = (int)x + k;
                    if (!p.IsAfterGroup(px, y, 1) && !p.IsAfterGroup(px, y, 4)) continue;
                    p.Overlay(px, y, null, Rgba.Lerp(BronzeTones[3], new Rgba(255, 250, 230), k == 1 ? 0.7f : 0.25f), order);
                }
            }
        }

        /// <summary>
        /// Uma rachadura que anda e se ramifica. No antes e uma linha escura; a
        /// ordem dela vem antes da placa em volta (bias negativo), entao a luz
        /// vaza por ela primeiro.
        /// </summary>
        private static void Crack(Painter p, float x, float y, Random rng, int steps, OrderFn order, int bias, int depth)
        {
            double angle = rng.NextDouble() * Math.PI * 2;
            for (int i = 0; i < steps; i++)
            {
                angle += (rng.NextDouble() - 0.5) * 0.9;
                x += (float)Math.Cos(angle);
                y += (float)Math.Sin(angle);

                int px = (int)Math.Round(x), py = (int)Math.Round(y);
                int o = order(px, py) + bias + i;
                p.OverlayOrder(px, py, DeepRust.Scaled(0.55f), null, o);

                if (depth > 0 && rng.NextDouble() < 0.08)
                {
                    int start = o;
                    Crack(p, x, y, rng, steps / 2, (cx, cy) => start, 0, depth - 1);
                }
            }
        }

        /// <summary>Um tufo de musgo com cristais: seco e cinza antes, verde e aceso depois.</summary>
        private static void Moss(Painter p, float cx, float cy, Random rng, OrderFn order, int group)
        {
            var dead = new[] { new Rgba(34, 34, 32), new Rgba(54, 52, 44), new Rgba(74, 70, 58), new Rgba(94, 88, 70) };
            var green = new[] { new Rgba(22, 56, 42), new Rgba(36, 90, 58), new Rgba(64, 132, 72), new Rgba(120, 188, 104) };

            int blobs = 4 + rng.Next(0, 4);
            for (int i = 0; i < blobs; i++)
            {
                float bx = cx + rng.Next(-12, 13), by = cy + rng.Next(-4, 5);
                p.Ellipse(bx, by, 4 + rng.Next(0, 5), 2 + rng.Next(0, 3), new Mat(dead, Surface.Moss, dead[3], 0f),
                    new Mat(green, Surface.Moss, new Rgba(180, 236, 140), 0f), order, group);
            }

            // Os cristais so existem depois.
            var crystal = new Mat(new[] { new Rgba(30, 96, 130), new Rgba(56, 160, 196), new Rgba(120, 220, 240), new Rgba(220, 255, 255) }, Surface.Crystal, PurifyCore, 0.5f);
            int count = 1 + rng.Next(0, 3);
            for (int i = 0; i < count; i++)
            {
                float bx = cx + rng.Next(-10, 11), by = cy - 1;
                float h = 6 + rng.Next(0, 8);
                float lean = (rng.Next(0, 2) == 0 ? -1 : 1) * rng.Next(0, 4);
                p.Poly(new[] { (bx - 3, by), (bx, by + 1), (bx + lean * 0.5f, by - h - 2), (bx - 1 + lean, by - h) },
                    Flat(-0.7f, -0.3f, 0.64f), null, crystal, order, group + 100);
                p.Poly(new[] { (bx, by + 1), (bx + 3, by), (bx + 1 + lean, by - h), (bx + lean * 0.5f, by - h - 2) },
                    Flat(0.5f, -0.2f, 0.84f), null, crystal, order, group + 100);
            }
        }

        // ------------------------------------------------------------------
        // Fundos
        // ------------------------------------------------------------------

        /// <summary>
        /// A boca de uma galeria: paredes escuras dos lados, chao, e la no
        /// fundo a luz do coracao com raios largos.
        /// </summary>
        private static Rgba Gallery(int x, int y, float gx, float gy, float strength)
        {
            float n = Noise(x / 40f, y / 30f, 2) * 0.6f + Noise(x / 9f, y / 9f, 3) * 0.4f;
            int level = Clamp((int)(0.6f + n * 1.8f + Bayer(x, y) * 0.5f), 0, 3);
            Rgba rock = CaveRock[level];

            float d = Dist(x, y, gx, gy);
            float g = Math.Max(0f, 1f - d / (90f + 70f * strength));
            g = g * g * (0.6f + 0.5f * strength);

            double angle = Math.Atan2(y - gy, x - gx);
            float ray = (float)Math.Pow(Math.Max(0, Math.Cos(angle * 6 + 0.4)), 3);
            float rays = ray * Math.Max(0f, 1f - d / (200f * strength + 40f)) * 0.35f * strength;

            float light = Math.Min(1f, g + rays);
            Rgba glow = Rgba.Lerp(Purify, PurifyCore, Math.Min(1f, g * 1.8f));
            float k = (float)Math.Floor(light * 7f + Bayer(x, y) * 0.95f) / 7f;
            Rgba color = Rgba.Lerp(rock, glow, k);

            // As paredes da galeria nos lados, escuras e recortadas.
            float wallL = 40 + Noise(0, y / 12f, 5) * 26 - Math.Max(0, y - 120) * 0.25f;
            float wallR = 280 - Noise(1, y / 12f, 6) * 26 + Math.Max(0, y - 120) * 0.25f;
            if (x < wallL || x > wallR) color = Rgba.Lerp(CaveRock[0], color, 0.18f);

            // O chao, com o reflexo da luz vindo ate o Scrap.
            float floor = 152 + Noise(x / 12f, 0, 7) * 4;
            if (y > floor)
            {
                float lane = Math.Max(0f, 1f - Math.Abs(x - gx) / (34f + (y - floor) * 1.2f));
                Rgba ground = CaveRock[Clamp((int)(Noise(x / 6f, y / 3f, 8) * 2f + Bayer(x, y) * 0.5f), 0, 1)];
                float lk = (float)Math.Floor(lane * strength * 4f + Bayer(x, y) * 0.9f) / 4f;
                color = Rgba.Lerp(ground, Purify, lk * 0.45f);
            }

            return Vignette(color, x, y, 0.55f);
        }

        /// <summary>Fundo fora de foco dos closes: escuro, com bolhas grandes de luz.</summary>
        private static Rgba Bokeh(int x, int y, float strength, int seed)
        {
            float n = Noise(x / 60f, y / 50f, seed) * 0.8f;
            Rgba color = Rgba.Lerp(CaveRock[0], CaveRock[2], n);

            var rng = new Random(seed * 31);
            for (int i = 0; i < 14; i++)
            {
                float bx = rng.Next(0, Width), by = rng.Next(0, Height);
                float r = 8 + rng.Next(0, 18);
                float d = Dist(x, y, bx, by);
                if (d > r) continue;

                bool warm = rng.Next(0, 3) == 0;
                Rgba tint = warm ? new Rgba(150, 120, 80) : new Rgba(80, 150, 160);
                float edge = d > r - 1.5f ? 1.4f : 1f;
                color = Rgba.Lerp(color, tint, 0.12f * strength * edge + 0.05f * strength);
            }

            // Uma luz larga vindo do canto de cima.
            float glow = Math.Max(0f, 1f - Dist(x, y, 300, 0) / 260f);
            float k = (float)Math.Floor(glow * glow * strength * 5f + Bayer(x, y) * 0.9f) / 5f;
            color = Rgba.Lerp(color, Purify, k * 0.35f);
            return Vignette(color, x, y, 0.5f);
        }

        /// <summary>A parede de pedra do respiro: blocos (Voronoi) e, no polido, uma luz verde-agua.</summary>
        private static Rgba StoneWall(int x, int y, bool lit)
        {
            float cell = CellEdge(x / 30f, y / 24f, 7);
            float n = Noise(x / 7f, y / 7f, 8);

            int level = Clamp((int)(1.1f + n * 1.4f + Bayer(x, y) * 0.4f), 0, 3);
            if (cell < 0.06f) level = 0;
            else if (cell < 0.11f) level = Math.Max(0, level - 1);
            else if (cell < 0.15f) level = Math.Min(3, level + 1);

            Rgba rock = CaveRock[level];
            if (y > 170) rock = CaveRock[0];

            float d = Dist(x, y, 160, 60);
            float g = Math.Max(0f, 1f - d / (lit ? 210f : 130f));
            g = (float)Math.Floor(g * g * 5f + Bayer(x, y) * 0.9f) / 5f;
            Rgba glow = lit ? new Rgba(110, 210, 200) : new Rgba(110, 100, 84);
            return Vignette(Rgba.Lerp(rock, glow, g * (lit ? 0.5f : 0.22f)), x, y, 0.5f);
        }

        /// <summary>O fundo da pose final: escuro antes; depois, uma explosao de raios.</summary>
        private static Rgba Burst(int x, int y, float cx, float cy, float spin, bool lit)
        {
            if (!lit)
            {
                float n = Noise(x / 30f, y / 24f, 3);
                return Vignette(CaveRock[Clamp((int)(n * 2.2f + Bayer(x, y) * 0.5f), 0, 2)], x, y, 0.5f);
            }

            float d = Dist(x, y, cx, cy);
            double angle = Math.Atan2(y - cy, x - cx) + spin;
            bool stripe = ((int)Math.Floor((angle + Math.PI) / (Math.PI * 2) * 22)) % 2 == 0;

            var deep = new Rgba(14, 30, 58);
            var mid = new Rgba(34, 86, 128);
            var light = new Rgba(110, 200, 230);

            float k = Math.Max(0f, 1f - d / 240f);
            k = (float)Math.Floor(k * 6f + Bayer(x, y) * 0.9f) / 6f;
            Rgba color = Rgba.Lerp(deep, stripe ? light : mid, k);

            // O miolo branco atras da cabeca.
            if (d < 80f)
            {
                float c = (float)Math.Floor((1f - d / 80f) * 5f + Bayer(x, y) * 0.9f) / 5f;
                color = Rgba.Lerp(color, PurifyCore, c * 0.9f);
            }

            // Cristais flutuando: losangos pequenos.
            var rng = new Random(91);
            for (int i = 0; i < 18; i++)
            {
                float sx = rng.Next(10, 310), sy = rng.Next(16, 150);
                float s = 2 + rng.Next(0, 3);
                float m = Math.Abs(x - sx) / s + Math.Abs(y - sy) / (s * 1.8f);
                if (m <= 1f) color = m < 0.45f ? PurifyCore : new Rgba(130, 220, 240);
            }

            return Vignette(color, x, y, 0.45f);
        }

        private static Rgba Vignette(Rgba color, int x, int y, float amount)
        {
            float dx = (x - Width / 2f) / (Width / 2f), dy = (y - Height / 2f) / (Height / 2f);
            float v = Math.Max(0f, (dx * dx + dy * dy) * 0.5f - 0.1f);
            float k = (float)Math.Floor(Math.Min(1f, v * amount * 2f) * 4f + Bayer(x, y) * 0.9f) / 4f;
            return Rgba.Lerp(color, CaveRock[0], k);
        }

        // ------------------------------------------------------------------
        // O pintor
        // ------------------------------------------------------------------

        private delegate bool Probe(int x, int y, out float nx, out float ny, out float nz);

        private delegate int OrderFn(int x, int y);

        private static Probe Flat(float nx, float ny, float nz)
        {
            Normalize(ref nx, ref ny, ref nz);
            return (int x, int y, out float a, out float b, out float c) =>
            {
                a = nx; b = ny; c = nz;
                return true;
            };
        }

        private static OrderFn Radial(float cx, float cy, float perPixel, int offset, float jitter)
        {
            return (x, y) => offset + (int)(Dist(x, y, cx, cy) * perPixel + (Noise(x / 5f, y / 5f, 31) - 0.5f) * jitter);
        }

        private sealed class Painter
        {
            public readonly PixelCanvas Before = new PixelCanvas(Width, Height);
            public readonly PixelCanvas After = new PixelCanvas(Width, Height);
            public readonly PixelCanvas Order = new PixelCanvas(Width, Height);

            public float LX, LY, LZ = 1f;
            public float Ambient = 0.2f;
            public float Diffuse = 0.65f;

            private readonly int[] _groupB = new int[Width * Height];
            private readonly int[] _groupA = new int[Width * Height];
            private readonly int[] _layerB = new int[Width * Height];
            private readonly int[] _layerA = new int[Width * Height];
            private readonly Rgba[] _backB = new Rgba[Width * Height];
            private readonly Rgba[] _backA = new Rgba[Width * Height];
            private int _draw;

            public Painter()
            {
                for (int i = 0; i < Order.Pixels.Length; i++) Order.Pixels[i] = OrderColor(Static);
            }

            public bool IsAfterGroup(int x, int y, int group)
            {
                return x >= 0 && y >= 0 && x < Width && y < Height && _groupA[x + y * Width] == group;
            }

            /// <summary>O fundo, nas duas versoes. A ordem so vale onde elas diferem.</summary>
            public void Backdrop(Func<int, int, Rgba> before, Func<int, int, Rgba> after, OrderFn order)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int i = x + y * Width;
                        Rgba b = before(x, y), a = after(x, y);
                        Before.Pixels[i] = _backB[i] = b;
                        After.Pixels[i] = _backA[i] = a;
                        if (!Same(a, b)) SetOrder(i, order(x, y));
                    }
                }
            }

            /// <summary>
            /// Pinta uma forma. Material nulo: a forma nao existe naquela versao
            /// (ex.: crosta so na sucata, cristal so no polido).
            /// </summary>
            public void Shape(Probe probe, int x0, int y0, int x1, int y1, Mat before, Mat after, OrderFn order, int group)
            {
                _draw++;
                float lx = LX, ly = LY, lz = LZ;
                Normalize(ref lx, ref ly, ref lz);

                for (int y = Math.Max(0, y0); y <= Math.Min(Height - 1, y1); y++)
                {
                    for (int x = Math.Max(0, x0); x <= Math.Min(Width - 1, x1); x++)
                    {
                        if (!probe(x, y, out float nx, out float ny, out float nz)) continue;

                        int i = x + y * Width;
                        float d = nx * lx + ny * ly + nz * lz;
                        float lum = Ambient + Diffuse * Math.Max(0f, d);

                        // Especular: reflexo da luz em direcao a camera (0, 0, 1).
                        float rz = 2f * d * nz - lz;
                        float spec = rz > 0f ? (float)Math.Pow(rz, 18) : 0f;

                        if (before != null)
                        {
                            Before.Pixels[i] = ShadeSurface(before, lum + spec * before.Shine, x, y);
                            _groupB[i] = group;
                            _layerB[i] = _draw;
                        }

                        if (after != null)
                        {
                            After.Pixels[i] = ShadeSurface(after, lum + spec * after.Shine, x, y);
                            _groupA[i] = group;
                            _layerA[i] = _draw;
                        }

                        SetOrder(i, order(x, y));
                    }
                }
            }

            public void Ellipse(float cx, float cy, float rx, float ry, Mat before, Mat after, OrderFn order, int group)
            {
                Probe probe = (int x, int y, out float nx, out float ny, out float nz) =>
                {
                    nx = (x + 0.5f - cx) / rx;
                    ny = (y + 0.5f - cy) / ry;
                    float s = nx * nx + ny * ny;
                    nz = (float)Math.Sqrt(Math.Max(0f, 1f - s));
                    return s <= 1f;
                };

                Shape(probe, (int)(cx - rx - 1), (int)(cy - ry - 1), (int)(cx + rx + 1), (int)(cy + ry + 1), before, after, order, group);
            }

            /// <summary>Um tubo de A ate B, de raio ra ate rb, com pontas redondas.</summary>
            public void Capsule(float ax, float ay, float bx, float by, float ra, float rb, Mat before, Mat after, OrderFn order, int group)
            {
                float dx = bx - ax, dy = by - ay;
                float len2 = Math.Max(0.0001f, dx * dx + dy * dy);

                Probe probe = (int x, int y, out float nx, out float ny, out float nz) =>
                {
                    float px = x + 0.5f, py = y + 0.5f;
                    float t = ((px - ax) * dx + (py - ay) * dy) / len2;
                    t = t < 0 ? 0 : (t > 1 ? 1 : t);
                    float qx = ax + dx * t, qy = ay + dy * t;
                    float r = ra + (rb - ra) * t;
                    nx = (px - qx) / r;
                    ny = (py - qy) / r;
                    float s = nx * nx + ny * ny;
                    nz = (float)Math.Sqrt(Math.Max(0f, 1f - s));
                    return s <= 1f;
                };

                float m = Math.Max(ra, rb) + 1;
                Shape(probe, (int)(Math.Min(ax, bx) - m), (int)(Math.Min(ay, by) - m), (int)(Math.Max(ax, bx) + m),
                    (int)(Math.Max(ay, by) + m), before, after, order, group);
            }

            /// <summary>Um cilindro de pe, de y0 a y1, com a tampa de cima em elipse.</summary>
            public void Cylinder(float cx, float y0, float y1, float r, Mat before, Mat after, OrderFn order, int group)
            {
                float cap = r * 0.22f;
                Probe probe = (int x, int y, out float nx, out float ny, out float nz) =>
                {
                    nx = (x + 0.5f - cx) / r;
                    ny = 0f;
                    nz = (float)Math.Sqrt(Math.Max(0f, 1f - nx * nx));
                    if (Math.Abs(nx) > 1f) return false;
                    if (y > y1) return false;
                    float top = y0 - cap * nz;
                    if (y < top) return false;
                    if (y < y0) { ny = -0.8f; nz = 0.6f; }
                    return true;
                };

                Shape(probe, (int)(cx - r - 1), (int)(y0 - cap - 1), (int)(cx + r + 1), (int)y1, before, after, order, group);
            }

            /// <summary>Um cone de boca larga em cima (o bocal do respiro).</summary>
            public void Flare(float cx, float yTop, float yBottom, float rTop, float rBottom, Mat before, Mat after, OrderFn order, int group)
            {
                Probe probe = (int x, int y, out float nx, out float ny, out float nz) =>
                {
                    float t = (y - yTop) / (yBottom - yTop);
                    float r = rTop + (rBottom - rTop) * t;
                    nx = (x + 0.5f - cx) / r;
                    ny = -0.3f;
                    nz = (float)Math.Sqrt(Math.Max(0f, 1f - nx * nx - ny * ny));
                    return t >= 0f && t <= 1f && Math.Abs(nx) <= 1f;
                };

                Shape(probe, (int)(cx - rTop - 1), (int)yTop, (int)(cx + rTop + 1), (int)yBottom, before, after, order, group);
            }

            /// <summary>Uma faixa horizontal dentro de uma elipse (o cinto).</summary>
            public void Band(float cx, float cy, float rx, float ry, int yFrom, int yTo, Mat before, Mat after, OrderFn order, int group)
            {
                Probe probe = (int x, int y, out float nx, out float ny, out float nz) =>
                {
                    nx = (x + 0.5f - cx) / rx;
                    ny = (y + 0.5f - cy) / ry;
                    float s = nx * nx + ny * ny;
                    nz = (float)Math.Sqrt(Math.Max(0f, 1f - s));
                    return s <= 1f && y >= yFrom && y <= yTo;
                };

                Shape(probe, (int)(cx - rx - 1), yFrom, (int)(cx + rx + 1), yTo, before, after, order, group);
            }

            public void Poly((float x, float y)[] points, Probe normal, Mat before, Mat after, OrderFn order, int group)
            {
                float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
                foreach (var pt in points)
                {
                    minX = Math.Min(minX, pt.x); maxX = Math.Max(maxX, pt.x);
                    minY = Math.Min(minY, pt.y); maxY = Math.Max(maxY, pt.y);
                }

                Probe probe = (int x, int y, out float nx, out float ny, out float nz) =>
                {
                    normal(x, y, out nx, out ny, out nz);
                    return InsidePoly(points, x + 0.5f, y + 0.5f);
                };

                Shape(probe, (int)minX - 1, (int)minY - 1, (int)maxX + 1, (int)maxY + 1, before, after, order, group);
            }

            /// <summary>Pinta um pixel por cima, sem mexer no contorno. Cor nula: nao mexe naquela versao.</summary>
            public void Overlay(int x, int y, Rgba? before, Rgba? after, OrderFn order)
            {
                if (x < 0 || y < 0 || x >= Width || y >= Height) return;
                OverlayOrder(x, y, before, after, order(x, y));
            }

            public void OverlayOrder(int x, int y, Rgba? before, Rgba? after, int order)
            {
                if (x < 0 || y < 0 || x >= Width || y >= Height) return;
                int i = x + y * Width;
                if (before.HasValue) Before.Pixels[i] = before.Value;
                if (after.HasValue) After.Pixels[i] = after.Value;
                SetOrder(i, order);
            }

            /// <summary>Tira um pedaco so da versao "antes" (mossa, buraco), mostrando o fundo.</summary>
            public void CarveBefore(int cx, int cy, int radius, OrderFn order)
            {
                for (int y = cy - radius; y <= cy + radius; y++)
                {
                    for (int x = cx - radius; x <= cx + radius; x++)
                    {
                        if (x < 0 || y < 0 || x >= Width || y >= Height) continue;
                        if (Dist(x, y, cx, cy) > radius + 0.5f) continue;
                        int i = x + y * Width;
                        Before.Pixels[i] = _backB[i];
                        _groupB[i] = 0;
                        _layerB[i] = 0;
                        SetOrder(i, order(x, y));
                    }
                }
            }

            /// <summary>O "antes" vira a silhueta escura do "depois".</summary>
            public void SilhouetteBefore(Rgba fill)
            {
                for (int i = 0; i < _groupA.Length; i++)
                {
                    _groupB[i] = _groupA[i] == 0 ? 0 : 1;
                    _layerB[i] = _layerA[i];
                    Before.Pixels[i] = _groupA[i] != 0 ? fill : _backB[i];
                }
            }

            /// <summary>Contorno escuro onde uma forma encosta em algo atras dela, nas duas versoes.</summary>
            public void Finish()
            {
                Outline(Before, _groupB, _layerB);
                Outline(After, _groupA, _layerA);
            }

            private static void Outline(PixelCanvas canvas, int[] group, int[] layer)
            {
                var mark = new bool[group.Length];

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int i = x + y * Width;
                        if (group[i] == 0) continue;

                        for (int k = 0; k < 4; k++)
                        {
                            int nx = x + (k == 0 ? 1 : (k == 1 ? -1 : 0));
                            int ny = y + (k == 2 ? 1 : (k == 3 ? -1 : 0));
                            if (nx < 0 || ny < 0 || nx >= Width || ny >= Height) continue;

                            int j = nx + ny * Width;
                            if (group[j] == group[i]) continue;
                            if (layer[j] >= layer[i] && group[j] != 0) continue;

                            mark[i] = true;
                            break;
                        }
                    }
                }

                for (int i = 0; i < mark.Length; i++)
                {
                    if (mark[i]) canvas.Pixels[i] = Ink;
                }
            }

            /// <summary>
            /// Luz de borda: os pixels da figura que tem fundo logo na direcao da
            /// luz clareiam. Ponto: a luz vem de (lx, ly); senao, (lx, ly) e a direcao.
            /// </summary>
            public void Rim(Rgba color, int width, float lx, float ly, float amount, bool point = true)
            {
                RimOn(Before, _groupB, color, width, lx, ly, amount * 0.6f, point);
                RimOn(After, _groupA, color, width, lx, ly, amount, point);
            }

            private static void RimOn(PixelCanvas canvas, int[] group, Rgba color, int width, float lx, float ly, float amount, bool point)
            {
                var source = (Rgba[])canvas.Pixels.Clone();

                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        int i = x + y * Width;
                        if (group[i] == 0) continue;

                        float dx = point ? lx - x : lx, dy = point ? ly - y : ly;
                        float len = (float)Math.Sqrt(dx * dx + dy * dy);
                        if (len < 0.001f) continue;
                        dx /= len; dy /= len;

                        for (int k = 1; k <= width + 1; k++)
                        {
                            int sx = (int)Math.Round(x + dx * k), sy = (int)Math.Round(y + dy * k);
                            if (sx < 0 || sy < 0 || sx >= Width || sy >= Height) break;
                            if (group[sx + sy * Width] != 0) continue;

                            float strength = amount / k;
                            canvas.Pixels[i] = Rgba.Lerp(source[i], color, Math.Min(1f, strength));
                            break;
                        }
                    }
                }
            }

            private void SetOrder(int i, int order)
            {
                Order.Pixels[i] = OrderColor((byte)Clamp(order, 0, 254));
            }

            private static Rgba OrderColor(byte value) => new Rgba(value, value, value);

            private static bool Same(Rgba a, Rgba b) => a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;
        }

        private static Rgba ShadeSurface(Mat m, float lum, int x, int y)
        {
            if (m.Shine > 0f && lum > 1.02f) return m.Accent;

            float t = lum * 4f + (Bayer(x, y) - 0.5f) * 0.7f;
            int level = Clamp((int)Math.Floor(t), 0, 3);

            switch (m.Surface)
            {
                case Surface.Rust:
                {
                    if (Noise(x / 5f, y / 5f, 3) > 0.6f) level = Math.Max(0, level - 1);
                    Rgba c = m.Tones[level];
                    if (Noise(x / 3f, y / 3f, 11) > 0.68f) c = Rgba.Lerp(c, m.Accent, 0.5f);
                    if (Noise(x / 8f, y / 8f, 13) > 0.7f) c = Rgba.Lerp(c, DeepRust, 0.45f);
                    if (Hash(x, y, 7) < 0.07f) c = c.Scaled(0.72f);
                    return c;
                }

                case Surface.Rock:
                {
                    Rgba c = m.Tones[level];
                    float h = Hash(x, y, 5);
                    if (h < 0.03f && level >= 1) return m.Accent;
                    if (h > 0.92f) return c.Scaled(0.8f);
                    return c;
                }

                case Surface.Crystal:
                {
                    Rgba c = m.Tones[level];
                    if (Hash(x, y, 9) < 0.01f && level >= 2) return m.Accent;
                    return c;
                }

                case Surface.Cloth:
                {
                    if (Noise(x / 4f, y / 4f, 15) > 0.66f) level = Math.Max(0, level - 1);
                    Rgba c = m.Tones[level];
                    if (((x + y) & 3) == 0 && Hash(x, y, 17) < 0.35f) c = c.Scaled(0.9f);
                    return c;
                }

                case Surface.Leather:
                {
                    Rgba c = m.Tones[level];
                    if (Noise(x / 2.5f, y / 2.5f, 19) > 0.72f) c = c.Scaled(0.92f);
                    return c;
                }

                case Surface.Wood:
                {
                    if (Noise((x + y) / 2f, (x - y) / 9f, 23) > 0.62f) level = Math.Max(0, level - 1);
                    return m.Tones[level];
                }

                case Surface.Moss:
                {
                    Rgba c = m.Tones[level];
                    if (Hash(x, y, 25) < 0.08f) return m.Accent;
                    return c;
                }

                default:
                    return m.Tones[level];
            }
        }

        // ------------------------------------------------------------------
        // Contas
        // ------------------------------------------------------------------

        private static float Dist(float x, float y, float cx, float cy)
        {
            float dx = x - cx, dy = y - cy;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>Onde (x, y) cai no segmento A-B, de 0 a 1.</summary>
        private static float Along(float x, float y, float ax, float ay, float bx, float by)
        {
            float dx = bx - ax, dy = by - ay;
            float t = ((x - ax) * dx + (y - ay) * dy) / Math.Max(0.0001f, dx * dx + dy * dy);
            return t < 0 ? 0 : (t > 1 ? 1 : t);
        }

        private static void Normalize(ref float x, ref float y, ref float z)
        {
            float len = (float)Math.Sqrt(x * x + y * y + z * z);
            if (len < 0.0001f) { x = 0; y = 0; z = 1; return; }
            x /= len; y /= len; z /= len;
        }

        private static bool InsidePoly((float x, float y)[] pts, float x, float y)
        {
            bool inside = false;
            for (int i = 0, j = pts.Length - 1; i < pts.Length; j = i++)
            {
                if ((pts[i].y > y) != (pts[j].y > y) &&
                    x < (pts[j].x - pts[i].x) * (y - pts[i].y) / (pts[j].y - pts[i].y) + pts[i].x)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        private static int Clamp(int v, int min, int max) => v < min ? min : (v > max ? max : v);

        private static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 374761393 + y * 668265263 + seed * 1442695041);
                h = (h ^ (h >> 13)) * 1274126177u;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / 16777216f;
            }
        }

        private static float Noise(float x, float y, int seed)
        {
            int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
            float fx = x - x0, fy = y - y0;
            fx = fx * fx * (3 - 2 * fx);
            fy = fy * fy * (3 - 2 * fy);

            float a = Hash(x0, y0, seed), b = Hash(x0 + 1, y0, seed);
            float c = Hash(x0, y0 + 1, seed), d = Hash(x0 + 1, y0 + 1, seed);
            return (a + (b - a) * fx) + ((c + (d - c) * fx) - (a + (b - a) * fx)) * fy;
        }

        /// <summary>Distancia ate a borda da celula de Voronoi mais proxima (0 = na junta).</summary>
        private static float CellEdge(float x, float y, int seed)
        {
            int cx = (int)Math.Floor(x), cy = (int)Math.Floor(y);
            float best = float.MaxValue, second = float.MaxValue;

            for (int j = -1; j <= 1; j++)
            {
                for (int i = -1; i <= 1; i++)
                {
                    float px = cx + i + Hash(cx + i, cy + j, seed);
                    float py = cy + j + Hash(cx + i, cy + j, seed + 1);
                    float d = Dist(x, y, px, py);
                    if (d < best) { second = best; best = d; }
                    else if (d < second) second = d;
                }
            }

            return second - best;
        }

        private static readonly float[] BayerMatrix =
        {
            0f, 8f, 2f, 10f,
            12f, 4f, 14f, 6f,
            3f, 11f, 1f, 9f,
            15f, 7f, 13f, 5f,
        };

        private static float Bayer(int x, int y) => BayerMatrix[(x & 3) + (y & 3) * 4] / 16f;
    }
}
