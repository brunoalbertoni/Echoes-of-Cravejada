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

        // ------------------------------------------------------------------
        // O rastejante de engrenagem
        // ------------------------------------------------------------------

        /// <summary>Quantos quadros tem a caminhada do rastejante de engrenagem.</summary>
        public const int CogCrawlerFrames = 4;

        private static readonly Rgba Copper = new Rgba(166, 96, 58);
        private static readonly Rgba CopperLight = new Rgba(212, 138, 84);
        private static readonly Rgba CopperDark = new Rgba(98, 54, 38);
        private static readonly Rgba OxideGlow = new Rgba(120, 255, 170);
        private static readonly Rgba OxideCore = new Rgba(230, 255, 235);
        private static readonly Rgba Steel = new Rgba(118, 116, 130);
        private static readonly Rgba SteelLight = new Rgba(170, 168, 180);
        private static readonly Rgba SteelDark = new Rgba(72, 70, 84);

        private const int ShellTeeth = 8;

        /// <summary>
        /// Um quadro do rastejante de engrenagem, 24x16, olhando pra esquerda:
        /// um bicho de relojoaria do poco. O casco e uma engrenagem grande de
        /// cobre, a cabeca e um pinhao de ferro, e nas costas ainda esta a
        /// chave de dar corda.
        ///
        /// O Oxide comeu o cobre: azinhavre por cima (mais embaixo, onde
        /// escorre), dois dentes partidos, uma rachadura que vaza verde e o
        /// olho aceso da mesma cor.
        ///
        /// O casco nao gira — a mancha e os dentes quebrados ficam no lugar e
        /// o ciclo fecha sem salto. Quem mexe sao as patas, o mecanismo que se
        /// ve pelo miolo do casco e a chave nas costas.
        /// </summary>
        public static PixelCanvas CogCrawler(int frame)
        {
            frame = (frame % CogCrawlerFrames + CogCrawlerFrames) % CogCrawlerFrames;

            // Nos quadros em que uma das patas esta no ar, o corpo sobe um pixel.
            int bob = frame % 2 == 1 ? -1 : 0;

            var legs = new PixelCanvas(24, 16);
            var body = new PixelCanvas(24, 16);

            CogCrawlerLegs(legs, frame, bob);
            CogCrawlerShell(body, frame, bob);
            CogCrawlerHead(body, bob);
            CogCrawlerKey(body, frame, bob);
            AddOutline(body);
            CogCrawlerAntennae(body, bob);

            legs.Blit(body, 0, 0);
            return legs;
        }

        /// <summary>
        /// Tres patas de arame, em tripe: a da frente e a de tras andam juntas,
        /// a do meio no contratempo. Cada uma fica tres quadros no chao,
        /// empurrando pra tras, e um no ar, voltando pra frente.
        /// </summary>
        private static void CogCrawlerLegs(PixelCanvas canvas, int frame, int bob)
        {
            int[] hips = { 9, 13, 17 };

            // Onde o pe fica em relacao ao quadril, e se esta no ar.
            int[] offset = { -1, 0, 1, 0 };
            bool[] lifted = { false, false, false, true };

            for (int leg = 0; leg < hips.Length; leg++)
            {
                int step = (frame + (leg == 1 ? 2 : 0)) % 4;
                int hipX = hips[leg];
                int hipY = 10 + bob;
                int footX = hipX + offset[step] - 1;
                int footY = lifted[step] ? 14 : 15;

                // O joelho dobra pra fora do corpo, pra tras.
                int kneeX = hipX + 1;
                int kneeY = 12 + bob;

                Line(canvas, hipX, hipY, kneeX, kneeY, Steel);
                Line(canvas, kneeX, kneeY, footX, footY, SteelLight);
                canvas.Set(kneeX, kneeY, SteelDark);
                canvas.Set(footX, footY, SteelDark);
            }
        }

        /// <summary>O casco: a engrenagem de cobre, o aro, e o mecanismo de latao girando la dentro.</summary>
        private static void CogCrawlerShell(PixelCanvas canvas, int frame, int bob)
        {
            const float cx = 13.5f;
            float cy = 6.5f + bob;
            const float tip = 6.3f;
            const float root = 4.6f;
            const float window = 2.6f;

            // Um quarto de volta a cada ciclo: as quatro varetas se repetem.
            double spin = frame * (Math.PI / 2.0) / CogCrawlerFrames;

            for (int py = 0; py < 16; py++)
            {
                for (int px = 0; px < 24; px++)
                {
                    float dx = px + 0.5f - cx;
                    float dy = py + 0.5f - cy;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    double angle = Math.Atan2(dy, dx);

                    // Os dentes: onde o cosseno passa do limiar, a borda vai ate a ponta.
                    bool tooth = Math.Cos(angle * ShellTeeth) > 0.4;
                    int toothIndex = ((int)Math.Round(angle * ShellTeeth / (2.0 * Math.PI)) % ShellTeeth + ShellTeeth) % ShellTeeth;

                    float edge = tooth ? tip : root;

                    // Dois dentes partidos, em cima e atras: um sumiu, o outro e um toco.
                    if (tooth && toothIndex == 7) edge = root;
                    if (tooth && toothIndex == 6) edge = root + 0.6f;

                    if (r > edge) continue;

                    Rgba color;

                    if (r < 1f)
                    {
                        // O eixo.
                        color = BrassShine;
                    }
                    else if (r < window)
                    {
                        // O miolo: o escuro de dentro, com as varetas de latao.
                        double nearest = Math.Abs(Math.IEEERemainder(angle - spin, Math.PI / 2.0));
                        bool spoke = nearest * r < 0.7;
                        color = spoke ? Brass : IronDark;
                    }
                    else if (r < window + 0.8f)
                    {
                        // O sulco entre o miolo e o aro.
                        color = CopperDark;
                    }
                    else
                    {
                        // O aro e os dentes, com a luz vindo de cima e da esquerda.
                        float light = -(dx + dy) / Math.Max(r, 0.01f);
                        color = light > 0.5f ? CopperLight : (light < -0.6f ? CopperDark : Copper);

                        // O azinhavre: em manchas de 2x2 no aro, nao nos dentes, e
                        // mais denso embaixo, onde escorre.
                        int local = py - bob;
                        float chance = 0.15f + 0.65f * (local / 13f);
                        if (r <= root && Noise(px >> 1, local >> 1) < chance)
                        {
                            color = light < -0.2f ? VerdigrisDark : Verdigris;
                        }
                    }

                    canvas.Set(px, py, color);
                }
            }

            // A rachadura no aro, de cima pra dentro, vazando o verde do Oxide.
            int top = (int)Math.Floor(cy);
            (int X, int Y, bool Glow)[] crack =
            {
                (11, top - 4, false),
                (11, top - 3, true),
                (12, top - 3, false),
                (12, top - 2, true),
            };

            foreach ((int X, int Y, bool Glow) piece in crack)
            {
                canvas.Set(piece.X, piece.Y, piece.Glow ? OxideGlow : Outline);
            }
        }

        /// <summary>A cabeca: um pinhao de ferro na frente do casco, com o olho aceso pelo Oxide.</summary>
        private static void CogCrawlerHead(PixelCanvas canvas, int bob)
        {
            const float cx = 5.5f;
            float cy = 8.5f + bob;
            const float tip = 3.4f;
            const float root = 2.6f;

            for (int py = 0; py < 16; py++)
            {
                for (int px = 0; px < 12; px++)
                {
                    float dx = px + 0.5f - cx;
                    float dy = py + 0.5f - cy;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    bool tooth = Math.Cos(Math.Atan2(dy, dx) * 6) > 0.2;

                    if (r > (tooth ? tip : root)) continue;

                    float light = -(dx + dy) / Math.Max(r, 0.01f);
                    canvas.Set(px, py, light > 0.4f ? SteelLight : (light < -0.5f ? SteelDark : Steel));
                }
            }

            // O olho, virado pra frente: uma fresta escura com o verde no fundo.
            int eyeY = (int)Math.Floor(cy) - 1;
            canvas.Fill(2, eyeY, 4, 2, Outline);
            canvas.Fill(2, eyeY, 3, 1, OxideGlow);
            canvas.Set(2, eyeY, OxideCore);

            // As pincas: dois dentes de ferro embaixo da cabeca.
            int jawY = (int)Math.Floor(cy) + 2;
            canvas.Set(2, jawY, SteelLight);
            canvas.Set(1, jawY + 1, Steel);
            canvas.Set(4, jawY + 1, SteelLight);
            canvas.Set(3, jawY + 2, Steel);
        }

        /// <summary>
        /// A chave de dar corda, espetada nas costas. Ela gira: vista de lado,
        /// a argola alarga e afina a cada quadro.
        /// </summary>
        private static void CogCrawlerKey(PixelCanvas canvas, int frame, int bob)
        {
            int[] widths = { 3, 2, 1, 2 };
            int width = widths[frame];
            int stemY = 5 + bob;

            canvas.Fill(19, stemY, 2, 1, BrassShade);

            int left = 22 - width / 2;
            for (int py = stemY - 2; py <= stemY + 2; py++)
            {
                for (int px = left; px < left + width; px++)
                {
                    // A argola e vazada no meio quando esta de frente.
                    bool hole = width == 3 && px == left + 1 && py != stemY - 2 && py != stemY + 2 && py != stemY;
                    if (hole) continue;

                    canvas.Set(px, py, px == left ? Brass : BrassShade);
                }
            }

            canvas.Set(left, stemY - 2, BrassShine);
        }

        /// <summary>Duas antenas de arame, curvadas pra frente. Ficam sem contorno, finas.</summary>
        private static void CogCrawlerAntennae(PixelCanvas canvas, int bob)
        {
            int baseY = 4 + bob;

            Line(canvas, 4, baseY, 2, baseY - 3, Steel);
            canvas.Set(1, baseY - 3, OxideGlow);

            Line(canvas, 6, baseY, 5, baseY - 3, SteelDark);
            canvas.Set(4, baseY - 4, Verdigris);
        }

        /// <summary>Contorna de escuro tudo o que nao e transparente, por fora.</summary>
        private static void AddOutline(PixelCanvas canvas)
        {
            var edges = new System.Collections.Generic.List<(int X, int Y)>();

            for (int py = 0; py < canvas.Height; py++)
            {
                for (int px = 0; px < canvas.Width; px++)
                {
                    if (canvas.Get(px, py).A != 0) continue;

                    bool touches = canvas.Get(px - 1, py).A != 0 || canvas.Get(px + 1, py).A != 0
                        || canvas.Get(px, py - 1).A != 0 || canvas.Get(px, py + 1).A != 0;

                    if (touches) edges.Add((px, py));
                }
            }

            foreach ((int X, int Y) edge in edges) canvas.Set(edge.X, edge.Y, Outline);
        }

        // ------------------------------------------------------------------
        // As ferramentas do ourives
        // ------------------------------------------------------------------

        /// <summary>Quantas ferramentas do ourives ha.</summary>
        public const int MasterToolCount = 3;

        private static readonly Rgba Wood = new Rgba(150, 96, 52);
        private static readonly Rgba WoodLight = new Rgba(198, 140, 82);
        private static readonly Rgba WoodDark = new Rgba(96, 58, 34);
        private static readonly Rgba Glass = new Rgba(150, 210, 230);
        private static readonly Rgba GlassShine = new Rgba(235, 250, 255);

        /// <summary>
        /// Uma ferramenta do mestre do Rebite, 12x12, contornada pra se ler
        /// solta no chao: 0 o buril (cabo de cogumelo e lamina), 1 a lupa
        /// (aro de latao e vidro), 2 a pinca (duas hastes que se fecham na ponta).
        /// </summary>
        public static PixelCanvas MasterTool(int kind)
        {
            var canvas = new PixelCanvas(12, 12);

            switch (kind)
            {
                case 0:
                    // O cabo redondo, embaixo a esquerda.
                    canvas.Fill(1, 7, 4, 3, Wood);
                    canvas.Fill(2, 6, 2, 1, WoodLight);
                    canvas.Fill(2, 10, 2, 1, WoodDark);
                    canvas.Set(1, 7, WoodLight);
                    canvas.Set(4, 9, WoodDark);

                    // A virola de latao e a lamina subindo na diagonal.
                    canvas.Fill(5, 6, 2, 2, Brass);
                    Line(canvas, 6, 5, 10, 1, IronLight);
                    Line(canvas, 7, 5, 10, 2, Iron);
                    canvas.Set(10, 1, BrassShine);
                    break;

                case 1:
                    // O aro e o vidro.
                    for (int py = 0; py < 12; py++)
                    {
                        for (int px = 0; px < 12; px++)
                        {
                            float dx = px + 0.5f - 5f;
                            float dy = py + 0.5f - 5f;
                            float r = (float)Math.Sqrt(dx * dx + dy * dy);

                            if (r <= 2.6f) canvas.Set(px, py, Glass);
                            else if (r <= 3.8f) canvas.Set(px, py, dx + dy < 0f ? BrassShine : Brass);
                        }
                    }

                    canvas.Set(4, 3, GlassShine);
                    canvas.Set(3, 4, GlassShine);

                    // O cabo, saindo pra baixo e pra direita.
                    Line(canvas, 8, 8, 10, 10, Wood);
                    Line(canvas, 9, 8, 11, 10, WoodDark);
                    canvas.Set(8, 8, BrassShade);
                    break;

                default:
                    // Unidas em cima, abertas no meio, fechadas na ponta.
                    Line(canvas, 5, 1, 3, 5, IronLight);
                    Line(canvas, 3, 5, 5, 10, IronLight);
                    Line(canvas, 6, 1, 8, 5, Iron);
                    Line(canvas, 8, 5, 6, 10, Iron);
                    canvas.Fill(5, 0, 2, 2, Brass);
                    canvas.Set(5, 11, IronLight);
                    canvas.Set(6, 11, Iron);
                    break;
            }

            AddOutline(canvas);
            return canvas;
        }

        // ------------------------------------------------------------------
        // Recompensas escondidas
        // ------------------------------------------------------------------

        private static readonly Rgba GemLight = new Rgba(200, 245, 255);
        private static readonly Rgba GemMid = new Rgba(110, 200, 230);
        private static readonly Rgba GemDark = new Rgba(50, 120, 170);

        /// <summary>
        /// Uma gema lapidada, 12x12: um losango facetado azul-claro, com a
        /// mesa (a face de cima) mais clara e um brilho no canto.
        /// </summary>
        public static PixelCanvas CutGem()
        {
            var canvas = new PixelCanvas(12, 12);

            // A coroa: a faixa de cima, larga e baixa.
            canvas.Fill(3, 2, 6, 1, GemLight);
            canvas.Fill(2, 3, 8, 2, GemMid);
            canvas.Fill(4, 3, 4, 1, GemLight);

            // O pavilhao: afunila ate a ponta de baixo.
            for (int row = 0; row < 5; row++)
            {
                int half = 4 - row;
                canvas.Fill(6 - half, 5 + row, half * 2, 1, row % 2 == 0 ? GemMid : GemDark);
                canvas.Set(6 - half, 5 + row, GemDark);
            }

            // As arestas da mesa e o brilho no canto de cima.
            canvas.Fill(2, 4, 8, 1, GemDark);
            canvas.Set(3, 3, BrassShine.WithAlpha(255));
            canvas.Set(4, 3, Rgba.Lerp(GemLight, new Rgba(255, 255, 255), 0.6f));

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>
        /// Um cofre de fragmentos, 14x12: uma caixinha de latao com cintas de
        /// ferro, entreaberta, com fragmentos aparecendo pela fresta.
        /// </summary>
        public static PixelCanvas ShardCache()
        {
            var canvas = new PixelCanvas(14, 12);

            // A caixa.
            canvas.Fill(1, 5, 12, 6, BrassShade);
            canvas.Fill(1, 5, 12, 1, Brass);
            canvas.Fill(4, 5, 1, 6, IronDark);
            canvas.Fill(9, 5, 1, 6, IronDark);
            canvas.Fill(6, 7, 2, 2, IronLight);

            // A tampa, entreaberta.
            canvas.Fill(1, 2, 12, 2, Brass);
            canvas.Fill(1, 2, 12, 1, BrassShine);

            // Os fragmentos pela fresta, azulados como os opacos.
            var shard = new Rgba(150, 175, 190);
            var shardLight = new Rgba(215, 230, 238);
            canvas.Fill(3, 4, 2, 1, shard);
            canvas.Fill(7, 4, 3, 1, shard);
            canvas.Set(8, 4, shardLight);
            canvas.Set(4, 4, shardLight);

            AddOutline(canvas);
            return canvas;
        }

        /// <summary>Uma linha de pixels entre dois pontos (Bresenham).</summary>
        private static void Line(PixelCanvas canvas, int x0, int y0, int x1, int y1, Rgba color)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = -Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int error = dx + dy;

            while (true)
            {
                canvas.Set(x0, y0, color);
                if (x0 == x1 && y0 == y1) return;

                int doubled = 2 * error;
                if (doubled >= dy) { error += dy; x0 += sx; }
                if (doubled <= dx) { error += dx; y0 += sy; }
            }
        }

        /// <summary>Um numero entre 0 e 1 que so depende da celula: o mesmo em todo quadro.</summary>
        private static float Noise(int x, int y)
        {
            unchecked
            {
                int hash = x * 374761393 + y * 668265263;
                hash = (hash ^ (hash >> 13)) * 1274126177;
                return ((hash ^ (hash >> 16)) & 1023) / 1023f;
            }
        }
    }
}
