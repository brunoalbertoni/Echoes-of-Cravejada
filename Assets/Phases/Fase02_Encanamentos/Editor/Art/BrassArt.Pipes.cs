using System;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class BrassArt
    {
        // ------------------------------------------------------------------
        // Canos de enfeite
        // ------------------------------------------------------------------







        /// <summary>
        /// A folha de canos, 16x4. Em cada material, a primeira linha tem as
        /// 16 combinacoes de lados (a coluna e a propria mascara: reto, curva,
        /// T, cruz, ponta) e a segunda tem pecas especiais: flange, registro,
        /// manometro, vazamento e bracadeira.
        /// </summary>
        public static PixelCanvas BuildPipes()
        {
            var sheet = new PixelCanvas(PipeSheetColumns * Tile, PipeSheetRows * Tile);

            BuildPipeSet(sheet, BrassPipeRow, BrassRamp, false);
            BuildPipeSet(sheet, CopperPipeRow, CopperRamp, true);

            return sheet;
        }

        private static void BuildPipeSet(PixelCanvas sheet, int row, Rgba[] ramp, bool aged)
        {
            for (int mask = 0; mask < 16; mask++)
            {
                PipePiece(sheet, mask * Tile, row * Tile, mask, ramp);
                if (aged) Age(sheet, mask * Tile, row * Tile, 500 + mask);
            }

            int sy = (row + 1) * Tile;
            const int across = PipeEast | PipeWest;
            const int up = PipeNorth | PipeSouth;

            PipePiece(sheet, PipeFlangeAcross * Tile, sy, across, ramp);
            Flange(sheet, PipeFlangeAcross * Tile, sy, true, ramp);

            PipePiece(sheet, PipeFlangeUp * Tile, sy, up, ramp);
            Flange(sheet, PipeFlangeUp * Tile, sy, false, ramp);

            // Registro: uma haste saindo do cano com um volantinho vermelho.
            PipePiece(sheet, PipeValve * Tile, sy, across, ramp);
            {
                int ox = PipeValve * Tile;
                sheet.Fill(ox + 7, sy + 2, 2, 3, IronMid);
                sheet.Fill(ox + 4, sy, 8, 2, DialRed);
                sheet.Fill(ox + 4, sy + 1, 8, 1, new Rgba(140, 34, 30));
                sheet.Set(ox + 4, sy, new Rgba(240, 110, 90));
            }

            // Manometro pequeno em cima do cano.
            PipePiece(sheet, PipeGauge * Tile, sy, across, ramp);
            {
                int ox = PipeGauge * Tile;
                Disc(sheet, ox + 8f, sy + 3.5f, 3.6f, BrassShade);
                Disc(sheet, ox + 8f, sy + 3.5f, 2.6f, DialFace);
                sheet.Set(ox + 9, sy + 2, DialRed);
                sheet.Set(ox + 9, sy + 3, DialRed);
                sheet.Set(ox + 8, sy + 3, NeedleDark);
            }

            // Vazamento: uma rachadura escura e o azinhavre que escorreu dela.
            PipePiece(sheet, PipeLeakAcross * Tile, sy, across, ramp);
            {
                int ox = PipeLeakAcross * Tile;
                sheet.Set(ox + 6, sy + 8, Outline);
                sheet.Set(ox + 7, sy + 9, Outline);
                sheet.Set(ox + 8, sy + 9, Outline);
                sheet.Set(ox + 9, sy + 10, Outline);
                Stain(sheet, ox + 8, sy + 10, 2, 610, ox, sy + 4, Tile, 8);
                Drip(sheet, ox + 8, sy + 12, 4, 611);
            }

            PipePiece(sheet, PipeLeakUp * Tile, sy, up, ramp);
            {
                int ox = PipeLeakUp * Tile;
                sheet.Set(ox + 9, sy + 5, Outline);
                sheet.Set(ox + 10, sy + 6, Outline);
                sheet.Set(ox + 10, sy + 7, Outline);
                Stain(sheet, ox + 9, sy + 8, 2, 620, ox + 4, sy, 8, Tile);
                Drip(sheet, ox + 10, sy + 8, 7, 621);
            }

            // Bracadeira: a cinta que prende o cano na parede.
            PipePiece(sheet, PipeStrapAcross * Tile, sy, across, ramp);
            {
                int ox = PipeStrapAcross * Tile;
                for (int py = 3; py <= 12; py++)
                {
                    sheet.Set(ox + 7, sy + py, py < 6 ? IronLight : IronMid);
                    sheet.Set(ox + 8, sy + py, IronDark);
                }

                sheet.Set(ox + 7, sy + 2, IronMid);
                sheet.Set(ox + 8, sy + 13, IronDark);
            }

            PipePiece(sheet, PipeStrapUp * Tile, sy, up, ramp);
            {
                int ox = PipeStrapUp * Tile;
                for (int px = 3; px <= 12; px++)
                {
                    sheet.Set(ox + px, sy + 7, px < 6 ? IronLight : IronMid);
                    sheet.Set(ox + px, sy + 8, IronDark);
                }
            }

            if (!aged) return;

            for (int piece = 0; piece < 8; piece++)
            {
                Age(sheet, piece * Tile, sy, 700 + piece);
            }
        }

        private static void PipePiece(PixelCanvas sheet, int ox, int oy, int mask, Rgba[] ramp)
        {
            bool north = (mask & PipeNorth) != 0;
            bool east = (mask & PipeEast) != 0;
            bool south = (mask & PipeSouth) != 0;
            bool west = (mask & PipeWest) != 0;

            int count = (north ? 1 : 0) + (east ? 1 : 0) + (south ? 1 : 0) + (west ? 1 : 0);

            if (count == 0)
            {
                // Sem ligacao nenhuma: so uma tampa aparafusada na parede.
                Fitting(sheet, ox + 4, oy + 4, 8, ramp);
                return;
            }

            bool straight = (north && south && count == 2) || (east && west && count == 2);
            if (count == 2 && !straight)
            {
                Elbow(sheet, ox, oy, east, south, ramp);
                return;
            }

            if (west) ArmAcross(sheet, ox, oy, 0, 7, ramp);
            if (east) ArmAcross(sheet, ox, oy, 8, 15, ramp);
            if (north) ArmUp(sheet, ox, oy, 0, 7, ramp);
            if (south) ArmUp(sheet, ox, oy, 8, 15, ramp);

            // Onde tres ou quatro canos se encontram vai uma conexao quadrada,
            // que e como juncao de verdade costuma ser.
            if (count >= 3) Fitting(sheet, ox + 2, oy + 2, 12, ramp);

            // Ponta solta: uma tampa na extremidade.
            if (count == 1)
            {
                if (west || east)
                {
                    // O braco vai da borda ate o meio; a tampa fecha logo depois.
                    int capX = west ? 8 : 6;
                    for (int py = 3; py <= 12; py++)
                    {
                        sheet.Set(ox + capX, oy + py, py == 3 ? ramp[3] : ramp[2]);
                        sheet.Set(ox + capX + 1, oy + py, py == 12 ? ramp[0] : ramp[1]);
                    }
                }
                else
                {
                    int capY = north ? 8 : 6;
                    for (int px = 3; px <= 12; px++)
                    {
                        sheet.Set(ox + px, oy + capY, px == 3 ? ramp[3] : ramp[2]);
                        sheet.Set(ox + px, oy + capY + 1, px == 12 ? ramp[0] : ramp[1]);
                    }
                }
            }
        }

        private static void ArmAcross(PixelCanvas sheet, int ox, int oy, int from, int to, Rgba[] ramp)
        {
            int start = (Tile - PipeThickness) / 2;

            for (int px = from; px <= to; px++)
            {
                for (int i = 0; i < PipeThickness; i++)
                {
                    float across = (i + 0.5f - PipeThickness * 0.5f) / (PipeThickness * 0.5f);
                    sheet.Set(ox + px, oy + start + i, Shade(ramp, Lit(0f, across)));
                }
            }
        }

        private static void ArmUp(PixelCanvas sheet, int ox, int oy, int from, int to, Rgba[] ramp)
        {
            int start = (Tile - PipeThickness) / 2;

            for (int py = from; py <= to; py++)
            {
                for (int i = 0; i < PipeThickness; i++)
                {
                    float across = (i + 0.5f - PipeThickness * 0.5f) / (PipeThickness * 0.5f);
                    sheet.Set(ox + start + i, oy + py, Shade(ramp, Lit(across, 0f)));
                }
            }
        }

        /// <summary>
        /// Uma curva de 90 graus. O centro da curva e o canto do tile entre os
        /// dois lados ligados, e o sombreamento usa a mesma conta dos trechos
        /// retos — por isso a emenda com o tile vizinho nao aparece.
        /// </summary>
        private static void Elbow(PixelCanvas sheet, int ox, int oy, bool east, bool south, Rgba[] ramp)
        {
            float cx = east ? Tile : 0f;
            float cy = south ? Tile : 0f;
            float radius = Tile * 0.5f;
            float half = PipeThickness * 0.5f;

            for (int py = 0; py < Tile; py++)
            {
                for (int px = 0; px < Tile; px++)
                {
                    float dx = px + 0.5f - cx;
                    float dy = py + 0.5f - cy;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (Math.Abs(distance - radius) >= half) continue;

                    float across = (distance - radius) / half;
                    sheet.Set(ox + px, oy + py,
                        Shade(ramp, Lit(dx / distance * across, dy / distance * across)));
                }
            }
        }

        /// <summary>Uma conexao quadrada chanfrada, com um parafuso em cada canto.</summary>
        private static void Fitting(PixelCanvas sheet, int x, int y, int size, Rgba[] ramp)
        {
            for (int py = 0; py < size; py++)
            {
                for (int px = 0; px < size; px++)
                {
                    Rgba color = ramp[2];

                    if (py == 0 || px == 0) color = ramp[3];
                    if (py == size - 1 || px == size - 1) color = ramp[0];
                    if (py == size - 2 && px > 0 && px < size - 1) color = ramp[1];
                    if (px == size - 2 && py > 0 && py < size - 1) color = ramp[1];

                    sheet.Set(x + px, y + py, color);
                }
            }

            sheet.Set(x, y, ramp[4]);

            foreach (int bx in new[] { 2, size - 4 })
            {
                foreach (int by in new[] { 2, size - 4 })
                {
                    sheet.Set(x + bx, y + by, ramp[4]);
                    sheet.Set(x + bx + 1, y + by + 1, ramp[0]);
                }
            }
        }

        /// <summary>O colar de uma flange, por cima de um trecho reto.</summary>
        private static void Flange(PixelCanvas sheet, int ox, int oy, bool across, Rgba[] ramp)
        {
            const int width = 4;
            const int thickness = 12;
            int start = (Tile - thickness) / 2;
            int band = (Tile - width) / 2;

            for (int i = 0; i < thickness; i++)
            {
                float offset = (i + 0.5f - thickness * 0.5f) / (thickness * 0.5f);

                for (int j = -1; j <= width; j++)
                {
                    bool edge = j < 0 || j >= width;
                    Rgba color = edge
                        ? ramp[0]
                        : Shade(ramp, (across ? Lit(0f, offset) : Lit(offset, 0f)) + 0.08f);

                    // A sombra de cada lado do colar so existe na altura do cano.
                    if (edge && (i < 2 || i >= thickness - 2)) continue;

                    if (across) sheet.Set(ox + band + j, oy + start + i, color);
                    else sheet.Set(ox + start + i, oy + band + j, color);
                }
            }

            if (across)
            {
                sheet.Set(ox + band + 1, oy + start + 2, ramp[4]);
                sheet.Set(ox + band + 2, oy + start + thickness - 3, ramp[0]);
            }
            else
            {
                sheet.Set(ox + start + 2, oy + band + 1, ramp[4]);
                sheet.Set(ox + start + thickness - 3, oy + band + 2, ramp[0]);
            }
        }

        /// <summary>Envelhece um tile de cano: manchas de azinhavre em blocos de 2x2.</summary>
        private static void Age(PixelCanvas sheet, int ox, int oy, int seed)
        {
            for (int py = 0; py < Tile; py++)
            {
                for (int px = 0; px < Tile; px++)
                {
                    Rgba pixel = sheet.Get(ox + px, oy + py);
                    if (pixel.A == 0) continue;
                    if (Hash(px >> 1, py >> 1, seed) % 6 != 0) continue;

                    int brightness = pixel.R + pixel.G + pixel.B;
                    Rgba green = brightness > 430 ? VerdigrisLight : (brightness > 250 ? Verdigris : VerdigrisDark);
                    sheet.Set(ox + px, oy + py, green);
                }
            }
        }

        // ------------------------------------------------------------------
        // Acido
        // ------------------------------------------------------------------





        /// <summary>
        /// A folha do acido, 5x5. As quatro primeiras linhas sao os quadros da
        /// superficie: e a animacao de agua do pack (water_animated.png, 80x64),
        /// com a forma das ondas intacta e as cores trocadas pelas do acido.
        /// A ultima linha e o corpo do liquido.
        /// </summary>
        public static PixelCanvas BuildAcid(PixelCanvas water)
        {
            var sheet = new PixelCanvas(AcidWaveColumns * Tile, (AcidFrames + 1) * Tile);

            for (int py = 0; py < AcidFrames * Tile && py < water.Height; py++)
            {
                for (int px = 0; px < sheet.Width && px < water.Width; px++)
                {
                    Rgba pixel = water.Get(px, py);
                    if (pixel.A == 0) continue;

                    // A agua do pack tem quatro tons; cada um vira um do acido.
                    int brightness = pixel.R + pixel.G + pixel.B;
                    Rgba color;

                    if (pixel.A < 255) color = BodyPixel(px, py, 31);
                    else if (brightness > 560) color = AcidFoam;
                    else if (brightness > 480) color = AcidLight;
                    else if (brightness > 320) color = AcidMid;
                    else color = AcidDeep;

                    sheet.Set(px, py, color);
                }
            }

            for (int variant = 0; variant < AcidBodyVariants; variant++)
            {
                int ox = variant * Tile;
                int oy = AcidBodyRow * Tile;

                for (int py = 0; py < Tile; py++)
                {
                    for (int px = 0; px < Tile; px++)
                    {
                        sheet.Set(ox + px, oy + py, BodyPixel(px, py, 40 + variant));
                    }
                }

                // Uma bolha parada em cada variacao, pra o corpo nao ser liso.
                int bx = 3 + Hash(variant, 1, 9) % 9;
                int by = 3 + Hash(variant, 2, 9) % 9;
                sheet.Set(ox + bx, oy + by, AcidLight);
                sheet.Set(ox + bx + 1, oy + by, AcidMid);
                sheet.Set(ox + bx, oy + by + 1, AcidMid);

                if (variant == 3) sheet.Set(ox + bx + 4, oy + by - 2, AcidFoam);
            }

            return sheet;
        }

        private static Rgba BodyPixel(int x, int y, int seed)
        {
            // Tracos curtos e raros: o bastante pra o liquido nao ser chapado,
            // sem virar camuflagem.
            int roll = Hash(x / 3, y, seed) % 16;
            Rgba color = roll == 0 ? AcidDark : (roll == 1 ? AcidMid : AcidDeep);

            // Quase opaco: da pra adivinhar o fundo do tanque, mas pouco.
            return color.WithAlpha(232);
        }
    }
}
