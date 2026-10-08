using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A arte do District 2, desenhada por codigo: chapa de latao rebitada,
    /// canos, valvulas, manometros, caldeiras e acido.
    ///
    /// Os packs do projeto so tem caverna, entao nada disso existia. Tudo aqui
    /// e placeholder em pixel art de 16 px, na mesma escala do cave_tileset:
    /// quando houver arte de verdade, e so trocar os PNGs em Assets/Generated.
    ///
    /// Nao depende de UnityEngine de proposito: assim da pra gerar as imagens
    /// fora do editor e olhar antes de abrir o jogo.
    /// </summary>
    public static class BrassArt
    {
        public const int Tile = 16;

        // ------------------------------------------------------------------
        // Paleta
        // ------------------------------------------------------------------

        private static readonly Rgba BrassShine = new Rgba(246, 224, 150);
        private static readonly Rgba BrassLight = new Rgba(214, 170, 78);
        private static readonly Rgba BrassMid = new Rgba(168, 122, 48);
        private static readonly Rgba BrassShade = new Rgba(112, 76, 36);
        private static readonly Rgba BrassDark = new Rgba(62, 42, 26);
        private static readonly Rgba Outline = new Rgba(30, 22, 20);

        // A chapa do miolo das paredes: escura e quente, pra as vigas de latao
        // das bordas saltarem.
        private static readonly Rgba PlateBase = new Rgba(40, 29, 25);
        private static readonly Rgba PlateLight = new Rgba(52, 38, 30);
        private static readonly Rgba PlateDark = new Rgba(31, 23, 22);
        private static readonly Rgba PlateSeam = new Rgba(21, 16, 17);

        // A chapa da parede do fundo: mais fria e mais escura que a da frente.
        // E essa diferenca de temperatura que separa "onde eu piso" de "o que
        // esta atras".
        private static readonly Rgba BackBase = new Rgba(44, 43, 54);
        private static readonly Rgba BackLight = new Rgba(56, 56, 68);
        private static readonly Rgba BackDark = new Rgba(35, 34, 44);
        private static readonly Rgba BackSeam = new Rgba(23, 23, 30);

        private static readonly Rgba Verdigris = new Rgba(72, 150, 124);
        private static readonly Rgba VerdigrisLight = new Rgba(118, 196, 160);
        private static readonly Rgba VerdigrisDark = new Rgba(40, 92, 84);

        private static readonly Rgba CopperShine = new Rgba(244, 184, 128);
        private static readonly Rgba CopperLight = new Rgba(214, 132, 82);
        private static readonly Rgba CopperMid = new Rgba(166, 92, 56);
        private static readonly Rgba CopperShade = new Rgba(106, 56, 40);
        private static readonly Rgba CopperDark = new Rgba(60, 34, 30);

        private static readonly Rgba IronLight = new Rgba(112, 108, 116);
        private static readonly Rgba IronMid = new Rgba(74, 70, 78);
        private static readonly Rgba IronDark = new Rgba(42, 40, 48);

        private static readonly Rgba Rust = new Rgba(158, 84, 44);
        private static readonly Rgba RustLight = new Rgba(196, 118, 62);
        private static readonly Rgba RustDark = new Rgba(96, 50, 32);

        private static readonly Rgba FireCore = new Rgba(255, 236, 160);
        private static readonly Rgba FireMid = new Rgba(255, 156, 44);
        private static readonly Rgba FireDeep = new Rgba(200, 72, 20);

        private static readonly Rgba AcidFoam = new Rgba(240, 255, 170);
        private static readonly Rgba AcidLight = new Rgba(196, 236, 70);
        private static readonly Rgba AcidMid = new Rgba(134, 194, 40);
        private static readonly Rgba AcidDeep = new Rgba(78, 134, 28);
        private static readonly Rgba AcidDark = new Rgba(44, 88, 26);

        private static readonly Rgba DialFace = new Rgba(232, 224, 198);
        private static readonly Rgba DialShade = new Rgba(192, 182, 152);
        private static readonly Rgba DialRed = new Rgba(212, 58, 44);
        private static readonly Rgba DialGreen = new Rgba(92, 168, 92);
        private static readonly Rgba NeedleDark = new Rgba(38, 26, 22);

        // Rampas de sombra, do mais escuro pro mais claro. O indice sai de
        // Lit(): quanto mais a superficie olha pra luz, mais alto.
        private static readonly Rgba[] BrassRamp = { BrassDark, BrassShade, BrassMid, BrassLight, BrassShine };
        private static readonly Rgba[] CopperRamp = { CopperDark, CopperShade, CopperMid, CopperLight, CopperShine };
        private static readonly Rgba[] IronRamp = { Outline, IronDark, IronMid, IronLight, IronLight };
        private static readonly Rgba[] RustRamp = { Outline, RustDark, Rust, RustLight, RustLight };

        // ------------------------------------------------------------------
        // Ferramentas
        // ------------------------------------------------------------------

        /// <summary>Numero pseudoaleatorio estavel: o mesmo pixel sempre sorteia o mesmo valor.</summary>
        private static int Hash(int x, int y, int seed)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + seed * 1442695041;
                h = (h ^ (h >> 13)) * 1274126177;
                return (h ^ (h >> 16)) & 0x7fffffff;
            }
        }

        /// <summary>
        /// Quanto um ponto de um cilindro olha pra luz, que vem de cima e da
        /// esquerda. (nx, ny) e a posicao atraves do cano, de -1 a 1; y cresce
        /// pra baixo, como na imagem.
        /// </summary>
        private static float Lit(float nx, float ny)
        {
            float nz = (float)Math.Sqrt(Math.Max(0f, 1f - nx * nx - ny * ny));
            return -0.35f * nx - 0.6f * ny + 0.72f * nz;
        }

        private static Rgba Shade(Rgba[] ramp, float brightness)
        {
            if (brightness > 0.9f) return ramp[4];
            if (brightness > 0.75f) return ramp[3];
            if (brightness > 0.5f) return ramp[2];
            if (brightness > 0.1f) return ramp[1];
            return ramp[0];
        }

        /// <summary>Um rebite de 2x2: claro em cima, escuro embaixo.</summary>
        private static void Rivet(PixelCanvas canvas, int x, int y, bool dim = false)
        {
            canvas.Set(x, y, dim ? BrassMid : BrassShine);
            canvas.Set(x + 1, y, dim ? BrassMid : BrassLight);
            canvas.Set(x, y + 1, BrassShade);
            canvas.Set(x + 1, y + 1, BrassDark);
        }

        /// <summary>Um fio de azinhavre escorrendo pra baixo.</summary>
        private static void Drip(PixelCanvas canvas, int x, int y, int length, int seed)
        {
            int px = x;

            for (int i = 0; i < length; i++)
            {
                Rgba color = i == 0 ? VerdigrisLight : (i < length - 1 ? Verdigris : VerdigrisDark);
                canvas.Set(px, y + i, color);

                if (i == 0) canvas.Set(px + 1, y, Verdigris);

                // De vez em quando o fio desvia um pixel, senao parece regua.
                int roll = Hash(i, seed, 7) % 6;
                if (roll == 0) px--;
                else if (roll == 1) px++;
            }
        }

        /// <summary>Uma mancha irregular de azinhavre em volta de um ponto.</summary>
        private static void Stain(PixelCanvas canvas, int cx, int cy, int radius, int seed,
            int clipX, int clipY, int clipWidth, int clipHeight)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int px = cx + dx;
                    int py = cy + dy;

                    if (px < clipX || px >= clipX + clipWidth || py < clipY || py >= clipY + clipHeight) continue;

                    int wobble = Hash(px, py, seed) % 3;
                    if (dx * dx + dy * dy > (radius - wobble) * (radius - wobble)) continue;

                    canvas.Set(px, py, Hash(px, py, seed + 1) % 4 == 0 ? Verdigris : VerdigrisDark);
                }
            }
        }

        private static void Disc(PixelCanvas canvas, float cx, float cy, float radius, Rgba color)
        {
            for (int y = (int)(cy - radius) - 1; y <= (int)(cy + radius) + 1; y++)
            {
                for (int x = (int)(cx - radius) - 1; x <= (int)(cx + radius) + 1; x++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    if (dx * dx + dy * dy <= radius * radius) canvas.Set(x, y, color);
                }
            }
        }

        /// <summary>Um anel com volume: claro em cima e a esquerda, escuro embaixo e a direita.</summary>
        private static void Ring(PixelCanvas canvas, float cx, float cy, float inner, float outer, Rgba[] ramp)
        {
            for (int y = (int)(cy - outer) - 1; y <= (int)(cy + outer) + 1; y++)
            {
                for (int x = (int)(cx - outer) - 1; x <= (int)(cx + outer) + 1; x++)
                {
                    float dx = x + 0.5f - cx;
                    float dy = y + 0.5f - cy;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (distance < inner || distance > outer) continue;

                    // Trata o anel como um tubo dobrado em circulo.
                    float across = (distance - (inner + outer) * 0.5f) / ((outer - inner) * 0.5f);
                    float nx = dx / distance * across;
                    float ny = dy / distance * across;

                    canvas.Set(x, y, Shade(ramp, Lit(nx, ny)));
                }
            }
        }

        // ------------------------------------------------------------------
        // Tileset de chapa de latao
        // ------------------------------------------------------------------

        public const int TileSheetColumns = 8;
        public const int TileSheetRows = 6;

        /// <summary>Linha das plataformas: grade nas colunas 0-3, cano nas 4-7.</summary>
        public const int ThinRow = 4;

        /// <summary>Linha da parede do fundo.</summary>
        public const int BackRow = 5;

        private const int SideTop = 0;
        private const int SideBottom = 1;
        private const int SideLeft = 2;
        private const int SideRight = 3;

        private const int InnerNone = 0;
        private const int InnerTopLeft = 1;
        private const int InnerTopRight = 2;
        private const int InnerBottomLeft = 3;
        private const int InnerBottomRight = 4;

        /// <summary>Espessura da viga de latao que contorna toda parede.</summary>
        private const int Beam = 7;

        // O perfil da viga, do pixel que encosta no ar pro que encosta na chapa.
        // A de cima pega luz em cheio; a de baixo fica na sombra.
        private static readonly Rgba[][] BeamProfile =
        {
            new[] { BrassLight, BrassShine, BrassLight, BrassMid, BrassMid, BrassShade, BrassDark },
            new[] { BrassShade, BrassMid, BrassMid, BrassShade, BrassShade, BrassDark, BrassDark },
            new[] { BrassLight, BrassShine, BrassMid, BrassMid, BrassShade, BrassShade, BrassDark },
            new[] { BrassMid, BrassLight, BrassMid, BrassShade, BrassShade, BrassDark, BrassDark },
        };

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

        // ------------------------------------------------------------------
        // Canos de enfeite
        // ------------------------------------------------------------------

        public const int PipeSheetColumns = 16;
        public const int PipeSheetRows = 4;

        // Pra que lados o cano de uma celula continua.
        public const int PipeNorth = 1;
        public const int PipeEast = 2;
        public const int PipeSouth = 4;
        public const int PipeWest = 8;

        /// <summary>Linha dos canos de latao, indexada pela mascara de lados.</summary>
        public const int BrassPipeRow = 0;

        /// <summary>Linha dos canos de cobre, mais velhos e esverdeados.</summary>
        public const int CopperPipeRow = 2;

        // Colunas da linha de pecas especiais, logo abaixo de cada material.
        public const int PipeFlangeAcross = 0;
        public const int PipeFlangeUp = 1;
        public const int PipeValve = 2;
        public const int PipeGauge = 3;
        public const int PipeLeakAcross = 4;
        public const int PipeLeakUp = 5;
        public const int PipeStrapAcross = 6;
        public const int PipeStrapUp = 7;

        private const int PipeThickness = 8;

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

        /// <summary>Quantas colunas tem a onda da superficie antes de repetir.</summary>
        public const int AcidWaveColumns = 5;

        /// <summary>Quantos quadros tem a animacao da superficie.</summary>
        public const int AcidFrames = 4;

        /// <summary>Linha do corpo do acido, embaixo dos quadros da superficie.</summary>
        public const int AcidBodyRow = 4;

        public const int AcidBodyVariants = 4;

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

        // ------------------------------------------------------------------
        // Pecas avulsas
        // ------------------------------------------------------------------

        /// <summary>
        /// Volante de valvula, 32x32. Enferrujado e como ela comeca; limpo e
        /// como fica depois de raspada e polida.
        /// </summary>
        public static PixelCanvas ValveWheel(bool clean)
        {
            var canvas = new PixelCanvas(32, 32);
            Rgba[] ramp = clean ? BrassRamp : RustRamp;
            const float center = 16f;

            // Os raios primeiro, o aro por cima.
            for (int i = 4; i < 28; i++)
            {
                canvas.Set(i, 15, ramp[3]);
                canvas.Set(i, 16, ramp[1]);
                canvas.Set(15, i, ramp[3]);
                canvas.Set(16, i, ramp[1]);
            }

            Ring(canvas, center, center, 10.2f, 13.6f, ramp);

            Disc(canvas, center, center, 3.6f, ramp[1]);
            Disc(canvas, center, center, 2.6f, ramp[2]);
            canvas.Set(14, 14, ramp[4]);
            canvas.Set(15, 14, ramp[3]);
            canvas.Set(17, 17, ramp[0]);

            if (!clean)
            {
                // Ferrugem verde comendo o aro.
                for (int py = 0; py < 32; py++)
                {
                    for (int px = 0; px < 32; px++)
                    {
                        if (canvas.Get(px, py).A == 0) continue;
                        if (Hash(px >> 1, py >> 1, 900) % 4 != 0) continue;

                        canvas.Set(px, py, Hash(px, py, 901) % 3 == 0 ? Verdigris : VerdigrisDark);
                    }
                }
            }

            return canvas;
        }

        /// <summary>
        /// Mostrador de manometro, sem o ponteiro (que e um sprite a parte, pra
        /// poder tremer). A zona vermelha fica a direita; o ponteiro aponta pra
        /// cima quando o angulo e zero.
        /// </summary>
        public static PixelCanvas GaugeFace(int size)
        {
            var canvas = new PixelCanvas(size, size);
            float center = size * 0.5f;
            float outer = size * 0.5f - 1f;
            float rim = Math.Max(1.5f, size / 12f);

            Disc(canvas, center, center, outer, BrassDark);
            Ring(canvas, center, center, outer - rim, outer, BrassRamp);
            Disc(canvas, center, center, outer - rim, DialFace);

            float face = outer - rim;

            for (int py = 0; py < size; py++)
            {
                for (int px = 0; px < size; px++)
                {
                    float dx = px + 0.5f - center;
                    float dy = py + 0.5f - center;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (distance > face) continue;

                    // Angulo a partir de "pra cima", crescendo no sentido horario.
                    float angle = (float)(Math.Atan2(dx, -dy) * 180.0 / Math.PI);

                    // Sombra do vidro na metade de baixo.
                    if (dy > face * 0.35f) canvas.Set(px, py, DialShade);

                    // As zonas de cor sao so um arco fino perto da borda.
                    if (distance < face * 0.68f || distance > face * 0.92f) continue;

                    if (angle > 40f && angle < 125f) canvas.Set(px, py, DialRed);
                    else if (angle < -75f && angle > -125f) canvas.Set(px, py, DialGreen);
                    else if (angle > -125f && angle < 125f && ((int)(angle + 360f) / 15) % 2 == 0)
                    {
                        // Tracinhos da escala no resto do arco.
                        canvas.Set(px, py, DialShade);
                    }
                }
            }

            Disc(canvas, center, center, Math.Max(1f, size / 16f), NeedleDark);

            // Brilho do vidro.
            canvas.Set((int)(center - face * 0.45f), (int)(center - face * 0.55f), new Rgba(255, 255, 255));

            return canvas;
        }

        /// <summary>Ponteiro de manometro: uma haste em pe, com o pivo na base.</summary>
        public static PixelCanvas GaugeNeedle(int length)
        {
            int width = length >= 10 ? 3 : 1;
            var canvas = new PixelCanvas(width, length);

            for (int py = 0; py < length; py++)
            {
                if (width == 1)
                {
                    canvas.Set(0, py, NeedleDark);
                    continue;
                }

                // Afina na ponta.
                if (py >= 2) canvas.Set(0, py, new Rgba(86, 60, 48));
                canvas.Set(1, py, NeedleDark);
                if (py >= length - 3) canvas.Set(2, py, NeedleDark);
            }

            return canvas;
        }

        public const int BoilerWidth = 64;
        public const int BoilerHeight = 96;

        /// <summary>
        /// Caldeira, 64x96 (4x6 tiles): tanque de cobre em pe com cintas de
        /// latao, fornalha gradeada na base e uma vigia no meio. O brilho do
        /// fogo esta em <see cref="BoilerGlow"/>, pra poder pulsar.
        /// </summary>
        public static PixelCanvas Boiler()
        {
            var canvas = new PixelCanvas(BoilerWidth, BoilerHeight);

            const int left = 8;
            const int right = 55;
            const int bodyTop = 22;
            const int bodyBottom = 85;
            const float center = 32f;
            const float half = 24f;

            // Dois canos saindo da cupula.
            foreach (int px0 in new[] { 19, 38 })
            {
                for (int py = 0; py < 14; py++)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        float across = (i + 0.5f - 3.5f) / 3.5f;
                        canvas.Set(px0 + i, py, Shade(BrassRamp, Lit(across, 0f)));
                    }
                }

                for (int i = -1; i < 8; i++)
                {
                    canvas.Set(px0 + i, 9, BrassDark);
                    canvas.Set(px0 + i, 10, BrassLight);
                    canvas.Set(px0 + i, 11, BrassShade);
                }
            }

            // Cupula: meia elipse.
            for (int py = 8; py < bodyTop; py++)
            {
                float t = (bodyTop - py) / (float)(bodyTop - 8);
                float halfWidth = half * (float)Math.Sqrt(Math.Max(0f, 1f - t * t));

                for (int px = left; px <= right; px++)
                {
                    float dx = px + 0.5f - center;
                    if (Math.Abs(dx) > halfWidth) continue;

                    canvas.Set(px, py, Shade(CopperRamp, Lit(dx / half * 0.9f, -t * 0.6f)));
                }
            }

            // Corpo.
            for (int py = bodyTop; py <= bodyBottom; py++)
            {
                for (int px = left; px <= right; px++)
                {
                    float across = (px + 0.5f - center) / half;
                    canvas.Set(px, py, Shade(CopperRamp, Lit(across * 0.96f, 0f)));
                }
            }

            // Emendas verticais apagadas, pra o tanque ler como chapa calandrada.
            foreach (int px in new[] { 20, 44 })
            {
                for (int py = bodyTop + 6; py <= bodyBottom; py++)
                {
                    if (py % 3 == 0) canvas.Set(px, py, CopperDark);
                }
            }

            // Cintas de latao, com rebites.
            foreach (int bandY in new[] { bodyTop, 50, 80 })
            {
                for (int px = left - 1; px <= right + 1; px++)
                {
                    float across = (px + 0.5f - center) / (half + 1f);
                    Rgba color = Shade(BrassRamp, Lit(across * 0.96f, 0f));

                    canvas.Set(px, bandY, BrassDark);
                    canvas.Set(px, bandY + 1, color);
                    canvas.Set(px, bandY + 2, color);
                    canvas.Set(px, bandY + 3, color.Scaled(0.8f));
                    canvas.Set(px, bandY + 4, BrassDark);

                    if ((px - left) % 6 == 2) canvas.Set(px, bandY + 2, BrassShine);
                }
            }

            // Azinhavre escorrendo das cintas.
            Drip(canvas, 14, 27, 9, 1001);
            Drip(canvas, 47, 55, 11, 1002);
            Drip(canvas, 27, 55, 5, 1003);
            Stain(canvas, 50, 38, 4, 1004, left, bodyTop + 5, right - left, 20);

            // Vigia: um olho de vidro com fogo atras.
            Disc(canvas, center, 38f, 6.5f, BrassDark);
            Ring(canvas, center, 38f, 4.2f, 6.2f, BrassRamp);
            Disc(canvas, center, 38f, 4.2f, new Rgba(60, 22, 14));

            // Fornalha: moldura de latao, boca escura, grade.
            const int doorLeft = 21;
            const int doorTop = 59;
            const int doorWidth = 22;
            const int doorHeight = 18;

            canvas.Fill(doorLeft - 2, doorTop - 2, doorWidth + 4, doorHeight + 4, BrassDark);
            canvas.Fill(doorLeft - 1, doorTop - 1, doorWidth + 2, doorHeight + 2, BrassMid);
            canvas.Fill(doorLeft - 1, doorTop - 1, doorWidth + 2, 1, BrassLight);
            canvas.Fill(doorLeft - 1, doorTop - 1, 1, doorHeight + 2, BrassLight);
            canvas.Fill(doorLeft, doorTop, doorWidth, doorHeight, new Rgba(34, 16, 14));

            for (int py = doorTop + 2; py < doorTop + doorHeight; py += 4)
            {
                canvas.Fill(doorLeft, py, doorWidth, 2, IronDark);
                canvas.Fill(doorLeft, py, doorWidth, 1, IronMid);
            }

            foreach (int px in new[] { doorLeft - 1, doorLeft + doorWidth - 1 })
            {
                Rivet(canvas, px, doorTop + 3);
                Rivet(canvas, px, doorTop + doorHeight - 5);
            }

            // Base: saia e pes.
            for (int py = bodyBottom + 1; py < BoilerHeight; py++)
            {
                for (int px = left + 2; px <= right - 2; px++)
                {
                    float across = (px + 0.5f - center) / half;
                    canvas.Set(px, py, Shade(IronRamp, Lit(across * 0.9f, 0f)));
                }
            }

            canvas.Fill(left, BoilerHeight - 3, right - left + 1, 3, IronDark);
            canvas.Fill(left, BoilerHeight - 3, right - left + 1, 1, IronLight);

            // Um manometro pequeno preso na lateral.
            PixelCanvas dial = GaugeFace(12);
            canvas.Blit(dial, 43, 30);

            return canvas;
        }

        /// <summary>
        /// So o fogo da caldeira — as frestas da fornalha e a vigia — pra ser
        /// desenhado por cima, sem luz, e pulsar.
        /// </summary>
        public static PixelCanvas BoilerGlow()
        {
            var canvas = new PixelCanvas(BoilerWidth, BoilerHeight);

            // Vigia.
            for (int py = 33; py < 44; py++)
            {
                for (int px = 27; px < 38; px++)
                {
                    float dx = px + 0.5f - 32f;
                    float dy = py + 0.5f - 38f;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                    if (distance > 4.2f) continue;

                    canvas.Set(px, py, distance < 1.6f ? FireCore : (distance < 3f ? FireMid : FireDeep));
                }
            }

            // Frestas entre as barras da grade.
            const int doorLeft = 21;
            const int doorTop = 59;
            const int doorWidth = 22;
            const int doorHeight = 18;

            for (int py = doorTop; py < doorTop + doorHeight; py++)
            {
                // As barras ocupam 2 de cada 4 linhas, a partir da terceira.
                int inBar = (py - doorTop - 2) % 4;
                if (py >= doorTop + 2 && (inBar == 0 || inBar == 1)) continue;

                for (int px = doorLeft; px < doorLeft + doorWidth; px++)
                {
                    float fromCenter = Math.Abs(px + 0.5f - (doorLeft + doorWidth * 0.5f)) / (doorWidth * 0.5f);
                    float depth = (py - doorTop) / (float)doorHeight;

                    // Mais quente no meio e embaixo, onde esta a brasa.
                    float heat = (1f - fromCenter) * 0.7f + depth * 0.5f;
                    if (Hash(px, py, 1100) % 5 == 0) heat -= 0.25f;

                    canvas.Set(px, py, heat > 0.75f ? FireCore : (heat > 0.4f ? FireMid : FireDeep));
                }
            }

            return canvas;
        }

        /// <summary>
        /// Bocal de vapor: uma caixa de latao com frestas, de onde sai a coluna.
        /// Tem a largura da coluna, em tiles, e 1 tile de altura. O vapor sai
        /// por cima; pra jato deitado, gire a imagem.
        /// </summary>
        public static PixelCanvas Nozzle(int widthTiles)
        {
            int width = widthTiles * Tile;
            var canvas = new PixelCanvas(width, Tile);

            // Corpo: um trapezio baixo, com a boca em cima.
            for (int py = 5; py < Tile; py++)
            {
                int inset = py < 7 ? 3 : (py < 9 ? 2 : (py < 11 ? 1 : 0));

                for (int px = inset; px < width - inset; px++)
                {
                    Rgba color = py == 5 ? BrassShine
                        : py == 6 ? BrassLight
                        : py < 12 ? BrassMid
                        : py < 15 ? BrassShade
                        : BrassDark;

                    canvas.Set(px, py, color);
                }
            }

            // A boca: uma faixa escura logo abaixo da borda, por onde o vapor sai.
            canvas.Fill(5, 7, width - 10, 2, Outline);

            // Aletas atravessando a boca.
            for (int px = 8; px < width - 8; px += 6)
            {
                canvas.Fill(px, 7, 2, 2, BrassShade);
            }

            // Rebites na base.
            for (int px = 3; px < width - 3; px += 8)
            {
                canvas.Set(px, 12, BrassShine);
                canvas.Set(px + 1, 13, BrassDark);
            }

            canvas.Fill(0, 15, width, 1, Outline);
            return canvas;
        }

        /// <summary>
        /// Moldura de fornalha de parede, 32x32, com o miolo vazado: o fogo
        /// animado do pack aparece por tras das barras.
        /// </summary>
        public static PixelCanvas FurnaceFrame()
        {
            var canvas = new PixelCanvas(32, 32);

            // Arco: retangulo com os cantos de cima arredondados.
            for (int py = 0; py < 32; py++)
            {
                for (int px = 0; px < 32; px++)
                {
                    float dx = px + 0.5f - 16f;
                    bool inOuter = py >= 12 || dx * dx + (py - 12f) * (py - 12f) <= 15.5f * 15.5f;
                    bool inInner = px >= 5 && px < 27 && py < 28
                        && (py >= 13 || dx * dx + (py - 13f) * (py - 13f) <= 10.5f * 10.5f);

                    if (!inOuter || inInner) continue;

                    float across = dx / 16f;
                    canvas.Set(px, py, Shade(BrassRamp, Lit(across * 0.8f, (py - 16f) / 32f)));
                }
            }

            canvas.Fill(0, 28, 32, 1, BrassLight);
            canvas.Fill(0, 31, 32, 1, Outline);

            // Barras em pe.
            for (int px = 8; px < 26; px += 5)
            {
                for (int py = 4; py < 28; py++)
                {
                    if (canvas.Get(px, py).A != 0) continue;
                    canvas.Set(px, py, IronMid);
                    canvas.Set(px + 1, py, IronDark);
                }
            }

            Rivet(canvas, 1, 16);
            Rivet(canvas, 29, 16);
            Rivet(canvas, 1, 24);
            Rivet(canvas, 29, 24);

            return canvas;
        }

        /// <summary>
        /// Escotilha da cisterna, 32x48: uma porta estanque redonda, com
        /// volante no meio. E so cenario — a cisterna e "selada contra o gas".
        /// </summary>
        public static PixelCanvas Hatch()
        {
            var canvas = new PixelCanvas(32, 48);

            for (int py = 0; py < 48; py++)
            {
                for (int px = 0; px < 32; px++)
                {
                    float dx = (px + 0.5f - 16f) / 15.5f;
                    float dy = (py + 0.5f - 24f) / 23.5f;
                    float distance = dx * dx + dy * dy;

                    if (distance > 1f) continue;

                    if (distance > 0.78f)
                    {
                        canvas.Set(px, py, Shade(BrassRamp, Lit(dx * 0.9f, dy * 0.9f)));
                    }
                    else
                    {
                        Rgba plate = Shade(IronRamp, 0.45f + (-dx * 0.25f - dy * 0.3f));
                        canvas.Set(px, py, plate);
                    }
                }
            }

            // Rebites em volta.
            for (int i = 0; i < 12; i++)
            {
                double angle = i * Math.PI / 6.0;
                int px = (int)Math.Round(16 + Math.Cos(angle) * 13.2 - 0.5);
                int py = (int)Math.Round(24 + Math.Sin(angle) * 20.6 - 0.5);
                canvas.Set(px, py, BrassShine);
                canvas.Set(px + 1, py + 1, BrassDark);
            }

            // Volante.
            Ring(canvas, 16f, 24f, 4.2f, 6.4f, BrassRamp);
            for (int i = -6; i <= 6; i++)
            {
                canvas.Set(16 + i, 24, BrassShade);
                canvas.Set(16, 24 + i, BrassShade);
            }

            Disc(canvas, 16f, 24f, 1.6f, BrassLight);
            return canvas;
        }

        /// <summary>
        /// Goldsmith Workbench, 48x32: bancada de madeira com bigorna e um
        /// cristal aceso em cima. Marca onde o save point vai ficar.
        /// </summary>
        public static PixelCanvas Workbench()
        {
            var canvas = new PixelCanvas(48, 32);

            var wood = new Rgba(120, 78, 48);
            var woodLight = new Rgba(160, 108, 66);
            var woodDark = new Rgba(74, 46, 32);

            // Tampo.
            canvas.Fill(2, 16, 44, 4, wood);
            canvas.Fill(2, 16, 44, 1, woodLight);
            canvas.Fill(2, 19, 44, 1, woodDark);

            // Pes e travessa.
            canvas.Fill(5, 20, 4, 12, wood);
            canvas.Fill(8, 20, 1, 12, woodDark);
            canvas.Fill(39, 20, 4, 12, wood);
            canvas.Fill(42, 20, 1, 12, woodDark);
            canvas.Fill(9, 26, 30, 2, woodDark);

            // Bigorna.
            canvas.Fill(8, 9, 16, 4, IronMid);
            canvas.Fill(8, 9, 16, 1, IronLight);
            canvas.Fill(5, 10, 3, 2, IronMid);
            canvas.Fill(12, 13, 8, 2, IronDark);
            canvas.Fill(10, 15, 12, 1, IronMid);

            // Torno de bancada, de latao.
            canvas.Fill(28, 11, 3, 5, BrassMid);
            canvas.Fill(33, 11, 3, 5, BrassMid);
            canvas.Fill(28, 11, 8, 1, BrassLight);
            canvas.Fill(26, 13, 12, 1, BrassShade);

            // O cristal: e ele que marca o save.
            var crystal = new Rgba(120, 230, 255);
            var crystalLight = new Rgba(220, 250, 255);
            var crystalDark = new Rgba(56, 150, 200);

            canvas.Fill(41, 9, 3, 7, crystal);
            canvas.Fill(42, 6, 1, 3, crystalLight);
            canvas.Set(41, 8, crystalLight);
            canvas.Fill(43, 10, 1, 6, crystalDark);
            canvas.Set(42, 5, crystalLight);

            return canvas;
        }

        /// <summary>
        /// O Scrap depois do primeiro polimento: mesma silhueta do placeholder
        /// da Phase 1 (16x20), com quartzo claro, friso de bronze e olhos azuis.
        /// </summary>
        public static PixelCanvas ScrapPolished()
        {
            const int width = 16;
            const int height = 20;
            var canvas = new PixelCanvas(width, height);

            var quartz = new Rgba(208, 222, 232);
            var quartzLight = new Rgba(240, 248, 252);
            var bronze = new Rgba(150, 104, 56);
            var eye = new Rgba(110, 200, 255);
            var eyeLight = new Rgba(226, 246, 255);
            var pupil = new Rgba(24, 40, 70);

            const float centerX = 7.5f;

            for (int row = 0; row < height; row++)
            {
                // A conta do placeholder original e de baixo pra cima.
                int fromBottom = height - 1 - row;
                float t = fromBottom / (float)(height - 1);
                float halfWidth = 6.8f + (5.2f - 6.8f) * t * t;
                if (fromBottom < 2) halfWidth = 6.2f;

                for (int px = 0; px < width; px++)
                {
                    float dx = Math.Abs(px - centerX);
                    if (dx > halfWidth) continue;

                    bool edge = dx > halfWidth - 1.2f || fromBottom == 0 || fromBottom == height - 1;
                    bool highlight = !edge && fromBottom > height - 7 && dx < 2.5f;

                    canvas.Set(px, row, edge ? bronze : (highlight ? quartzLight : quartz));
                }
            }

            // Um friso de bronze na cintura: e o que sobrou da sucata.
            for (int px = 2; px < width - 2; px++)
            {
                if (canvas.Get(px, 14).A != 0) canvas.Set(px, 14, bronze);
            }

            // Olhos: 3x3, na mesma posicao do original.
            foreach (int originX in new[] { 4, 9 })
            {
                for (int ey = 0; ey < 3; ey++)
                {
                    for (int ex = 0; ex < 3; ex++)
                    {
                        int row = height - 1 - (11 + ey);
                        Rgba color = ex == 1 && ey == 1 ? pupil : (ey == 2 ? eyeLight : eye);
                        canvas.Set(originX + ex, row, color);
                    }
                }
            }

            return canvas;
        }

        public const int SkylineWidth = 512;
        public const int SkylineHeight = 128;

        /// <summary>
        /// O horizonte de caldeiras colossais, 512x128, pra ficar la no fundo
        /// dos saloes abertos. E uma silhueta de uma cor so com as frestas das
        /// fornalhas acesas. Repete na horizontal sem emenda.
        /// </summary>
        public static PixelCanvas Skyline(bool near)
        {
            var canvas = new PixelCanvas(SkylineWidth, SkylineHeight);

            Rgba body = near ? new Rgba(17, 12, 11) : new Rgba(38, 25, 18);
            Rgba glow = near ? new Rgba(214, 96, 28) : new Rgba(150, 72, 26);
            Rgba spark = near ? new Rgba(255, 190, 90) : new Rgba(200, 120, 50);
            Rgba gauge = near ? new Rgba(96, 150, 80) : new Rgba(60, 96, 60);

            int seed = near ? 1300 : 1400;

            // Chao: uma faixa cheia embaixo, com um cano grosso correndo por cima.
            canvas.Fill(0, SkylineHeight - 14, SkylineWidth, 14, body);
            canvas.Fill(0, SkylineHeight - 26, SkylineWidth, 5, body);

            int x = 6;
            int index = 0;

            while (x < SkylineWidth - 70)
            {
                int kind = Hash(index, 1, seed) % 5;
                int width = 26 + Hash(index, 2, seed) % 30;
                int height = 46 + Hash(index, 3, seed) % 62;
                int top = SkylineHeight - 14 - height;

                if (kind == 0)
                {
                    // Chamine fina com um colar.
                    int stack = 10 + Hash(index, 4, seed) % 6;
                    canvas.Fill(x, top - 8, stack, height + 8, body);
                    canvas.Fill(x - 2, top + 6, stack + 4, 4, body);
                    canvas.Fill(x - 2, top + height / 2, stack + 4, 3, body);
                    x += stack + 10 + Hash(index, 5, seed) % 14;
                }
                else
                {
                    // Tanque de cupula redonda.
                    for (int py = top; py < SkylineHeight - 14; py++)
                    {
                        int depth = py - top;
                        int inset = 0;
                        int cap = width / 2;

                        if (depth < cap)
                        {
                            float t = 1f - depth / (float)cap;
                            inset = (int)(width * 0.5f * (1f - (float)Math.Sqrt(Math.Max(0f, 1f - t * t))));
                        }

                        canvas.Fill(x + inset, py, width - inset * 2, 1, body);
                    }

                    // Canos saindo de cima.
                    canvas.Fill(x + width / 3, top - 14, 5, 16, body);
                    if (kind >= 3) canvas.Fill(x + width * 2 / 3, top - 22, 4, 24, body);

                    // Passarela ligando ao proximo.
                    canvas.Fill(x + width - 2, top + height / 3, 24, 3, body);

                    // A fornalha acesa na base.
                    int doorWidth = Math.Max(8, width / 3);
                    int doorX = x + (width - doorWidth) / 2;
                    int doorY = SkylineHeight - 14 - 16;

                    for (int py = 0; py < 9; py++)
                    {
                        if (py % 3 == 2) continue;
                        canvas.Fill(doorX, doorY + py, doorWidth, 1, py > 4 ? spark : glow);
                    }

                    // Vigia.
                    if (height > 70)
                    {
                        Disc(canvas, x + width * 0.5f, top + height * 0.42f, 3.2f, glow);
                        Disc(canvas, x + width * 0.5f, top + height * 0.42f, 1.4f, spark);
                    }

                    // Um manometro, verde de proposito: ao longe ainda ha
                    // pressao sob controle em algum lugar.
                    if (kind == 2) Disc(canvas, x + width * 0.75f, top + height * 0.25f, 2.2f, gauge);

                    x += width + 8 + Hash(index, 6, seed) % 22;
                }

                index++;
            }

            return canvas;
        }
    }
}
