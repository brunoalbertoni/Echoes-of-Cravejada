using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A Grande Escadaria que sai dos Ateliers e sobe ate o palacio do Rei:
    /// degraus de marmore de meio tile, a passadeira de veludo vermelho com os
    /// varoes de latao em cada degrau, a balaustrada de marmore com o corrimao
    /// dourado acompanhando a subida, os patamares e, no alto, a porta do
    /// palacio, com a luz fria da prata vazando pela fresta.
    ///
    /// Tudo a 16 px por tile (o tamanho dos tiles de marmore). Os degraus sao
    /// so o desenho: quem segura o Scrap sao as caixas que o gerador poe.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class StairArt
    {
        public const int Tile = 16;

        /// <summary>A altura do corrimao acima do degrau, em pixels.</summary>
        private const int RailHeight = 22;

        private static readonly Rgba[] Marble = LabArt.Marble;
        private static readonly Rgba[] Velvet = LabArt.Velvet;
        private static readonly Rgba[] Gold = ArtKit.Brass;

        /// <summary>
        /// Um lance de degraus subindo pra direita: steps colunas, cada uma
        /// meio tile acima da anterior. O pivo e o canto de baixo, a esquerda,
        /// no chao de onde o lance sai.
        /// </summary>
        public static PixelCanvas Flight(int steps)
        {
            int width = steps * Tile;
            int height = steps * Tile / 2 + Tile + RailHeight + 6;
            var c = new PixelCanvas(width, height);

            // A balaustrada, atras: balaustres em cada degrau e o corrimao inclinado.
            for (int i = 0; i < steps; i++)
            {
                int top = height - (i + 1) * Tile / 2;
                for (int b = 0; b < 2; b++)
                {
                    int bx = i * Tile + 3 + b * 8;
                    int by = top - (b == 1 ? 4 : 0);
                    Baluster(c, bx, by, RailHeight - 2);
                }
            }

            for (int x = 0; x < width; x++)
            {
                int rail = height - (int)((x + 1) * 0.5f) - RailHeight - 2;
                c.Set(x, rail - 1, Gold[3]);
                c.Set(x, rail, Gold[2]);
                c.Set(x, rail + 1, Gold[1]);
                c.Set(x, rail + 2, Gold[0]);
            }

            // Os degraus: piso claro, espelho de marmore, sombra embaixo.
            for (int i = 0; i < steps; i++)
            {
                int x0 = i * Tile;
                int top = height - (i + 1) * Tile / 2;
                int bottom = Math.Min(height, top + Tile);

                for (int y = top; y < bottom; y++)
                {
                    for (int x = x0; x < x0 + Tile; x++)
                    {
                        float light = y == top ? 0.8f : (y == top + 1 ? 0.45f : (x == x0 ? 0.3f : -0.05f - (y - top) * 0.04f));
                        Rgba color = ArtKit.Tone(Marble, light);

                        // Os veios do marmore.
                        if (ArtKit.Hash(x / 3, y / 2, 8101) % 23 == 0 && y > top + 2) color = Marble[0];
                        c.Set(x, y, color);
                    }
                }

                // A passadeira de veludo, corrida: deitada no piso e descendo
                // pelo espelho do degrau, de um degrau ao outro.
                for (int x = x0; x < x0 + Tile; x++)
                {
                    c.Set(x, top, Velvet[3]);
                    c.Set(x, top + 1, Velvet[2]);
                    c.Set(x, top + 2, Velvet[1]);
                    c.Set(x, top + 3, Gold[1]);
                }

                for (int y = top; y < Math.Min(bottom, top + Tile / 2 + 3); y++)
                {
                    for (int x = x0; x < x0 + 3; x++) c.Set(x, y, x == x0 + 2 ? Velvet[0] : Velvet[1]);
                }

                // O varao de latao no canto do degrau, segurando a passadeira.
                for (int y = top; y < top + 4; y++) c.Set(x0 + 3, y, y == top ? Gold[3] : Gold[2]);
                c.Set(x0 + 4, top + 1, Gold[0]);
            }

            // Os pilares das pontas, com o pinaculo dourado.
            Newel(c, 1, height - Tile / 2);
            Newel(c, width - 6, height - steps * Tile / 2);
            return c;
        }

        /// <summary>
        /// Um patamar: o tapete por cima do marmore e a balaustrada reta atras.
        /// O pivo e o canto de baixo, a esquerda, no piso do patamar.
        /// </summary>
        public static PixelCanvas Landing(int tiles)
        {
            int width = tiles * Tile;
            int height = RailHeight + 8;
            var c = new PixelCanvas(width, height);
            int floor = height - 1;

            for (int x = 2; x < width - 2; x += 8) Baluster(c, x, floor - 2, RailHeight - 2);

            for (int x = 0; x < width; x++)
            {
                int rail = floor - RailHeight;
                c.Set(x, rail - 1, Gold[3]);
                c.Set(x, rail, Gold[2]);
                c.Set(x, rail + 1, Gold[0]);
            }

            // O tapete, rente ao piso.
            for (int x = 0; x < width; x++)
            {
                c.Set(x, floor - 1, x % 16 == 0 ? Gold[2] : Velvet[2]);
                c.Set(x, floor, Velvet[1]);
            }

            return c;
        }

        /// <summary>
        /// A porta do palacio no alto da escadaria: o arco de marmore com a
        /// moldura dourada, o brasao 925 na chave do arco e as duas folhas
        /// entreabertas, com a luz da prata vazando. 48x64, pivo na base.
        /// </summary>
        public static PixelCanvas Doorway()
        {
            const int w = 48;
            const int h = 64;
            var c = new PixelCanvas(w, h);
            float cx = w / 2f;

            // O arco de marmore.
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = (x + 0.5f - cx) / (w / 2f);
                    float arch = y < 24 ? (float)Math.Sqrt(Math.Max(0f, 1f - (24f - y) * (24f - y) / (24f * 24f))) : 1f;
                    if (Math.Abs(dx) > arch) continue;

                    float inner = y < 30 ? (float)Math.Sqrt(Math.Max(0f, 1f - (30f - y) * (30f - y) / (22f * 22f))) * 0.72f : 0.72f;
                    bool opening = Math.Abs(dx) < inner && y > 8;
                    if (opening)
                    {
                        // A luz fria que vem do palacio, mais forte no meio.
                        float k = 1f - Math.Abs(dx) / Math.Max(0.01f, inner);
                        Rgba glow = Rgba.Lerp(new Rgba(150, 160, 200), new Rgba(250, 252, 255), k * k);
                        c.Set(x, y, glow);
                        continue;
                    }

                    float light = -dx * 0.6f + 0.2f;
                    c.Set(x, y, ArtKit.Tone(Marble, light));
                }
            }

            // As duas folhas entreabertas, de madeira escura com almofadas douradas.
            for (int side = -1; side <= 1; side += 2)
            {
                int x0 = side < 0 ? 8 : w - 17;
                for (int y = 30; y < h; y++)
                {
                    for (int x = x0; x < x0 + 9; x++)
                    {
                        bool frame = x == x0 || x == x0 + 8 || y == 30 || (y - 30) % 14 == 0;
                        c.Set(x, y, frame ? Gold[1] : ArtKit.Tone(LabArt.Walnut, side < 0 ? 0.2f : -0.2f));
                    }
                }

                c.Set(side < 0 ? x0 + 7 : x0 + 1, 52, Gold[3]);
            }

            // A moldura dourada do arco e o brasao na chave.
            for (int a = 0; a <= 32; a++)
            {
                double t = Math.PI + a * Math.PI / 32.0;
                c.Set((int)Math.Round(cx + Math.Cos(t) * 22.5), (int)Math.Round(24 + Math.Sin(t) * 22.5), Gold[2]);
            }

            ArtKit.Ball(c, cx, 6f, 5f, 5f, Gold, 0.3f);
            c.Fill((int)cx - 2, 4, 4, 1, Velvet[2]);
            c.Set((int)cx, 6, Gold[0]);

            ArtKit.AddOutline(c);
            return c;
        }

        private static void Baluster(PixelCanvas c, int x, int bottom, int height)
        {
            for (int y = bottom - height; y < bottom; y++)
            {
                float k = (y - (bottom - height)) / (float)height;
                int half = k < 0.15f || k > 0.85f ? 2 : (Math.Abs(k - 0.5f) < 0.18f ? 2 : 1);
                for (int dx = -half; dx <= half; dx++)
                {
                    float light = -dx * 0.5f + 0.2f;
                    c.Set(x + dx, y, ArtKit.Tone(Marble, light - 0.15f));
                }
            }
        }

        private static void Newel(PixelCanvas c, int x, int bottom)
        {
            int top = bottom - RailHeight - 6;
            for (int y = top; y < bottom; y++)
            {
                for (int dx = 0; dx < 5; dx++) c.Set(x + dx, y, ArtKit.Tone(Marble, 0.4f - dx * 0.25f));
            }

            ArtKit.Ball(c, x + 2.5f, top - 2f, 2.6f, 2.6f, Gold, 0.3f);
            c.Set(x + 2, top - 5, Gold[3]);
        }
    }
}
