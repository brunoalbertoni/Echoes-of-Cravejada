using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// A decoracao do District 2 que nao e tile: o que os operarios deixaram
    /// pra tras. Lampioes, armarios, o cabide das mascaras, as prateleiras
    /// vazias do deposito, tambores, guarda-corpos, os cartazes da Coroa e as
    /// placas das salas.
    ///
    /// Como o resto da arte de latao, e desenhado pixel a pixel aqui (y cresce
    /// pra baixo) e nao depende de UnityEngine.
    /// </summary>
    public static partial class BrassArt
    {
        private static readonly Rgba Wood = new Rgba(120, 78, 48);
        private static readonly Rgba WoodLight = new Rgba(160, 108, 66);
        private static readonly Rgba WoodDark = new Rgba(74, 46, 32);
        private static readonly Rgba Glass = new Rgba(255, 214, 120);
        private static readonly Rgba GlassHot = new Rgba(255, 244, 200);
        private static readonly Rgba Silver = new Rgba(196, 202, 214);
        private static readonly Rgba SilverDark = new Rgba(128, 134, 150);
        private static readonly Rgba Navy = new Rgba(34, 40, 70);
        private static readonly Rgba NavyDark = new Rgba(22, 26, 48);
        private static readonly Rgba Paper = new Rgba(198, 184, 150);
        private static readonly Rgba PaperDark = new Rgba(150, 134, 102);

        /// <summary>Lampiao pendurado, 16x32: a corrente vem do teto e a gaiola acesa fica embaixo.</summary>
        public static PixelCanvas HangingLamp()
        {
            var canvas = new PixelCanvas(16, 32);

            // Corrente: elos alternados.
            for (int py = 0; py < 15; py++)
            {
                bool link = py % 4 < 2;
                canvas.Set(7, py, link ? IronLight : IronDark);
                canvas.Set(8, py, link ? IronMid : IronLight);
            }

            // Chapeu de latao.
            canvas.Fill(4, 15, 8, 1, BrassLight);
            canvas.Fill(3, 16, 10, 2, BrassMid);
            canvas.Fill(3, 17, 10, 1, BrassShade);
            canvas.Fill(6, 14, 4, 1, BrassShine);

            // Vidro aceso, com as barras da gaiola por cima.
            canvas.Fill(4, 18, 8, 9, Glass);
            canvas.Fill(6, 20, 4, 5, GlassHot);
            canvas.Fill(7, 21, 2, 3, FireCore);

            for (int py = 18; py < 27; py++)
            {
                canvas.Set(4, py, BrassShade);
                canvas.Set(8, py, BrassMid);
                canvas.Set(11, py, BrassDark);
            }

            // Base.
            canvas.Fill(3, 27, 10, 2, BrassMid);
            canvas.Fill(3, 28, 10, 1, BrassDark);
            canvas.Fill(6, 29, 4, 1, BrassShade);
            canvas.Set(7, 30, BrassDark);
            canvas.Set(8, 30, BrassDark);
            return canvas;
        }

        /// <summary>Armario de operario, 16x32. Aberto: porta escancarada e vazio por dentro.</summary>
        public static PixelCanvas Locker(bool open)
        {
            var canvas = new PixelCanvas(16, 32);

            // Caixa.
            canvas.Fill(1, 1, 14, 31, IronDark);
            canvas.Fill(1, 1, 14, 1, IronLight);
            canvas.Fill(1, 1, 1, 31, IronMid);
            canvas.Fill(14, 1, 1, 31, Outline);

            if (open)
            {
                // Vazio: so o gancho e a prateleira, e a porta aberta pro lado.
                canvas.Fill(2, 2, 12, 29, Outline);
                canvas.Fill(3, 8, 10, 1, IronMid);
                canvas.Set(8, 12, IronLight);
                canvas.Set(8, 13, IronMid);
                canvas.Fill(0, 3, 2, 27, IronMid);
                canvas.Fill(0, 3, 1, 27, IronLight);
            }
            else
            {
                canvas.Fill(2, 2, 12, 29, IronMid);
                canvas.Fill(2, 2, 12, 1, IronLight);

                // Respiros.
                for (int py = 5; py < 11; py += 2) canvas.Fill(4, py, 8, 1, IronDark);

                // Placa de numero e trinco.
                canvas.Fill(5, 13, 6, 3, BrassMid);
                canvas.Fill(5, 13, 6, 1, BrassLight);
                canvas.Set(7, 14, BrassDark);
                canvas.Set(9, 14, BrassDark);
                canvas.Fill(11, 17, 2, 4, BrassLight);
                canvas.Set(12, 20, BrassShade);
            }

            // Azinhavre subindo do pe.
            Drip(canvas, 3, 26, 4, 11);
            canvas.Fill(1, 30, 14, 2, IronDark);
            return canvas;
        }

        /// <summary>
        /// O cabide das mascaras, 32x24: uma tabua com quatro ganchos. So um
        /// ainda tem mascara; os outros guardam o contorno de onde elas ficavam.
        /// </summary>
        public static PixelCanvas MaskRack()
        {
            var canvas = new PixelCanvas(32, 24);

            canvas.Fill(0, 2, 32, 4, Wood);
            canvas.Fill(0, 2, 32, 1, WoodLight);
            canvas.Fill(0, 5, 32, 1, WoodDark);

            for (int i = 0; i < 4; i++)
            {
                int hx = 3 + i * 8;
                canvas.Set(hx, 6, IronLight);
                canvas.Set(hx, 7, IronMid);
                canvas.Set(hx + 1, 8, IronMid);

                if (i == 2)
                {
                    // A mascara que sobrou: casco de ferro e visor esverdeado.
                    canvas.Fill(hx - 3, 8, 8, 12, IronMid);
                    canvas.Fill(hx - 3, 8, 8, 1, IronLight);
                    canvas.Fill(hx - 2, 11, 6, 3, VerdigrisDark);
                    canvas.Fill(hx - 1, 11, 2, 1, Verdigris);
                    canvas.Fill(hx - 3, 19, 8, 1, Outline);
                    canvas.Set(hx + 4, 14, IronDark);
                }
                else
                {
                    // So a marca na parede onde ela pendia.
                    for (int py = 9; py < 20; py++)
                    {
                        if (py == 9 || py == 19) canvas.Fill(hx - 3, py, 8, 1, new Rgba(30, 22, 20, 120));
                        canvas.Set(hx - 3, py, new Rgba(30, 22, 20, 120));
                        canvas.Set(hx + 4, py, new Rgba(30, 22, 20, 120));
                    }
                }
            }

            Rivet(canvas, 0, 3, true);
            Rivet(canvas, 30, 3, true);
            return canvas;
        }

        /// <summary>Prateleira de ferro do deposito, 32x32: vazia, com o po marcando as caixas que sairam.</summary>
        public static PixelCanvas EmptyShelf()
        {
            var canvas = new PixelCanvas(32, 32);

            canvas.Fill(1, 0, 2, 32, IronMid);
            canvas.Fill(1, 0, 1, 32, IronLight);
            canvas.Fill(29, 0, 2, 32, IronMid);
            canvas.Fill(30, 0, 1, 32, IronDark);

            for (int level = 0; level < 3; level++)
            {
                int py = 9 + level * 10;
                canvas.Fill(1, py, 30, 2, IronLight);
                canvas.Fill(1, py + 1, 30, 1, IronDark);

                // O po: retangulos mais claros onde ficavam as caixas.
                var dust = new Rgba(150, 140, 120, 90);
                int boxX = 4 + Hash(level, 3, 21) % 6;
                canvas.Fill(boxX, py - 1, 9, 1, dust);
                canvas.Fill(boxX + 12, py - 1, 8, 1, dust);
            }

            return canvas;
        }

        /// <summary>Tambor de ferro com cintas de latao, 16x24.</summary>
        public static PixelCanvas Barrel()
        {
            var canvas = new PixelCanvas(16, 24);

            for (int py = 2; py < 24; py++)
            {
                for (int px = 1; px < 15; px++)
                {
                    float nx = (px + 0.5f - 8f) / 7f;
                    canvas.Set(px, py, Shade(IronRamp, Lit(nx * 0.95f, 0f)));
                }
            }

            foreach (int band in new[] { 5, 18 })
            {
                for (int px = 1; px < 15; px++)
                {
                    float nx = (px + 0.5f - 8f) / 7f;
                    canvas.Set(px, band, Shade(BrassRamp, Lit(nx * 0.95f, -0.3f)));
                    canvas.Set(px, band + 1, Shade(BrassRamp, Lit(nx * 0.95f, 0.4f)));
                }
            }

            canvas.Fill(2, 1, 12, 2, IronLight);
            canvas.Fill(3, 0, 10, 1, IronMid);
            Drip(canvas, 10, 7, 9, 5);
            return canvas;
        }

        /// <summary>Guarda-corpo de passarela, 16x16, pra repetir de lado: corrimao e um montante.</summary>
        public static PixelCanvas Railing()
        {
            var canvas = new PixelCanvas(16, 16);

            canvas.Fill(0, 2, 16, 2, BrassLight);
            canvas.Fill(0, 3, 16, 1, BrassShade);
            canvas.Fill(0, 9, 16, 1, BrassMid);

            canvas.Fill(1, 2, 2, 14, BrassMid);
            canvas.Fill(2, 2, 1, 14, BrassShade);
            canvas.Fill(9, 4, 1, 12, BrassShade);
            return canvas;
        }

        /// <summary>
        /// Cartaz da Coroa, 32x32: a coroa de prata dos 925 sobre azul-noite,
        /// rasgado num canto e manchado de verde.
        /// </summary>
        public static PixelCanvas CrownPoster()
        {
            var canvas = new PixelCanvas(32, 32);

            canvas.Fill(1, 1, 30, 30, Navy);
            canvas.Fill(1, 1, 30, 2, NavyDark);
            canvas.Fill(3, 3, 26, 1, Silver);
            canvas.Fill(3, 28, 26, 1, Silver);

            // A coroa: base e cinco pontas.
            canvas.Fill(9, 17, 14, 3, Silver);
            canvas.Fill(9, 19, 14, 1, SilverDark);
            for (int i = 0; i < 5; i++)
            {
                int px = 9 + i * 3;
                int height = i == 2 ? 8 : (i == 1 || i == 3 ? 6 : 5);
                canvas.Fill(px, 17 - height, 2, height, Silver);
                canvas.Set(px, 17 - height - 1, Glass);
            }

            // "925" em tracos, embaixo.
            canvas.Fill(10, 23, 3, 1, Silver);
            canvas.Fill(14, 23, 3, 1, Silver);
            canvas.Fill(18, 23, 3, 1, Silver);
            canvas.Fill(10, 25, 12, 1, SilverDark);

            // Rasgado no canto de baixo, a direita, e uma mancha de Oxide.
            for (int i = 0; i < 7; i++) canvas.Fill(31 - i, 24 + i, i + 1, 1, Rgba.Clear);
            Stain(canvas, 6, 26, 3, 41, 1, 1, 30, 30);

            canvas.Set(2, 2, BrassLight);
            canvas.Set(29, 2, BrassLight);
            return canvas;
        }

        /// <summary>Placa de sala, 48x16: latao fundido com letras gravadas (ilegiveis de proposito, em pixel).</summary>
        public static PixelCanvas RoomSign()
        {
            var canvas = new PixelCanvas(48, 16);

            for (int py = 1; py < 15; py++)
            {
                for (int px = 0; px < 48; px++)
                {
                    float ny = (py + 0.5f - 8f) / 7f;
                    canvas.Set(px, py, Shade(BrassRamp, Lit(0f, ny * 0.8f)));
                }
            }

            canvas.Fill(0, 1, 48, 1, BrassShine);
            canvas.Fill(0, 14, 48, 1, BrassDark);
            canvas.Fill(3, 4, 42, 8, BrassShade);

            // As letras: blocos de largura irregular.
            int x = 5;
            int seed = 0;
            while (x < 42)
            {
                int width = 1 + Hash(seed, 7, 3) % 3;
                int gap = Hash(seed, 9, 5) % 4 == 0 ? 3 : 1;
                canvas.Fill(x, 6, width, 4, BrassLight);
                canvas.Fill(x, 9, width, 1, BrassMid);
                x += width + gap;
                seed++;
            }

            Rivet(canvas, 1, 6);
            Rivet(canvas, 45, 6);
            return canvas;
        }

        /// <summary>
        /// A cabine do elevador, 48x64: uma gaiola de latao com teto em arco,
        /// a roldana do cabo em cima e o piso de chapa embaixo. Os pes do Scrap
        /// ficam na linha do piso (os ultimos 3 pixels).
        /// </summary>
        public static PixelCanvas ElevatorCage()
        {
            var canvas = new PixelCanvas(48, 64);

            // Roldana e o gancho do cabo.
            Ring(canvas, 24f, 5f, 2.2f, 4.6f, BrassRamp);
            canvas.Fill(23, 0, 2, 3, IronLight);
            canvas.Fill(22, 9, 4, 3, IronMid);

            // Teto em arco.
            for (int py = 10; py < 18; py++)
            {
                for (int px = 2; px < 46; px++)
                {
                    float dx = (px + 0.5f - 24f) / 22f;
                    if (py < 10 + (int)(dx * dx * 6f)) continue;
                    canvas.Set(px, py, Shade(BrassRamp, Lit(dx * 0.8f, (py - 14f) / 8f)));
                }
            }

            canvas.Fill(2, 17, 44, 1, BrassDark);

            // As colunas dos cantos e as barras da gaiola.
            canvas.Fill(2, 18, 3, 42, BrassMid);
            canvas.Fill(2, 18, 1, 42, BrassShine);
            canvas.Fill(43, 18, 3, 42, BrassShade);
            canvas.Fill(45, 18, 1, 42, BrassDark);

            for (int px = 9; px < 42; px += 6)
            {
                canvas.Fill(px, 18, 1, 42, BrassLight);
                canvas.Fill(px + 1, 18, 1, 42, BrassShade);
            }

            // Travessa do meio, com uma plaquinha.
            canvas.Fill(2, 36, 44, 2, BrassMid);
            canvas.Fill(2, 36, 44, 1, BrassLight);
            canvas.Fill(19, 34, 10, 5, BrassShade);
            canvas.Fill(20, 35, 8, 3, BrassLight);

            // Piso de chapa.
            canvas.Fill(0, 60, 48, 4, IronMid);
            canvas.Fill(0, 60, 48, 1, IronLight);
            canvas.Fill(0, 63, 48, 1, Outline);
            Rivet(canvas, 3, 61);
            Rivet(canvas, 43, 61);

            Drip(canvas, 40, 20, 8, 31);
            return canvas;
        }

        /// <summary>
        /// Os trilhos do poco do elevador, 48x16, pra repetir em pe: duas guias
        /// de ferro nas bordas, presas por grampos de latao.
        /// </summary>
        public static PixelCanvas ElevatorRail()
        {
            var canvas = new PixelCanvas(48, 16);

            foreach (int x in new[] { 0, 44 })
            {
                canvas.Fill(x, 0, 4, 16, IronMid);
                canvas.Fill(x, 0, 1, 16, IronLight);
                canvas.Fill(x + 3, 0, 1, 16, IronDark);
                canvas.Fill(x, 6, 4, 3, BrassMid);
                canvas.Fill(x, 6, 4, 1, BrassLight);
            }

            // O cabo, no meio.
            for (int py = 0; py < 16; py++)
            {
                canvas.Set(23, py, py % 3 == 0 ? IronLight : IronMid);
                canvas.Set(24, py, IronDark);
            }

            return canvas;
        }

        /// <summary>Corrente pendurada, 16x16, pra repetir em pe.</summary>
        public static PixelCanvas Chain()
        {
            var canvas = new PixelCanvas(16, 16);

            for (int py = 0; py < 16; py++)
            {
                bool wide = py % 8 < 4;
                if (wide)
                {
                    canvas.Set(6, py, IronLight);
                    canvas.Set(9, py, IronDark);
                    if (py % 8 == 0 || py % 8 == 3) canvas.Fill(6, py, 4, 1, IronMid);
                }
                else
                {
                    canvas.Set(7, py, IronLight);
                    canvas.Set(8, py, IronDark);
                }
            }

            return canvas;
        }
    }
}
