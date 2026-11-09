using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A arte provisoria dos habitantes do District 2, desenhada por codigo:
    /// os quatro inimigos, o Boiler Master, Crisol e as pecas de cenario que
    /// as mecanicas usam (acido de nivel variavel, parede rachada, geiser).
    ///
    /// Sao placeholders na escala do resto, 16 px por tile. Troque os PNGs em
    /// Assets/Generated/Brass quando houver arte de verdade.
    ///
    /// Nao depende de UnityEngine, como o resto da arte gerada.
    /// </summary>
    public static class EnemyArt
    {
        private static readonly Rgba Outline = new Rgba(28, 22, 24);

        private static readonly Rgba Brass = new Rgba(168, 122, 48);
        private static readonly Rgba BrassLight = new Rgba(214, 170, 78);
        private static readonly Rgba BrassShine = new Rgba(246, 224, 150);
        private static readonly Rgba BrassDark = new Rgba(96, 66, 34);

        private static readonly Rgba Copper = new Rgba(166, 92, 56);
        private static readonly Rgba CopperLight = new Rgba(214, 132, 82);
        private static readonly Rgba CopperDark = new Rgba(92, 50, 38);

        private static readonly Rgba Rubber = new Rgba(52, 46, 54);
        private static readonly Rgba RubberLight = new Rgba(86, 78, 90);

        private static readonly Rgba Iron = new Rgba(84, 80, 90);
        private static readonly Rgba IronLight = new Rgba(128, 124, 134);
        private static readonly Rgba IronDark = new Rgba(44, 42, 52);

        private static readonly Rgba Verdigris = new Rgba(72, 150, 124);
        private static readonly Rgba VerdigrisDark = new Rgba(40, 92, 84);

        private static readonly Rgba Fire = new Rgba(255, 156, 44);
        private static readonly Rgba FireCore = new Rgba(255, 236, 160);

        private static readonly Rgba Acid = new Rgba(134, 194, 40);
        private static readonly Rgba AcidLight = new Rgba(196, 236, 70);
        private static readonly Rgba AcidFoam = new Rgba(240, 255, 170);
        private static readonly Rgba AcidDeep = new Rgba(78, 134, 28);

        private static readonly Rgba Silver = new Rgba(176, 184, 196);
        private static readonly Rgba SilverLight = new Rgba(228, 234, 240);
        private static readonly Rgba SilverDark = new Rgba(104, 112, 128);

        private static void Disc(PixelCanvas canvas, float cx, float cy, float rx, float ry, Rgba light, Rgba body, Rgba dark)
        {
            for (int y = (int)(cy - ry) - 1; y <= (int)(cy + ry) + 1; y++)
            {
                for (int x = (int)(cx - rx) - 1; x <= (int)(cx + rx) + 1; x++)
                {
                    float dx = (x + 0.5f - cx) / rx;
                    float dy = (y + 0.5f - cy) / ry;
                    if (dx * dx + dy * dy > 1f) continue;

                    // Luz de cima e da esquerda.
                    float lit = -dx * 0.6f - dy * 0.6f;
                    canvas.Set(x, y, lit > 0.35f ? light : (lit > -0.35f ? body : dark));
                }
            }
        }

        /// <summary>Contorna de escuro, pra ler em cima de qualquer fundo.</summary>
        private static void AddOutline(PixelCanvas canvas)
        {
            int width = canvas.Width;
            int height = canvas.Height;
            var filled = new bool[width * height];
            for (int i = 0; i < filled.Length; i++) filled[i] = canvas.Pixels[i].A != 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (filled[x + y * width]) continue;

                    bool touches = (x > 0 && filled[x - 1 + y * width])
                        || (x < width - 1 && filled[x + 1 + y * width])
                        || (y > 0 && filled[x + (y - 1) * width])
                        || (y < height - 1 && filled[x + (y + 1) * width]);

                    if (touches) canvas.Set(x, y, Outline);
                }
            }
        }

        /// <summary>Pipe Leech, 16x16, olhando pra esquerda: uma lesma de borracha com aneis de cobre e uma ventosa.</summary>
        public static PixelCanvas PipeLeech()
        {
            var canvas = new PixelCanvas(16, 16);

            Disc(canvas, 8.5f, 10.5f, 6.5f, 4.5f, RubberLight, Rubber, Outline);

            // Aneis de cobre.
            foreach (int x in new[] { 6, 9, 12 })
            {
                for (int y = 6; y < 15; y++)
                {
                    if (canvas.Get(x, y).A == 0) continue;
                    canvas.Set(x, y, y < 9 ? CopperLight : Copper);
                }
            }

            // A boca: uma ventosa clara com dentes.
            canvas.Fill(1, 9, 3, 4, CopperDark);
            canvas.Fill(1, 10, 2, 2, new Rgba(220, 90, 70));
            canvas.Set(2, 9, new Rgba(240, 230, 210));
            canvas.Set(2, 12, new Rgba(240, 230, 210));

            // Um olho aceso.
            canvas.Set(5, 8, AcidFoam);
            canvas.Set(5, 9, AcidLight);

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>Valve Sentry, 16x24, olhando pra direita: uma torre de latao com um bocal e uma lente vermelha.</summary>
        public static PixelCanvas ValveSentry()
        {
            var canvas = new PixelCanvas(16, 24);

            // Base.
            canvas.Fill(2, 19, 12, 5, IronDark);
            canvas.Fill(3, 19, 10, 4, Iron);
            canvas.Fill(3, 19, 10, 1, IronLight);

            // Haste.
            canvas.Fill(6, 12, 4, 7, BrassDark);
            canvas.Fill(7, 12, 2, 7, Brass);

            // Cabeca: um tambor.
            Disc(canvas, 7.5f, 8f, 6f, 5.5f, BrassLight, Brass, BrassDark);
            canvas.Fill(4, 4, 7, 1, BrassShine);

            // O bocal, apontando pra direita.
            canvas.Fill(11, 6, 5, 4, BrassDark);
            canvas.Fill(11, 6, 5, 1, BrassLight);
            canvas.Fill(14, 7, 2, 2, Outline);

            // A lente.
            canvas.Fill(5, 6, 3, 3, Outline);
            canvas.Fill(6, 7, 2, 1, new Rgba(240, 70, 50));
            canvas.Set(6, 6, new Rgba(255, 170, 140));

            // Azinhavre.
            canvas.Set(9, 11, Verdigris);
            canvas.Set(10, 12, VerdigrisDark);
            canvas.Set(4, 10, Verdigris);

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>
        /// Brass Welder, 16x24, olhando pra direita: um Tarnished de mascara de
        /// soldador, com o macarico aceso na frente.
        /// </summary>
        public static PixelCanvas BrassWelder()
        {
            var canvas = new PixelCanvas(16, 24);

            // Pernas.
            canvas.Fill(4, 18, 3, 6, CopperDark);
            canvas.Fill(9, 18, 3, 6, CopperDark);
            canvas.Fill(5, 18, 1, 5, Copper);
            canvas.Fill(10, 18, 1, 5, Copper);
            canvas.Fill(3, 22, 5, 2, IronDark);
            canvas.Fill(8, 22, 5, 2, IronDark);

            // Tronco: um avental de couro.
            canvas.Fill(3, 9, 10, 10, new Rgba(104, 68, 46));
            canvas.Fill(4, 9, 8, 9, new Rgba(134, 90, 58));
            canvas.Fill(4, 9, 2, 9, new Rgba(160, 112, 72));
            canvas.Fill(3, 13, 10, 1, BrassDark);

            // A mascara: uma chapa escura com um visor estreito e aceso.
            canvas.Fill(4, 1, 9, 8, IronDark);
            canvas.Fill(5, 1, 7, 7, Iron);
            canvas.Fill(5, 1, 7, 1, IronLight);
            canvas.Fill(6, 4, 6, 2, Outline);
            canvas.Fill(7, 4, 4, 1, new Rgba(255, 120, 60));

            // O braco estendido e o macarico.
            canvas.Fill(11, 11, 3, 2, Copper);
            canvas.Fill(13, 10, 2, 4, Brass);
            canvas.Set(15, 11, FireCore);
            canvas.Set(15, 12, Fire);
            canvas.Set(15, 10, Fire);

            // Ferrugem verde: e um corrompido.
            canvas.Set(5, 15, Verdigris);
            canvas.Set(6, 16, VerdigrisDark);
            canvas.Set(10, 7, Verdigris);
            canvas.Set(4, 20, VerdigrisDark);

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>Acid Bloater, 24x24, olhando pra direita: um tanque redondo cheio de acido, de pernas curtas.</summary>
        public static PixelCanvas AcidBloater()
        {
            var canvas = new PixelCanvas(24, 24);

            // Pernas.
            canvas.Fill(5, 19, 4, 5, IronDark);
            canvas.Fill(15, 19, 4, 5, IronDark);
            canvas.Fill(6, 19, 2, 4, Iron);
            canvas.Fill(16, 19, 2, 4, Iron);

            // O tanque.
            Disc(canvas, 12f, 11f, 10f, 9.5f, CopperLight, Copper, CopperDark);

            // A janela de vidro, mostrando o acido chacoalhando la dentro.
            Disc(canvas, 12.5f, 12f, 6f, 5.5f, AcidLight, Acid, AcidDeep);
            for (int x = 7; x <= 18; x++)
            {
                if (canvas.Get(x, 9).R == Acid.R || canvas.Get(x, 9).R == AcidLight.R) canvas.Set(x, 9, AcidFoam);
            }

            canvas.Set(10, 13, AcidFoam);
            canvas.Set(15, 14, AcidFoam);

            // Cintas e rebites.
            canvas.Fill(2, 5, 20, 1, BrassDark);
            canvas.Fill(3, 18, 18, 1, BrassDark);
            canvas.Set(5, 5, BrassShine);
            canvas.Set(12, 5, BrassShine);
            canvas.Set(18, 5, BrassShine);

            // Uma valvulinha no topo.
            canvas.Fill(11, 0, 2, 2, BrassLight);

            // Um olho so, na frente.
            canvas.Fill(19, 8, 2, 2, Outline);
            canvas.Set(20, 8, AcidFoam);

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>Um jato de vapor escaldante, 16x8, voando pra direita.</summary>
        public static PixelCanvas SteamBolt()
        {
            var canvas = new PixelCanvas(16, 8);

            for (int x = 0; x < 16; x++)
            {
                // Mais grosso e mais cheio na frente.
                int half = 1 + x / 6;
                int alpha = 90 + x * 10;

                for (int y = 4 - half; y < 4 + half; y++)
                {
                    bool core = Math.Abs(y - 3.5f) < 1f && x > 6;
                    canvas.Set(x, y, (core ? new Rgba(255, 255, 255) : new Rgba(236, 240, 232)).WithAlpha(alpha));
                }
            }

            return canvas;
        }

        /// <summary>
        /// The Boiler Master, 64x64, olhando pra esquerda: um autômato enorme e
        /// atarracado, fundido a uma caldeira nas costas, com um martelo de
        /// chave inglesa.
        /// </summary>
        public static PixelCanvas BoilerMaster()
        {
            var canvas = new PixelCanvas(64, 64);

            // A caldeira nas costas (a direita), com duas chamines.
            Disc(canvas, 44f, 30f, 15f, 20f, CopperLight, Copper, CopperDark);
            canvas.Fill(38, 2, 5, 12, BrassDark);
            canvas.Fill(39, 2, 3, 12, Brass);
            canvas.Fill(48, 6, 5, 9, BrassDark);
            canvas.Fill(49, 6, 3, 9, Brass);

            for (int y = 18; y <= 46; y += 14)
            {
                canvas.Fill(30, y, 29, 2, BrassDark);
                canvas.Fill(30, y, 29, 1, BrassLight);
            }

            // A fornalha, virada pra tras: e o ponto fraco.
            canvas.Fill(50, 26, 8, 12, Outline);
            for (int y = 27; y < 37; y += 3)
            {
                canvas.Fill(51, y, 6, 2, Fire);
                canvas.Fill(52, y, 3, 1, FireCore);
            }

            // Pernas grossas.
            canvas.Fill(18, 48, 10, 16, IronDark);
            canvas.Fill(36, 48, 10, 16, IronDark);
            canvas.Fill(19, 48, 6, 14, Iron);
            canvas.Fill(37, 48, 6, 14, Iron);
            canvas.Fill(15, 60, 15, 4, Outline);
            canvas.Fill(34, 60, 15, 4, Outline);

            // O tronco, na frente da caldeira.
            Disc(canvas, 28f, 34f, 14f, 16f, BrassLight, Brass, BrassDark);
            canvas.Fill(15, 40, 26, 2, BrassDark);

            // A cabeca: pequena, enterrada, com um manometro no lugar do rosto.
            canvas.Fill(20, 10, 14, 10, IronDark);
            canvas.Fill(21, 11, 12, 8, Iron);
            Disc(canvas, 26f, 15f, 4.5f, 4.5f, new Rgba(240, 232, 208), new Rgba(232, 224, 198), new Rgba(190, 180, 152));
            canvas.Fill(27, 13, 2, 3, new Rgba(212, 58, 44));
            canvas.Set(26, 15, Outline);
            canvas.Set(25, 14, Outline);

            // O braco e o martelo de chave inglesa, erguido na frente.
            canvas.Fill(8, 28, 10, 6, BrassDark);
            canvas.Fill(9, 28, 8, 4, Brass);
            canvas.Fill(6, 8, 4, 26, IronDark);
            canvas.Fill(7, 8, 2, 26, IronLight);
            canvas.Fill(0, 2, 16, 9, IronDark);
            canvas.Fill(1, 3, 14, 6, Iron);
            canvas.Fill(1, 3, 14, 1, IronLight);
            canvas.Fill(5, 2, 6, 4, Outline);

            // As mangueiras, das costas pro chao.
            for (int i = 0; i < 14; i++)
            {
                canvas.Set(58 + i / 5, 40 + i, Rubber);
                canvas.Set(59 + i / 5, 40 + i, RubberLight);
            }

            // O Oxide tomando conta.
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    if (canvas.Get(x, y).A == 0) continue;
                    if (((x >> 1) * 7 + (y >> 1) * 13) % 9 != 0) continue;

                    Rgba pixel = canvas.Get(x, y);
                    if (pixel.R > 240 && pixel.G > 140) continue;   // nao por cima do fogo

                    canvas.Set(x, y, (x + y) % 3 == 0 ? Verdigris : VerdigrisDark);
                }
            }

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>
        /// Crisol, o Lapidario, 24x32, olhando pra direita: um autômato antigo
        /// de prata arranhada, com uma bigorna nas costas, muleta e monoculo.
        /// </summary>
        public static PixelCanvas Crisol()
        {
            var canvas = new PixelCanvas(24, 32);

            // A bigorna nas costas (a esquerda).
            canvas.Fill(1, 9, 8, 4, IronDark);
            canvas.Fill(1, 9, 8, 1, IronLight);
            canvas.Fill(3, 13, 4, 3, Iron);
            canvas.Fill(2, 16, 6, 2, IronDark);

            // Pernas — uma mais curta, por isso a muleta.
            canvas.Fill(9, 24, 3, 8, SilverDark);
            canvas.Fill(14, 24, 3, 7, SilverDark);
            canvas.Fill(10, 24, 1, 7, Silver);
            canvas.Fill(15, 24, 1, 6, Silver);
            canvas.Fill(8, 30, 5, 2, IronDark);
            canvas.Fill(13, 29, 5, 2, IronDark);

            // Tronco.
            Disc(canvas, 13f, 18f, 6f, 7f, SilverLight, Silver, SilverDark);

            // Arranhoes.
            canvas.Set(11, 15, SilverDark);
            canvas.Set(12, 16, SilverDark);
            canvas.Set(15, 20, SilverDark);
            canvas.Set(16, 21, SilverDark);
            canvas.Set(10, 21, SilverDark);

            // Cabeca.
            Disc(canvas, 13f, 7f, 5f, 5f, SilverLight, Silver, SilverDark);

            // O monoculo, com corrente.
            canvas.Fill(14, 5, 4, 4, BrassDark);
            canvas.Fill(15, 6, 2, 2, new Rgba(170, 230, 255));
            canvas.Set(15, 6, new Rgba(255, 255, 255));
            canvas.Set(18, 9, BrassLight);
            canvas.Set(18, 11, BrassLight);
            canvas.Set(17, 13, BrassLight);

            // O outro olho, semicerrado: ele nao esta impressionado.
            canvas.Fill(10, 7, 2, 1, Outline);

            // A muleta.
            canvas.Fill(20, 12, 1, 20, new Rgba(134, 86, 52));
            canvas.Fill(18, 12, 5, 1, new Rgba(160, 108, 66));
            canvas.Fill(18, 17, 3, 2, Silver);

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>O corpo do acido, 16x16, pra repetir: verde fundo com tracos curtos.</summary>
        public static PixelCanvas AcidBody()
        {
            var canvas = new PixelCanvas(16, 16);

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    int roll = ((x / 3) * 31 + y * 17) % 16;
                    Rgba color = roll == 0 ? new Rgba(44, 88, 26) : (roll == 1 ? Acid : AcidDeep);
                    canvas.Set(x, y, color.WithAlpha(232));
                }
            }

            return canvas;
        }

        /// <summary>A superficie do acido, 16x6, pra repetir de lado: uma linha de espuma ondulada.</summary>
        public static PixelCanvas AcidSurface()
        {
            var canvas = new PixelCanvas(16, 6);

            for (int x = 0; x < 16; x++)
            {
                // Uma onda que fecha em 16 px, pra a emenda nao aparecer.
                int top = 1 + (int)Math.Round(Math.Sin(x / 16.0 * Math.PI * 2.0) * 1.0);

                for (int y = top; y < 6; y++)
                {
                    Rgba color = y == top ? AcidFoam : (y == top + 1 ? AcidLight : Acid);
                    canvas.Set(x, y, color.WithAlpha(y == top ? 255 : 240));
                }
            }

            return canvas;
        }

        /// <summary>Um trecho de coluna de vapor, 16x16, pra repetir em pe: e o geiser do boss.</summary>
        public static PixelCanvas SteamColumn()
        {
            var canvas = new PixelCanvas(16, 16);

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    float dx = Math.Abs(x + 0.5f - 8f) / 8f;

                    // Borda ondulada, que fecha em 16 px na vertical.
                    float edge = 0.78f + 0.14f * (float)Math.Sin(y / 16.0 * Math.PI * 2.0 + x * 0.6);
                    if (dx > edge) continue;

                    int alpha = (int)(230 * (1f - dx * 0.7f));
                    canvas.Set(x, y, (dx < 0.3f ? new Rgba(255, 255, 255) : new Rgba(238, 240, 230)).WithAlpha(alpha));
                }
            }

            return canvas;
        }

        /// <summary>Uma chapa rachada, 16x16, pra repetir: a parede que a caldeira derruba.</summary>
        public static PixelCanvas CrackedWall()
        {
            var canvas = new PixelCanvas(16, 16);

            var plate = new Rgba(86, 62, 40);
            var plateLight = new Rgba(120, 88, 52);
            var plateDark = new Rgba(52, 38, 30);

            canvas.Fill(0, 0, 16, 16, plate);
            canvas.Fill(0, 0, 16, 1, plateLight);
            canvas.Fill(0, 0, 1, 16, plateLight);
            canvas.Fill(0, 15, 16, 1, plateDark);
            canvas.Fill(15, 0, 1, 16, plateDark);

            // A rachadura: um raio escuro de cima a baixo, com um ramo.
            int x = 6;
            for (int y = 0; y < 16; y++)
            {
                canvas.Set(x, y, Outline);
                canvas.Set(x + 1, y, plateDark);

                if (y % 3 == 1) x += y % 2 == 0 ? -1 : 2;
                if (y % 5 == 4) x -= 2;
                x = Math.Max(2, Math.Min(12, x));
            }

            canvas.Set(10, 6, Outline);
            canvas.Set(11, 5, Outline);
            canvas.Set(12, 5, Outline);
            canvas.Set(3, 11, Outline);
            canvas.Set(2, 12, Outline);

            // Um brilho de acido vazando pela fresta.
            canvas.Set(7, 8, AcidLight);
            canvas.Set(8, 12, Acid);

            return canvas;
        }
    }
}
