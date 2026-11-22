using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O material de trabalho dos operarios do latao, que ficou largado onde
    /// estava no dia em que o vapor ficou verde: caixotes de peca da Coroa,
    /// caixas de ferramenta abertas, paletes de cano novo que nunca foram
    /// instalados, os montes de carvao das fornalhas com a pa fincada, sacos
    /// de areia de vedacao e o carrinho de carvao tombado.
    ///
    /// Tudo assenta na linha de baixo do desenho (nada flutua quando o pivo
    /// fica no canto de baixo). Desenhado pixel a pixel, y cresce pra baixo,
    /// sem UnityEngine.
    /// </summary>
    public static partial class BrassArt
    {
        private static readonly Rgba Ink = new Rgba(24, 18, 22);
        private static readonly Rgba[] PlankRamp =
            { new Rgba(58, 38, 28), new Rgba(96, 64, 40), new Rgba(138, 96, 58), new Rgba(176, 132, 84) };
        private static readonly Rgba[] CoalRamp =
            { new Rgba(18, 16, 20), new Rgba(34, 30, 36), new Rgba(56, 52, 60), new Rgba(96, 92, 104) };
        private static readonly Rgba[] BurlapRamp =
            { new Rgba(84, 66, 44), new Rgba(124, 100, 66), new Rgba(160, 134, 92), new Rgba(196, 172, 128) };
        private static readonly Rgba[] RedRamp =
            { new Rgba(70, 24, 24), new Rgba(120, 40, 34), new Rgba(168, 62, 46), new Rgba(206, 98, 70) };

        /// <summary>Caixote de peca da Coroa, 16x16: tabuas, cantoneiras de latao, a coroa a estencil.</summary>
        public static PixelCanvas WorkCrate()
        {
            var c = new PixelCanvas(16, 16);
            for (int py = 1; py < 16; py++)
            {
                for (int px = 0; px < 16; px++)
                {
                    bool seam = (py - 1) % 5 == 4;
                    float grain = ArtKit.Noise(px * 0.5f, py * 2f, 611 + (py - 1) / 5) - 0.5f;
                    float light = 0.4f - (py - 1) * 0.04f + grain * 0.5f - (px > 12 ? 0.6f : 0f);
                    c.Set(px, py, seam ? PlankRamp[0] : ArtKit.Tone(PlankRamp, light));
                }
            }

            // As cantoneiras e os pregos.
            foreach ((int x, int y) in new[] { (0, 1), (13, 1), (0, 13), (13, 13) })
            {
                c.Fill(x, y, 3, 3, x == 0 ? BrassMid : BrassShade);
                c.Set(x + (x == 0 ? 0 : 2), y + (y == 1 ? 0 : 2), x == 0 ? BrassShine : BrassMid);
            }

            // A coroa a estencil, desbotada.
            var paint = new Rgba(222, 210, 180, 200);
            foreach (int x in new[] { 5, 8, 11 }) c.Blend(x, 6, paint);
            for (int x = 5; x <= 11; x++)
            {
                c.Blend(x, 7, paint);
                c.Blend(x, 8, paint);
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>
        /// Caixa de ferramentas aberta, 16x16 (com a tampa levantada): uma
        /// chave, um alicate e um martelo aparecendo, o trinco solto.
        /// </summary>
        public static PixelCanvas Toolbox()
        {
            var c = new PixelCanvas(16, 16);

            // A tampa aberta, inclinada pra tras.
            for (int i = 0; i < 4; i++)
            {
                for (int px = 2 + i; px < 14 - i; px++) c.Set(px, 4 + i, ArtKit.Tone(RedRamp, 0.3f - i * 0.25f));
            }

            // O que esta dentro.
            ArtKit.Line(c, 4, 9, 7, 5, IronLight);
            ArtKit.Line(c, 5, 9, 8, 5, IronMid);
            c.Fill(7, 4, 3, 2, IronLight);
            ArtKit.Line(c, 10, 9, 12, 5, new Rgba(120, 80, 50));
            c.Fill(11, 4, 3, 2, IronMid);

            // A caixa.
            for (int py = 9; py < 16; py++)
            {
                for (int px = 1; px < 15; px++)
                {
                    float light = 0.6f - (py - 9) * 0.12f - (px > 12 ? 0.5f : 0f);
                    c.Set(px, py, ArtKit.Tone(RedRamp, light));
                }
            }

            for (int px = 1; px < 15; px++) c.Set(px, 9, RedRamp[3]);
            c.Fill(7, 11, 2, 2, BrassLight);
            c.Set(7, 11, BrassShine);

            // Ferrugem e o verde que respingou.
            c.Set(3, 13, RedRamp[0]);
            c.Set(12, 14, Verdigris);
            c.Set(12, 15, VerdigrisDark);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>
        /// Palete de cano novo, 32x16: seis canos de latao ainda brilhando,
        /// amarrados com cinta, sobre o estrado de madeira. Nunca instalados.
        /// </summary>
        public static PixelCanvas PipePallet()
        {
            var c = new PixelCanvas(32, 16);

            // O estrado.
            for (int px = 0; px < 32; px++)
            {
                c.Set(px, 12, ArtKit.Tone(PlankRamp, 0.7f));
                c.Set(px, 13, PlankRamp[1]);
            }

            foreach (int x in new[] { 1, 14, 28 }) c.Fill(x, 14, 3, 2, PlankRamp[x == 28 ? 0 : 1]);

            // Os canos, vistos de lado, empilhados em dois andares.
            foreach ((int y, int x0, int x1) in new[] { (9, 0, 32), (6, 1, 31), (3, 3, 29) })
            {
                for (int px = x0; px < x1; px++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        float ny = (k - 1f) / 1.4f;
                        c.Set(px, y + k, Shade(BrassRamp, Lit(0f, ny) + 0.1f));
                    }
                }

                // A boca do cano, escura.
                c.Fill(x1 - 2, y, 2, 3, BrassDark);
                c.Set(x1 - 2, y + 1, Outline);
            }

            // As cintas de amarrar.
            foreach (int x in new[] { 7, 22 })
            {
                for (int py = 3; py < 12; py++) c.Set(x, py, py % 2 == 0 ? IronLight : IronMid);
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>
        /// Monte de carvao, 32x16, encostado: os pedacos com brilho de quina,
        /// umas brasas ainda vermelhas embaixo e a pa fincada no alto.
        /// </summary>
        public static PixelCanvas CoalPile()
        {
            var c = new PixelCanvas(32, 16);
            for (int px = 0; px < 32; px++)
            {
                float u = (px + 0.5f - 16f) / 16f;
                int top = (int)(15f - 10f * (float)Math.Pow(Math.Max(0f, 1f - u * u), 0.8) + ArtKit.Noise(px * 0.6f, 1f, 621) * 2f);
                for (int py = Math.Max(0, top); py < 16; py++)
                {
                    float n = ArtKit.Noise(px * 0.9f, py * 0.9f, 622);
                    float light = 0.5f - (py - top) * 0.15f + (n - 0.5f) * 1.2f;
                    c.Set(px, py, ArtKit.Tone(CoalRamp, light));
                    if (n > 0.82f && py > top + 1) c.Set(px, py, CoalRamp[3]);
                }
            }

            // Brasas.
            foreach ((int x, int y) in new[] { (9, 14), (20, 13), (24, 15) })
            {
                c.Set(x, y, new Rgba(214, 90, 40));
                c.Set(x + 1, y, new Rgba(160, 50, 30));
            }

            // A pa, fincada: o cabo e a lamina.
            ArtKit.Line(c, 19, 0, 17, 7, new Rgba(130, 90, 56));
            ArtKit.Line(c, 20, 0, 18, 7, new Rgba(96, 64, 40));
            c.Fill(15, 7, 5, 3, IronLight);
            c.Fill(15, 9, 5, 1, IronMid);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Saco de areia de vedacao, 16x16: juta amarrada, uma costura, a areia vazando.</summary>
        public static PixelCanvas Sack()
        {
            var c = new PixelCanvas(16, 16);
            ArtKit.Ball(c, 8f, 10.5f, 7f, 5.5f, BurlapRamp, 0.1f);
            for (int py = 13; py < 16; py++)
            {
                for (int px = 1; px < 15; px++)
                {
                    if (c.Get(px, py).A > 0 || py == 15) c.Set(px, py, ArtKit.Tone(BurlapRamp, -0.2f - (py - 13) * 0.2f));
                }
            }

            // A boca amarrada.
            c.Fill(6, 3, 4, 3, BurlapRamp[2]);
            c.Fill(6, 5, 4, 1, new Rgba(90, 60, 40));
            c.Set(5, 2, BurlapRamp[3]);
            c.Set(10, 2, BurlapRamp[1]);

            // A trama e a costura.
            for (int px = 3; px < 13; px += 2) c.Set(px, 10, BurlapRamp[1]);
            for (int py = 7; py < 14; py += 2) c.Set(11, py, BurlapRamp[0]);

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>
        /// Carrinho de carvao de mao, 32x32, tombado de lado: a cacamba de
        /// ferro rebitada, a roda pra cima, os cabos, e o carvao derramado.
        /// </summary>
        public static PixelCanvas CoalCart()
        {
            var c = new PixelCanvas(32, 32);

            // O carvao derramado no chao.
            for (int px = 12; px < 32; px++)
            {
                int top = 29 - (int)(3f * Math.Max(0f, 1f - Math.Abs(px - 24f) / 8f));
                for (int py = top; py < 32; py++)
                {
                    float n = ArtKit.Noise(px * 0.9f, py * 0.9f, 631);
                    c.Set(px, py, ArtKit.Tone(CoalRamp, 0.3f - (py - top) * 0.2f + (n - 0.5f)));
                }
            }

            // A cacamba tombada (de lado: a boca aberta pra direita).
            var bucket = new[] { (2f, 12f), (16f, 10f), (18f, 31f), (2f, 31f) };
            ArtKit.Polygon(c, bucket, (px, py) => ArtKit.Tone(ArtKit.Iron, 0.5f - (py - 10f) * 0.05f - (px - 2f) * 0.03f));
            for (int py = 12; py < 31; py++) c.Set(2, py, ArtKit.Iron[3]);
            for (int i = 0; i < 5; i++) ArtKit.Rivet(c, 4, 14 + i * 4, ArtKit.Iron);
            ArtKit.Polygon(c, new[] { (14f, 13f), (17f, 11f), (18f, 30f), (15f, 30f) }, (px, py) => CoalRamp[0]);

            // A roda, pra cima, e os cabos.
            ArtKit.Ball(c, 9f, 6f, 5f, 5f, ArtKit.Rubber, 0.1f);
            ArtKit.Ball(c, 9f, 6f, 1.5f, 1.5f, ArtKit.Iron, 0.3f);
            ArtKit.Line(c, 3, 11, 0, 3, PlankRamp[2]);
            ArtKit.Line(c, 4, 11, 1, 3, PlankRamp[1]);

            ArtKit.AddOutline(c, Ink);
            return c;
        }
    }
}
