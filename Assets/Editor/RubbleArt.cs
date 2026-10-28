using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Os escombros que entopem o poco do elevador no District 3: pedras da
    /// rocha azulada do distrito, os trilhos de latao do poco torcidos no meio
    /// delas e lascas de cristal. Enche o retangulo inteiro (e um tampao),
    /// menos a beirada que da pra sala, que desce em rampa.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class RubbleArt
    {
        private static readonly Rgba[] Stone =
            { new Rgba(30, 34, 50), new Rgba(52, 60, 84), new Rgba(78, 90, 118), new Rgba(112, 126, 156) };

        private static readonly Rgba[] Shard =
            { new Rgba(70, 96, 140), new Rgba(110, 150, 200), new Rgba(160, 200, 240), new Rgba(220, 240, 255) };

        public static PixelCanvas ElevatorRubble(int widthTiles, int heightTiles)
        {
            const int ppu = 16;
            int w = widthTiles * ppu;
            int h = heightTiles * ppu;
            var c = new PixelCanvas(w, h);

            // O fundo: o escuro entre as pedras. A beirada da direita (a da sala)
            // desce em rampa irregular.
            for (int y = 0; y < h; y++)
            {
                int edge = w - 1 - (int)(ArtKit.Noise(y * 0.12f, 0f, 2101) * 6f);
                for (int x = 0; x <= edge; x++) c.Set(x, y, new Rgba(20, 22, 32));
            }

            // As pedras, das grandes pras pequenas, de baixo pra cima.
            int index = 0;
            for (int pass = 0; pass < 3; pass++)
            {
                float size = pass == 0 ? 12f : (pass == 1 ? 7f : 4f);
                int step = pass == 0 ? 18 : (pass == 1 ? 13 : 9);

                for (int y = h - 4; y > 2; y -= step)
                {
                    for (int x = 4; x < w; x += step)
                    {
                        index++;
                        float jx = ArtKit.Hash(index, 0, 2102 + pass) % step - step * 0.5f;
                        float jy = ArtKit.Hash(index, 1, 2102 + pass) % step - step * 0.5f;
                        float cx = x + jx;
                        float cy = y + jy;
                        if (c.Get((int)cx, (int)cy).A == 0) continue;

                        float rx = size * (0.7f + ArtKit.Hash(index, 2, 2105) % 50 / 100f);
                        float ry = rx * (0.6f + ArtKit.Hash(index, 3, 2105) % 40 / 100f);
                        Boulder(c, cx, cy, rx, ry, index);
                    }
                }
            }

            // Os trilhos de latao do poco, partidos e torcidos no meio das pedras.
            ArtKit.Limb(c, 6f, h * 0.30f, w - 12f, h * 0.18f, 1.6f, ArtKit.Brass, 0.2f);
            ArtKit.Limb(c, 10f, h * 0.62f, w * 0.55f, h * 0.70f, 1.6f, ArtKit.Brass, 0.2f);
            ArtKit.Limb(c, w * 0.55f, h * 0.70f, w * 0.75f, h * 0.55f, 1.6f, ArtKit.Brass, 0.2f);

            // Uma chapa da parede do poco, amassada, atravessada.
            for (int y = (int)(h * 0.42f); y < (int)(h * 0.42f) + 10; y++)
            {
                for (int x = 14; x < 46; x++)
                {
                    int bend = (x - 14) / 8 % 2 == 0 ? 0 : 1;
                    if (c.Get(x, y + bend).A == 0) continue;
                    c.Set(x, y + bend, ArtKit.Tone(ArtKit.Brass, (y - h * 0.42f) < 2 ? 0.7f : (x % 9 == 0 ? -0.5f : 0.1f)));
                }
            }

            // Lascas de cristal presas entre as pedras.
            foreach ((float sx, float sy, float angle) in new[] { (18f, 20f, -60f), (60f, 52f, -110f), (30f, 96f, -80f), (64f, 112f, -40f) })
            {
                (float tx, float ty) = ArtKit.Along(sx, sy, angle, 9f);
                ArtKit.Taper(c, sx, sy, tx, ty, 2.6f, 0.4f, Shard, 0.4f);
            }

            ArtKit.AddOutline(c, ArtKit.Outline);
            return c;
        }

        /// <summary>Uma pedra: luz de cima a esquerda, e umas trincas.</summary>
        private static void Boulder(PixelCanvas c, float cx, float cy, float rx, float ry, int seed)
        {
            for (int y = (int)(cy - ry) - 1; y <= (int)(cy + ry) + 1; y++)
            {
                for (int x = (int)(cx - rx) - 1; x <= (int)(cx + rx) + 1; x++)
                {
                    if (c.Get(x, y).A == 0) continue;

                    float dx = (x + 0.5f - cx) / rx;
                    float dy = (y + 0.5f - cy) / ry;
                    float d = dx * dx + dy * dy;
                    if (d > 1f) continue;

                    // A borda de cada pedra escura: e o que as separa umas das outras.
                    if (d > 0.8f)
                    {
                        c.Set(x, y, new Rgba(18, 20, 30));
                        continue;
                    }

                    float light = -dx * 0.8f - dy * 0.9f + (ArtKit.Noise(x * 0.4f, y * 0.4f, seed) - 0.5f) * 0.3f;
                    c.Set(x, y, ArtKit.Tone(Stone, light));
                }
            }

            if (rx > 5f)
            {
                int x0 = (int)cx;
                int y0 = (int)(cy - ry * 0.3f);
                for (int i = 0; i < (int)rx; i++)
                {
                    int x = x0 + i - (int)(rx / 2);
                    int y = y0 + (ArtKit.Hash(i, seed, 2111) % 3 == 0 ? 1 : 0) + i / 3;
                    if (c.Get(x, y).A > 0) c.Set(x, y, Stone[0]);
                }
            }
        }
    }
}
