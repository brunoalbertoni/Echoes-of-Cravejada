using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O Rei de Prata caido, depois de deitar: o ultimo quadro da queda dele
    /// (KingArt, corrompido) com a armadura rachada, lascas de prata e
    /// facetas de quartzo espalhadas no chao do salao e o Oxide saindo do
    /// corpo em fiapos verdes que sobem e somem. Loop de FrameCount quadros,
    /// no mesmo tamanho e pivo da folha do Rei, pra trocar sem pulo.
    /// </summary>
    public static class KingWreckArt
    {
        public const int Width = KingArt.Width;
        public const int Height = KingArt.Height;
        public const int PixelsPerUnit = KingArt.PixelsPerUnit;
        public const float PivotX = KingArt.PivotX;
        public const float PivotY = KingArt.PivotY;
        public const int FrameCount = 8;

        private static readonly Rgba[] Silver =
            { new Rgba(84, 90, 112), new Rgba(146, 154, 178), new Rgba(206, 214, 230), new Rgba(250, 252, 255) };

        private static readonly Rgba[] Quartz =
            { new Rgba(130, 150, 186), new Rgba(186, 206, 232), new Rgba(230, 240, 252), new Rgba(255, 255, 255) };

        public static PixelCanvas[] Frames()
        {
            PixelCanvas lying = KingArt.Frames(true)[KingArt.Death + KingArt.DeathFrames - 1];
            var c = new PixelCanvas(Width, Height);
            c.Blit(lying, 0, 0);

            // Onde o corpo esta: a faixa opaca mais baixa da folha.
            int top = Height, left = Width, right = 0, ground = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (c.Get(x, y).A < 200) continue;
                    top = Math.Min(top, y);
                    left = Math.Min(left, x);
                    right = Math.Max(right, x);
                    ground = Math.Max(ground, y);
                }
            }

            int span = Math.Max(1, right - left);

            // A armadura rachando, com o verde aceso nas frestas.
            for (int i = 0; i < 5; i++)
            {
                float x = left + span * (0.25f + i * 0.13f);
                float y = top + (ground - top) * 0.45f;
                ArtKit.Crack(c, x, y, i % 2 == 0 ? 60f : 120f, 6 + i % 3 * 2, 9100 + i, 0.8f);
            }

            // Lascas de prata e facetas de quartzo espalhadas pelo chao.
            for (int i = 0; i < 14; i++)
            {
                int x = left - 14 + ArtKit.Hash(i, 3, 9200) % (span + 28);
                int y = ground - ArtKit.Hash(i, 4, 9200) % 2;
                if (i % 3 == 0)
                {
                    ArtKit.Polygon(c, new[] { (x - 1.5f, y + 0.5f), (x + 0.5f, y - 4f), (x + 2.5f, y + 0.5f) },
                        (px, py) => px < x + 0.5f ? Quartz[2] : Quartz[1]);
                }
                else
                {
                    c.Set(x, y, Silver[3]);
                    c.Set(x + 1, y, Silver[1]);
                    c.Set(x, y - 1, Silver[2]);
                }
            }

            var oxide = new Rgba(190, 255, 214);
            var oxideDark = new Rgba(120, 230, 170);
            float mid = top + (ground - top) * 0.4f;
            var vents = new[]
            {
                new WreckKit.Vent(left + span * 0.35f, mid, 70f, 5f, oxide, oxideDark),
                new WreckKit.Vent(left + span * 0.55f, mid, 90f, 6f, oxide, oxideDark),
                new WreckKit.Vent(left + span * 0.75f, mid, 60f, 4f, oxide, oxideDark),
            };

            PixelCanvas[] frames = WreckKit.Animate(c, vents, null, FrameCount, 93, 4, 105);

            // O brilho passando pelas lascas.
            for (int f = 0; f < frames.Length; f++)
            {
                int x = left - 14 + (f * 37) % (span + 28);
                for (int dx = -1; dx <= 1; dx++)
                {
                    Rgba under = frames[f].Get(x + dx, ground - 1);
                    if (under.A > 200 && under.B > 150) frames[f].Set(x + dx, ground - 1, new Rgba(255, 255, 255));
                }
            }

            return frames;
        }
    }
}
