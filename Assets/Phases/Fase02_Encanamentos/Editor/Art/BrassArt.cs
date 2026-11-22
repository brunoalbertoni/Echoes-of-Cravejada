using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A arte do District 2, desenhada por codigo: chapa de latao rebitada,
    /// canos, valvulas, manometros, caldeiras e acido.
    ///
    /// Os packs do projeto so tem caverna, entao nada disso existia. Tudo aqui
    /// e pixel art de 16 px, na mesma escala do cave_tileset. A versao oficial
    /// fica nos PNGs de Assets/Generated.
    ///
    /// Nao depende de UnityEngine de proposito: assim da pra gerar as imagens
    /// fora do editor e olhar antes de abrir o jogo.
    /// </summary>
    public static partial class BrassArt
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
        /// <summary>Quantas colunas tem a onda da superficie antes de repetir.</summary>
        public const int AcidWaveColumns = 5;
        /// <summary>Quantos quadros tem a animacao da superficie.</summary>
        public const int AcidFrames = 4;
        /// <summary>Linha do corpo do acido, embaixo dos quadros da superficie.</summary>
        public const int AcidBodyRow = 4;
        public const int AcidBodyVariants = 4;
        public const int BoilerWidth = 64;
        public const int BoilerHeight = 96;
        public const int SkylineWidth = 512;
        public const int SkylineHeight = 128;
    }
}
