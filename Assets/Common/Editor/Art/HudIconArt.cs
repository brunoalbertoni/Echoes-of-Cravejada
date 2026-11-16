using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Os icones do Scrap: a vida (um coracao de sucata, enferrujado e
    /// rebitado, com a brasa da vida aparecendo pela rachadura), o fragmento
    /// (uma pepita azul, irregular, com brilhos) e a gema (um rubi lapidado em
    /// brilhante).
    ///
    /// O HUD usa as versoes de 16 px (desenhadas em dobro); o mapa usa as
    /// pequenas (o fragmento solto e a gema lapidada). Nao depende de
    /// UnityEngine: da pra renderizar pelo console.
    /// </summary>
    public static class HudIconArt
    {
        private static readonly Rgba Outline = new Rgba(24, 14, 12);

        private static readonly Rgba[] Rust =
            { new Rgba(70, 32, 22), new Rgba(122, 58, 34), new Rgba(168, 92, 50), new Rgba(206, 136, 82) };

        private static readonly Rgba[] Iron =
            { new Rgba(46, 42, 50), new Rgba(76, 72, 82), new Rgba(112, 108, 118), new Rgba(158, 154, 160) };

        private static readonly Rgba[] Spent =
            { new Rgba(30, 26, 30), new Rgba(48, 42, 46), new Rgba(66, 58, 60), new Rgba(88, 78, 78) };

        private static readonly Rgba[] Blue =
            { new Rgba(20, 38, 92), new Rgba(36, 76, 168), new Rgba(70, 136, 230), new Rgba(150, 210, 255) };

        private static readonly Rgba[] Ruby =
            { new Rgba(70, 6, 20), new Rgba(130, 14, 34), new Rgba(196, 28, 48), new Rgba(244, 84, 92) };

        private static readonly Rgba Glint = new Rgba(255, 250, 240);
        private static readonly Rgba Ember = new Rgba(255, 120, 60);
        private static readonly Rgba EmberHot = new Rgba(255, 214, 130);

        // ------------------------------------------------------------------
        // A vida
        // ------------------------------------------------------------------

        /// <summary>O formato do coracao: 14 colunas por 12 linhas, comecando em (1, 2). '#' e chapa.</summary>
        private static readonly string[] HeartMask =
        {
            "..###....###..",
            ".#####..#####.",
            "##############",
            "##############",
            "##############",
            "##############",
            ".############.",
            "..##########..",
            "...########...",
            "....######....",
            ".....####.....",
            "......##......",
        };

        /// <summary>
        /// O coracao de sucata, 16x16: duas chapas enferrujadas soldadas no
        /// meio, um remendo de lata aparafusado e rebites. Cheio, a brasa da
        /// vida brilha pela rachadura; gasto, e ferro morto, apagado e
        /// lascado.
        /// </summary>
        public static PixelCanvas Heart(bool full)
        {
            var canvas = new PixelCanvas(16, 16);
            Rgba[] ramp = full ? Rust : Spent;

            for (int row = 0; row < HeartMask.Length; row++)
            {
                for (int col = 0; col < HeartMask[row].Length; col++)
                {
                    if (HeartMask[row][col] != '#') continue;

                    int x = col + 1;
                    int y = row + 2;

                    // Cada lobo estufado como uma chapa batida: luz de cima a esquerda.
                    float cx = x < 8 ? 4.5f : 11.5f;
                    float nx = (x + 0.5f - cx) / 4.5f;
                    float ny = (y + 0.5f - 6f) / 6f;
                    float light = -0.65f * nx - 0.75f * ny + 0.05f;

                    // Manchas de ferrugem mais escura, espalhadas.
                    int stain = (x * 5 + y * 3 + x * y) % 9;
                    if (stain == 0) light -= 0.55f;
                    else if (stain == 4 && full) light += 0.35f;

                    canvas.Set(x, y, ArtKit.Tone(ramp, light));
                }
            }

            // A costura soldada descendo do V do meio ate a ponta, torta.
            int[] seam = { 8, 8, 7, 7, 8, 8, 7, 8 };
            for (int i = 0; i < seam.Length; i++) canvas.Set(seam[i], 4 + i, ramp[0]);

            // O remendo de lata, aparafusado no lobo da direita.
            Rgba[] patch = full ? Iron : Spent;
            canvas.Fill(10, 4, 3, 3, patch[2]);
            canvas.Fill(10, 4, 3, 1, patch[3]);
            canvas.Fill(12, 5, 1, 2, patch[1]);
            canvas.Set(10, 4, patch[3]);

            // Rebites nas chapas.
            canvas.Set(3, 5, patch[3]);
            canvas.Set(3, 6, ramp[0]);
            canvas.Set(5, 10, patch[3]);
            canvas.Set(5, 11, ramp[0]);
            canvas.Set(11, 9, patch[3]);
            canvas.Set(11, 10, ramp[0]);

            if (full)
            {
                // A rachadura com a brasa da vida.
                (int X, int Y)[] crack = { (5, 7), (6, 7), (6, 8), (5, 9) };
                foreach (var (x, y) in crack) canvas.Set(x, y, Ember);
                canvas.Set(6, 8, EmberHot);

                // O brilho da chapa, na curva do lobo da esquerda.
                canvas.Set(3, 3, Rust[3].Scaled(1.15f));
                canvas.Set(4, 3, Rust[3]);
                canvas.Set(2, 4, Rust[3]);
            }
            else
            {
                // Gasto: a rachadura e so um rasgo escuro, e falta uma lasca no lobo.
                (int X, int Y)[] crack = { (5, 7), (6, 7), (6, 8), (5, 9), (6, 9), (7, 10) };
                foreach (var (x, y) in crack) canvas.Set(x, y, new Rgba(14, 10, 12));
                canvas.Set(12, 2, Rgba.Clear);
                canvas.Set(13, 3, Rgba.Clear);
                canvas.Set(14, 4, Rgba.Clear);
            }

            AddOutline(canvas);
            return canvas;
        }

        // ------------------------------------------------------------------
        // O fragmento
        // ------------------------------------------------------------------

        /// <summary>A pepita azul do HUD, 12x12: dois caroços grudados, com brilho e fendas.</summary>
        public static PixelCanvas Nugget() => Nugget(12, 12, 0);

        /// <summary>
        /// Uma pepita azul de <paramref name="width"/> x <paramref name="height"/>:
        /// um caroço principal e um menor grudado, sombreados como pedra
        /// polida, com fendas escuras e um ou dois pontos de brilho.
        /// </summary>
        public static PixelCanvas Nugget(int width, int height, int seed)
        {
            var canvas = new PixelCanvas(width, height);

            // O caroço grande embaixo a esquerda, o pequeno em cima a direita.
            float ax = width * 0.44f, ay = height * 0.56f, ar = Math.Min(width, height) * 0.33f;
            float bx = width * 0.65f, by = height * 0.34f, br = Math.Min(width, height) * 0.23f;

            // Uma borda de um pixel fica livre pro contorno.
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    float px = x + 0.5f;
                    float py = y + 0.5f;

                    // Borda irregular: o raio varia com o angulo.
                    float da = Lumpy(px - ax, py - ay, ar, seed);
                    float db = Lumpy(px - bx, py - by, br, seed + 3);
                    if (da > 1f && db > 1f) continue;

                    // Sombreado pelo caroço mais perto.
                    bool big = da <= db;
                    float cx = big ? ax : bx;
                    float cy = big ? ay : by;
                    float r = big ? ar : br;
                    float nx = (px - cx) / r;
                    float ny = (py - cy) / r;
                    float light = -0.7f * nx - 0.7f * ny + 0.25f * (float)Math.Sqrt(Math.Max(0f, 1f - nx * nx - ny * ny));

                    // Onde os dois se encostam, a fenda.
                    if (Math.Abs(da - db) < 0.18f && da < 1f && db < 1f) light = -1f;

                    canvas.Set(x, y, ArtKit.Tone(Blue, light));
                }
            }

            // Os brilhos: um ponto branco em cada caroço, em cima a esquerda.
            Spark(canvas, (int)(ax - ar * 0.45f), (int)(ay - ar * 0.45f));
            if (width >= 10) Spark(canvas, (int)(bx - br * 0.4f), (int)(by - br * 0.4f));

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>A distancia "torta" ate o centro (1 = na borda): uma pedra, nao um circulo.</summary>
        private static float Lumpy(float dx, float dy, float radius, int seed)
        {
            double angle = Math.Atan2(dy, dx);
            double wobble = 1.0 + 0.12 * Math.Sin(angle * 3 + seed) + 0.07 * Math.Sin(angle * 5 + seed * 2);
            return (float)(Math.Sqrt(dx * dx + dy * dy) / (radius * wobble));
        }

        private static void Spark(PixelCanvas canvas, int x, int y)
        {
            if (canvas.Get(x, y).A == 0) return;
            canvas.Set(x, y, Glint);
            if (canvas.Get(x + 1, y).A > 0) canvas.Set(x + 1, y, Blue[3]);
        }

        // ------------------------------------------------------------------
        // A gema
        // ------------------------------------------------------------------

        /// <summary>
        /// O rubi lapidado em brilhante, 12x12: a mesa clara em cima, as
        /// facetas da coroa alternando, o pavilhao afunilando em leque ate a
        /// ponta, e uma estrela de brilho.
        /// </summary>
        public static PixelCanvas RedGem()
        {
            var canvas = new PixelCanvas(12, 12);

            // A coroa: linhas 2 a 4. A mesa no meio, as facetas dos lados.
            canvas.Fill(4, 2, 4, 1, Ruby[3]);
            canvas.Fill(3, 3, 6, 1, Ruby[2]);
            canvas.Fill(4, 3, 4, 1, Ruby[3]);
            canvas.Set(3, 2, Ruby[2]);
            canvas.Set(8, 2, Ruby[1]);
            canvas.Fill(1, 4, 10, 1, Ruby[2]);
            canvas.Set(1, 4, Ruby[1]);
            canvas.Set(10, 4, Ruby[0]);
            canvas.Set(2, 3, Ruby[2]);
            canvas.Set(9, 3, Ruby[1]);

            // A cintura: uma linha clara de lado a lado.
            canvas.Fill(1, 5, 10, 1, Ruby[3]);
            canvas.Set(10, 5, Ruby[1]);

            // O pavilhao: afunila ate a ponta, em facetas de leque (claro, escuro, claro...).
            for (int row = 0; row < 5; row++)
            {
                int y = 6 + row;
                int half = 5 - row;
                for (int x = 6 - half; x < 6 + half; x++)
                {
                    int facet = (x - (6 - half)) * 4 / Math.Max(1, half * 2);
                    Rgba[] tones = { Ruby[2], Ruby[1], Ruby[2], Ruby[0] };
                    canvas.Set(x, y, tones[Math.Min(3, facet)]);
                }
            }

            canvas.Set(5, 10, Ruby[1]);
            canvas.Set(6, 10, Ruby[0]);

            // As linhas das facetas, saindo da ponta.
            canvas.Set(4, 7, Ruby[3]);
            canvas.Set(5, 8, Ruby[3]);
            canvas.Set(8, 7, Ruby[0]);

            // A estrela de brilho na mesa.
            canvas.Set(5, 2, Glint);
            canvas.Set(5, 1, Glint.WithAlpha(200));
            canvas.Set(4, 2, Glint.WithAlpha(220));
            canvas.Set(6, 2, Ruby[3]);
            canvas.Set(5, 3, Glint.WithAlpha(200));

            AddOutline(canvas);
            return canvas;
        }

        // ------------------------------------------------------------------
        // Contas
        // ------------------------------------------------------------------

        /// <summary>O contorno escuro de um pixel em volta de tudo que foi pintado.</summary>
        private static void AddOutline(PixelCanvas canvas)
        {
            var solid = new bool[canvas.Pixels.Length];
            for (int i = 0; i < solid.Length; i++) solid[i] = canvas.Pixels[i].A > 0;

            for (int y = 0; y < canvas.Height; y++)
            {
                for (int x = 0; x < canvas.Width; x++)
                {
                    if (solid[x + y * canvas.Width]) continue;

                    bool edge = (canvas.Inside(x - 1, y) && solid[x - 1 + y * canvas.Width])
                        || (canvas.Inside(x + 1, y) && solid[x + 1 + y * canvas.Width])
                        || (canvas.Inside(x, y - 1) && solid[x + (y - 1) * canvas.Width])
                        || (canvas.Inside(x, y + 1) && solid[x + (y + 1) * canvas.Width]);

                    if (edge) canvas.Set(x, y, Outline);
                }
            }
        }
    }
}
