using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class PurificationCartoonArt
    {
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
    }
}
