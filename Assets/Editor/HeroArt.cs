using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// O Scrap, desenhado por codigo a partir da arte de referencia: cabeca de
    /// minerio bruto com dois olhos acesos, mola no topo, lenco e capa
    /// esfarrapados, corpo redondo de ferrugem, pernas curtas e um cinzel
    /// grande demais pra ele.
    ///
    /// Cada quadro tem 32x32 e olha pra direita. Os pes ficam na ultima linha,
    /// no meio — e o pivo. O desenho e maior que a caixa de colisao (que nao
    /// muda): a cabeca e o cinzel passam dela, de proposito.
    ///
    /// Ha dois visuais com os mesmos quadros: a sucata (Fase 1 — The Cute
    /// Scrap) e o polido (Fase 2 — The Respected Artisan).
    ///
    /// Nao depende de UnityEngine, como o resto da arte gerada.
    /// </summary>
    public static class HeroArt
    {
        public const int Size = 32;

        // A ordem dos quadros. O ScrapAnimator, no jogo, usa os mesmos numeros.
        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Walk = 4;
        public const int WalkFrames = 4;
        public const int Jump = 8;
        public const int Fall = 9;
        public const int Wall = 10;
        public const int Windup = 11;
        public const int Strike = 12;
        public const int Dash = 13;
        public const int Hurt = 14;
        public const int FrameCount = 15;

        /// <summary>As cores de um visual.</summary>
        public sealed class Look
        {
            public Rgba Stone = new Rgba(92, 84, 80);
            public Rgba StoneLight = new Rgba(140, 130, 120);
            public Rgba StoneDark = new Rgba(54, 48, 50);
            public Rgba Fleck = new Rgba(176, 170, 160);

            public Rgba Eye = new Rgba(255, 246, 210);
            public Rgba EyeGlow = new Rgba(255, 206, 120);

            public Rgba Cloth = new Rgba(204, 194, 156);
            public Rgba ClothShade = new Rgba(150, 140, 110);
            public Rgba ClothDark = new Rgba(100, 92, 76);

            public Rgba Rust = new Rgba(150, 90, 56);
            public Rgba RustLight = new Rgba(196, 128, 78);
            public Rgba RustDark = new Rgba(94, 54, 40);

            public Rgba Spring = new Rgba(196, 124, 72);
            public Rgba SpringDark = new Rgba(112, 66, 44);

            public Rgba Handle = new Rgba(134, 86, 52);
            public Rgba HandleDark = new Rgba(84, 52, 36);
            public Rgba Blade = new Rgba(170, 126, 100);
            public Rgba BladeLight = new Rgba(220, 184, 152);
            public Rgba BladeDark = new Rgba(104, 68, 56);
        }

        /// <summary>Fase 1 — The Cute Scrap: pedra bruta, ferrugem e pano cru.</summary>
        public static Look Scrap() => new Look();

        /// <summary>
        /// Fase 2 — The Respected Artisan: quartzo limpo, olhos azuis, bronze
        /// escovado, capa de couro e cinzel afiado.
        /// </summary>
        public static Look Polished()
        {
            return new Look
            {
                Stone = new Rgba(198, 216, 228),
                StoneLight = new Rgba(238, 246, 252),
                StoneDark = new Rgba(140, 164, 186),
                Fleck = new Rgba(255, 255, 255),
                Eye = new Rgba(214, 244, 255),
                EyeGlow = new Rgba(92, 176, 255),
                Cloth = new Rgba(134, 90, 58),
                ClothShade = new Rgba(98, 64, 44),
                ClothDark = new Rgba(66, 42, 34),
                Rust = new Rgba(178, 130, 72),
                RustLight = new Rgba(226, 184, 110),
                RustDark = new Rgba(112, 78, 46),
                Spring = new Rgba(226, 184, 110),
                SpringDark = new Rgba(140, 100, 56),
                Blade = new Rgba(186, 198, 210),
                BladeLight = new Rgba(240, 246, 250),
                BladeDark = new Rgba(116, 128, 146),
            };
        }

        private static readonly Rgba Outline = new Rgba(28, 22, 24);

        private enum Chisel
        {
            /// <summary>No ombro, lamina pra cima: como ele carrega.</summary>
            Shoulder,

            /// <summary>Erguido pra tras: o preparo do golpe.</summary>
            Raised,

            /// <summary>Estendido pra frente: o golpe.</summary>
            Thrust,

            /// <summary>Arrastado atras: no dash.</summary>
            Trailing,
        }

        /// <summary>Tudo o que muda de um quadro pra outro.</summary>
        private sealed class Pose
        {
            /// <summary>Desloca o tronco e a cabeca (positivo e pra baixo, pra direita).</summary>
            public int ShiftX;
            public int ShiftY;

            /// <summary>Quanto cada perna avanca, e quanto ela encolhe.</summary>
            public int LeftLegX;
            public int RightLegX;
            public int LeftLegLift;
            public int RightLegLift;

            public int SpringSway;
            public int SpringHeight = 6;

            /// <summary>0 caida, 1 abrindo, 2 esticada pra tras.</summary>
            public int CapeFlare;

            public Chisel Chisel = Chisel.Shoulder;
            public bool EyesShut;
        }

        /// <summary>Todos os quadros de um visual, na ordem das constantes la de cima.</summary>
        public static PixelCanvas[] Frames(Look look)
        {
            var frames = new PixelCanvas[FrameCount];

            // Parado: respira (o tronco desce 1 px) e a mola balanca.
            frames[Idle + 0] = Draw(look, new Pose());
            frames[Idle + 1] = Draw(look, new Pose { SpringSway = 1 });
            frames[Idle + 2] = Draw(look, new Pose { ShiftY = 1 });
            frames[Idle + 3] = Draw(look, new Pose { ShiftY = 1, SpringSway = -1 });

            // Andando: as pernas se alternam e o tronco sobe e desce.
            frames[Walk + 0] = Draw(look, new Pose { LeftLegX = -2, RightLegX = 2, SpringSway = -1 });
            frames[Walk + 1] = Draw(look, new Pose { ShiftY = 1, LeftLegLift = 1, SpringSway = 0 });
            frames[Walk + 2] = Draw(look, new Pose { LeftLegX = 2, RightLegX = -2, SpringSway = 1 });
            frames[Walk + 3] = Draw(look, new Pose { ShiftY = 1, RightLegLift = 1, SpringSway = 0 });

            // No ar: subindo ele encolhe as pernas e a mola estica; caindo, o contrario.
            frames[Jump] = Draw(look, new Pose { ShiftY = -1, LeftLegLift = 2, RightLegLift = 2, SpringHeight = 4, CapeFlare = 0 });
            frames[Fall] = Draw(look, new Pose { LeftLegX = -1, RightLegX = 1, SpringHeight = 8, CapeFlare = 1 });

            // Na parede: encostado pra frente, uma perna encolhida.
            frames[Wall] = Draw(look, new Pose { ShiftX = 2, RightLegLift = 2, RightLegX = 2, SpringSway = -2, CapeFlare = 1 });

            // O golpe: ergue pra tras, e desce estendido.
            frames[Windup] = Draw(look, new Pose { ShiftX = -1, Chisel = Chisel.Raised, SpringSway = -1 });
            frames[Strike] = Draw(look, new Pose { ShiftX = 2, ShiftY = 1, LeftLegX = -2, RightLegX = 2, Chisel = Chisel.Thrust, SpringSway = 2 });

            frames[Dash] = Draw(look, new Pose { ShiftX = 3, ShiftY = 1, LeftLegX = -3, RightLegX = 1, LeftLegLift = 1, Chisel = Chisel.Trailing, SpringSway = -3, CapeFlare = 2 });
            frames[Hurt] = Draw(look, new Pose { ShiftX = -2, LeftLegX = 1, RightLegX = 2, RightLegLift = 1, SpringSway = 2, EyesShut = true, CapeFlare = 1 });

            return frames;
        }

        private static PixelCanvas Draw(Look look, Pose pose)
        {
            var canvas = new PixelCanvas(Size, Size);

            DrawCape(canvas, look, pose);
            if (pose.Chisel == Chisel.Raised || pose.Chisel == Chisel.Trailing) DrawChisel(canvas, look, pose);
            DrawLegs(canvas, look, pose);
            DrawBody(canvas, look, pose);
            if (pose.Chisel == Chisel.Shoulder || pose.Chisel == Chisel.Thrust) DrawChisel(canvas, look, pose);
            DrawScarf(canvas, look, pose);
            DrawHead(canvas, look, pose);
            DrawSpring(canvas, look, pose);
            DrawHand(canvas, look, pose);
            AddOutline(canvas);

            return canvas;
        }

        private static int Hash(int x, int y, int seed)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + seed * 1442695041;
                h = (h ^ (h >> 13)) * 1274126177;
                return (h ^ (h >> 16)) & 0x7fffffff;
            }
        }

        // Medidas do corpo, antes do deslocamento da pose.
        private const float HeadX = 15.5f;
        private const float HeadY = 13f;
        private const float HeadRadiusX = 7.5f;
        private const float HeadRadiusY = 6.5f;
        private const float BodyX = 15.5f;
        private const float BodyY = 24f;
        private const float BodyRadiusX = 5.6f;
        private const float BodyRadiusY = 4.6f;

        /// <summary>
        /// A capa, por tras de tudo. Caida ela desce das costas (a esquerda) ate
        /// quase o chao; com vento ela abre, e no dash fica esticada pra tras.
        /// </summary>
        private static void DrawCape(PixelCanvas canvas, Look look, Pose pose)
        {
            int top = 18 + pose.ShiftY;
            int length = pose.CapeFlare == 2 ? 6 : (pose.CapeFlare == 1 ? 9 : 11);

            for (int i = 0; i <= length; i++)
            {
                float t = i / (float)length;

                // Quanto mais aberta, mais ela vai pra tras em vez de pra baixo.
                int py = top + (pose.CapeFlare == 2 ? i / 3 : (pose.CapeFlare == 1 ? i * 3 / 4 : i));
                int reach = pose.CapeFlare == 2 ? 3 + i * 2 : (pose.CapeFlare == 1 ? 3 + i : 2 + i / 2);

                int right = 17 + pose.ShiftX;
                int left = 10 + pose.ShiftX - reach;

                for (int px = left; px <= right; px++)
                {
                    // A barra e esfarrapada: dentes irregulares nas duas ultimas linhas.
                    if (i >= length - 1 && Hash(px, 0, 5) % 3 == 0) continue;
                    if (i == length && Hash(px, 1, 5) % 2 == 0) continue;

                    Rgba color = px <= left + 1 ? look.ClothDark : (px <= left + 3 || t > 0.7f ? look.ClothShade : look.Cloth);
                    canvas.Set(px, py, color);
                }
            }
        }

        private static void DrawLegs(PixelCanvas canvas, Look look, Pose pose)
        {
            DrawLeg(canvas, look, 11 + pose.ShiftX / 2 + pose.LeftLegX, pose.LeftLegLift);
            DrawLeg(canvas, look, 17 + pose.ShiftX / 2 + pose.RightLegX, pose.RightLegLift);
        }

        private static void DrawLeg(PixelCanvas canvas, Look look, int x, int lift)
        {
            int bottom = Size - 1 - lift;

            for (int py = 27; py <= bottom; py++)
            {
                canvas.Set(x, py, look.RustDark);
                canvas.Set(x + 1, py, look.Rust);
                canvas.Set(x + 2, py, look.RustDark);
            }

            // O pe: um bloco mais largo, virado pra frente.
            canvas.Fill(x, bottom - 1, 4, 2, look.RustDark);
            canvas.Set(x + 1, bottom - 1, look.RustLight);
            canvas.Set(x + 2, bottom - 1, look.Rust);
        }

        /// <summary>O corpo: um barril de ferrugem, com uma cinta rebitada.</summary>
        private static void DrawBody(PixelCanvas canvas, Look look, Pose pose)
        {
            float cx = BodyX + pose.ShiftX;
            float cy = BodyY + pose.ShiftY;

            for (int py = 0; py < Size; py++)
            {
                for (int px = 0; px < Size; px++)
                {
                    float dx = (px + 0.5f - cx) / BodyRadiusX;
                    float dy = (py + 0.5f - cy) / BodyRadiusY;
                    if (dx * dx + dy * dy > 1f) continue;

                    // Luz de cima e da esquerda.
                    float light = -dx * 0.6f - dy * 0.5f;
                    Rgba color = light > 0.35f ? look.RustLight : (light > -0.3f ? look.Rust : look.RustDark);
                    canvas.Set(px, py, color);
                }
            }

            int belt = (int)cy + 1;
            for (int px = 0; px < Size; px++)
            {
                float dx = (px + 0.5f - cx) / BodyRadiusX;
                float dy = (belt + 0.5f - cy) / BodyRadiusY;
                if (dx * dx + dy * dy > 1f) continue;

                canvas.Set(px, belt, (px + pose.ShiftX) % 3 == 1 ? look.RustLight : look.RustDark);
            }
        }

        /// <summary>O lenco amarrado no pescoco, com as duas pontas caindo na frente.</summary>
        private static void DrawScarf(PixelCanvas canvas, Look look, Pose pose)
        {
            int neck = 19 + pose.ShiftY;
            int ox = pose.ShiftX;

            for (int px = 9; px <= 22; px++)
            {
                canvas.Set(px + ox, neck, look.Cloth);
                canvas.Set(px + ox, neck + 1, px < 12 ? look.ClothShade : look.Cloth);
                if (px > 10 && px < 21) canvas.Set(px + ox, neck + 2, look.ClothShade);
            }

            // O no e as pontas.
            canvas.Fill(17 + ox, neck + 1, 2, 2, look.Cloth);
            canvas.Set(17 + ox, neck + 3, look.Cloth);
            canvas.Set(17 + ox, neck + 4, look.ClothShade);
            canvas.Set(19 + ox, neck + 3, look.Cloth);
            canvas.Set(19 + ox, neck + 4, look.Cloth);
            canvas.Set(20 + ox, neck + 5, look.ClothShade);
        }

        /// <summary>
        /// A cabeca: uma pedra bruta, facetada. Cada pixel pega o tom da faceta
        /// mais proxima, e onde duas facetas se encontram fica um vinco escuro.
        /// </summary>
        private static void DrawHead(PixelCanvas canvas, Look look, Pose pose)
        {
            float cx = HeadX + pose.ShiftX;
            float cy = HeadY + pose.ShiftY;

            // Os centros das facetas, em volta do centro da cabeca, e o tom de cada uma.
            float[,] facets =
            {
                { -4.5f, -3.5f, 2 }, { 0.5f, -4.5f, 2 }, { 4.5f, -2.5f, 1 },
                { -5.5f, 1.0f, 1 }, { 0.0f, 0.5f, 1 }, { 5.0f, 2.0f, 0 },
                { -3.0f, 4.5f, 0 }, { 2.5f, 4.5f, 0 },
            };

            for (int py = 0; py < Size; py++)
            {
                for (int px = 0; px < Size; px++)
                {
                    float dx = px + 0.5f - cx;
                    float dy = py + 0.5f - cy;

                    // Contorno irregular: o raio varia com o angulo.
                    double angle = Math.Atan2(dy, dx);
                    float bump = 1f + 0.07f * (float)Math.Sin(angle * 5.0 + 1.3) + 0.05f * (float)Math.Sin(angle * 9.0);

                    float ex = dx / (HeadRadiusX * bump);
                    float ey = dy / (HeadRadiusY * bump);
                    if (ex * ex + ey * ey > 1f) continue;

                    float nearest = 999f;
                    float second = 999f;
                    int tone = 1;

                    for (int i = 0; i < facets.GetLength(0); i++)
                    {
                        float fx = dx - facets[i, 0];
                        float fy = dy - facets[i, 1];
                        float distance = fx * fx + fy * fy;

                        if (distance < nearest)
                        {
                            second = nearest;
                            nearest = distance;
                            tone = (int)facets[i, 2];
                        }
                        else if (distance < second)
                        {
                            second = distance;
                        }
                    }

                    Rgba color = tone == 2 ? look.StoneLight : (tone == 1 ? look.Stone : look.StoneDark);

                    // Vinco entre facetas.
                    if (Math.Sqrt(second) - Math.Sqrt(nearest) < 0.7) color = look.StoneDark;

                    // Lasquinhas claras. O sorteio usa a posicao na cabeca, e nao
                    // na tela, senao elas piscam de um quadro pro outro.
                    if (Hash((int)Math.Floor(dx + 16f), (int)Math.Floor(dy + 16f), 21) % 23 == 0) color = look.Fleck;

                    canvas.Set(px, py, color);
                }
            }

            DrawEye(canvas, look, (int)cx - 4, (int)cy - 1, false, pose.EyesShut);
            DrawEye(canvas, look, (int)cx + 2, (int)cy - 1, true, pose.EyesShut);
        }

        /// <summary>
        /// Um olho: um oval aceso, inclinado pra dentro, com o canto de cima
        /// cortado. E esse corte que da a expressao de teimoso da referencia.
        /// </summary>
        private static void DrawEye(PixelCanvas canvas, Look look, int x, int y, bool right, bool shut)
        {
            // Fundo escuro em volta, pra o brilho saltar.
            canvas.Fill(x - 1, y - 1, 5, 6, look.StoneDark);

            if (shut)
            {
                canvas.Fill(x, y + 2, 3, 1, look.EyeGlow);
                return;
            }

            // Linha a linha: quais das 3 colunas estao acesas.
            bool[,] shape =
            {
                { false, true, true },
                { true, true, true },
                { true, true, true },
                { true, true, false },
            };

            for (int row = 0; row < 4; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    // O olho direito e o esquerdo espelhado.
                    if (!shape[row, right ? 2 - column : column]) continue;

                    bool core = row == 1 || row == 2;
                    canvas.Set(x + column, y + row, core ? look.Eye : look.EyeGlow);
                }
            }
        }

        /// <summary>A mola no topo da cabeca. O balanco desloca mais as voltas de cima.</summary>
        private static void DrawSpring(PixelCanvas canvas, Look look, Pose pose)
        {
            int baseY = (int)(HeadY - HeadRadiusY) + pose.ShiftY;
            int baseX = (int)HeadX + 2 + pose.ShiftX;

            for (int i = 0; i < pose.SpringHeight; i++)
            {
                int py = baseY - i;
                if (py < 0) break;

                int sway = pose.SpringSway * i / Math.Max(1, pose.SpringHeight - 1);
                int zig = i % 2 == 0 ? 0 : 2;

                canvas.Set(baseX + zig + sway, py, look.Spring);
                canvas.Set(baseX + 1 + sway, py, i % 2 == 0 ? look.SpringDark : look.Spring);
            }
        }

        /// <summary>
        /// O cinzel: um cabo comprido e uma lamina larga, que alarga na ponta.
        /// E do tamanho do Scrap, de proposito.
        /// </summary>
        private static void DrawChisel(PixelCanvas canvas, Look look, Pose pose)
        {
            // Onde a mao segura, onde a lamina comeca e onde ela termina.
            float handX, handY, neckX, neckY, tipX, tipY;

            switch (pose.Chisel)
            {
                case Chisel.Raised:
                    handX = 17; handY = 22; neckX = 9; neckY = 9; tipX = 5; tipY = 1;
                    break;

                case Chisel.Thrust:
                    handX = 17; handY = 23; neckX = 23; neckY = 22; tipX = 31; tipY = 21;
                    break;

                case Chisel.Trailing:
                    handX = 17; handY = 25; neckX = 8; neckY = 24; tipX = 1; tipY = 24;
                    break;

                default:
                    handX = 18; handY = 26; neckX = 25; neckY = 13; tipX = 29; tipY = 3;
                    break;
            }

            handX += pose.ShiftX;
            handY += pose.ShiftY;

            // A ponta nao sai do quadro por causa do deslocamento do corpo.
            if (pose.Chisel == Chisel.Shoulder)
            {
                neckX += pose.ShiftX;
                neckY += pose.ShiftY;
                tipX = Math.Min(Size - 1, tipX + pose.ShiftX);
                tipY += pose.ShiftY;
            }

            // Um pouco do cabo sobra pra tras da mao.
            float backX = handX - (neckX - handX) * 0.22f;
            float backY = handY - (neckY - handY) * 0.22f;

            Stroke(canvas, backX, backY, neckX, neckY, 1.1f, 1.1f, look.Handle, look.Handle, look.HandleDark);
            Stroke(canvas, neckX, neckY, tipX, tipY, 1.4f, 3.2f, look.Blade, look.BladeLight, look.BladeDark);
        }

        /// <summary>
        /// Uma faixa de um ponto a outro, com meia largura que vai de um valor
        /// a outro. Um lado dela e claro e o outro escuro.
        /// </summary>
        private static void Stroke(PixelCanvas canvas, float x0, float y0, float x1, float y1,
            float halfStart, float halfEnd, Rgba body, Rgba light, Rgba dark)
        {
            float dx = x1 - x0;
            float dy = y1 - y0;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            if (length < 0.01f) return;

            // Perpendicular a faixa.
            float nx = -dy / length;
            float ny = dx / length;

            int minX = (int)Math.Floor(Math.Min(x0, x1) - halfEnd - 1);
            int maxX = (int)Math.Ceiling(Math.Max(x0, x1) + halfEnd + 1);
            int minY = (int)Math.Floor(Math.Min(y0, y1) - halfEnd - 1);
            int maxY = (int)Math.Ceiling(Math.Max(y0, y1) + halfEnd + 1);

            for (int py = minY; py <= maxY; py++)
            {
                for (int px = minX; px <= maxX; px++)
                {
                    float rx = px + 0.5f - x0;
                    float ry = py + 0.5f - y0;

                    float along = (rx * dx + ry * dy) / (length * length);
                    if (along < 0f || along > 1f) continue;

                    float across = rx * nx + ry * ny;
                    float half = halfStart + (halfEnd - halfStart) * along;
                    if (Math.Abs(across) > half) continue;

                    // O lado de cima (ou da esquerda) pega a luz.
                    bool upper = (ny < 0f ? across : -across) > half * 0.35f || (Math.Abs(ny) < 0.2f && (nx < 0f ? across : -across) > half * 0.35f);
                    bool lower = (ny < 0f ? -across : across) > half * 0.35f || (Math.Abs(ny) < 0.2f && (nx < 0f ? -across : across) > half * 0.35f);

                    canvas.Set(px, py, upper ? light : (lower ? dark : body));
                }
            }
        }

        /// <summary>A mao, por cima do cabo: e o que faz o cinzel parecer segurado.</summary>
        private static void DrawHand(PixelCanvas canvas, Look look, Pose pose)
        {
            int hx = pose.Chisel == Chisel.Shoulder ? 17 : 16;
            int hy = pose.Chisel == Chisel.Shoulder ? 24 : (pose.Chisel == Chisel.Trailing ? 24 : 21);

            hx += pose.ShiftX;
            hy += pose.ShiftY;

            canvas.Fill(hx, hy, 3, 3, look.RustDark);
            canvas.Fill(hx, hy, 2, 2, look.RustLight);
        }

        /// <summary>
        /// Contorna tudo de escuro. E o que faz um sprite pequeno ler bem em
        /// cima de qualquer fundo — e o fundo deste jogo e quase preto.
        /// </summary>
        private static void AddOutline(PixelCanvas canvas)
        {
            var filled = new bool[Size * Size];
            for (int i = 0; i < filled.Length; i++) filled[i] = canvas.Pixels[i].A != 0;

            for (int py = 0; py < Size; py++)
            {
                for (int px = 0; px < Size; px++)
                {
                    if (filled[px + py * Size]) continue;

                    bool touches = (px > 0 && filled[px - 1 + py * Size])
                        || (px < Size - 1 && filled[px + 1 + py * Size])
                        || (py > 0 && filled[px + (py - 1) * Size])
                        || (py < Size - 1 && filled[px + (py + 1) * Size]);

                    if (touches) canvas.Set(px, py, Outline);
                }
            }
        }
    }
}
