using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O Coracao de Quartzo na Sala do Trono, so cenario: a sala fica na borda
    /// da cratera que a broca abriu, e atras do trono um janelao em arco olha
    /// pra baixo, pra dentro dela. La no fundo, entre andaimes, esta o
    /// Coracao, com a broca ainda cravada. Veios de quartzo sobem dele, passam
    /// pelo peitoril e descem a parede ate o pe do trono: e dali que o Rei
    /// puxa a forca.
    ///
    ///   Window(false)  radiante: o Coracao branco-azulado, inteiro.
    ///   Window(true)   corrompido: rachado, o verde escorrendo, a fumaca.
    ///   Veins()        so os veios, em branco: o jogo pinta e faz pulsar.
    ///
    /// Tudo 320x320 (20 x 20 tiles a 16 px), pe no meio de baixo: o janelao
    /// ocupa os 224 px de cima; embaixo, so a parede (transparente) e os veios.
    /// Sem UnityEngine.
    /// </summary>
    public static class QuartzHeartArt
    {
        public const int Size = 320;
        public const int WindowHeight = 224;

        private static readonly Rgba Outline = new Rgba(22, 22, 34);
        private static readonly Rgba[] Silver = { new Rgba(70, 76, 98), new Rgba(130, 138, 164), new Rgba(196, 204, 226), new Rgba(244, 246, 255) };
        private static readonly Rgba[] Geode = { new Rgba(26, 18, 46), new Rgba(58, 40, 98), new Rgba(104, 76, 160), new Rgba(170, 140, 226) };
        private static readonly Rgba[] Iron = { new Rgba(40, 34, 30), new Rgba(84, 70, 56), new Rgba(130, 112, 86), new Rgba(180, 160, 124) };
        private static readonly Rgba Clean = new Rgba(200, 228, 255);
        private static readonly Rgba White = new Rgba(250, 252, 255);
        private static readonly Rgba Green = new Rgba(120, 255, 170);
        private static readonly Rgba GreenDeep = new Rgba(30, 110, 70);

        // O arco do janelao: centro, meia largura, base e o alto da curva.
        private const float Cx = 160f;
        private const float HalfW = 112f;
        private const float ArchTop = 8f;
        private const float SillY = WindowHeight - 10f;
        private const float SpringY = 104f;

        // O Coracao, la no fundo.
        private const float HeartX = 160f;
        private const float HeartY = 164f;
        private const float HeartR = 32f;

        /// <summary>Se o ponto esta dentro do vidro do arco.</summary>
        private static bool InsideArch(float x, float y, float inset)
        {
            float half = HalfW - inset;
            if (Math.Abs(x - Cx) > half || y > SillY - inset) return false;
            if (y >= SpringY) return true;

            float dy = (SpringY - y) / (SpringY - ArchTop - inset);
            return (x - Cx) * (x - Cx) / (half * half) + dy * dy <= 1f;
        }

        public static PixelCanvas Window(bool corrupt)
        {
            var c = new PixelCanvas(Size, Size);

            // 1. A cratera vista de cima: paredes de geode afunilando pro fundo escuro.
            for (int y = 0; y < WindowHeight; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    if (!InsideArch(x, y, 0f)) continue;

                    float depth = y / (float)WindowHeight;
                    float wall = Math.Abs(x - Cx) / HalfW;

                    // O funil: as paredes fecham quanto mais fundo.
                    float rim = 0.25f + depth * 0.55f;
                    Rgba tone;
                    if (wall > rim)
                    {
                        float n = ArtKit.Noise(x * 0.07f, y * 0.05f, 8101);
                        tone = ArtKit.Tone(Geode, 0.1f + (wall - rim) * 0.9f - depth * 0.6f + n * 0.5f);

                        // Facetas de cristal na parede.
                        if (ArtKit.Hash(x / 6, y / 9, 8102) % 9 == 0) tone = Rgba.Lerp(tone, Geode[3], 0.45f);
                    }
                    else
                    {
                        // O ar da cratera: escuro, com a bruma de baixo.
                        tone = Rgba.Lerp(new Rgba(20, 16, 36), new Rgba(8, 6, 16), depth);
                    }

                    c.Set(x, y, tone);
                }
            }

            // 2. As camadas da cidade nas bordas de cima: latao e geodes.
            for (int i = 0; i < 26; i++)
            {
                int x = 52 + ArtKit.Hash(i, 0, 8103) % 216;
                int y = 20 + ArtKit.Hash(i, 1, 8103) % 70;
                if (!InsideArch(x, y, 4f) || Math.Abs(x - Cx) < 40) continue;
                c.Set(x, y, new Rgba(255, 220, 150));
                c.Set(x + 1, y, new Rgba(255, 200, 120));
            }

            // 3. Os andaimes da escavacao, em volta do Coracao.
            for (int side = -1; side <= 1; side += 2)
            {
                float baseX = HeartX + side * 46f;
                for (int k = 0; k < 4; k++)
                {
                    float y = 120f + k * 22f;
                    ArtKit.Line(c, (int)(baseX - side * 20f), (int)y, (int)(baseX + side * 16f), (int)y, Iron[1]);
                }

                ArtKit.Line(c, (int)baseX, 112, (int)baseX, WindowHeight - 14, Iron[2]);
                ArtKit.Line(c, (int)(baseX + side * 12f), 112, (int)(baseX + side * 12f), WindowHeight - 14, Iron[1]);
                for (int k = 0; k < 4; k++)
                {
                    float y = 120f + k * 22f;
                    ArtKit.Line(c, (int)baseX, (int)y, (int)(baseX + side * 12f), (int)(y + 22f), Iron[0]);
                }
            }

            // 4. O Coracao.
            Rgba core = corrupt ? Green : Clean;
            ArtKit.Glow(c, HeartX, HeartY, HeartR * 3.4f, core, corrupt ? 0.45f : 0.55f, false);
            ArtKit.Polygon(c, new[] { (HeartX, HeartY - HeartR * 1.45f), (HeartX + HeartR, HeartY), (HeartX, HeartY + HeartR * 1.45f), (HeartX - HeartR, HeartY) }, (x, y) =>
            {
                float d = Math.Abs(x - HeartX) / HeartR + Math.Abs(y - HeartY) / (HeartR * 1.45f);
                bool left = x < HeartX;
                Rgba tone = d < 0.3f ? White : d < 0.65f ? (left ? Rgba.Lerp(core, White, 0.35f) : core) : core.Scaled(left ? 0.7f : 0.5f);

                // As facetas: riscos claros diagonais.
                if (((int)(x * 0.7f + y) % 13) == 0) tone = Rgba.Lerp(tone, White, 0.4f);

                // Corrompido: manchas de Oxido descendo da rachadura.
                if (corrupt && ArtKit.Noise(x * 0.12f, y * 0.12f, 8104) > 0.62f) tone = Rgba.Lerp(tone, GreenDeep, 0.6f);
                return tone;
            });

            // A broca, cravada de lado: o furo que comecou tudo.
            ArtKit.Taper(c, HeartX + 70f, HeartY - 44f, HeartX + 14f, HeartY - 8f, 9f, 2f, ArtKit.Plain(Iron), 0.1f);
            ArtKit.Ball(c, HeartX + 72f, HeartY - 45f, 10f, 8f, Iron, 0.2f);

            if (corrupt)
            {
                ArtKit.Crack(c, HeartX - 2f, HeartY - HeartR * 1.3f, 95f, (int)(HeartR * 2.6f), 8105, 1f);
                ArtKit.Crack(c, HeartX + 12f, HeartY - 6f, 140f, 30, 8106, 1f);

                // A fumaca subindo da cratera.
                for (int i = 0; i < 18; i++)
                {
                    float px = HeartX - 70f + ArtKit.Hash(i, 2, 8107) % 140;
                    float py = 60f + ArtKit.Hash(i, 3, 8107) % 130;
                    if (InsideArch(px, py, 10f)) ArtKit.Puff(c, px, py, 16f, new Rgba(90, 190, 120), GreenDeep, 45);
                }
            }

            // 5. Vidro: um reflexo diagonal por cima de tudo.
            for (int i = 0; i < 260; i++)
            {
                int x = 70 + i;
                for (int t = 0; t < 4; t++)
                {
                    int y = 210 - i + t;
                    if (InsideArch(x, y, 2f)) c.Blend(x, y, new Rgba(230, 240, 255, 26));
                }
            }

            // 6. A moldura de prata: o arco, os montantes e o peitoril.
            for (int y = 0; y < WindowHeight; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    bool glass = InsideArch(x, y, 0f);
                    bool frame = InsideArch(x, y, -8f) && !glass;
                    bool mullion = glass && (Math.Abs(x - Cx) < 2f || Math.Abs(x - (Cx - 56f)) < 1.5f || Math.Abs(x - (Cx + 56f)) < 1.5f || Math.Abs(y - SpringY) < 1.5f);
                    if (!frame && !mullion) continue;

                    float light = frame ? (x < Cx ? 0.55f : -0.1f) : 0.2f;
                    c.Set(x, y, ArtKit.Tone(Silver, light));
                }
            }

            c.Fill((int)(Cx - HalfW - 12f), (int)SillY, (int)(HalfW * 2f + 24f), 6, Silver[2]);
            c.Fill((int)(Cx - HalfW - 12f), (int)SillY + 6, (int)(HalfW * 2f + 24f), 4, Silver[0]);

            // A pedra de fecho, no alto do arco.
            ArtKit.Polygon(c, new[] { (Cx - 10f, ArchTop - 6f), (Cx + 10f, ArchTop - 6f), (Cx + 6f, ArchTop + 14f), (Cx - 6f, ArchTop + 14f) },
                (x, y) => ArtKit.Tone(Silver, x < Cx ? 0.6f : 0f));

            ArtKit.AddOutline(c, Outline);
            return c;
        }

        /// <summary>
        /// Os veios de quartzo, em branco (o jogo tinge): sobem do Coracao pelo
        /// vidro, passam o peitoril e descem a parede em leque ate o pe do trono.
        /// </summary>
        public static PixelCanvas Veins()
        {
            var c = new PixelCanvas(Size, Size);

            for (int v = 0; v < 5; v++)
            {
                float spread = (v - 2) * 18f;
                float x = HeartX + spread * 0.2f;
                float wobble = 3f + v;

                for (float y = HeartY + HeartR * 0.5f; y < Size - 1; y += 1f)
                {
                    float k = (y - HeartY) / (Size - HeartY);
                    float px = HeartX + spread * k * 2.4f + (float)Math.Sin(y * 0.09f + v * 1.7f) * wobble;
                    int ix = (int)px;
                    int iy = (int)y;

                    // Os veios sao veios de verdade so na parede (abaixo do peitoril) e,
                    // dentro do vidro, um fio fino no fundo da cratera.
                    byte a = y < SillY ? (byte)140 : (byte)255;
                    c.Set(ix, iy, new Rgba(255, 255, 255, a));
                    if (y >= SillY) c.Set(ix + 1, iy, new Rgba(220, 230, 255, 200));

                    // Brotos de cristal pelo caminho.
                    if (y >= SillY && ArtKit.Hash(v, iy, 8201) % 23 == 0)
                    {
                        int side = ArtKit.Hash(v, iy, 8202) % 2 == 0 ? -1 : 1;
                        for (int t = 1; t < 5; t++) c.Set(ix + side * t, iy - t, new Rgba(255, 255, 255, (byte)(220 - t * 30)));
                    }
                }
            }

            return c;
        }
    }
}
