using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Os escombros que entopem o poco do elevador no District 3, contados
    /// como desabamento: uma viga de latao do poco caiu atravessada e ficou
    /// entalada entre a parede oeste e o ombro de rocha da direita. Tudo o que
    /// veio depois — pedras da rocha azulada, um trilho arrancado e dobrado,
    /// lascas de cristal — se amontoou em cima dela, ate o teto do poco.
    /// Embaixo da viga, so os cabos do elevador pendurados e po caindo.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class RubbleArt
    {
        private static readonly Rgba[] Stone =
            { new Rgba(30, 34, 50), new Rgba(52, 60, 84), new Rgba(78, 90, 118), new Rgba(112, 126, 156) };

        private static readonly Rgba[] Shard =
            { new Rgba(70, 96, 140), new Rgba(110, 150, 200), new Rgba(160, 200, 240), new Rgba(220, 240, 255) };

        private static readonly Rgba Gap = new Rgba(20, 22, 32);

        public static PixelCanvas ElevatorRubble(int widthTiles, int heightTiles)
        {
            const int ppu = 16;
            int w = widthTiles * ppu;
            int h = heightTiles * ppu;
            var c = new PixelCanvas(w, h);

            // A viga: um pouco torta (a ponta oeste mais baixa), entrando nas
            // duas paredes. Embaixo dela, vao livre.
            float beamLeft = h - 20f;
            float beamRight = h - 25f;
            float BeamTop(float x) => beamLeft + (beamRight - beamLeft) * x / w;

            // A pilha: tudo acima da viga. O lado de baixo segue a viga.
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (y < BeamTop(x)) c.Set(x, y, Gap);
                }
            }

            // As pedras, das grandes pras pequenas. As grandes ficam embaixo,
            // em cima da viga, que e onde o peso assenta.
            int index = 0;
            for (int pass = 0; pass < 3; pass++)
            {
                float size = pass == 0 ? 12f : (pass == 1 ? 7f : 4f);
                int step = pass == 0 ? 18 : (pass == 1 ? 13 : 9);

                for (int y = h - 26; y > 2; y -= step)
                {
                    for (int x = 4; x < w; x += step)
                    {
                        index++;
                        float jx = ArtKit.Hash(index, 0, 2102 + pass) % step - step * 0.5f;
                        float jy = ArtKit.Hash(index, 1, 2102 + pass) % step - step * 0.5f;
                        float cx = x + jx;
                        float cy = y + jy;
                        if (c.Get((int)cx, (int)cy).A == 0) continue;

                        float grow = pass == 0 && cy > h * 0.6f ? 1.25f : 1f;
                        float rx = size * grow * (0.7f + ArtKit.Hash(index, 2, 2105) % 50 / 100f);
                        float ry = rx * (0.6f + ArtKit.Hash(index, 3, 2105) % 40 / 100f);
                        Boulder(c, cx, cy, rx, ry, index);
                    }
                }
            }

            // O trilho arrancado: dobrado em V, uma perna enterrada na pilha
            // e a outra apontando pra sala.
            ArtKit.Limb(c, 8f, h * 0.22f, w * 0.48f, h * 0.48f, 1.6f, ArtKit.Brass, 0.2f);
            ArtKit.Limb(c, w * 0.48f, h * 0.48f, w - 4f, h * 0.30f, 1.6f, ArtKit.Brass, 0.2f);
            for (int i = 0; i < 4; i++)
            {
                (float rx, float ry) = (8f + i * (w * 0.48f - 8f) / 4f, h * 0.22f + i * (h * 0.26f) / 4f);
                c.Set((int)rx, (int)ry, ArtKit.Brass[0]);
            }

            // Uma chapa da parede do poco, amassada, atravessada.
            for (int y = (int)(h * 0.62f); y < (int)(h * 0.62f) + 8; y++)
            {
                for (int x = 30; x < 62; x++)
                {
                    int bend = (x - 30) / 8 % 2 == 0 ? 0 : 1;
                    if (c.Get(x, y + bend).A == 0) continue;
                    c.Set(x, y + bend, ArtKit.Tone(ArtKit.Brass, (y - h * 0.62f) < 2 ? 0.7f : (x % 9 == 0 ? -0.5f : 0.1f)));
                }
            }

            // Lascas de cristal presas entre as pedras.
            foreach ((float sx, float sy, float angle) in new[] { (18f, 20f, -60f), (60f, 40f, -110f), (30f, 76f, -80f), (66f, 92f, -40f) })
            {
                (float tx, float ty) = ArtKit.Along(sx, sy, angle, 9f);
                ArtKit.Taper(c, sx, sy, tx, ty, 2.6f, 0.4f, Shard, 0.4f);
            }

            Beam(c, w, beamLeft, beamRight);

            // Os cabos do elevador, partidos, pendurados da viga.
            Cable(c, 22, (int)BeamTop(22) + 8, 13, 3);
            Cable(c, 47, (int)BeamTop(47) + 8, 9, 7);

            ArtKit.AddOutline(c, ArtKit.Outline);

            // As pontas da viga somem na rocha: sem contorno do lado das paredes.
            for (int y = 0; y < h; y++)
            {
                if (c.Get(0, y).R == ArtKit.Outline.R && c.Get(1, y).A > 0) c.Set(0, y, c.Get(1, y));
                if (c.Get(w - 1, y).R == ArtKit.Outline.R && c.Get(w - 2, y).A > 0) c.Set(w - 1, y, c.Get(w - 2, y));
            }

            return c;
        }

        /// <summary>
        /// A viga em I, de latao escurecido: aba de cima, alma com rebites,
        /// aba de baixo. Inclinada de beamLeft (oeste) a beamRight (leste),
        /// e envergada no meio pelo peso.
        /// </summary>
        private static void Beam(PixelCanvas c, int w, float beamLeft, float beamRight)
        {
            for (int x = 0; x < w; x++)
            {
                float sag = (float)Math.Sin(Math.PI * x / w) * 2.5f;
                int top = (int)(beamLeft + (beamRight - beamLeft) * x / w + sag);

                for (int i = 0; i < 9; i++)
                {
                    int y = top + i;
                    Rgba color;

                    if (i < 2) color = ArtKit.Tone(ArtKit.Brass, i == 0 ? 0.8f : 0.3f);
                    else if (i > 6) color = ArtKit.Tone(ArtKit.Brass, i == 7 ? 0.0f : -0.6f);
                    else color = ArtKit.Tone(ArtKit.Brass, (x % 12 == 0 ? -0.6f : -0.15f));

                    // Azinhavre e amassados, aqui e ali.
                    if (ArtKit.Noise(x * 0.15f, y * 0.15f, 2121) > 0.72f) color = ArtKit.Tone(ArtKit.Verdigris, 0f);
                    c.Set(x, y, color);
                }

                if (x % 12 == 6) ArtKit.Rivet(c, x, top + 4, ArtKit.Brass);
            }
        }

        /// <summary>Um cabo de aco partido, balancando, de (x, y) pra baixo.</summary>
        private static void Cable(PixelCanvas c, int x, int y, int length, int seed)
        {
            for (int i = 0; i < length; i++)
            {
                int dx = (int)Math.Round(Math.Sin((i + seed) * 0.5f) * 1.2f);
                c.Set(x + dx, y + i, i % 3 == 0 ? ArtKit.Iron[3] : ArtKit.Iron[2]);
            }

            // A ponta desfiada.
            int end = y + length;
            c.Set(x - 1, end, ArtKit.Iron[1]);
            c.Set(x + 1, end, ArtKit.Iron[1]);
        }

        /// <summary>Pedrinhas soltas que escorregaram da pilha, 16x6, assentadas no chao.</summary>
        public static PixelCanvas Pebbles(int seed)
        {
            var c = new PixelCanvas(16, 6);
            int count = 3 + seed % 2;

            for (int i = 0; i < count; i++)
            {
                float cx = 2f + ArtKit.Hash(i, seed, 2131) % 12;
                float rx = 1.3f + ArtKit.Hash(i, seed, 2132) % 20 / 10f;
                float ry = rx * 0.7f;
                float cy = 6f - ry;

                // A pedra so pinta em cima do que ja existe: primeiro, o miolo.
                for (int y = 0; y < 6; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        float dx = (x + 0.5f - cx) / rx;
                        float dy = (y + 0.5f - cy) / ry;
                        if (dx * dx + dy * dy <= 1f) c.Set(x, y, Gap);
                    }
                }

                Boulder(c, cx, cy, rx, ry, i + seed * 7);
            }

            if (seed % 2 == 0)
            {
                ArtKit.Taper(c, 11f, 5f, 13f, 2f, 1.2f, 0.3f, Shard, 0.4f);
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
