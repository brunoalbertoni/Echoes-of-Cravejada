using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O que os mineiros largaram nos Geodos Silenciosos quando a broca nao
    /// precisou mais deles: picaretas encostadas, sacos de minerio que nunca
    /// subiram, lamparinas apagadas (ninguem acende luz onde nao se pode fazer
    /// barulho), geodos cortados ao meio mostrando o miolo, estalagmites e os
    /// montinhos de pedregulho da escavacao.
    ///
    /// Tudo assenta na linha de baixo do desenho (pivo no canto de baixo,
    /// nada flutua). Desenhado pixel a pixel, y cresce pra baixo, sem
    /// UnityEngine.
    /// </summary>
    public static class MineDressArt
    {
        public const string Pickaxe = "mine_pickaxe";
        public const string OreSack = "mine_ore_sack";
        public const string Lantern = "mine_lantern";
        public const string CutGeode = "mine_cut_geode";
        public const string Stalagmite = "mine_stalagmite";
        public const string Rubble = "mine_rubble";

        /// <summary>O tamanho de cada peca, em tiles (largura, altura).</summary>
        public static (int W, int H) Size(string name)
        {
            switch (name)
            {
                case Pickaxe: return (1, 2);
                case OreSack: return (1, 1);
                case Lantern: return (1, 1);
                case CutGeode: return (2, 1);
                case Stalagmite: return (1, 2);
                case Rubble: return (2, 1);
                default: return (1, 1);
            }
        }

        public static (string Name, PixelCanvas Canvas)[] All() => new[]
        {
            (Pickaxe, DrawPickaxe()),
            (OreSack, DrawOreSack()),
            (Lantern, DrawLantern()),
            (CutGeode, DrawCutGeode()),
            (Stalagmite, DrawStalagmite()),
            (Rubble, DrawRubble()),
        };

        private static readonly Rgba Ink = new Rgba(18, 14, 26);
        private static readonly Rgba[] Wood =
            { new Rgba(58, 38, 28), new Rgba(96, 64, 40), new Rgba(138, 96, 58), new Rgba(176, 132, 84) };
        private static readonly Rgba[] Stone =
            { new Rgba(30, 26, 42), new Rgba(52, 46, 70), new Rgba(80, 72, 104), new Rgba(118, 108, 146) };
        private static readonly Rgba[] Amethyst =
            { new Rgba(70, 40, 110), new Rgba(120, 70, 170), new Rgba(170, 120, 220), new Rgba(222, 190, 250) };
        private static readonly Rgba[] Burlap =
            { new Rgba(70, 56, 40), new Rgba(110, 90, 62), new Rgba(150, 126, 88), new Rgba(186, 164, 124) };

        /// <summary>Picareta encostada na parede, 16x32: o cabo inclinado e a cabeca de ferro no alto.</summary>
        private static PixelCanvas DrawPickaxe()
        {
            var c = new PixelCanvas(16, 32);
            ArtKit.Limb(c, 4f, 31f, 10f, 6f, 1f, Wood, 0.2f);
            ArtKit.Taper(c, 10f, 6f, 2f, 3f, 1.6f, 0.4f, ArtKit.Plain(ArtKit.Iron), 0.3f);
            ArtKit.Taper(c, 10f, 6f, 15f, 9f, 1.6f, 0.4f, ArtKit.Plain(ArtKit.Iron), 0.1f);
            c.Fill(9, 5, 3, 3, ArtKit.Iron[3]);
            c.Fill(3, 30, 3, 2, Wood[0]);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Saco de minerio, 16x16: juta amarrada com pontas de ametista furando o pano.</summary>
        private static PixelCanvas DrawOreSack()
        {
            var c = new PixelCanvas(16, 16);
            ArtKit.Ball(c, 8f, 10.5f, 7f, 5.5f, Burlap, 0.1f);
            for (int px = 1; px < 15; px++)
            {
                if (c.Get(px, 15).A == 0) c.Set(px, 15, Burlap[0]);
            }

            c.Fill(6, 3, 4, 3, Burlap[2]);
            c.Fill(6, 5, 4, 1, Wood[1]);
            foreach ((int x, int y) in new[] { (4, 8), (11, 9), (8, 12) })
            {
                c.Set(x, y, Amethyst[3]);
                c.Set(x + 1, y, Amethyst[1]);
                c.Set(x, y + 1, Amethyst[2]);
            }

            for (int px = 3; px < 13; px += 3) c.Set(px, 11, Burlap[0]);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Lamparina de mineiro, 16x16, apagada: a alca, o vidro escuro e frio, a base amassada.</summary>
        private static PixelCanvas DrawLantern()
        {
            var c = new PixelCanvas(16, 16);
            for (int i = 0; i < 10; i++)
            {
                double a = Math.PI + i * Math.PI / 9.0;
                c.Set((int)Math.Round(8 + Math.Cos(a) * 3.5), (int)Math.Round(4 + Math.Sin(a) * 3.5), ArtKit.Iron[2]);
            }

            c.Fill(5, 4, 6, 2, ArtKit.Brass[2]);
            for (int py = 6; py < 13; py++)
            {
                for (int px = 5; px < 11; px++)
                {
                    bool bar = px == 5 || px == 10 || px == 8;
                    c.Set(px, py, bar ? ArtKit.Tone(ArtKit.Brass, px == 5 ? 0.7f : 0f) : new Rgba(46, 52, 70));
                }
            }

            c.Set(6, 7, new Rgba(140, 150, 180));
            c.Fill(4, 13, 8, 3, ArtKit.Brass[1]);
            c.Fill(4, 13, 8, 1, ArtKit.Brass[3]);
            c.Set(10, 14, ArtKit.Brass[0]);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>
        /// Um geodo cortado ao meio, 32x16, largado de lado: a casca de pedra
        /// por fora e o miolo de ametista por dentro — o que os mineiros vinham
        /// buscar.
        /// </summary>
        private static PixelCanvas DrawCutGeode()
        {
            var c = new PixelCanvas(32, 16);
            for (int py = 0; py < 16; py++)
            {
                for (int px = 0; px < 32; px++)
                {
                    float dx = (px + 0.5f - 16f) / 15f;
                    float dy = (py + 0.5f - 16f) / 14f;
                    float d = dx * dx + dy * dy;
                    if (d > 1f) continue;
                    float n = ArtKit.Noise(px * 0.7f, py * 0.7f, 951);
                    if (d > 0.62f) c.Set(px, py, ArtKit.Tone(Stone, 0.4f - dy * 0.4f + (n - 0.5f)));
                    else if (d > 0.5f) c.Set(px, py, Stone[3]);
                    else c.Set(px, py, ArtKit.Tone(Amethyst, 0.2f + (n - 0.5f) * 1.6f - d));
                }
            }

            // Pontas de cristal no miolo.
            foreach (int x in new[] { 10, 14, 18, 22 })
            {
                c.Set(x, 9, Amethyst[3]);
                c.Set(x, 10, Amethyst[2]);
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Estalagmite, 16x32: pedra pingada, mais clara na quina de cima.</summary>
        private static PixelCanvas DrawStalagmite()
        {
            var c = new PixelCanvas(16, 32);
            ArtKit.Taper(c, 8f, 31f, 7f, 6f, 6f, 0.8f, (x, y, light) => ArtKit.Tone(Stone, light), 0.2f);
            ArtKit.Taper(c, 12f, 31f, 12.5f, 20f, 2.5f, 0.6f, (x, y, light) => ArtKit.Tone(Stone, light - 0.2f), 0.2f);
            for (int py = 10; py < 30; py += 5) c.Set(6, py, Stone[3]);
            c.Fill(1, 31, 14, 1, Stone[0]);
            ArtKit.AddOutline(c, Ink);
            return c;
        }

        /// <summary>Montinho de pedregulho da escavacao, 32x16, com uma lasca de cristal.</summary>
        private static PixelCanvas DrawRubble()
        {
            var c = new PixelCanvas(32, 16);
            var rng = new Random(961);
            for (int i = 0; i < 14; i++)
            {
                float x = 4f + (float)rng.NextDouble() * 24f;
                float r = 1.5f + (float)rng.NextDouble() * 2.5f;
                float y = 15.5f - r * 0.8f - (float)Math.Max(0, 5 - Math.Abs(x - 16f) / 3f);
                ArtKit.Ball(c, x, y, r, r * 0.8f, Stone, 0.15f);
            }

            c.Set(17, 7, Amethyst[3]);
            c.Set(17, 8, Amethyst[1]);
            c.Set(18, 8, Amethyst[2]);
            for (int px = 2; px < 30; px++)
            {
                if (c.Get(px, 15).A == 0 && c.Get(px, 14).A > 0) c.Set(px, 15, Stone[0]);
            }

            ArtKit.AddOutline(c, Ink);
            return c;
        }
    }
}
