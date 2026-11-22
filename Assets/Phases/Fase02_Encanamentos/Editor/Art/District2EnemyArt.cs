using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Os quatro inimigos do District 2 no mesmo detalhe do Scrap (32 pixels
    /// por tile, sombreado em quatro tons, contorno escuro), cada um com o
    /// seu ataque, o tranco de apanhar e a morte. Olham pra direita.
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    internal static class D2
    {
        public static readonly Rgba[] Acid =
            { new Rgba(70, 120, 26), new Rgba(124, 184, 36), new Rgba(190, 232, 66), new Rgba(238, 255, 168) };

        public static readonly Rgba[] Fire =
            { new Rgba(176, 60, 20), new Rgba(236, 120, 30), new Rgba(255, 186, 70), new Rgba(255, 244, 196) };

        public static readonly Rgba[] RedPaint =
            { new Rgba(80, 26, 26), new Rgba(134, 42, 36), new Rgba(178, 66, 52), new Rgba(214, 108, 84) };

        public static readonly Rgba[] Gauge =
            { new Rgba(150, 140, 110), new Rgba(192, 182, 152), new Rgba(224, 216, 190), new Rgba(242, 238, 218) };

        public static readonly Rgba[] Glass =
            { new Rgba(40, 60, 64), new Rgba(70, 100, 104), new Rgba(120, 160, 160), new Rgba(200, 230, 228) };

        public static readonly Rgba SteamLight = new Rgba(240, 244, 240);
        public static readonly Rgba SteamDark = new Rgba(186, 196, 194);

        public static void Steam(PixelCanvas c, float x, float y, float r, int alpha)
        {
            ArtKit.Puff(c, x, y, r, SteamLight, SteamDark, alpha);
        }

        /// <summary>Um ponto numa curva de Bezier quadratica.</summary>
        public static (float X, float Y) Bezier((float X, float Y) a, (float X, float Y) b, (float X, float Y) c, float t)
        {
            float u = 1f - t;
            return (u * u * a.X + 2f * u * t * b.X + t * t * c.X, u * u * a.Y + 2f * u * t * b.Y + t * t * c.Y);
        }

        /// <summary>Um cone de fogo de um ponto, numa direcao: miolo branco, borda laranja, pontas soltas.</summary>
        public static void Flame(PixelCanvas c, float x, float y, float angle, float length, float width, int seed)
        {
            if (length <= 0.5f) return;
            double a = angle * Math.PI / 180.0;
            float ux = (float)Math.Cos(a);
            float uy = (float)Math.Sin(a);

            for (int py = (int)(y - length - width) - 2; py <= (int)(y + length + width) + 2; py++)
            {
                for (int px = (int)(x - length - width) - 2; px <= (int)(x + length + width) + 2; px++)
                {
                    float rx = px + 0.5f - x;
                    float ry = py + 0.5f - y;
                    float along = (rx * ux + ry * uy) / length;
                    if (along < 0f || along > 1f) continue;

                    float across = Math.Abs(-rx * uy + ry * ux);
                    float half = width * (0.35f + 0.65f * (float)Math.Sin(Math.Min(1f, along * 1.6f) * Math.PI * 0.5)) * (1f - along * 0.55f);
                    half += (ArtKit.Noise(along * 9f, seed, seed) - 0.5f) * 1.6f;
                    if (across > half) continue;

                    float k = across / Math.Max(0.5f, half) + along * 0.7f;
                    Rgba color = k < 0.45f ? Fire[3] : (k < 0.8f ? Fire[2] : (k < 1.15f ? Fire[1] : Fire[0]));
                    int alpha = along > 0.8f ? 170 : 245;
                    c.Blend(px, py, color.WithAlpha(alpha));
                }
            }
        }
    }

    // ======================================================================
    // Pipe Leech
    // ======================================================================

    // ======================================================================
    // Valve Sentry
    // ======================================================================

    // ======================================================================
    // Brass Welder
    // ======================================================================

    // ======================================================================
    // Acid Bloater
    // ======================================================================
}
