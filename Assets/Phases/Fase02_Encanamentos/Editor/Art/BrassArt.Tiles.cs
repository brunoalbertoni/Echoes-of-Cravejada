using System;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class BrassArt
    {
        // ------------------------------------------------------------------
        // Tileset de chapa de latao
        // ------------------------------------------------------------------








        /// <summary>
        /// A folha de tiles do terreno, 8x6, no mesmo arranjo de uma banda do
        /// cave_tileset — por isso o LevelMap escolhe os tiles das duas do
        /// mesmo jeito.
        ///
        ///   linhas 0-3: bordas, quinas, faces e (colunas 0-3 das linhas 2-3)
        ///               oito variacoes de miolo
        ///   linha 4:    grade (colunas 0-3) e cano de pisar (colunas 4-7)
        ///   linha 5:    parede do fundo
        /// </summary>
        public static PixelCanvas BuildTiles()
        {
            var sheet = new PixelCanvas(TileSheetColumns * Tile, TileSheetRows * Tile);

            for (int variant = 0; variant < 4; variant++)
            {
                Wall(sheet, variant, 0, true, false, false, false, InnerNone, variant);
                Wall(sheet, variant, 1, false, true, false, false, InnerNone, variant);
                Wall(sheet, 6, variant, false, false, false, true, InnerNone, variant);
                Wall(sheet, 7, variant, false, false, true, false, InnerNone, variant);

                Core(sheet, variant, 2, variant);
                Core(sheet, variant, 3, variant + 4);
            }

            Wall(sheet, 4, 0, true, false, false, true, InnerNone, 0);
            Wall(sheet, 5, 0, true, false, true, false, InnerNone, 0);
            Wall(sheet, 4, 1, false, true, false, true, InnerNone, 0);
            Wall(sheet, 5, 1, false, true, true, false, InnerNone, 0);

            Wall(sheet, 4, 2, false, false, false, false, InnerTopLeft, 0);
            Wall(sheet, 5, 2, false, false, false, false, InnerTopRight, 0);
            Wall(sheet, 4, 3, false, false, false, false, InnerBottomLeft, 0);
            Wall(sheet, 5, 3, false, false, false, false, InnerBottomRight, 0);

            for (int part = 0; part < 4; part++)
            {
                GrateTile(sheet, part, ThinRow, part);
                PipeFloorTile(sheet, 4 + part, ThinRow, part);
            }

            for (int variant = 0; variant < 8; variant++)
            {
                BackTile(sheet, variant, BackRow, variant);
            }

            return sheet;
        }

        private static Rgba PlatePixel(int x, int y, int seed)
        {
            int roll = Hash(x, y, seed) % 20;
            if (roll == 0) return PlateLight;
            if (roll <= 2) return PlateDark;
            return PlateBase;
        }

        private static void Wall(PixelCanvas sheet, int column, int row,
            bool top, bool bottom, bool left, bool right, int inner, int variant)
        {
            int ox = column * Tile;
            int oy = row * Tile;
            int seed = column * 31 + row * 17 + 5;

            bool innerLeft = inner == InnerTopLeft || inner == InnerBottomLeft;
            bool innerTop = inner == InnerTopLeft || inner == InnerTopRight;

            for (int py = 0; py < Tile; py++)
            {
                for (int px = 0; px < Tile; px++)
                {
                    // A viga acompanha a fronteira com o ar: cada pixel pega o
                    // perfil do lado aberto que estiver mais perto. Numa quina
                    // pra fora isso faz as duas vigas se encontrarem em esquadria.
                    int best = 99;
                    int side = -1;

                    if (top) { best = py; side = SideTop; }
                    if (left && px < best) { best = px; side = SideLeft; }
                    if (right && Tile - 1 - px < best) { best = Tile - 1 - px; side = SideRight; }
                    if (bottom && Tile - 1 - py < best) { best = Tile - 1 - py; side = SideBottom; }

                    if (inner != InnerNone)
                    {
                        // Quina pra dentro: o ar so encosta num canto, entao a
                        // distancia e ate esse canto, e as vigas dos dois
                        // vizinhos se encontram num quadrado.
                        int dx = innerLeft ? px : Tile - 1 - px;
                        int dy = innerTop ? py : Tile - 1 - py;
                        int distance = Math.Max(dx, dy);

                        if (distance < best)
                        {
                            best = distance;
                            side = dy >= dx
                                ? (innerTop ? SideTop : SideBottom)
                                : (innerLeft ? SideLeft : SideRight);
                        }
                    }

                    Rgba color;
                    if (best < Beam) color = BeamProfile[side][best];
                    else if (best == Beam && side == SideTop) color = PlateLight;
                    else color = PlatePixel(px, py, seed);

                    sheet.Set(ox + px, oy + py, color);
                }
            }

            // Rebites ao longo das vigas, menos onde duas se cruzam.
            if (top)
            {
                if (!left) Rivet(sheet, ox + 3, oy + 2);
                if (!right) Rivet(sheet, ox + 11, oy + 2);
            }

            if (bottom)
            {
                if (!left) Rivet(sheet, ox + 3, oy + 12, true);
                if (!right) Rivet(sheet, ox + 11, oy + 12, true);
            }

            if (left)
            {
                if (!top) Rivet(sheet, ox + 2, oy + 3);
                if (!bottom) Rivet(sheet, ox + 2, oy + 11);
            }

            if (right)
            {
                if (!top) Rivet(sheet, ox + 12, oy + 3, true);
                if (!bottom) Rivet(sheet, ox + 12, oy + 11, true);
            }

            // Azinhavre: o verde-cobre que escorre das juntas. So em algumas
            // variacoes, pra nao virar padrao.
            bool single = (top ? 1 : 0) + (bottom ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0) == 1;
            if (!single) return;

            if (top && variant == 1) Drip(sheet, ox + 6, oy + 4, 7, seed);
            if (top && variant == 3)
            {
                Drip(sheet, ox + 12, oy + 5, 4, seed);
                Drip(sheet, ox + 2, oy + 6, 3, seed + 3);
            }

            if (bottom && variant == 2) Drip(sheet, ox + 8, oy + 9, 5, seed);
            if (left && variant == 1) Stain(sheet, ox + 5, oy + 7, 3, seed, ox, oy, Tile, Tile);
            if (right && variant == 2) Stain(sheet, ox + 10, oy + 9, 3, seed, ox, oy, Tile, Tile);
            if (left && variant == 3) Drip(sheet, ox + 4, oy + 2, 9, seed);
            if (right && variant == 0) Drip(sheet, ox + 11, oy + 5, 6, seed);
        }

        /// <summary>
        /// O miolo da parede. Oito variacoes: a maioria e chapa lisa, e algumas
        /// tem painel, emenda, veneziana ou mancha — o bastante pra uma parede
        /// grande nao parecer papel de parede.
        /// </summary>
        private static void Core(PixelCanvas sheet, int column, int row, int variant)
        {
            int ox = column * Tile;
            int oy = row * Tile;
            int seed = 200 + variant * 13;

            for (int py = 0; py < Tile; py++)
            {
                for (int px = 0; px < Tile; px++)
                {
                    Rgba color = PlatePixel(px, py, seed);
                    if (variant == 7) color = color.Scaled(0.84f);
                    sheet.Set(ox + px, oy + py, color);
                }
            }

            switch (variant)
            {
                case 1:
                    // Painel emoldurado, com um rebite em cada canto.
                    for (int i = 1; i <= 14; i++)
                    {
                        sheet.Set(ox + i, oy + 1, PlateLight);
                        sheet.Set(ox + 1, oy + i, PlateLight);
                        sheet.Set(ox + i, oy + 14, PlateSeam);
                        sheet.Set(ox + 14, oy + i, PlateSeam);
                    }

                    foreach (int cx in new[] { 3, 12 })
                    {
                        foreach (int cy in new[] { 3, 12 })
                        {
                            sheet.Set(ox + cx, oy + cy, BrassShade);
                            sheet.Set(ox + cx, oy + cy - 1, BrassMid);
                        }
                    }

                    break;

                case 2:
                    // Emenda deitada, rebitada.
                    for (int px = 0; px < Tile; px++)
                    {
                        sheet.Set(ox + px, oy + 7, PlateSeam);
                        sheet.Set(ox + px, oy + 8, PlateLight);
                    }

                    foreach (int cx in new[] { 2, 7, 12 })
                    {
                        sheet.Set(ox + cx, oy + 5, BrassShade);
                        sheet.Set(ox + cx, oy + 10, BrassShade);
                    }

                    break;

                case 3:
                    Stain(sheet, ox + 8, oy + 8, 5, seed, ox, oy, Tile, Tile);
                    break;

                case 4:
                    // Veneziana: tres frestas escuras com o labio de baixo iluminado.
                    foreach (int cy in new[] { 4, 8, 12 })
                    {
                        for (int px = 3; px <= 12; px++)
                        {
                            sheet.Set(ox + px, oy + cy, PlateSeam);
                            sheet.Set(ox + px, oy + cy + 1, BrassDark);
                        }
                    }

                    break;

                case 5:
                    // Travessa em diagonal.
                    for (int i = 0; i < Tile; i++)
                    {
                        sheet.Set(ox + i, oy + 15 - i, PlateLight);
                        if (i > 0) sheet.Set(ox + i, oy + 16 - i, PlateSeam);
                    }

                    break;

                case 6:
                    // Emenda em pe.
                    for (int py = 0; py < Tile; py++)
                    {
                        sheet.Set(ox + 7, oy + py, PlateSeam);
                        sheet.Set(ox + 8, oy + py, PlateLight);
                    }

                    foreach (int cy in new[] { 3, 12 })
                    {
                        sheet.Set(ox + 5, oy + cy, BrassShade);
                        sheet.Set(ox + 10, oy + cy, BrassShade);
                    }

                    break;
            }
        }

        /// <summary>Piso de grade: um trilho em cima, barras em pe e vao entre elas.</summary>
        private static void GrateTile(PixelCanvas sheet, int column, int row, int part)
        {
            int ox = column * Tile;
            int oy = row * Tile;

            for (int px = 0; px < Tile; px++)
            {
                sheet.Set(ox + px, oy, BrassLight);
                sheet.Set(ox + px, oy + 1, BrassMid);
                sheet.Set(ox + px, oy + 6, BrassDark);

                // Barras de 2 px a cada 4: o resto fica vazado.
                if (px % 4 >= 2) continue;

                for (int py = 2; py <= 5; py++)
                {
                    sheet.Set(ox + px, oy + py, px % 4 == 0 ? BrassShade : BrassDark);
                }
            }

            // As pontas ganham uma cantoneira, pra a grade nao acabar no ar.
            if (part == 0 || part == 3)
            {
                int bx = part == 0 ? 0 : Tile - 3;

                for (int py = 0; py <= 9; py++)
                {
                    sheet.Set(ox + bx, oy + py, BrassMid);
                    sheet.Set(ox + bx + 1, oy + py, BrassShade);
                    sheet.Set(ox + bx + 2, oy + py, BrassDark);
                }

                sheet.Set(ox + bx, oy, BrassShine);
                sheet.Set(ox + bx + 1, oy + 4, BrassLight);
            }
        }

        /// <summary>
        /// Cano de pisar: um cano deitado de 12 px, com o topo rente ao topo do
        /// tile pra o Scrap apoiar o pe nele. As pontas tem flange.
        /// </summary>
        private static void PipeFloorTile(PixelCanvas sheet, int column, int row, int part)
        {
            int ox = column * Tile;
            int oy = row * Tile;
            const int thickness = 12;

            for (int px = 0; px < Tile; px++)
            {
                for (int py = 0; py < thickness; py++)
                {
                    float across = (py + 0.5f - thickness * 0.5f) / (thickness * 0.5f);
                    sheet.Set(ox + px, oy + py, Shade(BrassRamp, Lit(0f, across)));
                }

                sheet.Set(ox + px, oy + thickness - 1, Outline);
            }

            if (part == 1)
            {
                // Uma cinta no meio.
                for (int py = 0; py < thickness; py++)
                {
                    sheet.Set(ox + 7, oy + py, BrassDark);
                    sheet.Set(ox + 8, oy + py, py < 4 ? BrassShine : BrassShade);
                }
            }

            if (part == 2)
            {
                Stain(sheet, ox + 9, oy + 7, 3, 77, ox, oy + 3, Tile, thickness - 4);
                Drip(sheet, ox + 9, oy + thickness, 3, 78);
            }

            if (part == 0 || part == 3)
            {
                // Flange: um colar mais grosso, com a tampa na ponta.
                int fx = part == 0 ? 0 : Tile - 5;

                for (int px = 0; px < 5; px++)
                {
                    for (int py = 0; py < thickness + 2; py++)
                    {
                        float across = (py + 0.5f - (thickness + 2) * 0.5f) / ((thickness + 2) * 0.5f);
                        Rgba color = Shade(BrassRamp, Lit(0f, across) + 0.08f);

                        bool edge = px == 0 || px == 4;
                        sheet.Set(ox + fx + px, oy + py, edge ? BrassDark : color);
                    }
                }

                sheet.Set(ox + fx + 2, oy + 3, BrassShine);
                sheet.Set(ox + fx + 2, oy + 9, BrassDark);

                int capX = part == 0 ? 0 : Tile - 1;
                for (int py = 0; py < thickness + 2; py++) sheet.Set(ox + capX, oy + py, Outline);
            }
        }

        /// <summary>
        /// Parede do fundo. As quatro primeiras sao a mesma chapa com emenda a
        /// direita e/ou embaixo: o layout combina as quatro pra formar paineis
        /// maiores que um tile. As outras quatro sao detalhes avulsos.
        /// </summary>
        private static void BackTile(PixelCanvas sheet, int column, int row, int variant)
        {
            int ox = column * Tile;
            int oy = row * Tile;
            int seed = 400 + variant * 7;

            for (int py = 0; py < Tile; py++)
            {
                for (int px = 0; px < Tile; px++)
                {
                    int roll = Hash(px, py, seed) % 24;
                    Rgba color = roll == 0 ? BackLight : (roll <= 2 ? BackDark : BackBase);
                    if (variant == 7) color = color.Scaled(0.8f);
                    sheet.Set(ox + px, oy + py, color);
                }
            }

            bool seamRight = variant == 1 || variant == 3;
            bool seamBottom = variant == 2 || variant == 3;

            if (seamRight)
            {
                for (int py = 0; py < Tile; py++)
                {
                    sheet.Set(ox + 15, oy + py, BackSeam);
                    sheet.Set(ox + 14, oy + py, BackLight);
                }
            }

            if (seamBottom)
            {
                for (int px = 0; px < Tile; px++)
                {
                    sheet.Set(ox + px, oy + 15, BackSeam);
                    sheet.Set(ox + px, oy + 14, BackLight);
                }
            }

            if (variant == 3)
            {
                sheet.Set(ox + 12, oy + 12, BackLight);
                sheet.Set(ox + 12, oy + 13, BackSeam);
            }

            if (variant == 4)
            {
                // Fileira de rebites.
                foreach (int cx in new[] { 2, 7, 12 })
                {
                    sheet.Set(ox + cx, oy + 7, BackLight);
                    sheet.Set(ox + cx, oy + 8, BackSeam);
                }
            }

            if (variant == 5)
            {
                // Grelha de ventilacao.
                for (int py = 4; py <= 11; py += 2)
                {
                    for (int px = 3; px <= 12; px++)
                    {
                        sheet.Set(ox + px, oy + py, BackSeam);
                        sheet.Set(ox + px, oy + py + 1, BackLight);
                    }
                }
            }

            if (variant == 6)
            {
                // Mancha fria de azinhavre, bem apagada.
                for (int py = 3; py <= 12; py++)
                {
                    for (int px = 4; px <= 11; px++)
                    {
                        if (Hash(px, py, seed + 9) % 3 != 0) continue;
                        sheet.Set(ox + px, oy + py, new Rgba(40, 62, 60));
                    }
                }
            }
        }
    }
}
