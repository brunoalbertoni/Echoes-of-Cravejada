using System;
using System.Collections.Generic;

namespace EchoesOfCravejada.EditorTools
{
    /// <summary>
    /// As cores que os bichos do District 3 dividem: ametista, quartzo
    /// leitoso, a rocha violeta e o escuro da boca.
    /// </summary>
    internal static class D3
    {
        public static readonly Rgba[] Amethyst =
            { new Rgba(70, 40, 120), new Rgba(128, 82, 190), new Rgba(186, 142, 236), new Rgba(236, 216, 255) };

        public static readonly Rgba[] Milky =
            { new Rgba(130, 138, 168), new Rgba(190, 198, 224), new Rgba(228, 234, 248), new Rgba(255, 255, 255) };

        public static readonly Rgba[] Rock =
            { new Rgba(34, 30, 46), new Rgba(58, 54, 74), new Rgba(86, 80, 104), new Rgba(118, 110, 138) };

        public static readonly Rgba[] Skin =
            { new Rgba(84, 78, 102), new Rgba(124, 116, 142), new Rgba(160, 152, 178), new Rgba(196, 190, 212) };

        public static readonly Rgba[] Overalls =
            { new Rgba(30, 36, 52), new Rgba(50, 60, 82), new Rgba(74, 88, 112), new Rgba(104, 120, 142) };

        public static readonly Rgba[] Bandage =
            { new Rgba(96, 86, 72), new Rgba(140, 128, 104), new Rgba(178, 166, 138), new Rgba(214, 204, 176) };

        public static readonly Rgba Mouth = new Rgba(24, 12, 22);
        public static readonly Rgba Dust = new Rgba(150, 140, 176);
        public static readonly Rgba DustDark = new Rgba(92, 84, 116);

        /// <summary>Uma lasca de cristal: o corpo na rampa e o fio claro numa das bordas.</summary>
        public static void Crystal(PixelCanvas c, float x0, float y0, float x1, float y1, float half, Rgba[] ramp, float bias = 0.2f)
        {
            ArtKit.Taper(c, x0, y0, x1, y1, half, 0.4f, ramp, bias);
            ArtKit.Line(c, (int)Math.Round(x0 - half * 0.3f), (int)Math.Round(y0), (int)Math.Round(x1), (int)Math.Round(y1), ramp[3].WithAlpha(200));
        }

        /// <summary>Po de cristal: pontinhos claros espalhados em volta de um ponto.</summary>
        public static void Sparkle(PixelCanvas c, float cx, float cy, float spread, int count, int seed, int alpha)
        {
            for (int i = 0; i < count; i++)
            {
                int x = (int)(cx - spread + ArtKit.Hash(i, seed, 6001) % (int)(spread * 2f + 1f));
                int y = (int)(cy - spread * 0.6f + ArtKit.Hash(i, seed, 6002) % (int)(spread * 1.2f + 1f));
                c.Blend(x, y, (i % 3 == 0 ? Milky[3] : Amethyst[3]).WithAlpha(alpha));
            }
        }
    }

    // ======================================================================
    // Crystal Mite
    // ======================================================================

    /// <summary>
    /// O Crystal Mite: um tatuzinho de pedra. As placas das costas sao
    /// metades de geodo, com ametista brotando pelas frestas; tem quatorze
    /// perninhas, duas antenas compridas (e cego: tateia) e as mandibulas de
    /// quartzo. No silencio se enrola e vira uma pedra igual as outras; com
    /// barulho se desenrola e corre aos pulinhos. Morrendo, as placas racham
    /// e os cristais estouram em po. 40x28, pivo na base, no meio.
    /// </summary>
    public static class MiteArt
    {
        public const int Width = 40;
        public const int Height = 28;
        public const int PixelsPerUnit = 32;
        public const float PivotY = 1f / Height;

        public const int Curl = 0;
        public const int CurlFrames = 2;
        public const int Unroll = 2;
        public const int UnrollFrames = 3;
        public const int Run = 5;
        public const int RunFrames = 6;
        public const int Death = 11;
        public const int DeathFrames = 5;
        public const int FrameCount = 16;

        private const float Ground = 26f;

        private sealed class Pose
        {
            /// <summary>0 enrolado (uma pedra), 1 esticado.</summary>
            public float Open = 1f;
            public float Hop;
            public int Step;
            public float Antenna;
            public float Jaw;
            public float Glow;
            public float Crack;
            public float Burst;
        }

        public static PixelCanvas[] Frames()
        {
            var frames = new PixelCanvas[FrameCount];

            // Enrolado: so uma pedra, que respira de leve.
            for (int i = 0; i < CurlFrames; i++) frames[Curl + i] = Draw(new Pose { Open = 0f, Hop = i * 0.5f });

            // Desenrolando: as placas abrem, as antenas saem, os cristais acendem.
            float[] open = { 0.3f, 0.65f, 1f };
            for (int i = 0; i < UnrollFrames; i++) frames[Unroll + i] = Draw(new Pose { Open = open[i], Glow = open[i], Antenna = i == 2 ? 1f : 0f, Jaw = i == 2 ? 1f : 0f });

            // Correndo aos pulinhos, as perninhas em onda.
            float[] hop = { 0f, 1.5f, 2.5f, 1.5f, 0f, 0.5f };
            for (int i = 0; i < RunFrames; i++)
            {
                frames[Run + i] = Draw(new Pose { Hop = hop[i], Step = i, Antenna = i % 2 == 0 ? 1f : -1f, Jaw = i % 3 == 0 ? 1f : 0.3f, Glow = 1f });
            }

            // A morte: racha, acende, estoura em cacos e po.
            for (int i = 0; i < DeathFrames; i++)
            {
                frames[Death + i] = Draw(new Pose { Glow = i == 0 ? 2f : 1f, Crack = i == 0 ? 1f : 0f, Burst = i == 0 ? 0f : i / (DeathFrames - 1f), Jaw = 1f });
            }

            return frames;
        }

        private static PixelCanvas Draw(Pose pose)
        {
            var c = new PixelCanvas(Width, Height);

            if (pose.Burst > 0f)
            {
                DrawBurst(c, pose);
                return c;
            }

            float cx = 20f;
            float cy = Ground - 7f - pose.Hop;
            float rx = 8f + pose.Open * 6f;
            float ry = 8f - pose.Open * 2f;
            if (pose.Open < 0.05f) cy = Ground - 8f;

            // As perninhas: sete de cada lado (so as da frente aparecem inteiras).
            if (pose.Open > 0.2f)
            {
                int legs = 7;
                for (int i = 0; i < legs; i++)
                {
                    float t = i / (legs - 1f);
                    float x = cx - rx * 0.8f + t * rx * 1.6f;
                    float phase = (pose.Step + i) % 3;
                    float swing = phase == 0 ? -1.5f : (phase == 1 ? 0f : 1.5f);
                    float footY = Ground - (pose.Hop > 1f && i % 2 == 0 ? 1f : 0f);
                    ArtKit.Line(c, (int)x, (int)(cy + ry * 0.5f), (int)(x + swing - 1f), (int)footY, D3.Rock[0]);
                    ArtKit.Line(c, (int)x + 1, (int)(cy + ry * 0.5f), (int)(x + swing), (int)footY, D3.Rock[2]);
                }
            }

            // As antenas compridas, tateando o chao na frente.
            if (pose.Open > 0.6f)
            {
                float hx = cx + rx - 1f;
                float hy = cy + 1f;
                float wave = pose.Antenna * 2f;
                for (int a = 0; a < 2; a++)
                {
                    var points = new List<(float, float)>();
                    for (int k = 0; k <= 8; k++)
                    {
                        float t = k / 8f;
                        points.Add((hx + t * 9f, hy - 3f + t * t * (a == 0 ? 6f + wave : 9f - wave) - (float)Math.Sin(t * Math.PI) * 3f));
                    }

                    for (int k = 0; k < points.Count - 1; k++)
                    {
                        (float x0, float y0) = points[k];
                        (float x1, float y1) = points[k + 1];
                        ArtKit.Line(c, (int)x0, (int)y0, (int)x1, (int)y1, a == 0 ? D3.Amethyst[2] : D3.Amethyst[1]);
                    }
                }
            }

            // O corpo: as placas de geodo em gomos, a de tras mais escura.
            int plates = 5;
            for (int i = 0; i < plates; i++)
            {
                float t = i / (plates - 1f);
                float px = cx - rx * 0.6f + t * rx * 1.2f;
                float pr = ry * (0.75f + 0.25f * (float)Math.Sin(t * Math.PI));
                ArtKit.Ball(c, px, cy, rx * 0.36f + 1f, pr, ArtKit.Corroded(D3.Rock, 0.12f, 6101 + i, 0.4f, D3.Amethyst), 0.05f);
            }

            // As frestas entre as placas, com a ametista por dentro.
            for (int i = 1; i < plates; i++)
            {
                float t = (i - 0.5f) / (plates - 1f);
                int px = (int)(cx - rx * 0.6f + t * rx * 1.2f);
                for (int y = (int)(cy - ry) + 1; y < (int)(cy + ry * 0.4f); y++)
                {
                    if (c.Get(px, y).A == 0) continue;
                    c.Set(px, y, pose.Glow > 0.3f && y % 2 == 0 ? D3.Amethyst[2] : D3.Rock[0]);
                }
            }

            // A cabeca: um gomo pequeno na frente, com as mandibulas de quartzo.
            if (pose.Open > 0.4f)
            {
                float hx = cx + rx - 2f;
                float hy = cy + 1.5f;
                ArtKit.Ball(c, hx, hy, 4f, 3.6f, D3.Rock, 0.1f);
                float jaw = 1f + pose.Jaw * 1.5f;
                ArtKit.Taper(c, hx + 2f, hy + 1f, hx + 6f, hy - jaw + 1f, 1.2f, 0.3f, D3.Milky, 0.3f);
                ArtKit.Taper(c, hx + 2f, hy + 2f, hx + 6f, hy + jaw + 2f, 1.2f, 0.3f, D3.Milky, 0f);
            }

            // Os cristais das costas: crescem com o corpo aberto.
            float k2 = 0.5f + pose.Open * 0.5f;
            D3.Crystal(c, cx - 4f, cy - ry + 2f, cx - 7f, cy - ry - 5f * k2, 2.2f, D3.Amethyst);
            D3.Crystal(c, cx + 1f, cy - ry + 1f, cx + 1.5f, cy - ry - 8f * k2, 2.8f, D3.Amethyst);
            D3.Crystal(c, cx + 5f, cy - ry + 2f, cx + 8f, cy - ry - 4f * k2, 1.8f, D3.Milky);

            if (pose.Crack > 0f)
            {
                ArtKit.Line(c, (int)(cx - rx), (int)cy + 1, (int)(cx + rx), (int)cy - 2, D3.Amethyst[3]);
                ArtKit.Line(c, (int)cx - 2, (int)(cy - ry), (int)cx + 1, (int)(cy + ry), D3.Amethyst[3]);
            }

            ArtKit.AddOutline(c);

            if (pose.Glow > 0.3f) ArtKit.Glow(c, cx + 1f, cy - ry - 2f, 8f, D3.Amethyst[2], 0.25f * pose.Glow, true);
            return c;
        }

        /// <summary>Estourado: as placas no chao, os cacos voando e o po subindo.</summary>
        private static void DrawBurst(PixelCanvas c, Pose pose)
        {
            float k = pose.Burst;
            float spread = k * 10f;
            float fall = k * k * 6f;

            if (k < 0.9f)
            {
                ArtKit.Ball(c, 14f - spread * 0.4f, Ground - 3f, 4f, 2.6f, D3.Rock, -0.1f);
                ArtKit.Ball(c, 26f + spread * 0.4f, Ground - 3f, 4f, 2.6f, D3.Rock, -0.2f);
                ArtKit.Ball(c, 20f, Ground - 2f, 3f, 2f, D3.Rock, -0.2f);
            }

            if (k < 0.8f)
            {
                D3.Crystal(c, 16f - spread, 12f + fall, 14f - spread, 8f + fall, 1.6f, D3.Amethyst);
                D3.Crystal(c, 21f, 9f + fall - spread * 0.4f, 21.5f, 4f + fall - spread * 0.4f, 1.8f, D3.Amethyst);
                D3.Crystal(c, 25f + spread, 13f + fall, 27f + spread, 9f + fall, 1.4f, D3.Milky);
            }

            ArtKit.AddOutline(c);
            D3.Sparkle(c, 20f, 14f - k * 4f, 10f + k * 6f, 14, (int)(k * 10f), (int)(230 - k * 150f));
        }
    }

    // ======================================================================
    // Echo Hunter
    // ======================================================================

    /// <summary>
    /// O Echo Hunter: um mineiro do turno do Capataz, magro, de macacao
    /// rasgado e uma faixa de pano amarrada nos olhos. O capacete fundiu na
    /// cabeca e, no lugar da lanterna, cresceu uma trompa de cristal — uma
    /// orelha enorme, virada pra frente. Parado, escuta (a trompa treme);
    /// ouvindo, se ergue de estalo; correndo, vai de quatro, as garras de
    /// quartzo a frente; perdido, fareja rente ao chao. Morrendo, cai de
    /// joelhos e a trompa se desfaz. 64x56, pivo na base, embaixo dele.
    /// </summary>
    public static class HunterArt
    {
        public const int Width = 64;
        public const int Height = 56;
        public const int PixelsPerUnit = 32;
        public const float PivotX = 24f / Width;
        public const float PivotY = 1f / Height;

        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Alert = 4;
        public const int AlertFrames = 2;
        public const int Run = 6;
        public const int RunFrames = 6;
        public const int Sniff = 12;
        public const int SniffFrames = 4;
        public const int Walk = 16;
        public const int WalkFrames = 6;
        public const int Hurt = 22;
        public const int Death = 23;
        public const int DeathFrames = 6;
        public const int FrameCount = 29;

        private const float Ground = 54f;

        private sealed class Pose
        {
            public float X;
            public float Y;

            /// <summary>Quanto o tronco tomba pra frente, em graus (90 = de quatro).</summary>
            public float Lean = 35f;
            public float Head;

            public float BackFoot;
            public float FrontFoot;
            public float BackLift;
            public float FrontLift;
            public float Kneel;

            /// <summary>Os bracos: angulo de cada um a partir do ombro (90 = pra baixo).</summary>
            public float FrontArm = 80f;
            public float BackArm = 95f;
            public float Reach = 1f;

            public float Jaw = 0.3f;
            public float Horn = 1f;
            public float Twitch;
            public bool Lying;
            public float Dissolve;
            public List<(int X, int Y, int Size)> Sparks = new List<(int, int, int)>();
        }

        public static PixelCanvas[] Frames()
        {
            var frames = new PixelCanvas[FrameCount];

            // Escutando: curvado, a trompa tremendo, a boca entreaberta.
            for (int i = 0; i < IdleFrames; i++)
            {
                frames[Idle + i] = Draw(new Pose { Y = i % 2 * 0.5f, Lean = 32f, Head = (i == 1 ? -6f : (i == 3 ? 4f : 0f)), Twitch = i % 2, Jaw = 0.2f + (i == 1 ? 0.3f : 0f) });
            }

            // Ouviu: se ergue de estalo, a trompa pro alto, a boca aberta.
            frames[Alert + 0] = Draw(new Pose { Lean = 14f, Head = -8f, Jaw = 0.8f, FrontArm = 50f, BackArm = 70f, Twitch = 1f });
            frames[Alert + 1] = Draw(new Pose { Lean = 20f, Head = -10f, Jaw = 1f, FrontArm = 30f, BackArm = 60f, BackFoot = -4f, FrontFoot = 3f });

            // Correndo quase de quatro, as garras pra frente.
            for (int i = 0; i < RunFrames; i++)
            {
                double phase = i * Math.PI * 2.0 / RunFrames;
                float swing = (float)Math.Sin(phase);
                frames[Run + i] = Draw(new Pose
                {
                    Y = 4f + (float)Math.Abs(Math.Cos(phase)) * -2f, Lean = 62f, Head = 6f, Jaw = 0.9f,
                    BackFoot = -swing * 9f, FrontFoot = swing * 9f, BackLift = swing > 0f ? swing * 5f : 0f, FrontLift = swing < 0f ? -swing * 5f : 0f,
                    FrontArm = 35f - swing * 35f, BackArm = 35f + swing * 35f, Reach = 1.1f,
                });
            }

            // Farejando: rente ao chao, a cabeca varrendo de um lado pro outro.
            float[] sweep = { 0f, 8f, 14f, 6f };
            for (int i = 0; i < SniffFrames; i++)
            {
                frames[Sniff + i] = Draw(new Pose
                {
                    Y = 7f, Lean = 70f, Head = sweep[i], Jaw = 0.5f, BackFoot = -6f, FrontFoot = 6f, Kneel = 0.4f,
                    FrontArm = 70f + sweep[i], BackArm = 85f, Reach = 0.8f, Twitch = i % 2,
                });
            }

            // Voltando devagar, arrastando os pes.
            for (int i = 0; i < WalkFrames; i++)
            {
                double phase = i * Math.PI * 2.0 / WalkFrames;
                float swing = (float)Math.Sin(phase);
                frames[Walk + i] = Draw(new Pose
                {
                    Y = (1f - (float)Math.Abs(Math.Cos(phase))) * 1.5f, Lean = 38f, Head = 4f, Jaw = 0.2f,
                    BackFoot = -swing * 5f, FrontFoot = swing * 5f, BackLift = swing > 0.3f ? swing * 2f : 0f, FrontLift = swing < -0.3f ? -swing * 2f : 0f,
                    FrontArm = 90f - swing * 10f, BackArm = 95f + swing * 10f, Reach = 0.85f,
                });
            }

            Pose hurt = new Pose { X = -3f, Lean = 8f, Head = -12f, Jaw = 1f, FrontArm = -20f, BackArm = 20f, FrontLift = 3f };
            hurt.Sparks.Add((34, 22, 2));
            frames[Hurt] = Draw(hurt);

            // A morte: leva as maos a trompa, cai de joelhos, tomba e se desfaz.
            frames[Death + 0] = Draw(new Pose { X = -2f, Lean = 4f, Head = -10f, Jaw = 1f, FrontArm = -60f, BackArm = -40f, Reach = 0.8f, Horn = 1f });
            frames[Death + 1] = Draw(new Pose { Y = 6f, Lean = 20f, Head = -6f, Jaw = 0.8f, Kneel = 1f, FrontArm = -50f, BackArm = -30f, Reach = 0.8f, Horn = 0.7f });
            frames[Death + 2] = Draw(new Pose { Y = 8f, Lean = 55f, Head = 20f, Jaw = 0.4f, Kneel = 1f, FrontArm = 90f, BackArm = 100f, Reach = 0.9f, Horn = 0.4f, Dissolve = 0.3f });
            for (int i = 0; i < 3; i++)
            {
                frames[Death + 3 + i] = Draw(new Pose { Lying = true, Horn = 0f, Dissolve = 0.5f + i * 0.25f });
            }

            return frames;
        }

        private sealed class Frame
        {
            public PixelCanvas Canvas = new PixelCanvas(Width, Height);
            public Pose Pose;
            public float HX;
            public float HY;
            public float SX;
            public float SY;
            public float HeadX;
            public float HeadY;
        }

        private static PixelCanvas Draw(Pose pose)
        {
            var f = new Frame { Pose = pose };

            if (pose.Lying)
            {
                DrawLying(f);
                ArtKit.AddOutline(f.Canvas);
                D3.Sparkle(f.Canvas, 44f, Ground - 10f - pose.Dissolve * 10f, 9f, 12, (int)(pose.Dissolve * 10f), (int)(220 - pose.Dissolve * 140f));
                if (pose.Dissolve >= 0.99f) Fade(f.Canvas, 0.5f);
                return f.Canvas;
            }

            f.HX = 24f + pose.X;
            f.HY = 34f + pose.Y + pose.Kneel * 6f;
            (f.SX, f.SY) = ArtKit.Along(f.HX, f.HY, -90f + pose.Lean, 15f);
            (f.HeadX, f.HeadY) = ArtKit.Along(f.SX, f.SY, -90f + pose.Lean + 40f + pose.Head, 8f);

            DrawArm(f, true);
            DrawLeg(f, true);
            DrawLeg(f, false);
            DrawBody(f);
            DrawHead(f);
            DrawArm(f, false);

            ArtKit.AddOutline(f.Canvas);

            if (pose.Horn > 0.3f) ArtKit.Glow(f.Canvas, f.HeadX + 6f, f.HeadY - 6f, 8f, D3.Amethyst[2], 0.25f * pose.Horn, true);
            if (pose.Twitch > 0f && pose.Horn > 0.5f) DrawTwitch(f);
            if (pose.Dissolve > 0f) D3.Sparkle(f.Canvas, f.HeadX + 4f, f.HeadY - 8f, 8f, 10, 7, 200);
            foreach ((int X, int Y, int Size) spark in pose.Sparks) ArtKit.Spark(f.Canvas, spark.X, spark.Y, spark.Size);
            return f.Canvas;
        }

        private static void DrawLeg(Frame f, bool back)
        {
            Pose pose = f.Pose;
            float hipX = f.HX + (back ? -2f : 2f);
            float hipY = f.HY + 2f;
            float footX = 24f + pose.X * 0.4f + (back ? -5f + pose.BackFoot : 5f + pose.FrontFoot);
            float footY = Ground - 2f - (back ? pose.BackLift : pose.FrontLift);
            float bias = back ? -0.45f : 0f;

            float kx, ky;
            if (pose.Kneel > 0.5f && back)
            {
                kx = hipX + 6f;
                ky = Ground - 3f;
                footX = kx - 12f;
                footY = Ground - 2f;
            }
            else
            {
                (kx, ky) = ArtKit.Joint(hipX, hipY, footX, footY, 10f, 10f, -1);
            }

            ArtKit.Taper(f.Canvas, hipX, hipY, kx, ky, 3.4f, 2.8f, D3.Overalls, bias);
            ArtKit.Taper(f.Canvas, kx, ky, footX, footY, 2.6f, 2f, D3.Overalls, bias);

            // A canela rasgada mostrando a pele, e a bota furada.
            ArtKit.Limb(f.Canvas, (kx + footX) * 0.5f, (ky + footY) * 0.5f, footX, footY - 1f, 1.6f, D3.Skin, bias);
            ArtKit.Drum(f.Canvas, footX + 2f, footY + 0.5f, 4.5f, 2.2f, 3f, ArtKit.Plain(ArtKit.Leather), bias);
            f.Canvas.Set((int)footX + 5, (int)footY, D3.Skin[2]);
        }

        private static void DrawBody(Frame f)
        {
            PixelCanvas c = f.Canvas;
            float mx = (f.HX + f.SX) * 0.5f;
            float my = (f.HY + f.SY) * 0.5f;

            // O tronco magro dentro do macacao rasgado.
            ArtKit.Taper(c, f.HX, f.HY, f.SX, f.SY, 5.5f, 6.5f, ArtKit.Corroded(D3.Overalls, 0.1f, 6201, 0.3f, D3.Amethyst), 0f);

            // As costelas aparecendo pelo rasgo, e a rachadura verde.
            for (int i = 0; i < 3; i++)
            {
                int x = (int)(mx - 2f + i * 2f);
                int y = (int)(my - 1f + i);
                c.Set(x, y, D3.Skin[2]);
                c.Set(x + 1, y, D3.Skin[0]);
            }

            ArtKit.Crack(c, mx + 1f, my - 3f, 110f, 5, 6202, 0.7f);

            // As alcas do macacao e o botao de latao.
            ArtKit.Line(c, (int)f.SX - 2, (int)f.SY, (int)mx - 2, (int)my + 3, D3.Overalls[3]);
            ArtKit.Rivet(c, (int)mx - 2, (int)my + 3, ArtKit.Brass);

            // Cacos furando as costas.
            float a = -90f + f.Pose.Lean;
            (float bx, float by) = ArtKit.Along(mx, my, a - 90f, 4f);
            D3.Crystal(c, bx, by, bx - 5f, by - 6f, 1.8f, D3.Amethyst);
            (float bx2, float by2) = ArtKit.Along(f.SX, f.SY, a - 90f, 4f);
            D3.Crystal(c, bx2, by2, bx2 - 3f, by2 - 7f, 1.6f, D3.Milky);
        }

        /// <summary>
        /// A cabeca: o capacete fundido, a faixa nos olhos, a boca de dentes
        /// tortos e a trompa de cristal no lugar da lanterna.
        /// </summary>
        private static void DrawHead(Frame f)
        {
            PixelCanvas c = f.Canvas;
            Pose pose = f.Pose;
            float hx = f.HeadX;
            float hy = f.HeadY;

            ArtKit.Limb(c, f.SX, f.SY, hx - 1f, hy + 2f, 2.4f, D3.Skin, 0f);
            ArtKit.Ball(c, hx, hy, 5.5f, 5.5f, D3.Skin, 0.15f);

            // A faixa de pano nos olhos, com a ponta solta atras.
            for (int x = (int)hx - 5; x <= (int)hx + 6; x++)
            {
                if (c.Get(x, (int)hy - 1).A == 0) continue;
                c.Set(x, (int)hy - 1, D3.Bandage[2]);
                c.Set(x, (int)hy, D3.Bandage[1]);
            }

            ArtKit.Line(c, (int)hx - 6, (int)hy - 1, (int)hx - 9, (int)hy + 2 + (int)pose.Twitch, D3.Bandage[1]);

            // A boca, aberta pra farejar o ar.
            float open = 1f + pose.Jaw * 2.5f;
            c.Fill((int)hx + 2, (int)(hy + 2f), 4, (int)open, D3.Mouth);
            for (int i = 0; i < 3; i++) c.Set((int)hx + 2 + i, (int)(hy + 2f), D3.Milky[2]);

            // O capacete fundido, rachado.
            ArtKit.Ball(c, hx - 0.5f, hy - 4f, 6f, 3.4f, ArtKit.Corroded(ArtKit.Iron, 0.3f, 6203, 0.4f, ArtKit.Rust), 0.25f);
            for (int x = (int)hx - 6; x <= (int)hx + 6; x++) c.Set(x, (int)hy - 2, ArtKit.Iron[1]);
            ArtKit.Line(c, (int)hx - 2, (int)hy - 7, (int)hx - 1, (int)hy - 3, D3.Mouth);

            // A trompa de cristal: um funil que abre pra frente, no lugar da lanterna.
            if (pose.Horn > 0f)
            {
                float k = pose.Horn;
                float bx = hx + 3f;
                float by = hy - 5f;
                float angle = -35f + pose.Head * 0.5f - pose.Twitch * 6f;
                (float tx, float ty) = ArtKit.Along(bx, by, angle, 10f * k);
                ArtKit.Taper(c, bx, by, tx, ty, 1.4f, 5.5f * k, D3.Amethyst, 0.3f);

                // A boca da trompa: um aro claro e o escuro la dentro.
                double a = angle * Math.PI / 180.0;
                float ux = (float)Math.Cos(a);
                float uy = (float)Math.Sin(a);
                for (int py = (int)(ty - 8f); py <= (int)(ty + 8f); py++)
                {
                    for (int px = (int)(tx - 8f); px <= (int)(tx + 8f); px++)
                    {
                        float rx = px + 0.5f - tx;
                        float ry = py + 0.5f - ty;
                        float along = rx * ux + ry * uy;
                        float across = -rx * uy + ry * ux;
                        float d = along * along / (2.4f * 2.4f) + across * across / (6f * k * 6f * k + 0.5f);
                        if (d > 1f) continue;
                        c.Set(px, py, d > 0.45f ? (across < 0f ? D3.Amethyst[3] : D3.Amethyst[2]) : D3.Mouth);
                    }
                }
            }
        }

        /// <summary>Os bracos compridos, as maos de garra de quartzo.</summary>
        private static void DrawArm(Frame f, bool back)
        {
            Pose pose = f.Pose;
            float sx = f.SX + (back ? -1f : 1f);
            float sy = f.SY + 1f;
            float angle = back ? pose.BackArm : pose.FrontArm;
            (float ex, float ey) = ArtKit.Along(sx, sy, angle, 9f * pose.Reach);
            (float hx, float hy) = ArtKit.Along(ex, ey, angle - 20f, 9f * pose.Reach);
            float bias = back ? -0.45f : 0.05f;

            ArtKit.Limb(f.Canvas, sx, sy, ex, ey, 2.2f, D3.Overalls, bias);
            ArtKit.Limb(f.Canvas, ex, ey, hx, hy, 1.8f, D3.Skin, bias);
            ArtKit.Ball(f.Canvas, hx, hy, 2.4f, 2.2f, D3.Skin, bias + 0.1f);

            for (int i = -1; i <= 1; i++)
            {
                (float cx, float cy) = ArtKit.Along(hx, hy, angle - 20f + i * 28f, 4.5f);
                ArtKit.Taper(f.Canvas, hx, hy, cx, cy, 1f, 0.3f, D3.Milky, bias + 0.3f);
            }
        }

        /// <summary>A trompa tremendo: dois arquinhos de som na boca dela.</summary>
        private static void DrawTwitch(Frame f)
        {
            (float tx, float ty) = ArtKit.Along(f.HeadX + 3f, f.HeadY - 5f, -35f + f.Pose.Head * 0.5f, 12f);
            for (int k = 0; k < 2; k++)
            {
                float r = 3f + k * 3f;
                for (int a = -50; a <= 50; a += 10)
                {
                    double t = (a - 35.0) * Math.PI / 180.0;
                    f.Canvas.Blend((int)Math.Round(tx + Math.Cos(t) * r), (int)Math.Round(ty + Math.Sin(t) * r), new Rgba(226, 232, 255).WithAlpha(170 - k * 60));
                }
            }
        }

        private static void DrawLying(Frame f)
        {
            PixelCanvas c = f.Canvas;
            float g = Ground;
            ArtKit.Limb(c, 12f, g - 3f, 24f, g - 4f, 3f, D3.Overalls, -0.1f);
            ArtKit.Drum(c, 10f, g - 3f, 3f, 2.2f, 3f, ArtKit.Plain(ArtKit.Leather), 0f);
            ArtKit.Taper(c, 24f, g - 4f, 38f, g - 5f, 5f, 5.5f, D3.Overalls, 0.05f);
            ArtKit.Limb(c, 36f, g - 3f, 48f, g - 2f, 1.8f, D3.Skin, -0.1f);
            ArtKit.Ball(c, 43f, g - 5f, 5f, 4.5f, D3.Skin, 0.1f);
            ArtKit.Ball(c, 42f, g - 8f, 5.5f, 2.6f, ArtKit.Iron, 0.15f);
            for (int x = 39; x <= 47; x++) c.Set(x, (int)g - 5, D3.Bandage[2]);
            if (f.Pose.Dissolve < 0.7f) D3.Crystal(c, 46f, g - 8f, 51f, g - 12f, 1.8f, D3.Amethyst, -0.2f);
        }

        private static void Fade(PixelCanvas c, float amount)
        {
            for (int i = 0; i < c.Pixels.Length; i++)
            {
                Rgba p = c.Pixels[i];
                if (p.A > 0) c.Pixels[i] = p.WithAlpha((int)(p.A * amount));
            }
        }
    }

    // ======================================================================
    // Shard Sentinel
    // ======================================================================

    /// <summary>
    /// A Shard Sentinel: um geodo do tamanho de um barril, plantado num
    /// pedestal de cristais. Fechado, e uma pedra rachada com uma fresta
    /// violeta acesa. Ouvindo barulho, a metade de cima abre como uma tampa e
    /// mostra o miolo de ametista com o nucleo vibrando; dispara um leque de
    /// estilhacos e fecha de novo. Morrendo, as duas metades racham e caem
    /// pros lados, e o miolo estoura em po. 56x56, pivo na base, no meio.
    /// </summary>
    public static class SentinelArt
    {
        public const int Size = 56;
        public const int PixelsPerUnit = 32;
        public const float PivotY = 1f / Size;

        public const int Idle = 0;
        public const int IdleFrames = 4;
        public const int Open = 4;
        public const int OpenFrames = 3;
        public const int Fire = 7;
        public const int FireFrames = 2;
        public const int Close = 9;
        public const int CloseFrames = 2;
        public const int Hurt = 11;
        public const int Death = 12;
        public const int DeathFrames = 6;
        public const int FrameCount = 18;

        /// <summary>O nucleo, de onde saem os estilhacos, em tiles acima do pivo.</summary>
        public const float CoreY = 1.06f;

        private const float Ground = 54f;
        private const float CX = 28f;
        private const float CY = 33f;

        private sealed class Pose
        {
            /// <summary>A tampa: 0 fechada, 1 escancarada.</summary>
            public float Lid;
            public float Core = 0.4f;
            public float Shake;
            public bool Flash;
            public float Seam = 0.5f;
            public float Break;
            public List<(int X, int Y, int Size)> Sparks = new List<(int, int, int)>();
        }

        public static PixelCanvas[] Frames()
        {
            var frames = new PixelCanvas[FrameCount];

            float[] seam = { 0.4f, 0.6f, 0.8f, 0.6f };
            for (int i = 0; i < IdleFrames; i++) frames[Idle + i] = Draw(new Pose { Seam = seam[i], Lid = i == 2 ? 0.06f : 0f });

            float[] open = { 0.35f, 0.75f, 1f };
            for (int i = 0; i < OpenFrames; i++)
            {
                frames[Open + i] = Draw(new Pose { Lid = open[i], Core = 0.6f + i * 0.4f, Shake = i == 2 ? 1f : 0f, Seam = 1f });
            }

            Pose shot = new Pose { Lid = 1f, Core = 2f, Flash = true, Seam = 1f };
            shot.Sparks.Add((40, 22, 3));
            frames[Fire + 0] = Draw(shot);
            frames[Fire + 1] = Draw(new Pose { Lid = 0.95f, Core = 1.2f, Seam = 1f });

            frames[Close + 0] = Draw(new Pose { Lid = 0.5f, Core = 0.6f, Seam = 0.8f });
            frames[Close + 1] = Draw(new Pose { Lid = 0.15f, Core = 0.4f, Seam = 0.6f });

            Pose hurt = new Pose { Lid = 0.2f, Shake = -2f, Seam = 1f, Core = 1f };
            hurt.Sparks.Add((22, 26, 2));
            frames[Hurt] = Draw(hurt);

            for (int i = 0; i < DeathFrames; i++)
            {
                frames[Death + i] = Draw(new Pose { Break = (i + 1) / (float)DeathFrames, Core = i < 2 ? 2f : 0f, Lid = 0.3f });
            }

            return frames;
        }

        private static PixelCanvas Draw(Pose pose)
        {
            var c = new PixelCanvas(Size, Size);
            float cx = CX + pose.Shake;

            // O pedestal de cristais, plantado no chao.
            DrawBase(c, pose.Break);

            if (pose.Break > 0f)
            {
                DrawBroken(c, pose);
                return c;
            }

            ArtKit.Skin crust = ArtKit.Corroded(D3.Rock, 0.1f, 6301, 0.35f, D3.Amethyst);

            // A metade de baixo: a casca e o miolo de ametista.
            ArtKit.Ball(c, cx, CY + 2f, 15f, 12f, crust, -0.05f);
            if (pose.Lid > 0.05f)
            {
                DrawGeodeInside(c, cx, CY - 1f, 12f, 5f + pose.Lid * 3f, pose);
            }

            // A metade de cima, a tampa: gira pra tras na dobradica da esquerda.
            float hingeX = cx - 14f;
            float hingeY = CY - 1f;
            float angle = -pose.Lid * 70f;
            var lid = new List<(float, float)>();
            for (int i = 0; i <= 16; i++)
            {
                double t = Math.PI + i * Math.PI / 16.0;
                float px = cx + (float)Math.Cos(t) * 15f;
                float py = CY - 1f + (float)Math.Sin(t) * 13f;
                lid.Add(Rotate(px, py, hingeX, hingeY, angle));
            }

            ArtKit.Polygon(c, lid.ToArray(), (x, y) =>
            {
                (float lx, float ly) = Rotate(x, y, hingeX, hingeY, -angle);
                float dx = (lx - cx) / 15f;
                float dy = (ly - (CY - 1f)) / 13f;
                float light = -0.55f * dx - 0.65f * dy + 0.1f;
                return crust(lx, ly, light);
            });

            // A crosta: musgo de cristal, rachaduras e as marcas de giz dos mineiros.
            (float mx, float my) = Rotate(cx + 4f, CY - 8f, hingeX, hingeY, angle);
            c.Set((int)mx, (int)my, new Rgba(230, 226, 214));
            c.Set((int)mx + 1, (int)my + 1, new Rgba(230, 226, 214));
            c.Set((int)mx + 2, (int)my, new Rgba(230, 226, 214));
            (float kx, float ky) = Rotate(cx - 6f, CY - 10f, hingeX, hingeY, angle);
            D3.Crystal(c, kx, ky, kx - 3f, ky - 5f, 1.6f, D3.Amethyst);
            ArtKit.Ball(c, cx + 9f, CY + 8f, 3f, 2f, D3.Rock, -0.2f);

            // A fresta acesa entre as duas metades (fechada).
            if (pose.Lid < 0.1f)
            {
                for (int x = (int)(cx - 13f); x <= (int)(cx + 13f); x++)
                {
                    int y = (int)(CY - 1f + Math.Sin(x * 0.7) * 0.6);
                    c.Set(x, y, pose.Seam > 0.55f && x % 3 != 0 ? D3.Amethyst[3] : D3.Amethyst[1]);
                    c.Set(x, y + 1, D3.Rock[0]);
                }
            }

            ArtKit.AddOutline(c);

            if (pose.Lid < 0.1f) ArtKit.Glow(c, cx, CY - 1f, 16f, D3.Amethyst[2], 0.2f * pose.Seam, true);
            if (pose.Lid > 0.3f) ArtKit.Glow(c, cx + 2f, CY - 4f, 10f + pose.Core * 3f, D3.Amethyst[3], 0.3f * pose.Core, true);
            if (pose.Flash) ArtKit.Glow(c, cx + 8f, CY - 6f, 12f, D3.Milky[3], 0.8f, true);
            foreach ((int X, int Y, int Size) spark in pose.Sparks) ArtKit.Spark(c, spark.X, spark.Y, spark.Size);
            return c;
        }

        /// <summary>O miolo do geodo: o anel de quartzo, os dentes de ametista e o nucleo.</summary>
        private static void DrawGeodeInside(PixelCanvas c, float cx, float cy, float rx, float ry, Pose pose)
        {
            ArtKit.Ball(c, cx, cy, rx, ry, D3.Milky, 0.1f);
            ArtKit.Ball(c, cx, cy + 0.5f, rx - 2f, ry - 1.5f, D3.Amethyst, -0.1f);

            for (int i = 0; i < 7; i++)
            {
                float x = cx - rx + 3f + i * (rx * 2f - 6f) / 6f;
                float h = 3f + (i % 3);
                ArtKit.Taper(c, x, cy + ry - 2f, x + (i - 3) * 0.3f, cy + ry - 2f - h, 1.4f, 0.3f, D3.Amethyst, 0.4f);
            }

            float r = 2.5f + pose.Core * 1.2f;
            ArtKit.Ball(c, cx + 1f, cy - 1f, r, r, new[] { D3.Amethyst[2], D3.Amethyst[3], D3.Milky[3], D3.Milky[3] }, 0.3f);
        }

        private static void DrawBase(PixelCanvas c, float broken)
        {
            ArtKit.Ball(c, CX, Ground - 3f, 16f, 4f, D3.Rock, -0.1f);
            if (broken > 0.6f) return;

            foreach ((float x, float tilt, float h, Rgba[] ramp) in new[]
            {
                (CX - 12f, -18f, 9f, D3.Amethyst), (CX - 6f, -6f, 6f, D3.Milky), (CX + 7f, 8f, 7f, D3.Amethyst), (CX + 13f, 20f, 8f, D3.Milky),
            })
            {
                (float tx, float ty) = ArtKit.Along(x, Ground - 4f, -90f + tilt, h);
                D3.Crystal(c, x, Ground - 4f, tx, ty, 2.2f, ramp);
            }
        }

        /// <summary>Quebrando: as metades racham e caem pros lados, o miolo estoura em po.</summary>
        private static void DrawBroken(PixelCanvas c, Pose pose)
        {
            float k = pose.Break;
            float apart = k * 10f;
            float drop = Math.Min(1f, k * 1.6f) * 14f;

            ArtKit.Ball(c, CX - 6f - apart, CY + 4f + drop * 0.6f, 9f, 9f - k * 3f, D3.Rock, -0.1f);
            ArtKit.Ball(c, CX + 6f + apart, CY + 4f + drop * 0.6f, 9f, 9f - k * 3f, D3.Rock, -0.2f);
            ArtKit.Ball(c, CX - 6f - apart + 3f, CY + 2f + drop * 0.6f, 5f, 5f - k * 2f, D3.Amethyst, 0f);
            ArtKit.Ball(c, CX + 6f + apart - 3f, CY + 2f + drop * 0.6f, 5f, 5f - k * 2f, D3.Amethyst, -0.1f);

            if (k < 0.4f)
            {
                ArtKit.Line(c, (int)CX - 12, (int)CY - 8, (int)CX + 12, (int)CY + 6, D3.Amethyst[3]);
                ArtKit.Line(c, (int)CX + 3, (int)CY - 12, (int)CX - 2, (int)CY + 10, D3.Amethyst[3]);
            }

            ArtKit.AddOutline(c);
            if (pose.Core > 1f) ArtKit.Glow(c, CX, CY, 14f, D3.Amethyst[3], 0.7f, true);
            D3.Sparkle(c, CX, CY - k * 10f, 10f + k * 10f, 18, (int)(k * 10f), (int)(240 - k * 160f));
        }

        private static (float X, float Y) Rotate(float x, float y, float ox, float oy, float degrees)
        {
            double a = degrees * Math.PI / 180.0;
            float dx = x - ox;
            float dy = y - oy;
            return (ox + dx * (float)Math.Cos(a) - dy * (float)Math.Sin(a), oy + dx * (float)Math.Sin(a) + dy * (float)Math.Cos(a));
        }
    }
}
