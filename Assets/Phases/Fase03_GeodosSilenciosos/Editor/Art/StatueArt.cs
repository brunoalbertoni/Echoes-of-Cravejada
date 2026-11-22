using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A arte do Salao das Estatuas e da placa de palavras do District 3.
    ///
    /// As estatuas sao de pedra-prata (a mesma rocha lilas das gravuras,
    /// polida), cada uma uma cena da escavacao: o Mineiro, o Rei, a Broca e o
    /// Coracao. Todas em cima do mesmo pedestal, com o botao na frente. O
    /// "aceso" e uma camada a parte, so com os olhos (ou a fenda) brilhando
    /// num branco frio — a mesma cor pras quatro, pra nao entregar a ordem.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class StatueArt
    {
        public const int Width = 32;
        public const int Height = 48;
        public const int KindCount = 4;

        /// <summary>Onde o botao fica, em pixels a partir do canto de baixo a esquerda da estatua.</summary>
        public const int ButtonX = 12;
        public const int ButtonY = 4;

        private static readonly Rgba[] Stone =
            { new Rgba(44, 40, 62), new Rgba(78, 72, 102), new Rgba(118, 110, 148), new Rgba(170, 162, 200) };

        private static readonly Rgba[] Plinth =
            { new Rgba(32, 30, 46), new Rgba(54, 50, 72), new Rgba(82, 76, 104), new Rgba(116, 110, 142) };

        private static readonly Rgba[] Silver =
            { new Rgba(70, 76, 96), new Rgba(118, 126, 148), new Rgba(170, 178, 198), new Rgba(226, 232, 244) };

        private static readonly Rgba Cut = new Rgba(30, 26, 42);
        private static readonly Rgba Light = new Rgba(226, 238, 255);
        private static readonly Rgba LightCore = new Rgba(255, 255, 255);

        // Linhas do pedestal: o tampo e a base, de cima pra baixo.
        private const int PlinthTop = 36;

        // ------------------------------------------------------------------
        // As estatuas
        // ------------------------------------------------------------------

        public static PixelCanvas Statue(int kind)
        {
            var c = new PixelCanvas(Width, Height);

            switch (kind % KindCount)
            {
                case 0: Miner(c); break;
                case 1: King(c); break;
                case 2: Drill(c); break;
                default: Heart(c); break;
            }

            Pedestal(c);
            Weather(c, kind);
            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>So o que acende: olhos, a ponta da broca, a fenda do coracao.</summary>
        public static PixelCanvas StatueGlow(int kind)
        {
            var c = new PixelCanvas(Width, Height);

            switch (kind % KindCount)
            {
                case 0:
                    // A lanterna do capacete e os olhos, olhando pra baixo.
                    Dot(c, 17, 7, 2);
                    c.Set(14, 11, Light);
                    c.Set(17, 11, Light);
                    break;
                case 1:
                    c.Set(13, 10, Light);
                    c.Set(16, 10, Light);
                    Dot(c, 15, 4, 1);
                    break;
                case 2:
                    // Os dois olhinhos de vigia da cabine e a ponta.
                    c.Set(12, 9, Light);
                    c.Set(19, 9, Light);
                    Dot(c, 16, 32, 1);
                    break;
                default:
                    // A fenda, de cima a baixo.
                    for (int y = 9; y <= 30; y++)
                    {
                        int x = 16 + (y / 4 % 2 == 0 ? 0 : (y % 8 < 4 ? 1 : -1));
                        c.Set(x, y, y % 3 == 0 ? LightCore : Light);
                    }

                    break;
            }

            // Um halo leve em volta do que acendeu.
            var halo = new PixelCanvas(Width, Height);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (c.Get(x, y).A == 0) continue;
                    ArtKit.Glow(halo, x + 0.5f, y + 0.5f, 3f, Light, 0.6f, true);
                }
            }

            halo.Blit(c, 0, 0);
            return halo;
        }

        private static void Dot(PixelCanvas c, int x, int y, int r)
        {
            for (int dy = -r; dy <= r; dy++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    if (dx * dx + dy * dy > r * r) continue;
                    c.Set(x + dx, y + dy, dx == 0 && dy == 0 ? LightCore : Light);
                }
            }
        }

        /// <summary>O pedestal: tampo, corpo e base, com a moldura do botao na frente.</summary>
        private static void Pedestal(PixelCanvas c)
        {
            // Tampo.
            for (int x = 4; x < 28; x++)
            {
                c.Set(x, PlinthTop, Plinth[3]);
                c.Set(x, PlinthTop + 1, Plinth[2]);
            }

            // Corpo, um pouco mais estreito.
            for (int y = PlinthTop + 2; y < 44; y++)
            {
                for (int x = 6; x < 26; x++)
                {
                    float light = x < 9 ? 0.7f : (x > 22 ? -0.6f : 0.05f);
                    c.Set(x, y, ArtKit.Tone(Plinth, light));
                }
            }

            // Base larga.
            for (int y = 44; y < 48; y++)
            {
                for (int x = 3; x < 29; x++)
                {
                    c.Set(x, y, ArtKit.Tone(Plinth, y == 44 ? 0.7f : (x > 25 ? -0.6f : -0.1f)));
                }
            }

            // O nicho do botao: um quadro fundo no meio do corpo.
            int top = Height - ButtonY - 7;
            for (int y = top; y < top + 7; y++)
            {
                for (int x = ButtonX - 1; x < ButtonX + 9; x++)
                {
                    bool edge = y == top || x == ButtonX - 1;
                    c.Set(x, y, edge ? Plinth[0] : Cut);
                }
            }

            // Os rebites de prata nos cantos do tampo.
            ArtKit.Rivet(c, 5, PlinthTop, Silver);
            ArtKit.Rivet(c, 25, PlinthTop, Silver);
        }

        /// <summary>Pedra velha: umas trincas e o lascado das bordas.</summary>
        private static void Weather(PixelCanvas c, int kind)
        {
            for (int y = 0; y < PlinthTop; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Rgba under = c.Get(x, y);
                    if (under.A == 0) continue;
                    if (ArtKit.Hash(x, y, 701 + kind) % 29 == 0) c.Set(x, y, Stone[0]);
                }
            }
        }

        private static void Body(PixelCanvas c, (float, float)[] points)
        {
            ArtKit.Polygon(c, points, (x, y) =>
            {
                float light = (16f - x) / 12f * 0.8f - (y - 20f) / 30f * 0.4f;
                return ArtKit.Tone(Stone, light);
            });
        }

        /// <summary>O Mineiro: capacete com lanterna, picareta no ombro, olhando pra baixo.</summary>
        private static void Miner(PixelCanvas c)
        {
            // Pernas e tronco, um pouco curvado pra frente.
            ArtKit.Limb(c, 13f, 26f, 12f, 35f, 2f, Stone);
            ArtKit.Limb(c, 18f, 26f, 19f, 35f, 2f, Stone, -0.2f);
            Body(c, new[] { (10f, 14f), (21f, 14f), (21f, 27f), (10f, 27f) });

            // O cinto e a bolsa.
            for (int x = 10; x < 21; x++) c.Set(x, 23, Stone[0]);
            ArtKit.Ball(c, 20f, 25f, 2f, 1.6f, Stone, -0.2f);

            // Cabeca baixa e capacete.
            ArtKit.Ball(c, 16f, 11f, 3.5f, 3.2f, Stone, 0.1f);
            ArtKit.Ball(c, 15.5f, 8f, 4.5f, 2.6f, Stone, 0.25f);
            for (int x = 10; x < 22; x++) c.Set(x, 9, Stone[1]);
            ArtKit.Ball(c, 17.5f, 7.5f, 1.4f, 1.2f, Silver, 0.2f);

            // Os bracos: um segura o cabo no ombro, o outro pende.
            ArtKit.Limb(c, 11f, 16f, 9f, 24f, 1.6f, Stone, -0.1f);
            ArtKit.Limb(c, 20f, 16f, 22f, 12f, 1.6f, Stone);

            // A picareta: o cabo cruzando o ombro e a lamina de prata atras.
            ArtKit.Limb(c, 25f, 18f, 8f, 5f, 0.8f, Plinth, 0.3f);
            ArtKit.Taper(c, 6f, 9f, 11f, 1f, 1.4f, 0.4f, Silver, 0.2f);
            ArtKit.Taper(c, 9f, 5f, 4f, 2f, 1.2f, 0.4f, Silver, 0.1f);
        }

        /// <summary>O Rei: coroa, manto largo, o braco apontando pra baixo.</summary>
        private static void King(PixelCanvas c)
        {
            // O manto, um trapezio que chega no tampo.
            Body(c, new[] { (11f, 13f), (20f, 13f), (25f, 36f), (6f, 36f) });

            // As dobras do manto.
            for (int y = 18; y < 36; y++)
            {
                c.Set(11 - (y - 18) / 5, y, Stone[1]);
                c.Set(15, y, Stone[1]);
                c.Set(20 + (y - 18) / 5, y, Stone[0]);
            }

            // A gola de prata e a cabeca erguida.
            for (int x = 10; x < 22; x++) c.Set(x, 13, Silver[x < 15 ? 3 : 2]);
            ArtKit.Ball(c, 15f, 9.5f, 3.4f, 3.6f, Stone, 0.2f);

            // A coroa: tres pontas de prata.
            for (int x = 11; x <= 19; x++) c.Set(x, 6, Silver[2]);
            for (int x = 11; x <= 19; x++) c.Set(x, 5, Silver[x < 14 ? 3 : 2]);
            foreach (int x in new[] { 11, 15, 19 })
            {
                c.Set(x, 4, Silver[3]);
                c.Set(x, 3, Silver[2]);
            }

            c.Set(15, 2, Silver[3]);

            // O braco esticado, apontando pra baixo e pra frente. O dedo
            // aponta pro chao: para baixo, sempre mais fundo.
            ArtKit.Limb(c, 20f, 15f, 26f, 24f, 1.7f, Stone);
            ArtKit.Limb(c, 26f, 24f, 27.5f, 29f, 0.8f, Stone, 0.2f);

            // O outro braco, guardado no manto.
            ArtKit.Limb(c, 11f, 15f, 10f, 22f, 1.6f, Stone, -0.2f);
        }

        /// <summary>A Broca: a cabine da maquina e a espiral descendo ate o tampo.</summary>
        private static void Drill(PixelCanvas c)
        {
            // A cabine, quadrada, com rebites.
            Body(c, new[] { (8f, 5f), (24f, 5f), (25f, 15f), (7f, 15f) });
            for (int x = 7; x < 26; x++) c.Set(x, 15, Stone[0]);
            ArtKit.Rivet(c, 9, 6, Silver);
            ArtKit.Rivet(c, 22, 6, Silver);

            // As vigias, onde os olhos acendem.
            c.Set(12, 9, Cut);
            c.Set(19, 9, Cut);
            c.Set(12, 10, Cut);
            c.Set(19, 10, Cut);

            // A espiral: um cone que afina ate a ponta, com as roscas.
            ArtKit.Polygon(c, new[] { (8f, 16f), (24f, 16f), (16.5f, 34f), (15.5f, 34f) }, (x, y) =>
            {
                float light = (16f - x) / 8f * 0.7f;
                return ArtKit.Tone(Silver, light - 0.3f);
            });

            for (int i = 0; i < 6; i++)
            {
                float y0 = 17f + i * 3f;
                float half = 8f * (1f - (y0 + 1.5f - 16f) / 18f);
                ArtKit.Line(c, (int)(16f - half), (int)y0, (int)(16f + half), (int)(y0 + 2f), Stone[0]);
            }

            // Os mineiros nao estao na frente dela. Ninguem esta.
        }

        /// <summary>O Coracao de Quartzo: um losango enorme, rachado de cima a baixo.</summary>
        private static void Heart(PixelCanvas c)
        {
            ArtKit.Polygon(c, new[] { (16f, 3f), (28f, 19f), (16f, 35f), (4f, 19f) }, (x, y) =>
            {
                // As facetas: cada quarto do losango numa luz.
                bool left = x < 16f;
                bool top = y < 19f;
                float light = left && top ? 0.8f : (top ? 0.3f : (left ? -0.1f : -0.5f));
                return ArtKit.Tone(Stone, light);
            });

            // As arestas internas das facetas.
            ArtKit.Line(c, 16, 4, 16, 34, Stone[1]);
            ArtKit.Line(c, 5, 19, 27, 19, Stone[1]);

            // A fenda, pintada de verde pelo mesmo que gravou a galeria.
            for (int y = 8; y <= 31; y++)
            {
                int x = 16 + (y / 4 % 2 == 0 ? 0 : (y % 8 < 4 ? 1 : -1));
                c.Set(x, y, Cut);
                c.Set(x + 1, y, y % 2 == 0 ? ArtKit.OxideDeep : Cut);
            }
        }

        // ------------------------------------------------------------------
        // O templo: tijolos, piso e colunas do Salao das Estatuas
        // ------------------------------------------------------------------

        private static readonly Rgba[] Brick =
            { new Rgba(38, 34, 52), new Rgba(58, 52, 76), new Rgba(76, 68, 98), new Rgba(96, 88, 120) };

        private static readonly Rgba Mortar = new Rgba(24, 22, 34);

        /// <summary>A parede de tijolos do fundo, 16x16, emendavel: fiadas de 8x4 desencontradas.</summary>
        public static PixelCanvas BrickWall()
        {
            var c = new PixelCanvas(16, 16);

            for (int y = 0; y < 16; y++)
            {
                int row = y / 4;
                int offset = row % 2 == 0 ? 0 : 4;

                for (int x = 0; x < 16; x++)
                {
                    int bx = (x + offset) % 8;
                    int by = y % 4;

                    if (by == 3 || bx == 7)
                    {
                        c.Set(x, y, Mortar);
                        continue;
                    }

                    // Cada tijolo num tom, com a quina de cima clara.
                    int brick = (x + offset) / 8 + row * 3;
                    float light = (ArtKit.Hash(brick, row, 901) % 5 - 2) * 0.2f + (by == 0 ? 0.35f : 0f) - (bx == 6 ? 0.3f : 0f);
                    Rgba color = ArtKit.Tone(Brick, light);

                    // Lascado e musgo de cristal, aqui e ali.
                    if (ArtKit.Hash(x, y, 902) % 23 == 0) color = Brick[0];
                    c.Set(x, y, color);
                }
            }

            return c;
        }

        /// <summary>O piso de lajes, 16x16, emendavel de lado: tampo claro e lajes largas.</summary>
        public static PixelCanvas TempleFloor()
        {
            var c = new PixelCanvas(16, 16);

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    Rgba color;
                    if (y == 0) color = Stone[3];
                    else if (y == 1) color = Stone[2];
                    else if (x == 15 || y == 8) color = Mortar;
                    else color = ArtKit.Tone(Stone, (y < 8 ? 0.0f : -0.4f) + (ArtKit.Hash(x, y, 911) % 13 == 0 ? -0.5f : 0f));

                    c.Set(x, y, color);
                }
            }

            return c;
        }

        /// <summary>A viga do teto, 16x16, emendavel de lado: o piso de cabeca pra baixo, com dentes.</summary>
        public static PixelCanvas TempleLintel()
        {
            var c = new PixelCanvas(16, 16);

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    Rgba color;
                    if (y == 15) color = Stone[0];
                    else if (y == 14) color = (x / 2) % 2 == 0 ? Stone[1] : Mortar;
                    else if (y == 7) color = Mortar;
                    else color = ArtKit.Tone(Stone, y < 7 ? -0.4f : 0.0f);

                    c.Set(x, y, color);
                }
            }

            return c;
        }

        /// <summary>A coluna, 16x16, emendavel de cima pra baixo: fuste canelado, luz da esquerda.</summary>
        public static PixelCanvas Pillar()
        {
            var c = new PixelCanvas(16, 16);

            for (int y = 0; y < 16; y++)
            {
                for (int x = 2; x < 14; x++)
                {
                    // As caneluras: sulcos verticais a cada 3 pixels.
                    bool flute = (x - 2) % 3 == 2;
                    float light = 0.9f - (x - 2) / 11f * 1.6f;
                    Rgba color = flute ? ArtKit.Tone(Stone, light - 0.6f) : ArtKit.Tone(Stone, light);

                    // Uma trinca, so num ponto.
                    if (x == 8 && y >= 5 && y <= 8) color = Stone[0];
                    c.Set(x, y, color);
                }

                c.Set(1, y, ArtKit.Outline);
                c.Set(14, y, ArtKit.Outline);
            }

            return c;
        }

        // ------------------------------------------------------------------
        // O botao
        // ------------------------------------------------------------------

        /// <summary>O botao do pedestal, 8x6: solto ele sai do nicho; apertado, afunda.</summary>
        public static PixelCanvas Button(bool pressed)
        {
            var c = new PixelCanvas(8, 6);
            int top = pressed ? 3 : 1;

            for (int y = top; y < 6; y++)
            {
                for (int x = 1; x < 7; x++)
                {
                    float light = y == top ? 0.9f : (x < 3 ? 0.4f : -0.2f);
                    c.Set(x, y, ArtKit.Tone(Silver, light));
                }
            }

            // O simbolo de onda gravado no botao.
            if (!pressed)
            {
                c.Set(3, 3, Silver[0]);
                c.Set(4, 2, Silver[0]);
                c.Set(4, 4, Silver[0]);
                c.Set(5, 3, Silver[0]);
            }

            return c;
        }

        // ------------------------------------------------------------------
        // As rachaduras
        // ------------------------------------------------------------------

        /// <summary>
        /// A rachadura no pe da parede, 16x16, de onde saem os mites: 0 so
        /// uma trinca, 1 rachando (com po caindo), 2 aberta.
        /// </summary>
        public static PixelCanvas Crack(int frame)
        {
            var c = new PixelCanvas(16, 16);
            var dark = new Rgba(14, 12, 22);
            var rim = new Rgba(92, 84, 120);
            var dust = new Rgba(150, 140, 180, 170);

            // A trinca: um zigue-zague do chao pra cima.
            int[] path = { 8, 8, 7, 7, 8, 9, 9, 8, 7, 6, 6, 7 };
            for (int i = 0; i < path.Length; i++)
            {
                int y = 15 - i;
                c.Set(path[i], y, dark);
                if (frame >= 1) c.Set(path[i] + 1, y, dark);
                if (frame >= 1 && i % 3 == 0) c.Set(path[i] - 1, y, rim);
            }

            // Os galhos.
            c.Set(10, 10, dark);
            c.Set(11, 9, dark);
            c.Set(5, 6, dark);
            c.Set(4, 5, dark);

            if (frame == 1)
            {
                // Po caindo e uma lasca saltando.
                for (int i = 0; i < 6; i++) c.Blend(4 + ArtKit.Hash(i, 0, 811) % 9, 2 + ArtKit.Hash(i, 1, 811) % 10, dust);
                c.Set(12, 4, rim);
            }

            if (frame == 2)
            {
                // A boca aberta, rente ao chao.
                for (int y = 9; y < 16; y++)
                {
                    int half = (y - 8) / 2 + 1;
                    for (int x = 8 - half; x <= 8 + half; x++) c.Set(x, y, dark);
                    c.Set(8 - half - 1, y, rim);
                    c.Set(8 + half + 1, y, rim);
                }

                // Lascas caidas na frente.
                c.Set(3, 15, rim);
                c.Set(13, 15, rim);
                c.Set(12, 14, rim);
            }

            return c;
        }

        // ------------------------------------------------------------------
        // A placa de prata
        // ------------------------------------------------------------------

        /// <summary>
        /// A placa de palavras do mini boss, 32x24: uma placa de prata rebitada
        /// so com as cinco palavras gravadas e, presa embaixo dela, um bloco de
        /// notas pequeno e torto, de papel.
        /// </summary>
        public static PixelCanvas SilverPlaque()
        {
            var c = new PixelCanvas(32, 24);

            // A placa: 32x13.
            for (int y = 0; y < 13; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    bool frame = x < 2 || x > 29 || y < 2 || y > 10;
                    float light = frame ? (x < 2 || y < 2 ? 0.9f : -0.6f) : 0.3f - y / 30f;
                    c.Set(x, y, ArtKit.Tone(Silver, light));
                }
            }

            ArtKit.Rivet(c, 2, 2, Silver);
            ArtKit.Rivet(c, 28, 2, Silver);
            ArtKit.Rivet(c, 2, 9, Silver);
            ArtKit.Rivet(c, 28, 9, Silver);

            // So as cinco palavras, separadas por pontos.
            int[] words = { 3, 3, 2, 4, 2 };
            int at = 5;
            foreach (int length in words)
            {
                for (int x = at; x < at + length; x++)
                {
                    c.Set(x, 6, Silver[0]);
                    if (ArtKit.Hash(x, 1, 822) % 2 == 0) c.Set(x, 5, Silver[0]);
                }

                at += length + 1;
                c.Set(at - 1, 6, Silver[1]);
                at += 1;
            }

            // O bloco de notas: 10x10, papel amarelado, espiral em cima, torto.
            var paper = new Rgba(214, 204, 168);
            var paperDark = new Rgba(168, 156, 120);
            var ink = new Rgba(70, 64, 90);

            for (int y = 14; y < 24; y++)
            {
                int shift = (y - 14) / 4;
                for (int x = 19 + shift; x < 29 + shift && x < 32; x++)
                {
                    c.Set(x, y, x == 28 + shift || y == 23 ? paperDark : paper);
                }
            }

            for (int x = 20; x < 28; x += 2) c.Set(x, 14, ArtKit.Iron[3]);

            // A letra miuda, rabiscada.
            for (int line = 0; line < 3; line++)
            {
                int y = 16 + line * 2;
                int shift = (y - 14) / 4;
                for (int x = 20 + shift; x < 27 + shift - (line == 2 ? 3 : 0); x++)
                {
                    if (ArtKit.Hash(x, line, 824) % 4 != 0) c.Set(x, y, ink);
                }
            }

            ArtKit.AddOutline(c);
            return c;
        }
    }
}
