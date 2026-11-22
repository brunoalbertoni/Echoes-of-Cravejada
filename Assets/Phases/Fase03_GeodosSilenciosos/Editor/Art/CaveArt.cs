using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O que se pega e se usa no District 3: a CHAVE ANTIGA (o Capataz a
    /// carrega na argola do cinto) e a fechadura da grade do Salao das
    /// Estatuas. (Os bichos estao no District3EnemyArt; o Capataz, no
    /// CaveMinerArt.)
    ///
    /// Nao depende de UnityEngine.
    /// </summary>
    public static class CaveArt
    {
        private static readonly Rgba Mouth = new Rgba(18, 12, 22);

        // ------------------------------------------------------------------
        // A CHAVE ANTIGA e a fechadura
        // ------------------------------------------------------------------

        /// <summary>A CHAVE ANTIGA, 16x9: ferro velho, argola trabalhada, dois dentes, ferrugem.</summary>
        public static PixelCanvas AncientKey()
        {
            var c = new PixelCanvas(16, 9);

            // A argola, com um furo.
            ArtKit.Ball(c, 4f, 4.5f, 3.6f, 3.6f, ArtKit.Iron, 0.3f);
            c.Fill(3, 4, 2, 2, Rgba.Clear);
            c.Set(4, 3, Rgba.Clear);

            // A haste e os dentes.
            for (int x = 7; x < 15; x++)
            {
                c.Set(x, 4, ArtKit.Iron[3]);
                c.Set(x, 5, ArtKit.Iron[1]);
            }

            c.Fill(12, 6, 1, 2, ArtKit.Iron[2]);
            c.Fill(14, 6, 1, 3, ArtKit.Iron[2]);

            // Ferrugem.
            c.Set(9, 5, ArtKit.Rust[1]);
            c.Set(2, 6, ArtKit.Rust[1]);
            c.Set(6, 2, ArtKit.Rust[2]);

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>A fechadura da grade, 12x16: uma chapa de ferro rebitada com o buraco da chave.</summary>
        public static PixelCanvas Lock()
        {
            var c = new PixelCanvas(12, 16);

            for (int y = 1; y < 15; y++)
            {
                for (int x = 1; x < 11; x++)
                {
                    float light = (x < 3 ? 0.7f : (x > 8 ? -0.6f : 0f)) + (y < 3 ? 0.4f : 0f);
                    c.Set(x, y, ArtKit.Tone(ArtKit.Iron, light));
                }
            }

            ArtKit.Rivet(c, 2, 2, ArtKit.Iron);
            ArtKit.Rivet(c, 8, 2, ArtKit.Iron);
            ArtKit.Rivet(c, 2, 12, ArtKit.Iron);
            ArtKit.Rivet(c, 8, 12, ArtKit.Iron);

            // O buraco da chave.
            c.Fill(5, 5, 2, 2, Mouth);
            c.Fill(5, 7, 2, 3, Mouth);
            c.Set(4, 6, Mouth);
            c.Set(7, 6, Mouth);

            c.Set(3, 9, ArtKit.Rust[1]);
            c.Set(9, 5, ArtKit.Rust[2]);

            ArtKit.AddOutline(c);
            return c;
        }
    }
}
