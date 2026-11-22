using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// As cores que os corrompidos do District 4 dividem: o mercurio, a prata,
    /// o vidro, o jaleco e os frascos coloridos.
    /// </summary>
    internal static class D4
    {
        public static readonly Rgba[] Mercury = LabArt.Mercury;
        public static readonly Rgba[] Silver = LabArt.Silver;

        public static readonly Rgba[] Coat =
            { new Rgba(126, 130, 122), new Rgba(176, 180, 168), new Rgba(214, 216, 204), new Rgba(242, 242, 232) };

        public static readonly Rgba[] Glass =
            { new Rgba(70, 100, 132), new Rgba(130, 170, 200), new Rgba(190, 222, 240), new Rgba(244, 252, 255) };

        public static readonly Rgba[] Thread =
            { new Rgba(60, 40, 30), new Rgba(110, 80, 50), new Rgba(170, 130, 80), new Rgba(220, 190, 130) };

        public static readonly Rgba[] Trousers =
            { new Rgba(34, 32, 40), new Rgba(56, 52, 64), new Rgba(82, 78, 92), new Rgba(112, 108, 122) };

        public static readonly Rgba[] Oxide =
            { LabArt.OxideDark, new Rgba(70, 200, 126), LabArt.OxideGreen, new Rgba(200, 255, 220) };

        /// <summary>O vidro da sentinela: ciano, bem mais vivo que o dos frascos.</summary>
        public static readonly Rgba[] ArmorGlass =
            { new Rgba(46, 112, 142), new Rgba(96, 186, 214), new Rgba(164, 232, 246), new Rgba(240, 255, 255) };

        /// <summary>A prata polida da sentinela transmutada: clara, quase branca.</summary>
        public static readonly Rgba[] Polished =
            { new Rgba(92, 94, 110), new Rgba(160, 164, 178), new Rgba(222, 226, 236), new Rgba(255, 255, 255) };

        public static readonly Rgba[] Eye =
            { new Rgba(150, 156, 170), new Rgba(210, 214, 222), new Rgba(240, 242, 246), new Rgba(255, 255, 255) };

        /// <summary>As cores dos frascos da bandoleira: verde, azul, rosa, ambar.</summary>
        public static readonly Rgba[] Potions = { LabArt.OxideGreen, LabArt.Azure, LabArt.Rose, LabArt.Amber };

        public static readonly Rgba Ink = ArtKit.Outline;
        public static readonly Rgba Dark = new Rgba(24, 20, 28);

        /// <summary>Um frasquinho de vidro com o liquido de uma cor e a rolha.</summary>
        public static void Vial(PixelCanvas c, float x, float y, Rgba liquid, float size = 1f)
        {
            float r = 2.2f * size;
            ArtKit.Ball(c, x, y, r, r + 0.3f, Glass, 0.2f);
            for (int py = (int)y; py <= (int)(y + r); py++)
            {
                for (int px = (int)(x - r); px <= (int)(x + r); px++)
                {
                    if (c.Get(px, py).A > 0) c.Set(px, py, py == (int)y ? Rgba.Lerp(liquid, new Rgba(255, 255, 255), 0.4f) : liquid);
                }
            }

            c.Fill((int)x - 1, (int)(y - r - 2f), 2, 2, Glass[1]);
            c.Fill((int)x - 1, (int)(y - r - 3f), 2, 1, ArtKit.Leather[2]);
            c.Set((int)(x - r * 0.5f), (int)(y - r * 0.4f), Glass[3]);
        }

        /// <summary>Brilhinhos de vidro: pontinhos claros espalhados em volta de um ponto.</summary>
        public static void Sparkle(PixelCanvas c, float cx, float cy, float spread, int count, int seed, int alpha)
        {
            for (int i = 0; i < count; i++)
            {
                int x = (int)(cx - spread + ArtKit.Hash(i, seed, 7003) % (int)(spread * 2f + 1f));
                int y = (int)(cy - spread * 0.6f + ArtKit.Hash(i, seed, 7004) % (int)(spread * 1.2f + 1f));
                c.Blend(x, y, Glass[3].WithAlpha(alpha));
            }
        }

        public static void Splash(PixelCanvas c, float x, float y, float r, Rgba color, int seed, int count)
        {
            for (int i = 0; i < count; i++)
            {
                double a = Math.PI + (ArtKit.Hash(i, seed, 7001) % 100) / 100.0 * Math.PI;
                float d = r * (0.4f + (ArtKit.Hash(i, seed, 7002) % 60) / 100f);
                int px = (int)Math.Round(x + Math.Cos(a) * d);
                int py = (int)Math.Round(y + Math.Sin(a) * d * 0.7);
                c.Set(px, py, color);
                if (i % 2 == 0) c.Set(px + 1, py, color);
            }
        }
    }

    // ======================================================================
    // Gosma de Mercurio
    // ======================================================================

    // ======================================================================
    // Arremessador de Frascos
    // ======================================================================

    // ======================================================================
    // Experimento Costurado
    // ======================================================================

    // ======================================================================
    // Sentinela de Vidro
    // ======================================================================
}
