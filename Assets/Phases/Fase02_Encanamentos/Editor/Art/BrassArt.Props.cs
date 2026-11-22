using System;

namespace EchoesOfCravejada.EditorTools
{
    public static partial class BrassArt
    {
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
