using System;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// Os quadros da abertura do jogo: o Scrap caindo pela Calha N.º 1,
    /// rolando no ar, se esborrachando no monte de sucata, apagado, os olhos
    /// pegando e ele se levantando apoiado no cinzel.
    ///
    /// E o proprio boneco do HeroArt (mesmas pecas, cores e contorno), posto
    /// em poses novas e girado. O giro e feito no estilo RotSprite: o quadro e
    /// ampliado tres vezes com Scale2x (que arredonda as diagonais sem inventar
    /// cor) e so entao amostrado no angulo pedido, pra nao virar serrilhado.
    ///
    /// Duas folhas:
    ///   Scrap — 80x64 por quadro, pivo embaixo no meio, 32 px por tile (o do
    ///           Scrap). Nos quadros no chao, o tronco fica sempre na mesma
    ///           coluna, entao levantar nao arrasta o boneco de lado.
    ///   Fx    — 32x32, pivo no meio, 32 px por tile: a poeira do tombo, as
    ///           pecas de sucata que voam e a faisca.
    ///
    /// Nao depende de UnityEngine, como o resto da arte gerada.
    /// </summary>
    public static class IntroFallArt
    {
        public const int Width = 80;
        public const int Height = 64;
        public const int PixelsPerUnit = HeroArt.PixelsPerUnit;

        // A folha do Scrap. O WakeUpCutscene, no jogo, usa os mesmos numeros.
        public const int Tumble = 0;
        public const int TumbleFrames = 8;
        public const int Slam = 8;
        public const int Bounce = 9;
        public const int LieOff = 10;
        public const int LieDim = 11;
        public const int LieOn = 12;
        public const int LiftHead = 13;
        public const int Prop = 14;
        public const int Kneel = 15;
        public const int Crouch = 16;
        public const int FrameCount = 17;

        // A folha dos efeitos.
        public const int FxSize = 32;
        public const int Dust = 0;
        public const int DustFrames = 6;
        public const int Debris = 6;
        public const int DebrisFrames = 6;
        public const int Spark = 12;
        public const int SparkFrames = 2;
        public const int FxCount = 14;

        // O tronco do HeroArt, no quadro de 48: o pivo dos giros no chao.
        private const float TorsoX = 23.5f;
        private const float TorsoY = 37.5f;

        // ------------------------------------------------------------------
        // Os olhos
        // ------------------------------------------------------------------

        /// <summary>Apagado: as orbitas escuras, sem halo.</summary>
        private static HeroArt.Look Off()
        {
            HeroArt.Look look = HeroArt.Scrap();
            look.EyeCore = new Rgba(62, 56, 58);
            look.EyeGlow = new Rgba(40, 36, 40);
            look.EyeHalo = look.Stone[1];
            return look;
        }

        /// <summary>Pegando: uma brasa fraca no fundo do olho.</summary>
        private static HeroArt.Look Dim()
        {
            HeroArt.Look look = HeroArt.Scrap();
            look.EyeCore = new Rgba(214, 160, 92);
            look.EyeGlow = new Rgba(128, 84, 54);
            look.EyeHalo = look.Stone[1];
            return look;
        }

        // ------------------------------------------------------------------
        // A folha do Scrap
        // ------------------------------------------------------------------

        public static PixelCanvas[] Frames()
        {
            var frames = new PixelCanvas[FrameCount];
            HeroArt.Look off = Off();
            HeroArt.Look on = HeroArt.Scrap();

            // Rolando no ar: encolhido, a capa batendo, girando pra frente de 45 em 45.
            for (int i = 0; i < TumbleFrames; i++)
            {
                PixelCanvas tuck = HeroArt.DrawPose(off, TuckPose(i));
                frames[Tumble + i] = Spin(tuck, i * 45f);
            }

            // O tombo: deitado de costas e achatado contra o monte.
            frames[Slam] = Ground(HeroArt.DrawPose(off, LimpPose(true)), -90f, 1.14f, 0.74f);

            // O quique: ainda encolhido, meio de lado, subindo um tiquinho.
            frames[Bounce] = Spin(HeroArt.DrawPose(off, TuckPose(3)), -70f);

            // Caido de costas: apagado, pegando, aceso.
            frames[LieOff] = Ground(HeroArt.DrawPose(off, LimpPose(false)), -90f);
            frames[LieDim] = Ground(HeroArt.DrawPose(Dim(), LimpPose(false)), -90f);
            frames[LieOn] = Ground(HeroArt.DrawPose(on, LimpPose(false)), -90f);

            // Ergue a cabeca (pra frente dele e pra cima do mundo).
            frames[LiftHead] = Ground(HeroArt.DrawPose(on, new HeroArt.Pose
            {
                HeadX = 1.5f, HeadY = -0.5f, BackFootX = -2f, FrontFootX = 4f, FrontFootLift = 1f,
                GripX = 2f, GripY = 0f, Angle = -100f, WeaponBehind = true, Flutter = -0.2f, SpringSway = 2f, Squint = true,
            }), -78f);

            // Apoiado no cinzel, a lamina fincada no monte atras dele.
            frames[Prop] = Ground(HeroArt.DrawPose(on, new HeroArt.Pose
            {
                BodyY = 1.5f, HeadX = 1.5f, BackFootX = -1f, FrontFootX = 5f, FrontFootLift = 1f,
                GripX = 6f, GripY = 2f, Angle = 172f, WeaponBehind = true, Flutter = 0.1f, SpringSway = 1.5f, SpringStretch = 0.9f, Squint = true,
            }), -52f);

            // De joelhos, ainda escorado.
            frames[Kneel] = Ground(HeroArt.DrawPose(on, new HeroArt.Pose
            {
                BodyY = 3f, HeadY = 1f, HeadX = 1f, Squash = 0.88f, BackFootX = -5f, FrontFootX = 5f, FrontFootLift = 1f,
                GripX = 6f, GripY = 2f, Angle = 168f, WeaponBehind = true, Flutter = 0.2f, SpringSway = 1f, SpringStretch = 0.8f, Squint = true,
            }), -20f);

            // Agachado, pronto pra esticar.
            frames[Crouch] = Ground(HeroArt.DrawPose(on, new HeroArt.Pose
            {
                BodyY = 2.5f, HeadY = 1.5f, Squash = 0.86f, BackFootX = -4.5f, FrontFootX = 4.5f,
                GripX = 6f, GripY = 4f, Angle = -40f, Flutter = -0.2f, SpringSway = 0f, SpringStretch = 0.6f, Squint = true,
            }), 0f);

            return frames;
        }

        /// <summary>Encolhido no ar, a capa e a mola batendo com o vento.</summary>
        private static HeroArt.Pose TuckPose(int i)
        {
            return new HeroArt.Pose
            {
                BodyY = -1f, Squash = 0.94f, HeadY = 1f,
                BackFootX = -1f, FrontFootX = 2f, BackFootLift = 5f, FrontFootLift = 6f,
                GripX = 5f, GripY = -1f, Angle = -120f,
                Flutter = 1.1f, CapeWave = i, SpringSway = i % 2 == 0 ? 2f : -2f, SpringStretch = 1.3f,
            };
        }

        /// <summary>Largado: a cabeca pendendo, as pernas abertas, o cinzel solto na mao.</summary>
        private static HeroArt.Pose LimpPose(bool shut)
        {
            return new HeroArt.Pose
            {
                HeadX = -1f, HeadY = 1f, BackFootX = -2f, FrontFootX = 4.5f, FrontFootLift = 1f,
                GripX = 2f, GripY = 0f, Angle = -100f, WeaponBehind = true, Flutter = -0.3f, SpringSway = 3f, SpringStretch = 0.8f,
                EyesShut = shut,
            };
        }

        /// <summary>Gira em volta do meio do desenho e poe no meio do quadro (pros quadros no ar).</summary>
        private static PixelCanvas Spin(PixelCanvas source, float degrees)
        {
            (float cx, float cy) = Center(source);
            var target = new PixelCanvas(Width, Height);
            Transform(source, target, cx, cy, Width * 0.5f, Height * 0.5f, degrees, 1f, 1f);
            return target;
        }

        /// <summary>
        /// Gira em volta do tronco, poe o tronco na coluna do meio e assenta o
        /// desenho no chao (a ultima linha do quadro, que e o pivo).
        /// </summary>
        private static PixelCanvas Ground(PixelCanvas source, float degrees, float scaleX = 1f, float scaleY = 1f)
        {
            var loose = new PixelCanvas(Width, Height);
            Transform(source, loose, TorsoX, TorsoY, Width * 0.5f, Height * 0.5f, degrees, scaleX, scaleY);

            int bottom = -1;
            for (int y = Height - 1; y >= 0 && bottom < 0; y--)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (loose.Get(x, y).A > 128) { bottom = y; break; }
                }
            }

            if (bottom < 0) return loose;

            var placed = new PixelCanvas(Width, Height);
            placed.Blit(loose, 0, Height - 1 - bottom);
            return placed;
        }

        /// <summary>O meio da caixa do que e opaco.</summary>
        private static (float X, float Y) Center(PixelCanvas canvas)
        {
            int minX = canvas.Width, minY = canvas.Height, maxX = -1, maxY = -1;
            for (int y = 0; y < canvas.Height; y++)
            {
                for (int x = 0; x < canvas.Width; x++)
                {
                    if (canvas.Get(x, y).A <= 128) continue;
                    minX = Math.Min(minX, x);
                    maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y);
                }
            }

            return maxX < 0 ? (canvas.Width * 0.5f, canvas.Height * 0.5f) : ((minX + maxX + 1) * 0.5f, (minY + maxY + 1) * 0.5f);
        }

        /// <summary>
        /// Copia source pra target girado (graus, sentido horario na tela) e
        /// escalado em volta de (px, py) do source, que vai parar em (tx, ty).
        /// </summary>
        private static void Transform(PixelCanvas source, PixelCanvas target, float px, float py, float tx, float ty,
            float degrees, float scaleX, float scaleY)
        {
            const int Up = 8;
            PixelCanvas big = Scale2x(Scale2x(Scale2x(source)));

            double radians = degrees * Math.PI / 180.0;
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            for (int y = 0; y < target.Height; y++)
            {
                for (int x = 0; x < target.Width; x++)
                {
                    // Desfaz a escala (nos eixos da tela) e depois o giro.
                    float dx = (x + 0.5f - tx) / scaleX;
                    float dy = (y + 0.5f - ty) / scaleY;
                    float rx = dx * cos + dy * sin;
                    float ry = -dx * sin + dy * cos;

                    int sx = (int)Math.Floor((px + rx) * Up);
                    int sy = (int)Math.Floor((py + ry) * Up);

                    Rgba color = big.Get(sx, sy);
                    if (color.A > 0) target.Set(x, y, color);
                }
            }
        }

        /// <summary>Dobra o tamanho arredondando as diagonais (EPX/Scale2x), sem cor nova.</summary>
        private static PixelCanvas Scale2x(PixelCanvas source)
        {
            int w = source.Width;
            int h = source.Height;
            var target = new PixelCanvas(w * 2, h * 2);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Rgba p = source.Get(x, y);
                    Rgba a = source.Get(x, y - 1);
                    Rgba b = source.Get(x + 1, y);
                    Rgba c = source.Get(x - 1, y);
                    Rgba d = source.Get(x, y + 1);

                    Rgba e0 = Same(c, a) && !Same(c, d) && !Same(a, b) ? a : p;
                    Rgba e1 = Same(a, b) && !Same(a, c) && !Same(b, d) ? b : p;
                    Rgba e2 = Same(d, c) && !Same(d, b) && !Same(c, a) ? c : p;
                    Rgba e3 = Same(b, d) && !Same(b, a) && !Same(d, c) ? d : p;

                    target.Set(x * 2, y * 2, e0);
                    target.Set(x * 2 + 1, y * 2, e1);
                    target.Set(x * 2, y * 2 + 1, e2);
                    target.Set(x * 2 + 1, y * 2 + 1, e3);
                }
            }

            return target;
        }

        private static bool Same(Rgba a, Rgba b)
        {
            if (a.A == 0 && b.A == 0) return true;
            return a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;
        }

        // ------------------------------------------------------------------
        // Os efeitos
        // ------------------------------------------------------------------

        private static readonly Rgba DustLight = new Rgba(176, 156, 130);
        private static readonly Rgba DustMid = new Rgba(132, 112, 94);
        private static readonly Rgba DustDark = new Rgba(88, 72, 64);

        public static PixelCanvas[] Fx()
        {
            var frames = new PixelCanvas[FxCount];

            for (int i = 0; i < DustFrames; i++) frames[Dust + i] = DustPuff(i);

            frames[Debris + 0] = Bolt();
            frames[Debris + 1] = Nut();
            frames[Debris + 2] = Plate();
            frames[Debris + 3] = Cog();
            frames[Debris + 4] = Coil();
            frames[Debris + 5] = PipeBit();

            for (int i = 0; i < SparkFrames; i++) frames[Spark + i] = SparkBurst(i);

            return frames;
        }

        /// <summary>
        /// Uma nuvem de poeira de ferrugem: tres bolotas que incham e se
        /// desfazem, mais clara em cima, com graos soltos na borda no fim.
        /// </summary>
        private static PixelCanvas DustPuff(int i)
        {
            var c = new PixelCanvas(FxSize, FxSize);
            float t = i / (float)(DustFrames - 1);
            float grow = 3.5f + t * 9f;
            int alpha = (int)(235 - t * 150);

            (float X, float Y, float R)[] blobs =
            {
                (16f, 18f, 1.0f), (10f + t * -2f, 20f, 0.72f), (22f + t * 2f, 19.5f, 0.78f), (15f, 13f - t * 3f, 0.62f),
            };

            foreach ((float x, float y, float r) in blobs)
            {
                ArtKit.Puff(c, x, y, grow * r, DustLight, DustMid, alpha);
            }

            // O miolo mais escuro, que some primeiro.
            if (i < 3) ArtKit.Puff(c, 16f, 20f, grow * 0.5f, DustMid, DustDark, alpha);

            // Os graos soltos.
            if (i >= 2)
            {
                for (int k = 0; k < 10; k++)
                {
                    int x = (int)(16 + Math.Cos(k * 2.4 + i) * (grow + 2f));
                    int y = (int)(17 + Math.Sin(k * 2.4 + i) * (grow * 0.8f + 1f));
                    if (ArtKit.Hash(k, i, 77) % 3 == 0) continue;
                    c.Blend(x, y, DustMid.WithAlpha(alpha));
                }
            }

            return c;
        }

        /// <summary>Um parafuso: cabeca sextavada e a haste com rosca.</summary>
        private static PixelCanvas Bolt()
        {
            var c = new PixelCanvas(FxSize, FxSize);
            ArtKit.Limb(c, 13f, 16f, 22f, 16f, 1.6f, ArtKit.Iron, 0.2f);
            for (int x = 15; x <= 21; x += 2) c.Set(x, 15, ArtKit.Iron[0]);
            c.Fill(9, 12, 4, 9, ArtKit.Rust[2]);
            c.Fill(9, 12, 4, 2, ArtKit.Rust[3]);
            c.Fill(9, 19, 4, 2, ArtKit.Rust[0]);
            c.Fill(12, 12, 1, 9, ArtKit.Rust[1]);
            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Uma porca: um sextavado com o furo.</summary>
        private static PixelCanvas Nut()
        {
            var c = new PixelCanvas(FxSize, FxSize);
            var hex = new (float X, float Y)[6];
            for (int k = 0; k < 6; k++)
            {
                double a = k * Math.PI / 3.0;
                hex[k] = (16f + (float)Math.Cos(a) * 5.5f, 16f + (float)Math.Sin(a) * 5.5f);
            }

            ArtKit.Polygon(c, hex, (x, y) => ArtKit.Tone(ArtKit.Brass, -(x - 16f) * 0.12f - (y - 16f) * 0.14f));
            ArtKit.Ball(c, 16f, 16f, 2.2f, 2.2f, ArtKit.Iron, -0.6f);
            for (int y = 14; y <= 18; y++)
            {
                for (int x = 14; x <= 18; x++)
                {
                    float dx = x + 0.5f - 16f, dy = y + 0.5f - 16f;
                    if (dx * dx + dy * dy < 2.2f) c.Set(x, y, Rgba.Clear);
                }
            }

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Um pedaco de chapa torta, enferrujada, com dois rebites.</summary>
        private static PixelCanvas Plate()
        {
            var c = new PixelCanvas(FxSize, FxSize);
            ArtKit.Skin rust = ArtKit.Corroded(ArtKit.Tin, 0.45f, 913, 0.3f, ArtKit.Rust);
            (float X, float Y)[] shape = { (8f, 13f), (22f, 10f), (25f, 15f), (21f, 21f), (10f, 20f), (7f, 17f) };
            ArtKit.Polygon(c, shape, (x, y) => rust(x, y, -(y - 13f) * 0.16f + 0.2f));
            ArtKit.Line(c, 9, 17, 23, 14, ArtKit.Tin[0]);
            ArtKit.Rivet(c, 11, 14, ArtKit.Iron);
            ArtKit.Rivet(c, 20, 13, ArtKit.Iron);
            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Uma engrenagem pequena, de oito dentes.</summary>
        private static PixelCanvas Cog()
        {
            var c = new PixelCanvas(FxSize, FxSize);
            for (int y = 0; y < FxSize; y++)
            {
                for (int x = 0; x < FxSize; x++)
                {
                    float dx = x + 0.5f - 16f, dy = y + 0.5f - 16f;
                    float r = (float)Math.Sqrt(dx * dx + dy * dy);
                    double a = Math.Atan2(dy, dx);
                    bool tooth = Math.Cos(a * 8) > 0.2;
                    float outer = tooth ? 7.2f : 5.6f;
                    if (r > outer || r < 1.8f) continue;

                    float light = -dx * 0.1f - dy * 0.12f + (r < 3.2f ? -0.5f : 0f);
                    c.Set(x, y, ArtKit.Tone(ArtKit.Copper, light));
                }
            }

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Uma mola solta, esticada.</summary>
        private static PixelCanvas Coil()
        {
            var c = new PixelCanvas(FxSize, FxSize);
            for (int k = 0; k < 5; k++)
            {
                int x0 = 8 + k * 3;
                ArtKit.Line(c, x0, 12, x0 + 2, 20, ArtKit.Iron[3]);
                ArtKit.Line(c, x0 + 2, 20, x0 + 3, 12, ArtKit.Iron[1]);
            }

            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>Um toco de cano com a boca escura.</summary>
        private static PixelCanvas PipeBit()
        {
            var c = new PixelCanvas(FxSize, FxSize);
            ArtKit.Limb(c, 9f, 16f, 21f, 16f, 3.2f, ArtKit.Verdigris, 0.1f);
            ArtKit.Ball(c, 22f, 16f, 1.6f, 3.2f, ArtKit.Iron, 0.1f);
            ArtKit.Ball(c, 22.3f, 16f, 0.9f, 2.1f, ArtKit.Rubber, -1f);
            for (int y = 13; y <= 19; y++) c.Set(12, y, ArtKit.Verdigris[0]);
            ArtKit.AddOutline(c);
            return c;
        }

        /// <summary>A faisca do metal raspando: estrela grande, depois menor.</summary>
        private static PixelCanvas SparkBurst(int i)
        {
            var c = new PixelCanvas(FxSize, FxSize);
            ArtKit.Spark(c, 16, 16, i == 0 ? 5 : 3);
            var hot = new Rgba(255, 210, 120);
            int r = i == 0 ? 4 : 6;
            c.Set(16 - r, 16 - r, hot);
            c.Set(16 + r, 16 - r + 1, hot);
            c.Set(16 + r - 1, 16 + r, hot);
            c.Set(16 - r + 1, 16 + r - 1, hot);
            return c;
        }
    }
}
