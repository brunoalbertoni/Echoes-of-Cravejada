using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A arte provisoria das mecanicas do Scrap, desenhada por codigo: o risco
    /// do cinzel, o Opaque Shard, a old scrap, a alavanca e o portao.
    ///
    /// Nenhum dos packs tem essas pecas. Sao placeholders de 16 px por tile,
    /// pra as mecanicas poderem ser testadas; troque os PNGs em
    /// Assets/Generated/Brass quando houver arte de verdade.
    ///
    /// Nao depende de UnityEngine, como o BrassArt.
    /// </summary>
    public static class ScrapArt
    {
        private static readonly Rgba Outline = new Rgba(30, 22, 20);
        private static readonly Rgba Rust = new Rgba(150, 84, 46);
        private static readonly Rgba RustLight = new Rgba(196, 122, 66);
        private static readonly Rgba RustDark = new Rgba(92, 50, 34);
        private static readonly Rgba Verdigris = new Rgba(72, 150, 124);
        private static readonly Rgba VerdigrisDark = new Rgba(40, 92, 84);
        private static readonly Rgba Brass = new Rgba(214, 170, 78);
        private static readonly Rgba BrassShine = new Rgba(246, 224, 150);
        private static readonly Rgba BrassShade = new Rgba(112, 76, 36);
        private static readonly Rgba Iron = new Rgba(84, 80, 90);
        private static readonly Rgba IronLight = new Rgba(124, 120, 130);
        private static readonly Rgba IronDark = new Rgba(44, 42, 52);

        /// <summary>
        /// O risco do golpe do cinzel, 24x16, apontando pra direita: uma meia
        /// lua clara que afina nas pontas.
        /// </summary>
        public static PixelCanvas ChiselSlash()
        {
            var canvas = new PixelCanvas(24, 16);

            for (int py = 0; py < 16; py++)
            {
                for (int px = 0; px < 24; px++)
                {
                    // Dois circulos deslocados: o que sobra entre eles e a lua.
                    float dx = px + 0.5f - 6f;
                    float dy = py + 0.5f - 8f;
                    float outer = (float)Math.Sqrt(dx * dx / (17f * 17f) + dy * dy / (8f * 8f));

                    float ix = px + 0.5f - 2f;
                    float inner = (float)Math.Sqrt(ix * ix / (17f * 17f) + dy * dy / (7f * 7f));

                    if (outer > 1f || inner < 1f || dx < 0f) continue;

                    float edge = 1f - outer;
                    Rgba color = edge > 0.12f ? new Rgba(255, 250, 225) : new Rgba(250, 214, 140);
                    canvas.Set(px, py, color.WithAlpha(edge > 0.05f ? 240 : 170));
                }
            }

            return canvas;
        }

        /// <summary>Um Opaque Shard, 8x10: um caco de cristal baço, cinza-azulado.</summary>
        public static PixelCanvas Shard()
        {
            var canvas = new PixelCanvas(8, 10);

            var body = new Rgba(150, 176, 190);
            var light = new Rgba(208, 226, 234);
            var dark = new Rgba(86, 106, 122);

            // Losango alongado, linha a linha: (comeco, largura).
            int[,] rows =
            {
                { 3, 2 }, { 2, 4 }, { 2, 4 }, { 1, 6 }, { 1, 6 },
                { 1, 6 }, { 2, 4 }, { 2, 4 }, { 3, 2 }, { 3, 1 },
            };

            for (int py = 0; py < 10; py++)
            {
                int start = rows[py, 0];
                int width = rows[py, 1];

                for (int i = 0; i < width; i++)
                {
                    Rgba color = i == 0 ? light : (i >= width - 1 ? dark : body);
                    canvas.Set(start + i, py, color);
                }
            }

            canvas.Set(3, 2, light);
            canvas.Set(3, 3, light);
            return canvas;
        }

        /// <summary>
        /// A old scrap, 16x12: um montinho de ferrugem com um shard espetado.
        /// E o que fica no lugar onde o Scrap morreu.
        /// </summary>
        public static PixelCanvas OldScrap()
        {
            var canvas = new PixelCanvas(16, 12);

            for (int py = 5; py < 12; py++)
            {
                // Um monte: mais largo embaixo.
                int half = 2 + (py - 5);
                if (half > 7) half = 7;

                for (int px = 8 - half; px < 8 + half; px++)
                {
                    bool edge = px == 8 - half || px == 8 + half - 1 || py == 5;
                    canvas.Set(px, py, edge ? RustDark : ((px + py) % 5 == 0 ? RustLight : Rust));
                }
            }

            canvas.Fill(0, 11, 16, 1, Outline);

            // Um olho apagado, pra lembrar de quem era.
            canvas.Fill(5, 8, 2, 2, new Rgba(60, 50, 46));
            canvas.Set(11, 7, Verdigris);
            canvas.Set(12, 8, VerdigrisDark);

            canvas.Blit(Shard(), 6, 0);
            return canvas;
        }

        /// <summary>
        /// A alavanca, 16x32: uma base presa no chao e uma haste inclinada com
        /// punho. Enferrujada (coberta de verde) ou limpa (latao polido).
        /// </summary>
        public static PixelCanvas Lever(bool clean)
        {
            var canvas = new PixelCanvas(16, 32);

            Rgba body = clean ? Brass : Rust;
            Rgba light = clean ? BrassShine : RustLight;
            Rgba dark = clean ? BrassShade : RustDark;

            // Base.
            canvas.Fill(2, 26, 12, 6, dark);
            canvas.Fill(3, 26, 10, 5, body);
            canvas.Fill(3, 26, 10, 1, light);
            canvas.Fill(2, 31, 12, 1, Outline);
            canvas.Set(4, 29, light);
            canvas.Set(11, 29, light);

            // Haste: enferrujada cai pra esquerda, limpa aponta pra direita.
            for (int i = 0; i < 20; i++)
            {
                int px = clean ? 7 + i / 4 : 8 - i / 4;
                int py = 26 - i;

                canvas.Set(px, py, body);
                canvas.Set(px + 1, py, dark);
                if (i % 5 == 2) canvas.Set(px, py, light);
            }

            // Punho.
            int knobX = clean ? 10 : 2;
            canvas.Fill(knobX, 3, 5, 5, dark);
            canvas.Fill(knobX + 1, 3, 3, 4, body);
            canvas.Set(knobX + 1, 3, light);

            if (clean) return canvas;

            // A ferrugem verde tomando conta.
            for (int py = 0; py < 32; py++)
            {
                for (int px = 0; px < 16; px++)
                {
                    if (canvas.Get(px, py).A == 0) continue;
                    if ((px * 7 + py * 13) % 5 != 0 && (px + py * 3) % 7 != 0) continue;

                    canvas.Set(px, py, (px + py) % 3 == 0 ? Verdigris : VerdigrisDark);
                }
            }

            return canvas;
        }

        /// <summary>
        /// The Rusty Scrapper, 48x48, olhando pra esquerda: um autômato grande
        /// e atarracado, coberto de ferrugem verde, com duas garras de sucata.
        /// </summary>
        public static PixelCanvas RustyScrapper()
        {
            var canvas = new PixelCanvas(48, 48);

            // Pernas curtas.
            canvas.Fill(12, 38, 8, 10, RustDark);
            canvas.Fill(28, 38, 8, 10, RustDark);
            canvas.Fill(13, 38, 6, 9, Rust);
            canvas.Fill(29, 38, 6, 9, Rust);
            canvas.Fill(10, 45, 12, 3, IronDark);
            canvas.Fill(26, 45, 12, 3, IronDark);

            // Tronco: um barril largo.
            for (int py = 10; py < 40; py++)
            {
                float t = (py - 10) / 29f;
                int half = 13 + (int)(5f * (float)Math.Sin(t * Math.PI));

                for (int px = 24 - half; px < 24 + half; px++)
                {
                    bool edge = px == 24 - half || px == 24 + half - 1 || py == 10 || py == 39;
                    bool lit = px < 24 - half + 5;
                    canvas.Set(px, py, edge ? Outline : (lit ? RustLight : Rust));
                }
            }

            // Cintas de ferro.
            canvas.Fill(8, 18, 32, 2, IronDark);
            canvas.Fill(7, 30, 34, 2, IronDark);
            canvas.Fill(8, 18, 32, 1, Iron);

            // Cabeca enterrada nos ombros, com um olho so, aceso.
            canvas.Fill(14, 3, 18, 9, RustDark);
            canvas.Fill(15, 4, 16, 7, Rust);
            canvas.Fill(17, 6, 7, 4, Outline);
            canvas.Fill(18, 7, 4, 2, new Rgba(120, 255, 170));
            canvas.Set(18, 7, new Rgba(230, 255, 235));

            // Bracos e garras: tres dentes de sucata em cada mao.
            canvas.Fill(1, 16, 8, 6, Rust);
            canvas.Fill(39, 16, 8, 6, RustDark);
            canvas.Fill(0, 22, 6, 12, RustDark);
            canvas.Fill(42, 22, 6, 12, RustDark);

            for (int claw = 0; claw < 3; claw++)
            {
                canvas.Fill(claw * 2, 34, 1, 7, IronLight);
                canvas.Fill(claw * 2 + 1, 34, 1, 5, Iron);
                canvas.Fill(43 + claw * 2, 34, 1, 7, Iron);
            }

            // A ferrugem verde por cima de tudo.
            for (int py = 0; py < 48; py++)
            {
                for (int px = 0; px < 48; px++)
                {
                    if (canvas.Get(px, py).A == 0) continue;
                    if (((px >> 1) * 7 + (py >> 1) * 13) % 6 != 0) continue;

                    canvas.Set(px, py, (px + py) % 3 == 0 ? Verdigris : VerdigrisDark);
                }
            }

            return canvas;
        }

        /// <summary>
        /// Um esqueleto, 16x24, olhando pra direita: o que sobrou de quem foi
        /// descartado no poco antes do Scrap. A variacao 1 usa um capuz.
        /// </summary>
        public static PixelCanvas Skeleton(int variant)
        {
            var canvas = new PixelCanvas(16, 24);

            var bone = new Rgba(226, 220, 198);
            var boneShade = new Rgba(168, 160, 140);
            var hollow = new Rgba(34, 28, 30);
            var cloth = variant == 1 ? new Rgba(96, 62, 46) : new Rgba(58, 74, 78);
            var clothDark = variant == 1 ? new Rgba(62, 40, 32) : new Rgba(38, 50, 54);

            // Caveira.
            canvas.Fill(4, 1, 8, 7, bone);
            canvas.Fill(5, 0, 6, 1, bone);
            canvas.Fill(4, 1, 1, 6, boneShade);
            canvas.Fill(6, 3, 2, 2, hollow);
            canvas.Fill(10, 3, 2, 2, hollow);
            canvas.Set(9, 5, hollow);
            canvas.Fill(6, 8, 6, 1, bone);
            canvas.Set(7, 8, hollow);
            canvas.Set(9, 8, hollow);
            canvas.Set(11, 8, hollow);

            // Um brilho no fundo de um olho: ainda ha alguem ai.
            canvas.Set(10, 3, new Rgba(120, 230, 190));

            // Coluna e costelas.
            canvas.Fill(7, 9, 2, 8, boneShade);
            for (int py = 10; py <= 14; py += 2)
            {
                canvas.Fill(4, py, 8, 1, bone);
                canvas.Set(4, py, boneShade);
            }

            // Bacia e pernas.
            canvas.Fill(5, 16, 6, 2, bone);
            canvas.Fill(5, 18, 2, 5, bone);
            canvas.Fill(9, 18, 2, 5, bone);
            canvas.Fill(5, 18, 1, 5, boneShade);
            canvas.Fill(4, 23, 4, 1, bone);
            canvas.Fill(9, 23, 4, 1, bone);

            // Bracos.
            canvas.Fill(3, 10, 1, 6, boneShade);
            canvas.Fill(12, 10, 1, 5, bone);
            canvas.Fill(12, 15, 2, 1, bone);

            // Um trapo: lenco no pescoco, ou capuz.
            canvas.Fill(4, 9, 8, 1, cloth);
            canvas.Fill(5, 10, 2, 2, clothDark);

            if (variant == 1)
            {
                canvas.Fill(3, 0, 10, 2, cloth);
                canvas.Fill(3, 2, 1, 6, cloth);
                canvas.Fill(12, 2, 1, 4, clothDark);
                canvas.Fill(4, 0, 8, 1, clothDark);
            }

            return canvas;
        }

        /// <summary>Uma pagina solta, 12x12: papel amarelado, torto, com linhas de escrita.</summary>
        public static PixelCanvas LoosePage()
        {
            var canvas = new PixelCanvas(12, 12);

            var paper = new Rgba(214, 200, 160);
            var paperShade = new Rgba(168, 152, 116);
            var ink = new Rgba(70, 56, 48);

            canvas.Fill(2, 1, 8, 10, paper);
            canvas.Fill(2, 10, 8, 1, paperShade);
            canvas.Fill(9, 1, 1, 10, paperShade);

            // Um canto dobrado e uma mancha de cobre.
            canvas.Set(2, 1, Rgba.Clear);
            canvas.Set(3, 1, paperShade);
            canvas.Set(7, 8, Verdigris);
            canvas.Set(8, 9, VerdigrisDark);

            for (int y = 3; y <= 8; y += 2)
            {
                canvas.Fill(3, y, y == 7 ? 3 : 5, 1, ink);
            }

            return canvas;
        }

        /// <summary>
        /// Um rabisco de giz na parede, 32x16. Nao e pra ser lido no sprite: so
        /// marca que ha algo escrito ali. A variacao 1 e uma mascara de soldador.
        /// </summary>
        public static PixelCanvas Scribble(int variant)
        {
            var canvas = new PixelCanvas(32, 16);
            var chalk = new Rgba(226, 220, 204, 220);
            var faint = new Rgba(226, 220, 204, 130);

            if (variant == 1)
            {
                // Uma mascara: um retangulo torto com um visor.
                for (int i = 0; i < 9; i++)
                {
                    canvas.Set(11 + i, 2 + (i % 4 == 0 ? 1 : 0), chalk);
                    canvas.Set(11 + i, 13 - (i % 3 == 0 ? 1 : 0), chalk);
                }

                for (int i = 0; i < 11; i++)
                {
                    canvas.Set(10 + (i % 5 == 0 ? 1 : 0), 3 + i, chalk);
                    canvas.Set(20 - (i % 4 == 0 ? 1 : 0), 3 + i, chalk);
                }

                canvas.Fill(12, 6, 7, 2, chalk);
                canvas.Set(13, 10, faint);
                canvas.Set(17, 10, faint);
                return canvas;
            }

            // Tres linhas de letra tremida.
            for (int line = 0; line < 3; line++)
            {
                int y = 3 + line * 4;
                int length = line == 2 ? 16 : 26;

                for (int x = 0; x < length; x++)
                {
                    // Espacos entre as "palavras".
                    if ((x + line * 3) % 7 == 6) continue;

                    int wobble = ((x * 5 + line * 11) % 3) - 1;
                    canvas.Set(3 + x, y + wobble, (x + line) % 4 == 0 ? faint : chalk);
                }
            }

            return canvas;
        }

        /// <summary>Uma placa gravada, 32x12: latao corroido, com letras cavadas.</summary>
        public static PixelCanvas Plaque()
        {
            var canvas = new PixelCanvas(32, 12);

            canvas.Fill(0, 0, 32, 12, BrassShade);
            canvas.Fill(1, 1, 30, 10, new Rgba(150, 110, 52));
            canvas.Fill(1, 1, 30, 1, Brass);
            canvas.Fill(0, 11, 32, 1, Outline);

            // Parafusos nos cantos.
            foreach (int x in new[] { 2, 28 })
            {
                canvas.Set(x, 2, BrassShine);
                canvas.Set(x, 9, BrassShine);
            }

            // As letras: tracos cavados, em duas linhas.
            for (int x = 6; x < 26; x++)
            {
                if (x % 4 == 1) continue;
                canvas.Set(x, 4, Outline);
                if (x % 3 != 0) canvas.Set(x, 7, Outline);
            }

            // Corroido: o verde comendo a borda.
            for (int y = 0; y < 12; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    if ((x * 7 + y * 13) % 11 != 0) continue;
                    canvas.Set(x, y, (x + y) % 2 == 0 ? Verdigris : VerdigrisDark);
                }
            }

            return canvas;
        }

        /// <summary>Um pedaco de sucata arremessado, 8x8.</summary>
        public static PixelCanvas ScrapChunk()
        {
            var canvas = new PixelCanvas(8, 8);

            canvas.Fill(1, 1, 6, 6, RustDark);
            canvas.Fill(2, 1, 4, 5, Rust);
            canvas.Fill(2, 1, 2, 2, RustLight);
            canvas.Set(0, 3, IronDark);
            canvas.Set(7, 5, IronDark);
            canvas.Set(5, 5, Verdigris);
            return canvas;
        }

        /// <summary>Uma onda de choque, 16x12, correndo pra direita: uma crista de poeira e faisca.</summary>
        public static PixelCanvas ShockWave()
        {
            var canvas = new PixelCanvas(16, 12);

            for (int px = 0; px < 16; px++)
            {
                // Mais alta na frente, sumindo atras.
                int height = 2 + px * 9 / 15;

                for (int py = 12 - height; py < 12; py++)
                {
                    bool crest = py == 12 - height;
                    Rgba color = crest ? new Rgba(255, 244, 200) : ((px + py) % 3 == 0 ? RustLight : new Rgba(230, 170, 90));
                    canvas.Set(px, py, color.WithAlpha(px < 4 ? 140 : 235));
                }
            }

            return canvas;
        }

        /// <summary>O modulo do Steam Dash, 16x16: um par de pistoes de latao.</summary>
        public static PixelCanvas DashModule()
        {
            var canvas = new PixelCanvas(16, 16);

            foreach (int px in new[] { 2, 9 })
            {
                canvas.Fill(px, 2, 5, 9, BrassShade);
                canvas.Fill(px + 1, 2, 3, 9, Brass);
                canvas.Fill(px + 1, 2, 1, 9, BrassShine);
                canvas.Fill(px + 1, 11, 3, 4, Iron);
                canvas.Fill(px + 1, 11, 1, 4, IronLight);
                canvas.Fill(px, 14, 5, 2, IronDark);
            }

            canvas.Fill(6, 5, 4, 2, BrassShade);
            return canvas;
        }

        /// <summary>
        /// Um tile do portao, 16x16: barras de ferro em pe com uma travessa.
        /// Repete na vertical pra fazer um portao da altura que for.
        /// </summary>
        public static PixelCanvas GateBars()
        {
            var canvas = new PixelCanvas(16, 16);

            for (int px = 1; px < 16; px += 5)
            {
                for (int py = 0; py < 16; py++)
                {
                    canvas.Set(px, py, IronLight);
                    canvas.Set(px + 1, py, Iron);
                    canvas.Set(px + 2, py, IronDark);
                }
            }

            // Travessa.
            canvas.Fill(0, 6, 16, 3, Iron);
            canvas.Fill(0, 6, 16, 1, IronLight);
            canvas.Fill(0, 8, 16, 1, IronDark);

            for (int px = 2; px < 16; px += 5) canvas.Set(px, 7, BrassShine);

            return canvas;
        }
    }
}
